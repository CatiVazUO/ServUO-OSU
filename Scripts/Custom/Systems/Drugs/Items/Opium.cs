using System;
using Server.Mobiles;

namespace Server.Items
{
    public class Opium : Item
    {
        [Constructable]
        public Opium(int amount)
            : base(0x103D)
        {
            Name = "ópio";
            Hue = 2017;
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
        }

        [Constructable]
        public Opium()
            : this(1)
        {
        }

        public Opium(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Backpack == null || !IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            from.SendMessage("Use essa substância em um cachimbo vazio.");
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
