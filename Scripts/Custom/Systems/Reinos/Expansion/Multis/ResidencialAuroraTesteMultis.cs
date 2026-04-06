using Server;

namespace Server.Custom.Reinos
{
    public class ResidencialAuroraTesteFase1Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTesteFase1Multi() : this(0, string.Empty, 0)
        {
        }

        public ResidencialAuroraTesteFase1Multi(int referenceId, string constructionId, int stageIndex)
            : base(0x68, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Residencial Aurora";
        }

        public ResidencialAuroraTesteFase1Multi(Serial serial) : base(serial)
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
    public class ResidencialAuroraTesteFase2Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTesteFase2Multi() : this(0, string.Empty, 1)
        {
        }

        public ResidencialAuroraTesteFase2Multi(int referenceId, string constructionId, int stageIndex)
            : base(0x6A, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Residencial Aurora";
        }

        public ResidencialAuroraTesteFase2Multi(Serial serial) : base(serial)
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
    public class ResidencialAuroraTesteFase3Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTesteFase3Multi() : this(0, string.Empty, 2)
        {
        }

        public ResidencialAuroraTesteFase3Multi(int referenceId, string constructionId, int stageIndex)
            : base(0x64, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Residencial Aurora";
        }

        public ResidencialAuroraTesteFase3Multi(Serial serial) : base(serial)
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
    public class ResidencialAuroraTestePlacedMulti : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTestePlacedMulti() : this(0, string.Empty, -1)
        {
        }

        public ResidencialAuroraTestePlacedMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA9, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Residencial Aurora";
        }

        public ResidencialAuroraTestePlacedMulti(Serial serial) : base(serial)
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
    public class ResidencialAuroraTesteAbandonadoMulti : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTesteAbandonadoMulti() : this(0, string.Empty, -2)
        {
        }

        public ResidencialAuroraTesteAbandonadoMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA9, referenceId, constructionId, stageIndex)
        {
            Name = "Residencial Aurora";
        }

        public ResidencialAuroraTesteAbandonadoMulti(Serial serial) : base(serial)
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
