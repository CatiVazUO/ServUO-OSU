using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Systems.Biblioteca.Engine;
using Server.Custom.Systems.Biblioteca.Targets;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Custom.Systems.Biblioteca.Library;

namespace Server.Custom.Systems.Biblioteca.Gumps
{
    public class GumpLibrary : Gump
    {
        private readonly PlayerMobile _pm;
        private readonly Mobile _npc;

        private readonly int _tab; // 1=Card,2=Donate,3=List,4=Search
        private readonly int _listPage;
        private readonly int _searchPage;
        private readonly string _searchQueryRaw;

        private const int TAB_CARD = 1;
        private const int TAB_DONATE = 2;
        private const int TAB_LIST = 3;
        private const int TAB_SEARCH = 4;

        // Left menu buttons
        private const int BtnTabCard = 100;
        private const int BtnTabDonate = 101;
        private const int BtnTabList = 102;
        private const int BtnTabSearch = 103;

        // Card actions
        private const int BtnBuyCard = 200;
        private const int BtnRevokeCard = 201;

        // Donate
        private const int BtnDonateStart = 210;

        // List pagination + read
        private const int BtnListPrev = 300;
        private const int BtnListNext = 301;
        private const int BtnListReadBase = 4000;

        // Search
        private const int SearchEntryId = 500;
        private const int BtnSearchDo = 501;
        private const int BtnSearchPrev = 502;
        private const int BtnSearchNext = 503;
        private const int BtnSearchReadBase = 6000;

        // UI constants (your art)
        private const int HueText = 0x481;
        private const int HueTitle = 0x481;

        private readonly string _searchQuery;
        private readonly bool _searchDidRun;

        public GumpLibrary(PlayerMobile pm, Mobile npc) : this(pm, npc, TAB_CARD, 0, 0, "")
        {
        }

        public GumpLibrary(PlayerMobile pm, Mobile npc, int tab, int listPage, int searchPage, string searchQueryRaw, bool searchDidRun = false) : base(0, 0)
        {
            _pm = pm;
            _npc = npc;
            _tab = tab;
            _listPage = listPage;
            _searchPage = searchPage;
            _searchQueryRaw = searchQueryRaw ?? "";
            _searchQuery = (searchQueryRaw ?? "").Trim();
            _searchDidRun = searchDidRun;

            Closable = true;
            Dragable = true;
            Disposable = true;
            Resizable = false;

            AddPage(0);

            // Frame (exactly like your skeleton)
            AddImageTiled(199, 148, 248, 286, 375);
            AddImageTiled(448, 150, 423, 282, 395);
            AddImageTiled(867, 159, 25, 269, 369);
            AddImageTiled(178, 158, 26, 271, 370);
            AddImageTiled(193, 133, 676, 25, 371);
            AddImageTiled(205, 422, 664, 30, 372);
            AddImage(170, 125, 402);
            AddImage(854, 129, 402);
            AddImage(171, 419, 402);
            AddImage(854, 419, 402);

            // Check membership card + weekly fee enforcement
            LibraryCard card;
            bool hasCard = TryFindCard(_pm, out card);

            if (hasCard && card != null)
            {
                string feeFail;
                if (!card.EnsureWeeklyFee(_pm, out feeFail))
                {
                    // revoke if cannot pay
                    card.Delete();
                    hasCard = false;
                    _pm.SendMessage(0x22, feeFail);
                    _pm.SendMessage(0x22, "Seu cartão foi revogado por falta de pagamento.");
                }
            }

            // Left menu (only show Buy Card if no card; if has card, hide it)
            // Left menu (Card tab ALWAYS visible)
            AddButton(214, 173, 535, 535, BtnTabCard, GumpButtonType.Reply, 0);
            AddLabel(249, 173, 0x481, @"Cartão da Biblioteca");

            if (hasCard)
            {
                AddButton(214, 208, 535, 535, BtnTabDonate, GumpButtonType.Reply, 0);
                AddButton(214, 245, 535, 535, BtnTabList, GumpButtonType.Reply, 0);
                AddButton(214, 281, 535, 535, BtnTabSearch, GumpButtonType.Reply, 0);

                AddLabel(249, 208, 0x481, @"Entregar Publicação");
                AddLabel(250, 245, 0x481, @"Ver lista de Publicações");
                AddLabel(250, 281, 0x481, @"Procurar por Publicações");
            }

            // If player has no card, force tab to card
            int activeTab = hasCard ? _tab : TAB_CARD;

            switch (activeTab)
            {
                case TAB_CARD:
                    DrawCardTab(hasCard, card);
                    break;
                case TAB_DONATE:
                    if (!EnsureHasCardOrRedirect(hasCard))
                        break;
                    DrawDonateTab();
                    break;
                case TAB_LIST:
                    if (!EnsureHasCardOrRedirect(hasCard))
                        break;
                    DrawListTab();
                    break;
                case TAB_SEARCH:
                    if (!EnsureHasCardOrRedirect(hasCard))
                        break;
                    DrawSearchTab();
                    break;
                default:
                    DrawCardTab(hasCard, card);
                    break;
            }
        }

