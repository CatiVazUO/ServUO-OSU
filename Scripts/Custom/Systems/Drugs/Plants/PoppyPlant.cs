using System;

namespace Server.Items
{
    public class PoppyPlant : DrugPlantBase
    {
        protected override Type YieldType { get { return typeof(Opium); } }
        protected override int MinSkillFixed { get { return 600; } }
        protected override int BonusYield { get { return 1; } }

        [Constructable]
        public PoppyPlant()
            : base(0x0C86)
        {
            Hue = 2017;
            Name = "papoula de ópio";
        }

        public PoppyPlant(Serial serial) : base(serial) { }
    }
}
