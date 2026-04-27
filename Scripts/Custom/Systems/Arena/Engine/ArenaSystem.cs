using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Reinos;
using Server.Custom.Systems.Arena.Items;

namespace Server.Custom.Systems.Arena
{
    public static class ArenaSystem
    {
        public const int TicketPrice = 100;

        private static readonly Dictionary<string, ArenaState> m_States = new Dictionary<string, ArenaState>(StringComparer.OrdinalIgnoreCase);

        public static ArenaDefinition AuroraDefinition = new ArenaDefinition
        {
            ConstructionId = ArenaAuroraDefinition.BUILDING_ID,
            ControlOffset = new Point3D(15, 14, 0),
            BilheteriaOffset = new Point3D(14, 29, 0),
            PorteiroOffset = new Point3D(15, 29, 0),
            EntradaOffset = new Point3D(15, 28, 0),
            PublicoTeleportOffset = new Point3D(15, 26, 0),
            EjectOffset = new Point3D(0, 31, 0),
            CenterMultiOffset = new Point3D(8, 8, 0),
            LutaLivreMultiId = 0x0,
            BoxeMultiId = 0x0,
            LutaMagicaMultiId = 0x0,
            JustaMultiId = 0x0,
            GladiadoresMultiId = 0x0,
            BombermanMultiId = 0x0,
            JoustHitMinDx = -1,
            JoustHitMaxDx = 0,
            JoustHitDy = 1,
            BombermanGridStartX = 2,
            BombermanGridStartY = 2,
            BombermanGridWidth = 27,
            BombermanGridHeight = 27
        };

        public static ArenaDefinition GetDefinitionByConstructionId(string constructionId)
        {
            if (String.Equals(constructionId, ArenaAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase))
                return AuroraDefinition;

            return null;
        }


        public static ReinoLotDefinition GetLotFromConstructionKey(string constructionKey)
        {
            if (String.IsNullOrWhiteSpace(constructionKey) || !constructionKey.StartsWith("L:", StringComparison.OrdinalIgnoreCase))
                return null;

            int lotId;
            if (!Int32.TryParse(constructionKey.Substring(2), out lotId))
                return null;

            return ReinoExpansionSystem.GetLotDefinition(lotId);
        }


        public static Point3D[] GetGladiatorSpawnPoints(ReinoLotDefinition lot)
        {
            return new Point3D[]
            {
                new Point3D(lot.NorthWest.X + 15, lot.NorthWest.Y + 1, lot.NorthWest.Z),
                new Point3D(lot.NorthWest.X + 28, lot.NorthWest.Y + 15, lot.NorthWest.Z),
                new Point3D(lot.NorthWest.X + 15, lot.NorthWest.Y + 28, lot.NorthWest.Z),
                new Point3D(lot.NorthWest.X + 1, lot.NorthWest.Y + 15, lot.NorthWest.Z)
            };
        }

        public static void ApplyJoustPlacement(ReinoLotDefinition lot, bool flip, PlayerMobile a, PlayerMobile b)
        {
            if (lot == null || a == null || b == null)
                return;

            Point3D aLoc = flip ? new Point3D(lot.NorthWest.X + 24, lot.NorthWest.Y + 14, lot.NorthWest.Z) : new Point3D(lot.NorthWest.X + 5, lot.NorthWest.Y + 14, lot.NorthWest.Z);
            Point3D bLoc = flip ? new Point3D(lot.NorthWest.X + 24, lot.NorthWest.Y + 15, lot.NorthWest.Z) : new Point3D(lot.NorthWest.X + 5, lot.NorthWest.Y + 15, lot.NorthWest.Z);

            a.MoveToWorld(aLoc, lot.Map);
            b.MoveToWorld(bLoc, lot.Map);

            a.Direction = flip ? Direction.West : Direction.East;
            b.Direction = flip ? Direction.West : Direction.East;

            a.CantWalk = true;
            b.CantWalk = true;

            Timer.DelayCall(TimeSpan.FromSeconds(0.2), delegate { ForceRun(a, flip ? Direction.West : Direction.East, 12); });
            Timer.DelayCall(TimeSpan.FromSeconds(0.2), delegate { ForceRun(b, flip ? Direction.West : Direction.East, 12); });
        }

