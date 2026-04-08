using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.Reinos
{
    public enum ReinoMilitaryTab
    {
        Laws = 0,
        Guards = 1,
        Routes = 2,
        Training = 3
    }

    public enum ReinoGuardAction
    {
        None = 0,
        Report = 1,
        Arrest = 2,
        Kill = 3
    }

    public enum ReinoMilitaryLaw
    {
        HoodedWalk = 0,
        Stealing = 1,
        Snooping = 2,
        LootKnockedOut = 3,
        Lockpicking = 4,
        Fighting = 5,
        AnimalTaming = 6,
        AnimalKilling = 7,
        ForeignPlanting = 8,
        ForeignHarvesting = 9,
        DrugUse = 10,
        DrunkWalk = 11,
        TakingFruit = 12,
        FenceJumping = 13,
        ArmedWalk = 14
    }

    public enum ReinoGuardKind
    {
        Vigia = 0,
        Rua = 1,
        Armado = 2,
        Arqueiro = 3,
        CavalariaArmada = 4,
        CavalariaArqueira = 5,
        Oficial = 6
    }

    public enum ReinoRouteSpeed
    {
        Short = 0,
        Medium = 1,
        Long = 2
    }

    public enum ReinoRouteSchedule
    {
        Infinite = 0,
        Every15Minutes = 1,
        Every30Minutes = 2,
        Every45Minutes = 3,
        Every60Minutes = 4,
        DawnOnly = 5
    }

    public sealed class ReinoMilitaryPolicy
    {
        public ReinoGuardAction WantedDefaultAction;
        public ReinoGuardAction CrimeDefaultAction;
        public HashSet<ReinoMilitaryLaw> EnabledLaws;

        public ReinoMilitaryPolicy()
        {
            WantedDefaultAction = ReinoGuardAction.Arrest;
            CrimeDefaultAction = ReinoGuardAction.Report;
            EnabledLaws = new HashSet<ReinoMilitaryLaw>();
        }
    }

    public sealed class ReinoWantedEntry
    {
        public string PlayerName;
        public ReinoGuardAction Action;
        public DateTime AddedUtc;
        public int AddedBySerial;
        public string AddedByName;
    }

    public sealed class ReinoCrimeRecord
    {
        public int CityId;
        public int CriminalSerial;
        public string CriminalName;
        public ReinoMilitaryLaw Law;
        public DateTime Utc;
        public int WitnessGuardSerial;
        public string WitnessGuardName;
        public ReinoGuardAction Result;
        public bool GuardDied;
        public bool CriminalDied;
        public bool CriminalKnockedOut;
        public bool LootStoredInBarracks;
        public bool SentToPrison;
        public string Notes;
    }

    public sealed class ReinoPrisonRecord
    {
        public int CityId;
        public int PrisonerSerial;
        public string PrisonerName;
        public string ArrestedBy;
        public string CrimeLabel;
        public DateTime ArrestUtc;
        public DateTime ReleaseUtc;
        public int DurationHours;
        public string ReleasedBy;
        public bool ReleasedEarly;
        public string Notes;
    }

    public sealed class ReinoMilitaryReportState
    {
        public DateTime LastDeliveredUtc;
        public string LastDeliveredTo;
        public int LastDeliveredToSerial;
    }

    public sealed class ReinoMilitarySession
    {
        public ReinoMilitaryTab Tab;
        public ReinoGuardAction SelectedWantedAction;
        public ReinoGuardKind SelectedGuardKind;
        public int FacingIndex;
        public bool UniformConfirm;
        public int DetailMode;
        public int DetailIndex;
        public int PendingRouteLinkSerial;
        public int PendingRouteRootSerial;
        public int PendingTrainingPage;
        public ReinoRouteSchedule SelectedRouteSchedule;
        public ReinoRouteSpeed SelectedRouteSpeed;
        public bool RestrictToBarracksView;

        public ReinoMilitarySession()
        {
            Tab = ReinoMilitaryTab.Laws;
            SelectedWantedAction = ReinoGuardAction.Kill;
            SelectedGuardKind = ReinoGuardKind.Vigia;
            FacingIndex = 0;
            UniformConfirm = false;
            DetailMode = 0;
            DetailIndex = 0;
            PendingRouteLinkSerial = 0;
            PendingRouteRootSerial = 0;
            PendingTrainingPage = 0;
            SelectedRouteSchedule = ReinoRouteSchedule.Every30Minutes;
            SelectedRouteSpeed = ReinoRouteSpeed.Short;
            RestrictToBarracksView = false;
        }
    }

    public sealed class ReinoGuardPostInfo
    {
        public int Id;
        public int CityId;
        public string ConstructionKey;
        public Point3D Location;
        public int MapIndex;
        public int MarkerSerial;
        public int GuardSerial;
        public ReinoGuardKind GuardKind;
        public int Level;
        public int Facing;
        public bool Uniformized;
        public int RouteRootSerial;
        public ReinoRouteSchedule RouteSchedule;
        public ReinoRouteSpeed RouteSpeed;
        public DateTime LastRouteUtc;
        public bool Training;
        public DateTime TrainingEndsUtc;
        public bool Active;

        public ReinoGuardPostInfo()
        {
            ConstructionKey = String.Empty;
            Level = 1;
            RouteSchedule = ReinoRouteSchedule.Infinite;
            RouteSpeed = ReinoRouteSpeed.Short;
            LastRouteUtc = DateTime.MinValue;
            Active = true;
        }
    }

    public sealed class ReinoPrisonCellDefinition
    {
        public Point3D Offset;

        public ReinoPrisonCellDefinition(int x, int y, int z)
        {
            Offset = new Point3D(x, y, z);
        }
    }
}
