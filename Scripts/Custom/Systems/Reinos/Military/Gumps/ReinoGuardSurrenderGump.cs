using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoGuardSurrenderGump : Gump
    {
        private readonly int m_CityId;
        private readonly int m_GuardSerial;
        private readonly ReinoMilitaryLaw m_Law;

        private const int ButtonNo = 1;
        private const int ButtonYes = 2;

        public ReinoGuardSurrenderGump(PlayerMobile from, int cityId, int guardSerial, ReinoMilitaryLaw law)
            : base(0, 0)
        {
            m_CityId = cityId;
            m_GuardSerial = guardSerial;
            m_Law = law;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(338, 159, 176, 116, 392);
            AddImageTiled(318, 213, 78, 89, 359);
            AddImageTiled(463, 213, 74, 90, 360);
            AddImageTiled(463, 136, 74, 82, 361);
            AddImageTiled(318, 136, 74, 90, 362);
            AddImageTiled(389, 272, 74, 31, 367);
            AddImageTiled(384, 138, 79, 31, 368);
            AddButton(394, 221, 495, 495, ButtonYes, GumpButtonType.Reply, 0);
            AddLabel(376, 187, 1152, @"Você se rende??");

        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            if (info.ButtonID == ButtonYes)
            {
                if (!ReinoMilitarySystem.AcceptSurrender(pm, m_CityId, m_GuardSerial, m_Law))
                    pm.SendMessage("Os guardas não conseguiram levá-lo para a prisão.");
            }
        }
    }
}
