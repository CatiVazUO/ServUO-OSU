using System;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos
{
    public partial class ReinoExpansionGump : Gump
    {
        protected readonly PlayerMobile m_From;
        protected readonly int m_CityId;
        protected readonly int m_SelectedLotId;
        protected readonly int m_SelectedWallAreaId;
        protected readonly string m_SelectedBuildingId;
        protected readonly int m_BuildingPage;

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

            BuildBaseGump();
            BuildExpansionPage();
            BuildPlaceholderPages();
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
            AddLabel(253, 170, 0, @"Governo");

            AddLabel(250, 231, 0, @"Visão Geral");
            AddButton(211, 225, 439, 248, 1, GumpButtonType.Page, 1);

            AddLabel(250, 270, 0, @"Tesouro");
            AddButton(211, 264, 439, 248, 2, GumpButtonType.Page, 2);

            AddLabel(250, 310, 0, @"Manutenção");
            AddButton(211, 304, 439, 248, 3, GumpButtonType.Page, 3);

            AddLabel(250, 349, 0, @"Expansão");
            AddButton(211, 343, 439, 248, 4, GumpButtonType.Page, 4);

            AddLabel(250, 390, 0, @"Empregados");
            AddButton(211, 384, 439, 248, 5, GumpButtonType.Page, 5);

            AddLabel(250, 429, 0, @"Diplomacia");
            AddButton(211, 423, 439, 248, 6, GumpButtonType.Page, 6);

            AddLabel(250, 469, 0, @"Visual");
            AddButton(211, 463, 439, 248, 7, GumpButtonType.Page, 7);

            AddLabel(250, 508, 0, @"Slot 1");
            AddButton(211, 502, 439, 248, 8, GumpButtonType.Page, 8);

            AddLabel(250, 548, 0, @"Slot 2");
            AddButton(211, 542, 439, 248, 9, GumpButtonType.Page, 9);

            AddLabel(250, 587, 0, @"Slot 3");
            AddButton(211, 581, 439, 248, 10, GumpButtonType.Page, 10);

            AddLabel(250, 627, 0, @"Slot 4");
            AddButton(211, 621, 439, 248, 11, GumpButtonType.Page, 11);

            AddLabel(250, 666, 0, @"Slot 5");
            AddButton(211, 660, 439, 248, 12, GumpButtonType.Page, 12);
        }

        private void BuildPlaceholderPages()
        {
            AddPage(1);
            AddPage(2);
            AddPage(3);
            AddPage(5);
            AddPage(6);
            AddPage(7);
            AddPage(8);
            AddPage(9);
            AddPage(10);
            AddPage(11);
            AddPage(12);
        }
    }
}
