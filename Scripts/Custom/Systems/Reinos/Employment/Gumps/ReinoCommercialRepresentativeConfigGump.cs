using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoCommercialRepresentativeConfigGump : Gump
    {
        private readonly int m_CityId;
        private readonly int[] m_SellPrices;
        private readonly int[] m_BuyPrices;
        private readonly int[] m_SellCaps;
        private readonly int[] m_BuyCaps;

        public ReinoCommercialRepresentativeConfigGump(PlayerMobile from, int cityId) : this(from, cityId, null, null, null, null)
        {
        }

        public ReinoCommercialRepresentativeConfigGump(PlayerMobile from, int cityId, int[] sellPrices, int[] buyPrices, int[] sellCaps, int[] buyCaps) : base(0, 0)
        {
            m_CityId = cityId;

            ReinoCommercialTradeState state = ReinoEmploymentSystem.GetTradeState(cityId);
            m_SellPrices = sellPrices ?? (int[])state.SellPrices.Clone();
            m_BuyPrices = buyPrices ?? (int[])state.BuyPrices.Clone();
            m_SellCaps = sellCaps ?? (int[])state.WeeklySellCaps.Clone();
            m_BuyCaps = buyCaps ?? (int[])state.WeeklyBuyCaps.Clone();

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            int labelhue = 1152;

            AddPage(0);
            AddImageTiled(338, 159, 524, 565, 377);
            AddImageTiled(318, 653, 78, 89, 359);
            AddImageTiled(813, 653, 74, 90, 360);
            AddImageTiled(813, 136, 74, 82, 361);
            AddImageTiled(318, 136, 74, 90, 362);
            AddImageTiled(322, 216, 26, 441, 365);
            AddImageTiled(858, 213, 26, 441, 366);
            AddImageTiled(387, 712, 426, 31, 367);
            AddImageTiled(384, 138, 430, 31, 368);
            AddImageTiled(652, 201, 184, 21, 470);
            AddImageTiled(367, 201, 178, 21, 469);
            AddLabel(510, 178, labelhue, @"Representante Comercial Setup");
            AddImageTiled(419, 201, 316, 21, 471);

            AddImageTiled(594, 257, 6, 392, 365);
            AddLabel(572, 226, labelhue, @"Valores");
            AddImageTiled(362, 480, 478, 7, 367);
            AddImageTiled(362, 253, 478, 7, 367);
            AddImageTiled(362, 316, 478, 5, 367);
            AddImageTiled(362, 376, 478, 5, 367);
            AddImageTiled(362, 434, 478, 8, 367);
            AddImageTiled(362, 541, 478, 5, 367);
            AddImageTiled(362, 601, 478, 5, 367);
            AddButton(558, 677, 492, 492, 500, GumpButtonType.Reply, 0);

            for (int i = 0; i < 3; i++)
            {
                int yTop = 279 + (i * 61);
                int yBottom = 504 + (i * 61);
                string label = ReinoEmploymentSystem.GetTradeResourceLabel(i).TrimEnd('s');

                AddLabel(364, yTop, labelhue, "Vendo " + label + ":");
                AddButton(509, yTop - 5, 587, 587, 100 + i, GumpButtonType.Reply, 0);
                AddButton(548, yTop - 7, 584, 584, 110 + i, GumpButtonType.Reply, 0);
                AddLabel(473, yTop, labelhue, m_SellPrices[i].ToString());

                AddLabel(610, yTop, labelhue, "Compro " + label + ":");
                AddButton(763, yTop - 5, 587, 587, 200 + i, GumpButtonType.Reply, 0);
                AddButton(802, yTop - 7, 584, 584, 210 + i, GumpButtonType.Reply, 0);
                AddLabel(727, yTop, labelhue, m_BuyPrices[i].ToString());
            }

            AddLabel(419, 451, labelhue, @"Vendo No Máximo");
            AddLabel(675, 451, labelhue, @"Compro No Máximo");

            for (int i = 0; i < 3; i++)
            {
                int y = 504 + (i * 61);
                string label = ReinoEmploymentSystem.GetTradeResourceLabel(i).TrimEnd('s');

                AddLabel(364, y, labelhue, "Max " + label + ":");
                AddButton(509, y - 5, 587, 587, 300 + i, GumpButtonType.Reply, 0);
                AddButton(548, y - 7, 584, 584, 310 + i, GumpButtonType.Reply, 0);
                AddLabel(473, y, labelhue, m_SellCaps[i].ToString());

                AddLabel(610, y, labelhue, "Max " + label + ":");
                AddButton(763, y - 5, 587, 587, 400 + i, GumpButtonType.Reply, 0);
                AddButton(802, y - 7, 584, 584, 410 + i, GumpButtonType.Reply, 0);
                AddLabel(727, y, labelhue, m_BuyCaps[i].ToString());
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID == 0)
                return;

            if (!ReinoEmploymentSystem.CanUseCommercialRepresentative(from, m_CityId))
                return;

            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(m_CityId);

            if (info.ButtonID >= 100 && info.ButtonID < 103)
                m_SellPrices[info.ButtonID - 100]++;
            else if (info.ButtonID >= 110 && info.ButtonID < 113)
                m_SellPrices[info.ButtonID - 110] = Math.Max(0, m_SellPrices[info.ButtonID - 110] - 1);
            else if (info.ButtonID >= 200 && info.ButtonID < 203)
                m_BuyPrices[info.ButtonID - 200]++;
            else if (info.ButtonID >= 210 && info.ButtonID < 213)
                m_BuyPrices[info.ButtonID - 210] = Math.Max(0, m_BuyPrices[info.ButtonID - 210] - 1);
            else if (info.ButtonID >= 300 && info.ButtonID < 303)
            {
                int index = info.ButtonID - 300;
                int max = ledger.Get(ReinoEmploymentSystem.GetTradeResourceType(index));
                m_SellCaps[index] = Math.Min(max, m_SellCaps[index] + 5);
            }
            else if (info.ButtonID >= 310 && info.ButtonID < 313)
                m_SellCaps[info.ButtonID - 310] = Math.Max(0, m_SellCaps[info.ButtonID - 310] - 5);
            else if (info.ButtonID >= 400 && info.ButtonID < 403)
                m_BuyCaps[info.ButtonID - 400] += 5;
            else if (info.ButtonID >= 410 && info.ButtonID < 413)
                m_BuyCaps[info.ButtonID - 410] = Math.Max(0, m_BuyCaps[info.ButtonID - 410] - 5);
            else if (info.ButtonID == 500)
            {
                string message;
                ReinoEmploymentSystem.UpdateTradeConfig(from, m_CityId, m_BuyPrices, m_SellPrices, m_BuyCaps, m_SellCaps, out message);
                from.SendMessage(message);
            }

            from.SendGump(new ReinoCommercialRepresentativeConfigGump(from, m_CityId, m_SellPrices, m_BuyPrices, m_SellCaps, m_BuyCaps));
        }
    }
}
