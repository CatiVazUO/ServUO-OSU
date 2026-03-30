using Server.Custom.Systems.PlayerMadeStatues;
using Server.Network;

namespace Server.Items
{
    public abstract class BaseFinishedSculptureItem : Item
    {
        private int m_MaterialId;

        [CommandProperty(AccessLevel.GameMaster)]

        public override bool DisplayWeight { get { return false; } }
        public int MaterialId { get { return m_MaterialId; } set { m_MaterialId = value; ApplyMaterial(); } }

        public abstract string RecipeName { get; }
        public abstract StatueCraftCategory SculptureCategory { get; }

        protected BaseFinishedSculptureItem(int itemID) : base(itemID)
        {
            Movable = true;
            Weight = 10.0;
            Name = null;
            m_MaterialId = 1000;
            ApplyMaterial();
        }

        protected virtual void ApplyMaterial()
        {
            Hue = StatueMaterialOptions.GetHue(m_MaterialId);
        }

        public override void OnSingleClick(Mobile from)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
        }

        public BaseFinishedSculptureItem(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_MaterialId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_MaterialId = reader.ReadInt();
            ApplyMaterial();
        }
    }
}
