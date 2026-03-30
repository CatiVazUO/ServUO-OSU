using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.HtmlBooks.Gumps
{
    public class BookSealPreviewGump : Gump
    {
        public BookSealPreviewGump(PlayerMobile pm, int sealId) : base(0, 0)
        {
            Closable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Moldura simples (igual seu estilo)
            AddImageTiled(457, 251, 171, 173, 375);
            AddImageTiled(622, 259, 25, 165, 369);
            AddImageTiled(438, 258, 26, 166, 370);
            AddImageTiled(457, 236, 171, 25, 371);
            AddImageTiled(459, 415, 168, 30, 372);
            AddImage(439, 238, 402);
            AddImage(605, 238, 402);
            AddImage(605, 400, 402);
            AddImage(439, 400, 402);

            AddLabel(521, 262, 1152, "Selador");
            AddLabel(525, 392, 0x481, string.Format("ID: {0}", sealId));

            // Mostra imagem só se for >= 1
            if (sealId >= 1)
                AddImage(485, 277, 2821 + sealId - 1);
            else
                AddLabel(585, 430, 0x481, "");
        }
    }
}
