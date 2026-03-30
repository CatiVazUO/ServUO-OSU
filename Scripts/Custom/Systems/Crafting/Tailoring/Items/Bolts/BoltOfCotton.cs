using System;
using Server.Engines.Craft;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Cloths;
using Server.Items;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bolts
{
    public class BoltOfCotton : Item, IScissorable
    {
        [Constructable]
        public BoltOfCotton() : base(0xF95)
        {
            Weight = 5.0;
            Name = "rolo de algodão";
        }

        public BoltOfCotton(Serial serial) : base(serial) { }

        bool IScissorable.Scissor(Mobile from, Scissors scissors)
        {
            if (Deleted || !from.CanSee(this))
                return false;

            // Mesmo “yield” do bolt padrão: normalmente 50 cloth
            from.AddToBackpack(new CottonCloth(50));
            Delete();
            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
