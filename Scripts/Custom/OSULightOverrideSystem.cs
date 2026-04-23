using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Custom
{
    public static class OSULightOverrideSystem
    {
        private class Entry
        {
            public int Global;
            public int Personal;
        }

        private static readonly Dictionary<int, Entry> m_Entries = new Dictionary<int, Entry>();

        public static void SetOverride(PlayerMobile pm, int global, int personal)
        {
            if (pm == null || pm.Deleted)
                return;

            m_Entries[pm.Serial.Value] = new Entry { Global = Clamp(global), Personal = Clamp(personal) };
            pm.CheckLightLevels(true);
        }

        public static void ClearOverride(PlayerMobile pm)
        {
            if (pm == null)
                return;

            if (m_Entries.Remove(pm.Serial.Value) && !pm.Deleted)
                pm.CheckLightLevels(true);
        }

        public static bool TryGetOverride(PlayerMobile pm, out int global, out int personal)
        {
            global = 0;
            personal = 0;

            if (pm == null)
                return false;

            Entry entry;
            if (!m_Entries.TryGetValue(pm.Serial.Value, out entry) || entry == null)
                return false;

            global = Clamp(entry.Global);
            personal = Clamp(entry.Personal);
            return true;
        }

        private static int Clamp(int value)
        {
            if (value < 0)
                return 0;

            if (value > 30)
                return 30;

            return value;
        }
    }
}
