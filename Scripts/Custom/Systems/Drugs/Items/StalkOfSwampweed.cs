using System;
using Server.Mobiles;
using Server.Custom.Reinos;

namespace Server.Items
{
    public class StalkOfSwampweed : BaseSmokable
    {
        [Constructable]
        public StalkOfSwampweed()
            : base(12636, 3)
        {
            Hue = 246;
            Name = "cigarro de swampweed";
            Weight = 0.1;
            ContentType = ContentType.Swampweed;
            Stackable = false;
        }

        public StalkOfSwampweed(Serial serial)
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
                from.SendMessage("Não resta nada para fumar.");
                return;
            }

            OnSmoke(from);
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
