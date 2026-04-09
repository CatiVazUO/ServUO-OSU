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
        private const int ButtonOldReports = 59007;

        private readonly PlayerMobile m_From;
        private readonly int m_CityId;
        private readonly int m_ArchiveIndex;

        public ReinoMilitaryReportsGump(PlayerMobile from, int cityId)
            : this(from, cityId, -1)
        {
        }

        public ReinoMilitaryReportsGump(PlayerMobile from, int cityId, int archiveIndex)
            : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;
            m_ArchiveIndex = archiveIndex;

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

            bool archived = m_ArchiveIndex >= 0;
            string title = archived ? ReinoMilitarySystem.GetArchivedReportTitle(cityId, m_ArchiveIndex) : @"Relatório do Reino";
            AddLabel(430, 180, 0, title);

            string summaryHtml = archived
                ? ReinoMilitarySystem.GetArchivedReportsSummaryHtml(cityId, m_ArchiveIndex)
                : ReinoMilitarySystem.GetReportsSummaryHtml(cityId);

            string detailHtml = archived
                ? ReinoMilitarySystem.GetArchivedReportDetailHtml(cityId, m_ArchiveIndex, session.DetailMode, session.DetailIndex)
                : ReinoMilitarySystem.GetReportsDetailHtml(cityId, session.DetailMode, session.DetailIndex);

            AddHtml(381, 244, 444, 123, summaryHtml, false, false);
            AddHtml(419, 462, 368, 196, detailHtml, false, true);

            int count = archived
                ? ReinoMilitarySystem.GetArchivedDetailCount(cityId, m_ArchiveIndex, session.DetailMode)
                : ReinoMilitarySystem.GetDetailCount(cityId, session.DetailMode);
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

            if (!archived)
            {
                AddButton(739, 293, 531, 531, ButtonOldReports, GumpButtonType.Reply, 0);
                AddLabel(769, 294, 0, @"Antigos");
            }

            AddLabel(537, 676, 0, @"Pegar Relatório Impresso");
            AddButton(507, 675, 531, 531, ButtonPrint, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);

            int archiveIndex = m_ArchiveIndex;
            int maxCount = archiveIndex >= 0
                ? ReinoMilitarySystem.GetArchivedDetailCount(m_CityId, archiveIndex, session.DetailMode)
                : ReinoMilitarySystem.GetDetailCount(m_CityId, session.DetailMode);

            switch (info.ButtonID)
            {
                case 0:
                    if (archiveIndex < 0)
                        ReinoMilitarySystem.ArchiveAndClearCurrentReport(m_CityId, from);
                    break;
                case ButtonPrev:
                    if (session.DetailIndex > 0)
                        session.DetailIndex--;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonNext:
                    if (session.DetailIndex < Math.Max(0, maxCount - 1))
                        session.DetailIndex++;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonCrimes:
                    session.DetailMode = 1;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonPrisons:
                    session.DetailMode = 2;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonWanted:
                    session.DetailMode = 3;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonRecurring:
                    session.DetailMode = 4;
                    session.DetailIndex = 0;
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonPrint:
                    if (archiveIndex >= 0)
                        ReinoMilitarySystem.BeginPrintArchivedReportBook(from, m_CityId, archiveIndex);
                    else
                        ReinoMilitarySystem.BeginPrintReportBook(from, m_CityId);
                    from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, archiveIndex));
                    break;
                case ButtonOldReports:
                    from.SendGump(new ReinoMilitaryArchivedReportsGump(from, m_CityId));
                    break;
            }
        }
    }


    public class ReinoMilitaryArchivedReportsGump : Gump
    {
        private const int ButtonBase = 59100;
        private readonly PlayerMobile m_From;
        private readonly int m_CityId;

        public ReinoMilitaryArchivedReportsGump(PlayerMobile from, int cityId)
            : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;

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

            AddLabel(541, 228, 0, @"Relatorios Antigos");

            System.Collections.Generic.List<ReinoArchivedReport> archives = ReinoMilitarySystem.GetArchivedReports(cityId);
            int y = 279;
            for (int i = 0; i < 10; i++)
            {
                if (i < archives.Count)
                {
                    AddButton(382, y - 2, 531, 248, ButtonBase + i, GumpButtonType.Reply, 0);
                    AddLabel(410, y, 0, ReinoMilitarySystem.GetArchivedReportListLabel(cityId, i));
                }
                y += 28;
            }

            if (archives.Count <= 0)
                AddLabel(410, 279, 0, @"Nenhum relatório antigo.");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID >= ButtonBase && info.ButtonID < ButtonBase + 10)
            {
                int index = info.ButtonID - ButtonBase;
                ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);
                session.DetailIndex = 0;
                if (session.DetailMode <= 0)
                    session.DetailMode = 1;
                from.SendGump(new ReinoMilitaryReportsGump(from, m_CityId, index));
            }
        }
    }
}
