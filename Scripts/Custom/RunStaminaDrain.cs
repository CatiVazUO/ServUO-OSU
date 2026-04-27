using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom
{
    public static class RunStaminaDrain
    {
        // A pé (correndo): 1 stam a cada X passos
        private const int StepsPerDrainOnFoot = 4;

        // Montado (correndo): 1 stam a cada X passos (maior = drena menos)
        private const int StepsPerDrainMounted = 4; // experimente 8, 10, 12...

        private const int DrainAmount = 1;
        private const int RiderCargoWeight = 150;
        private static readonly bool BlockWhenZero = false;

        private static readonly Dictionary<Serial, int> _runSteps = new Dictionary<Serial, int>();

        public static void Initialize()
        {
            EventSink.Movement += OnMovement;
            EventSink.Disconnected += OnDisconnected;
        }

        private static void OnDisconnected(DisconnectedEventArgs e)
        {
            if (e != null && e.Mobile != null)
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

            // OSU: peso aumenta a frequência do gasto de stamina.
            // Exemplo: StepsPerDrainMounted = 6
            // multiplicador 2 => drena a cada 3 passos
            // multiplicador 3 => drena a cada 2 passos
            int drainMultiplier = GetDrainFrequencyMultiplier(from);
            if (drainMultiplier > 1)
                stepsPerDrain = Math.Max(1, stepsPerDrain / drainMultiplier);

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
                 //   from.SendLocalizedMessage(500110);
                    e.Blocked = true;
                }
                return;
            }

            from.Stam = Math.Max(0, from.Stam - DrainAmount);

            if (BlockWhenZero && from.Stam <= 0)
            {
             //   from.SendLocalizedMessage(500110);
                e.Blocked = true;
            }
        }

        private static int GetDrainFrequencyMultiplier(Mobile from)
        {
            if (from == null)
                return 1;

            HorseFrisioCarga cargoHorse = from.Mount as HorseFrisioCarga;

            if (cargoHorse != null)
            {
                double usedPercent = GetCargoHorseUsedPercent(cargoHorse);

                // Cheio/máximo: 3x mais rápido.
                if (usedPercent >= 2.0)
                    return 3;

                // Acima de 75%: 2x mais rápido.
                if (usedPercent > 1.00)
                    return 2;

                // Até 75%, incluindo a faixa até 50%, não aumenta o gasto.
                return 1;
            }

            // Regra a pé: se o jogador estiver com mais de 90% do peso máximo,
            // correr gasta stamina 2x mais rápido.
            if (!from.Mounted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm != null && GetPlayerUsedWeightPercent(pm) > 0.90m)
                    return 2;
            }

            return 1;
        }

        private static double GetCargoHorseUsedPercent(HorseFrisioCarga horse)
        {
            if (horse == null)
                return 0.0;

            int maxWeight = Math.Max(1, horse.GetCargoMaxWeight());
            int cargoWeight = 0;

            Container pack = horse.Backpack;
            if (pack != null)
                cargoWeight = (int)Math.Ceiling((double)pack.TotalWeight);

            // O cavaleiro conta como 150 stones de carga.
            int usedWeight = cargoWeight + RiderCargoWeight;

            double usedPercent = (double)usedWeight / (double)maxWeight;

            if (usedPercent < 0.0)
                usedPercent = 0.0;

            if (usedPercent > 1.0)
                usedPercent = 1.0;

            return usedPercent;
        }

        private static decimal GetPlayerUsedWeightPercent(PlayerMobile pm)
        {
            if (pm == null)
                return 0.0m;

            decimal maxWeight = Math.Max(1, pm.MaxWeight);
            decimal currentWeight = Mobile.BodyWeight + pm.TotalWeight;

            if (currentWeight < 0.0m)
                currentWeight = 0.0m;

            decimal usedPercent = currentWeight / maxWeight;

            if (usedPercent < 0.0m)
                usedPercent = 0.0m;

            if (usedPercent > 1.0m)
                usedPercent = 1.0m;

            return usedPercent;
        }
    }
}
