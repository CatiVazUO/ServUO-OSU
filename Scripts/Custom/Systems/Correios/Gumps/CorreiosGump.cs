using System;
using System.Collections.Generic;
using Server;
using Server.Custom.Systems.Correios.Correios;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Items;
using Server.Accounting;
using Server.Custom.Reinos;


namespace Server.Custom.Correios
{
    public class CorreioSession
    {
        // Mail composing
        public string ToName;
        public string Body;
        public List<Item> MailAttachments = new List<Item>();

        // Publication creation
        public string PubName;
        public int PubPrice;
        public PubPeriod PubPeriod = PubPeriod.Daily;
        public string PubDesc;
        public Item PubContentHeld; // moved into storage vault
        public int PubCostPerSubscriber;

        // Manual send new edition
        public Item EditionContentHeld; // moved into storage vault
        public int EditionCostPerSubscriber;

        // Page 2 mail browsing
        public int MailIndex;
        public int PubListPage;
        public int CancelListPage;

        // UI selections
        public int SelectedPublicationId;
        public int SelectedSubscriptionPubId;
        public int ContextNpcSerial;

        // Manage publication edits
        public int NewPrice;
        public string NewName;
        public PubPeriod NewPeriod = PubPeriod.Daily;
        public bool NewPeriodSelected;
        public string NewDesc;

        public PendingAction Pending;
        public string PendingHtml;

        public void ClearMailDraft(Mobile from)
        {
            ToName = null;
            Body = null;
            MailAttachments.Clear();
        }

        public void ClearPublicationDraft(Mobile from)
        {
            PubName = null;
            PubPrice = 0;
            PubPeriod = PubPeriod.Daily;
            PubDesc = null;
            PubCostPerSubscriber = 0;

            if (PubContentHeld != null && !PubContentHeld.Deleted)
            {
                // Return to player
                from?.Backpack?.DropItem(PubContentHeld);
            }

            PubContentHeld = null;
        }

        public void ClearEditionDraft(Mobile from)
        {
            EditionCostPerSubscriber = 0;

            if (EditionContentHeld != null && !EditionContentHeld.Deleted)
            {
                from?.Backpack?.DropItem(EditionContentHeld);
            }

            EditionContentHeld = null;
        }
    }


    public enum PendingAction
    {
        None,
        MailDeleteConfirm,
        MailSendConfirm,
        SubscribeConfirm,
        UnsubscribeConfirm,
        CreatePublicationConfirm,
        CancelPublicationConfirm,
        ChangePriceConfirm,
        ChangeNameConfirm,
        ChangePeriodConfirm,
        UpdatePublicationConfirm,
        SendNewEditionConfirm,
        AcceptNewTerms, // for subscriber notice
        DeclineNewTerms
    }

    public class CorreiosGump : Gump
    {
        private static readonly Dictionary<Serial, CorreioSession> Sessions = new Dictionary<Serial, CorreioSession>();

        #region CONFIG - Custos (edite aqui)
        // Criar publicação (taxa única)
        public const int Cost_CreatePublication = 100;

        // Envio de carta (custo fixo por destinatário)
        public const int Cost_SendLetter = 15;

        // Custo do anexo por stone (por destinatário)
        public const int Cost_AttachmentPerStone = 100;

        // Custo da publicação por assinante (por edição)
        public const int Cost_PerSubscriber_Scroll = 10; // Pergaminho
        public const int Cost_PerSubscriber_Book = 20;   // Livro

        private static readonly TimeSpan ComposeWindow = TimeSpan.FromHours(1);

        #endregion


        private readonly Mobile _from;
        private readonly CorreioSession _s;
        private readonly int _page;

        public static CorreioSession GetSession(Mobile m)
        {
            if (m == null)
                return null;

            CorreioSession s;
            if (!Sessions.TryGetValue(m.Serial, out s))
            {
                s = new CorreioSession();
                Sessions[m.Serial] = s;
            }
            return s;
        }

        private Mobile GetContextNpc()
        {
            return _s != null && _s.ContextNpcSerial > 0 ? World.FindMobile((Serial)_s.ContextNpcSerial) : null;
        }

        private int GetContextNpcCityId()
        {
            CorreioNPC npc = GetContextNpc() as CorreioNPC;
            return npc != null ? npc.GovernmentCityId : -1;
        }

        private bool WithdrawWithKingdomRevenue(Mobile payer, int amount)
        {
            if (amount <= 0)
                return true;

            if (!Banker.Withdraw(payer, amount))
                return false;

            Mobile npc = GetContextNpc();
            if (npc != null && !npc.Deleted)
                ReinoMaintenanceSystem.RecordRevenueFromNpc(npc, amount);

            return true;
        }

        public static void OpenSubscriptionNoticeIfAny(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            var mb = CorreioStorage.Instance.GetMailbox(from);
            if (mb == null || mb.Messages == null || mb.Messages.Count == 0)
                return;

            MailMessage msg = null;

            for (int i = 0; i < mb.Messages.Count; i++)
            {
                if (mb.Messages[i] != null && mb.Messages[i].IsPublicationChangeNotice)
                {
                    msg = mb.Messages[i];
                    break;
                }
            }

            if (msg == null)
                return;

            var s = GetSession(from);
            var pub = CorreioStorage.Instance.GetPublication(msg.PublicationId);

            string oldName = msg.OldName ?? (pub?.Name ?? "Publicação");
            string newName = msg.NewName ?? oldName;

            string oldDesc = msg.OldDescription ?? (pub?.Description ?? "");
            string newDesc = msg.NewDescription ?? oldDesc;

            int oldPrice = pub != null ? pub.PricePerIssue : 0;
            int newPrice = oldPrice;

            string oldPeriod = pub != null ? PeriodNamePT(pub.Period) : "";
            string newPeriod = oldPeriod;

            if (pub != null && pub.HasPendingPrice)
                newPrice = pub.PendingPrice;
            else if (msg.ProposedPrice > 0)
                newPrice = msg.ProposedPrice;

            if (pub != null && pub.HasPendingPeriod)
                newPeriod = PeriodNamePT(pub.PendingPeriod);
            else if (msg.ProposedPeriod.HasValue)
                newPeriod = PeriodNamePT(msg.ProposedPeriod.Value);

            s.Pending = PendingAction.AcceptNewTerms;
            s.SelectedSubscriptionPubId = msg.PublicationId;

            s.PendingHtml =
                "<BASEFONT COLOR=#FFFFFF>" +
                "<CENTER><B>Aviso de mudança na assinatura</B></CENTER><BR>" +
                $"<B>Nome antigo:</B> {oldName}<BR>" +
                $"<B>Novo nome:</B> {newName}<BR>" +
                $"<B>Periodicidade antiga:</B> {oldPeriod}<BR>" +
                $"<B>Nova periodicidade:</B> {newPeriod}<BR>" +
                $"<B>Valor antigo:</B> {oldPrice} moedas<BR>" +
                $"<B>Novo valor:</B> {newPrice} moedas<BR>" +
                $"<B>Descrição antiga:</B><BR>{oldDesc}<BR>" +
                $"<B>Nova descrição:</B><BR>{newDesc}<BR><BR>" +
                "<B>Sua assinatura está suspensa.</B><BR>" +
                "Você recebeu uma carta informando as mudanças.<BR>" +
                "Clique em <B>Aprovar</B> para aceitar os novos termos ou em <B>Cancelar</B> para encerrar a assinatura." +
                "</BASEFONT>";

            from.SendGump(new CorreiosGump(from, 9));
        }

        public CorreiosGump(Mobile from, int page = 1) : base(0, 0)
        {
            _from = from;
            _s = GetSession(from);
            _page = page;

            Closable = page == 9 ? false : true;
            Disposable = true;
            Dragable = page == 9 ? false : true;
            Resizable = false;

            BuildBase();
            BuildPage(page);
        }
        private static TimeSpan GetCooldown(PubPeriod p)
        {
            switch (p)
            {
                case PubPeriod.Weekly: return TimeSpan.FromHours(167);
                case PubPeriod.Monthly: return TimeSpan.FromHours(209);
                default: return TimeSpan.FromHours(23); // Daily
            }
        }

        // Called when the player closes the gump (e.g., presses ESC).
        // We use it to safely return any held publication attachment and reset the draft,
        // matching the behavior of the back arrow.
        public override void OnServerClose(NetState owner)
        {
            base.OnServerClose(owner);

            try
            {
                if (_from == null || _s == null)
                    return;

                // If the player was in the publication creation flow and we are holding an attachment,
                // return it and reset the draft.
                if (_page == 6 || _page == 7)
                {
                    if (_s.PubContentHeld != null || !String.IsNullOrEmpty(_s.PubName) || !String.IsNullOrEmpty(_s.PubDesc) || _s.PubPrice > 0)
                    {
                        _s.ClearPublicationDraft(_from);
                    }
                }
            }
            catch
            {
                // swallow
            }
        }

        private void BuildBase()
        {
            AddPage(0);
            AddImageTiled(324, 292, 417, 322, 388);
            AddImageTiled(732, 297, 21, 299, 369);
            AddImageTiled(304, 298, 26, 299, 370);
            AddImageTiled(322, 277, 420, 21, 371);
            AddImageTiled(322, 594, 420, 26, 372);
            AddImage(291, 574, 408);
            AddImage(715, 574, 409);
            AddImage(715, 267, 410);
            AddImage(291, 267, 411);
            AddImageTiled(330, 327, 402, 21, 475);
            AddLabel(501, 311, 0, "Correios");
        }

        private void BuildPage(int page)
        {
            switch (page)
            {
                case 1: BuildMenu(); break;
                case 2: BuildCheckMail(); break;
                case 3: BuildSendMail(); break;
                case 4: BuildSubscribeList(); break;
                case 5: BuildCancelSubscription(); break;
                case 6: BuildCreatePublication(); break;
                case 7: BuildCreatePublicationAttach(); break;
                case 8: BuildManagePublication(); break;
                case 10: BuildSendNewEditionAttach(); break;
                case 9: BuildAviso(); break;
                default: BuildMenu(); break;
            }
        }

        private void BuildMenu()
        {
            AddPage(0);

            AddButton(406, 387, 535, 535, 101, GumpButtonType.Reply, 0);
            AddLabel(443, 387, 0, "Checar");

            AddButton(406, 422, 535, 535, 102, GumpButtonType.Reply, 0);
            AddLabel(443, 422, 0, "Enviar");

            AddButton(406, 457, 535, 535, 103, GumpButtonType.Reply, 0);
            AddLabel(443, 457, 0, "Assinaturas");

            // Create publication
            var pub = CorreioStorage.Instance.GetPublicationByOwner(_from.Serial);

            AddButton(406, 492, 535, 535, 104, GumpButtonType.Reply, 0);
            AddLabel(443, 492, 0, pub != null ? "Gerenciar Publicação" : "Criar Publicação");

            // Mailbox id label
            int id = CorreioStorage.Instance.GetOrCreateMailboxId(_from);
            //AddLabel(350, 560, 0, $"Sua Caixa Postal: #{id}");
        }


