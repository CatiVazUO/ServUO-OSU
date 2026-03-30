using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook100 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 219; } }
        public override int HtmlHeight { get { return 278; } }
        public override int MailCostPerSubscriber { get { return 25; } }

        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 100;

        [Constructable]
        public HtmlBook100()
        {
            ItemID = 0xFEF;
            Name = "Livro (100 páginas)";
            Weight = 3.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3515;

            // use o tamanho do HTML do item;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
            l.BookImageX = 42;
            l.BookImageY = 260;

            // ====== PREVIEW
            l.PreviewLabelX = 215;
            l.PreviewLabelY = 289;

            // ====== HTML (página esquerda)
            l.LeftHtmlX = 125;
            l.HtmlY = 315;

            // Distância entre páginas (esquerda->direita)
            l.HtmlGap = 60;

            // ====== LABELS DE PÁGINA (1/10, 2/10)
            l.LeftPageLabelX = 217;
            l.LeftPageLabelY = 601;
            l.RightPageLabelX = 488;
            l.RightPageLabelY = 601;

            // ====== SETAS (botões) NO LIVRO
            l.PrevBtnX = 50;
            l.PrevBtnY = 448;

            l.NextBtnX = 676;
            l.NextBtnY = 448;

            // ====== SELO (gump 1823..1923)
            // Posição do selo quando o documento estiver selado.
            // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
            l.SealX = 35;
            l.SealY = 564;

            // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
            // Se você quiser layouts diferentes por livro, altere aqui no item.
            l.EditorPanelX = 715;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlBook100(Serial serial) : base(serial)
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
