using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Html.Readable;

namespace Server.Custom.Systems.HtmlBooks.Gumps
{
    public class HtmlReadGump : Gump
    {
        private readonly PlayerMobile _viewer;
        private readonly HtmlDocumentBase _doc;
        private readonly HtmlCompilationBook _comp;
        private readonly bool _previewOnly;
        private readonly int _page;

        // Button IDs
        private const int BtnPrev = 1;
        private const int BtnNext = 2;

        // construtor público (doc normal)
        public HtmlReadGump(PlayerMobile viewer, HtmlDocumentBase doc, bool previewOnly)
            : this(viewer, doc, null, 0, previewOnly)
        {
        }

        // construtor público (compilação)
        public HtmlReadGump(PlayerMobile viewer, HtmlCompilationBook comp)
            : this(viewer, null, comp, 0, false)
        {
        }

        // construtor interno
        private HtmlReadGump(PlayerMobile viewer, HtmlDocumentBase doc, HtmlCompilationBook comp,  int page, bool previewOnly)
            : base(0, 0)
        {
            _viewer = viewer;
            _doc = doc;
            _comp = comp;
            _previewOnly = previewOnly;

            int pageCount = (_doc != null) ? _doc.GetVisiblePageCount() : (_comp != null ? _comp.PageCount : 1);

            if (page < 0) page = 0;
            if (page >= pageCount) page = Math.Max(0, pageCount - 1);

            _page = page;

            Closable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Layout vem do documento (ou do comp, se você quiser depois estender)
            DocumentGumpLayout L = (_doc != null) ? _doc.GetLayout() : (_comp != null ? _comp.GetLayout() : new DocumentGumpLayout());

            // Preview-only: só imagem
            if (_previewOnly)
            {
                AddImage(L.BookImageX, L.BookImageY, L.BookImageID);
                return;
            }

            // Livro/pergaminho
            AddImage(L.BookImageX, L.BookImageY, L.BookImageID);

            // Selo (apenas quando estiver selado)
            int sealId = -1;
            bool isSealed = false;

            if (_doc != null)
            {
                isSealed = _doc.IsSealed;
                sealId = _doc.SealId;
            }
            else if (_comp != null)
            {
                isSealed = _comp.IsSealed;
                sealId = _comp.SealId;
            }

            if (isSealed)
            {
                if (_doc != null && !string.IsNullOrWhiteSpace(_doc.DocumentTitle) &&
                    _doc.DocumentTitle.StartsWith("relatórios de ", StringComparison.OrdinalIgnoreCase) &&
                    sealId <= 0)
                {
                    AddImage(L.SealX, L.SealY, 2923);
                }
                else
                {
                    if (sealId < 0) sealId = 0;

                    if (sealId > 100) sealId = 100;

                    if (sealId >= 1)
                        AddImage(L.SealX, L.SealY, 2821 + sealId - 1);
                }
            }

            // Spread pages (sempre mostra esquerda como par)
            int leftPage = (_page % 2 == 0) ? _page : _page - 1;
            if (leftPage < 0) leftPage = 0;
            int rightPage = leftPage + 1;

            if (pageCount > 1)
            {
                AddLabel(L.LeftPageLabelX, L.LeftPageLabelY, L.LabelHue,
                    string.Format("{0}/{1}", leftPage + 1, pageCount));

                if (rightPage < pageCount)
                {
                    AddLabel(L.RightPageLabelX, L.RightPageLabelY, L.LabelHue,
                        string.Format("{0}/{1}", rightPage + 1, pageCount));
                }
            }

            // setas: passa de 2 em 2
            if (leftPage > 0)
                AddButton(L.PrevBtnX, L.PrevBtnY, L.PrevBtnUpID, L.PrevBtnDownID, BtnPrev, GumpButtonType.Reply, 0);

            if (leftPage + 2 < pageCount)
                AddButton(L.NextBtnX, L.NextBtnY, L.NextBtnUpID, L.NextBtnDownID, BtnNext, GumpButtonType.Reply, 0);

            // HTML esquerdo/direito
            int leftX = L.LeftHtmlX;
            int y = L.HtmlY;
            int rightX = leftX + L.HtmlWidth + L.HtmlGap;

            AddHtml(leftX, y, L.HtmlWidth, L.HtmlHeight, GetHtmlForPage(leftPage), false, false);

            if (rightPage < pageCount)
                AddHtml(rightX, y, L.HtmlWidth, L.HtmlHeight, GetHtmlForPage(rightPage), false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_viewer == null)
                return;

            int pageCount = (_doc != null) ? _doc.GetVisiblePageCount() : (_comp != null ? _comp.PageCount : 1);

            switch (info.ButtonID)
            {
                case 0:
                    return;

                case BtnPrev:
                    _viewer.SendGump(new HtmlReadGump(_viewer, _doc, _comp, _page - 2, false));
                    return;

                case BtnNext:
                    _viewer.SendGump(new HtmlReadGump(_viewer, _doc, _comp, _page + 2, false));
                    return;
            }
        }

        private string GetHtmlForPage(int page)
        {
            if (_previewOnly)
                return "";

            if (_doc != null)
            {
                string raw = _doc.GetPageHtml(page);
                return _doc.WrapWithFont(raw);
            }

            if (_comp != null)
            {
                string raw = _comp.GetPageHtml(page);
                return _comp.WrapWithFont(raw);
            }

            return "";
        }
    }
}
