using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Items;

namespace Server.Custom.Systems.HtmlBooks.Gumps
{
    /// <summary>
    /// Gump simples para escolher selo da BookSealerTool.
    /// - Setas para navegar
    /// - Botão para selecionar
    /// - Página 2: confirmação
    /// </summary>
    public class BookSealSelectGump : Gump
    {
        private readonly PlayerMobile _pm;
        private readonly BookSealerTool _tool;
        private readonly List<int> _available;
        private readonly int _index;
        private readonly bool _confirm;

        private const int BtnPrev = 1;
        private const int BtnNext = 2;
        private const int BtnSelect = 3;
        private const int BtnBack = 4;
        private const int BtnConfirm = 5;

        public BookSealSelectGump(PlayerMobile pm, BookSealerTool tool)
            : this(pm, tool, 0, false)
        {
        }

        private BookSealSelectGump(PlayerMobile pm, BookSealerTool tool, int index, bool confirm)
            : base(0, 0)
        {
            _pm = pm;
            _tool = tool;
            _confirm = confirm;

            _available = BookSealRegistry.GetAvailableSealIds(tool);

            if (_available.Count == 0)
            {
                pm.SendMessage(0x22, "Não há selos disponíveis no momento.");
                return;
            }

            if (index < 0) index = 0;
            if (index >= _available.Count) index = _available.Count - 1;
            _index = index;

            Closable = true;
            Dragable = true;
            Resizable = false;

            int sealId = _available[_index];
            int sealGump = 2821 + Math.Max(0, Math.Min(100, sealId - 1));

            AddPage(0);

            // base (bem simples, reaproveita visual do seu exemplo)
            AddImageTiled(563, 342, 221, 274, 375);
            AddImageTiled(780, 348, 25, 271, 369);
            AddImageTiled(541, 351, 26, 265, 370);
            AddImageTiled(554, 330, 234, 25, 371);
            AddImageTiled(569, 608, 211, 30, 372);
            AddImage(539, 330, 402);
            AddImage(764, 332, 402);
            AddImage(543, 594, 402);
            AddImage(762, 594, 402);

            // setas
            AddButton(572, 453, 451, 451, BtnPrev, GumpButtonType.Reply, 0);
            AddButton(755, 453, 450, 450, BtnNext, GumpButtonType.Reply, 0);

            // selo atual
            AddImage(617, 413, sealGump);

            if (!_confirm)
            {
                AddLabel(633, 354, 1152, "Escolher Selo");
                AddLabel(590, 380, 0x481, string.Format("ID: {0}", sealId));
                AddButton(664, 582, 535, 535, BtnSelect, GumpButtonType.Reply, 0);
                AddLabel(625, 612, 0x481, "Selecionar");
            }
            else
            {
                AddLabel(602, 354, 0x481, "Confirmar Selo");
                AddLabel(590, 380, 0x481, string.Format("Confirma usar o selo ID {0}?", sealId));

                AddButton(610, 582, 535, 535, BtnBack, GumpButtonType.Reply, 0);
                AddLabel(605, 612, 0x481, "Voltar");

                AddButton(720, 582, 535, 535, BtnConfirm, GumpButtonType.Reply, 0);
                AddLabel(730, 612, 0x481, "OK");
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_pm == null || _tool == null || _tool.Deleted)
                return;

            if (!_tool.IsChildOf(_pm.Backpack))
            {
                _pm.SendMessage(0x22, "Você precisa estar com o selador na mochila.");
                return;
            }

            int nextIndex = _index;

            switch (info.ButtonID)
            {
                case 0:
                    return;

                case BtnPrev:
                    nextIndex = (_index - 1);
                    if (nextIndex < 0) nextIndex = _available.Count - 1;
                    _pm.SendGump(new BookSealSelectGump(_pm, _tool, nextIndex, _confirm));
                    return;

                case BtnNext:
                    nextIndex = (_index + 1);
                    if (nextIndex >= _available.Count) nextIndex = 0;
                    _pm.SendGump(new BookSealSelectGump(_pm, _tool, nextIndex, _confirm));
                    return;

                case BtnSelect:
                    _pm.SendGump(new BookSealSelectGump(_pm, _tool, _index, true));
                    return;

                case BtnBack:
                    _pm.SendGump(new BookSealSelectGump(_pm, _tool, _index, false));
                    return;

                case BtnConfirm:
                {
                    int sealId = _available[_index];

                    if (!BookSealRegistry.TryReserve(_tool, sealId))
                    {
                        _pm.SendMessage(0x22, "Este selo já foi escolhido por outra ferramenta.");
                        _pm.SendGump(new BookSealSelectGump(_pm, _tool, 0, false));
                        return;
                    }

                    _tool.SealId = sealId;
                    _pm.SendMessage(0x55, string.Format("Selo definido para ID {0}.", sealId));
                    return;
                }
            }
        }
    }
}
