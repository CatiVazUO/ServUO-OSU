using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Hoods;

namespace Server.Items
{
    public abstract class OSUBaseOcultingHood : BaseHat
    {
        public OSUBaseOcultingHood(int itemId) : base(itemId)
        {
            Name = "capuz";
            Weight = 1.0;
        }

        public OSUBaseOcultingHood(Serial serial) : base(serial)
        {
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);
            Mobile m = parent as Mobile;
            if (m != null)
                OSUHoodVisibilitySystem.Refresh(m);
        }

        public override void OnRemoved(object parent)
        {
            Mobile m = parent as Mobile;
            if (m != null)
                OSUHoodVisibilitySystem.Refresh(m);
            base.OnRemoved(parent);
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

    public class CapuzCB20 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB20() : base(0xCB20) { }

        public CapuzCB20(Serial serial) : base(serial) { }

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

    public class CapuzCB21 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB21() : base(0xCB21) { }

        public CapuzCB21(Serial serial) : base(serial) { }

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

    public class CapuzCB22 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB22() : base(0xCB22) { }

        public CapuzCB22(Serial serial) : base(serial) { }

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

    public class CapuzCB23 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB23() : base(0xCB23) { }

        public CapuzCB23(Serial serial) : base(serial) { }

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

    public class CapuzCB24 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB24() : base(0xCB24) { }

        public CapuzCB24(Serial serial) : base(serial) { }

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

    public class CapuzCB25 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB25() : base(0xCB25) { }

        public CapuzCB25(Serial serial) : base(serial) { }

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

    public class CapuzCB26 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB26() : base(0xCB26) { }

        public CapuzCB26(Serial serial) : base(serial) { }

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

    public class CapuzCB27 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB27() : base(0xCB27) { }

        public CapuzCB27(Serial serial) : base(serial) { }

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

    public class CapuzCB28 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB28() : base(0xCB28) { }

        public CapuzCB28(Serial serial) : base(serial) { }

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

    public class CapuzCB29 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB29() : base(0xCB29) { }

        public CapuzCB29(Serial serial) : base(serial) { }

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

    public class CapuzCB2A : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB2A() : base(0xCB2A) { }

        public CapuzCB2A(Serial serial) : base(serial) { }

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

    public class CapuzCB2B : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB2B() : base(0xCB2B) { }

        public CapuzCB2B(Serial serial) : base(serial) { }

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

    public class CapuzCB2C : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB2C() : base(0xCB2C) { }

        public CapuzCB2C(Serial serial) : base(serial) { }

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

    public class CapuzCB2D : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB2D() : base(0xCB2D) { }

        public CapuzCB2D(Serial serial) : base(serial) { }

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

    public class CapuzCB2E : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB2E() : base(0xCB2E) { }

        public CapuzCB2E(Serial serial) : base(serial) { }

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

    public class CapuzCB2F : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB2F() : base(0xCB2F) { }

        public CapuzCB2F(Serial serial) : base(serial) { }

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

    public class CapuzCB30 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB30() : base(0xCB30) { }

        public CapuzCB30(Serial serial) : base(serial) { }

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

    public class CapuzCB31 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB31() : base(0xCB31) { }

        public CapuzCB31(Serial serial) : base(serial) { }

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

    public class CapuzCB32 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB32() : base(0xCB32) { }

        public CapuzCB32(Serial serial) : base(serial) { }

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

    public class CapuzCB33 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB33() : base(0xCB33) { }

        public CapuzCB33(Serial serial) : base(serial) { }

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

    public class CapuzCB34 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB34() : base(0xCB34) { }

        public CapuzCB34(Serial serial) : base(serial) { }

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

    public class CapuzCB35 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB35() : base(0xCB35) { }

        public CapuzCB35(Serial serial) : base(serial) { }

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

    public class CapuzCB36 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB36() : base(0xCB36) { }

        public CapuzCB36(Serial serial) : base(serial) { }

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

    public class CapuzCB37 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB37() : base(0xCB37) { }

        public CapuzCB37(Serial serial) : base(serial) { }

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

    public class CapuzCB38 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB38() : base(0xCB38) { }

        public CapuzCB38(Serial serial) : base(serial) { }

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

    public class CapuzCB39 : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB39() : base(0xCB39) { }

        public CapuzCB39(Serial serial) : base(serial) { }

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

    public class CapuzCB3A : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB3A() : base(0xCB3A) { }

        public CapuzCB3A(Serial serial) : base(serial) { }

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

    public class CapuzCB3B : OSUBaseOcultingHood
    {
        [Constructable]
        public CapuzCB3B() : base(0xCB3B) { }

        public CapuzCB3B(Serial serial) : base(serial) { }

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
}
