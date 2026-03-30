using System;
using System.Text;
using Server.Targeting;
using Server.Items;
using Server;
using Server.Mobiles;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Custom.Systems.DefQual;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    public abstract class HtmlDocumentBase : Item, ISealableDocument
    {
        private bool _sealed;
        private string _sealedBy;
        private int _sealId;
        private OSULanguage _language;

        private TextColorMode _textColor;
        private string _documentTitle;

        private string[][] _pageLines;
        private LineStyle[][] _lineStyles;
        private WordStyle[][][] _wordStyles;

        private bool _wasEdited;
        private bool _showAuthorOnTooltip = true;

        private int _finalPageCount;

        private FontSizeMode _fontSize = FontSizeMode.Medium;
        private bool _stickyBold;
        private bool _stickyItalic;
        private bool _stickyUnderline;
        private FontSizeMode _stickyFontSize = FontSizeMode.Medium;

        public bool IsStickyBold { get { return _stickyBold; } }
        public bool IsStickyItalic { get { return _stickyItalic; } }
        public bool IsStickyUnderline { get { return _stickyUnderline; } }
        public FontSizeMode StickyFontSize { get { return _stickyFontSize; } }

        public void ToggleStickyBold()
        {
            _stickyBold = !_stickyBold;
        }

        public void ToggleStickyItalic()
        {
            _stickyItalic = !_stickyItalic;
        }

        public void ToggleStickyUnderline()
        {
            _stickyUnderline = !_stickyUnderline;
        }

        public void SetStickyFont(FontSizeMode size)
        {
            _stickyFontSize = size;
        }

        public bool ShowAuthorOnTooltip
        {
            get { return _showAuthorOnTooltip; }
            set { _showAuthorOnTooltip = value; InvalidateProperties(); }
        }

        public abstract int HtmlWidth { get; }
        public abstract int HtmlHeight { get; }
        public abstract int PageCount { get; }

        public int LinesPerPage
        {
            get
            {
                if (_pageLines != null && _pageLines.Length > 0 && _pageLines[0] != null)
                    return _pageLines[0].Length;

                return DocumentLayoutConfig.GetLinesPerPage(HtmlHeight);
            }
        }

        public virtual int MailCostPerSubscriber { get { return 0; } }

        public bool IsSealed { get { return _sealed; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SealId
        {
            get { return _sealId; }
            set { _sealId = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string SealedBy { get { return _sealedBy; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public OSULanguage Language
        {
            get { return _language; }
            set { _language = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public FontSizeMode FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TextColorMode TextColor
        {
            get { return _textColor; }
            set { _textColor = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string DocumentTitle
        {
            get { return _documentTitle; }
            set { _documentTitle = value; InvalidateProperties(); }
        }

        protected HtmlDocumentBase()
            : base(0xFF4)
        {
            _sealed = false;
            _sealedBy = null;
            _sealId = 0;
            _language = OSULanguage.Common;
            _textColor = TextColorMode.Black;
            _documentTitle = null;
            _fontSize = FontSizeMode.Medium;


            InitializePages(PageCount);
        }

        public HtmlDocumentBase(Serial serial) : base(serial)
        {
        }

        private void InitializePages(int pages)
        {
            pages = Math.Max(1, pages);

            int linesPerPage = DocumentLayoutConfig.GetLinesPerPage(HtmlHeight);
            linesPerPage = Math.Max(1, linesPerPage);

            _pageLines = new string[pages][];
            _lineStyles = new LineStyle[pages][];
            _wordStyles = new WordStyle[pages][][];

            for (int p = 0; p < pages; p++)
            {
                _pageLines[p] = new string[linesPerPage];
                _lineStyles[p] = new LineStyle[linesPerPage];
                _wordStyles[p] = new WordStyle[linesPerPage][];

                for (int l = 0; l < linesPerPage; l++)
                {
                    _pageLines[p][l] = string.Empty;
                    _lineStyles[p][l] = LineStyle.Default;
                    _wordStyles[p][l] = new WordStyle[0];
                }
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1060662, "{0}\t{1}", "Idioma", _language.ToString());

            if (IsSealed)
            {
                string authorName = _showAuthorOnTooltip ? _sealedBy : "Anônimo";
                if (string.IsNullOrWhiteSpace(authorName)) authorName = "Anônimo";
                list.Add(1060662, "{0}\t{1}", "Autor", authorName);
            }

            // if (_sealed)
            //      list.Add(1060662, "{0}\t{1}", "Selo N", _sealId);
        }

        public virtual bool IsAuthor(PlayerMobile pm)
        {
            return pm != null
                && !string.IsNullOrWhiteSpace(_sealedBy)
                && string.Equals(_sealedBy, pm.Name, StringComparison.OrdinalIgnoreCase);
        }

        public virtual void EnsureAuthor(PlayerMobile pm)
        {
            if (pm == null)
                return;

            if (string.IsNullOrWhiteSpace(_sealedBy))
            {
                _sealedBy = pm.Name;
                InvalidateProperties();
            }
        }

        public int GetWrittenPageCount()
        {
            EnsureInit();

            int lastPageWithContent = -1;

            for (int p = 0; p < _pageLines.Length; p++)
            {
                bool pageHasContent = false;

                for (int l = 0; l < _pageLines[p].Length; l++)
                {
                    if (!string.IsNullOrWhiteSpace(_pageLines[p][l]))
                    {
                        pageHasContent = true;
                        break;
                    }
                }

                if (pageHasContent)
                    lastPageWithContent = p;
            }

            if (lastPageWithContent < 0)
                return 1;

            return lastPageWithContent + 1;
        }

        public int GetVisiblePageCount()
        {
            if (IsSealed)
            {
                if (_finalPageCount > 0)
                    return _finalPageCount;

                return GetWrittenPageCount();
            }

            return PageCount;
        }

        public void CopyFullStateFrom(HtmlDocumentBase source)
        {
            if (source == null)
                return;

            source.EnsureInit();

            _sealed = source._sealed;
            _sealedBy = source._sealedBy;
            _sealId = source._sealId;
            _language = source._language;
            _textColor = source._textColor;
            _documentTitle = source._documentTitle;
            _fontSize = source._fontSize;
            _wasEdited = source._wasEdited;
            _showAuthorOnTooltip = source._showAuthorOnTooltip;
            _stickyBold = source._stickyBold;
            _stickyItalic = source._stickyItalic;
            _stickyUnderline = source._stickyUnderline;
            _stickyFontSize = source._stickyFontSize;
            _finalPageCount = source._finalPageCount;

            int pages = source._pageLines != null ? source._pageLines.Length : 1;
            int lines = (pages > 0 && source._pageLines[0] != null) ? source._pageLines[0].Length : 1;

            _pageLines = new string[pages][];
            _lineStyles = new LineStyle[pages][];
            _wordStyles = new WordStyle[pages][][];

            for (int p = 0; p < pages; p++)
            {
                _pageLines[p] = new string[lines];
                _lineStyles[p] = new LineStyle[lines];
                _wordStyles[p] = new WordStyle[lines][];

                for (int l = 0; l < lines; l++)
                {
                    _pageLines[p][l] = source._pageLines[p][l];
                    _lineStyles[p][l] = source._lineStyles[p][l];

                    WordStyle[] srcWs = source._wordStyles[p][l] ?? new WordStyle[0];
                    WordStyle[] dstWs = new WordStyle[srcWs.Length];

                    for (int w = 0; w < srcWs.Length; w++)
                        dstWs[w] = srcWs[w];

                    _wordStyles[p][l] = dstWs;
                }
            }

            UpdateDisplayName();
            InvalidateProperties();
        }

        public void ForceSealAsCopy(string authorName, int sealId)
        {
            _sealed = true;
            _sealedBy = authorName;
            _sealId = sealId;
            _finalPageCount = GetWrittenPageCount();
            UpdateDisplayName();
            UpdateMailAppearance();
            InvalidateProperties();
        }


        private bool IsPapelScrollForMail()
        {
            string n = GetType().Name;
            if (!n.StartsWith("HtmlScrollPapel", StringComparison.Ordinal))
                return false;

            // Aceita só 1..11, não os tipos G
            if (n.StartsWith("HtmlScrollPapelG", StringComparison.Ordinal))
                return false;

            string suffix = n.Substring("HtmlScrollPapel".Length);
            int v;
            return int.TryParse(suffix, out v) && v >= 1 && v <= 11;
        }

        private void UpdateMailAppearance()
        {
            if (IsPapelScrollForMail() && IsSealed)
                ItemID = 0x138D;
        }

        public virtual bool CanEdit(PlayerMobile pm)
        {
            if (pm == null)
                return false;

            if (!OSUDefQualDispatcher.CanReadAndWrite(pm))
                return false;

            if (_sealed)
                return false;

            if (pm.Backpack == null)
                return false;

            if (!IsChildOf(pm.Backpack))
                return false;

            if (!string.IsNullOrWhiteSpace(_sealedBy) && !IsAuthor(pm))
                return false;

            return true;
        }

        public void OpenWriteGump(PlayerMobile pm, Item penTool)
        {
            if (pm == null) return;

            if (!CanEdit(pm))
            {
                pm.SendMessage(0x22, "Você não pode editar este documento.");
                return;
            }

            EnsureAuthor(pm);

            pm.CloseGump(typeof(HtmlWriteGump));
            pm.SendGump(new HtmlWriteGump(pm, this, 0, 0));
        }

        public void OpenReadGump(PlayerMobile pm, bool previewOnly)
        {
            if (pm == null) return;

            pm.CloseGump(typeof(HtmlReadGump));
            pm.SendGump(new HtmlReadGump(pm, this, previewOnly));
        }

        public void Seal(PlayerMobile sealer)
        {
            if (sealer == null)
                return;

            if (_sealed)
                return;

            if (!string.IsNullOrWhiteSpace(_sealedBy) && !IsAuthor(sealer))
            {
                sealer.SendMessage(0x22, "Somente o autor deste documento pode selá-lo.");
                return;
            }

            EnsureAuthor(sealer);

            _finalPageCount = GetWrittenPageCount();
            _sealed = true;
            _sealedBy = sealer.Name;
            UpdateDisplayName();
            UpdateMailAppearance();
            InvalidateProperties();
        }

        public void ClearAll()
        {
            EnsureInit();

            for (int p = 0; p < _pageLines.Length; p++)
            {
                for (int l = 0; l < _pageLines[p].Length; l++)
                {
                    _pageLines[p][l] = string.Empty;
                    _lineStyles[p][l] = LineStyle.Default;
                    _wordStyles[p][l] = new WordStyle[0];
                }
            }

            _textColor = TextColorMode.Black;
            _documentTitle = null;
            _finalPageCount = 0;

            InvalidateProperties();
        }

        public bool HasAnyContent()
        {
            if (_pageLines == null)
                return false;

            for (int p = 0; p < _pageLines.Length; p++)
                for (int l = 0; l < _pageLines[p].Length; l++)
                    if (!string.IsNullOrWhiteSpace(_pageLines[p][l]))
                        return true;

            return false;
        }

        public string GetLineText(int page, int line)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return string.Empty;

            return _pageLines[page][line] ?? string.Empty;
        }

        public void SetLineText(int page, int line, string text)
        {
            if (!_wasEdited && !string.IsNullOrWhiteSpace(text))
            {
                _wasEdited = true;
                UpdateDisplayName();
            }

            EnsureInit();
            if (!ValidLine(page, line))
                return;

            text = (text ?? string.Empty).Replace("\r", "").Replace("\n", "");

            EnsureWordStylesMatchText(page, line, text);

            int maxChars = GetMaxCharsForLine(page, line);
            if (text.Length > maxChars)
                text = text.Substring(0, maxChars);

            _pageLines[page][line] = text;
            EnsureWordStylesMatchText(page, line, _pageLines[page][line]);
        }


        public LineStyle GetLineStyle(int page, int line)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return LineStyle.Default;

            return _lineStyles[page][line];
        }

        public void SetLineStyle(int page, int line, LineStyle style)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return;

            _lineStyles[page][line] = style;
        }

        public int GetWordCount(int page, int line)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return 0;

            return _wordStyles[page][line] != null ? _wordStyles[page][line].Length : 0;
        }

        public WordStyle GetWordStyle(int page, int line, int wordIndex)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return WordStyle.Default;

            WordStyle[] ws = _wordStyles[page][line];
            if (ws == null || wordIndex < 0 || wordIndex >= ws.Length)
                return WordStyle.Default;

            return ws[wordIndex];
        }

        public void SetWordStyle(int page, int line, int wordIndex, WordStyle style)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return;

            WordStyle[] ws = _wordStyles[page][line];
            if (ws == null || wordIndex < 0 || wordIndex >= ws.Length)
                return;

            ws[wordIndex] = style;
            _wordStyles[page][line] = ws;

            // Se estilo deixou a linha mais "cara", corta se precisar
            string cur = _pageLines[page][line] ?? string.Empty;
            int maxChars = GetMaxCharsForLine(page, line);
            if (cur.Length > maxChars)
            {
                _pageLines[page][line] = cur.Substring(0, maxChars);
                EnsureWordStylesMatchText(page, line, _pageLines[page][line]);
            }
        }

        public int GetMaxCharsPerLine(FontSizeMode size)
        {
            return DocumentLayoutConfig.GetMaxCharsPerLine(HtmlWidth, size);
        }

        public int GetMaxCharsForLine(int page, int line)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return GetMaxCharsPerLine(FontSizeMode.Medium);

            int baseMax = GetMaxCharsPerLine(FontSizeMode.Medium);

            bool anyBold = false;
            bool anyItalic = false;
            bool anyUnderline = false;
            bool anyLarge = false;

            WordStyle[] ws = _wordStyles[page][line];
            if (ws != null)
            {
                for (int i = 0; i < ws.Length; i++)
                {
                    if (ws[i].Bold) anyBold = true;
                    if (ws[i].Italic) anyItalic = true;
                    if (ws[i].Underline) anyUnderline = true;
                    if (ws[i].FontSize == FontSizeMode.Large) anyLarge = true;
                }
            }

            if (anyLarge)
                baseMax = Math.Min(baseMax, GetMaxCharsPerLine(FontSizeMode.Large));

            double factor = 1.0;
            if (anyBold) factor *= 0.92;
            if (anyItalic) factor *= 0.96;
            if (anyUnderline) factor *= 1.00;
            if (anyLarge) factor *= 0.95;

            int finalMax = (int)Math.Floor(baseMax * factor);
            return Math.Max(5, finalMax);
        }

        public string GetPageHtml(int pageIndex)
        {
            EnsureInit();
            if (pageIndex < 0 || pageIndex >= _pageLines.Length)
                return string.Empty;

            string colorTag = _textColor == TextColorMode.White
                ? "<BASEFONT COLOR=#FFFFFF>"
                : "<BASEFONT COLOR=#000000>";

            StringBuilder sb = new StringBuilder();
            sb.Append(colorTag);

            for (int l = 0; l < _pageLines[pageIndex].Length; l++)
            {
                // pega linha e alinhamento
                string lineText = _pageLines[pageIndex][l] ?? string.Empty;
                LineStyle ls = _lineStyles[pageIndex][l];

                bool isCentered = ls.Align == TextAlignMode.Center;

                if (isCentered)
                    sb.Append("<CENTER>");

                AppendLineHtml(sb, pageIndex, l, lineText);

                if (isCentered)
                    sb.Append("</CENTER>");

                // ⚠️ Aqui é a diferença:
                // se está centralizado, NÃO adiciona <BR>, porque o CENTER já “pula” no client
                if (!isCentered && l < _pageLines[pageIndex].Length - 1)
                    sb.Append("<BR>");
            }

            return sb.ToString();
        }

        private void AppendLineHtml(StringBuilder sb, int page, int line, string lineText)
        {
            string[] words = SplitWordsKeepEmpty(lineText);
            EnsureWordStylesMatchText(page, line, lineText);

            WordStyle[] ws = _wordStyles[page][line] ?? new WordStyle[0];

            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];

                if (string.IsNullOrEmpty(w))
                {
                    sb.Append(" ");
                    continue;
                }

                WordStyle st = i < ws.Length ? ws[i] : WordStyle.Default;

                if (st.FontSize == FontSizeMode.Large) sb.Append("<BIG>");
                else if (st.FontSize == FontSizeMode.Small) sb.Append("<SMALL>");

                if (st.Bold) sb.Append("<B>");
                if (st.Italic) sb.Append("<I>");
                if (st.Underline) sb.Append("<U>");

                sb.Append(FixHtml(w));

                if (st.Underline) sb.Append("</U>");
                if (st.Italic) sb.Append("</I>");
                if (st.Bold) sb.Append("</B>");

                if (st.FontSize == FontSizeMode.Large) sb.Append("</BIG>");
                else if (st.FontSize == FontSizeMode.Small) sb.Append("</SMALL>");

                if (i < words.Length - 1)
                    sb.Append(" ");
            }
        }

        public void SetPageHtml(int pageIndex, string html)
        {
            EnsureInit();
            if (pageIndex < 0 || pageIndex >= _pageLines.Length)
                return;

            html = html ?? string.Empty;

            string[] parts = html.Split(new string[] { "<BR>", "<br>", "<br/>", "<BR/>" }, StringSplitOptions.None);

            for (int l = 0; l < _pageLines[pageIndex].Length; l++)
            {
                string raw = l < parts.Length ? parts[l] : string.Empty;

                // alinhamento antigo (se tinha <CENTER>) vira center por linha
                LineStyle ls = _lineStyles[pageIndex][l];
                if (raw.IndexOf("<CENTER>", StringComparison.OrdinalIgnoreCase) >= 0)
                    ls.Align = TextAlignMode.Center;
                else
                    ls.Align = TextAlignMode.Left;
                _lineStyles[pageIndex][l] = ls;

                bool bold = raw.IndexOf("<B>", StringComparison.OrdinalIgnoreCase) >= 0;
                bool italic = raw.IndexOf("<I>", StringComparison.OrdinalIgnoreCase) >= 0;
                bool underline = raw.IndexOf("<U>", StringComparison.OrdinalIgnoreCase) >= 0;

                FontSizeMode f = FontSizeMode.Medium;
                if (raw.IndexOf("<BIG>", StringComparison.OrdinalIgnoreCase) >= 0) f = FontSizeMode.Large;
                else if (raw.IndexOf("<SMALL>", StringComparison.OrdinalIgnoreCase) >= 0) f = FontSizeMode.Small;

                string text = StripTagsSimple(raw);
                _pageLines[pageIndex][l] = text;

                EnsureWordStylesMatchText(pageIndex, l, text);

                WordStyle[] ws = _wordStyles[pageIndex][l];
                for (int i = 0; i < ws.Length; i++)
                {
                    ws[i].Bold = bold;
                    ws[i].Italic = italic;
                    ws[i].Underline = underline;
                    ws[i].FontSize = f;
                }
                _wordStyles[pageIndex][l] = ws;

                int max = GetMaxCharsForLine(pageIndex, l);
                if (_pageLines[pageIndex][l].Length > max)
                {
                    _pageLines[pageIndex][l] = _pageLines[pageIndex][l].Substring(0, max);
                    EnsureWordStylesMatchText(pageIndex, l, _pageLines[pageIndex][l]);
                }
            }
        }

        public string WrapWithFont(string html)
        {
            html = html ?? string.Empty;
            if (FontSize == FontSizeMode.Small) return "<SMALL>" + html + "</SMALL>";
            if (FontSize == FontSizeMode.Large) return "<BIG>" + html + "</BIG>";
            return html;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            // version 4: adiciona SealId
            writer.Write(4);

            writer.Write(_sealed);
            writer.Write(_sealedBy);
            writer.Write(_sealId);
            writer.Write((int)_language);
            writer.Write((int)_textColor);
            writer.Write(_documentTitle);
            writer.Write((int)_fontSize);

            int pages = _pageLines != null ? _pageLines.Length : Math.Max(1, PageCount);
            int lines = _pageLines != null && _pageLines.Length > 0 ? _pageLines[0].Length : DocumentLayoutConfig.GetLinesPerPage(HtmlHeight);

            writer.Write(pages);
            writer.Write(lines);

            EnsureInit();

            for (int p = 0; p < pages; p++)
            {
                for (int l = 0; l < lines; l++)
                {
                    writer.Write(_pageLines[p][l] ?? string.Empty);
                    writer.Write((int)_lineStyles[p][l].Align);

                    WordStyle[] ws = _wordStyles[p][l] ?? new WordStyle[0];
                    writer.Write(ws.Length);

                    for (int w = 0; w < ws.Length; w++)
                    {
                        writer.Write(ws[w].Bold);
                        writer.Write(ws[w].Italic);
                        writer.Write(ws[w].Underline);
                        writer.Write((int)ws[w].FontSize);
                    }
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            // versões antigas não tinham SealId
            _sealId = 0;

            if (version <= 1)
            {
                // antigo (por linha) — mantém lógica anterior que você já tinha
                // se você ainda tiver mundo com versão <=1, você já tinha conversão.
                // Para simplificar e não quebrar, inicializa limpo.
                _sealed = reader.ReadBool();
                _sealedBy = reader.ReadString();
                // sem sealId
                _language = (OSULanguage)reader.ReadInt();
                _textColor = (TextColorMode)reader.ReadInt();
                _documentTitle = reader.ReadString();
                _fontSize = (FontSizeMode)reader.ReadInt();

                int pages = Math.Max(1, reader.ReadInt());
                int lines = Math.Max(1, reader.ReadInt());

                _pageLines = new string[pages][];
                _lineStyles = new LineStyle[pages][];
                _wordStyles = new WordStyle[pages][][];

                for (int p = 0; p < pages; p++)
                {
                    _pageLines[p] = new string[lines];
                    _lineStyles[p] = new LineStyle[lines];
                    _wordStyles[p] = new WordStyle[lines][];

                    for (int l = 0; l < lines; l++)
                    {
                        _pageLines[p][l] = reader.ReadString();

                        bool oldBold = reader.ReadBool();
                        bool oldItalic = reader.ReadBool();
                        FontSizeMode oldFont = (FontSizeMode)reader.ReadInt();
                        TextAlignMode oldAlign = (TextAlignMode)reader.ReadInt();

                        _lineStyles[p][l] = new LineStyle(oldAlign);

                        EnsureWordStylesMatchText(p, l, _pageLines[p][l]);
                        WordStyle[] ws = _wordStyles[p][l];

                        for (int w = 0; w < ws.Length; w++)
                        {
                            ws[w].Bold = oldBold;
                            ws[w].Italic = oldItalic;
                            ws[w].Underline = false;
                            ws[w].FontSize = oldFont;
                        }
                        _wordStyles[p][l] = ws;
                    }
                }

                return;
            }

            if (version == 2)
            {
                // por palavra, mas sem underline
                _sealed = reader.ReadBool();
                _sealedBy = reader.ReadString();
                // sem sealId
                _language = (OSULanguage)reader.ReadInt();
                _textColor = (TextColorMode)reader.ReadInt();
                _documentTitle = reader.ReadString();
                _fontSize = (FontSizeMode)reader.ReadInt();

                int pages = Math.Max(1, reader.ReadInt());
                int lines = Math.Max(1, reader.ReadInt());

                _pageLines = new string[pages][];
                _lineStyles = new LineStyle[pages][];
                _wordStyles = new WordStyle[pages][][];

                for (int p = 0; p < pages; p++)
                {
                    _pageLines[p] = new string[lines];
                    _lineStyles[p] = new LineStyle[lines];
                    _wordStyles[p] = new WordStyle[lines][];

                    for (int l = 0; l < lines; l++)
                    {
                        _pageLines[p][l] = reader.ReadString();
                        TextAlignMode align = (TextAlignMode)reader.ReadInt();
                        _lineStyles[p][l] = new LineStyle(align);

                        int wc = Math.Max(0, reader.ReadInt());
                        WordStyle[] ws = new WordStyle[wc];

                        for (int w = 0; w < wc; w++)
                        {
                            WordStyle st = WordStyle.Default;
                            st.Bold = reader.ReadBool();
                            st.Italic = reader.ReadBool();
                            st.Underline = false;
                            st.FontSize = (FontSizeMode)reader.ReadInt();
                            ws[w] = st;
                        }

                        _wordStyles[p][l] = ws;
                        EnsureWordStylesMatchText(p, l, _pageLines[p][l]);
                    }
                }

                return;
            }

            // version >= 3 (com underline)
            _sealed = reader.ReadBool();
            _sealedBy = reader.ReadString();

            if (version >= 4)
                _sealId = reader.ReadInt();

            _language = (OSULanguage)reader.ReadInt();
            _textColor = (TextColorMode)reader.ReadInt();
            _documentTitle = reader.ReadString();
            _fontSize = (FontSizeMode)reader.ReadInt();

            int pagesNew = Math.Max(1, reader.ReadInt());
            int linesNew = Math.Max(1, reader.ReadInt());

            _pageLines = new string[pagesNew][];
            _lineStyles = new LineStyle[pagesNew][];
            _wordStyles = new WordStyle[pagesNew][][];

            for (int p = 0; p < pagesNew; p++)
            {
                _pageLines[p] = new string[linesNew];
                _lineStyles[p] = new LineStyle[linesNew];
                _wordStyles[p] = new WordStyle[linesNew][];

                for (int l = 0; l < linesNew; l++)
                {
                    _pageLines[p][l] = reader.ReadString();
                    TextAlignMode align = (TextAlignMode)reader.ReadInt();
                    _lineStyles[p][l] = new LineStyle(align);

                    int wc = Math.Max(0, reader.ReadInt());
                    WordStyle[] ws = new WordStyle[wc];

                    for (int w = 0; w < wc; w++)
                    {
                        WordStyle st = WordStyle.Default;
                        st.Bold = reader.ReadBool();
                        st.Italic = reader.ReadBool();
                        st.Underline = reader.ReadBool();
                        st.FontSize = (FontSizeMode)reader.ReadInt();
                        ws[w] = st;
                    }

                    _wordStyles[p][l] = ws;
                    EnsureWordStylesMatchText(p, l, _pageLines[p][l]);
                }
            }
        }

        private void EnsureInit()
        {
            if (_pageLines == null || _lineStyles == null || _wordStyles == null)
                InitializePages(PageCount);
        }

        private void EnsureLinesCapacity(int desiredLinesPerPage)
        {
            EnsureInit();

            if (desiredLinesPerPage < 1)
                desiredLinesPerPage = 1;

            if (_pageLines == null || _pageLines.Length == 0 || _pageLines[0] == null)
                return;

            int current = _pageLines[0].Length;

            // Só aumenta (não reduz)
            if (desiredLinesPerPage <= current)
                return;

            for (int p = 0; p < _pageLines.Length; p++)
            {
                // Lines
                string[] newLines = new string[desiredLinesPerPage];
                int copy = Math.Min(_pageLines[p].Length, desiredLinesPerPage);

                for (int i = 0; i < copy; i++)
                    newLines[i] = _pageLines[p][i];

                for (int i = copy; i < desiredLinesPerPage; i++)
                    newLines[i] = string.Empty;

                _pageLines[p] = newLines;

                // LineStyles
                LineStyle[] newStyles = new LineStyle[desiredLinesPerPage];
                copy = Math.Min(_lineStyles[p].Length, desiredLinesPerPage);

                for (int i = 0; i < copy; i++)
                    newStyles[i] = _lineStyles[p][i];

                for (int i = copy; i < desiredLinesPerPage; i++)
                    newStyles[i] = LineStyle.Default;

                _lineStyles[p] = newStyles;

                // WordStyles
                WordStyle[][] newWord = new WordStyle[desiredLinesPerPage][];
                copy = Math.Min(_wordStyles[p].Length, desiredLinesPerPage);

                for (int i = 0; i < copy; i++)
                    newWord[i] = _wordStyles[p][i];

                for (int i = copy; i < desiredLinesPerPage; i++)
                    newWord[i] = null;

                _wordStyles[p] = newWord;
            }
        }

        private bool ValidLine(int page, int line)
        {
            if (_pageLines == null) return false;
            if (page < 0 || page >= _pageLines.Length) return false;
            if (line < 0 || line >= _pageLines[page].Length) return false;
            return true;
        }

        private void EnsureWordStylesMatchText(int page, int line, string lineText)
        {
            EnsureInit();
            if (!ValidLine(page, line))
                return;

            string oldText = _pageLines[page][line] ?? string.Empty;

            string[] oldWords = SplitWordsKeepEmpty(oldText);
            string[] newWords = SplitWordsKeepEmpty(lineText);
            int count = newWords.Length;

            WordStyle[] ws = _wordStyles[page][line] ?? new WordStyle[0];
            WordStyle[] newWs = new WordStyle[count];

            for (int i = 0; i < count; i++)
            {
                bool hadOldStyle = (i < ws.Length);
                bool oldWasEmpty = (i >= oldWords.Length) || string.IsNullOrWhiteSpace(oldWords[i]);
                bool newHasText = !string.IsNullOrWhiteSpace(newWords[i]);

                if (hadOldStyle)
                {
                    newWs[i] = ws[i];

                    // caso importante:
                    // o slot já existia, mas antes estava vazio e agora virou uma palavra de verdade.
                    // nesse caso, a palavra nova deve nascer com o sticky atual.
                    if (oldWasEmpty && newHasText)
                    {
                        WordStyle w = newWs[i];
                        w.Bold = _stickyBold;
                        w.Italic = _stickyItalic;
                        w.Underline = _stickyUnderline;
                        w.FontSize = _stickyFontSize;
                        newWs[i] = w;
                    }
                }
                else
                {
                    WordStyle w = WordStyle.Default;
                    w.Bold = _stickyBold;
                    w.Italic = _stickyItalic;
                    w.Underline = _stickyUnderline;
                    w.FontSize = _stickyFontSize;
                    newWs[i] = w;
                }
            }

            _wordStyles[page][line] = newWs;
        }

        public virtual string EditedDisplayName
        {
            get { return "Livro editado"; } // padrão para livros
        }

        public bool WasEdited
        {
            get { return _wasEdited; }
        }

        private void UpdateDisplayName()
        {
            // Se selado: nome vira o título (se existir)
            if (IsSealed && !string.IsNullOrWhiteSpace(_documentTitle))
            {
                Name = _documentTitle.Trim();
                InvalidateProperties();
                return;
            }

            // Se foi editado: nome genérico de editado
            if (_wasEdited)
            {
                Name = EditedDisplayName;
                InvalidateProperties();
                UpdateMailAppearance();
            }
        }

        private static string[] SplitWordsKeepEmpty(string text)
        {
            if (text == null)
                return new string[0];

            return text.Split(new char[] { ' ' }, StringSplitOptions.None);
        }

        private static string FixHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string StripTagsSimple(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            int lt;
            while ((lt = html.IndexOf('<')) >= 0)
            {
                int gt = html.IndexOf('>', lt);
                if (gt < 0) break;
                html = html.Remove(lt, gt - lt + 1);
            }

            return html.Trim();
        }

        public virtual DocumentGumpLayout GetLayout()
        {
            var l = new DocumentGumpLayout();

            // Por padrão, usa o tamanho do HTML do item:
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // ✅ garante que o doc tenha linhas suficientes para esse layout
            EnsureLinesCapacity(DocumentLayoutConfig.GetLinesPerPage(l.HtmlHeight));

            return l;
        }

        public virtual bool SupportsBlankCopyTarget
        {
            get { return PageCount == 1; }
        }

        public virtual string BlankCopyNoun
        {
            get
            {
                if (this is Server.Custom.Systems.HtmlBooks.Html.Readable.HtmlLoosePage)
                    return "folha";

                return "pergaminho";
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack))
            {
                if (!pm.InRange(GetWorldLocation(), 2))
                {
                    pm.SendMessage("Você está muito longe.");
                    return;
                }
            }

            // Documento ainda não selado
            if (!IsSealed)
            {
                // Se está em branco: mostra preview normal
                if (!HasAnyContent())
                {
                    pm.CloseGump(typeof(HtmlReadGump));
                    pm.SendGump(new HtmlReadGump(pm, this, true));

                    if (SupportsBlankCopyTarget)
                    {
                        pm.SendMessage(0x55, "Você quer copiar algo neste item em branco? Pressione ESC para cancelar.");
                        pm.Target = new BlankCopySourceTarget(this);
                    }

                    return;
                }

                // Se já foi escrito, mas o jogador não é o autor OU não entende a língua:
                if (!IsAuthor(pm) || !LanguageKnowledge.Understands(pm, Language))
                {
                    pm.SendMessage(0x22, "Você não consegue ler o que está escrito aqui, são só rabiscos.");
                    return;
                }

                // Autor pode ver o preview/leitura normal
                pm.CloseGump(typeof(HtmlReadGump));
                pm.SendGump(new HtmlReadGump(pm, this, false));
                return;
            }

            // Documento selado
            if (!LanguageKnowledge.Understands(pm, Language))
            {
                pm.SendMessage(0x22, "Você não entende a língua deste documento.");
                return;
            }

            pm.CloseGump(typeof(HtmlReadGump));
            pm.SendGump(new HtmlReadGump(pm, this, false));
        }

    }

    internal class BlankCopySourceTarget : Target
    {
        private readonly HtmlDocumentBase _blankTarget;

        public BlankCopySourceTarget(HtmlDocumentBase blankTarget) : base(12, false, TargetFlags.None)
        {
            _blankTarget = blankTarget;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || _blankTarget == null || _blankTarget.Deleted)
                return;

            HtmlDocumentBase source = targeted as HtmlDocumentBase;
            if (source == null)
            {
                pm.SendMessage(0x22, "Isso não é uma folha/pergaminho copiável.");
                return;
            }

            if (source == _blankTarget)
            {
                pm.SendMessage(0x22, "Você não pode copiar o próprio item em branco.");
                return;
            }

            if (!_blankTarget.IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "O item em branco precisa estar na sua mochila.");
                return;
            }

            if (!source.IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "O original precisa estar na sua mochila.");
                return;
            }

            if (!source.IsSealed)
            {
                pm.SendMessage(0x22, "Você só pode copiar uma folha/pergaminho já selado.");
                return;
            }

            if (!_blankTarget.SupportsBlankCopyTarget || !source.SupportsBlankCopyTarget)
            {
                pm.SendMessage(0x22, "Somente folhas e pergaminhos podem ser copiados.");
                return;
            }

            if (_blankTarget.GetType() != source.GetType())
            {
                pm.SendMessage(0x22, "Você só pode copiar para um item do mesmo tipo.");
                return;
            }

            if (!LanguageKnowledge.Understands(pm, source.Language))
            {
                pm.SendMessage(0x22, "Você não entende a língua do original e não consegue copiá-lo.");
                return;
            }

            pm.SendMessage(0x55, "Selecione uma ferramenta de escrita para usar na cópia.");
            pm.Target = new BlankCopyToolTarget(_blankTarget, source);
        }


    }

    internal class BlankCopyToolTarget : Target
    {
        private readonly HtmlDocumentBase _blankTarget;
        private readonly HtmlDocumentBase _source;

        public BlankCopyToolTarget(HtmlDocumentBase blankTarget, HtmlDocumentBase source) : base(12, false, TargetFlags.None)
        {
            _blankTarget = blankTarget;
            _source = source;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || _blankTarget == null || _source == null || _blankTarget.Deleted || _source.Deleted)
                return;

            PenAndInkTool pen = targeted as PenAndInkTool;
            if (pen == null)
            {
                pm.SendMessage(0x22, "Você precisa selecionar uma ferramenta de escrita.");
                return;
            }

            if (!pen.IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "A ferramenta de escrita precisa estar na sua mochila.");
                return;
            }

            if (pen.UsesRemaining <= 0)
            {
                pm.SendMessage(0x22, "Sua ferramenta de escrita está sem usos.");
                pen.Delete();
                return;
            }

            pm.CantWalk = true;
            pm.PublicOverheadMessage(0x00, 0, true, "Você começa a copiar o texto.");

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (pm == null)
                    return;

                pm.CantWalk = false;

                if (_blankTarget.Deleted || _source.Deleted || pen.Deleted)
                    return;

                if (!_blankTarget.IsChildOf(pm.Backpack) || !_source.IsChildOf(pm.Backpack) || !pen.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "A cópia falhou porque algum item não estava mais na sua mochila.");
                    return;
                }

                HtmlDocumentBase clone = Activator.CreateInstance(_source.GetType()) as HtmlDocumentBase;
                if (clone == null)
                {
                    pm.SendMessage(0x22, "Falha ao copiar o documento.");
                    return;
                }

                clone.CopyFullStateFrom(_source);

                if (!pen.ConsumeOneUse(pm))
                {
                    clone.Delete();
                    return;
                }

                if (!pm.PlaceInBackpack(clone))
                    clone.MoveToWorld(pm.Location, pm.Map);

                _blankTarget.Delete();

                pm.SendMessage(0x55, "Você terminou a cópia com sucesso.");
            });
        }


    }

}
