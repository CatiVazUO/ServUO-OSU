using System;
using Server.Mobiles;
using Server.Custom.Reinos;

namespace Server.Items
{
    public class Qat : BaseChewable
    {
        [Constructable]
        public Qat()
            : base(0x1E01, 2)
        {
            Hue = 1454;
            Name = "qat";
            Weight = 0.2;
            Chewable = Chewable.Qat;
            Stackable = false;
        }

        public Qat(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (RootParent != from || !(from is PlayerMobile))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            if (ChewableRemaining <= 0)
            {
                from.SendMessage("Não resta nada para mastigar.");
                return;
            }

            OnChew(from);
            ReinoMilitarySystem.NotifyDrugUse(from);
            ChewableRemaining--;

            if (ChewableRemaining <= 0)
                Delete();
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
