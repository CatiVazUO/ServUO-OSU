using Server;

namespace Server.Items
{
    public class BauDoPostoAramute : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoAramute() : base("aramute")
        {
        }

        public BauDoPostoAramute(Serial serial) : base(serial)
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

    public class BauDoPostoDorvok : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoDorvok() : base("dorvok")
        {
        }

        public BauDoPostoDorvok(Serial serial) : base(serial)
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

    public class BauDoPostoSelgard : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoSelgard() : base("selgard")
        {
        }

        public BauDoPostoSelgard(Serial serial) : base(serial)
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

    public class BauDoPostoKarstun : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoKarstun() : base("karstun")
        {
        }

        public BauDoPostoKarstun(Serial serial) : base(serial)
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

    public class BauDoPostoVhalor : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoVhalor() : base("vhalor")
        {
        }

        public BauDoPostoVhalor(Serial serial) : base(serial)
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

    public class BauDoPostoNargesh : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoNargesh() : base("nargesh")
        {
        }

        public BauDoPostoNargesh(Serial serial) : base(serial)
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

    public class BauDoPostoTirak : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoTirak() : base("tirak")
        {
        }

        public BauDoPostoTirak(Serial serial) : base(serial)
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

    public class BauDoPostoThorma : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoThorma() : base("thorma")
        {
        }

        public BauDoPostoThorma(Serial serial) : base(serial)
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
