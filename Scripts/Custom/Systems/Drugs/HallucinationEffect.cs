using System;
using System.Collections;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
    public class HallucinationEffect
    {
        private static readonly Hashtable m_Table = new Hashtable();

        public static bool IsHallucinating(PlayerMobile m)
        {
            return m != null && m_Table.Contains(m);
        }

        public static void BeginHallucinating(PlayerMobile m, int duration)
        {
            if (m == null || m.Deleted)
                return;

            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new HallucinationTimer(m, duration);
            m_Table[m] = t;
            t.Start();
        }

        public static void EndHallucination(PlayerMobile m)
        {
            if (m == null)
                return;

            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            if (!m.Deleted)
                m.SendMessage("Sua mente volta lentamente ao normal.");
        }

        public static void SendHallucinationItem(PlayerMobile hallucinator, IEntity e, int itemID, int speed, int duration, int renderMode, int hue)
        {
            Map map = e.Map;

            if (map == null || hallucinator == null)
                return;

            NetState state = hallucinator.NetState;
            if (state == null)
                return;

            hallucinator.ProcessDelta();
            Packet regular = new LocationEffect(e, itemID, speed, duration, renderMode, hue);
            state.Send(regular);
        }

        private class HallucinationTimer : Timer
        {
            private readonly PlayerMobile m_Hallucinator;
            private int m_Duration;

            public HallucinationTimer(PlayerMobile from, int duration)
                : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            {
                Priority = TimerPriority.OneSecond;
                m_Hallucinator = from;
                m_Duration = duration;
            }

            protected override void OnTick()
            {
                if (m_Hallucinator == null || m_Hallucinator.Deleted || m_Hallucinator.Map == null || m_Hallucinator.Map == Map.Internal)
                {
                    Stop();
                    return;
                }

                m_Duration -= 1;
                if (m_Duration <= 0)
                {
                    EndHallucination(m_Hallucinator);
                    return;
                }

                int hue = Utility.Random(2, 550);
                IPooledEnumerable eable = m_Hallucinator.GetItemsInRange(10);
                int i = 0;

                foreach (Item item in eable)
                {
                    if (i > 5)
                        break;

                    if (item.Visible)
                    {
                        SendHallucinationItem(m_Hallucinator, item, item.ItemID, 5, 5000, hue, 4410);
                        i++;
                    }
                }

                eable.Free();

                IEntity entity = new Entity(Serial.Zero, new Point3D(m_Hallucinator.X + Utility.Random(-4, 5), m_Hallucinator.Y + Utility.Random(-4, 5), m_Hallucinator.Z + Utility.Random(0, 10)), m_Hallucinator.Map);
                SendHallucinationItem(m_Hallucinator, entity, 2444 + Utility.Random(15370 - 2444), 5, 5000, hue, 4410);
                Interval = Delay = TimeSpan.FromSeconds(1);
            }
        }
    }
}
