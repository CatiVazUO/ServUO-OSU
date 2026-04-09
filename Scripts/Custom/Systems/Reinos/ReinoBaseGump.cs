using System;
using Server.Gumps;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump : Gump
    {
        protected readonly PlayerMobile m_From;
        protected readonly int m_CityId;
        protected readonly int m_SelectedLotId;
        protected readonly int m_SelectedWallAreaId;
        protected readonly string m_SelectedBuildingId;
        protected readonly int m_BuildingPage;
        protected readonly int m_InitialPage;
        protected readonly bool m_DemolishConfirm;

        public ReinoExpansionGump(PlayerMobile from, int cityId)
            : this(from, cityId, -1, -1, String.Empty, 0, 1, false)
        {
        }

        public ReinoExpansionGump(PlayerMobile from, int cityId, int selectedLotId, int selectedWallAreaId, string selectedBuildingId, int buildingPage)
            : this(from, cityId, selectedLotId, selectedWallAreaId, selectedBuildingId, buildingPage, 1, false)
        {
        }

        public ReinoExpansionGump(PlayerMobile from, int cityId, int selectedLotId, int selectedWallAreaId, string selectedBuildingId, int buildingPage, int initialPage)
            : this(from, cityId, selectedLotId, selectedWallAreaId, selectedBuildingId, buildingPage, initialPage, false)
        {
        }
        private class ReinoMenuSlotEntry
        {
            public int Page;
            public string Label;

            public ReinoMenuSlotEntry(int page, string label)
            {
                Page = page;
                Label = label;
            }
        }

        public ReinoExpansionGump(PlayerMobile from, int cityId, int selectedLotId, int selectedWallAreaId, string selectedBuildingId, int buildingPage, int initialPage, bool demolishConfirm)
            : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;
            m_SelectedLotId = selectedLotId;
            m_SelectedWallAreaId = selectedWallAreaId;
            m_SelectedBuildingId = selectedBuildingId ?? String.Empty;
            m_BuildingPage = buildingPage < 0 ? 0 : buildingPage;
            m_InitialPage = initialPage;
            m_DemolishConfirm = demolishConfirm;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            BuildBaseGump();
            BuildCurrentSection();
        }

        private void BuildBaseGump()
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
            AddLabel(253, 170, 1152, @"Governo");

            AddLabel(250, 231, 1152, @"Visão Geral");
            AddButton(211, 225, 439, 438, 1, GumpButtonType.Reply, 0);

            AddLabel(250, 270, 1152, @"Tesouro");
            AddButton(211, 264, 439, 438, 2, GumpButtonType.Reply, 0);

            AddLabel(250, 310, 1152, @"Manutenção");
            AddButton(211, 304, 439, 438, 3, GumpButtonType.Reply, 0);

            AddLabel(250, 349, 1152, @"Expansão");
            AddButton(211, 343, 439, 438, 4, GumpButtonType.Reply, 0);

            AddLabel(250, 390, 1152, @"Cargos");
            AddButton(211, 384, 439, 438, 5, GumpButtonType.Reply, 0);

            AddLabel(250, 429, 1152, @"Diplomacia");
            AddButton(211, 423, 439, 438, 6, GumpButtonType.Reply, 0);

            AddLabel(250, 469, 1152, @"Visual");
            AddButton(211, 463, 439, 438, 7, GumpButtonType.Reply, 0);

            List<ReinoMenuSlotEntry> dynamicSlots = GetDynamicGovernmentSlots();

            int startY = 508;
            int spacing = 39;

            for (int i = 0; i < dynamicSlots.Count && i < 5; i++)
            {
                ReinoMenuSlotEntry entry = dynamicSlots[i];
                int y = startY + (i * spacing);

                AddLabel(250, y, 0, entry.Label);
                AddButton(211, y - 6, 439, 438, entry.Page, GumpButtonType.Reply, 0);
            }
        }

        private bool HasBuiltConstruction(string constructionId)
        {
            if (String.IsNullOrWhiteSpace(constructionId))
                return false;

            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);

            for (int i = 0; i < active.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = active[i];

                if (info == null || info.Definition == null)
                    continue;

                string id = info.Definition.Id ?? String.Empty;

                if (String.Equals(id, constructionId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        private List<ReinoMenuSlotEntry> GetDynamicGovernmentSlots()
        {
            List<ReinoMenuSlotEntry> list = new List<ReinoMenuSlotEntry>();

            // Militar sempre entra primeiro, mas só aparece se existir quartel
            if (HasBuiltConstruction("quartel_aurora"))
                list.Add(new ReinoMenuSlotEntry(8, "Militar"));

            // Exemplo de outros sistemas futuros:
            // if (HasBuiltConstruction("algumaoutracoisa"))
            //     list.Add(new ReinoMenuSlotEntry(11, "Outra Aba"));

            // Postos, se você quiser continuar mostrando no menu lateral:
            list.Add(new ReinoMenuSlotEntry(10, "Postos"));

            return list;
        }
        private void BuildCurrentSection()
        {
            int page = m_InitialPage;
            string pageMessage;

            if (!ReinoEmploymentSystem.CanUseGovernmentPage(m_From, m_CityId, page, out pageMessage))
                page = 5;

            switch (page)
            {
                case 3:
                    BuildMaintenancePage();
                    break;
                case 4:
                    BuildExpansionPage();
                    break;
                case 5:
                    BuildGovernmentPage();
                    break;
                case 6:
                    BuildDiplomacyPage();
                    break;
                case 8:
                    if (!HasBuiltConstruction("quartel_aurora"))
                    {
                        BuildPlaceholderPage("Militar");
                        break;
                    }

                    if (ReinoMilitarySystem.CanManageWantedList(m_From, m_CityId))
                        ReinoMilitarySystem.GetSession(m_From).RestrictToBarracksView = false;

                    BuildMilitaryPage();
                    break;
                case 1:
                    BuildGeneralPage();
                    break;
                case 2:
                    BuildTreasurePage();
                    break;
                case 9:
                    BuildConstructionDetailPage();
                    break;
                case 10:
                    BuildPostoDetailPage();
                    break;
                case 17:
                    BuildCreateRolePage();
                    break;
                default:
                    BuildPlaceholderPage(GetPlaceholderTitle(page));
                    break;
            }
        }

        private string GetPlaceholderTitle(int page)
        {
            switch (page)
            {
                case 1: return "Visão Geral";
                case 2: return "Tesouro";
                case 5: return "Cargos";
                case 6: return "Diplomacia";
                case 7: return "Visual";
                case 8: return "Militar";
                case 11: return "Slot 4";
                case 12: return "Slot 5";
                default: return "Página";
            }
        }

        private void BuildPlaceholderPage(string titulo)
        {
            AddPage(1);

            AddLabel(430, 185, 0, titulo);
            AddHtml(430, 220, 720, 420,
                "<BASEFONT COLOR=#D7C89A>Esta página ainda não foi implementada.</BASEFONT>",
                true, true);
        }
    }
}
