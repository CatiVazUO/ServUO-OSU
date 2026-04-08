using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class PrisaoAuroraDefinition
    {
        private const int CITY_ID = 0;
        private const string BUILDING_ID = "prisao_aurora";
        private const string DISPLAY_NAME = "Prisão";
        private const int REQUIRED_LOT_SIDE = 30;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 800),
            new ReinoResourceCost(ReinoResourceType.Cloth, 200),
            new ReinoResourceCost(ReinoResourceType.Iron, 200),
            new ReinoResourceCost(ReinoResourceType.Wood, 200)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[0];

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

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Prisão</B><BR><BR>");
            sb.Append("<B>Tamanho mínimo do lote:</B> 30x30.<BR>");
            sb.Append("<B>Construção:</B> 800 moedas, 200 tecidos, 200 ferro e 200 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 20 moedas por preso e 10 de cada recurso por preso.<BR><BR>");
            sb.Append("As 5 celas desta primeira versão ficam configuradas por offsets internos para facilitar o teste. Os offsets usados são: (5,6,0), (7,6,0), (9,6,0), (11,6,0) e (13,6,0), sempre a partir do canto noroeste do lote.<BR><BR>");
            sb.Append("Nesta primeira versão ela também usa o multi placeholder dos correios. O item interno da prisão já fica pronto para o futuro gump do diretor da prisão.");
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
            def.FinishedPlacedTypeName = typeof(ReinoPrisaoMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcTypeName = String.Empty;
            def.UseMultiDoors = true;
            def.ReactivateDuration = TimeSpan.FromMinutes(3.0);
            def.Permanent = false;
            def.MaintenancePriority = 2;
            def.AllowManualActivationToggle = true;
            def.AllowPriorityChange = true;
            def.NpcWeeklySalaryGold = 0;
            return def;
        }
    }
}
