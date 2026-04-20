using System;
using System.Text;
using Server;

namespace Server.Custom.Reinos
{
    public static class HospitalAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "hospital_aurora";
        private const string DISPLAY_NAME = "Hospital";
        private const int REQUIRED_LOT_SIDE = 20;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 200),
            new ReinoResourceCost(ReinoResourceType.Iron, 50),
            new ReinoResourceCost(ReinoResourceType.Cloth, 50),
            new ReinoResourceCost(ReinoResourceType.Wood, 50)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 100),
            new ReinoResourceCost(ReinoResourceType.Iron, 20),
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

        private static readonly Point3D[] MEDICATION_TUB_OFFSETS = new Point3D[]
        {
            new Point3D(3, 3, 6),
            new Point3D(4, 3, 6),
            new Point3D(5, 3, 6)
        };

        private static readonly Point3D[] HOSPITAL_STRETCHER_OFFSETS = new Point3D[]
        {
            new Point3D(2, 6, 6), new Point3D(4, 6, 6), new Point3D(6, 6, 6),
            new Point3D(8, 6, 6), new Point3D(10, 6, 6), new Point3D(12, 6, 6)
        };

        private static readonly Point3D[] SURGERY_STRETCHER_OFFSETS = new Point3D[]
        {
            new Point3D(11, 10, 6)
        };

        private static readonly Point3D[] SURGERY_TABLE_ORIGINS = new Point3D[]
        {
            new Point3D(9, 9, 6)
        };

        public static Point3D[] GetMedicationTubOffsets() { return (Point3D[])MEDICATION_TUB_OFFSETS.Clone(); }
        public static Point3D[] GetHospitalStretcherOffsets() { return (Point3D[])HOSPITAL_STRETCHER_OFFSETS.Clone(); }
        public static Point3D[] GetSurgeryStretcherOffsets() { return (Point3D[])SURGERY_STRETCHER_OFFSETS.Clone(); }
        public static Point3D[] GetSurgeryTableOrigins() { return (Point3D[])SURGERY_TABLE_ORIGINS.Clone(); }

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Hospital</B><BR><BR>");
            sb.Append("<B>Tamanho do lote:</B> 15x15.<BR>");
            sb.Append("<B>Construção:</B> 200 moedas, 50 ferro, 50 tecido e 50 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 100 moedas, 20 ferro, 20 tecido e 20 madeira.<BR>");
            sb.Append("<B>Extra por cirurgia na semana:</B> +50 moedas e +10 tecidos por cirurgia realizada.<BR><BR>");
            sb.Append("O hospital spawna 3 cubas medicinais, 6 macas de recuperação, 1 maca cirúrgica e 1 mesa cirúrgica com instrumentos expostos.");
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
            def.FinishedPlacedTypeName = typeof(ReinoHospitalMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.UseMultiDoors = true;
            def.ReactivateDuration = TimeSpan.FromMinutes(3.0);
            def.Permanent = false;
            def.MaintenancePriority = 2;
            def.AllowManualActivationToggle = true;
            def.AllowPriorityChange = true;
            return def;
        }
    }
}
