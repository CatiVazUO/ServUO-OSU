using System;
using Server;
using Server.Items;

namespace Server.Items
{
    public abstract class BaseReinoBanner : Item
    {
        [Constructable]
        public BaseReinoBanner(int itemID) : base(itemID)
        {
            Movable = false;
            Name = "banner do reino";
        }

        public BaseReinoBanner(Serial serial) : base(serial)
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
            Movable = false;
            if (string.IsNullOrEmpty(Name))
                Name = "banner do reino";
        }
    }

    public class Banner1 : BaseReinoBanner
    {
        [Constructable]
        public Banner1() : base(0x3BBD)
        {
        }

        public Banner1(Serial serial) : base(serial)
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

    public class Banner2 : BaseReinoBanner
    {
        [Constructable]
        public Banner2() : base(0x3BBE)
        {
        }

        public Banner2(Serial serial) : base(serial)
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

    public class Banner3 : BaseReinoBanner
    {
        [Constructable]
        public Banner3() : base(0x3BBF)
        {
        }

        public Banner3(Serial serial) : base(serial)
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

    public class Banner4 : BaseReinoBanner
    {
        [Constructable]
        public Banner4() : base(0x3BC0)
        {
        }

        public Banner4(Serial serial) : base(serial)
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

    public class Banner5 : BaseReinoBanner
    {
        [Constructable]
        public Banner5() : base(0x3BC1)
        {
        }

        public Banner5(Serial serial) : base(serial)
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

    public class Banner6 : BaseReinoBanner
    {
        [Constructable]
        public Banner6() : base(0x3BC2)
        {
        }

        public Banner6(Serial serial) : base(serial)
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

    public class Banner7 : BaseReinoBanner
    {
        [Constructable]
        public Banner7() : base(0x3BC3)
        {
        }

        public Banner7(Serial serial) : base(serial)
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

    public class Banner8 : BaseReinoBanner
    {
        [Constructable]
        public Banner8() : base(0x3BC4)
        {
        }

        public Banner8(Serial serial) : base(serial)
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

    public class Banner9 : BaseReinoBanner
    {
        [Constructable]
        public Banner9() : base(0x3BC5)
        {
        }

        public Banner9(Serial serial) : base(serial)
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

    public class Banner10 : BaseReinoBanner
    {
        [Constructable]
        public Banner10() : base(0x3BC6)
        {
        }

        public Banner10(Serial serial) : base(serial)
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

    public class Banner11 : BaseReinoBanner
    {
        [Constructable]
        public Banner11() : base(0x3BC7)
        {
        }

        public Banner11(Serial serial) : base(serial)
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

    public class Banner12 : BaseReinoBanner
    {
        [Constructable]
        public Banner12() : base(0x3BC8)
        {
        }

        public Banner12(Serial serial) : base(serial)
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

    public class Banner13 : BaseReinoBanner
    {
        [Constructable]
        public Banner13() : base(0x3BC9)
        {
        }

        public Banner13(Serial serial) : base(serial)
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

    public class Banner14 : BaseReinoBanner
    {
        [Constructable]
        public Banner14() : base(0x3BCA)
        {
        }

        public Banner14(Serial serial) : base(serial)
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

    public class Banner15 : BaseReinoBanner
    {
        [Constructable]
        public Banner15() : base(0x3BCB)
        {
        }

        public Banner15(Serial serial) : base(serial)
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

    public class Banner16 : BaseReinoBanner
    {
        [Constructable]
        public Banner16() : base(0x3BCC)
        {
        }

        public Banner16(Serial serial) : base(serial)
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

    public class Banner17 : BaseReinoBanner
    {
        [Constructable]
        public Banner17() : base(0x3BCD)
        {
        }

        public Banner17(Serial serial) : base(serial)
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

    public class Banner18 : BaseReinoBanner
    {
        [Constructable]
        public Banner18() : base(0x3BCE)
        {
        }

        public Banner18(Serial serial) : base(serial)
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

    public class Banner19 : BaseReinoBanner
    {
        [Constructable]
        public Banner19() : base(0x3BCF)
        {
        }

        public Banner19(Serial serial) : base(serial)
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

    public class Banner20 : BaseReinoBanner
    {
        [Constructable]
        public Banner20() : base(0x3BD0)
        {
        }

        public Banner20(Serial serial) : base(serial)
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

    public class Banner21 : BaseReinoBanner
    {
        [Constructable]
        public Banner21() : base(0x3BD1)
        {
        }

        public Banner21(Serial serial) : base(serial)
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

    public class Banner22 : BaseReinoBanner
    {
        [Constructable]
        public Banner22() : base(0x3BD2)
        {
        }

        public Banner22(Serial serial) : base(serial)
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

    public class Banner23 : BaseReinoBanner
    {
        [Constructable]
        public Banner23() : base(0x3BD3)
        {
        }

        public Banner23(Serial serial) : base(serial)
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

    public class Banner24 : BaseReinoBanner
    {
        [Constructable]
        public Banner24() : base(0x3BD4)
        {
        }

        public Banner24(Serial serial) : base(serial)
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

    public class Banner25 : BaseReinoBanner
    {
        [Constructable]
        public Banner25() : base(0x3BD5)
        {
        }

        public Banner25(Serial serial) : base(serial)
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

    public class Banner26 : BaseReinoBanner
    {
        [Constructable]
        public Banner26() : base(0x3BD6)
        {
        }

        public Banner26(Serial serial) : base(serial)
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

    public class Banner27 : BaseReinoBanner
    {
        [Constructable]
        public Banner27() : base(0x3BD7)
        {
        }

        public Banner27(Serial serial) : base(serial)
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

    public class Banner28 : BaseReinoBanner
    {
        [Constructable]
        public Banner28() : base(0x3BD8)
        {
        }

        public Banner28(Serial serial) : base(serial)
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

    public class Banner29 : BaseReinoBanner
    {
        [Constructable]
        public Banner29() : base(0x3BD9)
        {
        }

        public Banner29(Serial serial) : base(serial)
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

    public class Banner30 : BaseReinoBanner
    {
        [Constructable]
        public Banner30() : base(0x3BDA)
        {
        }

        public Banner30(Serial serial) : base(serial)
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

    public class Banner31 : BaseReinoBanner
    {
        [Constructable]
        public Banner31() : base(0x3BDB)
        {
        }

        public Banner31(Serial serial) : base(serial)
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

    public class Banner32 : BaseReinoBanner
    {
        [Constructable]
        public Banner32() : base(0x3BDC)
        {
        }

        public Banner32(Serial serial) : base(serial)
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

    public class Banner33 : BaseReinoBanner
    {
        [Constructable]
        public Banner33() : base(0x3BDD)
        {
        }

        public Banner33(Serial serial) : base(serial)
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

    public class Banner34 : BaseReinoBanner
    {
        [Constructable]
        public Banner34() : base(0x3BDE)
        {
        }

        public Banner34(Serial serial) : base(serial)
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

    public class Banner35 : BaseReinoBanner
    {
        [Constructable]
        public Banner35() : base(0x3BDF)
        {
        }

        public Banner35(Serial serial) : base(serial)
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

    public class Banner36 : BaseReinoBanner
    {
        [Constructable]
        public Banner36() : base(0x3BE0)
        {
        }

        public Banner36(Serial serial) : base(serial)
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

    public class Banner37 : BaseReinoBanner
    {
        [Constructable]
        public Banner37() : base(0x3BE1)
        {
        }

        public Banner37(Serial serial) : base(serial)
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

    public class Banner38 : BaseReinoBanner
    {
        [Constructable]
        public Banner38() : base(0x3BE2)
        {
        }

        public Banner38(Serial serial) : base(serial)
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

    public class Banner39 : BaseReinoBanner
    {
        [Constructable]
        public Banner39() : base(0x3BE3)
        {
        }

        public Banner39(Serial serial) : base(serial)
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

    public class Banner40 : BaseReinoBanner
    {
        [Constructable]
        public Banner40() : base(0x3BE4)
        {
        }

        public Banner40(Serial serial) : base(serial)
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

    public class Banner41 : BaseReinoBanner
    {
        [Constructable]
        public Banner41() : base(0x3BE5)
        {
        }

        public Banner41(Serial serial) : base(serial)
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

    public class Banner42 : BaseReinoBanner
    {
        [Constructable]
        public Banner42() : base(0x3BE6)
        {
        }

        public Banner42(Serial serial) : base(serial)
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

    public class Banner43 : BaseReinoBanner
    {
        [Constructable]
        public Banner43() : base(0x3BE7)
        {
        }

        public Banner43(Serial serial) : base(serial)
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

    public class Banner44 : BaseReinoBanner
    {
        [Constructable]
        public Banner44() : base(0x3BE8)
        {
        }

        public Banner44(Serial serial) : base(serial)
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

    public class Banner101 : BaseReinoBanner
    {
        [Constructable]
        public Banner101() : base(0x3BE9)
        {
        }

        public Banner101(Serial serial) : base(serial)
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

    public class Banner102 : BaseReinoBanner
    {
        [Constructable]
        public Banner102() : base(0x3BEA)
        {
        }

        public Banner102(Serial serial) : base(serial)
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

    public class Banner103 : BaseReinoBanner
    {
        [Constructable]
        public Banner103() : base(0x3BEB)
        {
        }

        public Banner103(Serial serial) : base(serial)
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

    public class Banner104 : BaseReinoBanner
    {
        [Constructable]
        public Banner104() : base(0x3BEC)
        {
        }

        public Banner104(Serial serial) : base(serial)
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

    public class Banner105 : BaseReinoBanner
    {
        [Constructable]
        public Banner105() : base(0x3BED)
        {
        }

        public Banner105(Serial serial) : base(serial)
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

    public class Banner106 : BaseReinoBanner
    {
        [Constructable]
        public Banner106() : base(0x3BEE)
        {
        }

        public Banner106(Serial serial) : base(serial)
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

    public class Banner107 : BaseReinoBanner
    {
        [Constructable]
        public Banner107() : base(0x3BEF)
        {
        }

        public Banner107(Serial serial) : base(serial)
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

    public class Banner108 : BaseReinoBanner
    {
        [Constructable]
        public Banner108() : base(0x3BF0)
        {
        }

        public Banner108(Serial serial) : base(serial)
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

    public class Banner109 : BaseReinoBanner
    {
        [Constructable]
        public Banner109() : base(0x3BF1)
        {
        }

        public Banner109(Serial serial) : base(serial)
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

    public class Banner110 : BaseReinoBanner
    {
        [Constructable]
        public Banner110() : base(0x3BF2)
        {
        }

        public Banner110(Serial serial) : base(serial)
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

    public class Banner111 : BaseReinoBanner
    {
        [Constructable]
        public Banner111() : base(0x3BF3)
        {
        }

        public Banner111(Serial serial) : base(serial)
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

    public class Banner112 : BaseReinoBanner
    {
        [Constructable]
        public Banner112() : base(0x3BF4)
        {
        }

        public Banner112(Serial serial) : base(serial)
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

    public class Banner113 : BaseReinoBanner
    {
        [Constructable]
        public Banner113() : base(0x3BF5)
        {
        }

        public Banner113(Serial serial) : base(serial)
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

    public class Banner114 : BaseReinoBanner
    {
        [Constructable]
        public Banner114() : base(0x3BF6)
        {
        }

        public Banner114(Serial serial) : base(serial)
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

    public class Banner115 : BaseReinoBanner
    {
        [Constructable]
        public Banner115() : base(0x3BF7)
        {
        }

        public Banner115(Serial serial) : base(serial)
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

    public class Banner116 : BaseReinoBanner
    {
        [Constructable]
        public Banner116() : base(0x3BF8)
        {
        }

        public Banner116(Serial serial) : base(serial)
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

    public class Banner117 : BaseReinoBanner
    {
        [Constructable]
        public Banner117() : base(0x3BF9)
        {
        }

        public Banner117(Serial serial) : base(serial)
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

    public class Banner118 : BaseReinoBanner
    {
        [Constructable]
        public Banner118() : base(0x3BFA)
        {
        }

        public Banner118(Serial serial) : base(serial)
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

    public class Banner119 : BaseReinoBanner
    {
        [Constructable]
        public Banner119() : base(0x3BFB)
        {
        }

        public Banner119(Serial serial) : base(serial)
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

    public class Banner120 : BaseReinoBanner
    {
        [Constructable]
        public Banner120() : base(0x3BFC)
        {
        }

        public Banner120(Serial serial) : base(serial)
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

    public class Banner121 : BaseReinoBanner
    {
        [Constructable]
        public Banner121() : base(0x3BFD)
        {
        }

        public Banner121(Serial serial) : base(serial)
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

    public class Banner122 : BaseReinoBanner
    {
        [Constructable]
        public Banner122() : base(0x3BFE)
        {
        }

        public Banner122(Serial serial) : base(serial)
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

    public class Banner123 : BaseReinoBanner
    {
        [Constructable]
        public Banner123() : base(0x3BFF)
        {
        }

        public Banner123(Serial serial) : base(serial)
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

    public class Banner124 : BaseReinoBanner
    {
        [Constructable]
        public Banner124() : base(0x3C00)
        {
        }

        public Banner124(Serial serial) : base(serial)
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

    public class Banner125 : BaseReinoBanner
    {
        [Constructable]
        public Banner125() : base(0x3C01)
        {
        }

        public Banner125(Serial serial) : base(serial)
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

    public class Banner126 : BaseReinoBanner
    {
        [Constructable]
        public Banner126() : base(0x3C02)
        {
        }

        public Banner126(Serial serial) : base(serial)
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

    public class Banner127 : BaseReinoBanner
    {
        [Constructable]
        public Banner127() : base(0x3C03)
        {
        }

        public Banner127(Serial serial) : base(serial)
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

    public class Banner128 : BaseReinoBanner
    {
        [Constructable]
        public Banner128() : base(0x3C04)
        {
        }

        public Banner128(Serial serial) : base(serial)
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

    public class Banner129 : BaseReinoBanner
    {
        [Constructable]
        public Banner129() : base(0x3C05)
        {
        }

        public Banner129(Serial serial) : base(serial)
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

    public class Banner130 : BaseReinoBanner
    {
        [Constructable]
        public Banner130() : base(0x3C06)
        {
        }

        public Banner130(Serial serial) : base(serial)
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

    public class Banner131 : BaseReinoBanner
    {
        [Constructable]
        public Banner131() : base(0x3C07)
        {
        }

        public Banner131(Serial serial) : base(serial)
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

    public class Banner132 : BaseReinoBanner
    {
        [Constructable]
        public Banner132() : base(0x3C08)
        {
        }

        public Banner132(Serial serial) : base(serial)
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

    public class Banner133 : BaseReinoBanner
    {
        [Constructable]
        public Banner133() : base(0x3C09)
        {
        }

        public Banner133(Serial serial) : base(serial)
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

    public class Banner134 : BaseReinoBanner
    {
        [Constructable]
        public Banner134() : base(0x3C0A)
        {
        }

        public Banner134(Serial serial) : base(serial)
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

    public class Banner135 : BaseReinoBanner
    {
        [Constructable]
        public Banner135() : base(0x3C0B)
        {
        }

        public Banner135(Serial serial) : base(serial)
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

    public class Banner136 : BaseReinoBanner
    {
        [Constructable]
        public Banner136() : base(0x3C0C)
        {
        }

        public Banner136(Serial serial) : base(serial)
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

    public class Banner137 : BaseReinoBanner
    {
        [Constructable]
        public Banner137() : base(0x3C0D)
        {
        }

        public Banner137(Serial serial) : base(serial)
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

    public class Banner138 : BaseReinoBanner
    {
        [Constructable]
        public Banner138() : base(0x3C0E)
        {
        }

        public Banner138(Serial serial) : base(serial)
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

    public class Banner139 : BaseReinoBanner
    {
        [Constructable]
        public Banner139() : base(0x3C0F)
        {
        }

        public Banner139(Serial serial) : base(serial)
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

    public class Banner140 : BaseReinoBanner
    {
        [Constructable]
        public Banner140() : base(0x3C10)
        {
        }

        public Banner140(Serial serial) : base(serial)
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

    public class Banner141 : BaseReinoBanner
    {
        [Constructable]
        public Banner141() : base(0x3C11)
        {
        }

        public Banner141(Serial serial) : base(serial)
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

    public class Banner142 : BaseReinoBanner
    {
        [Constructable]
        public Banner142() : base(0x3C12)
        {
        }

        public Banner142(Serial serial) : base(serial)
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

    public class Banner143 : BaseReinoBanner
    {
        [Constructable]
        public Banner143() : base(0x3C13)
        {
        }

        public Banner143(Serial serial) : base(serial)
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

    public class Banner144 : BaseReinoBanner
    {
        [Constructable]
        public Banner144() : base(0x3C14)
        {
        }

        public Banner144(Serial serial) : base(serial)
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

    public class Banner201 : BaseReinoBanner
    {
        [Constructable]
        public Banner201() : base(0x3C15)
        {
        }

        public Banner201(Serial serial) : base(serial)
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

    public class Banner202 : BaseReinoBanner
    {
        [Constructable]
        public Banner202() : base(0x3C16)
        {
        }

        public Banner202(Serial serial) : base(serial)
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

    public class Banner203 : BaseReinoBanner
    {
        [Constructable]
        public Banner203() : base(0x3C17)
        {
        }

        public Banner203(Serial serial) : base(serial)
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

    public class Banner204 : BaseReinoBanner
    {
        [Constructable]
        public Banner204() : base(0x3C18)
        {
        }

        public Banner204(Serial serial) : base(serial)
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

    public class Banner205 : BaseReinoBanner
    {
        [Constructable]
        public Banner205() : base(0x3C19)
        {
        }

        public Banner205(Serial serial) : base(serial)
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

    public class Banner206 : BaseReinoBanner
    {
        [Constructable]
        public Banner206() : base(0x3C1A)
        {
        }

        public Banner206(Serial serial) : base(serial)
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

    public class Banner207 : BaseReinoBanner
    {
        [Constructable]
        public Banner207() : base(0x3C1B)
        {
        }

        public Banner207(Serial serial) : base(serial)
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

    public class Banner208 : BaseReinoBanner
    {
        [Constructable]
        public Banner208() : base(0x3C1C)
        {
        }

        public Banner208(Serial serial) : base(serial)
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

    public class Banner209 : BaseReinoBanner
    {
        [Constructable]
        public Banner209() : base(0x3C1D)
        {
        }

        public Banner209(Serial serial) : base(serial)
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

    public class Banner210 : BaseReinoBanner
    {
        [Constructable]
        public Banner210() : base(0x3C1E)
        {
        }

        public Banner210(Serial serial) : base(serial)
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

    public class Banner211 : BaseReinoBanner
    {
        [Constructable]
        public Banner211() : base(0x3C1F)
        {
        }

        public Banner211(Serial serial) : base(serial)
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

    public class Banner212 : BaseReinoBanner
    {
        [Constructable]
        public Banner212() : base(0x3C20)
        {
        }

        public Banner212(Serial serial) : base(serial)
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

    public class Banner213 : BaseReinoBanner
    {
        [Constructable]
        public Banner213() : base(0x3C21)
        {
        }

        public Banner213(Serial serial) : base(serial)
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

    public class Banner214 : BaseReinoBanner
    {
        [Constructable]
        public Banner214() : base(0x3C22)
        {
        }

        public Banner214(Serial serial) : base(serial)
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

    public class Banner215 : BaseReinoBanner
    {
        [Constructable]
        public Banner215() : base(0x3C23)
        {
        }

        public Banner215(Serial serial) : base(serial)
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

    public class Banner216 : BaseReinoBanner
    {
        [Constructable]
        public Banner216() : base(0x3C24)
        {
        }

        public Banner216(Serial serial) : base(serial)
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

    public class Banner217 : BaseReinoBanner
    {
        [Constructable]
        public Banner217() : base(0x3C25)
        {
        }

        public Banner217(Serial serial) : base(serial)
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

    public class Banner218 : BaseReinoBanner
    {
        [Constructable]
        public Banner218() : base(0x3C26)
        {
        }

        public Banner218(Serial serial) : base(serial)
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

    public class Banner219 : BaseReinoBanner
    {
        [Constructable]
        public Banner219() : base(0x3C27)
        {
        }

        public Banner219(Serial serial) : base(serial)
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

    public class Banner220 : BaseReinoBanner
    {
        [Constructable]
        public Banner220() : base(0x3C28)
        {
        }

        public Banner220(Serial serial) : base(serial)
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

    public class Banner221 : BaseReinoBanner
    {
        [Constructable]
        public Banner221() : base(0x3C29)
        {
        }

        public Banner221(Serial serial) : base(serial)
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

    public class Banner222 : BaseReinoBanner
    {
        [Constructable]
        public Banner222() : base(0x3C2A)
        {
        }

        public Banner222(Serial serial) : base(serial)
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

    public class Banner223 : BaseReinoBanner
    {
        [Constructable]
        public Banner223() : base(0x3C2B)
        {
        }

        public Banner223(Serial serial) : base(serial)
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

    public class Banner224 : BaseReinoBanner
    {
        [Constructable]
        public Banner224() : base(0x3C2C)
        {
        }

        public Banner224(Serial serial) : base(serial)
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

    public class Banner225 : BaseReinoBanner
    {
        [Constructable]
        public Banner225() : base(0x3C2D)
        {
        }

        public Banner225(Serial serial) : base(serial)
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

    public class Banner226 : BaseReinoBanner
    {
        [Constructable]
        public Banner226() : base(0x3C2E)
        {
        }

        public Banner226(Serial serial) : base(serial)
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

    public class Banner227 : BaseReinoBanner
    {
        [Constructable]
        public Banner227() : base(0x3C2F)
        {
        }

        public Banner227(Serial serial) : base(serial)
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

    public class Banner228 : BaseReinoBanner
    {
        [Constructable]
        public Banner228() : base(0x3C30)
        {
        }

        public Banner228(Serial serial) : base(serial)
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

    public class Banner229 : BaseReinoBanner
    {
        [Constructable]
        public Banner229() : base(0x3C31)
        {
        }

        public Banner229(Serial serial) : base(serial)
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

    public class Banner230 : BaseReinoBanner
    {
        [Constructable]
        public Banner230() : base(0x3C32)
        {
        }

        public Banner230(Serial serial) : base(serial)
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

    public class Banner231 : BaseReinoBanner
    {
        [Constructable]
        public Banner231() : base(0x3C33)
        {
        }

        public Banner231(Serial serial) : base(serial)
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

    public class Banner232 : BaseReinoBanner
    {
        [Constructable]
        public Banner232() : base(0x3C34)
        {
        }

        public Banner232(Serial serial) : base(serial)
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

    public class Banner233 : BaseReinoBanner
    {
        [Constructable]
        public Banner233() : base(0x3C35)
        {
        }

        public Banner233(Serial serial) : base(serial)
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

    public class Banner234 : BaseReinoBanner
    {
        [Constructable]
        public Banner234() : base(0x3C36)
        {
        }

        public Banner234(Serial serial) : base(serial)
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

    public class Banner235 : BaseReinoBanner
    {
        [Constructable]
        public Banner235() : base(0x3C37)
        {
        }

        public Banner235(Serial serial) : base(serial)
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

    public class Banner236 : BaseReinoBanner
    {
        [Constructable]
        public Banner236() : base(0x3C38)
        {
        }

        public Banner236(Serial serial) : base(serial)
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

    public class Banner237 : BaseReinoBanner
    {
        [Constructable]
        public Banner237() : base(0x3C39)
        {
        }

        public Banner237(Serial serial) : base(serial)
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

    public class Banner238 : BaseReinoBanner
    {
        [Constructable]
        public Banner238() : base(0x3C3A)
        {
        }

        public Banner238(Serial serial) : base(serial)
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

    public class Banner239 : BaseReinoBanner
    {
        [Constructable]
        public Banner239() : base(0x3C3B)
        {
        }

        public Banner239(Serial serial) : base(serial)
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

    public class Banner240 : BaseReinoBanner
    {
        [Constructable]
        public Banner240() : base(0x3C3C)
        {
        }

        public Banner240(Serial serial) : base(serial)
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

    public class Banner241 : BaseReinoBanner
    {
        [Constructable]
        public Banner241() : base(0x3C3D)
        {
        }

        public Banner241(Serial serial) : base(serial)
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

    public class Banner242 : BaseReinoBanner
    {
        [Constructable]
        public Banner242() : base(0x3C3E)
        {
        }

        public Banner242(Serial serial) : base(serial)
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

    public class Banner243 : BaseReinoBanner
    {
        [Constructable]
        public Banner243() : base(0x3C3F)
        {
        }

        public Banner243(Serial serial) : base(serial)
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

    public class Banner244 : BaseReinoBanner
    {
        [Constructable]
        public Banner244() : base(0x3C40)
        {
        }

        public Banner244(Serial serial) : base(serial)
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

    public class Banner301 : BaseReinoBanner
    {
        [Constructable]
        public Banner301() : base(0x3C41)
        {
        }

        public Banner301(Serial serial) : base(serial)
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

    public class Banner302 : BaseReinoBanner
    {
        [Constructable]
        public Banner302() : base(0x3C42)
        {
        }

        public Banner302(Serial serial) : base(serial)
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

    public class Banner303 : BaseReinoBanner
    {
        [Constructable]
        public Banner303() : base(0x3C43)
        {
        }

        public Banner303(Serial serial) : base(serial)
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

    public class Banner304 : BaseReinoBanner
    {
        [Constructable]
        public Banner304() : base(0x3C44)
        {
        }

        public Banner304(Serial serial) : base(serial)
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

    public class Banner305 : BaseReinoBanner
    {
        [Constructable]
        public Banner305() : base(0x3C45)
        {
        }

        public Banner305(Serial serial) : base(serial)
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

    public class Banner306 : BaseReinoBanner
    {
        [Constructable]
        public Banner306() : base(0x3C46)
        {
        }

        public Banner306(Serial serial) : base(serial)
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

    public class Banner307 : BaseReinoBanner
    {
        [Constructable]
        public Banner307() : base(0x3C47)
        {
        }

        public Banner307(Serial serial) : base(serial)
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

    public class Banner308 : BaseReinoBanner
    {
        [Constructable]
        public Banner308() : base(0x3C48)
        {
        }

        public Banner308(Serial serial) : base(serial)
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

    public class Banner309 : BaseReinoBanner
    {
        [Constructable]
        public Banner309() : base(0x3C49)
        {
        }

        public Banner309(Serial serial) : base(serial)
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

    public class Banner310 : BaseReinoBanner
    {
        [Constructable]
        public Banner310() : base(0x3C4A)
        {
        }

        public Banner310(Serial serial) : base(serial)
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

    public class Banner311 : BaseReinoBanner
    {
        [Constructable]
        public Banner311() : base(0x3C4B)
        {
        }

        public Banner311(Serial serial) : base(serial)
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

    public class Banner312 : BaseReinoBanner
    {
        [Constructable]
        public Banner312() : base(0x3C4C)
        {
        }

        public Banner312(Serial serial) : base(serial)
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

    public class Banner313 : BaseReinoBanner
    {
        [Constructable]
        public Banner313() : base(0x3C4D)
        {
        }

        public Banner313(Serial serial) : base(serial)
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

    public class Banner314 : BaseReinoBanner
    {
        [Constructable]
        public Banner314() : base(0x3C4E)
        {
        }

        public Banner314(Serial serial) : base(serial)
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

    public class Banner315 : BaseReinoBanner
    {
        [Constructable]
        public Banner315() : base(0x3C4F)
        {
        }

        public Banner315(Serial serial) : base(serial)
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

    public class Banner316 : BaseReinoBanner
    {
        [Constructable]
        public Banner316() : base(0x3C50)
        {
        }

        public Banner316(Serial serial) : base(serial)
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

    public class Banner317 : BaseReinoBanner
    {
        [Constructable]
        public Banner317() : base(0x3C51)
        {
        }

        public Banner317(Serial serial) : base(serial)
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

    public class Banner318 : BaseReinoBanner
    {
        [Constructable]
        public Banner318() : base(0x3C52)
        {
        }

        public Banner318(Serial serial) : base(serial)
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

    public class Banner319 : BaseReinoBanner
    {
        [Constructable]
        public Banner319() : base(0x3C53)
        {
        }

        public Banner319(Serial serial) : base(serial)
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

    public class Banner320 : BaseReinoBanner
    {
        [Constructable]
        public Banner320() : base(0x3C54)
        {
        }

        public Banner320(Serial serial) : base(serial)
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

    public class Banner321 : BaseReinoBanner
    {
        [Constructable]
        public Banner321() : base(0x3C55)
        {
        }

        public Banner321(Serial serial) : base(serial)
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

    public class Banner322 : BaseReinoBanner
    {
        [Constructable]
        public Banner322() : base(0x3C56)
        {
        }

        public Banner322(Serial serial) : base(serial)
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

    public class Banner323 : BaseReinoBanner
    {
        [Constructable]
        public Banner323() : base(0x3C57)
        {
        }

        public Banner323(Serial serial) : base(serial)
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

    public class Banner324 : BaseReinoBanner
    {
        [Constructable]
        public Banner324() : base(0x3C58)
        {
        }

        public Banner324(Serial serial) : base(serial)
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

    public class Banner325 : BaseReinoBanner
    {
        [Constructable]
        public Banner325() : base(0x3C59)
        {
        }

        public Banner325(Serial serial) : base(serial)
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

    public class Banner326 : BaseReinoBanner
    {
        [Constructable]
        public Banner326() : base(0x3C5A)
        {
        }

        public Banner326(Serial serial) : base(serial)
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

    public class Banner327 : BaseReinoBanner
    {
        [Constructable]
        public Banner327() : base(0x3C5B)
        {
        }

        public Banner327(Serial serial) : base(serial)
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

    public class Banner328 : BaseReinoBanner
    {
        [Constructable]
        public Banner328() : base(0x3C5C)
        {
        }

        public Banner328(Serial serial) : base(serial)
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

    public class Banner329 : BaseReinoBanner
    {
        [Constructable]
        public Banner329() : base(0x3C5D)
        {
        }

        public Banner329(Serial serial) : base(serial)
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

    public class Banner330 : BaseReinoBanner
    {
        [Constructable]
        public Banner330() : base(0x3C5E)
        {
        }

        public Banner330(Serial serial) : base(serial)
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

    public class Banner331 : BaseReinoBanner
    {
        [Constructable]
        public Banner331() : base(0x3C5F)
        {
        }

        public Banner331(Serial serial) : base(serial)
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

    public class Banner332 : BaseReinoBanner
    {
        [Constructable]
        public Banner332() : base(0x3C60)
        {
        }

        public Banner332(Serial serial) : base(serial)
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

    public class Banner333 : BaseReinoBanner
    {
        [Constructable]
        public Banner333() : base(0x3C61)
        {
        }

        public Banner333(Serial serial) : base(serial)
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

    public class Banner334 : BaseReinoBanner
    {
        [Constructable]
        public Banner334() : base(0x3C62)
        {
        }

        public Banner334(Serial serial) : base(serial)
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

    public class Banner335 : BaseReinoBanner
    {
        [Constructable]
        public Banner335() : base(0x3C63)
        {
        }

        public Banner335(Serial serial) : base(serial)
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

    public class Banner336 : BaseReinoBanner
    {
        [Constructable]
        public Banner336() : base(0x3C64)
        {
        }

        public Banner336(Serial serial) : base(serial)
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

    public class Banner337 : BaseReinoBanner
    {
        [Constructable]
        public Banner337() : base(0x3C65)
        {
        }

        public Banner337(Serial serial) : base(serial)
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

    public class Banner338 : BaseReinoBanner
    {
        [Constructable]
        public Banner338() : base(0x3C66)
        {
        }

        public Banner338(Serial serial) : base(serial)
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

    public class Banner339 : BaseReinoBanner
    {
        [Constructable]
        public Banner339() : base(0x3C67)
        {
        }

        public Banner339(Serial serial) : base(serial)
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

    public class Banner340 : BaseReinoBanner
    {
        [Constructable]
        public Banner340() : base(0x3C68)
        {
        }

        public Banner340(Serial serial) : base(serial)
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

    public class Banner341 : BaseReinoBanner
    {
        [Constructable]
        public Banner341() : base(0x3C69)
        {
        }

        public Banner341(Serial serial) : base(serial)
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

    public class Banner342 : BaseReinoBanner
    {
        [Constructable]
        public Banner342() : base(0x3C6A)
        {
        }

        public Banner342(Serial serial) : base(serial)
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

    public class Banner343 : BaseReinoBanner
    {
        [Constructable]
        public Banner343() : base(0x3C6B)
        {
        }

        public Banner343(Serial serial) : base(serial)
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

    public class Banner344 : BaseReinoBanner
    {
        [Constructable]
        public Banner344() : base(0x3C6C)
        {
        }

        public Banner344(Serial serial) : base(serial)
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
