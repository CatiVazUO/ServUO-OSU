using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class ArenaGladiatorGump : Gump
    {
        private readonly int m_CityId;
        private readonly string m_ConstructionKey;

        public ArenaGladiatorGump(PlayerMobile from, int cityId, string constructionKey) : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;

            ArenaGameModes.GladiatorSession s = ArenaGameModes.GetOrCreateGladiator(m_ConstructionKey);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(199, 148, 328, 286, 375);
            AddLabel(330, 164, 0xFFFFFF, @"Arena - Gladiadores");
            AddButton(214, 218, 535, 535, 1, GumpButtonType.Reply, 0);
            AddButton(214, 255, 535, 535, 2, GumpButtonType.Reply, 0);
            AddButton(214, 291, 535, 535, 3, GumpButtonType.Reply, 0);
            AddButton(394, 217, 535, 535, 4, GumpButtonType.Reply, 0);
            AddButton(394, 255, 535, 535, 5, GumpButtonType.Reply, 0);
            AddButton(394, 291, 535, 535, 6, GumpButtonType.Reply, 0);
            AddLabel(249, 218, 0xFFFFFF, @"Add Jogador 1");
            AddLabel(250, 255, 0xFFFFFF, @"Add Jogador 2");
            AddLabel(250, 291, 0xFFFFFF, @"Add Jogador 3");
            AddLabel(429, 217, 0xFFFFFF, @"JOGAR");
            AddLabel(429, 255, 0xFFFFFF, s.Paused ? @"DESPAUSAR" : @"PAUSAR");
            AddLabel(430, 291, 0xFFFFFF, @"PARAR");
            AddHtml(220, 327, 279, 83, @"<BASEFONT COLOR=#FFFFFF>Onda atual: " + s.Wave + "<BR>Lutadores: " + s.Fighters.Count + "</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null)
                return;

            ArenaGameModes.GladiatorSession s = ArenaGameModes.GetOrCreateGladiator(m_ConstructionKey);
            ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_ConstructionKey);

            switch (info.ButtonID)
            {
                case 1:
                case 2:
                case 3:
                    s.AddFighter(from);
                    return;
                case 4:
                    if (lot != null) s.Play(lot);
                    from.SendGump(new ArenaGladiatorGump(from, m_CityId, m_ConstructionKey));
                    return;
                case 5:
                    if (lot != null) s.TogglePause(lot);
                    from.SendGump(new ArenaGladiatorGump(from, m_CityId, m_ConstructionKey));
                    return;
                case 6:
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
