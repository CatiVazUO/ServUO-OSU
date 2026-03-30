using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Rent
{
    public class CemeteryRentGump : Gump
    {
        private Mobile m_From;
        private TownHouseSign m_Sign;
        private int m_Index;
        private bool m_OwnedMode;

        public CemeteryRentGump(Mobile from, TownHouseSign sign, int index, bool ownedMode) : base(0, 0)
        {
            m_From = from;
            m_Sign = sign;
            m_Index = index;
            m_OwnedMode = ownedMode;

            List<TombstoneDefinition> defs = m_Sign.GetAvailableTombDefinitions();

            if (defs == null || defs.Count == 0)
                defs = new List<TombstoneDefinition>();

            if (m_OwnedMode && m_Sign.TombSelectedDefinition != null)
            {
                TombstoneDefinition selected = m_Sign.TombSelectedDefinition;
                int found = defs.IndexOf(selected);

                if (found >= 0)
                    m_Index = found;
            }

            if (m_Index < 0)
                m_Index = 0;

            if (m_Index >= defs.Count && defs.Count > 0)
                m_Index = defs.Count - 1;

            TombstoneDefinition def = defs.Count > 0 ? defs[m_Index] : null;

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

            AddLabel(430, 147, 0, m_OwnedMode ? "Gerenciar Lápide" : "Alugar Lápide");
            AddImage(293, 163, 443);
            AddImage(293, 250, 443);

            if (def != null)
            {
                AddImage(745, 91, def.GumpID);

                string colorPrefix = "<BASEFONT COLOR=" + def.TextColorHtml + ">";
                string colorSuffix = "</BASEFONT>";

                string previewName;
                string previewDate;
                string previewMessage;

                if (m_OwnedMode)
                {
                    previewName = m_Sign.GetTombDisplayName();
                    previewDate = m_Sign.GetTombDisplayDate();
                    previewMessage = m_Sign.GetTombDisplayMessage();
                }
                else
                {
                    previewName = FitPreviewText("<CENTER><BIG>João Ninguém</BIG></CENTER>", def.MaxNameLength);
                    previewMessage = FitPreviewText("Morreu bravamente mas ninguém sabia seu nome", def.MaxMessageLength);

                    string birth = FitPreviewText("181", def.MaxDateLength);
                    string death = FitPreviewText("216", def.MaxDateLength);

                    if (def.DateLayout == TombstoneDateLayout.Stacked)
                        previewDate = birth + "<BR>" + death;
                    else
                        previewDate = birth + " - " + death;
                }

                if (def.HasName)
                {
                    AddHtml(
                        def.NameX,
                        def.NameY,
                        def.NameWidth,
                        def.NameHeight,
                        colorPrefix + "<CENTER><BIG>" + previewName + "</BIG></CENTER>" + colorSuffix,
                        false,
                        false);
                }

                if (def.HasDate)
                {
                    AddHtml(
                        def.DateX,
                        def.DateY,
                        def.DateWidth,
                        def.DateHeight,
                        colorPrefix + "<CENTER>" + previewDate + "</CENTER>" + colorSuffix,
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
                        colorPrefix + "<CENTER>" + previewMessage + "</CENTER>" + colorSuffix,
                        false,
                        false);
                }

                AddItem(464, 206, def.ItemID);

                // Só mostra botões de trocar tipo se ainda não foi alugada
                if (!m_OwnedMode)
                {
                    AddButton(424, 207, 451, 451, 1, GumpButtonType.Reply, 0);
                    AddButton(531, 205, 450, 450, 2, GumpButtonType.Reply, 0);
                }

                AddLabel(305, 290, 0, "Valor Total: " + (m_Sign.Price + def.ExtraCost));
                AddLabel(305, 320, 0, "Valor do aluguel pago ao cemitério: " + m_Sign.Price);
                AddLabel(305, 350, 0, "Número de Lockdowns: " + m_Sign.Locks);

                string fields = BuildFieldsText(def);
                AddHtml(305, 375, 368, 95, fields, false, false);

                if (m_OwnedMode)
                {
                    AddButton(455, 499, 559, 559, 20, GumpButtonType.Reply, 0);
                    AddLabel(442, 476, 0, "Cancelar Aluguel");
                }
                else
                {
                    AddButton(455, 499, 559, 559, 10, GumpButtonType.Reply, 0);
                    AddLabel(451, 476, 0, "Alugar Lápide");
                }
            }
            else
            {
                AddHtml(317, 287, 368, 165, "Nenhuma definição de lápide encontrada para este tipo.", false, false);
            }
        }

        private string FitPreviewText(string text, int max)
        {
            if (String.IsNullOrEmpty(text))
                return "";

            if (max <= 0)
                return "";

            text = text.Trim();

            if (text.Length > max)
                text = text.Substring(0, max);

            return text;
        }

        public CemeteryRentGump(Mobile from, TownHouseSign sign, int index) : this(from, sign, index, false)
        {
        }

        private string BuildFieldsText(TombstoneDefinition def)
        {
            if (def == null)
                return "";

            if (m_OwnedMode)
            {
                string html = "<BASEFONT COLOR=#0x000000>";

                if (def.HasName)
                    html += "Nome: " + m_Sign.GetTombDisplayName() + "<BR>";

                if (def.HasDate)
                    html += "Data: " + m_Sign.GetTombDisplayBirthYear() + " - " + m_Sign.GetTombDisplayDeathYear() + "<BR>";

                if (def.HasMessage)
                    html += "Mensagem: " + m_Sign.GetTombDisplayMessage() + "<BR>";

                html += "</BASEFONT>";
                return html;
            }

            List<string> fields = new List<string>();

            if (def.HasName)
                fields.Add("Nome");
            if (def.HasDate)
                fields.Add("Data");
            if (def.HasMessage)
                fields.Add("Mensagem");

            string list = fields.Count > 0 ? String.Join(", ", fields.ToArray()) : "Nenhum";

            return String.Format(
                "<BASEFONT COLOR=#000000>Valor da Lápide: {0}<BR><BR>Custo extra: {1}</BASEFONT>",
                list,
                def.ExtraCost);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null || m_Sign == null || m_Sign.Deleted)
                return;

            List<TombstoneDefinition> defs = m_Sign.GetAvailableTombDefinitions();

            switch (info.ButtonID)
            {
                case 1: // anterior
                    if (!m_OwnedMode && defs != null && defs.Count > 0)
                    {
                        int index = m_Index - 1;
                        if (index < 0)
                            index = defs.Count - 1;

                        from.SendGump(new CemeteryRentGump(from, m_Sign, index, false));
                    }
                    break;

                case 2: // próximo
                    if (!m_OwnedMode && defs != null && defs.Count > 0)
                    {
                        int index = m_Index + 1;
                        if (index >= defs.Count)
                            index = 0;

                        from.SendGump(new CemeteryRentGump(from, m_Sign, index, false));
                    }
                    break;

                case 10: // alugar
                    if (!m_OwnedMode && defs != null && defs.Count > 0)
                    {
                        TombstoneDefinition def = defs[m_Index];
                        m_Sign.RentTomb(from, def);
                    }
                    break;

                case 20: // cancelar aluguel
                    if (m_OwnedMode)
                    {
                        if (m_Sign.House != null && !m_Sign.House.Deleted)
                            m_Sign.House.Delete();

                        m_Sign.ResetTombState();
                        m_Sign.Visible = true;

                        from.SendMessage("A lápide foi desalugada e voltou a estar disponível.");
                    }
                    break;
            }
        }
    }
}
