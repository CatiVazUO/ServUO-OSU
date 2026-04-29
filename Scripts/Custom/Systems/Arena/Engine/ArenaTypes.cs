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

    public class ArenaDoorOffset
    {
        public Point3D Offset;
        public bool IsEntryDoor;

        public ArenaDoorOffset() { Offset = Point3D.Zero; IsEntryDoor = false; }
        public ArenaDoorOffset(int x, int y, int z, bool isEntryDoor) { Offset = new Point3D(x, y, z); IsEntryDoor = isEntryDoor; }
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
        public Point3D BombermanStorageOffset;
        public ArenaDoorOffset[] Doors;
        public Point3D JoustKnight1Offset;
        public Point3D JoustKnight2Offset;
        public Direction JoustDirectionForward;
        public Point3D[] GladiatorSpawnOffsets;
        public Point3D[] BombermanRedSpawnOffsets;
        public Point3D[] BombermanBlueSpawnOffsets;

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
            BombermanStorageOffset = Point3D.Zero;
            Doors = new ArenaDoorOffset[0];
            JoustKnight1Offset = new Point3D(5, 14, 0);
            JoustKnight2Offset = new Point3D(5, 15, 0);
            JoustDirectionForward = Direction.East;
            GladiatorSpawnOffsets = new Point3D[]
            {
                new Point3D(15, 1, 0),
                new Point3D(28, 15, 0),
                new Point3D(15, 28, 0),
                new Point3D(1, 15, 0)
            };
            BombermanRedSpawnOffsets = new Point3D[]
            {
                new Point3D(2, 2, 0),
                new Point3D(2, 27, 0)
            };
            BombermanBlueSpawnOffsets = new Point3D[]
            {
                new Point3D(27, 2, 0),
                new Point3D(27, 27, 0)
            };
            JoustHitMinDx = -1;
            JoustHitMaxDx = 0;
            JoustHitDy = 1;
            BombermanGridStartX = 2;
            BombermanGridStartY = 2;
            BombermanGridWidth = 20;
            BombermanGridHeight = 20;
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
        public int[] DoorSerials;
        public int StorageChestSerial;

        public ArenaState()
        {
            ConstructionKey = String.Empty;
            SelectedMode = ArenaGameMode.LutaLivre;
            EventStarted = false;
            CenterMultiSerial = 0;
            LastChangedUtc = DateTime.UtcNow;
            DoorSerials = new int[0];
            StorageChestSerial = 0;
        }
    }
}
