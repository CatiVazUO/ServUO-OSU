using System;
using System.Collections.Generic;
using Server;
//using Server.Maps;
using Server.Regions;

namespace Server.Custom.Systems.WorldTime
{
    /// <summary>
    /// Simple climate layer (step 1):
    /// - Define surface temperature regions with a base temperature (-6..+6)
    /// - Compute effective temperature from base + season rules
    /// - Day/Night helper based on WorldNow hour
    ///
    /// Next steps (later): rain/snow chances, comfort/gear, penalties.
    /// </summary>
    public static class OSUClimate
    {
        // ===== Day/Night (WorldNow-based) =====
        public static int NightStartsHour { get; set; } = 20; // 20:00
        public static int NightEndsHour { get; set; } = 6;    // 06:00

        public static bool IsNight()
        {
            int h = OSUWorldTime.WorldNow.Hour;
            // night if >= start OR < end (wraps midnight)
            return (h >= NightStartsHour) || (h < NightEndsHour);
        }

        // ===== Temperature regions =====

        /// <summary>
        /// A temperature band/region (surface).
        /// You will create a few rectangles (north cold, south hot, etc.).
        /// </summary>
        public class TempRegionDef
        {
            public string Name;
            public Map Map;
            public Rectangle2D Area;
            public int BaseTemp; // recommended -6..+6 (but we allow -16..+16)

            // If true, this region ignores seasons (for dungeons later).
            public bool StaticClimate;

            public TempRegionDef(string name, Map map, Rectangle2D area, int baseTemp, bool staticClimate)
            {
                Name = name;
                Map = map;
                Area = area;
                BaseTemp = baseTemp;
                StaticClimate = staticClimate;
            }

            public bool Contains(Map map, Point3D loc)
            {
                if (map == null || Map == null)
                    return false;

                if (map != Map)
                    return false;

                return Area.Contains(loc);
            }
        }

        private static readonly List<TempRegionDef> _regions = new List<TempRegionDef>();

        public static IEnumerable<TempRegionDef> Regions { get { return _regions; } }

        public static void AddOrReplaceRegion(TempRegionDef def)
        {
            if (def == null)
                return;

            // replace by name
            for (int i = 0; i < _regions.Count; i++)
            {
                if (string.Equals(_regions[i].Name, def.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _regions[i] = def;
                    return;
                }
            }

            _regions.Add(def);
        }

        public static bool RemoveRegion(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            for (int i = 0; i < _regions.Count; i++)
            {
                if (string.Equals(_regions[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    _regions.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static TempRegionDef GetRegionAt(Map map, Point3D loc)
        {
            // last-added wins (so you can override with smaller rectangles)
            for (int i = _regions.Count - 1; i >= 0; i--)
            {
                var r = _regions[i];
                if (r != null && r.Contains(map, loc))
                    return r;
            }

            return null;
        }

        /// <summary>
        /// Returns base temperature for a location. If no region, returns 0.
        /// </summary>
        public static int GetBaseTemperature(Map map, Point3D loc)
        {
            var r = GetRegionAt(map, loc);
            return r != null ? r.BaseTemp : 0;
        }

        /// <summary>
        /// Season-adjusted temperature for a location (clamped -16..+16).
        /// Implements the math you described:
        /// - Spring/Autumn: base unchanged
        /// - Winter:
        ///     base < 0 => base * 2
        ///     base == 0 => -1
        ///     base > 0 => base - 2
        /// - Summer:
        ///     base > 0 => base * 2
        ///     base == 0 => +1
        ///     base < 0 => base + 2 (example: -1=>+1, -2=>0, -3=>-1, -4=>-2, etc.)
        /// </summary>
        public static int GetEffectiveTemperature(Map map, Point3D loc)
        {
            var r = GetRegionAt(map, loc);
            int baseTemp = r != null ? r.BaseTemp : 0;

            if (r != null && r.StaticClimate)
                return Clamp(baseTemp, -16, 16);

            OSUSeason s = OSUWorldTime.GetSeason();

            int t = baseTemp;

            if (s == OSUSeason.Winter)
            {
                if (baseTemp < 0)
                    t = baseTemp * 2;
                else if (baseTemp == 0)
                    t = -1;
                else
                    t = baseTemp - 2;
            }
            else if (s == OSUSeason.Summer)
            {
                if (baseTemp > 0)
                    t = baseTemp * 2;
                else if (baseTemp == 0)
                    t = 1;
                else
                    t = baseTemp + 2;
            }
            else
            {
                // Spring/Autumn: no change
                t = baseTemp;
            }

            // Your rule: above +6 has no rain/snow later; we keep temp as computed,
            // clamped to your global range for future penalty logic.
            return Clamp(t, -16, 16);
        }

        private static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}
