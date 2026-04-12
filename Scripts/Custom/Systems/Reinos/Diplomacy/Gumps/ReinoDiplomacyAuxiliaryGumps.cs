using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoDiplomacyApprovalGump : Gump
    {
        private readonly int m_RequestId;

        public ReinoDiplomacyApprovalGump(PlayerMobile from, int requestId) : base(0, 0)
        {
            m_RequestId = requestId;
            ReinoDiplomacyRequest request = ReinoDiplomacySystem.GetRequest(requestId);
            int sealCityId = request != null
                ? (request.State == ReinoDiplomacyRequestState.PendingTargetApproval ? request.TargetCityId : request.SourceCityId)
                : -1;

            Closable = false;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(303, 128, 0, request != null ? (request.State == ReinoDiplomacyRequestState.PendingTargetApproval ? request.TargetTitle : request.SourceTitle) : "Diplomacia");
            AddHtml(221, 154, 377, 168,
                request != null ? (request.State == ReinoDiplomacyRequestState.PendingTargetApproval ? request.TargetHtml : request.SourceHtml) : "<BASEFONT COLOR=#000000>Essa solicitação já foi resolvida.</BASEFONT>",
                false, true);
            AddButton(189, 359, 493, 493, 1, GumpButtonType.Reply, 0);
            AddImage(360, 324, sealCityId >= 0 ? ReinoVisualSystem.GetSealGumpId(sealCityId) : 2923);
            AddButton(537, 358, 492, 492, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID == 0)
            {
                from.SendGump(new ReinoDiplomacyApprovalGump(from, m_RequestId));
                return;
            }

            string message;
            if (!ReinoDiplomacySystem.VoteRequest(from, m_RequestId, info.ButtonID == 2, out message))
                from.SendMessage(message);
            else
                from.SendMessage(message);

            ReinoDiplomacySystem.ShowPendingGump(from);
        }
    }

    public class ReinoDiplomacyNoticeGump : Gump
    {
        private readonly int m_NoticeId;

        public ReinoDiplomacyNoticeGump(PlayerMobile from, int noticeId) : base(0, 0)
        {
            m_NoticeId = noticeId;
            ReinoDiplomacyNotice notice = ReinoDiplomacySystem.GetNotice(noticeId);
            int sealCityId = ReinoVisualSystem.ResolvePlayerCityId(from);

            Closable = notice == null || notice.Closable;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(303, 128, 0, notice != null ? notice.Title : "Diplomacia");
            AddHtml(221, 154, 377, 168,
                notice != null ? notice.Html : "<BASEFONT COLOR=#000000>Este aviso diplomático não está mais disponível.</BASEFONT>",
                false, true);
            AddImage(535, 307, sealCityId >= 0 ? ReinoVisualSystem.GetSealGumpId(sealCityId) : 2923);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            ReinoDiplomacyNotice notice = ReinoDiplomacySystem.GetNotice(m_NoticeId);
            if (notice != null && notice.Closable)
                ReinoDiplomacySystem.ConsumeNotice(m_NoticeId);

            if (from != null && !from.Deleted)
                ReinoDiplomacySystem.ShowPendingGump(from);
        }
    }
}