        private bool EnsureHasCardOrRedirect(bool hasCard)
        {
            if (hasCard)
                return true;

            _pm.SendMessage(0x22, "Você precisa de um cartão de membro para acessar a biblioteca.");
            return false;
        }

        private void DrawCardTab(bool hasCard, LibraryCard card)
        {
            AddLabel(609, 164, HueTitle, @"Cartão da Biblioteca");
            AddImage(465, 181, 443);

            string html;
            if (!hasCard)
            {
                html =
                    "O cartão de membros da biblioteca custa <B>50</B> moedas.<BR><BR>" +
                    "Enquanto esse cartão existir, você será cobrado <B>10</B> moedas por semana (retiradas do banco).<BR>" +
                    "Caso você não tenha esse valor no banco, seu cartão será revogado.<BR><BR>" +
                    "Para adquirir um cartão, clique no botão abaixo.";
                AddHtml(464, 219, 390, 165, html, false, false);

                // Accept button (559)
                AddButton(619, 394, 559, 559, BtnBuyCard, GumpButtonType.Reply, 0);
            }
            else
            {
                html =
                    "Você tem um cartão de membro da biblioteca.<BR><BR>" +
                    "Caso não queira mais ser membro, clique no botão abaixo para revogarmos seu cartão.";
                AddHtml(464, 219, 390, 165, html, false, false);

                // Cancel button (544)
                AddButton(619, 394, 544, 544, BtnRevokeCard, GumpButtonType.Reply, 0);
            }
        }

        private void DrawDonateTab()
        {
            AddLabel(598, 165, HueTitle, @"Entregar Publicação");
            AddImage(465, 181, 443);

            string html =
                "Caso você queira doar uma publicação para a biblioteca, clique no botão abaixo.<BR><BR>" +
                "A biblioteca ficará com seu exemplar e a terá em seu acervo para sempre.<BR><BR>" +
                "Ficamos gratos com a sua contribuição.";
            AddHtml(464, 219, 390, 165, html, false, false);

            AddButton(619, 394, 559, 559, BtnDonateStart, GumpButtonType.Reply, 0);
        }

