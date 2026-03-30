using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;

namespace Server.Custom.Systems.Animations
{
    public static class OSUKnockdownSystem
    {
        private static readonly Dictionary<Mobile, OSUKnockdownTimer> _active = new Dictionary<Mobile, OSUKnockdownTimer>();

        public static bool IsKnockedDown(Mobile m)
        {
            return m != null && _active.ContainsKey(m);
        }

        public static void KnockDown(Mobile m, TimeSpan duration)
        {
            if (m == null || m.Deleted || !m.Alive)
                return;

            OSUKnockdownTimer oldTimer;
            if (_active.TryGetValue(m, out oldTimer))
            {
                oldTimer.Stop();
                _active.Remove(m);
            }

            int fallAnim;
            int holdAnim;
            int getUpAnim;

            GetAnimSet(m, out fallAnim, out holdAnim, out getUpAnim);

            m.Frozen = true;
            m.Animate(fallAnim, 7, 1, true, false, 0);

            OSUKnockdownTimer timer = new OSUKnockdownTimer(m, duration, holdAnim, getUpAnim);
            _active[m] = timer;
            timer.Start();
        }

        internal static void Clear(Mobile m)
        {
            if (m == null)
                return;

            OSUKnockdownTimer oldTimer;
            if (_active.TryGetValue(m, out oldTimer))
                _active.Remove(m);

            if (!m.Deleted)
                m.Frozen = false;
        }

        private static void GetAnimSet(Mobile m, out int fallAnim, out int holdAnim, out int getUpAnim)
        {
            fallAnim = 21;
            holdAnim = 21;
            getUpAnim = 21;

            if (m.Body != null && m.Body.Type == BodyType.Human)
            {
                int chosen = Utility.RandomBool() ? 21 : 22;
                fallAnim = chosen;
                holdAnim = chosen;
                getUpAnim = chosen;
            }
            else if (m.Body != null && m.Body.Type == BodyType.Animal)
            {
                fallAnim = 8;
                holdAnim = 8;
                getUpAnim = 8;
            }
            else if (m.Body != null && m.Body.Type == BodyType.Monster)
            {
                fallAnim = 2;
                holdAnim = 2;
                getUpAnim = 2;
            }
        }

        private class OSUKnockdownTimer : Timer
        {
            private readonly Mobile _mob;
            private readonly DateTime _end;
            private readonly int _holdAnim;
            private readonly int _getUpAnim;
            private int _stage;

            public OSUKnockdownTimer(Mobile mob, TimeSpan duration, int holdAnim, int getUpAnim)
                : base(TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(0.8))
            {
                Priority = TimerPriority.TwoFiftyMS;

                _mob = mob;
                _end = DateTime.UtcNow + duration;
                _holdAnim = holdAnim;
                _getUpAnim = getUpAnim;
                _stage = 0;
            }

            protected override void OnTick()
            {
                if (_mob == null || _mob.Deleted || !_mob.Alive)
                {
                    Stop();
                    OSUKnockdownSystem.Clear(_mob);
                    return;
                }

                if (_stage == 0)
                {
                    // entra na pose caída
                    _mob.Animate(_holdAnim, 6, 1, false, false, 255);
                    _stage = 1;
                    return;
                }

                if (DateTime.UtcNow < _end)
                {
                    // refresca a pose caída periodicamente
                    _mob.Animate(_holdAnim, 6, 1, false, false, 255);
                    return;
                }

                // levantar
                _mob.Animate(_getUpAnim, 6, 1, false, false, 0);

                Timer.DelayCall(TimeSpan.FromSeconds(0.4), delegate
                {
                    OSUKnockdownSystem.Clear(_mob);
                });

                Stop();
            }
        }
    }
}
