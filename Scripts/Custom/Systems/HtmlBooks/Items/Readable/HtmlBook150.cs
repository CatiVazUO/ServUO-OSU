using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook150 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 245; } }
        public override int HtmlHeight { get { return 327; } }
        public override int MailCostPerSubscriber { get { return 25; } }

        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 150;

        [Constructable]
        public HtmlBook150()
        {
            ItemID = 0xFEF;
            Name = "Livro (150 páginas)";
            Weight = 3.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3510;

            // use o tamanho do HTML do item;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
            l.BookImageX = 42;
            l.BookImageY = 260;

            // ====== PREVIEW HTML (página esquerda)
            l.PreviewLabelX = 210;
            l.PreviewLabelY = 269;
            l.LeftHtmlX = 118;
            l.HtmlY = 303;

            // Distância entre páginas (esquerda->direita)
            l.HtmlGap = 60;

            // ====== LABELS DE PÁGINA (1/10, 2/10)
            l.LeftPageLabelX = 218;
            l.LeftPageLabelY = 642;
            l.RightPageLabelX = 544;
            l.RightPageLabelY = 642;

            // ====== SETAS (botões) NO LIVRO
            l.PrevBtnX = 53;
            l.PrevBtnY = 460;
            l.PrevBtnUpID = 451;
            l.PrevBtnDownID = 451;

            l.NextBtnX = 720;
            l.NextBtnY = 460;
            l.NextBtnUpID = 450;
            l.NextBtnDownID = 450;

            // ====== SELO (gump 1823..1923)
            // Posição do selo quando o documento estiver selado.
            // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
            l.SealX = 639;
            l.SealY = 590;

            // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
            // Se você quiser layouts diferentes por livro, altere aqui no item.
            l.EditorPanelX = 765;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlBook150(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
