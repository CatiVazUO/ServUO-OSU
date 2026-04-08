using System;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public enum ReinoDiplomacyRelationStatus
    {
        Neutral = 0,
        Allied = 1,
        Enemy = 2,
        War = 3
    }

    public enum ReinoDiplomacyActionKind
    {
        None = 0,
        ChangeRelation,
        DonateResources,
        DonatePosto,
        ProposeAgreement,
        CancelAgreement,
        CloseBorders,
        CommercialBlockade,
        DemandTribute
    }

    public enum ReinoDiplomacyApprovalCategory
    {
        General = 0,
        Economy = 1,
        Defense = 2
    }

    public enum ReinoDiplomacyRequestState
    {
        PendingSourceApproval = 0,
        PendingTargetApproval = 1,
        Approved = 2,
        Rejected = 3,
        Expired = 4
    }

    public enum ReinoDiplomacyTributeFrequency
    {
        Once = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    public class ReinoDiplomacyVote
    {
        public int VoterSerial;
        public string VoterName;
        public int Decision;
        public DateTime DecisionUtc;

        public ReinoDiplomacyVote()
        {
            VoterName = String.Empty;
        }
    }

    public class ReinoDiplomacyResourceBundle
    {
        public int Gold;
        public int Wood;
        public int Iron;
        public int Cloth;

        public ReinoDiplomacyResourceBundle()
        {
        }

        public ReinoDiplomacyResourceBundle(int gold, int wood, int iron, int cloth)
        {
            Gold = Math.Max(0, gold);
            Wood = Math.Max(0, wood);
            Iron = Math.Max(0, iron);
            Cloth = Math.Max(0, cloth);
        }

        public bool IsEmpty
        {
            get { return Gold <= 0 && Wood <= 0 && Iron <= 0 && Cloth <= 0; }
        }

        public ReinoDiplomacyResourceBundle Clone()
        {
            return new ReinoDiplomacyResourceBundle(Gold, Wood, Iron, Cloth);
        }
    }

    public class ReinoDiplomacyBorderPolicy
    {
        public int SourceCityId;
        public int TargetCityId;
        public bool BlockEnemyCitizens;
        public bool BlockEnemyCulture;
        public bool BlockEnemyAllies;
        public bool AllowEntry;

        public ReinoDiplomacyBorderPolicy Clone()
        {
            return (ReinoDiplomacyBorderPolicy)MemberwiseClone();
        }
    }

    public class ReinoDiplomacyCommercialBlockade
    {
        public int SourceCityId;
        public int TargetCityId;
        public bool BlockRepresentative;
        public bool CancelAgreements;
        public bool CancelDonations;
        public bool BlockPlayerVendors;

        public ReinoDiplomacyCommercialBlockade Clone()
        {
            return (ReinoDiplomacyCommercialBlockade)MemberwiseClone();
        }
    }

    public class ReinoDiplomacyAgreement
    {
        public int SourceCityId;
        public int TargetCityId;
        public ReinoDiplomacyResourceBundle SendFromSource;
        public ReinoDiplomacyResourceBundle SendFromTarget;
        public DateTime CreatedUtc;
        public DateTime NextRunUtc;
        public bool GraceActive;
        public DateTime GraceEndsUtc;
        public int GraceDebtorCityId;

        public ReinoDiplomacyAgreement()
        {
            SendFromSource = new ReinoDiplomacyResourceBundle();
            SendFromTarget = new ReinoDiplomacyResourceBundle();
        }
    }

    public class ReinoDiplomacyTribute
    {
        public int DemandingCityId;
        public int PayingCityId;
        public ReinoDiplomacyResourceBundle Bundle;
        public ReinoDiplomacyTributeFrequency Frequency;
        public DateTime CreatedUtc;
        public DateTime NextRunUtc;

        public ReinoDiplomacyTribute()
        {
            Bundle = new ReinoDiplomacyResourceBundle();
        }
    }

    public class ReinoDiplomacyNotice
    {
        public int NoticeId;
        public int TargetSerial;
        public string Title;
        public string Html;
        public bool Closable;
        public bool Consumed;
        public DateTime CreatedUtc;

        public ReinoDiplomacyNotice()
        {
            Title = String.Empty;
            Html = String.Empty;
            Closable = true;
        }
    }


    public class ReinoDiplomacyWarCitizenWarning
    {
        public int PlayerSerial;
        public int ForeignCitizenCityId;
        public int CapitalCityId;
        public DateTime StartedUtc;
    }


    public class ReinoDiplomacyRequest
    {
        public int RequestId;
        public ReinoDiplomacyActionKind Action;
        public ReinoDiplomacyApprovalCategory Category;
        public int SourceCityId;
        public int TargetCityId;
        public int CreatedBySerial;
        public string CreatedByName;
        public DateTime CreatedUtc;
        public DateTime ResolvedUtc;
        public DateTime ExpiresUtc;
        public ReinoDiplomacyRequestState State;
        public bool RequiresTargetDecision;
        public string SourceTitle;
        public string SourceHtml;
        public string TargetTitle;
        public string TargetHtml;
        public string NoticeTitle;
        public string NoticeHtml;
        public ReinoDiplomacyRelationStatus OldRelation;
        public ReinoDiplomacyRelationStatus NewRelation;
        public ReinoDiplomacyResourceBundle ResourceBundle;
        public string PostoId;
        public ReinoDiplomacyResourceBundle AgreementSourceSend;
        public ReinoDiplomacyResourceBundle AgreementTargetSend;
        public ReinoDiplomacyBorderPolicy BorderPolicy;
        public ReinoDiplomacyCommercialBlockade BlockadePolicy;
        public ReinoDiplomacyTribute Tribute;
        public List<ReinoDiplomacyVote> SourceVotes;
        public List<ReinoDiplomacyVote> TargetVotes;

        public ReinoDiplomacyRequest()
        {
            CreatedByName = String.Empty;
            SourceTitle = String.Empty;
            SourceHtml = String.Empty;
            TargetTitle = String.Empty;
            TargetHtml = String.Empty;
            NoticeTitle = String.Empty;
            NoticeHtml = String.Empty;
            PostoId = String.Empty;
            ResourceBundle = new ReinoDiplomacyResourceBundle();
            AgreementSourceSend = new ReinoDiplomacyResourceBundle();
            AgreementTargetSend = new ReinoDiplomacyResourceBundle();
            BorderPolicy = new ReinoDiplomacyBorderPolicy();
            BlockadePolicy = new ReinoDiplomacyCommercialBlockade();
            Tribute = new ReinoDiplomacyTribute();
            SourceVotes = new List<ReinoDiplomacyVote>();
            TargetVotes = new List<ReinoDiplomacyVote>();
        }

        public bool IsPending
        {
            get
            {
                return State == ReinoDiplomacyRequestState.PendingSourceApproval || State == ReinoDiplomacyRequestState.PendingTargetApproval;
            }
        }
    }

    public class ReinoDiplomacySession
    {
        public int CityId;
        public int TargetCityId;
        public ReinoDiplomacyRelationStatus? DraftRelation;
        public ReinoDiplomacyActionKind SelectedAction;
        public string SelectedPostoId;
        public ReinoDiplomacyResourceBundle DraftDonation;
        public ReinoDiplomacyResourceBundle DraftAgreementSend;
        public ReinoDiplomacyResourceBundle DraftAgreementReceive;
        public ReinoDiplomacyBorderPolicy DraftBorders;
        public ReinoDiplomacyCommercialBlockade DraftBlockade;
        public ReinoDiplomacyResourceBundle DraftTribute;
        public ReinoDiplomacyTributeFrequency DraftTributeFrequency;

        public ReinoDiplomacySession()
        {
            TargetCityId = -1;
            DraftRelation = null;
            SelectedAction = ReinoDiplomacyActionKind.None;
            SelectedPostoId = String.Empty;
            DraftDonation = new ReinoDiplomacyResourceBundle();
            DraftAgreementSend = new ReinoDiplomacyResourceBundle();
            DraftAgreementReceive = new ReinoDiplomacyResourceBundle();
            DraftBorders = new ReinoDiplomacyBorderPolicy();
            DraftBlockade = new ReinoDiplomacyCommercialBlockade();
            DraftTribute = new ReinoDiplomacyResourceBundle();
            DraftTributeFrequency = ReinoDiplomacyTributeFrequency.Once;
        }
    }
}
