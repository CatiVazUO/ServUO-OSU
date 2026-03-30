using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Items;
using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Gumps
{
    /// <summary>
    /// Pergunta o título do documento no momento da selagem.
    /// </summary>
    public class HtmlDocumentTitleGump : Gump
    {
        private readonly PlayerMobile _pm;
        private readonly HtmlDocumentBase _doc;
        private readonly BookSeal _seal;

        private readonly int _authorChoice;
        private const int TitleEntryId = 1;
        private readonly Html.Readable.HtmlCompilationBook _compBook;
        private readonly Html.Readable.HtmlLoosePage _firstPage;

        public HtmlDocumentTitleGump(PlayerMobile pm, HtmlDocumentBase doc, BookSeal seal, int authorChoice = 0) : base(200, 200)
        {
            _pm = pm;
            _doc = doc;
            _seal = seal;
            _authorChoice = authorChoice;

            Closable = true;
            Dragable = true;

            AddPage(0);
            AddImageTiled(435, 196, 519, 210, 375);
            AddImageTiled(948, 205, 25, 194, 369);
            AddImageTiled(412, 204, 26, 195, 370);
            AddImageTiled(427, 179, 518, 25, 371);
            AddImageTiled(438, 394, 510, 30, 372);
            AddImage(404, 171, 402);
            AddImage(938, 175, 402);
            AddImage(406, 385, 402);
            AddImage(941, 387, 402);

            AddLabel(635, 210, 0x481, "Titulo e Autoria");
            AddLabel(531, 233, 0x481, "Você deseja manter o nome do autor na publicação?");

            // Se ainda não escolheu: mostra os botões
            if (_authorChoice == 0)
            {
                // Botão SIM
                AddButton(598, 267, 559, 559, 2, GumpButtonType.Reply, 0);

                // Botão NÃO (Anônimo)
                AddButton(695, 267, 544, 544, 3, GumpButtonType.Reply, 0);
            }
            else
            {
                // Depois que escolheu: some com botões e mostra o resultado
                string txt = (_authorChoice == 1) ? "Autor: Visível" : "Autor: Anônimo";
                AddLabel(598, 267, 0x481, txt);
            }

            AddLabel(482, 300, 0x481, "Digite um título. Depois de selado, o título não poderá ser alterado.");

            AddImageTiled(467, 330, 445, 20, 400);
            AddTextEntry(467, 329, 449, 20, 0, TitleEntryId, "");

            AddButton(652, 370, 559, 559, 1, GumpButtonType.Reply, 0);
        }



        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (_doc == null && _compBook == null)
                return;

            if (_doc != null && _doc.Deleted)
                return;

            if (_compBook != null && _compBook.Deleted)
                return;

            if (_doc != null)
            {
                if (!_doc.CanEdit(_pm))
                {
                    _pm.SendMessage(0x22, "Somente o autor deste documento pode definir o título e selá-lo.");
                    return;
                }

                _doc.EnsureAuthor(_pm);
            }
            else if (_compBook != null)
            {
                if (!string.IsNullOrWhiteSpace(_compBook.CompiledBy) && !_compBook.IsCompiler(_pm))
                {
                    _pm.SendMessage(0x22, "Somente quem iniciou este livro de compilação pode definir o título.");
                    return;
                }

                _compBook.EnsureCompiler(_pm);
            }

            // Clique em SIM
            if (info.ButtonID == 2)
            {
                if (_doc != null)
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _doc, _seal, 1));
                else
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _compBook, _firstPage, _seal, 1));
                return;
            }

            if (info.ButtonID == 3)
            {
                if (_doc != null)
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _doc, _seal, 2));
                else
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _compBook, _firstPage, _seal, 2));
                return;
            }

            // Confirmar
            if (info.ButtonID == 1)
            {
                // Exige que tenha escolhido sim/não
                if (_authorChoice == 0)
                {
                    _pm.SendMessage(0x22, "Escolha se deseja manter o nome do autor: Sim ou Não.");
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _doc, _seal, 0));
                    return;
                }

                TextRelay t = info.GetTextEntry(TitleEntryId);
                string title = (t != null) ? t.Text : null;

                title = (title ?? string.Empty).Trim();
                title = title.Replace("\r", "").Replace("\n", "");
                title = title.Replace("<", "").Replace(">", "");

                if (title.Length < 3)
                {
                    _pm.SendMessage(0x22, "O título precisa ter pelo menos 3 letras.");
                    _pm.SendGump(new HtmlDocumentTitleGump(_pm, _doc, _seal, _authorChoice));
                    return;
                }

                if (title.Length > 40)
                    title = title.Substring(0, 40);

                if (_doc != null)
                {
                    _doc.DocumentTitle = title;

                    if (_seal == null || _seal.Deleted || !_seal.IsChildOf(_pm.Backpack))
                    {
                        _pm.SendMessage(0x22, "Você precisa estar com o selo na mochila para selar.");
                        return;
                    }

                    if (_doc.IsSealed)
                    {
                        _pm.SendMessage(0x22, "Este documento já está selado.");
                        return;
                    }

                    _doc.SealId = _seal.SealId;
                    _doc.ShowAuthorOnTooltip = (_authorChoice == 1);

                    _doc.Seal(_pm);

                    if (_doc.IsSealed)
                    {
                        _pm.SendMessage(0x55, "Você selou o documento. Ele não poderá mais ser editado.");
                        _seal.Delete();
                    }

                    return;
                }
                else
                {
                    if (_compBook == null || _firstPage == null || _firstPage.Deleted)
                        return;

                    _compBook.DocumentTitle = title;
                    _compBook.ShowAuthorOnTooltip = (_authorChoice == 1);

                    if (string.IsNullOrWhiteSpace(_compBook.CompiledBy))
                        _compBook.CompiledBy = _pm.Name;

                    _compBook.AddSealedPageDirect(_pm, _firstPage);
                    return;
                }


            }
        }

        public HtmlDocumentTitleGump
            (
            PlayerMobile pm,
            Html.Readable.HtmlCompilationBook compBook,
            Html.Readable.HtmlLoosePage firstPage,
            BookSeal seal, int authorChoice = 0)
            : base(200, 200)
            {

            _pm = pm;
            _doc = null;
            _compBook = compBook;
            _firstPage = firstPage;
            _seal = seal;
            _authorChoice = authorChoice;

            Closable = true;
            Dragable = true;

            AddPage(0);
            AddImageTiled(435, 196, 519, 210, 375);
            AddImageTiled(948, 205, 25, 194, 369);
            AddImageTiled(412, 204, 26, 195, 370);
            AddImageTiled(427, 179, 518, 25, 371);
            AddImageTiled(438, 394, 510, 30, 372);
            AddImage(404, 171, 402);
            AddImage(938, 175, 402);
            AddImage(406, 385, 402);
            AddImage(941, 387, 402);

            AddLabel(635, 210, 0x481, "Titulo e Autoria");
            AddLabel(531, 233, 0x481, "Você deseja manter o nome do autor na publicação?");

            if (_authorChoice == 0)
            {
                AddButton(598, 267, 559, 559, 2, GumpButtonType.Reply, 0);
                AddButton(695, 267, 544, 544, 3, GumpButtonType.Reply, 0);
            }
            else
            {
                string txt = (_authorChoice == 1) ? "Autor: Visível" : "Autor: Anônimo";
                AddLabel(598, 267, 0x481, txt);
            }

            AddLabel(482, 300, 0x481, "Digite um título. Depois de fechado, o título não poderá ser alterado.");
            AddImageTiled(467, 330, 445, 20, 400);
            AddTextEntry(467, 329, 449, 20, 0, TitleEntryId, "");

            AddButton(652, 370, 559, 559, 1, GumpButtonType.Reply, 0);
        }
    }
}

