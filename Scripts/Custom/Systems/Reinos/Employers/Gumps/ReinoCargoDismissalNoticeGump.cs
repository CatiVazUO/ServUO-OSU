using Server.Gumps;

namespace Server.Custom.Reinos
{
    public class ReinoCargoDismissalNoticeGump : Gump
    {
        public ReinoCargoDismissalNoticeGump(string title, string body) : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(303, 128, 0, title);
            AddHtml(221, 154, 377, 168, body, false, false);
            AddImage(535, 307, 2923);
        }
    }
}
