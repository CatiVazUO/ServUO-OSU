using System;
using System.Text;
using Server;

namespace Server.Custom.Systems.Reinos
{
    public static class CorreiosAuroraDefinition
    {
        // ===== CONFIGURAÇÃO DO CORREIOS DE AURORA =====
        private const int CITY_ID = 0; // Aurora
        private const string BUILDING_ID = "correios_aurora";
        private const string DISPLAY_NAME = "Correios";
        private const int REQUIRED_LOT_SIDE = 15;

        // NPC
        private const string NPC_TYPE_NAME = "Server.Custom.Correios.CorreioNPC";
        private static readonly Point3D NPC_OFFSET = new Point3D(7, 3, 0); // meio do lote 15x15
        private const int NPC_Z_OFFSET = 5; // sempre +5 do chão

        // Custos
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

        // Multis
        private static readonly int[] STAGE_MULTI_IDS = new int[]
        {
            0xA3, 
            0xA4, 
            0xA5,
            0xA6
        };

        private static readonly TimeSpan[] STAGE_DURATIONS = new TimeSpan[]
        {
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0)
        };

        private const int FINISHED_MULTI_ID = 0xA7;   
        private const int ABANDONED_MULTI_ID = 0xA8;  
        private static readonly TimeSpan REACTIVATE_DURATION = TimeSpan.FromMinutes(3.0);

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Correios</B><BR><BR>");
            sb.Append("<B>Construção:</B> 100 madeira, 100 ferro e 100 tecido.<BR>");
            sb.Append("<B>Manutenção:</B> 30 madeira, 30 ferro e 30 tecido por cobrança.<BR><BR>");
            sb.Append("Os correios ligam esse ponto do reino ao seu sistema de cartas, publicações e entregas já existente no shard. ");
            sb.Append("Quando ativos, os jogadores passam a ter ali um carteiro funcional para envio e recebimento de correspondência.<BR><BR>");
            sb.Append("Nesta versão de teste, as fases usam os placeholders que você pediu: Small Brick House, Small Wood House, Small Stone and Plaster House, Large Patio House e Small Tower.");
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
            def.UseMultiDoors = true;
            def.ReactivateDuration = REACTIVATE_DURATION;
            def.Permanent = false;
            def.MaintenancePriority = 5;
            return def;
        }
    }
}
