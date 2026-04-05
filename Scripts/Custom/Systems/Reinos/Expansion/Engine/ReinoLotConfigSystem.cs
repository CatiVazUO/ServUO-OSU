using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Custom.Systems.Reinos.Expansion.Engine;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos
{
    public static partial class ReinoExpansionSystem
    {
        private static ReinoLotConfigDefinition GetLotConfig(ReinoLotDefinition lot)
        {
            ReinoLotConfigRegistry.EnsureInitialized();

            if (lot == null)
                return ReinoLotConfigRegistry.Get(0);

            ReinoLotConfigDefinition def = ReinoLotConfigRegistry.Get(lot.LotConfigId);
            if (def == null && lot.Side > 0)
            {
                int randomId = ReinoLotConfigRegistry.GetRandomConfigIdForSide(lot.Side);
                def = ReinoLotConfigRegistry.Get(randomId);
            }

            if (def == null)
                def = ReinoLotConfigRegistry.Get(0);

            return def;
        }

        public static int GetLotConfigId(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.LotConfigId : 0;
        }

        public static int GetLotEncounterOffsetX(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.EncounterOffsetX : 0;
        }

        public static int GetLotEncounterOffsetY(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.EncounterOffsetY : 0;
        }

        public static int GetLotEncounterOffsetZ(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.EncounterOffsetZ : 0;
        }

        public static int GetLotSpawnOffsetX(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.SpawnOffsetX : 0;
        }

        public static int GetLotSpawnOffsetY(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.SpawnOffsetY : 0;
        }

        public static int GetLotSpawnOffsetZ(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            return lot != null ? lot.SpawnOffsetZ : 0;
        }

        public static bool SetLotEncounterOffset(int lotId, int x, int y, int z, out string message)
        {
            message = String.Empty;
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null)
            {
                message = "Lote inválido.";
                return false;
            }

            lot.EncounterOffsetX = x;
            lot.EncounterOffsetY = y;
            lot.EncounterOffsetZ = z;

            if (!state.HasConstructionProgress)
                RespawnLotEncounter(lot, state);

            EnsureLotSign(lot, state);
            message = String.Format("Offset do multi do lote {0} ajustado para {1},{2},{3}.", lotId, x, y, z);
            return true;
        }

        public static bool SetLotSpawnOffset(int lotId, int x, int y, int z, out string message)
        {
            message = String.Empty;
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null)
            {
                message = "Lote inválido.";
                return false;
            }

            lot.SpawnOffsetX = x;
            lot.SpawnOffsetY = y;
            lot.SpawnOffsetZ = z;

            if (!state.HasConstructionProgress)
                RespawnLotEncounter(lot, state);

            EnsureLotSign(lot, state);
            message = String.Format("Offset do spawn do lote {0} ajustado para {1},{2},{3}.", lotId, x, y, z);
            return true;
        }

        public static bool SetLotConfig(int lotId, int configId, out string message)
        {
            message = String.Empty;
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null)
            {
                message = "Lote inválido.";
                return false;
            }

            if (state.HasConstructionProgress)
            {
                message = "Esse lote já tem construção. Remova ou resete o lote antes de trocar o config.";
                return false;
            }

            ReinoLotConfigDefinition config = ReinoLotConfigRegistry.Get(configId);
            if (config == null && configId != 0)
            {
                message = "Config inválido.";
                return false;
            }

            if (configId != 0 && config != null && config.Side != lot.Side)
            {
                message = "Esse config não pertence ao tamanho desse lote.";
                return false;
            }

            lot.LotConfigId = configId;
            state.RearmThreatOnAvailableExpiry = false;
            ApplyLotConfigToDefinition(lot, configId);
            ResetLotEncounter(lot, state, true);

            message = String.Format("Lote {0} agora usa o config {1}.", lotId, configId);
            return true;
        }

        private static void ConfigureNewLot(ReinoLotDefinition lot, ReinoLotState state)
        {
            if (lot == null || state == null)
                return;

            int configId = ReinoLotConfigRegistry.GetRandomConfigIdForSide(lot.Side);
            lot.LotConfigId = configId;
            state.RearmThreatOnAvailableExpiry = false;
            ApplyLotConfigToDefinition(lot, configId);

            if (configId == 0)
            {
                state.Status = ReinoLotStatus.Available;
                state.ObjectiveProgress = 0;
                state.AvailableUntilUtc = DateTime.MinValue;
                CleanupLotWorldObjects(state);
                EnsureLotSign(lot, state);
                return;
            }

            ResetLotEncounter(lot, state, true);
        }

        private static void ApplyLotConfigToDefinition(ReinoLotDefinition lot, int configId)
        {
            if (lot == null)
                return;

            ReinoLotConfigDefinition config = ReinoLotConfigRegistry.Get(configId) ?? ReinoLotConfigRegistry.Get(0);

            if (config == null || configId == 0)
            {
                lot.Objective = new ReinoObjectiveDefinition();
                lot.LotConfigId = 0;
                return;
            }

            lot.Objective = new ReinoObjectiveDefinition();
            lot.Objective.Type = config.ObjectiveType;
            lot.Objective.DisplayName = config.ObjectiveDisplayName ?? String.Empty;
            lot.Objective.RequiredAmount = config.ObjectiveAmount;
            lot.Objective.ResourceType = ReinoResourceType.None;
            lot.Objective.TargetTypeNames = config.ObjectiveTargetTypeNames ?? new string[0];
        }

        private static void ResetLotEncounter(ReinoLotDefinition lot, ReinoLotState state, bool preserveAvailableWindow)
        {
            if (lot == null || state == null)
                return;

            CleanupLotWorldObjects(state);

            ReinoLotConfigDefinition config = GetLotConfig(lot);

            if (config == null || config.IsEmpty || lot.LotConfigId == 0)
            {
                state.Status = ReinoLotStatus.Available;
                state.ObjectiveProgress = 0;
                state.AvailableUntilUtc = DateTime.MinValue;
                state.ConstructionId = String.Empty;
                state.CurrentStageIndex = -1;
                state.NextStageUtc = DateTime.MinValue;
                state.ReactivateReadyUtc = DateTime.MinValue;

                EnsureLotSign(lot, state);
                return;
            }

            state.Status = ReinoLotStatus.Locked;
            state.ObjectiveProgress = 0;
            if (!preserveAvailableWindow)
                state.AvailableUntilUtc = DateTime.MinValue;
            else
                state.AvailableUntilUtc = DateTime.MinValue;

            state.ConstructionId = String.Empty;
            state.CurrentStageIndex = -1;
            state.NextStageUtc = DateTime.MinValue;
            state.ReactivateReadyUtc = DateTime.MinValue;
            state.NextThreatRespawnUtc = DateTime.MinValue;

            RespawnLotEncounter(lot, state);
            EnsureLotSign(lot, state);
        }

        private static void CompleteLotObjective(ReinoLotDefinition lot, ReinoLotState state)
        {
            if (lot == null || state == null)
                return;

            CleanupLotWorldObjects(state);
            CleanupDanglingLotEncounterMultis(lot);

            state.Status = ReinoLotStatus.Available;
            state.ObjectiveProgress = lot.Objective != null ? lot.Objective.RequiredAmount : state.ObjectiveProgress;
            state.AvailableUntilUtc = lot.LotConfigId == 0 ? DateTime.MinValue : (DateTime.UtcNow + TimeSpan.FromDays(7.0));
            EnsureLotSign(lot, state);
        }

        private static void HandleAvailableLotExpiry(ReinoLotDefinition lot, ReinoLotState state)
        {
            if (lot == null || state == null)
                return;

            if (state.RearmThreatOnAvailableExpiry)
            {
                state.RearmThreatOnAvailableExpiry = false;
                int randomId = ReinoLotConfigRegistry.GetRandomConfigIdForSide(lot.Side);
                if (randomId <= 0)
                {
                    lot.LotConfigId = 0;
                    ApplyLotConfigToDefinition(lot, 0);
                    state.AvailableUntilUtc = DateTime.MinValue;
                    state.Status = ReinoLotStatus.Available;
                    EnsureLotSign(lot, state);
                    return;
                }

                lot.LotConfigId = randomId;
                ApplyLotConfigToDefinition(lot, randomId);
                ResetLotEncounter(lot, state, false);
                return;
            }

            if (lot.LotConfigId == 0)
            {
                state.AvailableUntilUtc = DateTime.MinValue;
                state.Status = ReinoLotStatus.Available;
                EnsureLotSign(lot, state);
                return;
            }

            ResetLotEncounter(lot, state, false);
        }

        private static Point3D GetLotEncounterAnchor(ReinoLotDefinition lot, ReinoLotConfigDefinition config)
        {
            Point3D topLeft = new Point3D(lot.NorthWest.X + lot.EncounterOffsetX + config.EncounterOffset.X, lot.NorthWest.Y + lot.EncounterOffsetY + config.EncounterOffset.Y, lot.NorthWest.Z + lot.EncounterOffsetZ + config.EncounterOffset.Z);
            return GetAnchorForTopLeft(topLeft, config.EncounterMultiId, null);
        }

        private static Point3D GetLotSpawnCenter(ReinoLotDefinition lot, ReinoLotConfigDefinition config)
        {
            Point3D center = lot.GetCenter(lot.NorthWest.Z);
            return new Point3D(center.X + lot.SpawnOffsetX + config.SpawnOffset.X, center.Y + lot.SpawnOffsetY + config.SpawnOffset.Y, center.Z + lot.SpawnOffsetZ + config.SpawnOffset.Z);
        }

        private static void RespawnLotEncounter(ReinoLotDefinition lot, ReinoLotState state)
        {
            if (lot == null || state == null || state.HasConstructionProgress)
                return;

            ReinoLotConfigDefinition config = GetLotConfig(lot);
            if (config == null || config.IsEmpty || lot.LotConfigId == 0)
            {
                CleanupLotWorldObjects(state);
                state.Status = ReinoLotStatus.Available;
                state.AvailableUntilUtc = DateTime.MinValue;
                EnsureLotSign(lot, state);
                return;
            }

            if (state.MultiSerial <= 0 && config.EncounterMultiId > 0)
            {
                Point3D anchor = GetLotEncounterAnchor(lot, config);
                ReinoConstructionMulti placed = new ReinoConstructionMulti(config.EncounterMultiId, lot.LotId, "LotConfig:" + config.ConfigId, -2);
                placed.MoveToWorld(anchor, lot.Map);
                state.MultiSerial = placed.Serial.Value;
            }

            EnsureLotEncounterPopulation(lot, state, config);
            EnsureLotSign(lot, state);
        }

        private static void EnsureLotEncounterPopulation(ReinoLotDefinition lot, ReinoLotState state, ReinoLotConfigDefinition config)
        {
            if (lot == null || state == null || config == null || config.IsEmpty)
                return;

            if (state.NextThreatRespawnUtc != DateTime.MinValue && DateTime.UtcNow < state.NextThreatRespawnUtc)
                return;

            if (state.ThreatMobSerials == null)
                state.ThreatMobSerials = new List<int>();
            if (state.ThreatItemSerials == null)
                state.ThreatItemSerials = new List<int>();

            CleanupMissingThreatSerials(state);

            int remainingNeeded = lot.Objective != null ? (lot.Objective.RequiredAmount - state.ObjectiveProgress) : 0;
            if (remainingNeeded <= 0)
                return;

            int desiredCount = Math.Min(config.SpawnCount, remainingNeeded);
            if (desiredCount < 0)
                desiredCount = 0;

            if (config.ObjectiveType == ReinoObjectiveType.KillMob)
            {
                int missing = desiredCount - state.ThreatMobSerials.Count;
                for (int i = 0; i < missing; i++)
                    SpawnLotThreatMob(lot, state, config);
                state.NextThreatRespawnUtc = DateTime.UtcNow + config.RespawnDelay;
            }
            else if (config.ObjectiveType == ReinoObjectiveType.CollectItem)
            {
                int missing = desiredCount - state.ThreatItemSerials.Count;
                for (int i = 0; i < missing; i++)
                    SpawnLotCollectible(lot, state, config);
                state.NextThreatRespawnUtc = DateTime.UtcNow + config.RespawnDelay;
            }
        }

        private static void CleanupMissingThreatSerials(ReinoLotState state)
        {
            if (state == null)
                return;

            if (state.ThreatMobSerials != null)
            {
                for (int i = state.ThreatMobSerials.Count - 1; i >= 0; i--)
                {
                    Mobile mob = state.ThreatMobSerials[i] > 0 ? World.FindMobile((Serial)state.ThreatMobSerials[i]) : null;
                    if (mob == null || mob.Deleted)
                        state.ThreatMobSerials.RemoveAt(i);
                }
            }

            if (state.ThreatItemSerials != null)
            {
                for (int i = state.ThreatItemSerials.Count - 1; i >= 0; i--)
                {
                    Item item = state.ThreatItemSerials[i] > 0 ? World.FindItem((Serial)state.ThreatItemSerials[i]) : null;
                    if (item == null || item.Deleted)
                        state.ThreatItemSerials.RemoveAt(i);
                }
            }
        }

        private static void SpawnLotThreatMob(ReinoLotDefinition lot, ReinoLotState state, ReinoLotConfigDefinition config)
        {
            if (lot == null || state == null || config == null || config.MobEntries == null || config.MobEntries.Length == 0)
                return;

            ReinoLotMobSpawnEntry entry = GetRandomMobEntry(config.MobEntries);
            if (entry == null || String.IsNullOrWhiteSpace(entry.TypeName))
                return;

            Type mobType = ScriptCompiler.FindTypeByFullName(entry.TypeName);
            if (mobType == null)
                mobType = ScriptCompiler.FindTypeByName(entry.TypeName);

            if (mobType == null || !typeof(Mobile).IsAssignableFrom(mobType))
                return;

            object instance = Activator.CreateInstance(mobType);
            BaseReinoLotThreat mob = instance as BaseReinoLotThreat;
            if (mob == null)
            {
                Mobile m = instance as Mobile;
                if (m != null)
                    m.Delete();

                return;
            }

            Point3D center = GetLotSpawnCenter(lot, config);
            int range = config.SpawnRange;

            ReinoLotSpawnPointDefinition spawnPoint = entry.SpawnPoint ?? GetRandomSpawnPoint(config);
            if (spawnPoint != null)
            {
                center = new Point3D(
                    lot.GetCenter(lot.NorthWest.Z).X + lot.SpawnOffsetX + spawnPoint.Offset.X,
                    lot.GetCenter(lot.NorthWest.Z).Y + lot.SpawnOffsetY + spawnPoint.Offset.Y,
                    lot.GetCenter(lot.NorthWest.Z).Z + lot.SpawnOffsetZ + spawnPoint.Offset.Z);

                range = spawnPoint.Range;
            }

            Point3D loc = GetRandomPointInLot(lot, center, range, lot.NorthWest.Z);

            mob.LotId = lot.LotId;
            mob.ConfigId = config.ConfigId;

            if (!String.IsNullOrWhiteSpace(entry.DisplayName))
                mob.Name = entry.DisplayName;

            if (entry.Hue != 0)
                mob.Hue = entry.Hue;

            mob.Home = center;
            mob.RangeHome = Math.Max(2, range);
            mob.MoveToWorld(loc, lot.Map);

            state.ThreatMobSerials.Add(mob.Serial.Value);
        }

        private static ReinoLotSpawnPointDefinition GetRandomSpawnPoint(ReinoLotConfigDefinition config)
        {
            if (config == null || config.SpawnPoints == null || config.SpawnPoints.Length == 0)
                return null;

            int total = 0;

            for (int i = 0; i < config.SpawnPoints.Length; i++)
            {
                ReinoLotSpawnPointDefinition p = config.SpawnPoints[i];
                if (p != null && p.Weight > 0)
                    total += p.Weight;
            }

            if (total <= 0)
                return config.SpawnPoints[0];

            int roll = Utility.Random(total);

            for (int i = 0; i < config.SpawnPoints.Length; i++)
            {
                ReinoLotSpawnPointDefinition p = config.SpawnPoints[i];
                if (p == null || p.Weight <= 0)
                    continue;

                if (roll < p.Weight)
                    return p;

                roll -= p.Weight;
            }

            return config.SpawnPoints[0];
        }
        private static void SpawnLotCollectible(ReinoLotDefinition lot, ReinoLotState state, ReinoLotConfigDefinition config)
        {
            if (lot == null || state == null || config == null || config.CollectibleEntries == null || config.CollectibleEntries.Length == 0)
                return;

            ReinoLotCollectibleSpawnEntry entry = GetRandomCollectibleEntry(config.CollectibleEntries);
            if (entry == null)
                return;

            Point3D center = GetLotSpawnCenter(lot, config);
            int range = config.SpawnRange;

            ReinoLotSpawnPointDefinition spawnPoint = entry.SpawnPoint ?? GetRandomSpawnPoint(config);
            if (spawnPoint != null)
            {
                center = new Point3D(
                    lot.GetCenter(lot.NorthWest.Z).X + lot.SpawnOffsetX + spawnPoint.Offset.X,
                    lot.GetCenter(lot.NorthWest.Z).Y + lot.SpawnOffsetY + spawnPoint.Offset.Y,
                    lot.GetCenter(lot.NorthWest.Z).Z + lot.SpawnOffsetZ + spawnPoint.Offset.Z);

                range = spawnPoint.Range;
            }

            Point3D loc = GetRandomPointInLot(lot, center, range, lot.NorthWest.Z);

            ReinoLotCollectible item = new ReinoLotCollectible(entry.ItemId, entry.Hue, entry.DisplayName, lot.LotId, config.ConfigId, entry.TypeName, entry.RequiredToolTypeName);
            item.MoveToWorld(loc, lot.Map);
            state.ThreatItemSerials.Add(item.Serial.Value);
        }

        private static Point3D GetRandomPointInLot(ReinoLotDefinition lot, Point3D center, int range, int baseZ)
        {
            if (lot == null)
                return Point3D.Zero;

            int minX = lot.NorthWest.X + 1;
            int maxX = lot.NorthWest.X + lot.Side - 2;
            int minY = lot.NorthWest.Y + 1;
            int maxY = lot.NorthWest.Y + lot.Side - 2;

            int targetX = Utility.RandomMinMax(Math.Max(minX, center.X - range), Math.Min(maxX, center.X + range));
            int targetY = Utility.RandomMinMax(Math.Max(minY, center.Y - range), Math.Min(maxY, center.Y + range));

            int z = lot.Map != null ? lot.Map.GetAverageZ(targetX, targetY) : baseZ;
            return new Point3D(targetX, targetY, z);
        }

        private static ReinoLotMobSpawnEntry GetRandomMobEntry(ReinoLotMobSpawnEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
                return null;

            int total = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                ReinoLotMobSpawnEntry e = entries[i];
                if (e != null && e.Weight > 0)
                    total += e.Weight;
            }

            if (total <= 0)
                return entries[0];

            int roll = Utility.Random(total);
            for (int i = 0; i < entries.Length; i++)
            {
                ReinoLotMobSpawnEntry e = entries[i];
                if (e == null || e.Weight <= 0)
                    continue;

                if (roll < e.Weight)
                    return e;

                roll -= e.Weight;
            }

            return entries[0];
        }

        private static ReinoLotCollectibleSpawnEntry GetRandomCollectibleEntry(ReinoLotCollectibleSpawnEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
                return null;

            int total = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                ReinoLotCollectibleSpawnEntry e = entries[i];
                if (e != null && e.Weight > 0)
                    total += e.Weight;
            }

            if (total <= 0)
                return entries[0];

            int roll = Utility.Random(total);
            for (int i = 0; i < entries.Length; i++)
            {
                ReinoLotCollectibleSpawnEntry e = entries[i];
                if (e == null || e.Weight <= 0)
                    continue;

                if (roll < e.Weight)
                    return e;

                roll -= e.Weight;
            }

            return entries[0];
        }

        public static bool NotifyLotCollectibleUsed(PlayerMobile pm, int lotId, string collectibleTypeName)
        {
            if (pm == null || pm.Deleted || String.IsNullOrWhiteSpace(collectibleTypeName))
                return false;

            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null || state.Status != ReinoLotStatus.Locked || lot.Objective == null || lot.Objective.Type != ReinoObjectiveType.CollectItem)
                return false;

            if (!IsPlayerCitizenOfLot(pm, lot))
                return false;

            if (!MatchesAnyTypeName(collectibleTypeName, lot.Objective.TargetTypeNames))
                return false;

            state.ObjectiveProgress++;
            if (state.ObjectiveProgress > lot.Objective.RequiredAmount)
                state.ObjectiveProgress = lot.Objective.RequiredAmount;

            if (state.ObjectiveProgress >= lot.Objective.RequiredAmount)
            {
                CompleteLotObjective(lot, state);
                pm.SendMessage("O {0} foi limpo e agora está disponível para construção.", lot.Name);
            }
            else
            {
                pm.SendMessage("Progresso do {0}: {1}/{2}.", lot.Name, state.ObjectiveProgress, lot.Objective.RequiredAmount);
            }

            return true;
        }

        private static bool IsPlayerCitizenOfLot(PlayerMobile pm, ReinoLotDefinition lot)
        {
            if (pm == null || lot == null)
                return false;

            string citizenCity = PlayerMobile.NormalizeOSUCityId(pm.OSUCitizenCityId);
            if (String.IsNullOrWhiteSpace(citizenCity))
                return false;

            return String.Equals(citizenCity, PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(lot.CityId)), StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValidateLotCollectibleTool(PlayerMobile pm, string requiredToolTypeName, out string message)
        {
            message = String.Empty;

            if (pm == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(requiredToolTypeName))
                return true;

            Item one = pm.FindItemOnLayer(Layer.OneHanded);
            Item two = pm.FindItemOnLayer(Layer.TwoHanded);

            Type requiredType = ScriptCompiler.FindTypeByFullName(requiredToolTypeName);
            if (requiredType == null)
                requiredType = ScriptCompiler.FindTypeByName(requiredToolTypeName);

            if (requiredType == null)
                return true;

            bool valid = (one != null && requiredType.IsAssignableFrom(one.GetType())) || (two != null && requiredType.IsAssignableFrom(two.GetType()));
            if (valid)
                return true;

            message = String.Format("Você precisa estar com {0} equipado para limpar isso.", requiredToolTypeName);
            return false;
        }

        private static bool TryHandleLotThreatDeath(PlayerMobile killer, Mobile dead)
        {
            BaseReinoLotThreat threat = dead as BaseReinoLotThreat;
            if (killer == null || threat == null || threat.LotId <= 0)
                return false;

            ReinoLotDefinition lot = GetLotDefinition(threat.LotId);
            ReinoLotState state = GetLotState(threat.LotId);

            if (lot == null || state == null || state.Status != ReinoLotStatus.Locked || lot.Objective == null || lot.Objective.Type != ReinoObjectiveType.KillMob)
                return false;

            if (!IsPlayerCitizenOfLot(killer, lot))
                return true;

            state.ObjectiveProgress++;
            if (state.ObjectiveProgress > lot.Objective.RequiredAmount)
                state.ObjectiveProgress = lot.Objective.RequiredAmount;

            if (state.ObjectiveProgress >= lot.Objective.RequiredAmount)
            {
                CompleteLotObjective(lot, state);
                killer.SendMessage("O {0} foi limpo e agora está disponível para construção.", lot.Name);
            }
            else
            {
                killer.SendMessage("Progresso do {0}: {1}/{2}.", lot.Name, state.ObjectiveProgress, lot.Objective.RequiredAmount);
            }

            return true;
        }

        private static void WriteExtraLotData(BinaryWriter bw, ReinoLotDefinition def, ReinoLotState st)
        {
            bw.Write(def.LotConfigId);
            bw.Write(def.EncounterOffsetX);
            bw.Write(def.EncounterOffsetY);
            bw.Write(def.EncounterOffsetZ);
            bw.Write(def.SpawnOffsetX);
            bw.Write(def.SpawnOffsetY);
            bw.Write(def.SpawnOffsetZ);

            int mobCount = st.ThreatMobSerials != null ? st.ThreatMobSerials.Count : 0;
            bw.Write(mobCount);
            for (int i = 0; i < mobCount; i++)
                bw.Write(st.ThreatMobSerials[i]);

            int itemCount = st.ThreatItemSerials != null ? st.ThreatItemSerials.Count : 0;
            bw.Write(itemCount);
            for (int i = 0; i < itemCount; i++)
                bw.Write(st.ThreatItemSerials[i]);
        }

        private static void ReadExtraLotData(BinaryReader br, int version, ReinoLotDefinition def, ReinoLotState st)
        {
            if (version >= 5)
            {
                def.LotConfigId = br.ReadInt32();
                def.EncounterOffsetX = br.ReadInt32();
                def.EncounterOffsetY = br.ReadInt32();
                def.EncounterOffsetZ = br.ReadInt32();
                def.SpawnOffsetX = br.ReadInt32();
                def.SpawnOffsetY = br.ReadInt32();
                def.SpawnOffsetZ = br.ReadInt32();

                int mobCount = br.ReadInt32();
                for (int m = 0; m < mobCount; m++)
                    st.ThreatMobSerials.Add(br.ReadInt32());

                int itemCount = br.ReadInt32();
                for (int it = 0; it < itemCount; it++)
                    st.ThreatItemSerials.Add(br.ReadInt32());

                if (def.LotConfigId > 0)
                    ApplyLotConfigToDefinition(def, def.LotConfigId);
            }
            else
            {
                def.LotConfigId = 0;
                def.EncounterOffsetX = 0;
                def.EncounterOffsetY = 0;
                def.EncounterOffsetZ = 0;
                def.SpawnOffsetX = 0;
                def.SpawnOffsetY = 0;
                def.SpawnOffsetZ = 0;
            }
        }
    }
}
