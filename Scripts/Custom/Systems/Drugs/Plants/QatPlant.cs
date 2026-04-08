using System;

namespace Server.Items
{
    public class QatPlant : DrugPlantBase
    {
        protected override Type YieldType { get { return typeof(Qat); } }
        protected override int MinSkillFixed { get { return 400; } }
        protected override int BonusYield { get { return 1; } }

        [Constructable]
        public QatPlant()
            : base(0x0C61)
        {
            Hue = 1454;
            Name = "planta de qat";
        }

        public QatPlant(Serial serial) : base(serial) { }
    }
}
