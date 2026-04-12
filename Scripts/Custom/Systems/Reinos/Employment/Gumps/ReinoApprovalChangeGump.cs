using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoApprovalChangeGump : Gump
    {
        private readonly int m_ApprovalId;

        public ReinoApprovalChangeGump(PlayerMobile from, int approvalId) : base(0, 0)
        {
            m_ApprovalId = approvalId;
            ReinoPendingApproval proposal = ReinoEmploymentSystem.GetPendingApproval(approvalId);

            Closable = false;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(282, 128, 0, @"Aprovação de Mudança no Reino");
            AddHtml(221, 154, 377, 168,
                proposal != null ? proposal.Html : "<BASEFONT COLOR=#000000>Essa mudança já foi resolvida.</BASEFONT>",
                false, true);
            AddButton(189, 359, 493, 493, 1, GumpButtonType.Reply, 0);
            AddImage(360, 324, proposal != null ? ReinoVisualSystem.GetSealGumpId(proposal.CityId) : 2923);
            AddButton(537, 358, 492, 492, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID == 0)
            {
                from.SendGump(new ReinoApprovalChangeGump(from, m_ApprovalId));
                return;
            }

            string message;
            if (!ReinoEmploymentSystem.VotePendingApproval(from, m_ApprovalId, info.ButtonID == 2, out message))
            {
                from.SendMessage(message);
                return;
            }

            from.SendMessage(message);
            ReinoEmploymentSystem.ShowPendingApprovalGump(from);
        }
    }
}
