using Server.Items;
using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public class Uniforme1 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme1()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme1(int hue)
            : base(0x227E, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme1(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
    public class Uniforme2 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme2()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme2(int hue)
            : base(0x227F, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme2(Serial serial)
            : base(serial)
        {
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

    public class Uniforme3 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme3()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme3(int hue)
            : base(0x2280, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme3(Serial serial)
            : base(serial)
        {
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

    public class Uniforme4 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme4()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme4(int hue)
            : base(0x2281, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme4(Serial serial)
            : base(serial)
        {
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

    public class Uniforme5 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme5()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme5(int hue)
            : base(0x2282, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme5(Serial serial)
            : base(serial)
        {
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

    public class Uniforme6 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme6()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme6(int hue)
            : base(0x2283, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme6(Serial serial)
            : base(serial)
        {
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

    public class Uniforme7 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme7()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme7(int hue)
            : base(0x2284, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme7(Serial serial)
            : base(serial)
        {
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

    public class Uniforme8 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme8()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme8(int hue)
            : base(0x2285, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme8(Serial serial)
            : base(serial)
        {
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

    public class Uniforme9 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme9()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme9(int hue)
            : base(0x2286, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme9(Serial serial)
            : base(serial)
        {
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

    public class Uniforme10 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme10()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme10(int hue)
            : base(0x2287, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme10(Serial serial)
            : base(serial)
        {
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

    public class Uniforme11 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme11()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme11(int hue)
            : base(0x2288, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme11(Serial serial)
            : base(serial)
        {
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

    public class Uniforme12 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme12()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme12(int hue)
            : base(0x2289, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme12(Serial serial)
            : base(serial)
        {
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

    public class Uniforme13 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme13()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme13(int hue)
            : base(0x228A, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme13(Serial serial)
            : base(serial)
        {
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

    public class Uniforme14 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme14()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme14(int hue)
            : base(0x228B, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme14(Serial serial)
            : base(serial)
        {
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

    public class Uniforme15 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme15()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme15(int hue)
            : base(0x228C, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme15(Serial serial)
            : base(serial)
        {
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

    public class Uniforme16 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme16()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme16(int hue)
            : base(0x228D, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme16(Serial serial)
            : base(serial)
        {
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

    public class Uniforme17 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme17()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme17(int hue)
            : base(0x228E, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme17(Serial serial)
            : base(serial)
        {
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

    public class Uniforme18 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme18()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme18(int hue)
            : base(0x228F, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme18(Serial serial)
            : base(serial)
        {
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

    public class Uniforme19 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme19()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme19(int hue)
            : base(0x2290, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme19(Serial serial)
            : base(serial)
        {
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

    public class Uniforme20 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme20()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme20(int hue)
            : base(0x2291, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme20(Serial serial)
            : base(serial)
        {
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

    public class Uniforme21 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme21()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme21(int hue)
            : base(0x2292, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme21(Serial serial)
            : base(serial)
        {
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

    public class Uniforme22 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme22()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme22(int hue)
            : base(0x2293, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme22(Serial serial)
            : base(serial)
        {
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

    public class Uniforme23 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme23()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme23(int hue)
            : base(0x2294, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme23(Serial serial)
            : base(serial)
        {
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

    public class Uniforme24 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme24()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme24(int hue)
            : base(0x2295, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme24(Serial serial)
            : base(serial)
        {
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

    public class Uniforme25 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme25()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme25(int hue)
            : base(0x2296, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme25(Serial serial)
            : base(serial)
        {
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

    public class Uniforme26 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme26()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme26(int hue)
            : base(0x2297, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme26(Serial serial)
            : base(serial)
        {
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

    public class Uniforme27 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme27()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme27(int hue)
            : base(0x2298, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme27(Serial serial)
            : base(serial)
        {
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

    public class Uniforme28 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme28()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme28(int hue)
            : base(0x2299, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme28(Serial serial)
            : base(serial)
        {
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


    public class Uniforme29 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme29()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme29(int hue)
            : base(0x229A, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme29(Serial serial)
            : base(serial)
        {
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

    public class Uniforme30 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme30()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme30(int hue)
            : base(0x229B, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme30(Serial serial)
            : base(serial)
        {
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

    public class Uniforme31 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme31()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme31(int hue)
            : base(0x229C, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme31(Serial serial)
            : base(serial)
        {
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

    public class Uniforme32 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme32()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme32(int hue)
            : base(0x229D, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme32(Serial serial)
            : base(serial)
        {
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

    public class Uniforme33 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme33()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme33(int hue)
            : base(0x229E, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme33(Serial serial)
            : base(serial)
        {
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

    public class Uniforme34 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme34()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme34(int hue)
            : base(0x229F, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme34(Serial serial)
            : base(serial)
        {
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

    public class Uniforme35 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme35()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme35(int hue)
            : base(0x22A0, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme35(Serial serial)
            : base(serial)
        {
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

    public class Uniforme36 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme36()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme36(int hue)
            : base(0x22A1, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme36(Serial serial)
            : base(serial)
        {
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

    public class Uniforme37 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme37()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme37(int hue)
            : base(0x22A2, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme37(Serial serial)
            : base(serial)
        {
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

    public class Uniforme38 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme38()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme38(int hue)
            : base(0x22A3, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme38(Serial serial)
            : base(serial)
        {
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

    public class Uniforme39 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme39()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme39(int hue)
            : base(0x22A4, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme39(Serial serial)
            : base(serial)
        {
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

    public class Uniforme40 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme40()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme40(int hue)
            : base(0x22A5, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme40(Serial serial)
            : base(serial)
        {
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

    public class Uniforme41 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme41()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme41(int hue)
            : base(0x22A6, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme41(Serial serial)
            : base(serial)
        {
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

    public class Uniforme42 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme42()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme42(int hue)
            : base(0x22A7, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme42(Serial serial)
            : base(serial)
        {
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

    public class Uniforme43 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme43()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme43(int hue)
            : base(0x22A8, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme43(Serial serial)
            : base(serial)
        {
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

    public class Uniforme44 : BaseMiddleTorso
    {
        [Constructable]
        public Uniforme44()
            : this(0)
        {
        }

        [Constructable]
        public Uniforme44(int hue)
            : base(0x22A9, hue)
        {
            this.Weight = 7.0;
        }

        public Uniforme44(Serial serial)
            : base(serial)
        {
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
}
