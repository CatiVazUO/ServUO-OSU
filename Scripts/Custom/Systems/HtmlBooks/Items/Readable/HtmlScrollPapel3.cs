using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlScrollPapel3 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 222; } }
        public override int HtmlHeight { get { return 323; } }
        public override int MailCostPerSubscriber { get { return 15; } }

        public override string EditedDisplayName { get { return "Pergaminho editado"; } }
        public override int PageCount => 1;

        [Constructable]
        public HtmlScrollPapel3()
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
            l.BookImageID = 3525;

            // Matermática  q define o tamanho do Gump de edição (não mudar)
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // X e Y da imagem do Gump
            l.BookImageX = 42;
            l.BookImageY = 260;

            // Label de Preview X e Y
            l.PreviewLabelX = 181;
            l.PreviewLabelY = 284;

            //Tamanho da janela de Html
            l.LeftHtmlX = 97;
            l.HtmlY = 313;

            // Posição do selo quando o documento estiver selado.
            l.SealX = 155;
            l.SealY = 630;

            // Coords do gump de edição (tiradoes pela imagem do canto esquerdo superior)
            l.EditorPanelX = 426;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlScrollPapel3(Serial serial) : base(serial)
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
