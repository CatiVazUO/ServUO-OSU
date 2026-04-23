
using System;
using System.Text;
using Server;
using Server.Custom.Systems.Espetaculos;
using Server.Custom.Systems.Espetaculos.Mobiles;
using Server.Custom.Systems.Reinos.Expansion.Multis;

namespace Server.Custom.Reinos
{
    public static class TeatroAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "teatro_aurora";
        private const string DISPLAY_NAME = "Teatro";
        private const int REQUIRED_LOT_SIDE = 15;

        // =====================================================
        // MULTIS PLACEHOLDER
        // Troque depois para os multis finais do seu teatro.
        // =====================================================
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
            new ReinoResourceCost(ReinoResourceType.Gold, 200),
            new ReinoResourceCost(ReinoResourceType.Iron, 40),
            new ReinoResourceCost(ReinoResourceType.Cloth, 60),
            new ReinoResourceCost(ReinoResourceType.Wood, 60)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 80),
            new ReinoResourceCost(ReinoResourceType.Iron, 10),
            new ReinoResourceCost(ReinoResourceType.Cloth, 20),
            new ReinoResourceCost(ReinoResourceType.Wood, 10)
        };

        // =====================================================
        // EDITE AQUI AS POSIÇÕES DO TEATRO
        // Todas as coordenadas são offsets do canto noroeste do lote.
        // =====================================================
        private static readonly Point3D CONTROL_ITEM_OFFSET = new Point3D(7, 7, 5);
        private static readonly Point3D ENTRY_TELEPORT_OFFSET = new Point3D(7, 10, 5);

        private static readonly ReinoNpcSpawnDefinition[] NPC_SPAWNS = new ReinoNpcSpawnDefinition[]
        {
            new ReinoNpcSpawnDefinition(typeof(EspetaculoBilheteriaNpc).FullName, 6, 13, 5),
            new ReinoNpcSpawnDefinition(typeof(EspetaculoPortaNpc).FullName, 7, 14, 5)
        };

        // Portas físicas opcionais.
        // Se o seu multi final tiver portas próprias controladas por este sistema,
        // ajuste os offsets abaixo. Se deixar vazio, o controle vai ocorrer só
        // pelo NPC da porta + expulsão de intrusos durante o espetáculo.
        private static readonly EspetaculoDoorDefinition[] DOORS = new EspetaculoDoorDefinition[]
        {
        };

        private const bool CURTAINS_FACE_EAST = false;

        // INFORME SÓ A CORTINA INTERNA ESQUERDA.
        // O sistema calcula:
        // - a interna direita
        // - a externa esquerda
        // - a externa direita
        private static readonly Point3D CURTAIN_LEFT_INNER_OFFSET = new Point3D(7, 4, 5);

        private static readonly EspetaculoSetPieceDefinition[] SET_PIECES = CreateCurtainDefinitions();

        private static EspetaculoSetPieceDefinition[] CreateCurtainDefinitions()
        {
            int itemId = CURTAINS_FACE_EAST ? 0x12DB : 0x12EA;
            int hue = 33;

            Point3D rightInnerStep;
            Point3D leftOpenStep;
            Point3D rightOpenStep;

            if (CURTAINS_FACE_EAST)
            {
                // fechadas: direita fica 1 tile em -Y da esquerda
                rightInnerStep = new Point3D(0, -1, 0);

                // ao abrir:
                // esquerda vai para +Y
                // direita vai para -Y
                leftOpenStep = new Point3D(0, +1, 0);
                rightOpenStep = new Point3D(0, -1, 0);
            }
            else
            {
                // fechadas: direita fica 1 tile em +X da esquerda
                rightInnerStep = new Point3D(+1, 0, 0);

                // ao abrir:
                // esquerda vai para -X
                // direita vai para +X
                leftOpenStep = new Point3D(-1, 0, 0);
                rightOpenStep = new Point3D(+1, 0, 0);
            }

            Point3D leftInnerClosed = CURTAIN_LEFT_INNER_OFFSET;
            Point3D rightInnerClosed = AddOffset(leftInnerClosed, rightInnerStep, 1);

            // externas nascem ao lado das internas, fechando 4 tiles em linha
            Point3D leftOuterClosed = AddOffset(leftInnerClosed, leftOpenStep, 1);
            Point3D rightOuterClosed = AddOffset(rightInnerClosed, rightOpenStep, 1);

            // abertas:
            // internas andam 2
            // externas andam 1
            // e ficam juntas nas pontas
            Point3D leftOpen = AddOffset(leftInnerClosed, leftOpenStep, 2);
            Point3D rightOpen = AddOffset(rightInnerClosed, rightOpenStep, 2);

            return new EspetaculoSetPieceDefinition[]
            {
        new EspetaculoSetPieceDefinition(
            "curtain_left_outer",
            "Abrir Cortinas",
            "Fechar Cortinas",
            leftOuterClosed,
            leftOpen,
            itemId,
            hue,
            "cortina esquerda"),

        new EspetaculoSetPieceDefinition(
            "curtain_left_inner",
            "Abrir Cortinas",
            "Fechar Cortinas",
            leftInnerClosed,
            leftOpen,
            itemId,
            hue,
            "cortina esquerda"),

        new EspetaculoSetPieceDefinition(
            "curtain_right_inner",
            "Abrir Cortinas",
            "Fechar Cortinas",
            rightInnerClosed,
            rightOpen,
            itemId,
            hue,
            "cortina direita"),

        new EspetaculoSetPieceDefinition(
            "curtain_right_outer",
            "Abrir Cortinas",
            "Fechar Cortinas",
            rightOuterClosed,
            rightOpen,
            itemId,
            hue,
            "cortina direita")
            };
        }

        private static Point3D AddOffset(Point3D origin, Point3D delta, int times)
        {
            return new Point3D(
                origin.X + (delta.X * times),
                origin.Y + (delta.Y * times),
                origin.Z + (delta.Z * times));
        }

        // Até 3 luzes de palco, como você pediu.
        private static readonly EspetaculoStageLightDefinition[] STAGE_LIGHTS = new EspetaculoStageLightDefinition[]
        {
            new EspetaculoStageLightDefinition(4, 4, 40, 0x0A15, 0),
            new EspetaculoStageLightDefinition(7, 4, 40, 0x0A15, 0),
            new EspetaculoStageLightDefinition(10, 4, 40, 0x0A15, 0)
        };

        private static readonly EspetaculoSlotDefinition[] SLOTS = new EspetaculoSlotDefinition[]
        {
            new EspetaculoSlotDefinition(DayOfWeek.Friday, 20, 0, "- 20h"),
            new EspetaculoSlotDefinition(DayOfWeek.Saturday, 12, 0, "- 12h"),
            new EspetaculoSlotDefinition(DayOfWeek.Saturday, 19, 0, "- 19h")
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
            def.VenueType = EspetaculoVenueType.Theater;
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
                "Escolha um dos três horários fixos do <B>teatro</B> e depois escolha a duração do aluguel.<BR><BR>" +
                "<B>Durações:</B><BR>" +
                "1 hora = 100 moedas.<BR>" +
                "1 hora e 30 minutos = 150 moedas.<BR>" +
                "2 horas = 200 moedas.<BR><BR>" +
                "O valor do aluguel vai para o reino. Durante o espetáculo, a bilheteria vende ingressos de 10 moedas e esse valor vai para quem alugou o teatro.<BR><BR>" +
                "Dez minutos antes do início, todos os jogadores do servidor recebem um convite oficial com o selo do reino.</BASEFONT>";
            return def;
        }

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Teatro</B><BR><BR>");
            sb.Append("<B>Tamanho do lote:</B> 15x15.<BR>");
            sb.Append("<B>Construção:</B> 200 moedas, 40 ferro, 60 tecido e 60 madeira.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 80 moedas, 10 ferro, 20 tecidos e 10 madeiras.<BR><BR>");
            sb.Append("Possui bilheteria, NPC de porta, aluguel por horário fixo, ingresso, convite global com selo do reino, controle de luzes e abertura/fechamento de cortinas.");
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
            def.FinishedPlacedTypeName = typeof(ReinoTeatroMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcSpawns = NPC_SPAWNS;
            def.UseMultiDoors = false; // portas do teatro ficam sob controle do sistema de espetáculos
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
