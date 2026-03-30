using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlScrollCurto4 : HtmlDocumentBase
    {
        // Ajuste quando você souber o retângulo exato do HTML desse gump 3510
        public override int HtmlWidth { get { return 329; } }
        public override int HtmlHeight { get { return 156; } }
        public override int MailCostPerSubscriber { get { return 15; } }

        public override string EditedDisplayName { get { return "Pergaminho editado"; } }
        public override int PageCount => 1;

        [Constructable]
        public HtmlScrollCurto4()
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
            l.BookImageID = 3558;

            // Matermática  q define o tamanho do Gump de edição (não mudar)
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // X e Y da imagem do Gump
            l.BookImageX = 42;
            l.BookImageY = 260;

            // Label de Preview X e Y
            l.PreviewLabelX = 270;
            l.PreviewLabelY = 280;

            //Tamanho da janela de Html
            l.LeftHtmlX = 134;
            l.HtmlY = 337;

            // Posição do selo quando o documento estiver selado.
            l.SealX = 246;
            l.SealY = 492;

            // Coords do gump de edição (tiradoes pela imagem do canto esquerdo superior)
            l.EditorPanelX = 578;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlScrollCurto4(Serial serial) : base(serial)
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
