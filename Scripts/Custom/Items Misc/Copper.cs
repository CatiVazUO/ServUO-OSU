using System;
using Server;

namespace Server.Items
{
    public class Copper : Item
    {
        public override double DefaultWeight => 0.02;

        [Constructable]
        public Copper() : this(1)
        {
        }

        [Constructable]
        public Copper(int amountFrom, int amountTo) : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Copper(int amount) : base(0xEF0)
        {
            Stackable = true;
            Amount = amount;
            Name = "Copper Coin";
            Hue = 2236;
        }

        public Copper(Serial serial) : base(serial)
        {
        }

        public override int GetDropSound()
        {
            if (Amount <= 1) return 0x2E4;
            if (Amount <= 5) return 0x2E5;
            return 0x2E6;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
        }
    }
}
