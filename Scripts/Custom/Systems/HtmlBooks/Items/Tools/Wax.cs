using Server;
using System;

namespace Server.Items
{
    public class Wax : Item
    {
        [Constructable]
        public Wax() : this(1)
        {
        }

        [Constructable]
        public Wax(int amount) : base(0x142B)
        {
            Name = "Cera Quente";
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
        }

        public Wax(Serial serial) : base(serial)
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
