using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Custom.Systems.Postos;
using Server.Custom.Systems.Rent;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Custom.Reinos
{
    public class ReinoTreasuryResourceBundle
    {
        public int Gold;
        public int Cloth;
        public int Iron;
        public int Wood;

        public ReinoTreasuryResourceBundle()
        {
        }

        public ReinoTreasuryResourceBundle(int gold, int cloth, int iron, int wood)
        {
            Gold = gold;
            Cloth = cloth;
            Iron = iron;
            Wood = wood;
        }

        public ReinoTreasuryResourceBundle Clone()
        {
            return new ReinoTreasuryResourceBundle(Gold, Cloth, Iron, Wood);
        }

        public void Add(ReinoTreasuryResourceBundle other)
        {
            if (other == null)
                return;

            Gold += other.Gold;
            Cloth += other.Cloth;
            Iron += other.Iron;
            Wood += other.Wood;
        }
    }

    public class ReinoTreasuryWeekRecord
    {
        public ReinoTreasuryResourceBundle CitizenTaxes;
        public ReinoTreasuryResourceBundle ConstructionIncome;
        public ReinoTreasuryResourceBundle PostoIncome;
        public ReinoTreasuryResourceBundle DonationIncome;
        public ReinoTreasuryResourceBundle AuctionIncome;
        public ReinoTreasuryResourceBundle DiplomacyIncome;
        public ReinoTreasuryResourceBundle VendorIncome;
        public ReinoTreasuryResourceBundle RepresentativeIncome;

        public ReinoTreasuryResourceBundle MaintenanceExpense;
        public ReinoTreasuryResourceBundle SalaryExpense;
        public ReinoTreasuryResourceBundle GuardExpense;
        public ReinoTreasuryResourceBundle DiplomacyExpense;
        public ReinoTreasuryResourceBundle RepresentativeExpense;

        public ReinoTreasuryWeekRecord()
        {
            CitizenTaxes = new ReinoTreasuryResourceBundle();
            ConstructionIncome = new ReinoTreasuryResourceBundle();
            PostoIncome = new ReinoTreasuryResourceBundle();
            DonationIncome = new ReinoTreasuryResourceBundle();
            AuctionIncome = new ReinoTreasuryResourceBundle();
            DiplomacyIncome = new ReinoTreasuryResourceBundle();
            VendorIncome = new ReinoTreasuryResourceBundle();
            RepresentativeIncome = new ReinoTreasuryResourceBundle();

            MaintenanceExpense = new ReinoTreasuryResourceBundle();
            SalaryExpense = new ReinoTreasuryResourceBundle();
            GuardExpense = new ReinoTreasuryResourceBundle();
            DiplomacyExpense = new ReinoTreasuryResourceBundle();
            RepresentativeExpense = new ReinoTreasuryResourceBundle();
        }

        public ReinoTreasuryWeekRecord Clone()
        {
            ReinoTreasuryWeekRecord record = new ReinoTreasuryWeekRecord();
            record.CitizenTaxes = CitizenTaxes.Clone();
            record.ConstructionIncome = ConstructionIncome.Clone();
            record.PostoIncome = PostoIncome.Clone();
            record.DonationIncome = DonationIncome.Clone();
            record.AuctionIncome = AuctionIncome.Clone();
            record.DiplomacyIncome = DiplomacyIncome.Clone();
            record.VendorIncome = VendorIncome.Clone();
            record.RepresentativeIncome = RepresentativeIncome.Clone();

            record.MaintenanceExpense = MaintenanceExpense.Clone();
            record.SalaryExpense = SalaryExpense.Clone();
            record.GuardExpense = GuardExpense.Clone();
            record.DiplomacyExpense = DiplomacyExpense.Clone();
            record.RepresentativeExpense = RepresentativeExpense.Clone();
            return record;
        }
    }

    public class ReinoTreasuryApprovalVote
    {
        public int VoterSerial;
        public string VoterName;
        public int Decision;
        public DateTime DecisionUtc;

        public ReinoTreasuryApprovalVote()
        {
            VoterName = String.Empty;
            Decision = 0;
            DecisionUtc = DateTime.MinValue;
        }
    }

    public class ReinoTreasuryPendingApproval
    {
        public int ApprovalId;
        public int CityId;
        public int CreatedBySerial;
        public string CreatedByName;
        public DateTime CreatedUtc;
        public DateTime ResolvedUtc;
        public int Status;
        public string Html;

        public int OldWeeklyCitizenTax;
        public int OldVendorSalesTaxPercent;
        public int OldSalaryTaxPercent;
        public int OldAuctionTaxPercent;
        public int OldReligiousDonationTaxPercent;

        public int NewWeeklyCitizenTax;
        public int NewVendorSalesTaxPercent;
        public int NewSalaryTaxPercent;
        public int NewAuctionTaxPercent;
        public int NewReligiousDonationTaxPercent;

        public List<ReinoTreasuryApprovalVote> Votes;

        public ReinoTreasuryPendingApproval()
        {
            CreatedByName = String.Empty;
            Html = String.Empty;
            Votes = new List<ReinoTreasuryApprovalVote>();
        }

        public bool IsPending
        {
            get { return Status == 0; }
        }
    }

    public class ReinoTreasuryCityState
    {
        public int CityId;
        public int WeeklyCitizenTax;
        public int VendorSalesTaxPercent;
        public int SalaryTaxPercent;
        public int AuctionTaxPercent;
        public int ReligiousDonationTaxPercent;

        public int CitizenTaxNoticeVersion;
        public int CitizenTaxNoticeType; // 0 = nenhum, 1 = aumento, 2 = redução
        public Dictionary<int, int> SeenCitizenTaxNoticeByPlayer;

        public DateTime CurrentWeekStartUtc;
        public DateTime LastCitizenTaxChargeUtc;
        public DateTime LastSnapshotUtc;

        public ReinoTreasuryResourceBundle LastWeekSnapshot;
        public ReinoTreasuryResourceBundle PostoWeekStartSnapshot;
        public ReinoTreasuryWeekRecord CurrentWeek;
        public ReinoTreasuryWeekRecord LastClosedWeek;
        public ReinoTreasuryResourceBundle TotalDonationHistory;

        public ReinoTreasuryCityState()
        {
            SeenCitizenTaxNoticeByPlayer = new Dictionary<int, int>();
            LastWeekSnapshot = new ReinoTreasuryResourceBundle();
            PostoWeekStartSnapshot = new ReinoTreasuryResourceBundle();
            CurrentWeek = new ReinoTreasuryWeekRecord();
            LastClosedWeek = new ReinoTreasuryWeekRecord();
            TotalDonationHistory = new ReinoTreasuryResourceBundle();
        }

        public ReinoTreasuryCityState(int cityId) : this()
        {
            CityId = cityId;
        }
    }

    public static class ReinoTreasurySystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoTreasury.bin");
        private static readonly Dictionary<int, ReinoTreasuryCityState> m_Cities = new Dictionary<int, ReinoTreasuryCityState>();
        private static readonly List<ReinoTreasuryPendingApproval> m_PendingApprovals = new List<ReinoTreasuryPendingApproval>();

        private static int m_NextApprovalId = 1;

        public static void Initialize()
        {
            EnsureDefaults();
            Load();
            EnsureDefaults();

            EventSink.WorldSave += delegate { Save(); };
            EventSink.Login += OnLogin;

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(5.0), Pulse);
        }

        private static void EnsureDefaults()
        {
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;

            for (int cityId = 0; cityId < count; cityId++)
            {
                ReinoTreasuryCityState state = GetState(cityId);

                if (state.CurrentWeekStartUtc == DateTime.MinValue)
                    state.CurrentWeekStartUtc = GetStatsWeekStartUtc();

                if (state.PostoWeekStartSnapshot == null)
                    state.PostoWeekStartSnapshot = GetCurrentPostoResources(cityId);

                if (state.LastWeekSnapshot == null)
                    state.LastWeekSnapshot = GetCombinedTreasuryResources(cityId);

                if (state.CurrentWeek == null)
                    state.CurrentWeek = new ReinoTreasuryWeekRecord();

                if (state.LastClosedWeek == null)
                    state.LastClosedWeek = new ReinoTreasuryWeekRecord();

                if (state.TotalDonationHistory == null)
                    state.TotalDonationHistory = new ReinoTreasuryResourceBundle();

                if (state.SeenCitizenTaxNoticeByPlayer == null)
                    state.SeenCitizenTaxNoticeByPlayer = new Dictionary<int, int>();
            }
        }

        private static ReinoTreasuryCityState GetState(int cityId)
        {
            ReinoTreasuryCityState state;
            if (!m_Cities.TryGetValue(cityId, out state))
            {
                state = new ReinoTreasuryCityState(cityId);
                m_Cities[cityId] = state;
            }

            return state;
        }

        private static DateTime GetLocalNow()
        {
            return DateTime.UtcNow.AddHours(-3.0);
        }

        private static DateTime GetSnapshotWindowUtc()
        {
            DateTime localNow = GetLocalNow();
            int mondayOffset = ((int)localNow.DayOfWeek + 6) % 7;
            DateTime mondayNoon = localNow.Date.AddDays(-mondayOffset).AddHours(12.0);

            if (localNow < mondayNoon)
                mondayNoon = mondayNoon.AddDays(-7.0);

            return mondayNoon.AddHours(3.0);
        }

        private static DateTime GetStatsWeekStartUtc()
        {
            DateTime localNow = GetLocalNow();
            int mondayOffset = ((int)localNow.DayOfWeek + 6) % 7;
            DateTime mondayTax = localNow.Date.AddDays(-mondayOffset).AddHours(16.0);

            if (localNow < mondayTax)
                mondayTax = mondayTax.AddDays(-7.0);

            return mondayTax.AddHours(3.0);
        }

        private static DateTime GetCitizenTaxRunUtc()
        {
            return GetStatsWeekStartUtc();
        }

        private static void Pulse()
        {
            try
            {
                EnsureDefaults();
                ProcessWeekRoll();
                ProcessCitizenTaxes();
                ProcessPendingApprovals();
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

            try
            {
                ShowCitizenTaxNoticeIfNeeded(pm);
                ShowPendingApprovalGump(pm);
            }
            catch
            {
            }
        }

        private static ReinoTreasuryResourceBundle GetTreasuryLedgerResources(int cityId)
        {
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            return new ReinoTreasuryResourceBundle(
                ledger != null ? ledger.Gold : 0,
                ledger != null ? ledger.Cloth : 0,
                ledger != null ? ledger.Iron : 0,
                ledger != null ? ledger.Wood : 0);
        }

        public static ReinoTreasuryResourceBundle GetCombinedTreasuryResources(int cityId)
        {
            return GetTreasuryLedgerResources(cityId);
        }

        private static ReinoTreasuryResourceBundle GetCurrentPostoResources(int cityId)
        {
            PostoKingdomResourceLedger posto = GetPostoLedger(cityId);

            return new ReinoTreasuryResourceBundle(
                0,
                posto != null ? posto.Cotton : 0,
                posto != null ? posto.Iron : 0,
                posto != null ? posto.Wood : 0);
        }

        private static PostoKingdomResourceLedger GetPostoLedger(int cityId)
        {
            return PostoSystem.GetLedger(ReinoElectionsSystem.GetCityName(cityId));
        }

        public static ReinoTreasuryResourceBundle GetLastWeekSnapshot(int cityId)
        {
            return GetState(cityId).LastWeekSnapshot.Clone();
        }

        public static ReinoTreasuryWeekRecord GetLastClosedWeek(int cityId)
        {
            return GetState(cityId).LastClosedWeek.Clone();
        }

        public static ReinoTreasuryResourceBundle GetAllTimeDonations(int cityId)
        {
            return GetState(cityId).TotalDonationHistory.Clone();
        }

        public static int GetWeeklyCitizenTax(int cityId)
        {
            return GetState(cityId).WeeklyCitizenTax;
        }

        public static int GetVendorSalesTaxPercent(int cityId)
        {
            return GetState(cityId).VendorSalesTaxPercent;
        }

        public static int GetSalaryTaxPercent(int cityId)
        {
            return GetState(cityId).SalaryTaxPercent;
        }

        public static int GetAuctionTaxPercent(int cityId)
        {
            return GetState(cityId).AuctionTaxPercent;
        }

        public static int GetReligiousDonationTaxPercent(int cityId)
        {
            return GetState(cityId).ReligiousDonationTaxPercent;
        }

        public static int GetCitizenCount(int cityId)
        {
            int total = 0;
            string cityName = PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(cityId));

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (String.Equals(PlayerMobile.NormalizeOSUCityId(pm.OSUCitizenCityId), cityName, StringComparison.OrdinalIgnoreCase))
                    total++;
            }

            return total;
        }

        private static void ProcessWeekRoll()
        {
            DateTime currentWeekStart = GetStatsWeekStartUtc();
            DateTime currentSnapshot = GetSnapshotWindowUtc();
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;

            for (int cityId = 0; cityId < count; cityId++)
            {
                ReinoTreasuryCityState state = GetState(cityId);

                if (state.LastSnapshotUtc < currentSnapshot)
                {
                    state.LastSnapshotUtc = currentSnapshot;
                    state.LastWeekSnapshot = GetCombinedTreasuryResources(cityId);
                }

                if (state.CurrentWeekStartUtc < currentWeekStart)
                {
                    FinalizeWeek(cityId, state);
                    state.CurrentWeekStartUtc = currentWeekStart;
                    state.CurrentWeek = new ReinoTreasuryWeekRecord();
                    state.PostoWeekStartSnapshot = GetCurrentPostoResources(cityId);
                }
            }
        }

        private static void FinalizeWeek(int cityId, ReinoTreasuryCityState state)
        {
            if (state == null)
                return;

            ReinoTreasuryWeekRecord closed = state.CurrentWeek != null ? state.CurrentWeek.Clone() : new ReinoTreasuryWeekRecord();
            closed.ConstructionIncome = GetConstructionIncomeLast7Days(cityId);
            closed.MaintenanceExpense = GetMaintenanceBaseline(cityId);

            ReinoTreasuryResourceBundle postoNow = GetCurrentPostoResources(cityId);
            int cloth = Math.Max(0, postoNow.Cloth - state.PostoWeekStartSnapshot.Cloth);
            int iron = Math.Max(0, postoNow.Iron - state.PostoWeekStartSnapshot.Iron);
            int wood = Math.Max(0, postoNow.Wood - state.PostoWeekStartSnapshot.Wood);

            closed.PostoIncome = new ReinoTreasuryResourceBundle(0, cloth, iron, wood);

            state.LastClosedWeek = closed;
        }

        public static ReinoTreasuryResourceBundle GetConstructionIncomeLast7Days(int cityId)
        {
            ReinoTreasuryResourceBundle bundle = new ReinoTreasuryResourceBundle();
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);

            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null)
                    continue;

                bundle.Gold += Math.Max(0, ReinoMaintenanceSystem.GetRevenueLast7DaysGold(info));
            }

            return bundle;
        }

        public static ReinoTreasuryResourceBundle GetMaintenanceBaseline(int cityId)
        {
            ReinoTreasuryResourceBundle bundle = new ReinoTreasuryResourceBundle();
            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(cityId);

            for (int i = 0; i < active.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = active[i];
                if (info == null)
                    continue;

                bundle.Gold += Math.Max(0, ReinoMaintenanceSystem.GetBaseMaintenanceGoldOnly(info));

                if (info.Definition == null || info.Definition.MaintenanceCosts == null)
                    continue;

                for (int c = 0; c < info.Definition.MaintenanceCosts.Length; c++)
                {
                    ReinoResourceCost cost = info.Definition.MaintenanceCosts[c];
                    if (cost == null || cost.Amount <= 0)
                        continue;

                    switch (cost.Type)
                    {
                        case ReinoResourceType.Cloth:
                            bundle.Cloth += cost.Amount;
                            break;
                        case ReinoResourceType.Iron:
                            bundle.Iron += cost.Amount;
                            break;
                        case ReinoResourceType.Wood:
                            bundle.Wood += cost.Amount;
                            break;
                        case ReinoResourceType.Gold:
                            bundle.Gold += cost.Amount;
                            break;
                    }
                }
            }

            return bundle;
        }

        public static ReinoTreasuryResourceBundle GetRecurringIncomeBaseline(int cityId)
        {
            ReinoTreasuryResourceBundle bundle = new ReinoTreasuryResourceBundle();
            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(cityId);

            for (int i = 0; i < active.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = active[i];
                if (info == null)
                    continue;

                bundle.Gold += Math.Max(0, ReinoMaintenanceSystem.GetCurrentRecurringRevenueGold(info));
            }

            bundle.Gold += Math.Max(0, GetWeeklyCitizenTax(cityId)) * Math.Max(0, GetCitizenCount(cityId));
            return bundle;
        }

        public static ReinoTreasuryResourceBundle GetRecurringExpenseBaseline(int cityId)
        {
            ReinoTreasuryResourceBundle bundle = GetMaintenanceBaseline(cityId);
            bundle.Gold += Math.Max(0, GetNetWeeklyCommissionSalary(cityId));
            return bundle;
        }

        public static int GetGrossWeeklyCommissionSalary(int cityId)
        {
            int total = 0;
            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);
            bool sarang = String.Equals(ReinoEmploymentSystem.GetGovernmentCultureId(cityId), "sarangs", StringComparison.OrdinalIgnoreCase);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied || !role.IsApproved || role.WeeklySalaryGold <= 0)
                    continue;

                if (sarang && role.Kind == ReinoCargoKind.Leader)
                    continue;

                total += role.WeeklySalaryGold;
            }

            return total;
        }

        public static int GetNetWeeklyCommissionSalary(int cityId)
        {
            int gross = GetGrossWeeklyCommissionSalary(cityId);
            int tax = (gross * Math.Max(0, Math.Min(50, GetSalaryTaxPercent(cityId)))) / 100;
            return Math.Max(0, gross - tax);
        }

        public static int GetOccupiedGovernmentRoleCount(int cityId)
        {
            int total = 0;
            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role != null && role.IsOccupied && role.IsApproved)
                    total++;
            }

            return total;
        }

        public static int GetGovernmentRoleSlotCount(int cityId)
        {
            int total = 0;
            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);

            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i] != null)
                    total++;
            }

            return total;
        }

        public static bool HasAuctionHouse(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(cityId);

            for (int i = 0; i < active.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = active[i];
                if (info == null || info.Definition == null)
                    continue;

                string id = info.Definition.Id ?? String.Empty;
                string name = info.Definition.Name ?? String.Empty;

                if (id.IndexOf("leil", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    id.IndexOf("auction", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("leil", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("auction", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static void ProcessCitizenTaxes()
        {
            DateTime runUtc = GetCitizenTaxRunUtc();
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;

            for (int cityId = 0; cityId < count; cityId++)
            {
                ReinoTreasuryCityState state = GetState(cityId);
                int tax = Math.Max(0, state.WeeklyCitizenTax);

                if (tax <= 0)
                    continue;

                if (state.LastCitizenTaxChargeUtc >= runUtc)
                    continue;

                string cityName = PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(cityId));
                int collected = 0;

                foreach (Mobile m in World.Mobiles.Values)
                {
                    PlayerMobile pm = m as PlayerMobile;
                    if (pm == null || pm.Deleted)
                        continue;

                    if (!String.Equals(PlayerMobile.NormalizeOSUCityId(pm.OSUCitizenCityId), cityName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (Banker.Withdraw(pm, tax, false))
                    {
                        ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, tax);
                        collected += tax;

                        if (pm.NetState != null)
                            pm.SendMessage("O imposto semanal de " + tax + " moedas do reino foi retirado do seu banco.");
                    }
                    else if (pm.NetState != null)
                    {
                        pm.SendMessage("Você não tinha saldo suficiente no banco para pagar o imposto semanal de " + tax + " moedas ao reino.");
                    }
                }

                if (collected > 0)
                    state.CurrentWeek.CitizenTaxes.Gold += collected;

                state.LastCitizenTaxChargeUtc = runUtc;
            }
        }

        public static bool ApplyVendorSaleTax(PlayerVendor vendor, int grossSaleValue, out int kingdomTax)
        {
            kingdomTax = 0;

            if (vendor == null || vendor.Deleted || grossSaleValue <= 0)
                return false;

            int cityId = GetVendorGovernmentCityId(vendor);
            if (cityId < 0)
                return false;

            int percent = Math.Max(0, Math.Min(50, GetVendorSalesTaxPercent(cityId)));
            if (percent <= 0)
                return false;

            kingdomTax = (grossSaleValue * percent) / 100;
            if (kingdomTax <= 0)
                return false;

            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, kingdomTax);
            GetState(cityId).CurrentWeek.VendorIncome.Gold += kingdomTax;
            return true;
        }

        public static int GetVendorGovernmentCityId(PlayerVendor vendor)
        {
            if (vendor == null || vendor.Deleted || vendor.House == null)
                return -1;

            for (int i = 0; i < TownHouseSign.AllSigns.Count; i++)
            {
                TownHouseSign sign = TownHouseSign.AllSigns[i] as TownHouseSign;
                if (sign == null || sign.Deleted || sign.House == null)
                    continue;

                if (sign.House != vendor.House)
                    continue;

                if (!sign.GovernmentManaged || sign.GovernmentCityId < 0)
                    return -1;

                if (sign.PropertyType != OSUPropertyType.Commercial)
                    return -1;

                return sign.GovernmentCityId;
            }

            return -1;
        }

        public static void CalculateSalaryTax(int cityId, int grossSalary, out int taxAmount, out int netSalary)
        {
            grossSalary = Math.Max(0, grossSalary);
            int percent = Math.Max(0, Math.Min(50, GetSalaryTaxPercent(cityId)));
            taxAmount = (grossSalary * percent) / 100;
            if (taxAmount > grossSalary)
                taxAmount = grossSalary;
            netSalary = Math.Max(0, grossSalary - taxAmount);
        }

        public static void RecordSalaryPayout(int cityId, int grossSalary, int taxAmount, int netSalary)
        {
            ReinoTreasuryCityState state = GetState(cityId);
            if (state == null)
                return;

            if (netSalary > 0)
                state.CurrentWeek.SalaryExpense.Gold += netSalary;
        }

        public static void RecordRepresentativeSale(int cityId, int goldReceived)
        {
            if (cityId < 0 || goldReceived <= 0)
                return;

            GetState(cityId).CurrentWeek.RepresentativeIncome.Gold += goldReceived;
        }

        public static void RecordRepresentativePurchase(int cityId, int goldSpent)
        {
            if (cityId < 0 || goldSpent <= 0)
                return;

            GetState(cityId).CurrentWeek.RepresentativeExpense.Gold += goldSpent;
        }

        public static void RecordDonationToKingdom(int cityId, int gold, int cloth, int iron, int wood)
        {
            if (cityId < 0)
                return;

            ReinoTreasuryCityState state = GetState(cityId);
            gold = Math.Max(0, gold);
            cloth = Math.Max(0, cloth);
            iron = Math.Max(0, iron);
            wood = Math.Max(0, wood);

            if (gold > 0)
                ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, gold);
            if (cloth > 0)
                ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Cloth, cloth);
            if (iron > 0)
                ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Iron, iron);
            if (wood > 0)
                ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Wood, wood);

            state.CurrentWeek.DonationIncome.Gold += gold;
            state.CurrentWeek.DonationIncome.Cloth += cloth;
            state.CurrentWeek.DonationIncome.Iron += iron;
            state.CurrentWeek.DonationIncome.Wood += wood;

            state.TotalDonationHistory.Gold += gold;
            state.TotalDonationHistory.Cloth += cloth;
            state.TotalDonationHistory.Iron += iron;
            state.TotalDonationHistory.Wood += wood;
        }

        public static void RecordAuctionTax(int cityId, int gold)
        {
            if (cityId < 0 || gold <= 0)
                return;

            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, gold);
            GetState(cityId).CurrentWeek.AuctionIncome.Gold += gold;
        }

        public static void RecordDiplomacyIncome(int cityId, int gold, int cloth, int iron, int wood)
        {
            if (cityId < 0)
                return;

            GetState(cityId).CurrentWeek.DiplomacyIncome.Add(new ReinoTreasuryResourceBundle(gold, cloth, iron, wood));
        }

        public static void RecordDiplomacyExpense(int cityId, int gold, int cloth, int iron, int wood)
        {
            if (cityId < 0)
                return;

            GetState(cityId).CurrentWeek.DiplomacyExpense.Add(new ReinoTreasuryResourceBundle(gold, cloth, iron, wood));
        }

        private static string GetLeaderSignatureName(int cityId)
        {
            ReinoCityData city;
            if (ReinoElectionsSystem._cities.TryGetValue(cityId, out city) && city != null && !String.IsNullOrWhiteSpace(city.GovernorName))
                return city.GovernorName;

            return ReinoEmploymentSystem.GetLeaderTitle(cityId);
        }

        public static string GetCitizenTaxNoticeTitle(int cityId, int amount, int noticeType)
        {
            if (noticeType == 2)
            {
                if (amount <= 0)
                    return "Redução de impostos";
                return "Redução de impostos";
            }

            return "Nova cobrança de impostos";
        }

        public static string GetCitizenTaxNoticeText(int cityId, int amount, int noticeType)
        {
            string cityName = ReinoElectionsSystem.GetCityName(cityId);
            string signature = GetLeaderSignatureName(cityId);

            if (noticeType == 2)
            {
                switch (ReinoEmploymentSystem.GetGovernmentCultureId(cityId))
                {
                    case "kamay":
                        if (amount <= 0)
                        {
                            return
                                "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                                "Cidadãos de " + cityName + ", o imposto semanal do reino foi retirado.<BR><BR>" +
                                "Os armazéns estão cheios, as oficinas seguem produzindo, e a administração entende que este " +
                                "é um momento em que o peso do governo pode ser aliviado sobre os ombros do povo. Entre os kamay, " +
                                "prosperar não é apenas acumular riqueza, mas saber quando repartir os frutos de uma cidade estável " +
                                "e bem conduzida.<BR><BR>" +
                                "Que cada casa sinta este respiro como sinal de força, de organização e de confiança no futuro do " +
                                "reino. Quando a cidade caminha bem, sua abundância deve ser percebida também pelos seus cidadãos.<BR><BR>" +
                                "A cobrança de impostos deixa de ser aplicada a partir de agora.<BR><BR>" +
                                "Assinado, " + signature + ".";
                        }

                        return
                            "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                            "Cidadãos de " + cityName + ", o imposto semanal do reino foi reduzido para <B>" + amount + " moedas</B> " +
                            "por cidadão.<BR><BR>" +
                            "Os celeiros permanecem fartos, os armazéns seguem abastecidos e o trabalho do reino deu frutos. " +
                            "Entre os kamay, prosperidade não se mede apenas pelo que o governo guarda, mas também pela capacidade " +
                            "de aliviar o peso que recai sobre o seu povo quando a cidade atravessa tempos favoráveis.<BR><BR>" +
                            "Esta redução representa confiança na estabilidade do reino e reconhecimento pelo esforço de todos que " +
                            "ajudaram a sustentar sua ordem, sua produção e seu crescimento. Quando a cidade prospera, seus cidadãos " +
                            "devem sentir essa prosperidade em sua própria rotina.<BR><BR>" +
                            "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                            "Assinado, " + signature + ".";

                    case "matalun":
                        if (amount <= 0)
                        {
                            return
                                "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                                "Povo de " + cityName + ", a contribuição semanal do reino foi suspensa.<BR><BR>" +
                                "Os tempos são favoráveis, e a cidade atravessa um período de equilíbrio e fartura. Entre os matalun, " +
                                "governar também é reconhecer quando o reino está suficientemente forte para exigir menos dos seus " +
                                "fiéis e devolver ao povo parte da tranquilidade que ele ajudou a construir.<BR><BR>" +
                                "Que esta decisão seja recebida como sinal de harmonia, prosperidade e gratidão. Quando o reino floresce, " +
                                "sua paz deve alcançar também os lares de quem o sustenta.<BR><BR>" +
                                "A cobrança de impostos deixa de ser aplicada a partir de agora.<BR><BR>" +
                                "Assinado, " + signature + ".";
                        }

                        return
                            "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                            "Povo de " + cityName + ", a contribuição semanal do reino foi reduzida para <B>" + amount + " moedas</B> " +
                            "por cidadão.<BR><BR>" +
                            "A cidade atravessa um período de equilíbrio, e os sinais de boa administração permitem que o reino peça " +
                            "menos àqueles que o sustentam. Entre os matalun, governar também é reconhecer quando a ordem foi preservada " +
                            "e quando é justo devolver ao povo parte da tranquilidade conquistada em conjunto.<BR><BR>" +
                            "Que esta decisão seja recebida como um gesto de prudência, gratidão e confiança no caminho que o reino vem " +
                            "seguindo. Quando a comunidade floresce, seu alívio também deve ser compartilhado entre os seus cidadãos.<BR><BR>" +
                            "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                            "Assinado, " + signature + ".";

                    case "sarangs":
                        if (amount <= 0)
                        {
                            return
                                "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                                "Por determinação do governo de " + cityName + ", a cobrança semanal de " +
                                "impostos foi encerrada.<BR><BR>" +
                                "A disciplina do reino deu resultado. Os cofres permanecem sólidos, a estrutura " +
                                "continua firme, e a autoridade reconhece que um povo obediente e produtivo merece " +
                                "sentir, em tempos de prosperidade, o peso reduzido do comando. Entre os sarangs, " +
                                "força também é saber aliviar a cobrança quando a ordem já foi conquistada.<BR><BR>" +
                                "Recebam esta decisão como prova de que o reino permanece estável, abastecido e " +
                                "seguro sob sua própria solidez.<BR><BR>" +
                                "A cobrança de impostos deixa de ser aplicada a partir de agora.<BR><BR>" +
                                "Assinado, " + signature + ".";
                        }

                        return
                            "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                            "Por determinação do governo de " + cityName + ", a cobrança semanal foi reduzida para <B>" + amount + " moedas</B> " +
                            "por cidadão.<BR><BR>" +
                            "A disciplina do reino produziu resultado. Os cofres permanecem firmes, a estrutura continua sólida e a " +
                            "autoridade entende que, em tempos de estabilidade, o peso da cobrança pode ser aliviado sem comprometer " +
                            "a força do governo.<BR><BR>" +
                            "Recebam esta redução como prova de que o reino segue abastecido, organizado e suficientemente seguro para " +
                            "diminuir a exigência feita ao seu povo. Entre os sarangs, a solidez também se demonstra pela capacidade " +
                            "de cobrar menos quando a ordem já foi garantida.<BR><BR>" +
                            "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                            "Assinado, " + signature + ".";

                    case "zosteros":
                        if (amount <= 0)
                        {
                            return
                                "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                                "Cidadãos de " + cityName + ", a contribuição semanal do reino foi removida.<BR><BR>" +
                                "Os últimos resultados mostram que a cidade atravessa um tempo de prosperidade e boa sustentação " +
                                "coletiva. Entre os zosteros, quando a comunidade se fortalece e o governo se mantém estável, " +
                                "é justo que essa prosperidade retorne ao povo na forma de menor cobrança e maior alívio no " +
                                "cotidiano.<BR><BR>" +
                                "Que esta medida seja entendida como fruto de um reino bem conduzido, capaz de crescer sem " +
                                "esquecer aqueles que o mantêm vivo todos os dias.<BR><BR>" +
                                "A cobrança de impostos deixa de ser aplicada a partir de agora.<BR><BR>" +
                                "Assinado, " + signature + ".";
                        }

                        return
                            "<BIG><B>Redução de impostos</B></BIG><BR><BR>" +
                            "Cidadãos de " + cityName + ", a contribuição semanal do reino foi reduzida para <B>" + amount + " moedas</B> " +
                            "por cidadão.<BR><BR>" +
                            "Os últimos resultados mostram que a cidade atravessa um momento de estabilidade e boa sustentação coletiva. " +
                            "Entre os zosteros, quando o reino cresce com equilíbrio e seus compromissos permanecem assegurados, é justo " +
                            "que essa prosperidade retorne ao povo também na forma de uma cobrança mais leve.<BR><BR>" +
                            "Que esta medida seja entendida como sinal de confiança, de organização e de reconhecimento pelo papel que " +
                            "cada cidadão tem na continuidade do reino. Quando todos ajudam a construir a cidade, todos também devem " +
                            "sentir os benefícios do seu bom momento.<BR><BR>" +
                            "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                            "Assinado, " + signature + "."; 
                }
            }

            switch (ReinoEmploymentSystem.GetGovernmentCultureId(cityId))
            {
                case "kamay":
                    return
                        "<BIG><B>Nova cobrança de impostos</B></BIG><BR><BR>" +
                        "Cidadãos de " + cityName + ", o governo passa a cobrar <B>" + amount + " moedas</B> por semana de " +
                        "cada morador do reino.<BR><BR>" +
                        "Entre os kamay, a cidade não se sustenta só pela palavra dos ministros, mas pelo esforço constante " +
                        "de quem vive, trabalha e prospera sob a proteção do reino. Essa contribuição ajuda a manter oficinas, " +
                        "armazéns, serviços públicos e toda a estrutura que permite ao governo continuar funcionando com ordem " +
                        "e estabilidade.<BR><BR>" +
                        "Nenhuma cidade cresce sem compromisso. Quem carrega o nome do reino também ajuda a sustentar aquilo" +
                        " que protege sua casa, seu comércio e seu futuro.<BR><BR>" +
                        "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                        "Assinado, " + signature + ".";

                case "matalun":
                    return
                        "<BIG><B>Nova cobrança de impostos</B></BIG><BR><BR>" +
                        "Povo de " + cityName + ", foi instituída uma contribuição semanal de <B>" + amount + " moedas</B> " +
                        "para cada cidadão do reino.<BR><BR>" +
                        "Entre os matalun, governar não é apenas administrar pedras e moedas, mas preservar a ordem espiritual " +
                        "e material da cidade. Esse valor ajudará a sustentar os deveres do reino, a manutenção dos espaços " +
                        "sagrados, o cuidado com a comunidade e o equilíbrio necessário para que a cidade continue de pé.<BR><BR>" +
                        "Contribuir também é participar da continuidade do reino. A cidadania não se mede apenas por " +
                        "pertencer, mas por ajudar a manter viva a estrutura que acolhe e protege o seu povo.<BR><BR>" +
                        "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                        "Assinado, " + signature + ".";

                case "sarangs":
                    return
                        "<BIG><B>Nova cobrança de impostos</B></BIG><BR><BR>" +
                        "Por determinação do governo de " + cityName + ", passa a vigorar uma cobrança semanal " +
                        "de <B>" + amount + " moedas</B> para cada cidadão do reino.<BR><BR>" +
                        "Entre os sarangs, força e permanência dependem de disciplina. Um reino só permanece sólido " +
                        "quando cada um assume sua parte no peso da muralha, do soldo, do abastecimento e da máquina que " +
                        "mantém a autoridade funcionando sem fraquejar.<BR><BR>" +
                        "Essa contribuição existe para garantir que o reino continue firme, organizado e capaz de sustentar " +
                        "suas decisões com recursos reais, não apenas com promessas. Viver sob a bandeira do reino também " +
                        "significa sustentá-la.<BR><BR>" +
                        "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                        "Assinado, " + signature + ".";

                case "zosteros":
                    return
                        "<BIG><B>Nova cobrança de impostos</B></BIG><BR><BR>" +
                        "Cidadãos de " + cityName + ", foi aprovada uma nova contribuição semanal de <B>" + amount + " moedas</B> " +
                        "para todos os membros do reino.<BR><BR>" +
                        "Entre os zosteros, a cidade se mantém porque muitos aceitam sustentar em conjunto aquilo que beneficia " +
                        "a coletividade. Essa cobrança ajudará a preservar os serviços do reino, a organização pública, os " +
                        "compromissos do governo e a continuidade das estruturas que servem a todos.<BR><BR>" +
                        "Ser cidadão não é apenas possuir direitos dentro do reino, mas também assumir uma parte do seu custo. " +
                        "Quando cada um contribui, a cidade permanece estável, presente e capaz de responder às necessidades " +
                        "do seu povo.<BR><BR>" +
                        "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                        "Assinado, " + signature + ".";
            }

            return
                "<BIG><B>Nova cobrança de impostos</B></BIG><BR><BR>" +
                "Passará a ser cobrada uma contribuição semanal de <B>" + amount + " moedas</B> de cada cidadão para ajudar a " +
                "manter o reino em funcionamento.<BR><BR>" +
                "A nova cobrança será feita automaticamente por nosso sistema bancário.<BR><BR>" +
                "Assinado, " + signature + ".";
        }

        private static void ShowCitizenTaxNoticeIfNeeded(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            int cityId;
            if (!ReinoExpansionSystem.TryParseCityId(pm.OSUCitizenCityId, out cityId))
                return;

            ReinoTreasuryCityState state = GetState(cityId);
            if (state == null || state.CitizenTaxNoticeVersion <= 0)
                return;

            int seen = 0;
            state.SeenCitizenTaxNoticeByPlayer.TryGetValue(pm.Serial.Value, out seen);

            if (seen >= state.CitizenTaxNoticeVersion)
                return;

            state.SeenCitizenTaxNoticeByPlayer[pm.Serial.Value] = state.CitizenTaxNoticeVersion;
            pm.SendGump(new ReinoTaxNoticeGump(pm, cityId, state.WeeklyCitizenTax, state.CitizenTaxNoticeType));
        }

        private static bool GovernmentNeedsApprovals(int cityId)
        {
            return !String.Equals(ReinoEmploymentSystem.GetGovernmentCultureId(cityId), "sarangs", StringComparison.OrdinalIgnoreCase);
        }

        private static List<ReinoCargoEntry> GetTreasuryApprovers(int cityId)
        {
            List<ReinoCargoEntry> result = new List<ReinoCargoEntry>();
            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);
            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(cityId);

            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied)
                    continue;

                if (String.Equals(culture, "kamay", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Kind == ReinoCargoKind.MinisterEconomy)
                        result.Add(role);
                }
                else if (String.Equals(culture, "matalun", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Kind == ReinoCargoKind.Priest)
                        result.Add(role);
                }
                else if (String.Equals(culture, "zosteros", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Kind == ReinoCargoKind.CouncilMember)
                        result.Add(role);
                }
            }

            return result;
        }

        private static int CountConfigChanges(ReinoTreasuryCityState state, int weeklyCitizenTax, int vendorSalesTaxPercent, int salaryTaxPercent, int auctionTaxPercent, int religiousDonationTaxPercent)
        {
            int count = 0;

            if (state.WeeklyCitizenTax != weeklyCitizenTax)
                count++;
            if (state.VendorSalesTaxPercent != vendorSalesTaxPercent)
                count++;
            if (state.SalaryTaxPercent != salaryTaxPercent)
                count++;
            if (state.AuctionTaxPercent != auctionTaxPercent)
                count++;
            if (state.ReligiousDonationTaxPercent != religiousDonationTaxPercent)
                count++;

            return count;
        }

        private static string BuildConfigChangeHtml(int cityId, int oldCitizenTax, int oldVendorTax, int oldSalaryTax, int oldAuctionTax, int oldReligiousTax, int newCitizenTax, int newVendorTax, int newSalaryTax, int newAuctionTax, int newReligiousTax)
        {
            string cityName = ReinoElectionsSystem.GetCityName(cityId);
            int changes = 0;
            string title = "Alteração no Tesouro";
            string body = String.Empty;

            if (oldCitizenTax != newCitizenTax)
            {
                changes++;
                title = "Alteração de Impostos Semanais";
                body += "O governante deseja alterar o imposto semanal cobrado dos cidadãos do reino.<BR><BR>" +
                    "<B>Reino:</B> " + cityName + "<BR>" +
                    "<B>Valor atual:</B> " + oldCitizenTax + " moedas<BR>" +
                    "<B>Novo valor:</B> " + newCitizenTax + " moedas";
            }

            if (oldVendorTax != newVendorTax)
            {
                changes++;
                title = "Alteração de Imposto sobre Vendas de NPCs";
                body += (body.Length > 0 ? "<BR><BR>" : String.Empty) +
                    "O governante deseja alterar a percentagem cobrada sobre vendas feitas em vendedores de " +
                    "jogadores vinculados ao reino.<BR><BR>" +
                    "<B>Reino:</B> " + cityName + "<BR>" +
                    "<B>Valor atual:</B> " + oldVendorTax + "%<BR>" +
                    "<B>Novo valor:</B> " + newVendorTax + "%";
            }

            if (oldSalaryTax != newSalaryTax)
            {
                changes++;
                title = "Alteração de Imposto sobre Salários";
                body += (body.Length > 0 ? "<BR><BR>" : String.Empty) +
                    "O governante deseja alterar a percentagem retida no pagamento dos cargos comissionados do reino.<BR><BR>" +
                    "<B>Reino:</B> " + cityName + "<BR>" +
                    "<B>Valor atual:</B> " + oldSalaryTax + "%<BR>" +
                    "<B>Novo valor:</B> " + newSalaryTax + "%";
            }

            if (oldAuctionTax != newAuctionTax)
            {
                changes++;
                title = "Alteração de Imposto sobre Leilões";
                body += (body.Length > 0 ? "<BR><BR>" : String.Empty) +
                    "O governante deseja alterar a percentagem recolhida sobre vendas realizadas na casa de " +
                    "leilões do reino.<BR><BR>" +
                    "<B>Reino:</B> " + cityName + "<BR>" +
                    "<B>Valor atual:</B> " + oldAuctionTax + "%<BR>" +
                    "<B>Novo valor:</B> " + newAuctionTax + "%";
            }

            if (oldReligiousTax != newReligiousTax)
            {
                changes++;
                title = "Alteração de Imposto sobre Doações Religiosas";
                body += (body.Length > 0 ? "<BR><BR>" : String.Empty) +
                    "O governante deseja alterar a percentagem recolhida em moedas das doações feitas aos " +
                    "templos do reino.<BR><BR>" +
                    "<B>Reino:</B> " + cityName + "<BR>" +
                    "<B>Valor atual:</B> " + oldReligiousTax + "%<BR>" +
                    "<B>Novo valor:</B> " + newReligiousTax + "%";
            }

            if (changes <= 0)
            {
                title = "Alteração no Tesouro";
                body = "Nenhuma alteração válida foi encontrada para aprovação.";
            }
            else if (changes > 1)
            {
                title = "Alterações no Tesouro";
            }

            return "<BASEFONT COLOR=#000000><BIG><B>" + title + "</B></BIG><BR><BR>" + body + "</BASEFONT>";
        }

        public static bool UpdateConfiguration(PlayerMobile actor, int cityId, int weeklyCitizenTax, int vendorSalesTaxPercent, int salaryTaxPercent, int auctionTaxPercent, int religiousDonationTaxPercent, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted || !ReinoAccessHelper.HasGovernmentAccess(actor, cityId))
            {
                message = "Somente o governador ou alguém com a chave do governador pode alterar o tesouro.";
                return false;
            }

            weeklyCitizenTax = Math.Max(0, weeklyCitizenTax);
            vendorSalesTaxPercent = Math.Max(0, Math.Min(50, vendorSalesTaxPercent));
            salaryTaxPercent = Math.Max(0, Math.Min(50, salaryTaxPercent));
            auctionTaxPercent = Math.Max(0, Math.Min(50, auctionTaxPercent));
            religiousDonationTaxPercent = Math.Max(0, Math.Min(50, religiousDonationTaxPercent));

            if (!HasAuctionHouse(cityId))
                auctionTaxPercent = 0;

            ReinoTreasuryCityState state = GetState(cityId);
            int changedCount = CountConfigChanges(state, weeklyCitizenTax, vendorSalesTaxPercent, salaryTaxPercent, auctionTaxPercent, religiousDonationTaxPercent);

            if (changedCount <= 0)
            {
                message = "Nenhuma alteração foi feita.";
                return false;
            }

            if (!GovernmentNeedsApprovals(cityId))
            {
                ApplyConfiguration(cityId, weeklyCitizenTax, vendorSalesTaxPercent, salaryTaxPercent, auctionTaxPercent, religiousDonationTaxPercent);
                message = "Configuração do tesouro atualizada.";
                return true;
            }

            if (GetPendingApprovalForCity(cityId) != null)
            {
                message = "Já existe uma mudança do tesouro aguardando aprovação.";
                return false;
            }

            ReinoTreasuryPendingApproval approval = new ReinoTreasuryPendingApproval();
            approval.ApprovalId = m_NextApprovalId++;
            approval.CityId = cityId;
            approval.CreatedBySerial = actor.Serial.Value;
            approval.CreatedByName = actor.Name;
            approval.CreatedUtc = DateTime.UtcNow;
            approval.Status = 0;

            approval.OldWeeklyCitizenTax = state.WeeklyCitizenTax;
            approval.OldVendorSalesTaxPercent = state.VendorSalesTaxPercent;
            approval.OldSalaryTaxPercent = state.SalaryTaxPercent;
            approval.OldAuctionTaxPercent = state.AuctionTaxPercent;
            approval.OldReligiousDonationTaxPercent = state.ReligiousDonationTaxPercent;

            approval.NewWeeklyCitizenTax = weeklyCitizenTax;
            approval.NewVendorSalesTaxPercent = vendorSalesTaxPercent;
            approval.NewSalaryTaxPercent = salaryTaxPercent;
            approval.NewAuctionTaxPercent = auctionTaxPercent;
            approval.NewReligiousDonationTaxPercent = religiousDonationTaxPercent;

            approval.Html = BuildConfigChangeHtml(cityId,
                approval.OldWeeklyCitizenTax,
                approval.OldVendorSalesTaxPercent,
                approval.OldSalaryTaxPercent,
                approval.OldAuctionTaxPercent,
                approval.OldReligiousDonationTaxPercent,
                approval.NewWeeklyCitizenTax,
                approval.NewVendorSalesTaxPercent,
                approval.NewSalaryTaxPercent,
                approval.NewAuctionTaxPercent,
                approval.NewReligiousDonationTaxPercent);

            List<ReinoCargoEntry> approvers = GetTreasuryApprovers(cityId);
            for (int i = 0; i < approvers.Count; i++)
            {
                ReinoCargoEntry role = approvers[i];
                if (role == null || !role.IsOccupied)
                    continue;

                ReinoTreasuryApprovalVote vote = new ReinoTreasuryApprovalVote();
                vote.VoterSerial = role.OccupantSerial;
                vote.VoterName = role.OccupantName ?? String.Empty;
                approval.Votes.Add(vote);
            }

            m_PendingApprovals.Add(approval);
            NotifyApprovalVoters(approval);
            message = "Mudança do tesouro enviada para aprovação do governo.";
            return true;
        }

        private static ReinoTreasuryPendingApproval GetPendingApprovalForCity(int cityId)
        {
            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoTreasuryPendingApproval approval = m_PendingApprovals[i];
                if (approval != null && approval.CityId == cityId && approval.IsPending)
                    return approval;
            }

            return null;
        }

        private static void NotifyApprovalVoters(ReinoTreasuryPendingApproval approval)
        {
            if (approval == null)
                return;

            for (int i = 0; i < approval.Votes.Count; i++)
            {
                PlayerMobile pm = World.FindMobile((Serial)approval.Votes[i].VoterSerial) as PlayerMobile;
                if (pm != null && !pm.Deleted && pm.NetState != null)
                    ShowPendingApprovalGump(pm);
            }
        }

        public static ReinoTreasuryPendingApproval GetPendingApproval(int approvalId)
        {
            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoTreasuryPendingApproval approval = m_PendingApprovals[i];
                if (approval != null && approval.ApprovalId == approvalId)
                    return approval;
            }

            return null;
        }

        public static void ShowPendingApprovalGump(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.NetState == null)
                return;

            for (int i = 0; i < m_PendingApprovals.Count; i++)
            {
                ReinoTreasuryPendingApproval approval = m_PendingApprovals[i];
                if (approval == null || !approval.IsPending)
                    continue;

                for (int v = 0; v < approval.Votes.Count; v++)
                {
                    if (approval.Votes[v].VoterSerial == pm.Serial.Value && approval.Votes[v].Decision == 0)
                    {
                        pm.CloseGump(typeof(ReinoTreasuryApprovalGump));
                        pm.SendGump(new ReinoTreasuryApprovalGump(pm, approval.ApprovalId));
                        return;
                    }
                }
            }
        }

        public static bool VotePendingApproval(PlayerMobile pm, int approvalId, bool approve, out string message)
        {
            message = String.Empty;

            ReinoTreasuryPendingApproval approval = GetPendingApproval(approvalId);
            if (approval == null || !approval.IsPending)
            {
                message = "Essa mudança já foi resolvida.";
                return false;
            }

            for (int i = 0; i < approval.Votes.Count; i++)
            {
                ReinoTreasuryApprovalVote vote = approval.Votes[i];
                if (vote != null && vote.VoterSerial == pm.Serial.Value)
                {
                    vote.Decision = approve ? 1 : 2;
                    vote.DecisionUtc = DateTime.UtcNow;
                    EvaluateApproval(approval, true);
                    message = approve ? "Você aprovou a mudança do tesouro." : "Você vetou a mudança do tesouro.";
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
                ReinoTreasuryPendingApproval approval = m_PendingApprovals[i];
                if (approval != null && approval.IsPending)
                    EvaluateApproval(approval, false);
            }
        }

        private static void EvaluateApproval(ReinoTreasuryPendingApproval approval, bool interactive)
        {
            if (approval == null || !approval.IsPending)
                return;

            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(approval.CityId);
            DateTime now = DateTime.UtcNow;
            bool anyYes = false;
            bool allNo = approval.Votes.Count > 0;
            bool allAnswered = true;

            for (int i = 0; i < approval.Votes.Count; i++)
            {
                ReinoTreasuryApprovalVote vote = approval.Votes[i];
                if (vote == null)
                    continue;

                if (vote.Decision == 0 && (now - approval.CreatedUtc) >= TimeSpan.FromHours(48.0))
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
                    FinalizeApproval(approval, true);
                else if (allAnswered && allNo)
                    FinalizeApproval(approval, false);
            }
            else
            {
                if (anyYes && allAnswered)
                    FinalizeApproval(approval, true);
                else if (allAnswered && !anyYes)
                    FinalizeApproval(approval, false);
            }

            if (interactive && approval.IsPending)
            {
                for (int i = 0; i < approval.Votes.Count; i++)
                {
                    PlayerMobile voter = World.FindMobile((Serial)approval.Votes[i].VoterSerial) as PlayerMobile;
                    if (voter != null && !voter.Deleted && voter.NetState != null)
                        ShowPendingApprovalGump(voter);
                }
            }
        }

        private static void FinalizeApproval(ReinoTreasuryPendingApproval approval, bool approved)
        {
            if (approval == null || !approval.IsPending)
                return;

            approval.Status = approved ? 1 : 2;
            approval.ResolvedUtc = DateTime.UtcNow;

            if (approved)
            {
                ApplyConfiguration(approval.CityId,
                    approval.NewWeeklyCitizenTax,
                    approval.NewVendorSalesTaxPercent,
                    approval.NewSalaryTaxPercent,
                    approval.NewAuctionTaxPercent,
                    approval.NewReligiousDonationTaxPercent);
            }
        }

        private static void ApplyConfiguration(int cityId, int weeklyCitizenTax, int vendorSalesTaxPercent, int salaryTaxPercent, int auctionTaxPercent, int religiousDonationTaxPercent)
        {
            ReinoTreasuryCityState state = GetState(cityId);
            int previousCitizenTax = state.WeeklyCitizenTax;

            state.WeeklyCitizenTax = Math.Max(0, weeklyCitizenTax);
            state.VendorSalesTaxPercent = Math.Max(0, Math.Min(50, vendorSalesTaxPercent));
            state.SalaryTaxPercent = Math.Max(0, Math.Min(50, salaryTaxPercent));
            state.AuctionTaxPercent = Math.Max(0, Math.Min(50, auctionTaxPercent));
            state.ReligiousDonationTaxPercent = Math.Max(0, Math.Min(50, religiousDonationTaxPercent));

            if (state.WeeklyCitizenTax > previousCitizenTax)
            {
                state.CitizenTaxNoticeType = 1;
                state.CitizenTaxNoticeVersion++;
                BroadcastCitizenTaxNotice(cityId);
            }
            else if (state.WeeklyCitizenTax < previousCitizenTax)
            {
                state.CitizenTaxNoticeType = 2;
                state.CitizenTaxNoticeVersion++;
                BroadcastCitizenTaxNotice(cityId);
            }
        }

        private static void BroadcastCitizenTaxNotice(int cityId)
        {
            string cityName = PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(cityId));
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                if (!String.Equals(PlayerMobile.NormalizeOSUCityId(pm.OSUCitizenCityId), cityName, StringComparison.OrdinalIgnoreCase))
                    continue;

                ShowCitizenTaxNoticeIfNeeded(pm);
            }
        }

        public static bool CanWithdrawReligiousDonations(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted || cityId < 0)
                return false;

            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(cityId);
            for (int i = 0; i < roles.Count; i++)
            {
                ReinoCargoEntry role = roles[i];
                if (role == null || !role.IsOccupied || role.OccupantSerial != pm.Serial.Value)
                    continue;

                if (role.Kind == ReinoCargoKind.Priest)
                    return true;

                string title = role.Title ?? String.Empty;
                if (title.IndexOf("relig", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    title.IndexOf("sacerd", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        public static bool ResignLeadership(PlayerMobile actor, int cityId, out string message)
        {
            message = String.Empty;

            if (actor == null || actor.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            ReinoCityData city;
            if (!ReinoElectionsSystem._cities.TryGetValue(cityId, out city) || city == null)
            {
                message = "Reino inválido.";
                return false;
            }

            if (city.GovernorSerial != actor.Serial.Value)
            {
                message = "Somente o líder atual pode resignar a liderança do reino.";
                return false;
            }

            ReinoElectionsSystem.SetGovernor(cityId, null);
            ReinoEmploymentSystem.SyncRoleDependentState(cityId);
            message = "Você resignou a liderança de " + ReinoElectionsSystem.GetCityName(cityId) + ". O reino ficará sem líder até a próxima eleição.";
            return true;
        }

        private static void WriteBundle(BinaryWriter bw, ReinoTreasuryResourceBundle bundle)
        {
            bw.Write(bundle != null ? bundle.Gold : 0);
            bw.Write(bundle != null ? bundle.Cloth : 0);
            bw.Write(bundle != null ? bundle.Iron : 0);
            bw.Write(bundle != null ? bundle.Wood : 0);
        }

        private static ReinoTreasuryResourceBundle ReadBundle(BinaryReader br)
        {
            ReinoTreasuryResourceBundle bundle = new ReinoTreasuryResourceBundle();
            bundle.Gold = br.ReadInt32();
            bundle.Cloth = br.ReadInt32();
            bundle.Iron = br.ReadInt32();
            bundle.Wood = br.ReadInt32();
            return bundle;
        }

        private static void WriteWeek(BinaryWriter bw, ReinoTreasuryWeekRecord week)
        {
            WriteBundle(bw, week.CitizenTaxes);
            WriteBundle(bw, week.ConstructionIncome);
            WriteBundle(bw, week.PostoIncome);
            WriteBundle(bw, week.DonationIncome);
            WriteBundle(bw, week.AuctionIncome);
            WriteBundle(bw, week.DiplomacyIncome);
            WriteBundle(bw, week.VendorIncome);
            WriteBundle(bw, week.RepresentativeIncome);

            WriteBundle(bw, week.MaintenanceExpense);
            WriteBundle(bw, week.SalaryExpense);
            WriteBundle(bw, week.GuardExpense);
            WriteBundle(bw, week.DiplomacyExpense);
            WriteBundle(bw, week.RepresentativeExpense);
        }

        private static ReinoTreasuryWeekRecord ReadWeek(BinaryReader br)
        {
            ReinoTreasuryWeekRecord week = new ReinoTreasuryWeekRecord();
            week.CitizenTaxes = ReadBundle(br);
            week.ConstructionIncome = ReadBundle(br);
            week.PostoIncome = ReadBundle(br);
            week.DonationIncome = ReadBundle(br);
            week.AuctionIncome = ReadBundle(br);
            week.DiplomacyIncome = ReadBundle(br);
            week.VendorIncome = ReadBundle(br);
            week.RepresentativeIncome = ReadBundle(br);

            week.MaintenanceExpense = ReadBundle(br);
            week.SalaryExpense = ReadBundle(br);
            week.GuardExpense = ReadBundle(br);
            week.DiplomacyExpense = ReadBundle(br);
            week.RepresentativeExpense = ReadBundle(br);
            return week;
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
                    bw.Write(m_NextApprovalId);

                    bw.Write(m_Cities.Count);
                    foreach (KeyValuePair<int, ReinoTreasuryCityState> kv in m_Cities)
                    {
                        ReinoTreasuryCityState state = kv.Value;
                        bw.Write(kv.Key);
                        bw.Write(state.WeeklyCitizenTax);
                        bw.Write(state.VendorSalesTaxPercent);
                        bw.Write(state.SalaryTaxPercent);
                        bw.Write(state.AuctionTaxPercent);
                        bw.Write(state.ReligiousDonationTaxPercent);
                        bw.Write(state.CitizenTaxNoticeVersion);
                        bw.Write(state.CitizenTaxNoticeType);
                        bw.Write(state.CurrentWeekStartUtc.ToBinary());
                        bw.Write(state.LastCitizenTaxChargeUtc.ToBinary());
                        bw.Write(state.LastSnapshotUtc.ToBinary());
                        WriteBundle(bw, state.LastWeekSnapshot);
                        WriteBundle(bw, state.PostoWeekStartSnapshot);
                        WriteWeek(bw, state.CurrentWeek);
                        WriteWeek(bw, state.LastClosedWeek);
                        WriteBundle(bw, state.TotalDonationHistory);

                        bw.Write(state.SeenCitizenTaxNoticeByPlayer.Count);
                        foreach (KeyValuePair<int, int> notice in state.SeenCitizenTaxNoticeByPlayer)
                        {
                            bw.Write(notice.Key);
                            bw.Write(notice.Value);
                        }
                    }

                    bw.Write(m_PendingApprovals.Count);
                    for (int i = 0; i < m_PendingApprovals.Count; i++)
                    {
                        ReinoTreasuryPendingApproval approval = m_PendingApprovals[i];
                        bw.Write(approval.ApprovalId);
                        bw.Write(approval.CityId);
                        bw.Write(approval.CreatedBySerial);
                        bw.Write(approval.CreatedByName ?? String.Empty);
                        bw.Write(approval.CreatedUtc.ToBinary());
                        bw.Write(approval.ResolvedUtc.ToBinary());
                        bw.Write(approval.Status);
                        bw.Write(approval.Html ?? String.Empty);

                        bw.Write(approval.OldWeeklyCitizenTax);
                        bw.Write(approval.OldVendorSalesTaxPercent);
                        bw.Write(approval.OldSalaryTaxPercent);
                        bw.Write(approval.OldAuctionTaxPercent);
                        bw.Write(approval.OldReligiousDonationTaxPercent);

                        bw.Write(approval.NewWeeklyCitizenTax);
                        bw.Write(approval.NewVendorSalesTaxPercent);
                        bw.Write(approval.NewSalaryTaxPercent);
                        bw.Write(approval.NewAuctionTaxPercent);
                        bw.Write(approval.NewReligiousDonationTaxPercent);

                        bw.Write(approval.Votes.Count);
                        for (int v = 0; v < approval.Votes.Count; v++)
                        {
                            ReinoTreasuryApprovalVote vote = approval.Votes[v];
                            bw.Write(vote.VoterSerial);
                            bw.Write(vote.VoterName ?? String.Empty);
                            bw.Write(vote.Decision);
                            bw.Write(vote.DecisionUtc.ToBinary());
                        }
                    }
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
                    m_NextApprovalId = br.ReadInt32();

                    m_Cities.Clear();
                    int cityCount = br.ReadInt32();
                    for (int i = 0; i < cityCount; i++)
                    {
                        int cityId = br.ReadInt32();
                        ReinoTreasuryCityState state = new ReinoTreasuryCityState(cityId);
                        state.WeeklyCitizenTax = br.ReadInt32();
                        state.VendorSalesTaxPercent = br.ReadInt32();
                        state.SalaryTaxPercent = br.ReadInt32();
                        state.AuctionTaxPercent = br.ReadInt32();
                        state.ReligiousDonationTaxPercent = br.ReadInt32();
                        state.CitizenTaxNoticeVersion = br.ReadInt32();

                        if (version >= 2)
                            state.CitizenTaxNoticeType = br.ReadInt32();
                        else
                            state.CitizenTaxNoticeType = 0;

                        state.CurrentWeekStartUtc = DateTime.FromBinary(br.ReadInt64());
                        state.LastCitizenTaxChargeUtc = DateTime.FromBinary(br.ReadInt64());
                        state.LastSnapshotUtc = DateTime.FromBinary(br.ReadInt64());
                        state.LastWeekSnapshot = ReadBundle(br);
                        state.PostoWeekStartSnapshot = ReadBundle(br);
                        state.CurrentWeek = ReadWeek(br);
                        state.LastClosedWeek = ReadWeek(br);
                        state.TotalDonationHistory = ReadBundle(br);

                        int seenCount = br.ReadInt32();
                        for (int s = 0; s < seenCount; s++)
                        {
                            int serial = br.ReadInt32();
                            int seenVersion = br.ReadInt32();
                            state.SeenCitizenTaxNoticeByPlayer[serial] = seenVersion;
                        }

                        m_Cities[cityId] = state;
                    }

                    m_PendingApprovals.Clear();
                    int approvalCount = br.ReadInt32();
                    for (int i = 0; i < approvalCount; i++)
                    {
                        ReinoTreasuryPendingApproval approval = new ReinoTreasuryPendingApproval();
                        approval.ApprovalId = br.ReadInt32();
                        approval.CityId = br.ReadInt32();
                        approval.CreatedBySerial = br.ReadInt32();
                        approval.CreatedByName = br.ReadString();
                        approval.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
                        approval.ResolvedUtc = DateTime.FromBinary(br.ReadInt64());
                        approval.Status = br.ReadInt32();
                        approval.Html = br.ReadString();

                        approval.OldWeeklyCitizenTax = br.ReadInt32();
                        approval.OldVendorSalesTaxPercent = br.ReadInt32();
                        approval.OldSalaryTaxPercent = br.ReadInt32();
                        approval.OldAuctionTaxPercent = br.ReadInt32();
                        approval.OldReligiousDonationTaxPercent = br.ReadInt32();

                        approval.NewWeeklyCitizenTax = br.ReadInt32();
                        approval.NewVendorSalesTaxPercent = br.ReadInt32();
                        approval.NewSalaryTaxPercent = br.ReadInt32();
                        approval.NewAuctionTaxPercent = br.ReadInt32();
                        approval.NewReligiousDonationTaxPercent = br.ReadInt32();

                        int voteCount = br.ReadInt32();
                        for (int v = 0; v < voteCount; v++)
                        {
                            ReinoTreasuryApprovalVote vote = new ReinoTreasuryApprovalVote();
                            vote.VoterSerial = br.ReadInt32();
                            vote.VoterName = br.ReadString();
                            vote.Decision = br.ReadInt32();
                            vote.DecisionUtc = DateTime.FromBinary(br.ReadInt64());
                            approval.Votes.Add(vote);
                        }

                        m_PendingApprovals.Add(approval);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
