using Server;
using Server.Custom.Systems.Postos;

namespace Server.Mobiles
{
    public class LenhadorDoPostoCunhau : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoCunhau() : base("cunhau")
        {
            Name = "Ivar";
            Title = "o guarda-trilhas";
        }

        public LenhadorDoPostoCunhau(Serial serial) : base(serial)
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
            Name = "Ivar";
            Title = "o guarda-trilhas";
        }
    }

    public class LenhadorDoPostoBelorim : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoBelorim() : base("belorim")
        {
            Name = "Alenna";
            Title = "a mestra dos pinhais";
        }

        public LenhadorDoPostoBelorim(Serial serial) : base(serial)
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
            Name = "Alenna";
            Title = "a mestra dos pinhais";
        }
    }

    public class LenhadorDoPostoValesca : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoValesca() : base("valesca")
        {
            Name = "Toren";
            Title = "o abridor do emaranhado";
        }

        public LenhadorDoPostoValesca(Serial serial) : base(serial)
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
            Name = "Toren";
            Title = "o abridor do emaranhado";
        }
    }

    public class LenhadorDoPostoNorvind : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoNorvind() : base("norvind")
        {
            Name = "Celis";
            Title = "a vigia das clareiras";
        }

        public LenhadorDoPostoNorvind(Serial serial) : base(serial)
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
            Name = "Celis";
            Title = "a vigia das clareiras";
        }
    }

    public class LenhadorDoPostoTalbrasa : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoTalbrasa() : base("talbrasa")
        {
            Name = "Hadrik";
            Title = "o capataz da serraria";
        }

        public LenhadorDoPostoTalbrasa(Serial serial) : base(serial)
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
            Name = "Hadrik";
            Title = "o capataz da serraria";
        }
    }

    public class LenhadorDoPostoRivenoak : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoRivenoak() : base("rivenoak")
        {
            Name = "Orwen";
            Title = "o supervisor do corte";
        }

        public LenhadorDoPostoRivenoak(Serial serial) : base(serial)
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
            Name = "Orwen";
            Title = "o supervisor do corte";
        }
    }

    public class LenhadorDoPostoGaldrin : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoGaldrin() : base("galdrin")
        {
            Name = "Mirela";
            Title = "a mestra das carroças";
        }

        public LenhadorDoPostoGaldrin(Serial serial) : base(serial)
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
            Name = "Mirela";
            Title = "a mestra das carroças";
        }
    }

    public class LenhadorDoPostoUlmora : BasePostoNPC
    {
        [Constructable]
        public LenhadorDoPostoUlmora() : base("ulmora")
        {
            Name = "Edrin";
            Title = "o guardião da mata funda";
        }

        public LenhadorDoPostoUlmora(Serial serial) : base(serial)
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
            Name = "Edrin";
            Title = "o guardião da mata funda";
        }
    }

}