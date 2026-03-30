using System;
using Server.Custom.Systems.WorldTime;

namespace Server.Custom.Systems.Climate
{
    public static class OSUClimateTimeAdapter
    {
        // Quantos DIAS DO MUNDO cada estação dura.
        // Se você quer 15 dias por estação, deixa 15.
        public static int SeasonLengthDays = 15;

        // ✅ Essas duas propriedades existem porque o resto do seu sistema está pedindo elas:
        public static DateTime WorldNow
        {
            get { return OSUWorldTime.WorldNow; }
        }

        public static OSUSeason SeasonNow
        {
            get { return GetSeasonFromWorldNow(); }
        }

        // Se você precisar em outro lugar:
        public static bool IsNightNow
        {
            get { return IsNight(WorldNow); }
        }

        // ===== Cálculo da estação baseado SEMPRE no WorldNow =====
        private static OSUSeason GetSeasonFromWorldNow()
        {
            DateTime now = OSUWorldTime.WorldNow;

            // segurança
            if (now.Year < 1)
                now = new DateTime(1500, 1, 1);

            int dayIndex = now.DayOfYear - 1; // 0..364
            int len = SeasonLengthDays;

            if (len < 1)
                len = 15;

            int seasonIndex = (dayIndex / len) % 4;

            if (seasonIndex == 0) return OSUSeason.Spring;
            if (seasonIndex == 1) return OSUSeason.Summer;
            if (seasonIndex == 2) return OSUSeason.Autumn;
            return OSUSeason.Winter;
        }

        // ===== Dia/noite baseado no WorldNow =====
        private static bool IsNight(DateTime worldNow)
        {
            int h = worldNow.Hour;
            return (h >= 20 || h < 6); // 20:00 até 06:00
        }
    }
}
