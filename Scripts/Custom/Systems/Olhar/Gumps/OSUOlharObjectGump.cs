using System;
using Server;
using Server.Gumps;

namespace Server.Custom.Systems.Olhar.Gumps
{
    public class OSUOlharObjectGump : Gump
    {
        private readonly Mobile _viewer;
        private readonly string _text;

        public OSUOlharObjectGump(Mobile viewer, string text) : base(0, 0)
        {
            _viewer = viewer;
            _text = text;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Layout baseado no seu "olharobjto"
            AddImageTiled(226, 93, 551, 440, 376);
            AddImage(214, 457, 359);
            AddImage(717, 457, 360);
            AddImage(717, 84, 361);
            AddImage(214, 84, 362);
            AddImageTiled(285, 516, 433, 30, 367);
            AddImageTiled(286, 86, 431, 30, 368);
            AddImageTiled(218, 169, 27, 290, 365);
            AddImageTiled(762, 167, 26, 292, 366);

            AddLabel(480, 128, 0, "Olhar");

            string msg = _text;
            if (String.IsNullOrWhiteSpace(msg))
                msg = "Esse objeto não tem nada de especial.";

            AddHtml(270, 177, 464, 309, $"<BASEFONT COLOR=#FFFFFF>{msg}</BASEFONT>", false, true);
        }
    }
}
