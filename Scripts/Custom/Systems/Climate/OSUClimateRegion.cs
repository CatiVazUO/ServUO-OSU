using System;

namespace Server.Custom.Systems.Climate
{
    public class OSUClimateRegion
    {
        public string Name { get; private set; }
        public int BaseTemperature { get; set; }     // ex: -3, 0, +4
        public bool IsStatic { get; set; }          // true = não muda por estação (dungeon)
        public int MapIndex { get; private set; }   // 0=Felucca, 1=Trammel etc

        public int X1 { get; private set; }
        public int Y1 { get; private set; }
        public int X2 { get; private set; }
        public int Y2 { get; private set; }

        public OSUClimateRegion(string name, int baseTemp, bool isStatic, int mapIndex, int x1, int y1, int x2, int y2)
        {
            Name = name;
            BaseTemperature = baseTemp;
            IsStatic = isStatic;
            MapIndex = mapIndex;

            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
        }

        public bool Contains(int x, int y, int mapIndex)
        {
            if (mapIndex != MapIndex)
                return false;

            int minX = Math.Min(X1, X2);
            int maxX = Math.Max(X1, X2);
            int minY = Math.Min(Y1, Y2);
            int maxY = Math.Max(Y1, Y2);

            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }

        // Para impedir "regiões se intercalarem"
        public bool Intersects(OSUClimateRegion other, bool countTouchAsIntersect)
        {
            if (other == null) return false;
            if (other.MapIndex != MapIndex) return false;

            int ax1 = Math.Min(X1, X2);
            int ax2 = Math.Max(X1, X2);
            int ay1 = Math.Min(Y1, Y2);
            int ay2 = Math.Max(Y1, Y2);

            int bx1 = Math.Min(other.X1, other.X2);
            int bx2 = Math.Max(other.X1, other.X2);
            int by1 = Math.Min(other.Y1, other.Y2);
            int by2 = Math.Max(other.Y1, other.Y2);

            if (countTouchAsIntersect)
            {
                return ax1 <= bx2 && ax2 >= bx1 && ay1 <= by2 && ay2 >= by1;
            }
            else
            {
                return ax1 < bx2 && ax2 > bx1 && ay1 < by2 && ay2 > by1;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} (Map {1}) BaseTemp={2} Static={3} [{4},{5}] -> [{6},{7}]",
                Name, MapIndex, BaseTemperature, IsStatic, X1, Y1, X2, Y2);
        }
    }
}
