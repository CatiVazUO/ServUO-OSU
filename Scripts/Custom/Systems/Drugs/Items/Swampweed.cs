using System;
using Server.Mobiles;

namespace Server.Items
{
    public class Swampweed : Item
    {
        [Constructable]
        public Swampweed(int amount)
            : base(0x18E7)
        {
            Name = "swampweed";
            Hue = 246;
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
        }

        [Constructable]
        public Swampweed()
            : this(1)
        {
        }

        public Swampweed(Serial serial)
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

            Consume(1);
            from.AddToBackpack(new StalkOfSwampweed());
            from.SendMessage("Você prepara um rolo de swampweed.");
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
