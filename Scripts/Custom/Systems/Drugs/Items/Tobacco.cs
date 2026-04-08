using System;
using Server.Mobiles;

namespace Server.Items
{
    public class Tobacco : Item
    {
        [Constructable]
        public Tobacco(int amount)
            : base(3193)
        {
            Name = "tabaco";
            Hue = 2581;
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
        }

        [Constructable]
        public Tobacco()
            : this(1)
        {
        }

        public Tobacco(Serial serial)
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
            from.AddToBackpack(new Cigarette());
            from.SendMessage("Você enrola um cigarro.");
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
