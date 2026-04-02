using Server;

namespace Server.Custom.Systems.Reinos
{
    public class ResidencialAuroraTesteFase1Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTesteFase1Multi()
            : this(0, string.Empty, 0) { }
        public ResidencialAuroraTesteFase1Multi(int referenceId, string constructionId, int stageIndex)
            : base(0x68, referenceId, constructionId, stageIndex)
        {
            Name = "Residencial Aurora (Fase 1)";
        }

        public ResidencialAuroraTesteFase1Multi (Serial serial) : base(serial) { }
    }
    public class ResidencialAuroraTesteFase2Multi : ReinoPlacedMultiBase
    {
        [Constructable]
        public ResidencialAuroraTesteFase2Multi()
            : this(0, string.Empty, 1) { }
        public ResidencialAuroraTesteFase2Multi(int referenceId, string constructionId, int stageIndex)
            : base(0x6A, referenceId, constructionId, stageIndex)
        {
            Name = "Residencial Aurora (Fase 2)";
        }
        public ResidencialAuroraTesteFase2Multi(Serial serial) : base(serial) { }
    }
    public class ResidencialAuroraTesteFase3Multi : ReinoPlacedMultiBase
    {
        [Constructable] public ResidencialAuroraTesteFase3Multi()
            : this(0, string.Empty, 2) { }
        public ResidencialAuroraTesteFase3Multi(int referenceId, string constructionId, int stageIndex)
            : base(0x64, referenceId, constructionId, stageIndex)
        {
            Name = "Residencial Aurora (Fase 3)";
        }
        public ResidencialAuroraTesteFase3Multi(Serial serial) : base(serial) { }
    }
    public class ResidencialAuroraTestePlacedMulti : ReinoPlacedMultiBase
    {
        [Constructable] public ResidencialAuroraTestePlacedMulti()
            : this(0, string.Empty, -1) { }
        public ResidencialAuroraTestePlacedMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA3, referenceId, constructionId, stageIndex)
        {
            Name = "Residencial Aurora";
        }
        public ResidencialAuroraTestePlacedMulti(Serial serial) : base(serial) { }
    }
    public class ResidencialAuroraTesteAbandonadoMulti : ReinoPlacedMultiBase
    {
        [Constructable] public ResidencialAuroraTesteAbandonadoMulti()
            : this(0, string.Empty, -2) { }
        public ResidencialAuroraTesteAbandonadoMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA4, referenceId, constructionId, stageIndex)
        {
            Name = "Residencial Aurora Abandonado";
        }
        public ResidencialAuroraTesteAbandonadoMulti(Serial serial) : base(serial) { }
    }
}