        private void DrawListTab()
        {
            AddLabel(595, 163, HueTitle, @"Lista de Publicações");
            AddImage(465, 181, 443);

            // Header
            AddLabel(493, 213, HueText, @"Titulo");
            AddLabel(651, 213, HueText, @"Lingua");
            AddLabel(749, 213, HueText, @"Autor");

            List<LibraryEngine.LibraryEntry> entries = new List<LibraryEngine.LibraryEntry>(LibraryEngine.GetEntries());

            // Pagination: 7 rows
            int pageSize = 7;
            int pageCount = (entries.Count + pageSize - 1) / pageSize;
            if (pageCount < 1) pageCount = 1;

            int page = _listPage;
            if (page < 0) page = 0;
            if (page > pageCount - 1) page = pageCount - 1;

            int start = page * pageSize;
            int end = Math.Min(start + pageSize, entries.Count);

            // Row y positions based on your skeleton (labels)
            int[] rowY = new int[] { 235, 257, 279, 302, 324, 345, 367 }; // last is hidden by footer; we only use 7 but 367 may overlap, so we clamp to 345/7 rows
            // Actually your skeleton shows 7 rows at 235..345. We'll use 7 fixed positions:
            rowY = new int[] { 235, 257, 279, 302, 324, 345, 367 };

            // We'll compute y starting from 235 with step 22 for 7 rows:
            for (int i = 0; i < (end - start); i++)
            {
                int idx = start + i;
                LibraryEngine.LibraryEntry e = entries[idx];
                int y = 235 + (i * 22);

                // Read button
                AddButton(466, y + 1, 543, 248, BtnListReadBase + idx, GumpButtonType.Reply, 0);

                AddLabel(493, y, 0, Trunc(e.Title, 18));
                AddLabel(651, y, 0, Trunc(OSULanguageNames.GetName(e.Language), 10));
                AddLabel(749, y, 0, Trunc(e.Author, 12));
            }

            // Pagination footer
            AddLabel(654, 394, 0, string.Format("{0}/{1}", page + 1, pageCount));

            bool showPrev = pageCount > 1 && page > 0;
            bool showNext = pageCount > 1 && page < pageCount - 1;

            if (showPrev)
                AddButton(617, 386, 451, 451, BtnListPrev, GumpButtonType.Reply, 0);
            if (showNext)
                AddButton(697, 386, 450, 450, BtnListNext, GumpButtonType.Reply, 0);
        }

        private void DrawSearchTab()
        {
            AddLabel(595, 163, HueTitle, @"Procurar Publicação");
            AddImage(465, 181, 443);

            AddLabel(473, 213, HueText, @"Digite:");
            AddTextEntry(533, 214, 267, 20, 0, SearchEntryId, _searchQuery);

            AddButton(816, 209, 562, 562, BtnSearchDo, GumpButtonType.Reply, 0);

            AddImage(465, 239, 443);

            if (!_searchDidRun || string.IsNullOrWhiteSpace(_searchQuery))
            {
                AddLabel(502, 260, 0x481, "Digite algo e clique em buscar.");
            }
            else
            {
                AddLabel(502, 268, HueText, @"Titulo");
                AddLabel(660, 268, HueText, @"Lingua");
                AddLabel(758, 268, HueText, @"Autor");
            }

            if (!_searchDidRun || string.IsNullOrWhiteSpace(_searchQuery))
                return;

            string qNorm = LibraryUtil.Normalize(_searchQueryRaw);
            List<LibraryEngine.LibraryEntry> results = GetSearchResults(qNorm);

            int pageSize = 5;
            int pageCount = (results.Count + pageSize - 1) / pageSize;
            if (pageCount < 1) pageCount = 1;

            int page = _searchPage;
            if (page < 0) page = 0;
            if (page > pageCount - 1) page = pageCount - 1;

            int start = page * pageSize;
            int end = Math.Min(start + pageSize, results.Count);

            for (int i = 0; i < (end - start); i++)
            {
                int idx = start + i;
                LibraryEngine.LibraryEntry e = results[idx];
                int y = 290 + (i * 22);

                AddButton(475, y + 1, 543, 248, BtnSearchReadBase + idx, GumpButtonType.Reply, 0);

                AddLabel(502, y, 0, Trunc(e.Title, 18));
                AddLabel(660, y, 0, Trunc(OSULanguageNames.GetName(e.Language), 10));
                AddLabel(758, y, 0, Trunc(e.Author, 12));
            }

            AddLabel(654, 394, 0, string.Format("{0}/{1}", page + 1, pageCount));

            bool showPrev = pageCount > 1 && page > 0;
            bool showNext = pageCount > 1 && page < pageCount - 1;

            if (showPrev)
                AddButton(617, 386, 451, 451, BtnSearchPrev, GumpButtonType.Reply, 0);
            if (showNext)
                AddButton(697, 386, 450, 450, BtnSearchNext, GumpButtonType.Reply, 0);
        }

