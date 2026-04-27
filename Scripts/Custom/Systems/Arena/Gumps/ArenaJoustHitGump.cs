using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Arena;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class JoustHitGump : Gump
    {
        private readonly PlayerMobile m_Player;

        public JoustHitGump(PlayerMobile pm) : base(50, 50)
        {
            m_Player = pm;
            Closable = false;
            Dragable = true;
            Disposable = true;
            Resizable = false;

            AddPage(0);
            AddImage(0, 0, 5585);
            AddButton(10, 10, 5601, 5605, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID != 1)
                return;

            if (m_Player == null)
                return;

            string key;
            int city;
            ArenaDefinition def;
            Server.Custom.Reinos.ReinoLotDefinition lot;

            if (!ArenaSystem.TryResolveArenaAt(m_Player.Location, m_Player.Map, out key, out city, out def, out lot))
                return;

            ArenaGameModes.JoustSession s = ArenaGameModes.GetOrCreateJoust(key);
            s.Click(m_Player);
        }
    }
}
