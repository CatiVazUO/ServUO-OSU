
using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.Systems.Espetaculos
{
    public enum EspetaculoVenueType
    {
        Theater = 0,
        Circus
    }

    public enum EspetaculoLightColor
    {
        Blue = 0,
        Red,
        Green,
        Purple,
        White,
        Yellow
    }

    public class EspetaculoSlotDefinition
    {
        public DayOfWeek Day;
        public int Hour;
        public int Minute;
        public string Label;

        public EspetaculoSlotDefinition()
        {
            Day = DayOfWeek.Friday;
            Hour = 20;
            Minute = 0;
            Label = String.Empty;
        }

        public EspetaculoSlotDefinition(DayOfWeek day, int hour, int minute, string label)
        {
            Day = day;
            Hour = hour;
            Minute = minute;
            Label = label ?? String.Empty;
        }
    }

    public class EspetaculoDurationDefinition
    {
        public TimeSpan Duration;
        public int GoldCost;
        public string Label;

        public EspetaculoDurationDefinition()
        {
            Duration = TimeSpan.FromHours(1.0);
            GoldCost = 100;
            Label = "1:00 Hora";
        }

        public EspetaculoDurationDefinition(TimeSpan duration, int goldCost, string label)
        {
            Duration = duration;
            GoldCost = goldCost;
            Label = label ?? String.Empty;
        }
    }

    public class EspetaculoStageLightDefinition
    {
        public Point3D Offset;
        public int ItemId;
        public int Hue;

        public EspetaculoStageLightDefinition()
        {
            Offset = Point3D.Zero;
            ItemId = 0x0A15;
            Hue = 0;
        }

        public EspetaculoStageLightDefinition(int x, int y, int z, int itemId, int hue)
        {
            Offset = new Point3D(x, y, z);
            ItemId = itemId;
            Hue = hue;
        }
    }

    public class EspetaculoSetPieceDefinition
    {
        public string Id;
        public string ClosedLabel;
        public string OpenLabel;
        public Point3D ClosedOffset;
        public Point3D OpenOffset;
        public int ItemId;
        public int Hue;
        public string Name;

        public EspetaculoSetPieceDefinition()
        {
            Id = String.Empty;
            ClosedLabel = String.Empty;
            OpenLabel = String.Empty;
            ClosedOffset = Point3D.Zero;
            OpenOffset = Point3D.Zero;
            ItemId = 0x1224;
            Hue = 0;
            Name = "cenário";
        }

        public EspetaculoSetPieceDefinition(string id, string closedLabel, string openLabel, Point3D closedOffset, Point3D openOffset, int itemId, int hue, string name)
        {
            Id = id ?? String.Empty;
            ClosedLabel = closedLabel ?? String.Empty;
            OpenLabel = openLabel ?? String.Empty;
            ClosedOffset = closedOffset;
            OpenOffset = openOffset;
            ItemId = itemId;
            Hue = hue;
            Name = name ?? "cenário";
        }
    }

    public class EspetaculoDoorDefinition
    {
        public Point3D Offset;
        public int ClosedId;
        public int OpenedId;
        public int OpenedSound;
        public int ClosedSound;
        public Point3D LinkOffset;
        public string Name;

        public EspetaculoDoorDefinition()
        {
            Offset = Point3D.Zero;
            ClosedId = 0x675;
            OpenedId = 0x676;
            OpenedSound = 0xEC;
            ClosedSound = 0xF3;
            LinkOffset = Point3D.Zero;
            Name = "porta";
        }

        public EspetaculoDoorDefinition(Point3D offset, int closedId, int openedId, int openedSound, int closedSound, Point3D linkOffset, string name)
        {
            Offset = offset;
            ClosedId = closedId;
            OpenedId = openedId;
            OpenedSound = openedSound;
            ClosedSound = closedSound;
            LinkOffset = linkOffset;
            Name = name ?? "porta";
        }
    }

    public class EspetaculoVenueDefinition
    {
        public string ConstructionId;
        public string DisplayName;
        public EspetaculoVenueType VenueType;
        public Point3D ControlItemOffset;
        public Point3D EntryTeleportOffset;
        public EspetaculoSlotDefinition[] Slots;
        public EspetaculoDurationDefinition[] Durations;
        public EspetaculoStageLightDefinition[] StageLights;
        public EspetaculoSetPieceDefinition[] SetPieces;
        public EspetaculoDoorDefinition[] Doors;
        public int TicketPriceGold;
        public int TicketItemId;
        public string ReservationHtml;
        public int TicketSellLeadMinutes;

        public EspetaculoVenueDefinition()
        {
            ConstructionId = String.Empty;
            DisplayName = String.Empty;
            VenueType = EspetaculoVenueType.Theater;
            ControlItemOffset = Point3D.Zero;
            EntryTeleportOffset = Point3D.Zero;
            Slots = new EspetaculoSlotDefinition[0];
            Durations = new EspetaculoDurationDefinition[0];
            StageLights = new EspetaculoStageLightDefinition[0];
            SetPieces = new EspetaculoSetPieceDefinition[0];
            Doors = new EspetaculoDoorDefinition[0];
            TicketPriceGold = 10;
            TicketItemId = 0xE17;
            ReservationHtml = String.Empty;
            TicketSellLeadMinutes = 15;
        }
    }

    public class EspetaculoReservation
    {
        public int ReservationId;
        public string ConstructionKey;
        public int CityId;
        public EspetaculoVenueType VenueType;
        public int RenterSerial;
        public string RenterName;
        public DateTime StartLocal;
        public TimeSpan Duration;
        public int RentalCostGold;
        public bool NoticeSent;
        public bool Started;
        public bool Ended;
        public List<int> AdmittedPlayerSerials;

        public EspetaculoReservation()
        {
            ConstructionKey = String.Empty;
            CityId = -1;
            VenueType = EspetaculoVenueType.Theater;
            RenterSerial = 0;
            RenterName = String.Empty;
            StartLocal = DateTime.MinValue;
            Duration = TimeSpan.Zero;
            RentalCostGold = 0;
            NoticeSent = false;
            Started = false;
            Ended = false;
            AdmittedPlayerSerials = new List<int>();
        }

        public DateTime EndLocal
        {
            get { return StartLocal + Duration; }
        }

        public bool IsActive(DateTime now)
        {
            return Started && !Ended && now >= StartLocal && now < EndLocal;
        }

        public bool HasStarted(DateTime now)
        {
            return now >= StartLocal;
        }

        public bool HasEnded(DateTime now)
        {
            return now >= EndLocal;
        }

        public bool ContainsAdmitted(int serial)
        {
            return AdmittedPlayerSerials != null && AdmittedPlayerSerials.Contains(serial);
        }

        public void Admit(int serial)
        {
            if (AdmittedPlayerSerials == null)
                AdmittedPlayerSerials = new List<int>();

            if (!AdmittedPlayerSerials.Contains(serial))
                AdmittedPlayerSerials.Add(serial);
        }
    }

    public class EspetaculoVenueState
    {
        public string ConstructionKey;
        public int CityId;
        public EspetaculoVenueType VenueType;
        public int ControlItemSerial;
        public List<int> StageLightSerials;
        public List<int> SetPieceSerials;
        public List<int> DoorSerials;
        public List<EspetaculoReservation> Reservations;
        public bool AudienceLightsDimmed;
        public EspetaculoLightColor SelectedLightColor;
        public bool StageLightsOn;
        public bool SetPieceState1;
        public bool SetPieceState2;
        public Dictionary<int, int> LightRestoreLevels;

        public EspetaculoVenueState()
        {
            ConstructionKey = String.Empty;
            CityId = -1;
            VenueType = EspetaculoVenueType.Theater;
            ControlItemSerial = 0;
            StageLightSerials = new List<int>();
            SetPieceSerials = new List<int>();
            DoorSerials = new List<int>();
            Reservations = new List<EspetaculoReservation>();
            AudienceLightsDimmed = false;
            SelectedLightColor = EspetaculoLightColor.Blue;
            StageLightsOn = false;
            SetPieceState1 = false;
            SetPieceState2 = false;
            LightRestoreLevels = new Dictionary<int, int>();
        }
    }

    public class EspetaculoSlotOption
    {
        public int SlotIndex;
        public DateTime StartLocal;
        public string Label;
        public bool Available;

        public EspetaculoSlotOption()
        {
            SlotIndex = -1;
            StartLocal = DateTime.MinValue;
            Label = String.Empty;
            Available = false;
        }
    }
}
