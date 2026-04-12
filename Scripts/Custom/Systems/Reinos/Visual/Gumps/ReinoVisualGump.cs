using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonVisualSealPrev = 59700;
        private const int ButtonVisualSealNext = 59701;
        private const int ButtonVisualSealOk = 59702;

        private const int ButtonVisualRingPrev = 59710;
        private const int ButtonVisualRingNext = 59711;
        private const int ButtonVisualRingOk = 59712;

        private const int ButtonVisualBannerPrev = 59720;
        private const int ButtonVisualBannerNext = 59721;
        private const int ButtonVisualBannerOk = 59722;

        private const int ButtonVisualConfirm = 59730;
        private const int ButtonVisualAddBanner = 59731;

        private void BuildVisualPage()
        {
            AddPage(1);

            ReinoVisualSession session = ReinoVisualSystem.GetSession(m_From, m_CityId);
         //   ReinoVisualCityState state = null;

            int currentSeal = ReinoVisualSystem.GetSealGumpId(m_CityId);
            int currentRing = ReinoVisualSystem.GetRingGumpId(m_CityId);
            int currentBanner = ReinoVisualSystem.GetBannerGumpId(m_CityId);

            List<int> seals = ReinoVisualSystem.GetAvailableSealGumpIds(m_CityId);
            List<int> rings = ReinoVisualSystem.GetAvailableRingGumpIds(m_CityId);
            List<int> banners = ReinoVisualSystem.GetAvailableBannerGumpIds(m_CityId);

            int sealCost = ReinoVisualSystem.GetSelectionCost(currentSeal, session.PendingSealGumpId, ReinoVisualSystem.SealCost);
            int ringCost = ReinoVisualSystem.GetSelectionCost(currentRing, session.PendingRingGumpId, ReinoVisualSystem.RingCost);
            int bannerCost = ReinoVisualSystem.GetSelectionCost(currentBanner, session.PendingBannerGumpId, ReinoVisualSystem.BannerCost);
            int totalCost = sealCost + ringCost + bannerCost;

            AddLabel(791, 173, 0, @"Visual");
            AddImage(session.BrowseBannerGumpId >= 0 ? 705 : 705, 250, session.BrowseBannerGumpId);
            AddImageTiled(637, 221, 6, 471, 365);
            AddImageTiled(979, 221, 6, 470, 365);

            if (seals.IndexOf(session.BrowseSealGumpId) > 0)
                AddButton(407, 305, 451, 451, ButtonVisualSealPrev, GumpButtonType.Reply, 0);

            if (seals.IndexOf(session.BrowseSealGumpId) >= 0 && seals.IndexOf(session.BrowseSealGumpId) < seals.Count - 1)
                AddButton(599, 305, 450, 450, ButtonVisualSealNext, GumpButtonType.Reply, 0);

            if (rings.IndexOf(session.BrowseRingGumpId) > 0)
                AddButton(406, 559, 451, 451, ButtonVisualRingPrev, GumpButtonType.Reply, 0);

            if (rings.IndexOf(session.BrowseRingGumpId) >= 0 && rings.IndexOf(session.BrowseRingGumpId) < rings.Count - 1)
                AddButton(598, 559, 450, 450, ButtonVisualRingNext, GumpButtonType.Reply, 0);

            if (banners.IndexOf(session.BrowseBannerGumpId) > 0)
                AddButton(650, 433, 451, 451, ButtonVisualBannerPrev, GumpButtonType.Reply, 0);

            if (banners.IndexOf(session.BrowseBannerGumpId) >= 0 && banners.IndexOf(session.BrowseBannerGumpId) < banners.Count - 1)
                AddButton(956, 433, 450, 450, ButtonVisualBannerNext, GumpButtonType.Reply, 0);

            AddImageTiled(395, 253, 230, 5, 367);
            AddLabel(787, 223, 0, @"Banner");
            AddLabel(470, 229, 0, @"Selo do Reino");
            AddImage(464, 278, session.BrowseSealGumpId);
            AddButton(479, 405, 495, 495, ButtonVisualSealOk, GumpButtonType.Reply, 0);

            AddImageTiled(395, 515, 230, 5, 367);
            AddLabel(475, 490, 0, @"Anel do Reino");
            AddImage(468, 537, session.BrowseRingGumpId);
            AddButton(477, 640, 495, 495, ButtonVisualRingOk, GumpButtonType.Reply, 0);

            AddButton(777, 674, 495, 495, ButtonVisualBannerOk, GumpButtonType.Reply, 0);

            AddImageTiled(395, 475, 230, 5, 367);
            AddImageTiled(1005, 253, 230, 5, 367);
            AddLabel(1057, 230, 0, @"Custo das Mudanças:");
            AddLabel(1010, 279, 0, @"Custo Selo:");
            AddLabel(1130, 279, 0, sealCost.ToString());
            AddLabel(1010, 310, 0, @"Custo Anel:");
            AddLabel(1130, 310, 0, ringCost.ToString());
            AddLabel(1010, 344, 0, @"Custo Banner:");
            AddLabel(1130, 344, 0, bannerCost.ToString());
            AddLabel(1012, 377, 0, @"Custo Uniformes:");
            AddLabel(1130, 377, 0, @"0");
            AddLabel(1013, 411, 0, @"Custo Total:");
            AddLabel(1130, 411, 0, totalCost.ToString());

            AddHtml(1010, 506, 222, 125, session.InfoHtml, false, false);

            AddLabel(1072, 675, 0, @"Add Banner");
            AddButton(1154, 675, 531, 531, ButtonVisualAddBanner, GumpButtonType.Reply, 0);

            if (totalCost > 0)
                AddButton(1080, 457, 492, 492, ButtonVisualConfirm, GumpButtonType.Reply, 0);
        }

        private bool HandleVisualResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;

            if (button < ButtonVisualSealPrev || button > ButtonVisualAddBanner)
                return false;

            ReinoVisualSession session = ReinoVisualSystem.GetSession(from, m_CityId);

            if (button == ButtonVisualSealPrev)
            {
                session.BrowseSealGumpId = ReinoVisualSystem.GetPreviousValue(ReinoVisualSystem.GetAvailableSealGumpIds(m_CityId), session.BrowseSealGumpId);
                session.InfoHtml = ReinoVisualSystem.GetSealInfoHtml();
            }
            else if (button == ButtonVisualSealNext)
            {
                session.BrowseSealGumpId = ReinoVisualSystem.GetNextValue(ReinoVisualSystem.GetAvailableSealGumpIds(m_CityId), session.BrowseSealGumpId);
                session.InfoHtml = ReinoVisualSystem.GetSealInfoHtml();
            }
            else if (button == ButtonVisualSealOk)
            {
                session.PendingSealGumpId = session.BrowseSealGumpId;
                session.InfoHtml = ReinoVisualSystem.GetSealInfoHtml();
            }
            else if (button == ButtonVisualRingPrev)
            {
                session.BrowseRingGumpId = ReinoVisualSystem.GetPreviousValue(ReinoVisualSystem.GetAvailableRingGumpIds(m_CityId), session.BrowseRingGumpId);
                session.InfoHtml = ReinoVisualSystem.GetRingInfoHtml();
            }
            else if (button == ButtonVisualRingNext)
            {
                session.BrowseRingGumpId = ReinoVisualSystem.GetNextValue(ReinoVisualSystem.GetAvailableRingGumpIds(m_CityId), session.BrowseRingGumpId);
                session.InfoHtml = ReinoVisualSystem.GetRingInfoHtml();
            }
            else if (button == ButtonVisualRingOk)
            {
                session.PendingRingGumpId = session.BrowseRingGumpId;
                session.InfoHtml = ReinoVisualSystem.GetRingInfoHtml();
            }
            else if (button == ButtonVisualBannerPrev)
            {
                session.BrowseBannerGumpId = ReinoVisualSystem.GetPreviousValue(ReinoVisualSystem.GetAvailableBannerGumpIds(m_CityId), session.BrowseBannerGumpId);
                session.InfoHtml = ReinoVisualSystem.GetBannerInfoHtml();
            }
            else if (button == ButtonVisualBannerNext)
            {
                session.BrowseBannerGumpId = ReinoVisualSystem.GetNextValue(ReinoVisualSystem.GetAvailableBannerGumpIds(m_CityId), session.BrowseBannerGumpId);
                session.InfoHtml = ReinoVisualSystem.GetBannerInfoHtml();
            }
            else if (button == ButtonVisualBannerOk)
            {
                session.PendingBannerGumpId = session.BrowseBannerGumpId;
                session.InfoHtml = ReinoVisualSystem.GetBannerInfoHtml();
            }
            else if (button == ButtonVisualConfirm)
            {
                string message;
                if (!ReinoVisualSystem.CommitSession(from, m_CityId, out message))
                    from.SendMessage(message);
                else
                    from.SendMessage(message);
            }
            else if (button == ButtonVisualAddBanner)
            {
                from.CloseGump(typeof(ReinoAddBannerGump));
                from.SendGump(new ReinoAddBannerGump(from, m_CityId));
                return true;
            }

            from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 7));
            return true;
        }
    }
}
