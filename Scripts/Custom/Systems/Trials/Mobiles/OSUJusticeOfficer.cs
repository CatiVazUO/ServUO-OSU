using System;
using Server;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class OSUJusticeOfficer : OSUCityGuard
    {
        private bool m_Busy;

        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId
        {
            get { return CityId; }
            set
            {
                CityId = value;
                ApplyUniform();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Busy
        {
            get { return m_Busy; }
            set { m_Busy = value; }
        }

        [Constructable]
        public OSUJusticeOfficer() : base(0, ReinoGuardKind.Oficial)
        {
            Name = "Oficial de Justiça";
            Title = String.Empty;
            Blessed = false;
            CantWalk = false;
        }

        public OSUJusticeOfficer(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoTrialsSystem.CanAccessTribunalControl(pm, CityId))
            {
                pm.SendMessage("Você não tem acesso ao tribunal deste reino.");
                return;
            }

            pm.SendGump(new ReinoTribunalGump(pm, CityId));
        }

        public override void OnThink()
        {
            if (m_Busy)
                return;

            if (Map != null && Map != Map.Internal && (PostLocation == Point3D.Zero || (PostLocation.X == 0 && PostLocation.Y == 0 && PostLocation.Z == 0)))
                PostLocation = Location;

            base.OnThink();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_Busy);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Busy = version >= 0 && reader.ReadBool();
            m_Busy = false;
        }
    }
}
