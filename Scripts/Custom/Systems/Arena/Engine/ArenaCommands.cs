using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Custom.Systems.Arena
{
    public class ArenaCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Bomba", AccessLevel.Player, OnBomba);
        }

        private static void OnBomba(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            if (!ArenaGameModes.HandleBombCommand(pm))
                pm.SendMessage("Você só pode usar [bomba durante um evento de bomberman da arena.");
        }
    }
}
