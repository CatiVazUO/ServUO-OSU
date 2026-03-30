using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Items;

namespace Server.Custom.Systems.HtmlBooks.Gumps
{
    public class HtmlWriteGump : Gump
    {
        private readonly PlayerMobile _writer;
        private readonly HtmlDocumentBase _doc;
        private readonly int _page;
        private readonly int _selectedLine;

        // Botões
        private const int BtnUpdatePreview = 1;

        private const int BtnBoldToggle = 3;
        private const int BtnItalicToggle = 4;
        private const int BtnUnderlineToggle = 5;

        private const int BtnCenter = 6;
        private const int BtnAlignLeft = 7;

        private const int BtnColorToggle = 8;

        private const int BtnFontSmall = 10;
        private const int BtnFontMedium = 11;
        private const int BtnFontLarge = 12;

        private const int BtnLanguage = 13;
        private const int BtnSeal = 14;
        private const int BtnClearText = 15;

        private const int BtnPrevPage = 20;
        private const int BtnNextPage = 21;

        private const int LineEntryBase = 2000;
        private const int LineRadioBase = 1000;

        public HtmlWriteGump(PlayerMobile writer, HtmlDocumentBase doc, int page, int selectedLine)
            : base(0, 0)
        {
            _writer = writer;
            _doc = doc;

            DocumentGumpLayout L = _doc.GetLayout();

            _page = Math.Max(0, Math.Min(page, doc.PageCount - 1));
            _selectedLine = Math.Max(0, Math.Min(selectedLine, doc.LinesPerPage - 1));

            Closable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // ===== LIVRO (imagem)
            AddImage(L.BookImageX, L.BookImageY, L.BookImageID);

            // ===== páginas (spread)
            int leftPage = (_page % 2 == 0) ? _page : _page - 1;
            if (leftPage < 0) leftPage = 0;
            int rightPage = leftPage + 1;

            if (_doc.PageCount > 1)
            {
                AddLabel(L.LeftPageLabelX, L.LeftPageLabelY, L.LabelHue,
                    string.Format("{0}/{1}", leftPage + 1, _doc.PageCount));

                if (rightPage < _doc.PageCount)
                {
                    AddLabel(L.RightPageLabelX, L.RightPageLabelY, L.LabelHue,
                        string.Format("{0}/{1}", rightPage + 1, _doc.PageCount));
                }
            }

            // ===== setas
            if (_doc.PageCount > 1 && _page > 0)
                AddButton(L.PrevBtnX, L.PrevBtnY, L.PrevBtnUpID, L.PrevBtnDownID, BtnPrevPage, GumpButtonType.Reply, 0);

            if (_doc.PageCount > 1 && _page < (_doc.PageCount - 1))
                AddButton(L.NextBtnX, L.NextBtnY, L.NextBtnUpID, L.NextBtnDownID, BtnNextPage, GumpButtonType.Reply, 0);

            // ===== painel direito (o seu frame atual)
            // Mantive seus IDs e posições RELATIVAS ao layout
            // (Você pode mudar EditorPanelX/Y no item)
            int px = L.EditorPanelX;
            int py = L.EditorPanelY;

            // ======= AUTO-MATEMÁTICA DO PAINEL DIREITO =======
            // Offsets (mantém exatamente o mesmo desenho, só que relativo ao px/py)
            const int Off_LeftBorderX = 8;
            const int Off_LeftBorderW = 26;

            const int Off_RightBorderW = 25;

            const int Off_TopBarX = 23;
            const int Off_TopBarY = 8;
            const int Off_TopBarH = 25;

            const int Off_LeftButtonsBgX = 30;
            const int Off_LeftButtonsBgY = 24;
            const int Off_LeftButtonsBgW = 171;

            const int Off_MainBgX = 198;
            const int Off_MainBgY = 27;

            const int Off_BottomBarX = 35;
            const int Off_BottomBarH = 30;

            const int Off_BottomLeftCornerX = 1;  // imagem 402 inferior esquerda

            const int Off_LineStartY = 39; // era 262 quando py=223
            const int Off_LineRowH = 25;

            const int Off_ButtonsX = 42;    // ButtonsX = px + 42
            const int Off_LineNumberX = 222; // LineNumberX = px + 222
            const int Off_RadioX = 202;     // RadioX = px + 202
            const int Off_TextEntryX = 242; // TextEntryX = px + 242

            // Relação que já existia no seu layout: TextEntryWidth = HtmlWidth + 72
            int textEntryW = L.HtmlWidth + 72;
            int textEntryH = L.TextEntryHeight; // mantém

            int lines = DocumentLayoutConfig.GetLinesPerPage(L.HtmlHeight);
            

            // Margem direita que já existia (no seu modelo padrão dava 43)
            const int RightMargin = 43;

            // Largura total do painel (frame) baseada no TextEntry
            int panelW = Off_TextEntryX + textEntryW + RightMargin;

            // Recalcula os offsets que dependem da largura
            int offRightBorderX = panelW - 26;   // igual ao 544 no padrão
            int offRightCornerX = panelW - 36;   // igual ao 534 no padrão

            // Altura necessária para caber todas as linhas
            int linesBottom = Off_LineStartY + (lines * Off_LineRowH) + textEntryH - 10; // +20 por causa da altura do entry
            int bottomBarY = Math.Max(411, linesBottom +20);               // 411 era o padrão, + folga
            int bottomCornerYRel = Math.Max(411 - 14, linesBottom);
            int bottomBarYRel = bottomCornerYRel + 14;
            // Altura total do painel
            int panelH = bottomCornerYRel + 30; // a 402 “de baixo” no seu código estava usando -50

            // Alturas dos tiles laterais
            int sideTileY = 33;
            int sideTileH = bottomBarY - sideTileY; // do topo até antes do rodapé

            // Altura do BG principal
            int mainBgH = bottomBarY - Off_MainBgY;

            // ======= DESENHA O PAINEL (RELATIVO E AUTO) =======
            AddImageTiled(px + Off_LeftButtonsBgX, py + Off_LeftButtonsBgY, Off_LeftButtonsBgW, mainBgH, 375);

            AddImageTiled(px + Off_MainBgX, py + Off_MainBgY, panelW - Off_MainBgX - 26, mainBgH, 395);

            AddImageTiled(px + offRightBorderX, py + 34, Off_RightBorderW, sideTileH - 1, 369);
            AddImageTiled(px + Off_LeftBorderX, py + 33, Off_LeftBorderW, sideTileH, 370);

            AddImageTiled(px + Off_TopBarX, py + Off_TopBarY, panelW - Off_TopBarX - 29, Off_TopBarH, 371);
            AddImageTiled(px + Off_BottomBarX, py + bottomBarYRel, panelW - Off_BottomBarX - 25, Off_BottomBarH, 372);

            AddImage(px + 0, py + 0, 402);
            AddImage(px + offRightCornerX, py + 4, 402);
            AddImage(px + Off_BottomLeftCornerX, py + bottomCornerYRel, 402);
            AddImage(px + offRightCornerX, py + bottomCornerYRel, 402);

            // ======= Atualiza posições do layout para serem relativas =======
            L.ButtonsX = px + Off_ButtonsX;
            L.LineNumberX = px + Off_LineNumberX;
            L.RadioX = px + Off_RadioX;
            L.TextEntryX = px + Off_TextEntryX;
            L.LinesStartY = py + Off_LineStartY;
            L.LineRowHeight = Off_LineRowH;
            L.TextEntryWidth = textEntryW;
            L.TextEntryHeight = textEntryH;

            // ===== preview (duas páginas)
            AddLabel(L.PreviewLabelX, L.PreviewLabelY, L.LabelHue, "Preview");

            int previewLeftX = L.LeftHtmlX;
            int previewY = L.HtmlY;
            int previewRightX = previewLeftX + L.HtmlWidth + L.HtmlGap;

            AddHtml(previewLeftX, previewY, L.HtmlWidth, L.HtmlHeight, _doc.GetPageHtml(leftPage), false, false);
            if (rightPage < _doc.PageCount)
                AddHtml(previewRightX, previewY, L.HtmlWidth, L.HtmlHeight, _doc.GetPageHtml(rightPage), false, false);

            // ===== botões (labels brancos)
            int bx = L.ButtonsX;
            int by = py + 38;

            int btnNormal = 535;
            int btnSelected = 436;

            LineStyle selectedLineStyle = _doc.GetLineStyle(_page, _selectedLine);

            int boldGump = _doc.IsStickyBold ? btnSelected : btnNormal;
            int italicGump = _doc.IsStickyItalic ? btnSelected : btnNormal;
            int underlineGump = _doc.IsStickyUnderline ? btnSelected : btnNormal;

            int centerGump = (selectedLineStyle.Align == TextAlignMode.Center) ? btnSelected : btnNormal;
            int leftGump = (selectedLineStyle.Align == TextAlignMode.Left) ? btnSelected : btnNormal;

            int fontSmallGump = (_doc.StickyFontSize == FontSizeMode.Small) ? btnSelected : btnNormal;
            int fontMediumGump = (_doc.StickyFontSize == FontSizeMode.Medium) ? btnSelected : btnNormal;
            int fontLargeGump = (_doc.StickyFontSize == FontSizeMode.Large) ? btnSelected : btnNormal;

            // ESTES DOIS TÊM QUE CONTINUAR FIXOS
            AddButton(bx, by + 0, 535, 535, BtnUpdatePreview, GumpButtonType.Reply, 0);
            AddLabel(bx + 34, by + 0, L.LabelHue, "Atualizar Preview");

            AddButton(bx, by + 25, 535, 535, BtnSeal, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 25, L.LabelHue, "Selar Livro");

            // ESTES AQUI PODEM MUDAR DE VISUAL
            AddButton(bx, by + 65, boldGump, boldGump, BtnBoldToggle, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 65, L.LabelHue, "Negrito");

            AddButton(bx, by + 90, italicGump, italicGump, BtnItalicToggle, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 90, L.LabelHue, "Itálico");

            AddButton(bx, by + 115, underlineGump, underlineGump, BtnUnderlineToggle, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 115, L.LabelHue, "Sublinhado");

            AddButton(bx, by + 174, centerGump, centerGump, BtnCenter, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 174, L.LabelHue, "Centraliza");

            AddButton(bx, by + 199, leftGump, leftGump, BtnAlignLeft, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 199, L.LabelHue, "Alinha Esquerda");

            AddButton(bx, by + 224, 535, 535, BtnColorToggle, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 224, L.LabelHue, _doc.TextColor == TextColorMode.White ? "Cor: Branco" : "Cor: Preto");

            AddButton(bx, by + 265, fontSmallGump, fontSmallGump, BtnFontSmall, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 265, L.LabelHue, "Fonte Pequena");

            AddButton(bx, by + 290, fontMediumGump, fontMediumGump, BtnFontMedium, GumpButtonType.Reply, 0);
            AddLabel(bx + 34, by + 290, L.LabelHue, "Fonte Média");

            AddButton(bx, by + 316, fontLargeGump, fontLargeGump, BtnFontLarge, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 316, L.LabelHue, "Fonte Grande");

            AddButton(bx, by + 350, 535, 535, BtnLanguage, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 350, L.LabelHue, "Idioma: " + OSULanguageNames.GetName(_doc.Language));

            AddButton(bx, by + 375, 535, 535, BtnClearText, GumpButtonType.Reply, 0);
            AddLabel(bx + 35, by + 375, L.LabelHue, "Limpar Texto");

            // ===== linhas editáveis

            for (int i = 0; i < lines; i++)
            {
                int y = L.LinesStartY + (i * L.LineRowHeight);

                AddLabel(L.LineNumberX, y, L.LabelHue, (i + 1).ToString());
                AddRadio(L.RadioX, y + 2, 455, 454, i == _selectedLine, LineRadioBase + i);

                string text = _doc.GetLineText(_page, i);
                AddTextEntry(L.TextEntryX, y, L.TextEntryWidth, L.TextEntryHeight, 0, LineEntryBase + i, text);
            }

            int maxLine = _doc.GetMaxCharsForLine(_page, _selectedLine);
         //   AddLabel(L.LineInfoX, L.LineInfoY, L.LabelHue, string.Format("Linha {0} (máx ~{1} chars)", _selectedLine + 1, maxLine));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_writer == null || _doc == null || _doc.Deleted)
                return;

            if (_doc.IsSealed)
            {
                _writer.SendMessage(0x22, "Este documento já foi selado e não pode mais ser editado.");
                return;
            }

            int sel = _selectedLine;
            for (int i = 0; i < _doc.LinesPerPage; i++)
            {
                if (info.IsSwitched(LineRadioBase + i))
                {
                    sel = i;
                    break;
                }
            }

            SaveAllLinesFromEntries(info);

            switch (info.ButtonID)
            {
                case 0:
                    return;

                case BtnPrevPage:
                    _writer.SendGump(new HtmlWriteGump(_writer, _doc, _page - 1, sel));
                    return;

                case BtnNextPage:
                    _writer.SendGump(new HtmlWriteGump(_writer, _doc, _page + 1, sel));
                    return;

                case BtnUpdatePreview:
                    _writer.SendGump(new HtmlWriteGump(_writer, _doc, _page, sel));
                    return;

                case BtnBoldToggle:
                    ToggleLastWordBold(sel);
                    break;

                case BtnItalicToggle:
                    ToggleLastWordItalic(sel);
                    break;

                case BtnUnderlineToggle:
                    ToggleLastWordUnderline(sel);
                    break;

                case BtnFontSmall:
                    SetLastWordFont(sel, FontSizeMode.Small);
                    break;

                case BtnFontMedium:
                    SetLastWordFont(sel, FontSizeMode.Medium);
                    break;

                case BtnFontLarge:
                    SetLastWordFont(sel, FontSizeMode.Large);
                    break;

                case BtnCenter:
                    SetAlign(sel, TextAlignMode.Center);
                    break;

                case BtnAlignLeft:
                    SetAlign(sel, TextAlignMode.Left);
                    break;

                case BtnColorToggle:
                    _doc.TextColor = (_doc.TextColor == TextColorMode.White) ? TextColorMode.Black : TextColorMode.White;
                    break;

                case BtnLanguage:
                    _writer.SendGump(new HtmlLanguageSelectGump(_writer, _doc, _page, sel));
                    return;

                case BtnClearText:
                    ClearDocumentTextOnly();
                    _writer.SendMessage(0x55, "Todo o texto do documento foi apagado.");
                    _writer.SendGump(new HtmlWriteGump(_writer, _doc, 0, 0));
                    return;

                case BtnSeal:
                    _writer.SendMessage(0x55, "Selecione o Selo (BookSeal) na sua mochila para selar este livro.");
                    _writer.Target = new SelectSealTarget(_writer, _doc);
                    return; ;
            }

            _writer.SendGump(new HtmlWriteGump(_writer, _doc, _page, sel));
        }