        private void BuildCheckMail()
        {
            AddPage(0);

            var mb = CorreioStorage.Instance.GetMailbox(_from);
            int count = mb?.Messages?.Count ?? 0;

            AddLabel(394, 368, 0, $"Você tem {count} cartas");

            AddButton(350, 303, 508, 508, 299, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");

            int startY = 401;
            int max = Math.Min(count, 5);

            for (int i = 0; i < max; i++)
            {
                var msg = mb.Messages[i];
                int y = startY + (i * 28);

                string title = !string.IsNullOrWhiteSpace(msg.Title)
                    ? msg.Title
                    : "Carta";

                string fromName = !string.IsNullOrWhiteSpace(msg.FromName)
                    ? msg.FromName
                    : "Desconhecido";

                string data = msg.WorldCreated == DateTime.MinValue
                    ? msg.Created.ToString("dd/MM/yyyy")
                    : msg.WorldCreated.ToString("dd/MM/yyyy");

                string line = string.Format("{0} - {1}", fromName, title);

                AddButton(355, y + 2, 536, 536, 2200 + i, GumpButtonType.Reply, 0);
                AddLabel(379, y, 0, line);
                AddLabel(620, y, 0, data);
            }

            // AddHtml(367, 546, 333, 24, "<BASEFONT COLOR=#FFFFFF>Clique no botão ao lado da carta para recebê-la na mochila.</BASEFONT>", false, false);
        }


        private void BuildSendMail()
        {
            AddPage(0);

            AddLabel(369, 368, 0, "Destinatário:");
            AddImageTiled(456, 361, 239, 27, 400);
            AddTextEntry(460, 364, 239, 27, 0, 10, _s.ToName ?? "");

            AddLabel(369, 399, 0, "Custo do envio:");
            AddImageTiled(371, 429, 322, 107, 400);

            string html = "<BASEFONT COLOR=#000000>" + GetMailCostHtml(_s.MailAttachments) + "</BASEFONT>";
            AddHtml(376, 434, 312, 97, html, false, true);

            AddButton(671, 398, 535, 535, 301, GumpButtonType.Reply, 0);
            AddLabel(573, 398, 0, "Anexar Objeto");

            AddButton(676, 556, 521, 521, 302, GumpButtonType.Reply, 0);
            AddLabel(627, 559, 0, "Enviar");

            AddButton(350, 303, 508, 508, 303, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");

            int startY = 556;
            for (int i = 0; i < _s.MailAttachments.Count && i < 5; i++)
            {
                Item item = _s.MailAttachments[i];
                int y = startY - (i * 18);
                AddButton(361, y, 525, 525, 3300 + i, GumpButtonType.Reply, 0);
                AddLabel(401, 559, 0, "Cancelar Anexos");
            }
        }

        private void BuildSubscribeList()
        {
            AddPage(0);
            AddLabel(366, 351, 0, "Publicações disponíveis");

            var pubs = new List<Publication>(CorreioStorage.Instance.GetPublications());

            // remove da lista as que o jogador já assina
            var mySubs = CorreioStorage.Instance.GetSubscriptions(_from);
            var subSet = new HashSet<int>();
            for (int i = 0; i < mySubs.Count; i++)
                subSet.Add(mySubs[i].PublicationId);

            pubs.RemoveAll(p => subSet.Contains(p.PublicationId));

            if (CorreioStorage.Instance.GetPublication(_s.SelectedPublicationId) == null || subSet.Contains(_s.SelectedPublicationId))
                _s.SelectedPublicationId = 0;

            pubs.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            int startY = 393;
            int shown = 0;

            const int pageSize = 5;
            int page = Math.Max(0, _s.PubListPage);
            int startIndex = page * pageSize;
            bool hasPrev = page > 0;
            bool hasNext = pubs.Count > ((page + 1) * pageSize);

            for (int i = startIndex; i < pubs.Count && shown < pageSize; i++)
            {
                var p = pubs[i];
                int y = startY + (shown * 24);

                AddButton(365, y + 4, 536, 536, 400 + p.PublicationId, GumpButtonType.Reply, 0);
                AddLabel(390, y, 0, p.Name);

                shown++;
            }

            if (hasNext)
                AddButton(449, 517, 450, 450, 4201, GumpButtonType.Reply, 0); // próxima

            if (hasPrev)
                AddButton(394, 517, 451, 451, 4202, GumpButtonType.Reply, 0); // anterior

            string html = "<BASEFONT COLOR=#FFFFFF>Selecione uma publicação na lista.</BASEFONT>";
            var selected = CorreioStorage.Instance.GetPublication(_s.SelectedPublicationId);
            if (selected != null)
            {
                html = $"<BASEFONT COLOR=#FFFFFF><B>{selected.Name}</B><BR>" +
                       $"Valor: {selected.PricePerIssue} moedas<BR>" +
                       $"Frequencia: {selected.PeriodNamePT()}<BR><BR>" +
                       $"{(selected.Description ?? "").Replace("\n", "<BR>")}" +
                       "<BR><BR><I>Você receberá os exemplares pelo correio. </I>" +
                       "</BASEFONT>";
            }

            AddHtml(551, 366, 167, 180, html, false, true);

            AddButton(678, 563, 521, 521, 411, GumpButtonType.Reply, 0);
            AddLabel(621, 567, 0, "Assinar");

            AddButton(361, 563, 525, 525, 410, GumpButtonType.Reply, 0);
            AddLabel(397, 569, 0, "Cancelar assinaturas");

            AddButton(350, 303, 508, 508, 299, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");
        }

        private void BuildCancelSubscription()
        {
            AddPage(0);
            AddLabel(370, 359, 0, "Cancelar Assinatura");

            var subs = CorreioStorage.Instance.GetSubscriptions(_from);

            const int pageSize = 5;
            int page = Math.Max(0, _s.CancelListPage);
            int startIndex = page * pageSize;

            subs.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            int startY = 402;
            int shown = 0;
            for (int i = startIndex; i < subs.Count && shown < pageSize; i++)
            {
                var p = subs[i];
                int y = startY + (i * 24);
                AddButton(358, y + 4, 536, 536, 500 + p.PublicationId, GumpButtonType.Reply, 0);
                AddLabel(383, y, 0, p.Name);
                shown++;
            }

            string html = "<BASEFONT COLOR=#FFFFFF>Selecione uma assinatura.</BASEFONT>";
            var selected = CorreioStorage.Instance.GetPublication(_s.SelectedSubscriptionPubId);
            if (selected != null)
            {
                html = $"<BASEFONT COLOR=#FFFFFF><B>{selected.Name}</B><BR>" +
                       $"Valor: {selected.PricePerIssue} moedas<BR>" +
                       $"Periodicidade: {selected.PeriodNamePT()}<BR><BR>" +
                       $"{(selected.Description ?? "").Replace("\n", "<BR>")}" +
                       "</BASEFONT>";
            }

            AddHtml(551, 366, 167, 180, html, false, true);

            AddButton(678, 563, 525, 525, 511, GumpButtonType.Reply, 0);
            AddLabel(621, 567, 0, "Cancelar");

            AddButton(350, 303, 508, 508, 512, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");

            bool hasPrev = page > 0;
            bool hasNext = subs.Count > ((page + 1) * pageSize);

            if (hasNext)
                AddButton(449, 553, 450, 450, 5201, GumpButtonType.Reply, 0);

            if (hasPrev)
                AddButton(394, 553, 451, 451, 5202, GumpButtonType.Reply, 0);
        }

        private void BuildCreatePublication()
        {
            AddPage(0);
            AddLabel(475, 349, 0, "Criar Publicação");

            AddLabel(362, 384, 0, "Nome");
            AddImageTiled(458, 381, 237, 20, 400);
            AddTextEntry(462, 381, 237, 20, 0, 20, _s.PubName ?? "");

            AddLabel(362, 416, 0, "Valor");
            AddImageTiled(458, 413, 238, 20, 400);
            AddTextEntry(462, 413, 238, 20, 0, 21, _s.PubPrice > 0 ? _s.PubPrice.ToString() : "");


            int dArt = (_s.PubPeriod == PubPeriod.Daily ? 541 : 543);
            int wArt = (_s.PubPeriod == PubPeriod.Weekly ? 541 : 543);
            int mArt = (_s.PubPeriod == PubPeriod.Monthly ? 541 : 543);

            AddButton(363, 451, dArt, 541, 601, GumpButtonType.Reply, 0);
            AddLabel(388, 441, 0, "Diária");
            AddButton(489, 451, wArt, 541, 602, GumpButtonType.Reply, 0);
            AddLabel(514, 441, 0, "Semanal");
            AddButton(621, 451, mArt, 541, 603, GumpButtonType.Reply, 0);
            AddLabel(646, 441, 0, "Mensal");

            AddImageTiled(363, 476, 337, 80, 400);
            AddTextEntry(367, 476, 337, 80, 0, 22, _s.PubDesc ?? "");


            AddButton(624, 569, 559, 559, 604, GumpButtonType.Reply, 0);
         

            AddButton(350, 303, 508, 508, 605, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");
        }

        private void BuildCreatePublicationAttach()
        {
            AddPage(0);
            AddLabel(475, 349, 0, "Criar Publicação");

            AddLabel(370, 388, 0, "Anexar Publicação");
            AddButton(494, 392, 536, 536, 701, GumpButtonType.Reply, 0);

            AddLabel(372, 427, 0, "Custo por Assinante");
            AddHtml(548, 421, 168, 28, $"<BASEFONT COLOR=#FFFFFF>{(_s.PubCostPerSubscriber > 0 ? _s.PubCostPerSubscriber.ToString() : "-")}</BASEFONT>", true, false);

            AddLabel(372, 470, 0, "Custo Total");
            AddHtml(548, 465, 168, 28, $"<BASEFONT COLOR=#FFFFFF>{Cost_CreatePublication + (_s.PubCostPerSubscriber > 0 ? _s.PubCostPerSubscriber : 0)}</BASEFONT>", true, false);

            string status = _s.PubContentHeld == null ? "Nenhum item anexado" : $"Anexado: {GetPubItemTitle(_s.PubContentHeld)}";
            // Status label aligned with the other left-side labels.
            AddHtml(548, 387, 332, 20, $"<BASEFONT COLOR=#FFFFFF>{status}</BASEFONT>", false, false);

            AddButton(605, 517, 535, 535, 702, GumpButtonType.Reply, 0);
            AddLabel(640, 518, 0, "Publicar");

            AddButton(350, 303, 508, 508, 703, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");
        }



        private void BuildSendNewEditionAttach()
        {
            AddPage(0);

            var pub = CorreioStorage.Instance.GetPublicationByOwner(_from.Serial);
            if (pub == null)
            {
                AddLabel(360, 380, 0, "Você não tem publicação.");
                AddButton(350, 551, 508, 508, 299, GumpButtonType.Reply, 0);
                AddLabel(398, 558, 0, "Voltar");
                return;
            }

            AddLabel(455, 349, 0, "Enviar nova edição");

            AddLabel(370, 388, 0, "Anexar Publicação");
            AddButton(494, 392, 535, 535, 811, GumpButtonType.Reply, 0);

            // Status do anexo
            string status = _s.EditionContentHeld == null ? "Nenhum item anexado" : $"Anexado: {GetPubItemTitle(_s.EditionContentHeld)}";
            AddHtml(548, 387, 332, 20, $"<BASEFONT COLOR=#FFFFFF>{status}</BASEFONT>", false, false);

            int subs = pub.Subscribers != null ? pub.Subscribers.Count : 0;

            int perSub = CalculateEditionPerSubscriberCost(_s.EditionContentHeld);
            int total = perSub * subs;

            AddLabel(372, 427, 0, "Custo por Assinante");
            AddHtml(548, 421, 168, 28, $"<BASEFONT COLOR=#FFFFFF>{(perSub > 0 ? perSub.ToString() : "-")}</BASEFONT>", true, false);

            AddLabel(372, 464, 0, "Numero de Assinantes");
            AddHtml(548, 459, 168, 28, $"<BASEFONT COLOR=#FFFFFF>{subs}</BASEFONT>", true, false);

            AddLabel(372, 505, 0, "Custo Total");
            AddHtml(548, 500, 168, 28, $"<BASEFONT COLOR=#FFFFFF>{(subs > 0 && perSub > 0 ? total.ToString() : "-")}</BASEFONT>", true, false);

            AddButton(609, 560, 535, 535, 812, GumpButtonType.Reply, 0);
            AddLabel(644, 561, 0, "Publicar");

            AddButton(350, 551, 508, 508, 813, GumpButtonType.Reply, 0);
            AddLabel(398, 558, 0, "Voltar");
        }

        public static int CalculateEditionPerSubscriberCost(Item attached)
        {
            if (attached == null || attached.Deleted)
                return 0;

            int baseCost = 0;

            if (attached is HtmlDocumentBase hdoc)
                baseCost = hdoc.MailCostPerSubscriber;
            else if (attached is HtmlCompilationBook comp)
                baseCost = comp.MailCostPerSubscriber;
            else
                return 0;

            int pages = GetPublicationPageCount(attached);
            if (pages < 1)
                pages = 1;

            return baseCost + (pages * 2);
        }

        public static int GetPublicationPageCount(Item attached)
        {
            if (attached == null)
                return 0;

            try
            {
                var t = attached.GetType();

                var m = t.GetMethod("GetVisiblePageCount");
                if (m != null)
                {
                    object val = m.Invoke(attached, null);
                    if (val is int)
                        return (int)val;
                }

                var p = t.GetProperty("PageCount");
                if (p != null)
                {
                    object val = p.GetValue(attached, null);
                    if (val is int)
                        return (int)val;
                }

                var p2 = t.GetProperty("Pages");
                if (p2 != null)
                {
                    var list = p2.GetValue(attached, null) as System.Collections.ICollection;
                    if (list != null)
                        return list.Count;
                }
            }
            catch
            {
            }

            return 1;
        }

        public static bool IsClosedCompilationBook(Item item)
        {
            if (item == null)
                return false;

            if (!(item is HtmlCompilationBook))
                return true;

            try
            {
                var t = item.GetType();

                var pOpen = t.GetProperty("Opened");
                if (pOpen != null)
                {
                    object val = pOpen.GetValue(item, null);
                    if (val is bool)
                        return !((bool)val);
                }

                var pIsOpen = t.GetProperty("IsOpen");
                if (pIsOpen != null)
                {
                    object val = pIsOpen.GetValue(item, null);
                    if (val is bool)
                        return !((bool)val);
                }
            }
            catch
            {
            }

            return true;
        }

        private void BuildManagePublication()
        {
            AddPage(0);
            AddLabel(464, 348, 0, "Gerenciar Assinatura");

            var pub = CorreioStorage.Instance.GetPublicationByOwner(_from.Serial);
            if (pub == null)
            {
                AddLabel(360, 380, 0, "Você não tem publicação.");
                AddButton(350, 551, 508, 508, 299, GumpButtonType.Reply, 0);
                AddLabel(398, 558, 0, "Voltar");
                return;
            }

            // Enviar nova edição (manual)
            AddButton(357, 377, 535, 535, 809, GumpButtonType.Reply, 0);
            AddLabel(390, 377, 0, "Enviar nova edição");

            // Alterações (salvas somente ao clicar em "Salvar Alterações")
            AddLabel(544, 377, 0, "Mudar Valor");
            AddImageTiled(634, 374, 76, 20, 400);
            AddTextEntry(637, 374, 76, 20, 0, 30, "");

            AddLabel(355, 406, 0, "Mudar Nome");
            AddImageTiled(446, 403, 266, 20, 400);
            AddTextEntry(449, 403, 266, 20, 0, 31, "");

            AddLabel(353, 432, 0, "Mudar Periodicidade");

            PubPeriod shown = _s.NewPeriodSelected ? _s.NewPeriod : pub.Period;

            int dArt = (shown == PubPeriod.Daily ? 541 : 543);
            int wArt = (shown == PubPeriod.Weekly ? 541 : 543);
            int mArt = (shown == PubPeriod.Monthly ? 541 : 543);

            AddButton(506, 436, dArt, 543, 806, GumpButtonType.Reply, 0);
            AddLabel(531, 432, 0, "D");
            AddButton(594, 436, wArt, 543, 807, GumpButtonType.Reply, 0);
            AddLabel(619, 432, 0, "S");
            AddButton(672, 437, mArt, 543, 808, GumpButtonType.Reply, 0);
            AddLabel(697, 433, 0, "M");

            AddLabel(355, 456, 0, "Mudar Descrição da Publicação");
            AddImageTiled(359, 484, 346, 71, 400);
            AddTextEntry(361, 484, 346, 71, 0, 32, "");

            // Salvar alterações
            AddButton(675, 564, 521, 521, 521, GumpButtonType.Reply, 0);
            AddLabel(555, 568, 0, "Salvar Alterações");

            // Cancelar publicação (movido para baixo)
            AddButton(360, 564, 525, 525, 801, GumpButtonType.Reply, 0);
            AddLabel(394, 568, 0, "Cancela Publicação");

            // Voltar
            AddButton(350, 303, 508, 508, 299, GumpButtonType.Reply, 0);
            AddLabel(396, 310, 0, "Voltar");

            if (pub.Suspended)
                AddLabel(360, 565, 0, "(Assinaturas suspensas aguardando confirmações)");
        }


        private void BuildAviso()
        {
            AddPage(0);
            AddLabel(510, 352, 0, "Aviso");
            AddHtml(362, 390, 339, 144, _s.PendingHtml ?? "", false, true);

            AddButton(621, 551, 559, 559, 902, GumpButtonType.Reply, 0);
            AddButton(365, 551, 544, 544, 901, GumpButtonType.Reply, 0);
        }

        private static string PeriodNamePT(PubPeriod p)
        {
            switch (p)
            {
                case PubPeriod.Weekly: return "Semanal";
                case PubPeriod.Monthly: return "Mensal";
                default: return "Diária";
            }
        }


        private static string GetAttachmentLabel(Item item)
        {
            if (item == null || item.Deleted)
                return "anexo inválido";

            int stones = GetAttachmentStoneCost(item);
            string name = item.Name ?? item.GetType().Name;
            return string.Format("{0} ({1} stones)", name, stones);
        }

        private static int GetAttachmentStoneCost(Item item)
        {
            if (item == null || item.Deleted)
                return 0;

            double weight = item.TotalWeight;
            if (weight <= 0.0)
                weight = item.Weight;

            return (int)Math.Ceiling(weight);
        }

        private static bool ContainsCoins(Item item)
        {
            if (item == null || item.Deleted)
                return false;

            if (item is Gold || item is BankCheck)
                return true;

            Container c = item as Container;
            if (c != null)
            {
                foreach (Item child in c.Items)
                    if (ContainsCoins(child))
                        return true;
            }

            return false;
        }

        private static bool IsValidMailAttachment(Item item, Mobile from, out string reason)
        {
            reason = null;

            if (item == null || item.Deleted)
            {
                reason = "Item inválido.";
                return false;
            }

            if (!item.IsChildOf(from.Backpack))
            {
                reason = "O item precisa estar na sua mochila.";
                return false;
            }

            if (item is Gold || item is BankCheck)
            {
                reason = "Não é possível enviar moedas.";
                return false;
            }

            if (item is Carta)
                return true;

            CorreioPacote pacote = item as CorreioPacote;
            if (pacote == null)
            {
                reason = "Você só pode enviar cartas ou pacotes com selo.";
                return false;
            }

            if (!pacote.SealedForMail)
            {
                reason = "O pacote precisa estar com selo para envio.";
                return false;
            }

            if (pacote.Items == null || pacote.Items.Count == 0)
            {
                reason = "O pacote está vazio.";
                return false;
            }

            if (ContainsCoins(pacote))
            {
                reason = "Não é possível enviar moedas dentro de pacotes.";
                return false;
            }

            int currentWeight = GetAttachmentStoneCost(item);

            if (pacote.SealedWeight <= 0)
            {
                reason = "Esse pacote está inválido.";
                return false;
            }

            if (currentWeight != pacote.SealedWeight)
            {
                reason = "O peso do pacote mudou depois de ser selado.";
                return false;
            }

            if (currentWeight > 26)
            {
                reason = "Cada anexo pode ter no máximo 26 stones, contando o pacote.";
                return false;
            }

            return true;
        }

        private static int CalculateMailSendCost(List<Item> attachments)
        {
            int total = 0;

            if (attachments == null)
                return total;

            for (int i = 0; i < attachments.Count; i++)
            {
                Item item = attachments[i];
                if (item == null || item.Deleted)
                    continue;

                if (item is Carta)
                    total += 20;
                else
                    total += GetAttachmentStoneCost(item) * Cost_AttachmentPerStone;
            }

            return total;
        }

        private static string GetMailCostHtml(List<Item> attachments)
        {
            if (attachments == null || attachments.Count == 0)
                return "Anexe até 5 itens.<BR>Cartas custam 20 moedas cada.<BR>Pacotes custam 100 moedas por stone e devem ter no máximo 26 stones no total.";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int total = 0;

            for (int i = 0; i < attachments.Count; i++)
            {
                Item item = attachments[i];
                if (item == null || item.Deleted)
                    continue;

                string name = item.Name ?? item.GetType().Name;
                int cost = item is Carta ? 20 : GetAttachmentStoneCost(item) * Cost_AttachmentPerStone;
                total += cost;

                sb.AppendFormat("{0}. {1}: {2} moedas<BR>", i + 1, name, cost);
            }

            sb.AppendFormat("<BR><B>Total:</B> {0} moedas", total);
            return sb.ToString();
        }

        private void HandleReceiveMail(Mobile from, int index)
        {
            var mb = CorreioStorage.Instance.GetMailbox(from);
            if (mb == null || index < 0 || index >= mb.Messages.Count)
            {
                from.SendGump(new CorreiosGump(from, 2));
                return;
            }

            var msg = mb.Messages[index];

            if (msg.IsPublicationChangeNotice)
            {
                from.SendMessage("Esse aviso deve ser resolvido na janela de assinatura.");
                from.SendGump(new CorreiosGump(from, 2));
                return;
            }

            if (from.Backpack == null)
            {
                from.SendMessage("Você precisa de uma mochila.");
                return;
            }

            if (msg.Attachments != null && msg.Attachments.Count > 0)
            {
                foreach (Item item in msg.Attachments)
                {
                    if (item != null && !item.Deleted)
                        from.Backpack.DropItem(item);
                }
            }
            else if (msg.Attachment != null && !msg.Attachment.Deleted)
            {
                from.Backpack.DropItem(msg.Attachment);
            }

            mb.Messages.RemoveAt(index);
            from.SendGump(new CorreiosGump(from, 2));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (from == null)
                return;

            CorreioStorage.Ensure();

            // FECHOU O GUMP (botão direito / ESC) -> ButtonID = 0
            if (info.ButtonID == 0)
            {
                // Se estava no fluxo de criação de publicação e tinha coisa anexada/digitada,
                // devolve o item e reseta como o botão "Voltar"
                if (_page == 6 || _page == 7)
                {
                    _s.ClearPublicationDraft(_from);
                }

                if (_page == 10)
                {
                    _s.ClearEditionDraft(_from);
                }

                // não reabre gump
                return;
            }

            switch (info.ButtonID)
            {
                case 0:
                    {
                        // Se fechou durante criação/attach da publicação, devolve item e limpa rascunho
                        if (_page == 6 || _page == 7)
                            _s.ClearPublicationDraft(from);

                        if (_page == 10)
                            _s.ClearEditionDraft(from);

                        // Se fechou durante envio de carta, também limpa (opcional, mas recomendado)
                        if (_page == 3)
                            _s.ClearMailDraft(from);

                        return;
                    }

                case 101:
                    {
                        var mb = CorreioStorage.Instance.GetMailbox(from);
                        int count = mb?.Messages?.Count ?? 0;

                        if (count > 0)
                        {
                            _s.MailIndex = Math.Max(0, Math.Min(_s.MailIndex, count - 1));
                            var msg = mb.Messages[_s.MailIndex];

                            // Se for aviso especial de mudança de assinatura
                            if (msg != null && msg.IsPublicationChangeNotice)
                            {
                                var pub = CorreioStorage.Instance.GetPublication(msg.PublicationId);

                                string oldName = msg.OldName ?? (pub?.Name ?? "Publicação");
                                string newName = msg.NewName ?? oldName;

                                string oldDesc = msg.OldDescription ?? (pub?.Description ?? "");
                                string newDesc = msg.NewDescription ?? oldDesc;

                                int oldPrice = pub != null ? pub.PricePerIssue : 0;
                                int newPrice = oldPrice;

                                string oldPeriod = pub != null ? PeriodNamePT(pub.Period) : "";
                                string newPeriod = oldPeriod;

                                // O seu sistema hoje só tem "Pending" para Preço e Periodicidade
                                if (pub != null && pub.HasPendingPrice)
                                    newPrice = pub.PendingPrice;

                                if (pub != null && pub.HasPendingPeriod)
                                    newPeriod = PeriodNamePT(pub.PendingPeriod);

                                // Se no futuro você adicionar PendingName/PendingDescription, aqui entra:
                                // if (pub.HasPendingName) newName = pub.PendingName;
                                // if (pub.HasPendingDescription) newDesc = pub.PendingDescription;

                                _s.Pending = PendingAction.AcceptNewTerms;
                                _s.SelectedSubscriptionPubId = msg.PublicationId;

                                _s.PendingHtml =
                                    "<BASEFONT COLOR=#FFFFFF>" +
                                    "<CENTER><B>Aviso de mudança na assinatura</B></CENTER><BR>" +
                                    $"<B>Nome antigo:</B> {oldName}<BR>" +
                                    $"<B>Novo nome:</B> {newName}<BR>" +
                                    $"<B>Periodicidade antiga:</B> {oldPeriod}<BR>" +
                                    $"<B>Nova periodicidade:</B> {newPeriod}<BR>" +
                                    $"<B>Valor antigo:</B> {oldPrice} moedas<BR>" +
                                    $"<B>Novo valor:</B> {newPrice} moedas<BR>" +
                                    $"<B>Descrição antiga:</B><BR>{oldDesc}<BR>" +
                                    $"<B>Nova descrição:</B><BR>{newDesc}<BR>" +
                                    "<B>Sua assinatura está suspensa.</B>" +
                                    "Nenhum valor foi cobrado desde a mudança.<BR>" +
                                    "Para <B>aceitar</B> as mudanças, clique em <B>Aceitar</B>.<BR>" +
                                    "Para <B>cancelar</B>, clique em <B>Cancelar</B> e sua assinatura será encerrada." +
                                    "</BASEFONT>";

                                from.SendGump(new CorreiosGump(from, 9)); // AVISO
                                return;
                            }
                        }

                        // Se não for aviso especial, abre o gump normal de checar cartas
                        from.SendGump(new CorreiosGump(from, 2));
                        return;
                    }

                case 102:
                    from.SendGump(new CorreiosGump(from, 3));
                    return;

                case 103:
                    from.SendGump(new CorreiosGump(from, 4));
                    return;

                case 104:
                    {
                        if (CorreioStorage.Instance.GetPublicationByOwner(from.Serial) != null)
                            from.SendGump(new CorreiosGump(from, 8));
                        else
                            from.SendGump(new CorreiosGump(from, 6));

                        return;
                    }

                case 105:
                    _s.NewPeriodSelected = false;
                    from.SendGump(new CorreiosGump(from, 8));
                    return;

                case 809:
                    {
                        var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
                        if (pub == null)
                        {
                            from.SendMessage("Você não tem uma publicação.");
                            from.SendGump(new CorreiosGump(from, 8));
                            return;
                        }

                        // Se já começou a compor e ainda está dentro da 1h, deixa continuar
                        if (pub.ComposeStartUtc != DateTime.MinValue && (DateTime.UtcNow - pub.ComposeStartUtc) <= ComposeWindow)
                        {
                            from.SendGump(new CorreiosGump(from, 10));
                            return;
                        }

                        // Se passou da 1h, expira e força começar do zero
                        pub.ComposeStartUtc = DateTime.MinValue;

                        // COOLDOWN por periodicidade
                        TimeSpan cd = GetCooldown(pub.Period);

                        if (pub.LastEditionSentUtc != DateTime.MinValue && (DateTime.UtcNow - pub.LastEditionSentUtc) < cd)
                        {
                            TimeSpan falta = (pub.LastEditionSentUtc + cd) - DateTime.UtcNow;

                            // Use TotalHours (Hours engana porque só mostra 0-23)
                            double horas = Math.Ceiling(falta.TotalHours);
                            from.SendMessage($"Você ainda não pode enviar outra edição. Faltam {horas} horas.");
                            from.SendGump(new CorreiosGump(from, 8));
                            return;
                        }

                        // Marca início da composição (1h pra preparar)
                        pub.ComposeStartUtc = DateTime.UtcNow;

                        from.SendGump(new CorreiosGump(from, 10));
                        return;
                    }

                case 811:
                    from.SendMessage("Clique em um livro escrevível ou no Pergaminho em Branco (na sua mochila). O item será guardado pelo sistema.");
                    from.Target = new PublicationEditionAttachTarget(_s, from);
                    from.SendGump(new CorreiosGump(from, 10));
                    return;

                case 812:
                    PrepareSendNewEditionConfirm(from);
                    return;

                case 813:
                    _s.ClearEditionDraft(from);
                    from.SendGump(new CorreiosGump(from, 8));
                    return;

                case 299:
                case 512:
                    from.SendGump(new CorreiosGump(from, 1));
                    return;

                case 202:
                    HandleGuardarCarta(from);
                    return;

                case 203:
                    // Next: warning first
                    _s.Pending = PendingAction.MailDeleteConfirm;
                    _s.PendingHtml = " <BASEFONT COLOR=#FFFFFF>Passar para a próxima carta irá <B>deletar esta carta para sempre</B>.\n<BR><BR>Se você quer guardar, clique em <B>Guardar carta</B> primeiro.\n<BR><BR>Se fechar a janela, este aviso aparecerá novamente.</BASEFONT>";
                    from.SendGump(new CorreiosGump(from, 9));
                    return;

                // Page3
                case 301:
                    _s.ToName = info.GetTextEntry(10)?.Text;
                    from.SendMessage("Selecione um item da sua mochila para anexar.");
                    from.Target = new MailAttachTarget(_s, from);
                    from.SendGump(new CorreiosGump(from, 3));
                    return;

                case 302:
                    _s.ToName = info.GetTextEntry(10)?.Text;
                    PrepareSendMailConfirm(from);
                    return;

                case 303:
                    // Back cancels and returns attachment
                    _s.ClearMailDraft(from);
                    from.SendGump(new CorreiosGump(from, 1));
                    return;

                // Page4 publication selection
                case 410:
                    from.SendGump(new CorreiosGump(from, 5));
                    return;

                case 411:
                    PrepareSubscribeConfirm(from);
                    return;

                // Page5 select and cancel
                case 511:
                    PrepareUnsubscribeConfirm(from);
                    return;

                // Page6 periodicity
                case 601:
                    SaveCreatePubEntries(info);
                    _s.PubPeriod = PubPeriod.Daily;
                    from.SendGump(new CorreiosGump(from, 6));
                    return;

                case 602:
                    SaveCreatePubEntries(info);
                    _s.PubPeriod = PubPeriod.Weekly;
                    from.SendGump(new CorreiosGump(from, 6));
                    return;

                case 603:
                    SaveCreatePubEntries(info);
                    _s.PubPeriod = PubPeriod.Monthly;
                    from.SendGump(new CorreiosGump(from, 6));
                    return;

                case 604:
                    SaveCreatePubEntries(info);
                    // Validate basic fields
                    if (String.IsNullOrWhiteSpace(_s.PubName))
                    {
                        from.SendMessage("Digite o nome da publicação.");
                        from.SendGump(new CorreiosGump(from, 6));
                        return;
                    }
                    if (String.IsNullOrWhiteSpace(_s.PubDesc))
                    {
                        from.SendMessage("Digite a descrição da publicação.");
                        from.SendGump(new CorreiosGump(from, 6));
                        return;
                    }
                    if (_s.PubPrice <= 0)
                    {
                        from.SendMessage("Digite um valor maior que 0.");
                        from.SendGump(new CorreiosGump(from, 6));
                        return;
                    }

                    from.SendGump(new CorreiosGump(from, 7));
                    return;

                case 605:
                    _s.ClearPublicationDraft(from);
                    from.SendGump(new CorreiosGump(from, 1));
                    return;

                // Page7
                case 701:
                    from.SendMessage("Clique em um livro escrevível ou no Pergaminho em Branco (na sua mochila). O item será guardado pelo sistema.");
                    from.Target = new PublicationAttachTarget(_s, from);
                    from.SendGump(new CorreiosGump(from, 7));
                    return;

                case 702:
                    PrepareCreatePublicationConfirm(from);
                    return;

                case 703:
                    // Back to page6 keeping info
                    from.SendGump(new CorreiosGump(from, 6));
                    return;

                // Page8 manage publication
                case 801:
                    PrepareCancelPublicationConfirm(from);
                    return;

                case 806:
                    _s.NewPeriod = PubPeriod.Daily;
                    _s.NewPeriodSelected = true;
                    from.SendGump(new CorreiosGump(from, 8));
                    return;

                case 807:
                    _s.NewPeriod = PubPeriod.Weekly;
                    _s.NewPeriodSelected = true;
                    from.SendGump(new CorreiosGump(from, 8));
                    return;

                case 808:
                    _s.NewPeriod = PubPeriod.Monthly;
                    _s.NewPeriodSelected = true;
                    from.SendGump(new CorreiosGump(from, 8));
                    return;

                case 802:
                    PrepareChangePriceConfirm(from, info);
                    return;

                case 803:
                    PrepareChangeNameConfirm(from, info);
                    return;

                case 804:
                    PrepareChangePeriodConfirm(from);
                    return;

                case 805:
                    ApplyChangeDescription(from, info);
                    return;

                // Aviso
                case 901:
                    HandleAvisoVoltar(from);
                    return;

                case 521:
                    PrepareUpdatePublicationConfirm(from, info);
                    return;

                case 902:
                    HandleAvisoProximo(from);
                    return;

                case 4201: // próxima página publicações
                    _s.PubListPage++;
                    from.SendGump(new CorreiosGump(from, 4));
                    return;

                case 4202: // página anterior publicações
                    _s.PubListPage = Math.Max(0, _s.PubListPage - 1);
                    from.SendGump(new CorreiosGump(from, 4));
                    return;

                case 5201:
                    _s.CancelListPage++;
                    from.SendGump(new CorreiosGump(from, 5));
                    return;

                case 5202:
                    _s.CancelListPage = Math.Max(0, _s.CancelListPage - 1);
                    from.SendGump(new CorreiosGump(from, 5));
                    return;

                default:
                    if (info.ButtonID >= 2200 && info.ButtonID < 2205)
                    {
                        HandleReceiveMail(from, info.ButtonID - 2200);
                        return;
                    }

                    if (info.ButtonID >= 3300 && info.ButtonID < 3305)
                    {
                        int idx = info.ButtonID - 3300;
                        if (_s.MailAttachments != null && idx >= 0 && idx < _s.MailAttachments.Count)
                            _s.MailAttachments.RemoveAt(idx);

                        from.SendGump(new CorreiosGump(from, 3));
                        return;
                    }

                    // Publication list selection buttons
                    if (info.ButtonID >= 400 && info.ButtonID < 500)
                    {
                        _s.SelectedPublicationId = info.ButtonID - 400;
                        from.SendGump(new CorreiosGump(from, 4));
                        return;
                    }

                    if (info.ButtonID >= 500 && info.ButtonID < 600)
                    {
                        _s.SelectedSubscriptionPubId = info.ButtonID - 500;
                        from.SendGump(new CorreiosGump(from, 5));
                        return;
                    }

                    break;
            }
        }

        private void SaveCreatePubEntries(RelayInfo info)
        {
            _s.PubName = info.GetTextEntry(20)?.Text;
            int val;
            if (int.TryParse(info.GetTextEntry(21)?.Text, out val))
                _s.PubPrice = val;
            _s.PubDesc = info.GetTextEntry(22)?.Text;
        }

        private void HandleGuardarCarta(Mobile from)
        {
            HandleReceiveMail(from, 0);
        }


        private void PrepareSendMailConfirm(Mobile from)
        {
            string toName = _s.ToName;

            if (String.IsNullOrWhiteSpace(toName))
            {
                from.SendMessage("Digite o nome do destinatário.");
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }

            if (String.Equals(from.Name, toName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                from.SendMessage("Você não pode enviar correio para você mesmo.");
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }

            Mobile dest;
            if (!CorreioStorage.Instance.TryFindMobileByName(toName, out dest))
            {
                from.SendMessage("Não encontrei nenhum personagem com esse nome.");
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }

            PlayerMobile senderPm = from as PlayerMobile;

            string diplomacyReason;

            if (senderPm != null && !ReinoDiplomacySystem.CanExchangeMail(senderPm, dest, GetContextNpcCityId(), out diplomacyReason))
            {
                from.SendMessage(diplomacyReason);
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }

            if (_s.MailAttachments == null || _s.MailAttachments.Count == 0)
            {
                from.SendMessage("Você precisa anexar ao menos um item.");
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }


            for (int i = 0; i < _s.MailAttachments.Count; i++)
            {
                string reason;
                if (!IsValidMailAttachment(_s.MailAttachments[i], from, out reason))
                {
                    from.SendMessage(reason);
                    from.SendGump(new CorreiosGump(from, 3));
                    return;
                }
            }

            ExecuteSendMail(from);

        }

        private void PrepareSubscribeConfirm(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublication(_s.SelectedPublicationId);
            if (pub == null)
            {
                from.SendMessage("Selecione uma publicação primeiro.");
                from.SendGump(new CorreiosGump(from, 4));
                return;
            }

            _s.Pending = PendingAction.SubscribeConfirm;
            _s.PendingHtml = $"<BASEFONT COLOR=#FFFFFF>Você está prestes a assinar <B>{pub.Name}</B>.\n<BR><BR>Valor: {pub.PricePerIssue} moedas ({pub.PeriodNamePT()}).\n<BR><BR>O banco fará o saque automático de acordo com a periodicidade.\n<BR>Ao confirmar, você recebe o último exemplar (sem retroativos).\n<BR><BR>Confirmar?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void PrepareUnsubscribeConfirm(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublication(_s.SelectedSubscriptionPubId);
            if (pub == null)
            {
                from.SendMessage("Selecione uma assinatura.");
                from.SendGump(new CorreiosGump(from, 5));
                return;
            }

            _s.Pending = PendingAction.UnsubscribeConfirm;
            _s.PendingHtml = $"<BASEFONT COLOR=#FFFFFF>Você está prestes a cancelar a assinatura de <B>{pub.Name}</B>.\n<BR><BR>Você não receberá o próximo exemplar e o banco não fará mais saques automáticos.\n<BR><BR>Confirmar?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void PrepareCreatePublicationConfirm(Mobile from)
        {
            if (_s.PubContentHeld == null || _s.PubContentHeld.Deleted)
            {
                from.SendMessage("Você precisa anexar um livro ou pergaminho antes.");
                from.SendGump(new CorreiosGump(from, 7));
                return;
            }

            _s.Pending = PendingAction.CreatePublicationConfirm;
            int totalCreateCost = Cost_CreatePublication + _s.PubCostPerSubscriber;
            _s.PendingHtml = $"<BASEFONT COLOR=#FFFFFF>Você está prestes a criar a publicação <B>{_s.PubName}</B>.\n<BR><BR>Custo de criação: <B>{Cost_CreatePublication}</B> moedas.\n<BR>Custo do item anexado: <B>{_s.PubCostPerSubscriber}</B> moedas.\n<BR>Custo total agora: <B>{totalCreateCost}</B> moedas.\n<BR><BR>Para cada assinante, você receberá: (valor cobrado) - (custo por assinante).\n<BR>Exemplo: se cobrar 50 e o custo por assinante for {_s.PubCostPerSubscriber}, você recebe {Math.Max(0, 50 - _s.PubCostPerSubscriber)} por assinante.\n<BR><BR>Confirmar criação?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }


        private void PrepareSendNewEditionConfirm(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // 1h pra preparar
            if (pub.ComposeStartUtc == DateTime.MinValue || (DateTime.UtcNow - pub.ComposeStartUtc) > ComposeWindow)
            {
                from.SendMessage("Seu tempo para preparar a edição expirou. Clique em 'Enviar nova edição' novamente.");
                pub.ComposeStartUtc = DateTime.MinValue;
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // garantia extra: cooldown ainda válido
            TimeSpan cd = GetCooldown(pub.Period);
            if (pub.LastEditionSentUtc != DateTime.MinValue && (DateTime.UtcNow - pub.LastEditionSentUtc) < cd)
            {
                TimeSpan falta = (pub.LastEditionSentUtc + cd) - DateTime.UtcNow;
                double horas = Math.Ceiling(falta.TotalHours);
                from.SendMessage($"Você ainda não pode publicar. Faltam {horas} horas.");
                pub.ComposeStartUtc = DateTime.MinValue;
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            if (_s.EditionContentHeld == null || _s.EditionContentHeld.Deleted)
            {
                from.SendMessage("Você precisa anexar um livro ou pergaminho antes.");
                from.SendGump(new CorreiosGump(from, 10));
                return;
            }

            int subs = pub.Subscribers != null ? pub.Subscribers.Count : 0;
            int perSub = CalculateEditionPerSubscriberCost(_s.EditionContentHeld);
            int total = perSub * subs;

            _s.Pending = PendingAction.SendNewEditionConfirm;
            _s.PendingHtml =
                $"<BASEFONT COLOR=#FFFFFF>Você está prestes a enviar uma <B>nova edição</B> de <B>{pub.Name}</B>.\n" +
                $"<BR><BR>Assinantes: <B>{subs}</B>\n" +
                $"<BR>Custo por assinante: <B>{perSub}</B> moedas\n" +
                $"<BR>Custo total: <B>{total}</B> moedas\n" +
                "<BR><BR>Confirmar publicação?</BASEFONT>";

            from.SendGump(new CorreiosGump(from, 9));
        }

        private int CountItems<T>(Mobile m) where T : Item
        {
            if (m?.Backpack == null)
                return 0;

            int count = 0;
            foreach (Item it in m.Backpack.FindItemsByType(typeof(T), true))
            {
                if (it != null && !it.Deleted)
                    count++;
            }
            return count;
        }

        private bool ConsumeItems<T>(Mobile m, int amount) where T : Item
        {
            if (amount <= 0)
                return true;

            if (m?.Backpack == null)
                return false;

            var items = m.Backpack.FindItemsByType(typeof(T), true);
            if (items == null || items.Length < amount)
                return false;

            int consumed = 0;
            foreach (Item it in items)
            {
                if (it == null || it.Deleted)
                    continue;

                it.Delete();
                consumed++;
                if (consumed >= amount)
                    break;
            }

            return consumed >= amount;
        }

        private void PrepareUpdatePublicationConfirm(Mobile from, RelayInfo info)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // Read entries (optional)
            string priceText = info.GetTextEntry(30)?.Text;
            string newName = info.GetTextEntry(31)?.Text;
            string newDesc = info.GetTextEntry(32)?.Text;

            int parsedPrice = -1;
            if (!String.IsNullOrWhiteSpace(priceText))
            {
                int p;
                if (!int.TryParse(priceText.Trim(), out p) || p <= 0)
                {
                    from.SendMessage("O novo valor precisa ser um número maior que 0.");
                    from.SendGump(new CorreiosGump(from, 8));
                    return;
                }
                parsedPrice = p;
            }

            // Determine changes
            bool changePrice = parsedPrice > 0 && parsedPrice != pub.PricePerIssue;
            bool changeName = !String.IsNullOrWhiteSpace(newName) && !String.Equals(newName.Trim(), pub.Name, StringComparison.OrdinalIgnoreCase);
            bool changeDesc = !String.IsNullOrWhiteSpace(newDesc) && !String.Equals(newDesc.Trim(), pub.Description ?? "", StringComparison.Ordinal);
            bool changePeriod = _s.NewPeriodSelected && _s.NewPeriod != pub.Period;

            if (!changePrice && !changeName && !changeDesc && !changePeriod)
            {
                from.SendMessage("Você não mudou nada.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // Store in session
            _s.NewPrice = parsedPrice; // -1 means unchanged
            _s.NewName = newName?.Trim();
            _s.NewDesc = newDesc?.Trim();

            // Build aviso
            int subs = pub.Subscribers.Count;
            int moneyPerSub = Cost_SendLetter;
            int totalMoney = (changePrice || changePeriod) ? (moneyPerSub * subs) : 0;

            string html = "<BASEFONT COLOR=#FFFFFF>";

            html += "<B>Você está prestes a alterar sua publicação.</B><BR><BR>";

            if (changeName)
                html += $"• <B>Nome</B> (custo 50 moedas): {_s.NewName}<BR>";

            if (changeDesc)
                html += "• <B>Descrição</B> (grátis)<BR>";

            if (changePrice || changePeriod)
            {
                html += "<BR><B>Mudanças que exigem aviso aos assinantes:</B><BR>";
                if (changePrice)
                    html += $"• <B>Novo valor</B>: {parsedPrice} moedas<BR>";
                if (changePeriod)
                    html += $"• <B>Nova periodicidade</B>: {PeriodNamePT(_s.NewPeriod)}<BR>";

                html += "<BR>";
                html += $"Para enviar o aviso a todos os assinantes, você precisa pagar <B>{moneyPerSub} moedas por assinante</B><BR>";
                html += $"Assinantes atuais: <B>{subs}</B><BR>";
                html += $"Total: <B>{totalMoney} moedas</B> (retiradas do banco)<BR><BR>";
                html += "<B>Importante:</B> até os jogadores concordarem com a mudança, as assinaturas ficam <B>suspensas</B>.<BR>";
            }

            html += "<BR>Confirmar?</BASEFONT>";

            _s.Pending = PendingAction.UpdatePublicationConfirm;
            _s.PendingHtml = html;
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void ExecuteUpdatePublication(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            bool changePrice = _s.NewPrice > 0 && _s.NewPrice != pub.PricePerIssue;
            bool changeName = !String.IsNullOrWhiteSpace(_s.NewName) && !String.Equals(_s.NewName, pub.Name, StringComparison.OrdinalIgnoreCase);
            bool changeDesc = !String.IsNullOrWhiteSpace(_s.NewDesc) && !String.Equals(_s.NewDesc, pub.Description ?? "", StringComparison.Ordinal);
            bool changePeriod = _s.NewPeriodSelected && _s.NewPeriod != pub.Period;

            int subs = pub.Subscribers.Count;

            string oldNameSnapshot = pub.Name;
            string oldDescSnapshot = pub.Description ?? "";

            // Name change (always immediate, no suspension)
            if (changeName)
            {
                if (!WithdrawWithKingdomRevenue(from, 50))
                {
                    from.SendMessage("Você precisa de 50 moedas no banco para mudar o nome.");
                    from.SendGump(new CorreiosGump(from, 8));
                    return;
                }

                pub.Name = _s.NewName;
            }

            // Description change (free)
            if (changeDesc)
            {
                pub.Description = _s.NewDesc;
            }

            string newNameSnapshot = pub.Name;
            string newDescSnapshot = pub.Description ?? "";

            // Price/Period changes
            if (changePrice || changePeriod)
            {
                if (subs <= 0)
                {
                    // No subscribers: apply immediately
                    if (changePrice)
                        pub.PricePerIssue = _s.NewPrice;
                    if (changePeriod)
                        pub.Period = _s.NewPeriod;

                    from.SendMessage("Publicação atualizada (sem assinantes).");
                    _s.NewPeriodSelected = false;
                    from.SendGump(new CorreiosGump(from, 8));
                    return;
                }

                int totalMoney = Cost_SendLetter * subs;

                if (totalMoney > 0 && !WithdrawWithKingdomRevenue(from, totalMoney))
                {
                    from.SendMessage("Você não tem moedas suficientes no banco.");
                    from.SendGump(new CorreiosGump(from, 8));
                    return;
                }

                // Mark pending changes + suspend + send notices
                pub.Suspended = true;

                int? proposedPrice = null;
                PubPeriod? proposedPeriod = null;

                if (changePrice)
                {
                    pub.HasPendingPrice = true;
                    pub.PendingPrice = _s.NewPrice;
                    proposedPrice = _s.NewPrice;
                }

                if (changePeriod)
                {
                    pub.HasPendingPeriod = true;
                    pub.PendingPeriod = _s.NewPeriod;
                    proposedPeriod = _s.NewPeriod;
                }

                SendChangeNotices(pub,
                    proposedPrice: proposedPrice,
                    proposedPeriod: proposedPeriod,
                    oldName: oldNameSnapshot,
                    newName: newNameSnapshot,
                    oldDesc: oldDescSnapshot,
                    newDesc: newDescSnapshot
                );

                from.SendMessage("Avisos enviados. Assinaturas suspensas até confirmação.");
            }
            else
            {
                from.SendMessage("Publicação atualizada.");
            }

            _s.NewPeriodSelected = false;
            from.SendGump(new CorreiosGump(from, 8));
        }

        private void PrepareCancelPublicationConfirm(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            _s.Pending = PendingAction.CancelPublicationConfirm;
            _s.PendingHtml = "<BASEFONT COLOR=#FFFFFF>Você está prestes a <B>cancelar sua publicação</B>.\n<BR><BR>Ela sairá da lista, e você deixará de receber pelas assinaturas.\n<BR><BR>Confirmar?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void PrepareChangePriceConfirm(Mobile from, RelayInfo info)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            int newPrice;
            if (!int.TryParse(info.GetTextEntry(30)?.Text, out newPrice) || newPrice <= 0)
            {
                from.SendMessage("Digite um valor válido.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            int cost = Cost_SendLetter * pub.Subscribers.Count;
            _s.NewPrice = newPrice;
            _s.Pending = PendingAction.ChangePriceConfirm;
            _s.PendingHtml = $"<BASEFONT COLOR=#FFFFFF>Para mudar o valor, você pagará <B>{cost} moedas</B> (10 por assinante) para avisar a todos.\n<BR><BR>Até eles concordarem, as assinaturas ficam suspensas.\n<BR><BR>Novo valor: {newPrice} moedas.\n<BR><BR>Confirmar?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void PrepareChangeNameConfirm(Mobile from, RelayInfo info)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            string newName = info.GetTextEntry(31)?.Text;
            if (String.IsNullOrWhiteSpace(newName))
            {
                from.SendMessage("Digite um nome.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            _s.NewName = newName.Trim();
            _s.Pending = PendingAction.ChangeNameConfirm;
            _s.PendingHtml = $"<BASEFONT COLOR=#FFFFFF>Mudar o nome custa <B>50 moedas</B>.\n<BR><BR>Novo nome: {_s.NewName}\n<BR><BR>Confirmar?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void PrepareChangePeriodConfirm(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            int cost = Cost_SendLetter * pub.Subscribers.Count;
            _s.Pending = PendingAction.ChangePeriodConfirm;
            _s.PendingHtml = $"<BASEFONT COLOR=#FFFFFF>Para mudar a periodicidade, você pagará <B>{cost} moedas</B> (10 por assinante) para avisar a todos.\n<BR><BR>Até eles concordarem, as assinaturas ficam suspensas.\n<BR><BR>Nova periodicidade: {PeriodNamePT(_s.NewPeriod)}.\n<BR><BR>Confirmar?</BASEFONT>";
            from.SendGump(new CorreiosGump(from, 9));
        }

        private void ApplyChangeDescription(Mobile from, RelayInfo info)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            string desc = info.GetTextEntry(32)?.Text;
            pub.Description = desc;
            from.SendMessage("Descrição atualizada.");
            from.SendGump(new CorreiosGump(from, 8));
        }

        private void HandleAvisoVoltar(Mobile from)
        {
            switch (_s.Pending)
            {
                case PendingAction.MailDeleteConfirm:
                    // Return to mail view, keep letter
                    from.SendGump(new CorreiosGump(from, 2));
                    break;

                case PendingAction.MailSendConfirm:
                    from.SendGump(new CorreiosGump(from, 3));
                    break;

                case PendingAction.SubscribeConfirm:
                    from.SendGump(new CorreiosGump(from, 4));
                    break;

                case PendingAction.UnsubscribeConfirm:
                    from.SendGump(new CorreiosGump(from, 5));
                    break;

                case PendingAction.CreatePublicationConfirm:
                    // Return item to player
                    if (_s.PubContentHeld != null && from.Backpack != null && _s.PubContentHeld.Parent == CorreioStorage.Instance.ItemVault)
                    {
                        from.Backpack.DropItem(_s.PubContentHeld);
                    }
                    _s.PubContentHeld = null;
                    _s.PubCostPerSubscriber = 0;
                    from.SendGump(new CorreiosGump(from, 7));
                    break;

                case PendingAction.CancelPublicationConfirm:
                case PendingAction.ChangePriceConfirm:
                case PendingAction.ChangeNameConfirm:
                case PendingAction.ChangePeriodConfirm:
                case PendingAction.UpdatePublicationConfirm:
                    from.SendGump(new CorreiosGump(from, 8));
                    break;

                case PendingAction.AcceptNewTerms:
                    // Decline -> cancel
                    ExecuteDeclineNewTerms(from);
                    break;

                default:
                    from.SendGump(new CorreiosGump(from, 1));
                    break;
            }

            _s.Pending = PendingAction.None;
            _s.PendingHtml = null;
        }

        private void HandleAvisoProximo(Mobile from)
        {
            switch (_s.Pending)
            {
                case PendingAction.MailDeleteConfirm:
                    ExecuteDeleteCurrentMail(from);
                    break;

                case PendingAction.SubscribeConfirm:
                    ExecuteSubscribe(from);
                    break;

                case PendingAction.UnsubscribeConfirm:
                    ExecuteUnsubscribe(from);
                    break;

                case PendingAction.CreatePublicationConfirm:
                    ExecuteCreatePublication(from);
                    break;

                case PendingAction.CancelPublicationConfirm:
                    ExecuteCancelPublication(from);
                    break;

                case PendingAction.ChangePriceConfirm:
                    ExecuteChangePrice(from);
                    break;

                case PendingAction.ChangeNameConfirm:
                    ExecuteChangeName(from);
                    break;

                case PendingAction.ChangePeriodConfirm:
                    ExecuteChangePeriod(from);
                    break;

                case PendingAction.UpdatePublicationConfirm:
                    ExecuteUpdatePublication(from);
                    break;

                case PendingAction.SendNewEditionConfirm:
                    ExecuteSendNewEdition(from);
                    break;

                case PendingAction.AcceptNewTerms:
                    ExecuteAcceptNewTerms(from);
                    break;

                default:
                    from.SendGump(new CorreiosGump(from, 1));
                    break;
            }

            _s.Pending = PendingAction.None;
            _s.PendingHtml = null;
        }

        private void ExecuteDeleteCurrentMail(Mobile from)
        {
            var mb = CorreioStorage.Instance.GetMailbox(from);
            if (mb == null || mb.Messages.Count == 0)
            {
                from.SendGump(new CorreiosGump(from, 2));
                return;
            }

            _s.MailIndex = Math.Max(0, Math.Min(_s.MailIndex, mb.Messages.Count - 1));
            var msg = mb.Messages[_s.MailIndex];

            if (msg.Attachments != null && msg.Attachments.Count > 0)
            {
                foreach (Item item in msg.Attachments)
                {
                    if (item != null && !item.Deleted)
                        item.Delete();
                }
            }
            else if (msg.Attachment != null && !msg.Attachment.Deleted)
                msg.Attachment.Delete();

            mb.Messages.RemoveAt(_s.MailIndex);
            if (_s.MailIndex >= mb.Messages.Count)
                _s.MailIndex = Math.Max(0, mb.Messages.Count - 1);

            from.SendGump(new CorreiosGump(from, 2));
        }


        private void ExecuteSendMail(Mobile from)
        {
            Mobile dest;
            if (!CorreioStorage.Instance.TryFindMobileByName(_s.ToName, out dest))
            {
                from.SendMessage("Destinatário não encontrado.");
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }

            PlayerMobile senderPm = from as PlayerMobile;
            string diplomacyReason;

            if (senderPm != null && !ReinoDiplomacySystem.CanExchangeMail(senderPm, dest, GetContextNpcCityId(), out diplomacyReason))
            {
                from.SendMessage(diplomacyReason);
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }


            if (_s.MailAttachments == null || _s.MailAttachments.Count == 0)
            {
                from.SendMessage("Você precisa anexar ao menos um item.");
                from.SendGump(new CorreiosGump(from, 3));
            return;
            }   

            List<Item> attachments = new List<Item>();

            for (int i = 0; i < _s.MailAttachments.Count; i++)
            {
                Item item = _s.MailAttachments[i];
                string reason;
                if (!IsValidMailAttachment(item, from, out reason))
                {
                    from.SendMessage(reason);
                    from.SendGump(new CorreiosGump(from, 3));
                    return;
                }

                CorreioPacote pacote = item as CorreioPacote;
                if (pacote != null)
                {
                    int currentWeight = GetAttachmentStoneCost(pacote);

                    if (!pacote.SealedForMail)
                    {
                        from.SendMessage("Um dos pacotes não está mais selado.");
                        from.SendGump(new CorreiosGump(from, 3));
                        return;
                    }

                    if (pacote.SealedWeight <= 0 || currentWeight != pacote.SealedWeight)
                    {
                        from.SendMessage("Um dos pacotes foi alterado depois de ser selado.");
                        from.SendGump(new CorreiosGump(from, 3));
                        return;
                    }
                }

                attachments.Add(item);
            }

            int total = CalculateMailSendCost(attachments);
            if (total > 0 && !WithdrawWithKingdomRevenue(from, total))
            {
                from.SendMessage("Você não tem dinheiro suficiente no banco.");
                from.SendGump(new CorreiosGump(from, 3));
                return;
            }

            for (int i = 0; i < attachments.Count; i++)
            {
                try
                {
                    CorreioStorage.Instance.ItemVault.DropItem(attachments[i]);
                }
                catch
                {
                    Banker.DepositUpTo(from, total);
                    from.SendMessage("Não foi possível mover um dos anexos.");
                    from.SendGump(new CorreiosGump(from, 3));
                    return;
                }
            }

            string title = attachments.Count == 1
                ? (attachments[0].Name ?? attachments[0].GetType().Name)
                : string.Format("Correio de {0}", from.Name);

            CorreioStorage.Instance.CreateMail(from, dest, title, null, attachments);

            from.SendMessage("Correio enviado.");
            _s.ClearMailDraft(from);
            from.SendGump(new CorreiosGump(from, 1));

        }

        private void ExecuteSubscribe(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublication(_s.SelectedPublicationId);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 4));
                return;
            }

            // IMPORTANTE:
            // CorreioStorage.Subscribe() já faz tudo: adiciona o assinante, cobra e envia UMA cópia da última edição.
            // Antes este método cobrava de novo e criava uma segunda carta, causando carta duplicada/vazia.
            if (!CorreioStorage.Instance.Subscribe(from, pub.PublicationId))
            {
                from.SendMessage("Não foi possível assinar (talvez você já assinou, é o dono, ou não tem ouro suficiente no banco)." );
                from.SendGump(new CorreiosGump(from, 4));
                return;
            }

            from.SendMessage("Assinatura realizada.");
            from.SendGump(new CorreiosGump(from, 4));
        }

        private void ExecuteUnsubscribe(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublication(_s.SelectedSubscriptionPubId);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 5));
                return;
            }

            CorreioStorage.Instance.Unsubscribe(from, pub.PublicationId);
            from.SendMessage("Assinatura cancelada.");
            from.SendGump(new CorreiosGump(from, 5));
        }

        private void ExecuteCreatePublication(Mobile from)
        {
            int totalCreateCost = Cost_CreatePublication + _s.PubCostPerSubscriber;

            if (!WithdrawWithKingdomRevenue(from, totalCreateCost))
            {
                from.SendMessage("Você precisa de " + totalCreateCost + " moedas no banco para criar uma publicação.");
                from.SendGump(new CorreiosGump(from, 7));
                return;
            }

            // Keep master content in vault
            Item master = _s.PubContentHeld;
            if (master == null || master.Deleted)
            {
                Banker.DepositUpTo(from, totalCreateCost);
                from.SendMessage("Item anexado inválido.");
                from.SendGump(new CorreiosGump(from, 7));
                return;
            }

            // Create publication
            var pub = CorreioStorage.Instance.CreatePublication(from, _s.PubName, _s.PubPrice, _s.PubPeriod, _s.PubDesc, master, _s.PubCostPerSubscriber);
            if (pub == null)
            {
                // refund and return item
                Banker.DepositUpTo(from, Cost_CreatePublication);
                from.Backpack?.DropItem(master);
                _s.PubContentHeld = null;
                from.SendMessage("Você já tem uma publicação.");
                from.SendGump(new CorreiosGump(from, 1));
                return;
            }

            // Clear draft state (but keep publication)
            _s.PubContentHeld = null;
            _s.PubCostPerSubscriber = 0;
            _s.PubName = null;
            _s.PubDesc = null;
            _s.PubPrice = 0;

            from.SendMessage("Publicação criada.");
            from.SendGump(new CorreiosGump(from, 8));
        }

        private void ExecuteCancelPublication(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            CorreioStorage.Instance.CancelPublication(pub.PublicationId);
            from.SendMessage("Publicação cancelada.");
            from.SendGump(new CorreiosGump(from, 1));
        }

        private void ExecuteChangeName(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            if (!WithdrawWithKingdomRevenue(from, 50))
            {
                from.SendMessage("Você precisa de 50 moedas no banco.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            pub.Name = _s.NewName;
            from.SendMessage("Nome alterado.");
            from.SendGump(new CorreiosGump(from, 8));
        }

        private void ExecuteChangePrice(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            int cost = Cost_SendLetter * pub.Subscribers.Count;

            int subs = pub.Subscribers.Count;
           
            if (cost > 0 && !WithdrawWithKingdomRevenue(from, cost))
            {
                from.SendMessage("Você não tem moedas suficientes no banco.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            pub.Suspended = true;
            pub.HasPendingPrice = true;
            pub.PendingPrice = _s.NewPrice;

            // Para mudança apenas de preço, nome/descrição permanecem os mesmos.
            var nameSnapshot = pub.Name;
            var descSnapshot = pub.Description;

            SendChangeNotices(
                pub,
                proposedPrice: _s.NewPrice,
                proposedPeriod: null,
                oldName: nameSnapshot,
                newName: nameSnapshot,
                oldDesc: descSnapshot,
                newDesc: descSnapshot);

            from.SendMessage("Avisos enviados. Assinaturas suspensas até confirmação.");
            from.SendGump(new CorreiosGump(from, 8));
        }

        private void ExecuteChangePeriod(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            int cost = Cost_SendLetter * pub.Subscribers.Count;

            int subs = pub.Subscribers.Count;
          
            if (cost > 0 && !WithdrawWithKingdomRevenue(from, cost))
            {
                from.SendMessage("Você não tem moedas suficientes no banco.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            pub.Suspended = true;
            pub.HasPendingPeriod = true;
            pub.PendingPeriod = _s.NewPeriod;

            // Para mudança apenas de período, nome/descrição permanecem os mesmos.
            var nameSnapshot2 = pub.Name;
            var descSnapshot2 = pub.Description;

            SendChangeNotices(
                pub,
                proposedPrice: null,
                proposedPeriod: _s.NewPeriod,
                oldName: nameSnapshot2,
                newName: nameSnapshot2,
                oldDesc: descSnapshot2,
                newDesc: descSnapshot2);

            from.SendMessage("Avisos enviados. Assinaturas suspensas até confirmação.");
            from.SendGump(new CorreiosGump(from, 8));
        }

        private void SendChangeNotices(Publication pub, int? proposedPrice, PubPeriod? proposedPeriod, string oldName, string newName, string oldDesc, string newDesc)
        {
            foreach (var s in pub.Subscribers)
            {
                Mobile sub;
                if (!World.Mobiles.TryGetValue(s, out sub) || sub == null || sub.Deleted)
                    continue;

                var toBox = CorreioStorage.Instance.GetMailbox(sub);
                if (toBox == null)
                {
                    CorreioStorage.Instance.GetOrCreateMailboxId(sub);
                    toBox = CorreioStorage.Instance.GetMailbox(sub);
                }

                var msg = new MailMessage();
                msg.Id = 0; // will be assigned below
                var ownerMob = pubOwnerMobile(pub);
                msg.FromMailboxId = ownerMob != null ? CorreioStorage.Instance.GetOrCreateMailboxId(ownerMob) : 0;
                msg.FromName = pubOwnerMobile(pub)?.Name ?? "Publicação";
                msg.Body = "Mudança de termos";
                msg.Created = DateTime.UtcNow;
                msg.IsPublicationChangeNotice = true;
                msg.PublicationId = pub.PublicationId;
                msg.ProposedPrice = proposedPrice ?? pub.PricePerIssue;
                msg.ProposedPeriod = proposedPeriod;
                // Guarda no aviso o "antes e depois" (nome e descrição)
                msg.OldName = oldName;
                msg.NewName = newName;
                msg.OldDescription = oldDesc;
                msg.NewDescription = newDesc;

                // assign unique id
                // we reuse CreateMail to get ids & store, but we need flags; easiest: create then edit
                var created = CorreioStorage.Instance.CreateMail(pubOwnerMobile(pub) ?? sub, sub, "Aviso de assinatura", msg.Body, null);
                created.IsPublicationChangeNotice = true;
                created.PublicationId = pub.PublicationId;
                created.ProposedPrice = msg.ProposedPrice;
                created.ProposedPeriod = proposedPeriod;
                created.OldName = oldName;
                created.NewName = newName;
                created.OldDescription = oldDesc;
                created.NewDescription = newDesc;
            }
        }

        private Mobile pubOwnerMobile(Publication p)
        {
            return CorreioStorage.Instance.GetOwnerMobile(p);
        }


        private void ExecuteSendNewEdition(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublicationByOwner(from.Serial);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // Janela de 1h para preparar
            if (pub.ComposeStartUtc == DateTime.MinValue || (DateTime.UtcNow - pub.ComposeStartUtc) > ComposeWindow)
            {
                from.SendMessage("Seu tempo para preparar a edição expirou. Abra 'Enviar nova edição' novamente.");
                _s.ClearEditionDraft(from);
                pub.ComposeStartUtc = DateTime.MinValue;
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            Item master = _s.EditionContentHeld;
            if (master == null || master.Deleted)
            {
                from.SendMessage("Item anexado inválido.");
                from.SendGump(new CorreiosGump(from, 10));
                return;
            }

            int subs = pub.Subscribers != null ? pub.Subscribers.Count : 0;
            if (subs <= 0)
            {
                from.SendMessage("Sua publicação não tem assinantes.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            int perSub = CalculateEditionPerSubscriberCost(master);
            int total = perSub * subs;

            int price = pub.PricePerIssue; // valor que cada assinante paga por edição

            var pagantes = new List<Mobile>();

            foreach (var serial in pub.Subscribers)
            {
                Mobile sub;
                if (!World.Mobiles.TryGetValue(serial, out sub) || sub == null || sub.Deleted)
                    continue;

                // tenta cobrar do assinante
                if (price > 0 && !WithdrawWithKingdomRevenue(sub, price))
                {
                    // não pagou = não recebe
                    sub.SendMessage("Você não tinha ouro no banco para pagar esta edição. Você não recebeu a publicação.");
                    continue;
                }

                pagantes.Add(sub);
            }

            // ninguém pagou
            if (pagantes.Count == 0)
            {
                from.SendMessage("Nenhum assinante conseguiu pagar esta edição.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // agora o publicador paga só pelos que vão receber
            int totalEnvio = perSub * pagantes.Count;
            if (!WithdrawWithKingdomRevenue(from, totalEnvio))
            {
                // se o publicador não consegue pagar envio, devolve o dinheiro cobrado dos assinantes
                foreach (var sub in pagantes)
                {
                    if (price > 0)
                        Banker.DepositUpTo(sub, price);
                }

                from.SendMessage($"Você precisa de {totalEnvio} moedas no banco para pagar o envio desta edição.");
                from.SendGump(new CorreiosGump(from, 10));
                return;
            }

            int sent = 0;
            int failedSends = 0;

            foreach (var sub in pagantes)
            {
                if (sub == null || sub.Deleted)
                    continue;

                Item copy = TryDupeItem(master);
                if (copy == null)
                {
                    failedSends++;

                    if (price > 0)
                        Banker.DepositUpTo(sub, price);

                    continue;
                }

                string body =
                    $"Nova edição de: {pub.Name}\n" +
                    $"{(string.IsNullOrWhiteSpace(pub.Description) ? "" : ("\n" + pub.Description + "\n"))}\n" +
                    "Obrigado por assinar!";

                try
                {
                    CorreioStorage.Instance.CreateMail(from, sub, pub.Name, body, new List<Item> { copy });
                    if (price > 0)
                        Banker.DepositUpTo(from, price); // deposita no banco do publicador

                    sent++;
                }
                catch
                {
                    if (copy != null && !copy.Deleted)
                        copy.Delete();

                    // devolve a taxa da edição porque não recebeu
                    if (price > 0)
                        Banker.DepositUpTo(sub, price);

                    failedSends++;
                }
            }

            if (failedSends > 0 && perSub > 0)
                Banker.DepositUpTo(from, perSub * failedSends);

            if (sent <= 0)
            {
                from.SendMessage("Não foi possível enviar para nenhum assinante.");
                from.SendGump(new CorreiosGump(from, 8));
                return;
            }

            // Guarda esta edição como a nova "última edição" da publicação.
            // Antes o código deletava o master novo; aí futuros assinantes podiam receber a edição antiga ou vazia.
            Item oldMaster = pub.MasterContent;
            pub.MasterContent = master;

            if (oldMaster != null && oldMaster != master && !oldMaster.Deleted)
                oldMaster.Delete();

            _s.EditionContentHeld = null;
            _s.EditionCostPerSubscriber = 0;

            pub.LastEditionSentUtc = DateTime.UtcNow;
            pub.ComposeStartUtc = DateTime.MinValue;

            from.SendMessage($"Edição enviada para {sent} assinante(s).");
            from.SendGump(new CorreiosGump(from, 8));
        }

        private static Item TryDupeItem(Item item)
        {
            if (item == null || item.Deleted)
                return null;

            // Usa o clone próprio do correio, que sabe copiar HtmlDocumentBase com texto, selo, autor, idioma e estilos.
            // O método antigo usava Dupe()/Activator e criava documentos HTML visualmente em branco.
            CorreioStorage.Ensure();
            return CorreioStorage.Instance.ClonePublicationItem(item);
        }

        private void ExecuteAcceptNewTerms(Mobile from)
        {
            // Here SelectedSubscriptionPubId is pub id
            var pub = CorreioStorage.Instance.GetPublication(_s.SelectedSubscriptionPubId);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 2));
                return;
            }

            // If player is not subscriber, ignore
            if (!pub.Subscribers.Contains(from.Serial))
            {
                from.SendGump(new CorreiosGump(from, 2));
                return;
            }

            // Apply pending changes immediately for this subscriber (global when all accept)
            // We implement global acceptance: when all remaining subscribers have acknowledged latest change, we apply and unsuspend.

            // Mark acknowledgement by removing their notice mails
            RemoveChangeNotices(from, pub.PublicationId);

            // Track acknowledgements via NextCharge being DateTime.Min? We'll add a simple set in pub.NextCharge with negative? too complex.
            // We'll treat acceptance as: do nothing; once owner changes, subscribers either accept by keeping subscription or cancel.
            // After they accept, we check if there are any remaining change-notice mails for this pub among subscribers online. If none online, we unsuspend.

            // For practicality: if any subscriber declines, removed. We'll unsuspend right away.
            if (pub.HasPendingPrice)
            {
                pub.PricePerIssue = pub.PendingPrice;
                pub.HasPendingPrice = false;
            }
            if (pub.HasPendingPeriod)
            {
                pub.Period = pub.PendingPeriod;
                pub.HasPendingPeriod = false;
            }
            pub.Suspended = false;

            from.SendMessage("Você aceitou os novos termos.");
            from.SendGump(new CorreiosGump(from, 1));
        }

        private void ExecuteDeclineNewTerms(Mobile from)
        {
            var pub = CorreioStorage.Instance.GetPublication(_s.SelectedSubscriptionPubId);
            if (pub == null)
            {
                from.SendGump(new CorreiosGump(from, 2));
                return;
            }

            CorreioStorage.Instance.Unsubscribe(from, pub.PublicationId);
            RemoveChangeNotices(from, pub.PublicationId);

            from.SendMessage("Você cancelou a assinatura.");
            from.SendGump(new CorreiosGump(from, 1));
        }

        private void RemoveChangeNotices(Mobile from, int pubId)
        {
            var mb = CorreioStorage.Instance.GetMailbox(from);
            if (mb == null)
                return;

            for (int i = mb.Messages.Count - 1; i >= 0; i--)
            {
                var msg = mb.Messages[i];
                if (msg.IsPublicationChangeNotice && msg.PublicationId == pubId)
                {
                    mb.Messages.RemoveAt(i);
                }
            }
        }

        private static string GetPubItemTitle(Item item)
        {
            if (item == null)
                return "Nenhum anexo";

            // Seus itens HTML: usa DocumentTitle (se tiver), senão Name
            if (item is HtmlDocumentBase doc)
            {
                if (!string.IsNullOrWhiteSpace(doc.Name))
                    return doc.Name;

                if (!string.IsNullOrWhiteSpace(doc.Name))
                    return doc.Name;

                return "Documento";
            }

            // Livro de compilação (caso você queira um nome específico)
            if (item is HtmlCompilationBook comp)
            {
                if (!string.IsNullOrWhiteSpace(comp.Name))
                    return comp.Name;

                return "Livro de Páginas";
            }

            // Qualquer item: tenta Name
            if (!string.IsNullOrWhiteSpace(item.Name))
                return item.Name;

            return "Anexo";
        }
    }


    public class MailAttachTarget : Target
    {
        private readonly CorreioSession _s;
        private readonly Mobile _from;

        public MailAttachTarget(CorreioSession s, Mobile from) : base(12, false, TargetFlags.None)
        {
            _s = s;
            _from = from;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (_s == null || _from == null)
                return;

            Item item = targeted as Item;
            if (item == null)
            {
                from.SendMessage("Isso não é um item.");
                return;
            }

            if (!item.IsChildOf(from.Backpack))
            {
                from.SendMessage("O item precisa estar na sua mochila.");
                return;
            }

            if (_s.MailAttachments == null)
                _s.MailAttachments = new List<Item>();

            if (_s.MailAttachments.Count >= 5)
            {
                from.SendMessage("Você só pode anexar até 5 itens por envio.");
                return;
            }

            if (_s.MailAttachments.Contains(item))
            {
                from.SendMessage("Esse item já foi anexado.");
                return;
            }

            string reason;
            if (!CorreiosGump_IsValidMailAttachment(item, from, out reason))
            {
                from.SendMessage(reason);
                return;
            }

            _s.MailAttachments.Add(item);
            from.SendMessage("Anexo selecionado.");
            from.SendGump(new CorreiosGump(from, 3));
        }

        private static bool CorreiosGump_IsValidMailAttachment(Item item, Mobile from, out string reason)
        {
            reason = null;

            if (item == null || item.Deleted)
            {
                reason = "Item inválido.";
                return false;
            }

            if (!item.IsChildOf(from.Backpack))
            {
                reason = "O item precisa estar na sua mochila.";
                return false;
            }

            if (item is Gold || item is BankCheck)
            {
                reason = "Não é possível enviar moedas.";
                return false;
            }

            if (item is Carta)
                return true;

            if (item is CorreioPacote pacote)
            {
                if (!pacote.SealedForMail)
                {
                    reason = "O pacote precisa estar selado para envio.";
                    return false;
                }

                if (pacote.Items == null || pacote.Items.Count == 0)
                {
                    reason = "O pacote está vazio.";
                    return false;
                }

                if (CorreiosGump_ContainsCoins(pacote))
                {
                    reason = "Não é possível enviar moedas dentro de pacotes.";
                    return false;
                }

                int stones = CorreiosGump_GetAttachmentStoneCost(item);

                if (pacote.SealedWeight <= 0)
                {
                    reason = "Esse pacote selado está inválido.";
                    return false;
                }

                if (stones != pacote.SealedWeight)
                {
                    reason = "O peso do pacote mudou depois de ser anexado.";
                    return false;
                }

                if (stones > 26)
                {
                    reason = "Cada anexo pode ter no máximo 26 stones contando o pacote.";
                    return false;
                }

                return true;
            }

            reason = "Você só pode anexar cartas ou pacotes com selo.";
            return false;
        }

        private static int CorreiosGump_GetAttachmentStoneCost(Item item)
        {
            double weight = item.TotalWeight;
            if (weight <= 0.0)
                weight = item.Weight;

            return (int)Math.Ceiling(weight);
        }

        private static bool CorreiosGump_ContainsCoins(Item item)
        {
            if (item is Gold || item is BankCheck)
                return true;

            Container c = item as Container;
            if (c != null)
            {
                foreach (Item child in c.Items)
                    if (CorreiosGump_ContainsCoins(child))
                        return true;
            }

            return false;
        }
    }

    public class PublicationAttachTarget : Target
    {
        private readonly CorreioSession _s;
        private readonly Mobile _from;

        public PublicationAttachTarget(CorreioSession s, Mobile from) : base(12, false, TargetFlags.None)
        {
            _s = s;
            _from = from;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {

            if (_s == null || _from == null)
                return;

            Item item = targeted as Item;
            if (item == null)
            {
                from.SendMessage("Isso não é um item.");
                return;
            }

            if (!item.IsChildOf(from.Backpack))
            {
                from.SendMessage("O item precisa estar na sua mochila.");
                return;
            }

            if (!CorreiosGump_IsValidPublicationItem(item))
            {
                from.SendMessage("Você só pode publicar livros/pergaminhos HTML selados.");
                return;
            }

            // Move to vault
            CorreioStorage.Ensure();
            try
            {
                CorreioStorage.Instance.ItemVault.DropItem(item);
            }
            catch
            {
                from.SendMessage("Não foi possível anexar.");
                return;
            }

            _s.PubContentHeld = item;

            // Cost: 20 book, 10 scroll
            // 1) NÃO permitir loosepages
            if (item is HtmlLoosePage)
            {
                from.SendMessage(0x22, "Páginas soltas não podem ser publicadas pelo correio.");
                return;
            }

            // 2) HtmlDocumentBase (inclui livros, scrolls etc)
            if (item is HtmlCompilationBook && !CorreiosGump.IsClosedCompilationBook(item))
            {
                from.SendMessage("O livro de compilação precisa estar fechado.");
                return;
            }

            _s.PubCostPerSubscriber = CorreiosGump.CalculateEditionPerSubscriberCost(item);

            if (_s.PubCostPerSubscriber <= 0)
            {
                from.SendMessage(0x22, "Este item não pode ser publicado pelo correio.");
                return;
            }

            from.SendMessage("Publicação anexada.");
            from.SendGump(new CorreiosGump(from, 7));
        }

        private bool CorreiosGump_IsValidPublicationItem(Item item)
        {
            if (item == null || item.Deleted)
                return false;

            if (item is HtmlLoosePage)
                return false;

            if (item is HtmlDocumentBase doc)
                return doc.IsSealed;

            if (item is HtmlCompilationBook)
                return CorreiosGump.IsClosedCompilationBook(item);

            return false;
        }


        private static readonly TimeSpan ComposeWindow = TimeSpan.FromHours(1);

        private static TimeSpan GetCooldown(PubPeriod p)
        {
            switch (p)
            {
                case PubPeriod.Weekly: return TimeSpan.FromHours(167);
                case PubPeriod.Monthly: return TimeSpan.FromHours(719);
                default: return TimeSpan.FromHours(23); // Daily
            }
        }
    }
    public class PublicationEditionAttachTarget : Target
    {
        private readonly CorreioSession _s;
        private readonly Mobile _from;

        public PublicationEditionAttachTarget(CorreioSession s, Mobile from) : base(12, false, TargetFlags.None)
        {
            _s = s;
            _from = from;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (_s == null || _from == null)
                return;

            Item item = targeted as Item;
            if (item == null)
            {
                from.SendMessage("Isso não é um item.");
                return;
            }

            if (!item.IsChildOf(from.Backpack))
            {
                from.SendMessage("O item precisa estar na sua mochila.");
                return;
            }

            if (!CorreiosGump_IsValidPublicationItem(item))
            {
                from.SendMessage("Você só pode publicar livros/pergaminhos HTML selados.");
                return;
            }

            // Move to vault (same flow used by criação de publicação)
            try
            {
                if (CorreioStorage.Instance.ItemVault != null)
                {
                    item.Movable = false;
                    CorreioStorage.Instance.ItemVault.DropItem(item);
                }
            }
            catch
            {
                from.SendMessage("Não foi possível anexar.");
                return;
            }

            _s.EditionContentHeld = item;

            // Custo base da edição (tipo do conteúdo)

            // 1) NÃO permitir loosepages
            if (item is HtmlLoosePage)
            {
                from.SendMessage(0x22, "Páginas soltas não podem ser publicadas pelo correio.");
                return;
            }

            // 2) HtmlDocumentBase (inclui livros, scrolls etc)
            if (item is HtmlCompilationBook && !CorreiosGump.IsClosedCompilationBook(item))
            {
                from.SendMessage("O livro de compilação precisa estar fechado.");
                return;
            }

            _s.EditionCostPerSubscriber = CorreiosGump.CalculateEditionPerSubscriberCost(item);

            if (_s.EditionCostPerSubscriber <= 0)
            {
                from.SendMessage(0x22, "Este item não pode ser publicado pelo correio.");
                return;
            }

            from.SendMessage("Edição anexada.");
            from.SendGump(new CorreiosGump(from, 10));
        }

        private bool CorreiosGump_IsValidPublicationItem(Item item)
        {
            if (item == null || item.Deleted)
                return false;

            if (item is HtmlLoosePage)
                return false;

            if (item is HtmlDocumentBase doc)
                return doc.IsSealed;

            if (item is HtmlCompilationBook)
                return CorreiosGump.IsClosedCompilationBook(item);

            return false;
        }
    }

}
