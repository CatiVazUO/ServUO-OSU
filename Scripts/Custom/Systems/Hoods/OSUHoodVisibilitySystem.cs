
using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Hoods
{
    public static class OSUHoodVisibilitySystem
    {
        private static readonly Dictionary<int, string> m_PreviousNameMods = new Dictionary<int, string>();
        private static Timer m_Timer;

        public static void Initialize()
        {
            EventSink.Login += OnLogin;
            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(2.0), RefreshAllPlayers);
        }

        public static bool IsOcultingHood(Item item)
        {
            if (item == null || item.Deleted)
                return false;

            return item.ItemID >= 0xCB20 && item.ItemID <= 0xCB3C;
        }

        public static bool IsEncoberto(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null || pm.Deleted)
                return false;

            for (int i = 0; i < pm.Items.Count; i++)
            {
                if (IsOcultingHood(pm.Items[i]))
                    return true;
            }

            return pm.NameMod == "Encoberto";
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm != null)
                Refresh(pm);
        }

        private static void RefreshAllPlayers()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm != null && !pm.Deleted)
                    Refresh(pm);
            }
        }

        public static void Refresh(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            bool hiddenByHood = false;

            for (int i = 0; i < pm.Items.Count; i++)
            {
                if (IsOcultingHood(pm.Items[i]))
                {
                    hiddenByHood = true;
                    break;
                }
            }

            if (hiddenByHood)
            {
                if (!m_PreviousNameMods.ContainsKey(pm.Serial.Value))
                    m_PreviousNameMods[pm.Serial.Value] = pm.NameMod;

                if (pm.NameMod != "Encoberto")
                    pm.NameMod = "Encoberto";
            }
            else
            {
                string previous;
                if (m_PreviousNameMods.TryGetValue(pm.Serial.Value, out previous))
                {
                    if (pm.NameMod == "Encoberto")
                        pm.NameMod = previous;

                    m_PreviousNameMods.Remove(pm.Serial.Value);
                }
            }

            pm.InvalidateProperties();
        }
    }
}
