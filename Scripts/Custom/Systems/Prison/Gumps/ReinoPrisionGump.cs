using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Custom.Reinos
{
    public class ReinoPrisionGump : Gump
    {
        private readonly PlayerMobile m_From;
        private readonly int m_CityId;

        private const int ButtonFeed = 1;
        private const int ButtonToggleFines = 2;
        private const int ButtonPrevCell = 3;
        private const int ButtonNextCell = 4;
        private const int ButtonMinusHours = 5;
        private const int ButtonPlusHours = 6;
        private const int ButtonApplyHours = 7;
        private const int ButtonInterrogation = 8;
        private const int ButtonRelease = 9;
        private const int ButtonToggleDoors = 10;
        private const int ButtonOpenCell = 11;
        private const int ButtonLinkDoor = 12;
        private const int ButtonSendToTribunal = 13;
        private const int ButtonArrest = 14;
        private const int ButtonChargeFine = 15;

        private const int WhiteHue = 1152;

        public ReinoPrisionGump(PlayerMobile from, int cityId) : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoPrisionSession session = ReinoPrisionSystem.GetSession(from);
            ReinoPrisionSettings settings = ReinoPrisionSystem.GetSettings(cityId);
            List<int> occupied = ReinoPrisionSystem.GetOccupiedCellIndices(cityId);
            bool allLinked = ReinoPrisionSystem.AreAllCellDoorsLinked(cityId);

            if (allLinked && occupied.Count > 0)
            {
                if (!occupied.Contains(session.ViewedCellIndex))
                    session.ViewedCellIndex = occupied[0];
            }
            else if (session.ViewedCellIndex < 0 || session.ViewedCellIndex > 4)
            {
                session.ViewedCellIndex = 0;
            }

            int remaining = ReinoPrisionSystem.GetRemainingHours(cityId, session.ViewedCellIndex);
            if (session.PendingRemainingHours < 0)
                session.PendingRemainingHours = Math.Max(0, remaining);

            ReinoPrisionerState inmate = ReinoPrisionSystem.GetInmateByCell(cityId, session.ViewedCellIndex);
            bool hasTribunal = ReinoPrisionSystem.HasTribunal(cityId);
            bool currentCellLinked = ReinoPrisionSystem.IsCellDoorLinked(cityId, session.ViewedCellIndex);
            bool currentCellOpen = ReinoPrisionSystem.IsCellDoorOpen(cityId, session.ViewedCellIndex);

            int occupiedIndex = occupied.IndexOf(session.ViewedCellIndex);
            bool canPrev = !allLinked
                ? session.ViewedCellIndex > 0
                : occupied.Count > 1 && occupiedIndex > 0;

            bool canNext = !allLinked
                ? session.ViewedCellIndex < 4
                : occupied.Count > 1 && occupiedIndex >= 0 && occupiedIndex < occupied.Count - 1;


            AddPage(0);
            AddImageTiled(338, 160, 422, 565, 387);
            AddImageTiled(318, 653, 78, 89, 359);
            AddImageTiled(711, 655, 74, 90, 360);
            AddImageTiled(712, 136, 74, 82, 361);
            AddImageTiled(318, 136, 74, 90, 362);
            AddImageTiled(322, 216, 26, 441, 365);
            AddImageTiled(757, 215, 26, 441, 366);
            AddImageTiled(387, 712, 326, 31, 367);
            AddImageTiled(384, 138, 328, 31, 368);
            AddImageTiled(556, 201, 184, 21, 470);
            AddImageTiled(367, 201, 178, 21, 469);
            AddImageTiled(419, 201, 177, 21, 471);

            AddLabel(483, 181, WhiteHue, @"Controle da Prisão");
            AddHtml(406, 343, 289, 183, ReinoPrisionSystem.GetPrisonHtml(cityId, session.ViewedCellIndex), false, false);

            if (canPrev)
                AddButton(360, 417, 580, 580, ButtonPrevCell, GumpButtonType.Reply, 0);

            if (canNext)
                AddButton(716, 417, 581, 581, ButtonNextCell, GumpButtonType.Reply, 0);

            AddLabel(532, 311, WhiteHue, ReinoPrisionSystem.GetCellLabel(session.ViewedCellIndex));

            AddLabel(395, 233, WhiteHue, @"Alimentar Presos");
            AddButton(365, 232,
                settings.FeedPrisoners ? 530 : 531,
                settings.FeedPrisoners ? 530 : 531,
                ButtonFeed, GumpButtonType.Reply, 0);

            AddLabel(639, 233, WhiteHue, @"Liberar Multas");
            AddButton(611, 233,
                settings.AllowFinePayment ? 530 : 531,
                settings.AllowFinePayment ? 530 : 531,
                ButtonToggleFines, GumpButtonType.Reply, 0);

            AddLabel(395, 261, WhiteHue, @"Prender");
            AddButton(365, 260, 531, 248, ButtonArrest, GumpButtonType.Reply, 0);

            AddLabel(641, 261, WhiteHue, @"Cobrar Multa");
            AddButton(611, 260, 531, 248, ButtonChargeFine, GumpButtonType.Reply, 0);

            if (!hasTribunal)
            {
                AddLabel(421, 547, WhiteHue, @"Ajustar Pena");
                AddButton(393, 545, 531, 248, ButtonApplyHours, GumpButtonType.Reply, 0);
                AddButton(528, 545, 580, 580, ButtonMinusHours, GumpButtonType.Reply, 0);
                AddButton(618, 544, 581, 581, ButtonPlusHours, GumpButtonType.Reply, 0);
                AddLabel(580, 547, WhiteHue, Math.Max(0, session.PendingRemainingHours).ToString());
            }

            string interrogationLabel = inmate != null && inmate.InInterrogation
                ? "Levar pra a cela"
                : "Levar Para Interrogatorio";

            AddLabel(420, 585, WhiteHue, interrogationLabel);
            AddButton(392, 585, 531, 531, ButtonInterrogation, GumpButtonType.Reply, 0);

            AddLabel(420, 615, WhiteHue, @"Soltar Preso");
            AddButton(392, 615, 531, 531, ButtonRelease, GumpButtonType.Reply, 0);

            AddLabel(420, 645, WhiteHue, settings.OuterDoorsLocked ? @"Abrir Portas" : @"Fechar Portas");
            AddButton(392, 645, 531, 531, ButtonToggleDoors, GumpButtonType.Reply, 0);

            AddLabel(422, 675, WhiteHue, currentCellOpen ? @"Fechar Cela" : @"Abrir Cela");
            AddButton(392, 675, 531, 531, ButtonOpenCell, GumpButtonType.Reply, 0);

            if (!currentCellLinked)
            {
                AddLabel(640, 645, WhiteHue, @"Linkar Portas");
                AddButton(612, 645, 531, 531, ButtonLinkDoor, GumpButtonType.Reply, 0);
            }

            if (hasTribunal)
            {
                AddLabel(640, 675, WhiteHue, @"Mandar para tribunal");
                AddButton(612, 675, 531, 531, ButtonSendToTribunal, GumpButtonType.Reply, 0);     
            }

            AddImageTiled(351, 295, 404, 5, 368);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            if (info.ButtonID == 0)
                return;

            ReinoPrisionSession session = ReinoPrisionSystem.GetSession(pm);
            List<int> occupied = ReinoPrisionSystem.GetOccupiedCellIndices(m_CityId);

            bool allLinked = ReinoPrisionSystem.AreAllCellDoorsLinked(m_CityId);

            if (allLinked && occupied.Count > 0 && !occupied.Contains(session.ViewedCellIndex))
                session.ViewedCellIndex = occupied[0];

            string message = String.Empty;

            bool preservePendingHours = false;

            switch (info.ButtonID)
            {
                case ButtonFeed:
                    ReinoPrisionSystem.ToggleFeedPrisoners(m_CityId);
                    break;
                case ButtonToggleFines:
                    ReinoPrisionSystem.ToggleFinePayments(m_CityId);
                    break;
                case ButtonPrevCell:
                    MoveAcrossOccupiedCells(session, occupied, -1, m_CityId);
                    break;
                case ButtonNextCell:
                    MoveAcrossOccupiedCells(session, occupied, 1, m_CityId);
                    break;
                case ButtonMinusHours:
                    session.PendingRemainingHours = Math.Max(0, session.PendingRemainingHours - 1);
                    preservePendingHours = true;
                    break;

                case ButtonPlusHours:
                    session.PendingRemainingHours = Math.Min(720, session.PendingRemainingHours + 1);
                    preservePendingHours = true;
                    break;
                case ButtonApplyHours:
                    ReinoPrisionSystem.AdjustRemainingHours(m_CityId, session.ViewedCellIndex, session.PendingRemainingHours, pm.Name, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
                case ButtonInterrogation:
                    ReinoPrisionSystem.ToggleInterrogation(m_CityId, session.ViewedCellIndex, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
                case ButtonRelease:
                    ReinoPrisionSystem.ReleaseInmate(m_CityId, session.ViewedCellIndex, pm.Name, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
                case ButtonToggleDoors:
                    bool locked = ReinoPrisionSystem.ToggleOuterDoors(m_CityId);
                    pm.SendMessage(locked ? "As portas externas da prisão foram trancadas." : "As portas externas da prisão foram destrancadas.");
                    break;
                case ButtonOpenCell:
                    ReinoPrisionSystem.OpenCellDoor(m_CityId, session.ViewedCellIndex, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
                case ButtonLinkDoor:
                    pm.Target = new PrisonCellDoorTarget(m_CityId, session.ViewedCellIndex);
                    pm.SendMessage("Selecione a porta da cela atual.");
                    return;
                case ButtonSendToTribunal:
                    ReinoPrisionSystem.SendInmateToTribunal(m_CityId, session.ViewedCellIndex, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
                case ButtonArrest:
                    pm.Target = new PrisonAdministrativeArrestTarget(m_CityId);
                    pm.SendMessage("Selecione o jogador que deseja prender.");
                    return;
                case ButtonChargeFine:
                    ReinoPrisionSystem.ResendFineGumpToViewedInmate(m_CityId, session.ViewedCellIndex, pm, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
            }

            if (ReinoPrisionSystem.GetInmateByCell(m_CityId, session.ViewedCellIndex) == null)
            {
                occupied = ReinoPrisionSystem.GetOccupiedCellIndices(m_CityId);
                if (occupied.Count > 0)
                    session.ViewedCellIndex = occupied[0];
            }

            if (!preservePendingHours)
                session.PendingRemainingHours = Math.Max(0, ReinoPrisionSystem.GetRemainingHours(m_CityId, session.ViewedCellIndex));

            pm.CloseGump(typeof(ReinoPrisionGump));
            pm.SendGump(new ReinoPrisionGump(pm, m_CityId));
        }

        private static void MoveAcrossOccupiedCells(ReinoPrisionSession session, List<int> occupied, int delta, int cityId)
        {
            if (session == null)
                return;

            if (!ReinoPrisionSystem.AreAllCellDoorsLinked(cityId))
            {
                session.ViewedCellIndex = Math.Max(0, Math.Min(4, session.ViewedCellIndex + delta));
                session.PendingRemainingHours = Math.Max(0, ReinoPrisionSystem.GetRemainingHours(cityId, session.ViewedCellIndex));
                return;
            }

            if (occupied == null || occupied.Count <= 1)
                return;

            int index = occupied.IndexOf(session.ViewedCellIndex);
            if (index < 0)
                index = 0;

            index += delta;

            if (index < 0)
                index = 0;

            if (index >= occupied.Count)
                index = occupied.Count - 1;

            session.ViewedCellIndex = occupied[index];
            session.PendingRemainingHours = Math.Max(0, ReinoPrisionSystem.GetRemainingHours(cityId, session.ViewedCellIndex));
        }

        private class PrisonAdministrativeArrestTarget : Target
        {
            private readonly int m_CityId;

            public PrisonAdministrativeArrestTarget(int cityId) : base(12, false, TargetFlags.None)
            {
                m_CityId = cityId;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile jailer = from as PlayerMobile;
                PlayerMobile target = targeted as PlayerMobile;

                if (jailer == null)
                    return;

                if (target == null || target.Deleted)
                {
                    jailer.SendMessage("Você deve selecionar um jogador.");
                    jailer.CloseGump(typeof(ReinoPrisionGump));
                    jailer.SendGump(new ReinoPrisionGump(jailer, m_CityId));
                    return;
                }

                string message;
                if (ReinoPrisionSystem.TryAdministrativeArrest(target, m_CityId, jailer, out message))
                    jailer.SendMessage(message);
                else
                    jailer.SendMessage(message);

                jailer.CloseGump(typeof(ReinoPrisionGump));
                jailer.SendGump(new ReinoPrisionGump(jailer, m_CityId));
            }
        }

        private class PrisonCellDoorTarget : Target
        {
            private readonly int m_CityId;
            private readonly int m_CellIndex;

            public PrisonCellDoorTarget(int cityId, int cellIndex) : base(12, false, TargetFlags.None)
            {
                m_CityId = cityId;
                m_CellIndex = cellIndex;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null)
                    return;

                string message;
                if (!ReinoPrisionSystem.LinkCellDoor(m_CityId, m_CellIndex, targeted as Item, out message))
                    pm.SendMessage(message);
                else
                    pm.SendMessage(message);

                pm.CloseGump(typeof(ReinoPrisionGump));
                pm.SendGump(new ReinoPrisionGump(pm, m_CityId));
            }
        }
    }

    public class ReinoPrisionFineGump : Gump
    {
        private readonly int m_CityId;
        private readonly int m_PrisonerSerial;

        public ReinoPrisionFineGump(PlayerMobile from, int cityId, int prisonerSerial) : base(0, 0)
        {
            m_CityId = cityId;
            m_PrisonerSerial = prisonerSerial;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoPrisionerState inmate = ReinoPrisionSystem.GetInmateBySerial(cityId, prisonerSerial);
            string html = BuildFineHtml(inmate);

            AddPage(0);
            AddImageTiled(337, 166, 274, 317, 387);
            AddImageTiled(317, 420, 78, 89, 359);
            AddImageTiled(557, 420, 74, 90, 360);
            AddImageTiled(557, 137, 74, 82, 361);
            AddImageTiled(317, 137, 74, 90, 362);
            AddImageTiled(321, 217, 26, 207, 365);
            AddImageTiled(602, 214, 26, 209, 366);
            AddImageTiled(386, 479, 174, 31, 367);
            AddImageTiled(383, 139, 201, 31, 368);
            AddImageTiled(395, 202, 180, 21, 470);
            AddImageTiled(366, 202, 178, 21, 469);
            AddLabel(447, 179, 1152, @"Multa");
            AddLabel(441, 443, 1152, @"Pagar Multa");
            AddButton(413, 443, 530, 248, 1, GumpButtonType.Reply, 0);
            AddHtml(370, 239, 210, 184, html, false, true);
        }

        private static string BuildFineHtml(ReinoPrisionerState inmate)
        {
            if (inmate == null)
                return "<BASEFONT COLOR=#FFFFFF>Nenhuma multa pendente.</BASEFONT>";

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#FFFFFF>");
            sb.Append("<B>Valor:</B> ").Append(inmate.FineGold).Append(" moedas<BR>");

            if (inmate.Judged && !String.IsNullOrWhiteSpace(inmate.JudgeName))
                sb.Append("<B>Quem decretou:</B> ").Append(inmate.JudgeName).Append("<BR>");

            sb.Append("<B>Crime:</B> ").Append(String.IsNullOrWhiteSpace(inmate.CrimeLabel) ? "não informado" : inmate.CrimeLabel).Append("<BR>");
            sb.Append("<B>Pena:</B> ").Append(inmate.SentenceHours).Append(" hora(s)");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            if (info.ButtonID == 1)
            {
                string msg;
                if (!ReinoPrisionSystem.ConsumeFineAndRelease(pm, m_CityId, out msg))
                    pm.SendMessage(msg);
                else
                    pm.SendMessage(msg);
            }
        }
    }
}
