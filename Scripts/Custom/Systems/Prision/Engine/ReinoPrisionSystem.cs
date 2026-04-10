using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Server.Items.Uniforme44;

namespace Server.Custom.Reinos
{
    public static class ReinoPrisionSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoPrision_v1.bin");

        private static readonly Dictionary<int, ReinoPrisionSettings> m_SettingsByCity = new Dictionary<int, ReinoPrisionSettings>();
        private static readonly Dictionary<int, List<ReinoPrisionerState>> m_InmatesByCity = new Dictionary<int, List<ReinoPrisionerState>>();
        private static readonly Dictionary<int, ReinoPrisionSession> m_Sessions = new Dictionary<int, ReinoPrisionSession>();
        private static Timer m_PulseTimer;

        public static void Initialize()
        {
            Load();
            EnsureDefaults();
            EventSink.WorldSave += delegate { Save(); };

            if (m_PulseTimer != null)
                m_PulseTimer.Stop();

            m_PulseTimer = Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(1.0), Pulse);
        }

        private static void Pulse()
        {
            ReleaseExpiredPrisoners();
            ProcessDailyChargesAndMeals();
            RefreshOnlineFineGumps();
        }

        private static void EnsureDefaults()
        {
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;
            for (int i = 0; i < count; i++)
            {
                GetSettings(i);
                GetInmates(i);
            }
        }

        public static ReinoPrisionSettings GetSettings(int cityId)
        {
            ReinoPrisionSettings st;
            if (!m_SettingsByCity.TryGetValue(cityId, out st))
            {
                st = new ReinoPrisionSettings();
                st.CityId = cityId;
                m_SettingsByCity[cityId] = st;
            }

            return st;
        }

        public static List<ReinoPrisionerState> GetInmates(int cityId)
        {
            List<ReinoPrisionerState> list;
            if (!m_InmatesByCity.TryGetValue(cityId, out list))
            {
                list = new List<ReinoPrisionerState>();
                m_InmatesByCity[cityId] = list;
            }

            return list;
        }

        public static ReinoPrisionSession GetSession(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return new ReinoPrisionSession();

            ReinoPrisionSession session;
            if (!m_Sessions.TryGetValue(pm.Serial.Value, out session))
            {
                session = new ReinoPrisionSession();
                m_Sessions[pm.Serial.Value] = session;
            }

            return session;
        }

        public static bool HasPrison(int cityId)
        {
            return FindPrimaryPrisonRuntime(cityId) != null;
        }

        public static bool HasTribunal(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info != null && info.Definition != null && String.Equals(info.Definition.Id, "tribunal_aurora", StringComparison.OrdinalIgnoreCase)
                    && (info.Status == ReinoLotStatus.Active || info.Status == ReinoLotStatus.UnderConstruction))
                    return true;
            }

            return false;
        }

        public static bool CanAccessPrisonControl(PlayerMobile pm, int cityId)
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

            if (role.Hierarchy <= 2)
                return true;

            if (!String.IsNullOrWhiteSpace(role.LinkedConstructionKey) && IsPrisonConstructionKey(cityId, role.LinkedConstructionKey))
                return true;

            return false;
        }

        private static bool IsPrisonConstructionKey(int cityId, string key)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(key);
            return info != null && info.CityId == cityId && info.Definition != null
                && String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase);
        }

        public static int GetDynamicWeeklyGold(int cityId)
        {
            return Math.Max(0, GetSettings(cityId).PendingWeeklyGold);
        }

        public static void ConsumeDynamicWeeklyGold(int cityId)
        {
            GetSettings(cityId).PendingWeeklyGold = 0;
        }

        public static bool FindFirstEmptyPrisonCell(int cityId, out int cellIndex, out Point3D point, out Map map)
        {
            cellIndex = -1;
            point = Point3D.Zero;
            map = null;

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null)
                return false;

            map = prison.Lot.Map;

            for (int i = 0; i < 5; i++)
            {
                if (GetInmateByCell(cityId, i) != null)
                    continue;

                cellIndex = i;
                point = GetCellPoint(prison.Lot, i);
                return true;
            }

            point = prison.Lot.GetCenter(prison.Lot.NorthWest.Z);
            return true;
        }

        public static ReinoPrisionerState GetInmateByCell(int cityId, int cellIndex)
        {
            List<ReinoPrisionerState> list = GetInmates(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoPrisionerState inmate = list[i];
                if (inmate != null && inmate.CellIndex == cellIndex)
                    return inmate;
            }

            return null;
        }

        public static ReinoPrisionerState GetInmateBySerial(int cityId, int prisonerSerial)
        {
            List<ReinoPrisionerState> list = GetInmates(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoPrisionerState inmate = list[i];
                if (inmate != null && inmate.PrisonerSerial == prisonerSerial)
                    return inmate;
            }

            return null;
        }

        public static Point3D GetCellPoint(ReinoLotDefinition lot, int cellIndex)
        {
            Point3D offset = PrisaoAuroraDefinition.GetCellOffset(cellIndex);
            return new Point3D(lot.NorthWest.X + offset.X, lot.NorthWest.Y + offset.Y, lot.NorthWest.Z + offset.Z);
        }

        public static Point3D GetInterrogationPoint(ReinoLotDefinition lot)
        {
            Point3D offset = PrisaoAuroraDefinition.GetInterrogationOffset();
            return new Point3D(lot.NorthWest.X + offset.X, lot.NorthWest.Y + offset.Y, lot.NorthWest.Z + offset.Z);
        }

        public static Point3D GetReleasePoint(ReinoLotDefinition lot)
        {
            Point3D offset = PrisaoAuroraDefinition.GetReleaseOffset();
            return new Point3D(lot.NorthWest.X + offset.X, lot.NorthWest.Y + offset.Y, lot.NorthWest.Z + offset.Z);
        }

        public static string GetCellLabel(int cellIndex)
        {
            return String.Format("Cela {0}", Math.Max(1, cellIndex + 1));
        }

        public static string GetPrisonHtml(int cityId, int viewedCellIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            ReinoPrisionerState inmate = GetInmateByCell(cityId, viewedCellIndex);
            if (inmate == null)
            {
                sb.Append("Essa cela está vazia.");
                sb.Append("</BASEFONT>");
                return sb.ToString();
            }

            TimeSpan remaining = inmate.ReleaseUtc > DateTime.UtcNow ? inmate.ReleaseUtc - DateTime.UtcNow : TimeSpan.Zero;
            int remainingHours = Math.Max(0, (int)Math.Ceiling(remaining.TotalHours));

            sb.Append("<B>Preso:</B> ").Append(inmate.PrisonerName).Append("<BR>");
            sb.Append("<B>Crime:</B> ").Append(String.IsNullOrWhiteSpace(inmate.CrimeLabel) ? "não informado" : inmate.CrimeLabel).Append("<BR>");
            sb.Append("<B>Pena:</B> ").Append(inmate.SentenceHours).Append(" hora(s)<BR>");
            sb.Append("<B>Restante:</B> ").Append(remainingHours).Append(" hora(s)<BR>");
            sb.Append("<B>Julgado:</B> ").Append(inmate.Judged ? "sim" : "não").Append("<BR>");
            if (inmate.Judged && !String.IsNullOrWhiteSpace(inmate.JudgeName))
                sb.Append("<B>Quem julgou:</B> ").Append(inmate.JudgeName).Append("<BR>");
            if (inmate.Judged && inmate.JudgedUtc != DateTime.MinValue)
                sb.Append("<B>Quando:</B> ").Append(inmate.JudgedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");

            if (inmate.InTribunal)
                sb.Append("<B>No tribunal:</B> sim<BR>");
            sb.Append("<B>Multa:</B> ").Append(inmate.FineGold).Append(" moedas<BR>");
            sb.Append("<B>Interrogatório:</B> ").Append(inmate.InInterrogation ? "sim" : "não").Append("<BR>");
            if (!String.IsNullOrWhiteSpace(inmate.Notes))
                sb.Append("<B>Observação:</B> ").Append(inmate.Notes);

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static int GetRemainingHours(int cityId, int viewedCellIndex)
        {
            ReinoPrisionerState inmate = GetInmateByCell(cityId, viewedCellIndex);
            if (inmate == null)
                return 0;

            if (inmate.ReleaseUtc <= DateTime.UtcNow)
                return 0;

            return Math.Max(0, (int)Math.Ceiling((inmate.ReleaseUtc - DateTime.UtcNow).TotalHours));
        }

        public static bool AreAllCellDoorsLinked(int cityId)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);
            for (int i = 0; i < settings.CellDoorSerials.Length; i++)
                if (settings.CellDoorSerials[i] <= 0)
                    return false;
            return true;
        }

        public static bool LinkCellDoor(int cityId, int cellIndex, Item item, out string message)
        {
            message = String.Empty;
            BaseDoor door = item as BaseDoor;
            if (door == null)
            {
                message = "Você precisa selecionar uma porta.";
                return false;
            }

            if (cellIndex < 0 || cellIndex >= 5)
            {
                message = "Cela inválida.";
                return false;
            }

            ReinoPrisionSettings settings = GetSettings(cityId);
            settings.CellDoorSerials[cellIndex] = door.Serial.Value;
            door.Locked = true;
            door.Open = false;
            message = String.Format("Porta da cela {0} vinculada.", cellIndex + 1);
            return true;
        }

        public static bool ToggleFeedPrisoners(int cityId)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);
            settings.FeedPrisoners = !settings.FeedPrisoners;
            if (settings.FeedPrisoners)
                SpawnMealsForOccupiedCells(cityId);
            return settings.FeedPrisoners;
        }

        public static bool ToggleFinePayments(int cityId)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);
            settings.AllowFinePayment = !settings.AllowFinePayment;
            return settings.AllowFinePayment;
        }

        public static bool ToggleOuterDoors(int cityId)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);
            settings.OuterDoorsLocked = !settings.OuterDoorsLocked;
            SetOuterDoorsLocked(cityId, settings.OuterDoorsLocked);
            return settings.OuterDoorsLocked;
        }

        public static bool OpenCellDoor(int cityId, int cellIndex, out string message)
        {
            message = String.Empty;
            ReinoPrisionSettings settings = GetSettings(cityId);
            if (cellIndex < 0 || cellIndex >= settings.CellDoorSerials.Length || settings.CellDoorSerials[cellIndex] <= 0)
            {
                message = "Essa cela ainda não tem uma porta vinculada.";
                return false;
            }

            BaseDoor door = World.FindItem((Serial)settings.CellDoorSerials[cellIndex]) as BaseDoor;
            if (door == null || door.Deleted)
            {
                message = "A porta vinculada não foi encontrada.";
                return false;
            }

            door.Locked = false;
            door.Open = true;
            message = String.Format("Cela {0} aberta.", cellIndex + 1);
            return true;
        }

        public static bool AdjustRemainingHours(int cityId, int cellIndex, int hours, string actorName, out string message)
        {
            message = String.Empty;
            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(Math.Max(1, hours));
            inmate.SentenceHours = Math.Max(1, hours);
            inmate.Notes = String.Format("Pena ajustada por {0}.", actorName ?? "administração");
            message = "Pena ajustada.";
            return true;
        }

        public static bool ToggleInterrogation(int cityId, int cellIndex, out string message)
        {
            message = String.Empty;
            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            Mobile m = World.FindMobile((Serial)inmate.PrisonerSerial);
            PlayerMobile pm = m as PlayerMobile;
            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (pm == null || prison == null || prison.Lot == null)
            {
                message = "Preso ou prisão não encontrados.";
                return false;
            }

            if (!inmate.InInterrogation)
            {
                List<ReinoPrisionerState> list = GetInmates(cityId);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].InInterrogation && list[i].PrisonerSerial != inmate.PrisonerSerial)
                    {
                        message = "Já existe alguém na sala de interrogatório.";
                        return false;
                    }
                }

                pm.MoveToWorld(GetInterrogationPoint(prison.Lot), prison.Lot.Map);
                pm.CantWalk = true;
                inmate.InInterrogation = true;
                message = "Preso levado para o interrogatório.";
            }
            else
            {
                Point3D cellPoint = inmate.CellIndex >= 0 ? GetCellPoint(prison.Lot, inmate.CellIndex) : prison.Lot.GetCenter(prison.Lot.NorthWest.Z);
                pm.MoveToWorld(cellPoint, prison.Lot.Map);
                pm.CantWalk = false;
                inmate.InInterrogation = false;
                message = "Preso devolvido à cela.";
            }

            return true;
        }

        public static bool ReleaseInmate(int cityId, int cellIndex, string releasedBy, out string message)
        {
            message = String.Empty;
            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            ReleaseInmateInternal(inmate, releasedBy, false, out message);
            return true;
        }

        public static bool ConsumeFineAndRelease(PlayerMobile pm, int cityId, out string message)
        {
            message = String.Empty;
            if (pm == null || pm.Deleted)
            {
                message = "Preso inválido.";
                return false;
            }

            ReinoPrisionerState inmate = GetInmateBySerial(cityId, pm.Serial.Value);
            if (inmate == null)
            {
                message = "Você não está registrado como preso.";
                return false;
            }

            if (inmate.FineGold <= 0)
            {
                message = "Não há multa pendente.";
                return false;
            }

            if (!Banker.Withdraw(pm, inmate.FineGold))
            {
                message = "Você não tem ouro suficiente para pagar essa multa.";
                return false;
            }

            inmate.FinePaid = true;
            return ReleaseInmateInternal(inmate, pm.Name, true, out message);
        }

        public static bool TrySendToPrison(Mobile prisoner, int cityId, OSUCityGuard guard, ReinoMilitaryLaw law)
        {
            PlayerMobile pm = prisoner as PlayerMobile;
            if (pm == null || pm.Deleted || pm.Map == null)
                return false;

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null)
                return false;

            RemoveExistingInmate(cityId, pm.Serial.Value);

            int sentenceHours = 48;
            int fineGold = 0;
            if (GetSettings(cityId).AllowFinePayment && !HasTribunal(cityId))
                fineGold = 5000;

            Bag bag;
            if (!ConfiscateBelongings(pm, cityId, out bag))
                bag = null;

            EquipPrisonUniform(pm);

            int cellIndex;
            Point3D dest;
            Map destMap;
            FindFirstEmptyPrisonCell(cityId, out cellIndex, out dest, out destMap);
            if (destMap == null)
                destMap = prison.Lot.Map;

            pm.Combatant = null;
            pm.Warmode = false;
            pm.CantWalk = false;
            pm.MoveToWorld(dest, destMap);

            ReinoPrisionerState inmate = new ReinoPrisionerState();
            inmate.CityId = cityId;
            inmate.PrisonerSerial = pm.Serial.Value;
            inmate.PrisonerName = pm.Name;
            inmate.CrimeLabel = ReinoMilitarySystem.GetLawLabel(law);
            inmate.ArrestUtc = DateTime.UtcNow;
            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(sentenceHours);
            inmate.SentenceHours = sentenceHours;
            inmate.CellIndex = cellIndex;
            inmate.InInterrogation = false;
            inmate.JudgeName = String.Empty;
            inmate.Judged = false;
            inmate.JudgedUtc = DateTime.MinValue;
            inmate.InTribunal = false;
            inmate.FineGold = fineGold;
            inmate.BelongingsBagSerial = bag != null ? bag.Serial.Value : 0;
            inmate.Notes = guard != null ? "Preso pelos guardas do reino." : "Prisão administrativa.";
            GetInmates(cityId).Add(inmate);

            GetSettings(cityId).PendingWeeklyGold += 10;
            ReinoMilitarySystem.AddPrisonRecord(cityId, pm, guard, law, sentenceHours, inmate.Notes);

            if (GetSettings(cityId).FeedPrisoners)
                SpawnMealForInmate(cityId, inmate);

            if (fineGold > 0)
                ShowFineGump(pm, inmate);

            pm.SendMessage("Você foi levado para a prisão do reino.");
            return true;
        }

        public static bool ApplyJudgement(int cityId, int prisonerSerial, int days, int fineGold, string judgeName)
        {
            ReinoPrisionerState inmate = GetInmateBySerial(cityId, prisonerSerial);
            if (inmate == null)
                return false;

            int hours = Math.Max(1, days * 24);
            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(hours);
            inmate.SentenceHours = hours;
            inmate.FineGold = Math.Max(0, fineGold);
            inmate.Judged = true;
            inmate.JudgeName = judgeName ?? String.Empty;
            inmate.JudgedUtc = DateTime.UtcNow;
            inmate.FineGumpShown = false;
            Mobile m = World.FindMobile((Serial)prisonerSerial);
            PlayerMobile pm = m as PlayerMobile;
            if (pm != null && inmate.FineGold > 0 && GetSettings(cityId).AllowFinePayment)
                ShowFineGump(pm, inmate);
            return true;
        }

        private static void RefreshOnlineFineGumps()
        {
            foreach (KeyValuePair<int, List<ReinoPrisionerState>> kv in m_InmatesByCity)
            {
                if (!GetSettings(kv.Key).AllowFinePayment)
                    continue;

                List<ReinoPrisionerState> list = kv.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoPrisionerState inmate = list[i];
                    if (inmate == null || inmate.FinePaid || inmate.FineGold <= 0)
                        continue;

                    PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
                    if (pm == null || pm.NetState == null)
                        continue;

                    if (!inmate.FineGumpShown)
                        ShowFineGump(pm, inmate);
                }
            }
        }

        private static void ShowFineGump(PlayerMobile pm, ReinoPrisionerState inmate)
        {
            if (pm == null || inmate == null || pm.Deleted)
                return;

            inmate.FineGumpShown = true;
            pm.CloseGump(typeof(ReinoPrisionFineGump));
            pm.SendGump(new ReinoPrisionFineGump(pm, inmate.CityId, inmate.PrisonerSerial));
        }

        private static bool ReleaseInmateInternal(ReinoPrisionerState inmate, string releasedBy, bool byFine, out string message)
        {
            message = "Preso solto.";
            if (inmate == null)
            {
                message = "Preso inválido.";
                return false;
            }

            int cityId = inmate.CityId;
            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (pm != null && prison != null && prison.Lot != null)
            {
                ReturnBelongings(pm, inmate.BelongingsBagSerial);
                pm.CantWalk = false;

                Point3D release = GetReleasePoint(prison.Lot);
                pm.MoveToWorld(release, prison.Lot.Map);
            }

            SetOuterDoorsLocked(cityId, false);
            OpenCellDoor(cityId, inmate.CellIndex, out message);

            List<ReinoPrisionerState> list = GetInmates(cityId);
            list.Remove(inmate);
            return true;
        }

        private static void RemoveExistingInmate(int cityId, int prisonerSerial)
        {
            ReinoPrisionerState existing = GetInmateBySerial(cityId, prisonerSerial);
            if (existing != null)
            {
                string unused;
                ReleaseInmateInternal(existing, "Sistema", false, out unused);
            }
        }

        private static void ReleaseExpiredPrisoners()
        {
            List<Tuple<int, int>> toRelease = new List<Tuple<int, int>>();
            foreach (KeyValuePair<int, List<ReinoPrisionerState>> kv in m_InmatesByCity)
            {
                List<ReinoPrisionerState> list = kv.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    ReinoPrisionerState inmate = list[i];
                    if (inmate != null && inmate.ReleaseUtc <= DateTime.UtcNow)
                        toRelease.Add(new Tuple<int, int>(kv.Key, inmate.CellIndex));
                }
            }

            for (int i = 0; i < toRelease.Count; i++)
            {
                string unused;
                ReleaseInmate(toRelease[i].Item1, toRelease[i].Item2, "Fim da pena", out unused);
            }
        }

        private static void ProcessDailyChargesAndMeals()
        {
            DateTime localNow = DateTime.Now;
            if (localNow.Hour < 12)
                return;

            foreach (KeyValuePair<int, List<ReinoPrisionerState>> kv in m_InmatesByCity)
            {
                ReinoPrisionSettings settings = GetSettings(kv.Key);
                if (settings.LastDailyChargeLocalDate.Date == localNow.Date)
                    continue;

                int count = kv.Value != null ? kv.Value.Count : 0;
                if (count > 0)
                {
                    settings.PendingWeeklyGold += count * 5;
                    if (settings.FeedPrisoners)
                        SpawnMealsForOccupiedCells(kv.Key);
                }

                settings.LastDailyChargeLocalDate = localNow.Date;
            }
        }

        private static void SpawnMealsForOccupiedCells(int cityId)
        {
            List<ReinoPrisionerState> list = GetInmates(cityId);
            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null)
                return;

            for (int i = 0; i < list.Count; i++)
                SpawnMealForInmate(cityId, list[i]);
        }

        private static void SpawnMealForInmate(int cityId, ReinoPrisionerState inmate)
        {
            if (inmate == null || inmate.CellIndex < 0)
                return;

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null)
                return;

            Point3D cell = GetCellPoint(prison.Lot, inmate.CellIndex);
            RefeicaoDoPreso meal = new RefeicaoDoPreso();
            meal.MoveToWorld(cell, prison.Lot.Map);
        }

        private static bool ConfiscateBelongings(PlayerMobile pm, int cityId, out Bag bag)
        {
            bag = new Bag();
            bag.Name = "pertences de " + pm.Name;
            bag.Movable = true;

            List<Item> toMove = new List<Item>();

            if (pm.Backpack != null)
            {
                for (int i = 0; i < pm.Backpack.Items.Count; i++)
                {
                    Item item = pm.Backpack.Items[i];
                    if (item != null && !item.Deleted && item.Movable)
                        toMove.Add(item);
                }
            }

            for (int i = 0; i < pm.Items.Count; i++)
            {
                Item item = pm.Items[i];
                if (item == null || item.Deleted || !item.Movable)
                    continue;

                if (item.Layer == Layer.Backpack || item is BankBox)
                    continue;

                toMove.Add(item);
            }

            for (int i = 0; i < toMove.Count; i++)
            {
                Item item = toMove[i];
                if (item == null || item.Deleted)
                    continue;

                bag.DropItem(item);
            }

            if (bag.Items.Count == 0)
            {
                bag.Delete();
                bag = null;
                return false;
            }

            ReinoPrisonLocker locker = FindPrisonLocker(cityId);
            if (locker == null)
            {
                bag.Delete();
                bag = null;
                return false;
            }

            locker.DropItem(bag);
            return true;
        }

        private static void ReturnBelongings(PlayerMobile pm, int bagSerial)
        {
            if (pm == null || pm.Deleted || bagSerial == 0)
                return;

            Bag bag = World.FindItem((Serial)bagSerial) as Bag;
            if (bag == null || bag.Deleted)
                return;

            if (pm.Backpack == null)
                pm.AddItem(new Backpack());

            pm.Backpack.DropItem(bag);
        }

        private static void ReturnBelongingsToBank(PlayerMobile pm, int bagSerial)
        {
            if (pm == null || pm.Deleted || bagSerial == 0)
                return;

            Bag bag = World.FindItem((Serial)bagSerial) as Bag;
            if (bag == null || bag.Deleted)
                return;

            if (pm.BankBox == null)
                return;

            pm.BankBox.DropItem(bag);
        }

        public static bool SendInmateToTribunal(int cityId, int cellIndex, out string message)
        {
            message = String.Empty;

            if (!ReinoTrialsSystem.HasTribunal(cityId))
            {
                message = "Não existe tribunal construído nesse reino.";
                return false;
            }

            if (!ReinoTrialsSystem.HasActiveSession(cityId))
            {
                message = "Não existe sessão de tribunal iniciada.";
                return false;
            }

            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            if (pm == null || pm.Deleted)
            {
                message = "O preso não está disponível.";
                return false;
            }

            Point3D point;
            Map map;
            if (!ReinoTrialsSystem.TryGetAccusedLocation(cityId, out point, out map))
            {
                message = "O tribunal não está configurado corretamente.";
                return false;
            }

            pm.MoveToWorld(point, map);
            pm.CantWalk = true;
            inmate.InTribunal = true;
            inmate.InInterrogation = false;

            ReinoTrialsSystem.SetActiveAccused(cityId, inmate.PrisonerSerial, inmate.PrisonerName);

            message = "O preso foi enviado ao tribunal.";
            return true;
        }

        public static bool ReturnInmateFromTribunal(int cityId, int prisonerSerial, int hours, int fineGold, string judgeName, DateTime judgedUtc, out string message)
        {
            message = String.Empty;

            ReinoPrisionerState inmate = GetInmateBySerial(cityId, prisonerSerial);
            if (inmate == null)
            {
                message = "O réu não está registrado na prisão.";
                return false;
            }

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null)
            {
                message = "A prisão do reino não foi encontrada.";
                return false;
            }

            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(Math.Max(1, hours));
            inmate.SentenceHours = Math.Max(1, hours);
            inmate.FineGold = Math.Max(0, fineGold);
            inmate.FinePaid = false;
            inmate.FineGumpShown = false;
            inmate.Judged = true;
            inmate.JudgeName = judgeName ?? String.Empty;
            inmate.JudgedUtc = judgedUtc == DateTime.MinValue ? DateTime.UtcNow : judgedUtc;
            inmate.InTribunal = false;

            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            if (pm != null && !pm.Deleted)
            {
                pm.CantWalk = false;
                pm.MoveToWorld(GetCellPoint(prison.Lot, inmate.CellIndex), prison.Lot.Map);

                if (inmate.FineGold > 0 && GetSettings(cityId).AllowFinePayment)
                    ShowFineGump(pm, inmate);
            }

            message = "O réu voltou para a prisão.";
            return true;
        }

        public static bool ReleaseInmateToBankFromTribunal(int cityId, int prisonerSerial, string releasedBy, out string message)
        {
            message = String.Empty;

            ReinoPrisionerState inmate = GetInmateBySerial(cityId, prisonerSerial);
            if (inmate == null)
            {
                message = "O réu não está registrado na prisão.";
                return false;
            }

            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            if (pm != null && !pm.Deleted)
            {
                pm.CantWalk = false;
                ReturnBelongingsToBank(pm, inmate.BelongingsBagSerial);
            }

            GetInmates(cityId).Remove(inmate);
            message = "O réu foi considerado livre e seus pertences foram enviados ao banco.";
            return true;
        }

        private static void EquipPrisonUniform(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            DeleteLayerItem(pm, Layer.InnerTorso);
            DeleteLayerItem(pm, Layer.Shirt);
            DeleteLayerItem(pm, Layer.Pants);
            DeleteLayerItem(pm, Layer.Shoes);
            DeleteLayerItem(pm, Layer.OneHanded);
            DeleteLayerItem(pm, Layer.TwoHanded);
            DeleteLayerItem(pm, Layer.MiddleTorso);
            DeleteLayerItem(pm, Layer.OuterTorso);
            DeleteLayerItem(pm, Layer.OuterLegs);

            UniformePrisaoShirt shirt = new UniformePrisaoShirt();
            UniformePrisaoPants pants = new UniformePrisaoPants();

            pm.AddItem(shirt);
            pm.AddItem(pants);
        }

        private static void RemovePrisonUniform(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            Item item = pm.FindItemOnLayer(Layer.Shirt);
            if (item is Shirt && !item.Movable)
                item.Delete();

            item = pm.FindItemOnLayer(Layer.Pants);
            if (item is LongPants && !item.Movable)
                item.Delete();
        }

        private static void DeleteLayerItem(Mobile m, Layer layer)
        {
            Item item = m.FindItemOnLayer(layer);
            if (item != null && !item.Deleted && item.Movable)
                item.Delete();
        }

        private static void SetOuterDoorsLocked(int cityId, bool locked)
        {
            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.LotState == null || prison.LotState.DoorSerials == null)
                return;

            ReinoPrisionSettings settings = GetSettings(cityId);
            HashSet<int> cellDoors = new HashSet<int>(settings.CellDoorSerials);

            for (int i = 0; i < prison.LotState.DoorSerials.Count; i++)
            {
                int serial = prison.LotState.DoorSerials[i];
                if (cellDoors.Contains(serial))
                    continue;

                BaseDoor door = World.FindItem((Serial)serial) as BaseDoor;
                if (door == null || door.Deleted)
                    continue;

                door.Locked = locked;
                if (!locked)
                    door.Open = true;
                else
                    door.Open = false;
            }
        }

        private static ReinoConstructionRuntimeInfo FindPrimaryPrisonRuntime(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null)
                    continue;

                if (String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase)
                    && (info.Status == ReinoLotStatus.Active || info.Status == ReinoLotStatus.UnderConstruction))
                    return info;
            }

            return null;
        }

        public static ReinoPrisonLocker FindPrisonLocker(int cityId)
        {
            foreach (Item item in World.Items.Values)
            {
                ReinoPrisonLocker locker = item as ReinoPrisonLocker;
                if (locker != null && !locker.Deleted && locker.CityId == cityId)
                    return locker;
            }

            return null;
        }

        public static List<ReinoPrisionerState> GetAllActiveInmatesSorted(int cityId)
        {
            List<ReinoPrisionerState> list = new List<ReinoPrisionerState>(GetInmates(cityId));
            list.Sort(delegate (ReinoPrisionerState a, ReinoPrisionerState b)
            {
                int ax = a != null ? a.CellIndex : -1;
                int bx = b != null ? b.CellIndex : -1;
                return ax.CompareTo(bx);
            });
            return list;
        }

        public static void Save()
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(2);

                bw.Write(m_SettingsByCity.Count);
                foreach (KeyValuePair<int, ReinoPrisionSettings> kv in m_SettingsByCity)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.FeedPrisoners);
                    bw.Write(kv.Value.AllowFinePayment);
                    bw.Write(kv.Value.OuterDoorsLocked);
                    bw.Write(kv.Value.PendingWeeklyGold);
                    bw.Write(kv.Value.LastDailyChargeLocalDate.ToBinary());
                    for (int i = 0; i < 5; i++)
                        bw.Write(kv.Value.CellDoorSerials[i]);
                }

                bw.Write(m_InmatesByCity.Count);
                foreach (KeyValuePair<int, List<ReinoPrisionerState>> kv in m_InmatesByCity)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoPrisionerState inmate = kv.Value[i];
                        bw.Write(inmate.PrisonerSerial);
                        bw.Write(inmate.PrisonerName ?? String.Empty);
                        bw.Write(inmate.CrimeLabel ?? String.Empty);
                        bw.Write(inmate.ArrestUtc.ToBinary());
                        bw.Write(inmate.ReleaseUtc.ToBinary());
                        bw.Write(inmate.SentenceHours);
                        bw.Write(inmate.CellIndex);
                        bw.Write(inmate.InInterrogation);
                        bw.Write(inmate.JudgeName ?? String.Empty);
                        bw.Write(inmate.Judged);
                        bw.Write(inmate.JudgedUtc.ToBinary());
                        bw.Write(inmate.FineGold);
                        bw.Write(inmate.FinePaid);
                        bw.Write(inmate.FineGumpShown);
                        bw.Write(inmate.InTribunal);
                        bw.Write(inmate.BelongingsBagSerial);
                        bw.Write(inmate.Notes ?? String.Empty);
                    }
                }
            }
        }

        public static void Load()
        {
            m_SettingsByCity.Clear();
            m_InmatesByCity.Clear();

            if (!File.Exists(FilePath))
                return;

            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                int version = br.ReadInt32();
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int cityId = br.ReadInt32();
                    ReinoPrisionSettings settings = new ReinoPrisionSettings();
                    settings.CityId = cityId;
                    settings.FeedPrisoners = br.ReadBoolean();
                    settings.AllowFinePayment = br.ReadBoolean();
                    settings.OuterDoorsLocked = br.ReadBoolean();
                    settings.PendingWeeklyGold = br.ReadInt32();
                    settings.LastDailyChargeLocalDate = DateTime.FromBinary(br.ReadInt64());
                    for (int x = 0; x < 5; x++)
                        settings.CellDoorSerials[x] = br.ReadInt32();
                    m_SettingsByCity[cityId] = settings;
                }

                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int cityId = br.ReadInt32();
                    int inner = br.ReadInt32();
                    List<ReinoPrisionerState> list = new List<ReinoPrisionerState>();
                    for (int x = 0; x < inner; x++)
                    {
                        ReinoPrisionerState inmate = new ReinoPrisionerState();
                        inmate.CityId = cityId;
                        inmate.PrisonerSerial = br.ReadInt32();
                        inmate.PrisonerName = br.ReadString();
                        inmate.CrimeLabel = br.ReadString();
                        inmate.ArrestUtc = DateTime.FromBinary(br.ReadInt64());
                        inmate.ReleaseUtc = DateTime.FromBinary(br.ReadInt64());
                        inmate.SentenceHours = br.ReadInt32();
                        inmate.CellIndex = br.ReadInt32();
                        inmate.InInterrogation = br.ReadBoolean();
                        inmate.JudgeName = br.ReadString();
                        inmate.Judged = br.ReadBoolean();
                        inmate.JudgedUtc = version >= 2 ? DateTime.FromBinary(br.ReadInt64()) : DateTime.MinValue;
                        inmate.FineGold = br.ReadInt32();
                        inmate.FinePaid = br.ReadBoolean();
                        inmate.FineGumpShown = br.ReadBoolean();
                        inmate.InTribunal = version >= 2 ? br.ReadBoolean() : false;
                        inmate.BelongingsBagSerial = br.ReadInt32();
                        inmate.Notes = br.ReadString();
                        list.Add(inmate);
                    }
                    m_InmatesByCity[cityId] = list;
                }
            }
        }
    }
}
