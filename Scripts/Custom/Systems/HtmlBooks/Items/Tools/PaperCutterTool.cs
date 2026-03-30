using System;
using Server;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Systems.HtmlBooks.Html.Readable;

namespace Server.Items
{
    public class PaperCutterTool : BaseTool
    {
        public override CraftSystem CraftSystem { get { return null; } }

        [Constructable]
        public PaperCutterTool() : this(CraftResource.Iron)
        {
        }

        [Constructable]
        public PaperCutterTool(CraftResource resource) : base(50, 0x13F7)
        {
            Name = "Cortador de Papel";
            Weight = 1.0;
            Resource = resource;
        }

        public PaperCutterTool(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "Você precisa estar com o cortador na mochila.");
                return;
            }

            if (UsesRemaining <= 0)
            {
                pm.SendMessage(0x22, "Seu cortador está sem usos.");
                Delete();
                return;
            }

            pm.SendMessage(0x55, "Selecione o livro de compilação.");
            pm.Target = new CutterTarget(this);
        }

        private class CutterTarget : Target
        {
            private readonly PaperCutterTool _tool;

            public CutterTarget(PaperCutterTool tool) : base(12, false, TargetFlags.None)
            {
                _tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || _tool == null || _tool.Deleted)
                    return;

                HtmlCompilationBook book = targeted as HtmlCompilationBook;
                if (book == null)
                {
                    pm.SendMessage(0x22, "Isso não é um livro de compilação.");
                    return;
                }

                if (!book.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "O livro precisa estar na sua mochila.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(book.CompiledBy) && !book.IsCompiler(pm))
                {
                    pm.SendMessage(0x22, "Somente o autor deste livro pode retirar páginas.");
                    return;
                }

                pm.CloseGump(typeof(PaperCutterGump));
                pm.SendGump(new PaperCutterGump(pm, _tool, book));
            }
        }

        private class PaperCutterGump : Gump
        {
            private readonly PlayerMobile _pm;
            private readonly PaperCutterTool _tool;
            private readonly HtmlCompilationBook _book;

            private const int EntryId = 1;

            public PaperCutterGump(PlayerMobile pm, PaperCutterTool tool, HtmlCompilationBook book) : base(0, 0)
            {
                _pm = pm;
                _tool = tool;
                _book = book;

                Closable = true;
                Dragable = true;
                Resizable = false;
                Disposable = true;

                AddPage(0);

                // fundo principal
                AddImageTiled(174, 102, 242, 157, 375);
                AddImageTiled(409, 111, 25, 144, 369);
                AddImageTiled(151, 110, 26, 143, 370);
                AddImageTiled(166, 85, 241, 25, 371);
                AddImageTiled(178, 255, 231, 30, 372);

                // cantos
                AddImage(143, 77, 402);
                AddImage(401, 81, 402);
                AddImage(144, 246, 402);
                AddImage(404, 244, 402);

                // título / instruções
                AddLabel(198, 116, 0, "Cortador de Papel");
                AddLabel(181, 137, 0, "Qual página você deseja cortar?");
                AddLabel(196, 154, 0, "Digite um número de 1 a " + _book.PageCount.ToString());

                // campo de texto
                AddTextEntry(222, 178, 144, 20, 0, EntryId, "");

                // botão OK
                AddButton(255, 223, 559, 560, 1, GumpButtonType.Reply, 0);

                // botão cancelar
                AddButton(325, 223, 241, 242, 0, GumpButtonType.Reply, 0);
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (_pm == null || _tool == null || _book == null || _tool.Deleted || _book.Deleted)
                    return;

                if (info.ButtonID != 1)
                    return;

                if (!_tool.IsChildOf(_pm.Backpack) || !_book.IsChildOf(_pm.Backpack))
                {
                    _pm.SendMessage(0x22, "O livro e o cortador precisam estar na sua mochila.");
                    return;
                }

                if (_tool.UsesRemaining <= 0)
                {
                    _pm.SendMessage(0x22, "Seu cortador está sem usos.");
                    _tool.Delete();
                    return;
                }

                TextRelay tr = info.GetTextEntry(EntryId);
                string txt = tr != null ? tr.Text ?? "" : "";
                txt = txt.Trim();

                int pageNumber;
                if (!int.TryParse(txt, out pageNumber))
                {
                    _pm.SendMessage(0x22, "Digite apenas números.");
                    _pm.SendGump(new PaperCutterGump(_pm, _tool, _book));
                    return;
                }

                if (pageNumber < 1 || pageNumber > _book.PageCount)
                {
                    _pm.SendMessage(0x22, "Esse número de página não existe nesse livro.");
                    _pm.SendGump(new PaperCutterGump(_pm, _tool, _book));
                    return;
                }

                if (_book.RemovePageToLoose(_pm, pageNumber))
                {
                    _tool.UsesRemaining--;

                    if (_tool.UsesRemaining <= 0 && _tool.BreakOnDepletion)
                    {
                        _pm.SendMessage(0x22, "Seu cortador se desgastou e desapareceu.");
                        _tool.Delete();
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
