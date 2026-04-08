using System;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Reinos;

namespace Server.Items
{
    public class Pipe : BaseSmokable
    {
        [Constructable]
        public Pipe()
            : base(0x0E89, 0)
        {
            Name = "cachimbo";
            Weight = 1.0;
            ContentType = ContentType.Tobacco;
        }

        public Pipe(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (ContentRemaining > 0)
                list.Add("Substância: {0}", GetContentLabel(ContentType));
            else
                list.Add("Vazio");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (RootParent != from || !(from is PlayerMobile))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            if (ContentRemaining > 0)
            {
                OnSmoke(from);
                ReinoMilitarySystem.NotifyDrugUse(from);
                ContentRemaining--;
                InvalidateProperties();

                if (ContentRemaining <= 0)
                    from.SendMessage("O cachimbo se apagou.");
            }
            else
            {
                from.SendMessage("Selecione a substância para encher o cachimbo.");
                from.Target = new PipeFillTarget(this);
            }
        }

        private static string GetContentLabel(ContentType type)
        {
            switch (type)
            {
                case ContentType.Swampweed: return "Swampweed";
                case ContentType.Opium: return "Ópio";
                default: return "Tabaco";
            }
        }

        private class PipeFillTarget : Target
        {
            private readonly Pipe m_Pipe;

            public PipeFillTarget(Pipe pipe)
                : base(12, false, TargetFlags.None)
            {
                m_Pipe = pipe;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Pipe == null || m_Pipe.Deleted)
                    return;

                if (m_Pipe.RootParent != from)
                {
                    from.SendMessage("O cachimbo precisa estar na sua mochila.");
                    return;
                }

                Item item = targeted as Item;
                if (item == null || !item.IsChildOf(from.Backpack))
                {
                    from.SendMessage("A substância precisa estar na sua mochila.");
                    return;
                }

                if (m_Pipe.ContentRemaining > 0)
                {
                    from.SendMessage("Esse cachimbo ainda contém substância. Esvazie-o primeiro.");
                    return;
                }

                ContentType type;
                if (item is Tobacco)
                    type = ContentType.Tobacco;
                else if (item is Swampweed)
                    type = ContentType.Swampweed;
                else if (item is Opium)
                    type = ContentType.Opium;
                else
                {
                    from.SendMessage("Você só pode encher o cachimbo com tabaco, swampweed ou ópio.");
                    return;
                }

                item.Consume(1);
                m_Pipe.ContentType = type;
                m_Pipe.ContentRemaining = 3;
                m_Pipe.InvalidateProperties();
                from.SendMessage("Você enche o cachimbo com {0}.", GetContentLabel(type).ToLowerInvariant());
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