using System;
using Server.Mobiles;
using Server.Custom.Reinos;

namespace Server.Items
{
    public class BanestoneAsh : BaseSnortable
    {
        [Constructable]
        public BanestoneAsh()
            : base(3983, 2)
        {
            Hue = 2989;
            Name = "cinza de banestone";
            Weight = 0.1;
            ContentType2 = ContentType2.Banestone;
            Stackable = false;
        }

        public BanestoneAsh(Serial serial)
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

            if (ContentRemaining <= 0)
            {
                from.SendMessage("Não resta nada para cheirar.");
                return;
            }

            OnSnort(from);
            ReinoMilitarySystem.NotifyDrugUse(from);
            ContentRemaining--;

            if (ContentRemaining <= 0)
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
