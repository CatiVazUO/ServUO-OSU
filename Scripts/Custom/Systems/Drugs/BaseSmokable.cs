using System;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Items
{
    public enum ContentType
    {
        Swampweed = 0,
        Tobacco = 1,
        Opium = 2
    }

    public abstract class BaseSmokable : Item
    {
        private int m_ContentRemaining;
        private ContentType m_ContentType;

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual int ContentRemaining
        {
            get { return m_ContentRemaining; }
            set { m_ContentRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual ContentType ContentType
        {
            get { return m_ContentType; }
            set { m_ContentType = value; InvalidateProperties(); }
        }

        [Constructable]
        public BaseSmokable(int itemID, int contentTotal)
            : base(itemID)
        {
            m_ContentRemaining = contentTotal;
            Weight = 0.1;
        }

        public BaseSmokable(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Cargas restantes: {0}", m_ContentRemaining);
        }

        public virtual void OnSmoke(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm != null && m_ContentType == ContentType.Swampweed)
                HallucinationEffect.BeginHallucinating(pm, 60);

            from.Emote("*solta fumaça*");
            from.PlaySound(1208);

            if (from.Body.IsHuman)
                from.Animate(34, 5, 1, true, false, 0);

            switch (m_ContentType)
            {
                case ContentType.Swampweed:
                    from.SendMessage("As cores ao seu redor parecem vibrar.");
                    break;
                case ContentType.Opium:
                    from.SendMessage("Um torpor quente toma conta do seu corpo.");
                    break;
                default:
                    from.SendMessage("Você sente a fumaça descer pesada pelos pulmões.");
                    break;
            }
        }

        protected virtual bool CanUse(Mobile from)
        {
            if (from == null || !(from is PlayerMobile) || RootParent != from)
            {
                from.SendLocalizedMessage(1042001);
                return false;
            }

            if (m_ContentRemaining <= 0)
            {
                from.SendMessage("Não resta nada para fumar.");
                return false;
            }

            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!CanUse(from))
                return;

            OnSmoke(from);
            ReinoMilitarySystem.NotifyDrugUse(from);
        }

        public override bool StackWith(Mobile from, Item dropped, bool playSound)
        {
            BaseSmokable other = dropped as BaseSmokable;

            if (other == null)
                return false;

            if (other.GetType() != GetType())
                return false;

            if (other.ContentRemaining != ContentRemaining || other.ContentType != ContentType)
                return false;

            return base.StackWith(from, dropped, playSound);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);
            writer.Write((int)m_ContentType);
            writer.Write(m_ContentRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_ContentType = (ContentType)reader.ReadInt();
                    goto case 0;
                case 0:
                    m_ContentRemaining = reader.ReadInt();
                    break;
            }
        }
    }
}