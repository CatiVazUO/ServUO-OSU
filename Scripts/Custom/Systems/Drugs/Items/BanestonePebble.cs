using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class BanestonePebble : Item
    {
        [Constructable]
        public BanestonePebble(int amount)
            : base(2514)
        {
            Name = "pedra de banestone";
            Hue = 2989;
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
        }

        [Constructable]
        public BanestonePebble()
            : this(1)
        {
        }

        public BanestonePebble(Serial serial)
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

            from.SendMessage("Selecione um almofariz para moer a pedra.");
            from.Target = new PebbleTarget(this);
        }

        private class PebbleTarget : Target
        {
            private readonly BanestonePebble m_Pebble;

            public PebbleTarget(BanestonePebble pebble)
                : base(12, false, TargetFlags.None)
            {
                m_Pebble = pebble;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Pebble == null || m_Pebble.Deleted || from.Backpack == null || !m_Pebble.IsChildOf(from.Backpack))
                    return;

                MortarPestle mortar = targeted as MortarPestle;
                if (mortar == null)
                {
                    from.SendMessage("Você precisa selecionar um almofariz.");
                    return;
                }

                if (!mortar.IsChildOf(from.Backpack))
                {
                    from.SendMessage("O almofariz precisa estar na sua mochila.");
                    return;
                }

                m_Pebble.Consume(1);
                from.AddToBackpack(new BanestoneAsh());
                from.SendMessage("Você mói a pedra até virar uma cinza fina.");
            }
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
