using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.Systems.Climate
{
    public static class OSUClimateRegions
    {
        // Se true, até "encostar a borda" conta como interseção (mais rígido).
        public static bool CountTouchAsIntersect = false;

        public static readonly List<OSUClimateRegion> Regions = new List<OSUClimateRegion>();

        public static bool TryAddRegion(OSUClimateRegion r, out string error)
        {
            error = null;

            for (int i = 0; i < Regions.Count; i++)
            {
                OSUClimateRegion existing = Regions[i];

                if (r.Intersects(existing, CountTouchAsIntersect))
                {
                    error = "Região '" + r.Name + "' cruza com '" + existing.Name + "'.";
                    return false;
                }
            }

            Regions.Add(r);
            return true;
        }

        public static OSUClimateRegion FindAt(int x, int y, int mapIndex)
        {
            // Se duas regiões cobrirem o mesmo ponto, isso é problema — mas a checagem impede.
            for (int i = 0; i < Regions.Count; i++)
            {
                var r = Regions[i];
                if (r.Contains(x, y, mapIndex))
                    return r;
            }

            return null;
        }

        public static bool RemoveByName(string name)
        {
            for (int i = Regions.Count - 1; i >= 0; i--)
            {
                if (string.Equals(Regions[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    Regions.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static void Clear()
        {
            Regions.Clear();
        }
    }
}
