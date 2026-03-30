using Server;

namespace Server.Items
{
    public class BauDoPostoSaial : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoSaial() : base("saial")
        {
        }

        public BauDoPostoSaial(Serial serial) : base(serial)
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

    public class BauDoPostoIriande : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoIriande() : base("iriande")
        {
        }

        public BauDoPostoIriande(Serial serial) : base(serial)
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

    public class BauDoPostoBelsara : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoBelsara() : base("belsara")
        {
        }

        public BauDoPostoBelsara(Serial serial) : base(serial)
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

    public class BauDoPostoRosamar : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoRosamar() : base("rosamar")
        {
        }

        public BauDoPostoRosamar(Serial serial) : base(serial)
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

    public class BauDoPostoDalvila : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoDalvila() : base("dalvila")
        {
        }

        public BauDoPostoDalvila(Serial serial) : base(serial)
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

    public class BauDoPostoOrquessa : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoOrquessa() : base("orquessa")
        {
        }

        public BauDoPostoOrquessa(Serial serial) : base(serial)
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

    public class BauDoPostoVentalva : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoVentalva() : base("ventalva")
        {
        }

        public BauDoPostoVentalva(Serial serial) : base(serial)
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

    public class BauDoPostoLumera : PostoResourceChest
    {
        [Constructable]
        public BauDoPostoLumera() : base("lumera")
        {
        }

        public BauDoPostoLumera(Serial serial) : base(serial)
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
