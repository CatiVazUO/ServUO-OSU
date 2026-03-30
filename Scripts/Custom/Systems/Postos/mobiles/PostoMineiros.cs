using Server;
using Server.Custom.Systems.Postos;

namespace Server.Mobiles
{
    public class MineiroDoPostoAramute : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoAramute() : base("aramute")
        {
            Name = "Darian";
            Title = "o capataz das galerias";
        }

        public MineiroDoPostoAramute(Serial serial) : base(serial)
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
            Name = "Darian";
            Title = "o capataz das galerias";
        }
    }

    public class MineiroDoPostoDorvok : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoDorvok() : base("dorvok")
        {
            Name = "Bruna";
            Title = "a mestra das escoras";
        }

        public MineiroDoPostoDorvok(Serial serial) : base(serial)
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
            Name = "Bruna";
            Title = "a mestra das escoras";
        }
    }

    public class MineiroDoPostoSelgard : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoSelgard() : base("selgard")
        {
            Name = "Odrik";
            Title = "o vigia dos veios";
        }

        public MineiroDoPostoSelgard(Serial serial) : base(serial)
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
            Name = "Odrik";
            Title = "o vigia dos veios";
        }
    }

    public class MineiroDoPostoKarstun : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoKarstun() : base("karstun")
        {
            Name = "Maela";
            Title = "a capataz da pedreira";
        }

        public MineiroDoPostoKarstun(Serial serial) : base(serial)
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
            Name = "Maela";
            Title = "a capataz da pedreira";
        }
    }

    public class MineiroDoPostoVhalor : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoVhalor() : base("vhalor")
        {
            Name = "Serkan";
            Title = "o administrador do corte profundo";
        }

        public MineiroDoPostoVhalor(Serial serial) : base(serial)
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
            Name = "Serkan";
            Title = "o administrador do corte profundo";
        }
    }

    public class MineiroDoPostoNargesh : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoNargesh() : base("nargesh")
        {
            Name = "Borun";
            Title = "o mestre dos guinchos";
        }

        public MineiroDoPostoNargesh(Serial serial) : base(serial)
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
            Name = "Borun";
            Title = "o mestre dos guinchos";
        }
    }

    public class MineiroDoPostoTirak : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoTirak() : base("tirak")
        {
            Name = "Helka";
            Title = "a guardiã do poço de carvão";
        }

        public MineiroDoPostoTirak(Serial serial) : base(serial)
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
            Name = "Helka";
            Title = "a guardiã do poço de carvão";
        }
    }

    public class MineiroDoPostoThorma : BasePostoNPC
    {
        [Constructable]
        public MineiroDoPostoThorma() : base("thorma")
        {
            Name = "Edras";
            Title = "o vigia dos túneis quentes";
        }

        public MineiroDoPostoThorma(Serial serial) : base(serial)
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
            Name = "Edras";
            Title = "o vigia dos túneis quentes";
        }
    }

}