        private static void ForceRun(PlayerMobile pm, Direction dir, int steps)
        {
            if (pm == null || pm.Deleted)
                return;

            pm.CantWalk = false;
            for (int i = 0; i < steps; i++)
            {
                double pct = Utility.RandomMinMax(90, 100) / 100.0;
                Timer.DelayCall(TimeSpan.FromSeconds(i * (0.12 / pct)), delegate
                {
                    if (pm != null && !pm.Deleted)
                        pm.Move(dir);
                });
            }
        }


        public static ArenaDefinition GetJoustDefinition(string constructionKey)
        {
            ReinoLotDefinition lot = GetLotFromConstructionKey(constructionKey);
            if (lot == null)
                return AuroraDefinition;

            ReinoLotState st = ReinoExpansionSystem.GetLotState(lot.LotId);
            if (st == null)
                return AuroraDefinition;

            ArenaDefinition def = GetDefinitionByConstructionId(st.ConstructionId);
            return def ?? AuroraDefinition;
        }

        public static bool TryResolveArenaAt(Point3D location, Map map, out string constructionKey, out int cityId, out ArenaDefinition def, out ReinoLotDefinition lot)
        {
            constructionKey = String.Empty;
            cityId = -1;
            def = null;
            lot = null;

            if (map == null || map == Map.Internal)
                return false;

            lot = ReinoExpansionSystem.FindLotAt(location, map);
            if (lot == null)
                return false;

            ReinoLotState state = ReinoExpansionSystem.GetLotState(lot.LotId);
            if (state == null || String.IsNullOrWhiteSpace(state.ConstructionId))
                return false;

            def = GetDefinitionByConstructionId(state.ConstructionId);
            if (def == null)
                return false;

            constructionKey = ReinoMaintenanceSystem.BuildLotKey(lot.LotId);
            cityId = lot.CityId;
            return true;
        }

        public static ArenaState EnsureState(string constructionKey, int cityId)
        {
            ArenaState state;
            if (!m_States.TryGetValue(constructionKey, out state) || state == null)
            {
                state = new ArenaState();
                state.ConstructionKey = constructionKey;
                state.CityId = cityId;
                m_States[constructionKey] = state;
            }

            state.CityId = cityId;
            return state;
        }

        public static ArenaState GetState(string constructionKey)
        {
            ArenaState state;
            m_States.TryGetValue(constructionKey, out state);
            return state;
        }

        public static bool CanAccessControl(PlayerMobile pm, int cityId, string constructionKey)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            return role != null && role.IsOccupied && !String.IsNullOrWhiteSpace(role.LinkedConstructionKey)
                && String.Equals(role.LinkedConstructionKey, constructionKey, StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryBuyTicket(PlayerMobile from, int cityId, string constructionKey, out string message)
        {
            message = String.Empty;

            if (from == null || from.Backpack == null)
            {
                message = "Mochila inválida.";
                return false;
            }

            if (!from.Backpack.ConsumeTotal(typeof(Gold), TicketPrice))
            {
                message = "Você precisa de 100 moedas para comprar o ingresso.";
                return false;
            }

            from.Backpack.DropItem(new ArenaTicket(cityId, constructionKey));
            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, TicketPrice);
            ArenaState st = EnsureState(constructionKey, cityId);
            st.TicketSalesGold += TicketPrice;
            message = "Ingresso comprado com sucesso.";
            return true;
        }

        public static bool TryUseGate(PlayerMobile from, string constructionKey, int cityId, Point3D inside, out string message)
        {
            message = String.Empty;

            ArenaState state = GetState(constructionKey);
            if (state == null || !state.EventStarted)
            {
                message = "A arena ainda não abriu para entrada.";
                return false;
            }

            if (from == null || from.Backpack == null)
            {
                message = "Mochila inválida.";
                return false;
            }

            ArenaTicket ticket = from.Backpack.FindItemByType(typeof(ArenaTicket), true) as ArenaTicket;
            if (ticket == null || !String.Equals(ticket.ConstructionKey, constructionKey, StringComparison.OrdinalIgnoreCase))
            {
                message = "Você precisa de um ingresso desta arena.";
                return false;
            }

            ticket.Delete();
            from.MoveToWorld(inside, from.Map);
            message = "Entrada liberada. Bom evento!";
            return true;
        }

