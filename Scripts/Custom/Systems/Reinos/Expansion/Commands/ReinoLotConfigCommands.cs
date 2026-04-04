using System;
using Server.Commands;

namespace Server.Custom.Systems.Reinos
{
    public class ReinoLotConfigCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReinoLotConfig", AccessLevel.GameMaster, OnLotConfig);
            CommandSystem.Register("ReinoLotSpawnOffset", AccessLevel.GameMaster, OnLotSpawnOffset);
            CommandSystem.Register("ReinoLotMultiOffset", AccessLevel.GameMaster, OnLotMultiOffset);
        }

        private static void OnLotConfig(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 2)
            {
                e.Mobile.SendMessage("Use [ReinoLotConfig <idDoLote> <configId>.");
                return;
            }

            int lotId;
            int configId;

            if (!Int32.TryParse(e.Arguments[0], out lotId) || !Int32.TryParse(e.Arguments[1], out configId))
            {
                e.Mobile.SendMessage("Valores inválidos.");
                return;
            }

            string message;
            ReinoExpansionSystem.SetLotConfig(lotId, configId, out message);
            e.Mobile.SendMessage(message);
        }

        private static void OnLotSpawnOffset(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 4)
            {
                e.Mobile.SendMessage("Use [ReinoLotSpawnOffset <idDoLote> <x> <y> <z>.");
                return;
            }

            int lotId;
            int x;
            int y;
            int z;

            if (!Int32.TryParse(e.Arguments[0], out lotId) || !Int32.TryParse(e.Arguments[1], out x) || !Int32.TryParse(e.Arguments[2], out y) || !Int32.TryParse(e.Arguments[3], out z))
            {
                e.Mobile.SendMessage("Valores inválidos.");
                return;
            }

            string message;
            ReinoExpansionSystem.SetLotSpawnOffset(lotId, x, y, z, out message);
            e.Mobile.SendMessage(message);
        }

        private static void OnLotMultiOffset(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 4)
            {
                e.Mobile.SendMessage("Use [ReinoLotMultiOffset <idDoLote> <x> <y> <z>.");
                return;
            }

            int lotId;
            int x;
            int y;
            int z;

            if (!Int32.TryParse(e.Arguments[0], out lotId) || !Int32.TryParse(e.Arguments[1], out x) || !Int32.TryParse(e.Arguments[2], out y) || !Int32.TryParse(e.Arguments[3], out z))
            {
                e.Mobile.SendMessage("Valores inválidos.");
                return;
            }

            string message;
            ReinoExpansionSystem.SetLotEncounterOffset(lotId, x, y, z, out message);
            e.Mobile.SendMessage(message);
        }
    }
}
