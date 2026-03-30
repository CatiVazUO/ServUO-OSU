using System;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Needs;
using Server.Custom.Systems.Needs.Gumps;

namespace Server.Custom.Systems.Crafting.Cooking.Consumables.Drinks
{
    public class Cantil : BaseBeverage
    {
        public override int MaxQuantity
        {
            get { return 2; }
        }

        public override int ComputeItemID()
        {
            return 0x13F3; // gráfico do flask
        }

        [Constructable]
        public Cantil()
        {
            Name = "cantil";
            Weight = 1.0;
            Hue = 0;
        }

        public Cantil(Serial serial) : base(serial)
        {
        }

        public override void Fill_OnTarget(Mobile from, object targ)
        {
            // Impede encher de outras bebidas. Só água de fontes/corpos d’água/etc.
           // if (targ is BaseBeverage)
           // {
           //     from.SendMessage("Você só pode encher esse cantil com água.");
           //     return;
           // }

            base.Fill_OnTarget(from, targ);
        }

        public override void Pour_OnTarget(Mobile from, object targ)
        {
            if (targ == from)
            {
                PlayerMobile pm = from as PlayerMobile;

                if (pm == null)
                    return;

                if (IsEmpty)
                {
                    pm.SendMessage("O cantil está vazio.");
                    return;
                }

                // 4 por gole = 20 no total quando o flask está cheio
                if (!OSUNeedsSystem.TryAddThirst(pm, 20))
                {
                    pm.SendMessage("Você está satisfeito demais para beber agora.");
                    return;
                }

                pm.PlaySound(Utility.RandomList(0x30, 0x31, 0x2D6));
                pm.SendMessage("Você toma um gole de água.");

                Quantity -= 1;

                OSUNeedsGump.TryRefresh(pm);
                return;
            }

            from.PlaySound(0x025);
            base.Pour_OnTarget(from, targ);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
