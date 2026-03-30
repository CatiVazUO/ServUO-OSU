using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook35 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 229; } }
        public override int HtmlHeight { get { return 222; } }
        public override int MailCostPerSubscriber { get { return 25; } }

        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 35;

        [Constructable]
        public HtmlBook35()
        {
            ItemID = 0xFEF;
            Name = "Livro (35 páginas)";
            Weight = 3.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3518;

            // use o tamanho do HTML do item;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
            l.BookImageX = 42;
            l.BookImageY = 260;

            // ====== PREVIEW HTML (página esquerda)
            l.PreviewLabelX = 191;
            l.PreviewLabelY = 283;
            l.LeftHtmlX = 107;
            l.HtmlY = 314;

            // Distância entre páginas (esquerda->direita)
            l.HtmlGap = 55;

            // ====== LABELS DE PÁGINA (1/10, 2/10)
            l.LeftPageLabelX = 303;
            l.LeftPageLabelY = 545;
            l.RightPageLabelX = 395;
            l.RightPageLabelY = 545;

            // ====== SETAS (botões) NO LIVRO
            l.PrevBtnX = 48;
            l.PrevBtnY = 417;

            l.NextBtnX = 659;
            l.NextBtnY = 417;

            // ====== SELO (gump 1823..1923)
            // Posição do selo quando o documento estiver selado.
            // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
            l.SealX = 580;
            l.SealY = 522;

            // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
            // Se você quiser layouts diferentes por livro, altere aqui no item.
            l.EditorPanelX = 707;
            l.EditorPanelY = 221;

            return l;
        }

        public HtmlBook35(Serial serial) : base(serial)
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
