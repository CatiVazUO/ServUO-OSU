using System;
using Server;

namespace Server.Custom.Systems.WorldTime
{
    public enum OSUSeason
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    /// <summary>
    /// Single-source-of-truth world clock:
    /// - 60 real days (UTC) == +1 world year
    /// - GM can set/add time; age & seasons follow WorldNow
    /// </summary>
    public static class OSUWorldTime
    {
        // ===== Fixed rule: 60 real days == 1 world year =====
        public const double RealDaysPerWorldYear = 60.0;

        // We keep a normal DateTime calendar (365/366 days per year) for display.
        // This is how many world days advance over one world year (auto from calendar).
        // For scaling we use an average 365-day year to keep it simple & stable.
        public const double WorldDaysPerYearForScaling = 365.0;

        // Season durations expressed in REAL days within the 60-day real-year.
        // You can change these live with [seasoncfg (GM).
        public static int SpringRealDays { get; set; } = 15;
        public static int SummerRealDays { get; set; } = 15;
        public static int AutumnRealDays { get; set; } = 15;
        public static int WinterRealDays { get; set; } = 15;

        // Clock base
        private static DateTime _baseWorld;
        private static DateTime _baseRealUtc;

        public static bool Paused { get; private set; }

        public static void Initialize()
        {
            // If persistence loaded already, it will overwrite these.
            if (_baseWorld == default(DateTime))
            {
                // Default "world start" (change this if you want a different starting date)
                // 260 eh o ano, 12 eh o mes, 14 eh o dia, as 8:00 da manha
                _baseWorld = new DateTime(260, 12, 14, 8, 0, 0);
                _baseRealUtc = DateTime.UtcNow;
            }
        }

        /// <summary>Current world date/time.</summary>
        public static DateTime WorldNow
        {
            get
            {
                // Auto-cura: se a base ainda não foi inicializada direito, inicializa
                if (_baseWorld == default(DateTime) || _baseRealUtc == default(DateTime))
                {
                    Initialize();
                }

                if (Paused)
                    return _baseWorld;

                // Se o baseRealUtc veio quebrado (muito no futuro), reseta ele
                var nowUtc = DateTime.UtcNow;

                if (_baseRealUtc > nowUtc.AddMinutes(5))
                {
                    _baseRealUtc = nowUtc;
                    return _baseWorld;
                }

                // Calcula avanço
                var realDelta = nowUtc - _baseRealUtc;

                // Se por algum motivo o delta ficou absurdo (ex: anos), limita pra não estourar DateTime
                // (isso evita crash mesmo com arquivo de persistência corrompido)
                if (realDelta.TotalDays > 3650) // 10 anos
                {
                    _baseRealUtc = nowUtc;
                    return _baseWorld;
                }

                double worldSeconds = realDelta.TotalSeconds * (WorldDaysPerYearForScaling / RealDaysPerWorldYear);

                // Clamp para não passar do range do DateTime
                double maxAdd = (DateTime.MaxValue - _baseWorld).TotalSeconds;
                double minAdd = (DateTime.MinValue - _baseWorld).TotalSeconds;

                if (worldSeconds > maxAdd) worldSeconds = maxAdd;
                if (worldSeconds < minAdd) worldSeconds = minAdd;

                return _baseWorld.AddSeconds(worldSeconds);
            }
        }

        // ===== Clock control =====

        /// <summary>Sets the current world time (also re-bases real time mapping).</summary>
        public static void SetWorldNow(DateTime newWorldNow)
        {
            _baseWorld = newWorldNow;
            _baseRealUtc = DateTime.UtcNow;
        }

        /// <summary>Adds world time (also re-bases).</summary>
        public static void AddWorldTime(TimeSpan delta)
        {
            SetWorldNow(WorldNow.Add(delta));
        }

        public static void SetPaused(bool paused)
        {
            if (Paused == paused)
                return;

            if (paused)
            {
                // freeze at current computed time
                _baseWorld = WorldNow;
                Paused = true;
            }
            else
            {
                // resume from frozen time
                Paused = false;
                _baseRealUtc = DateTime.UtcNow;
            }
        }

        // ===== Seasons =====

        /// <summary>
        /// Returns current season based on position within the current world year.
        /// Season lengths are configured in REAL days of the 60-day real-year.
        /// </summary>
        public static OSUSeason GetSeason()
        {
            double sp = Math.Max(0, SpringRealDays);
            double su = Math.Max(0, SummerRealDays);
            double au = Math.Max(0, AutumnRealDays);
            double wi = Math.Max(0, WinterRealDays);

            double total = sp + su + au + wi;

            // If total isn't 60, we normalize like this:
            // - If total < 60: leftover goes to winter.
            // - If total > 60: we scale down proportionally.
            if (total <= 0)
            {
                return OSUSeason.Winter;
            }

            if (total < RealDaysPerWorldYear)
            {
                wi += (RealDaysPerWorldYear - total);
                total = RealDaysPerWorldYear;
            }
            else if (total > RealDaysPerWorldYear)
            {
                double scale = RealDaysPerWorldYear / total;
                sp *= scale; su *= scale; au *= scale; wi *= scale;
                total = RealDaysPerWorldYear;
            }

            // Convert real-day durations into fractions of a year (0..1)
            double fSp = sp / RealDaysPerWorldYear;
            double fSu = su / RealDaysPerWorldYear;
            double fAu = au / RealDaysPerWorldYear;
            // winter is the remainder (including any leftover normalization)

            var now = WorldNow;
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0);
            double daysInYear = DateTime.IsLeapYear(now.Year) ? 366.0 : 365.0;

            double progress = (now - startOfYear).TotalDays / daysInYear; // 0..1

            if (progress < fSp) return OSUSeason.Spring;
            if (progress < fSp + fSu) return OSUSeason.Summer;
            if (progress < fSp + fSu + fAu) return OSUSeason.Autumn;
            return OSUSeason.Winter;
        }

        // ===== Persistence helpers (used by OSUWorldTimePersistence) =====
        internal static void SetBase(DateTime baseWorld, DateTime baseRealUtc, bool paused)
        {
            // Se o arquivo de persistência vier quebrado, volta pro default seguro
            if (baseWorld == default(DateTime))
                baseWorld = new DateTime(260, 12, 14, 8, 0, 0);

            if (baseRealUtc == default(DateTime))
                baseRealUtc = DateTime.UtcNow;

            // Se vier no futuro, reseta também
            if (baseRealUtc > DateTime.UtcNow.AddMinutes(5))
                baseRealUtc = DateTime.UtcNow;

            _baseWorld = baseWorld;
            _baseRealUtc = baseRealUtc.Kind == DateTimeKind.Utc ? baseRealUtc : DateTime.SpecifyKind(baseRealUtc, DateTimeKind.Utc);
            Paused = paused;
        }

        internal static void GetBase(out DateTime baseWorld, out DateTime baseRealUtc, out bool paused)
        {
            baseWorld = _baseWorld;
            baseRealUtc = _baseRealUtc;
            paused = Paused;
        }
    }
}
