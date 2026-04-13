using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Custom.Reinos
{
    public static class ReinoPrisionSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoPrision_v2.bin");

        private static readonly Dictionary<int, ReinoPrisionSettings> m_SettingsByCity = new Dictionary<int, ReinoPrisionSettings>();
        private static readonly Dictionary<int, List<ReinoPrisionerState>> m_InmatesByCity = new Dictionary<int, List<ReinoPrisionerState>>();
        private static readonly Dictionary<int, ReinoPrisionSession> m_Sessions = new Dictionary<int, ReinoPrisionSession>();
        private static Timer m_PulseTimer;

        public static void Initialize()
        {
            Load();
            EnsureDefaults();
            EventSink.WorldSave += delegate { Save(); };
            EventSink.Login += EventSink_Login;

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

        private static void EventSink_Login(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), delegate
            {
                TryFinalizePendingRelease(pm);
            });
        }

        private static void TryFinalizePendingRelease(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            foreach (KeyValuePair<int, List<ReinoPrisionerState>> kv in m_InmatesByCity)
            {
                List<ReinoPrisionerState> list = kv.Value;
                if (list == null)
                    continue;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    ReinoPrisionerState inmate = list[i];
                    if (inmate == null || !inmate.ReleasePending || inmate.PrisonerSerial != pm.Serial.Value)
                        continue;

                    UnlockPrisonUniform(pm);

                    if (inmate.ReleasePendingToBank)
                        ReturnBelongingsToBank(pm, inmate.BelongingsBagSerial);
                    else
                        ReturnBelongings(pm, inmate.BelongingsBagSerial);

                    pm.CantWalk = false;
                    OpenOuterDoorsForRelease(kv.Key);
                    SetCellDoorOccupiedState(kv.Key, inmate.CellIndex, false);

                    pm.CloseGump(typeof(ReinoPrisionFineGump));

                    OSUCarcereiro carcereiro = FindCarcereiro(kv.Key);
                    if (carcereiro != null && !carcereiro.Deleted)
                        carcereiro.Say("Aqui estão seus pertences, dá o fora daqui!");
                    else
                        pm.SendMessage("Aqui estão seus pertences, dá o fora daqui!");

                    list.RemoveAt(i);
                    return;
                }
            }
        }

        private static void MoveKnockoutCorpseToCellAfterDelay(PlayerMobile pm, Point3D dest, Map map)
        {
            if (pm == null || pm.Deleted)
                return;

            Corpse corpse = pm.Corpse as Corpse;
            if (corpse == null || corpse.Deleted)
                return;

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), delegate
            {
                if (pm == null || pm.Deleted)
                    return;

                Corpse currentCorpse = pm.Corpse as Corpse;
                if (currentCorpse == null || currentCorpse.Deleted)
                    return;

                try
                {
                    currentCorpse.MoveToWorld(dest, map);
                }
                catch
                {
                }
            });
        }
        private static OSUCarcereiro FindCarcereiro(int cityId)
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                OSUCarcereiro npc = m as OSUCarcereiro;
                if (npc != null && !npc.Deleted && npc.CityId == cityId)
                    return npc;
            }

            return null;
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
                if (info == null || info.Definition == null)
                    continue;

                if (String.Equals(info.Definition.Id, TribunalAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase)
                    && info.Status == ReinoLotStatus.Active)
                    return true;
            }

            return false;
        }

        public static bool CanAccessPrisonControl(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

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

        public static List<int> GetOccupiedCellIndices(int cityId)
        {
            List<int> list = new List<int>();
            List<ReinoPrisionerState> inmates = GetInmates(cityId);

            for (int i = 0; i < inmates.Count; i++)
            {
                ReinoPrisionerState inmate = inmates[i];
                if (inmate == null || inmate.ReleasePending || inmate.CellIndex < 0 || inmate.CellIndex >= 5)
                    continue;

                if (!list.Contains(inmate.CellIndex))
                    list.Add(inmate.CellIndex);
            }

            list.Sort();
            return list;
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

            return false;
        }

        public static ReinoPrisionerState GetInmateByCell(int cityId, int cellIndex)
        {
            List<ReinoPrisionerState> list = GetInmates(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoPrisionerState inmate = list[i];
                if (inmate != null && !inmate.ReleasePending && inmate.CellIndex == cellIndex)
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
                if (inmate != null && !inmate.ReleasePending && inmate.PrisonerSerial == prisonerSerial)
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
            sb.Append("<BASEFONT COLOR=#FFFFFF>");

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
            {
                sb.Append("<B>Quem julgou:</B> ").Append(inmate.JudgeName).Append("<BR>");
                if (inmate.JudgedUtc > DateTime.MinValue)
                    sb.Append("<B>Quando:</B> ").Append(inmate.JudgedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
            }

            sb.Append("<B>Multa:</B> ").Append(inmate.FineGold).Append(" moedas<BR>");
            sb.Append("<B>Interrogatório:</B> ").Append(inmate.InInterrogation ? "sim" : "não").Append("<BR>");
            sb.Append("<B>No tribunal:</B> ").Append(inmate.InTribunal ? "sim" : "não").Append("<BR>");

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
            {
                if (settings.CellDoorSerials[i] <= 0)
                    return false;
            }

            return true;
        }

        public static bool IsCellDoorLinked(int cityId, int cellIndex)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);

            return cellIndex >= 0
                && cellIndex < settings.CellDoorSerials.Length
                && settings.CellDoorSerials[cellIndex] > 0;
        }

        public static bool IsCellDoorOpen(int cityId, int cellIndex)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);

            if (cellIndex < 0 || cellIndex >= settings.CellDoorSerials.Length)
                return false;

            int serialValue = settings.CellDoorSerials[cellIndex];
            if (serialValue <= 0)
                return false;

            BaseDoor door = World.FindItem((Serial)serialValue) as BaseDoor;
            if (door == null || door.Deleted)
                return false;

            return door.Open || !door.Locked;
        }

        private static List<BaseDoor> CollectPrisonLotDoors(int cityId)
        {
            List<BaseDoor> list = new List<BaseDoor>();
            HashSet<int> seen = new HashSet<int>();

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null || prison.Lot.Map == null || prison.Lot.Map == Map.Internal)
                return list;

            Rectangle2D rect = prison.Lot.Rect;
            int minZ = prison.Lot.NorthWest.Z - 20;
            int maxZ = prison.Lot.NorthWest.Z + 40;
            IPooledEnumerable eable = prison.Lot.Map.GetItemsInBounds(rect);

            foreach (Item item in eable)
            {
                BaseDoor door = item as BaseDoor;
                if (door == null || door.Deleted || door.RootParent != null)
                    continue;

                if (!rect.Contains(new Point2D(door.X, door.Y)))
                    continue;

                if (door.Z < minZ || door.Z > maxZ)
                    continue;

                if (seen.Add(door.Serial.Value))
                    list.Add(door);
            }

            eable.Free();

            if (prison.LotState != null)
            {
                prison.LotState.DoorSerials.Clear();
                for (int i = 0; i < list.Count; i++)
                    prison.LotState.DoorSerials.Add(list[i].Serial.Value);
            }

            return list;
        }

        private static bool DoorBelongsToPrisonLot(int cityId, BaseDoor door)
        {
            if (door == null || door.Deleted)
                return false;

            List<BaseDoor> doors = CollectPrisonLotDoors(cityId);
            for (int i = 0; i < doors.Count; i++)
            {
                if (doors[i] != null && doors[i].Serial == door.Serial)
                    return true;
            }

            return false;
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

            if (!DoorBelongsToPrisonLot(cityId, door))
            {
                message = "Essa porta não pertence ao lote da prisão.";
                return false;
            }

            ReinoPrisionSettings settings = GetSettings(cityId);
            settings.CellDoorSerials[cellIndex] = door.Serial.Value;

            SetCellDoorOccupiedState(cityId, cellIndex, GetInmateByCell(cityId, cellIndex) != null);

            message = String.Format("Porta da cela {0} vinculada.", cellIndex + 1);
            return true;
        }

        private static void SetCellDoorOccupiedState(int cityId, int cellIndex, bool occupied)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);

            if (cellIndex < 0 || cellIndex >= settings.CellDoorSerials.Length)
                return;

            int serialValue = settings.CellDoorSerials[cellIndex];
            if (serialValue <= 0)
                return;

            BaseDoor door = World.FindItem((Serial)serialValue) as BaseDoor;
            if (door == null || door.Deleted)
                return;

            door.Locked = occupied;
            door.Open = !occupied;
        }

        private static void OpenOuterDoorsForRelease(int cityId)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);
            settings.OuterDoorsLocked = false;

            HashSet<int> cellDoors = new HashSet<int>(settings.CellDoorSerials);
            List<BaseDoor> doors = CollectPrisonLotDoors(cityId);

            for (int i = 0; i < doors.Count; i++)
            {
                BaseDoor door = doors[i];
                if (door == null || door.Deleted)
                    continue;

                if (cellDoors.Contains(door.Serial.Value))
                    continue;

                door.Locked = false;
                door.Open = true;
            }
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

            bool aberta = door.Open || !door.Locked;

            if (aberta)
            {
                door.Open = false;
                door.Locked = true;
                message = String.Format("Cela {0} fechada.", cellIndex + 1);
            }
            else
            {
                door.Locked = false;
                door.Open = true;
                message = String.Format("Cela {0} aberta.", cellIndex + 1);
            }

            return true;
        }

        public static bool AdjustRemainingHours(int cityId, int cellIndex, int hours, string actorName, out string message)
        {
            message = String.Empty;

            if (HasTribunal(cityId))
            {
                message = "Quando existe tribunal, a pena deve ser ajustada pelo juiz.";
                return false;
            }

            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            if (hours <= 0)
                return ReleaseInmate(cityId, cellIndex, actorName ?? "administração", out message);

            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(hours);
            inmate.SentenceHours = hours;
            inmate.Notes = String.Format("Pena ajustada por {0}.", actorName ?? "administração");

            UpdateLatestPrisonRecord(inmate.CityId, inmate.PrisonerSerial, inmate.CrimeLabel, inmate.SentenceHours, inmate.FineGold, inmate.Notes, false);
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

            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (pm == null || prison == null || prison.Lot == null)
            {
                message = "Preso ou prisão não encontrados.";
                return false;
            }

            if (inmate.InTribunal)
            {
                message = "O preso já está no tribunal.";
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
                Point3D cellPoint = GetCellPoint(prison.Lot, inmate.CellIndex);
                pm.MoveToWorld(cellPoint, prison.Lot.Map);
                pm.CantWalk = false;
                inmate.InInterrogation = false;
                message = "Preso devolvido à cela.";
            }

            return true;
        }

        public static bool SendInmateToTribunal(int cityId, int cellIndex, out string message)
        {
            message = String.Empty;

            if (!HasTribunal(cityId))
            {
                message = "Não existe um tribunal ativo para este reino.";
                return false;
            }

            if (!ReinoTrialsSystem.HasActiveSession(cityId))
            {
                message = "Nenhuma sessão do tribunal foi iniciada.";
                return false;
            }

            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            int accusedSerial = ReinoTrialsSystem.GetActiveAccusedSerial(cityId);
            if (accusedSerial > 0 && accusedSerial != inmate.PrisonerSerial)
            {
                message = "A sessão atual foi aberta para outro réu.";
                return false;
            }

            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            ReinoConstructionRuntimeInfo tribunal = ReinoTrialsSystem.FindPrimaryTribunalRuntime(cityId);
            if (pm == null || tribunal == null || tribunal.Lot == null)
            {
                message = "Réu ou tribunal não encontrados.";
                return false;
            }

            pm.Combatant = null;
            pm.Warmode = false;
            pm.CantWalk = true;
            pm.MoveToWorld(ReinoTrialsSystem.GetAccusedPoint(cityId), tribunal.Lot.Map);

            inmate.InInterrogation = false;
            inmate.InTribunal = true;
            message = "Preso enviado ao tribunal.";
            return true;
        }

        public static bool ReturnInmateFromTribunalToCell(int cityId, int prisonerSerial, out string message)
        {
            message = String.Empty;

            ReinoPrisionerState inmate = GetInmateBySerial(cityId, prisonerSerial);
            if (inmate == null)
            {
                message = "Preso não encontrado.";
                return false;
            }

            PlayerMobile pm = World.FindMobile((Serial)prisonerSerial) as PlayerMobile;
            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (pm == null || prison == null || prison.Lot == null)
            {
                message = "Preso ou prisão não encontrados.";
                return false;
            }

            pm.CantWalk = false;
            pm.MoveToWorld(GetCellPoint(prison.Lot, inmate.CellIndex), prison.Lot.Map);
            inmate.InTribunal = false;
            inmate.InInterrogation = false;
            message = "Preso devolvido para a cela.";
            return true;
        }

        public static bool ReleaseInmateToBank(int cityId, int prisonerSerial, string releasedBy, out string message)
        {
            ReinoPrisionerState inmate = GetInmateBySerial(cityId, prisonerSerial);
            if (inmate == null)
            {
                message = "Preso não encontrado.";
                return false;
            }

            return ReleaseInmateInternal(inmate, releasedBy, false, true, false, out message);
        }

        public static bool ReleaseInmate(int cityId, int cellIndex, string releasedBy, out string message)
        {
            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            return ReleaseInmateInternal(inmate, releasedBy, false, false, false, out message);
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
            return ReleaseInmateInternal(inmate, pm.Name, true, false, false, out message);
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

            int cellIndex;
            Point3D dest;
            Map destMap;
            if (!FindFirstEmptyPrisonCell(cityId, out cellIndex, out dest, out destMap))
            {
                pm.SendMessage("Todas as celas da prisão estão ocupadas.");
                return false;
            }

            ReinoWantedEntry wantedEntry = ReinoMilitarySystem.FindWanted(cityId, pm);

            int sentenceHours = 48;
            int fineGold = 5000;

            if (wantedEntry == null && ReinoTrialsSystem.HasTribunal(cityId))
            {
                sentenceHours = ReinoTrialsSystem.GetLawDefaultHours(cityId, law);
                fineGold = ReinoTrialsSystem.GetLawDefaultFine(cityId, law);
            }

            if (!GetSettings(cityId).AllowFinePayment)
                fineGold = 0;

            int belongingsBagSerial = 0;

            Bag existingBag = TryMoveNamedBagFromBarracksToPrisonLocker(pm, cityId);
            if (existingBag != null && !existingBag.Deleted)
            {
                belongingsBagSerial = existingBag.Serial.Value;
            }
            else
            {
                Bag bag;
                if (ConfiscateBelongings(pm, cityId, out bag) && bag != null && !bag.Deleted)
                    belongingsBagSerial = bag.Serial.Value;
            }

            EquipPrisonUniform(pm);

            pm.Combatant = null;
            pm.Warmode = false;
            pm.CantWalk = false;

            Map finalMap = destMap != null ? destMap : prison.Lot.Map;

            if (!pm.Alive && pm.Corpse is Corpse)
            {
                MoveKnockoutCorpseToCellAfterDelay(pm, dest, finalMap);
            }
            else
            {
                pm.MoveToWorld(dest, finalMap);
            }

            ReinoPrisionerState inmate = new ReinoPrisionerState();
            inmate.CityId = cityId;
            inmate.PrisonerSerial = pm.Serial.Value;
            inmate.PrisonerName = pm.Name;
            inmate.CrimeLabel = wantedEntry != null ? "Pessoa procurada" : ReinoMilitarySystem.GetLawLabel(law);
            inmate.SourceLawId = wantedEntry != null ? -1 : (int)law;
            inmate.ArrestUtc = DateTime.UtcNow;
            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(sentenceHours);
            inmate.SentenceHours = sentenceHours;
            inmate.CellIndex = cellIndex;
            inmate.InInterrogation = false;
            inmate.InTribunal = false;
            inmate.JudgeName = String.Empty;
            inmate.Judged = false;
            inmate.JudgedUtc = DateTime.MinValue;
            inmate.FineGold = fineGold;
            inmate.FinePaid = false;
            inmate.FineGumpShown = false;
            inmate.BelongingsBagSerial = belongingsBagSerial;
            inmate.Notes = guard != null ? "Preso pelos guardas do reino." : "Prisão administrativa.";

            GetInmates(cityId).Add(inmate);
            GetSettings(cityId).PendingWeeklyGold += 10;
            SetCellDoorOccupiedState(cityId, cellIndex, true);

            ReinoMilitarySystem.AddPrisonRecord(cityId, pm, guard, law, sentenceHours, inmate.Notes);
            UpdateLatestPrisonRecord(cityId, pm.Serial.Value, inmate.CrimeLabel, sentenceHours, fineGold, inmate.Notes, true);

            if (GetSettings(cityId).FeedPrisoners)
                SpawnMealForInmate(cityId, inmate);

            if (fineGold > 0)
                ShowFineGump(pm, inmate);

            pm.SendMessage("Você foi levado para a prisão do reino.");
            return true;
        }

        public static bool TryAdministrativeArrest(PlayerMobile prisoner, int cityId, PlayerMobile actor, out string message)
        {
            message = String.Empty;

            if (prisoner == null || prisoner.Deleted || prisoner.Map == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            ReinoConstructionRuntimeInfo prison = FindPrimaryPrisonRuntime(cityId);
            if (prison == null || prison.Lot == null || prison.Lot.Map == null)
            {
                message = "Não existe prisão construída nesse reino.";
                return false;
            }

            if (prisoner.Map != prison.Lot.Map || !prison.Lot.Contains(prisoner.Location))
            {
                message = "O jogador precisa estar dentro do lote da prisão.";
                return false;
            }

            RemoveExistingInmate(cityId, prisoner.Serial.Value);

            int cellIndex;
            Point3D dest;
            Map destMap;

            if (!FindFirstEmptyPrisonCell(cityId, out cellIndex, out dest, out destMap))
            {
                message = "Todas as celas da prisão estão ocupadas.";
                return false;
            }

            int sentenceHours = 48;
            int fineGold = 0;

            if (GetSettings(cityId).AllowFinePayment && !HasTribunal(cityId))
                fineGold = 5000;

            int belongingsBagSerial = 0;

            Bag existingBag = TryMoveNamedBagFromBarracksToPrisonLocker(prisoner, cityId);
            if (existingBag != null && !existingBag.Deleted)
            {
                belongingsBagSerial = existingBag.Serial.Value;
            }
            else
            {
                Bag bag;
                if (ConfiscateBelongings(prisoner, cityId, out bag) && bag != null && !bag.Deleted)
                    belongingsBagSerial = bag.Serial.Value;
            }

            EquipPrisonUniform(prisoner);

            prisoner.Combatant = null;
            prisoner.Warmode = false;
            prisoner.CantWalk = false;
            prisoner.MoveToWorld(dest, destMap != null ? destMap : prison.Lot.Map);

            ReinoPrisionerState inmate = new ReinoPrisionerState();
            inmate.CityId = cityId;
            inmate.PrisonerSerial = prisoner.Serial.Value;
            inmate.PrisonerName = prisoner.Name;
            inmate.CrimeLabel = "Prisão administrativa";
            inmate.ArrestUtc = DateTime.UtcNow;
            inmate.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(sentenceHours);
            inmate.SourceLawId = -1;
            inmate.SentenceHours = sentenceHours;
            inmate.CellIndex = cellIndex;
            inmate.InInterrogation = false;
            inmate.InTribunal = false;
            inmate.JudgeName = String.Empty;
            inmate.Judged = false;
            inmate.JudgedUtc = DateTime.MinValue;
            inmate.FineGold = fineGold;
            inmate.FinePaid = false;
            inmate.FineGumpShown = false;
            inmate.BelongingsBagSerial = belongingsBagSerial;
            inmate.Notes = actor != null ? "Preso manualmente por " + actor.Name + "." : "Prisão administrativa.";

            GetInmates(cityId).Add(inmate);
            GetSettings(cityId).PendingWeeklyGold += 10;
            SetCellDoorOccupiedState(cityId, cellIndex, true);

            UpdateLatestPrisonRecord(cityId, prisoner.Serial.Value, inmate.CrimeLabel, sentenceHours, fineGold, inmate.Notes, true);

            if (GetSettings(cityId).FeedPrisoners)
                SpawnMealForInmate(cityId, inmate);

            if (fineGold > 0)
                ShowFineGump(prisoner, inmate);

            message = prisoner.Name + " foi levado para a cela.";
            return true;
        }

        public static bool ResendFineGumpToViewedInmate(int cityId, int cellIndex, PlayerMobile actor, out string message)
        {
            message = String.Empty;

            ReinoPrisionerState inmate = GetInmateByCell(cityId, cellIndex);
            if (inmate == null)
            {
                message = "Não há preso nessa cela.";
                return false;
            }

            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            if (pm == null || pm.Deleted)
            {
                message = "O preso não está online para receber o gump da multa.";
                return false;
            }

            int fine = inmate.Judged
             ? inmate.FineGold
             : ResolveDefaultFineForInmate(inmate);

            if (!GetSettings(cityId).AllowFinePayment)
            {
                message = "As multas estão desabilitadas nessa prisão.";
                return false;
            }

            inmate.FineGold = Math.Max(0, fine);
            inmate.FineGumpShown = false;
            ShowFineGump(pm, inmate);

            message = "O gump da multa foi enviado novamente para o preso.";
            return true;
        }

        private static int ResolveDefaultFineForInmate(ReinoPrisionerState inmate)
        {
            if (inmate == null)
                return 5000;

            if (inmate.SourceLawId >= 0 && ReinoTrialsSystem.HasTribunal(inmate.CityId))
                return ReinoTrialsSystem.GetLawDefaultFine(inmate.CityId, (ReinoMilitaryLaw)inmate.SourceLawId);

            return 5000;
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
            inmate.InTribunal = false;
            inmate.Notes = "Pena decretada pelo tribunal.";

            UpdateLatestPrisonRecord(cityId, prisonerSerial, inmate.CrimeLabel, hours, inmate.FineGold, inmate.Notes, false);

            PlayerMobile pm = World.FindMobile((Serial)prisonerSerial) as PlayerMobile;
            if (pm != null)
            {
                string unused;
                ReturnInmateFromTribunalToCell(cityId, prisonerSerial, out unused);

                if (inmate.FineGold > 0 && GetSettings(cityId).AllowFinePayment)
                    ShowFineGump(pm, inmate);
            }

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
                    if (inmate == null || inmate.ReleasePending || inmate.FinePaid || inmate.FineGold <= 0 || inmate.InTribunal)
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

        private static bool ReleaseInmateInternal(ReinoPrisionerState inmate, string releasedBy, bool byFine, bool toBank, bool moveOutside, out string message)
        {
            message = "Preso solto.";
            if (inmate == null)
            {
                message = "Preso inválido.";
                return false;
            }

            int cityId = inmate.CityId;
            PlayerMobile pm = World.FindMobile((Serial)inmate.PrisonerSerial) as PlayerMobile;
            bool online = pm != null && !pm.Deleted && pm.NetState != null;

            if (pm != null && !pm.Deleted)
            {
                UnlockPrisonUniform(pm);
                pm.CantWalk = false;

                if (toBank)
                {
                    ReturnBelongingsToBank(pm, inmate.BelongingsBagSerial);
                }
                else if (online)
                {
                    ReturnBelongings(pm, inmate.BelongingsBagSerial);
                }
                else
                {
                    inmate.ReleasePending = true;
                    inmate.ReleasePendingToBank = false;
                }
            }
            else
            {
                inmate.ReleasePending = true;
                inmate.ReleasePendingToBank = toBank;
            }

            OpenOuterDoorsForRelease(cityId);
            SetCellDoorOccupiedState(cityId, inmate.CellIndex, false);

            MarkLatestPrisonRecordReleased(cityId, inmate.PrisonerSerial, releasedBy, byFine || toBank);

            if (inmate.ReleasePending)
            {
                message = "A soltura ficará pendente até o jogador logar.";
                return true;
            }

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
                ReleaseInmateInternal(existing, "Sistema", false, false, false, out unused);
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
                    if (inmate != null && !inmate.ReleasePending && !inmate.InTribunal && inmate.ReleaseUtc <= DateTime.UtcNow)
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
            for (int i = 0; i < list.Count; i++)
                SpawnMealForInmate(cityId, list[i]);
        }

        private static void SpawnMealForInmate(int cityId, ReinoPrisionerState inmate)
        {
            if (inmate == null || inmate.CellIndex < 0 || inmate.InTribunal)
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

        private static Bag TryMoveNamedBagFromBarracksToPrisonLocker(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return null;

            Container barracks = ReinoMilitarySystem.FindBarracksChest(cityId);
            ReinoPrisonLocker prisonLocker = FindPrisonLocker(cityId);

            if (barracks == null || prisonLocker == null)
                return null;

            string expectedName = "pertences de " + pm.Name;
            for (int i = barracks.Items.Count - 1; i >= 0; i--)
            {
                Bag bag = barracks.Items[i] as Bag;
                if (bag == null || bag.Deleted)
                    continue;

                if (!String.Equals(bag.Name ?? String.Empty, expectedName, StringComparison.OrdinalIgnoreCase))
                    continue;

                prisonLocker.DropItem(bag);
                return bag;
            }

            return null;
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

            if (pm.BankBox != null)
                pm.BankBox.DropItem(bag);
            else
                ReturnBelongings(pm, bagSerial);
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
            shirt.Movable = false;
            pants.Movable = false;
            pm.AddItem(shirt);
            pm.AddItem(pants);
        }

        private static void UnlockPrisonUniform(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            Item item = pm.FindItemOnLayer(Layer.Shirt);
            UniformePrisaoShirt shirt = item as UniformePrisaoShirt;
            if (shirt != null && !shirt.Deleted)
                shirt.Movable = true;

            item = pm.FindItemOnLayer(Layer.Pants);
            UniformePrisaoPants pants = item as UniformePrisaoPants;
            if (pants != null && !pants.Deleted)
                pants.Movable = true;
        }

        private static void DeleteLayerItem(Mobile m, Layer layer)
        {
            Item item = m.FindItemOnLayer(layer);
            if (item != null && !item.Deleted && item.Movable)
                item.Delete();
        }

        private static void SetOuterDoorsLocked(int cityId, bool locked)
        {
            ReinoPrisionSettings settings = GetSettings(cityId);
            HashSet<int> cellDoors = new HashSet<int>(settings.CellDoorSerials);
            List<BaseDoor> doors = CollectPrisonLotDoors(cityId);

            for (int i = 0; i < doors.Count; i++)
            {
                BaseDoor door = doors[i];
                if (door == null || door.Deleted)
                    continue;

                if (cellDoors.Contains(door.Serial.Value))
                    continue;

                door.Locked = locked;
            }
        }

        private static ReinoConstructionRuntimeInfo FindPrimaryPrisonRuntime(int cityId)
        {
            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);
            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];
                if (info == null || info.Definition == null || info.Lot == null)
                    continue;

                if (String.Equals(info.Definition.Id, "prisao_aurora", StringComparison.OrdinalIgnoreCase)
                    && info.Status == ReinoLotStatus.Active)
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
            List<ReinoPrisionerState> list = new List<ReinoPrisionerState>();
            List<ReinoPrisionerState> source = GetInmates(cityId);

            for (int i = 0; i < source.Count; i++)
            {
                ReinoPrisionerState inmate = source[i];
                if (inmate != null && !inmate.ReleasePending)
                    list.Add(inmate);
            }

            list.Sort(delegate (ReinoPrisionerState a, ReinoPrisionerState b)
            {
                int ax = a != null ? a.CellIndex : -1;
                int bx = b != null ? b.CellIndex : -1;
                return ax.CompareTo(bx);
            });

            return list;
        }

        private static ReinoPrisonRecord FindLatestPrisonRecord(int cityId, int prisonerSerial)
        {
            List<ReinoPrisonRecord> list = ReinoMilitarySystem.GetPrisonList(cityId);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ReinoPrisonRecord r = list[i];
                if (r != null && r.PrisonerSerial == prisonerSerial)
                    return r;
            }

            return null;
        }

        private static void UpdateLatestPrisonRecord(int cityId, int prisonerSerial, string crimeLabel, int hours, int fineGold, string notes, bool keepArrestUtc)
        {
            ReinoPrisonRecord r = FindLatestPrisonRecord(cityId, prisonerSerial);
            if (r == null)
                return;

            if (!String.IsNullOrWhiteSpace(crimeLabel))
                r.CrimeLabel = crimeLabel;

            r.DurationHours = Math.Max(1, hours);

            if (!keepArrestUtc)
                r.ArrestUtc = DateTime.UtcNow;

            r.ReleaseUtc = DateTime.UtcNow + TimeSpan.FromHours(Math.Max(1, hours));
            r.Notes = (notes ?? String.Empty) + (fineGold > 0 ? (" Multa prevista: " + fineGold + " moedas.") : String.Empty);
        }

        private static void MarkLatestPrisonRecordReleased(int cityId, int prisonerSerial, string releasedBy, bool early)
        {
            ReinoPrisonRecord r = FindLatestPrisonRecord(cityId, prisonerSerial);
            if (r == null)
                return;

            r.ReleasedBy = releasedBy ?? String.Empty;
            r.ReleasedEarly = early;
            r.ReleaseUtc = DateTime.UtcNow;
        }

        public static void Save()
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(3);

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
                        bw.Write(inmate.InTribunal);
                        bw.Write(inmate.JudgeName ?? String.Empty);
                        bw.Write(inmate.Judged);
                        bw.Write(inmate.JudgedUtc.ToBinary());
                        bw.Write(inmate.FineGold);
                        bw.Write(inmate.FinePaid);
                        bw.Write(inmate.FineGumpShown);
                        bw.Write(inmate.BelongingsBagSerial);
                        bw.Write(inmate.Notes ?? String.Empty);
                        bw.Write(inmate.SourceLawId);
                        bw.Write(inmate.ReleasePending);
                        bw.Write(inmate.ReleasePendingToBank);
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
                        inmate.InTribunal = version >= 2 ? br.ReadBoolean() : false;
                        inmate.JudgeName = br.ReadString();
                        inmate.Judged = br.ReadBoolean();
                        inmate.JudgedUtc = version >= 2 ? DateTime.FromBinary(br.ReadInt64()) : DateTime.MinValue;
                        inmate.FineGold = br.ReadInt32();
                        inmate.FinePaid = br.ReadBoolean();
                        inmate.FineGumpShown = br.ReadBoolean();
                        inmate.BelongingsBagSerial = br.ReadInt32();
                        inmate.Notes = br.ReadString();
                        inmate.SourceLawId = version >= 3 ? br.ReadInt32() : -1;
                        inmate.ReleasePending = version >= 3 ? br.ReadBoolean() : false;
                        inmate.ReleasePendingToBank = version >= 3 ? br.ReadBoolean() : false;
                        list.Add(inmate);
                    }

                    m_InmatesByCity[cityId] = list;
                }
            }
        }
    }
}
