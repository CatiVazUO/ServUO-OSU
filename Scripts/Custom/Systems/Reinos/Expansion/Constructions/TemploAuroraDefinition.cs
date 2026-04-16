using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class TemploAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "templo_aurora";
        private const string DISPLAY_NAME = "Templo";
        private const int REQUIRED_LOT_SIDE = 20;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 500),
            new ReinoResourceCost(ReinoResourceType.Iron, 200),
            new ReinoResourceCost(ReinoResourceType.Cloth, 100),
            new ReinoResourceCost(ReinoResourceType.Wood, 100)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 200),
            new ReinoResourceCost(ReinoResourceType.Iron, 10),
            new ReinoResourceCost(ReinoResourceType.Cloth, 20),
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

        private const int FINISHED_MULTI_ID = 0x147B;
        private const int ABANDONED_MULTI_ID = 0xA8;

         
        // =========================
        // EDITE AQUI AS COORDENADAS
        // =========================
        private static readonly Point3D ALTAR_OFFSET = new Point3D(9, 7, 5);
        private static readonly Point3D DONATION_CHEST_OFFSET = new Point3D(2, 2, 5);
        private static readonly Point3D STATUE_OFFSET = new Point3D(9, 5, 9);

        private static readonly Point3D[] RITE_ITEM_OFFSETS = new Point3D[]
        {
            new Point3D(8, 7, 6),
            new Point3D(9, 7, 6),
            new Point3D(10, 7, 6)
        };

        private static readonly Point3D[] CEREMONY_DECOR_OFFSETS = new Point3D[]
        {
            new Point3D(5, 4, 6),
            new Point3D(6, 4, 6),
            new Point3D(7, 4, 6),
            new Point3D(8, 4, 6),
            new Point3D(9, 4, 6),
            new Point3D(10, 4, 6),
            new Point3D(11, 4, 6),
            new Point3D(12, 4, 6),
            new Point3D(13, 4, 6),
            new Point3D(14, 4, 6)
        };

        private static readonly Point3D FUNERAL_COFFIN_OFFSET = new Point3D(9, 9, 6);

        public static Point3D GetAltarOffset() { return ALTAR_OFFSET; }
        public static Point3D GetDonationChestOffset() { return DONATION_CHEST_OFFSET; }
        public static Point3D GetStatueOffset() { return STATUE_OFFSET; }
        public static Point3D[] GetRiteOffsets() { return (Point3D[])RITE_ITEM_OFFSETS.Clone(); }
        public static Point3D[] GetMarriageOffsets() { return (Point3D[])CEREMONY_DECOR_OFFSETS.Clone(); }
        public static Point3D[] GetFuneralCandleOffsets() { return (Point3D[])CEREMONY_DECOR_OFFSETS.Clone(); }
        public static Point3D GetFuneralCoffinOffset() { return FUNERAL_COFFIN_OFFSET; }

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Templo</B><BR><BR>");
            sb.Append("<B>Tamanho do lote:</B> 20x20.<BR>");
            sb.Append("<B>Construção:</B> 500 moedas, 200 ferro, 100 tecidos e 100 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 200 moedas, 10 ferro, 20 tecidos e 20 madeira.<BR><BR>");
            sb.Append("Este arquivo controla as posições do altar, do baú de doações, da estátua do deus, dos 3 itens do rito, das 10 posições do casamento/funeral e do caixão.<BR><BR>");
            sb.Append("O multi final está como 0x147B, como você pediu. Como ele ainda é só a fundação, o altar, o baú e a estátua são criados separadamente pelo item interno do templo.");
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
            def.FinishedPlacedTypeName = typeof(ReinoTemploMulti).FullName;
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
