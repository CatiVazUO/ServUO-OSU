using System;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Cloths;
using Server.Targeting;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Raw;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Stations
{
    public class TanningSolution : Item
    {
        [Constructable]
        public TanningSolution() : base(0xE24)
        {
            Stackable = true;
            Name = "solução de curtume";
        }

        public TanningSolution(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("Isso precisa estar na sua mochila.");
                return;
            }

            from.SendMessage("Escolha a pele seca para tratar.");
            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private readonly TanningSolution _sol;

            public InternalTarget(TanningSolution sol) : base(2, false, TargetFlags.None)
            {
                _sol = sol;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_sol.Deleted)
                    return;

                Item it = targeted as Item;
                if (it == null || it.Deleted)
                    return;

                if (!it.IsChildOf(from.Backpack))
                {
                    from.SendMessage("A pele precisa estar na sua mochila.");
                    return;
                }

                if (it is DriedSkin)
                {
                    int amt = it.Amount;
                    it.Delete();

                    _sol.Consume(); // consome 1

                    from.AddToBackpack(new SkinCloth(amt));
                    from.SendMessage("Você trata a pele e cria tecido de pele.");
                }
                else
                {
                    from.SendMessage("Isso não é uma pele seca.");
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
