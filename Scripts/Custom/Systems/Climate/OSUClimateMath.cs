using System;

namespace Server.Custom.Systems.Climate
{
    public static class OSUClimateMath
    {
        public const int MinTemp = -16;
        public const int MaxTemp = 16;

        // Sua matemática das estações (passo 1)
        public static int ApplySeasonToBase(int baseTemp, WorldTime.OSUSeason season)
        {
            int t = baseTemp;

            if (season == WorldTime.OSUSeason.Spring || season == WorldTime.OSUSeason.Autumn)
            {
                // não muda
            }
            else if (season == WorldTime.OSUSeason.Winter)
            {
                if (t == 0)
                    t = -1;
                else if (t < 0)
                    t = t * 2;
                else // t > 0
                    t = t - 2;
            }
            else if (season == WorldTime.OSUSeason.Summer)
            {
                if (t == 0)
                    t = 1;
                else if (t > 0)
                    t = t * 2;
                else // t < 0
                    t = t + 2;
            }

            if (t < MinTemp) t = MinTemp;
            if (t > MaxTemp) t = MaxTemp;

            return t;
        }

        // Dia/noite baseado na hora do WorldNow
        public static bool IsNight(DateTime worldNow, int nightStartHour, int nightEndHour)
        {
            int h = worldNow.Hour;

            // Ex: noite começa 20 e termina 6
            if (nightStartHour > nightEndHour)
                return (h >= nightStartHour) || (h < nightEndHour);

            // Ex: noite começa 18 e termina 22 (raro)
            return (h >= nightStartHour) && (h < nightEndHour);
        }
    }
}
