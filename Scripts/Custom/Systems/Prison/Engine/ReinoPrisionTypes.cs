using System;
using Server;

namespace Server.Custom.Reinos
{
    public sealed class ReinoPrisionSettings
    {
        public int CityId;
        public bool FeedPrisoners;
        public bool AllowFinePayment;
        public bool OuterDoorsLocked;
        public int[] CellDoorSerials;
        public DateTime LastDailyChargeLocalDate;
        public int PendingWeeklyGold;

        public ReinoPrisionSettings()
        {
            CellDoorSerials = new int[5];
            LastDailyChargeLocalDate = DateTime.MinValue;
        }
    }

    public sealed class ReinoPrisionerState
    {
        public int CityId;
        public int PrisonerSerial;
        public string PrisonerName;
        public string CrimeLabel;
        public DateTime ArrestUtc;
        public DateTime ReleaseUtc;
        public int SentenceHours;
        public int CellIndex;
        public bool InInterrogation;
        public bool InTribunal;
        public string JudgeName;
        public bool Judged;
        public DateTime JudgedUtc;
        public int FineGold;
        public bool FinePaid;
        public bool FineGumpShown;
        public int BelongingsBagSerial;
        public string Notes;
        public bool ReleasePending;
        public bool ReleasePendingToBank;
        public int SourceLawId;

        public ReinoPrisionerState()
        {
            PrisonerName = String.Empty;
            CrimeLabel = String.Empty;
            JudgeName = String.Empty;
            Notes = String.Empty;
            CellIndex = -1;
            JudgedUtc = DateTime.MinValue;
            SourceLawId = -1;
        }
    }

    public sealed class ReinoPrisionSession
    {
        public int ViewedCellIndex;
        public int PendingRemainingHours;

        public ReinoPrisionSession()
        {
            ViewedCellIndex = 0;
            PendingRemainingHours = -1;
        }
    }
}
