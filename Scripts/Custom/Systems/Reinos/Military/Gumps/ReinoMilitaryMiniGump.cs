using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoMilitaryMiniGump : Gump
    {
        private const int ButtonTabGuards = 1;
        private const int ButtonTabRoutes = 2;
        private const int ButtonGuardKindBase = 100;
        private const int ButtonAddGuardPoint = 130;
        private const int ButtonRemoveGuardPoint = 131;
        private const int ButtonFacing = 132;
        private const int ButtonAddGuard = 133;
        private const int ButtonCreateRoutePoint = 200;
        private const int ButtonLinkRouteToGuard = 201;
        private const int ButtonRevealRoutePoints = 202;
        private const int ButtonRemoveRoutePoint = 203;
        private const int ButtonActivateRoute = 204;
        private const int ButtonSpeedShort = 205;
        private const int ButtonSpeedMedium = 206;
        private const int ButtonSpeedLong = 207;
        private const int ButtonResetRoute = 208;
        private const int ButtonRouteSchedule = 209;
        private const int ButtonResetRouteConfig = 210;

        private readonly int m_CityId;
        private readonly ReinoMilitaryTab m_Tab;

        public ReinoMilitaryMiniGump(PlayerMobile from, int cityId, ReinoMilitaryTab tab) : base(0, 0)
        {
            m_CityId = cityId;
            m_Tab = tab == ReinoMilitaryTab.Routes ? ReinoMilitaryTab.Routes : ReinoMilitaryTab.Guards;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);

            AddPage(0);
            AddImageTiled(338, 159, 375, 565, 392);
            AddImageTiled(318, 653, 78, 89, 359);
            AddImageTiled(653, 653, 74, 90, 360);
            AddImageTiled(653, 136, 74, 82, 361);
            AddImageTiled(318, 136, 74, 90, 362);
            AddImageTiled(322, 216, 26, 441, 365);
            AddImageTiled(698, 213, 26, 441, 366);
            AddImageTiled(387, 712, 273, 31, 367);
            AddImageTiled(384, 138, 270, 31, 368);
            AddImageTiled(492, 201, 184, 21, 470);
            AddImageTiled(367, 201, 178, 21, 469);
            AddLabel(471, 179, 0, @"Guard Setup");
            AddImageTiled(419, 201, 165, 21, 471);
            AddLabel(407, 228, 0, @"Guardas");
            AddLabel(618, 231, 0, @"Rotas");
            AddButton(379, 228, m_Tab == ReinoMilitaryTab.Guards ? 534 : 531, m_Tab == ReinoMilitaryTab.Guards ? 248 : 248, ButtonTabGuards, GumpButtonType.Reply, 0);
            AddButton(590, 231, m_Tab == ReinoMilitaryTab.Routes ? 534 : 531, m_Tab == ReinoMilitaryTab.Routes ? 248 : 248, ButtonTabRoutes, GumpButtonType.Reply, 0);
            AddImageTiled(369, 259, 311, 5, 367);

            if (m_Tab == ReinoMilitaryTab.Guards)
                BuildGuards(session);
            else
                BuildRoutes(session);
        }

        private void BuildGuards(ReinoMilitarySession session)
        {
            AddLabel(366, 272, 0, @"Tipos de Guarda");

            ReinoGuardKind[] kinds = new ReinoGuardKind[]
            {
                ReinoGuardKind.Vigia,
                ReinoGuardKind.Rua,
                ReinoGuardKind.Armado,
                ReinoGuardKind.Arqueiro,
                ReinoGuardKind.CavalariaArmada,
                ReinoGuardKind.CavalariaArqueira
            };

            string[] labels = new string[]
            {
                "Vigia",
                "Guarda de Rua",
                "Guarda Armado",
                "Guarda Arqueiro",
                "Cavalaria Armada",
                "Cavalaria Arqueira"
            };

            int[] ys = new int[] { 323, 353, 383, 413, 475, 505 };
            for (int i = 0; i < kinds.Length; i++)
            {
                bool selected = session.SelectedGuardKind == kinds[i];
                AddButton(366, ys[i], selected ? 530 : 531, 248, ButtonGuardKindBase + (int)kinds[i], GumpButtonType.Reply, 0);
                AddLabel(396, ys[i] + 1, 0, labels[i]);
            }

            int hg, hc, hi, hw, wg, wc, wi, ww;
            ReinoMilitarySystem.GetGuardCosts(session.SelectedGuardKind, out hg, out hc, out hi, out hw, out wg, out wc, out wi, out ww);
            int tg, tc, ti, tw;
            ReinoMilitarySystem.GetTotalWeeklyGuardCost(m_CityId, out tg, out tc, out ti, out tw);

            AddLabel(600, 276, 0, @"Por Semana");
            AddLabel(560, 313, 0, @"Moedas:"); AddLabel(630, 313, 0, wg.ToString());
            AddLabel(560, 341, 0, @"Tecidos:"); AddLabel(630, 341, 0, wc.ToString());
            AddLabel(560, 371, 0, @"Ferro:"); AddLabel(630, 371, 0, wi.ToString());
            AddLabel(560, 402, 0, @"Madeira:"); AddLabel(630, 402, 0, ww.ToString());

            AddLabel(574, 499, 0, @"Total Semanal");
            AddLabel(564, 536, 0, @"Moedas:"); AddLabel(630, 536, 0, tg + " (+" + wg + ")");
            AddLabel(564, 564, 0, @"Tecidos:"); AddLabel(630, 564, 0, tc + " (+" + wc + ")");
            AddLabel(564, 594, 0, @"Ferro:"); AddLabel(630, 594, 0, ti + " (+" + wi + ")");
            AddLabel(564, 625, 0, @"Madeira:"); AddLabel(630, 625, 0, tw + " (+" + ww + ")");

            AddLabel(398, 574, 0, @"Add Ponto de Guarda");
            AddLabel(398, 601, 0, @"Del Ponto de Guarda");
            AddLabel(398, 632, 0, @"Direção");
            AddLabel(470, 632, 0, ReinoMilitarySystem.GetFacingLabel(session.FacingIndex));
            AddLabel(398, 662, 0, @"Adicionar Guarda");
            AddButton(368, 573, 531, 248, ButtonAddGuardPoint, GumpButtonType.Reply, 0);
            AddButton(368, 603, 531, 248, ButtonRemoveGuardPoint, GumpButtonType.Reply, 0);
            AddButton(368, 633, 531, 248, ButtonFacing, GumpButtonType.Reply, 0);
            AddButton(368, 663, 531, 248, ButtonAddGuard, GumpButtonType.Reply, 0);
            AddImageTiled(365, 545, 155, 5, 367);
        }

        private void BuildRoutes(ReinoMilitarySession session)
        {
            AddLabel(406, 282, 1152, @"Criar Ponto de Rota");
            AddLabel(406, 309, 1152, @"Ligar a Um Ponto de Guarda");
            AddLabel(406, 340, 1152, @"Mostrar Pontos de Rota");
            AddLabel(406, 370, 1152, @"Remover Ponto de Rota");
            AddButton(376, 281, 531, 248, ButtonCreateRoutePoint, GumpButtonType.Reply, 0);
            AddButton(376, 311, 531, 248, ButtonLinkRouteToGuard, GumpButtonType.Reply, 0);
            AddButton(376, 341, 531, 248, ButtonRevealRoutePoints, GumpButtonType.Reply, 0);
            AddButton(376, 371, 531, 248, ButtonRemoveRoutePoint, GumpButtonType.Reply, 0);
            AddLabel(406, 401, 1152, @"Acionar Rota");
            AddButton(376, 402, 531, 248, ButtonActivateRoute, GumpButtonType.Reply, 0);
            AddLabel(406, 455, 1152, @"Tempo de Rota Curto");
            AddLabel(406, 482, 1152, @"Tempo de Rota Médio");
            AddLabel(406, 513, 1152, @"Tempo de Rota Longo");
            AddButton(376, 454, session.SelectedRouteSpeed == ReinoRouteSpeed.Short ? 530 : 531, 248, ButtonSpeedShort, GumpButtonType.Reply, 0);
            AddButton(376, 484, session.SelectedRouteSpeed == ReinoRouteSpeed.Medium ? 530 : 531, 248, ButtonSpeedMedium, GumpButtonType.Reply, 0);
            AddButton(376, 514, session.SelectedRouteSpeed == ReinoRouteSpeed.Long ? 530 : 531, 248, ButtonSpeedLong, GumpButtonType.Reply, 0);
            AddLabel(407, 574, 1152, @"Resetar Rota");
            AddButton(376, 576, 531, 248, ButtonResetRoute, GumpButtonType.Reply, 0);
            AddLabel(407, 636, 1152, @"Rota por Tempo");
            AddLabel(520, 636, 0, ReinoMilitarySystem.GetRouteScheduleLabel(session.SelectedRouteSchedule));
            AddButton(377, 637, 531, 248, ButtonRouteSchedule, GumpButtonType.Reply, 0);
            AddLabel(407, 667, 1152, @"Resetar Config de Rota");
            AddButton(377, 668, 531, 248, ButtonResetRouteConfig, GumpButtonType.Reply, 0);
            AddImageTiled(365, 434, 311, 5, 367);
            AddImageTiled(366, 552, 311, 5, 367);
            AddImageTiled(365, 612, 311, 5, 367);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null)
                return;

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);
            switch (info.ButtonID)
            {
                case 0:
                    return;
                case ButtonTabGuards:
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
                    return;
                case ButtonTabRoutes:
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonAddGuardPoint:
                    from.SendMessage(ReinoMilitarySystem.AddGuardPost(from, m_CityId, ReinoMilitarySystem.GetFacingByIndex(session.FacingIndex)));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
                    return;
                case ButtonRemoveGuardPoint:
                    from.SendMessage(ReinoMilitarySystem.RemoveGuardPost(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
                    return;
                case ButtonFacing:
                    session.FacingIndex = (session.FacingIndex + 1) % 4;
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
                    return;
                case ButtonAddGuard:
                    from.SendMessage(ReinoMilitarySystem.AddGuardToCurrentPost(from, m_CityId, session.SelectedGuardKind, ReinoMilitarySystem.GetFacingByIndex(session.FacingIndex)));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
                    return;
                case ButtonCreateRoutePoint:
                    from.SendMessage(ReinoMilitarySystem.CreateRoutePoint(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonLinkRouteToGuard:
                    from.SendMessage(ReinoMilitarySystem.LinkRouteToGuardPost(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonRevealRoutePoints:
                    from.SendMessage(ReinoMilitarySystem.RevealRoutePoints(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonRemoveRoutePoint:
                    from.SendMessage(ReinoMilitarySystem.RemoveRoutePoint(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonActivateRoute:
                    from.SendMessage(ReinoMilitarySystem.ActivateRouteAtCurrentPoint(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonSpeedShort:
                    session.SelectedRouteSpeed = ReinoRouteSpeed.Short;
                    from.SendMessage(ReinoMilitarySystem.SetRouteSpeedAtCurrentPoint(from, m_CityId, session.SelectedRouteSpeed));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonSpeedMedium:
                    session.SelectedRouteSpeed = ReinoRouteSpeed.Medium;
                    from.SendMessage(ReinoMilitarySystem.SetRouteSpeedAtCurrentPoint(from, m_CityId, session.SelectedRouteSpeed));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonSpeedLong:
                    session.SelectedRouteSpeed = ReinoRouteSpeed.Long;
                    from.SendMessage(ReinoMilitarySystem.SetRouteSpeedAtCurrentPoint(from, m_CityId, session.SelectedRouteSpeed));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonResetRoute:
                    from.SendMessage(ReinoMilitarySystem.ResetRouteAtCurrentPoint(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonRouteSchedule:
                    session.SelectedRouteSchedule = NextRouteSchedule(session.SelectedRouteSchedule);
                    from.SendMessage(ReinoMilitarySystem.SetRouteScheduleAtCurrentPoint(from, m_CityId, session.SelectedRouteSchedule));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
                case ButtonResetRouteConfig:
                    session.SelectedRouteSchedule = ReinoRouteSchedule.Infinite;
                    session.SelectedRouteSpeed = ReinoRouteSpeed.Short;
                    from.SendMessage(ReinoMilitarySystem.ResetRouteConfigAtCurrentPoint(from, m_CityId));
                    from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Routes));
                    return;
            }

            if (info.ButtonID >= ButtonGuardKindBase && info.ButtonID < ButtonGuardKindBase + 20)
            {
                session.SelectedGuardKind = (ReinoGuardKind)(info.ButtonID - ButtonGuardKindBase);
                from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
            }
        }

        private static ReinoRouteSchedule NextRouteSchedule(ReinoRouteSchedule current)
        {
            switch (current)
            {
                case ReinoRouteSchedule.Every15Minutes: return ReinoRouteSchedule.Every30Minutes;
                case ReinoRouteSchedule.Every30Minutes: return ReinoRouteSchedule.Every45Minutes;
                case ReinoRouteSchedule.Every45Minutes: return ReinoRouteSchedule.Every60Minutes;
                case ReinoRouteSchedule.Every60Minutes: return ReinoRouteSchedule.DawnOnly;
                case ReinoRouteSchedule.DawnOnly: return ReinoRouteSchedule.Infinite;
                default: return ReinoRouteSchedule.Every15Minutes;
            }
        }
    }

    public class ReinoLawBoardGump : Gump
    {
        public ReinoLawBoardGump(PlayerMobile from, int cityId) : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(280, 100, 3547);
            AddHtml(363, 168, 211, 393, ReinoMilitarySystem.GetCurrentLawsHtml(cityId), false, false);
        }
    }
}
