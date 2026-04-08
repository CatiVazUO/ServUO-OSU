using System;
using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public static class ReinoMilitaryCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("PularCerca", AccessLevel.Player, new CommandEventHandler(OnJumpFence));
            CommandSystem.Register("Embainhar", AccessLevel.Player, new CommandEventHandler(OnSheathe));
            CommandSystem.Register("EmbainharAuto", AccessLevel.Player, new CommandEventHandler(OnAutoSheathe));
        }

        [Usage("[PularCerca")]
        [Description("Tenta pular uma meia parede ou cerca adjacente.")]
        private static void OnJumpFence(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            string message;
            if (ReinoMilitarySystem.TryJumpFence(pm, out message))
                pm.SendMessage("Você pula a cerca.");
            else if (!String.IsNullOrWhiteSpace(message))
                pm.SendMessage(message);
        }

        [Usage("[Embainhar")]
        [Description("Tenta embainhar a arma da mão.")]
        private static void OnSheathe(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            if (ReinoMilitarySystem.SheatheWeapons(pm, true))
                pm.SendMessage("Você embainha sua arma.");
            else
                pm.SendMessage("Você não conseguiu embainhar a arma agora.");
        }

        [Usage("[EmbainharAuto")]
        [Description("Liga ou desliga o embainhar automático ao entrar em terras de reino.")]
        private static void OnAutoSheathe(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            bool active = ReinoMilitarySystem.ToggleAutoSheathe(pm);
            pm.SendMessage(active
                ? "Embainhar automático ativado para terras de reino."
                : "Embainhar automático desativado.");
        }
    }
}
