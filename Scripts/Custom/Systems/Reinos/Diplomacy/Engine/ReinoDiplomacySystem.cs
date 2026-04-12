using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;
using Server.Custom.Systems.Rent;
using Server.Custom.Systems.Postos;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Culture;
using Server.Custom.Biblioteca.Library;

namespace Server.Custom.Reinos
{
    public static class ReinoDiplomacySystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoDiplomacy.bin");

        private static readonly Dictionary<string, ReinoDiplomacyRelationStatus> m_Relations = new Dictionary<string, ReinoDiplomacyRelationStatus>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ReinoDiplomacyBorderPolicy> m_Borders = new Dictionary<string, ReinoDiplomacyBorderPolicy>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ReinoDiplomacyCommercialBlockade> m_Blockades = new Dictionary<string, ReinoDiplomacyCommercialBlockade>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ReinoDiplomacyAgreement> m_Agreements = new Dictionary<string, ReinoDiplomacyAgreement>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ReinoDiplomacyTribute> m_Tributes = new Dictionary<string, ReinoDiplomacyTribute>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<ReinoDiplomacyRequest> m_Requests = new List<ReinoDiplomacyRequest>();
        private static readonly List<ReinoDiplomacyNotice> m_Notices = new List<ReinoDiplomacyNotice>();
        private static readonly List<ReinoDiplomacyWarCitizenWarning> m_WarCitizenWarnings = new List<ReinoDiplomacyWarCitizenWarning>();
        private static readonly Dictionary<int, ReinoDiplomacySession> m_Sessions = new Dictionary<int, ReinoDiplomacySession>();

