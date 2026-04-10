using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;
using Server.Commands;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Custom.Systems.Rent;
using Server.Network;
using Server.SkillHandlers;
using Server.Targeting;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Custom;
using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Reinos
{
    public sealed class ReinoArchivedReport
    {
        public DateTime ClosedUtc;
        public string ClosedBy;
        public string SummaryHtml;
        public List<string> CrimeDetails;
        public List<string> PrisonDetails;
        public List<string> WantedDetails;
        public List<string> RecurringDetails;

        public ReinoArchivedReport()
        {
            ClosedBy = String.Empty;
            SummaryHtml = String.Empty;
            CrimeDetails = new List<string>();
            PrisonDetails = new List<string>();
            WantedDetails = new List<string>();
            RecurringDetails = new List<string>();
        }
    }

    public static class ReinoMilitarySystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoMilitary_v1.bin");

        private static readonly Dictionary<int, ReinoMilitaryPolicy> m_Policies = new Dictionary<int, ReinoMilitaryPolicy>();
        private static readonly Dictionary<int, List<ReinoWantedEntry>> m_Wanted = new Dictionary<int, List<ReinoWantedEntry>>();
        private static readonly Dictionary<int, List<ReinoCrimeRecord>> m_Crimes = new Dictionary<int, List<ReinoCrimeRecord>>();
        private static readonly Dictionary<int, List<ReinoPrisonRecord>> m_Prisons = new Dictionary<int, List<ReinoPrisonRecord>>();
        private static readonly Dictionary<int, ReinoMilitaryReportState> m_ReportStates = new Dictionary<int, ReinoMilitaryReportState>();
        private static readonly Dictionary<int, List<ReinoArchivedReport>> m_ArchivedReportsByCity = new Dictionary<int, List<ReinoArchivedReport>>();
        private static readonly Dictionary<int, List<ReinoGuardPostInfo>> m_PostsByCity = new Dictionary<int, List<ReinoGuardPostInfo>>();
        private static readonly Dictionary<int, ReinoMilitarySession> m_Sessions = new Dictionary<int, ReinoMilitarySession>();
        private static readonly HashSet<int> m_AutoSheathe = new HashSet<int>();
        private static readonly Dictionary<int, DateTime> m_LastPassiveCrimeNotice = new Dictionary<int, DateTime>();
        private static readonly Dictionary<int, List<string>> m_PendingLawNoticesByPlayer = new Dictionary<int, List<string>>();
        private static readonly HashSet<int> m_QueuedOnlineLawNotice = new HashSet<int>();
        private static readonly Dictionary<int, DateTime> m_LastWantedNotice = new Dictionary<int, DateTime>();

        private static int m_NextPostId = 1;
        private static Timer m_PulseTimer;

        private enum ReinoAreaScope
        {
            Total = 0,
            Public = 1,
            LotOnly = 2
        }

        public static readonly int[] HoodItemIds = new int[]
        {
            52000, 52001, 52002, 52003, 52004, 52005, 52006,
            52007, 52008, 52009, 52010, 52011, 52012, 52013,
            52014, 52015, 52016, 52017, 52018, 52019, 52020,
            52021, 52022, 52023, 52024, 52025, 52026, 52027,
            52028
        };

        public static readonly ReinoPrisonCellDefinition[] PrisonCells = new ReinoPrisonCellDefinition[]
        {
            new ReinoPrisonCellDefinition(5, 6, 0),
            new ReinoPrisonCellDefinition(7, 6, 0),
            new ReinoPrisonCellDefinition(9, 6, 0),
            new ReinoPrisonCellDefinition(11, 6, 0),
            new ReinoPrisonCellDefinition(13, 6, 0)
        };

        public static void Initialize()
        {
            Load();
            EnsureDefaults();

            EventSink.WorldSave += delegate { Save(); };
            EventSink.AggressiveAction += OnAggressiveAction;
            EventSink.CreatureDeath += OnCreatureDeath;
            EventSink.Speech += OnSpeech;
            EventSink.Login += OnLogin;
            Stealing.ItemStolen += OnItemStolen;

            if (m_PulseTimer != null)
                m_PulseTimer.Stop();

            m_PulseTimer = Timer.DelayCall(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0), Pulse);
        }

        private static void EnsureDefaults()
        {
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;
            for (int i = 0; i < count; i++)
            {
                GetPolicy(i);
                GetWantedList(i);
                GetCrimeList(i);
                GetPrisonList(i);
                GetReportState(i);
                GetArchivedReports(i);
                GetPosts(i);
            }
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e != null ? e.Mobile as PlayerMobile : null;
            if (pm == null || pm.Deleted)
                return;

            ShowPendingLawNotice(pm);
        }

        public static ReinoMilitaryPolicy GetPolicy(int cityId)
        {
            ReinoMilitaryPolicy p;
            if (!m_Policies.TryGetValue(cityId, out p))
            {
                p = new ReinoMilitaryPolicy();
                m_Policies[cityId] = p;
            }

            return p;
        }

        public static List<ReinoWantedEntry> GetWantedList(int cityId)
        {
            List<ReinoWantedEntry> list;
            if (!m_Wanted.TryGetValue(cityId, out list))
            {
                list = new List<ReinoWantedEntry>();
                m_Wanted[cityId] = list;
            }

            return list;
        }

        public static List<ReinoCrimeRecord> GetCrimeList(int cityId)
        {
            List<ReinoCrimeRecord> list;
            if (!m_Crimes.TryGetValue(cityId, out list))
            {
                list = new List<ReinoCrimeRecord>();
                m_Crimes[cityId] = list;
            }

            return list;
        }

        public static List<ReinoPrisonRecord> GetPrisonList(int cityId)
        {
            List<ReinoPrisonRecord> list;
            if (!m_Prisons.TryGetValue(cityId, out list))
            {
                list = new List<ReinoPrisonRecord>();
                m_Prisons[cityId] = list;
            }

            return list;
        }

        public static ReinoMilitaryReportState GetReportState(int cityId)
        {
            ReinoMilitaryReportState st;
            if (!m_ReportStates.TryGetValue(cityId, out st))
            {
                st = new ReinoMilitaryReportState();
                m_ReportStates[cityId] = st;
            }

            return st;
        }

        public static List<ReinoArchivedReport> GetArchivedReports(int cityId)
        {
            List<ReinoArchivedReport> list;
            if (!m_ArchivedReportsByCity.TryGetValue(cityId, out list))
            {
                list = new List<ReinoArchivedReport>();
                m_ArchivedReportsByCity[cityId] = list;
            }

            return list;
        }

        public static List<ReinoGuardPostInfo> GetPosts(int cityId)
        {
            List<ReinoGuardPostInfo> list;
            if (!m_PostsByCity.TryGetValue(cityId, out list))
            {
                list = new List<ReinoGuardPostInfo>();
                m_PostsByCity[cityId] = list;
            }

            return list;
        }

        public static ReinoMilitarySession GetSession(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return new ReinoMilitarySession();

            ReinoMilitarySession s;
            if (!m_Sessions.TryGetValue(pm.Serial.Value, out s))
            {
                s = new ReinoMilitarySession();
                m_Sessions[pm.Serial.Value] = s;
            }

            return s;
        }

        public static bool CanAccessMilitaryGovernmentPage(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null || !role.CanMilitary)
                return false;

            if (role.Kind == ReinoCargoKind.MinisterEconomy)
                return false;

            if (role.Hierarchy <= 2)
                return true;

            return false;
        }

        public static bool CanAccessBarracksSubGump(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null || !role.CanMilitary)
                return false;

            if (role.Kind == ReinoCargoKind.MinisterEconomy)
                return false;

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && IsBarracksConstructionKey(cityId, role.LinkedConstructionKey))
                return true;

            return false;
        }

        public static bool CanManageWantedList(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null || !role.CanMilitary)
                return false;

            if (role.Kind == ReinoCargoKind.MinisterEconomy)
                return false;

            if (role.Kind == ReinoCargoKind.MinisterDefense)
                return true;

            return role.Hierarchy <= 2;
        }

        public static bool IsBarracksConstructionKey(int cityId, string key)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(key);
            return info != null && info.CityId == cityId && info.Definition != null && String.Equals(info.Definition.Id, "quartel_aurora", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPrisonConstructionKey(int cityId, string key)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(key);
            return info != null && info.CityId == cityId && info.Definition != null && String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetLawLabel(ReinoMilitaryLaw law)
        {
            switch (law)
            {
                case ReinoMilitaryLaw.HoodedWalk: return "Andar encapuzado";
                case ReinoMilitaryLaw.Stealing: return "Roubar";
                case ReinoMilitaryLaw.Snooping: return "Olhar bolsa";
                case ReinoMilitaryLaw.LootKnockedOut: return "Furtar desmaiado";
                case ReinoMilitaryLaw.Lockpicking: return "Arrombar fechaduras";
                case ReinoMilitaryLaw.Fighting: return "Brigar";
                case ReinoMilitaryLaw.AnimalTaming: return "Domar animais";
                case ReinoMilitaryLaw.AnimalKilling: return "Matar animais";
                case ReinoMilitaryLaw.ForeignPlanting: return "Plantar em fazendas alheias";
                case ReinoMilitaryLaw.ForeignHarvesting: return "Colher em fazendas alheias";
                case ReinoMilitaryLaw.DrugUse: return "Usar drogas";
                case ReinoMilitaryLaw.DrunkWalk: return "Andar bêbado";
                case ReinoMilitaryLaw.TakingFruit: return "Tirar frutos do reino";
                case ReinoMilitaryLaw.FenceJumping: return "Pular cerca";
                case ReinoMilitaryLaw.ArmedWalk: return "Andar armado";
                default: return "Lei";
            }
        }

        public static string GetActionLabel(ReinoGuardAction action)
        {
            switch (action)
            {
                case ReinoGuardAction.Report: return "Reportar";
                case ReinoGuardAction.Arrest: return "Prender";
                case ReinoGuardAction.Kill: return "Matar";
                default: return "Nenhuma";
            }
        }

        public static string GetGuardKindLabel(ReinoGuardKind kind)
        {
            switch (kind)
            {
                case ReinoGuardKind.Vigia: return "Vigia";
                case ReinoGuardKind.Rua: return "Guarda de Rua";
                case ReinoGuardKind.Armado: return "Guarda Armado";
                case ReinoGuardKind.Arqueiro: return "Guarda Arqueiro";
                case ReinoGuardKind.CavalariaArmada: return "Cavalaria Armada";
                case ReinoGuardKind.CavalariaArqueira: return "Cavalaria Arqueira";
                case ReinoGuardKind.Oficial: return "Oficial";
                default: return "Guarda";
            }
        }

        public static string GetRouteScheduleLabel(ReinoRouteSchedule schedule)
        {
            switch (schedule)
            {
                case ReinoRouteSchedule.Every15Minutes: return "A cada 15 minutos";
                case ReinoRouteSchedule.Every30Minutes: return "A cada 30 minutos";
                case ReinoRouteSchedule.Every45Minutes: return "A cada 45 minutos";
                case ReinoRouteSchedule.Every60Minutes: return "A cada 1 hora";
                case ReinoRouteSchedule.DawnOnly: return "Somente de madrugada";
                case ReinoRouteSchedule.Infinite: return "Rota infinita";
                default: return "Rota infinita";
            }
        }

        public static string GetRouteSpeedLabel(ReinoRouteSpeed speed)
        {
            switch (speed)
            {
                case ReinoRouteSpeed.Short: return "Curto";
                case ReinoRouteSpeed.Medium: return "Médio";
                case ReinoRouteSpeed.Long: return "Longo";
                default: return "Curto";
            }
        }

        public static bool IsLawEnabled(int cityId, ReinoMilitaryLaw law)
        {
            return GetPolicy(cityId).EnabledLaws.Contains(law);
        }

        public static void ToggleLaw(int cityId, ReinoMilitaryLaw law)
        {
            ReinoMilitaryPolicy p = GetPolicy(cityId);
            bool enabled;

            if (p.EnabledLaws.Contains(law))
            {
                p.EnabledLaws.Remove(law);
                enabled = false;
            }
            else
            {
                p.EnabledLaws.Add(law);
                enabled = true;
            }

            QueueLawChangeNotice(cityId, law, enabled);
        }

        public static bool HasBarracks(int cityId)
        {
            return !String.IsNullOrWhiteSpace(FindPrimaryBarracksKey(cityId));
        }

        public static bool HasPrison(int cityId)
        {
            return !String.IsNullOrWhiteSpace(FindPrimaryPrisonKey(cityId));
        }

        private static void QueueLawChangeNotice(int cityId, ReinoMilitaryLaw law, bool enabled)
        {
            string line = enabled
                ? "Nova lei em vigor: " + GetLawLabel(law) + "."
                : "Lei revogada: " + GetLawLabel(law) + ".";

            foreach (Mobile mobile in World.Mobiles.Values)
            {
                PlayerMobile pm = mobile as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (!String.Equals(PlayerMobile.NormalizeOSUCityId(pm.OSUCitizenCityId), PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(cityId)), StringComparison.OrdinalIgnoreCase))
                    continue;

                List<string> list;
                if (!m_PendingLawNoticesByPlayer.TryGetValue(pm.Serial.Value, out list))
                {
                    list = new List<string>();
                    m_PendingLawNoticesByPlayer[pm.Serial.Value] = list;
                }

                if (!list.Contains(line))
                    list.Add(line);

                if (pm.NetState != null && m_QueuedOnlineLawNotice.Add(pm.Serial.Value))
                {
                    int serial = pm.Serial.Value;

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), delegate
                    {
                        m_QueuedOnlineLawNotice.Remove(serial);

                        Mobile found;
                        if (World.Mobiles.TryGetValue(serial, out found))
                            ShowPendingLawNotice(found as PlayerMobile);
                    });
                }
            }
        }

        public static void ShowPendingLawNotice(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            List<string> list;
            if (!m_PendingLawNoticesByPlayer.TryGetValue(pm.Serial.Value, out list) || list == null || list.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("Foi registrada uma alteração nas leis vigentes do reino.<BR><BR>");

            for (int i = 0; i < list.Count; i++)
                sb.Append("• ").Append(list[i]).Append("<BR>");

            sb.Append("</BASEFONT>");

            pm.SendGump(new ReinoCargoDismissalNoticeGump("Aviso de leis", sb.ToString(), 0));
            list.Clear();
        }

        public static string GetCurrentLawsHtml(int cityId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000><BIG><B>Leis vigentes</B></BIG><BR><BR>");

            bool any = false;
            foreach (ReinoMilitaryLaw law in Enum.GetValues(typeof(ReinoMilitaryLaw)))
            {
                if (!IsLawEnabled(cityId, law))
                    continue;

                any = true;
                sb.Append("• ").Append(GetLawLabel(law)).Append("<BR>");
            }

            if (!any)
                sb.Append("Nenhuma lei militar especial está em vigor neste reino.");

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string AddWanted(PlayerMobile from, int cityId, string name, ReinoGuardAction action)
        {
            if (!CanManageWantedList(from, cityId))
                return "Você não tem permissão para alterar a lista de procurados.";

            name = (name ?? String.Empty).Trim();
            if (name.Length < 2)
                return "Digite um nome válido.";

            List<ReinoWantedEntry> list = GetWantedList(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                if (String.Equals(list[i].PlayerName, name, StringComparison.OrdinalIgnoreCase))
                {
                    list[i].Action = action;
                    list[i].AddedUtc = DateTime.UtcNow;
                    list[i].AddedBySerial = from.Serial.Value;
                    list[i].AddedByName = from.Name;
                    AddCrimeNote(cityId, name + " foi atualizado na lista de procurados.");
                    return "Ação do procurado atualizada.";
                }
            }

            list.Add(new ReinoWantedEntry
            {
                PlayerName = name,
                Action = action,
                AddedUtc = DateTime.UtcNow,
                AddedBySerial = from.Serial.Value,
                AddedByName = from.Name
            });

            AddCrimeNote(cityId, name + " foi adicionado à lista de procurados.");
            return "Nome adicionado à lista de procurados.";
        }

        public static string RemoveWanted(PlayerMobile from, int cityId, string name)
        {
            if (!CanManageWantedList(from, cityId))
                return "Você não tem permissão para alterar a lista de procurados.";

            name = (name ?? String.Empty).Trim();
            if (name.Length < 2)
                return "Digite um nome válido.";

            List<ReinoWantedEntry> list = GetWantedList(cityId);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (String.Equals(list[i].PlayerName, name, StringComparison.OrdinalIgnoreCase))
                {
                    list.RemoveAt(i);
                    AddCrimeNote(cityId, name + " foi removido da lista de procurados.");
                    return "Nome removido da lista de procurados.";
                }
            }

            return "Esse nome não está na lista.";
        }

        public static ReinoWantedEntry FindWanted(int cityId, Mobile m)
        {
            if (m == null)
                return null;

            List<ReinoWantedEntry> list = GetWantedList(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                if (String.Equals(list[i].PlayerName, m.Name, StringComparison.OrdinalIgnoreCase))
                    return list[i];
            }

            return null;
        }

        public static bool ResolveWantedAfterGuardAction(int cityId, Mobile target, string details)
        {
            if (target == null || target.Deleted || String.IsNullOrWhiteSpace(target.Name))
                return false;

            List<ReinoWantedEntry> list = GetWantedList(cityId);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ReinoWantedEntry entry = list[i];
                if (entry == null)
                    continue;

                if (!String.Equals(entry.PlayerName, target.Name, StringComparison.OrdinalIgnoreCase))
                    continue;

                list.RemoveAt(i);
                AddCrimeNote(cityId, target.Name + " foi removido da lista de procurados. " + (details ?? String.Empty));
                return true;
            }

            return false;
        }

        public static void NotifyCrime(Mobile actor, ReinoMilitaryLaw law, string notes)
        {
            NotifyCrimeByScope(actor, law, notes, ReinoAreaScope.Total);
        }

        public static bool NotifyCrimeAt(Mobile actor, Point3D location, Map map, ReinoMilitaryLaw law, string notes)
        {
            return NotifyCrimeAt(actor, location, map, law, notes, ReinoAreaScope.Total);
        }

        private static bool NotifyCrimeByScope(Mobile actor, ReinoMilitaryLaw law, string notes, ReinoAreaScope scope)
        {
            if (actor == null || actor.Deleted || actor.Map == null || actor.Map == Map.Internal)
                return false;

            return NotifyCrimeAt(actor, actor.Location, actor.Map, law, notes, scope);
        }

        private static bool NotifyCrimeAt(Mobile actor, Point3D location, Map map, ReinoMilitaryLaw law, string notes, ReinoAreaScope scope)
        {
            if (actor == null || actor.Deleted || map == null || map == Map.Internal)
                return false;

            int cityId;
            if (!TryResolveCityIdAt(location, map, scope, out cityId))
                return false;

            if (!IsLawEnabled(cityId, law))
                return false;

            HandleObservedCrime(cityId, actor, law, notes);
            return true;
        }

        public static void NotifyLootKnockedOut(Mobile actor, Corpse corpse)
        {
            if (actor == null || corpse == null || !IsRecoverableKnockoutCorpse(corpse))
                return;

            NotifyCrimeAt(actor, corpse.GetWorldLocation(), corpse.Map, ReinoMilitaryLaw.LootKnockedOut, "Furtando corpo desmaiado.", ReinoAreaScope.Total);
        }

        public static void NotifySnooping(Mobile actor, object target)
        {
            Point3D loc;
            Map map;
            if (!TryResolveActionLocation(actor, target, out loc, out map))
                return;

            NotifyCrimeAt(actor, loc, map, ReinoMilitaryLaw.Snooping, "Olhou a bolsa de outra pessoa.", ReinoAreaScope.Total);
        }

        public static void NotifyLockpicking(Mobile actor, object target)
        {
            Point3D loc;
            Map map;
            if (!TryResolveActionLocation(actor, target, out loc, out map))
                return;

            NotifyCrimeAt(actor, loc, map, ReinoMilitaryLaw.Lockpicking, "Tentou arrombar uma fechadura.", ReinoAreaScope.Total);
        }

        public static void NotifyAnimalTaming(Mobile actor, Mobile target)
        {
            Point3D loc;
            Map map;
            if (!TryResolveActionLocation(actor, target, out loc, out map))
                return;

            NotifyCrimeAt(actor, loc, map, ReinoMilitaryLaw.AnimalTaming, "Tentou domar um animal do reino.", ReinoAreaScope.Total);
        }

        public static void NotifyAnimalKilled(Mobile actor, Mobile target)
        {
            if (actor == null || target == null || target.Player)
                return;

            BaseCreature bc = target as BaseCreature;
            if (bc != null && bc.IsAggressiveMonster)
                return;

            NotifyCrimeAt(actor, target.Location, target.Map, ReinoMilitaryLaw.AnimalKilling, "Matou um animal em território do reino.", ReinoAreaScope.Total);
        }

        public static void NotifyForeignPlanting(Mobile actor)
        {
            NotifyForeignPlanting(actor, actor);
        }

        public static void NotifyForeignPlanting(Mobile actor, object target)
        {
            Point3D loc;
            Map map;
            if (!TryResolveActionLocation(actor, target, out loc, out map))
                return;

            int cityId;
            if (!TryResolveCityIdAt(loc, map, ReinoAreaScope.LotOnly, out cityId))
                return;

            if (IsActorAuthorizedForLot(actor, loc, map))
                return;

            NotifyCrimeAt(actor, loc, map, ReinoMilitaryLaw.ForeignPlanting, "Plantou em fazenda alheia.", ReinoAreaScope.LotOnly);
        }

        public static void NotifyForeignHarvesting(Mobile actor)
        {
            NotifyForeignHarvesting(actor, actor);
        }

        public static void NotifyForeignHarvesting(Mobile actor, object target)
        {
            Point3D loc;
            Map map;
            if (!TryResolveActionLocation(actor, target, out loc, out map))
                return;

            int cityId;
            if (!TryResolveCityIdAt(loc, map, ReinoAreaScope.LotOnly, out cityId))
                return;

            if (IsActorAuthorizedForLot(actor, loc, map))
                return;

            NotifyCrimeAt(actor, loc, map, ReinoMilitaryLaw.ForeignHarvesting, "Colheu em fazenda alheia.", ReinoAreaScope.LotOnly);
        }

        public static void NotifyDrugUse(Mobile actor)
        {
            NotifyCrimeByScope(actor, ReinoMilitaryLaw.DrugUse, "Usou drogas em território do reino.", ReinoAreaScope.Total);
        }

        public static void NotifyDrunkWalk(Mobile actor)
        {
            NotifyCrimeByScope(actor, ReinoMilitaryLaw.DrunkWalk, "Andava bêbado em espaço público do reino.", ReinoAreaScope.Public);
        }

        public static void NotifyFruitTaken(Mobile actor)
        {
            NotifyCrimeByScope(actor, ReinoMilitaryLaw.TakingFruit, "Retirou frutos do reino.", ReinoAreaScope.Public);
        }

        public static void NotifyFruitTaken(Mobile actor, object target)
        {
            Point3D loc;
            Map map;
            if (!TryResolveActionLocation(actor, target, out loc, out map))
                return;

            NotifyCrimeAt(actor, loc, map, ReinoMilitaryLaw.TakingFruit, "Retirou frutos do reino.", ReinoAreaScope.Public);
        }

        public static void NotifyFenceJump(Mobile actor)
        {
            NotifyCrimeByScope(actor, ReinoMilitaryLaw.FenceJumping, "Pulou cerca ou meia parede.", ReinoAreaScope.Total);
        }

        public static void NotifyArmedWalk(Mobile actor)
        {
            NotifyCrimeByScope(actor, ReinoMilitaryLaw.ArmedWalk, "Entrou armado no reino.", ReinoAreaScope.Total);
        }

        private static void OnItemStolen(ItemStolenEventArgs e)
        {
            if (e == null || e.Mobile == null)
                return;

            NotifyCrime(e.Mobile, ReinoMilitaryLaw.Stealing, "Roubo presenciado pelos guardas.");
        }

        private static void OnAggressiveAction(AggressiveActionEventArgs e)
        {
            if (e == null || e.Aggressor == null || e.Aggressed == null)
                return;

            Mobile aggressor = e.Aggressor as Mobile;
            Mobile aggressed = e.Aggressed as Mobile;

            if (aggressor == null || aggressed == null)
                return;

            OSUCityGuard guard = aggressed as OSUCityGuard;
            if (guard != null)
            {
                AlertNearbyGuards(guard.CityId, aggressor, guard.Location, 18);
                return;
            }

            if (aggressed.Player)
                NotifyCrime(aggressor, ReinoMilitaryLaw.Fighting, "Briga presenciada por guardas.");
        }

        private static void OnCreatureDeath(CreatureDeathEventArgs e)
        {
            if (e == null || e.Creature == null || e.Creature.Deleted)
                return;

            if (e.Creature.Player)
                return;

            BaseCreature bc = e.Creature as BaseCreature;
            if (bc != null && bc.IsAggressiveMonster)
                return;

            PlayerMobile killer = ResolvePlayer(e.Killer);
            if (killer == null || killer.Deleted)
                return;

            NotifyAnimalKilled(killer, e.Creature);
        }

        private static void OnSpeech(SpeechEventArgs e)
        {
            if (e == null || e.Mobile == null || String.IsNullOrWhiteSpace(e.Speech))
                return;

            if (e.Speech.IndexOf("*hic*", StringComparison.OrdinalIgnoreCase) >= 0)
                NotifyDrunkWalk(e.Mobile);
        }

        public static void HandleObservedCrime(int cityId, Mobile actor, ReinoMilitaryLaw law, string notes)
        {
            if (actor == null || actor.Deleted || actor.Map == null)
                return;

            PlayerMobile trialPlayer = actor as PlayerMobile;
            if (trialPlayer != null && ReinoTrialsSystem.IsInsideTribunal(cityId, actor.Location, actor.Map))
            {
                ReinoTrialsSystem.HandleCourtCrime(trialPlayer, cityId, GetLawLabel(law), false);
                return;
            }

            List<OSUCityGuard> witnesses = GetWitnessGuards(cityId, actor.Location, actor.Map, 18, true);
            if (witnesses.Count <= 0)
                return;

            ReinoWantedEntry wanted = FindWanted(cityId, actor);
            ReinoMilitaryPolicy policy = GetPolicy(cityId);

            ReinoGuardAction result = wanted != null ? wanted.Action : policy.CrimeDefaultAction;
            OSUCityGuard witness = witnesses[0];

            AddCrimeRecord(cityId, actor, law, witness, result, notes);

            if (result == ReinoGuardAction.Report)
                return;

            if (result == ReinoGuardAction.Kill)
            {
                for (int i = 0; i < witnesses.Count; i++)
                    witnesses[i].BeginAttack(actor, law, false);
            }
            else if (result == ReinoGuardAction.Arrest)
            {
                for (int i = 0; i < witnesses.Count; i++)
                    witnesses[i].BeginAttack(actor, law, true);
            }
        }

        private static bool TryResolveActionLocation(Mobile actor, object target, out Point3D loc, out Map map)
        {
            loc = Point3D.Zero;
            map = Map.Internal;

            Item item = target as Item;
            if (item != null)
            {
                loc = item.GetWorldLocation();
                map = item.Map;

                if (map != null && map != Map.Internal)
                    return true;
            }

            Mobile mob = target as Mobile;
            if (mob != null)
            {
                loc = mob.Location;
                map = mob.Map;

                if (map != null && map != Map.Internal)
                    return true;
            }

            if (actor != null && !actor.Deleted && actor.Map != null && actor.Map != Map.Internal)
            {
                loc = actor.Location;
                map = actor.Map;
                return true;
            }

            return false;
        }

        private static bool IsRecoverableKnockoutCorpse(Corpse corpse)
        {
            if (corpse == null || corpse.Deleted || !corpse.OSUKnockoutCorpse)
                return false;

            PlayerMobile pm = corpse.Owner as PlayerMobile;
            if (pm != null)
                return pm.OSULives > 0;

            BaseCreature bc = corpse.Owner as BaseCreature;
            if (bc != null)
                return bc.OSULives > 0;

            return true;
        }

        private static PlayerMobile ResolvePlayer(Mobile m)
        {
            if (m == null)
                return null;

            PlayerMobile pm = m as PlayerMobile;
            if (pm != null)
                return pm;

            BaseCreature bc = m as BaseCreature;
            if (bc != null)
            {
                if (bc.ControlMaster is PlayerMobile)
                    return (PlayerMobile)bc.ControlMaster;

                if (bc.SummonMaster is PlayerMobile)
                    return (PlayerMobile)bc.SummonMaster;
            }

            return null;
        }

        private static bool TryResolveCityIdAt(Point3D p, Map map, ReinoAreaScope scope, out int cityId)
        {
            cityId = -1;

            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;
            for (int i = 0; i < count; i++)
            {
                bool inKingdom = ReinoExpansionSystem.IsPointInsideKingdomArea(i, p, map);
                bool inLot = ReinoExpansionSystem.IsPointInsideAnyLot(i, p, map);
                bool inDecor = ReinoExpansionSystem.IsPointInsideUnlockedDecorativeArea(i, p, map);
                bool inWall = ReinoExpansionSystem.IsPointInsideBuiltWallArea(i, p, map);

                bool inside = false;
                switch (scope)
                {
                    case ReinoAreaScope.Public:
                        inside = inKingdom || inDecor || inWall;
                        break;
                    case ReinoAreaScope.LotOnly:
                        inside = inLot;
                        break;
                    default:
                        inside = inKingdom || inLot || inDecor || inWall;
                        break;
                }

                if (inside)
                {
                    cityId = i;
                    return true;
                }
            }

            return false;
        }

        public static bool IsPointInsideReinoAreaTotal(Point3D p, Map map, out int cityId)
        {
            return TryResolveCityIdAt(p, map, ReinoAreaScope.Total, out cityId);
        }

        public static bool IsPointInsideReinoAreaPublica(Point3D p, Map map, out int cityId)
        {
            return TryResolveCityIdAt(p, map, ReinoAreaScope.Public, out cityId);
        }

        public static bool IsPointInsideReinoLot(Point3D p, Map map, out int cityId)
        {
            return TryResolveCityIdAt(p, map, ReinoAreaScope.LotOnly, out cityId);
        }

        public static bool IsActorAuthorizedForLot(Mobile actor, Point3D p, Map map)
        {
            if (actor == null || actor.Deleted || map == null || map == Map.Internal)
                return false;

            ReinoLotDefinition lot = ReinoExpansionSystem.FindLotAt(p, map);
            if (lot == null)
                return false;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(ReinoMaintenanceSystem.BuildLotKey(lot.LotId));
            if (info != null && info.LotState != null && info.LotState.RentalSignSerials != null)
            {
                for (int i = 0; i < info.LotState.RentalSignSerials.Count; i++)
                {
                    TownHouseSign sign = World.FindItem((Serial)info.LotState.RentalSignSerials[i]) as TownHouseSign;
                    if (sign != null && sign.IsOwnedBy(actor))
                        return true;
                }
            }

            TownHouse townHouse = BaseHouse.FindHouseAt(p, map, 16) as TownHouse;
            if (townHouse != null && townHouse.ForSaleSign != null && townHouse.ForSaleSign.IsOwnedBy(actor))
                return true;

            return false;
        }


        private static void AddCrimeNote(int cityId, string note)
        {
            List<ReinoCrimeRecord> list = GetCrimeList(cityId);
            list.Add(new ReinoCrimeRecord
            {
                CityId = cityId,
                CriminalSerial = 0,
                CriminalName = "Registro do Reino",
                Law = ReinoMilitaryLaw.Fighting,
                Utc = DateTime.UtcNow,
                WitnessGuardSerial = 0,
                WitnessGuardName = "Ofício Militar",
                Result = ReinoGuardAction.Report,
                Notes = note
            });
        }

        private static bool IsAdministrativeCrimeRecord(ReinoCrimeRecord r)
        {
            return r != null && r.CriminalSerial == 0 && r.WitnessGuardSerial == 0;
        }

        public static void AddCrimeRecord(int cityId, Mobile actor, ReinoMilitaryLaw law, OSUCityGuard witness, ReinoGuardAction result, string notes)
        {
            List<ReinoCrimeRecord> list = GetCrimeList(cityId);
            list.Add(new ReinoCrimeRecord
            {
                CityId = cityId,
                CriminalSerial = actor != null ? actor.Serial.Value : 0,
                CriminalName = actor != null ? actor.Name : "Desconhecido",
                Law = law,
                Utc = DateTime.UtcNow,
                WitnessGuardSerial = witness != null ? witness.Serial.Value : 0,
                WitnessGuardName = witness != null ? witness.Name : "Guarda desconhecido",
                Result = result,
                Notes = notes ?? String.Empty
            });

            if (list.Count > 500)
                list.RemoveRange(0, list.Count - 500);
        }

        public static void AddPrisonRecord(int cityId, Mobile prisoner, OSUCityGuard guard, ReinoMilitaryLaw law, int durationHours, string notes)
        {
            List<ReinoPrisonRecord> list = GetPrisonList(cityId);
            list.Add(new ReinoPrisonRecord
            {
                CityId = cityId,
                PrisonerSerial = prisoner != null ? prisoner.Serial.Value : 0,
                PrisonerName = prisoner != null ? prisoner.Name : "Desconhecido",
                ArrestedBy = guard != null ? guard.Name : "Guarda",
                CrimeLabel = GetLawLabel(law),
                ArrestUtc = DateTime.UtcNow,
                ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(Math.Max(1, durationHours)),
                DurationHours = Math.Max(1, durationHours),
                Notes = notes ?? String.Empty
            });

            if (list.Count > 500)
                list.RemoveRange(0, list.Count - 500);
        }

        public static Dictionary<string, int> GetRecurringCriminals(int cityId)
        {
            Dictionary<string, int> data = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            List<ReinoCrimeRecord> list = GetCrimeList(cityId);

            for (int i = 0; i < list.Count; i++)
            {
                ReinoCrimeRecord r = list[i];
                if (r == null || String.IsNullOrWhiteSpace(r.CriminalName) || r.CriminalSerial == 0)
                    continue;

                int count;
                data.TryGetValue(r.CriminalName, out count);
                data[r.CriminalName] = count + 1;
            }

            return data;
        }

        public static string GetWantedHtml(int cityId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            List<ReinoWantedEntry> list = GetWantedList(cityId);
            if (list.Count <= 0)
            {
                sb.Append("Nenhum procurado registrado.");
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoWantedEntry e = list[i];
                    sb.Append("• <B>").Append(e.PlayerName).Append("</B> — ").Append(GetActionLabel(e.Action));
                    sb.Append("<BR>");
                }
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string GetLawSummaryHtml(int cityId)
        {
            StringBuilder sb = new StringBuilder();
            ReinoMilitaryPolicy p = GetPolicy(cityId);
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Ação para procurados:</B> ").Append(GetActionLabel(p.WantedDefaultAction)).Append(".<BR>");
            sb.Append("<B>Ação para atos criminais:</B> ").Append(GetActionLabel(p.CrimeDefaultAction)).Append(".<BR><BR>");
            sb.Append("Guardas só agem se presenciaram o ato, tiverem <B>line of sight</B> e estiverem a até <B>18 tiles</B> do infrator.<BR><BR>");
            sb.Append("As leis já preparadas nesta versão funcionam assim:<BR>");
            sb.Append("• Roubar, brigar, andar armado, andar encapuzado e pular cerca já entram no fluxo do sistema.<BR>");
            sb.Append("• Snooping, arrombar, domar, colher, plantar, drogas, embriaguez e frutos já estão com <B>hooks públicos</B> prontos para você plugar nos arquivos-base certos.<BR>");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string GetGuardDescriptionHtml(ReinoGuardKind kind)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>").Append(GetGuardKindLabel(kind)).Append("</B><BR><BR>");

            switch (kind)
            {
                case ReinoGuardKind.Vigia:
                    sb.Append("Uniforme simples do reino, botas, calças e arma leve como club ou mace.<BR>");
                    sb.Append("Stats e skills baixos. Morre fácil para um jogador mediano.<BR>");
                    break;
                case ReinoGuardKind.Rua:
                    sb.Append("Armadura de couro simples, uniforme do reino, botas, escudo de madeira e armas como broadsword, kriss, axe ou war mace.<BR>");
                    sb.Append("Patrulheiro básico para ruas e entradas.<BR>");
                    break;
                case ReinoGuardKind.Armado:
                    sb.Append("Plate de copper, heater shield de copper, uniforme do reino e armas corpo a corpo de copper.<BR>");
                    sb.Append("Mais resistente que o guarda de rua.<BR>");
                    break;
                case ReinoGuardKind.Arqueiro:
                    sb.Append("Studded leather, bow equivalente a copper, 100 flechas, uniforme do reino e botas.<BR>");
                    sb.Append("Bom para vigiar de longe.<BR>");
                    break;
                case ReinoGuardKind.CavalariaArmada:
                    sb.Append("Montado, com chainmail de copper, lança ou halberd, uniforme do reino e botas.<BR>");
                    sb.Append("Guarda pesado de deslocamento rápido.<BR>");
                    break;
                case ReinoGuardKind.CavalariaArqueira:
                    sb.Append("Montado, com arco, 100 bolts, uniforme do reino e botas.<BR>");
                    sb.Append("Forte em perseguição e pressão à distância.<BR>");
                    break;
                case ReinoGuardKind.Oficial:
                    sb.Append("Uniforme, botas e luvas de studded leather.<BR>");
                    sb.Append("Não luta nem vigia. Só recebe e entrega relatórios.<BR>");
                    break;
            }

            int hireGold, hireCloth, hireIron, hireWood;
            int wkGold, wkCloth, wkIron, wkWood;
            GetGuardCosts(kind, out hireGold, out hireCloth, out hireIron, out hireWood, out wkGold, out wkCloth, out wkIron, out wkWood);
            sb.Append("<BR><B>Contratação:</B> ").Append(hireGold).Append(" moedas, ").Append(hireCloth).Append(" tecido, ").Append(hireIron).Append(" ferro e ").Append(hireWood).Append(" madeira.<BR>");
            sb.Append("<B>Semanal:</B> ").Append(wkGold).Append(" moedas, ").Append(wkCloth).Append(" tecido, ").Append(wkIron).Append(" ferro e ").Append(wkWood).Append(" madeira.");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static void GetGuardCosts(ReinoGuardKind kind, out int hireGold, out int hireCloth, out int hireIron, out int hireWood, out int weeklyGold, out int weeklyCloth, out int weeklyIron, out int weeklyWood)
        {
            switch (kind)
            {
                default:
                case ReinoGuardKind.Vigia:
                    weeklyGold = 100; weeklyCloth = 10; weeklyIron = 10; weeklyWood = 10;
                    break;
                case ReinoGuardKind.Rua:
                    weeklyGold = 130; weeklyCloth = 12; weeklyIron = 12; weeklyWood = 10;
                    break;
                case ReinoGuardKind.Armado:
                    weeklyGold = 160; weeklyCloth = 14; weeklyIron = 18; weeklyWood = 12;
                    break;
                case ReinoGuardKind.Arqueiro:
                    weeklyGold = 170; weeklyCloth = 15; weeklyIron = 12; weeklyWood = 18;
                    break;
                case ReinoGuardKind.CavalariaArmada:
                    weeklyGold = 220; weeklyCloth = 18; weeklyIron = 24; weeklyWood = 14;
                    break;
                case ReinoGuardKind.CavalariaArqueira:
                    weeklyGold = 230; weeklyCloth = 20; weeklyIron = 18; weeklyWood = 22;
                    break;
                case ReinoGuardKind.Oficial:
                    weeklyGold = 180; weeklyCloth = 12; weeklyIron = 8; weeklyWood = 8;
                    break;
            }

            hireGold = weeklyGold;
            hireCloth = weeklyCloth;
            hireIron = weeklyIron;
            hireWood = weeklyWood;
        }

        public static void GetTotalWeeklyGuardCost(int cityId, out int gold, out int cloth, out int iron, out int wood)
        {
            gold = cloth = iron = wood = 0;

            List<ReinoGuardPostInfo> posts = GetPosts(cityId);
            for (int i = 0; i < posts.Count; i++)
            {
                ReinoGuardPostInfo post = posts[i];
                if (post == null || (!post.Active) || post.Training)
                    continue;

                if (post.GuardKind == ReinoGuardKind.Vigia && post.GuardSerial == 0 && FindGuard(post) == null)
                {
                    if (post.Level <= 0)
                        continue;
                }

                if (post.GuardSerial == 0 && FindGuard(post) == null && !post.Training)
                    continue;

                int a, b, c, d, e, f, g, h;
                GetGuardCosts(post.GuardKind, out a, out b, out c, out d, out e, out f, out g, out h);
                gold += e; cloth += f; iron += g; wood += h;
            }
        }

        public static int GetGuardTrainingCost(ReinoGuardKind kind, int level)
        {
            int baseCost;
            switch (kind)
            {
                default:
                case ReinoGuardKind.Vigia: baseCost = 350; break;
                case ReinoGuardKind.Rua: baseCost = 500; break;
                case ReinoGuardKind.Armado: baseCost = 700; break;
                case ReinoGuardKind.Arqueiro: baseCost = 750; break;
                case ReinoGuardKind.CavalariaArmada: baseCost = 950; break;
                case ReinoGuardKind.CavalariaArqueira: baseCost = 1000; break;
                case ReinoGuardKind.Oficial: baseCost = 850; break;
            }

            return baseCost + Math.Max(0, level - 1) * 250;
        }

        public static string AddGuardPost(PlayerMobile from, int cityId, Direction dir)
        {
            if (from == null || from.Deleted || from.Map == null)
                return "Jogador inválido.";

            if (!CanAccessBarracksSubGump(from, cityId))
                return "Você não tem permissão militar para criar pontos de guarda.";

            if (!CanPlaceGuardPointAt(cityId, from.Location, from.Map, out string constructionKey))
                return "O ponto de guarda só pode ser criado em área do reino ou área decorativa liberada, nunca ocupando lote.";

            if (GetPostAt(cityId, from.Location, from.Map) != null)
                return "Já existe um ponto de guarda nesse local.";

            ReinoGuardPostMarker marker = new ReinoGuardPostMarker(cityId);
            marker.MoveToWorld(from.Location, from.Map);
            marker.MakeVisibleFor(TimeSpan.FromMinutes(1.0));

            ReinoGuardPostInfo post = new ReinoGuardPostInfo();
            post.Id = m_NextPostId++;
            marker.PostId = post.Id;
            post.CityId = cityId;
            post.ConstructionKey = constructionKey;
            post.Location = from.Location;
            post.MapIndex = from.Map.MapID;
            post.MarkerSerial = marker.Serial.Value;
            post.Facing = (int)NormalizeFacing(dir);
            post.Active = true;
            GetPosts(cityId).Add(post);

            return "Ponto de guarda criado. O marcador ficará visível por 1 minuto.";
        }

        public static string RemoveGuardPost(PlayerMobile from, int cityId)
        {
            if (from == null || from.Deleted || from.Map == null)
                return "Jogador inválido.";

            ReinoGuardPostInfo post = GetPostAt(cityId, from.Location, from.Map);
            if (post == null)
                return "Não existe ponto de guarda nesse tile.";

            DeletePostWorldObjects(post);
            GetPosts(cityId).Remove(post);
            return "Ponto de guarda removido.";
        }

        public static string AddGuardToCurrentPost(PlayerMobile from, int cityId, ReinoGuardKind kind, Direction dir)
        {
            if (from == null || from.Deleted || from.Map == null)
                return "Jogador inválido.";

            ReinoGuardPostInfo post = GetPostAt(cityId, from.Location, from.Map);
            if (post == null)
                return "Fique sobre um ponto de guarda para adicionar um guarda.";

            if (post.Training)
                return "Esse ponto está reservado porque o guarda original ainda está em treinamento.";

            if (post.GuardSerial != 0 && FindGuard(post) != null)
                return "Já existe um guarda nesse ponto.";

            int hireGold, hireCloth, hireIron, hireWood;
            int wkGold, wkCloth, wkIron, wkWood;
            GetGuardCosts(kind, out hireGold, out hireCloth, out hireIron, out hireWood, out wkGold, out wkCloth, out wkIron, out wkWood);

            if (!ConsumeResources(cityId, hireGold, hireCloth, hireIron, hireWood, out string fail))
                return fail;

            post.GuardKind = kind;
            post.Facing = (int)NormalizeFacing(dir);
            post.Level = Math.Max(1, post.Level);
            post.Uniformized = true;

            OSUCityGuard guard = SpawnGuard(post);
            if (guard == null)
                return "Não foi possível criar o guarda.";

            return "Guarda criado. O custo de contratação foi retirado do tesouro.";
        }

        public static string UniformizeAllGuards(PlayerMobile from, int cityId)
        {
            List<ReinoGuardPostInfo> posts = GetPosts(cityId);
            int pending = 0;

            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i] != null && !posts[i].Uniformized && FindGuard(posts[i]) != null)
                    pending++;
            }

            if (pending <= 0)
                return "Todos os guardas já estão uniformizados.";

            int gold = pending * 100;
            int cloth = pending * 15;

            if (!ConsumeResources(cityId, gold, cloth, 0, 0, out string fail))
                return fail;

            for (int i = 0; i < posts.Count; i++)
            {
                ReinoGuardPostInfo post = posts[i];
                if (post == null || post.Uniformized)
                    continue;

                OSUCityGuard guard = FindGuard(post);
                if (guard == null)
                    continue;

                post.Uniformized = true;
                guard.Uniformized = true;
                guard.ApplyUniform();
            }

            return "Todos os guardas do reino foram uniformizados.";
        }

        public static string GetUniformizationCostPreview(int cityId)
        {
            List<ReinoGuardPostInfo> posts = GetPosts(cityId);
            int pending = 0;
            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i] != null && !posts[i].Uniformized && FindGuard(posts[i]) != null)
                    pending++;
            }

            if (pending <= 0)
                return "Não há guardas sem uniforme.";

            return "Uniformizar agora vai custar " + (pending * 100) + " moedas e " + (pending * 15) + " tecidos para o reino. Clique mais uma vez para confirmar.";
        }

        public static List<ReinoGuardPostInfo> GetTrainingEntries(int cityId)
        {
            List<ReinoGuardPostInfo> source = GetPosts(cityId);
            List<ReinoGuardPostInfo> list = new List<ReinoGuardPostInfo>();

            for (int i = 0; i < source.Count; i++)
            {
                ReinoGuardPostInfo post = source[i];
                if (post == null)
                    continue;

                if (post.Training)
                {
                    list.Add(post);
                    continue;
                }

                OSUCityGuard guard = FindGuard(post);
                if (guard == null || guard.Deleted || String.IsNullOrWhiteSpace(guard.Name))
                    continue;

                list.Add(post);
            }

            list.Sort(delegate (ReinoGuardPostInfo a, ReinoGuardPostInfo b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                int cmp = a.Location.X.CompareTo(b.Location.X);
                if (cmp != 0) return cmp;
                return a.Location.Y.CompareTo(b.Location.Y);
            });

            return list;
        }

        private static void WantedPulse()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.Map == null || pm.Map == Map.Internal)
                    continue;

                int cityId;
                if (!TryResolveCityIdAt(pm.Location, pm.Map, ReinoAreaScope.Total, out cityId))
                    continue;

                ReinoWantedEntry wanted = FindWanted(cityId, pm);
                if (wanted == null)
                    continue;

                List<OSUCityGuard> witnesses = GetWitnessGuards(cityId, pm.Location, pm.Map, 18, true);
                if (witnesses.Count <= 0)
                    continue;

                int key = (pm.Serial.Value * 1000) + cityId;
                DateTime next;
                if (m_LastWantedNotice.TryGetValue(key, out next) && next > DateTime.UtcNow)
                    continue;

                m_LastWantedNotice[key] = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);

                AddCrimeNote(cityId, pm.Name + " foi avistado por guardas na lista de procurados.");

                if (wanted.Action == ReinoGuardAction.Report || wanted.Action == ReinoGuardAction.None)
                    continue;

                bool arrest = wanted.Action == ReinoGuardAction.Arrest;

                for (int i = 0; i < witnesses.Count; i++)
                    witnesses[i].BeginAttack(pm, ReinoMilitaryLaw.Fighting, arrest);
            }
        }

        public static string StartGuardTraining(PlayerMobile from, int cityId, int postId)
        {
            ReinoGuardPostInfo post = FindPostById(cityId, postId);
            if (post == null)
                return "Guarda inválido.";

            if (post.Level >= 5)
                return "Esse guarda já chegou ao nível máximo.";

            OSUCityGuard guard = FindGuard(post);
            if (guard == null)
                return "Esse ponto está sem guarda ativo.";

            int goldCost = GetGuardTrainingCost(post.GuardKind, post.Level);
            if (!ConsumeResources(cityId, goldCost, 0, 0, 0, out string fail))
                return fail;

            post.Training = true;
            post.TrainingEndsUtc = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);
            post.GuardSerial = 0;

            guard.Delete();
            return "Treinamento iniciado. Durante os testes, o guarda voltará ao posto em 1 minuto.";
        }

        public static void Pulse()
        {
            UpdateHoodedNames();
            AutoSheathePulse();
            WantedPulse();
            RoutePulse();
            TrainingPulse();
            FaceHomePulse();
        }

        private static void UpdateHoodedNames()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (IsHooded(pm))
                {
                    if (pm.NameMod != "Encoberto")
                        pm.NameMod = "Encoberto";

                    int cityId = ResolveCityIdAt(pm.Location, pm.Map);
                    if (cityId >= 0)
                        TryPassiveCrimeNotice(pm, cityId, ReinoMilitaryLaw.HoodedWalk, "Circulava encapuzado no reino.");
                }
                else if (pm.NameMod == "Encoberto")
                {
                    pm.NameMod = null;
                }
            }
        }

        private static void AutoSheathePulse()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.Map == null || pm.Map == Map.Internal)
                    continue;

                int cityId = ResolveCityIdAt(pm.Location, pm.Map);
                if (cityId < 0)
                    continue;

                Item one = pm.FindItemOnLayer(Layer.OneHanded);
                Item two = pm.FindItemOnLayer(Layer.TwoHanded);
                bool armed = one is BaseWeapon || two is BaseWeapon;

                if (m_AutoSheathe.Contains(pm.Serial.Value) && armed)
                {
                    SheatheWeapons(pm, false);
                    one = pm.FindItemOnLayer(Layer.OneHanded);
                    two = pm.FindItemOnLayer(Layer.TwoHanded);
                    armed = one is BaseWeapon || two is BaseWeapon;
                }

                if (armed)
                    TryPassiveCrimeNotice(pm, cityId, ReinoMilitaryLaw.ArmedWalk, "Circulava armado no reino.");
            }
        }

        private static void TryPassiveCrimeNotice(PlayerMobile pm, int cityId, ReinoMilitaryLaw law, string notes)
        {
            if (pm == null || pm.Deleted)
                return;

            if (!IsLawEnabled(cityId, law))
                return;

            int key = (pm.Serial.Value * 100) + (int)law;
            DateTime next;
            if (m_LastPassiveCrimeNotice.TryGetValue(key, out next) && next > DateTime.UtcNow)
                return;

            m_LastPassiveCrimeNotice[key] = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
            HandleObservedCrime(cityId, pm, law, notes);
        }

        private static void RoutePulse()
        {
            for (int cityId = 0; cityId < (ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4); cityId++)
            {
                List<ReinoGuardPostInfo> posts = GetPosts(cityId);
                for (int i = 0; i < posts.Count; i++)
                {
                    ReinoGuardPostInfo post = posts[i];
                    if (post == null || post.Training)
                        continue;

                    OSUCityGuard guard = FindGuard(post);
                    if (guard == null || guard.Combatant != null || post.RouteRootSerial == 0 || !post.RouteActivated)
                        continue;

                    if (guard.CurrentWayPoint != null)
                        continue;

                    WayPoint root = FindItem(post.RouteRootSerial) as WayPoint;
                    if (root == null || root.Deleted)
                        continue;

                    if (!guard.InRange(post.Location, 0))
                        continue;

                    if (!ShouldStartRoute(post))
                        continue;

                    guard.CurrentWayPoint = root;
                    guard.ConfigureRouteSpeed(post.RouteSpeed);
                    post.LastRouteUtc = DateTime.UtcNow;
                }
            }
        }

        private static bool ShouldStartRoute(ReinoGuardPostInfo post)
        {
            DateTime now = DateTime.UtcNow;

            switch (post.RouteSchedule)
            {
                case ReinoRouteSchedule.Infinite:
                    return true;
                case ReinoRouteSchedule.Every15Minutes:
                    return now >= post.LastRouteUtc + TimeSpan.FromMinutes(15.0);
                case ReinoRouteSchedule.Every30Minutes:
                    return now >= post.LastRouteUtc + TimeSpan.FromMinutes(30.0);
                case ReinoRouteSchedule.Every45Minutes:
                    return now >= post.LastRouteUtc + TimeSpan.FromMinutes(45.0);
                case ReinoRouteSchedule.Every60Minutes:
                    return now >= post.LastRouteUtc + TimeSpan.FromHours(1.0);
                case ReinoRouteSchedule.DawnOnly:
                    return now.Hour >= 1 && now.Hour <= 5 && now >= post.LastRouteUtc + TimeSpan.FromHours(6.0);
                default:
                    return false;
            }
        }

        private static void TrainingPulse()
        {
            for (int cityId = 0; cityId < (ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4); cityId++)
            {
                List<ReinoGuardPostInfo> posts = GetPosts(cityId);
                for (int i = 0; i < posts.Count; i++)
                {
                    ReinoGuardPostInfo post = posts[i];
                    if (post == null || !post.Training || DateTime.UtcNow < post.TrainingEndsUtc)
                        continue;

                    post.Training = false;
                    post.Level = Math.Min(5, post.Level + 1);
                    post.TrainingEndsUtc = DateTime.MinValue;
                    OSUCityGuard guard = SpawnGuard(post);
                    if (guard != null)
                        ApplyTrainingGain(guard, post.Level);
                }
            }
        }

        private static void FaceHomePulse()
        {
            for (int cityId = 0; cityId < (ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4); cityId++)
            {
                List<ReinoGuardPostInfo> posts = GetPosts(cityId);
                for (int i = 0; i < posts.Count; i++)
                {
                    OSUCityGuard guard = FindGuard(posts[i]);
                    if (guard == null || guard.Combatant != null || guard.CurrentWayPoint != null)
                        continue;

                    guard.Home = guard.PostLocation;
                    guard.Direction = (Direction)posts[i].Facing;

                    if (guard.CurrentWayPoint == null && !guard.InRange(guard.PostLocation, 0) && guard.Map != null && guard.Map != Map.Internal)
                        guard.Home = guard.PostLocation;
                }
            }
        }

        public static void ApplyTrainingGain(OSUCityGuard guard, int level)
        {
            if (guard == null || guard.Deleted)
                return;

            guard.RawStr += Utility.RandomMinMax(1, 3);
            guard.RawDex += Utility.RandomMinMax(1, 3);
            guard.HitsMaxSeed += Utility.RandomMinMax(8, 12);
            guard.StamMaxSeed += Utility.RandomMinMax(3, 6);
            guard.Hits = guard.HitsMax;
            guard.Stam = guard.StamMax;

            List<SkillName> skills = guard.GetTrainableSkills();
            if (skills.Count > 0)
            {
                SkillName chosen = skills[Utility.Random(skills.Count)];
                Skill sk = guard.Skills[chosen];
                if (sk != null)
                    sk.Base = sk.Base + Utility.RandomMinMax(4, 5);
            }
        }

        public static OSUCityGuard SpawnGuard(ReinoGuardPostInfo post)
        {
            if (post == null)
                return null;

            Map map = GetMapByIndex(post.MapIndex);
            if (map == null)
                map = Map.Felucca;

            OSUCityGuard guard = new OSUCityGuard(post.CityId, post.GuardKind);
            guard.PostId = post.Id;
            guard.PostLocation = post.Location;
            guard.Home = post.Location;
            guard.RangeHome = 0;
            guard.Direction = (Direction)post.Facing;
            guard.Uniformized = true;
            guard.GuardLevel = Math.Max(1, post.Level);
            guard.ConstructionKey = post.ConstructionKey;
            guard.ApplyLoadout();
            guard.MoveToWorld(post.Location, map);

            for (int i = 1; i < post.Level; i++)
                ApplyTrainingGain(guard, i + 1);

            post.GuardSerial = guard.Serial.Value;
            return guard;
        }

        public static OSUCityGuard FindGuard(ReinoGuardPostInfo post)
        {
            if (post == null || post.GuardSerial == 0)
                return null;

            Mobile m;
            if (!World.Mobiles.TryGetValue(post.GuardSerial, out m))
                return null;

            return m as OSUCityGuard;
        }

        public static ReinoGuardPostInfo FindPostById(int cityId, int postId)
        {
            List<ReinoGuardPostInfo> list = GetPosts(cityId);
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].Id == postId)
                    return list[i];
            return null;
        }

        public static ReinoGuardPostInfo FindPostByGuard(OSUCityGuard guard)
        {
            if (guard == null)
                return null;

            return FindPostById(guard.CityId, guard.PostId);
        }

        public static ReinoGuardPostInfo GetPostAt(int cityId, Point3D loc, Map map)
        {
            List<ReinoGuardPostInfo> list = GetPosts(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoGuardPostInfo post = list[i];
                if (post == null || post.MapIndex != (map != null ? map.MapID : 0))
                    continue;

                if (post.Location.X == loc.X && post.Location.Y == loc.Y && Math.Abs(post.Location.Z - loc.Z) <= 8)
                    return post;
            }

            return null;
        }

        public static bool CanPlaceGuardPointAt(int cityId, Point3D loc, Map map, out string constructionKey)
        {
            constructionKey = FindPrimaryBarracksKey(cityId);
            if (String.IsNullOrWhiteSpace(constructionKey))
                return false;

            if (!ReinoExpansionSystem.IsPointInsideKingdomArea(cityId, loc, map)
                && !ReinoExpansionSystem.IsPointInsideUnlockedDecorativeArea(cityId, loc, map))
                return false;

            if (ReinoExpansionSystem.IsPointInsideAnyLot(cityId, loc, map))
                return false;

            return true;
        }

        public static string FindPrimaryBarracksKey(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null)
                    continue;

                if (String.Equals(info.Definition.Id, "quartel_aurora", StringComparison.OrdinalIgnoreCase)
                    && (info.Status == ReinoLotStatus.Active || info.Status == ReinoLotStatus.UnderConstruction))
                    return info.Key;
            }

            return String.Empty;
        }

        public static string FindPrimaryPrisonKey(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null)
                    continue;

                if (String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase)
                    && (info.Status == ReinoLotStatus.Active || info.Status == ReinoLotStatus.UnderConstruction))
                    return info.Key;
            }

            return String.Empty;
        }

        public static bool TrySendToPrison(Mobile prisoner, int cityId, OSUCityGuard guard, ReinoMilitaryLaw law)
        {
            return ReinoPrisionSystem.TrySendToPrison(prisoner, cityId, guard, law);
        }

        public static bool FindFirstEmptyPrisonCell(int cityId, out Point3D cell)
        {
            cell = Point3D.Zero;

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null)
                return false;

            Map map = prison.Lot.Map;
            for (int i = 0; i < PrisonCells.Length; i++)
            {
                Point3D p = new Point3D(
                    prison.Lot.NorthWest.X + PrisonCells[i].Offset.X,
                    prison.Lot.NorthWest.Y + PrisonCells[i].Offset.Y,
                    prison.Lot.NorthWest.Z + PrisonCells[i].Offset.Z);

                bool occupied = false;
                IPooledEnumerable eable = map.GetMobilesInRange(p, 0);
                foreach (Mobile m in eable)
                {
                    if (m != null && !m.Deleted && m.Player)
                    {
                        occupied = true;
                        break;
                    }
                }
                eable.Free();

                if (!occupied)
                {
                    cell = p;
                    return true;
                }
            }

            return false;
        }

        private static ReinoConstructionRuntimeInfo FindPrimaryPrisonRuntime(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info != null && info.Definition != null && String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase))
                    return info;
            }

            return null;
        }

        public static bool StoreLootInBarracks(int cityId, Item item)
        {
            if (item == null || item.Deleted)
                return false;

            Container chest = FindBarracksChest(cityId);
            if (chest == null)
                return false;

            try
            {
                chest.DropItem(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Container FindBarracksChest(int cityId)
        {
            foreach (Item item in World.Items.Values)
            {
                ReinoBarracksLocker locker = item as ReinoBarracksLocker;
                if (locker != null && locker.CityId == cityId && !locker.Deleted)
                    return locker;
            }

            return null;
        }

        public static void RegisterGuardOutcome(OSUCityGuard guard, Mobile target, ReinoMilitaryLaw law, bool knockedOut, bool died, bool lootStored, bool prisoned)
        {
            if (guard == null)
                return;

            if (target != null && (knockedOut || died || prisoned))
                ResolveWantedAfterGuardAction(guard.CityId, target, "A guarda executou a ordem registrada.");

            List<ReinoCrimeRecord> list = GetCrimeList(guard.CityId);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ReinoCrimeRecord r = list[i];
                if (r == null)
                    continue;

                if (r.CriminalSerial == (target != null ? target.Serial.Value : 0) && r.Law == law)
                {
                    r.CriminalKnockedOut = knockedOut;
                    r.CriminalDied = died;
                    r.LootStoredInBarracks = lootStored;
                    r.SentToPrison = prisoned;
                    return;
                }
            }
        }

        public static List<OSUCityGuard> GetWitnessGuards(int cityId, Point3D loc, Map map, int range, bool canFight)
        {
            List<OSUCityGuard> list = new List<OSUCityGuard>();
            List<ReinoGuardPostInfo> posts = GetPosts(cityId);
            for (int i = 0; i < posts.Count; i++)
            {
                OSUCityGuard guard = FindGuard(posts[i]);
                if (guard == null || guard.Map != map || !guard.InRange(loc, range) || !guard.InLOS(loc))
                    continue;

                if (guard.IsOfficial && canFight)
                    continue;

                list.Add(guard);
            }

            list.Sort(delegate (OSUCityGuard a, OSUCityGuard b)
            {
                return a.GetDistanceToSqrt(loc).CompareTo(b.GetDistanceToSqrt(loc));
            });

            return list;
        }

        public static bool AcceptSurrender(PlayerMobile pm, int cityId, int guardSerial, ReinoMilitaryLaw law)
        {
            if (pm == null || pm.Deleted)
                return false;

            Mobile mob;
            if (!World.Mobiles.TryGetValue(guardSerial, out mob))
                return false;

            OSUCityGuard guard = mob as OSUCityGuard;
            if (guard == null || guard.Deleted)
                return false;

            bool prisoned = ReinoPrisionSystem.TrySendToPrison(pm, cityId, guard, law);

            if (!prisoned)
                return false;

            guard.Combatant = null;
            guard.Warmode = false;
            guard.FightMode = FightMode.None;
            guard.CurrentWayPoint = null;

            RegisterGuardOutcome(guard, pm, law, false, false, false, true);
            return true;
        }

        private static bool IsConfiscableItem(Item item)
        {
            if (item == null || item.Deleted)
                return false;

            if (item is BaseClothing)
                return false;

            if (!item.Movable)
                return false;

            return true;
        }

        public static bool ConfiscateLivingPrisonerItems(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            Bag lootBag = new Bag();
            lootBag.Name = "pertences de " + pm.Name;
            lootBag.Movable = true;

            List<Item> toMove = new List<Item>();

            if (pm.Backpack != null)
            {
                for (int i = 0; i < pm.Backpack.Items.Count; i++)
                {
                    Item item = pm.Backpack.Items[i];
                    if (IsConfiscableItem(item))
                        toMove.Add(item);
                }
            }

            Layer[] layers =
            {
        Layer.OneHanded, Layer.TwoHanded,
        Layer.Gloves, Layer.Helm, Layer.Neck,
        Layer.Ring, Layer.Bracelet, Layer.Earrings,
        Layer.Waist, Layer.Cloak, Layer.OuterTorso,
        Layer.MiddleTorso, Layer.Arms, Layer.InnerLegs,
        Layer.OuterLegs, Layer.Shoes, Layer.Pants,
        Layer.Shirt, Layer.InnerTorso
    };

            for (int i = 0; i < layers.Length; i++)
            {
                Item item = pm.FindItemOnLayer(layers[i]);
                if (IsConfiscableItem(item))
                    toMove.Add(item);
            }

            for (int i = 0; i < toMove.Count; i++)
            {
                Item item = toMove[i];
                if (item == null || item.Deleted)
                    continue;

                lootBag.DropItem(item);
            }

            if (lootBag.Items.Count == 0)
            {
                lootBag.Delete();
                return false;
            }

            if (!StoreLootInBarracks(cityId, lootBag))
            {
                lootBag.Delete();
                return false;
            }

            return true;
        }

        public static void AlertNearbyGuards(int cityId, Mobile attacker, Point3D around, int range)
        {
            List<OSUCityGuard> guards = GetWitnessGuards(cityId, around, attacker != null ? attacker.Map : Map.Internal, range, true);
            for (int i = 0; i < guards.Count; i++)
            {
                if (attacker != null)
                    guards[i].BeginAttack(attacker, ReinoMilitaryLaw.Fighting, false);
            }
        }

        public static bool IsHooded(Mobile m)
        {
            if (m == null)
                return false;

            for (int i = 0; i < HoodItemIds.Length; i++)
            {
                if (FindWornItemByItemId(m, HoodItemIds[i]) != null)
                    return true;
            }

            return false;
        }

        private static Item FindWornItemByItemId(Mobile m, int itemId)
        {
            if (m == null)
                return null;

            for (int i = 0; i < m.Items.Count; i++)
            {
                Item item = m.Items[i];
                if (item != null && item.ItemID == itemId)
                    return item;
            }

            return null;
        }

        public static bool SheatheWeapons(PlayerMobile pm, bool sendMessage)
        {
            if (pm == null || pm.Deleted || pm.Backpack == null)
                return false;

            bool moved = false;
            Item one = pm.FindItemOnLayer(Layer.OneHanded);
            Item two = pm.FindItemOnLayer(Layer.TwoHanded);

            if (one is BaseWeapon)
            {
                pm.Backpack.DropItem(one);
                moved = true;
            }

            if (two is BaseWeapon)
            {
                pm.Backpack.DropItem(two);
                moved = true;
            }

            if (moved && sendMessage)
                pm.SendMessage("Você embainha suas armas.");

            return moved;
        }

        public static bool ToggleAutoSheathe(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (m_AutoSheathe.Contains(pm.Serial.Value))
            {
                m_AutoSheathe.Remove(pm.Serial.Value);
                return false;
            }

            m_AutoSheathe.Add(pm.Serial.Value);
            return true;
        }

        public static bool TryJumpFence(PlayerMobile pm, out string message)
        {
            message = "";
            if (pm == null || pm.Deleted || pm.Map == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            Direction d = pm.Direction & Direction.Mask;
            int dx = 0, dy = 0;
            switch (d)
            {
                case Direction.North: dy = -1; break;
                case Direction.South: dy = 1; break;
                case Direction.East: dx = 1; break;
                case Direction.West: dx = -1; break;
                default:
                    message = "Fique virado para norte, sul, leste ou oeste.";
                    return false;
            }

            int midX = pm.X + dx;
            int midY = pm.Y + dy;
            int destX = pm.X + (dx * 2);
            int destY = pm.Y + (dy * 2);
            int z = pm.Z;

            if (!HasImpassableAt(pm.Map, midX, midY, z))
            {
                message = "Não há cerca ou meia parede na direção em que você está olhando.";
                return false;
            }

            if (!pm.Map.CanFit(destX, destY, z, 16, false, false))
            {
                message = "Há algo bloqueando o outro lado da cerca.";
                return false;
            }

            pm.MoveToWorld(new Point3D(destX, destY, z), pm.Map);
            NotifyFenceJump(pm);
            message = "Você pula a cerca.";
            return true;
        }

        private static bool HasImpassableAt(Map map, int x, int y, int z)
        {
            if (map == null)
                return false;

            StaticTile[] statics = map.Tiles.GetStaticTiles(x, y, true);
            for (int i = 0; i < statics.Length; i++)
            {
                ItemData data = TileData.ItemTable[statics[i].ID & TileData.MaxItemValue];
                if ((data.Flags & TileFlag.Impassable) != 0 && Math.Abs(statics[i].Z - z) <= 20)
                    return true;
            }

            IPooledEnumerable eable = map.GetItemsInRange(new Point3D(x, y, z), 0);
            foreach (Item item in eable)
            {
                if (item == null || item.Deleted)
                    continue;

                ItemData data = item.ItemData;
                if ((data.Flags & TileFlag.Impassable) != 0 && Math.Abs(item.Z - z) <= 20)
                {
                    eable.Free();
                    return true;
                }
            }
            eable.Free();

            return false;
        }

        public static Direction NormalizeFacing(Direction d)
        {
            d &= Direction.Mask;
            switch (d)
            {
                case Direction.Up:
                case Direction.North: return Direction.North;
                case Direction.Right:
                case Direction.East: return Direction.East;
                case Direction.Down:
                case Direction.South: return Direction.South;
                case Direction.Left:
                case Direction.West: return Direction.West;
                default: return Direction.North;
            }
        }

        public static Direction GetFacingByIndex(int index)
        {
            switch (index % 4)
            {
                default:
                case 0: return Direction.North;
                case 1: return Direction.East;
                case 2: return Direction.South;
                case 3: return Direction.West;
            }
        }

        public static string GetFacingLabel(int index)
        {
            switch (index % 4)
            {
                default:
                case 0: return "Norte";
                case 1: return "Leste";
                case 2: return "Sul";
                case 3: return "Oeste";
            }
        }

        public static int ResolveCityIdAt(Point3D p, Map map)
        {
            int cityId;
            return TryResolveCityIdAt(p, map, ReinoAreaScope.Total, out cityId) ? cityId : -1;
        }

        public static void AddDynamicWeeklyCosts(ReinoConstructionRuntimeInfo info, List<ReinoResourceCost> list)
        {
            if (info == null || info.Definition == null || list == null)
                return;

            if (String.Equals(info.Definition.Id, "quartel_aurora", StringComparison.OrdinalIgnoreCase))
            {
                int gold, cloth, iron, wood;
                GetTotalWeeklyGuardCost(info.CityId, out gold, out cloth, out iron, out wood);

                if (gold > 0) list.Add(new ReinoResourceCost(ReinoResourceType.Gold, gold));
                if (cloth > 0) list.Add(new ReinoResourceCost(ReinoResourceType.Cloth, cloth));
                if (iron > 0) list.Add(new ReinoResourceCost(ReinoResourceType.Iron, iron));
                if (wood > 0) list.Add(new ReinoResourceCost(ReinoResourceType.Wood, wood));
            }

            if (String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase))
            {
                int gold = ReinoPrisionSystem.GetDynamicWeeklyGold(info.CityId);
                if (gold > 0)
                    list.Add(new ReinoResourceCost(ReinoResourceType.Gold, gold));
            }
        }

        public static int GetActivePrisonerCount(int cityId)
        {
            int total = 0;
            List<ReinoPrisonRecord> list = GetPrisonList(cityId);
            DateTime now = DateTime.UtcNow;

            for (int i = 0; i < list.Count; i++)
            {
                ReinoPrisonRecord r = list[i];
                if (r == null)
                    continue;

                bool active = r.ArrestUtc <= now && r.ReleaseUtc > now && String.IsNullOrWhiteSpace(r.ReleasedBy);
                if (active)
                    total++;
            }

            return total;
        }

        private static bool ConsumeResources(int cityId, int gold, int cloth, int iron, int wood, out string fail)
        {
            fail = String.Empty;
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);

            if (gold > 0 && !ledger.Has(ReinoResourceType.Gold, gold)) { fail = "O reino não tem moedas suficientes."; return false; }
            if (cloth > 0 && !ledger.Has(ReinoResourceType.Cloth, cloth)) { fail = "O reino não tem tecido suficiente."; return false; }
            if (iron > 0 && !ledger.Has(ReinoResourceType.Iron, iron)) { fail = "O reino não tem ferro suficiente."; return false; }
            if (wood > 0 && !ledger.Has(ReinoResourceType.Wood, wood)) { fail = "O reino não tem madeira suficiente."; return false; }

            if (gold > 0) ledger.Add(ReinoResourceType.Gold, -gold);
            if (cloth > 0) ledger.Add(ReinoResourceType.Cloth, -cloth);
            if (iron > 0) ledger.Add(ReinoResourceType.Iron, -iron);
            if (wood > 0) ledger.Add(ReinoResourceType.Wood, -wood);
            return true;
        }

        public static string GetReportsSummaryHtml(int cityId)
        {
            List<ReinoCrimeRecord> crimes = GetCrimeList(cityId);
            ReinoMilitaryReportState st = GetReportState(cityId);

            int since = 0;
            for (int i = 0; i < crimes.Count; i++)
            {
                ReinoCrimeRecord r = crimes[i];
                if (r == null)
                    continue;

                if (IsAdministrativeCrimeRecord(r))
                    continue;

                if (r.Utc > st.LastDeliveredUtc)
                    since++;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Relatório</B> ").Append(since).Append(".<BR><BR>");
            sb.Append("<B>Último Relatório:</B><BR>").Append(st.LastDeliveredUtc == DateTime.MinValue ? "nunca" : st.LastDeliveredUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append(".<BR>");
            sb.Append("<B>Entregue a:</B><BR> ").Append(String.IsNullOrWhiteSpace(st.LastDeliveredTo) ? "ninguém" : st.LastDeliveredTo).Append(".");

            if (!String.IsNullOrWhiteSpace(st.Summary))
                {
                    sb.Append("<BR><BR><B>Anotações do ofício:</B><BR>");
                    sb.Append(st.Summary);
                }
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }




        public static string GetReportsDetailHtml(int cityId, int mode, int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            if (mode == 1)
            {
                List<ReinoCrimeRecord> list = GetCrimeList(cityId);
                
                if (list.Count <= 0)
                {
                    sb.Append("Nenhum crime registrado.");
                }
                else
                {
                    if (index < 0) index = 0;
                    if (index >= list.Count) index = list.Count - 1;
                    ReinoCrimeRecord r = list[index];
                    sb.Append("<B>Registro ").Append(index + 1).Append(" de ").Append(list.Count).Append("</B><BR><BR>");

                    if (IsAdministrativeCrimeRecord(r))
                    {
                        sb.Append("<B>Tipo:</B> Registro administrativo<BR>");
                        sb.Append("<B>Origem:</B> ").Append(r.WitnessGuardName).Append("<BR>");
                        sb.Append("<B>Quando:</B> ").Append(r.Utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                        if (!String.IsNullOrWhiteSpace(r.Notes))
                            sb.Append("<B>Detalhes:</B> ").Append(r.Notes);
                    }
                    else
                    {
                        sb.Append("<B>Quem:</B> ").Append(r.CriminalName).Append("<BR>");
                        sb.Append("<B>Crime:</B> ").Append(GetLawLabel(r.Law)).Append("<BR>");
                        sb.Append("<B>Guarda:</B> ").Append(r.WitnessGuardName).Append("<BR>");
                        sb.Append("<B>Quando:</B> ").Append(r.Utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                        sb.Append("<B>Resultado:</B> ").Append(GetActionLabel(r.Result)).Append("<BR>");
                        sb.Append("<B>Guarda morto:</B> ").Append(r.GuardDied ? "sim" : "não").Append("<BR>");
                        sb.Append("<B>Personagem morto:</B> ").Append(r.CriminalDied ? "sim" : "não").Append("<BR>");
                        sb.Append("<B>Desmaiado:</B> ").Append(r.CriminalKnockedOut ? "sim" : "não").Append("<BR>");
                        sb.Append("<B>Itens no quartel:</B> ").Append(r.LootStoredInBarracks ? "sim" : "não").Append("<BR>");
                        sb.Append("<B>Preso:</B> ").Append(r.SentToPrison ? "sim" : "não").Append("<BR>");

                        if (!String.IsNullOrWhiteSpace(r.Notes))
                            sb.Append("<B>Observação:</B> ").Append(r.Notes);
                    }
                }
            }

            else if (mode == 2)
            {
                List<ReinoPrisonRecord> list = GetPrisonList(cityId);
                if (list.Count <= 0)
                {
                    sb.Append("Nenhuma prisão registrada.");
                }
                else
                {
                    if (index < 0) index = 0;
                    if (index >= list.Count) index = list.Count - 1;
                    ReinoPrisonRecord r = list[index];
                    sb.Append("<B>Prisão ").Append(index + 1).Append(" de ").Append(list.Count).Append("</B><BR><BR>");
                    sb.Append("<B>Preso:</B> ").Append(r.PrisonerName).Append("<BR>");
                    sb.Append("<B>Quem prendeu:</B> ").Append(r.ArrestedBy).Append("<BR>");
                    sb.Append("<B>Crime:</B> ").Append(r.CrimeLabel).Append("<BR>");
                    sb.Append("<B>Entrada:</B> ").Append(r.ArrestUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                    sb.Append("<B>Saída:</B> ").Append(r.ReleaseUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                    sb.Append("<B>Pena:</B> ").Append(r.DurationHours).Append(" horas<BR>");
                    sb.Append("<B>Solto por:</B> ").Append(String.IsNullOrWhiteSpace(r.ReleasedBy) ? "ainda preso" : r.ReleasedBy).Append("<BR>");
                    if (!String.IsNullOrWhiteSpace(r.Notes))
                        sb.Append("<B>Observação:</B> ").Append(r.Notes);
                }
            }
            else if (mode == 3)
            {
                List<ReinoWantedEntry> list = GetWantedList(cityId);
                if (list.Count <= 0)
                    sb.Append("Nenhum procurado registrado.");
                else
                {
                    if (index < 0) index = 0;
                    if (index >= list.Count) index = list.Count - 1;
                    ReinoWantedEntry e = list[index];
                    sb.Append("<B>Procurado ").Append(index + 1).Append(" de ").Append(list.Count).Append("</B><BR><BR>");
                    sb.Append("<B>Nome:</B> ").Append(e.PlayerName).Append("<BR>");
                    sb.Append("<B>Ação:</B> ").Append(GetActionLabel(e.Action)).Append("<BR>");
                    sb.Append("<B>Adicionado por:</B> ").Append(e.AddedByName).Append("<BR>");
                    sb.Append("<B>Quando:</B> ").Append(e.AddedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                }
            }
            else if (mode == 4)
            {
                Dictionary<string, int> recurring = GetRecurringCriminals(cityId);
                if (recurring.Count <= 0)
                    sb.Append("Nenhum criminoso recorrente ainda.");
                else
                {
                    List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(recurring);
                    list.Sort(delegate (KeyValuePair<string, int> a, KeyValuePair<string, int> b) { return b.Value.CompareTo(a.Value); });
                    if (index < 0) index = 0;
                    if (index >= list.Count) index = list.Count - 1;
                    sb.Append("<B>Criminoso recorrente ").Append(index + 1).Append(" de ").Append(list.Count).Append("</B><BR><BR>");
                    sb.Append("<B>Nome:</B> ").Append(list[index].Key).Append("<BR>");
                    sb.Append("<B>Ocorrências:</B> ").Append(list[index].Value).Append("<BR>");
                    sb.Append("<B>Crimes:</B> ").Append(GetCrimeListForCriminal(cityId, list[index].Key));
                }
            }
            else
            {
                sb.Append("Selecione um dos tipos de relatório detalhado.");
                                }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        private static string GetCrimeListForCriminal(int cityId, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return "-";

            HashSet<string> labels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<ReinoCrimeRecord> list = GetCrimeList(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoCrimeRecord r = list[i];
                if (r == null || !String.Equals(r.CriminalName, name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (r.CriminalSerial == 0)
                    continue;

                labels.Add(GetLawLabel(r.Law));
            }

            if (labels.Count <= 0)
                return "-";

            return String.Join(", ", new List<string>(labels).ToArray());
        }

        public static int GetDetailCount(int cityId, int mode)
        {
            switch (mode)
            {
                case 1: return GetCrimeList(cityId).Count;
                case 2: return GetPrisonList(cityId).Count;
                case 3: return GetWantedList(cityId).Count;
                case 4: return GetRecurringCriminals(cityId).Count;
                default: return 0;
            }
        }
        private static bool HasReportDataToArchive(int cityId)
        {
            return GetCrimeList(cityId).Count > 0 || GetPrisonList(cityId).Count > 0 || !String.IsNullOrWhiteSpace(GetReportState(cityId).Summary);
        }

        private static ReinoArchivedReport BuildArchivedReportSnapshot(int cityId, string closedBy)
        {
            ReinoArchivedReport report = new ReinoArchivedReport();
            report.ClosedUtc = DateTime.UtcNow;
            report.ClosedBy = closedBy ?? String.Empty;
            report.SummaryHtml = GetReportsSummaryHtml(cityId);

            int count = GetDetailCount(cityId, 1);
            for (int i = 0; i < count; i++)
                report.CrimeDetails.Add(GetReportsDetailHtml(cityId, 1, i));

            count = GetDetailCount(cityId, 2);
            for (int i = 0; i < count; i++)
                report.PrisonDetails.Add(GetReportsDetailHtml(cityId, 2, i));

            count = GetDetailCount(cityId, 3);
            for (int i = 0; i < count; i++)
                report.WantedDetails.Add(GetReportsDetailHtml(cityId, 3, i));

            count = GetDetailCount(cityId, 4);
            for (int i = 0; i < count; i++)
                report.RecurringDetails.Add(GetReportsDetailHtml(cityId, 4, i));

            return report;
        }

        public static void ArchiveAndClearCurrentReport(int cityId, PlayerMobile closedBy)
        {
            if (cityId < 0)
                return;

            if (HasReportDataToArchive(cityId))
            {
                List<ReinoArchivedReport> archives = GetArchivedReports(cityId);
                archives.Insert(0, BuildArchivedReportSnapshot(cityId, closedBy != null ? closedBy.Name : String.Empty));
                if (archives.Count > 10)
                    archives.RemoveRange(10, archives.Count - 10);
            }

            ReinoMilitaryReportState st = GetReportState(cityId);
            st.LastDeliveredUtc = DateTime.UtcNow;
            st.LastDeliveredTo = closedBy != null ? closedBy.Name : String.Empty;
            st.LastDeliveredToSerial = closedBy != null ? closedBy.Serial.Value : 0;
            st.Summary = String.Empty;

            GetCrimeList(cityId).Clear();
            GetPrisonList(cityId).Clear();
        }

        public static int GetArchivedReportCount(int cityId)
        {
            return GetArchivedReports(cityId).Count;
        }

        public static string GetArchivedReportTitle(int cityId, int archiveIndex)
        {
            List<ReinoArchivedReport> list = GetArchivedReports(cityId);
            if (archiveIndex < 0 || archiveIndex >= list.Count)
                return "Relatorio Antigo";

            return "Relatorio " + (archiveIndex + 1) + ": " + list[archiveIndex].ClosedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        public static string GetArchivedReportListLabel(int cityId, int archiveIndex)
        {
            return GetArchivedReportTitle(cityId, archiveIndex);
        }

        public static string GetArchivedReportsSummaryHtml(int cityId, int archiveIndex)
        {
            List<ReinoArchivedReport> list = GetArchivedReports(cityId);
            if (archiveIndex < 0 || archiveIndex >= list.Count)
                return "<BASEFONT COLOR=#000000>Nenhum relatório antigo encontrado.</BASEFONT>";

            return list[archiveIndex].SummaryHtml ?? "<BASEFONT COLOR=#000000>Nenhum conteúdo.</BASEFONT>";
        }

        public static int GetArchivedDetailCount(int cityId, int archiveIndex, int mode)
        {
            List<ReinoArchivedReport> list = GetArchivedReports(cityId);
            if (archiveIndex < 0 || archiveIndex >= list.Count)
                return 0;

            ReinoArchivedReport r = list[archiveIndex];
            switch (mode)
            {
                case 1: return r.CrimeDetails != null ? r.CrimeDetails.Count : 0;
                case 2: return r.PrisonDetails != null ? r.PrisonDetails.Count : 0;
                case 3: return r.WantedDetails != null ? r.WantedDetails.Count : 0;
                case 4: return r.RecurringDetails != null ? r.RecurringDetails.Count : 0;
                default: return 0;
            }
        }

        public static string GetArchivedReportDetailHtml(int cityId, int archiveIndex, int mode, int detailIndex)
        {
            List<ReinoArchivedReport> list = GetArchivedReports(cityId);
            if (archiveIndex < 0 || archiveIndex >= list.Count)
                return "<BASEFONT COLOR=#000000>Nenhum relatório antigo encontrado.</BASEFONT>";

            List<string> source;
            ReinoArchivedReport r = list[archiveIndex];
            switch (mode)
            {
                case 1: source = r.CrimeDetails; break;
                case 2: source = r.PrisonDetails; break;
                case 3: source = r.WantedDetails; break;
                case 4: source = r.RecurringDetails; break;
                default: source = null; break;
            }

            if (source == null || source.Count == 0)
                return "<BASEFONT COLOR=#000000>Nenhum registro nessa seção.</BASEFONT>";

            if (detailIndex < 0) detailIndex = 0;
            if (detailIndex >= source.Count) detailIndex = source.Count - 1;
            return source[detailIndex] ?? "<BASEFONT COLOR=#000000>Nenhum conteúdo.</BASEFONT>";
        }

        private static string StripHtmlForBook(string html)
        {
            if (String.IsNullOrEmpty(html))
                return String.Empty;

            string source = html.Replace("<BR>", "\n").Replace("<br>", "\n").Replace("<BR/>", "\n").Replace("<br/>", "\n");
            StringBuilder sb = new StringBuilder(source.Length);
            bool inside = false;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (c == '<')
                {
                    inside = true;
                    continue;
                }
                if (c == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                    sb.Append(c);
            }

            return sb.ToString().Replace("&nbsp;", " ").Replace("&amp;", "&");
        }

        private static List<string> WrapPlainTextLines(string text, int maxChars)
        {
            List<string> lines = new List<string>();
            if (String.IsNullOrWhiteSpace(text))
            {
                lines.Add(String.Empty);
                return lines;
            }

            string[] sourceLines = text.Replace("\r", String.Empty).Split('\n');
            for (int i = 0; i < sourceLines.Length; i++)
            {
                string remaining = (sourceLines[i] ?? String.Empty).Trim();
                if (remaining.Length == 0)
                {
                    lines.Add(String.Empty);
                    continue;
                }

                while (remaining.Length > maxChars)
                {
                    int split = remaining.LastIndexOf(' ', Math.Min(maxChars, remaining.Length - 1));
                    if (split <= 0)
                        split = maxChars;

                    string piece = remaining.Substring(0, split).Trim();
                    if (piece.Length == 0)
                        piece = remaining.Substring(0, Math.Min(maxChars, remaining.Length));

                    lines.Add(piece);
                    remaining = remaining.Substring(Math.Min(split, remaining.Length)).TrimStart();
                }

                lines.Add(remaining);
            }

            return lines;
        }

        private static string PrepareBookPageHtml(string html)
        {
            string plain = StripHtmlForBook(html);
            StringBuilder sb = new StringBuilder();
            List<string> lines = WrapPlainTextLines(plain, 20);
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0)
                    sb.Append("<BR>");
                sb.Append(lines[i]);
            }
            return sb.ToString();
        }

        private static bool IsBlankHtmlBook(HtmlBook30 book)
        {
            if (book == null || book.Deleted)
                return false;

            if (book.IsSealed)
                return false;

            if (!String.IsNullOrWhiteSpace(book.DocumentTitle))
                return false;

            if (book.GetWrittenPageCount() > 1)
                return false;

            string page0 = StripHtmlForBook(book.GetPageHtml(0));
            return String.IsNullOrWhiteSpace(page0);
        }

        private static void ClearHtmlBook(HtmlBook30 book)
        {
            if (book == null || book.Deleted)
                return;

            for (int i = 0; i < book.PageCount; i++)
                book.SetPageHtml(i, String.Empty);
        }

        private static void FillReportBook(HtmlBook30 book, string title, string summaryHtml, List<string> crimePages, List<string> prisonPages, List<string> wantedPages, List<string> recurringPages)
        {
            if (book == null || book.Deleted)
                return;

            ClearHtmlBook(book);
            book.Name = title;
            book.DocumentTitle = title;
            book.Language = OSULanguage.Common;
            book.SetPageHtml(0, PrepareBookPageHtml(summaryHtml));

            int page = 1;
            if (crimePages != null)
                for (int i = 0; i < crimePages.Count && page < book.PageCount; i++, page++)
                    book.SetPageHtml(page, PrepareBookPageHtml(crimePages[i]));

            if (prisonPages != null)
                for (int i = 0; i < prisonPages.Count && page < book.PageCount; i++, page++)
                    book.SetPageHtml(page, PrepareBookPageHtml(prisonPages[i]));

            if (wantedPages != null)
                for (int i = 0; i < wantedPages.Count && page < book.PageCount; i++, page++)
                    book.SetPageHtml(page, PrepareBookPageHtml(wantedPages[i]));

            if (recurringPages != null)
                for (int i = 0; i < recurringPages.Count && page < book.PageCount; i++, page++)
                    book.SetPageHtml(page, PrepareBookPageHtml(recurringPages[i]));

            book.ForceSealAsCopy("Ofício Militar", 0);
        }

        public static void BeginPrintReportBook(PlayerMobile from, int cityId)
        {
            if (from == null || from.Deleted)
                return;

            if (!LanguageKnowledge.Understands(from, OSULanguage.Common))
            {
                from.SendMessage("Você precisa falar a língua comum para copiar esse relatório para um livro.");
                return;
            }

            from.SendMessage("Selecione um livro HTML de 30 páginas em branco.");
            from.Target = new ReinoMilitaryReportBookTarget(cityId, -1);
        }

        public static void BeginPrintArchivedReportBook(PlayerMobile from, int cityId, int archiveIndex)
        {
            if (from == null || from.Deleted)
                return;

            if (!LanguageKnowledge.Understands(from, OSULanguage.Common))
            {
                from.SendMessage("Você precisa falar a língua comum para copiar esse relatório para um livro.");
                return;
            }

            from.SendMessage("Selecione um livro HTML de 30 páginas em branco.");
            from.Target = new ReinoMilitaryReportBookTarget(cityId, archiveIndex);
        }

        private sealed class ReinoMilitaryReportBookTarget : Target
        {
            private readonly int m_CityId;
            private readonly int m_ArchiveIndex;

            public ReinoMilitaryReportBookTarget(int cityId, int archiveIndex)
                : base(12, false, TargetFlags.None)
            {
                m_CityId = cityId;
                m_ArchiveIndex = archiveIndex;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                HtmlBook30 book = targeted as HtmlBook30;

                if (pm == null || pm.Deleted)
                    return;

                if (book == null || book.Deleted || book.RootParent != pm)
                {
                    pm.SendMessage("Você precisa selecionar um livro HTML de 30 páginas na sua mochila.");
                    return;
                }

                if (!IsBlankHtmlBook(book))
                {
                    pm.SendMessage("Esse livro não está em branco.");
                    return;
                }

                pm.Frozen = true;
                Timer.DelayCall(TimeSpan.FromSeconds(5.0), delegate
                {
                    if (pm != null && !pm.Deleted)
                        pm.Frozen = false;
                });

                if (m_ArchiveIndex >= 0)
                {
                    List<ReinoArchivedReport> archives = GetArchivedReports(m_CityId);
                    if (m_ArchiveIndex < 0 || m_ArchiveIndex >= archives.Count)
                    {
                        pm.SendMessage("Esse relatório antigo não existe mais.");
                        return;
                    }

                    ReinoArchivedReport archived = archives[m_ArchiveIndex];
                    string title = "relatórios de " + archived.ClosedUtc.ToLocalTime().ToString("dd-MM-yyyy HH-mm");
                    FillReportBook(book, title, archived.SummaryHtml, archived.CrimeDetails, archived.PrisonDetails, archived.WantedDetails, archived.RecurringDetails);
                    pm.SendMessage("O relatório antigo foi copiado para o livro.");
                }
                else
                {
                    string title = "relatórios de " + DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH-mm");
                    List<string> crimePages = new List<string>();
                    List<string> prisonPages = new List<string>();
                    List<string> wantedPages = new List<string>();
                    List<string> recurringPages = new List<string>();

                    int count = GetDetailCount(m_CityId, 1);
                    for (int i = 0; i < count; i++)
                        crimePages.Add(GetReportsDetailHtml(m_CityId, 1, i));

                    count = GetDetailCount(m_CityId, 2);
                    for (int i = 0; i < count; i++)
                        prisonPages.Add(GetReportsDetailHtml(m_CityId, 2, i));

                    count = GetDetailCount(m_CityId, 3);
                    for (int i = 0; i < count; i++)
                        wantedPages.Add(GetReportsDetailHtml(m_CityId, 3, i));

                    count = GetDetailCount(m_CityId, 4);
                    for (int i = 0; i < count; i++)
                        recurringPages.Add(GetReportsDetailHtml(m_CityId, 4, i));

                    FillReportBook(book, title, GetReportsSummaryHtml(m_CityId), crimePages, prisonPages, wantedPages, recurringPages);
                    pm.SendMessage("O relatório foi copiado para o livro.");
                }
            }
        }


        public static void DeletePostWorldObjects(ReinoGuardPostInfo post)
        {
            if (post == null)
                return;

            Item marker = FindItem(post.MarkerSerial);
            if (marker != null)
                marker.Delete();

            OSUCityGuard guard = FindGuard(post);
            if (guard != null)
                guard.Delete();

            if (post.RouteRootSerial != 0)
            {
                WayPoint point = FindItem(post.RouteRootSerial) as WayPoint;
                if (point != null)
                    point.Delete();
            }

            post.MarkerSerial = 0;
            post.GuardSerial = 0;
            post.RouteRootSerial = 0;
        }

        public static Item FindItem(int serial)
        {
            Item item;
            if (serial != 0 && World.Items.TryGetValue(serial, out item))
                return item;
            return null;
        }

        private static Map GetMapByIndex(int index)
        {
            return Map.Maps != null && index >= 0 && index < Map.Maps.Length ? Map.Maps[index] : Map.Felucca;
        }

        public static string CreateRoutePoint(PlayerMobile from, int cityId)
        {
            if (from == null || from.Deleted || from.Map == null)
                return "Jogador inválido.";

            int resolved;
            if (!TryResolveCityIdAt(from.Location, from.Map, ReinoAreaScope.Total, out resolved) || resolved != cityId)
                return "Pontos de rota só podem ser criados dentro da área total do reino.";

            ReinoMilitaryRoutePoint point = new ReinoMilitaryRoutePoint(cityId);
            point.MoveToWorld(from.Location, from.Map);
            point.MakeVisibleFor(TimeSpan.FromMinutes(5.0));

            GetSession(from).PendingRouteRootSerial = point.Serial.Value;
            return "Ponto de rota criado. Ele ficará visível por 5 minutos e depois se tornará invisível até ser revelado.";
        }

        public static string RemoveRoutePoint(PlayerMobile from, int cityId)
        {
            if (from == null || from.Deleted || from.Map == null)
                return "Jogador inválido.";

            ReinoMilitaryRoutePoint point = FindRoutePointAt(from.Location, from.Map, cityId);
            if (point == null)
                return "Não existe ponto de rota nesse tile.";

            if (point.ClosedRoute || point.PostId > 0)
                return "Esse ponto faz parte de uma rota fechada. Use resetar rota para desfazer a rota inteira.";

            point.Delete();
            return "Ponto de rota removido.";
        }

        public static ReinoMilitaryRoutePoint FindRoutePointAt(Point3D loc, Map map, int cityId)
        {
            IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
            foreach (Item item in eable)
            {
                ReinoMilitaryRoutePoint point = item as ReinoMilitaryRoutePoint;
                if (point != null && point.CityId == cityId && point.X == loc.X && point.Y == loc.Y)
                {
                    eable.Free();
                    return point;
                }
            }
            eable.Free();
            return null;
        }

        public static string LinkRoutePoint(PlayerMobile from, int cityId)
        {
            if (from == null || from.Deleted)
                return "Jogador inválido.";

            ReinoMilitarySession session = GetSession(from);
            session.PendingRouteRootSerial = 0;
            session.PendingRouteLinkSerial = 0;
            from.Target = new ReinoMilitaryRouteLinkTarget(cityId);

            return "Selecione o primeiro ponto de rota. Continue selecionando os próximos pontos e termine clicando no ponto de guarda.";
        }

        public static string CancelPendingRouteLink(PlayerMobile from)
        {
            if (from == null || from.Deleted)
                return "Jogador inválido.";

            ReinoMilitarySession session = GetSession(from);
            session.PendingRouteRootSerial = 0;
            session.PendingRouteLinkSerial = 0;
            return "Ligação de rota cancelada.";
        }

        public static string LinkRouteToGuardPost(PlayerMobile from, int cityId)
        {
            return "Agora a rota é fechada no próprio target. Aperte ligar pontos de rota e termine clicando no ponto de guarda.";
        }

        private static void RetargetRouteLink(PlayerMobile from, int cityId)
        {
            if (from != null && !from.Deleted)
                from.Target = new ReinoMilitaryRouteLinkTarget(cityId);
        }

        private static string ClosePendingRouteOnGuardPost(PlayerMobile from, int cityId, ReinoGuardPostInfo post)
        {
            if (post == null)
                return "Ponto de guarda inválido.";

            ReinoMilitarySession session = GetSession(from);
            if (session.PendingRouteRootSerial == 0)
            {
                RetargetRouteLink(from, cityId);
                return "Selecione antes o primeiro ponto de rota.";
            }

            if (post.RouteRootSerial != 0)
            {
                RetargetRouteLink(from, cityId);
                return "Esse ponto de guarda já tem uma rota fechada. Use resetar rota primeiro.";
            }

            WayPoint root = FindItem(session.PendingRouteRootSerial) as WayPoint;
            if (root == null)
            {
                session.PendingRouteRootSerial = 0;
                session.PendingRouteLinkSerial = 0;
                return "O primeiro ponto de rota não existe mais.";
            }

            WayPoint last = root;
            int hops = 0;
            while (last.NextPoint != null && hops++ < 512)
                last = last.NextPoint;

            WayPoint oldHome = FindItem(post.RouteHomeSerial) as WayPoint;
            if (oldHome != null && !oldHome.Deleted)
                oldHome.Delete();

            WayPoint home = new WayPoint();
            home.Movable = false;
            home.Visible = false;
            home.MoveToWorld(post.Location, from.Map);
            last.NextPoint = home;

            int hue = GetNextRouteHue(cityId);
            PaintRouteChain(root, post.Id, hue);

            ReinoGuardPostMarker marker = FindItem(post.MarkerSerial) as ReinoGuardPostMarker;
            if (marker != null)
            {
                marker.Hue = hue;
                marker.PostId = post.Id;
            }

            post.RouteRootSerial = root.Serial.Value;
            post.RouteHomeSerial = home.Serial.Value;
            post.RouteColorHue = hue;
            post.RouteActivated = false;
            post.LastRouteUtc = DateTime.MinValue;

            session.PendingRouteRootSerial = 0;
            session.PendingRouteLinkSerial = 0;
            return "Rota ligada ao ponto de guarda. Agora acione a rota quando quiser começar.";
        }

        private sealed class ReinoMilitaryRouteLinkTarget : Target
        {
            private readonly int m_CityId;

            public ReinoMilitaryRouteLinkTarget(int cityId)
                : base(12, false, TargetFlags.None)
            {
                m_CityId = cityId;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || pm.Deleted)
                    return;

                ReinoMilitarySession session = GetSession(pm);

                ReinoMilitaryRoutePoint point = targeted as ReinoMilitaryRoutePoint;
                if (point != null)
                {
                    if (point.Deleted || point.CityId != m_CityId)
                    {
                        RetargetRouteLink(pm, m_CityId);
                        pm.SendMessage("Selecione um ponto de rota desse reino.");
                        return;
                    }

                    if (point.ClosedRoute || point.PostId > 0)
                    {
                        RetargetRouteLink(pm, m_CityId);
                        pm.SendMessage("Esse ponto já faz parte de uma rota fechada.");
                        return;
                    }

                    if (session.PendingRouteRootSerial == 0)
                    {
                        session.PendingRouteRootSerial = point.Serial.Value;
                        session.PendingRouteLinkSerial = point.Serial.Value;
                        RetargetRouteLink(pm, m_CityId);
                        pm.SendMessage("Primeiro ponto selecionado. Agora selecione o próximo ponto de rota ou o ponto de guarda para fechar.");
                        return;
                    }

                    if (session.PendingRouteLinkSerial == point.Serial.Value)
                    {
                        RetargetRouteLink(pm, m_CityId);
                        pm.SendMessage("Esse já é o ponto selecionado.");
                        return;
                    }

                    WayPoint last = FindItem(session.PendingRouteLinkSerial) as WayPoint;
                    if (last == null)
                    {
                        session.PendingRouteRootSerial = 0;
                        session.PendingRouteLinkSerial = 0;
                        RetargetRouteLink(pm, m_CityId);
                        pm.SendMessage("O último ponto selecionado sumiu. Comece de novo.");
                        return;
                    }

                    last.NextPoint = point;
                    session.PendingRouteLinkSerial = point.Serial.Value;
                    RetargetRouteLink(pm, m_CityId);
                    pm.SendMessage("Ponto ligado. Selecione o próximo ponto de rota ou o ponto de guarda para fechar.");
                    return;
                }

                ReinoGuardPostMarker marker = targeted as ReinoGuardPostMarker;
                if (marker != null)
                {
                    if (marker.Deleted || marker.CityId != m_CityId)
                    {
                        RetargetRouteLink(pm, m_CityId);
                        pm.SendMessage("Selecione um ponto de guarda desse reino.");
                        return;
                    }

                    ReinoGuardPostInfo post = FindPostById(m_CityId, marker.PostId);
                    pm.SendMessage(ClosePendingRouteOnGuardPost(pm, m_CityId, post));
                    return;
                }

                RetargetRouteLink(pm, m_CityId);
                pm.SendMessage("Selecione um ponto de rota. Para fechar a rota, selecione um ponto de guarda.");
            }
        }



        public static string ActivateRouteAtCurrentPoint(PlayerMobile from, int cityId)
        {
            ReinoGuardPostInfo linkedPost = FindLinkedPostForSelectedRoute(from, cityId);
            if (linkedPost == null)
                return "Fique sobre o primeiro ponto de uma rota ligada para acioná-la.";

            linkedPost.RouteActivated = true;
            linkedPost.LastRouteUtc = DateTime.MinValue;
            return "A rota foi acionada e o guarda passará a obedecer ao agendamento configurado.";
        }

        public static string RevealRoutePoints(PlayerMobile from, int cityId)
        {
            if (from == null || from.Deleted || from.Map == null)
                return "Jogador inválido.";

            ReinoMilitarySession session = GetSession(from);
            bool show = !session.RoutePointsVisible;
            session.RoutePointsVisible = show;

            int changed = 0;
            IPooledEnumerable eable = from.Map.GetItemsInRange(from.Location, 15);
            foreach (Item item in eable)
            {
                ReinoMilitaryRoutePoint point = item as ReinoMilitaryRoutePoint;
                if (point != null && point.CityId == cityId)
                {
                    point.SetTemporaryVisible(show, show ? (TimeSpan?)TimeSpan.FromMinutes(1.0) : null);
                    changed++;
                    continue;
                }

                ReinoGuardPostMarker marker = item as ReinoGuardPostMarker;
                if (marker != null && marker.CityId == cityId)
                {
                    marker.SetTemporaryVisible(show, show ? (TimeSpan?)TimeSpan.FromMinutes(1.0) : null);
                    changed++;
                }
            }
            eable.Free();

            if (changed <= 0)
                return "Nenhum ponto de rota ou de guarda foi encontrado num raio de 15 tiles.";

            return show ? "Os pontos próximos ficaram visíveis por 1 minuto." : "Os pontos próximos voltaram a ficar invisíveis.";
        }

        public static string SetRouteSpeedAtCurrentPoint(PlayerMobile from, int cityId, ReinoRouteSpeed speed)
        {
            ReinoGuardPostInfo linkedPost = FindLinkedPostForSelectedRoute(from, cityId);
            if (linkedPost == null)
                return "Selecione primeiro uma rota ligada a um ponto de guarda.";

            linkedPost.RouteSpeed = speed;
            return "Velocidade da rota ajustada para " + GetRouteSpeedLabel(speed).ToLower() + ".";
        }

        public static string SetRouteScheduleAtCurrentPoint(PlayerMobile from, int cityId, ReinoRouteSchedule schedule)
        {
            ReinoGuardPostInfo linkedPost = FindLinkedPostForSelectedRoute(from, cityId);
            if (linkedPost == null)
                return "Selecione primeiro uma rota ligada a um ponto de guarda.";

            linkedPost.RouteSchedule = schedule;
            return "Agendamento da rota ajustado para " + GetRouteScheduleLabel(schedule).ToLower() + ".";
        }

        public static string ResetRouteAtCurrentPoint(PlayerMobile from, int cityId)
        {
            ReinoGuardPostInfo linkedPost = FindLinkedPostForSelectedRoute(from, cityId);
            if (linkedPost == null)
                return "Fique sobre um ponto da rota que você quer desfazer.";

            DeleteRouteChain(linkedPost);
            linkedPost.RouteRootSerial = 0;
            linkedPost.RouteHomeSerial = 0;
            linkedPost.RouteColorHue = 0;
            linkedPost.RouteActivated = false;
            linkedPost.LastRouteUtc = DateTime.MinValue;
            return "A rota inteira foi desfeita.";
        }

        public static string ResetRouteConfigAtCurrentPoint(PlayerMobile from, int cityId)
        {
            ReinoGuardPostInfo linkedPost = FindLinkedPostForSelectedRoute(from, cityId);
            if (linkedPost == null)
                return "Selecione primeiro uma rota ligada a um ponto de guarda.";

            linkedPost.RouteSpeed = ReinoRouteSpeed.Short;
            linkedPost.RouteSchedule = ReinoRouteSchedule.Infinite;
            return "A configuração da rota voltou para o padrão: tempo curto e rota infinita.";
        }

        private static ReinoGuardPostInfo FindLinkedPostForSelectedRoute(PlayerMobile from, int cityId)
        {
            ReinoMilitaryRoutePoint point = FindRoutePointAt(from.Location, from.Map, cityId);
            if (point == null)
                return null;

            List<ReinoGuardPostInfo> list = GetPosts(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoGuardPostInfo post = list[i];
                if (post == null || post.RouteRootSerial == 0)
                    continue;

                WayPoint current = FindItem(post.RouteRootSerial) as WayPoint;
                int guard = 0;
                while (current != null && guard++ < 512)
                {
                    if (current.Serial.Value == point.Serial.Value)
                        return post;

                    current = current.NextPoint;
                }
            }

            return null;
        }


        private static readonly int[] RouteHues = new int[] { 0x44E, 0x489, 0x4F2, 0x58C, 0x66D, 0x47E, 0x53D, 0x83F, 0x8A5, 0x90F };

        private static int GetNextRouteHue(int cityId)
        {
            HashSet<int> used = new HashSet<int>();
            List<ReinoGuardPostInfo> list = GetPosts(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].RouteColorHue > 0)
                    used.Add(list[i].RouteColorHue);
            }

            for (int i = 0; i < RouteHues.Length; i++)
                if (!used.Contains(RouteHues[i]))
                    return RouteHues[i];

            return RouteHues[Utility.Random(RouteHues.Length)];
        }

        private static void PaintRouteChain(WayPoint root, int postId, int hue)
        {
            WayPoint current = root;
            int guard = 0;
            while (current != null && guard++ < 512)
            {
                ReinoMilitaryRoutePoint point = current as ReinoMilitaryRoutePoint;
                if (point != null)
                {
                    point.PostId = postId;
                    point.RouteHue = hue;
                    point.ClosedRoute = true;
                }

                current = current.NextPoint;
            }
        }

        private static void DeleteRouteChain(ReinoGuardPostInfo post)
        {
            if (post == null || post.RouteRootSerial == 0)
                return;

            HashSet<int> visited = new HashSet<int>();
            WayPoint current = FindItem(post.RouteRootSerial) as WayPoint;

            while (current != null && !visited.Contains(current.Serial.Value))
            {
                visited.Add(current.Serial.Value);
                WayPoint next = current.NextPoint;
                current.Delete();
                current = next;
            }

            if (post.RouteHomeSerial != 0)
            {
                Item home = FindItem(post.RouteHomeSerial);
                if (home != null && !home.Deleted)
                    home.Delete();
            }

            ReinoGuardPostMarker marker = FindItem(post.MarkerSerial) as ReinoGuardPostMarker;
            if (marker != null)
                marker.Hue = 0x44E;
        }

        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(4);

                bw.Write(m_Policies.Count);
                foreach (KeyValuePair<int, ReinoMilitaryPolicy> kv in m_Policies)
                {
                    bw.Write(kv.Key);
                    bw.Write((int)kv.Value.WantedDefaultAction);
                    bw.Write((int)kv.Value.CrimeDefaultAction);
                    bw.Write(kv.Value.EnabledLaws.Count);
                    foreach (ReinoMilitaryLaw law in kv.Value.EnabledLaws)
                        bw.Write((int)law);
                }

                bw.Write(m_Wanted.Count);
                foreach (KeyValuePair<int, List<ReinoWantedEntry>> kv in m_Wanted)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoWantedEntry e = kv.Value[i];
                        bw.Write(e.PlayerName ?? String.Empty);
                        bw.Write((int)e.Action);
                        bw.Write(e.AddedUtc.ToBinary());
                        bw.Write(e.AddedBySerial);
                        bw.Write(e.AddedByName ?? String.Empty);
                    }
                }

                bw.Write(m_Crimes.Count);
                foreach (KeyValuePair<int, List<ReinoCrimeRecord>> kv in m_Crimes)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoCrimeRecord r = kv.Value[i];
                        bw.Write(r.CityId);
                        bw.Write(r.CriminalSerial);
                        bw.Write(r.CriminalName ?? String.Empty);
                        bw.Write((int)r.Law);
                        bw.Write(r.Utc.ToBinary());
                        bw.Write(r.WitnessGuardSerial);
                        bw.Write(r.WitnessGuardName ?? String.Empty);
                        bw.Write((int)r.Result);
                        bw.Write(r.GuardDied);
                        bw.Write(r.CriminalDied);
                        bw.Write(r.CriminalKnockedOut);
                        bw.Write(r.LootStoredInBarracks);
                        bw.Write(r.SentToPrison);
                        bw.Write(r.Notes ?? String.Empty);
                    }
                }

                bw.Write(m_Prisons.Count);
                foreach (KeyValuePair<int, List<ReinoPrisonRecord>> kv in m_Prisons)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoPrisonRecord r = kv.Value[i];
                        bw.Write(r.CityId);
                        bw.Write(r.PrisonerSerial);
                        bw.Write(r.PrisonerName ?? String.Empty);
                        bw.Write(r.ArrestedBy ?? String.Empty);
                        bw.Write(r.CrimeLabel ?? String.Empty);
                        bw.Write(r.ArrestUtc.ToBinary());
                        bw.Write(r.ReleaseUtc.ToBinary());
                        bw.Write(r.DurationHours);
                        bw.Write(r.ReleasedBy ?? String.Empty);
                        bw.Write(r.ReleasedEarly);
                        bw.Write(r.Notes ?? String.Empty);
                    }
                }

                bw.Write(m_ReportStates.Count);
                foreach (KeyValuePair<int, ReinoMilitaryReportState> kv in m_ReportStates)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.LastDeliveredUtc.ToBinary());
                    bw.Write(kv.Value.LastDeliveredTo ?? String.Empty);
                    bw.Write(kv.Value.LastDeliveredToSerial);
                    bw.Write(kv.Value.Summary ?? String.Empty);
                }

                bw.Write(m_ArchivedReportsByCity.Count);
                foreach (KeyValuePair<int, List<ReinoArchivedReport>> kv in m_ArchivedReportsByCity)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoArchivedReport r = kv.Value[i];
                        bw.Write(r.ClosedUtc.ToBinary());
                        bw.Write(r.ClosedBy ?? String.Empty);
                        bw.Write(r.SummaryHtml ?? String.Empty);

                        bw.Write(r.CrimeDetails != null ? r.CrimeDetails.Count : 0);
                        if (r.CrimeDetails != null)
                            for (int y = 0; y < r.CrimeDetails.Count; y++)
                                bw.Write(r.CrimeDetails[y] ?? String.Empty);

                        bw.Write(r.PrisonDetails != null ? r.PrisonDetails.Count : 0);
                        if (r.PrisonDetails != null)
                            for (int y = 0; y < r.PrisonDetails.Count; y++)
                                bw.Write(r.PrisonDetails[y] ?? String.Empty);

                        bw.Write(r.WantedDetails != null ? r.WantedDetails.Count : 0);
                        if (r.WantedDetails != null)
                            for (int y = 0; y < r.WantedDetails.Count; y++)
                                bw.Write(r.WantedDetails[y] ?? String.Empty);

                        bw.Write(r.RecurringDetails != null ? r.RecurringDetails.Count : 0);
                        if (r.RecurringDetails != null)
                            for (int y = 0; y < r.RecurringDetails.Count; y++)
                                bw.Write(r.RecurringDetails[y] ?? String.Empty);
                    }
                }

                bw.Write(m_PostsByCity.Count);
                foreach (KeyValuePair<int, List<ReinoGuardPostInfo>> kv in m_PostsByCity)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoGuardPostInfo p = kv.Value[i];
                        bw.Write(p.Id);
                        bw.Write(p.CityId);
                        bw.Write(p.ConstructionKey ?? String.Empty);
                        bw.Write(p.Location.X);
                        bw.Write(p.Location.Y);
                        bw.Write(p.Location.Z);
                        bw.Write(p.MapIndex);
                        bw.Write(p.MarkerSerial);
                        bw.Write(p.GuardSerial);
                        bw.Write((int)p.GuardKind);
                        bw.Write(p.Level);
                        bw.Write(p.Facing);
                        bw.Write(p.Uniformized);
                        bw.Write(p.RouteRootSerial);
                        bw.Write(p.RouteHomeSerial);
                        bw.Write(p.RouteColorHue);
                        bw.Write(p.RouteActivated);
                        bw.Write((int)p.RouteSchedule);
                        bw.Write((int)p.RouteSpeed);
                        bw.Write(p.LastRouteUtc.ToBinary());
                        bw.Write(p.Training);
                        bw.Write(p.TrainingEndsUtc.ToBinary());
                        bw.Write(p.Active);
                    }
                }

                bw.Write(m_AutoSheathe.Count);
                foreach (int serial in m_AutoSheathe)
                    bw.Write(serial);

                bw.Write(m_PendingLawNoticesByPlayer.Count);
                foreach (KeyValuePair<int, List<string>> kv in m_PendingLawNoticesByPlayer)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value != null ? kv.Value.Count : 0);
                    if (kv.Value != null)
                        for (int i = 0; i < kv.Value.Count; i++)
                            bw.Write(kv.Value[i] ?? String.Empty);
                }

                bw.Write(m_NextPostId);
            }
        }

        public static void Load()
        {
            m_Policies.Clear();
            m_Wanted.Clear();
            m_Crimes.Clear();
            m_Prisons.Clear();
            m_ReportStates.Clear();
            m_ArchivedReportsByCity.Clear();
            m_PostsByCity.Clear();
            m_AutoSheathe.Clear();
            m_PendingLawNoticesByPlayer.Clear();
            m_NextPostId = 1;

            if (!File.Exists(FilePath))
                return;

            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                int version = br.ReadInt32();
                if (version < 0)
                    return;

                int count;
                int listCount;
                int cityId;

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    cityId = br.ReadInt32();
                    ReinoMilitaryPolicy p = new ReinoMilitaryPolicy();
                    p.WantedDefaultAction = (ReinoGuardAction)br.ReadInt32();
                    p.CrimeDefaultAction = (ReinoGuardAction)br.ReadInt32();
                    listCount = br.ReadInt32();
                    for (int x = 0; x < listCount; x++)
                        p.EnabledLaws.Add((ReinoMilitaryLaw)br.ReadInt32());
                    m_Policies[cityId] = p;
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    cityId = br.ReadInt32();
                    listCount = br.ReadInt32();
                    List<ReinoWantedEntry> list = new List<ReinoWantedEntry>();
                    for (int x = 0; x < listCount; x++)
                    {
                        list.Add(new ReinoWantedEntry
                        {
                            PlayerName = br.ReadString(),
                            Action = (ReinoGuardAction)br.ReadInt32(),
                            AddedUtc = DateTime.FromBinary(br.ReadInt64()),
                            AddedBySerial = br.ReadInt32(),
                            AddedByName = br.ReadString()
                        });
                    }
                    m_Wanted[cityId] = list;
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    cityId = br.ReadInt32();
                    listCount = br.ReadInt32();
                    List<ReinoCrimeRecord> list = new List<ReinoCrimeRecord>();
                    for (int x = 0; x < listCount; x++)
                    {
                        list.Add(new ReinoCrimeRecord
                        {
                            CityId = br.ReadInt32(),
                            CriminalSerial = br.ReadInt32(),
                            CriminalName = br.ReadString(),
                            Law = (ReinoMilitaryLaw)br.ReadInt32(),
                            Utc = DateTime.FromBinary(br.ReadInt64()),
                            WitnessGuardSerial = br.ReadInt32(),
                            WitnessGuardName = br.ReadString(),
                            Result = (ReinoGuardAction)br.ReadInt32(),
                            GuardDied = br.ReadBoolean(),
                            CriminalDied = br.ReadBoolean(),
                            CriminalKnockedOut = br.ReadBoolean(),
                            LootStoredInBarracks = br.ReadBoolean(),
                            SentToPrison = br.ReadBoolean(),
                            Notes = br.ReadString()
                        });
                    }
                    m_Crimes[cityId] = list;
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    cityId = br.ReadInt32();
                    listCount = br.ReadInt32();
                    List<ReinoPrisonRecord> list = new List<ReinoPrisonRecord>();
                    for (int x = 0; x < listCount; x++)
                    {
                        list.Add(new ReinoPrisonRecord
                        {
                            CityId = br.ReadInt32(),
                            PrisonerSerial = br.ReadInt32(),
                            PrisonerName = br.ReadString(),
                            ArrestedBy = br.ReadString(),
                            CrimeLabel = br.ReadString(),
                            ArrestUtc = DateTime.FromBinary(br.ReadInt64()),
                            ReleaseUtc = DateTime.FromBinary(br.ReadInt64()),
                            DurationHours = br.ReadInt32(),
                            ReleasedBy = br.ReadString(),
                            ReleasedEarly = br.ReadBoolean(),
                            Notes = br.ReadString()
                        });
                    }
                    m_Prisons[cityId] = list;
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    cityId = br.ReadInt32();
                    ReinoMilitaryReportState st = new ReinoMilitaryReportState();
                    st.LastDeliveredUtc = DateTime.FromBinary(br.ReadInt64());
                    st.LastDeliveredTo = br.ReadString();
                    st.LastDeliveredToSerial = br.ReadInt32();
                    st.Summary = version >= 3 ? br.ReadString() : String.Empty;
                    m_ReportStates[cityId] = st;
                }

                if (version >= 4)
                {
                    count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        cityId = br.ReadInt32();
                        listCount = br.ReadInt32();
                        List<ReinoArchivedReport> list = new List<ReinoArchivedReport>();
                        for (int x = 0; x < listCount; x++)
                        {
                            ReinoArchivedReport r = new ReinoArchivedReport();
                            r.ClosedUtc = DateTime.FromBinary(br.ReadInt64());
                            r.ClosedBy = br.ReadString();
                            r.SummaryHtml = br.ReadString();

                            int inner = br.ReadInt32();
                            for (int y = 0; y < inner; y++)
                                r.CrimeDetails.Add(br.ReadString());

                            inner = br.ReadInt32();
                            for (int y = 0; y < inner; y++)
                                r.PrisonDetails.Add(br.ReadString());

                            inner = br.ReadInt32();
                            for (int y = 0; y < inner; y++)
                                r.WantedDetails.Add(br.ReadString());

                            inner = br.ReadInt32();
                            for (int y = 0; y < inner; y++)
                                r.RecurringDetails.Add(br.ReadString());

                            list.Add(r);
                        }
                        m_ArchivedReportsByCity[cityId] = list;
                    }
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    cityId = br.ReadInt32();
                    listCount = br.ReadInt32();
                    List<ReinoGuardPostInfo> list = new List<ReinoGuardPostInfo>();
                    for (int x = 0; x < listCount; x++)
                    {
                        ReinoGuardPostInfo p = new ReinoGuardPostInfo();
                        p.Id = br.ReadInt32();
                        p.CityId = br.ReadInt32();
                        p.ConstructionKey = br.ReadString();
                        p.Location = new Point3D(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                        p.MapIndex = br.ReadInt32();
                        p.MarkerSerial = br.ReadInt32();
                        p.GuardSerial = br.ReadInt32();
                        p.GuardKind = (ReinoGuardKind)br.ReadInt32();
                        p.Level = br.ReadInt32();
                        p.Facing = br.ReadInt32();
                        p.Uniformized = br.ReadBoolean();
                        p.RouteRootSerial = br.ReadInt32();
                        p.RouteHomeSerial = version >= 2 ? br.ReadInt32() : 0;
                        p.RouteColorHue = version >= 2 ? br.ReadInt32() : 0;
                        p.RouteActivated = version >= 2 && br.ReadBoolean();
                        p.RouteSchedule = (ReinoRouteSchedule)br.ReadInt32();
                        p.RouteSpeed = (ReinoRouteSpeed)br.ReadInt32();
                        p.LastRouteUtc = DateTime.FromBinary(br.ReadInt64());
                        p.Training = br.ReadBoolean();
                        p.TrainingEndsUtc = DateTime.FromBinary(br.ReadInt64());
                        p.Active = br.ReadBoolean();
                        list.Add(p);
                    }
                    m_PostsByCity[cityId] = list;
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                    m_AutoSheathe.Add(br.ReadInt32());

                if (version >= 2 && fs.Position < fs.Length)
                {
                    count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        int serial = br.ReadInt32();
                        int lines = br.ReadInt32();
                        List<string> list = new List<string>();
                        for (int x = 0; x < lines; x++)
                            list.Add(br.ReadString());
                        m_PendingLawNoticesByPlayer[serial] = list;
                    }
                }

                if (fs.Position < fs.Length)
                    m_NextPostId = br.ReadInt32();
            }
        }
    }
}
