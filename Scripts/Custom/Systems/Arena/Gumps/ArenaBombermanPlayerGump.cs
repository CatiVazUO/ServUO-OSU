using System;
using Server.Gumps;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class ArenaBombermanPlayerGump : Gump
    {
        public ArenaBombermanPlayerGump() : base(0, 0)
        {
            Closable = false;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(199, 148, 328, 286, 375);
            AddImageTiled(515, 159, 25, 269, 369);
            AddImageTiled(178, 158, 26, 271, 370);
            AddImageTiled(194, 134, 331, 25, 371);
            AddImageTiled(205, 422, 304, 30, 372);
            AddImage(170, 125, 402);
            AddImage(502, 129, 402);
            AddImage(171, 419, 402);
            AddImage(502, 419, 402);
            AddHtml(220, 214, 279, 127, @"<BASEFONT COLOR=#FFFFFF>Crie um atalho para [bomba. Objetivo: explodir os adversários.</BASEFONT>", false, false);
            AddLabel(226, 357, 1152, @"Tempo: 5s");
            AddLabel(227, 391, 1152, @"Alcance: 1");
            AddLabel(344, 357, 1152, @"Bombas: 1");
            AddLabel(345, 391, 1152, @"Movimento:");
        }
    }
}
