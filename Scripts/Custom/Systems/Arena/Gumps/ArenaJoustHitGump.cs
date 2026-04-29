using Server.Gumps;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class JoustHitGump : Gump
    {
        public JoustHitGump(int lanceCur, int lanceMax, int shieldCur, int shieldMax, int armorCur, int armorMax) : base(0, 0)
        {
            Closable = false;
            Dragable = true;
            Disposable = true;
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

            AddLabel(313, 163, 1152, "Arena - Justa");
            AddHtml(220, 205, 280, 58, "<BASEFONT COLOR=#FFFFFF>Use o comando [lanca para atacar quando o adversário estiver paralelo.</BASEFONT>", false, false);
            AddLabel(223, 283, 1152, "Lança:");
            AddLabel(300, 283, 1152, lanceCur + "/" + lanceMax);
            AddLabel(223, 315, 1152, "Escudo:");
            AddLabel(300, 315, 1152, shieldCur + "/" + shieldMax);
            AddLabel(223, 347, 1152, "Armadura:");
            AddLabel(320, 347, 1152, armorCur + "/" + armorMax);
        }
    }
}
