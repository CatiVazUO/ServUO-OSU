using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class ArenaBombermanGump : Gump
    {
        private readonly int m_CityId;
        private readonly string m_ConstructionKey;

        public ArenaBombermanGump(PlayerMobile from, int cityId, string constructionKey) : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;

            ArenaGameModes.BombermanSession s = ArenaGameModes.GetOrCreateBomberman(m_ConstructionKey);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(199, 148, 328, 286, 375);
            AddLabel(333, 164, 0xFFFFFF, @"Arena - Bomberman");
            AddButton(214, 218, 535, 535, 1, GumpButtonType.Reply, 0);
            AddButton(214, 255, 535, 535, 2, GumpButtonType.Reply, 0);
            AddButton(214, 291, 535, 535, 3, GumpButtonType.Reply, 0);
            AddButton(394, 217, 535, 535, 4, GumpButtonType.Reply, 0);
            AddButton(394, 255, 535, 535, 5, GumpButtonType.Reply, 0);
            AddLabel(249, 218, 0xFFFFFF, s.TeamMode ? @"Time" : @"Individual");
            AddLabel(250, 255, 0xFFFFFF, s.TeamMode ? @"Add Time Vermelho" : @"Add Jogador 1");
            AddLabel(250, 291, 0xFFFFFF, s.TeamMode ? @"Add Time Azul" : @"Add Jogador 2");
            AddLabel(429, 217, 0xFFFFFF, @"JOGAR");
            AddLabel(429, 255, 0xFFFFFF, @"PARAR");
            AddHtml(220, 327, 279, 83, @"<BASEFONT COLOR=#FFFFFF>Vermelho: " + s.Red.Count + " | Azul: " + s.Blue.Count + "<BR>Use [bomba dentro do jogo.</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null)
                return;

            ArenaGameModes.BombermanSession s = ArenaGameModes.GetOrCreateBomberman(m_ConstructionKey);
            ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_ConstructionKey);

            switch (info.ButtonID)
            {
                case 1:
                    s.ToggleMode();
                    from.SendGump(new ArenaBombermanGump(from, m_CityId, m_ConstructionKey));
                    return;
                case 2:
                    s.AddSide(from, true);
                    return;
                case 3:
                    s.AddSide(from, false);
                    return;
                case 4:
                    if (lot != null) s.Play(lot);
                    from.SendGump(new ArenaBombermanGump(from, m_CityId, m_ConstructionKey));
                    return;
                case 5:
                    s.Stop(false);
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
                default:
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
            }
        }
    }
}
