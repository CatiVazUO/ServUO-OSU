using System;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Raw
{
    public class SilkCocoon : Item
    {
        [Constructable]
        public SilkCocoon() : this(1) { }

        [Constructable]
        public SilkCocoon(int amount) : base(0xDF9)
        {
            Stackable = true;
            Amount = amount;
            Name = "casulo de seda";
        }

        public SilkCocoon(Serial serial) : base(serial) { }

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
