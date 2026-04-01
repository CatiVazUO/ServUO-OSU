using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Reinos
{
    public enum ReinoAreaType
    {
        Kingdom = 0,
        Decorative,
        Wall
    }

    public enum ReinoLotStatus
    {
        Locked = 0,
        Available,
        UnderConstruction,
        Active,
        Abandoned
    }

    public enum ReinoObjectiveType
    {
        None = 0,
        KillMob,
        DeliverVirtualResource
    }

    public enum ReinoResourceType
    {
        None = 0,
        Wood,
        Iron,
        Cloth,
        Gold
    }

    public enum ReinoBuildTargetType
    {
        Lot = 0,
        WallArea
    }

    public enum ReinoPreviewKind
    {
        KingdomArea = 0,
        Lot,
        DecorativeArea,
        WallArea
    }

    public class ReinoAreaDefinition
    {
        public int AreaId;
        public int CityId;
        public Map Map;
        public Rectangle2D Rect;
        public int Z;
        public ReinoAreaType AreaType;
        public int LinkedLotId;
        public string Name;

        public ReinoAreaDefinition()
        {
            Name = String.Empty;
            LinkedLotId = 0;
        }

        public ReinoAreaDefinition(int areaId, int cityId, Map map, Rectangle2D rect, int z, ReinoAreaType areaType, int linkedLotId, string name)
        {
            AreaId = areaId;
            CityId = cityId;
            Map = map;
            Rect = rect;
            Z = z;
            AreaType = areaType;
            LinkedLotId = linkedLotId;
            Name = name ?? String.Empty;
        }

        public bool Contains(Point3D p)
        {
            return Rect.Contains(new Point2D(p.X, p.Y));
        }

        public Point3D GetNorthWestPoint()
        {
            return new Point3D(Rect.Start.X, Rect.Start.Y, Z);
        }

        public Point3D GetCenterPoint(int zOverride)
        {
            return new Point3D(Rect.Start.X + (Rect.Width / 2), Rect.Start.Y + (Rect.Height / 2), zOverride);
        }
    }

    public class ReinoAreaState
    {
        public int AreaId;
        public bool Unlocked;
        public ReinoLotStatus Status;
        public string ConstructionId;
        public int CurrentStageIndex;
        public DateTime NextStageUtc;
        public int MultiSerial;
        public List<int> DoorSerials;
        public List<int> RentalSignSerials;

        public ReinoAreaState()
        {
            ConstructionId = String.Empty;
            DoorSerials = new List<int>();
            RentalSignSerials = new List<int>();
            Status = ReinoLotStatus.Locked;
            CurrentStageIndex = -1;
            NextStageUtc = DateTime.MinValue;
        }

        public ReinoAreaState(int areaId) : this()
        {
            AreaId = areaId;
        }

        public bool IsBuilt
        {
            get { return Status == ReinoLotStatus.Active || Status == ReinoLotStatus.Abandoned; }
        }
    }

    public class ReinoResourceLedger
    {
        public int CityId;
        public int Wood;
        public int Iron;
        public int Cloth;
        public int Gold;

        public ReinoResourceLedger()
        {
        }

        public ReinoResourceLedger(int cityId)
        {
            CityId = cityId;
        }

        public int Get(ReinoResourceType type)
        {
            switch (type)
            {
                case ReinoResourceType.Wood: return Wood;
                case ReinoResourceType.Iron: return Iron;
                case ReinoResourceType.Cloth: return Cloth;
                case ReinoResourceType.Gold: return Gold;
                default: return 0;
            }
        }

        public void Add(ReinoResourceType type, int amount)
        {
            if (amount == 0)
                return;

            switch (type)
            {
                case ReinoResourceType.Wood: Wood += amount; break;
                case ReinoResourceType.Iron: Iron += amount; break;
                case ReinoResourceType.Cloth: Cloth += amount; break;
                case ReinoResourceType.Gold: Gold += amount; break;
            }

            if (Wood < 0) Wood = 0;
            if (Iron < 0) Iron = 0;
            if (Cloth < 0) Cloth = 0;
            if (Gold < 0) Gold = 0;
        }

        public bool Has(ReinoResourceType type, int amount)
        {
            return Get(type) >= amount;
        }

        public string GetDebugLine()
        {
            return String.Format("Madeira: {0} | Ferro: {1} | Tecido: {2} | Moedas: {3}", Wood, Iron, Cloth, Gold);
        }
    }

    public class ReinoObjectiveDefinition
    {
        public ReinoObjectiveType Type;
        public string DisplayName;
        public string[] TargetTypeNames;
        public int RequiredAmount;
        public ReinoResourceType ResourceType;

        public ReinoObjectiveDefinition()
        {
            Type = ReinoObjectiveType.None;
            DisplayName = String.Empty;
            TargetTypeNames = new string[0];
            RequiredAmount = 0;
            ResourceType = ReinoResourceType.None;
        }
    }

    public class ReinoResourceCost
    {
        public ReinoResourceType Type;
        public int Amount;

        public ReinoResourceCost()
        {
        }

        public ReinoResourceCost(ReinoResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    public class ReinoDoorDefinition
    {
        public int X;
        public int Y;
        public int Z;
        public DoorFacing Facing;
        public bool DarkWood;

        public ReinoDoorDefinition()
        {
            DarkWood = true;
        }

        public ReinoDoorDefinition(int x, int y, int z, DoorFacing facing, bool darkWood)
        {
            X = x;
            Y = y;
            Z = z;
            Facing = facing;
            DarkWood = darkWood;
        }
    }

    public class ReinoConstructionDefinition
    {
        public string Id;
        public string Name;
        public int RequiredCityId;
        public ReinoBuildTargetType TargetType;
        public int MinimumLotSide;
        public int[] AllowedLotSides;
        public string DescriptionHtml;
        public ReinoResourceCost[] BuildCosts;
        public ReinoResourceCost[] MaintenanceCosts;
        public int[] StageMultiIds;
        public TimeSpan[] StageDurations;
        public int FinishedMultiId;
        public string FinishedPlacedTypeName;
        public int AbandonedMultiId;
        public string NpcTypeName;
        public Point3D NpcOffset;
        public int NpcZOffset;
        public ReinoDoorDefinition[] FinishedDoors;
        public bool UseMultiDoors;
        public TimeSpan ReactivateDuration;
        public bool Permanent;
        public ReinoRentalTemplate[] RentalTemplates;

        public ReinoConstructionDefinition()
        {
            Id = String.Empty;
            Name = String.Empty;
            RequiredCityId = -1;
            TargetType = ReinoBuildTargetType.Lot;
            MinimumLotSide = 0;
            AllowedLotSides = new int[0];
            DescriptionHtml = String.Empty;
            BuildCosts = new ReinoResourceCost[0];
            MaintenanceCosts = new ReinoResourceCost[0];
            StageMultiIds = new int[0];
            StageDurations = new TimeSpan[0];
            FinishedMultiId = 0;
            FinishedPlacedTypeName = String.Empty;
            AbandonedMultiId = 0;
            NpcTypeName = String.Empty;
            NpcOffset = Point3D.Zero;
            NpcZOffset = 5;
            FinishedDoors = new ReinoDoorDefinition[0];
            UseMultiDoors = true;
            ReactivateDuration = TimeSpan.FromDays(3.0);
            Permanent = false;
            RentalTemplates = new ReinoRentalTemplate[0];
        }

        public bool SupportsLot(ReinoLotDefinition lot)
        {
            if (TargetType != ReinoBuildTargetType.Lot || lot == null)
                return false;

            if (RequiredCityId >= 0 && lot.CityId != RequiredCityId)
                return false;

            if (AllowedLotSides != null && AllowedLotSides.Length > 0)
            {
                for (int i = 0; i < AllowedLotSides.Length; i++)
                {
                    if (AllowedLotSides[i] == lot.Side)
                        return true;
                }

                return false;
            }

            return lot.Side >= MinimumLotSide;
        }

        public bool SupportsArea(ReinoAreaDefinition area)
        {
            if (TargetType != ReinoBuildTargetType.WallArea || area == null || area.AreaType != ReinoAreaType.Wall)
                return false;

            if (RequiredCityId >= 0 && area.CityId != RequiredCityId)
                return false;

            return true;
        }
    }


    public class ReinoRentalRectOffset
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public ReinoRentalRectOffset()
        {
        }

        public ReinoRentalRectOffset(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle2D ToAbsoluteRect(Point3D anchor)
        {
            return new Rectangle2D(anchor.X + X, anchor.Y + Y, Width, Height);
        }
    }

    public class ReinoRentalDoorTemplate
    {
        public int X;
        public int Y;
        public int Z;
        public int ClosedID;
        public int OpenedID;
        public int OpenedSound;
        public int ClosedSound;
        public Point3D Offset;

        public ReinoRentalDoorTemplate()
        {
            Offset = Point3D.Zero;
        }

        public ReinoRentalDoorTemplate(int x, int y, int z, int closedID, int openedID, int openedSound, int closedSound, Point3D offset)
        {
            X = x;
            Y = y;
            Z = z;
            ClosedID = closedID;
            OpenedID = openedID;
            OpenedSound = openedSound;
            ClosedSound = closedSound;
            Offset = offset;
        }
    }

    public class ReinoRentalTemplate
    {
        public string TemplateId;
        public string DisplayName;
        public OSUPropertyType PropertyType;
        public string GroupTag;
        public Point3D SignOffset;
        public Point3D BanLocOffset;
        public ReinoRentalRectOffset[] BlockOffsets;
        public int MinZOffset;
        public int MaxZOffset;
        public int Lockdowns;
        public int Secures;
        public int DefaultPrice;
        public TimeSpan DefaultRentByTime;
        public bool DefaultRecurRent;
        public string DefaultAllowedCulturesCsv;
        public bool GovernorManaged;
        public bool Flip;
        public bool StartConfigured;
        public ReinoRentalDoorTemplate[] DoorTemplates;

        public ReinoRentalTemplate()
        {
            TemplateId = String.Empty;
            DisplayName = String.Empty;
            PropertyType = OSUPropertyType.House;
            GroupTag = "Residential";
            SignOffset = Point3D.Zero;
            BanLocOffset = Point3D.Zero;
            BlockOffsets = new ReinoRentalRectOffset[0];
            MinZOffset = 0;
            MaxZOffset = 20;
            Lockdowns = 125;
            Secures = 4;
            DefaultPrice = 0;
            DefaultRentByTime = TimeSpan.FromDays(7.0);
            DefaultRecurRent = true;
            DefaultAllowedCulturesCsv = "Todos";
            GovernorManaged = true;
            Flip = false;
            StartConfigured = false;
            DoorTemplates = new ReinoRentalDoorTemplate[0];
        }

        public ArrayList BuildAbsoluteBlocks(Point3D anchor)
        {
            ArrayList list = new ArrayList();

            if (BlockOffsets == null)
                return list;

            for (int i = 0; i < BlockOffsets.Length; i++)
            {
                ReinoRentalRectOffset rect = BlockOffsets[i];
                if (rect != null)
                    list.Add(rect.ToAbsoluteRect(anchor));
            }

            return list;
        }
    }

    public class ReinoLotDefinition
    {
        public int LotId;
        public int CityId;
        public Map Map;
        public Point3D NorthWest;
        public int Side;
        public Rectangle2D Rect;
        public string Name;
        public ReinoObjectiveDefinition Objective;

        public ReinoLotDefinition()
        {
            Name = String.Empty;
            Objective = new ReinoObjectiveDefinition();
        }

        public ReinoLotDefinition(int lotId, int cityId, Map map, Point3D northWest, int side)
        {
            LotId = lotId;
            CityId = cityId;
            Map = map;
            NorthWest = northWest;
            Side = side;
            Rect = new Rectangle2D(northWest.X, northWest.Y, side, side);
            Name = String.Format("Lote {0}: {1}x{1}", lotId, side);
            Objective = new ReinoObjectiveDefinition();
        }

        public bool Contains(Point3D p)
        {
            return Rect.Contains(new Point2D(p.X, p.Y));
        }

        public Point3D GetCenter(int z)
        {
            return new Point3D(NorthWest.X + (Side / 2), NorthWest.Y + (Side / 2), z);
        }
    }

    public class ReinoLotState
    {
        public int LotId;
        public ReinoLotStatus Status;
        public int ObjectiveProgress;
        public DateTime AvailableUntilUtc;
        public string ConstructionId;
        public int CurrentStageIndex;
        public DateTime NextStageUtc;
        public DateTime ReactivateReadyUtc;
        public int SignSerial;
        public int MultiSerial;
        public int NpcSerial;
        public List<int> DoorSerials;
        public List<int> RentalSignSerials;

        public ReinoLotState()
        {
            Status = ReinoLotStatus.Locked;
            ObjectiveProgress = 0;
            AvailableUntilUtc = DateTime.MinValue;
            ConstructionId = String.Empty;
            CurrentStageIndex = -1;
            NextStageUtc = DateTime.MinValue;
            ReactivateReadyUtc = DateTime.MinValue;
            DoorSerials = new List<int>();
            RentalSignSerials = new List<int>();
        }

        public ReinoLotState(int lotId) : this()
        {
            LotId = lotId;
        }

        public bool HasConstructionProgress
        {
            get { return Status == ReinoLotStatus.UnderConstruction || Status == ReinoLotStatus.Active || Status == ReinoLotStatus.Abandoned; }
        }

        public bool IsBuilt
        {
            get { return Status == ReinoLotStatus.Active || Status == ReinoLotStatus.Abandoned; }
        }
    }
}
