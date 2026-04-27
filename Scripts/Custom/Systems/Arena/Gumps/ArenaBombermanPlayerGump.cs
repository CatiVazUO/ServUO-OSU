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
            AddHtml(220, 214, 279, 127, @"<BASEFONT COLOR=#FFFFFF>Crie um atalho para [bomba. Objetivo: explodir os adversários.</BASEFONT>", false, false);
            AddLabel(226, 357, 0xFFFFFF, @"Tempo: 5s");
            AddLabel(227, 391, 0xFFFFFF, @"Alcance: 1");
            AddLabel(344, 357, 0xFFFFFF, @"Bombas: 1");
            AddLabel(345, 391, 0xFFFFFF, @"Movimento:");
        }
    }
}