        private void ClearDocumentTextOnly()
        {
            for (int p = 0; p < _doc.PageCount; p++)
            {
                for (int l = 0; l < _doc.LinesPerPage; l++)
                {
                    _doc.SetLineText(p, l, string.Empty);
                    _doc.SetLineStyle(p, l, LineStyle.Default);
                }
            }

            if (_doc.IsStickyBold)
                _doc.ToggleStickyBold();

            if (_doc.IsStickyItalic)
                _doc.ToggleStickyItalic();

            if (_doc.IsStickyUnderline)
                _doc.ToggleStickyUnderline();

            _doc.SetStickyFont(FontSizeMode.Medium);
        }

        private void SaveAllLinesFromEntries(RelayInfo info)
        {
            for (int i = 0; i < _doc.LinesPerPage; i++)
            {
                TextRelay tr = info.GetTextEntry(LineEntryBase + i);
                string t = tr != null ? (tr.Text ?? "") : "";

                t = t.Replace("\r", "").Replace("\n", "");
                _doc.SetLineText(_page, i, t);

                int max = _doc.GetMaxCharsForLine(_page, i);
                if (t.Length > max)
                    _writer.SendMessage(0x22, string.Format("A linha {0} foi cortada para caber no limite.", i + 1));
            }
        }

        private void SetAlign(int line, TextAlignMode align)
        {
            LineStyle st = _doc.GetLineStyle(_page, line);
            st.Align = align;
            _doc.SetLineStyle(_page, line, st);
        }