        public static void SelectMode(string constructionKey, int cityId, ArenaGameMode mode)
        {
            ArenaState state = EnsureState(constructionKey, cityId);
            state.SelectedMode = mode;
            state.LastChangedUtc = DateTime.UtcNow;
        }

        public static void StartEvent(string constructionKey, int cityId, ReinoLotDefinition lot)
        {
            ArenaState state = EnsureState(constructionKey, cityId);
            state.EventStarted = true;
            state.LastChangedUtc = DateTime.UtcNow;
            state.LastEventRevenueGold = 0;

            if (state.SelectedMode == ArenaGameMode.LutaLivre || state.SelectedMode == ArenaGameMode.Boxe || state.SelectedMode == ArenaGameMode.LutaMagica)
            {
                EnsureCenterMulti(state, lot);
            }
            else if (state.SelectedMode == ArenaGameMode.Justa)
            {
                ArenaGameModes.GetOrCreateJoust(constructionKey).Running = true;
            }
            else if (state.SelectedMode == ArenaGameMode.Gladiadores)
            {
                ArenaGameModes.GetOrCreateGladiator(constructionKey).Play(lot);
            }
            else if (state.SelectedMode == ArenaGameMode.Bomberman)
            {
                ArenaGameModes.GetOrCreateBomberman(constructionKey).Play(lot);
            }
        }

        public static void StopEvent(string constructionKey, int cityId, ReinoLotDefinition lot)
        {
            ArenaState state = EnsureState(constructionKey, cityId);
            state.EventStarted = false;
            state.LastChangedUtc = DateTime.UtcNow;

            DeleteCenterMulti(state);
            ArenaGameModes.StopAll(constructionKey);
            state.LastEventRevenueGold = state.TicketSalesGold;
            state.TicketSalesGold = 0;
            EjectPlayersFromLot(lot);
        }

        private static void EnsureCenterMulti(ArenaState state, ReinoLotDefinition lot)
        {
            ArenaDefinition def = GetDefinitionByConstructionId(ArenaAuroraDefinition.BUILDING_ID);
            if (def == null || lot == null)
                return;

            int multiId = def.GetMultiId(state.SelectedMode);
            if (multiId <= 0)
                return;

            DeleteCenterMulti(state);

            Point3D loc = new Point3D(lot.NorthWest.X + def.CenterMultiOffset.X, lot.NorthWest.Y + def.CenterMultiOffset.Y, lot.NorthWest.Z + def.CenterMultiOffset.Z);
            ReinoConstructionMulti multi = new ReinoConstructionMulti(multiId, lot.LotId, ArenaAuroraDefinition.BUILDING_ID, -1);
            multi.MoveToWorld(loc, lot.Map);
            state.CenterMultiSerial = multi.Serial.Value;
        }

        private static void DeleteCenterMulti(ArenaState state)
        {
            if (state == null || state.CenterMultiSerial <= 0)
                return;

            Item item = World.FindItem((Serial)state.CenterMultiSerial);
            if (item != null && !item.Deleted)
                item.Delete();

            state.CenterMultiSerial = 0;
        }

        private static void EjectPlayersFromLot(ReinoLotDefinition lot)
        {
            if (lot == null || lot.Map == null)
                return;

            ArenaDefinition def = GetDefinitionByConstructionId(ArenaAuroraDefinition.BUILDING_ID);
            if (def == null)
                return;

            Point3D outLoc = new Point3D(lot.NorthWest.X + def.EjectOffset.X, lot.NorthWest.Y + def.EjectOffset.Y, lot.NorthWest.Z + def.EjectOffset.Z);

            foreach (Mobile m in lot.Map.GetMobilesInBounds(lot.Rect))
            {
                if (m is PlayerMobile)
                    m.MoveToWorld(outLoc, lot.Map);
            }
        }
    }
}
