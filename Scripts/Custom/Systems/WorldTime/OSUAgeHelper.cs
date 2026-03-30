using System;
using Server.Mobiles;

namespace Server.Custom.Systems.WorldTime
{
    public static class OSUAgeHelper
    {
        /// <summary>
        /// Age is based ONLY on world time.
        /// If GM sets world date forward 3 years, everybody ages 3 years.
        /// </summary>
        public static int GetCurrentAge(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return 0;

            int baseAge = pm.OSURpAgeBase; // idade escolhida na criação (base)

            DateTime birthWorld = pm.OSURpBirthWorldTime;
            if (birthWorld == default(DateTime))
                return baseAge;

            DateTime now = OSUWorldTime.WorldNow;

            int years = now.Year - birthWorld.Year;

            // If birthday (month/day) hasn't happened yet this year, subtract one.
            if (now.Month < birthWorld.Month || (now.Month == birthWorld.Month && now.Day < birthWorld.Day))
                years--;

            if (years < 0) years = 0;

            return baseAge + years;
        }
    }
}
