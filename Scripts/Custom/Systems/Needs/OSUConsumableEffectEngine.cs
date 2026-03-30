using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Custom.Systems.Needs
{
    public static class OSUConsumableEffectEngine
    {
        private static readonly Dictionary<Serial, List<Timer>> _active = new Dictionary<Serial, List<Timer>>();

        // tick a cada 5 minutos
        public static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(3.0);

        public static void StartHot(PlayerMobile pm, int durationMinutes, int hpPerTick, int stamPerTick, int manaPerTick, bool debug = false)
        {
            if (pm == null || pm.Deleted)
                return;

            if (durationMinutes <= 0)
                return;

            if (hpPerTick == 0 && stamPerTick == 0 && manaPerTick == 0)
                return;

            int ticks = durationMinutes / 3;
            if (ticks <= 0)
                ticks = 1;

            var t = new HotTimer(pm, ticks, hpPerTick, stamPerTick, manaPerTick, debug);
            Register(pm, t);
            t.Start();
        }

        private static void Register(PlayerMobile pm, Timer t)
        {
            List<Timer> list;
            if (!_active.TryGetValue(pm.Serial, out list))
            {
                list = new List<Timer>();
                _active[pm.Serial] = list;
            }

            list.Add(t);
        }

        private static void Unregister(PlayerMobile pm, Timer t)
        {
            List<Timer> list;
            if (_active.TryGetValue(pm.Serial, out list))
            {
                list.Remove(t);
                if (list.Count == 0)
                    _active.Remove(pm.Serial);
            }
        }

        private class HotTimer : Timer
        {
            private readonly PlayerMobile _pm;
            private readonly int _hp;
            private readonly int _stam;
            private readonly int _mana;
            private readonly bool _debug;

            private int _remaining;
            private bool _unregistered;

            public HotTimer(PlayerMobile pm, int ticks, int hp, int stam, int mana, bool debug)
                : base(TickInterval, TickInterval)
            {
                _pm = pm;
                _remaining = ticks;
                _hp = hp;
                _stam = stam;
                _mana = mana;
                _debug = debug;

                Priority = TimerPriority.OneMinute;
            }

            protected override void OnTick()
            {
                if (_pm == null || _pm.Deleted || _pm.NetState == null || !_pm.NetState.Running)
                {
                    SafeUnregister();
                    Stop();
                    return;
                }

                if (_hp != 0)
                    _pm.Hits = Math.Min(_pm.HitsMax, _pm.Hits + _hp);

                if (_stam != 0)
                    _pm.Stam = Math.Min(_pm.StamMax, _pm.Stam + _stam);

                if (_mana != 0)
                    _pm.Mana = Math.Min(_pm.ManaMax, _pm.Mana + _mana);

                if (_debug)
                    _pm.SendMessage("HOT tick aplicado.");

                _remaining--;

                if (_remaining <= 0)
                {
                    SafeUnregister();
                    Stop();
                }
            }

            private void SafeUnregister()
            {
                if (_unregistered)
                    return;

                _unregistered = true;
                Unregister(_pm, this);
            }
        }

    }
}

