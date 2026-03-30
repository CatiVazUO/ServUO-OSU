using System.Collections.Generic;
using Server.ContextMenus;
using Server.Mobiles;

namespace Server.Custom.Mobiles
{
    public class BaseNoTradeVendor : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        public override bool IsActiveVendor { get { return false; } }

        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        [Constructable]
        public BaseNoTradeVendor(string title) : base(title)
        {
        }

        public BaseNoTradeVendor(Serial serial) : base(serial)
        {
        }

        public override void InitSBInfo()
        {
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
