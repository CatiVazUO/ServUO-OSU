using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.OSUDrag
{
    public static class OSUDragSystem
    {
        private class ForcedLayState : Timer
        {
            public Mobile Mobile;
            public int Anim;
            public int AnchorSerial;
            public int XOffset;
            public int YOffset;
            public int ZOffset;

            public ForcedLayState(Mobile m, int anim, int anchorSerial, int xOffset, int yOffset, int zOffset)
                : base(TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(0.8))
            {
                Priority = TimerPriority.TwoFiftyMS;
                Mobile = m;
                Anim = anim;
                AnchorSerial = anchorSerial;
                XOffset = xOffset;
                YOffset = yOffset;
                ZOffset = zOffset;
            }

            protected override void OnTick()
            {
                if (Mobile == null || Mobile.Deleted || !Mobile.Alive)
                {
                    Stop();
                    OSUDragSystem.RemoveForcedLay(Mobile, false);
                    return;
                }

                Mobile.Direction = Direction.East;

                Item anchor = World.FindItem(AnchorSerial);
                if (anchor != null && !anchor.Deleted && anchor.Map != null && anchor.Map != Map.Internal)
                    Mobile.MoveToWorld(new Point3D(anchor.X + XOffset, anchor.Y + YOffset, anchor.Z + ZOffset), anchor.Map);

                Mobile.Animate(Anim, 6, 1, false, false, 255);
            }
        }

        private static readonly Dictionary<int, ForcedLayState> _lay = new Dictionary<int, ForcedLayState>();

        public static void Initialize()
        {
            EventSink.Movement += OnMovement;
            EventSink.Disconnected += OnDisconnected;
        }

        public static bool IsForcedLying(Mobile m)
        {
            return m != null && _lay.ContainsKey(m.Serial.Value);
        }

        public static void ForceLayDown(Mobile m, Item anchor, int zOffset)
        {
            ForceLayDown(m, anchor, 0, 0, zOffset);
        }

        public static void ForceLayDown(Mobile m, Item anchor, int xOffset, int yOffset, int zOffset)
        {
            if (m == null || m.Deleted)
                return;

            ForcedLayState state;
            int anchorSerial = (anchor != null ? anchor.Serial.Value : 0);

            if (_lay.TryGetValue(m.Serial.Value, out state))
            {
                state.AnchorSerial = anchorSerial;
                state.XOffset = xOffset;
                state.YOffset = yOffset;
                state.ZOffset = zOffset;

                m.Frozen = true;
                m.Direction = Direction.East;

                if (anchor != null && anchor.Map != null && anchor.Map != Map.Internal)
                    m.MoveToWorld(new Point3D(anchor.X + xOffset, anchor.Y + yOffset, anchor.Z + zOffset), anchor.Map);

                m.Animate(state.Anim, 7, 1, true, false, 0);
                return;
            }

            int anim = 21;
            state = new ForcedLayState(m, anim, anchorSerial, xOffset, yOffset, zOffset);
            _lay[m.Serial.Value] = state;

            m.Frozen = true;
            m.Direction = Direction.East;

            if (anchor != null && anchor.Map != null && anchor.Map != Map.Internal)
                m.MoveToWorld(new Point3D(anchor.X + xOffset, anchor.Y + yOffset, anchor.Z + zOffset), anchor.Map);

            m.Animate(anim, 7, 1, true, false, 0);
            state.Start();
        }

        public static void ReleaseForcedLay(Mobile m)
        {
            RemoveForcedLay(m, true);
        }

        private static void RemoveForcedLay(Mobile m, bool unfreezeIfAllowed)
        {
            if (m == null)
                return;

            ForcedLayState state;
            if (_lay.TryGetValue(m.Serial.Value, out state))
            {
                state.Stop();
                _lay.Remove(m.Serial.Value);
            }

            if (unfreezeIfAllowed)
                m.Frozen = false;
        }

        private static void OnMovement(MovementEventArgs e)
        {
            if (e == null || e.Mobile == null || e.Blocked)
                return;

            Mobile m = e.Mobile;

            ForcedLayState state;
            if (!_lay.TryGetValue(m.Serial.Value, out state))
                return;

            e.Blocked = true;
            m.Frozen = true;
            m.Direction = Direction.East;

            Item anchor = World.FindItem(state.AnchorSerial);
            if (anchor != null && !anchor.Deleted && anchor.Map != null && anchor.Map != Map.Internal)
                m.MoveToWorld(new Point3D(anchor.X + state.XOffset, anchor.Y + state.YOffset, anchor.Z + state.ZOffset), anchor.Map);

            m.Animate(state.Anim, 6, 1, false, false, 255);
        }

        private static void OnDisconnected(DisconnectedEventArgs e)
        {
            if (e == null || e.Mobile == null)
                return;

            if (_lay.ContainsKey(e.Mobile.Serial.Value))
                e.Mobile.Frozen = true;
        }
    }
}
