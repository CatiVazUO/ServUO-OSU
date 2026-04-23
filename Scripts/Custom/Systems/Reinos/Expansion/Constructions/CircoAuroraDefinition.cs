
using System;
using System.Text;
using Server;
using Server.Custom.Systems.Espetaculos;
using Server.Custom.Systems.Espetaculos.Mobiles;
using Server.Custom.Systems.Reinos.Expansion.Multis;

namespace Server.Custom.Reinos
{
    public static class CircoAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "circo_aurora";
        private const string DISPLAY_NAME = "Circo";
        private const int REQUIRED_LOT_SIDE = 20;

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

        private static readonly ReinoResourceCost[] BUILD_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 100),
            new ReinoResourceCost(ReinoResourceType.Iron, 20),
            new ReinoResourceCost(ReinoResourceType.Cloth, 60),
            new ReinoResourceCost(ReinoResourceType.Wood, 20)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Iron, 10),
            new ReinoResourceCost(ReinoResourceType.Wood, 10),
            new ReinoResourceCost(ReinoResourceType.Cloth, 40)
        };

        // =====================================================
        // EDITE AQUI AS POSIÇÕES DO CIRCO
        // =====================================================
        private static readonly Point3D CONTROL_ITEM_OFFSET = new Point3D(10, 9, 5);
        private static readonly Point3D ENTRY_TELEPORT_OFFSET = new Point3D(10, 13, 5);

        private static readonly ReinoNpcSpawnDefinition[] NPC_SPAWNS = new ReinoNpcSpawnDefinition[]
        {
            new ReinoNpcSpawnDefinition(typeof(EspetaculoBilheteriaNpc).FullName, 9, 18, 5),
            new ReinoNpcSpawnDefinition(typeof(EspetaculoPortaNpc).FullName, 10, 19, 5)
        };

        private static readonly EspetaculoDoorDefinition[] DOORS = new EspetaculoDoorDefinition[]
        {
        };

        // Jaula 1 e Jaula 2
        private static readonly EspetaculoSetPieceDefinition[] SET_PIECES = new EspetaculoSetPieceDefinition[]
        {
            new EspetaculoSetPieceDefinition(
                "cage_1",
                "Abrir Jaula 1",
                "Fechar Jaula 1",
                new Point3D(6, 8, 5),
                new Point3D(3, 8, 5),
                0x846,
                0,
                "jaula 1"),
            new EspetaculoSetPieceDefinition(
                "cage_2",
                "Abrir Jaula 2",
                "Fechar Jaula 2",
                new Point3D(13, 8, 5),
                new Point3D(16, 8, 5),
                0x846,
                0,
                "jaula 2")
        };

        // Até 4 luzes de palco, como você pediu.
        private static readonly EspetaculoStageLightDefinition[] STAGE_LIGHTS = new EspetaculoStageLightDefinition[]
        {
            new EspetaculoStageLightDefinition(5, 5, 40, 0x0A15, 0),
            new EspetaculoStageLightDefinition(8, 5, 40, 0x0A15, 0),
            new EspetaculoStageLightDefinition(11, 5, 40, 0x0A15, 0),
            new EspetaculoStageLightDefinition(14, 5, 40, 0x0A15, 0)
        };

        private static readonly EspetaculoSlotDefinition[] SLOTS = new EspetaculoSlotDefinition[]
        {
            new EspetaculoSlotDefinition(DayOfWeek.Friday, 21, 0, "- 21h"),
            new EspetaculoSlotDefinition(DayOfWeek.Saturday, 16, 0, "- 16h"),
            new EspetaculoSlotDefinition(DayOfWeek.Sunday, 12, 0, "- 12h")
        };

        private static readonly EspetaculoDurationDefinition[] DURATIONS = new EspetaculoDurationDefinition[]
        {
            new EspetaculoDurationDefinition(TimeSpan.FromHours(1.0), 100, "1:00 Hora"),
            new EspetaculoDurationDefinition(TimeSpan.FromMinutes(90.0), 150, "1:30 Horas"),
            new EspetaculoDurationDefinition(TimeSpan.FromHours(2.0), 200, "2:00 Horas")
        };

        public static EspetaculoVenueDefinition CreateVenue()
        {
            EspetaculoVenueDefinition def = new EspetaculoVenueDefinition();
            def.ConstructionId = BUILDING_ID;
            def.DisplayName = DISPLAY_NAME;
            def.VenueType = EspetaculoVenueType.Circus;
            def.ControlItemOffset = CONTROL_ITEM_OFFSET;
            def.EntryTeleportOffset = ENTRY_TELEPORT_OFFSET;
            def.Slots = SLOTS;
            def.Durations = DURATIONS;
            def.StageLights = STAGE_LIGHTS;
            def.SetPieces = SET_PIECES;
            def.Doors = DOORS;
            def.TicketPriceGold = 10;
            def.TicketItemId = 0xE17;
            def.TicketSellLeadMinutes = 15;
            def.ReservationHtml =
                "<BASEFONT COLOR=#000000>" +
                "Escolha um dos horários fixos do <B>circo</B> e depois a duração do aluguel.<BR><BR>" +
                "<B>Durações:</B><BR>" +
                "1 hora = 100 moedas.<BR>" +
                "1 hora e 30 minutos = 150 moedas.<BR>" +
                "2 horas = 200 moedas.<BR><BR>" +
                "O valor do aluguel vai para o reino. Durante o espetáculo, a bilheteria vende ingressos de 10 moedas e esse valor vai para quem alugou o circo.<BR><BR>" +
                "O item de controle permite mexer nas luzes e nas duas jaulas.</BASEFONT>";
            return def;
        }

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Circo</B><BR><BR>");
            sb.Append("<B>Tamanho do lote:</B> 20x20.<BR>");
            sb.Append("<B>Construção:</B> 100 moedas, 20 ferro, 60 tecido e 20 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 10 ferro, 10 madeiras e 40 tecidos.<BR><BR>");
            sb.Append("Possui bilheteria, NPC de porta, aluguel por horário fixo, ingresso, convite global com selo do reino, controle de luzes e abertura/fechamento de duas jaulas.");
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
            def.FinishedPlacedTypeName = typeof(ReinoCircoMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcSpawns = NPC_SPAWNS;
            def.UseMultiDoors = false;
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
