using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Gumps;
using Server.Network;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    /// <summary>
    /// Livro de compilação:
    /// - Não é editável direto
    /// - Aceita páginas soltas SELADAS (HtmlLoosePage)
    /// - Qualquer pessoa pode adicionar páginas (mesmo sem saber a língua)
    /// - Para parar de aceitar novas páginas: usar BookSeal (fecha para sempre)
    /// </summary>
    public class HtmlCompilationBook : Item, ISealableDocument
    {
        private OSULanguage _language;
        private FontSizeMode _fontSize;
        private List<string> _pages;

        private int _sealId;

        private List<string> _pageAuthors;
        private List<int> _pageSealIds;

        // "Fechado" = não aceita mais páginas
        private bool _closed;
        private string _compiledBy;
        private string _documentTitle;

        private bool _showAuthorOnTooltip = true;

        public bool ShowAuthorOnTooltip
        {
            get { return _showAuthorOnTooltip; }
            set { _showAuthorOnTooltip = value; InvalidateProperties(); }
        }

        public virtual int MaxPages { get { return 100; } }
        public int MailCostPerSubscriber { get { return 50; } }

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
        public string DocumentTitle
        {
            get { return _documentTitle; }
            set
            {
                _documentTitle = value;

                if (string.IsNullOrWhiteSpace(_documentTitle))
                    Name = "Livro de Páginas (Compilação)";
                else
                    Name = _documentTitle;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string CompiledBy
        {
            get { return _compiledBy; }
            set { _compiledBy = value; InvalidateProperties(); }
        }

        // Quantidade de páginas já adicionadas
        public int PageCount
        {
            get { return _pages != null ? _pages.Count : 0; }
        }

        // Layout (ajuste depois quando tiver arte própria)
        public virtual int HtmlWidth { get { return 255; } }
        public virtual int HtmlHeight { get { return 377; } }

        // ISealableDocument: aqui "selado" significa "fechado para novas páginas"
        public bool IsSealed { get { return _closed; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SealId
        {
            get { return _sealId; }
            set { _sealId = value; InvalidateProperties(); }
        }

        [Constructable]
        public HtmlCompilationBook()
        {
            ItemID = 0xA760;
            Name = "Livro Compilado";
            Weight = 2.0;

            _language = OSULanguage.Common;
            _fontSize = FontSizeMode.Medium;
            _pages = new List<string>();

            _closed = false;
            _compiledBy = null;
            _documentTitle = null;

            _pageAuthors = new List<string>();
            _pageSealIds = new List<int>();

            _sealId = 0;

            EnsurePageMetaLists();
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            string authorName = _showAuthorOnTooltip ? _compiledBy : "Anônimo";
            if (string.IsNullOrWhiteSpace(authorName)) authorName = "Anônimo";
            list.Add(1060662, "{0}\t{1}", "Autor", authorName);

            list.Add(1060662, "{0}\t{1}", "Idioma", OSULanguageNames.GetName(Language));

            list.Add(1060662, "{0}\t{1}", "Páginas", PageCount + "/" + MaxPages);

            list.Add(1060662, "{0}\t{1}", "Selado", _closed ? "Sim" : "Não (Aberto)");

          //  if (_closed)
           //     list.Add(1060662, "{0}\t{1}", "Selo", _sealId);
        }

        private void EnsurePageMetaLists()
        {
            if (_pages == null)
                _pages = new List<string>();

            if (_pageAuthors == null)
                _pageAuthors = new List<string>();

            if (_pageSealIds == null)
                _pageSealIds = new List<int>();
        }
        public virtual bool IsCompiler(PlayerMobile pm)
        {
            return pm != null
                && !string.IsNullOrWhiteSpace(_compiledBy)
                && string.Equals(_compiledBy, pm.Name, StringComparison.OrdinalIgnoreCase);
        }

        public virtual void EnsureCompiler(PlayerMobile pm)
        {
            if (pm == null)
                return;

            if (string.IsNullOrWhiteSpace(_compiledBy))
            {
                _compiledBy = pm.Name;
                InvalidateProperties();
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2) || !pm.InLOS(this))
            {
                pm.SendLocalizedMessage(500446);
                return;
            }

            // Se tiver páginas e o jogador entende a língua, abre leitura.
            if (PageCount > 0 && LanguageKnowledge.Understands(pm, Language))
            {
                pm.CloseGump(typeof(HtmlReadGump));
                pm.SendGump(new HtmlReadGump(pm, this));
            }
            else if (PageCount > 0 && !LanguageKnowledge.Understands(pm, Language))
            {
                // Não lê, mas ainda pode adicionar páginas (se estiver aberto)
                pm.SendMessage(0x22, "Você não entende a língua deste livro. Você ainda pode adicionar páginas.");
            }

            // Se estiver fechado, não abre target para adicionar
            if (_closed)
            {
                if (PageCount == 0)
                    pm.SendMessage(0x22, "Este livro está vazio e fechado.");
                return;
            }

            // Sempre oferece target para adicionar páginas (mesmo sem falar a língua)
            BeginAddPage(pm);
        }

        public void BeginAddPage(PlayerMobile pm)
        {
            if (pm == null)
                return;

            if (_closed)
            {
                pm.SendMessage(0x22, "Este livro está fechado e não aceita mais páginas.");
                return;
            }

            if (pm.Backpack == null || !IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "O livro precisa estar na sua mochila para adicionar páginas.");
                return;
            }

            if (PageCount >= MaxPages)
            {
                pm.SendMessage(0x22, "Este livro já está cheio.");
                return;
            }

            pm.SendMessage(0x55, "Selecione uma Página Solta SELADA para adicionar ao livro. (ESC cancela)");
            pm.Target = new AddPageTarget(this);
        }

        private class AddPageTarget : Target
        {
            private readonly HtmlCompilationBook _book;

            public AddPageTarget(HtmlCompilationBook book) : base(12, false, TargetFlags.None)
            {
                _book = book;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || _book == null || _book.Deleted)
                    return;

                if (_book._closed)
                {
                    pm.SendMessage(0x22, "Este livro está fechado e não aceita mais páginas.");
                    return;
                }

                if (pm.Backpack == null || !_book.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "O livro precisa estar na sua mochila para adicionar páginas.");
                    return;
                }

                HtmlLoosePage page = targeted as HtmlLoosePage;
                if (page == null)
                {
                    pm.SendMessage(0x22, "Isso não é uma Página Solta.");
                    return;
                }

                if (!page.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "A página precisa estar na sua mochila.");
                    return;
                }

                if (!page.IsSealed)
                {
                    pm.SendMessage(0x22, "Esta página ainda não foi selada.");
                    return;
                }

                if (_book.PageCount >= _book.MaxPages)
                {
                    pm.SendMessage(0x22, "Este livro já está cheio.");
                    return;
                }

                // Primeira página: define idioma/fonte e pede título do livro
                if (_book.PageCount == 0)
                {
                    _book.Language = page.Language;
                    _book.FontSize = page.FontSize;

                    if (string.IsNullOrWhiteSpace(_book.DocumentTitle))
                    {
                        pm.CloseGump(typeof(HtmlDocumentTitleGump));
                        pm.SendGump(new HtmlDocumentTitleGump(pm, _book, page, null));
                        return;
                    }

                    // Se por algum motivo já tiver título, cai e adiciona
                }
                else
                {
                    if (_book.Language != page.Language)
                    {
                        pm.SendMessage(0x22, "A página está em outro idioma.");
                        return;
                    }
                }

                _book.AddSealedPageDirect(pm, page);
            }
        }

        // Chamado pelo target ou pelo gump de título
        public void AddSealedPageDirect(PlayerMobile pm, HtmlLoosePage page)
        {
            EnsurePageMetaLists();

            if (pm == null || page == null || page.Deleted)
                return;

            if (_closed)
            {
                pm.SendMessage(0x22, "Este livro está fechado e não aceita mais páginas.");
                return;
            }

            if (PageCount >= MaxPages)
            {
                pm.SendMessage(0x22, "Este livro já atingiu o limite de páginas.");
                return;
            }

            // Se já tem páginas, a língua precisa bater
            if (PageCount > 0 && Language != page.Language)
            {
                pm.SendMessage(0x22, "A página está em outro idioma.");
                return;
            }

            if (PageCount == 0)
            {
                EnsureCompiler(pm);
                Language = page.Language;
                FontSize = page.FontSize;
            }

            _pages.Add(page.GetPageHtml(0) ?? string.Empty);
            _pageAuthors.Add(page.SealedBy ?? string.Empty);
            _pageSealIds.Add(page.SealId); 

            page.Delete();

            pm.SendMessage(0x55, "Página adicionada. Para fechar o livro e impedir novas páginas, use um Selo nele.");
            InvalidateProperties();
        }

        // ===== leitura
        public string GetPageHtml(int pageIndex)
        {
            if (_pages == null || pageIndex < 0 || pageIndex >= _pages.Count)
                return string.Empty;

            return _pages[pageIndex] ?? string.Empty;
        }

        public string WrapWithFont(string html)
        {
            html = html ?? string.Empty;

            switch (FontSize)
            {
                case FontSizeMode.Small:
                    return "<SMALL>" + html + "</SMALL>";
                case FontSizeMode.Large:
                    return "<BIG>" + html + "</BIG>";
                default:
                case FontSizeMode.Medium:
                    return html;
            }
        }

        public int GetMaxCharsForCurrentFont()
        {
            return DocumentLayoutConfig.GetMaxContentChars(HtmlWidth, HtmlHeight, FontSize);
        }

        public virtual DocumentGumpLayout GetLayout()
        {
            var l = new DocumentGumpLayout();

            l.BookImageID = 3512;

            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            l.BookImageX = 42;
            l.BookImageY = 260;

            l.PreviewLabelX = 231;
            l.PreviewLabelY = 279;

            l.LeftHtmlX = 132;
            l.HtmlY = 303;
            l.HtmlGap = 65;

            l.LeftPageLabelX = 250;
            l.LeftPageLabelY = 687;
            l.RightPageLabelX = 556;
            l.RightPageLabelY = 687;

            l.PrevBtnX = 50;
            l.PrevBtnY = 476;

            l.NextBtnX = 770;
            l.NextBtnY = 476;
 
            l.SealX = 694;
            l.SealY = 649;

            return l;
        }

        // ====== ISealableDocument (para o BookSeal funcionar e "fechar" o livro)
        public void Seal(PlayerMobile pm)
        {
            if (_closed)
            {
                if (pm != null)
                    pm.SendMessage(0x22, "Este livro já está fechado.");
                return;
            }

            if (PageCount == 0)
            {
                if (pm != null)
                    pm.SendMessage(0x22, "Você não pode fechar um livro vazio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(DocumentTitle))
            {
                if (pm != null)
                    pm.SendMessage(0x22, "Este livro ainda não tem título.");
                return;
            }

            if (pm != null && !string.IsNullOrWhiteSpace(_compiledBy) && !IsCompiler(pm))
            {
                pm.SendMessage(0x22, "Somente quem iniciou este livro de compilação pode fechá-lo.");
                return;
            }

            EnsureCompiler(pm);

            _closed = true;

            if (pm != null)
                pm.SendMessage(0x55, "Você fechou o livro. Não será possível adicionar mais páginas.");

            InvalidateProperties();
        }

        public void ClearAll()
        {
            // não usado no fluxo normal (GM pode limpar)
            if (_pages != null)
                _pages.Clear();

            _closed = false;
            _documentTitle = null;
            _compiledBy = null;

            InvalidateProperties();
        }

        // Interface também existe no seu shard para compatibilidade,
        // mas não usamos SetPageHtml aqui (compilation guarda HTML pronto).
        public void SetPageHtml(int page, string html)
        {
            // Mantém compatibilidade: permite GM ajustar página específica
            if (_pages == null)
                _pages = new List<string>();

            if (page < 0)
                return;

            while (_pages.Count <= page)
                _pages.Add(string.Empty);

            _pages[page] = html ?? string.Empty;
            InvalidateProperties();
        }

        public bool RemovePageToLoose(PlayerMobile pm, int pageNumber)
        {
            if (pm == null)
                return false;

            if (pm.Backpack == null || !IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "O livro precisa estar na sua mochila.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_compiledBy) && !IsCompiler(pm))
            {
                pm.SendMessage(0x22, "Somente o autor deste livro pode retirar páginas.");
                return false;
            }

            if (pageNumber < 1 || pageNumber > PageCount)
            {
                pm.SendMessage(0x22, "Número de página inválido.");
                return false;
            }

            int index = pageNumber - 1;

            string html = _pages[index] ?? string.Empty;
            string author = (_pageAuthors != null && index < _pageAuthors.Count) ? _pageAuthors[index] : _compiledBy;
            int sealId = (_pageSealIds != null && index < _pageSealIds.Count) ? _pageSealIds[index] : 0;

            _pages.RemoveAt(index);

            if (_pageAuthors != null && index < _pageAuthors.Count)
                _pageAuthors.RemoveAt(index);

            if (_pageSealIds != null && index < _pageSealIds.Count)
                _pageSealIds.RemoveAt(index);

            HtmlLoosePage loose = new HtmlLoosePage();
            loose.Language = Language;
            loose.FontSize = FontSize;
            loose.SetPageHtml(0, html);
            loose.ForceSealAsCopy(author, sealId);

            if (!pm.PlaceInBackpack(loose))
                loose.MoveToWorld(pm.Location, pm.Map);

            if (_closed)
            {
                _closed = false;
                pm.SendMessage(0x55, "O livro foi reaberto porque uma página foi retirada. Sele-o novamente quando terminar.");
            }

            InvalidateProperties();
            return true;
        }

        // ====== Gump de título interno (assim você só copia 1 arquivo)
        private class HtmlCompilationTitleGump : Gump
        {
            private readonly PlayerMobile _pm;
            private readonly HtmlCompilationBook _book;
            private readonly HtmlLoosePage _page;

            private const int EntryId = 1;

            public HtmlCompilationTitleGump(PlayerMobile pm, HtmlCompilationBook book, HtmlLoosePage page) : base(150, 150)
            {
                _pm = pm;
                _book = book;
                _page = page;

                Closable = true;
                Dragable = true;
                Resizable = false;

                AddPage(0);

                AddBackground(0, 0, 360, 160, 9270);
                AddLabel(20, 20, 0x481, "Título do Livro:");
                AddTextEntry(20, 50, 320, 20, 0, EntryId, "");

                AddButton(70, 100, 247, 248, 1, GumpButtonType.Reply, 0);
                AddLabel(105, 100, 0x481, "Confirmar");

                AddButton(200, 100, 241, 242, 0, GumpButtonType.Reply, 0);
                AddLabel(235, 100, 0x481, "Cancelar");
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (_pm == null || _book == null || _book.Deleted || _page == null || _page.Deleted)
                    return;

                if (info.ButtonID != 1)
                    return;

                TextRelay tr = info.GetTextEntry(EntryId);
                string title = tr != null ? tr.Text ?? "" : "";

                title = title.Trim();
                title = title.Replace("\r", "").Replace("\n", "");
                title = title.Replace("<", "").Replace(">", "");

                if (title.Length < 3)
                {
                    _pm.SendMessage(0x22, "O título precisa ter pelo menos 3 letras.");
                    _pm.SendGump(new HtmlCompilationTitleGump(_pm, _book, _page));
                    return;
                }

                if (title.Length > 40)
                    title = title.Substring(0, 40);

                _book.DocumentTitle = title;

                // O "autor" do livro é quem colocou a primeira página
                if (string.IsNullOrWhiteSpace(_book.CompiledBy))
                    _book.CompiledBy = _pm.Name;

                _book.AddSealedPageDirect(_pm, _page);
            }
        }

        public HtmlCompilationBook(Serial serial) : base(serial)
        {
            EnsurePageMetaLists();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); // version

            writer.Write((int)_language);
            writer.Write((int)_fontSize);
            writer.Write(_closed);

            writer.Write(_sealId);

            writer.Write(_documentTitle);
            writer.Write(_compiledBy);

            writer.Write(_pages != null ? _pages.Count : 0);
            if (_pages != null)
            {
                for (int i = 0; i < _pages.Count; i++)
                    writer.Write(_pages[i] ?? string.Empty);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            _sealId = 0;

            _language = (OSULanguage)reader.ReadInt();
            _fontSize = (FontSizeMode)reader.ReadInt();
            _closed = reader.ReadBool();

            if (version >= 2)
                _sealId = reader.ReadInt();

            _documentTitle = reader.ReadString();
            _compiledBy = reader.ReadString();

            int count = reader.ReadInt();
            _pages = new List<string>(Math.Max(0, count));
            for (int i = 0; i < count; i++)
                _pages.Add(reader.ReadString());
        }
    }
}
