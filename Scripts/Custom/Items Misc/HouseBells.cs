using Server;
using Server.Network;
using System;

namespace Server.Items
{
    [Flipable(0x4C5C, 0x4C5D)]
    public class HouseBellSmall : Item
    {
        [Constructable]
        public HouseBellSmall()
            : base(0x4C5C)
        {
            Weight = 1.0;
        }

        public HouseBellSmall(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Deleted)
                return;

            Map map = Map;
            Point3D loc = GetWorldLocation();

            if (map == null || map == Map.Internal)
            {
                from.PlaySound(0x5BD);
                from.PublicOverheadMessage(MessageType.Emote, 0x3B2, false, "*toca sino*");
                return;
            }

            from.PublicOverheadMessage(MessageType.Emote, 0x3B2, false, "*toca sino*");
            Effects.PlaySound(loc, map, 0x5BD);
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

    [Flipable(0x4C5E, 0x4C5F)]
    public class HouseBellLarge : Item
    {
        [Constructable]
        public HouseBellLarge()
            : base(0x4C5E)
        {
            Weight = 2.0;
        }

        public HouseBellLarge(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Deleted)
                return;

            Map map = Map;
            Point3D loc = GetWorldLocation();

            if (map == null || map == Map.Internal)
            {
                from.PublicOverheadMessage(MessageType.Emote, 0x3B2, false, "*toca sino*");
                from.PlaySound(0x5BF);
                return;
            }

            from.PublicOverheadMessage(MessageType.Emote, 0x3B2, false, "*toca sino*");
            Effects.PlaySound(loc, map, 0x5BF);
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
