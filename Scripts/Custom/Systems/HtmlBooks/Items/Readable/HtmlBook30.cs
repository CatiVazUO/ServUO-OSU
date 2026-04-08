using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook30 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 239; } }
        public override int HtmlHeight { get { return 222; } }
        public override int MailCostPerSubscriber { get { return 25; } }

        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 30;

        [Constructable]
        public HtmlBook30()
        {
            ItemID = 0xFEF;
            Name = "Livro (30 páginas)";
            Weight = 3.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3517;

            // use o tamanho do HTML do item;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
            l.BookImageX = 42;
            l.BookImageY = 260;

            // ====== PREVIEW
            l.PreviewLabelX = 210;
            l.PreviewLabelY = 282;

            // ====== HTML (página esquerda)
            l.LeftHtmlX = 140;
            l.HtmlY = 313;

            // Distância entre páginas (esquerda->direita)
            l.HtmlGap = 40;

            // ====== LABELS DE PÁGINA (1/10, 2/10)
            l.LeftPageLabelX = 130;
            l.LeftPageLabelY = 542;
            l.RightPageLabelX = 584;
            l.RightPageLabelY = 542;

            // ====== SETAS (botões) NO LIVRO
            l.PrevBtnX = 50;
            l.PrevBtnY = 415;

            l.NextBtnX = 674;
            l.NextBtnY = 415;

            // ====== SELO (gump 1823..1923)
            // Posição do selo quando o documento estiver selado.
            // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
            l.SealX = 597;
            l.SealY = 229;

            // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
            // Se você quiser layouts diferentes por livro, altere aqui no item.
            l.EditorPanelX = 707;
            l.EditorPanelY = 265;
            return l;
        }

        public HtmlBook30(Serial serial) : base(serial)
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
