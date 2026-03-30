
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.Correios.Correios
{

    public class CorreioPapel : Item
    {
        [Constructable]
        public CorreioPapel() : base(0x138C)
        {
            Name = "papel";
            Weight = 0.1;
        }

        public CorreioPapel(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }


    public static class CorreioLetterHelper
    {
        public static bool IsValidLetterDocument(Item item)
        {
            if (!(item is HtmlDocumentBase doc))
                return false;

            if (!doc.IsSealed)
                return false;

            string n = item.GetType().Name;
            if (!n.StartsWith("HtmlScrollPapel", StringComparison.Ordinal))
                return false;

            if (n.StartsWith("HtmlScrollPapelG", StringComparison.Ordinal))
                return false;

            string suffix = n.Substring("HtmlScrollPapel".Length);
            int v;
            return int.TryParse(suffix, out v) && v >= 1 && v <= 11;
        }

        public static string GetDocumentTitle(HtmlDocumentBase doc)
        {
            if (doc == null)
                return "Carta";

            if (!string.IsNullOrWhiteSpace(doc.DocumentTitle))
                return doc.DocumentTitle;

            if (!string.IsNullOrWhiteSpace(doc.Name))
                return doc.Name;

            return "Carta";
        }
    }

    public class CorreioSelo : Item
    {
        [Constructable]
        public CorreioSelo() : base(0x1422)
        {
            Name = "selo de envio";
            Weight = 0.1;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from?.Backpack == null || !IsChildOf(from.Backpack))
            {
                from?.SendMessage("O selo precisa estar na sua mochila.");
                return;
            }

            from.SendMessage("Selecione uma carta ou pacote");

            from.Target = new SealTarget(this);
        }

        private class SealTarget : Target
        {
            private readonly CorreioSelo _stamp;

            public SealTarget(CorreioSelo stamp) : base(12, false, TargetFlags.None)
            {
                _stamp = stamp;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_stamp == null || _stamp.Deleted || from?.Backpack == null)
                    return;

                Item item = targeted as Item;
                if (item == null || !item.IsChildOf(from.Backpack))
                {
                    from.SendMessage("Você deve selecionar uma carta aberta ou um pacote na sua mochila.");
                    return;
                }

                CartaAberta open = item as CartaAberta;
                if (open != null)
                {
                    HtmlDocumentBase doc = open.GetLetterDocument();
                    if (doc == null)
                    {
                        from.SendMessage("Essa carta aberta está vazia.");
                        return;
                    }

                    Carta finalLetter = new Carta();
                    finalLetter.ItemID = Utility.RandomBool() ? 0x1391 : 0x1392;
                    finalLetter.Name = "carta";
                    finalLetter.DropItem(doc);

                    if (open.Parent is Container c)
                        c.DropItem(finalLetter);
                    else
                        from.Backpack.DropItem(finalLetter);

                    open.Delete();
                    _stamp.Delete();

                    from.SendMessage("A carta foi selada para envio.");
                    return;
                }

                CorreioPacote pacote = item as CorreioPacote;
                if (pacote != null)
                {
                    if (!pacote.TrySeal(from))
                        return;

                    _stamp.Delete();
                    return;
                }

                from.SendMessage("O selo só pode ser usado em cartas abertas ou pacotes.");
            }
        }

        public CorreioSelo(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class CorreioEnvelope : Item
    {
        [Constructable]
        public CorreioEnvelope() : base(0x1390)
        {
            Name = "envelope";
            Weight = 0.1;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from?.Backpack == null || !IsChildOf(from.Backpack))
            {
                from?.SendMessage("O envelope precisa estar na sua mochila.");
                return;
            }

            from.SendMessage("Selecione um pergaminho de papel selado.");
            from.Target = new EnvelopeTarget(this);
        }

        private class EnvelopeTarget : Target
        {
            private readonly CorreioEnvelope _envelope;

            public EnvelopeTarget(CorreioEnvelope envelope) : base(12, false, TargetFlags.None)
            {
                _envelope = envelope;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_envelope == null || _envelope.Deleted || from?.Backpack == null)
                    return;

                HtmlDocumentBase doc = targeted as HtmlDocumentBase;
                if (doc == null || !doc.IsChildOf(from.Backpack) || !CorreioLetterHelper.IsValidLetterDocument(doc))
                {
                    from.SendMessage("O envelope só aceita HtmlScrollPapel 1 a 11 selados.");
                    return;
                }

                CartaAberta open = new CartaAberta();
                open.ItemID = Utility.RandomBool() ? 0x138F : 0x1394;
                open.Name = "carta aberta";
                open.DropItem(doc);

                if (_envelope.Parent is Container c)
                    c.DropItem(open);
                else
                    from.Backpack.DropItem(open);

                _envelope.Delete();
                from.SendMessage("O pergaminho foi colocado no envelope.");
            }
        }

        public CorreioEnvelope(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class CartaAberta : Bag
    {
        [Constructable]
        public CartaAberta() : base()
        {
            ItemID = 0x138F;
            Name = "carta aberta";
            Weight = 0.1;
            Movable = true;
        }

        public HtmlDocumentBase GetLetterDocument()
        {
            foreach (Item item in Items)
                if (item is HtmlDocumentBase doc)
                    return doc;

            return null;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Você precisa usar um selo de envio nesta carta.");
        }

        public CartaAberta(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class Carta : Bag
    {
        [Constructable]
        public Carta() : base()
        {
            ItemID = 0x1391;
            Name = "carta";
            Weight = 0.1;
            Movable = true;
        }

        public HtmlDocumentBase GetLetterDocument()
        {
            foreach (Item item in Items)
                if (item is HtmlDocumentBase doc)
                    return doc;

            return null;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from?.Backpack == null || !IsChildOf(from.Backpack))
            {
                from?.SendMessage("A carta precisa estar na sua mochila.");
                return;
            }

            HtmlDocumentBase doc = GetLetterDocument();
            if (doc == null)
            {
                from.SendMessage("Essa carta está vazia.");
                return;
            }

            doc.ItemID = Utility.RandomBool() ? 0x138E : 0x1395;

            if (Parent is Container c)
                c.DropItem(doc);
            else
                from.Backpack.DropItem(doc);

            Delete();

            PlayerMobile pm = from as PlayerMobile;
            if (pm != null)
            {
                pm.CloseGump(typeof(HtmlReadGump));
                doc.OpenReadGump(pm, false);
            }
        }

        public Carta(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    // Compatibilidade com nome antigo
    public class CartaRecebida : Carta
    {
        [Constructable]
        public CartaRecebida() : base()
        {
        }

        public CartaRecebida(Serial serial) : base(serial)
        {
        }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }



    public class CorreioPacote : Bag
    {
        private bool _sealedForMail;
        private int _sealedWeight;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SealedForMail
        {
            get { return _sealedForMail; }
            set
            {
                _sealedForMail = value;
                Hue = _sealedForMail ? 903 : 878;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SealedWeight
        {
            get { return _sealedWeight; }
            set { _sealedWeight = value; }
        }

        [Constructable]
        public CorreioPacote() : base()
        {
            ItemID = 0xE79; // pouch
            Name = "pacote";
            Weight = 1.0;
            Movable = true;
            Hue = 878;
            _sealedForMail = false;
            _sealedWeight = 0;
        }

        public override bool DisplaysContent
        {
            get { return false; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from?.Backpack == null || !IsChildOf(from.Backpack))
            {
                from?.SendMessage("O pacote precisa estar na sua mochila.");
                return;
            }

            if (SealedForMail)
            {
                OpenSealedPackage(from);
                return;
            }

            from.SendMessage("Selecione o item que deseja colocar no pacote.");
            from.Target = new PacoteTarget(this);
        }


        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            from.SendMessage("Use dois cliques no pacote para selecionar o item que deseja colocar nele.");
            return false;
        }

        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (from != null)
                from.SendMessage("Use dois cliques no pacote para selecionar o item que deseja colocar nele.");

            return false;
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            return false;
        }

        private void OpenSealedPackage(Mobile from)
        {
            Container pack = from.Backpack;
            if (pack == null)
                return;

            List<Item> items = new List<Item>(Items);

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                if (item == null || item.Deleted)
                    continue;

                try
                {
                    pack.DropItem(item);
                }
                catch
                {
                    item.MoveToWorld(from.Location, from.Map);
                }
            }

            SealedForMail = false;
            SealedWeight = 0;
            Hue = 0;

            from.SendMessage("Você abre o pacote e retira os itens.");
            Delete();
        }

        public bool TrySeal(Mobile from)
        {
            if (Deleted)
                return false;

            if (SealedForMail)
            {
                from.SendMessage("Esse pacote já está selado.");
                return false;
            }

            if (Items == null || Items.Count == 0)
            {
                from.SendMessage("O pacote está vazio.");
                return false;
            }

            if (Items.Count > 5)
            {
                from.SendMessage("O pacote pode conter no máximo 5 itens.");
                return false;
            }

            if (ContainsCoins(this))
            {
                from.SendMessage("Você não pode selar um pacote com moedas.");
                return false;
            }

            double totalWeight = TotalWeight;
            int stones = (int)Math.Ceiling(totalWeight);

            if (stones > 26)
            {
                from.SendMessage("O pacote selado não pode passar de 26 stones no total.");
                return false;
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is Container)
                {
                    from.SendMessage("Você não pode colocar bolsas, caixas ou outros containers no pacote.");
                    return false;
                }
            }

            SealedForMail = true;
            SealedWeight = stones;
            Hue = 903;
            from.SendMessage("Você sela o pacote para envio.");
            return true;
        }

        private static bool ContainsCoins(Item item)
        {
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

        private class PacoteTarget : Target
        {
            private readonly CorreioPacote _pacote;

            public PacoteTarget(CorreioPacote pacote) : base(12, false, TargetFlags.None)
            {
                _pacote = pacote;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_pacote == null || _pacote.Deleted || from?.Backpack == null)
                    return;

                if (_pacote.SealedForMail)
                {
                    from.SendMessage("Esse pacote está selado.");
                    return;
                }

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

                if (item == _pacote)
                {
                    from.SendMessage("Você não pode colocar o pacote dentro dele mesmo.");
                    return;
                }

                if (item is Container)
                {
                    from.SendMessage("Você não pode colocar bolsas, caixas ou outros containers no pacote.");
                    return;
                }

                if (_pacote.Items.Count >= 5)
                {
                    from.SendMessage("O pacote já está cheio. O máximo é 5 itens.");
                    return;
                }

                if (item is Gold || item is BankCheck)
                {
                    from.SendMessage("Você não pode colocar moedas no pacote.");
                    return;
                }

                if (item is Carta || item is CartaAberta)
                {
                    from.SendMessage("Você não pode colocar cartas no pacote.");
                    return;
                }

                if (item.Weight > 5.0)
                {
                    from.SendMessage("O item não pode passar de 5 stones.");
                    return;
                }

                try
                {
                    _pacote.DropItem(item);
                    from.SendMessage("Item colocado no pacote.");
                }
                catch
                {
                    from.SendMessage("Não foi possível colocar o item no pacote.");
                }
            }
        }

        public CorreioPacote(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (SealedForMail)
                list.Add("pacote com selo");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(_sealedForMail);
            writer.Write(_sealedWeight);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
            {
                _sealedForMail = reader.ReadBool();
                _sealedWeight = reader.ReadInt();
            }

            Hue = _sealedForMail ? 903 : 878;
        }
    }
}
