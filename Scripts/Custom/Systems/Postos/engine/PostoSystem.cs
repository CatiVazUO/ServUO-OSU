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

        private static readonly Dictionary<string, PostoState> m_States = new Dictionary<string, PostoState>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, PostoKingdomResourceLedger> m_Ledgers = new Dictionary<string, PostoKingdomResourceLedger>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            EnsureDefaults();
            Load();
            EventSink.WorldSave += delegate { Save(); };
            EventSink.CreatureDeath += OnCreatureDeath;

            Timer.DelayCall(
                TimeSpan.FromHours(1.0),
                TimeSpan.FromHours(1.0),
                ProductionPulse);
        }

        private static void ProductionPulse()
        {
            foreach (PostoDefinition def in PostoRegistry.All)
            {
                if (def == null)
                    continue;

                TouchProduction(def.Id);
            }
        }

        private static void EnsureDefaults()
        {
            foreach (PostoDefinition def in PostoRegistry.All)
            {
                EnsureState(def.Id);
            }

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

            TouchProduction(postoId);
            return state.StoredAmount;
        }

        public static void TouchProduction(string postoId)
        {
            PostoDefinition def = GetDefinition(postoId);
            PostoState state = GetState(postoId);

            if (def == null || state == null)
                return;

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


            // TEMPO CERTO POR DIAS 
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

        public static string GetLockedHtmlForViewer(PlayerMobile viewer, PostoDefinition def, PostoState state)
        {
            string viewerCity = GetCitizenCity(viewer);

            if (String.IsNullOrWhiteSpace(viewerCity) && viewer != null)
                viewerCity = GetAmbassadorCity(viewer);

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId) &&
                !SameCity(viewerCity, state.OwnerCityId) &&
                DateTime.UtcNow < state.ProtectedUntilUtc)
            {
                return String.Format(
                    "Nós acabamos de fazer um acordo com o reino {0} para nos ajudar. Vamos ver se eles conseguem cumprir suas promessas de nos proteger primeiro.",
                    NormalizeCityId(state.OwnerCityId));
            }

            if (!String.IsNullOrWhiteSpace(state.ProgressCityId) &&
                !SameCity(viewerCity, state.ProgressCityId))
            {
                return String.Format(
                    "Já firmamos um acordo provisório com o reino {0}. Só vamos ouvir outra proposta depois que esse trato terminar ou falhar.",
                    NormalizeCityId(state.ProgressCityId));
            }

            return String.Empty;
        }

        public static string BuildMainHtml(PlayerMobile viewer, PostoDefinition def, PostoState state)
        {
            if (def == null || state == null)
                return "<BASEFONT COLOR=#000000>Posto inválido.";

            TouchProduction(def.Id);

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
            sb.Append(" por dia");
            sb.Append("<br>");

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId))
            {
                sb.Append("<BASEFONT COLOR=#000000><b>Reino atual:</b></BASEFONT> ");
                sb.Append(NormalizeCityId(state.OwnerCityId));
                sb.Append("<br>");
            }

            if (!String.IsNullOrWhiteSpace(state.ProgressCityId))
            {
                sb.Append("<BASEFONT COLOR=#000000><b>Posto contestado por:</b></BASEFONT> ");
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

        public static PostoActionType GetAvailableAction(PlayerMobile viewer, PostoDefinition def, PostoState state, out string buttonLabel, out string reason)
        {
            buttonLabel = String.Empty;
            reason = String.Empty;

            if (viewer == null || def == null || state == null)
            {
                reason = "Interação inválida.";
                return PostoActionType.None;
            }

            string ambassadorCity = GetAmbassadorCity(viewer);

            if (String.IsNullOrWhiteSpace(ambassadorCity))
            {
                string citizenCity = GetCitizenCity(viewer);

                if (!String.IsNullOrWhiteSpace(state.ProgressCityId) && SameCity(citizenCity, state.ProgressCityId))
                    reason = "Seu reino já aceitou o acordo. Agora os cidadãos precisam cumprir o objetivo.";

                else
                    reason = "Somente um embaixador pode firmar ou concluir acordos de posto.";

                return PostoActionType.None;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId) &&
                SameCity(ambassadorCity, state.OwnerCityId) &&
                String.IsNullOrWhiteSpace(state.ProgressCityId))
            {
                reason = "Este posto já pertence ao seu reino.";
                return PostoActionType.None;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId) &&
                !SameCity(ambassadorCity, state.OwnerCityId) &&
                DateTime.UtcNow < state.ProtectedUntilUtc)
            {
                reason = "Este posto ainda está protegido pelo acordo recém-firmado.";
                return PostoActionType.None;
            }

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

            string ambassadorCity = GetAmbassadorCity(from);

            if (String.IsNullOrWhiteSpace(ambassadorCity))
            {
                message = "Somente um embaixador pode firmar acordos de posto.";
                return false;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId) &&
                SameCity(ambassadorCity, state.OwnerCityId) &&
                String.IsNullOrWhiteSpace(state.ProgressCityId))
            {
                message = "Este posto já pertence ao seu reino.";
                return false;
            }

            if (!String.IsNullOrWhiteSpace(state.OwnerCityId) &&
                !SameCity(ambassadorCity, state.OwnerCityId) &&
                DateTime.UtcNow < state.ProtectedUntilUtc)
            {
                message = "Este posto ainda está sob o prazo mínimo de proteção.";
                return false;
            }

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

            if (String.IsNullOrWhiteSpace(state.OwnerCityId))
                message = "Acordo aceito. Agora os cidadãos de " + ambassadorCity + " já podem cumprir a missão do posto.";
            else
                message = "Posto em disputa. Enquanto isso, ele continua produzindo para " + NormalizeCityId(state.OwnerCityId) + ".";

            RefreshPostoChests(postoId);
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

            string ambassadorCity = GetAmbassadorCity(from);

            if (String.IsNullOrWhiteSpace(ambassadorCity))
            {
                message = "Somente um embaixador pode concluir a conquista.";
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

            if (state.LastProductionUtc == DateTime.MinValue)
                state.LastProductionUtc = DateTime.UtcNow;
            else
                state.LastProductionUtc = DateTime.UtcNow;

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

                    if (String.IsNullOrWhiteSpace(state.ProgressCityId))
                        continue;

                    if (!SameCity(state.ProgressCityId, citizenCity))
                        continue;

                    if (state.ProgressValue >= def.ObjectiveAmount)
                        continue;

                    if (def.ObjectiveType != PostoObjectiveType.KillMob)
                        continue;

                    if (!MatchesAnyTypeName(killedTypeName, def.ObjectiveTypeNames))
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

            string citizenCity = GetCitizenCity(destroyer);

            if (String.IsNullOrWhiteSpace(citizenCity))
                return;

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
                    bw.Write(2); // version

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
