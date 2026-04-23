using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Items;
using Server.Custom.Systems.Common.Engine;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;
using Server.Custom.Systems.Religion;
using Server.Custom.Systems.Templos.Items;

namespace Server.Custom.Systems.Templos
{
    public enum TemploEventoTipo
    {
        None = 0,
        Rito,
        Casamento,
        Funeral
    }

    public enum TemploMusicaRito
    {
        Canticos = 0,
        Coro,
        Tambores
    }

    public sealed class TemploStopMusic : Packet
    {
        public static readonly Packet Instance = Packet.SetStatic(new TemploStopMusic());

        public TemploStopMusic() : base(0x6D, 3)
        {
            m_Stream.Write((short)0x1FFF);
        }
    }

    public sealed class TemploPlayMusic : Packet
    {
        public TemploPlayMusic(short number) : base(0x6D, 3)
        {
            m_Stream.Write(number);
        }
    }

    public class TemploDonationBundle
    {
        public int Gold;
        public int Cloth;
        public int Iron;
        public int Wood;

        public void Clear()
        {
            Gold = Cloth = Iron = Wood = 0;
        }
    }

    public class TemploActiveEvent
    {
        public TemploEventoTipo Type;
        public DateTime StartUtc;
        public int HourlyCost;
        public int PaidAmount;
        public int MusicIndex;
        public Timer DelayedStartTimer;
        public Timer PresenceTimer;
        public HashSet<int> PlayersWithMusic;
        public List<int> SpawnedSerials;
        public Dictionary<int, int> FuneralLightRestore;

        public TemploActiveEvent()
        {
            Type = TemploEventoTipo.None;
            StartUtc = DateTime.UtcNow;
            HourlyCost = 0;
            PaidAmount = 0;
            MusicIndex = 0;
            DelayedStartTimer = null;
            PresenceTimer = null;
            PlayersWithMusic = new HashSet<int>();
            SpawnedSerials = new List<int>();
            FuneralLightRestore = new Dictionary<int, int>();
        }
    }

    public class TemploState
    {
        public string ConstructionKey;
        public int CityId;
        public string SelectedReligionId;
        public TemploMusicaRito SelectedRiteMusic;
        public bool DoorsClosedToPublic;
        public TemploDonationBundle WeeklyDonations;
        public DateTime WeeklyStampUtc;
        public int AltarSerial;
        public int ChestSerial;
        public int StatueSerial;
        public int LastEventTotalCost;
        public TemploActiveEvent ActiveEvent;

        public TemploState()
        {
            ConstructionKey = String.Empty;
            CityId = -1;
            SelectedReligionId = String.Empty;
            SelectedRiteMusic = TemploMusicaRito.Canticos;
            DoorsClosedToPublic = false;
            WeeklyDonations = new TemploDonationBundle();
            WeeklyStampUtc = DateTime.MinValue;
            AltarSerial = 0;
            ChestSerial = 0;
            StatueSerial = 0;
            LastEventTotalCost = 0;
            ActiveEvent = null;
        }
    }

