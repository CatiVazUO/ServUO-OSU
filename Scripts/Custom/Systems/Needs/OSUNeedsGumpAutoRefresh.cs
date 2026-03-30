using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Custom.Systems.Needs.Gumps;

namespace Server.Custom.Systems.Needs
{
    public static class OSUNeedsGumpAutoRefresh
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(2.0);

        private class Snapshot
        {
            public int Hunger;
            public int Thirst;
            public int Comfort;
        }

        private static readonly Dictionary<Serial, Snapshot> _last = new Dictionary<Serial, Snapshot>();

        public static void Initialize()
        {
            Timer.DelayCall(Interval, Interval, Tick);
            EventSink.Logout += OnLogout;
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm != null)
                _last.Remove(pm.Serial);
        }

        private static void Tick()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null || pm.Deleted || pm.NetState == null || !pm.NetState.Running)
                    continue;

                if (!pm.HasGump(typeof(OSUNeedsGump)))
                    continue;

                int hunger = pm.OSUHunger;
                int thirst = pm.OSUThirst;
                int comfort = Server.Custom.Systems.Climate.OSUClimatePenaltySystem.GetThermalComfort(pm);

                Snapshot s;
                if (!_last.TryGetValue(pm.Serial, out s))
                {
                    s = new Snapshot();
                    _last[pm.Serial] = s;
                    s.Hunger = hunger;
                    s.Thirst = thirst;
                    s.Comfort = comfort;
                    continue;
                }

                if (s.Hunger != hunger || s.Thirst != thirst || s.Comfort != comfort)
                {
                    s.Hunger = hunger;
                    s.Thirst = thirst;
                    s.Comfort = comfort;

                    OSUNeedsGump.TryRefresh(pm);
                }
            }
        }
    }
}
