using Server;

namespace Server.Custom.Systems.Reinos
{
    public class CorreiosAuroraFase1Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public CorreiosAuroraFase1Multi() : this(0, string.Empty, 0)
        {
        }

        public CorreiosAuroraFase1Multi(int referenceId, string constructionId, int stageIndex)
            : base(0xA3, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Correios";
        }

        public CorreiosAuroraFase1Multi(Serial serial) : base(serial)
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
    public class CorreiosAuroraFase2Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public CorreiosAuroraFase2Multi() : this(0, string.Empty, 1)
        {
        }

        public CorreiosAuroraFase2Multi(int referenceId, string constructionId, int stageIndex)
            : base(0xA4, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Correios";
        }

        public CorreiosAuroraFase2Multi(Serial serial) : base(serial)
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
    public class CorreiosAuroraFase3Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public CorreiosAuroraFase3Multi() : this(0, string.Empty, 2)
        {
        }

        public CorreiosAuroraFase3Multi(int referenceId, string constructionId, int stageIndex)
            : base(0xA5, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Correios";
        }

        public CorreiosAuroraFase3Multi(Serial serial) : base(serial)
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

    public class CorreiosAuroraFase4Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public CorreiosAuroraFase4Multi() : this(0, string.Empty, 3)
        {
        }

        public CorreiosAuroraFase4Multi(int referenceId, string constructionId, int stageIndex)
            : base(0xA6, referenceId, constructionId, stageIndex)
        {
            Name = "Construção Correios";
        }

        public CorreiosAuroraFase4Multi(Serial serial) : base(serial)
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
    public class CorreiosAuroraProntoMulti : ReinoPlacedMultiBase
    {
        [Constructable]
        public CorreiosAuroraProntoMulti() : this(0, string.Empty, -1)
        {
        }

        public CorreiosAuroraProntoMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA7, referenceId, constructionId, stageIndex)
        {
            Name = "Correios Aurora";
        }

        public CorreiosAuroraProntoMulti(Serial serial) : base(serial)
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
    public class CorreiosAuroraAbandonadoMulti : ReinoPlacedMultiBase
    {
        [Constructable]
        public CorreiosAuroraAbandonadoMulti() : this(0, string.Empty, -2)
        {
        }

        public CorreiosAuroraAbandonadoMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA8, referenceId, constructionId, stageIndex)
        {
            Name = "Fechado";
        }

        public CorreiosAuroraAbandonadoMulti(Serial serial) : base(serial)
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
