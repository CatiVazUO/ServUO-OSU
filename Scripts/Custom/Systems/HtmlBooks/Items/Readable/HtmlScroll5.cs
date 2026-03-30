using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlScroll5 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 208; } }
        public override int HtmlHeight { get { return 387; } }
        public override int MailCostPerSubscriber { get { return 15; } }

        public override string EditedDisplayName { get { return "Pergaminho editado"; } }
        public override int PageCount => 1;

        [Constructable]
        public HtmlScroll5()
        {
            ItemID = 0x14ED;
            Name = "pergaminho em branco";
            Weight = 2.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3547;

            // Matermática  q define o tamanho do Gump de edição (não mudar)
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // X e Y da imagem do Gump
            l.BookImageX = 42;
            l.BookImageY = 260;

            // Label de Preview X e Y
            l.PreviewLabelX = 204;
            l.PreviewLabelY = 278;

            //Tamanho da janela de Html
            l.LeftHtmlX = 124;
            l.HtmlY = 326;

            // Posição do selo quando o documento estiver selado.
            l.SealX = 147;
            l.SealY = 710;

            // Coords do gump de edição (tiradoes pela imagem do canto esquerdo superior)
            l.EditorPanelX = 433;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlScroll5(Serial serial) : base(serial)
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
