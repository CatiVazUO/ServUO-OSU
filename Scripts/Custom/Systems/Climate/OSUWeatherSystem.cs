using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.Systems.Climate
{
    // Sistema simples de chuva/neve por região.
    // Usa SEMPRE o WorldNow (OSUClimateTimeAdapter.WorldNow), então [timeadd / [timeset afeta tudo.
    public static class OSUWeatherSystem
    {
        // Você pode ajustar aqui (tempo real) com o server ligado.
        public static TimeSpan UpdateInterval = TimeSpan.FromMinutes(10.0);

        // Duração do clima (em MINUTOS DO MUNDO). Como WorldNow avança, isso acompanha [timeadd.
        public static int MinWeatherDurationMinutes = 30;
        public static int MaxWeatherDurationMinutes = 120;

        // Cache do estado por região
        private static readonly Dictionary<string, RegionWeatherState> _states = new Dictionary<string, RegionWeatherState>(StringComparer.OrdinalIgnoreCase);

        private static Timer _timer;

        public static void Initialize()
        {
            // roda defaults de regiões primeiro (se você usar)
            // (OSUClimateDefaults já tem Initialize também, chamado pelo ScriptCompiler)

            _timer = Timer.DelayCall(TimeSpan.FromSeconds(5.0), UpdateInterval, Tick);
        }

        public static WeatherType GetWeatherAt(int x, int y, int mapIndex)
        {
            OSUClimateRegion region = OSUClimateRegions.FindAt(x, y, mapIndex);
            string key = GetRegionKey(region, mapIndex);

            RegionWeatherState st;
            if (_states.TryGetValue(key, out st))
                return st.Current;

            return WeatherType.None;
        }

        public static void ForceRefreshNow()
        {
            Tick();
        }

        private static void Tick()
        {
            try
            {
                DateTime now = OSUClimateTimeAdapter.WorldNow;

                // garante estados para todas as regiões conhecidas
                for (int i = 0; i < OSUClimateRegions.Regions.Count; i++)
                {
                    OSUClimateRegion r = OSUClimateRegions.Regions[i];
                    UpdateRegionState(r, now);
                }

                // também mantém um estado "default" para áreas sem região
                UpdateRegionState(null, now);
            }
            catch
            {
                // não derruba o server por clima
            }
        }

        private static void UpdateRegionState(OSUClimateRegion region, DateTime now)
        {
            int mapIndex = region != null ? region.MapIndex : 0;
            string key = GetRegionKey(region, mapIndex);

            RegionWeatherState st;
            if (!_states.TryGetValue(key, out st))
            {
                st = new RegionWeatherState();
                _states[key] = st;
            }

            // ainda está valendo?
            if (st.UntilWorldTime > now)
                return;

            // rola um novo clima
            WorldTime.OSUSeason season = OSUClimateTimeAdapter.SeasonNow;

            int baseTemp = 0;
            bool isStatic = false;

            if (region != null)
            {
                baseTemp = region.BaseTemperature;
                isStatic = region.IsStatic;
            }

            int effectiveTemp = baseTemp;
            if (!isStatic)
                effectiveTemp = OSUClimateMath.ApplySeasonToBase(baseTemp, season);

            WeatherType next = RollWeather(effectiveTemp, season);

            st.Current = next;

            int dur = Utility.RandomMinMax(MinWeatherDurationMinutes, MaxWeatherDurationMinutes);
            st.UntilWorldTime = now.AddMinutes(dur);
        }

        private static WeatherType RollWeather(int temp, WorldTime.OSUSeason season)
        {
            // acima de +6 nunca tem nada
            if (temp > 6)
                return WeatherType.None;

            // Neve se temp < 0
            if (temp < 0)
            {
                double baseChance = 0.0;

                // chances por estação
                if (season == WorldTime.OSUSeason.Winter) baseChance = 0.35;
                else if (season == WorldTime.OSUSeason.Autumn) baseChance = 0.10;
                else if (season == WorldTime.OSUSeason.Spring) baseChance = 0.05;
                else baseChance = 0.0; // verão

                // quanto mais frio, mais chance
                double coldFactor = Math.Min(1.0, Math.Abs(temp) / 12.0); // -12 ou menos = 1.0
                double chance = baseChance * (0.5 + (0.5 * coldFactor));

                if (Utility.RandomDouble() < chance)
                    return WeatherType.Snow;

                // se não nevou, pode ficar sem nada
                return WeatherType.None;
            }

            // Chuva se temp entre 0..6
            {
                double baseChance = 0.0;

                if (season == WorldTime.OSUSeason.Spring) baseChance = 0.30;
                else if (season == WorldTime.OSUSeason.Autumn) baseChance = 0.20;
                else if (season == WorldTime.OSUSeason.Winter) baseChance = 0.12;
                else baseChance = 0.08; // verão

                // quanto mais perto de 0 (fresco), mais chance
                double mildFactor = 1.0 - Math.Min(1.0, temp / 6.0); // 0=1.0, 6=0.0
                double chance = baseChance * (0.6 + (0.4 * mildFactor));

                if (Utility.RandomDouble() < chance)
                    return WeatherType.Rain;

                return WeatherType.None;
            }
        }

        private static string GetRegionKey(OSUClimateRegion region, int mapIndex)
        {
            if (region == null)
                return "__DEFAULT__MAP_" + mapIndex;

            return region.Name + "__MAP_" + mapIndex;
        }

        private sealed class RegionWeatherState
        {
            public WeatherType Current = WeatherType.None;
            public DateTime UntilWorldTime = DateTime.MinValue;
        }

        public enum WeatherType
        {
            None = 0,
            Rain = 1,
            Snow = 2
        }
    }
}
