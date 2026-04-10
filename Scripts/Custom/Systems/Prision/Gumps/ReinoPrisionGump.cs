using System;
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

        public ReinoPrisionGump(PlayerMobile from, int cityId) : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoPrisionSession session = ReinoPrisionSystem.GetSession(from);
            if (session.ViewedCellIndex < 0 || session.ViewedCellIndex > 4)
                session.ViewedCellIndex = 0;

            int remaining = ReinoPrisionSystem.GetRemainingHours(cityId, session.ViewedCellIndex);
            if (session.PendingRemainingHours <= 0)
                session.PendingRemainingHours = Math.Max(1, remaining);

            ReinoPrisionSettings settings = ReinoPrisionSystem.GetSettings(cityId);
            bool allLinked = ReinoPrisionSystem.AreAllCellDoorsLinked(cityId);
            ReinoPrisionerState inmate = ReinoPrisionSystem.GetInmateByCell(cityId, session.ViewedCellIndex);
            bool hasTribunal = ReinoTrialsSystem.HasTribunal(cityId);
            bool activeTribunalSession = hasTribunal && ReinoTrialsSystem.HasActiveSession(cityId);

            AddPage(0);
            AddImageTiled(339, 158, 422, 565, 387);
            AddImageTiled(318, 653, 78, 89, 359);
            AddImageTiled(712, 653, 74, 90, 360);
            AddImageTiled(712, 136, 74, 82, 361);
            AddImageTiled(318, 136, 74, 90, 362);
            AddImageTiled(322, 216, 26, 441, 365);
            AddImageTiled(757, 213, 26, 441, 366);
            AddImageTiled(387, 712, 326, 31, 367);
            AddImageTiled(384, 138, 328, 31, 368);
            AddImageTiled(556, 201, 184, 21, 470);
            AddImageTiled(367, 201, 178, 21, 469);
            AddLabel(483, 181, 0, @"Controle da Prisão");
            AddImageTiled(419, 201, 177, 21, 471);
            AddHtml(406, 327, 289, 183, ReinoPrisionSystem.GetPrisonHtml(cityId, session.ViewedCellIndex), false, true);

            AddButton(360, 401, 580, 580, ButtonPrevCell, GumpButtonType.Reply, 0);
            AddButton(716, 401, 581, 581, ButtonNextCell, GumpButtonType.Reply, 0);
            AddLabel(532, 295, 0, ReinoPrisionSystem.GetCellLabel(session.ViewedCellIndex));

            AddLabel(395, 246, 0, @"Alimentar Presos");
            AddButton(365, 245, settings.FeedPrisoners ? 530 : 531, settings.FeedPrisoners ? 530 : 531, ButtonFeed, GumpButtonType.Reply, 0);

            AddLabel(639, 246, 0, @"Liberar Multas");
            AddButton(611, 246, settings.AllowFinePayment ? 530 : 531, settings.AllowFinePayment ? 530 : 531, ButtonToggleFines, GumpButtonType.Reply, 0);

            AddLabel(420, 539, 0, @"Ajustar Pena");
            AddLabel(580, 539, 0, Math.Max(1, session.PendingRemainingHours).ToString());

            if (!hasTribunal)
            {
                AddButton(528, 537, 580, 580, ButtonMinusHours, GumpButtonType.Reply, 0);
                AddButton(618, 536, 581, 581, ButtonPlusHours, GumpButtonType.Reply, 0);
                AddButton(393, 537, 531, 248, ButtonApplyHours, GumpButtonType.Reply, 0);
            }

            string interrogationLabel = inmate != null && inmate.InInterrogation ? "Levar pra a cela" : "Levar Para Interrogatorio";
            AddLabel(420, 577, 0, interrogationLabel);
            AddButton(392, 577, 531, 248, ButtonInterrogation, GumpButtonType.Reply, 0);

            AddLabel(420, 607, 0, @"Soltar Preso");
            AddButton(392, 607, 531, 248, ButtonRelease, GumpButtonType.Reply, 0);

            AddLabel(420, 637, 0, settings.OuterDoorsLocked ? @"Abrir Portas" : @"Fechar Portas");
            AddButton(392, 637, 531, 248, ButtonToggleDoors, GumpButtonType.Reply, 0);

            if (hasTribunal && activeTribunalSession && inmate != null)
            {
                AddButton(580, 638, 531, 531, ButtonSendToTribunal, GumpButtonType.Reply, 0);
                AddLabel(607, 638, 0, @"Mandar para tribunal");
            }

            AddLabel(422, 667, 0, @"Abrir Cela");
            AddButton(392, 667, 531, 248, ButtonOpenCell, GumpButtonType.Reply, 0);

            if (!allLinked)
            {
                AddLabel(639, 667, 0, @"Linkar Portas");
                AddButton(612, 667, 531, 248, ButtonLinkDoor, GumpButtonType.Reply, 0);
            }

            AddImageTiled(347, 280, 408, 5, 368);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            ReinoPrisionSession session = ReinoPrisionSystem.GetSession(pm);
            string message = String.Empty;

            switch (info.ButtonID)
            {
                case ButtonFeed:
                    ReinoPrisionSystem.ToggleFeedPrisoners(m_CityId);
                    break;
                case ButtonToggleFines:
                    ReinoPrisionSystem.ToggleFinePayments(m_CityId);
                    break;
                case ButtonPrevCell:
                    if (session.ViewedCellIndex > 0)
                    {
                        session.ViewedCellIndex--;
                        session.PendingRemainingHours = Math.Max(1, ReinoPrisionSystem.GetRemainingHours(m_CityId, session.ViewedCellIndex));
                    }
                    break;
                case ButtonNextCell:
                    if (session.ViewedCellIndex < 4)
                    {
                        session.ViewedCellIndex++;
                        session.PendingRemainingHours = Math.Max(1, ReinoPrisionSystem.GetRemainingHours(m_CityId, session.ViewedCellIndex));
                    }
                    break;
                case ButtonMinusHours:
                    session.PendingRemainingHours = Math.Max(1, session.PendingRemainingHours - 1);
                    break;
                case ButtonSendToTribunal:
                    ReinoPrisionSystem.SendInmateToTribunal(m_CityId, session.ViewedCellIndex, out message);
                    if (!String.IsNullOrWhiteSpace(message))
                        pm.SendMessage(message);
                    break;
                case ButtonPlusHours:
                    session.PendingRemainingHours = Math.Min(720, session.PendingRemainingHours + 1);
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
                    pm.SendMessage(locked ? "As portas externas da prisão foram trancadas." : "As portas externas da prisão foram abertas.");
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
            }

            pm.SendGump(new ReinoPrisionGump(pm, m_CityId));
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
            AddLabel(447, 179, 0, @"Multa");
            AddLabel(441, 443, 0, @"Pagar Multa");
            AddButton(413, 443, 530, 248, 1, GumpButtonType.Reply, 0);
            AddHtml(370, 239, 210, 184, html, false, true);
        }

        private static string BuildFineHtml(ReinoPrisionerState inmate)
        {
            if (inmate == null)
                return "<BASEFONT COLOR=#000000>Nenhuma multa pendente.</BASEFONT>";

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
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
