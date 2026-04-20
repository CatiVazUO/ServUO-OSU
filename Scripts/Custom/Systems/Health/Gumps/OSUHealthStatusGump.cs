
using System;
using System.Text;
using Server;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Health.Gumps
{
    public class OSUHealthStatusGump : Gump
    {
        public OSUHealthStatusGump(Mobile viewer, PlayerMobile target) : base(0, 0)
        {
            OSUHealthProfile profile = OSUHealthSystem.GetProfile(target, false);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            StringBuilder inj = new StringBuilder();
            StringBuilder dis = new StringBuilder();
            StringBuilder imm = new StringBuilder();

            if (profile == null)
            {
                inj.Append("<BASEFONT COLOR=#FFFFFF>Sem dados.</BASEFONT>");
                dis.Append("<BASEFONT COLOR=#FFFFFF>Sem dados.</BASEFONT>");
                imm.Append("<BASEFONT COLOR=#FFFFFF>Sem dados.</BASEFONT>");
            }
            else
            {
                if (profile.Injuries.Count == 0)
                {
                    inj.Append("<BASEFONT COLOR=#FFFFFF>Nenhuma lesão ativa.</BASEFONT>");
                }
                else
                {
                    for (int i = 0; i < profile.Injuries.Count; i++)
                    {
                        OSUInjuryState s = profile.Injuries[i];
                        inj.Append("<BASEFONT COLOR=#FFFFFF><B>");
                        inj.Append(OSUHealthSystem.GetDisplayName(s.Type));
                        inj.Append("</B><BR>");
                        inj.Append("Severidade: ");
                        inj.Append(OSUHealthSystem.GetSeverityDisplayName(s.Severity));
                        inj.Append("<BR>");
                        inj.Append("Tipo: ");
                        inj.Append(s.RequiresSurgery ? "Cirúrgica" : "Cura natural");
                        inj.Append("<BR><BR></BASEFONT>");
                    }
                }

                if (profile.Diseases.Count == 0)
                {
                    dis.Append("<BASEFONT COLOR=#FFFFFF>Nenhuma doença ativa.</BASEFONT>");
                }
                else
                {
                    for (int i = 0; i < profile.Diseases.Count; i++)
                    {
                        OSUDiseaseState s = profile.Diseases[i];
                        OSUHealthSystem.DiseaseDefinition def = OSUHealthSystem.GetDiseaseDefinition(s.Type);

                        dis.Append("<BASEFONT COLOR=#FFFFFF><B>");
                        dis.Append(OSUHealthSystem.GetDisplayName(s.Type));
                        dis.Append("</B><BR>");
                        if (def != null)
                        {
                            dis.Append(def.DiagnoseText);
                            dis.Append("<BR>");
                            dis.Append("Recuperação: ");
                            dis.Append(s.RecoveryCount);
                            dis.Append("/");
                            dis.Append(def.RecoveryTarget);
                        }
                        dis.Append("<BR><BR></BASEFONT>");
                    }
                }

                if (profile.Immunities.Count == 0)
                {
                    imm.Append("<BASEFONT COLOR=#FFFFFF>Nenhuma imunidade temporária.</BASEFONT>");
                }
                else
                {
                    for (int i = 0; i < profile.Immunities.Count; i++)
                    {
                        OSUImmunityState s = profile.Immunities[i];
                        imm.Append("<BASEFONT COLOR=#FFFFFF><B>");
                        imm.Append(OSUHealthSystem.GetDisplayName(s.Disease));
                        imm.Append("</B><BR>");
                        imm.Append("Proteção: ");
                        imm.Append((int)Math.Round(s.ReductionScalar * 100.0));
                        imm.Append("%<BR>");
                        imm.Append("Até: ");
                        imm.Append(s.EndsUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                        imm.Append("<BR><BR></BASEFONT>");
                    }
                }
            }

            AddPage(0);
            AddImageTiled(422, 206, 365, 394, 392);
            AddImageTiled(395, 209, 40, 396, 631);
            AddImageTiled(773, 197, 40, 408, 631);
            AddImageTiled(419, 177, 364, 37, 630);
            AddImageTiled(416, 592, 368, 37, 630);
            AddImage(391, 173, 1315);
            AddImage(756, 170, 1316);
            AddImage(758, 577, 1317);
            AddImage(391, 577, 1318);

            AddLabel(550, 217, 1152, "Saúde de " + (target != null ? target.Name : "Desconhecido"));
            AddImageTiled(435, 234, 335, 13, 630);

            AddLabel(444, 264, 1152, "Lesões");
            AddHtml(440, 289, 152, 289, inj.ToString(), false, true);

            AddLabel(618, 264, 1152, "Doenças");
            AddHtml(613, 288, 152, 149, dis.ToString(), false, true);

            AddLabel(618, 458, 1152, "Imunidades");
            AddHtml(613, 483, 152, 94, imm.ToString(), false, true);
        }
    }
}
