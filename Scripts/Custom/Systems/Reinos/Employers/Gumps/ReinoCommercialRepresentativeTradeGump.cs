using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoCommercialRepresentativeTradeGump : Gump
    {
        private readonly int m_CityId;
        private readonly int[] m_BuyQty;
        private readonly int[] m_SellQty;
        private readonly bool m_CanSell;

        public ReinoCommercialRepresentativeTradeGump(PlayerMobile from, int cityId) : this(from, cityId, new int[3], new int[3])
        {
        }

        public ReinoCommercialRepresentativeTradeGump(PlayerMobile from, int cityId, int[] buyQty, int[] sellQty) : base(0, 0)
        {
            m_CityId = cityId;
            m_BuyQty = buyQty ?? new int[3];
            m_SellQty = sellQty ?? new int[3];
            m_CanSell = from != null && ReinoEmploymentSystem.CanUseCommercialRepresentative(from, cityId);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoCommercialTradeState state = ReinoEmploymentSystem.GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            int labelhue = 1152;

            AddPage(0);
            AddImageTiled(338, 159, 524, 565, 392);
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
            AddLabel(526, 178, labelhue, @"Representante Comercial");
            AddImageTiled(419, 201, 316, 21, 471);
            AddImageTiled(598, 233, 6, 462, 365);
            AddLabel(430, 228, labelhue, @"Estou Comprando");
            AddLabel(671, 228, labelhue, @"Estou Vendendo");
            AddImageTiled(361, 255, 478, 5, 367);
            AddImageTiled(361, 395, 478, 5, 367);
            AddImageTiled(361, 457, 478, 5, 367);
            AddImageTiled(361, 517, 478, 5, 367);
            AddImageTiled(362, 580, 478, 5, 367);

            for (int i = 0; i < 3; i++)
            {
                int yTop = 279 + (i * 34);
                int yQty = 420 + (i * 61);
                string label = ReinoEmploymentSystem.GetTradeResourceLabel(i);

                AddLabel(373, yTop, labelhue, state.WeeklyBuyRemaining[i] + " " + label + ":");
                AddLabel(476, yTop, labelhue, state.BuyPrices[i] > 0 ? (state.BuyPrices[i] + " moedas") : "-");

                AddLabel(618, yTop, labelhue, state.WeeklySellRemaining[i] + " " + label + ":");
                AddLabel(721, yTop, labelhue, state.SellPrices[i] > 0 ? (state.SellPrices[i] + " moedas") : "-");

                AddLabel(371, yQty, labelhue, ReinoEmploymentSystem.GetTradeResourceLabel(i));
                AddButton(498, yQty - 4, 587, 587, 100 + i, GumpButtonType.Reply, 0);
                AddButton(549, yQty - 6, 584, 584, 110 + i, GumpButtonType.Reply, 0);
                AddLabel(443, yQty, labelhue, m_BuyQty[i].ToString());

                AddLabel(619, yQty, labelhue, ReinoEmploymentSystem.GetTradeResourceLabel(i));
                if (m_CanSell)
                {
                    AddButton(746, yQty - 4, 587, 587, 200 + i, GumpButtonType.Reply, 0);
                    AddButton(797, yQty - 6, 584, 584, 210 + i, GumpButtonType.Reply, 0);
                }
                AddLabel(691, yQty, labelhue, m_SellQty[i].ToString());
            }

            AddLabel(410, 604, labelhue, @"Total:");
            AddLabel(480, 604, labelhue, GetBuyTotal(state).ToString());
            AddButton(422, 657, 492, 492, 300, GumpButtonType.Reply, 0);

            AddLabel(620, 605, labelhue, @"Total:");
            AddLabel(692, 605, labelhue, GetSellTotal(state).ToString());
            if (m_CanSell)
                AddButton(686, 657, 492, 492, 301, GumpButtonType.Reply, 0);

      //      AddLabel(365, 680, labelhue, "Tesouro: " + ledger.Gold + " moedas");
        }

        private int GetBuyTotal(ReinoCommercialTradeState state)
        {
            int total = 0;
            for (int i = 0; i < 3; i++)
                total += Math.Max(0, m_BuyQty[i]) * Math.Max(0, state.BuyPrices[i]);
            return total;
        }

        private int GetSellTotal(ReinoCommercialTradeState state)
        {
            int total = 0;
            for (int i = 0; i < 3; i++)
                total += Math.Max(0, m_SellQty[i]) * Math.Max(0, state.SellPrices[i]);
            return total;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID == 0)
                return;

            ReinoCommercialTradeState state = ReinoEmploymentSystem.GetTradeState(m_CityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(m_CityId);

            if (info.ButtonID >= 100 && info.ButtonID < 103)
            {
                int index = info.ButtonID - 100;
                int otherTotal = 0;
                for (int i = 0; i < 3; i++)
                    if (i != index) otherTotal += Math.Max(0, m_BuyQty[i]) * Math.Max(0, state.BuyPrices[i]);

                int maxByRemaining = state.WeeklyBuyRemaining[index];
                int maxByBackpack = ReinoEmploymentSystem.CountPlayerTradeResource(from, index);
                int maxByGold = state.BuyPrices[index] > 0 ? Math.Max(0, (ledger.Gold - otherTotal) / state.BuyPrices[index]) : 0;
                int limit = Math.Min(maxByRemaining, Math.Min(maxByBackpack, maxByGold));
                m_BuyQty[index] = Math.Min(limit, m_BuyQty[index] + 5);
            }
            else if (info.ButtonID >= 110 && info.ButtonID < 113)
                m_BuyQty[info.ButtonID - 110] = Math.Max(0, m_BuyQty[info.ButtonID - 110] - 5);
            else if (info.ButtonID >= 200 && info.ButtonID < 203)
            {
                int index = info.ButtonID - 200;
                int limit = Math.Min(state.WeeklySellRemaining[index], ledger.Get(ReinoEmploymentSystem.GetTradeResourceType(index)));
                m_SellQty[index] = Math.Min(limit, m_SellQty[index] + 5);
            }
            else if (info.ButtonID >= 210 && info.ButtonID < 213)
                m_SellQty[info.ButtonID - 210] = Math.Max(0, m_SellQty[info.ButtonID - 210] - 5);
            else if (info.ButtonID == 300)
            {
                string message;
                ReinoEmploymentSystem.ExecuteRepresentativeBuyFromPlayer(from, m_CityId, m_BuyQty, out message);
                from.SendMessage(message);
                for (int i = 0; i < 3; i++)
                    m_BuyQty[i] = 0;
            }
            else if (info.ButtonID == 301)
            {
                if (!ReinoEmploymentSystem.CanUseCommercialRepresentative(from, m_CityId))
                {
                    from.SendMessage("Você não pode vender recursos virtuais deste reino.");
                }
                else
                {
                    string message;
                    ReinoEmploymentSystem.ExecuteRepresentativeSell(m_CityId, m_SellQty, out message);
                    from.SendMessage(message);
                    for (int i = 0; i < 3; i++)
                        m_SellQty[i] = 0;
                }
            }

            from.SendGump(new ReinoCommercialRepresentativeTradeGump(from, m_CityId, m_BuyQty, m_SellQty));
        }
    }
}
