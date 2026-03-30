using Server.Custom.Systems.PlayerMadeStatues;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
    public class LiveStatueCustomizeGump : Gump
    {
        private readonly PlayerMobile m_From;
        private readonly LiveModelStatue m_Statue;
        private readonly int m_DirectionIndex;
        private readonly int m_PoseIndex;

        public LiveStatueCustomizeGump(PlayerMobile from, LiveModelStatue statue)
            : this(from, statue, 4, 0)
        {
        }

        public LiveStatueCustomizeGump(PlayerMobile from, LiveModelStatue statue, int directionIndex, int poseIndex)
            : base(0, 0)
        {
            m_From = from;
            m_Statue = statue;
            m_DirectionIndex = directionIndex;
            m_PoseIndex = poseIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            AddImageTiled(163, 66, 338, 370, 382);
            AddImageTiled(172, 418, 328, 30, 634);
            AddImageTiled(483, 78, 37, 348, 635);
            AddImageTiled(143, 74, 37, 348, 635);
            AddImageTiled(166, 47, 328, 30, 634);
            AddImage(134, 38, 1361);
            AddImage(475, 38, 1361);
            AddImage(474, 410, 1361);
            AddImage(134, 410, 1361);

            AddLabel(269, 83, 0, "Finalizar Escultura");

            AddLabel(241, 124, 0, "Direção");
            AddLabel(411, 126, 0, "Poses");

            AddImageTiled(319, 116, 13, 305, 635);
            AddImageTiled(180, 145, 303, 13, 634);
            AddImageTiled(180, 102, 303, 13, 634);
            AddImageTiled(345, 290, 131, 13, 634);

            AddDirectionButton(182, 170, 100, "Noroeste", 0);
            AddDirectionButton(182, 200, 101, "Norte", 1);
            AddDirectionButton(182, 230, 102, "Nordeste", 2);
            AddDirectionButton(182, 260, 103, "Leste", 3);
            AddDirectionButton(183, 289, 104, "Sudeste", 4);
            AddDirectionButton(183, 319, 105, "Sul", 5);
            AddDirectionButton(183, 349, 106, "Suldoeste", 6);
            AddDirectionButton(183, 379, 107, "Oeste", 7);

            AddPoseButton(366, 170, 200, 0);
            AddPoseButton(366, 200, 201, 1);
            AddPoseButton(366, 230, 202, 2);
            AddPoseButton(366, 260, 203, 3);

            AddLabel(389, 310, 0, "Altura");
            AddButton(379, 340, 582, 582, 300, GumpButtonType.Reply, 0);
            AddButton(417, 340, 583, 583, 301, GumpButtonType.Reply, 0);

            AddButton(379, 391, 495, 248, 1, GumpButtonType.Reply, 0);
        }

        private void AddDirectionButton(int x, int y, int buttonID, string label, int directionIndex)
        {
            int normal = (m_DirectionIndex == directionIndex ? 517 : 518);
            int pressed = 518;

            AddButton(x, y, normal, pressed, buttonID, GumpButtonType.Reply, 0);
            AddLabel(x + 36, y + 3, 0, label);
        }

        private void AddPoseButton(int x, int y, int buttonID, int poseIndex)
        {
            int normal = (m_PoseIndex == poseIndex ? 517 : 518);
            int pressed = 518;
            string label = "Pose " + (poseIndex + 1).ToString();

            if (m_Statue != null && poseIndex < m_Statue.PoseCount)
                label = m_Statue.GetPoseName(poseIndex);

            AddButton(x, y, normal, pressed, buttonID, GumpButtonType.Reply, 0);
            AddLabel(x + 36, y + 3, 0, label);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            PlayerMobile from = state.Mobile as PlayerMobile;

            if (from == null || m_Statue == null || m_Statue.Deleted)
                return;

            int dirIndex = m_DirectionIndex;
            int poseIndex = m_PoseIndex;

            if (info.ButtonID >= 100 && info.ButtonID <= 107)
            {
                dirIndex = info.ButtonID - 100;
                m_Statue.ApplyPreview(poseIndex, dirIndex);

                from.CloseGump(typeof(LiveStatueCustomizeGump));
                from.SendGump(new LiveStatueCustomizeGump(from, m_Statue, dirIndex, poseIndex));
                return;
            }

            if (info.ButtonID >= 200 && info.ButtonID <= 203)
            {
                poseIndex = info.ButtonID - 200;

                if (poseIndex >= m_Statue.PoseCount)
                    poseIndex = 0;

                m_Statue.ApplyPreview(poseIndex, dirIndex);

                from.CloseGump(typeof(LiveStatueCustomizeGump));
                from.SendGump(new LiveStatueCustomizeGump(from, m_Statue, dirIndex, poseIndex));
                return;
            }

            if (info.ButtonID == 300)
            {
                if (!m_Statue.TryAdjustPlacement(1))
                    from.SendMessage("Você só pode subir a estátua até 8 tiles acima da plataforma.");

                from.CloseGump(typeof(LiveStatueCustomizeGump));
                from.SendGump(new LiveStatueCustomizeGump(from, m_Statue, dirIndex, poseIndex));
                return;
            }

            if (info.ButtonID == 301)
            {
                if (!m_Statue.TryAdjustPlacement(-1))
                    from.SendMessage("Você só pode descer a estátua até 4 tiles abaixo da plataforma.");

                from.CloseGump(typeof(LiveStatueCustomizeGump));
                from.SendGump(new LiveStatueCustomizeGump(from, m_Statue, dirIndex, poseIndex));
                return;
            }

            if (info.ButtonID == 1)
            {
                m_Statue.ConfirmPreview();
            }
        }
    }
}
