using System;
using Server;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class NobleRingBase : BaseRing
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int VisualGumpId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsLeaderRing { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LeaderCityId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string MetalName { get; set; }

        protected NobleRingBase(int itemId, int visualGumpId, string metalName) : base(itemId)
        {
            VisualGumpId = NormalizeVisualGumpId(visualGumpId);
            LeaderCityId = -1;
            MetalName = metalName ?? String.Empty;
            Weight = 1.0;
            Movable = true;
            Configure(false, -1);
        }

        protected NobleRingBase(Serial serial) : base(serial)
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
            IsLeaderRing = isLeaderRing;
            LeaderCityId = isLeaderRing ? leaderCityId : -1;
            Name = isLeaderRing ? "anel do líder" : "anel nobre";
            LootType = isLeaderRing ? LootType.Blessed : LootType.Regular;
            InvalidateProperties();
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (IsLeaderRing && LeaderCityId >= 0)
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
            writer.Write(VisualGumpId);
            writer.Write(IsLeaderRing);
            writer.Write(LeaderCityId);
            writer.Write(MetalName ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                default:
                    VisualGumpId = NormalizeVisualGumpId(reader.ReadInt());
                    IsLeaderRing = reader.ReadBool();
                    LeaderCityId = reader.ReadInt();
                    MetalName = reader.ReadString();
                    break;
            }

            Configure(IsLeaderRing, LeaderCityId);
        }
    }

    public abstract class NobleRingMetalBase : NobleRingBase
    {
        protected NobleRingMetalBase(int itemId, int visualGumpId, string metalName) : base(itemId, visualGumpId, metalName)
        {
        }

        protected NobleRingMetalBase(Serial serial) : base(serial)
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

            ring.Configure(isLeaderRing, leaderCityId);
            return ring;
        }
    }

    public class NobleRing3010 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3010() : base(3010)
        {
        }

        public NobleRing3010(Serial serial) : base(serial)
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
            VisualGumpId = 3010;
        }
    }

    public class NobleRing3011 : NobleRingBronze
    {
        [Constructable]
        public NobleRing3011() : base(3011)
        {
        }

        public NobleRing3011(Serial serial) : base(serial)
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
            VisualGumpId = 3011;
        }
    }

    public class NobleRing3012 : NobleRingGold
    {
        [Constructable]
        public NobleRing3012() : base(3012)
        {
        }

        public NobleRing3012(Serial serial) : base(serial)
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
            VisualGumpId = 3012;
        }
    }

    public class NobleRing3013 : NobleRingGold
    {
        [Constructable]
        public NobleRing3013() : base(3013)
        {
        }

        public NobleRing3013(Serial serial) : base(serial)
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
            VisualGumpId = 3013;
        }
    }

    public class NobleRing3014 : NobleRingGold
    {
        [Constructable]
        public NobleRing3014() : base(3014)
        {
        }

        public NobleRing3014(Serial serial) : base(serial)
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
            VisualGumpId = 3014;
        }
    }

    public class NobleRing3015 : NobleRingGold
    {
        [Constructable]
        public NobleRing3015() : base(3015)
        {
        }

        public NobleRing3015(Serial serial) : base(serial)
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
            VisualGumpId = 3015;
        }
    }

    public class NobleRing3016 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3016() : base(3016)
        {
        }

        public NobleRing3016(Serial serial) : base(serial)
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
            VisualGumpId = 3016;
        }
    }

    public class NobleRing3017 : NobleRingGold
    {
        [Constructable]
        public NobleRing3017() : base(3017)
        {
        }

        public NobleRing3017(Serial serial) : base(serial)
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
            VisualGumpId = 3017;
        }
    }

    public class NobleRing3018 : NobleRingGold
    {
        [Constructable]
        public NobleRing3018() : base(3018)
        {
        }

        public NobleRing3018(Serial serial) : base(serial)
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
            VisualGumpId = 3018;
        }
    }

    public class NobleRing3019 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3019() : base(3019)
        {
        }

        public NobleRing3019(Serial serial) : base(serial)
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
            VisualGumpId = 3019;
        }
    }

    public class NobleRing3020 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3020() : base(3020)
        {
        }

        public NobleRing3020(Serial serial) : base(serial)
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
            VisualGumpId = 3020;
        }
    }

    public class NobleRing3021 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3021() : base(3021)
        {
        }

        public NobleRing3021(Serial serial) : base(serial)
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
            VisualGumpId = 3021;
        }
    }

    public class NobleRing3022 : NobleRingGold
    {
        [Constructable]
        public NobleRing3022() : base(3022)
        {
        }

        public NobleRing3022(Serial serial) : base(serial)
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
            VisualGumpId = 3022;
        }
    }

    public class NobleRing3023 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3023() : base(3023)
        {
        }

        public NobleRing3023(Serial serial) : base(serial)
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
            VisualGumpId = 3023;
        }
    }

    public class NobleRing3024 : NobleRingGold
    {
        [Constructable]
        public NobleRing3024() : base(3024)
        {
        }

        public NobleRing3024(Serial serial) : base(serial)
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
            VisualGumpId = 3024;
        }
    }

    public class NobleRing3025 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3025() : base(3025)
        {
        }

        public NobleRing3025(Serial serial) : base(serial)
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
            VisualGumpId = 3025;
        }
    }

    public class NobleRing3026 : NobleRingGold
    {
        [Constructable]
        public NobleRing3026() : base(3026)
        {
        }

        public NobleRing3026(Serial serial) : base(serial)
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
            VisualGumpId = 3026;
        }
    }

    public class NobleRing3027 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3027() : base(3027)
        {
        }

        public NobleRing3027(Serial serial) : base(serial)
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
            VisualGumpId = 3027;
        }
    }

    public class NobleRing3028 : NobleRingGold
    {
        [Constructable]
        public NobleRing3028() : base(3028)
        {
        }

        public NobleRing3028(Serial serial) : base(serial)
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
            VisualGumpId = 3028;
        }
    }

    public class NobleRing3029 : NobleRingBronze
    {
        [Constructable]
        public NobleRing3029() : base(3029)
        {
        }

        public NobleRing3029(Serial serial) : base(serial)
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
            VisualGumpId = 3029;
        }
    }

    public class NobleRing3030 : NobleRingGold
    {
        [Constructable]
        public NobleRing3030() : base(3030)
        {
        }

        public NobleRing3030(Serial serial) : base(serial)
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
            VisualGumpId = 3030;
        }
    }

    public class NobleRing3031 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3031() : base(3031)
        {
        }

        public NobleRing3031(Serial serial) : base(serial)
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
            VisualGumpId = 3031;
        }
    }

    public class NobleRing3032 : NobleRingBronze
    {
        [Constructable]
        public NobleRing3032() : base(3032)
        {
        }

        public NobleRing3032(Serial serial) : base(serial)
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
            VisualGumpId = 3032;
        }
    }

    public class NobleRing3033 : NobleRingBronze
    {
        [Constructable]
        public NobleRing3033() : base(3033)
        {
        }

        public NobleRing3033(Serial serial) : base(serial)
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
            VisualGumpId = 3033;
        }
    }

    public class NobleRing3034 : NobleRingGold
    {
        [Constructable]
        public NobleRing3034() : base(3034)
        {
        }

        public NobleRing3034(Serial serial) : base(serial)
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
            VisualGumpId = 3034;
        }
    }

    public class NobleRing3035 : NobleRingGold
    {
        [Constructable]
        public NobleRing3035() : base(3035)
        {
        }

        public NobleRing3035(Serial serial) : base(serial)
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
            VisualGumpId = 3035;
        }
    }

    public class NobleRing3036 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3036() : base(3036)
        {
        }

        public NobleRing3036(Serial serial) : base(serial)
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
            VisualGumpId = 3036;
        }
    }

    public class NobleRing3037 : NobleRingGold
    {
        [Constructable]
        public NobleRing3037() : base(3037)
        {
        }

        public NobleRing3037(Serial serial) : base(serial)
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
            VisualGumpId = 3037;
        }
    }

    public class NobleRing3038 : NobleRingGold
    {
        [Constructable]
        public NobleRing3038() : base(3038)
        {
        }

        public NobleRing3038(Serial serial) : base(serial)
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
            VisualGumpId = 3038;
        }
    }

    public class NobleRing3039 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3039() : base(3039)
        {
        }

        public NobleRing3039(Serial serial) : base(serial)
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
            VisualGumpId = 3039;
        }
    }

    public class NobleRing3040 : NobleRingGold
    {
        [Constructable]
        public NobleRing3040() : base(3040)
        {
        }

        public NobleRing3040(Serial serial) : base(serial)
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
            VisualGumpId = 3040;
        }
    }

    public class NobleRing3041 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3041() : base(3041)
        {
        }

        public NobleRing3041(Serial serial) : base(serial)
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
            VisualGumpId = 3041;
        }
    }

    public class NobleRing3042 : NobleRingGold
    {
        [Constructable]
        public NobleRing3042() : base(3042)
        {
        }

        public NobleRing3042(Serial serial) : base(serial)
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
            VisualGumpId = 3042;
        }
    }

    public class NobleRing3043 : NobleRingGold
    {
        [Constructable]
        public NobleRing3043() : base(3043)
        {
        }

        public NobleRing3043(Serial serial) : base(serial)
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
            VisualGumpId = 3043;
        }
    }

    public class NobleRing3044 : NobleRingGold
    {
        [Constructable]
        public NobleRing3044() : base(3044)
        {
        }

        public NobleRing3044(Serial serial) : base(serial)
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
            VisualGumpId = 3044;
        }
    }

    public class NobleRing3045 : NobleRingGold
    {
        [Constructable]
        public NobleRing3045() : base(3045)
        {
        }

        public NobleRing3045(Serial serial) : base(serial)
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
            VisualGumpId = 3045;
        }
    }

    public class NobleRing3046 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3046() : base(3046)
        {
        }

        public NobleRing3046(Serial serial) : base(serial)
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
            VisualGumpId = 3046;
        }
    }

    public class NobleRing3047 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3047() : base(3047)
        {
        }

        public NobleRing3047(Serial serial) : base(serial)
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
            VisualGumpId = 3047;
        }
    }

    public class NobleRing3048 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3048() : base(3048)
        {
        }

        public NobleRing3048(Serial serial) : base(serial)
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
            VisualGumpId = 3048;
        }
    }

    public class NobleRing3049 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3049() : base(3049)
        {
        }

        public NobleRing3049(Serial serial) : base(serial)
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
            VisualGumpId = 3049;
        }
    }

    public class NobleRing3050 : NobleRingGold
    {
        [Constructable]
        public NobleRing3050() : base(3050)
        {
        }

        public NobleRing3050(Serial serial) : base(serial)
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
            VisualGumpId = 3050;
        }
    }

    public class NobleRing3051 : NobleRingGold
    {
        [Constructable]
        public NobleRing3051() : base(3051)
        {
        }

        public NobleRing3051(Serial serial) : base(serial)
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
            VisualGumpId = 3051;
        }
    }

    public class NobleRing3052 : NobleRingGold
    {
        [Constructable]
        public NobleRing3052() : base(3052)
        {
        }

        public NobleRing3052(Serial serial) : base(serial)
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
            VisualGumpId = 3052;
        }
    }

    public class NobleRing3053 : NobleRingGold
    {
        [Constructable]
        public NobleRing3053() : base(3053)
        {
        }

        public NobleRing3053(Serial serial) : base(serial)
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
            VisualGumpId = 3053;
        }
    }

    public class NobleRing3054 : NobleRingGold
    {
        [Constructable]
        public NobleRing3054() : base(3054)
        {
        }

        public NobleRing3054(Serial serial) : base(serial)
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
            VisualGumpId = 3054;
        }
    }

    public class NobleRing3055 : NobleRingGold
    {
        [Constructable]
        public NobleRing3055() : base(3055)
        {
        }

        public NobleRing3055(Serial serial) : base(serial)
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
            VisualGumpId = 3055;
        }
    }

    public class NobleRing3056 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3056() : base(3056)
        {
        }

        public NobleRing3056(Serial serial) : base(serial)
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
            VisualGumpId = 3056;
        }
    }

    public class NobleRing3057 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3057() : base(3057)
        {
        }

        public NobleRing3057(Serial serial) : base(serial)
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
            VisualGumpId = 3057;
        }
    }

    public class NobleRing3058 : NobleRingGold
    {
        [Constructable]
        public NobleRing3058() : base(3058)
        {
        }

        public NobleRing3058(Serial serial) : base(serial)
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
            VisualGumpId = 3058;
        }
    }

    public class NobleRing3059 : NobleRingGold
    {
        [Constructable]
        public NobleRing3059() : base(3059)
        {
        }

        public NobleRing3059(Serial serial) : base(serial)
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
            VisualGumpId = 3059;
        }
    }

    public class NobleRing3060 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3060() : base(3060)
        {
        }

        public NobleRing3060(Serial serial) : base(serial)
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
            VisualGumpId = 3060;
        }
    }

    public class NobleRing3061 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3061() : base(3061)
        {
        }

        public NobleRing3061(Serial serial) : base(serial)
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
            VisualGumpId = 3061;
        }
    }

    public class NobleRing3062 : NobleRingGold
    {
        [Constructable]
        public NobleRing3062() : base(3062)
        {
        }

        public NobleRing3062(Serial serial) : base(serial)
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
            VisualGumpId = 3062;
        }
    }

    public class NobleRing3063 : NobleRingBronze
    {
        [Constructable]
        public NobleRing3063() : base(3063)
        {
        }

        public NobleRing3063(Serial serial) : base(serial)
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
            VisualGumpId = 3063;
        }
    }

    public class NobleRing3064 : NobleRingGold
    {
        [Constructable]
        public NobleRing3064() : base(3064)
        {
        }

        public NobleRing3064(Serial serial) : base(serial)
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
            VisualGumpId = 3064;
        }
    }

    public class NobleRing3065 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3065() : base(3065)
        {
        }

        public NobleRing3065(Serial serial) : base(serial)
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
            VisualGumpId = 3065;
        }
    }

    public class NobleRing3066 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3066() : base(3066)
        {
        }

        public NobleRing3066(Serial serial) : base(serial)
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
            VisualGumpId = 3066;
        }
    }

    public class NobleRing3067 : NobleRingBronze
    {
        [Constructable]
        public NobleRing3067() : base(3067)
        {
        }

        public NobleRing3067(Serial serial) : base(serial)
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
            VisualGumpId = 3067;
        }
    }

    public class NobleRing3068 : NobleRingSilver
    {
        [Constructable]
        public NobleRing3068() : base(3068)
        {
        }

        public NobleRing3068(Serial serial) : base(serial)
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
            VisualGumpId = 3068;
        }
    }

    public class NobleRing3069 : NobleRingGold
    {
        [Constructable]
        public NobleRing3069() : base(3069)
        {
        }

        public NobleRing3069(Serial serial) : base(serial)
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
            VisualGumpId = 3069;
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
            AddLabel(390, 252, 0, label);
            AddImage(382, 167, gumpId);
        }
    }
}
