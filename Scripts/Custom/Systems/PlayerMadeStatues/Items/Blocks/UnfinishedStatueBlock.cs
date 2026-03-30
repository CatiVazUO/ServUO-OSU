using Server.Custom.Systems.PlayerMadeStatues;
using Server.Mobiles;

namespace Server.Items
{
    public class UnfinishedStatueBlock : Item
    {
        private Mobile m_Sculptor;
        private StatueCraftCategory m_Category;
        private int m_MaterialId;
        private string m_PlannedName;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Sculptor { get { return m_Sculptor; } set { m_Sculptor = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public StatueCraftCategory Category { get { return m_Category; } set { m_Category = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaterialId { get { return m_MaterialId; } set { m_MaterialId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string PlannedName { get { return m_PlannedName; } set { m_PlannedName = value; } }

        [Constructable]
        public UnfinishedStatueBlock() : this(StatueCraftSystem.SmallStoneBlockSouth, 0)
        {
        }

        public UnfinishedStatueBlock(int itemID, int hue) : base(itemID)
        {
            Name = null;
            Hue = hue;
            Movable = false;
            Weight = 90.0;
        }

        public UnfinishedStatueBlock(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_Sculptor);
            writer.Write((int)m_Category);
            writer.Write(m_MaterialId);
            writer.Write(m_PlannedName ?? string.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Sculptor = reader.ReadMobile();
            m_Category = (StatueCraftCategory)reader.ReadInt();
            m_MaterialId = reader.ReadInt();
            m_PlannedName = reader.ReadString();
        }
    }
}
