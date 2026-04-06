using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class BibliotecaAuroraTesteDefinition
    {
        private const int CITY_ID = 0;
        private const string BUILDING_ID = "biblioteca_aurora_teste";
        private const string DISPLAY_NAME = "Biblioteca";
        private const int REQUIRED_LOT_SIDE = 15;

        private const string NPC_TYPE_NAME = "Server.Custom.Biblioteca.Bibliotecario";
        private static readonly Point3D NPC_OFFSET = new Point3D(8, 4, 0);
        private const int NPC_Z_OFFSET = 5;

        private static readonly string[] EXTRA_NPC_TYPES = new string[]
        {
            NPC_TYPE_NAME
        };

        private static readonly Point3D[] EXTRA_NPC_OFFSETS = new Point3D[]
        {
            new Point3D(8, 3, 0)
        };

        private static readonly int[] EXTRA_NPC_Z_OFFSETS = new int[]
        {
            5
        };

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Wood, 100),
            new ReinoResourceCost(ReinoResourceType.Iron, 100),
            new ReinoResourceCost(ReinoResourceType.Cloth, 100)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Wood, 30),
            new ReinoResourceCost(ReinoResourceType.Iron, 30),
            new ReinoResourceCost(ReinoResourceType.Cloth, 30)
        };

        private static readonly int[] STAGE_MULTI_IDS = new int[] { 0xA3, 0xA4, 0xA5, 0xA6 };
        private static readonly TimeSpan[] STAGE_DURATIONS = new TimeSpan[]
        {
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0)
        };

        private const int FINISHED_MULTI_ID = 0xA7;
        private const int ABANDONED_MULTI_ID = 0xA8;
        private static readonly TimeSpan REACTIVATE_DURATION = TimeSpan.FromMinutes(1.0);

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Biblioteca</B><BR><BR>");
            sb.Append("<B>Construção:</B> 100 madeira, 100 ferro e 100 tecido.<BR>");
            sb.Append("<B>Manutenção:</B> 30 madeira, 30 ferro e 30 tecido por cobrança.<BR><BR>");
            sb.Append("Biblioteca de teste usando exatamente os mesmos multis do Correios, mas com dois bibliotecários para validar a matemática do gump de manutenção.<BR><BR>");
            sb.Append("Quando o multi final for trocado, basta substituir os IDs aqui mais tarde.");
            sb.Append("</BASEFONT>");

            ReinoConstructionDefinition def = new ReinoConstructionDefinition();
            def.Id = BUILDING_ID;
            def.Name = DISPLAY_NAME;
            def.RequiredCityId = CITY_ID;
            def.TargetType = ReinoBuildTargetType.Lot;
            def.MinimumLotSide = REQUIRED_LOT_SIDE;
            def.AllowedLotSides = new int[] { REQUIRED_LOT_SIDE };
            def.DescriptionHtml = sb.ToString();
            def.BuildCosts = BUILD_COSTS;
            def.MaintenanceCosts = MAINTENANCE_COSTS;
            def.StageMultiIds = STAGE_MULTI_IDS;
            def.StageDurations = STAGE_DURATIONS;
            def.FinishedMultiId = FINISHED_MULTI_ID;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcTypeName = NPC_TYPE_NAME;
            def.NpcOffset = NPC_OFFSET;
            def.NpcZOffset = NPC_Z_OFFSET;
            def.AdditionalNpcTypeNames = EXTRA_NPC_TYPES;
            def.AdditionalNpcOffsets = EXTRA_NPC_OFFSETS;
            def.AdditionalNpcZOffsets = EXTRA_NPC_Z_OFFSETS;
            def.UseMultiDoors = true;
            def.ReactivateDuration = REACTIVATE_DURATION;
            def.Permanent = false;
            def.MaintenancePriority = 2;
            def.AllowManualActivationToggle = true;
            def.AllowPriorityChange = true;
            def.NpcWeeklySalaryGold = 50;
            return def;
        }
    }
}
