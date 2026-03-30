using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlBook10 : HtmlDocumentBase
    {
        public override int HtmlWidth { get { return 213; } }
        public override int HtmlHeight { get { return 284; } }
        public override int MailCostPerSubscriber { get { return 20; } }
        public override string EditedDisplayName { get { return "Livro editado"; } }
        public override int PageCount => 10;

        [Constructable]
        public HtmlBook10()
        {
            ItemID = 0xFEF;
            Name = "Livro (10 páginas)";
            Weight = 2.0;
            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;
        }

        public override DocumentGumpLayout GetLayout()
        {
            //Os Valores são o padrão do DocumentGumpLayout
            var l = base.GetLayout();
            l.BookImageID = 3509;
            l.HtmlWidth = 213;
            l.HtmlHeight = 284;
            l.HtmlGap = 50;

            return l;
        }

        public HtmlBook10(Serial serial) : base(serial)
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
