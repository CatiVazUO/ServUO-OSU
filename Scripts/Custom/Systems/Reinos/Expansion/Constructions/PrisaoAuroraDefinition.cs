using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class PrisaoAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "prisao_aurora";
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

        private static readonly Point3D[] CELL_OFFSETS = new Point3D[]
        {
            new Point3D(5, 6, 0),
            new Point3D(7, 6, 0),
            new Point3D(9, 6, 0),
            new Point3D(11, 6, 0),
            new Point3D(13, 6, 0)
        };

        private static readonly Point3D INTERROGATION_OFFSET = new Point3D(15, 10, 0);
        private static readonly Point3D DESK_OFFSET = new Point3D(1, 1, 0);
        private static readonly Point3D LOCKER_OFFSET = new Point3D(2, 1, 0);
        private static readonly Point3D JAILER_OFFSET = new Point3D(3, 2, 0);
        private static readonly Point3D GUARD_OFFSET = new Point3D(5, 2, 0);
        private static readonly Point3D RELEASE_OFFSET = new Point3D(1, 0, 0);

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

        public static Point3D GetCellOffset(int index)
        {
            if (index < 0 || index >= CELL_OFFSETS.Length)
                index = 0;

            return CELL_OFFSETS[index];
        }

        public static Point3D GetInterrogationOffset()
        {
            return INTERROGATION_OFFSET;
        }

        public static Point3D GetDeskOffset()
        {
            return DESK_OFFSET;
        }

        public static Point3D GetLockerOffset()
        {
            return LOCKER_OFFSET;
        }

        public static Point3D GetJailerOffset()
        {
            return JAILER_OFFSET;
        }

        public static Point3D GetPrisonGuardOffset()
        {
            return GUARD_OFFSET;
        }

        public static Point3D GetReleaseOffset()
        {
            return RELEASE_OFFSET;
        }

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Prisão</B><BR><BR>");
            sb.Append("<B>Tamanho mínimo do lote:</B> 30x30.<BR>");
            sb.Append("<B>Construção:</B> 800 moedas, 200 tecidos, 200 ferro e 200 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> variável conforme presos, refeições e multas ativas.<BR>");
            sb.Append("<B>Salário dos npcs:</B> 50 moedas por semana para cada npc da prisão.<BR><BR>");
            sb.Append("A definition da prisão guarda offsets configuráveis para 5 celas, sala de interrogatório, ponto de soltura, mesa, baú e os 2 npcs próprios da prisão.<BR><BR>");
            sb.Append("Nesta primeira versão ela continua usando o multi placeholder dos correios, como você pediu.");
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
            def.NpcWeeklySalaryGold = 50;
            return def;
        }
    }
}