        private List<LibraryEngine.LibraryEntry> GetSearchResults(string qNorm)
        {
            var all = new List<LibraryEngine.LibraryEntry>(LibraryEngine.GetEntries());

            if (string.IsNullOrEmpty(qNorm))
                return new List<LibraryEngine.LibraryEntry>();

            var res = new List<LibraryEngine.LibraryEntry>();
            for (int i = 0; i < all.Count; i++)
            {
                LibraryEngine.LibraryEntry e = all[i];
                if (e == null) continue;

                string t = e.TitleNorm ?? LibraryUtil.Normalize(e.Title);
                string a = LibraryUtil.Normalize(e.Author);

                if ((t != null && t.IndexOf(qNorm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a != null && a.IndexOf(qNorm, StringComparison.OrdinalIgnoreCase) >= 0))
                    res.Add(e);
            }

            return res;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_pm == null || _pm.Deleted || _npc == null || _npc.Deleted)
                return;

            if (info.ButtonID == 0)
                return; // permite fechar no botão direito

            LibraryCard card;
            bool hasCard = TryFindCard(_pm, out card);

            // Weekly fee check whenever interacting
            if (hasCard && card != null)
            {
                string feeFail;
                if (!card.EnsureWeeklyFee(_pm, out feeFail))
                {
                    card.Delete();
                    hasCard = false;
                    _pm.SendMessage(0x22, feeFail);
                    _pm.SendMessage(0x22, "Seu cartão foi revogado por falta de pagamento.");
                }
            }

            // Tabs
            if (info.ButtonID == BtnTabCard)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_CARD, 0, 0, _searchQueryRaw));
                return;
            }
            if (info.ButtonID == BtnTabDonate)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_DONATE, 0, 0, _searchQueryRaw));
                return;
            }
            if (info.ButtonID == BtnTabList)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_LIST, 0, 0, _searchQueryRaw));
                return;
            }
            if (info.ButtonID == BtnTabSearch)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_SEARCH, 0, 0, "", false));
                return;
            }

            // Card actions
            if (info.ButtonID == BtnBuyCard)
            {
                if (hasCard)
                {
                    _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_CARD, 0, 0, _searchQueryRaw));
                    return;
                }

                if (!Banker.Withdraw(_pm, 50))
                {
                    _pm.SendMessage(0x22, "Você precisa de 50 moedas no banco.");
                    _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_CARD, 0, 0, _searchQueryRaw));
                    return;
                }

                LibraryCard c = new LibraryCard();
                if (_pm.Backpack != null)
                    _pm.Backpack.DropItem(c);
                else
                    c.Delete();

                _pm.SendMessage(0x55, "Você recebeu um Cartão da Biblioteca.");
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_DONATE, 0, 0, _searchQueryRaw));
                return;
            }

            if (info.ButtonID == BtnRevokeCard)
            {
                if (hasCard && card != null)
                {
                    card.Delete();
                    _pm.SendMessage(0x22, "Seu cartão de membro foi revogado.");
                }

                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_CARD, 0, 0, _searchQueryRaw));
                return;
            }

            // Donate
            if (info.ButtonID == BtnDonateStart)
            {
                if (!hasCard)
                {
                    _pm.SendMessage(0x22, "Você precisa de um cartão de membro para isso.");
                    _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_CARD, 0, 0, _searchQueryRaw));
                    return;
                }

                _pm.SendMessage(0x55, "Selecione a publicação selada na sua mochila.");
                _pm.Target = new LibraryDonateTarget(_pm, _npc);
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_DONATE, 0, 0, _searchQueryRaw));
                return;
            }

            // List pagination
            if (info.ButtonID == BtnListPrev)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_LIST, _listPage - 1, _searchPage, _searchQueryRaw));
                return;
            }
            if (info.ButtonID == BtnListNext)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_LIST, _listPage + 1, _searchPage, _searchQueryRaw));
                return;
            }

            // Search actions
            if (info.ButtonID == BtnSearchDo)
            {
                TextRelay tr = info.GetTextEntry(SearchEntryId);
                string q = tr != null ? (tr.Text ?? "") : "";
                q = q.Trim();

                // Reabre a aba de busca com a busca EXECUTADA e com o texto digitado
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_SEARCH, _listPage, 0, q, true));
                return;
            }
            if (info.ButtonID == BtnSearchPrev)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_SEARCH, _listPage, _searchPage - 1, _searchQueryRaw));
                return;
            }
            if (info.ButtonID == BtnSearchNext)
            {
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_SEARCH, _listPage, _searchPage + 1, _searchQueryRaw));
                return;
            }

            // Read from list
            if (info.ButtonID >= BtnListReadBase && info.ButtonID < BtnSearchReadBase)
            {
                int idx = info.ButtonID - BtnListReadBase;
                ReadFromLibraryIndex(idx);
                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_LIST, _listPage, _searchPage, _searchQueryRaw));
                return;
            }

            // Read from search results
            if (info.ButtonID >= BtnSearchReadBase)
            {
                int ridx = info.ButtonID - BtnSearchReadBase;
                string qNorm = LibraryUtil.Normalize(_searchQueryRaw);
                List<LibraryEngine.LibraryEntry> results = GetSearchResults(qNorm);

                if (ridx >= 0 && ridx < results.Count)
                    ReadEntry(results[ridx]);

                _pm.SendGump(new GumpLibrary(_pm, _npc, TAB_SEARCH, _listPage, _searchPage, _searchQueryRaw));
                return;
            }

            // default reopen
            _pm.SendGump(new GumpLibrary(_pm, _npc, hasCard ? _tab : TAB_CARD, _listPage, _searchPage, _searchQueryRaw));
        }

        private void ReadFromLibraryIndex(int idx)
        {
            List<LibraryEngine.LibraryEntry> entries = new List<LibraryEngine.LibraryEntry>(LibraryEngine.GetEntries());
            if (idx < 0 || idx >= entries.Count)
                return;

            ReadEntry(entries[idx]);
        }

        private void ReadEntry(LibraryEngine.LibraryEntry e)
        {
            if (!LibraryEngine.CanReadHere(_pm, _npc, 10))
            {
                _pm.SendMessage(0x22, "Você precisa estar perto do bibliotecário (10 tiles).");
                return;
            }

            if (!LibraryEngine.PlayerUnderstands(_pm, e.Language))
            {
                _pm.SendMessage(0x22, "Você não entende o idioma desta publicação.");
                return;
            }

            if (!LibraryEngine.IsReady)
                return;

            Item item = FindPublicationItem(e);
            if (item == null)
            {
                _pm.SendMessage(0x22, "Publicação não encontrada.");
                return;
            }

            HtmlDocumentBase doc = item as HtmlDocumentBase;
            if (doc != null)
            {
                _pm.CloseGump(typeof(HtmlReadGump));
                _pm.SendGump(new HtmlReadGump(_pm, doc, false));
                return;
            }

            HtmlCompilationBook comp = item as HtmlCompilationBook;
            if (comp != null)
            {
                _pm.CloseGump(typeof(HtmlReadGump));
                _pm.SendGump(new HtmlReadGump(_pm, comp));
            }
        }

        private Item FindPublicationItem(LibraryEngine.LibraryEntry e)
        {
            // item está dentro do storage (bag)
            var storageField = typeof(LibraryEngine).GetField("_storage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (storageField == null) return null;

            object storageObj = storageField.GetValue(null);
            var storage = storageObj as LibraryEngine.LibraryStorage;
            if (storage == null) return null;

            return storage.FindItem((Serial)e.ItemSerial);
        }

        private static bool TryFindCard(PlayerMobile pm, out LibraryCard card)
        {
            card = null;

            if (pm == null || pm.Backpack == null)
                return false;

            Item it = pm.Backpack.FindItemByType(typeof(LibraryCard), true);
            card = it as LibraryCard;
            return card != null && !card.Deleted;
        }

        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            if (s.Length <= max)
                return s;
            return s.Substring(0, max - 1) + "…";
        }
    }
}
