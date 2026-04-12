using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonSelectLeftLotBase = 1000;
        private const int ButtonSelectRightLotBase = 10000;
        private const int ButtonSelectBuildingBase = 30000;
        private const int ButtonConstructionPrev = 40001;
        private const int ButtonConstructionConfirm = 40002;
        private const int ButtonConstructionNext = 40003;

        private void BuildExpansionPage()
        {
            AddPage(1);

            List<ReinoLotDefinition> leftLots = ReinoExpansionSystem.GetVisibleLeftLotsForCity(m_CityId);
            List<ReinoLotDefinition> rightLots = ReinoExpansionSystem.GetUnavailableLotsForCity(m_CityId);

            ReinoLotDefinition selectedLot = ReinoExpansionSystem.GetLotDefinition(m_SelectedLotId);
            ReinoLotState selectedLotState = ReinoExpansionSystem.GetLotState(m_SelectedLotId);

            AddLabel(789, 173, 0, @"Expansão");
            AddLabel(554, 232, 0, @"Lotes Disponíveis");
            AddLabel(980, 232, 0, @"Lotes Não Disponíveis");
            AddImageTiled(407, 261, 825, 5, 367);
            AddImageTiled(814, 225, 6, 258, 365);
            AddImageTiled(407, 483, 825, 5, 367);
            AddLabel(482, 496, 0, @"Construções");

            int[] leftY = new int[] { 278, 303, 328, 352 };
            for (int i = 0; i < leftLots.Count && i < leftY.Length; i++)
            {
                ReinoLotDefinition lot = leftLots[i];
                ReinoLotState state = ReinoExpansionSystem.GetLotState(lot.LotId);

                AddButton(421, leftY[i] + 3, 536, 435, ButtonSelectLeftLotBase + lot.LotId, Server.Gumps.GumpButtonType.Reply, 0);
                AddLabel(449, leftY[i], 0, ReinoExpansionSystem.GetLotListLabel(lot, state));
            }

            int[] rightColumn1Y = new int[] { 278, 303, 328, 352, 377, 402 };
            int[] rightColumn2Y = new int[] { 278, 303, 328 };

            for (int i = 0; i < rightLots.Count && i < rightColumn1Y.Length; i++)
            {
                ReinoLotDefinition lot = rightLots[i];
                AddButton(854, rightColumn1Y[i] + 3, 536, 435, ButtonSelectRightLotBase + lot.LotId, Server.Gumps.GumpButtonType.Reply, 0);
                AddLabel(882, rightColumn1Y[i], 0, lot.Name);
            }

            for (int i = rightColumn1Y.Length; i < rightLots.Count && (i - rightColumn1Y.Length) < rightColumn2Y.Length; i++)
            {
                int index = i - rightColumn1Y.Length;
                ReinoLotDefinition lot = rightLots[i];
                AddButton(1076, rightColumn2Y[index] + 3, 536, 435, ButtonSelectRightLotBase + lot.LotId, Server.Gumps.GumpButtonType.Reply, 0);
                AddLabel(1104, rightColumn2Y[index], 0, lot.Name);
            }

            string infoHtml = "<BASEFONT COLOR=#000000>Selecione um lote à esquerda para ver as construções possíveis, ou um lote à direita para ver o objetivo necessário para limpá-lo.</BASEFONT>";
            List<ReinoConstructionDefinition> buildingOptions = new List<ReinoConstructionDefinition>();
            bool showConfirm = false;
            bool showPrev = false;
            bool showNext = false;

            if (selectedLot != null && selectedLotState != null)
            {
                if (selectedLotState.Status == ReinoLotStatus.Locked)
                {
                    infoHtml = ReinoExpansionDefinitions.FormatObjectiveHtml(selectedLot, selectedLotState);
                }
                else if (selectedLotState.Status == ReinoLotStatus.Available)
                {
                    buildingOptions = ReinoExpansionDefinitions.GetBuildingsForLot(selectedLot);
                    showConfirm = true;
                }
                else if (selectedLotState.Status == ReinoLotStatus.Abandoned)
                {
                    ReinoConstructionDefinition only = ReinoExpansionDefinitions.GetBuilding(selectedLotState.ConstructionId);
                    if (only != null)
                    {
                        buildingOptions.Add(only);
                        showConfirm = true;
                    }
                }
                else if (selectedLotState.Status == ReinoLotStatus.UnderConstruction || selectedLotState.Status == ReinoLotStatus.Active)
                {
                    ReinoConstructionDefinition built = ReinoExpansionDefinitions.GetBuilding(selectedLotState.ConstructionId);
                    if (built != null)
                        infoHtml = ReinoExpansionDefinitions.FormatConstructionHtml(built);
                }
            }

            if (buildingOptions.Count > 0)
            {
                int pageCount = (buildingOptions.Count + 5) / 6;
                int page = m_BuildingPage;

                if (page < 0)
                    page = 0;
                if (page >= pageCount)
                    page = pageCount - 1;

                int start = page * 6;
                int[] buildY = new int[] { 533, 558, 583, 607, 630, 655 };

                for (int i = 0; i < 6; i++)
                {
                    int realIndex = start + i;
                    if (realIndex >= buildingOptions.Count)
                        break;

                    ReinoConstructionDefinition def = buildingOptions[realIndex];
                    AddButton(486, buildY[i] + 3, 536, 435, ButtonSelectBuildingBase + i, Server.Gumps.GumpButtonType.Reply, 0);
                    AddLabel(514, buildY[i], 0, def.Name);
                }

                ReinoConstructionDefinition selectedBuilding = null;

                if (!String.IsNullOrWhiteSpace(m_SelectedBuildingId))
                    selectedBuilding = ReinoExpansionDefinitions.GetBuilding(m_SelectedBuildingId);

                if (selectedBuilding == null && start < buildingOptions.Count)
                    selectedBuilding = buildingOptions[start];

                if (selectedBuilding != null)
                    infoHtml = ReinoExpansionDefinitions.FormatConstructionHtml(selectedBuilding);

                showPrev = page > 0;
                showNext = (start + 6) < buildingOptions.Count;
            }

            AddHtml(686, 503, 434, 186, infoHtml, false, false);

            if (showPrev)
                AddButton(399, 584, 453, 453, ButtonConstructionPrev, Server.Gumps.GumpButtonType.Reply, 0);

            if (showNext)
                AddButton(648, 584, 452, 452, ButtonConstructionNext, Server.Gumps.GumpButtonType.Reply, 0);

            if (showConfirm)
                AddButton(1141, 580, 492, 492, ButtonConstructionConfirm, Server.Gumps.GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            int button = info.ButtonID;

            if (button >= 1 && button <= 12)
            {
                if (button != 5)
                {
                    string pageMessage;
                    if (!ReinoEmploymentSystem.CanUseGovernmentPage(from, m_CityId, button, out pageMessage))
                    {
                        if (!String.IsNullOrWhiteSpace(pageMessage))
                            from.SendMessage(pageMessage);

                        from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                        return;
                    }
                }

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, button));
                return;
            }

            if (HandleGeneralResponse(from, info))
                return;

            if (HandleTreasuryResponse(from, info))
                return;

            if (HandleMaintenanceResponse(from, info))
                return;

            if (HandleGovernmentResponse(from, info))
                return;

            if (HandleDiplomacyResponse(from, info))
                return;

            if (HandleVisualResponse(from, info))
                return;

            if (HandleMilitaryResponse(from, info))
                return;

            if (button >= ButtonSelectLeftLotBase && button < ButtonSelectRightLotBase)
            {
                int lotId = button - ButtonSelectLeftLotBase;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, lotId, -1, String.Empty, 0, 4));
                return;
            }

            if (button >= ButtonSelectRightLotBase && button < ButtonSelectBuildingBase)
            {
                int lotId = button - ButtonSelectRightLotBase;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, lotId, -1, String.Empty, 0, 4));
                return;
            }

            if (button >= ButtonSelectBuildingBase && button < ButtonConstructionPrev)
            {
                int slot = button - ButtonSelectBuildingBase;
                List<ReinoConstructionDefinition> options = GetCurrentBuildingOptions();
                int start = m_BuildingPage * 6;
                int realIndex = start + slot;

                if (realIndex >= 0 && realIndex < options.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, -1, options[realIndex].Id, m_BuildingPage, 4));
                else
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, -1, m_SelectedBuildingId, m_BuildingPage, 4));

                return;
            }

            if (button == ButtonConstructionPrev)
            {
                int page = m_BuildingPage - 1;
                if (page < 0)
                    page = 0;

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, -1, m_SelectedBuildingId, page, 4));
                return;
            }

            if (button == ButtonConstructionNext)
            {
                int page = m_BuildingPage + 1;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, -1, m_SelectedBuildingId, page, 4));
                return;
            }

            if (button == ButtonConstructionConfirm)
            {
                string message;
                string buildingId = m_SelectedBuildingId;
                List<ReinoConstructionDefinition> options = GetCurrentBuildingOptions();

                if (String.IsNullOrWhiteSpace(buildingId) && options.Count > 0)
                {
                    int start = m_BuildingPage * 6;
                    if (start < 0 || start >= options.Count)
                        start = 0;

                    buildingId = options[start].Id;
                }

                if (m_SelectedLotId > 0)
                    ReinoExpansionSystem.TryConfirmLotConstruction(from, m_CityId, m_SelectedLotId, buildingId, out message);
                else
                    message = "Selecione um lote primeiro.";

                from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, -1, buildingId, m_BuildingPage, 4));
            }
        }

        private List<ReinoConstructionDefinition> GetCurrentBuildingOptions()
        {
            List<ReinoConstructionDefinition> list = new List<ReinoConstructionDefinition>();

            if (m_SelectedLotId > 0)
            {
                ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(m_SelectedLotId);
                ReinoLotState st = ReinoExpansionSystem.GetLotState(m_SelectedLotId);

                if (lot != null && st != null)
                {
                    if (st.Status == ReinoLotStatus.Available)
                        list = ReinoExpansionDefinitions.GetBuildingsForLot(lot);
                    else if (st.Status == ReinoLotStatus.Abandoned || st.Status == ReinoLotStatus.Active || st.Status == ReinoLotStatus.UnderConstruction)
                    {
                        ReinoConstructionDefinition built = ReinoExpansionDefinitions.GetBuilding(st.ConstructionId);
                        if (built != null)
                            list.Add(built);
                    }
                }
            }

            return list;
        }
    }
}
