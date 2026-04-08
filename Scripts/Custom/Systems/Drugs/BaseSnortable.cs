using System;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Items
{
    public enum ContentType2
    {
        Banestone = 0
    }

    public abstract class BaseSnortable : Item
    {
        private int m_ContentRemaining;
        private ContentType2 m_ContentType2;

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual int ContentRemaining
        {
            get { return m_ContentRemaining; }
            set { m_ContentRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual ContentType2 ContentType2
        {
            get { return m_ContentType2; }
            set { m_ContentType2 = value; InvalidateProperties(); }
        }

        [Constructable]
        public BaseSnortable(int itemID, int contentTotal)
            : base(itemID)
        {
            m_ContentRemaining = contentTotal;
            Weight = 0.1;
        }

        public BaseSnortable(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Doses restantes: {0}", m_ContentRemaining);
        }

        public virtual void OnSnort(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm != null && m_ContentType2 == ContentType2.Banestone)
                HallucinationEffect.BeginHallucinating(pm, 120);

            from.Emote("*cheira*");
            from.SendMessage("Uma euforia repentina toma conta de você.");
            from.PlaySound(1208);

            if (from.Body.IsHuman)
                from.Animate(34, 5, 1, true, false, 0);
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
                from.SendMessage("Não resta nada para cheirar.");
                return false;
            }

            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!CanUse(from))
                return;

            OnSnort(from);
            ReinoMilitarySystem.NotifyDrugUse(from);
        }

        public override bool StackWith(Mobile from, Item dropped, bool playSound)
        {
            BaseSnortable other = dropped as BaseSnortable;

            if (other == null)
                return false;

            if (other.GetType() != GetType())
                return false;

            if (other.ContentRemaining != ContentRemaining || other.ContentType2 != ContentType2)
                return false;

            return base.StackWith(from, dropped, playSound);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);
            writer.Write((int)m_ContentType2);
            writer.Write(m_ContentRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_ContentType2 = (ContentType2)reader.ReadInt();
                    goto case 0;
                case 0:
                    m_ContentRemaining = reader.ReadInt();
                    break;
            }
        }
    }
}