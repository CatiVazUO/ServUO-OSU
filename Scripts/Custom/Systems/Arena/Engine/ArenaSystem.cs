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
            if (lot == null)
                return new Point3D[0];

            ArenaDefinition def = GetJoustDefinition(ReinoMaintenanceSystem.BuildLotKey(lot.LotId));
            if (def == null || def.GladiatorSpawnOffsets == null || def.GladiatorSpawnOffsets.Length == 0)
                return new Point3D[]
                {
                    new Point3D(lot.NorthWest.X + 15, lot.NorthWest.Y + 1, lot.NorthWest.Z),
                    new Point3D(lot.NorthWest.X + 28, lot.NorthWest.Y + 15, lot.NorthWest.Z),
                    new Point3D(lot.NorthWest.X + 15, lot.NorthWest.Y + 28, lot.NorthWest.Z),
                    new Point3D(lot.NorthWest.X + 1, lot.NorthWest.Y + 15, lot.NorthWest.Z)
                };

            Point3D[] result = new Point3D[def.GladiatorSpawnOffsets.Length];
            for (int i = 0; i < def.GladiatorSpawnOffsets.Length; i++)
            {
                Point3D o = def.GladiatorSpawnOffsets[i];
                result[i] = new Point3D(lot.NorthWest.X + o.X, lot.NorthWest.Y + o.Y, lot.NorthWest.Z + o.Z);
            }

            return result;
        }

        public static void ApplyJoustPlacement(ReinoLotDefinition lot, bool flip, PlayerMobile a, PlayerMobile b)
        {
            if (lot == null || a == null || b == null)
                return;

            ArenaDefinition def = GetJoustDefinition(ReinoMaintenanceSystem.BuildLotKey(lot.LotId));
            Point3D leftA = def != null ? def.JoustKnight1Offset : new Point3D(5, 14, 0);
            Point3D leftB = def != null ? def.JoustKnight2Offset : new Point3D(5, 15, 0);

            int mirrorX = 29;
            Point3D rightA = new Point3D(mirrorX - leftA.X, leftA.Y, leftA.Z);
            Point3D rightB = new Point3D(mirrorX - leftB.X, leftB.Y, leftB.Z);

            Point3D aLoc = flip
                ? new Point3D(lot.NorthWest.X + rightA.X, lot.NorthWest.Y + rightA.Y, lot.NorthWest.Z + rightA.Z)
                : new Point3D(lot.NorthWest.X + leftA.X, lot.NorthWest.Y + leftA.Y, lot.NorthWest.Z + leftA.Z);
            Point3D bLoc = flip
                ? new Point3D(lot.NorthWest.X + rightB.X, lot.NorthWest.Y + rightB.Y, lot.NorthWest.Z + rightB.Z)
                : new Point3D(lot.NorthWest.X + leftB.X, lot.NorthWest.Y + leftB.Y, lot.NorthWest.Z + leftB.Z);

            Direction fwd = def != null ? def.JoustDirectionForward : Direction.East;
            Direction runDir = flip ? ReverseDirection(fwd) : fwd;

            a.MoveToWorld(aLoc, lot.Map);
            b.MoveToWorld(bLoc, lot.Map);

            a.Direction = runDir;
            b.Direction = runDir;

            a.CantWalk = true;
            b.CantWalk = true;

            Timer.DelayCall(TimeSpan.FromSeconds(0.2), delegate { ForceRun(a, runDir, 12); });
            Timer.DelayCall(TimeSpan.FromSeconds(0.2), delegate { ForceRun(b, runDir, 12); });
        }

        public static Direction ReverseDirection(Direction dir)
        {
            return (Direction)(((int)(dir & Direction.Mask) + 4) & (int)Direction.Mask);
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
                EnsureBombermanStorage(state, lot);
                ArenaGameModes.GetOrCreateBomberman(constructionKey).Play(lot);
            }

            EnsureDoors(state, lot, false);
        }

        public static void StopEvent(string constructionKey, int cityId, ReinoLotDefinition lot)
        {
            ArenaState state = EnsureState(constructionKey, cityId);
            state.EventStarted = false;
            state.LastChangedUtc = DateTime.UtcNow;

            DeleteCenterMulti(state);
            ArenaGameModes.StopAll(constructionKey);
            DeleteBombermanStorage(state);
            EnsureDoors(state, lot, true);
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

        public static Item GetBombermanStorage(ArenaState state)
        {
            if (state == null || state.StorageChestSerial <= 0)
                return null;

            Item chest = World.FindItem((Serial)state.StorageChestSerial);
            if (chest == null || chest.Deleted)
            {
                state.StorageChestSerial = 0;
                return null;
            }

            return chest;
        }

        private static void EnsureBombermanStorage(ArenaState state, ReinoLotDefinition lot)
        {
            if (state == null || lot == null)
                return;

            if (GetBombermanStorage(state) != null)
                return;

            ArenaDefinition def = GetDefinitionByConstructionId(ArenaAuroraDefinition.BUILDING_ID);
            if (def == null)
                return;

            MetalChest chest = new MetalChest();
            chest.Movable = false;
            chest.Name = "Baú da Arena (Bomberman)";
            chest.Hue = 1150;
            chest.MoveToWorld(new Point3D(lot.NorthWest.X + def.BombermanStorageOffset.X, lot.NorthWest.Y + def.BombermanStorageOffset.Y, lot.NorthWest.Z + def.BombermanStorageOffset.Z), lot.Map);
            state.StorageChestSerial = chest.Serial.Value;
        }

        private static void DeleteBombermanStorage(ArenaState state)
        {
            Item chest = GetBombermanStorage(state);
            if (chest != null)
                chest.Delete();

            if (state != null)
                state.StorageChestSerial = 0;
        }

        private static void EnsureDoors(ArenaState state, ReinoLotDefinition lot, bool keepAll)
        {
            if (state == null || lot == null)
                return;

            ArenaDefinition def = GetDefinitionByConstructionId(ArenaAuroraDefinition.BUILDING_ID);
            if (def == null || def.Doors == null || def.Doors.Length == 0)
                return;

            DeleteDoors(state);

            List<int> serials = new List<int>();
            for (int i = 0; i < def.Doors.Length; i++)
            {
                ArenaDoorOffset door = def.Doors[i];
                if (door == null)
                    continue;

                if (!keepAll && !door.IsEntryDoor)
                    continue;

                ArenaGateBlockItem gate = new ArenaGateBlockItem(door.IsEntryDoor);
                gate.MoveToWorld(new Point3D(lot.NorthWest.X + door.Offset.X, lot.NorthWest.Y + door.Offset.Y, lot.NorthWest.Z + door.Offset.Z), lot.Map);
                serials.Add(gate.Serial.Value);
            }

            state.DoorSerials = serials.ToArray();
        }

        private static void DeleteDoors(ArenaState state)
        {
            if (state == null || state.DoorSerials == null)
                return;

            for (int i = 0; i < state.DoorSerials.Length; i++)
            {
                if (state.DoorSerials[i] <= 0)
                    continue;

                Item item = World.FindItem((Serial)state.DoorSerials[i]);
                if (item != null && !item.Deleted)
                    item.Delete();
            }

            state.DoorSerials = new int[0];
        }
    }
}
