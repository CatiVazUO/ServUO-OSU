using System;

namespace Server.Items
{
    public class TobaccoPlant : DrugPlantBase
    {
        protected override Type YieldType { get { return typeof(Tobacco); } }
        protected override int MinSkillFixed { get { return 500; } }
        protected override int BonusYield { get { return 2; } }

        [Constructable]
        public TobaccoPlant()
            : base(0x0C63)
        {
            Hue = 2976;
            Name = "planta de tabaco";
        }

        public TobaccoPlant(Serial serial) : base(serial) { }

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
