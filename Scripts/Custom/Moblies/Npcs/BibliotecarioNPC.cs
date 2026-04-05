using Server.ContextMenus;
using Server.Commands;
using Server.Custom.Mobiles;
using Server.Custom.Systems.Reinos;
using Server.Custom.Biblioteca.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Custom.Biblioteca
{
    public class Bibliotecario : BaseNoTradeVendor
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
        public Bibliotecario()
            : base("bibliotecário")
        {
            CanMove = false;
            m_GovernmentCityId = -1;
        }

        public Bibliotecario(Serial serial) : base(serial)
        {
        }

        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            pm.CloseGump(typeof(GumpLibrary));
            pm.SendGump(new GumpLibrary(pm, this));
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBNoVendor());
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
