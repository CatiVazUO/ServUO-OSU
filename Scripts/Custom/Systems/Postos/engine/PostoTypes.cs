using System;

namespace Server.Custom.Systems.Postos
{
    public enum PostoResourceType
    {
        None = 0,
        Iron,
        Wood,
        Cotton
    }

    public enum PostoSize
    {
        Small = 0,
        Large = 1
    }

    public enum PostoObjectiveType
    {
        None = 0,
        KillMob,
        DestroyItem
    }

    public enum PostoActionType
    {
        None = 0,
        AcceptAgreement,
        Conquer
    }

    public class PostoState
    {
        public string PostoId;
        public string OwnerCityId;
        public string ProgressCityId;
        public int ProgressValue;
        public int StoredAmount;
        public DateTime LastProductionUtc;
        public DateTime ProtectedUntilUtc;

        public PostoState(string postoId)
        {
            PostoId = postoId ?? String.Empty;
            OwnerCityId = String.Empty;
            ProgressCityId = String.Empty;
            ProgressValue = 0;
            StoredAmount = 0;
            LastProductionUtc = DateTime.MinValue;
            ProtectedUntilUtc = DateTime.MinValue;
        }
    }

    public class PostoKingdomResourceLedger
    {
        public string CityId;
        public int Iron;
        public int Wood;
        public int Cotton;

        public PostoKingdomResourceLedger(string cityId)
        {
            CityId = cityId ?? String.Empty;
        }

        public int Get(PostoResourceType type)
        {
            switch (type)
            {
                case PostoResourceType.Iron:
                    return Iron;
                case PostoResourceType.Wood:
                    return Wood;
                case PostoResourceType.Cotton:
                    return Cotton;
                default:
                    return 0;
            }
        }

        public void Add(PostoResourceType type, int amount)
        {
            if (amount <= 0)
                return;

            switch (type)
            {
                case PostoResourceType.Iron:
                    Iron += amount;
                    break;
                case PostoResourceType.Wood:
                    Wood += amount;
                    break;
                case PostoResourceType.Cotton:
                    Cotton += amount;
                    break;
            }
        }
    }
}
