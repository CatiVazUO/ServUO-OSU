using Server;
using Server.Mobiles;

namespace Server.Custom.Systems.Creation.Engine
{
    public static class OSUAvatarReleaseOnDeath
    {
        public static void Initialize()
        {
            EventSink.PlayerDeath += OnPlayerDeath;
        }

        private static void OnPlayerDeath(PlayerDeathEventArgs args)
        {
            var pm = args.Mobile as PlayerMobile;

            if (pm == null || pm.Deleted)
                return;

            int avatarId = pm.OSUAvatarId;

            if (avatarId > 0)
            {
                OSUAvatarRegistry.UnmarkUsed(avatarId);

                // Opcional (eu NÃO faria, a não ser que você queira “apagar” o avatar do personagem):
                // pm.OSUAvatarId = 0;
            }
        }
    }
}
