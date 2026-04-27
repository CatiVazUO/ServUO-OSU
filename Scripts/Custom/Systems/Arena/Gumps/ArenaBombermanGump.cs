using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

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
            AddLabel(249, 218, 0xFFFFFF, @"Time");
            AddLabel(250, 255, 0xFFFFFF, @"Add Jogador 1");
            AddLabel(250, 291, 0xFFFFFF, @"Add Jogador 2");
            AddLabel(429, 217, 0xFFFFFF, @"JOGAR");
            AddLabel(429, 255, 0xFFFFFF, @"PARAR");
            AddHtml(220, 327, 279, 83, @"<BASEFONT COLOR=#FFFFFF>Fase 1: painel base criado. Lógica do comando [bomba entra na próxima etapa.</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null)
                return;

            from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
        }
    }
}
