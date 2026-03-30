using Server;

namespace Server.Items
{
    public class BauDoPostoCunhau : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoCunhau() : base("cunhau")
        {
        }

        public BauDoPostoCunhau(Serial serial) : base(serial)
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

    public class BauDoPostoBelorim : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoBelorim() : base("belorim")
        {
        }

        public BauDoPostoBelorim(Serial serial) : base(serial)
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

    public class BauDoPostoValesca : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoValesca() : base("valesca")
        {
        }

        public BauDoPostoValesca(Serial serial) : base(serial)
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

    public class BauDoPostoNorvind : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoNorvind() : base("norvind")
        {
        }

        public BauDoPostoNorvind(Serial serial) : base(serial)
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

    public class BauDoPostoTalbrasa : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoTalbrasa() : base("talbrasa")
        {
        }

        public BauDoPostoTalbrasa(Serial serial) : base(serial)
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

    public class BauDoPostoRivenoak : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoRivenoak() : base("rivenoak")
        {
        }

        public BauDoPostoRivenoak(Serial serial) : base(serial)
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

    public class BauDoPostoGaldrin : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoGaldrin() : base("galdrin")
        {
        }

        public BauDoPostoGaldrin(Serial serial) : base(serial)
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

    public class BauDoPostoUlmora : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoUlmora() : base("ulmora")
        {
        }

        public BauDoPostoUlmora(Serial serial) : base(serial)
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
