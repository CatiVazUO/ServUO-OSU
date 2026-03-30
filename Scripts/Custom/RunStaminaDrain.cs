using System;
using System.Collections.Generic;
using Server;
using Server.Network;

namespace Server.Custom
{
    public static class RunStaminaDrain
    {
        // A pé (correndo): 1 stam a cada X passos
        private const int StepsPerDrainOnFoot = 2;

        // Montado (correndo): 1 stam a cada X passos (maior = drena menos)
        private const int StepsPerDrainMounted = 6; // experimente 8, 10, 12...

        private const int DrainAmount = 1;
        private static readonly bool BlockWhenZero = false;

        private static readonly Dictionary<Serial, int> _runSteps = new Dictionary<Serial, int>();

        public static void Initialize()
        {
            EventSink.Movement += OnMovement;
            EventSink.Disconnected += OnDisconnected;
        }

        private static void OnDisconnected(DisconnectedEventArgs e)
        {
            if (e?.Mobile != null)
                _runSteps.Remove(e.Mobile.Serial);
        }

        private static void OnMovement(MovementEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null || e.Blocked)
                return;

            if (!from.Alive || !from.Player)
                return;

            bool running = (e.Direction & Direction.Running) != 0;

            if (!running)
            {
                _runSteps.Remove(from.Serial);
                return;
            }

            // Define “quão frequente” drena baseado em estar montado
            int stepsPerDrain = from.Mounted ? StepsPerDrainMounted : StepsPerDrainOnFoot;

            int steps;
            if (!_runSteps.TryGetValue(from.Serial, out steps))
                steps = 0;

            steps++;

            if (steps < stepsPerDrain)
            {
                _runSteps[from.Serial] = steps;
                return;
            }

            // bateu o limite: drena e reseta
            _runSteps[from.Serial] = 0;

            if (from.Stam <= 0)
            {
                if (BlockWhenZero)
                {
                    from.SendLocalizedMessage(500110);
                    e.Blocked = true;
                }
                return;
            }

            from.Stam = Math.Max(0, from.Stam - DrainAmount);

            if (BlockWhenZero && from.Stam <= 0)
            {
                from.SendLocalizedMessage(500110);
                e.Blocked = true;
            }
        }
    }
}
