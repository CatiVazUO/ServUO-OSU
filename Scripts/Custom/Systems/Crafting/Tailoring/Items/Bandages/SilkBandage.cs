using Server.Items;
using System;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bandages
{
    public class SilkBandage : Bandage
    {
        [Constructable]
        public SilkBandage() : this(1)
        {
        }

        [Constructable]
        public SilkBandage(int amount) : base(amount)
        {
            Name = "bandagem de seda";
        }

        public SilkBandage(Serial serial) : base(serial)
        {
        }

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
