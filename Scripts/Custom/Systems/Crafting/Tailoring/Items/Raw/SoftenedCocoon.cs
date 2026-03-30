using Server.Targeting;
using System;
using Server.Items;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Raw
{
    public class SoftenedCocoon : Item, IDyable
    {
        [Constructable]
        public SoftenedCocoon() : this(1) { }

        [Constructable]
        public SoftenedCocoon(int amount) : base(0x10D9)
        {
            Stackable = true;
            Amount = amount;
            Weight = 2.0;
            Name = "casulo amolecido";
        }

        public SoftenedCocoon(Serial serial) : base(serial) { }

        public static void OnSpun(ISpinningWheel wheel, Mobile from, int hue)
        {
            // Mantém o mesmo padrão do algodão/linho: cada uso gera 6 unidades
            Item item = new SpoolOfSilk(1);
            item.Hue = hue;

            from.AddToBackpack(item);
            from.SendMessage("Você coloca o novelo de seda na sua bolsa"); // You put the spools of thread in your backpack.
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
            private readonly SoftenedCocoon m_SilkCocoon;
            public PickWheelTarget(SoftenedCocoon silkcocoon)
                : base(3, false, TargetFlags.None)
            {
                m_SilkCocoon = silkcocoon;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_SilkCocoon.Deleted)
                    return;

                ISpinningWheel wheel = targeted as ISpinningWheel;

                if (wheel == null && targeted is AddonComponent)
                    wheel = ((AddonComponent)targeted).Addon as ISpinningWheel;

                if (wheel is Item)
                {
                    Item item = (Item)wheel;

                    if (!m_SilkCocoon.IsChildOf(from.Backpack))
                    {
                        from.SendMessage("Isso precisa estar na sua bolsa para ser usado"); // That must be in your pack for you to use it.
                    }
                    else if (wheel.Spinning)
                    {
                        from.SendMessage("Essa roda de fiar está sendo usada"); // That spinning wheel is being used.
                    }
                    else
                    {
                        m_SilkCocoon.Consume();

                        // IMPORTANTE: precisa usar a callback da própria seda.
                        // Se chamar Cotton.OnSpun, vira spool comum e o tear gera bolt comum.
                        wheel.BeginSpin(new SpinCallback(OnSpun), from, m_SilkCocoon.Hue);
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
