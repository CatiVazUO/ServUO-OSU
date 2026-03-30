using System;
using Server.Targeting;

namespace Server.Items.Resource
{
    public class Wool : Item, IDyable
    {
        [Constructable]
        public Wool()
            : this(1)
        {
        }

        [Constructable]
        public Wool(int amount)
            : base(0xDF8)
        {
            Stackable = true;
            Weight = 4.0;
            Amount = amount;
            Name = "lã bruta";
        }

        public Wool(Serial serial)
            : base(serial)
        {
        }

     //   TextDefinition ICommodity.Description { get { return LabelNumber; } }
     //   bool ICommodity.IsDeedable { get { return true; } }

        public static void OnSpun(ISpinningWheel wheel, Mobile from, int hue)
        {
            Item item = new SpoolOfWool(1);
            item.Hue = hue;

            from.AddToBackpack(item);
            from.SendMessage("Você coloca os novelos de lã na sua bolsa"); // You put the balls of yarn in your backpack.
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
            private readonly Wool m_Wool;
            public PickWheelTarget(Wool wool)
                : base(3, false, TargetFlags.None)
            {
                m_Wool = wool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Wool.Deleted)
                    return;

                ISpinningWheel wheel = targeted as ISpinningWheel;

                if (wheel == null && targeted is AddonComponent)
                    wheel = ((AddonComponent)targeted).Addon as ISpinningWheel;

                if (wheel is Item)
                {
                    Item item = (Item)wheel;

                    if (!m_Wool.IsChildOf(from.Backpack))
                    {
                        from.SendMessage("Isso precisa estar na sua bolsa para ser usado"); // That must be in your pack for you to use it.
                    }
                    else if (wheel.Spinning)
                    {
                        from.SendMessage("Essa roda de fiar está sendo usada"); // That spinning wheel is being used.
                    }
                    else
                    {
                        m_Wool.Consume();
                        if (m_Wool is TaintedWool)
                            wheel.BeginSpin(new SpinCallback(TaintedWool.OnSpun), from, m_Wool.Hue);
                        else
                            wheel.BeginSpin(new SpinCallback(OnSpun), from, m_Wool.Hue);
                    }
                }
                else
                {
                    from.SendMessage("Use isso numa roda de fiar"); // Use that on a spinning wheel.
                }
            }
        }
    }

    public class TaintedWool : Wool
    {
        [Constructable]
        public TaintedWool()
            : this(1)
        {
        }

        [Constructable]
        public TaintedWool(int amount)
            : base(0x101F)
        {
            Stackable = true;
            Weight = 4.0;
            Amount = amount;
        }

        public TaintedWool(Serial serial)
            : base(serial)
        {
        }

        new public static void OnSpun(ISpinningWheel wheel, Mobile from, int hue)
        {
            Item item = new DarkYarn(1);
            item.Hue = hue;

            from.AddToBackpack(item);
            from.SendLocalizedMessage(1010574); // You put a ball of yarn in your backpack.
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
    }
}
