using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoMilitaryReportsGump : Gump
    {
        private const int ButtonPrev = 59000;
        private const int ButtonNext = 59001;
        private const int ButtonCrimes = 59002;
        private const int ButtonPrisons = 59003;
        private const int ButtonWanted = 59004;
        private const int ButtonRecurring = 59005;
        private const int ButtonPrint = 59006;

        private readonly PlayerMobile m_From;
        private readonly int m_CityId;

        public ReinoMilitaryReportsGump(PlayerMobile from, int cityId)
            : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);

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
            AddImageTiled(419, 201, 316, 21, 471);
            AddLabel(538, 180, 0, @"Relatório do Reino");

            AddHtml(381, 244, 444, 123, ReinoMilitarySystem.GetReportsSummaryHtml(cityId), false, false);
            AddHtml(419, 462, 368, 196, ReinoMilitarySystem.GetReportsDetailHtml(cityId, session.DetailMode, session.DetailIndex), false, true);

            int count = ReinoMilitarySystem.GetDetailCount(cityId, session.DetailMode);
            if (count > 1 && session.DetailIndex > 0)
                AddButton(372, 554, 580, 580, ButtonPrev, GumpButtonType.Reply, 0);
            if (count > 1 && session.DetailIndex < count - 1)
                AddButton(806, 556, 581, 581, ButtonNext, GumpButtonType.Reply, 0);

            AddLabel(419, 391, 0, @"Ver Crimes Detalhados");
            AddLabel(419, 419, 0, @"Ver Prisões Detalhadas");
            AddLabel(644, 391, 0, @"Ver Procurados Detalhados");
            AddLabel(644, 418, 0, @"Ver Criminosos Recorrentes");

            AddButton(391, 389, session.DetailMode == 1 ? 530 : 531, session.DetailMode == 1 ? 530 : 531, ButtonCrimes, GumpButtonType.Reply, 0);
            AddButton(391, 419, session.DetailMode == 2 ? 530 : 531, session.DetailMode == 2 ? 530 : 531, ButtonPrisons, GumpButtonType.Reply, 0);
            AddButton(614, 390, session.DetailMode == 3 ? 530 : 531, session.DetailMode == 3 ? 530 : 531, ButtonWanted, GumpButtonType.Reply, 0);
            AddButton(614, 420, session.DetailMode == 4 ? 530 : 531, session.DetailMode == 4 ? 530 : 531, ButtonRecurring, GumpButtonType.Reply, 0);

            AddLabel(537, 676, 0, @"Pegar Relatório Impresso");
            AddButton(507, 675, 531, 531, ButtonPrint, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);

            switch (info.ButtonID)
            {
                case ButtonPrev:
                    if (session.DetailIndex > 0)
                        session.DetailIndex--;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
                case ButtonNext:
                    if (session.DetailIndex < Math.Max(0, ReinoMilitarySystem.GetDetailCount(m_CityId, session.DetailMode) - 1))
                        session.DetailIndex++;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
                case ButtonCrimes:
                    session.DetailMode = 1;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
                case ButtonPrisons:
                    session.DetailMode = 2;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
                case ButtonWanted:
                    session.DetailMode = 3;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
                case ButtonRecurring:
                    session.DetailMode = 4;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
                case ButtonPrint:
                    from.SendMessage(ReinoMilitarySystem.PrintReportBook(from, m_CityId));
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    break;
            }
        }
    }
}
