using Server;
using Server.Custom.Systems.Postos;
using Server.Mobiles;
using System;

namespace Server.Items
{
    public class PostoThreatDummy : Item
    {
        private string m_PostoId;
        private string m_ThreatTypeName;

        [CommandProperty(AccessLevel.GameMaster)]
        public string PostoId
        {
            get { return m_PostoId; }
            set { m_PostoId = value ?? String.Empty; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ThreatTypeName
        {
            get { return m_ThreatTypeName; }
            set { m_ThreatTypeName = value ?? String.Empty; }
        }

        [Constructable]
        public PostoThreatDummy() : base(0xD15)
        {
            Name = "ameaça de posto";
            Movable = false;
            m_PostoId = String.Empty;
            m_ThreatTypeName = "DummyThreat";
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null || pm.Deleted)
                return;

            if (!from.InRange(Location, 2))
            {
                from.SendLocalizedMessage(500446);
                return;
            }

            PostoSystem.NotifyItemDestroyed(pm, m_PostoId, m_ThreatTypeName);
            pm.SendMessage("Você destruiu a ameaça vinculada ao posto {0}.", m_PostoId);
            Delete();
        }

        public PostoThreatDummy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(m_PostoId);
            writer.Write(m_ThreatTypeName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_PostoId = reader.ReadString();
            m_ThreatTypeName = reader.ReadString();
        }
    }
}
