using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook300 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 233; } }
        public override int HtmlHeight { get { return 362; } }
        public override int MailCostPerSubscriber { get { return 25; } }

        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 300;

        [Constructable]
        public HtmlBook300()
        {
            ItemID = 0xFEF;
            Name = "Livro (300 páginas)";
            Weight = 3.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3511;

            // use o tamanho do HTML do item;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
            l.BookImageX = 42;
            l.BookImageY = 260;

            // ====== PREVIEW
            l.PreviewLabelX = 249;
            l.PreviewLabelY = 291;

            // ====== HTML (página esquerda)
            l.LeftHtmlX = 153;
            l.HtmlY = 318;

            // Distância entre páginas (esquerda->direita)
            l.HtmlGap = 60;

            // ====== LABELS DE PÁGINA (1/10, 2/10)
            l.LeftPageLabelX = 250;
            l.LeftPageLabelY = 687;
            l.RightPageLabelX = 556;
            l.RightPageLabelY = 687;

            // ====== SETAS (botões) NO LIVRO
            l.PrevBtnX = 61;
            l.PrevBtnY = 448;

            l.NextBtnX = 745;
            l.NextBtnY = 448;

            // ====== SELO (gump 1823..1923)
            // Posição do selo quando o documento estiver selado.
            // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
            l.SealX = 670;
            l.SealY = 639;

            // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
            // Se você quiser layouts diferentes por livro, altere aqui no item.
            l.EditorPanelX = 792;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlBook300(Serial serial) : base(serial)
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