        private void ToggleLastWordBold(int line)
        {
            _doc.ToggleStickyBold();
        }

        private void ToggleLastWordItalic(int line)
        {
            _doc.ToggleStickyItalic();
        }

        private void ToggleLastWordUnderline(int line)
        {
            _doc.ToggleStickyUnderline();
        }

        private void SetLastWordFont(int line, FontSizeMode size)
        {
            if (_doc.StickyFontSize == size)
            {
                _doc.SetStickyFont(FontSizeMode.Medium);
                return;
            }

            _doc.SetStickyFont(size);
        }

        private class SelectSealTarget : Target
        {
            private readonly PlayerMobile _pm;
            private readonly HtmlDocumentBase _doc;

            public SelectSealTarget(PlayerMobile pm, HtmlDocumentBase doc)
                : base(12, false, TargetFlags.None)
            {
                _pm = pm;
                _doc = doc;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_pm == null || _doc == null || _doc.Deleted)
                    return;

                if (_doc.IsSealed)
                {
                    _pm.SendMessage(0x22, "Este documento já está selado.");
                    return;
                }

                if (!_doc.IsAuthor(_pm))
                {
                    _pm.SendMessage(0x22, "Somente o autor deste documento pode selá-lo.");
                    return;
                }

                BookSeal seal = targeted as BookSeal;
                if (seal == null)
                {
                    _pm.SendMessage(0x22, "Isso não é um Selo (BookSeal).");
                    return;
                }

                if (_pm.Backpack == null || !seal.IsChildOf(_pm.Backpack))
                {
                    _pm.SendMessage(0x22, "O selo precisa estar na sua mochila.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_doc.DocumentTitle))
                {
                    _pm.CloseGump(typeof(HtmlDocumentTitleGump));
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _doc, seal));
                    return;
                }

                _doc.SealId = seal.SealId;
                _doc.Seal(_pm);

                if (_doc.IsSealed)
                {
                    seal.Delete();
                }

                _pm.SendMessage(0x55, "Você selou o documento. Ele não poderá mais ser editado.");
            }
        }
    }
}
