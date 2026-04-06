using System;
using System.Collections.Generic;
using Server.Custom.Systems.Postos;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonMaintenanceActiveBase = 50000;
        private const int ButtonMaintenanceInactiveBase = 50100;
        private const int ButtonMaintenancePostoActiveBase = 50200;
        private const int ButtonMaintenancePostoDisputeBase = 50300;
        private const int ButtonMaintenanceToggle = 50400;
        private const int ButtonMaintenancePriorityDown = 50401;
        private const int ButtonMaintenancePriorityUp = 50402;
        private const int ButtonMaintenanceDemolish = 50403;
        private const int ButtonMaintenanceResetPosto = 50404;

        private void BuildMaintenancePage()
        {
            AddPage(1);
            AddLabel(757, 173, 0, @"Manutenção");
            AddImageTiled(407, 271, 819, 5, 367);
            AddImageTiled(580, 226, 6, 470, 365);
            AddImageTiled(785, 226, 6, 470, 365);
            AddImageTiled(1016, 226, 6, 470, 365);
            AddImageTiled(802, 540, 201, 5, 367);
            AddImageTiled(1038, 451, 201, 5, 367);
            AddImageTiled(1038, 631, 201, 5, 367);

            AddLabel(416, 237, 0, @"Construções Ativas");
            AddLabel(615, 237, 0, @"Construções Inativas");
            AddLabel(818, 237, 0, @"Postos de Materiais Ativos");
            AddLabel(838, 516, 0, @"Postos em Disputa:");
            AddLabel(1037, 237, 0, @"Custo de Construções Ativas");
            AddLabel(1046, 423, 0, @"Custo para Ativar Construções");
            AddLabel(1063, 648, 0, @"Tempo de Funcionamento:");

            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);
            List<ReinoConstructionRuntimeInfo> inactive = ReinoMaintenanceSystem.GetInactiveConstructions(m_CityId);
            List<PostoDefinition> activePostos = ReinoMaintenanceSystem.GetActivePostos(m_CityId);
            List<PostoDefinition> disputePostos = ReinoMaintenanceSystem.GetDisputedPostos(m_CityId);

            int[] leftY = new int[] { 290, 315, 339, 364, 389, 414 };
            int[] midY = new int[] { 291, 316, 340, 365, 390, 415 };
            int[] postoY = new int[] { 289, 314, 338, 363, 388 };
            int[] disputeY = new int[] { 562, 587, 611 };

            for (int i = 0; i < active.Count && i < leftY.Length; i++)
            {
                string label = active[i].Name;

                if (active[i].Status == ReinoLotStatus.UnderConstruction)
                    label += "";

                AddButton(413, leftY[i] + 3, 536, 435, ButtonMaintenanceActiveBase + i, GumpButtonType.Reply, 0);
                AddLabel(437, leftY[i], 0, label);
                AddLabel(557, leftY[i], 0, ReinoMaintenanceSystem.GetDisplayPriority(active[i]).ToString());
            }

            for (int i = 0; i < inactive.Count && i < midY.Length; i++)
            {
                AddButton(613, midY[i] + 3, 536, 435, ButtonMaintenanceInactiveBase + i, GumpButtonType.Reply, 0);
                AddLabel(637, midY[i], 0, inactive[i].Name);
            }

            for (int i = 0; i < activePostos.Count && i < postoY.Length; i++)
            {
                AddButton(816, postoY[i] + 3, 536, 435, ButtonMaintenancePostoActiveBase + i, GumpButtonType.Reply, 0);
                AddLabel(840, postoY[i], 0, activePostos[i].Name);
            }

            for (int i = 0; i < disputePostos.Count && i < disputeY.Length; i++)
            {
                AddButton(817, disputeY[i] + 3, 536, 435, ButtonMaintenancePostoDisputeBase + i, GumpButtonType.Reply, 0);
                AddLabel(841, disputeY[i], 0, disputePostos[i].Name);
            }

            DrawCostBlock(1044, 295, ReinoMaintenanceSystem.GetTotalActiveWeeklyCost(m_CityId));
            DrawCostBlock(1044, 469, ReinoMaintenanceSystem.GetTotalInactiveActivationCost(m_CityId));

            int weeks = ReinoMaintenanceSystem.GetWeeksOfOperationRemaining(m_CityId);
            AddLabel(1103, 673, 0, weeks == ReinoMaintenanceSystem.WeeksInfinite ? @"Permanente" : (weeks + @" semanas"));
        }

        private void DrawCostBlock(int x, int y, List<ReinoResourceCost> costs)
        {
            int gold = GetCostAmount(costs, ReinoResourceType.Gold);
            int cloth = GetCostAmount(costs, ReinoResourceType.Cloth);
            int iron = GetCostAmount(costs, ReinoResourceType.Iron);
            int wood = GetCostAmount(costs, ReinoResourceType.Wood);

            AddLabel(x, y, 0, @"Moedas: " + gold);
            AddLabel(x, y + 25, 0, @"Tecidos: " + cloth);
            AddLabel(x, y + 49, 0, @"Ferro: " + iron);
            AddLabel(x, y + 74, 0, @"Madeira: " + wood);
        }

        private int GetCostAmount(List<ReinoResourceCost> costs, ReinoResourceType type)
        {
            int amount = 0;
            if (costs == null)
                return 0;

            for (int i = 0; i < costs.Count; i++)
            {
                ReinoResourceCost cost = costs[i];
                if (cost != null && cost.Type == type)
                    amount += cost.Amount;
            }

            return amount;
        }

        private void BuildConstructionDetailPage()
        {
            AddPage(1);

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(m_SelectedBuildingId);
            string title = info != null ? info.Name : @"Construção";
            AddLabel(785, 173, 0, title);

            AddImageTiled(408, 259, 819, 5, 367);
            AddImageTiled(408, 366, 819, 5, 367);
            AddImageTiled(408, 449, 819, 5, 367);
            AddImageTiled(408, 563, 819, 5, 367);
            AddImageTiled(714, 273, 6, 19, 365);
            AddImageTiled(887, 273, 6, 19, 365);
            AddImageTiled(1054, 273, 6, 19, 365);
            AddImageTiled(806, 587, 6, 103, 365);

            if (info == null)
            {
                AddHtml(430, 250, 760, 390, @"<BASEFONT COLOR=#000000>Selecione uma construção na página de manutenção para ver os detalhes dela aqui.</BASEFONT>", false, false);
                return;
            }

            List<ReinoResourceCost> maintenance = new List<ReinoResourceCost>();
            if (info.Definition != null && info.Definition.MaintenanceCosts != null)
            {
                for (int i = 0; i < info.Definition.MaintenanceCosts.Length; i++)
                {
                    ReinoResourceCost cost = info.Definition.MaintenanceCosts[i];
                    if (cost != null)
                        maintenance.Add(new ReinoResourceCost(cost.Type, cost.Amount));
                }
            }

            bool active = info.Status == ReinoLotStatus.Active;
            int toggleArt = active ? 541 : 543;
            string toggleText = active ? @"Desativar Construção" : @"Ativar Construção";
            DateTime openSince = ReinoMaintenanceSystem.GetLastActivatedUtc(info);
            int priority = ReinoMaintenanceSystem.GetDisplayPriority(info);
            int npcCount = ReinoMaintenanceSystem.GetNpcCount(info);
            int npcSalary = info.Definition != null ? Math.Max(0, info.Definition.NpcWeeklySalaryGold) : 0;
            int npcWeeklyTotal = ReinoMaintenanceSystem.GetNpcWeeklyTotalGold(info);
            int commissionCount = ReinoMaintenanceSystem.GetCommissionCount(info);
            int commissionWeekly = ReinoMaintenanceSystem.GetCommissionWeeklySalaryGold(info);
            int commissionWeeklyTotal = ReinoMaintenanceSystem.GetCommissionWeeklyTotalGold(info);
            int recurringWeekly = ReinoMaintenanceSystem.GetCurrentRecurringRevenueGold(info);
            int revenueThisWeek = ReinoMaintenanceSystem.GetRevenueLast7DaysGold(info);
            int totalRevenue = ReinoMaintenanceSystem.GetTotalRevenueGold(info);
            int operatingWeeks = ReinoMaintenanceSystem.GetOperatingWeeks(info);
            int netWeekly = ReinoMaintenanceSystem.GetNetWeeklyGold(info);

            bool canToggle = info.Definition != null && info.Definition.AllowManualActivationToggle;
            bool canDemolish = !info.IsArea && info.Definition != null && (info.Definition.RentalTemplates == null || info.Definition.RentalTemplates.Length == 0);

            AddLabel(410, 225, 0, @"Status:");
            if (canToggle)
            {
                AddButton(712, 230, toggleArt, toggleArt, ButtonMaintenanceToggle, GumpButtonType.Reply, 0);
                AddLabel(740, 228, 0, toggleText);
            }

            AddLabel(410, 272, 0, @"Valor de Manutenção Semanal:");
            AddLabel(410, 297, 0, @"Rende Semanalmente:");
            AddLabel(410, 322, 0, @"Rendeu Nesta Semana:");
            AddLabel(410, 387, 0, @"Número de Npcs:");
            AddLabel(410, 412, 0, @"Valor por Npc:");
            AddLabel(410, 469, 0, @"Funcionando Desde:");
            AddLabel(410, 496, 0, @"Tempo de Funcionamento:");
            AddLabel(410, 523, 0, @"Rendimento Total:");
            AddLabel(410, 583, 0, @"Cargos Comissionados:");
            AddLabel(410, 610, 0, @"Salários:");
            AddLabel(410, 637, 0, @"Valor Total de Salários:");
            AddLabel(410, 664, 0, @"Rendimento Líquido:");

            AddLabel(728, 272, 0, @"Ferro: " + GetCostAmount(maintenance, ReinoResourceType.Iron));
            AddLabel(906, 272, 0, @"Madeira: " + GetCostAmount(maintenance, ReinoResourceType.Wood));
            AddLabel(1077, 273, 0, @"Tecido: " + GetCostAmount(maintenance, ReinoResourceType.Cloth));

            AddLabel(465, 225, 0, active ? @"Ativa" : @"Inativa");
            AddLabel(620, 272, 0, ReinoMaintenanceSystem.GetBaseMaintenanceGoldOnly(info) + @" moedas");
            AddLabel(620, 297, 0, recurringWeekly + @" moedas");
            AddLabel(620, 322, 0, revenueThisWeek + @" moedas");
            AddLabel(620, 387, 0, npcCount.ToString());
            AddLabel(620, 412, 0, npcSalary + @" moedas");
            AddLabel(620, 469, 0, openSince == DateTime.MinValue ? @"Nunca" : openSince.ToString("dd/MM/yyyy HH:mm"));
            AddLabel(620, 496, 0, operatingWeeks + @" semanas");
            AddLabel(620, 523, 0, totalRevenue + @" moedas");
            AddLabel(620, 583, 0, commissionCount.ToString());
            AddLabel(620, 610, 0, commissionWeekly + @" moedas");
            AddLabel(620, 637, 0, commissionWeeklyTotal + @" moedas");
            AddLabel(620, 664, 0, netWeekly + @" moedas");

            if (canDemolish)
            {
                AddButton(1037, 230, 533, 533, ButtonMaintenanceDemolish, GumpButtonType.Reply, 0);
                AddLabel(1065, 228, 0, m_DemolishConfirm ? @"Confirmar Demolição" : @"Demolir Construção");
            }

            AddLabel(950, 596, 0, @"Prioridade de Funcionamento");
            AddButton(964, 640, 583, 248, ButtonMaintenancePriorityDown, GumpButtonType.Reply, 0);
            AddButton(1090, 638, 582, 248, ButtonMaintenancePriorityUp, GumpButtonType.Reply, 0);
            AddLabel(1034, 642, 0, priority.ToString());
        }

        private void BuildPostoDetailPage()
        {
            AddPage(1);

            string postoId = m_SelectedBuildingId != null && m_SelectedBuildingId.StartsWith("P:", StringComparison.OrdinalIgnoreCase) ? m_SelectedBuildingId.Substring(2) : String.Empty;
            PostoDefinition def = !String.IsNullOrWhiteSpace(postoId) ? PostoSystem.GetDefinition(postoId) : null;
            PostoState state = def != null ? PostoSystem.GetState(def.Id) : null;

            AddLabel(772, 173, 0, def != null ? def.Name : @"Posto");
            AddImageTiled(408, 366, 819, 5, 367);
            AddImageTiled(408, 259, 819, 5, 367);
            AddImageTiled(408, 533, 819, 5, 367);

            if (def == null || state == null)
            {
                AddHtml(430, 250, 760, 390, @"<BASEFONT COLOR=#000000>Selecione um posto na página de manutenção para ver os detalhes dele aqui.</BASEFONT>", false, false);
                return;
            }

            bool inContest = PostoSystem.IsContestActive(state);
            string status = inContest ? @"Em disputa" : @"Ativo";
            DateTime conqueredSince = PostoSystem.GetLastConqueredUtc(def.Id);
            DateTime lastDispatchUtc = PostoSystem.GetLastDispatchUtc(def.Id);
            int totalGenerated = PostoSystem.GetTotalGeneratedCurrentOwner(def.Id);
            int stored = PostoSystem.GetStoredAmount(def.Id);
            int lastDispatch = PostoSystem.GetLastDispatchAmount(def.Id);
            string dispatchers = PostoSystem.GetCurrentDispatcherNames(def.Id);
            string previousOwner = PostoSystem.GetPreviousOwnerLabel(def.Id);
            string donatedBy = PostoSystem.GetDonatedByLabel(def.Id);
            string chestLoc = PostoSystem.GetChestLocationLabel(def.Id);
            string previousHeld = PostoSystem.GetPreviousOwnerHeldLabel(def.Id);
            string averageHeld = PostoSystem.GetAverageOwnershipLabel(def.Id);

            AddLabel(410, 225, 0, @"Status:");
            AddButton(1037, 230, 533, 533, ButtonMaintenanceResetPosto, GumpButtonType.Reply, 0);
            AddLabel(1065, 228, 0, @"Desapropriar Posto");

            AddLabel(410, 278, 0, @"Material:");
            AddLabel(410, 303, 0, @"Gera Diariamente:");
            AddLabel(410, 328, 0, @"Total Gerado:");
            AddLabel(410, 387, 0, @"Conquistado Desde:");
            AddLabel(410, 412, 0, @"Atualmente Armazenando:");
            AddLabel(410, 437, 0, @"Última Retirada:");
            AddLabel(410, 462, 0, @"Despachante:");
            AddLabel(410, 487, 0, @"Data da Última Retirada:");
            AddLabel(410, 552, 0, @"Conquistado De:");
            AddLabel(410, 577, 0, @"Doado Por:");
            AddLabel(410, 602, 0, @"Localização do Posto:");
            AddLabel(410, 627, 0, @"Tempo Anterior de Posse:");
            AddLabel(410, 652, 0, @"Tempo Médio de Posse:");

            AddLabel(620, 225, 0, status);
            AddLabel(620, 278, 0, PostoSystem.GetResourceDisplayName(def.ResourceType));
            AddLabel(620, 303, 0, def.DailyYield + @" por dia");
            AddLabel(620, 328, 0, totalGenerated + @" unidades");
            AddLabel(620, 387, 0, conqueredSince == DateTime.MinValue ? @"-" : conqueredSince.ToString("dd/MM/yyyy HH:mm"));
            AddLabel(620, 412, 0, stored.ToString());
            AddLabel(620, 437, 0, lastDispatch + @" unidades");
            AddHtml(620, 460, 560, 38, @"<BASEFONT COLOR=#000000>" + dispatchers + @"</BASEFONT>", false, false);
            AddLabel(620, 487, 0, lastDispatchUtc == DateTime.MinValue ? @"-" : lastDispatchUtc.ToString("dd/MM/yyyy HH:mm"));
            AddLabel(620, 552, 0, previousOwner);
            AddLabel(620, 577, 0, donatedBy);
            AddLabel(620, 602, 0, chestLoc);
            AddLabel(620, 627, 0, previousHeld);
            AddLabel(620, 652, 0, averageHeld);
        }

        private bool HandleMaintenanceResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;

            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);
            List<ReinoConstructionRuntimeInfo> inactive = ReinoMaintenanceSystem.GetInactiveConstructions(m_CityId);
            List<PostoDefinition> activePostos = ReinoMaintenanceSystem.GetActivePostos(m_CityId);
            List<PostoDefinition> disputePostos = ReinoMaintenanceSystem.GetDisputedPostos(m_CityId);

            if (button >= ButtonMaintenanceActiveBase && button < ButtonMaintenanceActiveBase + 100)
            {
                int index = button - ButtonMaintenanceActiveBase;
                if (index >= 0 && index < active.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, active[index].Key, m_BuildingPage, 9));
                return true;
            }

            if (button >= ButtonMaintenanceInactiveBase && button < ButtonMaintenanceInactiveBase + 100)
            {
                int index = button - ButtonMaintenanceInactiveBase;
                if (index >= 0 && index < inactive.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, inactive[index].Key, m_BuildingPage, 9));
                return true;
            }

            if (button >= ButtonMaintenancePostoActiveBase && button < ButtonMaintenancePostoActiveBase + 100)
            {
                int index = button - ButtonMaintenancePostoActiveBase;
                if (index >= 0 && index < activePostos.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, "P:" + activePostos[index].Id, m_BuildingPage, 10));
                return true;
            }

            if (button >= ButtonMaintenancePostoDisputeBase && button < ButtonMaintenancePostoDisputeBase + 100)
            {
                int index = button - ButtonMaintenancePostoDisputeBase;
                if (index >= 0 && index < disputePostos.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, "P:" + disputePostos[index].Id, m_BuildingPage, 10));
                return true;
            }

            if (button == ButtonMaintenanceToggle)
            {
                string message;
                ReinoMaintenanceSystem.TryToggleActivation(from, m_CityId, m_SelectedBuildingId, out message);
                if (!String.IsNullOrWhiteSpace(message))
                    from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 9));
                return true;
            }

            if (button == ButtonMaintenancePriorityDown)
            {
                string message;
                ReinoMaintenanceSystem.TryShiftPriority(from, m_CityId, m_SelectedBuildingId, +1, out message);
                if (!String.IsNullOrWhiteSpace(message))
                    from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 9));
                return true;
            }

            if (button == ButtonMaintenancePriorityUp)
            {
                string message;
                ReinoMaintenanceSystem.TryShiftPriority(from, m_CityId, m_SelectedBuildingId, -1, out message);
                if (!String.IsNullOrWhiteSpace(message))
                    from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 9));
                return true;
            }

            if (button == ButtonMaintenanceDemolish)
            {
                if (!m_DemolishConfirm)
                {
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 9, true));
                    return true;
                }

                string message;
                ReinoMaintenanceSystem.TryDemolishConstruction(from, m_CityId, m_SelectedBuildingId, out message);
                if (!String.IsNullOrWhiteSpace(message))
                    from.SendMessage(message);

                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, m_SelectedWallAreaId, String.Empty, 0, 3));
                return true;
            }

            if (button == ButtonMaintenanceResetPosto)
            {
                string postoId = m_SelectedBuildingId != null && m_SelectedBuildingId.StartsWith("P:", StringComparison.OrdinalIgnoreCase) ? m_SelectedBuildingId.Substring(2) : String.Empty;

                if (!String.IsNullOrWhiteSpace(postoId))
                {
                    if (!ReinoAccessHelper.HasGovernmentAccess(from, m_CityId))
                        from.SendMessage("Somente o governador pode desapropriar postos.");
                    else
                    {
                        string message;
                        PostoSystem.ResetPosto(postoId, out message);
                        if (!String.IsNullOrWhiteSpace(message))
                            from.SendMessage(message);
                    }
                }

                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, m_SelectedWallAreaId, String.Empty, 0, 3));
                return true;
            }

            return false;
        }
    }
}