        private static int m_NextRequestId = 1;
        private static int m_NextNoticeId = 1;

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };
            EventSink.Login += OnLogin;
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(1.0), Pulse);
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            try
            {
                HandleWarCitizenLogin(pm);
                ShowPendingGump(pm);
            }
            catch
            {
            }
        }

        private static void Pulse()
        {
            try
            {
                ProcessPendingRequests();
                ProcessAgreements();
                ProcessTributes();
                ProcessWarCitizenWarnings();
            }
            catch
            {
            }
        }

        private static DateTime GetLocalNow()
        {
            return DateTime.UtcNow.AddHours(-3.0);
        }

        private static DateTime GetNextMonday19Utc(DateTime fromUtc)
        {
            DateTime local = fromUtc.AddHours(-3.0);
            int mondayOffset = ((int)local.DayOfWeek + 6) % 7;
            DateTime monday = local.Date.AddDays(-mondayOffset).AddHours(19.0);

            if (local >= monday)
                monday = monday.AddDays(7.0);

            return monday.AddHours(3.0);
        }

        private static DateTime GetNextTuesday19Utc(DateTime fromUtc)
        {
            DateTime monday = GetNextMonday19Utc(fromUtc).AddDays(-7.0);
            DateTime localMonday = monday.AddHours(-3.0);
            DateTime localTuesday = localMonday.AddDays(1.0);

            if (fromUtc < monday)
                localTuesday = localTuesday.AddDays(-7.0);

            if (fromUtc >= localTuesday.AddHours(3.0))
                localTuesday = localTuesday.AddDays(7.0);

            return localTuesday.AddHours(3.0);
        }

        private static DateTime GetFirstTributeRunUtc(ReinoDiplomacyTributeFrequency frequency)
        {
            DateTime now = DateTime.UtcNow;

            switch (frequency)
            {
                case ReinoDiplomacyTributeFrequency.Daily:
                    return now.AddDays(1.0);
                case ReinoDiplomacyTributeFrequency.Weekly:
                    return now.AddDays(7.0);
                case ReinoDiplomacyTributeFrequency.Monthly:
                    return now.AddDays(30.0);
                default:
                    return now.AddHours(24.0);
            }
        }

        private static DateTime AdvanceTributeRunUtc(DateTime currentUtc, ReinoDiplomacyTributeFrequency frequency)
        {
            switch (frequency)
            {
                case ReinoDiplomacyTributeFrequency.Daily:
                    return currentUtc.AddDays(1.0);
                case ReinoDiplomacyTributeFrequency.Weekly:
                    return currentUtc.AddDays(7.0);
                case ReinoDiplomacyTributeFrequency.Monthly:
                    return currentUtc.AddDays(30.0);
                default:
                    return DateTime.MaxValue;
            }
        }

        private static string PairKey(int a, int b)
        {
            if (a <= b)
                return a.ToString() + ":" + b.ToString();

            return b.ToString() + ":" + a.ToString();
        }

        private static string DirectionKey(int sourceCityId, int targetCityId)
        {
            return sourceCityId.ToString() + ">" + targetCityId.ToString();
        }

        public static List<int> GetOtherCityIds(int cityId)
        {
            List<int> list = new List<int>();
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;

            for (int i = 0; i < count; i++)
            {
                if (i != cityId)
                    list.Add(i);
            }

            return list;
        }

        public static ReinoDiplomacySession GetSession(PlayerMobile pm, int cityId)
        {
            if (pm == null)
                return new ReinoDiplomacySession();

            ReinoDiplomacySession session;
            if (!m_Sessions.TryGetValue(pm.Serial.Value, out session) || session == null)
            {
                session = new ReinoDiplomacySession();
                session.CityId = cityId;
                List<int> others = GetOtherCityIds(cityId);
                session.TargetCityId = others.Count > 0 ? others[0] : -1;
                session.DraftRelation = session.TargetCityId >= 0 ? GetRelation(cityId, session.TargetCityId) : ReinoDiplomacyRelationStatus.Neutral;
                m_Sessions[pm.Serial.Value] = session;
            }

            session.CityId = cityId;

            if (session.TargetCityId < 0 || session.TargetCityId == cityId)
            {
                List<int> others = GetOtherCityIds(cityId);
                session.TargetCityId = others.Count > 0 ? others[0] : -1;
            }

            if (session.TargetCityId >= 0 && !session.DraftRelation.HasValue)
                session.DraftRelation = GetRelation(cityId, session.TargetCityId);

            return session;
        }

        public static void ResetSessionSelection(PlayerMobile pm, int cityId, int targetCityId)
        {
            ReinoDiplomacySession session = GetSession(pm, cityId);
            session.TargetCityId = targetCityId;
            session.DraftRelation = GetRelation(cityId, targetCityId);
            session.SelectedAction = ReinoDiplomacyActionKind.None;
            session.SelectedPostoId = String.Empty;
            session.DraftDonation = new ReinoDiplomacyResourceBundle();
            session.DraftAgreementSend = new ReinoDiplomacyResourceBundle();
            session.DraftAgreementReceive = new ReinoDiplomacyResourceBundle();
            session.DraftBorders = GetStoredBorderPolicy(cityId, targetCityId);
            session.DraftBlockade = GetStoredBlockade(cityId, targetCityId);
            session.DraftTribute = new ReinoDiplomacyResourceBundle();
            session.DraftTributeFrequency = ReinoDiplomacyTributeFrequency.Once;
        }

        public static int ResolveCityId(string cityOrPeopleName)
        {
            if (String.IsNullOrWhiteSpace(cityOrPeopleName))
                return -1;

            for (int i = 0; i < (ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4); i++)
            {
                if (String.Equals(ReinoElectionsSystem.GetCityName(i), cityOrPeopleName, StringComparison.OrdinalIgnoreCase))
                    return i;

                if (String.Equals(ReinoElectionsSystem.GetCityPeopleName(i), cityOrPeopleName, StringComparison.OrdinalIgnoreCase))
                    return i;

                if (String.Equals(ReinoElectionsSystem.GetRequiredCultureId(i), cityOrPeopleName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        public static int ResolvePlayerOriginCityId(PlayerMobile pm)
        {
            if (pm == null)
                return -1;

            int byCulture = ResolveCityId(pm.OSUCultureId);
            if (byCulture >= 0)
                return byCulture;

            return ResolveCityId(pm.OSUCitizenCityId);
        }

        public static int ResolvePlayerCitizenCityId(PlayerMobile pm)
        {
            if (pm == null)
                return -1;

            int byCitizen = ResolveCityId(pm.OSUCitizenCityId);
            if (byCitizen >= 0)
                return byCitizen;

            return ResolvePlayerOriginCityId(pm);
        }

        private static int ResolvePlayerCapitalCityId(PlayerMobile pm)
        {
            if (pm == null || String.IsNullOrWhiteSpace(pm.OSUCultureId))
                return -1;

            OSUCultureDefinition culture = OSUCultureRegistry.GetById(pm.OSUCultureId);

            if (culture != null && !String.IsNullOrWhiteSpace(culture.CapitalCityId))
                return ResolveCityId(culture.CapitalCityId);

            return ResolveCityId(pm.OSUCultureId);
        }

        public static string ResolvePlayerCapitalCityName(PlayerMobile pm)
        {
            if (pm == null || String.IsNullOrWhiteSpace(pm.OSUCultureId))
                return String.Empty;

            OSUCultureDefinition culture = OSUCultureRegistry.GetById(pm.OSUCultureId);

            if (culture != null && !String.IsNullOrWhiteSpace(culture.CapitalCityId))
                return culture.CapitalCityId;

            return String.Empty;
        }

        public static bool CanAcquireProperty(PlayerMobile pm, string propertyCityName, out string reason)
        {
            reason = String.Empty;

            if (pm == null || pm.Deleted)
                return false;

            if (String.IsNullOrWhiteSpace(propertyCityName))
                return true;

            int targetCityId = ResolveCityId(propertyCityName);
            if (targetCityId < 0)
                return true;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == targetCityId)
                return true;

            ReinoDiplomacyRelationStatus relation = GetRelation(sourceCityId, targetCityId);

            if (relation == ReinoDiplomacyRelationStatus.Enemy || relation == ReinoDiplomacyRelationStatus.War)
            {
                reason = "Cidadãos do reino " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) +
                         " não podem alugar ou comprar propriedades em " + ReinoElectionsSystem.GetCityName(targetCityId) +
                         " enquanto a relação for " + GetRelationLabel(relation).ToLower() + ".";
                return false;
            }

            int occupiedRoleCityId;
            ReinoCargoEntry occupiedRole = ReinoEmploymentSystem.GetOccupiedRoleAnywhere(pm, out occupiedRoleCityId);

            if (occupiedRole != null
                && occupiedRole.Hierarchy <= 2
                && occupiedRoleCityId >= 0
                && occupiedRoleCityId != targetCityId)
            {
                reason = "Quem ocupa um cargo de hierarquia " + occupiedRole.Hierarchy +
                         " em " + ReinoElectionsSystem.GetCityName(occupiedRoleCityId) +
                         " não pode alugar ou comprar propriedades em outro reino.";

                return false;
            }

            return true;
        }

        public static void HandleCitizenshipAfterPropertyAcquired(PlayerMobile pm, string propertyCityName)
        {
            if (pm == null || pm.Deleted)
                return;

            if (String.IsNullOrWhiteSpace(propertyCityName))
                return;

            string currentCitizenCity = pm.OSUCitizenCityId;

            if (String.IsNullOrWhiteSpace(currentCitizenCity))
                return;

            if (String.Equals(currentCitizenCity, propertyCityName, StringComparison.OrdinalIgnoreCase))
                return;

            string capitalCityName = ResolvePlayerCapitalCityName(pm);

            if (String.IsNullOrWhiteSpace(capitalCityName))
                return;

            if (String.Equals(currentCitizenCity, capitalCityName, StringComparison.OrdinalIgnoreCase))
                return;

            pm.OSUCitizenCityId = capitalCityName;

            pm.SendMessage("Ao adquirir uma propriedade em outro reino, sua cidadania anterior em " + currentCitizenCity + " foi encerrada.");
            pm.SendMessage("Até se registrar novamente, sua cidadania voltou para " + capitalCityName + ".");
        }

        private static bool IsWarForeignCitizen(PlayerMobile pm, out int capitalCityId, out int foreignCitizenCityId)
        {
            capitalCityId = ResolvePlayerCapitalCityId(pm);
            foreignCitizenCityId = ResolveCityId(pm.OSUCitizenCityId);

            if (capitalCityId < 0 || foreignCitizenCityId < 0)
                return false;

            if (capitalCityId == foreignCitizenCityId)
                return false;

            return GetRelation(capitalCityId, foreignCitizenCityId) == ReinoDiplomacyRelationStatus.War;
        }

        private static ReinoDiplomacyWarCitizenWarning GetWarCitizenWarning(int playerSerial)
        {
            for (int i = 0; i < m_WarCitizenWarnings.Count; i++)
            {
                ReinoDiplomacyWarCitizenWarning warning = m_WarCitizenWarnings[i];

                if (warning != null && warning.PlayerSerial == playerSerial)
                    return warning;
            }

            return null;
        }

        private static void RemoveWarCitizenWarning(int playerSerial)
        {
            for (int i = m_WarCitizenWarnings.Count - 1; i >= 0; i--)
            {
                ReinoDiplomacyWarCitizenWarning warning = m_WarCitizenWarnings[i];

                if (warning != null && warning.PlayerSerial == playerSerial)
                    m_WarCitizenWarnings.RemoveAt(i);
            }
        }

        private static string BuildWarCitizenWarningHtml(int foreignCitizenCityId, int capitalCityId, TimeSpan remaining)
        {
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            string foreignCity = ReinoElectionsSystem.GetCityName(foreignCitizenCityId);
            string capitalCity = ReinoElectionsSystem.GetCityName(capitalCityId);
            string peopleName = ReinoElectionsSystem.GetCityPeopleName(capitalCityId);

            return "<BASEFONT COLOR=#000000><BIG><B>Estado de Guerra</B></BIG><BR><BR>" +
                "Você pertence ao povo de " + peopleName + ", mas ainda possui cidadania em " + foreignCity + ". " +
                "Como os reinos estão em guerra, você tem 24 horas a partir deste aviso para deixar de ser cidadão dessa cidade.<BR><BR>" +
                "Se o prazo acabar, sua cidadania será removida automaticamente e voltará para " + capitalCity + ".<BR><BR>" +
                "Tempo restante aproximado: " + Math.Ceiling(remaining.TotalHours) + " horas.</BASEFONT>";
        }

        private static string BuildWarCitizenExpiredHtml(int foreignCitizenCityId, int capitalCityId)
        {
            string foreignCity = ReinoElectionsSystem.GetCityName(foreignCitizenCityId);
            string capitalCity = ReinoElectionsSystem.GetCityName(capitalCityId);

            return "<BASEFONT COLOR=#000000><BIG><B>Estado de Guerra</B></BIG><BR><BR>" +
                "O prazo de 24 horas terminou. Sua cidadania em " + foreignCity + " foi encerrada automaticamente por causa do estado de guerra.<BR><BR>" +
                "Sua cidadania agora voltou para " + capitalCity + ".</BASEFONT>";
        }

       private static void EnsureWarCitizenWarning(PlayerMobile pm, bool showNotice)
{
    if (pm == null || pm.Deleted)
        return;

    int capitalCityId;
    int foreignCitizenCityId;

    if (!IsWarForeignCitizen(pm, out capitalCityId, out foreignCitizenCityId))
    {
        RemoveWarCitizenWarning(pm.Serial);
        return;
    }

    ReinoDiplomacyWarCitizenWarning warning = GetWarCitizenWarning(pm.Serial);
    bool isNew = false;

    if (warning == null ||
        warning.ForeignCitizenCityId != foreignCitizenCityId ||
        warning.CapitalCityId != capitalCityId)
    {
        RemoveWarCitizenWarning(pm.Serial);

        warning = new ReinoDiplomacyWarCitizenWarning();
        warning.PlayerSerial = pm.Serial;
        warning.ForeignCitizenCityId = foreignCitizenCityId;
        warning.CapitalCityId = capitalCityId;
        warning.StartedUtc = DateTime.UtcNow;

        m_WarCitizenWarnings.Add(warning);
        isNew = true;
        showNotice = false;
    }

    DateTime expiresUtc = warning.StartedUtc.AddHours(24.0);

    if (DateTime.UtcNow >= expiresUtc)
    {
        FinishWarExile(pm, foreignCitizenCityId, capitalCityId);
        RemoveWarCitizenWarning(pm.Serial);
        return;
    }

    if (isNew)
        BeginWarExile(pm, foreignCitizenCityId, capitalCityId);

    if (showNotice)
        AddNoticeToSerial(pm.Serial, "Decreto de Exílio", BuildWarCitizenWarningHtml(foreignCitizenCityId, capitalCityId, false, String.Empty, false), false);
}

        private static void HandleWarCitizenLogin(PlayerMobile pm)
        {
            EnsureWarCitizenWarning(pm, true);
        }

        private static void ProcessWarCitizenWarnings()
        {
            foreach (NetState ns in NetState.Instances)
            {
                PlayerMobile pm = ns.Mobile as PlayerMobile;

                if (pm == null || pm.Deleted)
                    continue;

                EnsureWarCitizenWarning(pm, false);
            }
        }

        private static void NotifyOnlineWarCitizens(int cityA, int cityB)
        {
            foreach (NetState ns in NetState.Instances)
            {
                PlayerMobile pm = ns.Mobile as PlayerMobile;

                if (pm == null || pm.Deleted)
                    continue;

                int capitalCityId;
                int foreignCitizenCityId;

                if (!IsWarForeignCitizen(pm, out capitalCityId, out foreignCitizenCityId))
                    continue;

                bool match =
                    (capitalCityId == cityA && foreignCitizenCityId == cityB) ||
                    (capitalCityId == cityB && foreignCitizenCityId == cityA);

                if (match)
                    EnsureWarCitizenWarning(pm, true);
            }
        }

        public static ReinoDiplomacyRelationStatus GetRelation(int sourceCityId, int targetCityId)
        {
            if (sourceCityId < 0 || targetCityId < 0 || sourceCityId == targetCityId)
                return ReinoDiplomacyRelationStatus.Neutral;

            ReinoDiplomacyRelationStatus relation;
            if (m_Relations.TryGetValue(DirectionKey(sourceCityId, targetCityId), out relation))
                return relation;

            return ReinoDiplomacyRelationStatus.Neutral;
        }

        private static void SetRelation(int sourceCityId, int targetCityId, ReinoDiplomacyRelationStatus relation)
        {
            if (sourceCityId < 0 || targetCityId < 0 || sourceCityId == targetCityId)
                return;

            string key = DirectionKey(sourceCityId, targetCityId);

            if (relation == ReinoDiplomacyRelationStatus.Neutral)
                m_Relations.Remove(key);
            else
                m_Relations[key] = relation;
        }

        public static bool AreAllies(int cityA, int cityB)
        {
            return GetRelation(cityA, cityB) == ReinoDiplomacyRelationStatus.Allied;
        }

        public static bool AreEnemiesOrAtWar(int cityA, int cityB)
        {
            ReinoDiplomacyRelationStatus relation = GetRelation(cityA, cityB);
            return relation == ReinoDiplomacyRelationStatus.Enemy || relation == ReinoDiplomacyRelationStatus.War;
        }

        public static ReinoDiplomacyBorderPolicy GetStoredBorderPolicy(int sourceCityId, int targetCityId)
        {
            string key = DirectionKey(sourceCityId, targetCityId);
            ReinoDiplomacyBorderPolicy value;
            if (m_Borders.TryGetValue(key, out value) && value != null)
                return value.Clone();

            return new ReinoDiplomacyBorderPolicy
            {
                SourceCityId = sourceCityId,
                TargetCityId = targetCityId
            };
        }

        public static ReinoDiplomacyCommercialBlockade GetStoredBlockade(int sourceCityId, int targetCityId)
        {
            string key = DirectionKey(sourceCityId, targetCityId);
            ReinoDiplomacyCommercialBlockade value;
            if (m_Blockades.TryGetValue(key, out value) && value != null)
                return value.Clone();

            return new ReinoDiplomacyCommercialBlockade
            {
                SourceCityId = sourceCityId,
                TargetCityId = targetCityId
            };
        }

        public static ReinoDiplomacyBorderPolicy GetEffectiveBorderPolicy(int sourceCityId, int targetCityId)
        {
            ReinoDiplomacyBorderPolicy policy = GetStoredBorderPolicy(sourceCityId, targetCityId);
            if (GetRelation(sourceCityId, targetCityId) == ReinoDiplomacyRelationStatus.War)
            {
                policy.BlockEnemyCitizens = true;
                policy.BlockEnemyCulture = true;
                policy.BlockEnemyAllies = true;
                policy.AllowEntry = false;
            }

            return policy;
        }

        public static ReinoDiplomacyCommercialBlockade GetEffectiveBlockade(int sourceCityId, int targetCityId)
        {
            ReinoDiplomacyCommercialBlockade policy = GetStoredBlockade(sourceCityId, targetCityId);
            if (GetRelation(sourceCityId, targetCityId) == ReinoDiplomacyRelationStatus.War)
            {
                policy.BlockRepresentative = true;
                policy.CancelAgreements = true;
                policy.CancelDonations = true;
                policy.BlockPlayerVendors = true;
            }

            return policy;
        }

        public static ReinoDiplomacyAgreement GetAgreement(int cityA, int cityB)
        {
            ReinoDiplomacyAgreement agreement;
            if (m_Agreements.TryGetValue(PairKey(cityA, cityB), out agreement))
                return agreement;

            return null;
        }

        public static ReinoDiplomacyTribute GetTribute(int demandingCityId, int payingCityId)
        {
            ReinoDiplomacyTribute tribute;
            if (m_Tributes.TryGetValue(DirectionKey(demandingCityId, payingCityId), out tribute))
                return tribute;

            return null;
        }

        public static bool CanBecomeCitizen(PlayerMobile pm, int cityId, out string reason)
        {
            reason = String.Empty;

            if (pm == null || pm.Deleted)
            {
                reason = "Jogador inválido.";
                return false;
            }

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == cityId)
                return true;

            ReinoDiplomacyRelationStatus relation = GetRelation(sourceCityId, cityId);
            if (relation == ReinoDiplomacyRelationStatus.Enemy || relation == ReinoDiplomacyRelationStatus.War)
            {
                reason = "Cidadãos do reino " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " não podem adquirir cidadania em " + ReinoElectionsSystem.GetCityName(cityId) + " enquanto a relação for " + GetRelationLabel(relation).ToLower() + ".";
                return false;
            }

            return true;
        }

        public static bool CanUseBank(PlayerMobile pm, int cityId, out string reason)
        {
            reason = String.Empty;

            if (pm == null || pm.Deleted)
                return false;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            string ownerCityName = ReinoElectionsSystem.GetCityName(cityId);
            bool isCitizen = !String.IsNullOrWhiteSpace(ownerCityName) && pm.IsCitizenOf(ownerCityName);

            if (sourceCityId < 0 || sourceCityId == cityId)
                return true;

            ReinoDiplomacyRelationStatus relation = GetRelation(sourceCityId, cityId);

            switch (relation)
            {
                case ReinoDiplomacyRelationStatus.Allied:
                    return true;

                case ReinoDiplomacyRelationStatus.Neutral:
                    if (isCitizen)
                        return true;

                    reason = "Apenas cidadãos de " + ownerCityName + " podem usar este banco enquanto a relação for neutra.";
                    return false;

                case ReinoDiplomacyRelationStatus.Enemy:
                    if (isCitizen)
                        return true;

                    reason = "Cidadãos do reino inimigo só podem usar este banco se já forem cidadãos de " + ownerCityName + ".";
                    return false;

                case ReinoDiplomacyRelationStatus.War:
                    reason = "Em estado de guerra, cidadãos de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " não podem usar o banco de " + ownerCityName + ".";
                    return false;
            }

            return false;
        }

        public static bool CanUseCommercialRepresentative(PlayerMobile pm, int ownerCityId, out string reason)
        {
            reason = String.Empty;
            if (pm == null || pm.Deleted)
                return false;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == ownerCityId)
                return true;

            ReinoDiplomacyCommercialBlockade blockade = GetEffectiveBlockade(ownerCityId, sourceCityId);
            if (blockade.BlockRepresentative)
            {
                reason = "O representante comercial deste reino está bloqueado para cidadãos de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + ".";
                return false;
            }

            return true;
        }

        public static bool CanUseDonationChest(PlayerMobile pm, int ownerCityId, out string reason)
        {
            reason = String.Empty;
            if (pm == null || pm.Deleted)
                return false;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == ownerCityId)
                return true;

            ReinoDiplomacyCommercialBlockade blockade = GetEffectiveBlockade(ownerCityId, sourceCityId);
            if (blockade.CancelDonations)
            {
                reason = "Este reino não está recebendo doações diplomáticas vindas de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + ".";
                return false;
            }

            return true;
        }


        public static bool CanRentGovernmentProperty(PlayerMobile pm, TownHouseSign sign, out string reason)
        {
            reason = String.Empty;
            if (pm == null || pm.Deleted || sign == null || sign.Deleted || sign.GovernmentCityId < 0)
                return true;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == sign.GovernmentCityId)
                return true;

            ReinoDiplomacyRelationStatus relation = GetRelation(sourceCityId, sign.GovernmentCityId);
            if (relation == ReinoDiplomacyRelationStatus.Enemy || relation == ReinoDiplomacyRelationStatus.War)
            {
                reason = "A relação diplomática atual impede cidadãos de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " de alugar propriedades neste reino.";
                return false;
            }

            if (sign.PropertyType == OSUPropertyType.Tomb && relation != ReinoDiplomacyRelationStatus.Allied)
            {
                reason = "Terrenos de cemitério só podem ser alugados por reinos aliados.";
                return false;
            }

            return true;
        }

        public static bool CanUsePostOffice(PlayerMobile pm, int officeCityId, out string reason)
        {
            if (IsPendingWarExile(pm, officeCityId))
            {
                reason = "Você está em exílio de guerra e não pode mais usar os correios deste reino.";
                return false;
            }

            reason = String.Empty;

            if (pm == null || pm.Deleted || officeCityId < 0)
                return true;

            int citizenCityId = ResolvePlayerCitizenCityId(pm);

            if (citizenCityId < 0 || citizenCityId == officeCityId)
                return true;

            if (GetRelation(citizenCityId, officeCityId) == ReinoDiplomacyRelationStatus.Allied)
                return true;

            reason = "Os correios de " + ReinoElectionsSystem.GetCityName(officeCityId) +
                     " só atendem cidadãos desse reino e de reinos aliados.";
            return false;
        }

        public static bool CanUseLibrary(PlayerMobile pm, int libraryCityId, out string reason)
        {
            if (IsPendingWarExile(pm, libraryCityId))
            {
                reason = "Você está em exílio de guerra e não pode mais usar a biblioteca deste reino.";
                return false;
            }

            reason = String.Empty;

            if (pm == null || pm.Deleted || libraryCityId < 0)
                return true;

            int citizenCityId = ResolvePlayerCitizenCityId(pm);

            if (citizenCityId < 0 || citizenCityId == libraryCityId)
                return true;

            if (GetRelation(citizenCityId, libraryCityId) == ReinoDiplomacyRelationStatus.Allied)
                return true;

            reason = "A biblioteca de " + ReinoElectionsSystem.GetCityName(libraryCityId) +
                     " só atende cidadãos desse reino e de reinos aliados.";
            return false;
        }

        public static bool CanUseLibraryCard(int cardCityId, int libraryCityId, out string reason)
        {
            reason = String.Empty;

            if (cardCityId < 0 || libraryCityId < 0 || cardCityId == libraryCityId)
                return true;

            if (GetRelation(cardCityId, libraryCityId) == ReinoDiplomacyRelationStatus.Allied)
                return true;

            reason = "Este cartão da biblioteca não é aceito nesta biblioteca.";
            return false;
        }

        public static bool CanExchangeMail(PlayerMobile fromPm, Mobile toMobile, int officeCityId, out string reason)
        {
            reason = String.Empty;

            PlayerMobile toPm = toMobile as PlayerMobile;
            if (fromPm == null || fromPm.Deleted || toPm == null || toPm.Deleted)
                return true;

            int senderCitizenCityId = ResolvePlayerCitizenCityId(fromPm);
            int targetCitizenCityId = ResolvePlayerCitizenCityId(toPm);

            if (senderCitizenCityId < 0 || targetCitizenCityId < 0)
                return true;

            if (officeCityId >= 0)
            {
                if (!CanUsePostOffice(fromPm, officeCityId, out reason))
                    return false;

                if (targetCitizenCityId != officeCityId &&
                    GetRelation(officeCityId, targetCitizenCityId) != ReinoDiplomacyRelationStatus.Allied)
                {
                    reason = "O correio de " + ReinoElectionsSystem.GetCityName(officeCityId) +
                             " só envia correspondência para cidadãos desse reino e de reinos aliados.";
                    return false;
                }

                if (targetCitizenCityId != officeCityId &&
                    !ReinoEmploymentSystem.CityHasOperationalPostOffice(targetCitizenCityId))
                {
                    reason = "O reino de " + ReinoElectionsSystem.GetCityPeopleName(targetCitizenCityId) +
                             " ainda não possui correios em funcionamento.";
                    return false;
                }

                return true;
            }

            if (senderCitizenCityId == targetCitizenCityId)
                return true;

            if (GetRelation(senderCitizenCityId, targetCitizenCityId) != ReinoDiplomacyRelationStatus.Allied)
            {
                reason = "Os correios só permitem correspondência entre cidadãos do mesmo reino ou de reinos aliados.";
                return false;
            }

            if (!ReinoEmploymentSystem.CityHasOperationalPostOffice(targetCitizenCityId))
            {
                reason = "O reino de " + ReinoElectionsSystem.GetCityPeopleName(targetCitizenCityId) +
                         " ainda não possui correios em funcionamento.";
                return false;
            }

            return true;
        }

        public static bool CanUsePlayerVendor(PlayerMobile pm, TownHouseSign sign, out string reason)
        {
            reason = String.Empty;
            if (pm == null || pm.Deleted || sign == null || sign.Deleted)
                return true;

            if (sign.GovernmentCityId < 0 || sign.PropertyType != OSUPropertyType.Commercial)
                return true;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == sign.GovernmentCityId)
                return true;

            ReinoDiplomacyCommercialBlockade blockade = GetEffectiveBlockade(sign.GovernmentCityId, sourceCityId);
            if (blockade.BlockPlayerVendors)
            {
                reason = "Vendedores particulares deste reino não podem comerciar com cidadãos de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + ".";
                return false;
            }

            return true;
        }

        public static bool CanAcceptRoleIn(PlayerMobile pm, int cityId, int hierarchy, out string reason)
        {
            reason = String.Empty;
            if (pm == null || pm.Deleted)
                return false;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == cityId)
                return true;

            ReinoDiplomacyRelationStatus relation = GetRelation(sourceCityId, cityId);
            if (relation == ReinoDiplomacyRelationStatus.Enemy || relation == ReinoDiplomacyRelationStatus.War)
            {
                reason = "A diplomacia atual impede qualquer cidadão de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " de ocupar cargos neste reino.";
                return false;
            }

            if (relation == ReinoDiplomacyRelationStatus.Allied)
            {
                if (hierarchy <= 1)
                {
                    reason = "Aliados ainda não podem ocupar o cargo máximo do reino.";
                    return false;
                }

                return true;
            }

            if (hierarchy >= 4)
                return true;

            reason = "Em situação neutra, apenas cargos de hierarquia inferior podem ser ocupados por cidadãos estrangeiros.";
            return false;
        }

        public static bool CanVoteInCity(PlayerMobile pm, int cityId, out string reason)
        {
            reason = String.Empty;
            if (pm == null || pm.Deleted)
                return false;

            int sourceCityId = ResolvePlayerOriginCityId(pm);
            if (sourceCityId < 0 || sourceCityId == cityId)
                return true;

            ReinoDiplomacyRelationStatus relation = GetRelation(sourceCityId, cityId);
            if (relation == ReinoDiplomacyRelationStatus.Enemy || relation == ReinoDiplomacyRelationStatus.War)
            {
                reason = "A situação diplomática atual impede cidadãos de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " de votar em " + ReinoElectionsSystem.GetCityName(cityId) + ".";
                return false;
            }

            return true;
        }

        public static List<ReinoDiplomacyActionKind> GetAvailableActions(int cityId, int targetCityId)
        {
            return GetAvailableActionsForRelation(GetRelation(cityId, targetCityId));
        }

        public static List<ReinoDiplomacyActionKind> GetAvailableActionsForRelation(ReinoDiplomacyRelationStatus relation)
        {
            List<ReinoDiplomacyActionKind> list = new List<ReinoDiplomacyActionKind>();

            if (relation == ReinoDiplomacyRelationStatus.Allied)
            {
                list.Add(ReinoDiplomacyActionKind.DonateResources);
                list.Add(ReinoDiplomacyActionKind.DonatePosto);
                list.Add(ReinoDiplomacyActionKind.ProposeAgreement);
            }
            else if (relation == ReinoDiplomacyRelationStatus.Enemy)
            {
                list.Add(ReinoDiplomacyActionKind.CloseBorders);
                list.Add(ReinoDiplomacyActionKind.CommercialBlockade);
                list.Add(ReinoDiplomacyActionKind.DemandTribute);
            }
            else if (relation == ReinoDiplomacyRelationStatus.War)
            {
                list.Add(ReinoDiplomacyActionKind.DemandTribute);
            }

            return list;
        }

        public static string GetActionLabel(ReinoDiplomacyActionKind kind)
        {
            switch (kind)
            {
                case ReinoDiplomacyActionKind.DonateResources: return "Doar Recursos";
                case ReinoDiplomacyActionKind.DonatePosto: return "Doar Posto";
                case ReinoDiplomacyActionKind.ProposeAgreement: return "Estabelecer Acordo";
                case ReinoDiplomacyActionKind.CloseBorders: return "Fechar Fronteiras";
                case ReinoDiplomacyActionKind.CommercialBlockade: return "Bloqueio Comercial";
                case ReinoDiplomacyActionKind.DemandTribute: return "Exigir Tributo";
                case ReinoDiplomacyActionKind.ChangeRelation: return "Relação Diplomática";
                case ReinoDiplomacyActionKind.CancelAgreement: return "Quebra de Acordo";
                default: return String.Empty;
            }
        }

        public static string GetRelationLabel(ReinoDiplomacyRelationStatus relation)
        {
            switch (relation)
            {
                case ReinoDiplomacyRelationStatus.Allied: return "Aliados";
                case ReinoDiplomacyRelationStatus.Enemy: return "Inimigos";
                case ReinoDiplomacyRelationStatus.War: return "Guerra";
                default: return "Neutro";
            }
        }

        private static string BuildWarGeneralHtml(int viewerCityId, int otherCityId)
        {
            string myPeople = ReinoElectionsSystem.GetCityPeopleName(viewerCityId);
            string otherPeople = ReinoElectionsSystem.GetCityPeopleName(otherCityId);
            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(viewerCityId);

            string text;

            switch (culture)
            {
                case "kamay":
                    text = "Com pesar, anunciamos que os entendimentos com " + otherPeople + " chegaram ao fim. Que cada cidadão mantenha a calma e honre o reino neste tempo difícil.";
                    break;
                case "matalun":
                    text = "As tensões com " + otherPeople + " romperam o último fio de paz. Que o povo permaneça firme, unido e atento aos dias que virão.";
                    break;
                case "sarangs":
                    text = "Os caminhos entre " + myPeople + " e " + otherPeople + " se fecharam. É hora de prudência, disciplina e lealdade ao reino.";
                    break;
                case "zorteros":
                    text = "A paz com " + otherPeople + " foi quebrada. Que cada habitante de " + myPeople + " se prepare com seriedade para tempos severos.";
                    break;
                default:
                    text = "Os desentendimentos com " + otherPeople + " chegaram ao ponto de guerra. Que o povo atravesse este período com ordem e firmeza.";
                    break;
            }

            return "<BASEFONT COLOR=#000000><BIG><B>Estado de Guerra</B></BIG><BR><BR>" +
                text + "</BASEFONT>";
        }
        private static void NotifyWarGeneralToKingdom(int cityId, int otherCityId)
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null || pm.Deleted)
                    continue;

                int originCityId = ResolvePlayerOriginCityId(pm);
                int citizenCityId = ResolvePlayerCitizenCityId(pm);

                if (originCityId != cityId && citizenCityId != cityId)
                    continue;

                AddNoticeToSerial(pm.Serial, "Estado de Guerra", BuildWarGeneralHtml(cityId, otherCityId), true);
            }
        }

        private static void BeginWarExile(PlayerMobile pm, int foreignCitizenCityId, int capitalCityId)
        {
            if (pm == null || pm.Deleted)
                return;

            string removedRoles;
            bool lostRole = RemoveGovernmentRolesForPlayer(pm, foreignCitizenCityId, out removedRoles);
            bool lostLibraryCard = CancelLibraryCardsForCity(pm, foreignCitizenCityId);

            AddNoticeToSerial(pm.Serial, "Decreto de Exílio", BuildWarCitizenWarningHtml(foreignCitizenCityId, capitalCityId, lostRole, removedRoles, lostLibraryCard), false);
        }

        private static void VacatePlayerPropertiesInCity(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted || cityId < 0)
                return;

            string cityName = ReinoElectionsSystem.GetCityName(cityId);

            foreach (TownHouse house in TownHouse.AllTownHouses)
            {
                if (house == null || house.Deleted)
                    continue;

                TownHouseSign sign = house.ForSaleSign;

                if (sign == null || sign.Deleted || !sign.Owned)
                    continue;

                if (!sign.IsOwnedBy(pm))
                    continue;

                if (sign.PropertyType != OSUPropertyType.House && sign.PropertyType != OSUPropertyType.Commercial)
                    continue;

                if (!String.Equals(sign.CitizenCityId, cityName, StringComparison.OrdinalIgnoreCase))
                    continue;

                sign.ForceGovernmentExileVacate("Sua propriedade em " + cityName + " foi retomada por causa do exílio de guerra.");
            }
        }

        private static void FinishWarExile(PlayerMobile pm, int foreignCitizenCityId, int capitalCityId)
        {
            if (pm == null || pm.Deleted)
                return;

            pm.OSUCitizenCityId = ReinoElectionsSystem.GetCityName(capitalCityId);
            VacatePlayerPropertiesInCity(pm, foreignCitizenCityId);
            AddNoticeToSerial(pm.Serial, "Exílio Consumado", BuildWarCitizenExpiredHtml(foreignCitizenCityId, capitalCityId), true);
        }

        private static string BuildWarCitizenWarningHtml(int foreignCitizenCityId, int capitalCityId, bool lostRole, string removedRoles, bool lostLibraryCard)
        {
            string foreignCity = ReinoElectionsSystem.GetCityName(foreignCitizenCityId);
            string capitalCity = ReinoElectionsSystem.GetCityName(capitalCityId);

            string html = "<BASEFONT COLOR=#000000><BIG><B>Decreto de Exílio</B></BIG><BR><BR>" +
                "Por causa do estado de guerra, sua cidadania em " + foreignCity + " foi cassada politicamente e será encerrada ao fim do prazo de retirada.<BR><BR>";

            if (lostRole)
                html += "• Você perdeu seu cargo no governo" + (String.IsNullOrWhiteSpace(removedRoles) ? "." : ": " + removedRoles + ".") + "<BR>";

            if (lostLibraryCard)
                html += "• Seu cartão da biblioteca deste reino foi cancelado.<BR>";

            html += "• Você não pode mais usar bancos deste reino.<BR>";
            html += "• Você não pode mais usar os correios deste reino.<BR>";
            html += "• Ao final do prazo, sua cidadania voltará para " + capitalCity + ".<BR>";
            html += "• Você tem <B>24 horas</B> para recolher seus pertences e deixar a cidade, pois depois desse tempo a guarda passará a tratá-lo como hostil.<BR>";
            html += "</BASEFONT>";

            return html;
        }
        public static string BuildStatusDescription(int cityId, int targetCityId, ReinoDiplomacyRelationStatus relation)
        {
            string targetPeople = ReinoElectionsSystem.GetCityPeopleName(targetCityId);
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            if (relation == ReinoDiplomacyRelationStatus.Neutral)
            {
                sb.Append("<BIG><B>Neutro</B></BIG><BR><BR>");
                sb.Append("Uma relação aberta, estável e sem hostilidade declarada.<BR><BR>");
                sb.Append("• Jogadores do reino de " + targetPeople + " podem entrar e sair livremente.<BR>");
                sb.Append("• Podem alugar casas dentro do reino.<BR>");
                sb.Append("• Podem manter casas comerciais.<BR>");
                sb.Append("• Embaixadores podem comprar e vender com o representante comercial.<BR>");
                sb.Append("• Podem se tornar cidadãos se cumprirem as exigências do reino.<BR>");
                sb.Append("• Podem usar o banco do reino, caso seja cidadão<BR>");
                sb.Append("• Podem votar, desde que já sejam cidadãos.<BR>");
                sb.Append("• Podem ocupar cargos de hierarquia inferior a 3.");
            }
            else if (relation == ReinoDiplomacyRelationStatus.Allied)
            {
                sb.Append("<BIG><B>Aliados</B></BIG><BR><BR>");
                sb.Append("Uma relação de confiança, cooperação e abertura ampliada entre os dois reinos.<BR><BR>");
                sb.Append("• Jogadores do reino de " + targetPeople + " podem entrar e sair livremente.<BR>");
                sb.Append("• Podem alugar casas dentro do reino.<BR>");
                sb.Append("• Podem manter casas comerciais.<BR>");
                sb.Append("• Embaixadores podem comprar e vender com o representante comercial.<BR>");
                sb.Append("• Podem se tornar cidadãos se cumprirem as exigências do reino.<BR>");
                sb.Append("• Podem usar o banco do reino, mesmo sem ser cidadão<BR>");
                sb.Append("• Podem votar, desde que seja cidadão.<BR>");
                sb.Append("• Podem ter lápide no cemitério.<BR>");
                sb.Append("• Podem ocupar cargos de hierarquia 2 e 3.<BR>");
                sb.Append("• Podem enviar e receber correio entre os dois reinos.<BR>");
                sb.Append("• Podem comprar lotes fora, adjacentes ao reino.<BR>");
                sb.Append("• Tem todos os direito que um nativo do povo tem, menos de ser eleito líder do reino.<BR>");
            }
            else if (relation == ReinoDiplomacyRelationStatus.Enemy)
            {
                sb.Append("<BIG><B>Inimigos</B></BIG><BR><BR>");
                sb.Append("Uma relação hostil, porém ainda abaixo de guerra declarada.<BR><BR>");
                sb.Append("• Antigos cidadãos do reino não perdem a maior parte de seus poderes.<BR>");
                sb.Append("• São exonerados de qualquer cargo do governo.<BR>");
                sb.Append("• Não podem votar.<BR>");
                sb.Append("• Novas pessoas desse povo não podem alugar casas ou comércios.<BR>");
                sb.Append("• Novas pessoas desse povo não podem se tornar cidadãos do reino.<BR>");
                sb.Append("• Novas pessoas desse povo não podem usar os correios ou ter um cartao da biblioteca.<BR>");
            }
            else
            {
                sb.Append("<BIG><B>Guerra</B></BIG><BR><BR>");
                sb.Append("Uma relação de hostilidade máxima, com medidas automáticas de contenção e combate.<BR><BR>");
                sb.Append("• Não podem ocupar qualquer cargo do governo.<BR>");
                sb.Append("• Não podem alugar casas ou comércios.<BR>");
                sb.Append("• Não podem se tornar cidadãos do reino.<BR>");
                sb.Append("• Não podem usar o banco<BR>");
                sb.Append("• Não podem votar.<BR>");
                sb.Append("• Perdem a cidadania e são exilados do reino em 24 horas.<BR>");
                sb.Append("• Fechamento de todas as fronteiras entra em vigor automaticamente.<BR>");
                sb.Append("• Bloqueio comerciais entra em vigor automaticamente.<BR>");
                sb.Append("• Guardas do reino devem atacar cidadãos do reino inimigo quando os sistemas " +
                    "militares existirem e estiverem ativos.<BR>");
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string BuildActionDescription(int cityId, int targetCityId, ReinoDiplomacyActionKind action)
        {
            string targetPeople = ReinoElectionsSystem.GetCityPeopleName(targetCityId);
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            switch (action)
            {
                case ReinoDiplomacyActionKind.DonateResources:
                    sb.Append("<BIG><B>Doar Recursos</B></BIG><BR><BR>");
                    sb.Append("Define quantidades de moedas, madeira, ferro e tecido para enviar ao reino de ");
                    sb.Append(targetPeople);
                    sb.Append(". Após a confirmação, a proposta passa pela aprovação econômica do seu reino. Quando a doação é concluída, o outro reino apenas recebe o aviso.");
                    break;
                case ReinoDiplomacyActionKind.DonatePosto:
                    sb.Append("<BIG><B>Doar Posto</B></BIG><BR><BR>");
                    sb.Append("Transfere um posto já dominado para o outro reino sem conquista. Postos em disputa não podem ser escolhidos. A proposta depende de aprovação defensiva e do aceite do reino destinatário.");
                    break;
                case ReinoDiplomacyActionKind.ProposeAgreement:
                    sb.Append("<BIG><B>Estabelecer Acordo</B></BIG><BR><BR>");
                    sb.Append("Cria uma troca semanal fixa entre os dois reinos. Ao ser firmado, a primeira remessa já é enviada como sinal de confiança. Depois disso, o acordo roda toda segunda-feira às 19h e tenta novamente na terça às 19h se faltar recurso naquela semana.");
                    break;
                case ReinoDiplomacyActionKind.CloseBorders:
                    sb.Append("<BIG><B>Fechar Fronteiras</B></BIG><BR><BR>");
                    sb.Append("Define como guardas e sistemas futuros de quartel/prisão devem reagir a cidadãos, nativos e aliados do reino inimigo ao entrarem no território.");
                    break;
                case ReinoDiplomacyActionKind.CommercialBlockade:
                    sb.Append("<BIG><B>Bloqueio Comercial</B></BIG><BR><BR>");
                    sb.Append("Impede interações econômicas específicas com o reino alvo: representante comercial, acordos, doações e vendedores particulares em propriedades comerciais.");
                    break;
                case ReinoDiplomacyActionKind.DemandTribute:
                    sb.Append("<BIG><B>Exigir Tributo</B></BIG><BR><BR>");
                    sb.Append("Estabelece uma cobrança hostil de recursos com frequência definida. A exigência depende de aprovação defensiva e precisa ser aceita ou recusada pelo outro reino.");
                    break;
                default:
                    sb.Append(BuildStatusDescription(cityId, targetCityId, GetRelation(cityId, targetCityId)));
                    break;
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string FormatBundleLine(ReinoDiplomacyResourceBundle bundle)
        {
            if (bundle == null || bundle.IsEmpty)
                return "nenhum recurso";

            List<string> parts = new List<string>();
            if (bundle.Gold > 0) parts.Add(bundle.Gold + " moedas");
            if (bundle.Wood > 0) parts.Add(bundle.Wood + " madeiras");
            if (bundle.Iron > 0) parts.Add(bundle.Iron + " ferros");
            if (bundle.Cloth > 0) parts.Add(bundle.Cloth + " tecidos");
            return String.Join(", ", parts.ToArray());
        }

        public static string FormatAgreementShort(ReinoDiplomacyAgreement agreement)
        {
            if (agreement == null)
                return String.Empty;

            return "Enviar " + FormatBundleLine(agreement.SendFromSource) + " e receber " + FormatBundleLine(agreement.SendFromTarget) + ".";
        }

        public static List<PostoDefinition> GetDonatablePostos(int cityId)
        {
            List<PostoDefinition> list = new List<PostoDefinition>();

            foreach (PostoDefinition def in PostoSystem.AllDefinitions)
            {
                if (def == null)
                    continue;

                PostoState state = PostoSystem.GetState(def.Id);
                if (state == null)
                    continue;

                if (!String.Equals(state.OwnerCityId, ReinoElectionsSystem.GetCityName(cityId), StringComparison.OrdinalIgnoreCase))
                    continue;

                if (state.ContestEndsUtc > DateTime.UtcNow)
                    continue;

                list.Add(def);
            }

            return list;
        }

        private static List<ReinoDiplomacyVote> BuildVotes(int cityId, ReinoDiplomacyApprovalCategory category, int excludeSerial, bool includeLeader)
        {
            List<ReinoDiplomacyVote> votes = new List<ReinoDiplomacyVote>();
            HashSet<int> added = new HashSet<int>();

            ReinoCityData city = ReinoElectionsSystem.GetCityData(cityId);
            if (includeLeader && city != null && city.GovernorSerial > 0 && city.GovernorSerial != excludeSerial)
            {
                votes.Add(new ReinoDiplomacyVote { VoterSerial = city.GovernorSerial, VoterName = city.GovernorName ?? ReinoEmploymentSystem.GetLeaderTitle(cityId) });
                added.Add(city.GovernorSerial);
            }

            if (String.Equals(ReinoEmploymentSystem.GetGovernmentCultureId(cityId), "sarangs", StringComparison.OrdinalIgnoreCase))
                return votes;

            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);
            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(cityId);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied || role.IsLeaderRole)
                    continue;

                if (added.Contains(role.OccupantSerial) || role.OccupantSerial == excludeSerial)
                    continue;

                bool include = false;

                if (String.Equals(culture, "kamay", StringComparison.OrdinalIgnoreCase))
                {
                    if (category == ReinoDiplomacyApprovalCategory.Economy)
                        include = role.Kind == ReinoCargoKind.MinisterEconomy;
                    else if (category == ReinoDiplomacyApprovalCategory.Defense)
                        include = role.Kind == ReinoCargoKind.MinisterDefense;
                    else
                        include = role.Kind == ReinoCargoKind.MinisterEconomy || role.Kind == ReinoCargoKind.MinisterDefense;
                }
                else if (String.Equals(culture, "matalun", StringComparison.OrdinalIgnoreCase))
                {
                    include = role.Kind == ReinoCargoKind.Priest;
                }
                else if (String.Equals(culture, "zosteros", StringComparison.OrdinalIgnoreCase))
                {
                    include = role.Kind == ReinoCargoKind.CouncilMember;
                }

                if (include)
                {
                    votes.Add(new ReinoDiplomacyVote { VoterSerial = role.OccupantSerial, VoterName = role.OccupantName ?? role.Title });
                    added.Add(role.OccupantSerial);
                }
            }

            return votes;
        }

        private static void NotifyVotes(List<ReinoDiplomacyVote> votes)
        {
            for (int i = 0; i < votes.Count; i++)
            {
                PlayerMobile pm = World.FindMobile((Serial)votes[i].VoterSerial) as PlayerMobile;
                if (pm != null && !pm.Deleted && pm.NetState != null)
                    ShowPendingGump(pm);
            }
        }

        private static void AddNoticeToSerial(int targetSerial, string title, string html, bool closable)
        {
            if (targetSerial <= 0)
                return;

            ReinoDiplomacyNotice notice = new ReinoDiplomacyNotice();
            notice.NoticeId = m_NextNoticeId++;
            notice.TargetSerial = targetSerial;
            notice.Title = title ?? String.Empty;
            notice.Html = html ?? String.Empty;
            notice.Closable = closable;
            notice.CreatedUtc = DateTime.UtcNow;
            m_Notices.Add(notice);

            PlayerMobile pm = World.FindMobile((Serial)targetSerial) as PlayerMobile;
            if (pm != null && !pm.Deleted && pm.NetState != null)
                ShowPendingGump(pm);
        }

        private static void AddNoticeToLeader(int cityId, string title, string html, bool closable)
        {
            ReinoCityData city = ReinoElectionsSystem.GetCityData(cityId);
            if (city != null && city.GovernorSerial > 0)
                AddNoticeToSerial(city.GovernorSerial, title, html, closable);
        }

        private static void AddNoticeToOfficials(int cityId, ReinoDiplomacyApprovalCategory category, string title, string html, bool closable)
        {
            List<ReinoDiplomacyVote> votes = BuildVotes(cityId, category, 0, true);
            for (int i = 0; i < votes.Count; i++)
                AddNoticeToSerial(votes[i].VoterSerial, title, html, closable);
        }

        private static void AddNoticeToOfficialsExcept(int cityId, ReinoDiplomacyApprovalCategory category, int exceptSerial, string title, string html, bool closable)
        {
            List<ReinoDiplomacyVote> votes = BuildVotes(cityId, category, exceptSerial, true);
            for (int i = 0; i < votes.Count; i++)
                AddNoticeToSerial(votes[i].VoterSerial, title, html, closable);
        }

        public static void ShowPendingGump(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.NetState == null)
                return;

            for (int i = 0; i < m_Requests.Count; i++)
            {
                ReinoDiplomacyRequest request = m_Requests[i];
                if (request == null || !request.IsPending)
                    continue;

                List<ReinoDiplomacyVote> votes = request.State == ReinoDiplomacyRequestState.PendingSourceApproval ? request.SourceVotes : request.TargetVotes;
                for (int v = 0; v < votes.Count; v++)
                {
                    if (votes[v].VoterSerial == pm.Serial.Value && votes[v].Decision == 0)
                    {
                        pm.CloseGump(typeof(ReinoDiplomacyApprovalGump));
                        pm.SendGump(new ReinoDiplomacyApprovalGump(pm, request.RequestId));
                        return;
                    }
                }
            }

            for (int i = 0; i < m_Notices.Count; i++)
            {
                ReinoDiplomacyNotice notice = m_Notices[i];
                if (notice == null || notice.Consumed || notice.TargetSerial != pm.Serial.Value)
                    continue;

                pm.CloseGump(typeof(ReinoDiplomacyNoticeGump));
                pm.SendGump(new ReinoDiplomacyNoticeGump(pm, notice.NoticeId));
                return;
            }
        }

        public static ReinoDiplomacyNotice GetNotice(int noticeId)
        {
            for (int i = 0; i < m_Notices.Count; i++)
            {
                ReinoDiplomacyNotice notice = m_Notices[i];
                if (notice != null && notice.NoticeId == noticeId)
                    return notice;
            }

            return null;
        }

        public static void ConsumeNotice(int noticeId)
        {
            ReinoDiplomacyNotice notice = GetNotice(noticeId);
            if (notice != null)
                notice.Consumed = true;
        }

        public static ReinoDiplomacyRequest GetRequest(int requestId)
        {
            for (int i = 0; i < m_Requests.Count; i++)
            {
                ReinoDiplomacyRequest request = m_Requests[i];
                if (request != null && request.RequestId == requestId)
                    return request;
            }

            return null;
        }

        private static void ProcessPendingRequests()
        {
            DateTime now = DateTime.UtcNow;

            for (int i = 0; i < m_Requests.Count; i++)
            {
                ReinoDiplomacyRequest request = m_Requests[i];
                if (request == null || !request.IsPending)
                    continue;

                if (now >= request.ExpiresUtc)
                {
                    request.State = ReinoDiplomacyRequestState.Expired;
                    request.ResolvedUtc = now;
                    AddNoticeToOfficials(request.SourceCityId, request.Category, GetActionLabel(request.Action), "<BASEFONT COLOR=#000000>A proposta diplomática expirou sem aprovação completa no prazo de 48 horas.</BASEFONT>", true);
                    if (request.TargetCityId >= 0)
                        AddNoticeToOfficials(request.TargetCityId, request.Category, GetActionLabel(request.Action), "<BASEFONT COLOR=#000000>Uma proposta diplomática expirou sem resposta completa no prazo de 48 horas.</BASEFONT>", true);
                }
            }
        }

        private static bool AreAllVotesApproved(List<ReinoDiplomacyVote> votes)
        {
            if (votes == null || votes.Count == 0)
                return true;

            for (int i = 0; i < votes.Count; i++)
            {
                if (votes[i].Decision != 1)
                    return false;
            }

            return true;
        }

        private static bool HasAnyReject(List<ReinoDiplomacyVote> votes)
        {
            if (votes == null)
                return false;

            for (int i = 0; i < votes.Count; i++)
            {
                if (votes[i].Decision == 2)
                    return true;
            }

            return false;
        }

        public static bool VoteRequest(PlayerMobile pm, int requestId, bool approve, out string message)
        {
            message = String.Empty;

            if (pm == null || pm.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            ReinoDiplomacyRequest request = GetRequest(requestId);
            if (request == null || !request.IsPending)
            {
                message = "Essa solicitação diplomática já foi resolvida.";
                return false;
            }

            List<ReinoDiplomacyVote> votes = request.State == ReinoDiplomacyRequestState.PendingSourceApproval ? request.SourceVotes : request.TargetVotes;
            for (int i = 0; i < votes.Count; i++)
            {
                if (votes[i].VoterSerial != pm.Serial.Value)
                    continue;

                votes[i].Decision = approve ? 1 : 2;
                votes[i].DecisionUtc = DateTime.UtcNow;

                if (HasAnyReject(votes))
                {
                    bool wasTargetPhase = request.State == ReinoDiplomacyRequestState.PendingTargetApproval;
                    request.State = ReinoDiplomacyRequestState.Rejected;
                    request.ResolvedUtc = DateTime.UtcNow;
                    AddNoticeToOfficials(request.SourceCityId, request.Category, GetActionLabel(request.Action), "<BASEFONT COLOR=#000000>A proposta diplomática foi vetada.</BASEFONT>", true);
                    if (wasTargetPhase)
                        AddNoticeToOfficials(request.TargetCityId, request.Category, GetActionLabel(request.Action), "<BASEFONT COLOR=#000000>A proposta diplomática foi recusada.</BASEFONT>", true);
                }
                else if (AreAllVotesApproved(votes))
                {
                    if (request.State == ReinoDiplomacyRequestState.PendingSourceApproval)
                    {
                        if (request.RequiresTargetDecision)
                        {
                            if (request.TargetVotes != null && request.TargetVotes.Count > 0)
                            {
                                request.State = ReinoDiplomacyRequestState.PendingTargetApproval;
                                request.ExpiresUtc = DateTime.UtcNow.AddHours(48.0);
                                NotifyVotes(request.TargetVotes);
                            }
                            else
                            {
                                FinalizeRequest(request, true);
                            }
                        }
                        else
                        {
                            FinalizeRequest(request, true);
                        }
                    }
                    else
                    {
                        FinalizeRequest(request, true);
                    }
                }

                message = approve ? "Você aprovou a decisão diplomática." : "Você vetou a decisão diplomática.";
                ShowPendingGump(pm);
                return true;
            }

            message = "Você não pode decidir essa solicitação diplomática.";
            return false;
        }

        private static bool IsPendingWarExile(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted || cityId < 0)
                return false;

            ReinoDiplomacyWarCitizenWarning warning = GetWarCitizenWarning(pm.Serial);

            return warning != null && warning.ForeignCitizenCityId == cityId;
        }

        private static bool CancelLibraryCardsForCity(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted || cityId < 0)
                return false;

            bool removed = false;
            List<Item> items = new List<Item>();

            if (pm.Backpack != null)
                items.AddRange(pm.Backpack.FindItemsByType(typeof(LibraryCard), true));

            if (pm.BankBox != null)
                items.AddRange(pm.BankBox.FindItemsByType(typeof(LibraryCard), true));

            for (int i = 0; i < items.Count; i++)
            {
                LibraryCard card = items[i] as LibraryCard;

                if (card == null || card.Deleted)
                    continue;

                if (card.IssuerCityId != cityId)
                    continue;

                card.Delete();
                removed = true;
            }

            return removed;
        }

        private static bool RemoveGovernmentRolesForPlayer(PlayerMobile pm, int cityId, out string removedTitles)
        {
            removedTitles = String.Empty;

            if (pm == null || pm.Deleted || cityId < 0)
                return false;

            List<string> removed = new List<string>();
            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];

                if (role == null || role.IsLeaderRole || !role.IsOccupied)
                    continue;

                if (role.OccupantSerial != pm.Serial.Value)
                    continue;

                string msg;
                if (ReinoEmploymentSystem.RemoveRoleOccupant(null, cityId, role.RoleId, false, out msg))
                    removed.Add(role.Title);
            }

            if (removed.Count > 0)
                removedTitles = String.Join(", ", removed.ToArray());

            return removed.Count > 0;
        }

        private static void FinalizeRequest(ReinoDiplomacyRequest request, bool approved)
        {
            if (request == null)
                return;

            request.State = approved ? ReinoDiplomacyRequestState.Approved : ReinoDiplomacyRequestState.Rejected;
            request.ResolvedUtc = DateTime.UtcNow;

            if (!approved)
                return;

            switch (request.Action)
            {
                case ReinoDiplomacyActionKind.ChangeRelation:
                    ApplyRelationChange(request);
                    break;
                case ReinoDiplomacyActionKind.DonateResources:
                    ApplyResourceDonation(request);
                    break;
                case ReinoDiplomacyActionKind.DonatePosto:
                    ApplyPostoDonation(request);
                    break;
                case ReinoDiplomacyActionKind.ProposeAgreement:
                    ApplyAgreement(request);
                    break;
                case ReinoDiplomacyActionKind.CloseBorders:
                    ApplyBorders(request);
                    break;
                case ReinoDiplomacyActionKind.CommercialBlockade:
                    ApplyBlockade(request);
                    break;
                case ReinoDiplomacyActionKind.DemandTribute:
                    ApplyTribute(request);
                    break;
                case ReinoDiplomacyActionKind.CancelAgreement:
                    ApplyAgreementCancel(request.SourceCityId, request.TargetCityId, true);
                    break;
            }
        }

        private static void ApplyRelationChange(ReinoDiplomacyRequest request)
        {
            SetRelation(request.SourceCityId, request.TargetCityId, request.NewRelation);

            if (request.NewRelation == ReinoDiplomacyRelationStatus.Allied ||
                request.NewRelation == ReinoDiplomacyRelationStatus.War)
            {
                SetRelation(request.TargetCityId, request.SourceCityId, request.NewRelation);
            }

            if (request.NewRelation == ReinoDiplomacyRelationStatus.War)
            {
                NotifyWarGeneralToKingdom(request.SourceCityId, request.TargetCityId);
                NotifyWarGeneralToKingdom(request.TargetCityId, request.SourceCityId);

                NotifyOnlineWarCitizens(request.SourceCityId, request.TargetCityId);
            }
            else if (request.OldRelation == ReinoDiplomacyRelationStatus.Allied &&
                     request.NewRelation != ReinoDiplomacyRelationStatus.Allied)
            {
                string html = "<BASEFONT COLOR=#000000><BIG><B>Relação Diplomática</B></BIG><BR><BR>O reino de " +
                    ReinoElectionsSystem.GetCityPeopleName(request.SourceCityId) +
                    " alterou a relação diplomática de " + GetRelationLabel(request.OldRelation) +
                    " para " + GetRelationLabel(request.NewRelation) + ".</BASEFONT>";

                AddNoticeToOfficials(request.TargetCityId, ReinoDiplomacyApprovalCategory.Defense, "Relação Diplomática", html, true);
            }
        }

        private static bool TryConsumeCityResources(int cityId, ReinoDiplomacyResourceBundle bundle)
        {
            if (bundle == null || bundle.IsEmpty)
                return true;

            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null)
                return false;

            if (ledger.Gold < bundle.Gold || ledger.Wood < bundle.Wood || ledger.Iron < bundle.Iron || ledger.Cloth < bundle.Cloth)
                return false;

            ledger.Add(ReinoResourceType.Gold, -bundle.Gold);
            ledger.Add(ReinoResourceType.Wood, -bundle.Wood);
            ledger.Add(ReinoResourceType.Iron, -bundle.Iron);
            ledger.Add(ReinoResourceType.Cloth, -bundle.Cloth);
            return true;
        }

        private static void AddCityResources(int cityId, ReinoDiplomacyResourceBundle bundle)
        {
            if (bundle == null || bundle.IsEmpty)
                return;

            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null)
                return;

            ledger.Add(ReinoResourceType.Gold, bundle.Gold);
            ledger.Add(ReinoResourceType.Wood, bundle.Wood);
            ledger.Add(ReinoResourceType.Iron, bundle.Iron);
            ledger.Add(ReinoResourceType.Cloth, bundle.Cloth);
        }

        private static void ApplyResourceDonation(ReinoDiplomacyRequest request)
        {
            if (!TryConsumeCityResources(request.SourceCityId, request.ResourceBundle))
            {
                AddNoticeToOfficials(request.SourceCityId, ReinoDiplomacyApprovalCategory.Economy, "Doação de Recursos", "<BASEFONT COLOR=#000000>O tesouro do reino não tinha recursos suficientes para concluir a doação aprovada.</BASEFONT>", true);
                return;
            }

            AddCityResources(request.TargetCityId, request.ResourceBundle);
            ReinoTreasurySystem.RecordDiplomacyExpense(request.SourceCityId, request.ResourceBundle.Gold, request.ResourceBundle.Cloth, request.ResourceBundle.Iron, request.ResourceBundle.Wood);
            ReinoTreasurySystem.RecordDiplomacyIncome(request.TargetCityId, request.ResourceBundle.Gold, request.ResourceBundle.Cloth, request.ResourceBundle.Iron, request.ResourceBundle.Wood);

            string html = "<BASEFONT COLOR=#000000><BIG><B>Doação de Recursos</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(request.SourceCityId) + " enviou " + FormatBundleLine(request.ResourceBundle) + " ao seu tesouro.</BASEFONT>";
            AddNoticeToOfficials(request.TargetCityId, ReinoDiplomacyApprovalCategory.Economy, "Doação de Recursos", html, true);
        }

        private static void ApplyPostoDonation(ReinoDiplomacyRequest request)
        {
            PostoDefinition def = PostoSystem.GetDefinition(request.PostoId);
            PostoState state = PostoSystem.GetState(request.PostoId);
            if (def == null || state == null)
                return;

            if (state.ContestEndsUtc > DateTime.UtcNow)
            {
                AddNoticeToLeader(request.SourceCityId, "Doação de Posto", "<BASEFONT COLOR=#000000>O posto entrou em disputa e não pôde ser doado.</BASEFONT>", true);
                return;
            }

            string oldOwner = state.OwnerCityId;
            state.PreviousOwnerCityId = oldOwner;
            state.OwnerCityId = ReinoElectionsSystem.GetCityName(request.TargetCityId);
            state.ProgressCityId = String.Empty;
            state.ContestEndsUtc = DateTime.MinValue;
            state.ContestScores.Clear();
            state.LastConqueredUtc = DateTime.UtcNow;
            state.DonatedByCityId = ReinoElectionsSystem.GetCityName(request.SourceCityId);

            string html = "<BASEFONT COLOR=#000000><BIG><B>Doação de Posto</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(request.SourceCityId) + " concluiu a doação do posto " + def.Name + " para o seu reino.<BR><BR>Local: " + PostoSystem.GetChestLocationLabel(def.Id) + ".</BASEFONT>";
            AddNoticeToOfficials(request.TargetCityId, ReinoDiplomacyApprovalCategory.Defense, "Doação de Posto", html, true);
        }

        private static void ApplyAgreement(ReinoDiplomacyRequest request)
        {
            string key = PairKey(request.SourceCityId, request.TargetCityId);
            ReinoDiplomacyAgreement agreement = new ReinoDiplomacyAgreement();
            agreement.SourceCityId = request.SourceCityId;
            agreement.TargetCityId = request.TargetCityId;
            agreement.SendFromSource = request.AgreementSourceSend.Clone();
            agreement.SendFromTarget = request.AgreementTargetSend.Clone();
            agreement.CreatedUtc = DateTime.UtcNow;
            agreement.NextRunUtc = GetNextMonday19Utc(DateTime.UtcNow);
            m_Agreements[key] = agreement;

            bool sourceEnough = HasEnoughResources(agreement.SourceCityId, agreement.SendFromSource);
            bool targetEnough = HasEnoughResources(agreement.TargetCityId, agreement.SendFromTarget);

            if (sourceEnough && targetEnough)
            {
                TryConsumeCityResources(agreement.SourceCityId, agreement.SendFromSource);
                TryConsumeCityResources(agreement.TargetCityId, agreement.SendFromTarget);
                AddCityResources(agreement.TargetCityId, agreement.SendFromSource);
                AddCityResources(agreement.SourceCityId, agreement.SendFromTarget);
                ReinoTreasurySystem.RecordDiplomacyExpense(agreement.SourceCityId, agreement.SendFromSource.Gold, agreement.SendFromSource.Cloth, agreement.SendFromSource.Iron, agreement.SendFromSource.Wood);
                ReinoTreasurySystem.RecordDiplomacyExpense(agreement.TargetCityId, agreement.SendFromTarget.Gold, agreement.SendFromTarget.Cloth, agreement.SendFromTarget.Iron, agreement.SendFromTarget.Wood);
                ReinoTreasurySystem.RecordDiplomacyIncome(agreement.TargetCityId, agreement.SendFromSource.Gold, agreement.SendFromSource.Cloth, agreement.SendFromSource.Iron, agreement.SendFromSource.Wood);
                ReinoTreasurySystem.RecordDiplomacyIncome(agreement.SourceCityId, agreement.SendFromTarget.Gold, agreement.SendFromTarget.Cloth, agreement.SendFromTarget.Iron, agreement.SendFromTarget.Wood);
            }
            else
            {
                agreement.GraceActive = true;
                agreement.GraceDebtorCityId = !sourceEnough ? agreement.SourceCityId : agreement.TargetCityId;
                agreement.GraceEndsUtc = GetNextTuesday19Utc(DateTime.UtcNow);

                int debtor = agreement.GraceDebtorCityId;
                int other = debtor == agreement.SourceCityId ? agreement.TargetCityId : agreement.SourceCityId;
                AddNoticeToOfficials(debtor, ReinoDiplomacyApprovalCategory.Economy, "Acordo Semanal", "<BASEFONT COLOR=#000000>A primeira remessa do acordo não pôde ser enviada por falta de recursos. O reino tem até terça-feira às 19h para regularizar o tesouro ou o acordo será quebrado.</BASEFONT>", true);
                AddNoticeToOfficials(other, ReinoDiplomacyApprovalCategory.Economy, "Acordo Semanal", "<BASEFONT COLOR=#000000>A primeira remessa do acordo ainda não foi concluída porque o outro reino está regularizando o tesouro. Se os recursos não forem entregues até terça-feira às 19h, o acordo será quebrado.</BASEFONT>", true);
            }

            string htmlSource = "<BASEFONT COLOR=#000000><BIG><B>Proposta de Acordo</B></BIG><BR><BR>O acordo semanal foi firmado.<BR><BR>Seu reino enviará " + FormatBundleLine(agreement.SendFromSource) + " e receberá " + FormatBundleLine(agreement.SendFromTarget) + ".</BASEFONT>";
            string htmlTarget = "<BASEFONT COLOR=#000000><BIG><B>Proposta de Acordo</B></BIG><BR><BR>O acordo semanal foi firmado.<BR><BR>Seu reino enviará " + FormatBundleLine(agreement.SendFromTarget) + " e receberá " + FormatBundleLine(agreement.SendFromSource) + ".</BASEFONT>";
            AddNoticeToOfficials(request.SourceCityId, ReinoDiplomacyApprovalCategory.Economy, "Proposta de Acordo", htmlSource, true);
            AddNoticeToOfficials(request.TargetCityId, ReinoDiplomacyApprovalCategory.Economy, "Proposta de Acordo", htmlTarget, true);
        }

        private static void ApplyBorders(ReinoDiplomacyRequest request)
        {
            request.BorderPolicy.SourceCityId = request.SourceCityId;
            request.BorderPolicy.TargetCityId = request.TargetCityId;
            m_Borders[DirectionKey(request.SourceCityId, request.TargetCityId)] = request.BorderPolicy.Clone();
        }

        private static void ApplyBlockade(ReinoDiplomacyRequest request)
        {
            request.BlockadePolicy.SourceCityId = request.SourceCityId;
            request.BlockadePolicy.TargetCityId = request.TargetCityId;
            m_Blockades[DirectionKey(request.SourceCityId, request.TargetCityId)] = request.BlockadePolicy.Clone();

            if (request.BlockadePolicy.CancelAgreements)
                ApplyAgreementCancel(request.SourceCityId, request.TargetCityId, false);
        }

        private static void ApplyTribute(ReinoDiplomacyRequest request)
        {
            string key = DirectionKey(request.SourceCityId, request.TargetCityId);
            ReinoDiplomacyTribute tribute = new ReinoDiplomacyTribute();
            tribute.DemandingCityId = request.SourceCityId;
            tribute.PayingCityId = request.TargetCityId;
            tribute.Bundle = request.Tribute.Bundle.Clone();
            tribute.Frequency = request.Tribute.Frequency;
            tribute.CreatedUtc = DateTime.UtcNow;
            tribute.NextRunUtc = GetFirstTributeRunUtc(tribute.Frequency);
            m_Tributes[key] = tribute;

            string html = "<BASEFONT COLOR=#000000><BIG><B>Exigência de Tributos</B></BIG><BR><BR>Seu reino aceitou a exigência de " + FormatBundleLine(tribute.Bundle) + " com frequência " + GetTributeFrequencyLabel(tribute.Frequency).ToLower() + ".</BASEFONT>";
            AddNoticeToOfficials(request.SourceCityId, ReinoDiplomacyApprovalCategory.Defense, "Exigência de Tributos", html, true);
            AddNoticeToOfficials(request.TargetCityId, ReinoDiplomacyApprovalCategory.Defense, "Exigência de Tributos", html, true);
        }

        private static void ProcessAgreements()
        {
            List<ReinoDiplomacyAgreement> list = new List<ReinoDiplomacyAgreement>(m_Agreements.Values);
            DateTime now = DateTime.UtcNow;

            for (int i = 0; i < list.Count; i++)
            {
                ReinoDiplomacyAgreement agreement = list[i];
                if (agreement == null)
                    continue;

                if (!agreement.GraceActive && now >= agreement.NextRunUtc)
                {
                    bool sourceEnough = HasEnoughResources(agreement.SourceCityId, agreement.SendFromSource);
                    bool targetEnough = HasEnoughResources(agreement.TargetCityId, agreement.SendFromTarget);

                    if (sourceEnough && targetEnough)
                    {
                        TryConsumeCityResources(agreement.SourceCityId, agreement.SendFromSource);
                        TryConsumeCityResources(agreement.TargetCityId, agreement.SendFromTarget);
                        AddCityResources(agreement.TargetCityId, agreement.SendFromSource);
                        AddCityResources(agreement.SourceCityId, agreement.SendFromTarget);
                        ReinoTreasurySystem.RecordDiplomacyExpense(agreement.SourceCityId, agreement.SendFromSource.Gold, agreement.SendFromSource.Cloth, agreement.SendFromSource.Iron, agreement.SendFromSource.Wood);
                        ReinoTreasurySystem.RecordDiplomacyExpense(agreement.TargetCityId, agreement.SendFromTarget.Gold, agreement.SendFromTarget.Cloth, agreement.SendFromTarget.Iron, agreement.SendFromTarget.Wood);
                        ReinoTreasurySystem.RecordDiplomacyIncome(agreement.TargetCityId, agreement.SendFromSource.Gold, agreement.SendFromSource.Cloth, agreement.SendFromSource.Iron, agreement.SendFromSource.Wood);
                        ReinoTreasurySystem.RecordDiplomacyIncome(agreement.SourceCityId, agreement.SendFromTarget.Gold, agreement.SendFromTarget.Cloth, agreement.SendFromTarget.Iron, agreement.SendFromTarget.Wood);
                        agreement.NextRunUtc = GetNextMonday19Utc(now.AddHours(1.0));
                    }
                    else
                    {
                        int debtor = !sourceEnough ? agreement.SourceCityId : agreement.TargetCityId;
                        agreement.GraceActive = true;
                        agreement.GraceDebtorCityId = debtor;
                        agreement.GraceEndsUtc = GetNextTuesday19Utc(now);

                        int other = debtor == agreement.SourceCityId ? agreement.TargetCityId : agreement.SourceCityId;
                        AddNoticeToOfficials(debtor, ReinoDiplomacyApprovalCategory.Economy, "Acordo Semanal", "<BASEFONT COLOR=#000000>Faltaram recursos para cumprir o acordo semanal. O reino tem 24 horas para completar o tesouro ou o acordo será quebrado.</BASEFONT>", true);
                        AddNoticeToOfficials(other, ReinoDiplomacyApprovalCategory.Economy, "Acordo Semanal", "<BASEFONT COLOR=#000000>O outro reino informou atraso de recursos e tem 24 horas para concluir o envio antes da quebra do acordo.</BASEFONT>", true);
                    }
                }
                else if (agreement.GraceActive && now >= agreement.GraceEndsUtc)
                {
                    if (HasEnoughResources(agreement.SourceCityId, agreement.SendFromSource) && HasEnoughResources(agreement.TargetCityId, agreement.SendFromTarget))
                    {
                        TryConsumeCityResources(agreement.SourceCityId, agreement.SendFromSource);
                        TryConsumeCityResources(agreement.TargetCityId, agreement.SendFromTarget);
                        AddCityResources(agreement.TargetCityId, agreement.SendFromSource);
                        AddCityResources(agreement.SourceCityId, agreement.SendFromTarget);
                        ReinoTreasurySystem.RecordDiplomacyExpense(agreement.SourceCityId, agreement.SendFromSource.Gold, agreement.SendFromSource.Cloth, agreement.SendFromSource.Iron, agreement.SendFromSource.Wood);
                        ReinoTreasurySystem.RecordDiplomacyExpense(agreement.TargetCityId, agreement.SendFromTarget.Gold, agreement.SendFromTarget.Cloth, agreement.SendFromTarget.Iron, agreement.SendFromTarget.Wood);
                        ReinoTreasurySystem.RecordDiplomacyIncome(agreement.TargetCityId, agreement.SendFromSource.Gold, agreement.SendFromSource.Cloth, agreement.SendFromSource.Iron, agreement.SendFromSource.Wood);
                        ReinoTreasurySystem.RecordDiplomacyIncome(agreement.SourceCityId, agreement.SendFromTarget.Gold, agreement.SendFromTarget.Cloth, agreement.SendFromTarget.Iron, agreement.SendFromTarget.Wood);
                        agreement.GraceActive = false;
                        agreement.GraceDebtorCityId = -1;
                        agreement.NextRunUtc = GetNextMonday19Utc(now.AddHours(1.0));
                    }
                    else
                    {
                        ApplyAgreementCancel(agreement.SourceCityId, agreement.TargetCityId, false);
                    }
                }
            }
        }

        private static bool HasEnoughResources(int cityId, ReinoDiplomacyResourceBundle bundle)
        {
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null)
                return false;

            return ledger.Gold >= bundle.Gold && ledger.Wood >= bundle.Wood && ledger.Iron >= bundle.Iron && ledger.Cloth >= bundle.Cloth;
        }

        private static void ApplyAgreementCancel(int cityA, int cityB, bool manual)
        {
            ReinoDiplomacyAgreement agreement = GetAgreement(cityA, cityB);
            if (agreement == null)
                return;

            m_Agreements.Remove(PairKey(cityA, cityB));
            string html = "<BASEFONT COLOR=#000000><BIG><B>Quebra de Acordo</B></BIG><BR><BR>O acordo vigente foi encerrado.<BR><BR>Ele previa enviar " + FormatBundleLine(agreement.SendFromSource) + " e receber " + FormatBundleLine(agreement.SendFromTarget) + ".</BASEFONT>";
            AddNoticeToOfficials(agreement.SourceCityId, ReinoDiplomacyApprovalCategory.Economy, "Quebra de Acordo", html, true);
            AddNoticeToOfficials(agreement.TargetCityId, ReinoDiplomacyApprovalCategory.Economy, "Quebra de Acordo", html, true);
        }

        private static void ProcessTributes()
        {
            List<ReinoDiplomacyTribute> list = new List<ReinoDiplomacyTribute>(m_Tributes.Values);
            DateTime now = DateTime.UtcNow;

            for (int i = 0; i < list.Count; i++)
            {
                ReinoDiplomacyTribute tribute = list[i];
                if (tribute == null || now < tribute.NextRunUtc)
                    continue;

                if (!TryConsumeCityResources(tribute.PayingCityId, tribute.Bundle))
                {
                    string html = "<BASEFONT COLOR=#000000><BIG><B>Tributo Rompido</B></BIG><BR><BR>O reino não possuía recursos suficientes para pagar o tributo exigido. A cobrança foi encerrada.</BASEFONT>";
                    AddNoticeToOfficials(tribute.DemandingCityId, ReinoDiplomacyApprovalCategory.Defense, "Exigência de Tributos", html, true);
                    AddNoticeToOfficials(tribute.PayingCityId, ReinoDiplomacyApprovalCategory.Defense, "Exigência de Tributos", html, true);
                    m_Tributes.Remove(DirectionKey(tribute.DemandingCityId, tribute.PayingCityId));
                    continue;
                }

                AddCityResources(tribute.DemandingCityId, tribute.Bundle);
                ReinoTreasurySystem.RecordDiplomacyExpense(tribute.PayingCityId, tribute.Bundle.Gold, tribute.Bundle.Cloth, tribute.Bundle.Iron, tribute.Bundle.Wood);
                ReinoTreasurySystem.RecordDiplomacyIncome(tribute.DemandingCityId, tribute.Bundle.Gold, tribute.Bundle.Cloth, tribute.Bundle.Iron, tribute.Bundle.Wood);

                if (tribute.Frequency == ReinoDiplomacyTributeFrequency.Once)
                {
                    m_Tributes.Remove(DirectionKey(tribute.DemandingCityId, tribute.PayingCityId));
                }
                else
                {
                    tribute.NextRunUtc = AdvanceTributeRunUtc(tribute.NextRunUtc, tribute.Frequency);
                }
            }
        }

        public static string GetTributeFrequencyLabel(ReinoDiplomacyTributeFrequency frequency)
        {
            switch (frequency)
            {
                case ReinoDiplomacyTributeFrequency.Daily: return "Diariamente";
                case ReinoDiplomacyTributeFrequency.Weekly: return "Semanalmente";
                case ReinoDiplomacyTributeFrequency.Monthly: return "Mensalmente";
                default: return "Uma Vez";
            }
        }

        private static ReinoDiplomacyRequest CreateBaseRequest(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyActionKind action, ReinoDiplomacyApprovalCategory category, bool requiresTargetDecision)
        {
            ReinoDiplomacyRequest request = new ReinoDiplomacyRequest();
            request.RequestId = m_NextRequestId++;
            request.Action = action;
            request.Category = category;
            request.SourceCityId = sourceCityId;
            request.TargetCityId = targetCityId;
            request.CreatedBySerial = actor != null ? actor.Serial.Value : 0;
            request.CreatedByName = actor != null ? actor.Name : String.Empty;
            request.CreatedUtc = DateTime.UtcNow;
            request.ExpiresUtc = request.CreatedUtc.AddHours(48.0);
            request.State = ReinoDiplomacyRequestState.PendingSourceApproval;
            request.RequiresTargetDecision = requiresTargetDecision;
            request.SourceVotes = BuildVotes(sourceCityId, category, request.CreatedBySerial, true);
            request.TargetVotes = requiresTargetDecision ? BuildVotes(targetCityId, category, 0, true) : new List<ReinoDiplomacyVote>();
            m_Requests.Add(request);
            return request;
        }

        private static void ActivateRequest(ReinoDiplomacyRequest request)
        {
            if (request == null)
                return;

            if (request.SourceVotes != null && request.SourceVotes.Count > 0)
            {
                NotifyVotes(request.SourceVotes);
            }
            else if (request.RequiresTargetDecision && request.TargetVotes != null && request.TargetVotes.Count > 0)
            {
                request.State = ReinoDiplomacyRequestState.PendingTargetApproval;
                request.ExpiresUtc = DateTime.UtcNow.AddHours(48.0);
                NotifyVotes(request.TargetVotes);
            }
            else
            {
                FinalizeRequest(request, true);
            }
        }

        public static bool SubmitRelationChange(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyRelationStatus newRelation, out string message)
        {
            message = String.Empty;
            ReinoDiplomacyRelationStatus oldRelation = GetRelation(sourceCityId, targetCityId);
            if (targetCityId < 0 || targetCityId == sourceCityId)
            {
                message = "Selecione um reino válido.";
                return false;
            }

            if (oldRelation == newRelation)
            {
                message = "Essa relação já está definida assim.";
                return false;
            }

            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.ChangeRelation, ReinoDiplomacyApprovalCategory.Defense, newRelation == ReinoDiplomacyRelationStatus.Allied);
            request.OldRelation = oldRelation;
            request.NewRelation = newRelation;
            request.SourceTitle = "Relação Diplomática";
            request.TargetTitle = "Relação Diplomática";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Relação Diplomática</B></BIG><BR><BR>O governante solicitou alterar a relação com " + ReinoElectionsSystem.GetCityPeopleName(targetCityId) + " de " + GetRelationLabel(oldRelation) + " para " + GetRelationLabel(newRelation) + ".<BR><BR>Essa decisão precisa da aprovação interna antes de entrar em vigor.</BASEFONT>";
            request.TargetHtml = newRelation == ReinoDiplomacyRelationStatus.Allied
                ? "<BASEFONT COLOR=#000000><BIG><B>Relação Diplomática</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " estende uma proposta formal de aliança ao seu reino.<BR><BR>Se ela for aceita, os dois reinos passarão a figurar como aliados no painel diplomático.</BASEFONT>"
                : "<BASEFONT COLOR=#000000><BIG><B>Relação Diplomática</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " informa a alteração da relação diplomática para " + GetRelationLabel(newRelation) + ".</BASEFONT>";
            ActivateRequest(request);
            message = "A mudança diplomática foi enviada para aprovação.";
            return true;
        }

        public static bool SubmitResourceDonation(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyResourceBundle bundle, out string message)
        {
            message = String.Empty;
            if (bundle == null || bundle.IsEmpty)
            {
                message = "Informe ao menos um recurso para doar.";
                return false;
            }

            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.DonateResources, ReinoDiplomacyApprovalCategory.Economy, false);
            request.ResourceBundle = bundle.Clone();
            request.SourceTitle = "Doação de Recursos";
            request.TargetTitle = "Doação de Recursos";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Doação de Recursos</B></BIG><BR><BR>O reino propõe doar " + FormatBundleLine(bundle) + " ao reino de " + ReinoElectionsSystem.GetCityPeopleName(targetCityId) + ".</BASEFONT>";
            request.TargetHtml = "<BASEFONT COLOR=#000000><BIG><B>Doação de Recursos</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " enviará " + FormatBundleLine(bundle) + " ao seu reino assim que a aprovação interna deles for concluída.</BASEFONT>";
            ActivateRequest(request);
            message = "A doação de recursos foi enviada para aprovação.";
            return true;
        }

        public static bool SubmitPostoDonation(PlayerMobile actor, int sourceCityId, int targetCityId, string postoId, out string message)
        {
            message = String.Empty;
            PostoDefinition def = PostoSystem.GetDefinition(postoId);
            PostoState state = PostoSystem.GetState(postoId);
            if (def == null || state == null)
            {
                message = "Posto inválido.";
                return false;
            }

            if (state.ContestEndsUtc > DateTime.UtcNow)
            {
                message = "Postos em disputa não podem ser doados.";
                return false;
            }

            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.DonatePosto, ReinoDiplomacyApprovalCategory.Defense, true);
            request.PostoId = postoId;
            request.SourceTitle = "Doação de Posto";
            request.TargetTitle = "Doação de Posto";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Doação de Posto</B></BIG><BR><BR>O reino propõe doar o posto " + def.Name + " ao reino de " + ReinoElectionsSystem.GetCityPeopleName(targetCityId) + ".</BASEFONT>";
            request.TargetHtml = "<BASEFONT COLOR=#000000><BIG><B>Doação de Posto</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " oferece o posto " + def.Name + ".<BR><BR>Local: " + PostoSystem.GetChestLocationLabel(postoId) + ".<BR><BR>Seu reino pode aceitar ou recusar o recebimento deste posto.</BASEFONT>";
            ActivateRequest(request);
            message = "A doação de posto foi enviada para aprovação.";
            return true;
        }

        public static bool SubmitAgreement(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyResourceBundle send, ReinoDiplomacyResourceBundle receive, out string message)
        {
            message = String.Empty;
            if ((send == null || send.IsEmpty) && (receive == null || receive.IsEmpty))
            {
                message = "Defina ao menos um valor de envio ou recebimento.";
                return false;
            }

            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.ProposeAgreement, ReinoDiplomacyApprovalCategory.Economy, true);
            request.AgreementSourceSend = send != null ? send.Clone() : new ReinoDiplomacyResourceBundle();
            request.AgreementTargetSend = receive != null ? receive.Clone() : new ReinoDiplomacyResourceBundle();
            request.SourceTitle = "Proposta de Acordo";
            request.TargetTitle = "Proposta de Acordo";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Proposta de Acordo</B></BIG><BR><BR>Seu reino propõe enviar " + FormatBundleLine(request.AgreementSourceSend) + " e receber " + FormatBundleLine(request.AgreementTargetSend) + " por semana.</BASEFONT>";
            request.TargetHtml = "<BASEFONT COLOR=#000000><BIG><B>Proposta de Acordo</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " propõe enviar " + FormatBundleLine(request.AgreementSourceSend) + " e receber de seu reino " + FormatBundleLine(request.AgreementTargetSend) + " toda semana.<BR><BR>Se o acordo for aceito, a primeira remessa será enviada imediatamente.</BASEFONT>";
            ActivateRequest(request);
            message = "A proposta de acordo foi enviada para aprovação.";
            return true;
        }

        public static bool CancelAgreement(PlayerMobile actor, int sourceCityId, int targetCityId, out string message)
        {
            message = String.Empty;
            if (GetAgreement(sourceCityId, targetCityId) == null)
            {
                message = "Não existe acordo vigente com este reino.";
                return false;
            }

            ApplyAgreementCancel(sourceCityId, targetCityId, true);
            message = "O acordo vigente foi encerrado.";
            return true;
        }

        public static bool SubmitBorders(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyBorderPolicy policy, out string message)
        {
            message = String.Empty;
            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.CloseBorders, ReinoDiplomacyApprovalCategory.Defense, false);
            request.BorderPolicy = policy != null ? policy.Clone() : new ReinoDiplomacyBorderPolicy();
            request.SourceTitle = "Fechar Fronteiras";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Fechar Fronteiras</B></BIG><BR><BR>O reino propõe atualizar as diretrizes de fronteira contra " + ReinoElectionsSystem.GetCityPeopleName(targetCityId) + ".</BASEFONT>";
            ActivateRequest(request);
            message = "As regras de fronteira foram enviadas para aprovação.";
            return true;
        }

        public static bool SubmitBlockade(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyCommercialBlockade blockade, out string message)
        {
            message = String.Empty;
            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.CommercialBlockade, ReinoDiplomacyApprovalCategory.Defense, false);
            request.BlockadePolicy = blockade != null ? blockade.Clone() : new ReinoDiplomacyCommercialBlockade();
            request.SourceTitle = "Bloqueio Comercial";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Bloqueio Comercial</B></BIG><BR><BR>O reino propõe novas restrições econômicas contra " + ReinoElectionsSystem.GetCityPeopleName(targetCityId) + ".</BASEFONT>";
            ActivateRequest(request);
            message = "O bloqueio comercial foi enviado para aprovação.";
            return true;
        }

        public static bool SubmitTribute(PlayerMobile actor, int sourceCityId, int targetCityId, ReinoDiplomacyResourceBundle bundle, ReinoDiplomacyTributeFrequency frequency, out string message)
        {
            message = String.Empty;
            if (bundle == null || bundle.IsEmpty)
            {
                message = "Informe ao menos um recurso para o tributo.";
                return false;
            }

            ReinoDiplomacyRequest request = CreateBaseRequest(actor, sourceCityId, targetCityId, ReinoDiplomacyActionKind.DemandTribute, ReinoDiplomacyApprovalCategory.Defense, true);
            request.Tribute = new ReinoDiplomacyTribute();
            request.Tribute.DemandingCityId = sourceCityId;
            request.Tribute.PayingCityId = targetCityId;
            request.Tribute.Bundle = bundle.Clone();
            request.Tribute.Frequency = frequency;
            request.SourceTitle = "Exigência de Tributos";
            request.TargetTitle = "Exigência de Tributos";
            request.SourceHtml = "<BASEFONT COLOR=#000000><BIG><B>Exigência de Tributos</B></BIG><BR><BR>O reino propõe exigir " + FormatBundleLine(bundle) + " com frequência " + GetTributeFrequencyLabel(frequency).ToLower() + ".</BASEFONT>";
            request.TargetHtml = "<BASEFONT COLOR=#000000><BIG><B>Exigência de Tributos</B></BIG><BR><BR>O reino de " + ReinoElectionsSystem.GetCityPeopleName(sourceCityId) + " exige o pagamento de " + FormatBundleLine(bundle) + " com frequência " + GetTributeFrequencyLabel(frequency).ToLower() + ".<BR><BR>Seu reino tem até 48 horas para responder a esta exigência.</BASEFONT>";
            ActivateRequest(request);
            message = "A exigência de tributos foi enviada para aprovação.";
            return true;
        }

        private static void WriteWarCitizenWarning(BinaryWriter bw, ReinoDiplomacyWarCitizenWarning value)
        {
            bw.Write(value != null ? value.PlayerSerial : 0);
            bw.Write(value != null ? value.ForeignCitizenCityId : -1);
            bw.Write(value != null ? value.CapitalCityId : -1);
            bw.Write(value != null ? value.StartedUtc.ToBinary() : DateTime.UtcNow.ToBinary());
        }

        private static ReinoDiplomacyWarCitizenWarning ReadWarCitizenWarning(BinaryReader br)
        {
            ReinoDiplomacyWarCitizenWarning value = new ReinoDiplomacyWarCitizenWarning();
            value.PlayerSerial = br.ReadInt32();
            value.ForeignCitizenCityId = br.ReadInt32();
            value.CapitalCityId = br.ReadInt32();
            value.StartedUtc = DateTime.FromBinary(br.ReadInt64());
            return value;
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(2);
                    bw.Write(m_NextRequestId);
                    bw.Write(m_NextNoticeId);

                    bw.Write(m_Relations.Count);
                    foreach (KeyValuePair<string, ReinoDiplomacyRelationStatus> kv in m_Relations)
                    {
                        bw.Write(kv.Key);
                        bw.Write((int)kv.Value);
                    }

                    bw.Write(m_Borders.Count);
                    foreach (KeyValuePair<string, ReinoDiplomacyBorderPolicy> kv in m_Borders)
                    {
                        bw.Write(kv.Key);
                        WriteBorder(bw, kv.Value);
                    }

                    bw.Write(m_Blockades.Count);
                    foreach (KeyValuePair<string, ReinoDiplomacyCommercialBlockade> kv in m_Blockades)
                    {
                        bw.Write(kv.Key);
                        WriteBlockade(bw, kv.Value);
                    }

                    bw.Write(m_Agreements.Count);
                    foreach (KeyValuePair<string, ReinoDiplomacyAgreement> kv in m_Agreements)
                    {
                        bw.Write(kv.Key);
                        WriteAgreement(bw, kv.Value);
                    }

                    bw.Write(m_Tributes.Count);
                    foreach (KeyValuePair<string, ReinoDiplomacyTribute> kv in m_Tributes)
                    {
                        bw.Write(kv.Key);
                        WriteTribute(bw, kv.Value);
                    }

                    bw.Write(m_Requests.Count);
                    for (int i = 0; i < m_Requests.Count; i++)
                        WriteRequest(bw, m_Requests[i]);

                    bw.Write(m_Notices.Count);
                    for (int i = 0; i < m_Notices.Count; i++)
                        WriteNotice(bw, m_Notices[i]);

                    bw.Write(m_WarCitizenWarnings.Count);
                    for (int i = 0; i < m_WarCitizenWarnings.Count; i++)
                        WriteWarCitizenWarning(bw, m_WarCitizenWarnings[i]);
                }
            }
            catch
            {
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    m_NextRequestId = br.ReadInt32();
                    m_NextNoticeId = br.ReadInt32();

                    m_Relations.Clear();
                    int relationCount = br.ReadInt32();
                    for (int i = 0; i < relationCount; i++)
                        m_Relations[br.ReadString()] = (ReinoDiplomacyRelationStatus)br.ReadInt32();

                    m_Borders.Clear();
                    int borderCount = br.ReadInt32();
                    for (int i = 0; i < borderCount; i++)
                        m_Borders[br.ReadString()] = ReadBorder(br);

                    m_Blockades.Clear();
                    int blockadeCount = br.ReadInt32();
                    for (int i = 0; i < blockadeCount; i++)
                        m_Blockades[br.ReadString()] = ReadBlockade(br);

                    m_Agreements.Clear();
                    int agreementCount = br.ReadInt32();
                    for (int i = 0; i < agreementCount; i++)
                        m_Agreements[br.ReadString()] = ReadAgreement(br);

                    m_Tributes.Clear();
                    int tributeCount = br.ReadInt32();
                    for (int i = 0; i < tributeCount; i++)
                        m_Tributes[br.ReadString()] = ReadTribute(br);

                    m_Requests.Clear();
                    int requestCount = br.ReadInt32();
                    for (int i = 0; i < requestCount; i++)
                        m_Requests.Add(ReadRequest(br));

                    m_Notices.Clear();
                    int noticeCount = br.ReadInt32();
                    for (int i = 0; i < noticeCount; i++)
                        m_Notices.Add(ReadNotice(br));

                    m_WarCitizenWarnings.Clear();

                    if (version >= 2)
                    {
                        int warningCount = br.ReadInt32();
                        for (int i = 0; i < warningCount; i++)
                            m_WarCitizenWarnings.Add(ReadWarCitizenWarning(br));
                    }
                }
            }
            catch
            {
            }
        }

        private static void WriteBundle(BinaryWriter bw, ReinoDiplomacyResourceBundle bundle)
        {
            bw.Write(bundle != null ? bundle.Gold : 0);
            bw.Write(bundle != null ? bundle.Wood : 0);
            bw.Write(bundle != null ? bundle.Iron : 0);
            bw.Write(bundle != null ? bundle.Cloth : 0);
        }

        private static ReinoDiplomacyResourceBundle ReadBundle(BinaryReader br)
        {
            return new ReinoDiplomacyResourceBundle(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        }

        private static void WriteVotes(BinaryWriter bw, List<ReinoDiplomacyVote> votes)
        {
            bw.Write(votes != null ? votes.Count : 0);
            if (votes == null)
                return;

            for (int i = 0; i < votes.Count; i++)
            {
                bw.Write(votes[i].VoterSerial);
                bw.Write(votes[i].VoterName ?? String.Empty);
                bw.Write(votes[i].Decision);
                bw.Write(votes[i].DecisionUtc.ToBinary());
            }
        }

        private static List<ReinoDiplomacyVote> ReadVotes(BinaryReader br)
        {
            int count = br.ReadInt32();
            List<ReinoDiplomacyVote> list = new List<ReinoDiplomacyVote>();
            for (int i = 0; i < count; i++)
            {
                ReinoDiplomacyVote vote = new ReinoDiplomacyVote();
                vote.VoterSerial = br.ReadInt32();
                vote.VoterName = br.ReadString();
                vote.Decision = br.ReadInt32();
                vote.DecisionUtc = DateTime.FromBinary(br.ReadInt64());
                list.Add(vote);
            }
            return list;
        }

        private static void WriteBorder(BinaryWriter bw, ReinoDiplomacyBorderPolicy value)
        {
            bw.Write(value != null ? value.SourceCityId : -1);
            bw.Write(value != null ? value.TargetCityId : -1);
            bw.Write(value != null && value.BlockEnemyCitizens);
            bw.Write(value != null && value.BlockEnemyCulture);
            bw.Write(value != null && value.BlockEnemyAllies);
            bw.Write(value != null && value.AllowEntry);
        }

        private static ReinoDiplomacyBorderPolicy ReadBorder(BinaryReader br)
        {
            ReinoDiplomacyBorderPolicy value = new ReinoDiplomacyBorderPolicy();
            value.SourceCityId = br.ReadInt32();
            value.TargetCityId = br.ReadInt32();
            value.BlockEnemyCitizens = br.ReadBoolean();
            value.BlockEnemyCulture = br.ReadBoolean();
            value.BlockEnemyAllies = br.ReadBoolean();
            value.AllowEntry = br.ReadBoolean();
            return value;
        }

        private static void WriteBlockade(BinaryWriter bw, ReinoDiplomacyCommercialBlockade value)
        {
            bw.Write(value != null ? value.SourceCityId : -1);
            bw.Write(value != null ? value.TargetCityId : -1);
            bw.Write(value != null && value.BlockRepresentative);
            bw.Write(value != null && value.CancelAgreements);
            bw.Write(value != null && value.CancelDonations);
            bw.Write(value != null && value.BlockPlayerVendors);
        }

        private static ReinoDiplomacyCommercialBlockade ReadBlockade(BinaryReader br)
        {
            ReinoDiplomacyCommercialBlockade value = new ReinoDiplomacyCommercialBlockade();
            value.SourceCityId = br.ReadInt32();
            value.TargetCityId = br.ReadInt32();
            value.BlockRepresentative = br.ReadBoolean();
            value.CancelAgreements = br.ReadBoolean();
            value.CancelDonations = br.ReadBoolean();
            value.BlockPlayerVendors = br.ReadBoolean();
            return value;
        }

        private static void WriteAgreement(BinaryWriter bw, ReinoDiplomacyAgreement value)
        {
            bw.Write(value.SourceCityId);
            bw.Write(value.TargetCityId);
            WriteBundle(bw, value.SendFromSource);
            WriteBundle(bw, value.SendFromTarget);
            bw.Write(value.CreatedUtc.ToBinary());
            bw.Write(value.NextRunUtc.ToBinary());
            bw.Write(value.GraceActive);
            bw.Write(value.GraceEndsUtc.ToBinary());
            bw.Write(value.GraceDebtorCityId);
        }

        private static ReinoDiplomacyAgreement ReadAgreement(BinaryReader br)
        {
            ReinoDiplomacyAgreement value = new ReinoDiplomacyAgreement();
            value.SourceCityId = br.ReadInt32();
            value.TargetCityId = br.ReadInt32();
            value.SendFromSource = ReadBundle(br);
            value.SendFromTarget = ReadBundle(br);
            value.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
            value.NextRunUtc = DateTime.FromBinary(br.ReadInt64());
            value.GraceActive = br.ReadBoolean();
            value.GraceEndsUtc = DateTime.FromBinary(br.ReadInt64());
            value.GraceDebtorCityId = br.ReadInt32();
            return value;
        }

        private static void WriteTribute(BinaryWriter bw, ReinoDiplomacyTribute value)
        {
            bw.Write(value.DemandingCityId);
            bw.Write(value.PayingCityId);
            WriteBundle(bw, value.Bundle);
            bw.Write((int)value.Frequency);
            bw.Write(value.CreatedUtc.ToBinary());
            bw.Write(value.NextRunUtc.ToBinary());
        }

        private static ReinoDiplomacyTribute ReadTribute(BinaryReader br)
        {
            ReinoDiplomacyTribute value = new ReinoDiplomacyTribute();
            value.DemandingCityId = br.ReadInt32();
            value.PayingCityId = br.ReadInt32();
            value.Bundle = ReadBundle(br);
            value.Frequency = (ReinoDiplomacyTributeFrequency)br.ReadInt32();
            value.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
            value.NextRunUtc = DateTime.FromBinary(br.ReadInt64());
            return value;
        }

        private static void WriteRequest(BinaryWriter bw, ReinoDiplomacyRequest value)
        {
            bw.Write(value.RequestId);
            bw.Write((int)value.Action);
            bw.Write((int)value.Category);
            bw.Write(value.SourceCityId);
            bw.Write(value.TargetCityId);
            bw.Write(value.CreatedBySerial);
            bw.Write(value.CreatedByName ?? String.Empty);
            bw.Write(value.CreatedUtc.ToBinary());
            bw.Write(value.ResolvedUtc.ToBinary());
            bw.Write(value.ExpiresUtc.ToBinary());
            bw.Write((int)value.State);
            bw.Write(value.RequiresTargetDecision);
            bw.Write(value.SourceTitle ?? String.Empty);
            bw.Write(value.SourceHtml ?? String.Empty);
            bw.Write(value.TargetTitle ?? String.Empty);
            bw.Write(value.TargetHtml ?? String.Empty);
            bw.Write((int)value.OldRelation);
            bw.Write((int)value.NewRelation);
            WriteBundle(bw, value.ResourceBundle);
            bw.Write(value.PostoId ?? String.Empty);
            WriteBundle(bw, value.AgreementSourceSend);
            WriteBundle(bw, value.AgreementTargetSend);
            WriteBorder(bw, value.BorderPolicy);
            WriteBlockade(bw, value.BlockadePolicy);
            WriteTribute(bw, value.Tribute);
            WriteVotes(bw, value.SourceVotes);
            WriteVotes(bw, value.TargetVotes);
        }

        private static ReinoDiplomacyRequest ReadRequest(BinaryReader br)
        {
            ReinoDiplomacyRequest value = new ReinoDiplomacyRequest();
            value.RequestId = br.ReadInt32();
            value.Action = (ReinoDiplomacyActionKind)br.ReadInt32();
            value.Category = (ReinoDiplomacyApprovalCategory)br.ReadInt32();
            value.SourceCityId = br.ReadInt32();
            value.TargetCityId = br.ReadInt32();
            value.CreatedBySerial = br.ReadInt32();
            value.CreatedByName = br.ReadString();
            value.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
            value.ResolvedUtc = DateTime.FromBinary(br.ReadInt64());
            value.ExpiresUtc = DateTime.FromBinary(br.ReadInt64());
            value.State = (ReinoDiplomacyRequestState)br.ReadInt32();
            value.RequiresTargetDecision = br.ReadBoolean();
            value.SourceTitle = br.ReadString();
            value.SourceHtml = br.ReadString();
            value.TargetTitle = br.ReadString();
            value.TargetHtml = br.ReadString();
            value.OldRelation = (ReinoDiplomacyRelationStatus)br.ReadInt32();
            value.NewRelation = (ReinoDiplomacyRelationStatus)br.ReadInt32();
            value.ResourceBundle = ReadBundle(br);
            value.PostoId = br.ReadString();
            value.AgreementSourceSend = ReadBundle(br);
            value.AgreementTargetSend = ReadBundle(br);
            value.BorderPolicy = ReadBorder(br);
            value.BlockadePolicy = ReadBlockade(br);
            value.Tribute = ReadTribute(br);
            value.SourceVotes = ReadVotes(br);
            value.TargetVotes = ReadVotes(br);
            return value;
        }

        private static void WriteNotice(BinaryWriter bw, ReinoDiplomacyNotice value)
        {
            bw.Write(value.NoticeId);
            bw.Write(value.TargetSerial);
            bw.Write(value.Title ?? String.Empty);
            bw.Write(value.Html ?? String.Empty);
            bw.Write(value.Closable);
            bw.Write(value.Consumed);
            bw.Write(value.CreatedUtc.ToBinary());
        }

        private static ReinoDiplomacyNotice ReadNotice(BinaryReader br)
        {
            ReinoDiplomacyNotice value = new ReinoDiplomacyNotice();
            value.NoticeId = br.ReadInt32();
            value.TargetSerial = br.ReadInt32();
            value.Title = br.ReadString();
            value.Html = br.ReadString();
            value.Closable = br.ReadBoolean();
            value.Consumed = br.ReadBoolean();
            value.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
            return value;
        }
    }
}
