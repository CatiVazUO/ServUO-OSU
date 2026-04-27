using System;
using Server;

namespace Server.Custom.Systems.Arena
{
    public enum ArenaGameMode
    {
        None = 0,
        LutaLivre,
        Boxe,
        LutaMagica,
        Justa,
        Gladiadores,
        Bomberman
    }

    public class ArenaDefinition
    {
        public string ConstructionId;
        public Point3D ControlOffset;
        public Point3D BilheteriaOffset;
        public Point3D PorteiroOffset;
        public Point3D EntradaOffset;
        public Point3D PublicoTeleportOffset;
        public Point3D EjectOffset;
        public Point3D CenterMultiOffset;

        public int JoustHitMinDx;
        public int JoustHitMaxDx;
        public int JoustHitDy;
        public int BombermanGridStartX;
        public int BombermanGridStartY;
        public int BombermanGridWidth;
        public int BombermanGridHeight;

        public int LutaLivreMultiId;
        public int BoxeMultiId;
        public int LutaMagicaMultiId;
        public int JustaMultiId;
        public int GladiadoresMultiId;
        public int BombermanMultiId;

        public ArenaDefinition()
        {
            ConstructionId = String.Empty;
            ControlOffset = Point3D.Zero;
            BilheteriaOffset = Point3D.Zero;
            PorteiroOffset = Point3D.Zero;
            EntradaOffset = Point3D.Zero;
            PublicoTeleportOffset = Point3D.Zero;
            EjectOffset = Point3D.Zero;
            CenterMultiOffset = Point3D.Zero;
            JoustHitMinDx = -1;
            JoustHitMaxDx = 0;
            JoustHitDy = 1;
            BombermanGridStartX = 2;
            BombermanGridStartY = 2;
            BombermanGridWidth = 27;
            BombermanGridHeight = 27;
        }

        public int GetMultiId(ArenaGameMode mode)
        {
            switch (mode)
            {
                case ArenaGameMode.LutaLivre: return LutaLivreMultiId;
                case ArenaGameMode.Boxe: return BoxeMultiId;
                case ArenaGameMode.LutaMagica: return LutaMagicaMultiId;
                case ArenaGameMode.Justa: return JustaMultiId;
                case ArenaGameMode.Gladiadores: return GladiadoresMultiId;
                case ArenaGameMode.Bomberman: return BombermanMultiId;
                default: return 0;
            }
        }
    }

    public class ArenaState
    {
        public string ConstructionKey;
        public int CityId;
        public ArenaGameMode SelectedMode;
        public bool EventStarted;
        public int CenterMultiSerial;
        public DateTime LastChangedUtc;
        public int TicketSalesGold;
        public int LastEventRevenueGold;

        public ArenaState()
        {
            ConstructionKey = String.Empty;
            SelectedMode = ArenaGameMode.LutaLivre;
            EventStarted = false;
            CenterMultiSerial = 0;
            LastChangedUtc = DateTime.UtcNow;
            TicketSalesGold = 0;
            LastEventRevenueGold = 0;
        }
    }
}
