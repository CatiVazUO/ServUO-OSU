using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class ArenaJoustGump : Gump
    {
        private readonly int m_CityId;
        private readonly string m_ConstructionKey;

        public ArenaJoustGump(PlayerMobile from, int cityId, string constructionKey) : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(199, 148, 328, 286, 375);
            AddLabel(339, 164, 0xFFFFFF, @"Arena - Justa");
            AddButton(214, 218, 535, 535, 1, GumpButtonType.Reply, 0);
            AddButton(214, 255, 535, 535, 2, GumpButtonType.Reply, 0);
            AddButton(214, 291, 535, 535, 3, GumpButtonType.Reply, 0);
            AddButton(394, 217, 535, 535, 4, GumpButtonType.Reply, 0);
            AddButton(394, 252, 535, 535, 5, GumpButtonType.Reply, 0);
            AddLabel(249, 218, 0xFFFFFF, @"Add Cavaleiro 1");
            AddLabel(250, 255, 0xFFFFFF, @"Add Cavaleiro 2");
            AddLabel(250, 291, 0xFFFFFF, @"Checar Equipamentos");
            AddLabel(429, 217, 0xFFFFFF, @"JOGAR");
            AddLabel(429, 252, 0xFFFFFF, @"PARAR");
            AddHtml(220, 327, 279, 83, @"<BASEFONT COLOR=#FFFFFF>Adicione 2 cavaleiros, cheque equipamento e jogue. O botão de impacto (5585) aparece para os cavaleiros.</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null)
                return;

            ArenaGameModes.JoustSession s = ArenaGameModes.GetOrCreateJoust(m_ConstructionKey);
            ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_ConstructionKey);

            switch (info.ButtonID)
            {
                case 1: s.AddKnight(from, 1); return;
                case 2: s.AddKnight(from, 2); return;
                case 3:
                    string msg;
                    if (!s.CheckGear(out msg)) from.SendMessage(msg); else from.SendMessage(msg);
                    from.SendGump(new ArenaJoustGump(from, m_CityId, m_ConstructionKey));
                    return;
                case 4:
                    if (lot != null) s.Play(lot);
                    from.SendGump(new ArenaJoustGump(from, m_CityId, m_ConstructionKey));
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
