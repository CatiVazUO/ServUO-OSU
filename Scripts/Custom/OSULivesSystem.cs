using System;
using Server.Mobiles;

namespace Server.Custom.Systems
{
    public static class OSULivesSystem
    {
        // Defaults (você pode mexer depois)
        public const int DefaultPlayerMaxLives = 3;

        public static readonly TimeSpan DefaultPlayerKnockout = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan DefaultPlayerRegenInterval = TimeSpan.FromHours(1);

        // Local do “céu” (ajuste)
        public static Map HeavenMap = Map.Felucca;
        public static Point3D HeavenLocation = new Point3D(1000, 1000, 0);

        // Local do “limbo do KO” (ajuste)
        public static Map KnockoutMap = Map.Felucca;
        public static Point3D KnockoutLocation = new Point3D(1050, 1000, 0);

        // Hooks para traits/buffs (por enquanto 1.0 = sem mod)
        public static double GetKnockoutTimeMultiplier(Mobile m) => 1.0;
        public static double GetRegenTimeMultiplier(Mobile m) => 1.0;

        public static TimeSpan GetKnockoutDuration(Mobile m)
        {
            double mult = GetKnockoutTimeMultiplier(m);
            if (mult <= 0.0) mult = 1.0;
            return TimeSpan.FromSeconds(DefaultPlayerKnockout.TotalSeconds * mult);
        }

        public static TimeSpan GetRegenInterval(Mobile m)
        {
            double mult = GetRegenTimeMultiplier(m);
            if (mult <= 0.0) mult = 1.0;
            return TimeSpan.FromSeconds(DefaultPlayerRegenInterval.TotalSeconds * mult);
        }

        public static void MoveToHeaven(PlayerMobile pm)
        {
            pm.MoveToWorld(HeavenLocation, HeavenMap);
        }

        public static void MoveToKnockoutArea(PlayerMobile pm)
        {
            pm.MoveToWorld(KnockoutLocation, KnockoutMap);
        }
    }
}
