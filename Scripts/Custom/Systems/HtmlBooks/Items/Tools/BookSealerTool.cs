using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Gumps;
using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Server.Items
{
    /// <summary>
    /// Ferramenta de selagem:
    /// - 2x nela, clique na Cera Quente (Wax) para criar um Selo.
    /// - 2x nela, clique em você mesmo para escolher o selo (gump).
    /// </summary>
    public class BookSealerTool : BaseTool
    {
        private int _sealId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int SealId
        {
            get { return _sealId; }
            set { _sealId = value; InvalidateProperties(); }
        }

        // Não é uma ferramenta de craft (apenas para compatibilidade do BaseTool)
        public override CraftSystem CraftSystem { get { return null; } }

        [Constructable]
        public BookSealerTool() : this(CraftResource.Iron)
        {
        }

        [Constructable]
        public BookSealerTool(CraftResource resource) : base(GetDefaultUses(resource), 0x0DF5)
        {
            Name = "Selador";
            Weight = 1.0;

            Resource = resource;
            _sealId = 0;
        }

        public BookSealerTool(Serial serial) : base(serial)
        {
        }

        private static int GetDefaultUses(CraftResource resource)
        {
            // Quanto melhor o material, mais usos.
            // (valores simples, você pode ajustar depois)
            switch (resource)
            {
                case CraftResource.DullCopper: return 60;
                case CraftResource.ShadowIron: return 70;
                case CraftResource.Copper: return 80;
                case CraftResource.Bronze: return 90;
                case CraftResource.Gold: return 100;
                case CraftResource.Agapite: return 110;
                case CraftResource.Verite: return 120;
                case CraftResource.Valorite: return 130;
                default: return 50;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendMessage(0x22, "Você precisa estar com o selador na mochila.");
                return;
            }

            if (UsesRemaining <= 0)
            {
                pm.SendMessage(0x22, "Seu selador está sem usos.");
                Delete();
                return;
            }

            pm.SendMessage(0x55, "Clique na Cera Quente para criar um selo, ou clique em você para escolher o selo.");
            pm.Target = new WaxOrSelfTarget(this);
        }

        private class WaxOrSelfTarget : Target
        {
            private readonly BookSealerTool _tool;

            public WaxOrSelfTarget(BookSealerTool tool) : base(12, false, TargetFlags.None)
            {
                _tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || _tool == null || _tool.Deleted)
                    return;

                if (!_tool.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "Você precisa estar com o selador na mochila.");
                    return;
                }

                if (_tool.UsesRemaining <= 0)
                {
                    pm.SendMessage(0x22, "Seu selador está sem usos.");
                    _tool.Delete();
                    return;
                }

                // Clique em si mesmo: abre gump de seleção
                if (targeted == _tool)
                {
                    if (_tool.SealId != 0)
                    {
                        from.SendMessage("Esta ferramenta já tem um selo selecionado e não pode ser alterado.");
                        pm.CloseGump(typeof(BookSealPreviewGump));
                        pm.SendGump(new BookSealPreviewGump(pm, _tool.SealId));
                        return;
                    }

                    from.SendGump(new BookSealSelectGump(pm, _tool));
                    return;
                }

                // Clique na cera quente
                Wax wax = targeted as Wax;
                if (wax == null)
                {
                    pm.SendMessage(0x22, "Isso não é cera quente (use Cooking para fazer).\nOu clique em você para escolher o selo.");
                    return;
                }

                if (!wax.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage(0x22, "A cera quente precisa estar na sua mochila.");
                    return;
                }

                if (wax.Amount < 1)
                {
                    pm.SendMessage(0x22, "Você não tem cera quente suficiente.");
                    return;
                }

                // Garante exclusividade do selo (exceto 0)
                if (_tool.SealId > 0 && !BookSealRegistry.TryReserve(_tool, _tool.SealId))
                {
                    pm.SendMessage(0x22, "Este selo já foi escolhido por outra ferramenta. Escolha outro.");
                    pm.CloseGump(typeof(BookSealSelectGump));
                    pm.SendGump(new BookSealSelectGump(pm, _tool));
                    return;
                }

                wax.Consume(1);

                BookSeal seal = new BookSeal();
                seal.SealId = _tool.SealId;

                if (!pm.PlaceInBackpack(seal))
                    seal.MoveToWorld(pm.Location, pm.Map);

                // usa a ferramenta
                _tool.UsesRemaining--;

                pm.SendMessage(0x55, "Você criou um selo.");

                if (_tool.UsesRemaining <= 0 && _tool.BreakOnDepletion)
                {
                    pm.SendMessage(0x22, "Seu selador se desgastou e desapareceu.");
                    _tool.Delete();
                }
            }
        }

        public override void OnDelete()
        {
            BookSealRegistry.Release(this);
            base.OnDelete();
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1060662, "{0}\t{1}", "Selo N:", _sealId);
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

            if (version >= 1)
                _sealId = reader.ReadInt();
            else
                _sealId = 0;

            BookSealRegistry.OnToolLoaded(this);
        }
    }
}
