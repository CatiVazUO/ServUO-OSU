using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Arena.Gumps
{
    public class ArenaMainGump : Gump
    {
        private readonly int m_CityId;
        private readonly string m_ConstructionKey;

        private const int BtnLutaLivre = 1;
        private const int BtnBoxe = 2;
        private const int BtnLutaMagica = 3;
        private const int BtnJusta = 4;
        private const int BtnGladiadores = 5;
        private const int BtnBomberman = 6;
        private const int BtnIniciar = 7;
        private const int BtnEncerrar = 8;

        public ArenaMainGump(PlayerMobile from, int cityId, string constructionKey) : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ArenaState st = ArenaSystem.GetState(m_ConstructionKey) ?? ArenaSystem.EnsureState(m_ConstructionKey, cityId);

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
            AddLabel(339, 164, 0xFFFFFF, @"Arena");
            AddImageTiled(214, 189, 286, 5, 371);

            AddButton(214, 218, st.SelectedMode == ArenaGameMode.LutaLivre ? 436 : 535, 535, BtnLutaLivre, GumpButtonType.Reply, 0);
            AddButton(214, 255, st.SelectedMode == ArenaGameMode.Boxe ? 436 : 535, 535, BtnBoxe, GumpButtonType.Reply, 0);
            AddLabel(249, 218, 0xFFFFFF, @"Luta Livre");
            AddLabel(250, 255, 0xFFFFFF, @"Boxe");
            AddButton(214, 291, st.SelectedMode == ArenaGameMode.LutaMagica ? 436 : 535, 535, BtnLutaMagica, GumpButtonType.Reply, 0);
            AddLabel(250, 291, 0xFFFFFF, @"Luta Mágica");

            AddButton(359, 217, st.SelectedMode == ArenaGameMode.Justa ? 436 : 535, 535, BtnJusta, GumpButtonType.Reply, 0);
            AddButton(359, 252, st.SelectedMode == ArenaGameMode.Gladiadores ? 436 : 535, 535, BtnGladiadores, GumpButtonType.Reply, 0);
            AddButton(359, 289, st.SelectedMode == ArenaGameMode.Bomberman ? 436 : 535, 535, BtnBomberman, GumpButtonType.Reply, 0);
            AddLabel(394, 217, 0xFFFFFF, @"Justa");
            AddLabel(394, 252, 0xFFFFFF, @"Gladiadores");
            AddLabel(395, 289, 0xFFFFFF, @"BomberMan");

            AddImageTiled(214, 341, 286, 5, 371);
            AddButton(214, 371, 535, 535, BtnIniciar, GumpButtonType.Reply, 0);
            AddLabel(250, 371, 0xFFFFFF, @"Iniciar Evento");
            AddButton(359, 369, 535, 535, BtnEncerrar, GumpButtonType.Reply, 0);
            AddLabel(395, 369, 0xFFFFFF, @"Encerrar Evento");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null)
                return;

            if (!ArenaSystem.CanAccessControl(from, m_CityId, m_ConstructionKey))
                return;

            ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_ConstructionKey);
            if (lot == null)
            {
                from.SendMessage("Não foi possível localizar o lote da arena.");
                return;
            }

            switch (info.ButtonID)
            {
                case BtnLutaLivre:
                    ArenaSystem.SelectMode(m_ConstructionKey, m_CityId, ArenaGameMode.LutaLivre);
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnBoxe:
                    ArenaSystem.SelectMode(m_ConstructionKey, m_CityId, ArenaGameMode.Boxe);
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnLutaMagica:
                    ArenaSystem.SelectMode(m_ConstructionKey, m_CityId, ArenaGameMode.LutaMagica);
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnJusta:
                    ArenaSystem.SelectMode(m_ConstructionKey, m_CityId, ArenaGameMode.Justa);
                    from.SendGump(new ArenaJoustGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnGladiadores:
                    ArenaSystem.SelectMode(m_ConstructionKey, m_CityId, ArenaGameMode.Gladiadores);
                    from.SendGump(new ArenaGladiatorGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnBomberman:
                    ArenaSystem.SelectMode(m_ConstructionKey, m_CityId, ArenaGameMode.Bomberman);
                    from.SendGump(new ArenaBombermanGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnIniciar:
                    ArenaSystem.StartEvent(m_ConstructionKey, m_CityId, lot);
                    from.SendMessage("Evento iniciado.");
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
                case BtnEncerrar:
                    ArenaSystem.StopEvent(m_ConstructionKey, m_CityId, lot);
                    from.SendMessage("Evento encerrado e lote limpo.");
                    from.SendGump(new ArenaMainGump(from, m_CityId, m_ConstructionKey));
                    return;
            }
        }
    }
}
