using System;
using System.Collections.Generic;
using Server;
using Server.Custom.Mobiles;
using Server.Custom.Systems.Stables.Engine;
using Server.Custom.Systems.Stables.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Stables.Mobiles
{
    public class OSUStableMaster : BaseNoTradeVendor
    {
        private int m_GovernmentCityId;
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId
        {
            get { return m_GovernmentCityId; }
            set { m_GovernmentCityId = value; InvalidateProperties(); }
        }

        [Constructable]
        public OSUStableMaster() : base("estalajadeiro do estábulo")
        {
            Name = "mestre do estábulo";
            CanMove = false;
            m_GovernmentCityId = -1;
        }

        public OSUStableMaster(Serial serial) : base(serial)
        {
        }

        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            pm.CloseGump(typeof(OSUStableMasterGump));
            pm.SendGump(new OSUStableMasterGump(pm, this));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_GovernmentCityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 1)
                m_GovernmentCityId = reader.ReadInt();
            else
                m_GovernmentCityId = -1;
        }
    }
}