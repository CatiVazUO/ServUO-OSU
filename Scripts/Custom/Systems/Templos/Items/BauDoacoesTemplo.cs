using System;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Reinos;
using Server.Custom.Systems.Templos;

namespace Server.Custom.Systems.Templos.Items
{
    public class BauDoacoesTemplo : Item
    {
        private int m_CityId;
        private int m_StoredGold;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId
        {
            get { return m_CityId; }
            set { m_CityId = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StoredGold
        {
            get { return m_StoredGold; }
            set { m_StoredGold = Math.Max(0, value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey
        {
            get { return m_ConstructionKey; }
            set { m_ConstructionKey = value ?? String.Empty; InvalidateProperties(); }
        }

        [Constructable]
        public BauDoacoesTemplo() : this(-1)
        {
        }

        [Constructable]
        public BauDoacoesTemplo(int cityId) : base(0xE43)
        {
            Movable = false;
            Name = "bau de doações";
            Weight = 255.0;
            CityId = cityId;
            ConstructionKey = String.Empty;
        }

        public BauDoacoesTemplo(Serial serial) : base(serial)
        {
        }

        public bool TryConsumeGold(int amount)
        {
            amount = Math.Max(0, amount);

            if (amount <= 0)
                return true;

            if (m_StoredGold < amount)
                return false;

            m_StoredGold -= amount;
            InvalidateProperties();
            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Use o menu de contexto do baú.");
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            list.Add(new DonateToKingdomEntry(pm, this));
            list.Add(new WithdrawGoldEntry(pm, this));
        }

        private static int GetAcceptedAmount(Item item, out int gold, out int cloth, out int iron, out int wood)
        {
            gold = cloth = iron = wood = 0;

            if (item is Gold)
            {
                gold = item.Amount;
                return gold;
            }

            if (item is Cloth)
            {
                cloth = item.Amount;
                return cloth;
            }

            if (item is BaseOre)
            {
                iron = item.Amount;
                return iron;
            }

            if (item is BaseLog)
            {
                wood = item.Amount;
                return wood;
            }

            return 0;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            int gold, cloth, iron, wood;
            int amount = GetAcceptedAmount(dropped, out gold, out cloth, out iron, out wood);

            if (amount <= 0)
            {
                from.SendMessage("Este baú só aceita logs, ore, cloth e gold.");
                return false;
            }

            PlayerMobile pm = from as PlayerMobile;
            if (pm != null)
                pm.Emote("*faz uma doação*");

            if (gold > 0)
                StoredGold += gold;

            string constructionKey = !String.IsNullOrWhiteSpace(ConstructionKey) ? ConstructionKey : TemploSystem.GetConstructionKeyByCityId(CityId);
            TemploSystem.RecordDonation(constructionKey, CityId, gold, cloth, iron, wood);

            dropped.Delete();
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Moedas: " + m_StoredGold);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_CityId);
            writer.Write(m_StoredGold);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_StoredGold = reader.ReadInt();
            m_ConstructionKey = version >= 1 ? reader.ReadString() : String.Empty;
            Movable = false;
        }

        private enum ChestAction
        {
            DonateToKingdom,
            WithdrawGold
        }

        private sealed class ChestPromptState
        {
            public BauDoacoesTemplo Chest;
            public ChestAction Action;

            public ChestPromptState(BauDoacoesTemplo chest, ChestAction action)
            {
                Chest = chest;
                Action = action;
            }
        }

        private static int ParseAmount(string text)
        {
            int value;
            return Int32.TryParse((text ?? String.Empty).Trim(), out value) ? Math.Max(0, value) : -1;
        }

        private static void BeginAmountPrompt(PlayerMobile pm, BauDoacoesTemplo chest, ChestAction action)
        {
            if (pm == null || chest == null || chest.Deleted)
                return;

            pm.BeginPrompt(OnAmountPrompt, new ChestPromptState(chest, action));
        }

        private static void OnAmountPrompt(Mobile from, string text, object stateObj)
        {
            PlayerMobile pm = from as PlayerMobile;
            ChestPromptState state = stateObj as ChestPromptState;

            if (pm == null || state == null || state.Chest == null || state.Chest.Deleted)
                return;

            BauDoacoesTemplo chest = state.Chest;

            if (!pm.InRange(chest.GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais do baú.");
                return;
            }

            int amount = ParseAmount(text);
            if (amount <= 0)
            {
                pm.SendMessage("Digite um valor inteiro maior que zero.");
                return;
            }

            if (amount > chest.StoredGold)
            {
                pm.SendMessage("O baú não possui essa quantidade de moedas.");
                return;
            }

            switch (state.Action)
            {
                case ChestAction.DonateToKingdom:
                    if (!TemploSystem.CanAccessTemple(pm, chest.CityId, !String.IsNullOrWhiteSpace(chest.ConstructionKey) ? chest.ConstructionKey : TemploSystem.GetConstructionKeyByCityId(chest.CityId)))
                    {
                        pm.SendMessage("Somente o líder ou o cargo ligado ao templo pode doar moedas ao reino.");
                        return;
                    }

                    chest.StoredGold -= amount;
                    ReinoTreasurySystem.RecordDonationToKingdom(chest.CityId, amount, 0, 0, 0);
                    chest.InvalidateProperties();
                    pm.SendMessage("Você doou " + amount + " moedas do baú do templo ao reino.");
                    break;

                case ChestAction.WithdrawGold:
                    if (!ReinoTreasurySystem.CanWithdrawDonations(pm, chest.CityId))
                    {
                        pm.SendMessage("Somente o responsável religioso pode sacar moedas deste baú.");
                        return;
                    }

                    chest.StoredGold -= amount;
                    chest.InvalidateProperties();

                    if (pm.Backpack != null)
                        pm.Backpack.DropItem(new Gold(amount));
                    else if (pm.BankBox != null)
                        pm.BankBox.DropItem(new Gold(amount));
                    else
                        new Gold(amount).MoveToWorld(pm.Location, pm.Map);

                    pm.SendMessage("Você retirou " + amount + " moedas do baú de doações.");
                    break;
            }
        }

        private sealed class DonateToKingdomEntry : ContextMenuEntry
        {
            private readonly PlayerMobile m_From;
            private readonly BauDoacoesTemplo m_Chest;

            public DonateToKingdomEntry(PlayerMobile from, BauDoacoesTemplo chest) : base(1061274, 2)
            {
                m_From = from;
                m_Chest = chest;
            }

            public override void OnClick()
            {
                if (m_From == null || m_Chest == null || m_Chest.Deleted)
                    return;

                if (!m_From.InRange(m_Chest.GetWorldLocation(), 2))
                {
                    m_From.SendMessage("Você está longe demais do baú.");
                    return;
                }

                m_From.SendMessage("Digite a quantidade de moedas que deseja doar ao reino.");
                BeginAmountPrompt(m_From, m_Chest, ChestAction.DonateToKingdom);
            }
        }

        private sealed class WithdrawGoldEntry : ContextMenuEntry
        {
            private readonly PlayerMobile m_From;
            private readonly BauDoacoesTemplo m_Chest;

            public WithdrawGoldEntry(PlayerMobile from, BauDoacoesTemplo chest) : base(1061275, 2)
            {
                m_From = from;
                m_Chest = chest;
            }

            public override void OnClick()
            {
                if (m_From == null || m_Chest == null || m_Chest.Deleted)
                    return;

                if (!m_From.InRange(m_Chest.GetWorldLocation(), 2))
                {
                    m_From.SendMessage("Você está longe demais do baú.");
                    return;
                }

                m_From.SendMessage("Digite a quantidade de moedas que deseja sacar.");
                BeginAmountPrompt(m_From, m_Chest, ChestAction.WithdrawGold);
            }
        }
    }
}
