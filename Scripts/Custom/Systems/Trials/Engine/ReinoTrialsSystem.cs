using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Server.Custom.Reinos.ReinoTrialVerdict;

namespace Server.Custom.Reinos
{
    public static class ReinoTrialsSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoTrials_v1.bin");
        private static readonly Dictionary<int, ReinoTrialSession> m_SessionsByPlayer = new Dictionary<int, ReinoTrialSession>();
        private static readonly Dictionary<int, List<ReinoTrialVerdict>> m_VerdictsByCity = new Dictionary<int, List<ReinoTrialVerdict>>();
        private static readonly Dictionary<int, DateTime> m_LastCourtWeaponAction = new Dictionary<int, DateTime>();
        private static readonly Dictionary<int, Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule>> m_LawRulesByCity =
            new Dictionary<int, Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule>>();
        private static Timer m_PulseTimer;

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };

            if (m_PulseTimer != null)
                m_PulseTimer.Stop();

            m_PulseTimer = Timer.DelayCall(TimeSpan.FromSeconds(3.0), TimeSpan.FromSeconds(2.0), Pulse);
        }

        public static ReinoTrialSession GetSession(PlayerMobile pm, int cityId)
        {
            if (pm == null)
                return new ReinoTrialSession();

            ReinoTrialSession st;
            if (!m_SessionsByPlayer.TryGetValue(pm.Serial.Value, out st))
            {
                st = new ReinoTrialSession();
                st.CityId = cityId;
                m_SessionsByPlayer[pm.Serial.Value] = st;
            }

            st.CityId = cityId;
            return st;
        }

        public static List<ReinoTrialVerdict> GetVerdicts(int cityId)
        {
            List<ReinoTrialVerdict> list;
            if (!m_VerdictsByCity.TryGetValue(cityId, out list))
            {
                list = new List<ReinoTrialVerdict>();
                m_VerdictsByCity[cityId] = list;
            }

            return list;
        }

        public static bool IsTribunalConstructionKey(int cityId, string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                return false;

            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null)
                    continue;

                if (!String.Equals(info.Definition.Id, TribunalAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (String.Equals(info.Key, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool HasTribunal(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);

            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null)
                    continue;

                if (String.Equals(info.Definition.Id, TribunalAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase)
                    && info.Status == ReinoLotStatus.Active)
                    return true;
            }

            return false;
        }

        public static bool CanAccessTribunalControl(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null || String.IsNullOrWhiteSpace(role.LinkedConstructionKey))
                return false;

            return IsTribunalConstructionKey(cityId, role.LinkedConstructionKey);
        }

        public static bool CanAccessLawSettings(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null)
                return false;

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && IsTribunalConstructionKey(cityId, role.LinkedConstructionKey))
                return true;

            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(cityId);
            if (String.Equals(culture, "kamay", StringComparison.OrdinalIgnoreCase))
                return role.Kind == ReinoCargoKind.MinisterDefense;

            return role.Hierarchy <= 2;
        }


        private static Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule> GetLawRuleMap(int cityId)
        {
            Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule> map;
            if (!m_LawRulesByCity.TryGetValue(cityId, out map))
            {
                map = new Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule>();
                m_LawRulesByCity[cityId] = map;
            }

            return map;
        }

        public static ReinoTrialLawRule GetLawRule(int cityId, ReinoMilitaryLaw law)
        {
            Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule> map = GetLawRuleMap(cityId);

            ReinoTrialLawRule rule;
            if (!map.TryGetValue(law, out rule))
            {
                rule = new ReinoTrialLawRule();
                rule.CityId = cityId;
                rule.Law = law;
                map[law] = rule;
            }

            return rule;
        }

        public static int GetLawDefaultHours(int cityId, ReinoMilitaryLaw law)
        {
            if (!HasTribunal(cityId))
                return 48;

            ReinoTrialLawRule rule = GetLawRule(cityId, law);
            if (rule == null || !rule.HasCustomValues)
                return 48;

            return Math.Max(1, rule.SentenceHours);
        }

        public static int GetLawDefaultFine(int cityId, ReinoMilitaryLaw law)
        {
            if (!HasTribunal(cityId))
                return 5000;

            ReinoTrialLawRule rule = GetLawRule(cityId, law);
            if (rule == null || !rule.HasCustomValues)
                return 5000;

            return Math.Max(0, rule.FineGold);
        }

        public static string SetLawDefaultHours(PlayerMobile pm, int cityId, ReinoMilitaryLaw law, int hours)
        {
            if (!CanAccessLawSettings(pm, cityId))
                return "Você não pode definir a pena padrão dessa lei.";

            ReinoTrialLawRule rule = GetLawRule(cityId, law);
            rule.HasCustomValues = true;
            rule.SentenceHours = Math.Max(1, hours);
            rule.LastChangedBySerial = pm != null ? pm.Serial.Value : 0;
            rule.LastChangedByName = pm != null ? pm.Name : String.Empty;
            rule.LastChangedUtc = DateTime.UtcNow;
            return "Pena padrão atualizada.";
        }

        public static string SetLawDefaultFine(PlayerMobile pm, int cityId, ReinoMilitaryLaw law, int fine)
        {
            if (!CanAccessLawSettings(pm, cityId))
                return "Você não pode definir a multa padrão dessa lei.";

            ReinoTrialLawRule rule = GetLawRule(cityId, law);
            rule.HasCustomValues = true;
            rule.FineGold = Math.Max(0, fine);
            rule.LastChangedBySerial = pm != null ? pm.Serial.Value : 0;
            rule.LastChangedByName = pm != null ? pm.Name : String.Empty;
            rule.LastChangedUtc = DateTime.UtcNow;
            return "Multa padrão atualizada.";
        }

        public static string GetLawDefinitionHtml(int cityId, ReinoMilitaryLaw law)
        {
            ReinoTrialLawRule rule = GetLawRule(cityId, law);

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append(GetLawDescription(law));
            sb.Append("<BR><BR>");
            sb.Append("<B>Pena atual:</B> ").Append(GetLawDefaultHours(cityId, law)).Append(" horas.<BR>");
            sb.Append("<B>Multa atual:</B> ").Append(GetLawDefaultFine(cityId, law)).Append(" moedas.<BR><BR>");

            if (rule != null && rule.HasCustomValues && !String.IsNullOrWhiteSpace(rule.LastChangedByName))
            {
                sb.Append("<B>Última definição:</B> ").Append(rule.LastChangedByName);

                if (rule.LastChangedUtc > DateTime.MinValue)
                    sb.Append(" em ").Append(rule.LastChangedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));

                sb.Append(".");
            }
            else
            {
                sb.Append("<B>Última definição:</B> valores padrão do reino.");
            }

            sb.Append("<BR><BR>");
            sb.Append("Quando nada for definido, a lei usa <B>48 horas</B> e <B>5000 moedas</B>. Procurados sempre usam esse valor fixo.");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string GetLawDescription(ReinoMilitaryLaw law)
        {
            switch (law)
            {
                case ReinoMilitaryLaw.HoodedWalk:
                    return "Define a punição padrão para quem circular encapuzado em área do reino.";
                case ReinoMilitaryLaw.Stealing:
                    return "Define a punição padrão para roubo presenciado ou registrado no reino.";
                case ReinoMilitaryLaw.Snooping:
                    return "Define a punição padrão para abrir e olhar recipientes, bolsas e pertences alheios sem autorização.";
                case ReinoMilitaryLaw.LootKnockedOut:
                    return "Define a punição padrão para furtar alguém desmaiado.";
                case ReinoMilitaryLaw.Lockpicking:
                    return "Define a punição padrão para arrombar fechaduras e acessos protegidos.";
                case ReinoMilitaryLaw.Fighting:
                    return "Define a punição padrão para brigas e agressões vistas pelos guardas.";
                case ReinoMilitaryLaw.AnimalTaming:
                    return "Define a punição padrão para domar animais em situação proibida pelo reino.";
                case ReinoMilitaryLaw.AnimalKilling:
                    return "Define a punição padrão para matar animais protegidos ou em local proibido.";
                case ReinoMilitaryLaw.ForeignPlanting:
                    return "Define a punição padrão para plantar em fazendas ou áreas agrícolas alheias.";
                case ReinoMilitaryLaw.ForeignHarvesting:
                    return "Define a punição padrão para colher em fazendas ou áreas agrícolas alheias.";
                case ReinoMilitaryLaw.DrugUse:
                    return "Define a punição padrão para uso de drogas em áreas públicas o reino.";
                case ReinoMilitaryLaw.DrunkWalk:
                    return "Define a punição padrão para circular embriagado quando isso for tratado como infração.";
                case ReinoMilitaryLaw.TakingFruit:
                    return "Define a punição padrão para retirar frutos do reino sem autorização.";
                case ReinoMilitaryLaw.FenceJumping:
                    return "Define a punição padrão para invasão por pulo de cerca.";
                case ReinoMilitaryLaw.ArmedWalk:
                    return "Define a punição padrão para circular armado onde isso for proibido.";
                default:
                    return "Define a punição padrão dessa lei.";
            }
        }

        public static void DeleteTribunalItems(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return;

            List<Item> found = new List<Item>();
            CollectTribunalItems(pm.Items, cityId, found);
            if (pm.Backpack != null)
                CollectTribunalItems(pm.Backpack.Items, cityId, found);
            if (pm.BankBox != null)
                CollectTribunalItems(pm.BankBox.Items, cityId, found);

            for (int i = 0; i < found.Count; i++)
            {
                if (found[i] != null && !found[i].Deleted)
                    found[i].Delete();
            }
        }

        private static void CollectTribunalItems(List<Item> items, int cityId, List<Item> found)
        {
            if (items == null)
                return;

            Item[] copy = items.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                Item item = copy[i];
                if (item == null || item.Deleted)
                    continue;

                ReinoTribunalHammer hammer = item as ReinoTribunalHammer;
                if (hammer != null && hammer.CityId == cityId)
                {
                    found.Add(hammer);
                    continue;
                }

                Container cont = item as Container;
                if (cont != null)
                    CollectTribunalItems(cont.Items, cityId, found);
            }
        }

        public static ReinoConstructionRuntimeInfo FindPrimaryTribunalRuntime(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null || info.Lot == null)
                    continue;

                if (String.Equals(info.Definition.Id, TribunalAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase)
                    && (info.Status == ReinoLotStatus.Active || info.Status == ReinoLotStatus.UnderConstruction))
                    return info;
            }

            return null;
        }

        public static ReinoTrialSession GetActiveSessionForCity(int cityId)
        {
            foreach (KeyValuePair<int, ReinoTrialSession> kv in m_SessionsByPlayer)
            {
                ReinoTrialSession st = kv.Value;
                if (st != null && st.CityId == cityId && st.SessionActive)
                    return st;
            }

            return null;
        }

        public static bool HasActiveSession(int cityId)
        {
            return GetActiveSessionForCity(cityId) != null;
        }

        public static int GetActiveAccusedSerial(int cityId)
        {
            ReinoTrialSession st = GetActiveSessionForCity(cityId);
            return st != null ? st.AccusedSerial : 0;
        }

        public static Point3D GetAccusedPoint(int cityId)
        {
            ReinoConstructionRuntimeInfo info = FindPrimaryTribunalRuntime(cityId);
            if (info == null || info.Lot == null)
                return Point3D.Zero;

            Point3D off = TribunalAuroraDefinition.ACCUSED_OFFSET;
            return new Point3D(info.Lot.NorthWest.X + off.X, info.Lot.NorthWest.Y + off.Y, info.Lot.NorthWest.Z + off.Z);
        }

        public static bool IsInsideTribunal(int cityId, Point3D loc, Map map)
        {
            ReinoConstructionRuntimeInfo info = FindPrimaryTribunalRuntime(cityId);
            if (info == null || info.Lot == null || info.Lot.Map != map)
                return false;

            return info.Lot.Contains(loc);
        }

        public static Point3D GetOutsidePoint(int cityId)
        {
            ReinoConstructionRuntimeInfo info = FindPrimaryTribunalRuntime(cityId);
            if (info == null || info.Lot == null || info.Lot.Map == null)
                return Point3D.Zero;

            Point3D origin = new Point3D(info.Lot.NorthWest.X + (info.Lot.Side / 2), info.Lot.NorthWest.Y + info.Lot.Side + 1, info.Lot.NorthWest.Z);
            return FindWalkable(info.Lot.Map, origin);
        }

        private static Point3D FindWalkable(Map map, Point3D origin)
        {
            if (map == null || map == Map.Internal)
                return origin;

            for (int r = 0; r <= 6; r++)
            {
                for (int x = origin.X - r; x <= origin.X + r; x++)
                {
                    for (int y = origin.Y - r; y <= origin.Y + r; y++)
                    {
                        int z = map.GetAverageZ(x, y);
                        if (map.CanSpawnMobile(x, y, z))
                            return new Point3D(x, y, z);
                    }
                }
            }

            return origin;
        }

        public static OSUJusticeOfficer FindJusticeOfficer(int cityId)
        {
            foreach (Mobile mob in World.Mobiles.Values)
            {
                OSUJusticeOfficer officer = mob as OSUJusticeOfficer;
                if (officer != null && !officer.Deleted && officer.GovernmentCityId == cityId)
                    return officer;
            }

            return null;
        }

        private static string GetCourtTitle(PlayerMobile pm, int cityId)
        {
            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role != null && !String.IsNullOrWhiteSpace(role.Title))
                return role.Title;

            return "Líder";
        }

        public static string StartSession(PlayerMobile judge, int cityId, PlayerMobile accused)
        {
            if (judge == null || accused == null || accused.Deleted)
                return "Jogador inválido.";

            ReinoTrialSession active = GetActiveSessionForCity(cityId);
            if (active != null && active != GetSession(judge, cityId))
                return "Já existe uma sessão ativa neste tribunal.";

            ReinoTrialSession st = GetSession(judge, cityId);
            st.SessionActive = true;
            st.AccusedSerial = accused.Serial.Value;
            st.AccusedName = accused.Name;
            st.PendingSentenceDays = 0;
            st.PendingFineGold = 0;

            LockTribunalDoors(cityId, true);

            string title = GetCourtTitle(judge, cityId);
            string msg = String.Format("sessão iniciada, {0} {1}, julga {2} por crime contra o reino, todos de pé", title, judge.Name, accused.Name);
            SayByOfficerOrJudge(cityId, judge, msg);
            return "Sessão iniciada.";
        }

        public static string EndSession(PlayerMobile judge, int cityId)
        {
            ReinoTrialSession st = GetSession(judge, cityId);
            int accusedSerial = st.AccusedSerial;
            string accusedName = st.AccusedName;
            int days = st.PendingSentenceDays;
            int fine = st.PendingFineGold;

            st.SessionActive = false;
            st.AccusedSerial = 0;
            st.AccusedName = String.Empty;
            st.PendingSentenceDays = 0;
            st.PendingFineGold = 0;

            LockTribunalDoors(cityId, false);

            if (accusedSerial > 0)
            {
                string unused;
                if (days > 0)
                {
                    ApplyJudgement(cityId, accusedSerial, accusedName, days, fine, judge);
                }
                else
                {
                    ReinoPrisionSystem.ReleaseInmateToBank(cityId, accusedSerial, judge != null ? judge.Name : "Tribunal", out unused);
                }
            }

            SayByOfficerOrJudge(cityId, judge, "sessão encerrada");
            return "Sessão encerrada.";
        }

        public static void BangHammer(PlayerMobile judge, int cityId)
        {
            if (judge == null || judge.Deleted)
                return;

            for (int i = 0; i < 3; i++)
            {
                TimeSpan delay = TimeSpan.FromMilliseconds(400 * i);
                Timer.DelayCall(delay, delegate
                {
                    if (judge == null || judge.Deleted)
                        return;

                    judge.PlaySound(0x136);
                    judge.Animate(12, 2, 2, true, false, 0);
                });
            }
        }

        public static string SetPendingSentence(PlayerMobile judge, int cityId, int days)
        {
            ReinoTrialSession st = GetSession(judge, cityId);
            if (st.AccusedSerial == 0 || String.IsNullOrWhiteSpace(st.AccusedName))
                return "Nenhum réu foi definido para esta sessão.";

            st.PendingSentenceDays = Math.Max(1, days);
            judge.Say(String.Format("A pena de {0} foi decretada aqui em {1} dias", st.AccusedName, st.PendingSentenceDays));
            EnsureVerdict(cityId, st.AccusedSerial, st.AccusedName, judge, "Julgamento em tribunal", st.PendingSentenceDays * 24, st.PendingFineGold, "Pena decretada no tribunal.");
            return "Pena registrada.";
        }

        public static string SetPendingFine(PlayerMobile judge, int cityId, int gold)
        {
            ReinoTrialSession st = GetSession(judge, cityId);
            if (st.AccusedSerial == 0 || String.IsNullOrWhiteSpace(st.AccusedName))
                return "Nenhum réu foi definido para esta sessão.";

            if (st.PendingSentenceDays <= 0)
                return "Você precisa decretar a pena antes de definir a multa.";

            st.PendingFineGold = Math.Max(0, gold);
            judge.Say(String.Format("A soltura de {0} fica pendente até pagamento de multa de {1}, ou fim da pena decretada", st.AccusedName, st.PendingFineGold));
            EnsureVerdict(cityId, st.AccusedSerial, st.AccusedName, judge, "Julgamento em tribunal", st.PendingSentenceDays * 24, st.PendingFineGold, "Multa decretada no tribunal.");
            return "Multa registrada.";
        }

        private static void ApplyJudgement(int cityId, int accusedSerial, string accusedName, int days, int fineGold, PlayerMobile judge)
        {
            ReinoPrisionSystem.ApplyJudgement(cityId, accusedSerial, days, fineGold, judge != null ? judge.Name : String.Empty);
            EnsureVerdict(cityId, accusedSerial, accusedName, judge, "Julgamento em tribunal", Math.Max(1, days) * 24, Math.Max(0, fineGold), "Veredito aplicado ao preso.");
        }

        private static void EnsureVerdict(int cityId, int prisonerSerial, string prisonerName, PlayerMobile judge, string crimeLabel, int durationHours, int fineGold, string notes)
        {
            List<ReinoTrialVerdict> list = GetVerdicts(cityId);
            ReinoTrialVerdict found = null;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] != null && list[i].PrisonerSerial == prisonerSerial)
                {
                    found = list[i];
                    break;
                }
            }

            if (found == null)
            {
                found = new ReinoTrialVerdict();
                found.CityId = cityId;
                found.PrisonerSerial = prisonerSerial;
                list.Add(found);
            }

            found.PrisonerName = prisonerName ?? String.Empty;
            found.JudgeSerial = judge != null ? judge.Serial.Value : 0;
            found.JudgeName = judge != null ? judge.Name : String.Empty;
            found.CrimeLabel = crimeLabel ?? String.Empty;
            found.DurationHours = Math.Max(1, durationHours);
            found.FineGold = Math.Max(0, fineGold);
            found.DeclaredUtc = DateTime.UtcNow;
            found.Notes = notes ?? String.Empty;
        }

        private static void SayByOfficerOrJudge(int cityId, PlayerMobile judge, string msg)
        {
            OSUJusticeOfficer officer = FindJusticeOfficer(cityId);
            if (officer != null && !officer.Deleted)
                officer.Say(msg);
            else if (judge != null && !judge.Deleted)
                judge.Say(msg);
        }

        private static void LockTribunalDoors(int cityId, bool locked)
        {
            ReinoConstructionRuntimeInfo info = FindPrimaryTribunalRuntime(cityId);
            if (info == null || info.LotState == null || info.LotState.DoorSerials == null)
                return;

            for (int i = 0; i < info.LotState.DoorSerials.Count; i++)
            {
                Item item;
                if (!World.Items.TryGetValue(info.LotState.DoorSerials[i], out item))
                    continue;

                BaseDoor door = item as BaseDoor;
                if (door == null)
                    continue;

                door.Locked = locked;
                if (!locked)
                    door.Open = false;
            }
        }

        public static void BeginExpel(PlayerMobile judge, int cityId, Mobile target)
        {
            if (judge == null || target == null || target.Deleted)
                return;

            if (!IsInsideTribunal(cityId, target.Location, target.Map))
            {
                judge.SendMessage("O alvo precisa estar dentro do tribunal.");
                return;
            }

            StartOfficerAction(cityId, judge, target, ReinoTribunalEscortAction.Expel);
        }

        public static void BeginContemptArrest(PlayerMobile judge, int cityId, Mobile target)
        {
            if (judge == null || target == null || target.Deleted)
                return;

            if (!IsInsideTribunal(cityId, target.Location, target.Map))
            {
                judge.SendMessage("O alvo precisa estar dentro do tribunal.");
                return;
            }

            StartOfficerAction(cityId, judge, target, ReinoTribunalEscortAction.PrisonForContempt);
        }

        private static void StartOfficerAction(int cityId, PlayerMobile judge, Mobile target, ReinoTribunalEscortAction action)
        {
            OSUJusticeOfficer officer = FindJusticeOfficer(cityId);
            if (officer == null || officer.Deleted)
            {
                if (action == ReinoTribunalEscortAction.Expel)
                    ExpelNow(cityId, target);
                else
                    HandleCourtCrime(target as PlayerMobile, cityId, "Desacato ao tribunal", true);
                return;
            }

            officer.Busy = true;
            new OfficerEscortTimer(officer, target, judge, cityId, action).Start();
        }

        private static void ExpelNow(int cityId, Mobile target)
        {
            if (target == null || target.Deleted || target.Map == null)
                return;

            Point3D p = GetOutsidePoint(cityId);
            if (p == Point3D.Zero)
                return;

            target.MoveToWorld(p, target.Map);
            target.SendMessage("Você foi expulso da corte.");
        }

        public static bool HandleCourtCrime(PlayerMobile pm, int cityId, string crimeLabel, bool contempt)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (!ReinoMilitarySystem.HasPrison(cityId))
                return false;

            OSUJusticeOfficer officer = FindJusticeOfficer(cityId);
            bool prisoned = officer != null
                ? ReinoMilitarySystem.TrySendToPrison(pm, cityId, officer, ReinoMilitaryLaw.Fighting)
                : ReinoMilitarySystem.TrySendToPrison(pm, cityId, null, ReinoMilitaryLaw.Fighting);

            if (!prisoned)
                return false;

            EnsureVerdict(cityId, pm.Serial.Value, pm.Name, null, crimeLabel, 3, 500, contempt ? "Prisão por desacato ao tribunal." : "Crime cometido dentro do tribunal.");
            UpdateLatestPrisonRecord(cityId, pm.Serial.Value, crimeLabel, 3, 500, contempt ? "Prisão por desacato ao tribunal." : "Crime cometido dentro do tribunal.");
            ReinoPrisionSystem.ApplyJudgement(cityId, pm.Serial.Value, 1, 500, "Tribunal");
            pm.SendMessage("Você foi conduzido à prisão do reino pelo tribunal.");
            return true;
        }

        private static void UpdateLatestPrisonRecord(int cityId, int prisonerSerial, string crimeLabel, int hours, int fineGold, string notes)
        {
            List<ReinoPrisonRecord> list = ReinoMilitarySystem.GetPrisonList(cityId);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ReinoPrisonRecord r = list[i];
                if (r == null || r.PrisonerSerial != prisonerSerial)
                    continue;

                r.CrimeLabel = crimeLabel ?? r.CrimeLabel;
                r.DurationHours = Math.Max(1, hours);
                r.ArrestUtc = DateTime.UtcNow;
                r.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(Math.Max(1, hours));
                r.Notes = (notes ?? String.Empty) + (fineGold > 0 ? (" Multa prevista: " + fineGold + " moedas.") : String.Empty);
                return;
            }
        }

        private static ReinoConstructionRuntimeInfo FindPrimaryPrisonRuntime(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info != null && info.Definition != null && info.Lot != null && String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase))
                    return info;
            }

            return null;
        }

        private static void Pulse()
        {
            foreach (Mobile mob in World.Mobiles.Values)
            {
                PlayerMobile pm = mob as PlayerMobile;
                if (pm == null || pm.Deleted || pm.Map == null || pm.Map == Map.Internal)
                    continue;

                int cityId = ResolveTribunalCityAt(pm.Location, pm.Map);
                if (cityId < 0)
                    continue;

                if (!HasForbiddenCourtWeapon(pm, cityId))
                    continue;

                DateTime next;
                if (m_LastCourtWeaponAction.TryGetValue(pm.Serial.Value, out next) && next > DateTime.UtcNow)
                    continue;

                m_LastCourtWeaponAction[pm.Serial.Value] = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
                HandleCourtCrime(pm, cityId, "Portava armas dentro do tribunal", true);
            }
        }

        private static int ResolveTribunalCityAt(Point3D loc, Map map)
        {
            int cityCount = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;
            for (int cityId = 0; cityId < cityCount; cityId++)
            {
                if (IsInsideTribunal(cityId, loc, map))
                    return cityId;
            }

            return -1;
        }

        private static bool HasForbiddenCourtWeapon(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            Item one = pm.FindItemOnLayer(Layer.OneHanded);
            Item two = pm.FindItemOnLayer(Layer.TwoHanded);

            if (one is ReinoTribunalHammer && CanAccessTribunalControl(pm, cityId))
                return false;

            if (two is ReinoTribunalHammer && CanAccessTribunalControl(pm, cityId))
                return false;

            return one is BaseWeapon || two is BaseWeapon;
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(2);
                    bw.Write(m_VerdictsByCity.Count);
                    foreach (KeyValuePair<int, List<ReinoTrialVerdict>> kv in m_VerdictsByCity)
                    {
                        bw.Write(kv.Key);
                        List<ReinoTrialVerdict> list = kv.Value ?? new List<ReinoTrialVerdict>();
                        bw.Write(list.Count);
                        for (int i = 0; i < list.Count; i++)
                        {
                            ReinoTrialVerdict v = list[i] ?? new ReinoTrialVerdict();
                            bw.Write(v.CityId);
                            bw.Write(v.PrisonerSerial);
                            bw.Write(v.PrisonerName ?? String.Empty);
                            bw.Write(v.JudgeSerial);
                            bw.Write(v.JudgeName ?? String.Empty);
                            bw.Write(v.CrimeLabel ?? String.Empty);
                            bw.Write(v.DurationHours);
                            bw.Write(v.FineGold);
                            bw.Write(v.DeclaredUtc.ToBinary());
                            bw.Write(v.Notes ?? String.Empty);
                        }
                    }

                    bw.Write(m_LawRulesByCity.Count);
                    foreach (KeyValuePair<int, Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule>> kv in m_LawRulesByCity)
                    {
                        bw.Write(kv.Key);
                        bw.Write(kv.Value.Count);

                        foreach (KeyValuePair<ReinoMilitaryLaw, ReinoTrialLawRule> inner in kv.Value)
                        {
                            ReinoTrialLawRule rule = inner.Value ?? new ReinoTrialLawRule();

                            bw.Write((int)inner.Key);
                            bw.Write(rule.HasCustomValues);
                            bw.Write(rule.SentenceHours);
                            bw.Write(rule.FineGold);
                            bw.Write(rule.LastChangedBySerial);
                            bw.Write(rule.LastChangedByName ?? String.Empty);
                            bw.Write(rule.LastChangedUtc.ToBinary());
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static void Load()
        {
            m_VerdictsByCity.Clear();
            m_LawRulesByCity.Clear();

            if (!File.Exists(FilePath))
                return;

            try
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();

                    int cityCount = br.ReadInt32();
                    for (int i = 0; i < cityCount; i++)
                    {
                        int cityId = br.ReadInt32();
                        int count = br.ReadInt32();

                        List<ReinoTrialVerdict> list = GetVerdicts(cityId);
                        list.Clear();

                        for (int j = 0; j < count; j++)
                        {
                            ReinoTrialVerdict v = new ReinoTrialVerdict();
                            v.CityId = br.ReadInt32();
                            v.PrisonerSerial = br.ReadInt32();
                            v.PrisonerName = br.ReadString();
                            v.JudgeSerial = br.ReadInt32();
                            v.JudgeName = br.ReadString();
                            v.CrimeLabel = br.ReadString();
                            v.DurationHours = br.ReadInt32();
                            v.FineGold = br.ReadInt32();
                            v.DeclaredUtc = DateTime.FromBinary(br.ReadInt64());
                            v.Notes = br.ReadString();
                            list.Add(v);
                        }
                    }

                    if (version >= 2 && fs.Position < fs.Length)
                    {
                        int cityCountRules = br.ReadInt32();

                        for (int i = 0; i < cityCountRules; i++)
                        {
                            int cityId = br.ReadInt32();
                            int countRules = br.ReadInt32();

                            Dictionary<ReinoMilitaryLaw, ReinoTrialLawRule> map = GetLawRuleMap(cityId);
                            map.Clear();

                            for (int j = 0; j < countRules; j++)
                            {
                                ReinoMilitaryLaw law = (ReinoMilitaryLaw)br.ReadInt32();

                                ReinoTrialLawRule rule = new ReinoTrialLawRule();
                                rule.CityId = cityId;
                                rule.Law = law;
                                rule.HasCustomValues = br.ReadBoolean();
                                rule.SentenceHours = br.ReadInt32();
                                rule.FineGold = br.ReadInt32();
                                rule.LastChangedBySerial = br.ReadInt32();
                                rule.LastChangedByName = br.ReadString();
                                rule.LastChangedUtc = DateTime.FromBinary(br.ReadInt64());

                                map[law] = rule;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private class OfficerEscortTimer : Timer
        {
            private readonly OSUJusticeOfficer m_Officer;
            private readonly Mobile m_Target;
            private readonly PlayerMobile m_Judge;
            private readonly int m_CityId;
            private readonly ReinoTribunalEscortAction m_Action;

            public OfficerEscortTimer(OSUJusticeOfficer officer, Mobile target, PlayerMobile judge, int cityId, ReinoTribunalEscortAction action)
                : base(TimeSpan.Zero, TimeSpan.FromMilliseconds(250.0))
            {
                m_Officer = officer;
                m_Target = target;
                m_Judge = judge;
                m_CityId = cityId;
                m_Action = action;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                if (m_Officer == null || m_Officer.Deleted || m_Target == null || m_Target.Deleted || m_Officer.Map == null || m_Officer.Map != m_Target.Map)
                {
                    Finish();
                    return;
                }

                if (m_Officer.InRange(m_Target.Location, 1))
                {
                    if (m_Action == ReinoTribunalEscortAction.Expel)
                        ExpelNow(m_CityId, m_Target);
                    else
                        HandleCourtCrime(m_Target as PlayerMobile, m_CityId, "Desacato ao tribunal", true);

                    Finish();
                    return;
                }

                Direction dir = m_Officer.GetDirectionTo(m_Target.Location);
                if (!m_Officer.Move(dir))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (m_Officer.Move((Direction)i))
                            return;
                    }
                }
            }

            private void Finish()
            {
                Stop();

                if (m_Officer != null && !m_Officer.Deleted)
                {
                    m_Officer.Busy = false;
                    if (m_Officer.Map != null && m_Officer.Map != Map.Internal)
                        m_Officer.MoveToWorld(m_Officer.PostLocation, m_Officer.Map);
                }
            }
        }
    }

    public class ReinoTribunalStartSessionTarget : Target
    {
        private readonly int m_CityId;

        public ReinoTribunalStartSessionTarget(int cityId) : base(12, false, TargetFlags.None)
        {
            m_CityId = cityId;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            PlayerMobile judge = from as PlayerMobile;
            PlayerMobile accused = targeted as PlayerMobile;
            if (judge == null || accused == null)
                return;

            judge.SendMessage(ReinoTrialsSystem.StartSession(judge, m_CityId, accused));
            judge.SendGump(new ReinoTribunalGump(judge, m_CityId));
        }
    }

    public class ReinoTribunalExpelTarget : Target
    {
        private readonly int m_CityId;

        public ReinoTribunalExpelTarget(int cityId) : base(12, false, TargetFlags.None)
        {
            m_CityId = cityId;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            PlayerMobile judge = from as PlayerMobile;
            Mobile mob = targeted as Mobile;
            if (judge == null || mob == null)
                return;

            ReinoTrialsSystem.BeginExpel(judge, m_CityId, mob);
            judge.SendGump(new ReinoTribunalGump(judge, m_CityId));
        }
    }

    public class ReinoTribunalContemptTarget : Target
    {
        private readonly int m_CityId;

        public ReinoTribunalContemptTarget(int cityId) : base(12, false, TargetFlags.None)
        {
            m_CityId = cityId;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            PlayerMobile judge = from as PlayerMobile;
            Mobile mob = targeted as Mobile;
            if (judge == null || mob == null)
                return;

            ReinoTrialsSystem.BeginContemptArrest(judge, m_CityId, mob);
            judge.SendGump(new ReinoTribunalGump(judge, m_CityId));
        }
    }
}
