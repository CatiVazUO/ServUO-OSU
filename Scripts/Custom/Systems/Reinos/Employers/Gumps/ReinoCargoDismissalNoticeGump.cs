using Server.Gumps;
using Server;
using Server.Items;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoCargoDismissalNoticeGump : Gump
    {
        private readonly int m_SourceLetterSerial;
        public ReinoCargoDismissalNoticeGump(string title, string body, int sourceLetterSerial) : base(0, 0)
        {
            m_SourceLetterSerial = sourceLetterSerial;

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

        private void DeleteSourceLetter()
        {
            if (m_SourceLetterSerial <= 0)
                return;

            Item item = World.FindItem((Serial)m_SourceLetterSerial);

            if (item != null && !item.Deleted)
                item.Delete();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            DeleteSourceLetter();
        }
    }
}
