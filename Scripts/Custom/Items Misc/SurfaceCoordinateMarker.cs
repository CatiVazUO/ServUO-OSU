using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public class SurfaceCoordinateMarker : Item
    {
        // Ajuste estes limites para combinar com a sua superfície custom.
        public static int SurfaceMinX = 0;
        public static int SurfaceMinY = 0;
        public static int SurfaceMaxX = 5115;
        public static int SurfaceMaxY = 4095;

        [Constructable]
        public SurfaceCoordinateMarker() : base(0x14EB)
        {
            Weight = 1.0;
            Name = "marcador de coordenadas";
        }

        public SurfaceCoordinateMarker(Serial serial) : base(serial)
        {
        }

        private static bool IsAllowedSurface(Mobile m)
        {
            if (m == null || m.Deleted || m.Map == null || m.Map == Map.Internal)
                return false;

            return m.X >= SurfaceMinX && m.X <= SurfaceMaxX && m.Y >= SurfaceMinY && m.Y <= SurfaceMaxY;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            if (!IsChildOf(from.Backpack) && RootParent != from)
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            if (!IsAllowedSurface(from))
            {
                from.SendMessage("Este item só funciona na superfície.");
                return;
            }

            from.SendMessage("Suas coordenadas são X: {0}, Y: {1}.", from.X, from.Y);
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
