using System;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Custom.Systems.Correios.Correios
{
    public class PergaminhoEditavel : Item
    {
        private string _texto;

        [CommandProperty(AccessLevel.GameMaster)]
        public string Texto
        {
            get => _texto;
            set { _texto = value; InvalidateProperties(); }
        }

        [Constructable]
        public PergaminhoEditavel() : base(0x14ED)
        {
            Name = "pergaminho em branco";
            Weight = 1.0;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            base.AddNameProperty(list);
            list.Add("(duplo clique para escrever)");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("Isso precisa estar na sua mochila.");
                return;
            }

            from.CloseGump(typeof(PergaminhoEditavelGump));
            from.SendGump(new PergaminhoEditavelGump(this));
        }

        public PergaminhoEditavel(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(_texto);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int v = reader.ReadInt();
            _texto = reader.ReadString();
        }
    }

    public class PergaminhoEditavelGump : Gump
    {
        private readonly PergaminhoEditavel _scroll;
        private const int EntryId = 1;

        public PergaminhoEditavelGump(PergaminhoEditavel scroll) : base(100, 50)
        {
            _scroll = scroll;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(0, 0, 1250); // user requested gump id

            // Full area text entry
            AddTextEntry(35, 35, 350, 380, 0, EntryId, scroll?.Texto ?? "");

            AddButton(340, 420, 247, 248, 1, GumpButtonType.Reply, 0);
            AddLabel(370, 420, 0, "Salvar");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile;
            if (_scroll == null || _scroll.Deleted)
                return;

            if (info.ButtonID == 1)
            {
                string text = info.GetTextEntry(EntryId)?.Text;
                _scroll.Texto = text;
                from.SendMessage("Pergaminho salvo.");
            }
        }
    }
}
