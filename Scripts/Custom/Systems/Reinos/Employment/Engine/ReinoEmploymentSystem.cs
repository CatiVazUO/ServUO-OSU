using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server.Engines.Craft;
using Server;
using Server.Custom.Correios;
using Server.Custom.Systems.Rent;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public enum ReinoCargoKind
    {
        None = 0,
        Leader,
        Ambassador,
        Dispatcher,
        CouncilMember,
        Priest,
        MinisterEconomy,
        MinisterDefense,
        Custom,
        CommercialRepresentative
    }

    public class ReinoCargoEntry
    {
        public int RoleId;
        public int CityId;
        public ReinoCargoKind Kind;
        public string Title;
        public string Description;
        public int WeeklySalaryGold;
        public int Hierarchy;
        public bool IsDefault;
        public bool IsRemovable;
        public bool IsEssential;
        public bool CanFinancial;
        public bool CanMilitary;
        public bool CanHireLower;
        public bool CanFireLower;
        public bool RepresentativeOnlyFinancial;
        public bool PostosOnlyMilitary;
        public string LinkedConstructionKey;

        public int OccupantSerial;
        public string OccupantName;
        public int ApprovalState;

        public ReinoCargoEntry()
        {
            Title = String.Empty;
            Description = String.Empty;
            LinkedConstructionKey = String.Empty;
            OccupantName = String.Empty;
            Hierarchy = 99;
            ApprovalState = 0;
        }

        public bool IsOccupied
        {
            get { return OccupantSerial > 0 && !String.IsNullOrWhiteSpace(OccupantName); }
        }

        public bool IsLeaderRole
        {
            get { return Kind == ReinoCargoKind.Leader; }
        }

        public bool IsApproved
        {
            get { return ApprovalState == 0; }
        }

        public bool IsPendingApproval
        {
            get { return ApprovalState == 1; }
        }

        public bool IsRejected
        {
            get { return ApprovalState == 2; }
        }
    }

    public class ReinoCommercialTradeState
    {
        public int CityId;
        public int[] BuyPrices;
        public int[] SellPrices;
        public int[] WeeklyBuyCaps;
        public int[] WeeklySellCaps;
        public int[] WeeklyBuyRemaining;
        public int[] WeeklySellRemaining;
        public DateTime WindowStartUtc;

        public ReinoCommercialTradeState()
        {
            BuyPrices = new int[3];
            SellPrices = new int[3];
            WeeklyBuyCaps = new int[3];
            WeeklySellCaps = new int[3];
            WeeklyBuyRemaining = new int[3];
            WeeklySellRemaining = new int[3];
            WindowStartUtc = DateTime.UtcNow;
        }

        public ReinoCommercialTradeState(int cityId) : this()
        {
            CityId = cityId;
        }
    }


    public enum ReinoApprovalChangeType
    {
        None = 0,
        CreateRole,
        SalaryChange,
        TradeConfig
    }

    public enum ReinoApprovalDecision
    {
        Pending = 0,
        Approved,
        Rejected
    }

    public class ReinoPendingApprovalVote
    {
        public int VoterSerial;
        public string VoterName;
        public int Decision;
        public DateTime DecisionUtc;

        public ReinoPendingApprovalVote()
        {
            VoterName = String.Empty;
            Decision = 0;
            DecisionUtc = DateTime.MinValue;
        }
    }

    public class ReinoPendingApproval
    {
        public int ApprovalId;
        public int CityId;
        public int CreatedBySerial;
        public string CreatedByName;
        public DateTime CreatedUtc;
        public DateTime ResolvedUtc;
        public int Status;
        public ReinoApprovalChangeType ChangeType;
        public string Html;

        public int RoleId;
        public int OldSalary;
        public int NewSalary;

        public int[] OldBuyPrices;
        public int[] OldSellPrices;
        public int[] OldBuyCaps;
        public int[] OldSellCaps;
        public int[] NewBuyPrices;
        public int[] NewSellPrices;
        public int[] NewBuyCaps;
        public int[] NewSellCaps;

        public List<ReinoPendingApprovalVote> Votes;

        public ReinoPendingApproval()
        {
            CreatedByName = String.Empty;
            Html = String.Empty;
            Votes = new List<ReinoPendingApprovalVote>();
            OldBuyPrices = new int[3];
            OldSellPrices = new int[3];
            OldBuyCaps = new int[3];
            OldSellCaps = new int[3];
            NewBuyPrices = new int[3];
            NewSellPrices = new int[3];
            NewBuyCaps = new int[3];
            NewSellCaps = new int[3];
            CreatedUtc = DateTime.UtcNow;
            ResolvedUtc = DateTime.MinValue;
            Status = 0;
            ChangeType = ReinoApprovalChangeType.None;
        }

        public bool IsPending
        {
            get { return Status == 0; }
        }
    }

    public class ReinoEmploymentSession
    {
        public int CityId;
        public int TopPage;
        public int BottomPage;
        public bool ShowAddList;
        public int SelectedTopRoleId;
        public int SelectedBottomIndex;
        public int EditingSalaryRoleId;
        public string EditingSalaryText;
        public string CreateInfoHtml;

        public int CreatedRolesPage;
        public int SelectedConstructionPage;
        public string CreateName;
        public string CreateSalary;
        public string CreateHierarchy;
        public string CreateDescription;
        public string CreateLinkedConstructionKey;
        public bool CreateCanFinancial;
        public bool CreateCanMilitary;
        public bool CreateCanHire;
        public bool CreateCanFire;

        public ReinoEmploymentSession()
        {
            CreateName = String.Empty;
            CreateSalary = "0";
            CreateHierarchy = "0";
            CreateDescription = String.Empty;
            CreateLinkedConstructionKey = String.Empty;
            EditingSalaryText = String.Empty;
            CreateInfoHtml = String.Empty;
        }
    }

    public static class ReinoEmploymentSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoEmployment.bin");

        private static readonly Dictionary<int, List<ReinoCargoEntry>> m_RolesByCity = new Dictionary<int, List<ReinoCargoEntry>>();
        private static readonly Dictionary<int, ReinoCommercialTradeState> m_TradeByCity = new Dictionary<int, ReinoCommercialTradeState>();
        private static readonly Dictionary<int, ReinoEmploymentSession> m_Sessions = new Dictionary<int, ReinoEmploymentSession>();
        private static readonly List<ReinoPendingApproval> m_PendingApprovals = new List<ReinoPendingApproval>();

        private static int m_NextRoleId = 1;
        private static int m_NextApprovalId = 1;
        private static DateTime m_LastWeeklyPayrollUtc = DateTime.MinValue;

        public const int RepresentativeWeeklySalary = 200;

        public static void Initialize()
        {
            EnsureDefaults();
            Load();
            EnsureDefaults();
            SyncAllLeaders();
            SyncAllLegacyFlags();
            SyncAllCommissionedConstructionState();

            if (m_LastWeeklyPayrollUtc == DateTime.MinValue)
                m_LastWeeklyPayrollUtc = GetCurrentWeeklyWindowStartUtc();

            EventSink.WorldSave += delegate { Save(); };
            EventSink.Login += OnLogin;

            Timer.DelayCall(TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(10.0), Pulse);
        }

        private static void Pulse()
        {
            try
            {
                ProcessWeeklyTickIfNeeded();
                ResetTradeWindowsIfNeeded();
                ProcessPendingApprovals();
                SyncAllLeaders();
                SyncAllLegacyFlags();
                SyncAllCommissionedConstructionState();
            }
            catch
            {
            }
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            RefreshLegacyFlags(pm);
            ShowPendingApprovalGump(pm);
        }

        private static bool IsSarangGovernment(int cityId)
        {
            return String.Equals(GetGovernmentCultureId(cityId), "sarangs", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetMinimumCustomHierarchy(int cityId)
        {
            return IsSarangGovernment(cityId) ? 2 : 3;
        }

        private static int GetDefaultAmbassadorHierarchy(int cityId)
        {
            return IsSarangGovernment(cityId) ? 2 : 3;
        }

        private static int GetDefaultDispatcherHierarchy(int cityId)
        {
            return IsSarangGovernment(cityId) ? 3 : 4;
        }

        private static bool IsProtectedHierarchyRole(int cityId, ReinoCargoEntry role)
        {
            if (role == null)
                return false;

            if (role.IsLeaderRole)
                return true;

            if (!IsSarangGovernment(cityId) && role.IsEssential && role.Hierarchy == 2)
                return true;

            return false;
        }

        private static DateTime GetCurrentWeeklyWindowStartUtc()
        {
            DateTime localNow = DateTime.UtcNow.AddHours(-3.0);
            int mondayOffset = ((int)localNow.DayOfWeek + 6) % 7;
            DateTime currentMonday = localNow.Date.AddDays(-mondayOffset).AddHours(18.0);

            if (localNow < currentMonday)
                currentMonday = currentMonday.AddDays(-7.0);

            return currentMonday.AddHours(3.0);
        }

        public static int GetNextAvailableHierarchy(int cityId)
        {
            int next = GetMinimumCustomHierarchy(cityId);
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role != null && role.Hierarchy >= next)
                    next = role.Hierarchy + 1;
            }
            return next;
        }

        private static int GetDistinctRoleTypeCount(int cityId)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || role.Kind == ReinoCargoKind.CommercialRepresentative)
                    continue;

                string title = (role.Title ?? String.Empty).Trim();
                if (!String.IsNullOrWhiteSpace(title))
                    seen.Add(title);
            }
            return seen.Count;
        }

        private static void ShiftHierarchiesForInsert(int cityId, List<ReinoCargoEntry> list, int startHierarchy, int exceptRoleId)
        {
            if (list == null)
                return;

            List<ReinoCargoEntry> ordered = new List<ReinoCargoEntry>(list);
            ordered.Sort(delegate (ReinoCargoEntry a, ReinoCargoEntry b) { return b.Hierarchy.CompareTo(a.Hierarchy); });

            for (int i = 0; i < ordered.Count; i++)
            {
                ReinoCargoEntry role = ordered[i];
                if (role == null || role.RoleId == exceptRoleId)
                    continue;

                if (IsProtectedHierarchyRole(cityId, role))
                    continue;

                if (role.Hierarchy >= startHierarchy)
                    role.Hierarchy++;
            }
        }

        private static bool HasPendingApprovalForRole(int cityId, int roleId, ReinoApprovalChangeType type)
        {
            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoPendingApproval proposal = m_PendingApprovals[i];
                if (proposal != null && proposal.IsPending && proposal.CityId == cityId && proposal.RoleId == roleId && proposal.ChangeType == type)
                    return true;
            }
            return false;
        }

        private static bool HasPendingTradeApproval(int cityId)
        {
            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoPendingApproval proposal = m_PendingApprovals[i];
                if (proposal != null && proposal.IsPending && proposal.CityId == cityId && proposal.ChangeType == ReinoApprovalChangeType.TradeConfig)
                    return true;
            }
            return false;
        }

        public static void EnsureDefaults()
        {
            int cityCount = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;

            for (int cityId = 0; cityId < cityCount; cityId++)
            {
                EnsureCityRoles(cityId);
                GetTradeState(cityId);
            }
        }

        private static void EnsureCityRoles(int cityId)
        {
            List<ReinoCargoEntry> list;

            if (!m_RolesByCity.TryGetValue(cityId, out list))
            {
                list = new List<ReinoCargoEntry>();
                m_RolesByCity[cityId] = list;
            }

            EnsureSingleRole(list, cityId, ReinoCargoKind.Leader, GetLeaderTitle(cityId), GetLeaderDescription(cityId), 0, 1, true, false, false, true, true, false, false, String.Empty);

            switch (GetGovernmentCultureId(cityId))
            {
                case "kamay":
                    EnsureSingleRole(list, cityId, ReinoCargoKind.MinisterEconomy, "Ministro da Economia", "Cargo essencial do governo kamay. Organiza as decisões econômicas do reino.", 280, 2, true, false, true, true, false, false, false, String.Empty);
                    EnsureSingleRole(list, cityId, ReinoCargoKind.MinisterDefense, "Ministro da Defesa", "Cargo essencial do governo kamay. Organiza as decisões militares do reino.", 280, 2, true, false, true, false, true, false, false, String.Empty);
                    break;
                case "matalun":
                    EnsureSingleRole(list, cityId, ReinoCargoKind.Priest, "Sacerdote", "Cargo essencial do governo matalun. O oráculo depende do sacerdote para governar.", 0, 2, true, false, true, false, false, false, false, String.Empty);
                    break;
                case "zosteros":
                    EnsureNthCouncilRole(list, cityId, 1);
                    EnsureNthCouncilRole(list, cityId, 2);
                    RemoveExtraDefaultCouncilRoles(list, 2);
                    break;
            }

            EnsureSingleRole(list, cityId, ReinoCargoKind.Ambassador, "Embaixador", "Pode agir em nome do reino em assuntos ligados a postos e representante comercial.", 0, GetDefaultAmbassadorHierarchy(cityId), true, false, false, true, true, false, false, String.Empty);
            EnsureSingleRole(list, cityId, ReinoCargoKind.Dispatcher, "Dispachante", "Responsável por retirar e despachar recursos dos postos do reino.", 0, GetDefaultDispatcherHierarchy(cityId), true, false, false, false, false, false, false, String.Empty);
        }

        private static void EnsureNthCouncilRole(List<ReinoCargoEntry> list, int cityId, int number)
        {
            int found = 0;

            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry role = list[i];
                if (role != null && role.Kind == ReinoCargoKind.CouncilMember)
                    found++;
            }

            while (found < number)
            {
                ReinoCargoEntry role = CreateRole(cityId, ReinoCargoKind.CouncilMember, "Conselheiro " + (found + 1),
                    "Cargo essencial do governo zortero. O presidente do conselho depende dos conselheiros para governar.",
                    0, 2, true, false, true, false, false, false, false, String.Empty);
                list.Add(role);
                found++;
            }
        }

        private static void RemoveExtraDefaultCouncilRoles(List<ReinoCargoEntry> list, int keepCount)
        {
            int found = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ReinoCargoEntry role = list[i];
                if (role == null || role.Kind != ReinoCargoKind.CouncilMember)
                    continue;

                found++;
                if (found > keepCount && !role.IsOccupied)
                    list.RemoveAt(i);
            }
        }

        private static void EnsureSingleRole(List<ReinoCargoEntry> list, int cityId, ReinoCargoKind kind, string title, string description, int salary, int hierarchy, bool isDefault, bool removable, bool essential, bool canFinancial, bool canMilitary, bool canHire, bool canFire, string linkedKey)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry role = list[i];
                if (role == null || role.Kind != kind)
                    continue;

                if (kind == ReinoCargoKind.CouncilMember)
                    continue;

                role.CityId = cityId;
                role.Kind = kind;
                role.IsDefault = isDefault;
                role.IsRemovable = removable;
                role.IsEssential = essential;

                if (String.IsNullOrWhiteSpace(role.Title))
                    role.Title = title;
                if (String.IsNullOrWhiteSpace(role.Description))
                    role.Description = description;
                if (role.Hierarchy <= 0)
                    role.Hierarchy = hierarchy;

                if (kind == ReinoCargoKind.Leader)
                {
                    role.Title = title;
                    role.Description = description;
                    if (IsSarangGovernment(cityId))
                        role.WeeklySalaryGold = 0;
                    role.Hierarchy = 1;
                }

                if (kind == ReinoCargoKind.Ambassador)
                {
                    if (role.Hierarchy < GetMinimumCustomHierarchy(cityId))
                        role.Hierarchy = GetDefaultAmbassadorHierarchy(cityId);
                }

                if (kind == ReinoCargoKind.Dispatcher)
                {
                    if (role.Hierarchy < GetMinimumCustomHierarchy(cityId))
                        role.Hierarchy = GetDefaultDispatcherHierarchy(cityId);
                }

                if (!role.IsApproved && role.IsDefault)
                    role.ApprovalState = 0;

                return;
            }

            list.Add(CreateRole(cityId, kind, title, description, salary, hierarchy, isDefault, removable, essential, canFinancial, canMilitary, canHire, canFire, linkedKey));
        }

        private static ReinoCargoEntry CreateRole(int cityId, ReinoCargoKind kind, string title, string description, int salary, int hierarchy, bool isDefault, bool removable, bool essential, bool canFinancial, bool canMilitary, bool canHire, bool canFire, string linkedKey)
        {
            ReinoCargoEntry role = new ReinoCargoEntry();
            role.RoleId = m_NextRoleId++;
            role.CityId = cityId;
            role.Kind = kind;
            role.Title = title ?? String.Empty;
            role.Description = description ?? String.Empty;
            role.WeeklySalaryGold = Math.Max(0, salary);
            role.Hierarchy = Math.Max(1, hierarchy);
            role.IsDefault = isDefault;
            role.IsRemovable = removable;
            role.IsEssential = essential;
            role.CanFinancial = canFinancial;
            role.CanMilitary = canMilitary;
            role.CanHireLower = canHire;
            role.CanFireLower = canFire;
            role.LinkedConstructionKey = linkedKey ?? String.Empty;

            if (kind == ReinoCargoKind.Ambassador)
            {
                role.RepresentativeOnlyFinancial = true;
                role.PostosOnlyMilitary = true;
            }

            return role;
        }

        public static string GetGovernmentCultureId(int cityId)
        {
            return ReinoElectionsSystem.GetRequiredCultureId(cityId);
        }

        public static string GetLeaderTitle(int cityId)
        {
            switch (GetGovernmentCultureId(cityId))
            {
                case "kamay": return "Primeiro Ministro";
                case "matalun": return "Oráculo";
                case "sarangs": return "Líder Absoluto";
                case "zosteros": return "Presidente do Conselho";
                default: return "Líder";
            }
        }

        private static string GetLeaderDescription(int cityId)
        {
            switch (GetGovernmentCultureId(cityId))
            {
                case "kamay": return "Chefe do governo kamay. Hierarquia 1.";
                case "matalun": return "Chefe do governo matalun. Hierarquia 1.";
                case "sarangs": return "Chefe do governo sarang. Hierarquia 1 e autoridade direta sobre o reino.";
                case "zosteros": return "Chefe do governo zortero. Hierarquia 1.";
                default: return "Chefe do governo do reino.";
            }
        }

        public static List<ReinoCargoEntry> GetRoles(int cityId)
        {
            EnsureCityRoles(cityId);
            List<ReinoCargoEntry> source = m_RolesByCity[cityId];
            List<ReinoCargoEntry> list = new List<ReinoCargoEntry>(source);
            list.Sort(CompareRoles);
            SyncLeaderRole(cityId, list);
            return list;
        }

        private static List<ReinoCargoEntry> GetRolesForWrite(int cityId)
        {
            EnsureCityRoles(cityId);
            return m_RolesByCity[cityId];
        }

        private static int CompareRoles(ReinoCargoEntry a, ReinoCargoEntry b)
        {
            if (a == null && b == null)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            int cmp = a.Hierarchy.CompareTo(b.Hierarchy);
            if (cmp != 0)
                return cmp;

            if (a.IsDefault != b.IsDefault)
                return a.IsDefault ? -1 : 1;

            return String.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
        }

        public static ReinoCargoEntry GetRole(int cityId, int roleId)
        {
            EnsureCityRoles(cityId);
            List<ReinoCargoEntry> list = m_RolesByCity[cityId];

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].RoleId == roleId)
                {
                    if (list[i].IsLeaderRole)
                        SyncLeaderRole(cityId, list);

                    return list[i];
                }
            }

            return null;
        }

        private static void SyncLeaderRole(int cityId, List<ReinoCargoEntry> list)
        {
            if (list == null)
                return;

            ReinoCargoEntry leader = null;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].Kind == ReinoCargoKind.Leader)
                {
                    leader = list[i];
                    break;
                }
            }

            if (leader == null)
                return;

            ReinoCityData city;
            if (!ReinoElectionsSystem._cities.TryGetValue(cityId, out city) || city == null)
            {
                leader.OccupantSerial = 0;
                leader.OccupantName = String.Empty;
                return;
            }

            leader.OccupantSerial = city.GovernorSerial;
            leader.OccupantName = city.GovernorName ?? String.Empty;
        }

        private static void SyncAllLeaders()
        {
            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
                SyncLeaderRole(kv.Key, kv.Value);
        }

        public static bool AreEssentialRolesFilled(int cityId)
        {
            string culture = GetGovernmentCultureId(cityId);
            if (String.Equals(culture, "sarangs", StringComparison.OrdinalIgnoreCase))
                return true;

            List<ReinoCargoEntry> list = GetRoles(cityId);

            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry role = list[i];
                if (role == null || !role.IsEssential)
                    continue;

                if (!role.IsOccupied)
                    return false;
            }

            return true;
        }

        public static string GetMissingEssentialsMessage(int cityId)
        {
            List<ReinoCargoEntry> list = GetRoles(cityId);
            List<string> missing = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry role = list[i];
                if (role == null || !role.IsEssential || role.IsOccupied)
                    continue;

                missing.Add(role.Title);
            }

            if (missing.Count == 0)
                return String.Empty;

            return "Faltam preencher os cargos essenciais: " + String.Join(", ", missing.ToArray()) + ".";
        }

        public static bool PlayerHasAnyCommissionedRole(PlayerMobile pm, int cityId)
        {
            return GetOccupiedRole(pm, cityId) != null;
        }

        public static ReinoCargoEntry GetOccupiedRole(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return null;

            List<ReinoCargoEntry> list = GetRoles(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry role = list[i];
                if (role == null || !role.IsOccupied || role.IsLeaderRole)
                    continue;

                if (role.OccupantSerial == pm.Serial.Value)
                    return role;
            }

            return null;
        }

        public static ReinoCargoEntry GetOccupiedRoleAnywhere(PlayerMobile pm, out int cityId)
        {
            cityId = -1;

            if (pm == null || pm.Deleted)
                return null;

            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                List<ReinoCargoEntry> list = kv.Value;

                for (int i = 0; i < list.Count; i++)
                {
                    ReinoCargoEntry role = list[i];

                    if (role == null || !role.IsOccupied || role.IsLeaderRole)
                        continue;

                    if (role.OccupantSerial == pm.Serial.Value)
                    {
                        cityId = kv.Key;
                        return role;
                    }
                }
            }

            return null;
        }

        public static bool DismissPlayerFromForeignRoles(PlayerMobile pm, int newCitizenCityId, out string removedRoles)
        {
            removedRoles = String.Empty;

            if (pm == null || pm.Deleted)
                return false;

            List<string> removed = new List<string>();

            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                int cityId = kv.Key;

                if (cityId == newCitizenCityId)
                    continue;

                List<ReinoCargoEntry> list = kv.Value;

                for (int i = 0; i < list.Count; i++)
                {
                    ReinoCargoEntry role = list[i];

                    if (role == null || role.IsLeaderRole || !role.IsOccupied)
                        continue;

                    if (role.OccupantSerial != pm.Serial.Value)
                        continue;

                    RemoveRoleLinkedItems(pm, cityId, role);

                    string title = role.Title;
                    string cityName = ReinoElectionsSystem.GetCityName(cityId);

                    role.OccupantSerial = 0;
                    role.OccupantName = String.Empty;
                    SyncRoleDependentState(cityId);

                    removed.Add(title + " de " + cityName);
                }
            }

            removedRoles = String.Join(", ", removed.ToArray());
            return removed.Count > 0;
        }

        public static bool PlayerOccupiesAnyRole(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return false;

            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                List<ReinoCargoEntry> list = kv.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoCargoEntry role = list[i];
                    if (role != null && !role.IsLeaderRole && role.IsOccupied && role.OccupantSerial == pm.Serial.Value)
                        return true;
                }
            }

            return false;
        }

        public static int GetActingGovernmentCityId(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return -1;

            int governorCityId = ReinoAccessHelper.GetGovernorCityId(pm);
            if (governorCityId >= 0)
                return governorCityId;

            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                List<ReinoCargoEntry> list = kv.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoCargoEntry role = list[i];
                    if (role != null && !role.IsLeaderRole && role.IsOccupied && role.OccupantSerial == pm.Serial.Value)
                        return kv.Key;
                }
            }

            return -1;
        }

        public static bool CanUseGovernmentPage(PlayerMobile pm, int cityId, int page, out string message)
        {
            message = String.Empty;

            if (pm == null || pm.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (page == 8)
            {
                if (!ReinoMilitarySystem.CanAccessMilitaryGovernmentPage(pm, cityId)
                    && !ReinoMilitarySystem.CanAccessBarracksSubGump(pm, cityId))
                {
                    message = "Você não tem acesso à aba militar deste reino.";
                    return false;
                }

                if (!AreEssentialRolesFilled(cityId))
                {
                    message = GetMissingEssentialsMessage(cityId);
                    return false;
                }

                return true;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
            {
                message = "Somente o governador ou alguém com a chave do governador pode abrir esta página.";
                return false;
            }

            if (page != 5 && page != 17 && !AreEssentialRolesFilled(cityId))
            {
                message = GetMissingEssentialsMessage(cityId);
                return false;
            }

            return true;
        }

        private static bool IsCitizenOfCity(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            string citizenCity = PlayerMobile.NormalizeOSUCityId(pm.OSUCitizenCityId);
            string cityName = PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(cityId));

            return !String.IsNullOrWhiteSpace(citizenCity)
                && String.Equals(citizenCity, cityName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanNominateFromEmploymentPage(PlayerMobile actor, int cityId, int roleId, Mobile target, out string message)
        {
            message = String.Empty;

            if (!(target is PlayerMobile))
            {
                message = "Selecione um jogador válido.";
                return false;
            }

            if (actor == null || actor.Deleted || target == null || target.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode nomear cargos por esta página.";
                return false;
            }

            ReinoCargoEntry role = GetRole(cityId, roleId);
            if (role == null)
            {
                message = "Cargo inválido.";
                return false;
            }

            if (role.IsLeaderRole)
            {
                message = "O cargo de líder é definido pelas eleições.";
                return false;
            }

            if (role.IsPendingApproval)
            {
                message = "Este cargo ainda aguarda aprovação do governo.";
                return false;
            }

            if (role.IsRejected)
            {
                message = "Este cargo não foi aprovado pelo governo e não pode receber nomeações.";
                return false;
            }

            if (role.IsOccupied)
            {
                message = "Este cargo já está ocupado.";
                return false;
            }

            PlayerMobile targetPm = (PlayerMobile)target;
            if (targetPm == actor)
            {
                message = "Você não pode nomear você mesmo por esta página.";
                return false;
            }

            if (!IsCitizenOfCity(targetPm, cityId))
            {
                message = "Esse jogador não é cidadão deste reino.";
                return false;
            }

            if (PlayerOccupiesAnyRole(targetPm))
            {
                message = "Esse jogador já ocupa outro cargo comissionado.";
                return false;
            }

            return true;
        }

        private static void GrantRoleLinkedItems(PlayerMobile target, int cityId, ReinoCargoEntry role)
        {
            if (target == null || target.Deleted || role == null)
                return;

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && ReinoMilitarySystem.IsBarracksConstructionKey(cityId, role.LinkedConstructionKey))
            {
                if (target.Backpack != null && target.Backpack.FindItemByType(typeof(ReinoBarracksBadge)) == null)
                    target.Backpack.DropItem(new ReinoBarracksBadge(cityId));
            }

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && ReinoTrialsSystem.IsTribunalConstructionKey(cityId, role.LinkedConstructionKey))
            {
                if (target.Backpack != null && target.Backpack.FindItemByType(typeof(ReinoTribunalHammer)) == null)
                    target.Backpack.DropItem(new ReinoTribunalHammer(cityId, role.Title));
            }
        }

        private static void RemoveRoleLinkedItems(PlayerMobile target, int cityId, ReinoCargoEntry role)
        {
            if (target == null || target.Deleted || target.Backpack == null || role == null)
                return;

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && ReinoMilitarySystem.IsBarracksConstructionKey(cityId, role.LinkedConstructionKey))
            {
                Item badge = target.Backpack.FindItemByType(typeof(ReinoBarracksBadge));
                if (badge != null && !badge.Deleted)
                    badge.Delete();
            }

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && ReinoTrialsSystem.IsTribunalConstructionKey(cityId, role.LinkedConstructionKey))
            {
                ReinoTrialsSystem.DeleteTribunalItems(target, cityId);
            }
        }

        public static bool AcceptInvitation(PlayerMobile target, int cityId, int roleId, string inviterName, out string message)
        {
            message = String.Empty;

            if (target == null || target.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            ReinoCargoEntry role = GetRole(cityId, roleId);
            if (role == null)
            {
                message = "O cargo não existe mais.";
                return false;
            }

            if (role.IsLeaderRole)
            {
                message = "O cargo de líder não pode ser aceito por carta.";
                return false;
            }

            if (role.IsOccupied)
            {
                message = "Esse cargo já foi ocupado por outra pessoa.";
                return false;
            }

            if (!IsCitizenOfCity(target, cityId))
            {
                message = "Você não é cidadão deste reino.";
                return false;
            }

            if (PlayerOccupiesAnyRole(target))
            {
                message = "Você já ocupa outro cargo comissionado.";
                return false;
            }

            role.OccupantSerial = target.Serial.Value;
            role.OccupantName = target.Name;
            SyncRoleDependentState(cityId);
            GrantRoleLinkedItems(target, cityId, role);

            message = String.IsNullOrWhiteSpace(inviterName)
                ? "Você aceitou o cargo de " + role.Title + "."
                : "Você aceitou o cargo de " + role.Title + " enviado por " + inviterName + ".";
            return true;
        }

        public static bool RemoveRoleOccupant(PlayerMobile actor, int cityId, int roleId, bool sendLetter, out string message)
        {
            message = String.Empty;
            ReinoCargoEntry role = GetRole(cityId, roleId);

            if (role == null)
            {
                message = "Cargo inválido.";
                return false;
            }

            if (role.IsLeaderRole)
            {
                message = "O cargo de líder é controlado pelas eleições.";
                return false;
            }

            if (!role.IsOccupied)
            {
                message = "Este cargo já está vazio.";
                return false;
            }

            PlayerMobile removed = World.FindMobile((Serial)role.OccupantSerial) as PlayerMobile;
            string removedName = role.OccupantName;

            RemoveRoleLinkedItems(removed, cityId, role);

            role.OccupantSerial = 0;
            role.OccupantName = String.Empty;
            SyncRoleDependentState(cityId);

            if (sendLetter && removed != null && !removed.Deleted)
                DeliverDismissalLetter(actor, removed, cityId, role.Title, removedName);

            message = String.IsNullOrWhiteSpace(removedName)
                ? "Cargo exonerado."
                : removedName + " foi exonerado do cargo de " + role.Title + ".";
            return true;
        }

        public static bool RemoveEmptyRole(PlayerMobile actor, int cityId, int roleId, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode remover este cargo.";
                return false;
            }

            List<ReinoCargoEntry> list = GetRolesForWrite(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry role = list[i];
                if (role == null || role.RoleId != roleId)
                    continue;

                if (!role.IsRemovable)
                {
                    message = "Os cargos padrão do reino não podem ser removidos.";
                    return false;
                }

                if (role.IsOccupied)
                {
                    message = "Você só pode remover um cargo vazio.";
                    return false;
                }

                list.RemoveAt(i);
                SyncRoleDependentState(cityId);
                message = "Cargo removido.";
                return true;
            }

            message = "Cargo inválido.";
            return false;
        }

        public static bool UpdateRoleSalary(PlayerMobile actor, int cityId, int roleId, int newSalary, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode alterar salários.";
                return false;
            }

            ReinoCargoEntry role = GetRole(cityId, roleId);
            if (role == null)
            {
                message = "Cargo inválido.";
                return false;
            }

            if (role.IsRejected)
            {
                message = "Esse cargo foi vetado pelo governo e não pode receber salário.";
                return false;
            }

            if (role.Kind == ReinoCargoKind.Leader && IsSarangGovernment(cityId))
            {
                message = "O Líder Absoluto sarang não recebe salário.";
                return false;
            }

            newSalary = Math.Max(0, newSalary);

            if (!GovernmentNeedsApprovals(cityId))
            {
                List<ReinoCargoEntry> matches = GetRolesByTitleForWrite(cityId, role.Title);
                for (int i = 0; i < matches.Count; i++)
                    matches[i].WeeklySalaryGold = newSalary;
                SyncRoleDependentState(cityId);
                message = "Salário atualizado para " + newSalary + " moedas semanais.";
                return true;
            }

            if (HasPendingApprovalForRole(cityId, roleId, ReinoApprovalChangeType.SalaryChange))
            {
                message = "Já existe uma mudança de salário desse cargo aguardando aprovação.";
                return false;
            }

            ReinoPendingApproval proposal = CreateSalaryApproval(actor, cityId, role, newSalary);
            if (proposal == null)
            {
                message = "Não foi possível criar a aprovação da mudança de salário.";
                return false;
            }

            message = "Mudança de salário enviada para aprovação do governo.";
            return true;
        }

        public static bool CreateCustomRole(PlayerMobile actor, int cityId, string title, string description, int salary, int hierarchy, bool canFinancial, bool canMilitary, bool canHire, bool canFire, string linkedConstructionKey, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode criar cargos.";
                return false;
            }

            title = title == null ? String.Empty : title.Trim();
            description = description == null ? String.Empty : description.Trim();
            linkedConstructionKey = linkedConstructionKey == null ? String.Empty : linkedConstructionKey.Trim();

            if (title.Length < 2)
            {
                message = "Digite um nome válido para o cargo.";
                return false;
            }

            int minimumHierarchy = GetMinimumCustomHierarchy(cityId);
            if (hierarchy < minimumHierarchy)
            {
                message = "A hierarquia mínima para esse reino é " + minimumHierarchy + ".";
                return false;
            }

            if (String.IsNullOrWhiteSpace(description))
            {
                message = "Preencha a descrição do cargo.";
                return false;
            }

            if (GetDistinctRoleTypeCount(cityId) >= 15)
            {
                message = "Cada reino pode ter no máximo 15 tipos de cargo.";
                return false;
            }

            List<ReinoCargoEntry> list = GetRolesForWrite(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoCargoEntry existing = list[i];
                if (existing == null)
                    continue;

                if (String.Equals(existing.Title, title, StringComparison.OrdinalIgnoreCase))
                {
                    message = "Já existe um cargo com esse nome.";
                    return false;
                }
            }

            salary = Math.Max(0, salary);
            ShiftHierarchiesForInsert(cityId, list, hierarchy, 0);

            ReinoCargoEntry role = CreateRole(cityId, ReinoCargoKind.Custom, title, description, salary, hierarchy, false, true, false, canFinancial, canMilitary, canHire, canFire, linkedConstructionKey);
            role.ApprovalState = GovernmentNeedsApprovals(cityId) ? 1 : 0;
            list.Add(role);
            SyncRoleDependentState(cityId);

            if (GovernmentNeedsApprovals(cityId))
            {
                CreateRoleApproval(actor, cityId, role);
                message = "Cargo criado e enviado para aprovação do governo.";
            }
            else
            {
                message = "Cargo criado com sucesso.";
            }

            return true;
        }

        public static int GetRepresentativeSalary(int cityId)
        {
            return RepresentativeWeeklySalary;
        }

        public static List<ReinoCargoEntry> GetCreatedRolesOnly(int cityId)
        {
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            List<ReinoCargoEntry> list = new List<ReinoCargoEntry>();

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role != null && role.Kind == ReinoCargoKind.Custom)
                    list.Add(role);
            }

            return list;
        }


        public static List<ReinoCargoEntry> GetAddableRoleTemplates(int cityId)
        {
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            List<ReinoCargoEntry> list = new List<ReinoCargoEntry>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null)
                    continue;

                if (role.Kind == ReinoCargoKind.Leader || role.Kind == ReinoCargoKind.CommercialRepresentative)
                    continue;

                if (role.Hierarchy <= 2)
                    continue;

                if (!role.IsApproved)
                    continue;

                string key = (role.Title ?? String.Empty).Trim();
                if (String.IsNullOrWhiteSpace(key) || seen.Contains(key))
                    continue;

                seen.Add(key);
                list.Add(role);
            }

            return list;
        }

        private static List<ReinoCargoEntry> GetRolesByTitleForWrite(int cityId, string title)
        {
            List<ReinoCargoEntry> matches = new List<ReinoCargoEntry>();
            if (String.IsNullOrWhiteSpace(title))
                return matches;

            List<ReinoCargoEntry> roles = GetRolesForWrite(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role != null && String.Equals(role.Title, title, StringComparison.OrdinalIgnoreCase))
                    matches.Add(role);
            }
            return matches;
        }

        private static int GetOccupiedRoleCountByTitle(int cityId, string title)
        {
            int count = 0;
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role != null && role.IsOccupied && String.Equals(role.Title, title, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        public static int GetRoleSlotCount(int cityId, string title)
        {
            if (String.IsNullOrWhiteSpace(title))
                return 0;

            int count = 0;
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role != null && String.Equals(role.Title, title, StringComparison.OrdinalIgnoreCase))
                    count++;
            }

            return count;
        }

        public static bool AddRoleFromTemplate(PlayerMobile actor, int cityId, int templateRoleId, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode adicionar cargos.";
                return false;
            }

            ReinoCargoEntry template = GetRole(cityId, templateRoleId);
            if (template == null)
            {
                message = "Selecione um cargo para adicionar uma nova vaga.";
                return false;
            }

            if (template.Kind == ReinoCargoKind.Leader || template.Kind == ReinoCargoKind.CommercialRepresentative || template.Hierarchy <= 2)
            {
                message = "Este cargo não pode receber vagas adicionais.";
                return false;
            }

            if (!template.IsApproved)
            {
                message = "Este cargo ainda não foi aprovado pelo governo.";
                return false;
            }

            List<ReinoCargoEntry> list = GetRolesForWrite(cityId);
            ReinoCargoEntry copy = CreateRole(cityId, template.Kind, template.Title, template.Description, template.WeeklySalaryGold, template.Hierarchy, template.IsDefault, template.IsRemovable, template.IsEssential, template.CanFinancial, template.CanMilitary, template.CanHireLower, template.CanFireLower, template.LinkedConstructionKey);
            copy.RepresentativeOnlyFinancial = template.RepresentativeOnlyFinancial;
            copy.PostosOnlyMilitary = template.PostosOnlyMilitary;
            copy.ApprovalState = template.ApprovalState;
            list.Add(copy);
            SyncRoleDependentState(cityId);
            message = "Nova vaga do cargo " + template.Title + " adicionada.";
            return true;
        }

        public static string[] GetOptionalRoleTemplatesForAdd()
        {
            return new string[] { "Embaixador", "Dispachante" };
        }

        public static bool AddOptionalRoleTemplate(PlayerMobile actor, int cityId, int templateIndex, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode adicionar cargos.";
                return false;
            }

            List<ReinoCargoEntry> list = GetRolesForWrite(cityId);
            ReinoCargoEntry role;

            switch (templateIndex)
            {
                case 0:
                    role = CreateRole(cityId, ReinoCargoKind.Ambassador, "Embaixador Auxiliar", "Cargo auxiliar focado em postos e representante comercial.", 0, 5, false, true, false, true, true, false, false, String.Empty);
                    role.RepresentativeOnlyFinancial = true;
                    role.PostosOnlyMilitary = true;
                    list.Add(role);
                    message = "Novo cargo de embaixador auxiliar adicionado.";
                    break;
                case 1:
                    role = CreateRole(cityId, ReinoCargoKind.Dispatcher, "Dispachante Auxiliar", "Cargo auxiliar focado em retirada de recursos dos postos.", 0, 5, false, true, false, false, false, false, false, String.Empty);
                    list.Add(role);
                    message = "Novo cargo de dispachante auxiliar adicionado.";
                    break;
                default:
                    message = "Opção inválida.";
                    return false;
            }

            SyncRoleDependentState(cityId);
            return true;
        }

        public static bool CanActorManageLowerRole(PlayerMobile actor, int cityId, int targetRoleId, bool forHire, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            ReinoCargoEntry target = GetRole(cityId, targetRoleId);
            if (target == null)
            {
                message = "Cargo inválido.";
                return false;
            }

            if (target.IsLeaderRole)
            {
                message = "O cargo de líder não pode ser alterado por cartas.";
                return false;
            }

            if (!target.IsApproved)
            {
                message = "Esse cargo ainda não pode receber alterações porque não foi aprovado pelo governo.";
                return false;
            }

            if (ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
                return true;

            ReinoCargoEntry actorRole = GetOccupiedRole(actor, cityId);
            if (actorRole == null)
            {
                message = "Você não ocupa um cargo comissionado neste reino.";
                return false;
            }

            if (forHire && !actorRole.CanHireLower)
            {
                message = "Seu cargo não pode contratar outros cargos.";
                return false;
            }

            if (!forHire && !actorRole.CanFireLower)
            {
                message = "Seu cargo não pode exonerar outros cargos.";
                return false;
            }

            if (target.Hierarchy <= actorRole.Hierarchy)
            {
                message = "Você só pode agir sobre cargos abaixo da sua hierarquia.";
                return false;
            }

            return true;
        }

        public static List<ReinoCargoEntry> GetRolesBelowActor(PlayerMobile actor, int cityId, bool forHire)
        {
            List<ReinoCargoEntry> result = new List<ReinoCargoEntry>();
            string message;

            if (actor == null || actor.Deleted)
                return result;

            bool isSarang = IsSarangGovernment(cityId);

            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || role.IsLeaderRole)
                    continue;

                if (!CanActorManageLowerRole(actor, cityId, role.RoleId, forHire, out message))
                    continue;

                if (forHire)
                {
                    // Convite só faz sentido para cargo vago.
                    if (role.IsOccupied)
                        continue;

                    // Nos reinos normais:
                    // - mostra cargos de hierarquia 3+
                    // - mas também mostra os de hierarquia 2 se estiverem vagos
                    // Nos sarangs:
                    // - começa na hierarquia 2
                    if (!isSarang)
                    {
                        if (role.Hierarchy < 2)
                            continue;
                    }
                    else
                    {
                        if (role.Hierarchy < 2)
                            continue;
                    }
                }

                result.Add(role);
            }

            return result;
        }

        public static bool FindPlayerByName(string name, out PlayerMobile found)
        {
            found = null;
            if (String.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (String.Equals(pm.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    found = pm;
                    return true;
                }
            }

            return false;
        }

        public static bool CanUseCommercialRepresentative(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = GetOccupiedRole(pm, cityId);
            if (role == null)
                return false;

            if (role.Kind == ReinoCargoKind.Ambassador)
                return true;

            if (!role.CanFinancial)
                return false;

            if (String.IsNullOrWhiteSpace(role.LinkedConstructionKey))
                return true;

            return false;
        }

        public static bool CanManageRentalSign(PlayerMobile pm, TownHouseSign sign)
        {
            if (pm == null || pm.Deleted || sign == null || sign.Deleted)
                return false;

            if (!sign.GovernmentManaged)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

            int cityId = sign.GovernmentCityId;
            if (cityId < 0)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = GetOccupiedRole(pm, cityId);
            if (role == null || !role.CanFinancial)
                return false;

            if (String.IsNullOrWhiteSpace(role.LinkedConstructionKey))
                return true;

            string signKey = FindConstructionKeyByRentalSign(sign);
            if (String.IsNullOrWhiteSpace(signKey))
                return false;

            return String.Equals(role.LinkedConstructionKey, signKey, StringComparison.OrdinalIgnoreCase);
        }

        public static string FindConstructionKeyByRentalSign(TownHouseSign sign)
        {
            if (sign == null || sign.Deleted)
                return String.Empty;

            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(sign.GovernmentCityId);

            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null)
                    continue;

                List<int> serials = info.IsArea ? info.AreaState.RentalSignSerials : info.LotState.RentalSignSerials;
                if (serials == null)
                    continue;

                for (int s = 0; s < serials.Count; s++)
                {
                    if (serials[s] == sign.Serial.Value)
                        return info.Key;
                }
            }

            return String.Empty;
        }

        public static string GetConstructionRoleDescription(string constructionKey)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return "Cargo sem vínculo de construção. Se tiver permissão financeira, ele atua nas decisões financeiras gerais do reino. Se tiver permissão militar, atua nas decisões militares gerais do reino.";

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(constructionKey);
            if (info == null || info.Definition == null)
                return "Construção não encontrada.";

            string id = info.Definition.Id ?? String.Empty;
            if (String.Equals(id, "residencial_aurora_teste", StringComparison.OrdinalIgnoreCase))
                return "Se este cargo tiver decisões financeiras e estiver ligado à área residencial, o ocupante poderá abrir o gump de configuração dos aluguéis das casas do reino.";

            return "Este vínculo torna o cargo responsável por decisões ligadas a esta construção. Sem as permissões adequadas, o cargo fica apenas como posição de roleplay.";
        }

        private static bool GovernmentNeedsApprovals(int cityId)
        {
            return !IsSarangGovernment(cityId);
        }

        private static List<ReinoCargoEntry> GetApprovalRoles(int cityId)
        {
            List<ReinoCargoEntry> list = new List<ReinoCargoEntry>();
            List<ReinoCargoEntry> roles = GetRoles(cityId);
            string culture = GetGovernmentCultureId(cityId);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied)
                    continue;

                if (String.Equals(culture, "kamay", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Kind == ReinoCargoKind.MinisterEconomy)
                        list.Add(role);
                }
                else if (String.Equals(culture, "matalun", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Kind == ReinoCargoKind.Priest)
                        list.Add(role);
                }
                else if (String.Equals(culture, "zosteros", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Kind == ReinoCargoKind.CouncilMember)
                        list.Add(role);
                }
            }

            return list;
        }

        private static string GetRolePowersLine(ReinoCargoEntry role)
        {
            List<string> powers = new List<string>();
            if (role.CanFinancial) powers.Add("decisões financeiras");
            if (role.CanMilitary) powers.Add("decisões militares");
            if (role.CanHireLower) powers.Add("pode contratar");
            if (role.CanFireLower) powers.Add("pode exonerar");
            if (powers.Count == 0)
                return "Poderes: nenhum.";
            return "Poderes: " + String.Join(", ", powers.ToArray()) + ".";
        }

        private static ReinoPendingApproval CreateBaseApproval(PlayerMobile actor, int cityId, ReinoApprovalChangeType type, string html)
        {
            List<ReinoCargoEntry> approvers = GetApprovalRoles(cityId);
            ReinoPendingApproval proposal = new ReinoPendingApproval();
            proposal.ApprovalId = m_NextApprovalId++;
            proposal.CityId = cityId;
            proposal.ChangeType = type;
            proposal.CreatedUtc = DateTime.UtcNow;
            proposal.CreatedBySerial = actor != null ? actor.Serial.Value : 0;
            proposal.CreatedByName = actor != null ? actor.Name : String.Empty;
            proposal.Html = html ?? String.Empty;

            for (int i = 0; i < approvers.Count; i++)
            {
                ReinoCargoEntry role = approvers[i];
                if (role == null || !role.IsOccupied)
                    continue;

                ReinoPendingApprovalVote vote = new ReinoPendingApprovalVote();
                vote.VoterSerial = role.OccupantSerial;
                vote.VoterName = role.OccupantName ?? String.Empty;
                proposal.Votes.Add(vote);
            }

            m_PendingApprovals.Add(proposal);
            return proposal;
        }

        private static void FinalizeNewProposal(ReinoPendingApproval proposal)
        {
            if (proposal == null)
                return;

            if (proposal.Votes.Count == 0)
            {
                proposal.Status = (int)ReinoApprovalDecision.Approved;
                proposal.ResolvedUtc = DateTime.UtcNow;
                ApplyApprovedProposal(proposal);
            }
            else
            {
                SendProposalToOnlineApprovers(proposal);
            }
        }

        private static ReinoPendingApproval CreateRoleApproval(PlayerMobile actor, int cityId, ReinoCargoEntry role)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000><BIG><B>Mudança proposta pelo líder do reino</B></BIG><BR><BR>");
            sb.Append("Foi criado o cargo <B>").Append(role.Title).Append("</B>.<BR>");
            sb.Append("Salário semanal: ").Append(role.WeeklySalaryGold).Append(" moedas.<BR>");
            sb.Append("Hierarquia: ").Append(role.Hierarchy).Append(".<BR>");
            sb.Append(GetRolePowersLine(role)).Append("<BR><BR>");
            sb.Append(role.Description).Append("</BASEFONT>");

            ReinoPendingApproval proposal = CreateBaseApproval(actor, cityId, ReinoApprovalChangeType.CreateRole, sb.ToString());
            proposal.RoleId = role.RoleId;
            FinalizeNewProposal(proposal);
            return proposal;
        }

        private static ReinoPendingApproval CreateSalaryApproval(PlayerMobile actor, int cityId, ReinoCargoEntry role, int newSalary)
        {
            int oldSalary = role != null ? role.WeeklySalaryGold : 0;
            int occupied = role != null ? GetOccupiedRoleCountByTitle(cityId, role.Title) : 0;
            int extra = (newSalary - oldSalary) * occupied;

            StringBuilder occupiedNames = new StringBuilder();
            List<ReinoCargoEntry> roles = GetRoles(cityId);

            if (role != null)
            {
                for (int i = 0; i < roles.Count; i++)
                {
                    ReinoCargoEntry other = roles[i];
                    if (other == null)
                        continue;

                    if (!String.Equals(other.Title, role.Title, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!other.IsOccupied)
                        continue;

                    if (occupiedNames.Length > 0)
                        occupiedNames.Append(", ");

                    occupiedNames.Append(other.OccupantName);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000><BIG><B>Mudança proposta pelo líder do reino</B></BIG><BR><BR>");
            sb.Append("Cargo: <B>").Append(role.Title).Append("</B><BR>");
            sb.Append("Salário antigo: ").Append(oldSalary).Append(" moedas.<BR>");
            sb.Append("Novo salário: ").Append(newSalary).Append(" moedas.<BR>");
            sb.Append("Posições ocupadas nesse cargo: ").Append(occupied).Append(".<BR>");

            if (occupiedNames.Length > 0)
                sb.Append("Ocupantes atuais: ").Append(occupiedNames.ToString()).Append(".<BR>");
            else
                sb.Append("Ocupantes atuais: nenhum.<BR>");

            sb.Append("Impacto semanal imediato: ").Append(extra).Append(" moedas.</BASEFONT>");

            ReinoPendingApproval proposal = CreateBaseApproval(actor, cityId, ReinoApprovalChangeType.SalaryChange, sb.ToString());
            proposal.RoleId = role.RoleId;
            proposal.OldSalary = oldSalary;
            proposal.NewSalary = newSalary;
            FinalizeNewProposal(proposal);
            return proposal;
        }

        private static string BuildTradeConfigHtml(ReinoCommercialTradeState oldState, int[] newBuyPrices, int[] newSellPrices, int[] newBuyCaps, int[] newSellCaps)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000><BIG><B>Mudança proposta pelo líder do reino</B></BIG><BR><BR>");

            for (int i = 0; i < 3; i++)
            {
                string label = GetTradeResourceLabel(i);
                if (oldState.BuyPrices[i] != newBuyPrices[i])
                    sb.Append(label).Append(" compra: ").Append(oldState.BuyPrices[i]).Append(" -> ").Append(newBuyPrices[i]).Append(" moedas.<BR>");
                if (oldState.SellPrices[i] != newSellPrices[i])
                    sb.Append(label).Append(" venda: ").Append(oldState.SellPrices[i]).Append(" -> ").Append(newSellPrices[i]).Append(" moedas.<BR>");
                if (oldState.WeeklyBuyCaps[i] != newBuyCaps[i])
                    sb.Append(label).Append(" máximo de compra semanal: ").Append(oldState.WeeklyBuyCaps[i]).Append(" -> ").Append(newBuyCaps[i]).Append(".<BR>");
                if (oldState.WeeklySellCaps[i] != newSellCaps[i])
                    sb.Append(label).Append(" máximo de venda semanal: ").Append(oldState.WeeklySellCaps[i]).Append(" -> ").Append(newSellCaps[i]).Append(".<BR>");
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        private static ReinoPendingApproval CreateTradeApproval(PlayerMobile actor, int cityId, int[] newBuyPrices, int[] newSellPrices, int[] newBuyCaps, int[] newSellCaps)
        {
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoPendingApproval proposal = CreateBaseApproval(actor, cityId, ReinoApprovalChangeType.TradeConfig, BuildTradeConfigHtml(state, newBuyPrices, newSellPrices, newBuyCaps, newSellCaps));
            for (int i = 0; i < 3; i++)
            {
                proposal.OldBuyPrices[i] = state.BuyPrices[i];
                proposal.OldSellPrices[i] = state.SellPrices[i];
                proposal.OldBuyCaps[i] = state.WeeklyBuyCaps[i];
                proposal.OldSellCaps[i] = state.WeeklySellCaps[i];
                proposal.NewBuyPrices[i] = Math.Max(0, newBuyPrices[i]);
                proposal.NewSellPrices[i] = Math.Max(0, newSellPrices[i]);
                proposal.NewBuyCaps[i] = Math.Max(0, newBuyCaps[i]);
                proposal.NewSellCaps[i] = Math.Max(0, newSellCaps[i]);
            }
            FinalizeNewProposal(proposal);
            return proposal;
        }

        private static void SendProposalToOnlineApprovers(ReinoPendingApproval proposal)
        {
            if (proposal == null || !proposal.IsPending)
                return;

            for (int i = 0; i < proposal.Votes.Count; i++)
            {
                PlayerMobile pm = World.FindMobile((Serial)proposal.Votes[i].VoterSerial) as PlayerMobile;
                if (pm != null && !pm.Deleted && pm.NetState != null)
                {
                    pm.CloseGump(typeof(ReinoApprovalChangeGump));
                    pm.SendGump(new ReinoApprovalChangeGump(pm, proposal.ApprovalId));
                }
            }
        }

        public static ReinoPendingApproval GetPendingApproval(int approvalId)
        {
            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoPendingApproval proposal = m_PendingApprovals[i];
                if (proposal != null && proposal.ApprovalId == approvalId)
                    return proposal;
            }
            return null;
        }

        public static ReinoPendingApproval GetPendingApprovalFor(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return null;

            ProcessPendingApprovals();

            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoPendingApproval proposal = m_PendingApprovals[i];
                if (proposal == null || !proposal.IsPending)
                    continue;

                for (int v = 0; v < proposal.Votes.Count; v++)
                {
                    ReinoPendingApprovalVote vote = proposal.Votes[v];
                    if (vote != null && vote.VoterSerial == pm.Serial.Value && vote.Decision == 0)
                        return proposal;
                }
            }

            return null;
        }

        public static void ShowPendingApprovalGump(PlayerMobile pm)
        {
            ReinoPendingApproval proposal = GetPendingApprovalFor(pm);
            if (proposal == null)
                return;

            pm.CloseGump(typeof(ReinoApprovalChangeGump));
            pm.SendGump(new ReinoApprovalChangeGump(pm, proposal.ApprovalId));
        }

        public static bool VotePendingApproval(PlayerMobile pm, int approvalId, bool approve, out string message)
        {
            message = String.Empty;
            ReinoPendingApproval proposal = GetPendingApproval(approvalId);
            if (proposal == null || !proposal.IsPending)
            {
                message = "Essa mudança já foi resolvida.";
                return false;
            }

            for (int i = 0; i < proposal.Votes.Count; i++)
            {
                ReinoPendingApprovalVote vote = proposal.Votes[i];
                if (vote != null && vote.VoterSerial == pm.Serial.Value)
                {
                    vote.Decision = approve ? 1 : 2;
                    vote.DecisionUtc = DateTime.UtcNow;
                    EvaluateProposal(proposal, true);
                    message = approve ? "Você aprovou a mudança." : "Você vetou a mudança.";
                    return true;
                }
            }

            message = "Você não pode votar nessa mudança.";
            return false;
        }

        private static void ProcessPendingApprovals()
        {
            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoPendingApproval proposal = m_PendingApprovals[i];
                if (proposal != null && proposal.IsPending)
                    EvaluateProposal(proposal, false);
            }
        }

        private static void EvaluateProposal(ReinoPendingApproval proposal, bool interactive)
        {
            if (proposal == null || !proposal.IsPending)
                return;

            string culture = GetGovernmentCultureId(proposal.CityId);
            DateTime now = DateTime.UtcNow;
            bool anyYes = false;
            bool allNo = proposal.Votes.Count > 0;
            bool allAnswered = true;

            for (int i = 0; i < proposal.Votes.Count; i++)
            {
                ReinoPendingApprovalVote vote = proposal.Votes[i];
                if (vote == null)
                    continue;

                if (vote.Decision == 0 && (now - proposal.CreatedUtc) >= TimeSpan.FromHours(48.0))
                {
                    vote.Decision = 1;
                    vote.DecisionUtc = now;
                }

                if (vote.Decision == 1)
                    anyYes = true;

                if (vote.Decision != 2)
                    allNo = false;

                if (vote.Decision == 0)
                    allAnswered = false;
            }

            if (String.Equals(culture, "zosteros", StringComparison.OrdinalIgnoreCase))
            {
                if (anyYes)
                    FinalizeProposal(proposal, true);
                else if (allAnswered && allNo)
                    FinalizeProposal(proposal, false);
            }
            else
            {
                if (anyYes && allAnswered)
                    FinalizeProposal(proposal, true);
                else if (allAnswered && !anyYes)
                    FinalizeProposal(proposal, false);
            }

            if (interactive)
            {
                for (int i = 0; i < proposal.Votes.Count; i++)
                {
                    PlayerMobile voter = World.FindMobile((Serial)proposal.Votes[i].VoterSerial) as PlayerMobile;
                    if (voter != null && !voter.Deleted && voter.NetState != null && proposal.IsPending)
                        ShowPendingApprovalGump(voter);
                }
            }
        }

        private static void FinalizeProposal(ReinoPendingApproval proposal, bool approved)
        {
            if (proposal == null || !proposal.IsPending)
                return;

            proposal.Status = approved ? 1 : 2;
            proposal.ResolvedUtc = DateTime.UtcNow;

            if (approved)
                ApplyApprovedProposal(proposal);
            else
                ApplyRejectedProposal(proposal);
        }

        private static void ApplyApprovedProposal(ReinoPendingApproval proposal)
        {
            if (proposal == null)
                return;

            if (proposal.ChangeType == ReinoApprovalChangeType.CreateRole)
            {
                ReinoCargoEntry role = GetRole(proposal.CityId, proposal.RoleId);
                if (role != null)
                    role.ApprovalState = 0;
            }
            else if (proposal.ChangeType == ReinoApprovalChangeType.SalaryChange)
            {
                ReinoCargoEntry role = GetRole(proposal.CityId, proposal.RoleId);
                if (role != null)
                {
                    List<ReinoCargoEntry> matches = GetRolesByTitleForWrite(proposal.CityId, role.Title);
                    for (int i = 0; i < matches.Count; i++)
                        matches[i].WeeklySalaryGold = Math.Max(0, proposal.NewSalary);
                }
            }
            else if (proposal.ChangeType == ReinoApprovalChangeType.TradeConfig)
            {
                ApplyTradeConfig(proposal.CityId, proposal.NewBuyPrices, proposal.NewSellPrices, proposal.NewBuyCaps, proposal.NewSellCaps);
            }

            SyncRoleDependentState(proposal.CityId);
        }

        private static void ApplyRejectedProposal(ReinoPendingApproval proposal)
        {
            if (proposal == null)
                return;

            if (proposal.ChangeType == ReinoApprovalChangeType.CreateRole)
            {
                ReinoCargoEntry role = GetRole(proposal.CityId, proposal.RoleId);
                if (role != null)
                    role.ApprovalState = 2;
            }

            SyncRoleDependentState(proposal.CityId);
        }

        private static void ProcessWeeklyTickIfNeeded()
        {
            DateTime currentWindow = GetCurrentWeeklyWindowStartUtc();
            if (m_LastWeeklyPayrollUtc >= currentWindow)
                return;

            for (int cityId = 0; cityId < (ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4); cityId++)
            {
                ResetTradeWindow(GetTradeState(cityId), false);
                PayWeeklySalaries(cityId);
            }

            m_LastWeeklyPayrollUtc = currentWindow;
        }

        private static void PayWeeklySalaries(int cityId)
        {
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null)
                return;

            List<ReinoCargoEntry> roles = GetRoles(cityId);
            roles.Sort(CompareRoles);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied || role.WeeklySalaryGold <= 0 || !role.IsApproved)
                    continue;

                if (role.Kind == ReinoCargoKind.Leader && IsSarangGovernment(cityId))
                    continue;

                int taxAmount;
                int netSalary;
                ReinoTreasurySystem.CalculateSalaryTax(cityId, role.WeeklySalaryGold, out taxAmount, out netSalary);

                if (ledger.Gold < netSalary)
                    continue;

                PlayerMobile pm = World.FindMobile((Serial)role.OccupantSerial) as PlayerMobile;
                if (pm == null || pm.Deleted || pm.BankBox == null)
                    continue;

                ledger.Add(ReinoResourceType.Gold, -netSalary);
                pm.BankBox.DropItem(new Gold(netSalary));
                ReinoTreasurySystem.RecordSalaryPayout(cityId, role.WeeklySalaryGold, taxAmount, netSalary);
            }
        }

        public static void SyncRoleDependentState(int cityId)
        {
            SyncLeaderRole(cityId, m_RolesByCity.ContainsKey(cityId) ? m_RolesByCity[cityId] : null);
            SyncCommissionedConstructionState(cityId);
            SyncAllLegacyFlags();
        }

        public static void SyncAllCommissionedConstructionState()
        {
            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
                SyncCommissionedConstructionState(kv.Key);
        }

        public static void SyncCommissionedConstructionState(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> constructions = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> salaries = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied || String.IsNullOrWhiteSpace(role.LinkedConstructionKey))
                    continue;

                int c;
                counts.TryGetValue(role.LinkedConstructionKey, out c);
                counts[role.LinkedConstructionKey] = c + 1;

                int s;
                salaries.TryGetValue(role.LinkedConstructionKey, out s);
                salaries[role.LinkedConstructionKey] = s + Math.Max(0, role.WeeklySalaryGold);
            }

            for (int i = 0; i < constructions.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = constructions[i];
                if (info == null)
                    continue;

                int count = 0;
                int salary = 0;
                counts.TryGetValue(info.Key, out count);
                salaries.TryGetValue(info.Key, out salary);

                if (info.IsArea)
                {
                    info.AreaState.CommissionedRoleCount = count;
                    info.AreaState.CommissionedRoleWeeklySalaryGold = salary;
                }
                else
                {
                    info.LotState.CommissionedRoleCount = count;
                    info.LotState.CommissionedRoleWeeklySalaryGold = salary;
                }
            }
        }

        private static void SyncAllLegacyFlags()
        {
        }

        public static void RefreshLegacyFlags(PlayerMobile pm)
        {
        }

        public static bool IsRoleAmbassadorFor(PlayerMobile pm, string cityName)
        {
            if (pm == null || pm.Deleted)
                return false;

            string normalized = PlayerMobile.NormalizeOSUCityId(cityName);

            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                string city = PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(kv.Key));
                if (!String.Equals(city, normalized, StringComparison.OrdinalIgnoreCase))
                    continue;

                List<ReinoCargoEntry> roles = kv.Value;
                for (int i = 0; i < roles.Count; i++)
                {
                    ReinoCargoEntry role = roles[i];
                    if (role != null && role.Kind == ReinoCargoKind.Ambassador && role.IsOccupied && role.OccupantSerial == pm.Serial.Value)
                        return true;
                }
            }

            return false;
        }

        public static bool IsRoleDispatcherFor(PlayerMobile pm, string cityName)
        {
            if (pm == null || pm.Deleted)
                return false;

            string normalized = PlayerMobile.NormalizeOSUCityId(cityName);

            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                string city = PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(kv.Key));
                if (!String.Equals(city, normalized, StringComparison.OrdinalIgnoreCase))
                    continue;

                List<ReinoCargoEntry> roles = kv.Value;
                for (int i = 0; i < roles.Count; i++)
                {
                    ReinoCargoEntry role = roles[i];
                    if (role != null && role.Kind == ReinoCargoKind.Dispatcher && role.IsOccupied && role.OccupantSerial == pm.Serial.Value)
                        return true;
                }
            }

            return false;
        }

        public static ReinoCommercialTradeState GetTradeState(int cityId)
        {
            ReinoCommercialTradeState state;
            if (!m_TradeByCity.TryGetValue(cityId, out state))
            {
                state = new ReinoCommercialTradeState(cityId);
                m_TradeByCity[cityId] = state;
                ResetTradeWindow(state, false);
            }

            ResetTradeWindowIfNeeded(state);
            return state;
        }

        public static void ResetTradeWindowsIfNeeded()
        {
            foreach (KeyValuePair<int, ReinoCommercialTradeState> kv in m_TradeByCity)
                ResetTradeWindowIfNeeded(kv.Value);
        }

        private static void ResetTradeWindowIfNeeded(ReinoCommercialTradeState state)
        {
            if (state == null)
                return;

            DateTime currentWindow = GetCurrentWeeklyWindowStartUtc();
            if (state.WindowStartUtc < currentWindow)
                ResetTradeWindow(state, false);
        }

        private static void ResetTradeWindow(ReinoCommercialTradeState state, bool keepRemaining)
        {
            if (state == null)
                return;

            if (!keepRemaining)
            {
                for (int i = 0; i < 3; i++)
                {
                    state.WeeklyBuyRemaining[i] = Math.Max(0, state.WeeklyBuyCaps[i]);
                    state.WeeklySellRemaining[i] = Math.Max(0, state.WeeklySellCaps[i]);
                }
            }

            state.WindowStartUtc = GetCurrentWeeklyWindowStartUtc();
        }

        public static ReinoResourceType GetTradeResourceType(int index)
        {
            switch (index)
            {
                case 0: return ReinoResourceType.Cloth;
                case 1: return ReinoResourceType.Iron;
                case 2: return ReinoResourceType.Wood;
                default: return ReinoResourceType.None;
            }
        }

        public static string GetTradeResourceLabel(int index)
        {
            switch (index)
            {
                case 0: return "Tecidos";
                case 1: return "Ferro";
                case 2: return "Madeira";
                default: return "Recurso";
            }
        }

        private static void ApplyTradeConfig(int cityId, int[] buyPrices, int[] sellPrices, int[] buyCaps, int[] sellCaps)
        {
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            for (int i = 0; i < 3; i++)
            {
                int oldBuyCap = state.WeeklyBuyCaps[i];
                int oldSellCap = state.WeeklySellCaps[i];
                int alreadyBought = Math.Max(0, oldBuyCap - state.WeeklyBuyRemaining[i]);
                int alreadySold = Math.Max(0, oldSellCap - state.WeeklySellRemaining[i]);

                state.BuyPrices[i] = Math.Max(0, buyPrices[i]);
                state.SellPrices[i] = Math.Max(0, sellPrices[i]);
                state.WeeklyBuyCaps[i] = Math.Max(0, buyCaps[i]);
                state.WeeklySellCaps[i] = Math.Max(0, Math.Min(sellCaps[i], ledger.Get(GetTradeResourceType(i))));

                state.WeeklyBuyRemaining[i] = Math.Max(0, state.WeeklyBuyCaps[i] - alreadyBought);
                state.WeeklySellRemaining[i] = Math.Max(0, state.WeeklySellCaps[i] - alreadySold);
            }
        }

        public static bool UpdateTradeConfig(PlayerMobile actor, int cityId, int[] buyPrices, int[] sellPrices, int[] buyCaps, int[] sellCaps, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governo do reino pode configurar o representante comercial.";
                return false;
            }

            int[] safeBuyPrices = new int[3];
            int[] safeSellPrices = new int[3];
            int[] safeBuyCaps = new int[3];
            int[] safeSellCaps = new int[3];
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            for (int i = 0; i < 3; i++)
            {
                safeBuyPrices[i] = Math.Max(0, buyPrices[i]);
                safeSellPrices[i] = Math.Max(0, sellPrices[i]);
                safeBuyCaps[i] = Math.Max(0, buyCaps[i]);
                safeSellCaps[i] = Math.Max(0, Math.Min(sellCaps[i], ledger.Get(GetTradeResourceType(i))));
            }

            if (!GovernmentNeedsApprovals(cityId))
            {
                ApplyTradeConfig(cityId, safeBuyPrices, safeSellPrices, safeBuyCaps, safeSellCaps);
                message = "Configuração do representante comercial atualizada.";
                return true;
            }

            if (HasPendingTradeApproval(cityId))
            {
                message = "Já existe uma mudança do representante comercial aguardando aprovação.";
                return false;
            }

            CreateTradeApproval(actor, cityId, safeBuyPrices, safeSellPrices, safeBuyCaps, safeSellCaps);
            message = "Mudança do representante comercial enviada para aprovação do governo.";
            return true;
        }

        public static bool ExecuteRepresentativeBuy(int cityId, int[] quantities, out string message)
        {
            message = String.Empty;
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            int total = 0;

            for (int i = 0; i < 3; i++)
            {
                int amount = quantities != null && i < quantities.Length ? Math.Max(0, quantities[i]) : 0;
                if (amount <= 0)
                    continue;

                if (state.BuyPrices[i] <= 0)
                {
                    message = "Defina um valor de compra para " + GetTradeResourceLabel(i) + ".";
                    return false;
                }

                if (amount > state.WeeklyBuyRemaining[i])
                {
                    message = "O limite semanal de compra de " + GetTradeResourceLabel(i) + " foi excedido.";
                    return false;
                }

                total += amount * state.BuyPrices[i];
            }

            if (total <= 0)
            {
                message = "Nenhum recurso foi selecionado para compra.";
                return false;
            }

            if (ledger.Gold < total)
            {
                message = "O tesouro do reino não possui moedas suficientes para esta compra.";
                return false;
            }

            ledger.Add(ReinoResourceType.Gold, -total);

            for (int i = 0; i < 3; i++)
            {
                int amount = quantities != null && i < quantities.Length ? Math.Max(0, quantities[i]) : 0;
                if (amount <= 0)
                    continue;

                state.WeeklyBuyRemaining[i] = Math.Max(0, state.WeeklyBuyRemaining[i] - amount);
                ledger.Add(GetTradeResourceType(i), amount);
            }

            message = "Compra confirmada. Total gasto: " + total + " moedas.";
            return true;
        }

        public static bool ExecuteRepresentativeSell(int cityId, int[] quantities, out string message)
        {
            message = String.Empty;
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            int total = 0;

            for (int i = 0; i < 3; i++)
            {
                int amount = quantities != null && i < quantities.Length ? Math.Max(0, quantities[i]) : 0;
                ReinoResourceType type = GetTradeResourceType(i);
                if (amount <= 0)
                    continue;

                if (state.SellPrices[i] <= 0)
                {
                    message = "Defina um valor de venda para " + GetTradeResourceLabel(i) + ".";
                    return false;
                }

                if (amount > state.WeeklySellRemaining[i])
                {
                    message = "O limite semanal de venda de " + GetTradeResourceLabel(i) + " foi excedido.";
                    return false;
                }

                if (ledger.Get(type) < amount)
                {
                    message = "O reino não possui " + GetTradeResourceLabel(i).ToLower() + " suficiente para esta venda.";
                    return false;
                }

                total += amount * state.SellPrices[i];
            }

            if (total <= 0)
            {
                message = "Nenhum recurso foi selecionado para venda.";
                return false;
            }

            for (int i = 0; i < 3; i++)
            {
                int amount = quantities != null && i < quantities.Length ? Math.Max(0, quantities[i]) : 0;
                if (amount <= 0)
                    continue;

                state.WeeklySellRemaining[i] = Math.Max(0, state.WeeklySellRemaining[i] - amount);
                ledger.Add(GetTradeResourceType(i), -amount);
            }

            ledger.Add(ReinoResourceType.Gold, total);
            ReinoTreasurySystem.RecordRepresentativeSale(cityId, total);
            message = "Venda confirmada. Total recebido: " + total + " moedas.";
            return true;
        }


        private static int CountTradeResourceInContainer(Container container, int index)
        {
            if (container == null)
                return 0;

            int total = 0;
            foreach (Item item in container.Items)
            {
                if (item == null || item.Deleted)
                    continue;

                Container sub = item as Container;
                if (sub != null)
                    total += CountTradeResourceInContainer(sub, index);

                if (IsMatchingTradeResource(item, index))
                    total += Math.Max(1, item.Amount);
            }
            return total;
        }

        public static int CountPlayerTradeResource(PlayerMobile pm, int index)
        {
            return pm != null && pm.Backpack != null ? CountTradeResourceInContainer(pm.Backpack, index) : 0;
        }

        private static bool IsMatchingTradeResource(Item item, int index)
        {
            if (item == null)
                return false;

            switch (index)
            {
                case 0:
                    return item is Cloth;
                case 1:
                    {
                        BaseOre ore = item as BaseOre;
                        return ore != null && ore.Resource == CraftResource.Iron;
                    }
                case 2:
                    return item is BaseLog;
                default:
                    return false;
            }
        }

        private static int ConsumeTradeResourceFromContainer(Container container, int index, int amount)
        {
            if (container == null || amount <= 0)
                return 0;

            int removed = 0;
            List<Item> snapshot = new List<Item>(container.Items);
            for (int i = 0; i < snapshot.Count && removed < amount; i++)
            {
                Item item = snapshot[i];
                if (item == null || item.Deleted)
                    continue;

                Container sub = item as Container;
                if (sub != null)
                    removed += ConsumeTradeResourceFromContainer(sub, index, amount - removed);

                if (removed >= amount)
                    break;

                if (!IsMatchingTradeResource(item, index))
                    continue;

                int take = Math.Min(amount - removed, Math.Max(1, item.Amount));
                if (item.Amount > take)
                    item.Amount -= take;
                else
                    item.Delete();

                removed += take;
            }

            return removed;
        }

        public static bool ExecuteRepresentativeBuyFromPlayer(PlayerMobile seller, int cityId, int[] quantities, out string message)
        {
            message = String.Empty;

            if (seller == null || seller.Deleted || seller.Backpack == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            int totalGold = 0;

            for (int i = 0; i < 3; i++)
            {
                int amount = quantities != null && i < quantities.Length ? Math.Max(0, quantities[i]) : 0;
                if (amount <= 0)
                    continue;

                if (state.BuyPrices[i] <= 0)
                {
                    message = "Defina um valor de compra para " + GetTradeResourceLabel(i) + ".";
                    return false;
                }

                if (amount > state.WeeklyBuyRemaining[i])
                {
                    message = "O limite semanal de compra de " + GetTradeResourceLabel(i) + " foi excedido.";
                    return false;
                }

                if (CountTradeResourceInContainer(seller.Backpack, i) < amount)
                {
                    message = "Você não possui " + GetTradeResourceLabel(i).ToLower() + " suficiente na mochila.";
                    return false;
                }

                totalGold += amount * state.BuyPrices[i];
            }

            if (totalGold <= 0)
            {
                message = "Nenhum recurso foi selecionado para venda ao reino.";
                return false;
            }

            if (ledger.Gold < totalGold)
            {
                message = "O tesouro do reino não possui moedas suficientes para esta compra.";
                return false;
            }

            for (int i = 0; i < 3; i++)
            {
                int amount = quantities != null && i < quantities.Length ? Math.Max(0, quantities[i]) : 0;
                if (amount <= 0)
                    continue;

                int removed = ConsumeTradeResourceFromContainer(seller.Backpack, i, amount);
                if (removed > 0)
                {
                    state.WeeklyBuyRemaining[i] = Math.Max(0, state.WeeklyBuyRemaining[i] - removed);
                    ledger.Add(GetTradeResourceType(i), removed);
                }
            }

            ledger.Add(ReinoResourceType.Gold, -totalGold);
            ReinoTreasurySystem.RecordRepresentativePurchase(cityId, totalGold);
            seller.AddToBackpack(new Gold(totalGold));
            message = "Venda confirmada. Você recebeu " + totalGold + " moedas.";
            return true;
        }

        public static ReinoCommercialRepresentative FindRepresentative(int cityId)
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                ReinoCommercialRepresentative npc = m as ReinoCommercialRepresentative;
                if (npc != null && !npc.Deleted && npc.GovernmentCityId == cityId)
                    return npc;
            }

            return null;
        }

        public static PedraDoReino FindGovernmentStone(int cityId)
        {
            foreach (Item item in World.Items.Values)
            {
                PedraDoReino stone = item as PedraDoReino;
                if (stone != null && !stone.Deleted && stone.CityId == cityId)
                    return stone;
            }

            return null;
        }

        public static bool SpawnRepresentative(PlayerMobile actor, int cityId, out string message)
        {
            message = String.Empty;

            if (FindRepresentative(cityId) != null)
            {
                message = "O reino já possui um representante comercial ativo.";
                return false;
            }

            PedraDoReino stone = FindGovernmentStone(cityId);
            if (stone == null || stone.Map == null || stone.Map == Map.Internal)
            {
                message = "Não foi possível localizar a pedra do reino para posicionar o representante comercial.";
                return false;
            }

            Point3D loc = new Point3D(stone.X + 1, stone.Y, stone.Z);
            int z = stone.Map.GetAverageZ(loc.X, loc.Y);
            loc = new Point3D(loc.X, loc.Y, z);

            ReinoCommercialRepresentative npc = new ReinoCommercialRepresentative(cityId);
            npc.MoveToWorld(loc, stone.Map);
            message = "Representante comercial criado com sucesso.";
            return true;
        }

        public static bool CityHasOperationalPostOffice(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null)
                    continue;

                if (info.Status != ReinoLotStatus.Active)
                    continue;

                if (!String.IsNullOrWhiteSpace(info.Definition.Id) && info.Definition.Id.IndexOf("correios", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        public static void DeliverInvitationLetter(PlayerMobile from, PlayerMobile to, int cityId, int roleId)
        {
            if (from == null || to == null)
                return;

            ReinoCargoInvitationLetter letter = new ReinoCargoInvitationLetter(cityId, roleId, to.Serial.Value, from.Name);
            letter.Name = "Carta de convite de cargo";
            DeliverLetterInternal(from, to, cityId, letter, "Carta de convite de cargo", "Você recebeu um convite para um cargo comissionado.");
        }

        public static void DeliverDismissalLetter(PlayerMobile from, PlayerMobile to, int cityId, string roleTitle, string oldName)
        {
            if (from == null || to == null)
                return;

            ReinoCargoDismissalLetter letter = new ReinoCargoDismissalLetter(cityId, roleTitle, from.Name, to.Serial.Value);
            letter.Name = "Carta de exoneração";
            DeliverLetterInternal(from, to, cityId, letter, "Carta de exoneração", "Você recebeu uma carta de exoneração.");
        }

        private static void DeliverLetterInternal(PlayerMobile from, PlayerMobile to, int cityId, Item letter, string title, string body)
        {
            if (from == null || to == null || letter == null)
                return;

            if (CityHasOperationalPostOffice(cityId))
            {
                CorreioStorage.Ensure();
                List<Item> attachments = new List<Item>();
                attachments.Add(letter);
                CorreioStorage.Instance.CreateMail(from, to, title, body, attachments);
                to.SendMessage("Uma carta foi enviada ao seu correio.");
                return;
            }

            if (to.BankBox != null)
            {
                to.BankBox.DropItem(letter);
                to.SendMessage("Uma carta foi deixada no seu banco.");
                return;
            }

            if (to.Backpack != null)
            {
                to.Backpack.DropItem(letter);
                to.SendMessage("Uma carta foi colocada na sua mochila.");
                return;
            }

            letter.MoveToWorld(to.Location, to.Map);
        }

        public static ReinoEmploymentSession GetSession(PlayerMobile pm, int cityId)
        {
            if (pm == null)
                return new ReinoEmploymentSession();

            ReinoEmploymentSession session;
            if (!m_Sessions.TryGetValue(pm.Serial.Value, out session))
            {
                session = new ReinoEmploymentSession();
                session.CityId = cityId;
                m_Sessions[pm.Serial.Value] = session;
            }

            session.CityId = cityId;
            return session;
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
                    bw.Write(3);
                    bw.Write(m_NextRoleId);
                    bw.Write(m_NextApprovalId);
                    bw.Write(m_LastWeeklyPayrollUtc.ToBinary());

                    bw.Write(m_RolesByCity.Count);
                    foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
                    {
                        bw.Write(kv.Key);
                        List<ReinoCargoEntry> list = kv.Value ?? new List<ReinoCargoEntry>();
                        bw.Write(list.Count);

                        for (int i = 0; i < list.Count; i++)
                        {
                            ReinoCargoEntry role = list[i] ?? new ReinoCargoEntry();
                            bw.Write(role.RoleId);
                            bw.Write(role.CityId);
                            bw.Write((int)role.Kind);
                            bw.Write(role.Title ?? String.Empty);
                            bw.Write(role.Description ?? String.Empty);
                            bw.Write(role.WeeklySalaryGold);
                            bw.Write(role.Hierarchy);
                            bw.Write(role.IsDefault);
                            bw.Write(role.IsRemovable);
                            bw.Write(role.IsEssential);
                            bw.Write(role.CanFinancial);
                            bw.Write(role.CanMilitary);
                            bw.Write(role.CanHireLower);
                            bw.Write(role.CanFireLower);
                            bw.Write(role.RepresentativeOnlyFinancial);
                            bw.Write(role.PostosOnlyMilitary);
                            bw.Write(role.LinkedConstructionKey ?? String.Empty);
                            bw.Write(role.OccupantSerial);
                            bw.Write(role.OccupantName ?? String.Empty);
                            bw.Write(role.ApprovalState);
                        }
                    }

                    bw.Write(m_TradeByCity.Count);
                    foreach (KeyValuePair<int, ReinoCommercialTradeState> kv in m_TradeByCity)
                    {
                        ReinoCommercialTradeState state = kv.Value;
                        bw.Write(kv.Key);
                        bw.Write(state.WindowStartUtc.ToBinary());
                        for (int i = 0; i < 3; i++)
                        {
                            bw.Write(state.BuyPrices[i]);
                            bw.Write(state.SellPrices[i]);
                            bw.Write(state.WeeklyBuyCaps[i]);
                            bw.Write(state.WeeklySellCaps[i]);
                            bw.Write(state.WeeklyBuyRemaining[i]);
                            bw.Write(state.WeeklySellRemaining[i]);
                        }
                    }

                    bw.Write(m_PendingApprovals.Count);
                    for (int i = 0; i < m_PendingApprovals.Count; i++)
                    {
                        ReinoPendingApproval proposal = m_PendingApprovals[i] ?? new ReinoPendingApproval();
                        bw.Write(proposal.ApprovalId);
                        bw.Write(proposal.CityId);
                        bw.Write(proposal.CreatedBySerial);
                        bw.Write(proposal.CreatedByName ?? String.Empty);
                        bw.Write(proposal.CreatedUtc.ToBinary());
                        bw.Write(proposal.ResolvedUtc.ToBinary());
                        bw.Write(proposal.Status);
                        bw.Write((int)proposal.ChangeType);
                        bw.Write(proposal.Html ?? String.Empty);
                        bw.Write(proposal.RoleId);
                        bw.Write(proposal.OldSalary);
                        bw.Write(proposal.NewSalary);
                        for (int r = 0; r < 3; r++)
                        {
                            bw.Write(proposal.OldBuyPrices[r]);
                            bw.Write(proposal.OldSellPrices[r]);
                            bw.Write(proposal.OldBuyCaps[r]);
                            bw.Write(proposal.OldSellCaps[r]);
                            bw.Write(proposal.NewBuyPrices[r]);
                            bw.Write(proposal.NewSellPrices[r]);
                            bw.Write(proposal.NewBuyCaps[r]);
                            bw.Write(proposal.NewSellCaps[r]);
                        }

                        bw.Write(proposal.Votes.Count);
                        for (int v = 0; v < proposal.Votes.Count; v++)
                        {
                            ReinoPendingApprovalVote vote = proposal.Votes[v] ?? new ReinoPendingApprovalVote();
                            bw.Write(vote.VoterSerial);
                            bw.Write(vote.VoterName ?? String.Empty);
                            bw.Write(vote.Decision);
                            bw.Write(vote.DecisionUtc.ToBinary());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO AO SALVAR OS CARGOS DO REINO:");
                Console.WriteLine(ex);
            }
        }

        public static void Load()
        {
            try
            {
                m_RolesByCity.Clear();
                m_TradeByCity.Clear();
                m_PendingApprovals.Clear();

                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    if (version < 1)
                        return;

                    m_NextRoleId = br.ReadInt32();
                    m_NextApprovalId = version >= 3 ? br.ReadInt32() : 1;
                    m_LastWeeklyPayrollUtc = version >= 3 ? DateTime.FromBinary(br.ReadInt64()) : DateTime.MinValue;

                    int cityCount = br.ReadInt32();
                    for (int c = 0; c < cityCount; c++)
                    {
                        int cityId = br.ReadInt32();
                        int count = br.ReadInt32();
                        List<ReinoCargoEntry> list = new List<ReinoCargoEntry>(count);

                        for (int i = 0; i < count; i++)
                        {
                            ReinoCargoEntry role = new ReinoCargoEntry();
                            role.RoleId = br.ReadInt32();
                            role.CityId = br.ReadInt32();
                            role.Kind = (ReinoCargoKind)br.ReadInt32();
                            role.Title = br.ReadString();
                            role.Description = br.ReadString();
                            role.WeeklySalaryGold = br.ReadInt32();
                            role.Hierarchy = br.ReadInt32();
                            role.IsDefault = br.ReadBoolean();
                            role.IsRemovable = br.ReadBoolean();
                            role.IsEssential = br.ReadBoolean();
                            role.CanFinancial = br.ReadBoolean();
                            role.CanMilitary = br.ReadBoolean();
                            role.CanHireLower = br.ReadBoolean();
                            role.CanFireLower = br.ReadBoolean();
                            role.RepresentativeOnlyFinancial = br.ReadBoolean();
                            role.PostosOnlyMilitary = br.ReadBoolean();
                            role.LinkedConstructionKey = br.ReadString();
                            role.OccupantSerial = br.ReadInt32();
                            role.OccupantName = br.ReadString();
                            role.ApprovalState = version >= 3 ? br.ReadInt32() : 0;
                            list.Add(role);
                        }

                        m_RolesByCity[cityId] = list;
                    }

                    int tradeCount = br.ReadInt32();
                    for (int t = 0; t < tradeCount; t++)
                    {
                        int cityId = br.ReadInt32();
                        ReinoCommercialTradeState state = new ReinoCommercialTradeState(cityId);
                        state.WindowStartUtc = DateTime.FromBinary(br.ReadInt64());
                        for (int i = 0; i < 3; i++)
                        {
                            state.BuyPrices[i] = br.ReadInt32();
                            state.SellPrices[i] = br.ReadInt32();
                            state.WeeklyBuyCaps[i] = br.ReadInt32();
                            state.WeeklySellCaps[i] = br.ReadInt32();
                            state.WeeklyBuyRemaining[i] = br.ReadInt32();
                            state.WeeklySellRemaining[i] = br.ReadInt32();
                        }
                        m_TradeByCity[cityId] = state;
                    }

                    if (version >= 3 && br.BaseStream.Position < br.BaseStream.Length)
                    {
                        int approvalCount = br.ReadInt32();
                        for (int a = 0; a < approvalCount; a++)
                        {
                            ReinoPendingApproval proposal = new ReinoPendingApproval();
                            proposal.ApprovalId = br.ReadInt32();
                            proposal.CityId = br.ReadInt32();
                            proposal.CreatedBySerial = br.ReadInt32();
                            proposal.CreatedByName = br.ReadString();
                            proposal.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
                            proposal.ResolvedUtc = DateTime.FromBinary(br.ReadInt64());
                            proposal.Status = br.ReadInt32();
                            proposal.ChangeType = (ReinoApprovalChangeType)br.ReadInt32();
                            proposal.Html = br.ReadString();
                            proposal.RoleId = br.ReadInt32();
                            proposal.OldSalary = br.ReadInt32();
                            proposal.NewSalary = br.ReadInt32();
                            for (int r = 0; r < 3; r++)
                            {
                                proposal.OldBuyPrices[r] = br.ReadInt32();
                                proposal.OldSellPrices[r] = br.ReadInt32();
                                proposal.OldBuyCaps[r] = br.ReadInt32();
                                proposal.OldSellCaps[r] = br.ReadInt32();
                                proposal.NewBuyPrices[r] = br.ReadInt32();
                                proposal.NewSellPrices[r] = br.ReadInt32();
                                proposal.NewBuyCaps[r] = br.ReadInt32();
                                proposal.NewSellCaps[r] = br.ReadInt32();
                            }

                            int voteCount = br.ReadInt32();
                            proposal.Votes.Clear();
                            for (int v = 0; v < voteCount; v++)
                            {
                                ReinoPendingApprovalVote vote = new ReinoPendingApprovalVote();
                                vote.VoterSerial = br.ReadInt32();
                                vote.VoterName = br.ReadString();
                                vote.Decision = br.ReadInt32();
                                vote.DecisionUtc = DateTime.FromBinary(br.ReadInt64());
                                proposal.Votes.Add(vote);
                            }

                            m_PendingApprovals.Add(proposal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO AO CARREGAR OS CARGOS DO REINO:");
                Console.WriteLine(ex);
            }
        }
    }
}
