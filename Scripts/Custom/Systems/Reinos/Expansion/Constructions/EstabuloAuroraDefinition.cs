using System;
using System.Text;
using Server;
using Server.Custom.Systems.Stables.Mobiles;

namespace Server.Custom.Reinos
{
    public static class EstabuloAuroraDefinition
    {
        public const string BUILDING_ID = "estabulo_aurora";
        private const string DISPLAY_NAME = "Estábulo";
        private const int CITY_ID = 0;
        private const int REQUIRED_LOT_SIDE = 20;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 100),
            new ReinoResourceCost(ReinoResourceType.Cloth, 30),
            new ReinoResourceCost(ReinoResourceType.Iron, 20),
            new ReinoResourceCost(ReinoResourceType.Wood, 80)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 20),
            new ReinoResourceCost(ReinoResourceType.Cloth, 10),
            new ReinoResourceCost(ReinoResourceType.Iron, 10),
            new ReinoResourceCost(ReinoResourceType.Wood, 30)
        };

        private static readonly int[] STAGE_MULTI_IDS = new int[] { 0xA3, 0xA4, 0xA5, 0xA6 };
        private static readonly TimeSpan[] STAGE_DURATIONS = new TimeSpan[]
        {
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0),
            TimeSpan.FromSeconds(20.0)
        };

        public static readonly Point3D[] StablePostOffsets = new Point3D[]
        {
            new Point3D(3, 3, 22), new Point3D(5, 3, 22), new Point3D(7, 3, 22), new Point3D(9, 3, 22), new Point3D(11, 3, 22),
            new Point3D(3, 6, 22), new Point3D(5, 6, 22), new Point3D(7, 6, 22), new Point3D(9, 6, 22), new Point3D(11, 6, 22)
        };

        // Ajuste esses offsets conforme o layout visual final do seu multi.
        // Cada índice representa 1 dos 3 quartinhos de cruzamento.
        public static readonly Point3D[] BreedingRoomFemaleOffsets = new Point3D[]
        {
            new Point3D(14, 4, 0),
            new Point3D(14, 9, 0),
            new Point3D(14, 14, 0)
        };

        public static readonly Point3D[] BreedingRoomMaleOffsets = new Point3D[]
        {
            new Point3D(16, 4, 0),
            new Point3D(16, 9, 0),
            new Point3D(16, 14, 0)
        };

        public static readonly Point3D[] BreedingRoomOffspringOffsets = new Point3D[]
        {
            new Point3D(15, 4, 0),
            new Point3D(15, 9, 0),
            new Point3D(15, 14, 0)
        };

        public static readonly Point3D[] BreedingRoomReleaseOffsets = new Point3D[]
        {
            new Point3D(13, 4, 0),
            new Point3D(13, 9, 0),
            new Point3D(13, 14, 0)
        };

        public static ReinoConstructionDefinition Create()
        {
            ReinoConstructionDefinition def = new ReinoConstructionDefinition();
            def.Id = BUILDING_ID;
            def.Name = DISPLAY_NAME;
            def.RequiredCityId = CITY_ID;
            def.TargetType = ReinoBuildTargetType.Lot;
            def.MinimumLotSide = REQUIRED_LOT_SIDE;
            def.BuildCosts = BUILD_COSTS;
            def.MaintenanceCosts = MAINTENANCE_COSTS;
            def.StageMultiIds = STAGE_MULTI_IDS;
            def.StageDurations = STAGE_DURATIONS;
            def.FinishedMultiId = 0xA7;
            def.AbandonedMultiId = 0xA8;
            def.FinishedPlacedTypeName = typeof(Server.Custom.Systems.Reinos.Expansion.Multis.ReinoEstabuloMulti).FullName;
            def.NpcTypeName = typeof(OSUStableMaster).FullName;
            def.NpcOffset = new Point3D(2, 10, 0);
            def.NpcZOffset = 0;
            def.NpcWeeklySalaryGold = 100;
            def.AllowedLotSides = new int[] { REQUIRED_LOT_SIDE };
            def.ReactivateDuration = TimeSpan.FromMinutes(3.0);
            def.Permanent = false;
            def.MaintenancePriority = 2;
            def.AllowManualActivationToggle = true;
            def.AllowPriorityChange = true;
            def.DescriptionHtml = BuildHtml();
            return def;
        }

        private static string BuildHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<BIG><B>Estábulo</B></BIG><BR><BR>");
            sb.Append("Permite treinar, cruzar, castrar e marcar animais.<BR>");
            sb.Append("Suporta até 10 postes de amarração internos.<BR><BR>");
            sb.Append("<B>Construção:</B> 100 moedas, 30 tecidos, 20 ferros, 80 madeiras.<BR>");
            sb.Append("<B>Manutenção:</B> 20 moedas, 10 tecidos, 10 ferros, 30 madeiras.<BR>");
            sb.Append("<B>Salário do NPC:</B> 100 moedas por semana.<BR>");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }
    }
}
