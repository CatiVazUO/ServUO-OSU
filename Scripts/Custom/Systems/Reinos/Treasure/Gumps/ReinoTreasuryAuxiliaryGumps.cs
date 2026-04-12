using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoTaxNoticeGump : Gump
    {
        public ReinoTaxNoticeGump(PlayerMobile from, int cityId, int amount, int noticeType) : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(260, 128, 0, ReinoTreasurySystem.GetCitizenTaxNoticeTitle(cityId, amount, noticeType));
            AddHtml(221, 154, 377, 168,
                "<BASEFONT COLOR=#000000>" +
                ReinoTreasurySystem.GetCitizenTaxNoticeText(cityId, amount, noticeType) +
                "</BASEFONT>",
                false, true);
            AddImage(535, 307, ReinoVisualSystem.GetSealGumpId(cityId));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }

    public class ReinoTreasuryApprovalGump : Gump
    {
        private readonly int m_ApprovalId;

        public ReinoTreasuryApprovalGump(PlayerMobile from, int approvalId) : base(0, 0)
        {
            m_ApprovalId = approvalId;
            ReinoTreasuryPendingApproval approval = ReinoTreasurySystem.GetPendingApproval(approvalId);

            Closable = false;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(294, 128, 0, @"Aprovação de Mudança no Tesouro");
            AddHtml(221, 154, 377, 168,
                approval != null ? approval.Html : "<BASEFONT COLOR=#000000>Essa mudança já foi resolvida.</BASEFONT>",
                false, true);
            AddButton(189, 359, 493, 493, 1, GumpButtonType.Reply, 0);
            AddImage(360, 324, approval != null ? ReinoVisualSystem.GetSealGumpId(approval.CityId) : 2923);
            AddButton(537, 358, 492, 492, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID == 0)
            {
                from.SendGump(new ReinoTreasuryApprovalGump(from, m_ApprovalId));
                return;
            }

            string message;
            if (!ReinoTreasurySystem.VotePendingApproval(from, m_ApprovalId, info.ButtonID == 2, out message))
            {
                from.SendMessage(message);
                return;
            }

            from.SendMessage(message);
            ReinoTreasurySystem.ShowPendingApprovalGump(from);
        }
    }
}
