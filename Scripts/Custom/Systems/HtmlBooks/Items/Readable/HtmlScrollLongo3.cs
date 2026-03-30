using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlScrollLongo3 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 155; } }
        public override int HtmlHeight { get { return 574; } }
        public override int MailCostPerSubscriber { get { return 20; } }
        public override string EditedDisplayName { get { return "Pergaminho editado"; } }
        public override int PageCount => 1;

        [Constructable]
        public HtmlScrollLongo3()
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
            l.BookImageID = 3560;

            // Matermática  q define o tamanho do Gump de edição (não mudar)
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // X e Y da imagem do Gump
            l.BookImageX = 42;
            l.BookImageY = 260;

            // Label de Preview X e Y
            l.PreviewLabelX = 167;
            l.PreviewLabelY = 277;

            //Tamanho da janela de Html
            l.LeftHtmlX = 105;
            l.HtmlY = 325;

            // Labels dos numeros das paginas (1/10, 2/10)
            l.LeftPageLabelX = 247;
            l.LeftPageLabelY = 955;

            // Posição do selo quando o documento estiver selado.
            l.SealX = 133;
            l.SealY = 908;

            // Coords do gump de edição (tiradoes pela imagem do canto esquerdo superior)
            l.EditorPanelX = 412;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlScrollLongo3(Serial serial) : base(serial)
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
