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

            Closable = false;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(199, 148, 328, 286, 375);
            AddImageTiled(515, 159, 25, 269, 369);
            AddImageTiled(178, 158, 26, 271, 370);
            AddImageTiled(194, 134, 331, 25, 371);
            AddImageTiled(205, 422, 304, 30, 372);
            AddImage(170, 125, 402);
            AddImage(502, 129, 402);
            AddImage(171, 419, 402);
            AddImage(502, 419, 402);
            AddLabel(333, 164, 1152, @"Arena - Bomberman");
            AddButton(214, 218, 535, 535, 1, GumpButtonType.Reply, 0);
            AddButton(214, 255, 535, 535, 2, GumpButtonType.Reply, 0);
            AddButton(214, 291, 535, 535, 3, GumpButtonType.Reply, 0);
            AddButton(394, 217, 535, 535, 4, GumpButtonType.Reply, 0);
            AddButton(394, 255, 535, 535, 5, GumpButtonType.Reply, 0);
            AddLabel(249, 218, 1152, s.TeamMode ? @"Time" : @"Individual");
            AddLabel(250, 255, 1152, s.TeamMode ? @"Add Time Vermelho" : @"Add Jogador 1");
            AddLabel(250, 291, 1152, s.TeamMode ? @"Add Time Azul" : @"Add Jogador 2");
            AddLabel(429, 217, 1152, @"JOGAR");
            AddLabel(429, 255, 1152, @"PARAR");
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
                case 0:
                    from.SendGump(new ArenaBombermanGump(from, m_CityId, m_ConstructionKey));
                    return;
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
