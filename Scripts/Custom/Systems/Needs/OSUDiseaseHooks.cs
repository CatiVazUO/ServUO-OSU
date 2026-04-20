using System;
using Server.Mobiles;
using Server.Items;
using Server.Custom.Systems.Health;

namespace Server.Custom.Systems.Needs
{
    // Placeholder pro futuro sistema de doenças.
    // Hoje: só decide se o jogador fica doente ao comer/beber algo estragado.
    public static class OSUDiseaseHooks
    {
        // O sistema de doenças pode assinar esse evento depois.
        public static event Action<PlayerMobile, Item> OnSpoiledConsumed;

        public static bool ShouldBecomeSick(PlayerMobile pm, Item source)
        {
            if (pm == null || source == null)
                return false;

            if (OSUHealthSystem.IsContaminated(source))
                return true;

            // Comida/bebida apenas estragada, mas não contaminada: ainda existe risco genérico.
            return Utility.RandomBool();
        }

        public static void NotifySpoiledConsumed(PlayerMobile pm, Item source)
        {
            if (pm != null && source != null)
                OSUHealthSystem.TryExposeFromItem(pm, source);

            OnSpoiledConsumed?.Invoke(pm, source);
        }
    }
}
