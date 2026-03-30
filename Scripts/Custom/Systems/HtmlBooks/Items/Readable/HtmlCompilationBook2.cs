using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlCompilationBook2 : HtmlCompilationBook
    {
        public override int MaxPages
        {
            get { return 200; }
        }

        public override int HtmlWidth
        {
            get { return 255; }
        }

        public override int HtmlHeight
        {
            get { return 377; }
        }

        [Constructable]
        public HtmlCompilationBook2()
        {
            ItemID = 0x9981;
            Name = "Livro Compilado";
            Weight = 2.0;

            Language = OSULanguage.Common;
            FontSize = FontSizeMode.Medium;
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();

            l.BookImageID = 3513;
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            l.BookImageX = 42;
            l.BookImageY = 260;

            l.PreviewLabelX = 261;
            l.PreviewLabelY = 307;

            l.LeftHtmlX = 159;
            l.HtmlY = 332;
            l.HtmlGap = 55;

            l.LeftPageLabelX = 263;
            l.LeftPageLabelY = 725;
            l.RightPageLabelX = 589;
            l.RightPageLabelY = 725;

            l.PrevBtnX = 127;
            l.PrevBtnY = 512;

            l.NextBtnX = 740;
            l.NextBtnY = 512;
            l.NextBtnUpID = 450;
            l.NextBtnDownID = 450;

            l.SealX = 742;
            l.SealY = 697;

            return l;
        }

        public HtmlCompilationBook2(Serial serial) : base(serial)
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
