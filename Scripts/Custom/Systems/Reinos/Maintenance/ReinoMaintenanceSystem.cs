using System;
using System.Collections.Generic;
using Server;
using Server.Custom.Systems.Postos;
using Server.Custom.Systems.Rent;
using Server.Custom.Correios;
using Server.Custom.Biblioteca;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class ReinoConstructionRuntimeInfo
    {
        public bool IsArea;
        public int ReferenceId;
        public int CityId;
        public string Key;
        public ReinoConstructionDefinition Definition;
        public ReinoLotDefinition Lot;
        public ReinoLotState LotState;
        public ReinoAreaDefinition Area;
        public ReinoAreaState AreaState;

        public string Name
        {
            get
            {
                if (Definition != null && !String.IsNullOrWhiteSpace(Definition.Name))
                    return Definition.Name;

                if (Lot != null)
                    return Lot.Name;

                if (Area != null)
                    return Area.Name;

                return "Construção";
            }
        }

        public ReinoLotStatus Status
        {
            get { return IsArea ? (AreaState != null ? AreaState.Status : ReinoLotStatus.Locked) : (LotState != null ? LotState.Status : ReinoLotStatus.Locked); }
        }
    }

    public static class ReinoMaintenanceSystem
    {
        public const int WeeksInfinite = Int32.MaxValue;

        private static DateTime m_LastWeeklyMaintenanceUtc = DateTime.MinValue;

        public static DateTime LastWeeklyMaintenanceUtc
        {
            get { return m_LastWeeklyMaintenanceUtc; }
            set { m_LastWeeklyMaintenanceUtc = value; }
        }

        public static string BuildLotKey(int lotId) { return "L:" + lotId; }
        public static string BuildAreaKey(int areaId) { return "A:" + areaId; }

        public static bool TryParseKey(string key, out bool isArea, out int referenceId)
        {
            isArea = false;
            referenceId = 0;

            if (String.IsNullOrWhiteSpace(key) || key.Length < 3 || key[1] != ':')
                return false;

            isArea = Char.ToUpperInvariant(key[0]) == 'A';
            return Int32.TryParse(key.Substring(2), out referenceId);
        }

        public static ReinoConstructionRuntimeInfo GetConstruction(string key)
        {
            bool isArea;
            int referenceId;

            if (!TryParseKey(key, out isArea, out referenceId))
                return null;

            if (isArea)
            {
                ReinoAreaDefinition area = ReinoExpansionSystem.GetAreaDefinition(referenceId);
                ReinoAreaState state = ReinoExpansionSystem.GetAreaState(referenceId);

                if (area == null || state == null || String.IsNullOrWhiteSpace(state.ConstructionId))
                    return null;

                return new ReinoConstructionRuntimeInfo
                {
                    IsArea = true,
                    ReferenceId = referenceId,
                    CityId = area.CityId,
                    Key = BuildAreaKey(referenceId),
                    Definition = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId),
                    Area = area,
                    AreaState = state
                };
            }

            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(referenceId);
            ReinoLotState stateLot = ReinoExpansionSystem.GetLotState(referenceId);

            if (lot == null || stateLot == null || String.IsNullOrWhiteSpace(stateLot.ConstructionId))
                return null;

            return new ReinoConstructionRuntimeInfo
            {
                IsArea = false,
                ReferenceId = referenceId,
                CityId = lot.CityId,
                Key = BuildLotKey(referenceId),
                Definition = ReinoExpansionDefinitions.GetBuilding(stateLot.ConstructionId),
                Lot = lot,
                LotState = stateLot
            };
        }

        public static List<ReinoConstructionRuntimeInfo> GetCityConstructions(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = new List<ReinoConstructionRuntimeInfo>();
            HashSet<int> seenLots = new HashSet<int>();
            List<ReinoLotDefinition> cityLots = ReinoExpansionSystem.GetAllLotsForCity(cityId);

            for (int i = 0; i < cityLots.Count; i++)
            {
                ReinoLotDefinition lot = cityLots[i];
                if (lot == null || seenLots.Contains(lot.LotId))
                    continue;

                seenLots.Add(lot.LotId);

                ReinoLotState state = ReinoExpansionSystem.GetLotState(lot.LotId);
                if (state == null || String.IsNullOrWhiteSpace(state.ConstructionId))
                    continue;

                ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
                if (def == null)
                    continue;

                list.Add(new ReinoConstructionRuntimeInfo
                {
                    IsArea = false,
                    ReferenceId = lot.LotId,
                    CityId = cityId,
                    Key = BuildLotKey(lot.LotId),
                    Definition = def,
                    Lot = lot,
                    LotState = state
                });
            }

            List<ReinoAreaDefinition> wallAreas = ReinoExpansionSystem.GetAreasForCity(cityId, ReinoAreaType.Wall);
            for (int i = 0; i < wallAreas.Count; i++)
            {
                ReinoAreaDefinition area = wallAreas[i];
                ReinoAreaState state = area != null ? ReinoExpansionSystem.GetAreaState(area.AreaId) : null;

                if (area == null || state == null || String.IsNullOrWhiteSpace(state.ConstructionId))
                    continue;

                ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
                if (def == null)
                    continue;

                list.Add(new ReinoConstructionRuntimeInfo
                {
                    IsArea = true,
                    ReferenceId = area.AreaId,
                    CityId = cityId,
                    Key = BuildAreaKey(area.AreaId),
                    Definition = def,
                    Area = area,
                    AreaState = state
                });
            }

            list.Sort(delegate (ReinoConstructionRuntimeInfo a, ReinoConstructionRuntimeInfo b)
            {
                int cmp = GetDisplayPriority(a).CompareTo(GetDisplayPriority(b));
                if (cmp != 0)
                    return cmp;

                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return list;
        }

        public static List<ReinoConstructionRuntimeInfo> GetActiveConstructions(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> all = GetCityConstructions(cityId);
            List<ReinoConstructionRuntimeInfo> list = new List<ReinoConstructionRuntimeInfo>();

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Status == ReinoLotStatus.Active || all[i].Status == ReinoLotStatus.UnderConstruction)
                    list.Add(all[i]);
            }

            return list;
        }

        public static List<ReinoConstructionRuntimeInfo> GetInactiveConstructions(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> all = GetCityConstructions(cityId);
            List<ReinoConstructionRuntimeInfo> list = new List<ReinoConstructionRuntimeInfo>();

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Status == ReinoLotStatus.Abandoned)
                    list.Add(all[i]);
            }

            return list;
        }

        public static List<PostoDefinition> GetActivePostos(int cityId)
        {
            List<PostoDefinition> list = new List<PostoDefinition>();
            string cityName = ReinoElectionsSystem.GetCityName(cityId);

            foreach (PostoDefinition def in PostoSystem.AllDefinitions)
            {
                if (def == null)
                    continue;

                PostoState state = PostoSystem.GetState(def.Id);
                if (state != null && PostoSystem.SameCity(state.OwnerCityId, cityName))
                    list.Add(def);
            }

            list.Sort(delegate (PostoDefinition a, PostoDefinition b)
            {
                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return list;
        }

        public static List<PostoDefinition> GetDisputedPostos(int cityId)
        {
            List<PostoDefinition> list = new List<PostoDefinition>();
            string cityName = ReinoElectionsSystem.GetCityName(cityId);

            foreach (PostoDefinition def in PostoSystem.AllDefinitions)
            {
                if (def == null)
                    continue;

                PostoState state = PostoSystem.GetState(def.Id);
                if (state == null)
                    continue;

                if (PostoSystem.SameCity(state.OwnerCityId, cityName) && PostoSystem.IsContestActive(state))
                    list.Add(def);
            }

            list.Sort(delegate (PostoDefinition a, PostoDefinition b)
            {
                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return list;
        }

        public static bool ShouldRunWeeklyMaintenanceNow()
        {
            DateTime slot = GetCurrentWeeklyMaintenanceSlotUtc();

            if (DateTime.UtcNow < slot)
                return false;

            if (m_LastWeeklyMaintenanceUtc >= slot)
                return false;

            m_LastWeeklyMaintenanceUtc = slot;
            return true;
        }

        private static DateTime GetCurrentWeeklyMaintenanceSlotUtc()
        {
            DateTime now = DateTime.UtcNow;

            int daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
            DateTime monday = now.Date.AddDays(-daysSinceMonday);

            DateTime slot = monday.AddHours(20); // segunda 20:00 UTC = 17:00 Recife

            if (now < slot)
                slot = slot.AddDays(-7);

            return slot;
        }

        public static int GetStoredPriority(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            int value = info.IsArea ? info.AreaState.MaintenancePriority : info.LotState.MaintenancePriority;

            if (value <= 0 && info.Definition != null)
            {
                value = info.Definition.MaintenancePriority;

                if (info.IsArea)
                    info.AreaState.MaintenancePriority = value;
                else
                    info.LotState.MaintenancePriority = value;
            }

            return value;
        }

        public static int GetDisplayPriority(ReinoConstructionRuntimeInfo info)
        {
            if (info == null || info.Definition == null)
                return 999;

            if (!info.Definition.AllowPriorityChange)
                return Math.Max(1, info.Definition.MaintenancePriority);

            int value = GetStoredPriority(info);
            return value <= 0 ? Math.Max(1, info.Definition.MaintenancePriority) : value;
        }

        public static void NormalizeCityPriorities(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> all = GetCityConstructions(cityId);
            HashSet<int> reserved = new HashSet<int>();
            List<ReinoConstructionRuntimeInfo> changeable = new List<ReinoConstructionRuntimeInfo>();

            for (int i = 0; i < all.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = all[i];

                if (info.Definition == null)
                    continue;

                if (!info.Definition.AllowPriorityChange)
                {
                    int fixedPriority = Math.Max(1, info.Definition.MaintenancePriority);
                    if (info.IsArea)
                        info.AreaState.MaintenancePriority = fixedPriority;
                    else
                        info.LotState.MaintenancePriority = fixedPriority;

                    reserved.Add(fixedPriority);
                }
                else
                {
                    changeable.Add(info);
                }
            }

            changeable.Sort(delegate (ReinoConstructionRuntimeInfo a, ReinoConstructionRuntimeInfo b)
            {
                int cmp = GetStoredPriority(a).CompareTo(GetStoredPriority(b));
                if (cmp != 0)
                    return cmp;

                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            int next = 1;
            for (int i = 0; i < changeable.Count; i++)
            {
                while (reserved.Contains(next))
                    next++;

                if (changeable[i].IsArea)
                    changeable[i].AreaState.MaintenancePriority = next;
                else
                    changeable[i].LotState.MaintenancePriority = next;

                next++;
            }
        }

        public static bool TryShiftPriority(PlayerMobile from, int cityId, string key, int delta, out string message)
        {
            message = String.Empty;

            if (from == null || from.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(from, cityId))
            {
                message = "Somente o governador pode alterar prioridades.";
                return false;
            }

            ReinoConstructionRuntimeInfo selected = GetConstruction(key);
            if (selected == null || selected.CityId != cityId || selected.Definition == null)
            {
                message = "Construção inválida.";
                return false;
            }

            if (!selected.Definition.AllowPriorityChange)
            {
                message = "A prioridade dessa construção é fixa.";
                return false;
            }

            NormalizeCityPriorities(cityId);
            List<ReinoConstructionRuntimeInfo> all = GetCityConstructions(cityId);
            List<ReinoConstructionRuntimeInfo> changeable = new List<ReinoConstructionRuntimeInfo>();

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Definition != null && all[i].Definition.AllowPriorityChange)
                    changeable.Add(all[i]);
            }

            changeable.Sort(delegate (ReinoConstructionRuntimeInfo a, ReinoConstructionRuntimeInfo b)
            {
                return GetStoredPriority(a).CompareTo(GetStoredPriority(b));
            });

            int index = -1;
            for (int i = 0; i < changeable.Count; i++)
            {
                if (String.Equals(changeable[i].Key, selected.Key, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                message = "Construção não encontrada na fila de prioridade.";
                return false;
            }

            int other = index + delta;
            if (other < 0 || other >= changeable.Count)
            {
                message = delta < 0 ? "Essa construção já está no topo das prioridades móveis." : "Essa construção já está na última prioridade móvel.";
                return false;
            }

            int current = GetStoredPriority(changeable[index]);
            int swap = GetStoredPriority(changeable[other]);

            if (changeable[index].IsArea)
                changeable[index].AreaState.MaintenancePriority = swap;
            else
                changeable[index].LotState.MaintenancePriority = swap;

            if (changeable[other].IsArea)
                changeable[other].AreaState.MaintenancePriority = current;
            else
                changeable[other].LotState.MaintenancePriority = current;

            NormalizeCityPriorities(cityId);
            message = "Prioridade ajustada.";
            return true;
        }

        public static bool TryToggleActivation(PlayerMobile from, int cityId, string key, out string message)
        {
            message = String.Empty;
            ReinoConstructionRuntimeInfo info = GetConstruction(key);

            if (info == null || info.CityId != cityId || info.Definition == null)
            {
                message = "Construção inválida.";
                return false;
            }

            if (info.Status == ReinoLotStatus.Active)
                return TryDeactivate(from, cityId, info, out message);

            if (info.Status == ReinoLotStatus.Abandoned)
                return TryActivate(from, cityId, info, out message);

            message = "Essa construção não pode ser alternada agora.";
            return false;
        }

        private static bool TryDeactivate(PlayerMobile from, int cityId, ReinoConstructionRuntimeInfo info, out string message)
        {
            message = String.Empty;

            if (from == null || from.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(from, cityId))
            {
                message = "Somente o governador pode desativar construções.";
                return false;
            }

            if (!info.Definition.AllowManualActivationToggle)
            {
                message = "Essa construção não pode ser desativada manualmente.";
                return false;
            }

            return info.IsArea
                ? ReinoExpansionSystem.TryDeactivateAreaConstruction(cityId, info.ReferenceId, out message)
                : ReinoExpansionSystem.TryDeactivateLotConstruction(cityId, info.ReferenceId, out message);
        }

        private static bool TryActivate(PlayerMobile from, int cityId, ReinoConstructionRuntimeInfo info, out string message)
        {
            message = String.Empty;

            if (from == null || from.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(from, cityId))
            {
                message = "Somente o governador pode ativar construções.";
                return false;
            }

            List<ReinoResourceCost> weeklyCosts = GetWeeklyCosts(info);
            string fail;

            if (!TryConsumeCosts(cityId, weeklyCosts, out fail))
            {
                message = fail;
                return false;
            }

            return info.IsArea
                ? ReinoExpansionSystem.TryConfirmAreaConstruction(from, cityId, info.ReferenceId, info.Definition.Id, out message)
                : ReinoExpansionSystem.TryConfirmLotConstruction(from, cityId, info.ReferenceId, info.Definition.Id, out message);
        }


        public static bool TryDemolishConstruction(PlayerMobile from, int cityId, string key, out string message)
        {
            message = String.Empty;

            if (from == null || from.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(from, cityId))
            {
                message = "Somente o governador pode demolir construções.";
                return false;
            }

            ReinoConstructionRuntimeInfo info = GetConstruction(key);
            if (info == null || info.CityId != cityId)
            {
                message = "Construção inválida.";
                return false;
            }

            if (info.IsArea)
            {
                message = "Esse botão só demole construções de lote.";
                return false;
            }

            if (info.Definition != null && info.Definition.RentalTemplates != null && info.Definition.RentalTemplates.Length > 0)
            {
                message = "Essa construção não pode ser demolida.";
                return false;
            }

            return ReinoExpansionSystem.TryDemolishLotConstruction(cityId, info.ReferenceId, out message);
        }
        public static List<ReinoResourceCost> GetWeeklyCosts(ReinoConstructionRuntimeInfo info)
        {
            List<ReinoResourceCost> list = new List<ReinoResourceCost>();

            if (info == null || info.Definition == null)
                return list;

            AddCosts(list, info.Definition.MaintenanceCosts);
            ReinoMilitarySystem.AddDynamicWeeklyCosts(info, list);
            Server.Custom.Systems.Health.OSUHealthSystem.AddDynamicWeeklyCosts(info, list);

            int npcCost = GetNpcCount(info) * Math.Max(0, info.Definition.NpcWeeklySalaryGold);
            if (npcCost > 0)
                list.Add(new ReinoResourceCost(ReinoResourceType.Gold, npcCost));

            int commission = GetCommissionCount(info) * Math.Max(0, GetCommissionWeeklySalaryGold(info));
            if (commission > 0)
                list.Add(new ReinoResourceCost(ReinoResourceType.Gold, commission));

            MergeCosts(list);
            return list;
        }

        private static void AddCosts(List<ReinoResourceCost> list, ReinoResourceCost[] costs)
        {
            if (list == null || costs == null)
                return;

            for (int i = 0; i < costs.Length; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost == null || cost.Amount <= 0)
                    continue;

                list.Add(new ReinoResourceCost(cost.Type, cost.Amount));
            }
        }

        private static void MergeCosts(List<ReinoResourceCost> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ReinoResourceCost source = list[i];
                if (source == null)
                {
                    list.RemoveAt(i);
                    continue;
                }

                for (int j = i - 1; j >= 0; j--)
                {
                    ReinoResourceCost target = list[j];
                    if (target != null && target.Type == source.Type)
                    {
                        target.Amount += source.Amount;
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public static bool TryConsumeCosts(int cityId, List<ReinoResourceCost> costs, out string message)
        {
            message = String.Empty;
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            for (int i = 0; i < costs.Count; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost == null || cost.Amount <= 0)
                    continue;

                if (!ledger.Has(cost.Type, cost.Amount))
                {
                    message = String.Format("Faltam {0} de {1} no tesouro do reino.", cost.Amount, ReinoExpansionSystem.GetResourceLabel(cost.Type));
                    return false;
                }
            }

            for (int i = 0; i < costs.Count; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost == null || cost.Amount <= 0)
                    continue;

                ledger.Add(cost.Type, -cost.Amount);
            }

            return true;
        }

        private static void RefundCosts(int cityId, List<ReinoResourceCost> costs)
        {
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            for (int i = 0; i < costs.Count; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost == null || cost.Amount <= 0)
                    continue;

                ledger.Add(cost.Type, cost.Amount);
            }
        }

        public static int GetNpcCount(ReinoConstructionRuntimeInfo info)
        {
            if (info == null || info.IsArea || info.LotState == null)
                return 0;

            int count = 0;

            if (info.LotState.NpcSerials != null && info.LotState.NpcSerials.Count > 0)
            {
                for (int i = 0; i < info.LotState.NpcSerials.Count; i++)
                {
                    Mobile mob = World.FindMobile((Serial)info.LotState.NpcSerials[i]);
                    if (mob != null && !mob.Deleted)
                        count++;
                }

                return count;
            }

            if (info.LotState.NpcSerial <= 0)
                return 0;

            Mobile single = World.FindMobile((Serial)info.LotState.NpcSerial);
            return single != null && !single.Deleted ? 1 : 0;
        }

        public static int GetCommissionCount(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            return info.IsArea ? info.AreaState.CommissionedRoleCount : info.LotState.CommissionedRoleCount;
        }

        public static int GetCommissionWeeklySalaryGold(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            return info.IsArea ? info.AreaState.CommissionedRoleWeeklySalaryGold : info.LotState.CommissionedRoleWeeklySalaryGold;
        }

        private static List<TownHouseSign> GetRuntimeRentalSigns(ReinoConstructionRuntimeInfo info)
        {
            List<TownHouseSign> list = new List<TownHouseSign>();

            if (info == null)
                return list;

            List<int> serials = info.IsArea ? info.AreaState.RentalSignSerials : info.LotState.RentalSignSerials;
            if (serials == null)
                return list;

            for (int i = 0; i < serials.Count; i++)
            {
                TownHouseSign sign = World.FindItem((Serial)serials[i]) as TownHouseSign;
                if (sign != null && !sign.Deleted)
                    list.Add(sign);
            }

            return list;
        }

        private static int ConvertToWeeklyGold(TownHouseSign sign)
        {
            if (sign == null || sign.Deleted || !sign.Owned || sign.Free || sign.Price <= 0)
                return 0;

            if (sign.RentByTime <= TimeSpan.Zero)
                return 0;

            double days = Math.Max(1.0, sign.RentByTime.TotalDays);
            return Math.Max(0, (int)Math.Round(sign.Price * (7.0 / days)));
        }

        public static int GetCurrentRecurringRevenueGold(ReinoConstructionRuntimeInfo info)
        {
            int total = 0;
            List<TownHouseSign> signs = GetRuntimeRentalSigns(info);

            for (int i = 0; i < signs.Count; i++)
                total += ConvertToWeeklyGold(signs[i]);

            return total;
        }

        public static int GetBaseMaintenanceGoldOnly(ReinoConstructionRuntimeInfo info)
        {
            if (info == null || info.Definition == null || info.Definition.MaintenanceCosts == null)
                return 0;

            int total = 0;

            for (int i = 0; i < info.Definition.MaintenanceCosts.Length; i++)
            {
                ReinoResourceCost cost = info.Definition.MaintenanceCosts[i];
                if (cost != null && cost.Type == ReinoResourceType.Gold)
                    total += Math.Max(0, cost.Amount);
            }

            return total;
        }

        public static int GetNpcWeeklyTotalGold(ReinoConstructionRuntimeInfo info)
        {
            if (info == null || info.Definition == null)
                return 0;

            return GetNpcCount(info) * Math.Max(0, info.Definition.NpcWeeklySalaryGold);
        }

        public static int GetCommissionWeeklyTotalGold(ReinoConstructionRuntimeInfo info)
        {
            return GetCommissionCount(info) * Math.Max(0, GetCommissionWeeklySalaryGold(info));
        }

        public static int GetOperatingWeeks(ReinoConstructionRuntimeInfo info)
        {
            DateTime since = GetLastActivatedUtc(info);

            if (since == DateTime.MinValue)
                return 0;

            return Math.Max(1, (int)Math.Floor((DateTime.UtcNow - since).TotalDays / 7.0));
        }

        public static int GetNetWeeklyGold(ReinoConstructionRuntimeInfo info)
        {
            int recurring = GetCurrentRecurringRevenueGold(info);
            int maintenance = GetBaseMaintenanceGoldOnly(info);
            int npc = GetNpcWeeklyTotalGold(info);
            int commission = GetCommissionWeeklyTotalGold(info);

            return recurring - maintenance - npc - commission;
        }

        public static int GetTotalRevenueGold(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            return info.IsArea ? info.AreaState.TotalRevenueGold : info.LotState.TotalRevenueGold;
        }

        public static int GetRevenueLast7DaysGold(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            if (info.IsArea)
            {
                TouchRevenueWindow(info.AreaState);
                return info.AreaState.RevenueLast7DaysGold;
            }

            TouchRevenueWindow(info.LotState);
            return info.LotState.RevenueLast7DaysGold;
        }

        public static DateTime GetLastActivatedUtc(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return DateTime.MinValue;

            return info.IsArea ? info.AreaState.LastActivatedUtc : info.LotState.LastActivatedUtc;
        }

        public static int GetAverageRevenuePerWeekCurrentActivation(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            DateTime since = GetLastActivatedUtc(info);
            int revenue = info.IsArea ? info.AreaState.RevenueCurrentActivationGold : info.LotState.RevenueCurrentActivationGold;

            if (since == DateTime.MinValue || revenue <= 0)
                return 0;

            double weeks = Math.Max(1.0, (DateTime.UtcNow - since).TotalDays / 7.0);
            return Math.Max(0, (int)Math.Round(revenue / weeks));
        }

        public static int GetNpcTotalWagesGold(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            return info.IsArea ? info.AreaState.TotalNpcWagesGold : info.LotState.TotalNpcWagesGold;
        }

        public static int GetNpcWagesAveragePerWeekCurrentActivation(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            DateTime since = GetLastActivatedUtc(info);
            int total = info.IsArea ? info.AreaState.NpcWagesCurrentActivationGold : info.LotState.NpcWagesCurrentActivationGold;

            if (since == DateTime.MinValue || total <= 0)
                return 0;

            double weeks = Math.Max(1.0, (DateTime.UtcNow - since).TotalDays / 7.0);
            return Math.Max(0, (int)Math.Round(total / weeks));
        }

        public static int GetCommissionTotalWagesGold(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            return info.IsArea ? info.AreaState.TotalCommissionWagesGold : info.LotState.TotalCommissionWagesGold;
        }

        public static int GetCommissionWagesAveragePerWeekCurrentActivation(ReinoConstructionRuntimeInfo info)
        {
            if (info == null)
                return 0;

            DateTime since = GetLastActivatedUtc(info);
            int total = info.IsArea ? info.AreaState.CommissionWagesCurrentActivationGold : info.LotState.CommissionWagesCurrentActivationGold;

            if (since == DateTime.MinValue || total <= 0)
                return 0;

            double weeks = Math.Max(1.0, (DateTime.UtcNow - since).TotalDays / 7.0);
            return Math.Max(0, (int)Math.Round(total / weeks));
        }

        public static List<ReinoResourceCost> GetTotalActiveWeeklyCost(int cityId)
        {
            return SumCosts(GetActiveConstructions(cityId));
        }

        public static List<ReinoResourceCost> GetTotalInactiveActivationCost(int cityId)
        {
            return SumCosts(GetInactiveConstructions(cityId));
        }

        private static List<ReinoResourceCost> SumCosts(List<ReinoConstructionRuntimeInfo> constructions)
        {
            List<ReinoResourceCost> list = new List<ReinoResourceCost>();

            for (int i = 0; i < constructions.Count; i++)
                list.AddRange(GetWeeklyCosts(constructions[i]));

            MergeCosts(list);
            return list;
        }

        public static int GetWeeksOfOperationRemaining(int cityId)
        {
            List<ReinoResourceCost> costs = GetTotalActiveWeeklyCost(cityId);
            if (costs.Count == 0)
                return WeeksInfinite;

            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            int minWeeks = WeeksInfinite;

            for (int i = 0; i < costs.Count; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost == null || cost.Amount <= 0)
                    continue;

                int available = ledger.Get(cost.Type);
                int weeks = available / cost.Amount;
                if (weeks < minWeeks)
                    minWeeks = weeks;
            }

            return minWeeks == WeeksInfinite ? 0 : Math.Max(0, minWeeks);
        }

        public static string FormatCostLine(List<ReinoResourceCost> costs)
        {
            int gold = 0;
            int cloth = 0;
            int iron = 0;
            int wood = 0;

            for (int i = 0; i < costs.Count; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost == null)
                    continue;

                switch (cost.Type)
                {
                    case ReinoResourceType.Gold: gold += cost.Amount; break;
                    case ReinoResourceType.Cloth: cloth += cost.Amount; break;
                    case ReinoResourceType.Iron: iron += cost.Amount; break;
                    case ReinoResourceType.Wood: wood += cost.Amount; break;
                }
            }

            return String.Format("Moedas: {0}  Tecidos: {1}  Ferro: {2}  Madeira: {3}", gold, cloth, iron, wood);
        }

        public static void RunWeeklyMaintenance()
        {
            if (!ShouldRunWeeklyMaintenanceNow())
                return;

            for (int cityId = 0; cityId < ReinoElectionsSystem.CityNames.Length; cityId++)
            {
                NormalizeCityPriorities(cityId);
                List<ReinoConstructionRuntimeInfo> active = GetActiveConstructions(cityId);

                active.Sort(delegate (ReinoConstructionRuntimeInfo a, ReinoConstructionRuntimeInfo b)
                {
                    int cmp = GetDisplayPriority(a).CompareTo(GetDisplayPriority(b));
                    if (cmp != 0)
                        return cmp;

                    cmp = GetWeeklyCostWeight(a).CompareTo(GetWeeklyCostWeight(b));
                    if (cmp != 0)
                        return cmp;

                    return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });

                for (int i = 0; i < active.Count; i++)
                {
                    ReinoConstructionRuntimeInfo info = active[i];
                    List<ReinoResourceCost> weekly = GetWeeklyCosts(info);
                    string fail;

                    if (TryConsumeCosts(cityId, weekly, out fail))
                    {
                        RecordWeeklyWages(info);
                        continue;
                    }

                    if (info.Definition != null && info.Definition.RentalTemplates != null && info.Definition.RentalTemplates.Length > 0)
                        continue;

                    if (info.IsArea)
                        ReinoExpansionSystem.ForceDeactivateAreaForMaintenance(info.ReferenceId);
                    else
                        ReinoExpansionSystem.ForceDeactivateLotForMaintenance(info.ReferenceId);
                }
            }
        }

        public static void RunWeeklyMaintenanceNow()
        {
            for (int cityId = 0; cityId < ReinoElectionsSystem.CityNames.Length; cityId++)
            {
                NormalizeCityPriorities(cityId);
                List<ReinoConstructionRuntimeInfo> active = GetActiveConstructions(cityId);

                active.Sort(delegate (ReinoConstructionRuntimeInfo a, ReinoConstructionRuntimeInfo b)
                {
                    int cmp = GetDisplayPriority(a).CompareTo(GetDisplayPriority(b));
                    if (cmp != 0)
                        return cmp;

                    cmp = GetWeeklyCostWeight(a).CompareTo(GetWeeklyCostWeight(b));
                    if (cmp != 0)
                        return cmp;

                    return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });

                for (int i = 0; i < active.Count; i++)
                {
                    ReinoConstructionRuntimeInfo info = active[i];
                    List<ReinoResourceCost> weekly = GetWeeklyCosts(info);
                    string fail;

                    if (TryConsumeCosts(cityId, weekly, out fail))
                    {
                        RecordWeeklyWages(info);
                        continue;
                    }

                    if (info.Definition != null && info.Definition.RentalTemplates != null && info.Definition.RentalTemplates.Length > 0)
                        continue;

                    if (info.IsArea)
                        ReinoExpansionSystem.ForceDeactivateAreaForMaintenance(info.ReferenceId);
                    else
                        ReinoExpansionSystem.ForceDeactivateLotForMaintenance(info.ReferenceId);
                }
            }
        }

        public static void RunWeeklyMaintenanceNow(int cityId)
        {
            NormalizeCityPriorities(cityId);
            List<ReinoConstructionRuntimeInfo> active = GetActiveConstructions(cityId);

            active.Sort(delegate (ReinoConstructionRuntimeInfo a, ReinoConstructionRuntimeInfo b)
            {
                int cmp = GetDisplayPriority(a).CompareTo(GetDisplayPriority(b));
                if (cmp != 0)
                    return cmp;

                cmp = GetWeeklyCostWeight(a).CompareTo(GetWeeklyCostWeight(b));
                if (cmp != 0)
                    return cmp;

                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            for (int i = 0; i < active.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = active[i];
                List<ReinoResourceCost> weekly = GetWeeklyCosts(info);
                string fail;

                if (TryConsumeCosts(cityId, weekly, out fail))
                {
                    RecordWeeklyWages(info);
                    continue;
                }

                if (info.Definition != null && info.Definition.RentalTemplates != null && info.Definition.RentalTemplates.Length > 0)
                    continue;

                if (info.IsArea)
                    ReinoExpansionSystem.ForceDeactivateAreaForMaintenance(info.ReferenceId);
                else
                    ReinoExpansionSystem.ForceDeactivateLotForMaintenance(info.ReferenceId);
            }
        }

        private static int GetWeeklyCostWeight(ReinoConstructionRuntimeInfo info)
        {
            List<ReinoResourceCost> costs = GetWeeklyCosts(info);
            int total = 0;
            for (int i = 0; i < costs.Count; i++)
            {
                if (costs[i] != null)
                    total += costs[i].Amount;
            }
            return total;
        }

        private static void RecordWeeklyWages(ReinoConstructionRuntimeInfo info)
        {
            if (info == null || info.Definition == null)
                return;

            if (String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase))
                ReinoPrisionSystem.ConsumeDynamicWeeklyGold(info.CityId);

            int npcGold = GetNpcCount(info) * Math.Max(0, info.Definition.NpcWeeklySalaryGold);
            int commissionGold = GetCommissionCount(info) * Math.Max(0, GetCommissionWeeklySalaryGold(info));

            if (info.IsArea)
            {
                TouchNpcWageWindow(info.AreaState);
                TouchCommissionWageWindow(info.AreaState);
                info.AreaState.TotalNpcWagesGold += npcGold;
                info.AreaState.NpcWagesCurrentActivationGold += npcGold;
                info.AreaState.NpcWagesLast7DaysGold += npcGold;
                info.AreaState.TotalCommissionWagesGold += commissionGold;
                info.AreaState.CommissionWagesCurrentActivationGold += commissionGold;
                info.AreaState.CommissionWagesLast7DaysGold += commissionGold;
            }
            else
            {
                TouchNpcWageWindow(info.LotState);
                TouchCommissionWageWindow(info.LotState);
                info.LotState.TotalNpcWagesGold += npcGold;
                info.LotState.NpcWagesCurrentActivationGold += npcGold;
                info.LotState.NpcWagesLast7DaysGold += npcGold;
                info.LotState.TotalCommissionWagesGold += commissionGold;
                info.LotState.CommissionWagesCurrentActivationGold += commissionGold;
                info.LotState.CommissionWagesLast7DaysGold += commissionGold;
            }
        }

        public static void RecordRevenueFromNpc(Mobile npc, int goldAmount)
        {
            if (npc == null || npc.Deleted || goldAmount <= 0)
                return;

            ReinoConstructionRuntimeInfo info = FindByNpc(npc);
            if (info != null)
            {
                CreditRevenue(info, goldAmount);
                return;
            }

            int cityId = GetExplicitRevenueCityId(npc);
            if (cityId >= 0)
                ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, goldAmount);
        }

        public static void RecordRevenueFromRentalSign(TownHouseSign sign, int goldAmount)
        {
            if (sign == null || sign.Deleted || goldAmount <= 0)
                return;

            ReinoConstructionRuntimeInfo info = FindByRentalSign(sign);
            if (info != null)
            {
                CreditRevenue(info, goldAmount);
                return;
            }

            int cityId = GetExplicitRevenueCityId(sign);
            if (cityId >= 0)
                ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, goldAmount);
        }

        public static ReinoConstructionRuntimeInfo FindByNpc(Mobile npc)
        {
            if (npc == null || npc.Deleted)
                return null;

            for (int cityId = 0; cityId < ReinoElectionsSystem.CityNames.Length; cityId++)
            {
                List<ReinoConstructionRuntimeInfo> list = GetCityConstructions(cityId);
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoConstructionRuntimeInfo info = list[i];
                    if (info.IsArea || info.LotState == null)
                        continue;

                    if (info.LotState.NpcSerial == npc.Serial.Value)
                        return info;
                }
            }

            return null;
        }

        public static ReinoConstructionRuntimeInfo FindByRentalSign(Item sign)
        {
            if (sign == null || sign.Deleted)
                return null;

            for (int cityId = 0; cityId < ReinoElectionsSystem.CityNames.Length; cityId++)
            {
                List<ReinoConstructionRuntimeInfo> list = GetCityConstructions(cityId);
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoConstructionRuntimeInfo info = list[i];
                    if (info.IsArea || info.LotState == null || info.LotState.RentalSignSerials == null)
                        continue;

                    for (int r = 0; r < info.LotState.RentalSignSerials.Count; r++)
                    {
                        if (info.LotState.RentalSignSerials[r] == sign.Serial.Value)
                            return info;
                    }
                }
            }

            return null;
        }

        private static int GetExplicitRevenueCityId(Mobile npc)
        {
            if (npc == null || npc.Deleted)
                return -1;

            CorreioNPC correio = npc as CorreioNPC;
            if (correio != null && correio.GovernmentCityId >= 0)
                return correio.GovernmentCityId;

            Bibliotecario bibliotecario = npc as Bibliotecario;
            if (bibliotecario != null && bibliotecario.GovernmentCityId >= 0)
                return bibliotecario.GovernmentCityId;

            return -1;
        }

        private static int GetExplicitRevenueCityId(TownHouseSign sign)
        {
            if (sign == null || sign.Deleted)
                return -1;

            if (sign.GovernmentCityId >= 0)
                return sign.GovernmentCityId;

            return -1;
        }

        private static void CreditRevenue(ReinoConstructionRuntimeInfo info, int goldAmount)
        {
            if (info == null || goldAmount <= 0)
                return;

            ReinoExpansionSystem.AddLedgerResource(info.CityId, ReinoResourceType.Gold, goldAmount);

            if (info.IsArea)
            {
                TouchRevenueWindow(info.AreaState);
                info.AreaState.TotalRevenueGold += goldAmount;
                info.AreaState.RevenueCurrentActivationGold += goldAmount;
                info.AreaState.RevenueLast7DaysGold += goldAmount;
            }
            else
            {
                TouchRevenueWindow(info.LotState);
                info.LotState.TotalRevenueGold += goldAmount;
                info.LotState.RevenueCurrentActivationGold += goldAmount;
                info.LotState.RevenueLast7DaysGold += goldAmount;
            }
        }

        private static void TouchRevenueWindow(ReinoLotState state)
        {
            if (state == null)
                return;

            if (state.RevenueWeekStartUtc == DateTime.MinValue)
            {
                state.RevenueWeekStartUtc = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - state.RevenueWeekStartUtc) >= TimeSpan.FromDays(7.0))
            {
                state.RevenueWeekStartUtc = DateTime.UtcNow;
                state.RevenueLast7DaysGold = 0;
            }
        }

        private static void TouchRevenueWindow(ReinoAreaState state)
        {
            if (state == null)
                return;

            if (state.RevenueWeekStartUtc == DateTime.MinValue)
            {
                state.RevenueWeekStartUtc = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - state.RevenueWeekStartUtc) >= TimeSpan.FromDays(7.0))
            {
                state.RevenueWeekStartUtc = DateTime.UtcNow;
                state.RevenueLast7DaysGold = 0;
            }
        }

        private static void TouchNpcWageWindow(ReinoLotState state)
        {
            if (state == null)
                return;

            if (state.NpcWagesWeekStartUtc == DateTime.MinValue)
            {
                state.NpcWagesWeekStartUtc = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - state.NpcWagesWeekStartUtc) >= TimeSpan.FromDays(7.0))
            {
                state.NpcWagesWeekStartUtc = DateTime.UtcNow;
                state.NpcWagesLast7DaysGold = 0;
            }
        }

        private static void TouchNpcWageWindow(ReinoAreaState state)
        {
            if (state == null)
                return;

            if (state.NpcWagesWeekStartUtc == DateTime.MinValue)
            {
                state.NpcWagesWeekStartUtc = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - state.NpcWagesWeekStartUtc) >= TimeSpan.FromDays(7.0))
            {
                state.NpcWagesWeekStartUtc = DateTime.UtcNow;
                state.NpcWagesLast7DaysGold = 0;
            }
        }

        private static void TouchCommissionWageWindow(ReinoLotState state)
        {
            if (state == null)
                return;

            if (state.CommissionWagesWeekStartUtc == DateTime.MinValue)
            {
                state.CommissionWagesWeekStartUtc = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - state.CommissionWagesWeekStartUtc) >= TimeSpan.FromDays(7.0))
            {
                state.CommissionWagesWeekStartUtc = DateTime.UtcNow;
                state.CommissionWagesLast7DaysGold = 0;
            }
        }

        private static void TouchCommissionWageWindow(ReinoAreaState state)
        {
            if (state == null)
                return;

            if (state.CommissionWagesWeekStartUtc == DateTime.MinValue)
            {
                state.CommissionWagesWeekStartUtc = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - state.CommissionWagesWeekStartUtc) >= TimeSpan.FromDays(7.0))
            {
                state.CommissionWagesWeekStartUtc = DateTime.UtcNow;
                state.CommissionWagesLast7DaysGold = 0;
            }
        }
    }
}
