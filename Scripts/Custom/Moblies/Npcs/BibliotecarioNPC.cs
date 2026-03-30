using Server.ContextMenus;
using Server.Custom.Mobiles;
using Server.Custom.Systems.Biblioteca.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.Biblioteca.Mobiles
{
    public class Bibliotecario : BaseNoTradeVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        [Constructable]
        public Bibliotecario()
            : base("bibliotecário")
        {
            CanMove = false;
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
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
