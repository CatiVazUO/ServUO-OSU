using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Arena.Gumps;

namespace Server.Custom.Systems.Arena.Items
{
    public class ArenaTicket : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [Constructable]
        public ArenaTicket() : this(-1, String.Empty)
        {
        }

        public ArenaTicket(int cityId, string constructionKey) : base(0x14F0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Name = "ingresso de arena";
            LootType = LootType.Blessed;
            Weight = 1.0;
        }

        public ArenaTicket(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            LootType = LootType.Blessed;
        }
    }

    public class ArenaControlItem : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [Constructable]
        public ArenaControlItem() : this(-1, String.Empty)
        {
        }

        public ArenaControlItem(int cityId, string constructionKey) : base(0x1B72)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Name = "controle da arena";
            Movable = false;
        }

        public ArenaControlItem(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ArenaSystem.CanAccessControl(pm, m_CityId, m_ConstructionKey))
            {
                pm.SendMessage("Somente líder do reino, GM ou cargo ligado à arena pode usar este controle.");
                return;
            }

            pm.CloseGump(typeof(ArenaMainGump));
            pm.SendGump(new ArenaMainGump(pm, m_CityId, m_ConstructionKey));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            Movable = false;
        }
    }
}
