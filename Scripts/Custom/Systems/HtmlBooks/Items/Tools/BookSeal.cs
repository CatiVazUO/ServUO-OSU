using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    /// <summary>
    /// Item "selo" que sela definitivamente um documento.
    /// 2x no selo, 1x no livro/pergaminho/página.
    /// </summary>
    public class BookSeal : Item
    {
        private int _sealId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int SealId
        {
            get { return _sealId; }
            set { _sealId = value; InvalidateProperties(); }
        }

        [Constructable]
        public BookSeal() : base(0x193F)
        {
            Name = "Selo";
            Weight = 0.1;
            _sealId = 0;
        }

        public BookSeal(Serial serial) : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            // Mostra o ID do selo usado para criar este item
            // 0 = genérico/invisível, 1..100 = custom
            list.Add(1060662, "{0}\t{1}", "ID do Selo", _sealId);
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "Você precisa estar com o selo na mochila.");
                return;
            }

            pm.SendMessage(0x55, "Selecione o livro/pergaminho/página que deseja selar.");
            pm.Target = new SealTarget(this);
        }

        private class SealTarget : Target
        {
            private readonly BookSeal _seal;

            public SealTarget(BookSeal seal) : base(12, false, TargetFlags.None)
            {
                _seal = seal;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || _seal == null || _seal.Deleted)
                    return;

                if (targeted == _seal)
                {
                    pm.CloseGump(typeof(BookSealPreviewGump));
                    pm.SendGump(new BookSealPreviewGump(pm, _seal.SealId));
                    return;
                }

                ISealableDocument doc = targeted as ISealableDocument;
                if (doc == null)
                {
                    pm.SendMessage(0x22, "Isso não é um documento selável.");
                    return;
                }

                Item item = targeted as Item;
                if (item == null)
                {
                    pm.SendMessage(0x22, "Erro interno.");
                    return;
                }

                if (!item.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "O documento precisa estar na sua mochila para selar.");
                    return;
                }

                if (doc.IsSealed)
                {
                    pm.SendMessage(0x22, "Este documento já está selado.");
                    return;
                }

                HtmlDocumentBase htmlDoc = item as HtmlDocumentBase;
                if (htmlDoc != null)
                {
                    if (!htmlDoc.CanEdit(pm))
                    {
                        pm.SendMessage(0x22, "Somente o autor deste documento pode selá-lo.");
                        return;
                    }

                    htmlDoc.EnsureAuthor(pm);
                }

                HtmlCompilationBook compDoc = item as HtmlCompilationBook;
                if (compDoc != null)
                {
                    if (!string.IsNullOrWhiteSpace(compDoc.CompiledBy) && !compDoc.IsCompiler(pm))
                    {
                        pm.SendMessage(0x22, "Somente quem iniciou este livro de compilação pode fechá-lo.");
                        return;
                    }

                    compDoc.EnsureCompiler(pm);
                }

                // Se for um documento HTML do nosso sistema, pedir título antes de selar
                if (htmlDoc != null && string.IsNullOrWhiteSpace(htmlDoc.DocumentTitle))
                {
                    pm.CloseGump(typeof(HtmlDocumentTitleGump));
                    pm.SendGump(new HtmlDocumentTitleGump(pm, htmlDoc, _seal));
                    return;
                }

                // Sela definitivamente
                doc.SealId = _seal.SealId;
                doc.Seal(pm);

                if (doc.IsSealed)
                {
                    if (compDoc != null)
                        pm.SendMessage(0x55, "Você fechou o livro de compilação. Não será possível adicionar mais páginas.");
                    else
                        pm.SendMessage(0x55, "Você selou o documento. Ele não poderá mais ser editado.");

                    _seal.Delete();
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(_sealId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
                _sealId = reader.ReadInt();
        }
    }
}
