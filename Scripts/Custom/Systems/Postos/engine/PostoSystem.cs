using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Reinos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.Custom.Systems.Postos
{
    public static class PostoSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_Postos.bin");
        private static readonly TimeSpan ContestDuration = TimeSpan.FromDays(3.0);

        private static readonly Dictionary<string, PostoState> m_States = new Dictionary<string, PostoState>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, PostoKingdomResourceLedger> m_Ledgers = new Dictionary<string, PostoKingdomResourceLedger>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<PostoLeaderAlert> m_PendingLeaderAlerts = new List<PostoLeaderAlert>();

        public static void Initialize()
        {
            EnsureDefaults();
            Load();
            EventSink.WorldSave += delegate { Save(); };
            EventSink.CreatureDeath += OnCreatureDeath;
            EventSink.Login += OnLogin;

            Timer.DelayCall(
                TimeSpan.FromMinutes(5.0),
                TimeSpan.FromMinutes(5.0),
                SystemPulse);
        }

        private static void SystemPulse()
        {
            foreach (PostoDefinition def in PostoRegistry.All)
            {
                if (def == null)
                    continue;

                PostoState state = GetState(def.Id);
                ResolveContestIfNeeded(def, state);
                TouchProduction(def.Id);
            }
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null || pm.Deleted || !pm.OSUReinoLeader)
                return;

            int leaderCityId = pm.OSUReinoLeaderCityId;
            string leaderCityName = GetCityNameByIndex(leaderCityId);

            if (String.IsNullOrWhiteSpace(leaderCityName))
                return;

            List<PostoLeaderAlert> alerts = new List<PostoLeaderAlert>();

            for (int i = m_PendingLeaderAlerts.Count - 1; i >= 0; i--)
            {
                PostoLeaderAlert alert = m_PendingLeaderAlerts[i];

                if (alert == null)
                    continue;

                if (!SameCity(alert.DefenderCityId, leaderCityName))
                    continue;

                alerts.Add(alert);
                m_PendingLeaderAlerts.RemoveAt(i);
            }

            for (int i = alerts.Count - 1; i >= 0; i--)
                SendLeaderAlertGump(pm, alerts[i]);
        }

        private static void EnsureDefaults()
        {
            foreach (PostoDefinition def in PostoRegistry.All)
                EnsureState(def.Id);

            EnsureLedger("Aurora");
            EnsureLedger("Xetá");
            EnsureLedger("Lurone");
            EnsureLedger("Willran");
        }

        public static IEnumerable<PostoDefinition> AllDefinitions
        {
            get { return PostoRegistry.All; }
        }

        public static PostoDefinition GetDefinition(string postoId)
        {
            return PostoRegistry.Get(postoId);
        }

        public static PostoState GetState(string postoId)
        {
            return EnsureState(postoId);
        }

        public static PostoKingdomResourceLedger GetLedger(string cityId)
        {
            return EnsureLedger(PlayerMobile.NormalizeOSUCityId(cityId));
        }

        private static PostoState EnsureState(string postoId)
        {
            if (String.IsNullOrWhiteSpace(postoId))
                return null;

            PostoState state;
            if (!m_States.TryGetValue(postoId, out state))
            {
                state = new PostoState(postoId);
                m_States[postoId] = state;
            }

            if (state.ContestScores == null)
                state.ContestScores = new List<PostoContestScore>();

            return state;
        }

        private static PostoKingdomResourceLedger EnsureLedger(string cityId)
        {
            cityId = PlayerMobile.NormalizeOSUCityId(cityId);

            if (String.IsNullOrWhiteSpace(cityId))
                cityId = "Sem Reino";

            PostoKingdomResourceLedger ledger;
            if (!m_Ledgers.TryGetValue(cityId, out ledger))
            {
                ledger = new PostoKingdomResourceLedger(cityId);
                m_Ledgers[cityId] = ledger;
            }

            return ledger;
        }

        public static string[] GetKnownCities()
        {
            if (ReinoElectionsSystem.CityNames != null && ReinoElectionsSystem.CityNames.Length > 0)
                return ReinoElectionsSystem.CityNames;

            return new string[] { "Aurora", "Xetá", "Lurone", "Willran" };
        }

        public static string GetResourceDisplayName(PostoResourceType type)
        {
            switch (type)
            {
                case PostoResourceType.Iron:
                    return "ferro";
                case PostoResourceType.Wood:
                    return "madeira";
                case PostoResourceType.Cotton:
                    return "algodão";
                default:
                    return "recurso";
            }
        }

        public static string GetObjectiveVerb(PostoObjectiveType type)
        {
            switch (type)
            {
                case PostoObjectiveType.KillMob:
                    return "Matar";
                case PostoObjectiveType.DestroyItem:
                    return "Destruir";
                default:
                    return "Concluir";
            }
        }

        public static string NormalizeCityId(string cityId)
        {
            return PlayerMobile.NormalizeOSUCityId(cityId);
        }

        public static bool SameCity(string a, string b)
        {
            return String.Equals(NormalizeCityId(a), NormalizeCityId(b), StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCitizenCity(PlayerMobile pm)
        {
            if (pm == null)
                return String.Empty;

            return NormalizeCityId(pm.OSUCitizenCityId);
        }

        public static string GetAmbassadorCity(PlayerMobile pm)
        {
            if (pm == null)
                return String.Empty;

            string citizenCity = GetCitizenCity(pm);

            if (!String.IsNullOrWhiteSpace(citizenCity) && pm.IsOSUAmbassadorFor(citizenCity))
                return citizenCity;

            return String.Empty;
        }

        public static string GetDispatcherCity(PlayerMobile pm)
        {
            if (pm == null)
                return String.Empty;

            string citizenCity = GetCitizenCity(pm);

            if (!String.IsNullOrWhiteSpace(citizenCity) && pm.IsOSUDispatcherFor(citizenCity))
                return citizenCity;

            return String.Empty;
        }

        public static string GetObjectiveProgressText(PostoDefinition def, PostoState state)
        {
            if (def == null || state == null)
                return "0/0";

            int current = state.ProgressValue;
            if (current < 0)
                current = 0;

            if (current > def.ObjectiveAmount)
                current = def.ObjectiveAmount;

            return String.Format("{0}/{1}", current, def.ObjectiveAmount);
        }

        public static string GetOwnerLabel(PostoState state)
        {
            if (state == null || String.IsNullOrWhiteSpace(state.OwnerCityId))
                return "Nenhum";

            return NormalizeCityId(state.OwnerCityId);
        }

        public static string GetProgressCityLabel(PostoState state)
        {
            if (state == null || String.IsNullOrWhiteSpace(state.ProgressCityId))
                return "Nenhum";

            return NormalizeCityId(state.ProgressCityId);
        }

        public static int GetStoredAmount(string postoId)
        {
            PostoState state = GetState(postoId);

            if (state == null)
                return 0;

            PostoDefinition def = GetDefinition(postoId);
            ResolveContestIfNeeded(def, state);
            TouchProduction(postoId);
            return state.StoredAmount;
        }

        public static void TouchProduction(string postoId)
        {
            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
                return;

            ResolveContestIfNeeded(def, state);

            if (String.IsNullOrWhiteSpace(state.OwnerCityId))
                return;

            if (state.StoredAmount >= 800)
                return;

            DateTime now = DateTime.UtcNow;

            if (state.LastProductionUtc == DateTime.MinValue)
            {
                state.LastProductionUtc = now;
                return;
            }

            TimeSpan elapsed = now - state.LastProductionUtc;
            TimeSpan productionInterval = TimeSpan.FromDays(1.0);

            if (elapsed < productionInterval)
                return;

            int cycles = (int)(elapsed.Ticks / productionInterval.Ticks);

            if (cycles <= 0)
                return;

            int add = cycles * def.DailyYield;
            int oldAmount = state.StoredAmount;

            state.StoredAmount += add;
            if (state.StoredAmount > 800)
                state.StoredAmount = 800;

            state.LastProductionUtc = state.LastProductionUtc.AddTicks(productionInterval.Ticks * cycles);

            if (state.StoredAmount != oldAmount)
                RefreshPostoChests(postoId);
        }

        public static bool IsContestActive(PostoState state)
        {
            return state != null
                && !String.IsNullOrWhiteSpace(state.OwnerCityId)
                && state.ContestScores != null
                && state.ContestScores.Count > 0
                && state.ContestEndsUtc > DateTime.UtcNow;
        }

        private static bool HasContestParticipant(PostoState state, string cityId)
        {
            if (state == null || state.ContestScores == null || String.IsNullOrWhiteSpace(cityId))
                return false;

            string normalized = NormalizeCityId(cityId);

            for (int i = 0; i < state.ContestScores.Count; i++)
            {
                if (SameCity(state.ContestScores[i].CityId, normalized))
                    return true;
            }

            return false;
        }

        private static int GetContestScore(PostoState state, string cityId)
        {
            if (state == null || state.ContestScores == null || String.IsNullOrWhiteSpace(cityId))
                return 0;

            for (int i = 0; i < state.ContestScores.Count; i++)
            {
                if (SameCity(state.ContestScores[i].CityId, cityId))
                    return state.ContestScores[i].Score;
            }

            return 0;
        }

        private static void AddContestParticipant(PostoState state, string cityId)
        {
            if (state == null || String.IsNullOrWhiteSpace(cityId))
                return;

            if (state.ContestScores == null)
                state.ContestScores = new List<PostoContestScore>();

            if (HasContestParticipant(state, cityId))
                return;

            state.ContestScores.Add(new PostoContestScore(NormalizeCityId(cityId)));
        }

        private static void ClearContest(PostoState state)
        {
            if (state == null)
                return;

            state.ContestEndsUtc = DateTime.MinValue;
            if (state.ContestScores == null)
                state.ContestScores = new List<PostoContestScore>();
            else
                state.ContestScores.Clear();
        }

        private static string GetContestRemainingLabel(PostoState state)
        {
            if (!IsContestActive(state))
                return String.Empty;

            TimeSpan remaining = state.ContestEndsUtc - DateTime.UtcNow;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            if (remaining.TotalDays >= 1.0)
                return String.Format("{0}d {1}h", Math.Max(0, remaining.Days), Math.Max(0, remaining.Hours));

            if (remaining.TotalHours >= 1.0)
                return String.Format("{0}h {1}m", Math.Max(0, (int)remaining.TotalHours), Math.Max(0, remaining.Minutes));

            return String.Format("{0}m", Math.Max(0, (int)Math.Ceiling(remaining.TotalMinutes)));
        }

        private static void ResolveContestIfNeeded(PostoDefinition def, PostoState state)
        {
            if (def == null || state == null)
                return;

            if (String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                ClearContest(state);
                return;
            }

            if (state.ContestScores == null)
                state.ContestScores = new List<PostoContestScore>();

            if (state.ContestScores.Count == 0 || state.ContestEndsUtc == DateTime.MinValue)
                return;

            if (state.ContestEndsUtc > DateTime.UtcNow)
                return;

            string winnerCity = NormalizeCityId(state.OwnerCityId);
            int bestScore = Int32.MinValue;
            bool tie = false;

            for (int i = 0; i < state.ContestScores.Count; i++)
            {
                PostoContestScore score = state.ContestScores[i];
                if (score == null || String.IsNullOrWhiteSpace(score.CityId))
                    continue;

                if (score.Score > bestScore)
                {
                    bestScore = score.Score;
                    winnerCity = NormalizeCityId(score.CityId);
                    tie = false;
                }
                else if (score.Score == bestScore)
                {
                    tie = true;
                }
            }

            if (tie)
                winnerCity = NormalizeCityId(state.OwnerCityId);

            state.OwnerCityId = winnerCity;
            state.ProgressCityId = String.Empty;
            state.ProgressValue = 0;
            state.ProtectedUntilUtc = DateTime.UtcNow + def.ProtectionDelay;
            state.LastProductionUtc = DateTime.UtcNow;
            ClearContest(state);
            RefreshPostoChests(def.Id);
        }

        public static string GetLockedHtmlForViewer(PlayerMobile viewer, PostoDefinition def, PostoState state)
        {
            if (def == null || state == null)
                return String.Empty;

            if (IsContestActive(state))
                return String.Empty;

            string viewerCity = GetCitizenCity(viewer);

            if (String.IsNullOrWhiteSpace(viewerCity) && viewer != null)
                viewerCity = GetAmbassadorCity(viewer);

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId)
                && !SameCity(viewerCity, state.OwnerCityId)
                && DateTime.UtcNow < state.ProtectedUntilUtc)
            {
                return String.Format(
                    "Nós acabamos de fazer um acordo com o reino {0} para nos ajudar. Vamos ver se eles conseguem cumprir suas promessas de nos proteger primeiro.",
                    NormalizeCityId(state.OwnerCityId));
            }

            return String.Empty;
        }

        public static string BuildMainHtml(PlayerMobile viewer, PostoDefinition def, PostoState state)
        {
            if (def == null || state == null)
                return "<BASEFONT COLOR=#000000>Posto inválido.";

            ResolveContestIfNeeded(def, state);
            TouchProduction(def.Id);

            if (IsContestActive(state))
                return BuildContestHtml(def, state);

            string lockedHtml = GetLockedHtmlForViewer(viewer, def, state);
            if (!String.IsNullOrWhiteSpace(lockedHtml))
                return lockedHtml;

            StringBuilder sb = new StringBuilder();
            sb.Append(def.StoryHtml);
            sb.Append("<br><br>");
            sb.Append("<BASEFONT COLOR=#000000><b>Objetivo:</b></BASEFONT> ");
            sb.Append(GetObjectiveVerb(def.ObjectiveType));
            sb.Append(" ");
            sb.Append(def.ObjectiveAmount);
            sb.Append(" ");
            sb.Append(def.ObjectiveDisplayName);
            sb.Append("<br>");
            sb.Append("<BASEFONT COLOR=#000000><b>Recompensa:</b></BASEFONT> ");
            sb.Append(def.DailyYield);
            sb.Append(" ");
            sb.Append(GetResourceDisplayName(def.ResourceType));
            sb.Append(" por dia<br>");

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                sb.Append("<BASEFONT COLOR=#000000><b>Reino atual:</b></BASEFONT> ");
                sb.Append(NormalizeCityId(state.OwnerCityId));
                sb.Append("<br>");
            }

            if (!String.IsNullOrWhiteSpace(state.ProgressCityId))
            {
                sb.Append("<BASEFONT COLOR=#000000><b>Acordo atual:</b></BASEFONT> ");
                sb.Append(NormalizeCityId(state.ProgressCityId));
                sb.Append("<br>");
            }

            if (state.StoredAmount > 0)
            {
                sb.Append("<BASEFONT COLOR=#000000><b>Baú:</b></BASEFONT> ");
                sb.Append(state.StoredAmount);
                sb.Append(" ");
                sb.Append(GetResourceDisplayName(def.ResourceType));
                sb.Append(" armazenado(s)");
            }

            return sb.ToString();
        }

        private static string BuildContestHtml(PostoDefinition def, PostoState state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetContestStoryHtml(def));
            sb.Append("<br><br>");
            sb.Append("<BASEFONT COLOR=#000000><b>Prazo da disputa:</b></BASEFONT> ");
            sb.Append(GetContestRemainingLabel(state));
            sb.Append(" restantes.<br>");
            sb.Append("<BASEFONT COLOR=#000000><b>Alvo:</b></BASEFONT> ");
            sb.Append(GetObjectiveVerb(def.ObjectiveType));
            sb.Append(" ");
            sb.Append(def.ObjectiveDisplayName);
            sb.Append("<br>");
            sb.Append("<BASEFONT COLOR=#000000><b>Placar atual:</b></BASEFONT><br>");

            List<PostoContestScore> ordered = new List<PostoContestScore>(state.ContestScores ?? new List<PostoContestScore>());
            ordered.Sort(delegate (PostoContestScore a, PostoContestScore b)
            {
                int scoreCompare = b.Score.CompareTo(a.Score);
                if (scoreCompare != 0)
                    return scoreCompare;

                if (SameCity(a.CityId, state.OwnerCityId))
                    return -1;

                if (SameCity(b.CityId, state.OwnerCityId))
                    return 1;

                return String.Compare(NormalizeCityId(a.CityId), NormalizeCityId(b.CityId), StringComparison.OrdinalIgnoreCase);
            });

            for (int i = 0; i < ordered.Count; i++)
            {
                PostoContestScore score = ordered[i];
                if (score == null)
                    continue;

                sb.Append("- ");
                sb.Append(NormalizeCityId(score.CityId));
                if (SameCity(score.CityId, state.OwnerCityId))
                    sb.Append(" (defensor)");
                sb.Append(": ");
                sb.Append(score.Score);
                sb.Append(" abatido(s)<br>");
            }

            sb.Append("<BASEFONT COLOR=#000000><b>Produção atual:</b></BASEFONT> ");
            sb.Append(NormalizeCityId(state.OwnerCityId));
            sb.Append(" continua recebendo os recursos até o fim da disputa.");
            return sb.ToString();
        }

        public static PostoActionType GetAvailableAction(PlayerMobile viewer, PostoDefinition def, PostoState state, out string buttonLabel, out string reason)
        {
            buttonLabel = String.Empty;
            reason = String.Empty;

            if (viewer == null || def == null || state == null)
            {
                reason = "Interação inválida.";
                return PostoActionType.None;
            }

            ResolveContestIfNeeded(def, state);
            string ambassadorCity = GetAmbassadorCity(viewer);
            string citizenCity = GetCitizenCity(viewer);

            if (IsContestActive(state))
            {
                if (String.IsNullOrWhiteSpace(ambassadorCity))
                {
                    if (HasContestParticipant(state, citizenCity))
                        reason = "Seu reino já está na disputa. Agora os cidadãos devem garantir a melhor proteção.";
                    else
                        reason = "Somente um embaixador pode colocar um novo reino nesta disputa.";

                    return PostoActionType.None;
                }

                if (HasContestParticipant(state, ambassadorCity))
                {
                    reason = SameCity(ambassadorCity, state.OwnerCityId)
                        ? "Seu reino já defende este posto na disputa atual."
                        : "Seu reino já entrou na disputa deste posto.";
                    return PostoActionType.None;
                }

                buttonLabel = "Entrar na disputa";
                return PostoActionType.AcceptAgreement;
            }

            if (String.IsNullOrWhiteSpace(ambassadorCity))
            {
                if (!String.IsNullOrWhiteSpace(state.ProgressCityId) && SameCity(citizenCity, state.ProgressCityId))
                    reason = "Seu reino já aceitou o acordo. Agora os cidadãos precisam cumprir o objetivo.";
                else
                    reason = "Somente um embaixador pode firmar ou concluir acordos de posto.";

                return PostoActionType.None;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId)
                && SameCity(ambassadorCity, state.OwnerCityId)
                && String.IsNullOrWhiteSpace(state.ProgressCityId))
            {
                reason = "Este posto já pertence ao seu reino.";
                return PostoActionType.None;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId)
                && !SameCity(ambassadorCity, state.OwnerCityId)
                && DateTime.UtcNow < state.ProtectedUntilUtc)
            {
                reason = "Este posto ainda está protegido pelo acordo recém-firmado.";
                return PostoActionType.None;
            }

            if (String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                if (String.IsNullOrWhiteSpace(state.ProgressCityId))
                {
                    buttonLabel = "Aceitar acordo";
                    return PostoActionType.AcceptAgreement;
                }

                if (!SameCity(state.ProgressCityId, ambassadorCity))
                {
                    reason = "Outro reino já está negociando este posto.";
                    return PostoActionType.None;
                }

                if (state.ProgressValue >= def.ObjectiveAmount)
                {
                    buttonLabel = "Conquistar Posto";
                    return PostoActionType.Conquer;
                }

                reason = "O acordo já foi aceito pelo seu reino. Falta cumprir o objetivo.";
                return PostoActionType.None;
            }

            buttonLabel = "Iniciar disputa";
            return PostoActionType.AcceptAgreement;
        }

        public static bool TryAcceptAgreement(PlayerMobile from, string postoId, out string message)
        {
            message = String.Empty;

            if (from == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
            {
                message = "Posto inválido.";
                return false;
            }

            ResolveContestIfNeeded(def, state);
            string ambassadorCity = GetAmbassadorCity(from);

            if (String.IsNullOrWhiteSpace(ambassadorCity))
            {
                message = "Somente um embaixador pode firmar acordos de posto.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                if (!String.IsNullOrWhiteSpace(state.ProgressCityId))
                {
                    if (SameCity(state.ProgressCityId, ambassadorCity))
                    {
                        message = "Seu reino já aceitou o acordo deste posto.";
                        return false;
                    }

                    message = "Outro reino já está tentando conquistar este posto.";
                    return false;
                }

                state.ProgressCityId = ambassadorCity;
                state.ProgressValue = 0;
                message = "Acordo aceito. Agora os cidadãos de " + ambassadorCity + " já podem cumprir a missão do posto.";
                RefreshPostoChests(postoId);
                return true;
            }

            if (SameCity(ambassadorCity, state.OwnerCityId))
            {
                message = "O reino dono já participa automaticamente de qualquer disputa deste posto.";
                return false;
            }

            if (DateTime.UtcNow < state.ProtectedUntilUtc && !IsContestActive(state))
            {
                message = "Este posto ainda está sob o prazo mínimo de proteção.";
                return false;
            }

            if (!IsContestActive(state))
            {
                state.ProgressCityId = String.Empty;
                state.ProgressValue = 0;
                state.ContestEndsUtc = DateTime.UtcNow + ContestDuration;
                ClearContest(state);
                state.ContestEndsUtc = DateTime.UtcNow + ContestDuration;
                AddContestParticipant(state, state.OwnerCityId);
                AddContestParticipant(state, ambassadorCity);
                QueueLeaderAlert(state.OwnerCityId, postoId, ambassadorCity);
                message = "A disputa pelo posto " + def.Name + " começou. Pelos próximos 3 dias, vence o reino que mais contiver a ameaça local.";
                RefreshPostoChests(postoId);
                return true;
            }

            if (HasContestParticipant(state, ambassadorCity))
            {
                message = "Seu reino já está participando da disputa deste posto.";
                return false;
            }

            AddContestParticipant(state, ambassadorCity);
            QueueLeaderAlert(state.OwnerCityId, postoId, ambassadorCity);
            message = "O reino de " + ambassadorCity + " entrou na disputa pelo posto " + def.Name + ".";
            return true;
        }

        public static bool TryConquer(PlayerMobile from, string postoId, out string message)
        {
            message = String.Empty;

            if (from == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
            {
                message = "Posto inválido.";
                return false;
            }

            ResolveContestIfNeeded(def, state);
            string ambassadorCity = GetAmbassadorCity(from);

            if (String.IsNullOrWhiteSpace(ambassadorCity))
            {
                message = "Somente um embaixador pode concluir a conquista.";
                return false;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                message = IsContestActive(state)
                    ? "Este posto está em disputa aberta. O vencedor será definido ao final do prazo."
                    : "Postos já conquistados só podem mudar de mãos através de disputa.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(state.ProgressCityId) || !SameCity(state.ProgressCityId, ambassadorCity))
            {
                message = "Seu reino não tem um acordo ativo neste posto.";
                return false;
            }

            if (state.ProgressValue < def.ObjectiveAmount)
            {
                message = "O objetivo da missão ainda não foi concluído.";
                return false;
            }

            state.OwnerCityId = ambassadorCity;
            state.ProgressCityId = String.Empty;
            state.ProgressValue = 0;
            state.ProtectedUntilUtc = DateTime.UtcNow + def.ProtectionDelay;
            state.LastProductionUtc = DateTime.UtcNow;
            ClearContest(state);

            message = "O posto " + def.Name + " agora pertence a " + ambassadorCity + ".";
            RefreshPostoChests(postoId);
            return true;
        }

        public static bool TryDispatch(PlayerMobile from, string postoId, out string message, out int amountDispatched)
        {
            message = String.Empty;
            amountDispatched = 0;

            if (from == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
            {
                message = "Posto inválido.";
                return false;
            }

            ResolveContestIfNeeded(def, state);

            if (String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                message = "Este posto ainda não pertence a nenhum reino.";
                return false;
            }

            string dispatcherCity = GetDispatcherCity(from);

            if (String.IsNullOrWhiteSpace(dispatcherCity))
            {
                message = "Somente um despachante do reino dono pode recolher os recursos.";
                return false;
            }

            if (!SameCity(dispatcherCity, state.OwnerCityId))
            {
                message = "Você não é despachante do reino que controla este posto.";
                return false;
            }

            TouchProduction(postoId);

            if (state.StoredAmount <= 0)
            {
                message = "O baú ainda está vazio.";
                return false;
            }

            int amount = state.StoredAmount;
            state.StoredAmount = 0;

            PostoKingdomResourceLedger ledger = EnsureLedger(state.OwnerCityId);
            ledger.Add(def.ResourceType, amount);

            amountDispatched = amount;
            message = String.Format(
                "Você despachou {0} {1} para o reino de {2}.",
                amount,
                GetResourceDisplayName(def.ResourceType),
                NormalizeCityId(state.OwnerCityId));

            RefreshPostoChests(postoId);
            return true;
        }

        private static void OnCreatureDeath(CreatureDeathEventArgs e)
        {
            try
            {
                if (e == null || e.Creature == null || e.Creature.Deleted)
                    return;

                PlayerMobile killer = ResolvePlayer(e.Killer);
                if (killer == null || killer.Deleted)
                    return;

                string citizenCity = GetCitizenCity(killer);
                if (String.IsNullOrWhiteSpace(citizenCity))
                    return;

                string killedTypeName = e.Creature.GetType().Name;

                foreach (PostoDefinition def in PostoRegistry.All)
                {
                    PostoState state = GetState(def.Id);
                    if (state == null)
                        continue;

                    ResolveContestIfNeeded(def, state);

                    if (def.ObjectiveType != PostoObjectiveType.KillMob)
                        continue;

                    if (!MatchesAnyTypeName(killedTypeName, def.ObjectiveTypeNames))
                        continue;

                    if (IsContestActive(state))
                    {
                        if (!HasContestParticipant(state, citizenCity))
                            continue;

                        for (int i = 0; i < state.ContestScores.Count; i++)
                        {
                            PostoContestScore score = state.ContestScores[i];
                            if (score == null || !SameCity(score.CityId, citizenCity))
                                continue;

                            score.Score++;
                            break;
                        }

                        break;
                    }

                    if (String.IsNullOrWhiteSpace(state.ProgressCityId))
                        continue;

                    if (!SameCity(state.ProgressCityId, citizenCity))
                        continue;

                    if (state.ProgressValue >= def.ObjectiveAmount)
                        continue;

                    state.ProgressValue++;
                    if (state.ProgressValue > def.ObjectiveAmount)
                        state.ProgressValue = def.ObjectiveAmount;

                    if (state.ProgressValue == def.ObjectiveAmount)
                        killer.SendMessage("O objetivo do posto {0} foi concluído. Um embaixador deve voltar lá para reivindicá-lo.", def.Name);

                    break;
                }
            }
            catch
            {
            }
        }

        public static void RefreshPostoChests(string postoId)
        {
            if (String.IsNullOrWhiteSpace(postoId))
                return;

            foreach (Item item in World.Items.Values)
            {
                PostoResourceChest chest = item as PostoResourceChest;
                if (chest == null || chest.Deleted)
                    continue;

                if (String.Equals(chest.PostoId, postoId, StringComparison.OrdinalIgnoreCase))
                    chest.RefreshState();
            }
        }

        public static void NotifyItemDestroyed(PlayerMobile destroyer, string postoId, string targetTypeName)
        {
            if (destroyer == null || String.IsNullOrWhiteSpace(postoId) || String.IsNullOrWhiteSpace(targetTypeName))
                return;

            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null || def.ObjectiveType != PostoObjectiveType.DestroyItem)
                return;

            ResolveContestIfNeeded(def, state);
            string citizenCity = GetCitizenCity(destroyer);
            if (String.IsNullOrWhiteSpace(citizenCity))
                return;

            if (IsContestActive(state))
            {
                if (!HasContestParticipant(state, citizenCity))
                    return;

                for (int i = 0; i < state.ContestScores.Count; i++)
                {
                    PostoContestScore score = state.ContestScores[i];
                    if (score == null || !SameCity(score.CityId, citizenCity))
                        continue;

                    score.Score++;
                    break;
                }

                return;
            }

            if (!SameCity(state.ProgressCityId, citizenCity))
                return;

            if (!MatchesAnyTypeName(targetTypeName, def.ObjectiveTypeNames))
                return;

            if (state.ProgressValue >= def.ObjectiveAmount)
                return;

            state.ProgressValue++;
            if (state.ProgressValue > def.ObjectiveAmount)
                state.ProgressValue = def.ObjectiveAmount;
        }

        private static bool MatchesAnyTypeName(string currentTypeName, string[] allowedNames)
        {
            if (String.IsNullOrWhiteSpace(currentTypeName) || allowedNames == null || allowedNames.Length == 0)
                return false;

            for (int i = 0; i < allowedNames.Length; i++)
            {
                string allowed = allowedNames[i];
                if (String.IsNullOrWhiteSpace(allowed))
                    continue;

                if (String.Equals(currentTypeName, allowed.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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

        public static bool ResetPosto(string postoId, out string message)
        {
            message = String.Empty;

            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
            {
                message = "Posto inválido.";
                return false;
            }

            state.OwnerCityId = String.Empty;
            state.ProgressCityId = String.Empty;
            state.ProgressValue = 0;
            state.StoredAmount = 0;
            state.LastProductionUtc = DateTime.MinValue;
            state.ProtectedUntilUtc = DateTime.MinValue;
            ClearContest(state);

            for (int i = m_PendingLeaderAlerts.Count - 1; i >= 0; i--)
            {
                if (String.Equals(m_PendingLeaderAlerts[i].PostoId, postoId, StringComparison.OrdinalIgnoreCase))
                    m_PendingLeaderAlerts.RemoveAt(i);
            }

            message = "O posto " + def.Name + " foi resetado.";
            RefreshPostoChests(postoId);
            return true;
        }

        public static bool SetProgress(string postoId, int value, out string message)
        {
            message = String.Empty;

            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
            {
                message = "Posto inválido.";
                return false;
            }

            if (value < 0)
                value = 0;

            if (value > def.ObjectiveAmount)
                value = def.ObjectiveAmount;

            state.ProgressValue = value;
            message = "Progresso do posto " + def.Name + " ajustado para " + value + ".";
            return true;
        }

        private static void QueueLeaderAlert(string defenderCityId, string postoId, string challengerCityId)
        {
            if (String.IsNullOrWhiteSpace(defenderCityId) || String.IsNullOrWhiteSpace(postoId) || String.IsNullOrWhiteSpace(challengerCityId))
                return;

            PostoLeaderAlert alert = new PostoLeaderAlert();
            alert.PostoId = postoId;
            alert.DefenderCityId = NormalizeCityId(defenderCityId);
            alert.ChallengerCityId = NormalizeCityId(challengerCityId);
            alert.CreatedUtc = DateTime.UtcNow;

            PlayerMobile onlineLeader = FindOnlineLeader(alert.DefenderCityId);
            if (onlineLeader != null)
            {
                SendLeaderAlertGump(onlineLeader, alert);
                return;
            }

            m_PendingLeaderAlerts.Add(alert);
        }

        private static PlayerMobile FindOnlineLeader(string cityId)
        {
            int cityIndex = GetCityIndexByName(cityId);
            if (cityIndex < 0)
                return null;

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                if (pm.OSUReinoLeader && pm.OSUReinoLeaderCityId == cityIndex)
                    return pm;
            }

            return null;
        }

        private static void SendLeaderAlertGump(PlayerMobile pm, PostoLeaderAlert alert)
        {
            if (pm == null || pm.Deleted || alert == null)
                return;

            PostoDefinition def = GetDefinition(alert.PostoId);
            if (def == null)
                return;

            pm.CloseGump(typeof(PostoContestAlertGump));
            pm.SendGump(new PostoContestAlertGump(pm, def.Name, NormalizeCityId(alert.ChallengerCityId), NormalizeCityId(alert.DefenderCityId)));
        }

        private static int GetCityIndexByName(string cityId)
        {
            cityId = NormalizeCityId(cityId);
            string[] cities = GetKnownCities();

            for (int i = 0; i < cities.Length; i++)
            {
                if (SameCity(cities[i], cityId))
                    return i;
            }

            return -1;
        }

        private static string GetCityNameByIndex(int cityId)
        {
            string[] cities = GetKnownCities();
            if (cityId < 0 || cityId >= cities.Length)
                return String.Empty;

            return NormalizeCityId(cities[cityId]);
        }

        private static string GetContestStoryHtml(PostoDefinition def)
        {
            if (def == null)
                return "<BASEFONT COLOR=#000000>Este posto está em disputa.";

            switch ((def.Id ?? String.Empty).ToLowerInvariant())
            {
                case "aramute": return "<BASEFONT COLOR=#000000>Aramute já não pede favores: pede prova. Os mineiros fecharam as entradas menores e decidiram observar qual reino realmente segura os corredores. Durante a disputa, vence quem abater mais ameaças nas galerias e provar que pode manter a mina aberta.";
                case "dorvok": return "<BASEFONT COLOR=#000000>Dorvok cansou de promessas vazias. A pedreira seguirá com o reino que mostrar, em números, que consegue limpar as escoras e manter o trabalho de pé. Três dias. Que a rocha escolha o mais firme.";
                case "selgard": return "<BASEFONT COLOR=#000000>Os trabalhadores de Selgard não querem mais juramentos bonitos. Querem corredor seguro e turnos inteiros sem gritos. A disputa está aberta: o reino que mais contiver a ameaça local ficará com o acordo.";
                case "karstun": return "<BASEFONT COLOR=#000000>Karstun virou palco de competição entre coroas. Os pedreiros juraram manter o trato com quem provar, golpe por golpe, que consegue proteger melhor o corte. O melhor guarda leva a pedra.";
                case "vhalor": return "<BASEFONT COLOR=#000000>Vhalor não vai trocar de bandeira por discurso. A pedreira ficará com o reino que, nestes próximos dias, derrubar mais invasores e mantiver os níveis fundos respirando. Aqui, a vitória se mede em abatidos.";
                case "nargesh": return "<BASEFONT COLOR=#000000>Nargesh abriu uma disputa direta entre reinos. As galerias continuarão produzindo para o atual dono por enquanto, mas o contrato final irá para quem provar mais proteção no subsolo.";
                case "tirak": return "<BASEFONT COLOR=#000000>Em Tirak, os mineiros decidiram assistir em silêncio. Que os reinos disputem, que os imps tombem, e que o posto fique com quem mostrar mais serviço antes do prazo acabar.";
                case "thorma": return "<BASEFONT COLOR=#000000>Thorma colocou a fornalha da decisão sobre as coroas. Os túneis quentes reconhecerão como aliado o reino que suportar melhor a pressão e abater mais ameaças até o fim da disputa.";
                case "cunhau": return "<BASEFONT COLOR=#000000>Cunhau suspendeu promessas e abriu desafio. O bosque ficará com o reino que conseguir manter as trilhas mais limpas e os machados trabalhando pelos próximos dias. Que vença o melhor protetor da mata.";
                case "belorim": return "<BASEFONT COLOR=#000000>Belorim declarou disputa aberta. As hárpias continuam rondando os pinheiros, e o bosque será entregue ao reino que provar, no alto e no chão, que sabe proteger melhor seus lenhadores.";
                case "valesca": return "<BASEFONT COLOR=#000000>Valesca está em disputa. Os lenhadores prometeram seguir o reino que cortar o emaranhado de perigo e apresentar o maior número de abatidos antes do prazo final.";
                case "norvind": return "<BASEFONT COLOR=#000000>Norvind quer resultados, não brasões. Os ettins continuam descendo do morro, e o trato ficará com o reino que mostrar força constante e a melhor defesa das carroças.";
                case "talbrasa": return "<BASEFONT COLOR=#000000>Talbrasa transformou a crise da floresta em competição aberta. O reino que mais derrubar ameaças e segurar o pátio de secagem ficará com a madeira quando a disputa terminar.";
                case "rivenoak": return "<BASEFONT COLOR=#000000>Rivenoak foi posto em prova. As trilhas vivas continuam estrangulando a mata, e agora cada reino terá de demonstrar, em trabalho real, quem merece conduzir o posto.";
                case "galdrin": return "<BASEFONT COLOR=#000000>Galdrin cansou de alianças frágeis. As rotas de corte seguem sob risco, e o bosque será mantido por quem mostrar a melhor proteção armada até o último dia da disputa.";
                case "ulmora": return "<BASEFONT COLOR=#000000>Ulmora abriu sua névoa para uma disputa entre reinos. Quem conseguir limpar mais feras e conservar as saídas da mata terá o direito de manter este acordo.";
                case "saial": return "<BASEFONT COLOR=#000000>Saial decidiu observar os reinos de perto. Os campos continuarão colhendo sob a guarda atual, mas o contrato final ficará com quem expulsar mais ameaças dos celeiros e das cercas.";
                case "iriande": return "<BASEFONT COLOR=#000000>Iriande está em disputa. Os trabalhadores manterão os fardos indo para quem já segura o posto por enquanto, porém o melhor defensor dos campos levará o trato no fim do prazo.";
                case "belsara": return "<BASEFONT COLOR=#000000>Belsara chamou todos os reinos à prova. Não basta oferecer escolta: é preciso mostrar quem realmente limpa os caminhos e mantém as plantações respirando.";
                case "rosamar": return "<BASEFONT COLOR=#000000>Rosamar decidiu que só a prática resolve. O reino que mais contiver a ameaça local durante a janela de disputa provará que merece a confiança destes agricultores.";
                case "lumera": return "<BASEFONT COLOR=#000000>Lumera abriu uma disputa de proteção. As margens e canais serão observados dia e noite, e o posto ficará com o reino que apresentar o maior esforço real contra a ameaça.";
                case "dalvila": return "<BASEFONT COLOR=#000000>Dalvila quer paz para a colheita, e paz agora vale placar. O reino que abater mais inimigos até o fim da disputa será reconhecido como o mais capaz de guardar estes campos.";
                case "ventalva": return "<BASEFONT COLOR=#000000>Ventalva transformou o desassossego da região em prova formal. Durante três dias, os reinos disputarão este posto abatendo as ameaças locais. O melhor desempenho fala mais alto.";
                case "orquessa": return "<BASEFONT COLOR=#000000>Orquessa abriu disputa sem rodeios. Os agricultores querem ver qual reino realmente protege as passagens, os poços e os fardos. O melhor vença — e fique com o posto.";
                default: return "<BASEFONT COLOR=#000000>Este posto está em disputa. Os trabalhadores manterão o trato com quem mostrar a melhor proteção local. O melhor vença.";
            }
        }

        public static void Save()
        {
            try
            {
                EnsureDefaults();

                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(3); // version

                    bw.Write(m_States.Count);
                    foreach (KeyValuePair<string, PostoState> kv in m_States)
                    {
                        PostoState st = kv.Value;

                        bw.Write(kv.Key ?? String.Empty);
                        bw.Write(st.OwnerCityId ?? String.Empty);
                        bw.Write(st.ProgressCityId ?? String.Empty);
                        bw.Write(st.ProgressValue);
                        bw.Write(st.StoredAmount);
                        bw.Write(st.LastProductionUtc.ToBinary());
                        bw.Write(st.ProtectedUntilUtc.ToBinary());
                        bw.Write(st.ContestEndsUtc.ToBinary());

                        int contestCount = st.ContestScores != null ? st.ContestScores.Count : 0;
                        bw.Write(contestCount);
                        for (int i = 0; i < contestCount; i++)
                        {
                            PostoContestScore score = st.ContestScores[i];
                            bw.Write(score != null ? score.CityId ?? String.Empty : String.Empty);
                            bw.Write(score != null ? score.Score : 0);
                        }
                    }

                    bw.Write(m_Ledgers.Count);
                    foreach (KeyValuePair<string, PostoKingdomResourceLedger> kv in m_Ledgers)
                    {
                        PostoKingdomResourceLedger ledger = kv.Value;
                        bw.Write(kv.Key ?? String.Empty);
                        bw.Write(ledger.Iron);
                        bw.Write(ledger.Wood);
                        bw.Write(ledger.Cotton);
                    }

                    bw.Write(m_PendingLeaderAlerts.Count);
                    for (int i = 0; i < m_PendingLeaderAlerts.Count; i++)
                    {
                        PostoLeaderAlert alert = m_PendingLeaderAlerts[i] ?? new PostoLeaderAlert();
                        bw.Write(alert.PostoId ?? String.Empty);
                        bw.Write(alert.DefenderCityId ?? String.Empty);
                        bw.Write(alert.ChallengerCityId ?? String.Empty);
                        bw.Write(alert.CreatedUtc.ToBinary());
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
                EnsureDefaults();

                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();

                    if (version >= 1)
                    {
                        m_States.Clear();
                        int stateCount = br.ReadInt32();

                        for (int i = 0; i < stateCount; i++)
                        {
                            string postoId = br.ReadString();
                            PostoState st = new PostoState(postoId);

                            st.OwnerCityId = br.ReadString();
                            st.ProgressCityId = br.ReadString();
                            st.ProgressValue = br.ReadInt32();
                            st.StoredAmount = br.ReadInt32();
                            st.LastProductionUtc = DateTime.FromBinary(br.ReadInt64());
                            st.ProtectedUntilUtc = DateTime.FromBinary(br.ReadInt64());

                            if (version >= 3)
                            {
                                st.ContestEndsUtc = DateTime.FromBinary(br.ReadInt64());
                                int contestCount = br.ReadInt32();
                                st.ContestScores = new List<PostoContestScore>();
                                for (int c = 0; c < contestCount; c++)
                                {
                                    PostoContestScore score = new PostoContestScore();
                                    score.CityId = br.ReadString();
                                    score.Score = br.ReadInt32();
                                    st.ContestScores.Add(score);
                                }
                            }

                            m_States[postoId] = st;
                        }

                        m_Ledgers.Clear();
                        int ledgerCount = br.ReadInt32();

                        for (int i = 0; i < ledgerCount; i++)
                        {
                            string cityId = br.ReadString();
                            PostoKingdomResourceLedger ledger = new PostoKingdomResourceLedger(cityId);
                            ledger.Iron = br.ReadInt32();
                            ledger.Wood = br.ReadInt32();
                            ledger.Cotton = br.ReadInt32();
                            m_Ledgers[cityId] = ledger;
                        }

                        m_PendingLeaderAlerts.Clear();
                        if (version >= 3)
                        {
                            int alertCount = br.ReadInt32();
                            for (int i = 0; i < alertCount; i++)
                            {
                                PostoLeaderAlert alert = new PostoLeaderAlert();
                                alert.PostoId = br.ReadString();
                                alert.DefenderCityId = br.ReadString();
                                alert.ChallengerCityId = br.ReadString();
                                alert.CreatedUtc = DateTime.FromBinary(br.ReadInt64());
                                m_PendingLeaderAlerts.Add(alert);
                            }
                        }

                        if (version == 1)
                        {
                            int oldRoleCount = br.ReadInt32();
                            for (int i = 0; i < oldRoleCount; i++)
                            {
                                br.ReadInt32();
                                br.ReadBoolean();
                                br.ReadBoolean();
                                br.ReadBoolean();
                                br.ReadBoolean();
                                br.ReadBoolean();
                                br.ReadBoolean();
                                br.ReadBoolean();
                                br.ReadBoolean();
                            }
                        }
                    }
                }

                EnsureDefaults();
            }
            catch
            {
            }
        }
    }
}
