using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.Health
{
    public class OSUInjuryState
    {
        public OSUInjuryType Type;
        public OSUInjurySeverity Severity;
        public DateTime StartedUtc;
        public DateTime EndsUtc;
        public bool RequiresSurgery;
        public bool Cured;
    }

    public class OSUDiseaseState
    {
        public OSUDiseaseType Type;
        public DateTime ContractedUtc;
        public DateTime IncubationEndsUtc;
        public DateTime NextPulseUtc;
        public int RecoveryCount;
        public bool Cured;
    }

    public class OSUImmunityState
    {
        public OSUDiseaseType Disease;
        public string SourceId;
        public double ReductionScalar;
        public DateTime EndsUtc;
    }

    public class OSUContaminatedItemState
    {
        public int ItemSerial;
        public OSUDiseaseType Disease;
        public DateTime ExpiresUtc;
        public string SourceLabel;
    }

    public class OSUSurgeryProgressState
    {
        public int PatientSerial;
        public int SurgeonSerial;
        public int SourceCityId;
        public string SourceConstructionKey;
        public OSUInjuryType Injury;

        public int Cut;
        public int Sew;
        public int Heat;
        public int Cool;
        public int Bleed;

        public int TargetCutMin;
        public int TargetCutMax;
        public int TargetHeatMin;
        public int TargetHeatMax;
        public int TargetCoolMin;
        public int TargetCoolMax;
        public int TargetBleedMin;
        public int TargetBleedMax;

        public bool AllowCut;
        public bool AllowHeat;
        public bool AllowCool;
        public bool AllowBleed;
        public bool RequiresOpeningCut;
        public bool OpeningCutDone;
        public bool Anesthetized;
        public bool SewingFinished;

        public int ConditionScore;
        public string StatusText;
        public DateTime StartedUtc;
        public DateTime DeadlineUtc;
        public DateTime LastActionUtc;
    }

    public class OSUHealthProfile
    {
        public int MobileSerial;
        public List<OSUInjuryState> Injuries = new List<OSUInjuryState>();
        public List<OSUDiseaseState> Diseases = new List<OSUDiseaseState>();
        public List<OSUImmunityState> Immunities = new List<OSUImmunityState>();
        public bool DeadlyLocked;
        public DateTime DeadlyDeadlineUtc;
        public int PortableStretcherSerial;
        public int HospitalStretcherSerial;
        public int LastCarrierSerial;
        public DateTime SurgeryBlockedUntilUtc;
        public DateTime ComaUntilUtc;
        public int SurgeryStretcherSerial;
        public int SurgeryFailureCount;
    }
}