    public static class TemploSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_Templos_v1.bin");
        private static readonly Dictionary<string, TemploState> m_States = new Dictionary<string, TemploState>(StringComparer.OrdinalIgnoreCase);
        private static Timer m_PulseTimer;
        private static readonly TimeSpan BellLeadTime = TimeSpan.FromSeconds(10.0);

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };

            if (m_PulseTimer != null)
                m_PulseTimer.Stop();

            m_PulseTimer = Timer.DelayCall(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0), Pulse);
        }

        private static void Pulse()
        {
            DateTime now = DateTime.UtcNow;
            List<string> keys = new List<string>(m_States.Keys);

            for (int i = 0; i < keys.Count; i++)
            {
                TemploState state;
                if (!m_States.TryGetValue(keys[i], out state) || state == null)
                    continue;

                EnsureWeeklyStamp(state);

                if (state.ActiveEvent == null)
                    continue;

                ProcessEventPulse(state, now);
            }
        }

        public static string GetConstructionKeyByCityId(int cityId)
        {
            if (cityId < 0)
                return null;

            List<ReinoConstructionRuntimeInfo> list = ReinoMaintenanceSystem.GetCityConstructions(cityId);

            if (list == null || list.Count == 0)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                ReinoConstructionRuntimeInfo info = list[i];

                if (info == null || info.Definition == null || String.IsNullOrWhiteSpace(info.Key))
                    continue;

                string id = info.Definition.Id ?? String.Empty;

                if (id.IndexOf("templo", StringComparison.OrdinalIgnoreCase) >= 0)
                    return info.Key;
            }

            return null;
        }

        private static void ProcessEventPulse(TemploState state, DateTime now)
        {
            if (state == null || state.ActiveEvent == null)
                return;

            TemploActiveEvent ev = state.ActiveEvent;
            int due = GetDueAmount(ev.HourlyCost, now - ev.StartUtc);
            int delta = due - ev.PaidAmount;

            if (delta > 0)
            {
                BauDoacoesTemplo chest = FindChest(state.ChestSerial);
                if (chest == null || !chest.TryConsumeGold(delta))
                {
                    StopEvent(state.ConstructionKey, "O evento foi encerrado porque o baú do templo ficou sem moedas suficientes.", true);
                    return;
                }

                ev.PaidAmount += delta;
            }

            if (ev.Type == TemploEventoTipo.Funeral)
                MaintainFuneralDarkness(state);
            else
                ReleasePlayersNotInFuneral(state);
        }

        public static bool CanAccessTemple(PlayerMobile pm, int cityId, string constructionKey)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null || !role.IsOccupied)
                return false;

            if (String.IsNullOrWhiteSpace(role.LinkedConstructionKey))
                return false;

            return String.Equals(role.LinkedConstructionKey, constructionKey, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTempleClosedToPublic(string constructionKey)
        {
            TemploState state = GetState(constructionKey, -1, false);
            return state != null && state.DoorsClosedToPublic;
        }

        public static bool IsInsideTempleLot(string constructionKey, Mobile m)
        {
            if (m == null || m.Deleted)
                return false;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(constructionKey);
            if (info == null || info.Lot == null || m.Map != info.Lot.Map)
                return false;

            return info.Lot.Contains(m.Location);
        }

        public static bool ToggleDoors(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;

            if (!CanAccessTemple(actor, cityId, constructionKey))
            {
                message = "Somente o líder ou o cargo ligado ao templo pode mudar isso.";
                return false;
            }

            TemploState state = GetState(constructionKey, cityId, true);
            state.DoorsClosedToPublic = !state.DoorsClosedToPublic;
            message = state.DoorsClosedToPublic ? "As portas do templo agora estão fechadas ao público." : "As portas do templo agora estão abertas ao público.";
            return true;
        }

        public static OSUReligionDefinition GetSelectedReligion(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);

            if (state == null)
                return GetFirstSelectableReligion();

            if (String.IsNullOrWhiteSpace(state.SelectedReligionId))
            {
                OSUReligionDefinition first = GetFirstSelectableReligion();
                state.SelectedReligionId = first != null ? first.Id : "none";
                return first;
            }

            OSUReligionDefinition def = OSUReligionRegistry.GetById(state.SelectedReligionId);
            if (def == null || IsExcludedReligion(def))
            {
                def = GetFirstSelectableReligion();
                state.SelectedReligionId = def != null ? def.Id : "none";
            }

            return def;
        }

        public static int GetRegisteredChestSerial(string constructionKey)
        {
            TemploState state = GetState(constructionKey, -1, false);
            return state != null ? state.ChestSerial : 0;
        }

        public static void OnTempleMultiDeleted(string constructionKey)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return;

            TemploState state = GetState(constructionKey, -1, false);
            if (state == null)
                return;

            StopEvent(constructionKey, String.Empty, false);

            state.AltarSerial = 0;
            state.StatueSerial = 0;

            // NÃO zera o ChestSerial
            // NÃO remove o estado
            // Assim o baú e as moedas continuam existindo
        }

        public static string CycleReligion(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;

            if (!CanAccessTemple(actor, cityId, constructionKey))
            {
                message = "Somente o líder ou o cargo ligado ao templo pode escolher o deus do templo.";
                return String.Empty;
            }

            List<OSUReligionDefinition> all = GetSelectableReligions();
            if (all.Count == 0)
            {
                message = "Nenhum deus configurado foi encontrado.";
                return String.Empty;
            }

            TemploState state = GetState(constructionKey, cityId, true);
            int index = 0;

            for (int i = 0; i < all.Count; i++)
            {
                if (String.Equals(all[i].Id, state.SelectedReligionId, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            index++;
            if (index >= all.Count)
                index = 0;

            state.SelectedReligionId = all[index].Id;
            RefreshTempleStatue(constructionKey);
            message = String.Equals(all[index].Id, "none", StringComparison.OrdinalIgnoreCase) ? "O templo agora está sem deus." : "O templo agora está preparado para celebrar " + all[index].Name + ".";
            return state.SelectedReligionId;
        }

        public static TemploMusicaRito GetSelectedRiteMusic(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);
            return state != null ? state.SelectedRiteMusic : TemploMusicaRito.Canticos;
        }

        public static string CycleRiteMusic(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;

            if (!CanAccessTemple(actor, cityId, constructionKey))
            {
                message = "Somente o líder ou o cargo ligado ao templo pode escolher a música do rito.";
                return GetRiteMusicLabel(TemploMusicaRito.Canticos);
            }

            TemploState state = GetState(constructionKey, cityId, true);

            switch (state.SelectedRiteMusic)
            {
                default:
                case TemploMusicaRito.Canticos:
                    state.SelectedRiteMusic = TemploMusicaRito.Coro;
                    break;
                case TemploMusicaRito.Coro:
                    state.SelectedRiteMusic = TemploMusicaRito.Tambores;
                    break;
                case TemploMusicaRito.Tambores:
                    state.SelectedRiteMusic = TemploMusicaRito.Canticos;
                    break;
            }

            message = "A música do rito agora é " + GetRiteMusicLabel(state.SelectedRiteMusic) + ".";
            return GetRiteMusicLabel(state.SelectedRiteMusic);
        }

        public static bool StartRite(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;
            if (!ValidateStart(actor, constructionKey, cityId, out message))
                return false;

            TemploState state = GetState(constructionKey, cityId, true);
            OSUReligionDefinition religion = GetSelectedReligion(constructionKey, cityId);
            if (religion == null || String.Equals(religion.Id, "none", StringComparison.OrdinalIgnoreCase))
            {
                message = "Você precisa escolher um deus para iniciar um rito.";
                return false;
            }

            int[] itemPool = religion.TempleRiteItemIds;
            Point3D[] offsets = TemploAuroraDefinition.GetRiteOffsets();

            if (itemPool == null || itemPool.Length == 0 || offsets == null || offsets.Length < 3)
            {
                message = "A configuração do rito ainda não está pronta.";
                return false;
            }

            List<int> chosen = ChooseUnique(itemPool, 3);
            if (chosen.Count < 3)
            {
                message = "A lista de itens do rito precisa ter pelo menos 3 itens.";
                return false;
            }

            if (!StartEventCommon(actor, state, TemploEventoTipo.Rito, GetDefaultHourlyCost(TemploEventoTipo.Rito), GetMusicIndexForRiteMusic(state.SelectedRiteMusic), out message))
                return false;

            SpawnCeremonyItems(state, offsets, chosen, false, 0);
            message = "O rito começou.";
            return true;
        }

        public static bool StartWedding(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;
            if (!ValidateStart(actor, constructionKey, cityId, out message))
                return false;

            TemploState state = GetState(constructionKey, cityId, true);
            OSUReligionDefinition religion = GetSelectedReligion(constructionKey, cityId);
            if (religion == null)
            {
                message = "Nenhum deus foi escolhido para este templo.";
                return false;
            }

            int[] itemPool = religion.TempleWeddingItemIds;
            Point3D[] offsets = TemploAuroraDefinition.GetMarriageOffsets();

            if (itemPool == null || itemPool.Length == 0 || offsets == null || offsets.Length == 0)
            {
                message = "A configuração do casamento ainda não está pronta.";
                return false;
            }

            if (!StartEventCommon(actor, state, TemploEventoTipo.Casamento, GetDefaultHourlyCost(TemploEventoTipo.Casamento), 106, out message))
                return false;

            List<int> chosen = new List<int>();
            for (int i = 0; i < offsets.Length; i++)
                chosen.Add(itemPool[Utility.Random(itemPool.Length)]);

            SpawnCeremonyItems(state, offsets, chosen, false, 0);
            message = "O casamento começou.";
            return true;
        }

        public static bool StartFuneral(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;
            if (!ValidateStart(actor, constructionKey, cityId, out message))
                return false;

            TemploState state = GetState(constructionKey, cityId, true);
            OSUReligionDefinition religion = GetSelectedReligion(constructionKey, cityId);
            if (religion == null)
            {
                message = "Nenhum deus foi escolhido para este templo.";
                return false;
            }

            Point3D[] candleOffsets = TemploAuroraDefinition.GetFuneralCandleOffsets();
            Point3D coffinOffset = TemploAuroraDefinition.GetFuneralCoffinOffset();

            if (candleOffsets == null || candleOffsets.Length == 0)
            {
                message = "As coordenadas do funeral ainda não foram configuradas.";
                return false;
            }

            if (!StartEventCommon(actor, state, TemploEventoTipo.Funeral, GetDefaultHourlyCost(TemploEventoTipo.Funeral), 107, out message))
                return false;

            List<int> candleIds = GetLitCandlePool();

            for (int i = 0; i < candleOffsets.Length; i++)
            {
                int candleId = candleIds[Utility.Random(candleIds.Count)];
                SpawnSingleCeremonyItem(state, candleOffsets[i], candleId, true, 0);
            }

            SpawnSingleCeremonyItem(state, coffinOffset, religion.TempleFuneralCoffinItemId, false, 0);
            MaintainFuneralDarkness(state);
            message = "O funeral começou.";
            return true;
        }

        private static bool ValidateStart(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;

            if (!CanAccessTemple(actor, cityId, constructionKey))
            {
                message = "Somente o líder ou o cargo ligado ao templo pode iniciar cerimônias.";
                return false;
            }

            TemploState state = GetState(constructionKey, cityId, true);
            if (state.ActiveEvent != null)
            {
                message = "Já existe um evento ativo neste templo.";
                return false;
            }

            return true;
        }

        private static bool StartEventCommon(PlayerMobile actor, TemploState state, TemploEventoTipo type, int hourlyCost, int musicIndex, out string message)
        {
            message = String.Empty;

            BauDoacoesTemplo chest = FindChest(state.ChestSerial);
            if (chest == null)
            {
                message = "O baú de doações do templo não foi encontrado.";
                return false;
            }

            if (!chest.TryConsumeGold(hourlyCost))
            {
                message = "Não há moedas suficientes no baú para começar esse evento.";
                return false;
            }

            if (state.ActiveEvent != null)
                StopEvent(state.ConstructionKey, String.Empty, false);

            state.ActiveEvent = new TemploActiveEvent();
            state.ActiveEvent.Type = type;
            state.ActiveEvent.StartUtc = DateTime.UtcNow;
            state.ActiveEvent.HourlyCost = hourlyCost;
            state.ActiveEvent.PaidAmount = hourlyCost;
            state.ActiveEvent.MusicIndex = musicIndex;

            TemploActiveEvent startedEvent = state.ActiveEvent;
            string constructionKey = state.ConstructionKey;

            BroadcastStopMusicToLot(constructionKey);
            BroadcastTempleBells(state.ConstructionKey, state.CityId);

            startedEvent.DelayedStartTimer = Timer.DelayCall(BellLeadTime, delegate
            {
                TemploState current = GetState(constructionKey, -1, false);

                if (current == null || current.ActiveEvent == null)
                    return;

                if (!Object.ReferenceEquals(current.ActiveEvent, startedEvent))
                    return;

                StartPresenceMonitor(constructionKey, startedEvent);
            });

            return true;
        }

        private static void StopEventAudio(TemploActiveEvent ev)
        {
            if (ev == null)
                return;

            if (ev.DelayedStartTimer != null)
            {
                ev.DelayedStartTimer.Stop();
                ev.DelayedStartTimer = null;
            }

            if (ev.PresenceTimer != null)
            {
                ev.PresenceTimer.Stop();
                ev.PresenceTimer = null;
            }

            StopTrackedMusic(ev);
        }

        private static void StartPresenceMonitor(string constructionKey, TemploActiveEvent ev)
        {
            if (ev == null)
                return;

            if (ev.PresenceTimer != null)
            {
                ev.PresenceTimer.Stop();
                ev.PresenceTimer = null;
            }

            ev.PresenceTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1.0), delegate
            {
                PulsePresenceMonitor(constructionKey, ev);
            });
        }

        private static void PulsePresenceMonitor(string constructionKey, TemploActiveEvent ev)
        {
            if (ev == null)
                return;

            TemploState state = GetState(constructionKey, -1, false);

            if (state == null || state.ActiveEvent == null || !Object.ReferenceEquals(state.ActiveEvent, ev))
            {
                if (ev.PresenceTimer != null)
                {
                    ev.PresenceTimer.Stop();
                    ev.PresenceTimer = null;
                }

                return;
            }

            HashSet<int> inside = new HashSet<int>();
            List<PlayerMobile> players = GetPlayersInLot(constructionKey);

            for (int i = 0; i < players.Count; i++)
            {
                PlayerMobile pm = players[i];

                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                int serial = pm.Serial.Value;
                inside.Add(serial);

                if (!ev.PlayersWithMusic.Contains(serial))
                {
                    pm.Send(new TemploPlayMusic((short)ev.MusicIndex));
                    ev.PlayersWithMusic.Add(serial);
                }
            }

            List<int> tracked = new List<int>(ev.PlayersWithMusic);

            for (int i = 0; i < tracked.Count; i++)
            {
                int serial = tracked[i];

                if (inside.Contains(serial))
                    continue;

                Mobile mob;
                if (World.Mobiles.TryGetValue(serial, out mob))
                {
                    PlayerMobile pm = mob as PlayerMobile;

                    if (pm != null && !pm.Deleted && pm.NetState != null)
                        pm.Send(TemploStopMusic.Instance);
                }

                ev.PlayersWithMusic.Remove(serial);
            }
        }

        private static void StopTrackedMusic(TemploActiveEvent ev)
        {
            if (ev == null || ev.PlayersWithMusic == null || ev.PlayersWithMusic.Count == 0)
                return;

            List<int> tracked = new List<int>(ev.PlayersWithMusic);

            for (int i = 0; i < tracked.Count; i++)
            {
                Mobile mob;
                if (World.Mobiles.TryGetValue(tracked[i], out mob))
                {
                    PlayerMobile pm = mob as PlayerMobile;

                    if (pm != null && !pm.Deleted && pm.NetState != null)
                        pm.Send(TemploStopMusic.Instance);
                }
            }

            ev.PlayersWithMusic.Clear();
        }

        public static bool EndEvent(PlayerMobile actor, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;

            if (!CanAccessTemple(actor, cityId, constructionKey))
            {
                message = "Somente o líder ou o cargo ligado ao templo pode encerrar o evento.";
                return false;
            }

            TemploState state = GetState(constructionKey, cityId, false);
            if (state == null || state.ActiveEvent == null)
            {
                message = "Não há evento ativo no momento.";
                return false;
            }

            StopEvent(constructionKey, "O evento foi encerrado.", true);
            message = "O evento foi encerrado.";
            return true;
        }

        private static void StopEvent(string constructionKey, string notice, bool notifyLot)
        {
            TemploState state = GetState(constructionKey, -1, false);
            if (state == null || state.ActiveEvent == null)
                return;

            TemploActiveEvent ev = state.ActiveEvent;
            state.LastEventTotalCost = ev.PaidAmount;

            StopEventAudio(ev);
            BroadcastStopMusicToLot(constructionKey);
            DeleteSpawnedItems(ev.SpawnedSerials);
            RestoreFuneralLights(ev);
            state.ActiveEvent = null;
        }

        public static TemploDonationBundle GetWeeklyDonations(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);
            EnsureWeeklyStamp(state);
            return state.WeeklyDonations;
        }

        public static void RecordDonation(string constructionKey, int cityId, int gold, int cloth, int iron, int wood)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return;

            TemploState state = GetState(constructionKey, cityId, true);
            EnsureWeeklyStamp(state);

            state.WeeklyDonations.Gold += Math.Max(0, gold);
            state.WeeklyDonations.Cloth += Math.Max(0, cloth);
            state.WeeklyDonations.Iron += Math.Max(0, iron);
            state.WeeklyDonations.Wood += Math.Max(0, wood);
        }

        public static void SyncPlacedAssets(string constructionKey, int cityId, int altarSerial, int chestSerial, int statueSerial)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return;

            TemploState state = GetState(constructionKey, cityId, true);
            state.CityId = cityId;
            if (altarSerial > 0) state.AltarSerial = altarSerial;
            if (chestSerial > 0) state.ChestSerial = chestSerial;
            state.StatueSerial = statueSerial;
            EnsureWeeklyStamp(state);
        }

        public static void DeleteConstructionData(string constructionKey)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return;

            StopEvent(constructionKey, String.Empty, false);
            m_States.Remove(constructionKey);
        }

        public static int GetChestGold(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);
            BauDoacoesTemplo chest = FindChest(state != null ? state.ChestSerial : 0);
            return chest != null ? chest.StoredGold : 0;
        }

        public static int GetDisplayedHourlyCost(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);
            if (state != null && state.ActiveEvent != null)
                return state.ActiveEvent.HourlyCost;

            return 0;
        }

        public static int GetDisplayedTotalCost(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);
            if (state == null)
                return 0;

            if (state.ActiveEvent == null)
                return 0;

            return GetDueAmount(state.ActiveEvent.HourlyCost, DateTime.UtcNow - state.ActiveEvent.StartUtc);
        }

        public static TemploEventoTipo GetActiveEventType(string constructionKey, int cityId)
        {
            TemploState state = GetState(constructionKey, cityId, true);
            return state != null && state.ActiveEvent != null ? state.ActiveEvent.Type : TemploEventoTipo.None;
        }

        public static string GetEventLabel(TemploEventoTipo type)
        {
            switch (type)
            {
                case TemploEventoTipo.Rito: return "Rito";
                case TemploEventoTipo.Casamento: return "Casamento";
                case TemploEventoTipo.Funeral: return "Funeral";
                default: return "Nenhum";
            }
        }

        public static string GetRiteMusicLabel(TemploMusicaRito music)
        {
            switch (music)
            {
                case TemploMusicaRito.Coro: return "Coro";
                case TemploMusicaRito.Tambores: return "Tambores";
                default: return "Cânticos";
            }
        }

        public static int GetDefaultHourlyCost(TemploEventoTipo type)
        {
            switch (type)
            {
                case TemploEventoTipo.Casamento: return 700;
                case TemploEventoTipo.Funeral: return 400;
                default: return 100;
            }
        }

        private static int GetMusicIndexForRiteMusic(TemploMusicaRito music)
        {
            switch (music)
            {
                case TemploMusicaRito.Coro: return 103;
                case TemploMusicaRito.Tambores: return 104;
                default: return 105;
            }
        }

        private static TemploState GetState(string constructionKey, int cityId, bool create)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return null;

            TemploState state;
            if (m_States.TryGetValue(constructionKey, out state))
                return state;

            if (!create)
                return null;

            state = new TemploState();
            state.ConstructionKey = constructionKey;
            state.CityId = cityId;
            OSUReligionDefinition first = GetFirstSelectableReligion();
            state.SelectedReligionId = first != null ? first.Id : "none";
            state.SelectedRiteMusic = TemploMusicaRito.Canticos;
            state.WeeklyStampUtc = GetCurrentWeeklyStampUtc();
            m_States[constructionKey] = state;
            return state;
        }

        private static void EnsureWeeklyStamp(TemploState state)
        {
            if (state == null)
                return;

            DateTime current = GetCurrentWeeklyStampUtc();
            if (state.WeeklyStampUtc == current)
                return;

            state.WeeklyStampUtc = current;
            if (state.WeeklyDonations == null)
                state.WeeklyDonations = new TemploDonationBundle();
            else
                state.WeeklyDonations.Clear();
        }

        private static DateTime GetCurrentWeeklyStampUtc()
        {
            DateTime local = DateTime.UtcNow.AddHours(-3.0);
            int daysSinceMonday = ((int)local.DayOfWeek + 6) % 7;
            DateTime monday = local.Date.AddDays(-daysSinceMonday).AddHours(14.0);

            if (local < monday)
                monday = monday.AddDays(-7.0);

            return monday.AddHours(3.0);
        }

        private static int GetDueAmount(int hourlyCost, TimeSpan elapsed)
        {
            hourlyCost = Math.Max(0, hourlyCost);

            if (hourlyCost <= 0)
                return 0;

            if (elapsed.TotalMinutes <= 60.0)
                return hourlyCost;

            return (int)Math.Ceiling(hourlyCost * (elapsed.TotalMinutes / 60.0));
        }

        private static void SpawnCeremonyItems(TemploState state, Point3D[] offsets, List<int> itemIds, bool lit, int hue)
        {
            if (state == null || state.ActiveEvent == null || offsets == null || itemIds == null)
                return;

            int count = Math.Min(offsets.Length, itemIds.Count);
            for (int i = 0; i < count; i++)
                SpawnSingleCeremonyItem(state, offsets[i], itemIds[i], lit, hue);
        }

        private static void SpawnSingleCeremonyItem(TemploState state, Point3D offset, int itemId, bool lit, int hue)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(state.ConstructionKey);
            if (info == null || info.Lot == null || info.Lot.Map == null)
                return;

            Point3D anchor = info.Lot.NorthWest;
            Static st = new Static(itemId);
            st.Movable = false;
            st.Name = "decoração de cerimônia";

            if (lit)
                st.Light = LightType.Circle150;

            if (hue > 0)
                st.Hue = hue;

            st.MoveToWorld(new Point3D(anchor.X + offset.X, anchor.Y + offset.Y, anchor.Z + offset.Z), info.Lot.Map);
            state.ActiveEvent.SpawnedSerials.Add(st.Serial.Value);
        }

        private static List<int> ChooseUnique(int[] pool, int amount)
        {
            List<int> src = new List<int>();
            List<int> dst = new List<int>();

            if (pool != null)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!src.Contains(pool[i]))
                        src.Add(pool[i]);
                }
            }

            while (src.Count > 0 && dst.Count < amount)
            {
                int index = Utility.Random(src.Count);
                dst.Add(src[index]);
                src.RemoveAt(index);
            }

            return dst;
        }

        private static List<int> GetLitCandlePool()
        {
            List<int> list = new List<int>();
            for (int itemId = 0x142C; itemId <= 0x1436; itemId++)
            {
                if (itemId == 0x1433 || itemId == 0x142F)
                    continue;

                list.Add(itemId);
            }

            return list;
        }

        private static void DeleteSpawnedItems(List<int> serials)
        {
            if (serials == null)
                return;

            for (int i = 0; i < serials.Count; i++)
            {
                Item item;
                if (World.Items.TryGetValue(serials[i], out item) && item != null && !item.Deleted)
                    item.Delete();
            }

            serials.Clear();
        }

        private static void MaintainFuneralDarkness(TemploState state)
        {
            if (state == null || state.ActiveEvent == null)
                return;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(state.ConstructionKey);
            if (info == null || info.Lot == null)
                return;

            Dictionary<int, int> restore = state.ActiveEvent.FuneralLightRestore;
            HashSet<int> currentInside = new HashSet<int>();

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.Map != info.Lot.Map || pm.NetState == null)
                    continue;

                if (!info.Lot.Contains(pm.Location))
                    continue;

                currentInside.Add(pm.Serial.Value);

                int oldLevel;
                if (!restore.TryGetValue(pm.Serial.Value, out oldLevel))
                    restore[pm.Serial.Value] = 1;

                OSULightOverrideSystem.SetOverride(pm, 26, 0);
            }

            List<int> restoreKeys = new List<int>(restore.Keys);
            for (int i = 0; i < restoreKeys.Count; i++)
            {
                if (currentInside.Contains(restoreKeys[i]))
                    continue;

                Mobile mob;
                if (World.Mobiles.TryGetValue(restoreKeys[i], out mob))
                {
                    PlayerMobile pm = mob as PlayerMobile;
                    if (pm != null && !pm.Deleted)
                    {
                        OSULightOverrideSystem.ClearOverride(pm);
                    }
                }

                restore.Remove(restoreKeys[i]);
            }
        }

        private static void ReleasePlayersNotInFuneral(TemploState state)
        {
            if (state == null || state.ActiveEvent == null)
                return;

            if (state.ActiveEvent.FuneralLightRestore.Count == 0)
                return;

            RestoreFuneralLights(state.ActiveEvent);
        }

        private static void RestoreFuneralLights(TemploActiveEvent ev)
        {
            if (ev == null || ev.FuneralLightRestore == null)
                return;

            List<int> keys = new List<int>(ev.FuneralLightRestore.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Mobile mob;
                if (World.Mobiles.TryGetValue(keys[i], out mob))
                {
                    PlayerMobile pm = mob as PlayerMobile;
                    if (pm != null && !pm.Deleted)
                    {
                        OSULightOverrideSystem.ClearOverride(pm);
                    }
                }
            }

            ev.FuneralLightRestore.Clear();
        }

        private static void BroadcastTempleBells(string constructionKey, int cityId)
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                if (!IsPlayerInsideCityArea(pm, cityId))
                    continue;

                pm.SendSound(0x688);
                pm.SendMessage("Ouve-se os sinos do templo.");
            }
        }

        private static bool IsPlayerInsideCityArea(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            List<ReinoLotDefinition> lots = ReinoExpansionSystem.GetAllLotsForCity(cityId);
            for (int i = 0; i < lots.Count; i++)
            {
                if (lots[i] != null && lots[i].Map == pm.Map && lots[i].Contains(pm.Location))
                    return true;
            }

            List<ReinoAreaDefinition> areas = ReinoExpansionSystem.GetAreasForCity(cityId, ReinoAreaType.Kingdom);
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] != null && areas[i].Map == pm.Map && areas[i].Contains(pm.Location))
                    return true;
            }

            return false;
        }

        private static void BroadcastStopMusicToLot(string constructionKey)
        {
            foreach (PlayerMobile pm in GetPlayersInLot(constructionKey))
            {
                if (pm != null && pm.NetState != null)
                    pm.Send(TemploStopMusic.Instance);
            }
        }

        private static List<PlayerMobile> GetPlayersInLot(string constructionKey)
        {
            List<PlayerMobile> list = new List<PlayerMobile>();
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(constructionKey);

            if (info == null || info.Lot == null)
                return list;

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.Map != info.Lot.Map || pm.NetState == null)
                    continue;

                if (info.Lot.Contains(pm.Location))
                    list.Add(pm);
            }

            return list;
        }

        private static OSUReligionDefinition GetFirstSelectableReligion()
        {
            List<OSUReligionDefinition> all = GetSelectableReligions();
            return all.Count > 0 ? all[0] : OSUReligionRegistry.GetById("none");
        }

        private static List<OSUReligionDefinition> GetSelectableReligions()
        {
            List<OSUReligionDefinition> all = OSUReligionRegistry.GetAll();
            List<OSUReligionDefinition> list = new List<OSUReligionDefinition>();

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] != null)
                    list.Add(all[i]);
            }

            return list;
        }

        private static bool IsExcludedReligion(OSUReligionDefinition def)
        {
            return def == null;
        }

        private static BauDoacoesTemplo FindChest(int serial)
        {
            Item item;
            if (serial != 0 && World.Items.TryGetValue(serial, out item))
                return item as BauDoacoesTemplo;

            return null;
        }

        public static void RefreshTempleStatue(string constructionKey)
        {
            TemploState state = GetState(constructionKey, -1, false);
            if (state == null)
                return;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(constructionKey);
            if (info == null || info.Lot == null || info.Lot.Map == null)
                return;

            int statueId = 0;
            OSUReligionDefinition religion = GetSelectedReligion(constructionKey, info.CityId);
            if (religion != null)
                statueId = religion.TempleStatueItemId;

            Point3D offset = TemploAuroraDefinition.GetStatueOffset();
            Item oldStatue = null;

            if (state.StatueSerial > 0)
                World.Items.TryGetValue(state.StatueSerial, out oldStatue);

            if (statueId <= 0)
            {
                if (oldStatue != null && !oldStatue.Deleted)
                    oldStatue.Delete();

                state.StatueSerial = 0;
                return;
            }

            Point3D worldLoc = new Point3D(info.Lot.NorthWest.X + offset.X, info.Lot.NorthWest.Y + offset.Y, info.Lot.NorthWest.Z + offset.Z);

            Server.Custom.Reinos.ReinoTemploStatua statue = oldStatue as Server.Custom.Reinos.ReinoTemploStatua;
            if (statue == null || statue.Deleted)
            {
                if (oldStatue != null && !oldStatue.Deleted)
                    oldStatue.Delete();

                statue = new Server.Custom.Reinos.ReinoTemploStatua(info.CityId, constructionKey, statueId);
                statue.MoveToWorld(worldLoc, info.Lot.Map);
                state.StatueSerial = statue.Serial.Value;
            }
            else
            {
                statue.ItemID = statueId;
                statue.CityId = info.CityId;
                statue.ConstructionKey = constructionKey;
                statue.MoveToWorld(worldLoc, info.Lot.Map);
                state.StatueSerial = statue.Serial.Value;
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
                    bw.Write(1);
                    bw.Write(m_States.Count);

                    foreach (KeyValuePair<string, TemploState> kv in m_States)
                    {
                        TemploState st = kv.Value;
                        bw.Write(kv.Key ?? String.Empty);
                        bw.Write(st.CityId);
                        bw.Write(st.SelectedReligionId ?? String.Empty);
                        bw.Write((int)st.SelectedRiteMusic);
                        bw.Write(st.DoorsClosedToPublic);
                        bw.Write(st.WeeklyStampUtc.ToBinary());
                        bw.Write(st.WeeklyDonations != null ? st.WeeklyDonations.Gold : 0);
                        bw.Write(st.WeeklyDonations != null ? st.WeeklyDonations.Cloth : 0);
                        bw.Write(st.WeeklyDonations != null ? st.WeeklyDonations.Iron : 0);
                        bw.Write(st.WeeklyDonations != null ? st.WeeklyDonations.Wood : 0);
                        bw.Write(st.AltarSerial);
                        bw.Write(st.ChestSerial);
                        bw.Write(st.StatueSerial);
                        bw.Write(st.LastEventTotalCost);
                    }
                }
            }
            catch
            {
            }
        }

        public static void Load()
        {
            m_States.Clear();

            try
            {
                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    int count = br.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        TemploState st = new TemploState();
                        st.ConstructionKey = br.ReadString();
                        st.CityId = br.ReadInt32();
                        st.SelectedReligionId = br.ReadString();
                        st.SelectedRiteMusic = (TemploMusicaRito)br.ReadInt32();
                        st.DoorsClosedToPublic = br.ReadBoolean();
                        st.WeeklyStampUtc = DateTime.FromBinary(br.ReadInt64());
                        st.WeeklyDonations.Gold = br.ReadInt32();
                        st.WeeklyDonations.Cloth = br.ReadInt32();
                        st.WeeklyDonations.Iron = br.ReadInt32();
                        st.WeeklyDonations.Wood = br.ReadInt32();
                        st.AltarSerial = br.ReadInt32();
                        st.ChestSerial = br.ReadInt32();
                        st.StatueSerial = br.ReadInt32();
                        st.LastEventTotalCost = br.ReadInt32();
                        st.ActiveEvent = null;
                        m_States[st.ConstructionKey] = st;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
