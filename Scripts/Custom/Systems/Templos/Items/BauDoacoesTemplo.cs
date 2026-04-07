using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Systems.Templos.Items
{
    public class BauDoacoesTemplo : Item
    {
        private int m_CityId;
        private int m_StoredReligiousGold;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId
        {
            get { return m_CityId; }
            set { m_CityId = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StoredReligiousGold
        {
            get { return m_StoredReligiousGold; }
            set { m_StoredReligiousGold = Math.Max(0, value); InvalidateProperties(); }
        }

        [Constructable]
        public BauDoacoesTemplo() : base(0xE43)
        {
            Movable = false;
            Name = "bau de doações";
            Weight = 255.0;
            CityId = -1;
        }

        public BauDoacoesTemplo(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais do baú.");
                return;
            }

            if (!Server.Custom.Reinos.ReinoTreasurySystem.CanWithdrawReligiousDonations(pm, CityId))
            {
                pm.SendMessage("Somente o líder religioso pode retirar as doações deste baú.");
                return;
            }

            pm.SendGump(new BauDoacoesTemploGump(pm, this));
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

            int kingdomGold = 0;
            int religiousGold = 0;

            if (gold > 0)
            {
                int percent = Server.Custom.Reinos.ReinoTreasurySystem.GetReligiousDonationTaxPercent(CityId);
                kingdomGold = (gold * Math.Max(0, Math.Min(50, percent))) / 100;
                religiousGold = Math.Max(0, gold - kingdomGold);
                StoredReligiousGold += religiousGold;
            }

            Server.Custom.Reinos.ReinoTreasurySystem.RecordDonationToKingdom(CityId, kingdomGold, cloth, iron, wood);
            dropped.Delete();
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Moedas religiosas guardadas: " + StoredReligiousGold);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_StoredReligiousGold);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_StoredReligiousGold = reader.ReadInt();
        }
    }

    public class BauDoacoesTemploGump : Gump
    {
        private readonly BauDoacoesTemplo m_Chest;

        public BauDoacoesTemploGump(PlayerMobile from, BauDoacoesTemplo chest) : base(100, 100)
        {
            m_Chest = chest;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 320, 160, 9270);
            AddLabel(70, 20, 0, "Doações do Templo");
            AddLabel(35, 60, 0, "Moedas disponíveis:");
            AddLabel(180, 60, 0, chest != null ? chest.StoredReligiousGold.ToString() : "0");
            AddButton(35, 100, 247, 248, 1, GumpButtonType.Reply, 0);
            AddLabel(70, 100, 0, "Sacar doações");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted || m_Chest == null || m_Chest.Deleted)
                return;

            if (info.ButtonID != 1)
                return;

            int amount = Math.Max(0, m_Chest.StoredReligiousGold);
            if (amount <= 0)
            {
                from.SendMessage("Não há moedas para sacar.");
                return;
            }

            m_Chest.StoredReligiousGold = 0;

            if (from.Backpack != null)
                from.Backpack.DropItem(new Gold(amount));
            else if (from.BankBox != null)
                from.BankBox.DropItem(new Gold(amount));
            else
                new Gold(amount).MoveToWorld(from.Location, from.Map);

            from.SendMessage("Você retirou " + amount + " moedas das doações religiosas.");
        }
    }
}
