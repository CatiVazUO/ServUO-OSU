using System;
using Server;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoPreviewMarker : Item
    {
        private int m_OwnerSerial;

        [Constructable]
        public ReinoPreviewMarker() : this(0, 0x1766, 0)
        {
        }

        public ReinoPreviewMarker(int ownerSerial, int itemId, int hue) : base(itemId)
        {
            m_OwnerSerial = ownerSerial;
            Movable = false;
            Visible = false;
            Hue = hue;
            Name = "marcador temporário";
        }

        public ReinoPreviewMarker(Serial serial) : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int OwnerSerial
        {
            get { return m_OwnerSerial; }
            set { m_OwnerSerial = value; }
        }

        protected override Packet GetWorldPacketFor(NetState state)
        {
            Mobile mob = state != null ? state.Mobile : null;

            if (mob != null && mob.Serial.Value == m_OwnerSerial)
                return new OwnerItemPacket(this);

            return null;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_OwnerSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
                m_OwnerSerial = reader.ReadInt();

            Timer.DelayCall(TimeSpan.Zero, Delete);
        }

        public sealed class OwnerItemPacket : Packet
        {
            public OwnerItemPacket(Item item)
                : base(0x1A)
            {
                EnsureCapacity(20);

                uint serial = (uint)item.Serial.Value;
                int itemID = item.ItemID;
                int amount = item.Amount;
                Point3D loc = item.Location;
                int x = loc.X;
                int y = loc.Y;
                int hue = item.Hue;
                int flags = 0;
                int direction = (int)item.Direction;

                if (amount != 0)
                    serial |= 0x80000000;
                else
                    serial &= 0x7FFFFFFF;

                m_Stream.Write((uint)serial);
                m_Stream.Write((short)(itemID & TileData.MaxItemValue));

                if (amount != 0)
                    m_Stream.Write((short)amount);

                x &= 0x7FFF;

                if (direction != 0)
                    x |= 0x8000;

                m_Stream.Write((short)x);

                y &= 0x3FFF;

                if (hue != 0)
                    y |= 0x8000;

                if (flags != 0)
                    y |= 0x4000;

                m_Stream.Write((short)y);

                if (direction != 0)
                    m_Stream.Write((byte)direction);

                m_Stream.Write((sbyte)loc.Z);

                if (hue != 0)
                    m_Stream.Write((ushort)hue);

                if (flags != 0)
                    m_Stream.Write((byte)flags);
            }
        }
    }
}
