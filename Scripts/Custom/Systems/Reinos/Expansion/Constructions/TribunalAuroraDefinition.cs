using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class TribunalAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "tribunal_aurora";
        private const string DISPLAY_NAME = "Tribunal";
        private const int REQUIRED_LOT_SIDE = 20;

        public static readonly Point3D DESK_OFFSET = new Point3D(1, 1, 0);
        public static readonly Point3D OFFICER_OFFSET = new Point3D(8, 5, 0);
        public const int OFFICER_Z_OFFSET = 0;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 700),
            new ReinoResourceCost(ReinoResourceType.Iron, 30),
            new ReinoResourceCost(ReinoResourceType.Cloth, 30),
            new ReinoResourceCost(ReinoResourceType.Wood, 50)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 200),
            new ReinoResourceCost(ReinoResourceType.Iron, 10),
            new ReinoResourceCost(ReinoResourceType.Cloth, 10),
            new ReinoResourceCost(ReinoResourceType.Wood, 20)
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

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Tribunal</B><BR><BR>");
            sb.Append("<B>Tamanho mínimo do lote:</B> 20x20.<BR>");
            sb.Append("<B>Construção:</B> 700 moedas, 30 ferro, 30 tecidos e 50 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 200 moedas, 10 ferro, 10 tecidos e 20 madeira.<BR>");
            sb.Append("<B>Salário do oficial de justiça:</B> 70 moedas por semana.<BR><BR>");
            sb.Append("Use este arquivo para ajustar as coordenadas da mesa do tribunal e do oficial de justiça. Nesta versão o tribunal usa o mesmo multi placeholder dos correios, como você pediu.");
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
            def.FinishedPlacedTypeName = typeof(ReinoTribunalMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcTypeName = typeof(OSUJusticeOfficer).FullName;
            def.NpcOffset = OFFICER_OFFSET;
            def.NpcZOffset = OFFICER_Z_OFFSET;
            def.UseMultiDoors = true;
            def.ReactivateDuration = TimeSpan.FromMinutes(3.0);
            def.Permanent = false;
            def.MaintenancePriority = 2;
            def.AllowManualActivationToggle = true;
            def.AllowPriorityChange = true;
            def.NpcWeeklySalaryGold = 70;
            return def;
        }
    }
}
