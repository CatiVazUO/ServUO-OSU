using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook50 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 229; } }
        public override int HtmlHeight { get { return 242; } }
        public override int MailCostPerSubscriber { get { return 25; } }

        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 50;

        [Constructable]
        public HtmlBook50()
        {
            ItemID = 0xFEF;
            Name = "Livro (50 páginas)";
            Weight = 3.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3519;

            // use o tamanho do HTML do item;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
            l.BookImageX = 42;
            l.BookImageY = 260;

            // ====== PREVIEW
            l.PreviewLabelX = 201;
            l.PreviewLabelY = 284;

            // ====== HTML (página esquerda)
            l.LeftHtmlX = 107;
            l.HtmlY = 314;

            // Distância entre páginas (esquerda->direita)
            l.HtmlGap = 57;

            // ====== LABELS DE PÁGINA (1/10, 2/10)
            l.LeftPageLabelX = 214;
            l.LeftPageLabelY = 571;
            l.RightPageLabelX = 479;
            l.RightPageLabelY = 571;

            // ====== SETAS (botões) NO LIVRO
            l.PrevBtnX = 50;
            l.PrevBtnY = 424;

            l.NextBtnX = 670;
            l.NextBtnY = 424;

            // ====== SELO (gump 1823..1923)
            // Posição do selo quando o documento estiver selado.
            // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
            l.SealX = 590;
            l.SealY = 531;

            // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
            // Se você quiser layouts diferentes por livro, altere aqui no item.
            l.EditorPanelX = 707;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlBook50(Serial serial) : base(serial)
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
