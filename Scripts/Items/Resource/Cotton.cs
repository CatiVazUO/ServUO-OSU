using System;
using Server.Targeting;

namespace Server.Items.Resource
{
    public class Cotton : Item, IDyable
    {
        [Constructable]
        public Cotton()
            : this(1)
        {
        }

        [Constructable]
        public Cotton(int amount)
            : base(0xDF9)
        {
            Stackable = true;
            Weight = 2.0;
            Amount = amount;
            Name = "algodão";
        }

        public Cotton(Serial serial)
            : base(serial)
        {
        }

//        TextDefinition ICommodity.Description { get { return LabelNumber; } }
//        bool ICommodity.IsDeedable { get { return true; } }

        public static void OnSpun(ISpinningWheel wheel, Mobile from, int hue)
        {
            Item item = new SpoolOfCotton(1);
            item.Hue = hue;

            from.AddToBackpack(item);
            from.SendMessage("Você coloca o novelo de algodão na sua bolsa"); // You put the spools of thread in your backpack.
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

        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;

            return true;
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
            private readonly Cotton m_Cotton;
            public PickWheelTarget(Cotton cotton)
                : base(3, false, TargetFlags.None)
            {
                m_Cotton = cotton;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Cotton.Deleted)
                    return;

                ISpinningWheel wheel = targeted as ISpinningWheel;

                if (wheel == null && targeted is AddonComponent)
                    wheel = ((AddonComponent)targeted).Addon as ISpinningWheel;

                if (wheel is Item)
                {
                    Item item = (Item)wheel;

                    if (!m_Cotton.IsChildOf(from.Backpack))
                    {
                        from.SendMessage("Isso precisa estar na sua bolsa para ser usado"); // That must be in your pack for you to use it.
                    }
                    else if (wheel.Spinning)
                    {
                        from.SendMessage("Essa roda de fiar está sendo usada"); // That spinning wheel is being used.
                    }
                    else
                    {
                        m_Cotton.Consume();
                        wheel.BeginSpin(new SpinCallback(OnSpun), from, m_Cotton.Hue);
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
