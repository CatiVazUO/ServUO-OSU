using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class BauDoacoesDoReino : Item
    {
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId
        {
            get { return m_CityId; }
            set { m_CityId = value; InvalidateProperties(); }
        }

        [Constructable]
        public BauDoacoesDoReino() : base(0xE43)
        {
            Movable = false;
            Name = "bau de doações do reino";
            Weight = 255.0;
            CityId = -1;
        }

        public BauDoacoesDoReino(Serial serial) : base(serial)
        {
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
            {
                string diplomacyReason;
                if (!ReinoDiplomacySystem.CanUseDonationChest(pm, CityId, out diplomacyReason))
                {
                    pm.SendMessage(diplomacyReason);
                    return false;
                }

                pm.Emote("*faz uma doação*");
            }

            ReinoTreasurySystem.RecordDonationToKingdom(CityId, gold, cloth, iron, wood);
            dropped.Delete();
            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Este baú envia as doações diretamente para o tesouro do reino.");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
        }
    }
}
