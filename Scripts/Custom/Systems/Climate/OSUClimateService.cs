using System;
using Server;
using Server.Custom.Systems.WorldTime;

namespace Server.Custom.Systems.Climate
{
    public static class OSUClimateService
    {
        // Ajuste aqui como você quer sua noite
        public static int NightStartsHour = 20;
        public static int NightEndsHour = 6;

        public static int GetEffectiveTemperatureAt(int x, int y, int mapIndex, out OSUClimateRegion region, out WorldTime.OSUSeason season)
        {
            season = OSUClimateTimeAdapter.SeasonNow;
            region = OSUClimateRegions.FindAt(x, y, mapIndex);

            int baseTemp = 0;
            bool isStatic = false;

            if (region != null)
            {
                baseTemp = region.BaseTemperature;
                isStatic = region.IsStatic;
            }

            int effective = baseTemp;

            // Se a região for static (dungeon), não aplica estação
            if (!isStatic)
                effective = OSUClimateMath.ApplySeasonToBase(baseTemp, season);

            return effective;
        }

        public static bool IsNightNow()
        {
            DateTime now = OSUClimateTimeAdapter.WorldNow;
            return OSUClimateMath.IsNight(now, NightStartsHour, NightEndsHour);
        }
    }
}
