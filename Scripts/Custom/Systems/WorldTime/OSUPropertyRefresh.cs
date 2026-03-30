using Server;
using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Custom.Systems.WorldTime
{
    public static class OSUPropertyRefresh
    {
        // Chame isso depois de [timeadd / [timeset
        public static void RefreshAllPlayersBackpacks()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                // força refresh do backpack inteiro (comidas/armas/armaduras etc dentro)
                Container pack = pm.Backpack;
                if (pack != null)
                    RefreshContainerRecursive(pack);
            }
        }

        private static void RefreshContainerRecursive(Container c)
        {
            if (c == null)
                return;

            c.InvalidateProperties();

            foreach (Item item in c.Items)
            {
                item.InvalidateProperties();

                Container sub = item as Container;
                if (sub != null)
                    RefreshContainerRecursive(sub);
            }
        }
    }
}
