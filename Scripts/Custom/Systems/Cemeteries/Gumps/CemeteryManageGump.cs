using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Rent
{
    public class CemeteryManageGump : Gump
    {
        private readonly TownHouseSign m_Sign;
        private Mobile m_From;

        public CemeteryManageGump(Mobile from, TownHouseSign sign) : this(from, sign, sign.TombDeadName, sign.TombBirthYear, sign.TombDeathYear, sign.TombMessage)
        {
        }

        public CemeteryManageGump(Mobile from, TownHouseSign sign, string deadName, string birthYear, string deathYear, string message) : base(0, 0)
        {
            m_From = from;
            m_Sign = sign;

            TombstoneDefinition def = m_Sign.TombSelectedDefinition;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(275, 131, 450, 414, 398);
            AddImageTiled(716, 138, 25, 406, 369);
            AddImageTiled(254, 140, 26, 403, 370);
            AddImageTiled(269, 115, 450, 25, 371);
            AddImageTiled(281, 534, 443, 30, 372);
            AddImage(246, 107, 415);
            AddImage(679, 105, 414);
            AddImage(249, 500, 412);
            AddImage(680, 498, 413);

            AddLabel(453, 147, 0, "OSU Tumba");
            AddImage(293, 163, 443);

            AddLabel(305, 200, 0, "Valor pago pela Tumba: " + m_Sign.GetTombInitialCost());
            AddLabel(305, 230, 0, "Valor semanal pago ao cemitério: " + m_Sign.GetTombWeeklyRent());
            AddLabel(305, 260, 0, "Número de Lockdowns: " + m_Sign.Locks);

            AddImage(745, 91, def != null ? def.GumpID : 570);

            // preview dentro do próprio manage gump
            if (def != null)
            {
                string colorPrefix = "<BASEFONT COLOR=" + def.TextColorHtml + ">";
                string colorSuffix = "</BASEFONT>";

                if (def.HasName)
                {
                    AddHtml(
                        def.NameX,
                        def.NameY,
                        def.NameWidth,
                        def.NameHeight,
                        colorPrefix + "<CENTER><BIG>" + m_Sign.FitTombText(deadName, def.MaxNameLength) + "</BIG></CENTER>" + colorSuffix,
                        false,
                        false);
                }

                if (def.HasDate)
                {
                    string dateText;

                    if (def.DateLayout == TombstoneDateLayout.Stacked)
                        dateText = m_Sign.FitTombText(birthYear, def.MaxDateLength) + "<BR>" + m_Sign.FitTombText(deathYear, def.MaxDateLength);
                    else
                        dateText = m_Sign.FitTombText(birthYear, def.MaxDateLength) + " - " + m_Sign.FitTombText(deathYear, def.MaxDateLength);

                    AddHtml(
                        def.DateX,
                        def.DateY,
                        def.DateWidth,
                        def.DateHeight,
                        colorPrefix + "<CENTER><BIG>" + dateText + "</BIG></CENTER>" + colorSuffix,
                        false,
                        false);
                }


                if (def.HasMessage)
                {
                    AddHtml(
                        def.MessageX,
                        def.MessageY,
                        def.MessageWidth,
                        def.MessageHeight,
                        colorPrefix + "<CENTER><BIG>" + m_Sign.FitTombText(message, def.MaxMessageLength) + "</BIG></CENTER>" + colorSuffix,
                        false,
                        false);
                }
            }

            int y = 330;

            if (def != null && def.HasName)
            {
                AddLabel(309, y, 0, "Nome:");
                AddTextEntry(390, y, 290, 20, 0, 1, deadName ?? "");
                y += 30;
            }

            if (def != null && def.HasDate)
            {
                AddLabel(309, y, 0, "Nascimento:");
                AddTextEntry(390, y, 120, 20, 0, 2, birthYear ?? "");
                y += 30;

                AddLabel(309, y, 0, "Morte:");
                AddTextEntry(390, y, 120, 20, 0, 3, deathYear ?? "");
                y += 30;
            }

            if (def != null && def.HasMessage)
            {
                AddLabel(309, y, 0, "Mensagem:");
                AddTextEntry(390, y, 290, 60, 0, 4, message ?? "");
                y += 70;
            }

            AddButton(306, 497, 535, 535, 5, GumpButtonType.Reply, 0);
            AddLabel(342, 497, 0, "Ver no Gump");

            AddButton(597, 501, 559, 559, 6, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null || m_Sign == null || m_Sign.Deleted)
                return;

            string deadName = "";
            string birthYear = "";
            string deathYear = "";
            string message = "";

            if (info.GetTextEntry(1) != null)
                deadName = info.GetTextEntry(1).Text.Trim();

            if (info.GetTextEntry(2) != null)
                birthYear = info.GetTextEntry(2).Text.Trim();

            if (info.GetTextEntry(3) != null)
                deathYear = info.GetTextEntry(3).Text.Trim();

            if (info.GetTextEntry(4) != null)
                message = info.GetTextEntry(4).Text.Trim();

            switch (info.ButtonID)
            {
                case 5: // atualizar preview interno
                    from.SendGump(new CemeteryManageGump(from, m_Sign, deadName, birthYear, deathYear, message));
                    break;

                case 6: // confirmar
                    {
                        TombstoneDefinition def = m_Sign.TombSelectedDefinition;

                      /*  if (def != null && def.HasDate)
                        {
                            if (!ValidateYears(from, birthYear, deathYear))
                            {
                                from.SendGump(new CemeteryManageGump(from, m_Sign, deadName, birthYear, deathYear, message));
                                return;
                            }
                        */

                        m_Sign.FinalizeTomb(deadName, birthYear, deathYear, message);
                        from.SendGump(new CemeteryPreviewGump(from, m_Sign));
                        break;
                    }
            }
        }

       /* private bool ValidateYears(Mobile from, string birthYear, string deathYear)
        {
            int b, d;

            if (!Int32.TryParse(birthYear, out b))
            {
                from.SendMessage("O ano de nascimento deve conter apenas números.");
                return false;
            }

            if (!Int32.TryParse(deathYear, out d))
            {
                from.SendMessage("O ano de morte deve conter apenas números.");
                return false;
            }

            int currentYear = OSU.Cemeteries.CemeteryHelpers.CurrentShardYearProvider();

            if (d > currentYear)
            {
                from.SendMessage("A data de morte não pode estar no futuro.");
                return false;
            }

            if (b > d)
            {
                from.SendMessage("O ano de nascimento não pode ser maior que o ano de morte.");
                return false;
            }

            return true;
        }*/
    }
}
