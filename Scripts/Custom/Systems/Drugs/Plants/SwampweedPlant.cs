using System;

namespace Server.Items
{
    public class SwampweedPlant : DrugPlantBase
    {
        protected override Type YieldType { get { return typeof(Swampweed); } }
        protected override int MinSkillFixed { get { return 500; } }
        protected override int BonusYield { get { return 2; } }

        [Constructable]
        public SwampweedPlant()
            : base(3157)
        {
            Hue = 256;
            Name = "planta de swampweed";
        }

        public SwampweedPlant(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
