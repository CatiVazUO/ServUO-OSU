using Server;

namespace Server.Custom.Systems.Reinos
{
    public class CorreiosAuroraFase1Multi : ReinoPlacedMultiBase
    {
        [Constructable] public CorreiosAuroraFase1Multi() : this(0, string.Empty, 0) { }
        public CorreiosAuroraFase1Multi(int referenceId, string constructionId, int stageIndex) : base(0x68, referenceId, constructionId, stageIndex) { Name = "Correios Aurora (Fase 1)"; }
        public CorreiosAuroraFase1Multi(Serial serial) : base(serial) { }
    }
    public class CorreiosAuroraFase2Multi : ReinoPlacedMultiBase
    {
        [Constructable] public CorreiosAuroraFase2Multi() : this(0, string.Empty, 1) { }
        public CorreiosAuroraFase2Multi(int referenceId, string constructionId, int stageIndex) : base(0x6A, referenceId, constructionId, stageIndex) { Name = "Correios Aurora (Fase 2)"; }
        public CorreiosAuroraFase2Multi(Serial serial) : base(serial) { }
    }
    public class CorreiosAuroraFase3Multi : ReinoPlacedMultiBase
    {
        [Constructable] public CorreiosAuroraFase3Multi() : this(0, string.Empty, 2) { }
        public CorreiosAuroraFase3Multi(int referenceId, string constructionId, int stageIndex) : base(0x64, referenceId, constructionId, stageIndex) { Name = "Correios Aurora (Fase 3)"; }
        public CorreiosAuroraFase3Multi(Serial serial) : base(serial) { }
    }
    public class CorreiosAuroraProntoMulti : ReinoPlacedMultiBase
    {
        [Constructable] public CorreiosAuroraProntoMulti() : this(0, string.Empty, -1) { }
        public CorreiosAuroraProntoMulti(int referenceId, string constructionId, int stageIndex) : base(0x8C, referenceId, constructionId, stageIndex) { Name = "Correios Aurora"; }
        public CorreiosAuroraProntoMulti(Serial serial) : base(serial) { }
    }
    public class CorreiosAuroraAbandonadoMulti : ReinoPlacedMultiBase
    {
        [Constructable] public CorreiosAuroraAbandonadoMulti() : this(0, string.Empty, -2) { }
        public CorreiosAuroraAbandonadoMulti(int referenceId, string constructionId, int stageIndex) : base(0x98, referenceId, constructionId, stageIndex) { Name = "Correios Aurora Abandonado"; }
        public CorreiosAuroraAbandonadoMulti(Serial serial) : base(serial) { }
    }
}
