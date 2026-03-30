using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Custom.Commands
{
    public static class DeitarCommand
    {
        private static readonly Dictionary<Mobile, DeitadoState> m_States = new Dictionary<Mobile, DeitadoState>();

        public static void Initialize()
        {
            CommandSystem.Register("Deitar", AccessLevel.Player, new CommandEventHandler(OnCommand));
        }

        private static void OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null || from.Deleted)
                return;

            if (from.Mounted)
            {
                from.SendMessage("Você não pode deitar enquanto estiver montado.");
                return;
            }

            if (!(from is PlayerMobile))
            {
                from.SendMessage("Apenas jogadores podem usar esse comando.");
                return;
            }

            if (IsDeitado(from))
                Levantar(from);
            else
                Deitar(from);
        }

        public static bool IsDeitado(Mobile m)
        {
            return m != null && m_States.ContainsKey(m);
        }

        public static void Deitar(Mobile from)
        {
            if (from == null || from.Deleted || IsDeitado(from))
                return;

            int anim = Utility.RandomBool() ? 21 : 22;

            from.Frozen = true;
            from.Animate(anim, 7, 1, true, false, 0);
            from.Say("*deita*");

            DeitadoState state = new DeitadoState(from, anim);
            m_States[from] = state;

            state.Start();
        }

        public static void Levantar(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            DeitadoState state;
            if (!m_States.TryGetValue(from, out state))
                return;

            state.Stop();
            m_States.Remove(from);

            from.Frozen = false;
            from.Animate(state.Anim, 6, 1, false, false, 0);
            from.Say("*se levanta*");
        }

        public static void OnMovement(Mobile m)
        {
            if (!IsDeitado(m))
                return;

            Levantar(m);
        }

        private class DeitadoState : Timer
        {
            private readonly Mobile m_Mobile;
            public int Anim { get; private set; }

            public DeitadoState(Mobile m, int anim)
                : base(TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(0.8))
            {
                Priority = TimerPriority.TwoFiftyMS;
                m_Mobile = m;
                Anim = anim;
            }

            protected override void OnTick()
            {
                if (m_Mobile == null || m_Mobile.Deleted || !m_Mobile.Alive)
                {
                    Stop();
                    if (m_Mobile != null)
                        m_States.Remove(m_Mobile);
                    return;
                }

                if (m_Mobile.Mounted)
                {
                    Levantar(m_Mobile);
                    return;
                }

                // Mantém a pose deitado
                m_Mobile.Animate(Anim, 6, 1, false, false, 255);
            }
        }
    }

    public class DeitarMovementHook
    {
        public static void Initialize()
        {
            EventSink.Movement += new MovementEventHandler(OnMovement);
            EventSink.Logout += new LogoutEventHandler(OnLogout);
        }

        private static void OnMovement(MovementEventArgs e)
        {
            Mobile m = e.Mobile;

            if (m == null || m.Deleted)
                return;

            if (DeitarCommand.IsDeitado(m))
                DeitarCommand.Levantar(m);
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            Mobile m = e.Mobile;

            if (m == null || m.Deleted)
                return;

            if (DeitarCommand.IsDeitado(m))
                DeitarCommand.Levantar(m);
        }
    }
}
