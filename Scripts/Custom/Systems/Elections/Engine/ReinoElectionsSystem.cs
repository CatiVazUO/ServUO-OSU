using Server;
using Server.Commands;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Server.Custom.Systems.Reinos
{
    public enum ReinoFase
    {
        Fechado = 0,
        Candidatura = 1,
        Votacao = 2,
        Resultado = 3,
        Governo = 4
    }

    public class ReinoCandidateEntry
    {
        public int Serial;
        public string Nome;
        public int Votes;

        public ReinoCandidateEntry()
        {
        }

        public ReinoCandidateEntry(PlayerMobile pm)
        {
            Serial = pm.Serial.Value;
            Nome = pm.Name;
            Votes = 0;
        }
    }

    public class ReinoVoteRecord
    {
        public int VoterSerial;
        public int CityId;
        public int CandidateSerial;
    }

    public class ReinoCityData
    {
        public int Index;
        public string CityName;
        public int GovernorSerial;
        public string GovernorName;
        public DateTime GovernorSinceUtc;

        public int PendingGovernorSerial;
        public string PendingGovernorName;
        public int PendingCycleId;
        public DateTime ResultStartDateUtc;
        public DateTime ResultEndDateUtc;
        public DateTime ResultVisibleUntilUtc;

        public List<ReinoCandidateEntry> Candidates;

        public ReinoCityData(int index, string cityName)
        {
            Index = index;
            CityName = cityName;
            GovernorSerial = 0;
            GovernorName = String.Empty;
            GovernorSinceUtc = DateTime.MinValue;

            PendingGovernorSerial = 0;
            PendingGovernorName = String.Empty;
            PendingCycleId = 0;


            Candidates = new List<ReinoCandidateEntry>();
        }
    }

    public static class ReinoElectionsSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_Reinos.bin");
        public static readonly Dictionary<int, ReinoCityData> _cities = new Dictionary<int, ReinoCityData>();
        public static readonly Dictionary<int, ReinoVoteRecord> _votesByVoter = new Dictionary<int, ReinoVoteRecord>();

        private static Timer _timer;
        public static int _activeElectionCycleId;
        public static int _lastAnnouncedCycleId;
        public static int _lastInstalledCycleId;
        private static int _lastKamayFineCycleId;

        public static bool _useManualPhase = false;
        public static ReinoFase _manualPhase = ReinoFase.Fechado;

        // ===== CONFIGURAÇÃO FÁCIL =====
        // Troque aqui pelos nomes das suas 4 cidades.
        public static readonly string[] CityNames = new string[]
        {
            "Aurora",
            "Xetá",
            "Lurone",
            "Willran"
        };

        // Horário do shard. Recife = UTC-3.
        public static readonly TimeSpan ServerUtcOffset = TimeSpan.FromHours(-3);

        public const int MaxCandidatesPerCity = 10;
        public const int CandidateStartDay = 1;
        public const int CandidateEndDay = 4; // dia 5 já vira votação, para não haver conflito
        public const int VoteStartDay = 5;
        public const int VoteEndDay = 20;
        public const int AnnouncementStartDay = 25;

        public static void Initialize()
        {
            EnsureCities();
            Load();

            EventSink.Login += OnLogin;
            EventSink.WorldSave += delegate { Save(); };

            _timer = Timer.DelayCall(TimeSpan.FromSeconds(5.0), TimeSpan.FromMinutes(5.0), CheckState);
            CheckState();
        }

        private static void EnsureCities()
        {
            for (int i = 0; i < CityNames.Length; i++)
            {
                if (!_cities.ContainsKey(i))
                    _cities[i] = new ReinoCityData(i, CityNames[i]);
            }
        }

        public static DateTime GetShardNow()
        {
            return DateTime.UtcNow + ServerUtcOffset;
        }

        public static string GetRequiredCultureId(int cityId)
        {
            switch (cityId)
            {
                case 0: return "kamay";    // Aurora
                case 1: return "matalun";  // Xetá
                case 2: return "sarangs";  // Lurone
                case 3: return "zosteros"; // Willran
                default: return String.Empty;
            }
        }

        public static bool IsPlayerAllowedForCity(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            string required = GetRequiredCultureId(cityId);

            if (String.IsNullOrWhiteSpace(required))
                return false;

            return String.Equals(pm.OSUCultureId, required, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCityPeopleName(int cityId)
        {
            switch (cityId)
            {
                case 0: return "Kamay";
                case 1: return "Matalun";
                case 2: return "Sarangs";
                case 3: return "Zosteros";
                default: return "desconhecido";
            }
        }
        public static string GetCityName(int cityId)
        {
            if (cityId < 0 || cityId >= CityNames.Length)
                return "Cidade inválida";

            return CityNames[cityId];
        }

        public static ReinoFase GetCurrentPhase()
        {
            if (_useManualPhase)
                return _manualPhase;

            DateTime now = GetShardNow();

            if (IsElectionMonth(now))
            {
                if (now.Day >= CandidateStartDay && now.Day <= CandidateEndDay)
                    return ReinoFase.Candidatura;

                if (now.Day >= VoteStartDay && now.Day <= VoteEndDay)
                    return ReinoFase.Votacao;

                if (now.Day >= AnnouncementStartDay)
                    return ReinoFase.Resultado;

                return ReinoFase.Fechado;
            }

            return ReinoFase.Governo;
        }

        public static bool IsElectionMonth(DateTime dt)
        {
            return (dt.Month % 2) == 1;
        }

        public static int GetElectionCycleId(DateTime dt)
        {
            int month = dt.Month;
            int year = dt.Year;

            if ((month % 2) == 0)
                month -= 1;

            if (month < 1)
            {
                month = 11;
                year -= 1;
            }

            return (year * 100) + month;
        }

        public static string DescribePhase()
        {
            ReinoFase phase = GetCurrentPhase();

            switch (phase)
            {
                case ReinoFase.Candidatura:
                    return "Candidaturas abertas (dias 1 a 4).";
                case ReinoFase.Votacao:
                    return "Votação aberta (dias 5 a 20).";
                case ReinoFase.Resultado:
                    return "Resultado disponível (a partir do dia 25).";
                case ReinoFase.Governo:
                    return "Mês de governo. Próxima eleição no próximo mês ímpar.";
                default:
                    return "Fora da janela de eleição.";
            }
        }

        public static void CheckState()
        {
            try
            {
                EnsureCities();

                DateTime now = GetShardNow();
                int cycleId = GetElectionCycleId(now);

                if (IsElectionMonth(now) && now.Day == CandidateStartDay && _activeElectionCycleId != cycleId)
                    StartNewElectionCycle(cycleId);
                else if (IsElectionMonth(now) && _activeElectionCycleId != cycleId)
                    EnsureCycleForCurrentElectionMonth(cycleId);

                if (IsElectionMonth(now) && now.Day == AnnouncementStartDay && now.Hour >= 19 && _lastAnnouncedCycleId != cycleId)
                    FinalizeAndAnnounce(cycleId);

                if (!IsElectionMonth(now))
                {
                    int previousCycleId = GetElectionCycleId(now);

                    if (_lastInstalledCycleId != previousCycleId)
                        InstallPendingGovernors(previousCycleId);
                }

                if (IsElectionMonth(now) && now.Day == AnnouncementStartDay && now.Hour >= 19 && _lastKamayFineCycleId != cycleId)
                    ApplyKamayNonVoterFines(cycleId);
            }
            catch
            {
            }
        }

        private static void EnsureCycleForCurrentElectionMonth(int cycleId)
        {
            if (_activeElectionCycleId == cycleId)
                return;

            _activeElectionCycleId = cycleId;

            foreach (KeyValuePair<int, ReinoCityData> kv in _cities)
            {
                if (kv.Value.PendingCycleId != cycleId)
                    kv.Value.Candidates.Clear();
            }

            _votesByVoter.Clear();
        }

        public static bool CancelCandidate(PlayerMobile pm, int cityId, out string reason)
        {
            reason = null;

            if (pm == null || pm.Deleted)
            {
                reason = "Jogador inválido.";
                return false;
            }

            if (GetCurrentPhase() != ReinoFase.Candidatura)
            {
                reason = "As candidaturas não estão abertas agora.";
                return false;
            }

            ReinoCityData city = GetCityData(cityId);

            if (city == null)
            {
                reason = "Cidade inválida.";
                return false;
            }

            for (int i = 0; i < city.Candidates.Count; i++)
            {
                if (city.Candidates[i].Serial == pm.Serial.Value)
                {
                    city.Candidates.RemoveAt(i);
                    Save();
                    return true;
                }
            }

            reason = "Você não está inscrito nesta eleição.";
            return false;
        }
        public static bool HasPlayerVoted(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return false;

            return _votesByVoter.ContainsKey(pm.Serial.Value);
        }

        public static string GetOfficeTitle(int cityId)
        {
            switch (cityId)
            {
                case 0: return "Primeiro Ministro";
                case 1: return "Sacerdotiza";
                case 2: return "Líder Soberano";
                case 3: return "Chefe do Conselho";
                default: return "governante";
            }
        }

        private static void StartNewElectionCycle(int cycleId)
        {
            _activeElectionCycleId = cycleId;
            _votesByVoter.Clear();

            foreach (KeyValuePair<int, ReinoCityData> kv in _cities)
                kv.Value.Candidates.Clear();

            Save();
        }

        private static void FinalizeAndAnnounce(int cycleId)
        {
            foreach (KeyValuePair<int, ReinoCityData> kv in _cities)
            {
                int cityId = kv.Key;
                ReinoCityData city = kv.Value;
                ReinoCandidateEntry winner = GetWinningCandidate(city);

                if (winner != null)
                {
                    city.PendingGovernorSerial = winner.Serial;
                    city.PendingGovernorName = winner.Nome;
                    city.PendingCycleId = cycleId;
                }
                else
                {
                    city.PendingGovernorSerial = 0;
                    city.PendingGovernorName = String.Empty;
                    city.PendingCycleId = cycleId;
                }

                DateTime now = GetShardNow();
                DateTime resultRelease = new DateTime(now.Year, now.Month, 25, 19, 0, 0);
                DateTime mandateStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(1);
                DateTime mandateEnd = mandateStart.AddMonths(2);

                city.ResultStartDateUtc = mandateStart.ToUniversalTime();
                city.ResultEndDateUtc = mandateEnd.ToUniversalTime();
                city.ResultVisibleUntilUtc = resultRelease.AddDays(3).ToUniversalTime();
            }

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                for (int cityId = 0; cityId < CityNames.Length; cityId++)
                {
                    if (pm.IsCitizenOf(GetCityName(cityId)) && HasVisibleResultForCity(cityId))
                    {
                        OSUResultadoEleicaoGump g = CreateResultGump(pm, cityId);

                        if (g != null)
                            pm.SendGump(g);
                    }
                }
            }

            _lastAnnouncedCycleId = cycleId;
            Save();
        }

        public static bool HasVisibleResultForCity(int cityId)
        {
            ReinoCityData city = GetCityData(cityId);

            if (city == null)
                return false;

            if (city.PendingGovernorSerial == 0 || String.IsNullOrEmpty(city.PendingGovernorName))
                return false;

            if (city.ResultVisibleUntilUtc == DateTime.MinValue)
                return false;

            return DateTime.UtcNow <= city.ResultVisibleUntilUtc;
        }


        public static void InstallPendingGovernors(int cycleId)
        {
            foreach (KeyValuePair<int, ReinoCityData> kv in _cities)
            {
                int cityId = kv.Key;
                ReinoCityData city = kv.Value;

                if (city.PendingCycleId != cycleId)
                    continue;

                if (city.PendingGovernorSerial == 0)
                    continue;

                PlayerMobile winner = FindPlayer(city.PendingGovernorSerial);

                if (winner == null || winner.Deleted || !winner.Alive)
                    continue;

                SetGovernor(city.Index, winner);
            }

            _lastInstalledCycleId = cycleId;
            Save();
        }

        public static bool CanRegisterCandidate(PlayerMobile pm, int cityId, out string reason)
        {
            reason = null;

            if (pm == null || pm.Deleted)
            {
                reason = "Jogador inválido.";
                return false;
            }

            if (!pm.Alive)
            {
                reason = "Você precisa estar vivo para se candidatar.";
                return false;
            }

            if (GetCurrentPhase() != ReinoFase.Candidatura)
            {
                reason = "As candidaturas não estão abertas agora.";
                return false;
            }

            ReinoCityData city = GetCityData(cityId);
            if (city == null)
            {
                reason = "Cidade inválida.";
                return false;
            }

            if (!IsPlayerAllowedForCity(pm, cityId))
            {
                reason = "Somente membros do povo " + GetCityPeopleName(cityId) + " podem se candidatar ao governo de " + GetCityName(cityId) + ".";
                return false;
            }

            if (!pm.IsCitizenOf(GetCityName(cityId)))
            {
                reason = "Somente cidadãos de " + GetCityName(cityId) + " podem se candidatar ao governo da cidade.";
                return false;
            }

            if (city.Candidates.Count >= MaxCandidatesPerCity)
            {
                reason = "Essa cidade já atingiu o limite de 10 candidatos.";
                return false;
            }

            if (FindCandidate(city, pm.Serial.Value) != null)
            {
                reason = "Você já está inscrito nessa eleição.";
                return false;
            }

            int existingCityId = FindCityWherePlayerIsCandidate(pm.Serial.Value);
            if (existingCityId >= 0)
            {
                reason = "Você já se candidatou em outra cidade nesta eleição.";
                return false;
            }

            return true;
        }

        public static bool RegisterCandidate(PlayerMobile pm, int cityId, out string reason)
        {
            if (!CanRegisterCandidate(pm, cityId, out reason))
                return false;

            ReinoCityData city = GetCityData(cityId);
            city.Candidates.Add(new ReinoCandidateEntry(pm));
            Save();
            return true;
        }

        public static void ApplyKamayNonVoterFines(int cycleId)
        {
            if (_lastKamayFineCycleId == cycleId)
                return;

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null || pm.Deleted)
                    continue;

                if (!String.Equals(pm.OSUCultureId, "kamay", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!pm.IsCitizenOf("Aurora"))
                    continue;

                if (HasPlayerVoted(pm))
                    continue;

                if (pm.BankBox != null)
                {
                    int remaining = 2000;

                    for (int i = pm.BankBox.Items.Count - 1; i >= 0 && remaining > 0; i--)
                    {
                        Gold gold = pm.BankBox.Items[i] as Gold;

                        if (gold == null)
                            continue;

                        if (gold.Amount <= remaining)
                        {
                            remaining -= gold.Amount;
                            gold.Delete();
                        }
                        else
                        {
                            gold.Amount -= remaining;
                            remaining = 0;
                        }
                    }
                }

                pm.SendMessage("Você não votou nas eleições de Aurora e recebeu uma multa de 2000 moedas de ouro.");
            }

            _lastKamayFineCycleId = cycleId;
        }

        public static bool CanVote(PlayerMobile pm, int cityId, int candidateSerial, out string reason)
        {
            reason = null;

            if (pm == null || pm.Deleted)
            {
                reason = "Jogador inválido.";
                return false;
            }

            if (!pm.Alive)
            {
                reason = "Você precisa estar vivo para votar.";
                return false;
            }

            if (GetCurrentPhase() != ReinoFase.Votacao)
            {
                reason = "A votação não está aberta agora.";
                return false;
            }

            if (_votesByVoter.ContainsKey(pm.Serial.Value))
            {
                reason = "Você já votou nesta eleição.";
                return false;
            }

            ReinoCityData city = GetCityData(cityId);
            if (city == null)
            {
                reason = "Cidade inválida.";
                return false;
            }

            if (!pm.IsCitizenOf(GetCityName(cityId)))
            {
                reason = "Somente cidadãos de " + GetCityName(cityId) + " podem votar nesta eleição.";
                return false;
            }

            ReinoCandidateEntry entry = FindCandidate(city, candidateSerial);
            if (entry == null)
            {
                reason = "Candidato inválido.";
                return false;
            }
            return true;
        }

        public static bool RegisterVote(PlayerMobile pm, int cityId, int candidateSerial, out string reason)
        {
            if (!CanVote(pm, cityId, candidateSerial, out reason))
                return false;

            ReinoCityData city = GetCityData(cityId);
            ReinoCandidateEntry entry = FindCandidate(city, candidateSerial);

            entry.Votes++;
            _votesByVoter[pm.Serial.Value] = new ReinoVoteRecord
            {
                VoterSerial = pm.Serial.Value,
                CityId = cityId,
                CandidateSerial = candidateSerial
            };

            Save();
            return true;
        }

        public static ReinoCityData GetCityData(int cityId)
        {
            ReinoCityData city;
            if (_cities.TryGetValue(cityId, out city))
                return city;

            return null;
        }

        public static OSUResultadoEleicaoGump CreateResultGump(PlayerMobile pm, int cityId)
        {
            ReinoCityData city = GetCityData(cityId);

            if (city == null)
                return null;

            DateTime start = city.ResultStartDateUtc == DateTime.MinValue ? DateTime.UtcNow : city.ResultStartDateUtc;
            DateTime end = city.ResultEndDateUtc == DateTime.MinValue ? start.AddMonths(2) : city.ResultEndDateUtc;

            pm.SendMessage("DEBUG Resultado -> Cidade: {0} | WinnerName: {1}", cityId, city.PendingGovernorName);

            return new OSUResultadoEleicaoGump(pm, cityId, city.PendingGovernorName, start.ToLocalTime(), end.ToLocalTime());
        }

        public static ReinoCandidateEntry FindCandidate(ReinoCityData city, int serialValue)
        {
            if (city == null)
                return null;

            for (int i = 0; i < city.Candidates.Count; i++)
            {
                if (city.Candidates[i].Serial == serialValue)
                    return city.Candidates[i];
            }

            return null;
        }

        public static int FindCityWherePlayerIsCandidate(int serialValue)
        {
            foreach (KeyValuePair<int, ReinoCityData> kv in _cities)
            {
                if (FindCandidate(kv.Value, serialValue) != null)
                    return kv.Key;
            }

            return -1;
        }

        public static ReinoCandidateEntry GetWinningCandidate(ReinoCityData city)
        {
            if (city == null || city.Candidates.Count == 0)
                return null;

            ReinoCandidateEntry winner = null;

            for (int i = 0; i < city.Candidates.Count; i++)
            {
                ReinoCandidateEntry c = city.Candidates[i];

                if (winner == null)
                {
                    winner = c;
                    continue;
                }

                if (c.Votes > winner.Votes)
                {
                    winner = c;
                    continue;
                }

                if (c.Votes == winner.Votes && String.Compare(c.Nome, winner.Nome, StringComparison.OrdinalIgnoreCase) < 0)
                    winner = c;
            }

            return winner;
        }

        public static void SetGovernor(int cityId, PlayerMobile newGovernor)
        {
            ReinoCityData city;

            if (!_cities.TryGetValue(cityId, out city))
                return;

            if (newGovernor != null && !newGovernor.Deleted)
            {
                if (!IsPlayerAllowedForCity(newGovernor, cityId))
                    return;
            }

            PlayerMobile oldGovernor = FindPlayer(city.GovernorSerial);

            if (oldGovernor != null)
                ReinoAccessHelper.RevokeGovernorAccess(oldGovernor, cityId);

            city.GovernorSerial = 0;
            city.GovernorName = String.Empty;
            city.GovernorSinceUtc = DateTime.MinValue;

            if (newGovernor == null || newGovernor.Deleted)
                return;

            city.GovernorSerial = newGovernor.Serial.Value;
            city.GovernorName = newGovernor.Name;
            city.GovernorSinceUtc = DateTime.UtcNow;

            ReinoAccessHelper.GrantGovernorAccess(newGovernor, cityId, true);
        }

        public static PlayerMobile FindPlayer(int serialValue)
        {
            if (serialValue <= 0)
                return null;

            Mobile m = World.FindMobile(serialValue);
            return m as PlayerMobile;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            CheckState();

            ReinoFase phase = GetCurrentPhase();

            if (phase == ReinoFase.Candidatura)
            {
                pm.SendMessage("As eleições dos reinos estão com candidaturas abertas. Use a urna da cidade para se candidatar.");
            }
            else if (phase == ReinoFase.Votacao)
            {
                pm.SendMessage("As eleições dos reinos estão com votação aberta. Cada jogador pode votar apenas 1 vez por eleição.");
            }
            else if (phase == ReinoFase.Resultado)
            {
                for (int cityId = 0; cityId < CityNames.Length; cityId++)
                {
                    if (pm.IsCitizenOf(GetCityName(cityId)) && HasVisibleResultForCity(cityId))
                    {
                        OSUResultadoEleicaoGump g = CreateResultGump(pm, cityId);

                        if (g != null)
                            pm.SendGump(g);
                    }
                }
            }
        }
        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(1); // version
                    bw.Write(_activeElectionCycleId);
                    bw.Write(_lastAnnouncedCycleId);
                    bw.Write(_lastInstalledCycleId);

                    bw.Write(_cities.Count);
                    foreach (KeyValuePair<int, ReinoCityData> kv in _cities)
                    {
                        ReinoCityData city = kv.Value;

                        bw.Write(kv.Key);
                        bw.Write(city.GovernorSerial);
                        bw.Write(city.GovernorName ?? String.Empty);
                        bw.Write(city.GovernorSinceUtc.ToBinary());

                        bw.Write(city.PendingGovernorSerial);
                        bw.Write(city.PendingGovernorName ?? String.Empty);
                        bw.Write(city.PendingCycleId);

                        bw.Write(city.ResultStartDateUtc.ToBinary());
                        bw.Write(city.ResultEndDateUtc.ToBinary());
                        bw.Write(city.ResultVisibleUntilUtc.ToBinary());

                        bw.Write(city.Candidates.Count);
                        for (int i = 0; i < city.Candidates.Count; i++)
                        {
                            bw.Write(city.Candidates[i].Serial);
                            bw.Write(city.Candidates[i].Nome ?? String.Empty);
                            bw.Write(city.Candidates[i].Votes);
                        }
                    }

                    bw.Write(_votesByVoter.Count);
                    foreach (KeyValuePair<int, ReinoVoteRecord> kv in _votesByVoter)
                    {
                        ReinoVoteRecord vr = kv.Value;
                        bw.Write(vr.VoterSerial);
                        bw.Write(vr.CityId);
                        bw.Write(vr.CandidateSerial);
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
                EnsureCities();

                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();

                    if (version >= 1)
                    {
                        _activeElectionCycleId = br.ReadInt32();
                        _lastAnnouncedCycleId = br.ReadInt32();
                        _lastInstalledCycleId = br.ReadInt32();

                        _cities.Clear();
                        int cityCount = br.ReadInt32();
                        for (int i = 0; i < cityCount; i++)
                        {
                            int cityId = br.ReadInt32();
                            ReinoCityData city = new ReinoCityData(cityId, GetCityName(cityId));

                            city.Index = cityId;
                            city.CityName = GetCityName(cityId);

                            city.GovernorSerial = br.ReadInt32();
                            city.GovernorName = br.ReadString();
                            city.GovernorSinceUtc = DateTime.FromBinary(br.ReadInt64());

                            city.PendingGovernorSerial = br.ReadInt32();
                            city.PendingGovernorName = br.ReadString();
                            city.PendingCycleId = br.ReadInt32();

                            city.ResultStartDateUtc = DateTime.FromBinary(br.ReadInt64());
                            city.ResultEndDateUtc = DateTime.FromBinary(br.ReadInt64());
                            city.ResultVisibleUntilUtc = DateTime.FromBinary(br.ReadInt64());

                            int candCount = br.ReadInt32();
                            for (int c = 0; c < candCount; c++)
                            {
                                ReinoCandidateEntry entry = new ReinoCandidateEntry();
                                entry.Serial = br.ReadInt32();
                                entry.Nome = br.ReadString();
                                entry.Votes = br.ReadInt32();
                                city.Candidates.Add(entry);
                            }

                            _cities[cityId] = city;
                        }

                        _votesByVoter.Clear();
                        int voteCount = br.ReadInt32();
                        for (int i = 0; i < voteCount; i++)
                        {
                            ReinoVoteRecord vr = new ReinoVoteRecord();
                            vr.VoterSerial = br.ReadInt32();
                            vr.CityId = br.ReadInt32();
                            vr.CandidateSerial = br.ReadInt32();
                            _votesByVoter[vr.VoterSerial] = vr;
                        }
                    }
                }

                EnsureCities();
            }
            catch
            {
            }
        }
    }
}
