using System;
using Server.Targeting;

namespace Server.Items.Resource
{
    public class Flax : Item
    {
        [Constructable]
        public Flax()
            : this(1)
        {
        }

        [Constructable]
        public Flax(int amount)
            : base(0x1A9C)
        {
            Stackable = true;
            Weight = 2.0;
            Amount = amount;
            Name = "linho";
        }

        public Flax(Serial serial)
            : base(serial)
        {
        }

        public static void OnSpun(ISpinningWheel wheel, Mobile from, int hue)
        {
            // Linho deve gerar spool próprio, para o tear criar BoltOfLinen
            Item item = new SpoolOfLinen(1);
            item.Hue = hue;

            from.AddToBackpack(item);
            from.SendMessage("Você coloca o novelo de linho na sua bolsa"); // You put the spools of thread in your backpack.
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.SendMessage("Em que roda de fiar você deseja usar isso?"); // What spinning wheel do you wish to spin this on?
                from.Target = new PickWheelTarget(this);
            }
            else
            {
                from.SendMessage("Isso precisa estar na sua bolsa para que você use"); // That must be in your pack for you to use it.
            }
        }

        private class PickWheelTarget : Target
        {
            private readonly Flax m_Flax;
            public PickWheelTarget(Flax flax)
                : base(3, false, TargetFlags.None)
            {
                m_Flax = flax;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Flax.Deleted)
                    return;

                ISpinningWheel wheel = targeted as ISpinningWheel;

                if (wheel == null && targeted is AddonComponent)
                    wheel = ((AddonComponent)targeted).Addon as ISpinningWheel;

                if (wheel is Item)
                {
                    Item item = (Item)wheel;

                    if (!m_Flax.IsChildOf(from.Backpack))
                    {
                        from.SendMessage("Isso precisa estar na sua bolsa para que você use"); // That must be in your pack for you to use it.
                    }
                    else if (wheel.Spinning)
                    {
                        from.SendMessage("Essa roda de fiar está sendo usada"); // That spinning wheel is being used.
                    }
                    else
                    {
                        m_Flax.Consume();
                        wheel.BeginSpin(new SpinCallback(OnSpun), from, m_Flax.Hue);
                    }
                }
                else
                {
                    from.SendMessage("Use isso numa roda de fiar"); // Use that on a spinning wheel.
                }
            }
        }
    }
}
