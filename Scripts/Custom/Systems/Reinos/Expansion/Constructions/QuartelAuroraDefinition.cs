using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class QuartelAuroraDefinition
    {
        private const int CITY_ID = 0;
        private const string BUILDING_ID = "quartel_aurora";
        private const string DISPLAY_NAME = "Quartel";
        private const int REQUIRED_LOT_SIDE = 30;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 2000),
            new ReinoResourceCost(ReinoResourceType.Cloth, 500),
            new ReinoResourceCost(ReinoResourceType.Iron, 500),
            new ReinoResourceCost(ReinoResourceType.Wood, 500)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 200),
            new ReinoResourceCost(ReinoResourceType.Cloth, 50),
            new ReinoResourceCost(ReinoResourceType.Iron, 50),
            new ReinoResourceCost(ReinoResourceType.Wood, 50)
        };

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

        private static readonly Point3D DESK_OFFSET = new Point3D(2, 1, 0);
        private static readonly Point3D LOCKER_OFFSET = new Point3D(1, 1, 0);
        private static readonly Point3D LAW_BOARD_OFFSET = new Point3D(3, 1, 0);


        public static Point3D GetDeskOffset()
        {
            return DESK_OFFSET;
        }

        public static Point3D GetLockerOffset()
        {
            return LOCKER_OFFSET;
        }

        public static Point3D GetLawBoardOffset()
        {
            return LAW_BOARD_OFFSET;
        }

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Quartel</B><BR><BR>");
            sb.Append("<B>Tamanho mínimo do lote:</B> 30x30.<BR>");
            sb.Append("<B>Construção:</B> 2000 moedas, 500 tecidos, 500 ferro e 500 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 200 moedas, 50 tecidos, 50 ferro e 50 madeira.<BR><BR>");
            sb.Append("O quartel habilita a parte militar do reino. É dele que saem a caixa de apreensões, a mesa do quartel, a contratação dos guardas, as rotas e o treinamento.<BR><BR>");
            sb.Append("Nesta primeira versão ele usa o mesmo multi placeholder dos correios, como você pediu. O item interno do quartel abre a aba militar já focada em guardas, rotas e treinamento.");
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
            def.FinishedPlacedTypeName = typeof(ReinoQuartelMulti).FullName;
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
