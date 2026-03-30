using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlScrollPapelG1 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 508; } }
        public override int HtmlHeight { get { return 300; } }
        public override int MailCostPerSubscriber { get { return 15; } }

        public override string EditedDisplayName { get { return "Pergaminho editado"; } }
        public override int PageCount => 1;

        [Constructable]
        public HtmlScrollPapelG1()
        {
            ItemID = 0x138C;
            Name = "pergaminho em branco";
            Weight = 2.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();
            l.BookImageID = 3522;

            // Matermática  q define o tamanho do Gump de edição (não mudar)
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // X e Y da imagem do Gump
            l.BookImageX = 42;
            l.BookImageY = 260;

            // Label de Preview X e Y
            l.PreviewLabelX = 321;
            l.PreviewLabelY = 265;

            //Tamanho da janela de Html
            l.LeftHtmlX = 93;
            l.HtmlY = 293;

            // Posição do selo quando o documento estiver selado.
            l.SealX = 497;
            l.SealY = 588;

            // Coords do gump de edição (tiradoes pela imagem do canto esquerdo superior)
            l.EditorPanelX = 658;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlScrollPapelG1(Serial serial) : base(serial)
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
