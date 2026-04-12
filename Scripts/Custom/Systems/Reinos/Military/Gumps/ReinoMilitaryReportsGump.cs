using System;
using System.CodeDom;
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
        private const int ButtonOldReports = 59007;
        private const int ButtonBackToCurrent = 59008;
        private const int ButtonArchiveBase = 59100;
    


        private readonly int m_CityId;
        private readonly bool m_ShowArchiveList;
        private readonly int m_ArchiveIndex;
        private readonly int m_DetailMode;
        private readonly int m_DetailIndex;

        public ReinoMilitaryReportsGump(PlayerMobile from, int cityId)
            : this(from, cityId, false, -1, 0, 0)
        {
        }

        public ReinoMilitaryReportsGump(PlayerMobile from, int cityId, bool showArchiveList, int archiveIndex, int detailMode, int detailIndex)
            : base(0, 0)
        {
            m_CityId = cityId;
            m_ShowArchiveList = showArchiveList;
            m_ArchiveIndex = archiveIndex;
            m_DetailMode = detailMode;
            m_DetailIndex = detailIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

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

            if (m_ShowArchiveList)
                BuildArchiveListView();
            else
                BuildMainReportView();
        }

        private void BuildMainReportView()
        {
            string title = m_ArchiveIndex >= 0
                ? ReinoMilitarySystem.GetArchivedReportTitle(m_CityId, m_ArchiveIndex)
                : "Relatório do Reino";

            AddLabel(538, 180, 0, title);

            string summaryHtml = m_ArchiveIndex >= 0
                ? ReinoMilitarySystem.GetArchivedReportsSummaryHtml(m_CityId, m_ArchiveIndex)
                : ReinoMilitarySystem.GetReportsSummaryHtml(m_CityId);

            string detailHtml = m_ArchiveIndex >= 0
                ? ReinoMilitarySystem.GetArchivedReportDetailHtml(m_CityId, m_ArchiveIndex, m_DetailMode, m_DetailIndex)
                : ReinoMilitarySystem.GetReportsDetailHtml(m_CityId, m_DetailMode, m_DetailIndex);

            int count = m_ArchiveIndex >= 0
                ? ReinoMilitarySystem.GetArchivedDetailCount(m_CityId, m_ArchiveIndex, m_DetailMode)
                : ReinoMilitarySystem.GetDetailCount(m_CityId, m_DetailMode);

            AddHtml(381, 244, 444, 123, summaryHtml, false, false);
            AddHtml(419, 462, 368, 196, detailHtml, false, true);

            if (count > 1 && m_DetailIndex > 0)
                AddButton(372, 554, 580, 580, ButtonPrev, GumpButtonType.Reply, 0);
            if (count > 1 && m_DetailIndex < count - 1)
                AddButton(806, 556, 581, 581, ButtonNext, GumpButtonType.Reply, 0);

            AddLabel(419, 391, 1152, @"Ver Crimes Detalhados");
            AddLabel(419, 419, 1152, @"Ver Prisões Detalhadas");
            AddLabel(644, 391, 1152, @"Ver Procurados Detalhados");
            AddLabel(644, 418, 1152, @"Ver Criminosos Recorrentes");

            AddButton(391, 389, m_DetailMode == 1 ? 530 : 531, m_DetailMode == 1 ? 530 : 531, ButtonCrimes, GumpButtonType.Reply, 0);
            AddButton(391, 419, m_DetailMode == 2 ? 530 : 531, m_DetailMode == 2 ? 530 : 531, ButtonPrisons, GumpButtonType.Reply, 0);
            AddButton(614, 390, m_DetailMode == 3 ? 530 : 531, m_DetailMode == 3 ? 530 : 531, ButtonWanted, GumpButtonType.Reply, 0);
            AddButton(614, 420, m_DetailMode == 4 ? 530 : 531, m_DetailMode == 4 ? 530 : 531, ButtonRecurring, GumpButtonType.Reply, 0);

            AddLabel(537, 676, 1152, @"Pegar Relatório Impresso");
            AddButton(507, 675, 531, 531, ButtonPrint, GumpButtonType.Reply, 0);

            if (m_ArchiveIndex < 0)
            {
                AddLabel(769, 294, 1152, @"Antigos");
                AddButton(739, 293, 531, 531, ButtonOldReports, GumpButtonType.Reply, 0);
            }
            else
            {
                AddLabel(769, 294, 1152, @"Atual");
                AddButton(739, 293, 531, 531, ButtonBackToCurrent, GumpButtonType.Reply, 0);
            }
        }

        private void BuildArchiveListView()
        {
            AddLabel(541, 228, 1152, @"Relatórios Antigos");

            int count = Math.Min(10, ReinoMilitarySystem.GetArchivedReportCount(m_CityId));
            if (count <= 0)
            {
                AddHtml(410, 279, 350, 40, "<BASEFONT COLOR=#FFFFFF>Nenhum relatório antigo encontrado.</BASEFONT>", false, false);
                AddLabel(769, 294, 1152, @"Atual");
                AddButton(739, 293, 531, 531, ButtonBackToCurrent, GumpButtonType.Reply, 0);
                return;
            }

            int y = 279;
            for (int i = 0; i < count; i++, y += 28)
            {
                AddLabel(410, y, 1152, ReinoMilitarySystem.GetArchivedReportListLabel(m_CityId, i));
                AddButton(382, y - 2, 531, 531, ButtonArchiveBase + i, GumpButtonType.Reply, 0);
            }

            AddLabel(769, 294, 1152, @"Atual");
            AddButton(739, 293, 531, 531, ButtonBackToCurrent, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID == 0)
            {
                if (!m_ShowArchiveList && m_ArchiveIndex < 0)
                    ReinoMilitarySystem.ArchiveAndClearCurrentReport(m_CityId, from);

                return;
            }

            int detailMode = m_DetailMode;
            int detailIndex = m_DetailIndex;

            switch (info.ButtonID)
            {
                case ButtonPrev:
                    if (detailIndex > 0)
                        detailIndex--;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, detailMode, detailIndex));
                    return;

                case ButtonNext:
                    int max = m_ArchiveIndex >= 0
                        ? ReinoMilitarySystem.GetArchivedDetailCount(m_CityId, m_ArchiveIndex, detailMode)
                        : ReinoMilitarySystem.GetDetailCount(m_CityId, detailMode);

                    if (detailIndex < Math.Max(0, max - 1))
                        detailIndex++;

                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, detailMode, detailIndex));
                    return;

                case ButtonCrimes:
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, 1, 0));
                    return;

                case ButtonPrisons:
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, 2, 0));
                    return;

                case ButtonWanted:
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, 3, 0));
                    return;

                case ButtonRecurring:
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, 4, 0));
                    return;

                case ButtonPrint:
                    if (m_ArchiveIndex >= 0)
                        ReinoMilitarySystem.BeginPrintArchivedReportBook(from, m_CityId, m_ArchiveIndex);
                    else
                        ReinoMilitarySystem.BeginPrintReportBook(from, m_CityId);

                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, m_ArchiveIndex, detailMode, detailIndex));
                    return;

                case ButtonOldReports:
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, true, -1, 0, 0));
                    return;

                case ButtonBackToCurrent:
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId));
                    return;
            }

            if (info.ButtonID >= ButtonArchiveBase && info.ButtonID < ButtonArchiveBase + 10)
            {
                int archiveIndex = info.ButtonID - ButtonArchiveBase;
                from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, false, archiveIndex, 1, 0));
            }
        }
    }

}
