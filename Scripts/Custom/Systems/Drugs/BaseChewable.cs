using System;
using System.Collections;
using Server.Network;
using Server.Mobiles;
using Server.Custom.Reinos;

namespace Server.Items
{
    public enum Chewable
    {
        Qat = 0
    }

    public abstract class BaseChewable : Item
    {
        private int m_ChewableRemaining;
        private Chewable m_Chewable;

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual int ChewableRemaining
        {
            get { return m_ChewableRemaining; }
            set { m_ChewableRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual Chewable Chewable
        {
            get { return m_Chewable; }
            set { m_Chewable = value; InvalidateProperties(); }
        }

        [Constructable]
        public BaseChewable(int itemID, int chewableTotal)
            : base(itemID)
        {
            m_ChewableRemaining = chewableTotal;
            Weight = 0.1;
        }

        public BaseChewable(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Doses restantes: {0}", m_ChewableRemaining);
        }

        public virtual void OnChew(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm != null && m_Chewable == Chewable.Qat)
                ChewTimer.BeginChew(pm, 15);

            from.Emote("*mastiga*");
            from.PlaySound(Utility.RandomMinMax(58, 60));
            from.SendMessage("Você sente uma onda narcótica.");
        }

        protected virtual bool CanUse(Mobile from)
        {
            if (from == null || !(from is PlayerMobile) || RootParent != from)
            {
                from.SendLocalizedMessage(1042001);
                return false;
            }

            if (m_ChewableRemaining <= 0)
            {
                from.SendMessage("Não resta nada para mastigar.");
                return false;
            }

            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!CanUse(from))
                return;

            OnChew(from);
            ReinoMilitarySystem.NotifyDrugUse(from);
        }

        public override bool StackWith(Mobile from, Item dropped, bool playSound)
        {
            BaseChewable other = dropped as BaseChewable;

            if (other == null)
                return false;

            if (other.GetType() != GetType())
                return false;

            if (other.ChewableRemaining != ChewableRemaining)
                return false;

            return base.StackWith(from, dropped, playSound);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);
            writer.Write((int)m_Chewable);
            writer.Write(m_ChewableRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_Chewable = (Chewable)reader.ReadInt();
                    goto case 0;
                case 0:
                    m_ChewableRemaining = reader.ReadInt();
                    break;
            }
        }
    }

    public class ChewTimer : Timer
    {
        private static readonly Hashtable m_Table = new Hashtable();

        public static bool IsChewing(PlayerMobile m)
        {
            return m != null && m_Table.Contains(m);
        }

        public static void BeginChew(PlayerMobile m, int duration)
        {
            if (m == null || m.Deleted)
                return;

            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new ChewTimer(m, duration);
            m_Table[m] = t;
            t.Start();
        }

        public static void EndChew(PlayerMobile m)
        {
            if (m == null)
                return;

            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            if (m.Deleted)
                return;

            m.SendMessage("Você cospe no chão.");
            m.Emote("*cospe*");
            m.PlaySound(m.Female ? 820 : 1094);
        }

        private readonly PlayerMobile m_Chewer;
        private int m_Duration;

        public ChewTimer(PlayerMobile from, int duration)
            : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
        {
            Priority = TimerPriority.OneSecond;
            m_Chewer = from;
            m_Duration = duration;
        }

        protected override void OnTick()
        {
            if (m_Chewer == null || m_Chewer.Deleted)
            {
                Stop();
                return;
            }

            m_Duration -= 1;

            if (m_Duration <= 0)
            {
                EndChew(m_Chewer);
                return;
            }

            m_Chewer.Emote("*mastiga*");
            m_Chewer.PlaySound(Utility.RandomMinMax(58, 60));
            Interval = Delay = TimeSpan.FromSeconds(15);
        }
    }
}