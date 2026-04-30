using System;
using System.Text;
using Server;
using Server.Custom.Systems.Arena;
using Server.Custom.Systems.Arena.Mobiles;
using Server.Custom.Systems.Reinos.Expansion.Multis;

namespace Server.Custom.Reinos
{
    public static class ArenaAuroraDefinition
    {
        private const int CITY_ID = 0;
        public const string BUILDING_ID = "arena_aurora";
        private const string DISPLAY_NAME = "Arena";
        private const int REQUIRED_LOT_SIDE = 30;

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
            new ReinoResourceCost(ReinoResourceType.Gold, 2000),
            new ReinoResourceCost(ReinoResourceType.Iron, 400),
            new ReinoResourceCost(ReinoResourceType.Wood, 300),
            new ReinoResourceCost(ReinoResourceType.Cloth, 200)
        };

        private static readonly ReinoResourceCost[] MAINTENANCE_COSTS = new ReinoResourceCost[]
        {
            new ReinoResourceCost(ReinoResourceType.Gold, 400),
            new ReinoResourceCost(ReinoResourceType.Iron, 100),
            new ReinoResourceCost(ReinoResourceType.Wood, 50),
            new ReinoResourceCost(ReinoResourceType.Cloth, 20)
        };

        private static readonly ReinoNpcSpawnDefinition[] NPC_SPAWNS = new ReinoNpcSpawnDefinition[]
        {
            new ReinoNpcSpawnDefinition(typeof(ArenaBilheteriaNpc).FullName, 14, 29, 0),
            new ReinoNpcSpawnDefinition(typeof(ArenaPorteiroNpc).FullName, 15, 29, 0)
        };

        public static readonly ArenaDefinition ArenaConfig = new ArenaDefinition
        {
            ConstructionId = BUILDING_ID,
            ControlOffset = new Point3D(15, 14, 0),
            BilheteriaOffset = new Point3D(14, 29, 0),
            PorteiroOffset = new Point3D(15, 29, 0),
            EntradaOffset = new Point3D(15, 28, 0),
            PublicoTeleportOffset = new Point3D(15, 26, 0),
            EjectOffset = new Point3D(0, 31, 0),
            CenterMultiOffset = new Point3D(8, 8, 0),
            BombermanStorageOffset = new Point3D(15, 16, 0),
            Doors = new ArenaDoorOffset[]
            {
                new ArenaDoorOffset(15, 27, 0, true),
                new ArenaDoorOffset(15, 25, 0, false)
            },
            LutaLivreMultiId = 0x0,
            BoxeMultiId = 0x0,
            LutaMagicaMultiId = 0x0,
            JustaMultiId = 0x0,
            GladiadoresMultiId = 0x0,
            BombermanMultiId = 0x0,
            JoustKnight1Offset = new Point3D(11, 22, 0),
            JoustDirectionForward = Direction.East,
            JoustHitMinDx = -1,
            JoustHitMaxDx = 0,
            JoustHitDy = 1,
            BombermanGridStartX = 2,
            BombermanGridStartY = 2,
            BombermanGridWidth = 20,
            BombermanGridHeight = 20
        };

        public static ReinoConstructionDefinition Create()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Arena</B><BR><BR>");
            sb.Append("<B>Tamanho do lote:</B> 30x30.<BR>");
            sb.Append("<B>Construção:</B> 2000 moedas, 400 ferro, 300 madeira e 200 tecido.<BR>");
            sb.Append("<B>Manutenção semanal:</B> 400 moedas, 100 ferro, 50 madeira e 20 tecido.<BR>");
            sb.Append("<B>NPCs:</B> Bilheteria + Porteiro (50 moedas/semana cada).<BR><BR>");
            sb.Append("Bilheteria vende ingresso por 100 moedas em qualquer momento. Porteiro só libera entrada quando o evento estiver iniciado.");
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
            def.FinishedPlacedTypeName = typeof(ReinoArenaMulti).FullName;
            def.AbandonedMultiId = ABANDONED_MULTI_ID;
            def.NpcSpawns = NPC_SPAWNS;
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
