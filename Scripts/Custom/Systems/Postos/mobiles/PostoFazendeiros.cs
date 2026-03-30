using Server;
using Server.Custom.Systems.Postos;

namespace Server.Mobiles
{
    public class FazendeiroDoPostoSaial : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoSaial() : base("saial")
        {
            Name = "Liora";
            Title = "a administradora dos canteiros";
        }

        public FazendeiroDoPostoSaial(Serial serial) : base(serial)
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
            Name = "Liora";
            Title = "a administradora dos canteiros";
        }
    }

    public class FazendeiroDoPostoIriande : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoIriande() : base("iriande")
        {
            Name = "Neris";
            Title = "a guardiã dos sulcos";
        }

        public FazendeiroDoPostoIriande(Serial serial) : base(serial)
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
            Name = "Neris";
            Title = "a guardiã dos sulcos";
        }
    }

    public class FazendeiroDoPostoBelsara : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoBelsara() : base("belsara")
        {
            Name = "Cassia";
            Title = "a mestra da colheita";
        }

        public FazendeiroDoPostoBelsara(Serial serial) : base(serial)
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
            Name = "Cassia";
            Title = "a mestra da colheita";
        }
    }

    public class FazendeiroDoPostoRosamar : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoRosamar() : base("rosamar")
        {
            Name = "Tavian";
            Title = "o zelador dos campos";
        }

        public FazendeiroDoPostoRosamar(Serial serial) : base(serial)
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
            Name = "Tavian";
            Title = "o zelador dos campos";
        }
    }

    public class FazendeiroDoPostoDalvila : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoDalvila() : base("dalvila")
        {
            Name = "Mareen";
            Title = "a intendente das plantações";
        }

        public FazendeiroDoPostoDalvila(Serial serial) : base(serial)
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
            Name = "Mareen";
            Title = "a intendente das plantações";
        }
    }

    public class FazendeiroDoPostoOrquessa : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoOrquessa() : base("orquessa")
        {
            Name = "Doral";
            Title = "o capataz dos celeiros";
        }

        public FazendeiroDoPostoOrquessa(Serial serial) : base(serial)
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
            Name = "Doral";
            Title = "o capataz dos celeiros";
        }
    }

    public class FazendeiroDoPostoVentalva : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoVentalva() : base("ventalva")
        {
            Name = "Sorel";
            Title = "o administrador das várzeas";
        }

        public FazendeiroDoPostoVentalva(Serial serial) : base(serial)
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
            Name = "Sorel";
            Title = "o administrador das várzeas";
        }
    }

    public class FazendeiroDoPostoLumera : BasePostoNPC
    {
        [Constructable]
        public FazendeiroDoPostoLumera() : base("lumera")
        {
            Name = "Evelin";
            Title = "a guardiã dos moinhos";
        }

        public FazendeiroDoPostoLumera(Serial serial) : base(serial)
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
            Name = "Evelin";
            Title = "a guardiã dos moinhos";
        }
    }

}