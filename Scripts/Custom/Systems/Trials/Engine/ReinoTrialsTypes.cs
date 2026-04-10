using System;
using Server;

namespace Server.Custom.Reinos
{
    public enum ReinoTribunalEscortAction
    {
        None = 0,
        Expel = 1,
        PrisonForContempt = 2
    }

    public sealed class ReinoTrialSession
    {
        public int CityId;
        public bool SessionActive;
        public int AccusedSerial;
        public string AccusedName;
        public int PendingSentenceDays;
        public int PendingFineGold;

        public ReinoTrialSession()
        {
            AccusedName = String.Empty;
        }
    }

    public sealed class ReinoTrialVerdict
    {
        public int CityId;
        public int PrisonerSerial;
        public string PrisonerName;
        public int JudgeSerial;
        public string JudgeName;
        public string CrimeLabel;
        public int DurationHours;
        public int FineGold;
        public DateTime DeclaredUtc;
        public string Notes;

        public ReinoTrialVerdict()
        {
            PrisonerName = String.Empty;
            JudgeName = String.Empty;
            CrimeLabel = String.Empty;
            Notes = String.Empty;
            DeclaredUtc = DateTime.UtcNow;
        }
    }

    public sealed class ReinoTrialLawRule
    {
        public int CityId;
        public ReinoMilitaryLaw Law;
        public bool HasCustomValues;
        public int SentenceHours;
        public int FineGold;
        public int LastChangedBySerial;
        public string LastChangedByName;
        public DateTime LastChangedUtc;

        public ReinoTrialLawRule()
        {
            SentenceHours = 48;
            FineGold = 5000;
            LastChangedByName = String.Empty;
            LastChangedUtc = DateTime.MinValue;
        }
    }
}
