using System;
using Server.Mobiles;
using Server.Items;

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
            // 50% de chance
            return Utility.RandomBool();
        }

        public static void NotifySpoiledConsumed(PlayerMobile pm, Item source)
        {
            OnSpoiledConsumed?.Invoke(pm, source);
        }
    }
}
