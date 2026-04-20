using System;
using System.Text;
using Server;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Health.Gumps
{
    public class OSUSurgeryStatusGump : Gump
    {
        public OSUSurgeryStatusGump(Mobile viewer, PlayerMobile patient, OSUSurgeryProgressState progress) : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(422, 206, 365, 282, 392);
            AddImageTiled(395, 209, 40, 284, 631);
            AddImageTiled(773, 197, 40, 296, 631);
            AddImageTiled(419, 177, 364, 37, 630);
            AddImageTiled(416, 480, 368, 37, 630);
            AddImage(391, 173, 1315);
            AddImage(756, 170, 1316);
            AddImage(758, 465, 1317);
            AddImage(391, 465, 1318);

            AddLabel(540, 217, 1152, "Cirurgia de " + (patient != null ? patient.Name : "Paciente"));

            if (progress == null)
            {
                AddHtml(445, 262, 320, 180, "<BASEFONT COLOR=#FFFFFF>Sem cirurgia ativa.</BASEFONT>", false, true);
                return;
            }

            int remaining = Math.Max(0, (int)Math.Ceiling((progress.DeadlineUtc - DateTime.UtcNow).TotalSeconds));
            int total = Math.Max(1, (int)Math.Ceiling((progress.DeadlineUtc - progress.StartedUtc).TotalSeconds));
            int barWidth = Math.Max(0, Math.Min(300, (int)Math.Round((remaining / (double)total) * 300.0)));

            AddLabel(445, 245, 1152, "Tempo restante: " + remaining + "s");
            AddImageTiled(445, 285, 300, 12, 2053);
            if (barWidth > 0)
                AddImageTiled(445, 285, barWidth, 12, 2056);

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#FFFFFF>");
            sb.Append("<B>Lesão:</B> ").Append(OSUHealthSystem.GetDisplayName(progress.Injury)).Append("<BR>");
            sb.Append("<B>Severidade:</B> ").Append(OSUHealthSystem.GetSeverityDisplayName(OSUHealthSystem.GetSurgerySeverity(progress))).Append("<BR>");
            sb.Append("<B>Estado:</B> ").Append(OSUHealthSystem.GetSurgeryConditionLabel(progress)).Append("<BR><BR>");
            sb.Append("<B>Corte:</B> ").Append(progress.Cut).Append("<BR>");
            sb.Append("<B>Cauterização:</B> ").Append(progress.Heat).Append("<BR>");
            sb.Append("<B>Drenagem:</B> ").Append(progress.Bleed).Append("<BR><BR>");

            if (!String.IsNullOrWhiteSpace(progress.StatusText))
                sb.Append("<B>Procedimento atual:</B> ").Append(progress.StatusText).Append("<BR>");

            sb.Append("</BASEFONT>");
            AddHtml(445, 305, 320, 150, sb.ToString(), false, true);
        }
    }
}
