using Server.Engines.Craft;
using Server.Mobiles;
using Server.Targeting;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Items
{
    public class PenAndInkTool : BaseTool
    {
        public override CraftSystem CraftSystem { get { return null; } }

        [Constructable]
        public PenAndInkTool() : this(CraftResource.Iron)
        {
        }

        [Constructable]
        public PenAndInkTool(CraftResource resource) : base(50, 0x0FBF)
        {
            Name = "Caneta e Tinta";
            Weight = 1.0;
            Resource = resource;
        }

        public PenAndInkTool(Serial serial) : base(serial)
        {
        }

        public bool ConsumeOneUse(PlayerMobile pm)
        {
            if (UsesRemaining <= 0)
            {
                if (pm != null)
                    pm.SendMessage(0x22, "Sua ferramenta de escrita está sem usos.");
                Delete();
                return false;
            }

            UsesRemaining--;

            if (UsesRemaining <= 0 && BreakOnDepletion)
            {
                if (pm != null)
                    pm.SendMessage(0x22, "Sua ferramenta de escrita se desgastou e desapareceu.");
                Delete();
            }

            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "Você precisa estar com a caneta na mochila.");
                return;
            }

            if (UsesRemaining <= 0)
            {
                pm.SendMessage(0x22, "Sua ferramenta de escrita está sem usos.");
                Delete();
                return;
            }

            pm.SendMessage(0x55, "Selecione o livro/pergaminho/página que deseja escrever.");
            pm.Target = new WriteTarget(this);
        }

        private class WriteTarget : Target
        {
            private readonly PenAndInkTool _tool;

            public WriteTarget(PenAndInkTool tool) : base(12, false, TargetFlags.None)
            {
                _tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || _tool == null || _tool.Deleted)
                    return;

                HtmlDocumentBase doc = targeted as HtmlDocumentBase;
                if (doc == null)
                {
                    pm.SendMessage(0x22, "Isso não é um livro/pergaminho/página escrevível.");
                    return;
                }

                if (!doc.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "Você precisa estar com o documento na mochila para escrever.");
                    return;
                }

                if (doc.IsSealed)
                {
                    pm.SendMessage(0x22, "Este documento já foi selado e não pode mais ser editado.");
                    return;
                }

                if (!doc.CanEdit(pm))
                {
                    pm.SendMessage(0x22, "Somente o autor deste documento pode continuar editando.");
                    return;
                }

                if (_tool.UsesRemaining <= 0)
                {
                    pm.SendMessage(0x22, "Sua ferramenta de escrita está sem usos.");
                    _tool.Delete();
                    return;
                }

                _tool.UsesRemaining--;

                if (_tool.UsesRemaining <= 0 && _tool.BreakOnDepletion)
                {
                    pm.SendMessage(0x22, "Sua ferramenta de escrita se desgastou e desapareceu.");
                    _tool.Delete();
                }

                doc.EnsureAuthor(pm);

                pm.CloseGump(typeof(HtmlWriteGump));
                pm.SendGump(new HtmlWriteGump(pm, doc, 0, 0));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
