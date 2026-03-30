using Server.Items.Resource;
using Server.Targeting;
using System;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Raw;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Stations
{
    public class HotWaterVat : Item
    {
        [Constructable]
        public HotWaterVat() : base(0x184A)
        {
            Name = "caldeirão de água quente";
            Movable = false;
        }

        public HotWaterVat(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446);
                return;
            }

            from.SendMessage("Escolha a seda crua para amolecer.");
            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private readonly HotWaterVat _vat;

            public InternalTarget(HotWaterVat vat) : base(2, false, TargetFlags.None)
            {
                _vat = vat;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_vat.Deleted)
                    return;

                if (!from.InRange(_vat.GetWorldLocation(), 2))
                {
                    from.SendLocalizedMessage(500446);
                    return;
                }

                Item it = targeted as Item;
                if (it == null || it.Deleted)
                    return;

                if (it is SilkCocoon)
                {
                    int amt = it.Amount;
                    it.Delete();
                    from.AddToBackpack(new SoftenedCocoon(amt));
                    from.SendMessage("Você amolece a seda na água quente.");
                }
                else
                {
                    from.SendMessage("Isso não pode ser amolecido aqui.");
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
