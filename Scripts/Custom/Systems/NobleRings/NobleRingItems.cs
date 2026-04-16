using System;
using Server;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class NobleRingBase : BaseRing
    {
        private int m_VisualGumpId;
        private bool m_IsLeaderRing;
        private int m_LeaderCityId;
        private string m_MetalName;

        [CommandProperty(AccessLevel.GameMaster)]
        public int VisualGumpId
        {
            get { return m_VisualGumpId; }
            set
            {
                m_VisualGumpId = NormalizeVisualGumpId(value);
                ApplyVisualMaterial();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsLeaderRing
        {
            get { return m_IsLeaderRing; }
            set
            {
                m_IsLeaderRing = value;
                ApplyLeaderState();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LeaderCityId
        {
            get { return m_LeaderCityId; }
            set
            {
                m_LeaderCityId = value;
                ApplyLeaderState();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string MetalName
        {
            get { return m_MetalName ?? String.Empty; }
            set { m_MetalName = value ?? String.Empty; InvalidateProperties(); }
        }

        protected NobleRingBase(int itemId, int visualGumpId, string metalName) : base(itemId)
        {
            Weight = 1.0;
            Movable = true;
            m_VisualGumpId = NormalizeVisualGumpId(visualGumpId);
            m_MetalName = metalName ?? String.Empty;
            m_LeaderCityId = -1;
            ApplyVisualMaterial();
            Configure(false, -1);
        }

        public NobleRingBase(Serial serial) : base(serial)
        {
        }

        public static int NormalizeVisualGumpId(int gumpId)
        {
            if (gumpId < 3010 || gumpId > 3069)
                return 3010;

            return gumpId;
        }

        public void Configure(bool isLeaderRing, int leaderCityId)
        {
            m_IsLeaderRing = isLeaderRing;
            m_LeaderCityId = isLeaderRing ? leaderCityId : -1;
            ApplyVisualMaterial();
            ApplyLeaderState();
        }

        protected virtual void ApplyVisualMaterial()
        {
        }

        protected virtual void ApplyLeaderState()
        {
            Name = m_IsLeaderRing ? "anel do líder" : "anel nobre";
            LootType = m_IsLeaderRing ? LootType.Blessed : LootType.Regular;
            InvalidateProperties();
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (m_IsLeaderRing && m_LeaderCityId >= 0)
                list.Add("símbolo de autoridade do reino");
            else
                list.Add("anel de nobreza");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null || Deleted)
                return;

            if (RootParent != from)
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            from.CloseGump(typeof(NobleRingDisplayGump));
            from.SendGump(new NobleRingDisplayGump(this));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_VisualGumpId);
            writer.Write(m_IsLeaderRing);
            writer.Write(m_LeaderCityId);
            writer.Write(m_MetalName ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_VisualGumpId = 3010;
            m_IsLeaderRing = false;
            m_LeaderCityId = -1;
            m_MetalName = String.Empty;

            if (version >= 1)
            {
                m_VisualGumpId = NormalizeVisualGumpId(reader.ReadInt());
                m_IsLeaderRing = reader.ReadBool();
                m_LeaderCityId = reader.ReadInt();
                m_MetalName = reader.ReadString();
            }

            ApplyVisualMaterial();
            ApplyLeaderState();
        }
    }

    public abstract class NobleRingMetalBase : NobleRingBase
    {
        protected NobleRingMetalBase(int itemId, int visualGumpId, string metalName) : base(itemId, visualGumpId, metalName)
        {
        }

        public NobleRingMetalBase(Serial serial) : base(serial)
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
            reader.ReadInt();
        }
    }

    public class NobleRingBronze : NobleRingMetalBase
    {
        [Constructable]
        public NobleRingBronze() : this(3011)
        {
        }

        public NobleRingBronze(int visualGumpId) : base(0x2BE9, visualGumpId, "bronze")
        {
        }

        public NobleRingBronze(Serial serial) : base(serial)
        {
        }

        protected override void ApplyVisualMaterial()
        {
            ItemID = 0x2BE9;
            MetalName = "bronze";
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

    public class NobleRingSilver : NobleRingMetalBase
    {
        [Constructable]
        public NobleRingSilver() : this(3010)
        {
        }

        public NobleRingSilver(int visualGumpId) : base(0x2BEA, visualGumpId, "prata")
        {
        }

        public NobleRingSilver(Serial serial) : base(serial)
        {
        }

        protected override void ApplyVisualMaterial()
        {
            ItemID = 0x2BEA;
            MetalName = "prata";
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

    public class NobleRingGold : NobleRingMetalBase
    {
        [Constructable]
        public NobleRingGold() : this(3012)
        {
        }

        public NobleRingGold(int visualGumpId) : base(0x2BEB, visualGumpId, "ouro")
        {
        }

        public NobleRingGold(Serial serial) : base(serial)
        {
        }

        protected override void ApplyVisualMaterial()
        {
            ItemID = 0x2BEB;
            MetalName = "ouro";
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

    public static class NobleRingFactory
    {
        private const string MetalPattern = "PBOOOOPOOPPPOPOPOPOBOPBBOOPOOPOPOOOOPPPPOOOOOOPPOOPPOBOPPBPO";

        public static char GetMetalCode(int gumpId)
        {
            gumpId = NobleRingBase.NormalizeVisualGumpId(gumpId);
            return MetalPattern[gumpId - 3010];
        }

        public static NobleRingBase Create(int gumpId, bool isLeaderRing, int leaderCityId)
        {
            gumpId = NobleRingBase.NormalizeVisualGumpId(gumpId);

            NobleRingBase ring;
            switch (GetMetalCode(gumpId))
            {
                case 'B':
                    ring = new NobleRingBronze(gumpId);
                    break;
                case 'O':
                    ring = new NobleRingGold(gumpId);
                    break;
                default:
                    ring = new NobleRingSilver(gumpId);
                    break;
            }

            ring.VisualGumpId = gumpId;
            ring.Configure(isLeaderRing, leaderCityId);
            return ring;
        }
    }

    public class NobleRingDisplayGump : Gump
    {
        public NobleRingDisplayGump(NobleRingBase ring) : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            int gumpId = ring != null ? NobleRingBase.NormalizeVisualGumpId(ring.VisualGumpId) : 3010;
            string label = ring != null && ring.IsLeaderRing ? "Anel do Líder" : "Anel Nobre";

            AddPage(0);
            AddImageTiled(338, 159, 176, 116, 392);
            AddImageTiled(318, 213, 78, 89, 359);
            AddImageTiled(463, 213, 74, 90, 360);
            AddImageTiled(463, 136, 74, 82, 361);
            AddImageTiled(318, 136, 74, 90, 362);
            AddImageTiled(389, 272, 74, 31, 367);
            AddImageTiled(384, 138, 79, 31, 368);
            AddLabel(390, 252, 1152, label);
            AddImage(382, 167, gumpId);
        }
    }
}
