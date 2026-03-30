using Server;
using Server.Accounting;
using Server.Custom.Systems.Correios.Correios;
using Server.Items;
using Server.Mobiles;
using Server.Spells.Bushido;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Custom.Correios
{
    public enum PubPeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    public class MailMessage
    {
        public int Id;
        public int FromMailboxId;
        public string FromName;
        public string Title;
        public string Body;
        public DateTime Created;
        public DateTime WorldCreated;
        public Item Attachment;
        public List<Item> Attachments = new List<Item>();

        // For special system notices
        public bool IsPublicationChangeNotice;
        public int PublicationId;
        public int ProposedPrice;
        public PubPeriod? ProposedPeriod;

        public string OldName;
        public string NewName;

        public string OldDescription;
        public string NewDescription;

        public MailMessage()
        {
        }

        public MailMessage(int id, int fromMailboxId, string fromName, string title, string body, List<Item> attachments)
        {
            Id = id;
            FromMailboxId = fromMailboxId;
            FromName = fromName;
            Title = title;
            Body = body;
            Created = DateTime.UtcNow;
            WorldCreated = Server.Custom.Systems.WorldTime.OSUWorldTime.WorldNow;

            if (attachments != null)
                Attachments = new List<Item>(attachments);

            Attachment = (Attachments != null && Attachments.Count > 0) ? Attachments[0] : null;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(2); // version

            // Campos do aviso (podem ser null)
            writer.Write(OldName);
            writer.Write(NewName);
            writer.Write(OldDescription);
            writer.Write(NewDescription);

            // Dados básicos da carta
            writer.Write(Id);
            writer.Write(FromMailboxId);
            writer.Write(FromName);
            writer.Write(Title);
            writer.Write(Body);
            writer.Write(Created);
            writer.Write(WorldCreated);

            writer.Write(Attachments != null ? Attachments.Count : 0);
            if (Attachments != null)
            {
                for (int i = 0; i < Attachments.Count; i++)
                    writer.Write(Attachments[i]);
            }

            // compat antiga
            writer.Write(Attachment);

            // Aviso especial
            writer.Write(IsPublicationChangeNotice);
            writer.Write(PublicationId);
            writer.Write(ProposedPrice);

            // ProposedPeriod (nullable)
            writer.Write(ProposedPeriod != null);
            writer.Write((int)(ProposedPeriod ?? PubPeriod.Daily));
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            if (version >= 1)
            {
                OldName = reader.ReadString();
                NewName = reader.ReadString();
                OldDescription = reader.ReadString();
                NewDescription = reader.ReadString();
            }
            else
            {
                OldName = null;
                NewName = null;
                OldDescription = null;
                NewDescription = null;
            }

            Id = reader.ReadInt();
            FromMailboxId = reader.ReadInt();
            FromName = reader.ReadString();

            if (version >= 2)
                Title = reader.ReadString();
            else
                Title = null;

            Body = reader.ReadString();
            Created = reader.ReadDateTime();

            if (version >= 2)
                WorldCreated = reader.ReadDateTime();
            else
                WorldCreated = Created;

            if (version >= 2)
            {
                int ac = reader.ReadInt();
                Attachments = new List<Item>(ac);
                for (int i = 0; i < ac; i++)
                    Attachments.Add(reader.ReadItem());

                Attachment = reader.ReadItem();
                if ((Attachments == null || Attachments.Count == 0) && Attachment != null)
                    Attachments = new List<Item> { Attachment };
            }
            else
            {
                Attachment = reader.ReadItem();
                Attachments = new List<Item>();
                if (Attachment != null)
                    Attachments.Add(Attachment);
            }

            IsPublicationChangeNotice = reader.ReadBool();
            PublicationId = reader.ReadInt();
            ProposedPrice = reader.ReadInt();

            bool has = reader.ReadBool();
            int p = reader.ReadInt();
            ProposedPeriod = has ? (PubPeriod)p : (PubPeriod?)null;
        }
    }

    public class Mailbox
    {
        public int MailboxId;
        public Serial Owner;
        public List<MailMessage> Messages = new List<MailMessage>();

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);
            writer.Write(MailboxId);
            writer.Write(Owner);
            writer.Write(Messages.Count);
            for (int i = 0; i < Messages.Count; i++)
            {
                Messages[i].Serialize(writer);
            }
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
            MailboxId = reader.ReadInt();
            Owner = reader.ReadInt();
            int count = reader.ReadInt();
            Messages = new List<MailMessage>(count);
            for (int i = 0; i < count; i++)
            {
                var msg = new MailMessage();
                msg.Deserialize(reader);
                Messages.Add(msg);
            }
        }
    }

    public class Publication
    {
        public int PublicationId;
        public Serial Owner;
        public string Name;
        public string Description;
        public int PricePerIssue;
        public PubPeriod Period;

        public int CostPerSubscriber; // 20 book, 10 scroll

        // Master copy stored in storage container
        public Item MasterContent;
        public DateTime Created;

        public HashSet<Serial> Subscribers = new HashSet<Serial>();
        public Dictionary<Serial, DateTime> NextCharge = new Dictionary<Serial, DateTime>();

        // Pending changes
        public bool Suspended;
        public int PendingPrice;
        public bool HasPendingPrice;
        public PubPeriod PendingPeriod;
        public bool HasPendingPeriod;

        public DateTime LastEditionSentUtc = DateTime.MinValue;     // quando publicou a última edição
        public DateTime ComposeStartUtc = DateTime.MinValue;        // quando abriu “Enviar nova edição”

        public void Serialize(GenericWriter writer)
        {
            writer.Write(2); // version
            writer.Write(LastEditionSentUtc);
            writer.Write(ComposeStartUtc);
            writer.Write(PublicationId);
            writer.Write(Owner);
            writer.Write(Name);
            writer.Write(Description);
            writer.Write(PricePerIssue);
            writer.Write((int)Period);
            writer.Write(CostPerSubscriber);
            writer.Write(MasterContent);
            writer.Write(Created);

            writer.Write(Subscribers.Count);
            foreach (var s in Subscribers)
                writer.Write(s);

            writer.Write(NextCharge.Count);
            foreach (var kv in NextCharge)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }

            writer.Write(Suspended);
            writer.Write(HasPendingPrice);
            writer.Write(PendingPrice);
            writer.Write(HasPendingPeriod);
            writer.Write((int)PendingPeriod);
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            if (version >= 2)
            {
                LastEditionSentUtc = reader.ReadDateTime();
                ComposeStartUtc = reader.ReadDateTime();
            }

            PublicationId = reader.ReadInt();
            Owner = reader.ReadInt();
            Name = reader.ReadString();
            Description = reader.ReadString();
            PricePerIssue = reader.ReadInt();
            Period = (PubPeriod)reader.ReadInt();
            CostPerSubscriber = reader.ReadInt();
            MasterContent = reader.ReadItem();
            Created = reader.ReadDateTime();

            int sc = reader.ReadInt();
            Subscribers = new HashSet<Serial>();
            for (int i = 0; i < sc; i++)
                Subscribers.Add(reader.ReadInt());

            int nc = reader.ReadInt();
            NextCharge = new Dictionary<Serial, DateTime>(nc);
            for (int i = 0; i < nc; i++)
            {
                var key = reader.ReadInt();
                var val = reader.ReadDateTime();
                NextCharge[key] = val;
            }

            Suspended = reader.ReadBool();
            HasPendingPrice = reader.ReadBool();
            PendingPrice = reader.ReadInt();
            HasPendingPeriod = reader.ReadBool();
            PendingPeriod = (PubPeriod)reader.ReadInt();
        }


        public TimeSpan GetPeriodSpan()
        {
            switch (Period)
            {
                case PubPeriod.Weekly: return TimeSpan.FromDays(7);
                case PubPeriod.Monthly: return TimeSpan.FromDays(30);
                default: return TimeSpan.FromDays(1);
            }
        }

        public string PeriodNamePT()
        {
            switch (Period)
            {
                case PubPeriod.Weekly: return "Semanal";
                case PubPeriod.Monthly: return "Mensal";
                default: return "Diária";
            }
        }
    }

    /// <summary>
    /// World-persistent hidden container that stores everything for the mail/publications system.
    /// We inherit from Backpack (Container) so we can hold items persistently.
    /// </summary>
    public class CorreioStorage : Backpack
    {
        public static CorreioStorage Instance { get; private set; }

        private Dictionary<Serial, Mailbox> _mailboxes = new Dictionary<Serial, Mailbox>();
        private Dictionary<int, Serial> _mailboxIdToOwner = new Dictionary<int, Serial>();
        private int _nextMailboxId = 1000;

        private Dictionary<int, Publication> _publications = new Dictionary<int, Publication>();
        private int _nextPublicationId = 1;

        private int _nextMailMessageId = 1;

        // We use this storage container itself as the vault.
        private Container _itemVault;

        [Constructable]
        public CorreioStorage() : base()
        {
            Movable = false;
            Visible = false;
            Name = "CorreioStorage";
            Map = Map.Internal;
            Location = Point3D.Zero;

            _itemVault = this;

            if (Instance == null)
                Instance = this;
        }

        public CorreioStorage(Serial serial) : base(serial)
        {
        }

        public static void Ensure()
        {
            if (Instance != null && !Instance.Deleted)
                return;

            foreach (Item item in World.Items.Values)
            {
                if (item is CorreioStorage cs && !cs.Deleted)
                {
                    Instance = cs;
                    return;
                }
            }

            Instance = new CorreioStorage();
        }

        public Container ItemVault => _itemVault;

        public int GetOrCreateMailboxId(Mobile m)
        {
            if (m == null)
                return 0;

            Mailbox mb;
            if (!_mailboxes.TryGetValue(m.Serial, out mb))
            {
                mb = new Mailbox();
                mb.Owner = m.Serial;
                mb.MailboxId = _nextMailboxId++;
                _mailboxes[m.Serial] = mb;
                _mailboxIdToOwner[mb.MailboxId] = m.Serial;
            }

            return mb.MailboxId;
        }

        public Mailbox GetMailbox(Mobile m)
        {
            if (m == null)
                return null;

            Mailbox mb;
            _mailboxes.TryGetValue(m.Serial, out mb);
            return mb;
        }

        public int GetMailboxMessageCount(Mobile m)
        {
            var mb = GetMailbox(m);
            return mb?.Messages?.Count ?? 0;
        }

        public bool TryFindMobileByName(string name, out Mobile found)
        {
            found = null;
            if (String.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m == null || m.Deleted)
                    continue;

                if (String.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    found = m;
                    return true;
                }
            }

            return false;
        }

        public MailMessage CreateMail(Mobile from, Mobile to, string title, string body, List<Item> attachments)
        {
            if (from == null || to == null)
                return null;

            var toBox = GetMailbox(to);
            if (toBox == null)
            {
                GetOrCreateMailboxId(to);
                toBox = GetMailbox(to);
            }

            int fromId = GetOrCreateMailboxId(from);

            var msg = new MailMessage(_nextMailMessageId++, fromId, from.Name, title, body, attachments);
            toBox.Messages.Add(msg);
            return msg;
        }

        public IEnumerable<Publication> GetPublications()
        {
            return _publications.Values;
        }

        public Publication GetPublication(int id)
        {
            Publication p;
            _publications.TryGetValue(id, out p);
            return p;
        }

        public Publication GetPublicationByOwner(Serial owner)
        {
            foreach (var p in _publications.Values)
            {
                if (p.Owner == owner)
                    return p;
            }
            return null;
        }

        public Publication CreatePublication(Mobile owner, string name, int price, PubPeriod period, string desc, Item masterContent, int costPerSubscriber)
        {
            if (owner == null || masterContent == null)
                return null;

            var existing = GetPublicationByOwner(owner.Serial);
            if (existing != null)
                return null;

            var pub = new Publication();
            pub.PublicationId = _nextPublicationId++;
            pub.Owner = owner.Serial;
            pub.Name = name;
            pub.Description = desc;
            pub.PricePerIssue = price;
            pub.Period = period;
            pub.MasterContent = masterContent;
            pub.CostPerSubscriber = costPerSubscriber;
            pub.Created = DateTime.UtcNow;

            _publications[pub.PublicationId] = pub;
            return pub;
        }

        public bool CancelPublication(int pubId)
        {
            Publication p;
            if (!_publications.TryGetValue(pubId, out p))
                return false;

            // Remove master content
            if (p.MasterContent != null && !p.MasterContent.Deleted)
                p.MasterContent.Delete();

            _publications.Remove(pubId);
            return true;
        }

        public bool Subscribe(Mobile m, int pubId)
        {
            var p = GetPublication(pubId);
            if (p == null || m == null)
                return false;

            if (p.Owner == m.Serial)
                return false;

            if (p.Subscribers.Contains(m.Serial))
                return false;

            p.Subscribers.Add(m.Serial);

            // 1) cobra na hora de assinar
            int charge = p.PricePerIssue;

            Mobile owner = GetOwnerMobile(p);

            if (charge > 0)
            {
                if (!Banker.Withdraw(m, charge))
                {
                    // sem dinheiro -> desfaz assinatura
                    p.Subscribers.Remove(m.Serial);
                    m.SendMessage("Você não tem ouro suficiente no banco para assinar esta publicação.");
                    return false;
                }

                // deposita no banco do publicador
                if (owner != null && owner.AccessLevel == AccessLevel.Player)
                    Banker.DepositUpTo(owner, charge);
            }

            // 2) entrega a última edição na hora (cópia do MasterContent)
            SendPublicationToSubscriber(p, m);

            // 3) opcional: manter NextCharge, mas jogando pra frente (pra não disparar ProcessSubscriptions se ainda existir)
            p.NextCharge[m.Serial] = DateTime.UtcNow + p.GetPeriodSpan();

            return true;
        }

        public bool Unsubscribe(Mobile m, int pubId)
        {
            var p = GetPublication(pubId);
            if (p == null || m == null)
                return false;

            p.Subscribers.Remove(m.Serial);
            p.NextCharge.Remove(m.Serial);
            return true;
        }

        public List<Publication> GetSubscriptions(Mobile m)
        {
            var list = new List<Publication>();
            if (m == null)
                return list;

            foreach (var p in _publications.Values)
            {
                if (p.Subscribers.Contains(m.Serial))
                    list.Add(p);
            }

            return list;
        }

        public void SendPublicationToSubscriber(Publication p, Mobile subscriber)
        {
            if (p == null || subscriber == null)
                return;

            // Create copy of master
            Item copy = ClonePublicationItem(p.MasterContent);

            if (copy != null)
            {
                // Store copy as attachment in mailbox message
                CreateMail(GetOwnerMobile(p), subscriber,
                    p.Name,
                    p.Description,
                    new List<Item> { copy });
            }
            else
            {
                CreateMail(GetOwnerMobile(p), subscriber,
                    p.Name,
                    $"{p.Description}\n\n(Erro: não foi possível copiar o anexo.)",
                    null);
            }
        }

        public Item ClonePublicationItem(Item original)
        {
            // ServUO builds differ: Item.Dupe() may not exist.
            // We only need to clone items that are valid as publication content:
            // - Writable books (BaseBook with Writable=true)
            // - PergaminhoEditavel (our custom scroll)
            if (original == null || original.Deleted)
                return null;

            try
            {
                Item copy = null;

                if (original is PergaminhoEditavel pe)
                {
                    var c = new PergaminhoEditavel();
                    c.Texto = pe.Texto;
                    c.Name = pe.Name;
                    c.Hue = pe.Hue;
                    c.Weight = pe.Weight;
                    copy = c;
                }
                else if (original is BaseBook book)
                {
                    // Only clone writable books
                    if (!book.Writable)
                        return null;

                    var nb = Activator.CreateInstance(original.GetType()) as BaseBook;
                    if (nb == null)
                        return null;

                    nb.Hue = book.Hue;
                    nb.Name = book.Name;
                    nb.Weight = book.Weight;
                    nb.Writable = book.Writable;
                    nb.Title = book.Title;
                    nb.Author = book.Author;

                    // Deep-copy pages via reflection (works across most ServUO/RunUO variants)
                    try
                    {
                        var pages = book.Pages;
                        if (pages != null)
                        {
                            var newPages = new BookPageInfo[pages.Length];
                            for (int i = 0; i < pages.Length; i++)
                            {
                                var lines = pages[i]?.Lines;
                                if (lines != null)
                                {
                                    var newLines = new string[lines.Length];
                                    for (int j = 0; j < lines.Length; j++)
                                        newLines[j] = lines[j];
                                    newPages[i] = new BookPageInfo(newLines);
                                }
                                else
                                {
                                    newPages[i] = new BookPageInfo(new string[0]);
                                }
                            }

                            // m_Pages is the most common private field name
                            var f = typeof(BaseBook).GetField("m_Pages", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (f != null)
                                f.SetValue(nb, newPages);
                        }
                    }
                    catch
                    {
                        // If page copy fails, still deliver an empty copy rather than crash.
                    }

                    copy = nb;
                }

                if (copy != null)
                {
                    // Put in vault so it's persisted until the player claims it.
                    _itemVault.DropItem(copy);
                }

                return copy;
            }
            catch
            {
                return null;
            }
        }

        public Mobile GetOwnerMobile(Publication p)
        {
            if (p == null)
                return null;

            Mobile m;
            World.Mobiles.TryGetValue(p.Owner, out m);
            return m;
        }

        public int PublishNewEdition(Mobile publisher, Publication p, Item newMasterContent)
        {
            if (publisher == null || p == null || newMasterContent == null)
                return 0;

            if (p.Owner != publisher.Serial)
                return 0;

            // troca o master content pela nova edição
            if (p.MasterContent != null && !p.MasterContent.Deleted)
                p.MasterContent.Delete();

            p.MasterContent = newMasterContent;

            int delivered = 0;
            int charge = p.PricePerIssue;

            // lista de assinantes pra não dar problema se remover alguém
            var subs = new List<Serial>(p.Subscribers);

            int perSubCost = p.CostPerSubscriber;
            int totalCost = perSubCost * p.Subscribers.Count;

            if (totalCost > 0 && !Banker.Withdraw(publisher, totalCost))
            {
                publisher.SendMessage("Você não tem ouro suficiente no banco para pagar o envio desta edição.");
                return 0;
            }

            foreach (var s in subs)
            {
                Mobile subscriber;
                if (!World.Mobiles.TryGetValue(s, out subscriber) || subscriber == null || subscriber.Deleted)
                    continue;

                // 1) cobra do assinante
                if (charge > 0 && !Banker.Withdraw(subscriber, charge))
                {
                    subscriber.SendMessage("Você não tinha ouro no banco para pagar esta edição. Você não recebeu a publicação.");
                    continue;
                }

                // 2) paga o publicador
                if (charge > 0)
                    Banker.DepositUpTo(publisher, charge);

                // 3) entrega a edição
                SendPublicationToSubscriber(p, subscriber);
                delivered++;
            }

            return delivered;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version
            writer.Write(_nextMailboxId);
            writer.Write(_nextPublicationId);
            writer.Write(_nextMailMessageId);

            writer.Write(_itemVault);

            writer.Write(_mailboxes.Count);
            foreach (var kv in _mailboxes)
            {
                writer.Write(kv.Key);
                kv.Value.Serialize(writer);
            }

            writer.Write(_publications.Count);
            foreach (var kv in _publications)
            {
                writer.Write(kv.Key);
                kv.Value.Serialize(writer);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            _nextMailboxId = reader.ReadInt();
            _nextPublicationId = reader.ReadInt();
            _nextMailMessageId = reader.ReadInt();

            _itemVault = reader.ReadItem() as Container;

            int mbCount = reader.ReadInt();
            _mailboxes = new Dictionary<Serial, Mailbox>(mbCount);
            _mailboxIdToOwner = new Dictionary<int, Serial>();
            for (int i = 0; i < mbCount; i++)
            {
                Serial key = reader.ReadInt();
                var mb = new Mailbox();
                mb.Deserialize(reader);
                _mailboxes[key] = mb;
                _mailboxIdToOwner[mb.MailboxId] = mb.Owner;
            }

            int pubCount = reader.ReadInt();
            _publications = new Dictionary<int, Publication>(pubCount);
            for (int i = 0; i < pubCount; i++)
            {
                int key = reader.ReadInt();
                var pub = new Publication();
                pub.Deserialize(reader);
                _publications[key] = pub;
            }

            // Backward/defensive: if we don't have a vault item, use this container.
            if (_itemVault == null || _itemVault.Deleted)
                _itemVault = this;

            Movable = false;
            Visible = false;
            Map = Map.Internal;
            Location = Point3D.Zero;

            Instance = this;
        }
    }

    public class CorreioCore
    {
        private static Timer _timer;

        public static void Initialize()
        {
            CorreioStorage.Ensure();

            // Process subscriptions periodically (every 5 minutes)
            _timer = Timer.DelayCall(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5), Process);
        }

        private static void Process()
        {
            try
            {
                CorreioStorage.Ensure();
            }
            catch
            {
            }
        }
    }
}
