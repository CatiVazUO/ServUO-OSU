using System;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Raw;
using Server.Items.Resource;
using Server.Targeting;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Stations
{
    public class HideDryingRack : Item
    {
        [Constructable]
        public HideDryingRack() : base(0x10E7)
        {
            Name = "estendedouro de peles";
            Movable = false;
        }

        public HideDryingRack(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446);
                return;
            }

            from.SendMessage("Escolha a pele crua para secar.");
            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private readonly HideDryingRack _rack;

            public InternalTarget(HideDryingRack rack) : base(2, false, TargetFlags.None)
            {
                _rack = rack;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_rack.Deleted)
                    return;

                if (!from.InRange(_rack.GetWorldLocation(), 2))
                {
                    from.SendLocalizedMessage(500446);
                    return;
                }

                Item it = targeted as Item;
                if (it == null || it.Deleted)
                    return;

                if (it is RawSkin)
                {
                    int amt = it.Amount;
                    it.Delete();
                    from.AddToBackpack(new DriedSkin(amt));
                    from.SendMessage("Você seca a pele no estendedouro.");
                }
                else
                {
                    from.SendMessage("Isso não pode ser seco aqui.");
                }
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
