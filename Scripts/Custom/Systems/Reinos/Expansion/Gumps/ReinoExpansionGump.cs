using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Systems.Reinos
{
    public class ReinoExpansionGump : Gump
    {
        private const int ButtonSelectLeftLotBase = 1000;
        private const int ButtonSelectRightLotBase = 10000;
        private const int ButtonSelectWallBase = 20000;
        private const int ButtonSelectBuildingBase = 30000;
        private const int ButtonConstructionPrev = 40001;
        private const int ButtonConstructionConfirm = 40002;
        private const int ButtonConstructionNext = 40003;

        private readonly PlayerMobile m_From;
        private readonly int m_CityId;
        private readonly int m_SelectedLotId;
        private readonly int m_SelectedWallAreaId;
        private readonly string m_SelectedBuildingId;
        private readonly int m_BuildingPage;

        public ReinoExpansionGump(PlayerMobile from, int cityId)
            : this(from, cityId, -1, -1, String.Empty, 0)
        {
        }

        public ReinoExpansionGump(PlayerMobile from, int cityId, int selectedLotId, int selectedWallAreaId, string selectedBuildingId, int buildingPage)
            : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;
            m_SelectedLotId = selectedLotId;
            m_SelectedWallAreaId = selectedWallAreaId;
            m_SelectedBuildingId = selectedBuildingId ?? String.Empty;
            m_BuildingPage = buildingPage < 0 ? 0 : buildingPage;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            BuildBase();
            BuildExpansionPage();
        }

        private void BuildBase()
        {
            AddPage(0);
            AddImageTiled(174, 151, 211, 565, 377);
            AddImageTiled(385, 146, 870, 565, 384);
            AddImageTiled(154, 645, 78, 89, 359);
            AddImageTiled(1207, 645, 74, 90, 360);
            AddImageTiled(1207, 128, 74, 82, 361);
            AddImageTiled(154, 128, 74, 90, 362);
            AddImageTiled(158, 208, 26, 441, 365);
            AddImageTiled(1252, 205, 26, 441, 366);
            AddImageTiled(223, 704, 985, 31, 367);
            AddImageTiled(220, 130, 989, 31, 368);
            AddImageTiled(1050, 193, 184, 21, 470);
            AddImageTiled(203, 193, 178, 21, 469);
            AddImageTiled(255, 193, 870, 21, 471);
            AddImageTiled(380, 170, 6, 524, 365);

            AddLabel(253, 170, 0, "Governo");
            AddLabel(250, 231, 0, "Expansão");
            AddButton(211, 225, 439, 439, 0, GumpButtonType.Reply, 0);

            AddLabel(250, 270, 0, "Visão Geral");
            AddLabel(250, 310, 0, "Tesouro");
            AddLabel(250, 349, 0, "Manutenção");
            AddLabel(250, 390, 0, "Empregados");
            AddLabel(250, 429, 0, "Diplomacia");
            AddLabel(250, 469, 0, "Visual");
            AddLabel(250, 508, 0, "Slot 1");
            AddLabel(250, 548, 0, "Slot 2");
            AddLabel(250, 587, 0, "Slot 3");
            AddLabel(250, 627, 0, "Slot 4");
            AddLabel(250, 666, 0, "Slot 5");
        }

        private void BuildExpansionPage()
        {
            List<ReinoLotDefinition> leftLots = ReinoExpansionSystem.GetVisibleLeftLotsForCity(m_CityId);
            List<ReinoAreaDefinition> wallAreas = ReinoExpansionSystem.GetVisibleWallAreasForCity(m_CityId);
            List<ReinoLotDefinition> rightLots = ReinoExpansionSystem.GetUnavailableLotsForCity(m_CityId);

            ReinoLotDefinition selectedLot = ReinoExpansionSystem.GetLotDefinition(m_SelectedLotId);
            ReinoLotState selectedLotState = ReinoExpansionSystem.GetLotState(m_SelectedLotId);
            ReinoAreaDefinition selectedWall = ReinoExpansionSystem.GetAreaDefinition(m_SelectedWallAreaId);
            ReinoAreaState selectedWallState = ReinoExpansionSystem.GetAreaState(m_SelectedWallAreaId);

            AddLabel(789, 173, 0, "Expansão");
            AddLabel(554, 232, 0, "Lotes Disponíveis");
            AddLabel(980, 232, 0, "Lotes Não Disponíveis");
            AddImageTiled(407, 261, 825, 5, 367);
            AddImageTiled(814, 225, 6, 470, 365);
            AddImageTiled(407, 483, 825, 5, 367);
            AddLabel(554, 456, 0, "Construções");
            AddLabel(1001, 455, 0, "Para Limpar");

            int yLeft = 278;
            int leftIndex = 0;

            for (int i = 0; i < leftLots.Count && leftIndex < 8; i++, leftIndex++)
            {
                ReinoLotDefinition lot = leftLots[i];
                ReinoLotState st = ReinoExpansionSystem.GetLotState(lot.LotId);
                AddButton(421, yLeft + (leftIndex * 25) + 3, 536, 435, ButtonSelectLeftLotBase + lot.LotId, GumpButtonType.Reply, 0);
                AddLabel(449, yLeft + (leftIndex * 25), 0, ReinoExpansionSystem.GetLotListLabel(lot, st));
            }

            for (int i = 0; i < wallAreas.Count && leftIndex < 8; i++, leftIndex++)
            {
                ReinoAreaDefinition area = wallAreas[i];
                ReinoAreaState st = ReinoExpansionSystem.GetAreaState(area.AreaId);
                AddButton(421, yLeft + (leftIndex * 25) + 3, 536, 435, ButtonSelectWallBase + area.AreaId, GumpButtonType.Reply, 0);
                AddLabel(449, yLeft + (leftIndex * 25), 0, ReinoExpansionSystem.GetWallAreaLabel(area, st));
            }

            int yRight = 278;
            for (int i = 0; i < rightLots.Count && i < 8; i++)
            {
                ReinoLotDefinition lot = rightLots[i];
                AddButton(854, yRight + (i * 25) + 3, 536, 435, ButtonSelectRightLotBase + lot.LotId, GumpButtonType.Reply, 0);
                AddLabel(882, yRight + (i * 25), 0, lot.Name);
            }

            string leftHtml = "<BASEFONT COLOR=#000000>Selecione um lote disponível à esquerda para escolher uma construção, ou um lote à direita para ver o que falta limpar.</BASEFONT>";
            string rightHtml = String.Empty;
            List<ReinoConstructionDefinition> buildingOptions = new List<ReinoConstructionDefinition>();
            int pageCount = 1;
            bool showConfirm = false;

            if (selectedLot != null && selectedLotState != null)
            {
                if (selectedLotState.Status == ReinoLotStatus.Locked)
                {
                    leftHtml = "<BASEFONT COLOR=#000000>Esse terreno ainda não está pronto para construir. Veja à direita o que o reino precisa fazer.</BASEFONT>";
                    rightHtml = ReinoExpansionDefinitions.FormatObjectiveHtml(selectedLot, selectedLotState);
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
                        rightHtml = "<BASEFONT COLOR=#000000>Essa construção está abandonada. Se você confirmar, o reino iniciará a reativação.</BASEFONT>";
                    }
                }
                else if (selectedLotState.Status == ReinoLotStatus.UnderConstruction || selectedLotState.Status == ReinoLotStatus.Active)
                {
                    ReinoConstructionDefinition built = ReinoExpansionDefinitions.GetBuilding(selectedLotState.ConstructionId);
                    if (built != null)
                    {
                        leftHtml = ReinoExpansionDefinitions.FormatConstructionHtml(built);
                        rightHtml = "<BASEFONT COLOR=#000000><B>Status:</B> " + ReinoExpansionSystem.GetStatusLabel(selectedLotState.Status) + "</BASEFONT>";
                    }
                }
            }
            else if (selectedWall != null && selectedWallState != null)
            {
                if (selectedWallState.Status == ReinoLotStatus.Locked)
                {
                    leftHtml = "<BASEFONT COLOR=#000000>Essa área de muralha ainda não está liberada.</BASEFONT>";
                    rightHtml = ReinoExpansionSystem.BuildWallRequirementHtml(selectedWall.CityId);
                }
                else if (selectedWallState.Status == ReinoLotStatus.Available)
                {
                    buildingOptions = ReinoExpansionDefinitions.GetBuildingsForArea(selectedWall);
                    showConfirm = true;
                    rightHtml = ReinoExpansionSystem.BuildWallRequirementHtml(selectedWall.CityId);
                }
                else if (selectedWallState.Status == ReinoLotStatus.Abandoned)
                {
                    ReinoConstructionDefinition only = ReinoExpansionDefinitions.GetBuilding(selectedWallState.ConstructionId);
                    if (only != null)
                    {
                        buildingOptions.Add(only);
                        showConfirm = true;
                        rightHtml = "<BASEFONT COLOR=#000000>Essa muralha foi abandonada e pode ser reativada.</BASEFONT>";
                    }
                }
                else if (selectedWallState.Status == ReinoLotStatus.UnderConstruction || selectedWallState.Status == ReinoLotStatus.Active)
                {
                    ReinoConstructionDefinition built = ReinoExpansionDefinitions.GetBuilding(selectedWallState.ConstructionId);
                    if (built != null)
                    {
                        leftHtml = ReinoExpansionDefinitions.FormatConstructionHtml(built);
                        rightHtml = "<BASEFONT COLOR=#000000><B>Status:</B> " + ReinoExpansionSystem.GetStatusLabel(selectedWallState.Status) + "</BASEFONT>";
                    }
                }
            }

            if (buildingOptions.Count > 0)
            {
                pageCount = (buildingOptions.Count + 5) / 6;
                int page = m_BuildingPage;
                if (page < 0)
                    page = 0;
                if (page >= pageCount)
                    page = pageCount - 1;

                int start = page * 6;
                int yBuild = 515;

                for (int i = start; i < buildingOptions.Count && i < start + 6; i++)
                {
                    ReinoConstructionDefinition def = buildingOptions[i];
                    AddButton(422, yBuild + ((i - start) * 25) + 3, 536, 435, ButtonSelectBuildingBase + i, GumpButtonType.Reply, 0);
                    AddLabel(450, yBuild + ((i - start) * 25), 0, def.Name);
                }

                ReinoConstructionDefinition selectedBuilding = null;
                if (!String.IsNullOrWhiteSpace(m_SelectedBuildingId))
                    selectedBuilding = ReinoExpansionDefinitions.GetBuilding(m_SelectedBuildingId);

                if (selectedBuilding == null && buildingOptions.Count > 0)
                    selectedBuilding = buildingOptions[start];

                if (selectedBuilding != null)
                    leftHtml = ReinoExpansionDefinitions.FormatConstructionHtml(selectedBuilding);

                if (showConfirm)
                {
                    AddButton(396, 666, 453, 453, ButtonConstructionPrev, GumpButtonType.Reply, 0);
                    AddButton(535, 666, 452, 452, ButtonConstructionNext, GumpButtonType.Reply, 0);
                    AddButton(431, 667, 492, 492, ButtonConstructionConfirm, GumpButtonType.Reply, 0);
                }
            }

            AddHtml(567, 503, 232, 186, leftHtml, false, false);
            AddHtml(845, 504, 375, 182, rightHtml, false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            int button = info.ButtonID;

            if (button >= ButtonSelectLeftLotBase && button < ButtonSelectRightLotBase)
            {
                int lotId = button - ButtonSelectLeftLotBase;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, lotId, -1, String.Empty, 0));
                return;
            }

            if (button >= ButtonSelectRightLotBase && button < ButtonSelectWallBase)
            {
                int lotId = button - ButtonSelectRightLotBase;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, lotId, -1, String.Empty, 0));
                return;
            }

            if (button >= ButtonSelectWallBase && button < ButtonSelectBuildingBase)
            {
                int areaId = button - ButtonSelectWallBase;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, areaId, String.Empty, 0));
                return;
            }

            if (button >= ButtonSelectBuildingBase && button < ButtonConstructionPrev)
            {
                int index = button - ButtonSelectBuildingBase;
                List<ReinoConstructionDefinition> options = GetCurrentBuildingOptions();

                if (index >= 0 && index < options.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, options[index].Id, m_BuildingPage));
                else
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage));

                return;
            }

            if (button == ButtonConstructionPrev)
            {
                int page = m_BuildingPage - 1;
                if (page < 0)
                    page = 0;

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, page));
                return;
            }

            if (button == ButtonConstructionNext)
            {
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage + 1));
                return;
            }

            if (button == ButtonConstructionConfirm)
            {
                string message;
                string buildingId = m_SelectedBuildingId;
                List<ReinoConstructionDefinition> options = GetCurrentBuildingOptions();

                if (String.IsNullOrWhiteSpace(buildingId) && options.Count > 0)
                {
                    int page = m_BuildingPage;
                    if (page < 0)
                        page = 0;

                    int start = page * 6;
                    if (start < 0 || start >= options.Count)
                        start = 0;

                    buildingId = options[start].Id;
                }

                if (m_SelectedLotId > 0)
                    ReinoExpansionSystem.TryConfirmLotConstruction(from, m_CityId, m_SelectedLotId, buildingId, out message);
                else if (m_SelectedWallAreaId > 0)
                    ReinoExpansionSystem.TryConfirmAreaConstruction(from, m_CityId, m_SelectedWallAreaId, buildingId, out message);
                else
                    message = "Selecione um lote ou uma muralha primeiro.";

                from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, buildingId, m_BuildingPage));
                return;
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
            else if (m_SelectedWallAreaId > 0)
            {
                ReinoAreaDefinition area = ReinoExpansionSystem.GetAreaDefinition(m_SelectedWallAreaId);
                ReinoAreaState st = ReinoExpansionSystem.GetAreaState(m_SelectedWallAreaId);

                if (area != null && st != null)
                {
                    if (st.Status == ReinoLotStatus.Available)
                        list = ReinoExpansionDefinitions.GetBuildingsForArea(area);
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
