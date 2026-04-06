using System;
using System.Collections.Generic;
using System.IO;
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

        public ReinoCargoEntry()
        {
            Title = String.Empty;
            Description = String.Empty;
            LinkedConstructionKey = String.Empty;
            OccupantName = String.Empty;
            Hierarchy = 99;
        }

        public bool IsOccupied
        {
            get { return OccupantSerial > 0 && !String.IsNullOrWhiteSpace(OccupantName); }
        }

        public bool IsLeaderRole
        {
            get { return Kind == ReinoCargoKind.Leader; }
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

    public class ReinoEmploymentSession
    {
        public int CityId;
        public int TopPage;
        public int BottomPage;
        public bool ShowAddList;
        public int SelectedTopRoleId;
        public int SelectedBottomIndex;
        public int EditingSalaryRoleId;
        public bool PreferBottomSelection;

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
        public string CreateInfoKey;
        public bool CreateInfoIsPermission;

        public ReinoEmploymentSession()
        {
            CreateName = String.Empty;
            CreateSalary = "0";
            CreateHierarchy = "3";
            CreateDescription = String.Empty;
            CreateLinkedConstructionKey = String.Empty;
            CreateInfoKey = String.Empty;
            CreateInfoIsPermission = false;
        }
    }

    public static class ReinoEmploymentSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoEmployment.bin");

        private static readonly Dictionary<int, List<ReinoCargoEntry>> m_RolesByCity = new Dictionary<int, List<ReinoCargoEntry>>();
        private static readonly Dictionary<int, ReinoCommercialTradeState> m_TradeByCity = new Dictionary<int, ReinoCommercialTradeState>();
        private static readonly Dictionary<int, ReinoEmploymentSession> m_Sessions = new Dictionary<int, ReinoEmploymentSession>();

        private static int m_NextRoleId = 1;
        private static DateTime m_LastWeeklyEmploymentRunUtc = DateTime.MinValue;

        public const int RepresentativeWeeklySalary = 200;

        public static void Initialize()
        {
            EnsureDefaults();
            Load();
            EnsureDefaults();
            SyncAllLeaders();
            SyncAllLegacyFlags();
            SyncAllCommissionedConstructionState();

            EventSink.WorldSave += delegate { Save(); };
            EventSink.Login += OnLogin;

            Timer.DelayCall(TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(10.0), Pulse);
        }

        private static void Pulse()
        {
            try
            {
                RunWeeklyEmploymentIfNeeded();
                ResetTradeWindowsIfNeeded();
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
                    EnsureNthCouncilRole(list, cityId, 3);
                    break;
            }

            EnsureSingleRole(list, cityId, ReinoCargoKind.Ambassador, "Embaixador", "Pode agir em nome do reino em assuntos ligados a postos e representante comercial.", 0, 3, true, false, false, true, true, false, false, String.Empty);
            EnsureSingleRole(list, cityId, ReinoCargoKind.Dispatcher, "Dispachante", "Responsável por retirar e despachar recursos dos postos do reino.", 0, 4, true, false, false, false, false, false, false, String.Empty);
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
                role.Title = title;
                role.Description = description;
                role.WeeklySalaryGold = Math.Max(0, salary);
                role.Hierarchy = hierarchy;
                role.IsDefault = isDefault;
                role.IsRemovable = removable;
                role.IsEssential = essential;
                role.CanFinancial = canFinancial;
                role.CanMilitary = canMilitary;
                role.CanHireLower = canHire;
                role.CanFireLower = canFire;
                role.LinkedConstructionKey = linkedKey ?? String.Empty;
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

            return "Cargos essenciais pendentes: " + String.Join(", ", missing.ToArray()) + ".";
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

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(targetPm, cityId))
            {
                message = "Esse jogador não pertence ao povo que pode governar este reino.";
                return false;
            }

            if (PlayerOccupiesAnyRole(targetPm))
            {
                message = "Esse jogador já ocupa outro cargo comissionado.";
                return false;
            }

            return true;
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

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(target, cityId))
            {
                message = "Você não pertence ao povo que pode governar este reino.";
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

            role.WeeklySalaryGold = Math.Max(0, newSalary);
            SyncRoleDependentState(cityId);
            message = "Salário atualizado para " + role.WeeklySalaryGold + " moedas semanais.";
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

            if (hierarchy <= 2)
            {
                message = "A hierarquia do cargo criado deve ser maior que 2.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(description))
            {
                message = "Preencha a descrição do cargo.";
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

                if (existing.Hierarchy == hierarchy && existing.Hierarchy != 2)
                {
                    message = "Essa hierarquia já está sendo usada por outro cargo.";
                    return false;
                }
            }

            ReinoCargoEntry role = CreateRole(cityId, ReinoCargoKind.Custom, title, description, salary, hierarchy, false, true, false, canFinancial, canMilitary, canHire, canFire, linkedConstructionKey);
            list.Add(role);
            SyncRoleDependentState(cityId);
            message = "Cargo criado com sucesso.";
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

                string key = (role.Title ?? String.Empty).Trim();
                if (String.IsNullOrWhiteSpace(key) || seen.Contains(key))
                    continue;

                seen.Add(key);
                list.Add(role);
            }

            return list;
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

            List<ReinoCargoEntry> list = GetRolesForWrite(cityId);
            ReinoCargoEntry copy = CreateRole(cityId, template.Kind, template.Title, template.Description, template.WeeklySalaryGold, template.Hierarchy, false, true, false, template.CanFinancial, template.CanMilitary, template.CanHireLower, template.CanFireLower, template.LinkedConstructionKey);
            copy.RepresentativeOnlyFinancial = template.RepresentativeOnlyFinancial;
            copy.PostosOnlyMilitary = template.PostosOnlyMilitary;
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

            List<ReinoCargoEntry> roles = GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || role.IsLeaderRole)
                    continue;

                if (CanActorManageLowerRole(actor, cityId, role.RoleId, forHire, out message))
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
                ResetTradeWindow(state, true, GetCurrentWeeklyEmploymentSlotUtc());
            }

            ResetTradeWindowIfNeeded(state);
            return state;
        }

        public static void ResetTradeWindowsIfNeeded()
        {
            foreach (KeyValuePair<int, ReinoCommercialTradeState> kv in m_TradeByCity)
                ResetTradeWindowIfNeeded(kv.Value);
        }

        private static DateTime GetCurrentWeeklyEmploymentSlotUtc()
        {
            DateTime now = DateTime.UtcNow;
            int daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
            DateTime monday = now.Date.AddDays(-daysSinceMonday);
            DateTime slot = monday.AddHours(21); // segunda 21:00 UTC = 18:00 Recife

            if (now < slot)
                slot = slot.AddDays(-7);

            return slot;
        }

        private static void RunWeeklyEmploymentIfNeeded()
        {
            DateTime slot = GetCurrentWeeklyEmploymentSlotUtc();
            if (DateTime.UtcNow < slot)
                return;

            if (m_LastWeeklyEmploymentRunUtc >= slot)
                return;

            m_LastWeeklyEmploymentRunUtc = slot;
            PayWeeklySalaries();

            foreach (KeyValuePair<int, ReinoCommercialTradeState> kv in m_TradeByCity)
                ResetTradeWindow(kv.Value, false, slot);
        }

        private static void PayWeeklySalaries()
        {
            foreach (KeyValuePair<int, List<ReinoCargoEntry>> kv in m_RolesByCity)
            {
                int cityId = kv.Key;
                List<ReinoCargoEntry> roles = GetRoles(cityId);
                ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

                for (int i = 0; i < roles.Count; i++)
                {
                    ReinoCargoEntry role = roles[i];
                    if (role == null || !role.IsOccupied || role.WeeklySalaryGold <= 0)
                        continue;

                    if (ledger.Gold < role.WeeklySalaryGold)
                        continue;

                    PlayerMobile pm = World.FindMobile((Serial)role.OccupantSerial) as PlayerMobile;
                    if (pm == null || pm.Deleted)
                        continue;

                    ledger.Add(ReinoResourceType.Gold, -role.WeeklySalaryGold);
                    Gold gold = new Gold(role.WeeklySalaryGold);

                    if (pm.BankBox != null)
                    {
                        pm.BankBox.DropItem(gold);
                        pm.SendMessage("Seu salário do cargo de " + role.Title + " foi pago no banco.");
                    }
                    else if (pm.Backpack != null)
                    {
                        pm.Backpack.DropItem(gold);
                        pm.SendMessage("Seu salário do cargo de " + role.Title + " foi pago na mochila.");
                    }
                    else
                    {
                        gold.MoveToWorld(pm.Location, pm.Map);
                        pm.SendMessage("Seu salário do cargo de " + role.Title + " foi pago no chão aos seus pés.");
                    }
                }
            }
        }

        private static void ResetTradeWindowIfNeeded(ReinoCommercialTradeState state)
        {
            if (state == null)
                return;

            DateTime slot = GetCurrentWeeklyEmploymentSlotUtc();
            if (state.WindowStartUtc < slot)
                ResetTradeWindow(state, false, slot);
        }

        private static void ResetTradeWindow(ReinoCommercialTradeState state, bool onlyIfEmpty, DateTime slot)
        {
            if (state == null)
                return;

            for (int i = 0; i < 3; i++)
            {
                int buyCap = Math.Max(0, state.WeeklyBuyCaps[i]);
                int sellCap = Math.Max(0, state.WeeklySellCaps[i]);

                if (onlyIfEmpty)
                {
                    if (state.WeeklyBuyRemaining[i] <= 0)
                        state.WeeklyBuyRemaining[i] = buyCap;
                    if (state.WeeklySellRemaining[i] <= 0)
                        state.WeeklySellRemaining[i] = sellCap;
                }
                else
                {
                    state.WeeklyBuyRemaining[i] = buyCap;
                    state.WeeklySellRemaining[i] = sellCap;
                }
            }

            state.WindowStartUtc = slot == DateTime.MinValue ? DateTime.UtcNow : slot;
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

        public static int GetEffectiveRepresentativeBuyRemaining(int cityId, int index)
        {
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            int remaining = index >= 0 && index < state.WeeklyBuyRemaining.Length ? Math.Max(0, state.WeeklyBuyRemaining[index]) : 0;
            int price = index >= 0 && index < state.BuyPrices.Length ? Math.Max(0, state.BuyPrices[index]) : 0;
            if (price <= 0)
                return 0;

            int affordable = ledger.Gold / price;
            if (affordable < 0)
                affordable = 0;

            return Math.Min(remaining, affordable);
        }

        public static int GetEffectiveRepresentativeSellRemaining(int cityId, int index)
        {
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            int remaining = index >= 0 && index < state.WeeklySellRemaining.Length ? Math.Max(0, state.WeeklySellRemaining[index]) : 0;
            int stock = ledger.Get(GetTradeResourceType(index));
            return Math.Min(remaining, Math.Max(0, stock));
        }

        public static bool UpdateTradeConfig(int cityId, int[] buyPrices, int[] sellPrices, int[] buyCaps, int[] sellCaps, out string message)
        {
            message = String.Empty;
            ReinoCommercialTradeState state = GetTradeState(cityId);
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            for (int i = 0; i < 3; i++)
            {
                int buyPrice = buyPrices != null && i < buyPrices.Length ? Math.Max(0, buyPrices[i]) : 0;
                int sellPrice = sellPrices != null && i < sellPrices.Length ? Math.Max(0, sellPrices[i]) : 0;
                int buyCap = buyCaps != null && i < buyCaps.Length ? Math.Max(0, buyCaps[i]) : 0;
                int sellCap = sellCaps != null && i < sellCaps.Length ? Math.Max(0, sellCaps[i]) : 0;

                int stock = Math.Max(0, ledger.Get(GetTradeResourceType(i)));
                if (sellCap > stock)
                    sellCap = stock;

                state.BuyPrices[i] = buyPrice;
                state.SellPrices[i] = sellPrice;
                state.WeeklyBuyCaps[i] = buyCap;
                state.WeeklySellCaps[i] = sellCap;
            }

            ResetTradeWindow(state, false, GetCurrentWeeklyEmploymentSlotUtc());
            message = "Configuração do representante comercial atualizada.";
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
                    bw.Write(2);
                    bw.Write(m_NextRoleId);
                    bw.Write(m_LastWeeklyEmploymentRunUtc.ToBinary());

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

                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    if (version < 1)
                        return;

                    m_NextRoleId = br.ReadInt32();
                    m_LastWeeklyEmploymentRunUtc = version >= 2 ? DateTime.FromBinary(br.ReadInt64()) : DateTime.MinValue;

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
