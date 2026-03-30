using System;
using Server;
using Server.Gumps;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Rent
{
    public class CemeteryPreviewGump : Gump
    {
        public CemeteryPreviewGump(Mobile from, TownHouseSign sign) : base(0, 0)
        {
            TombstoneDefinition def = sign.TombSelectedDefinition;
            int gumpID = def != null ? def.GumpID : 570;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(745, 91, gumpID);

            if (def == null)
                return;

            string start = "<BASEFONT COLOR=" + def.TextColorHtml + ">";
            string end = "</BASEFONT>";

            if (def.HasName)
            {
                AddHtml(def.NameX, def.NameY, def.NameWidth, def.NameHeight,
                    start + Center(sign.TombDeadName) + end, false, false);
            }

            if (def.HasDate)
            {
                AddHtml(def.DateX, def.DateY, def.DateWidth, def.DateHeight,
                    start + Center(BuildDate(def, sign.TombBirthYear, sign.TombDeathYear)) + end, false, false);
            }

            if (def.HasMessage)
            {
                AddHtml(def.MessageX, def.MessageY, def.MessageWidth, def.MessageHeight,
                    start + Center(sign.TombMessage) + end, false, false);
            }
        }

        private string BuildDate(TombstoneDefinition def, string birth, string death)
        {
            if (def.DateLayout == TombstoneDateLayout.Stacked)
                return String.Format("{0}<BR>{1}", birth, death);

            return String.Format("{0} - {1}", birth, death);
        }

        private string Center(string text)
        {
            if (String.IsNullOrEmpty(text))
                text = String.Empty;

            return "<CENTER>" + text + "</CENTER>";
        }
    }
}
