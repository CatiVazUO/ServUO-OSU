using Server;
using Server.Custom.Systems.Rent;
using Server.Items;
using Server.Multis;
using System;
using System.Text;
using System.Xml.Linq;

namespace Server.Custom.Systems.Reinos
{
    public static class ResidencialAuroraTesteDefinition
    {
        private const int CITY_ID = 0;
        private const string BUILDING_ID = "residencial_aurora_teste";
        private const string DISPLAY_NAME = "Residencial Aurora";
        private const int REQUIRED_LOT_SIDE = 15;

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Wood, 80),
            new ReinoResourceCost(ReinoResourceType.Iron, 80),
            new ReinoResourceCost(ReinoResourceType.Cloth, 80)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
{
            new ReinoResourceCost(ReinoResourceType.Wood, 30),
            new ReinoResourceCost(ReinoResourceType.Iron, 30),
            new ReinoResourceCost(ReinoResourceType.Cloth, 30)
};

        private static readonly int[] STAGE_MULTI_IDS = new int[]
        {
            0x68,
            0x6A,
            0x64
        };

        private static readonly TimeSpan[] STAGE_DURATIONS = new TimeSpan[]
        {
            TimeSpan.FromMinutes(1.0),
            TimeSpan.FromMinutes(1.0),
            TimeSpan.FromMinutes(1.0)
        };


        private const int FINISHED_MULTI_ID = 0xA3;
        private const int ABANDONED_MULTI_ID = 0x98;  // small tower

        private static readonly ReinoRentalTemplate[] RENTALS = new ReinoRentalTemplate[]
        {
            new ReinoRentalTemplate
            {
                TemplateId = "casa01",
                DisplayName = "Aurora Residential Casa 1",
                PropertyType = OSUPropertyType.House,
                GroupTag = "Residential",
                SignOffset = new Point3D(9, 7, 7),
                BanLocOffset = new Point3D(9, 7, 7),
                BlockOffsets = new ReinoRentalRectOffset[]
                {
                    new ReinoRentalRectOffset(1, 1, 11, 12),
                },
                DoorTemplates = new ReinoRentalDoorTemplate[]
                {
                    new ReinoRentalDoorTemplate(8, 4, 7, 1775, 1776, 234, 241, new Point3D(1, -1, 0)),
                    new ReinoRentalDoorTemplate(8, 5, 7, 1773, 1774, 234, 241, new Point3D(1, 1, 0)),
                },
                MinZOffset = 7,
                MaxZOffset = 26,
                Lockdowns = 200,
                Secures = 5,
                DefaultPrice = 50,
                DefaultRentByTime = TimeSpan.FromDays(7.0),
                DefaultAllowedCulturesCsv = "Todos",
                GovernorManaged = true,
                StartConfigured = false
            },
            new ReinoRentalTemplate
            {
                TemplateId = "casa01",
                DisplayName = "Aurora Residential Casa 2",
                PropertyType = OSUPropertyType.House,
                GroupTag = "Residential",
                SignOffset = new Point3D(8, 5, 27),
                BanLocOffset = new Point3D(8, 5, 27),
                BlockOffsets = new ReinoRentalRectOffset[]
                {
                    new ReinoRentalRectOffset(1, 1, 10, 11),
                },
                DoorTemplates = new ReinoRentalDoorTemplate[]
                {
                    new ReinoRentalDoorTemplate(6, 6, 27, 1775, 1776, 234, 241, new Point3D(1, -1, 0)),
                },
                MinZOffset = 27,
                MaxZOffset = 66,
                Lockdowns = 300,
                Secures = 8,
                DefaultPrice = 80,
                DefaultRentByTime = TimeSpan.FromDays(7.0),
                DefaultAllowedCulturesCsv = "Todos",
                GovernorManaged = true,
                StartConfigured = false,
                Flip = true

            }
        };

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Residencial Aurora</B><BR><BR>");
            sb.Append("<B>AVISO: ESSA ÁREA NÃO PODE SER DESATIVADA NEM DESCONSTRUIDA, ELA É PERMANENTE"
            sb.Append("<B>Construção:</B> 200 madeira, 120 ferro e 80 tecido.<BR>");
            sb.Append("<B>Manutenção:</B> nesta primeira versão de teste, a área residencial está marcada como permanente para você focar no aluguel e na configuração das casas sem risco de abandono.<BR><BR>");
            sb.Append("Quando ficar pronta, esta construção cria automaticamente 2 placas residenciais de teste já com área, altura, lockdowns, secures e local da placa definidos. O governador precisa apenas liberar cada imóvel, ajustar nome, preço, frequência e povos permitidos.<BR><BR>");
            sb.Append("Os offsets desta versão são placeholders. Quando você tiver o multi residencial definitivo, use o comando de exportação para medir as placas reais e substituir esses números com precisão.");
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
            def.FinishedPlacedTypeName = typeof(ResidencialAuroraTestePlacedMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcTypeName = String.Empty;
            def.UseMultiDoors = false;
            def.FinishedDoors = new ReinoDoorDefinition[]
                {
                    new ReinoDoorDefinition(-1, 3, 7, DoorFacing.WestCW, true),
                    new ReinoDoorDefinition(0, 3, 7, DoorFacing.EastCCW, true)
                };
            def.ReactivateDuration = TimeSpan.FromMinutes(3.0);
            def.Permanent = false;
            def.MaintenancePriority = 1;
            def.AllowManualActivationToggle = false;
            def.AllowPriorityChange = false;
            def.NpcWeeklySalaryGold = 0;
            def.RentalTemplates = RENTALS;
            return def;
        }
    }
}
