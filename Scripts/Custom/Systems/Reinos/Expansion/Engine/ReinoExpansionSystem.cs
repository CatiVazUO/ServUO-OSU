using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoExpansionSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoExpansion_v2.bin");

        private static readonly Dictionary<int, ReinoResourceLedger> m_Ledgers = new Dictionary<int, ReinoResourceLedger>();
        private static readonly Dictionary<int, ReinoLotDefinition> m_LotDefinitions = new Dictionary<int, ReinoLotDefinition>();
        private static readonly Dictionary<int, ReinoLotState> m_LotStates = new Dictionary<int, ReinoLotState>();
        private static readonly Dictionary<int, ReinoAreaDefinition> m_AreaDefinitions = new Dictionary<int, ReinoAreaDefinition>();
        private static readonly Dictionary<int, ReinoAreaState> m_AreaStates = new Dictionary<int, ReinoAreaState>();
        private static readonly Dictionary<int, List<int>> m_PreviewSerials = new Dictionary<int, List<int>>();
        private static readonly HashSet<int> m_InternalLotSignDeletes = new HashSet<int>();

        private static int m_NextLotId = 1;
        private static int m_NextAreaId = 1;

        public static void Initialize()
        {
            ReinoExpansionDefinitions.EnsureInitialized();
            EnsureDefaults();
            Load();
            EventSink.WorldSave += delegate { Save(); };
            EventSink.CreatureDeath += OnCreatureDeath;

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0), Pulse);
            Timer.DelayCall(TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(2.0), MaintenancePulse);
        }

        private static void EnsureDefaults()
        {
            for (int cityId = 0; cityId < 4; cityId++)
                GetLedger(cityId);
        }

        public static ReinoResourceLedger GetLedger(int cityId)
        {
            ReinoResourceLedger ledger;
            if (!m_Ledgers.TryGetValue(cityId, out ledger))
            {
                ledger = new ReinoResourceLedger(cityId);
                m_Ledgers[cityId] = ledger;
            }

            return ledger;
        }

        public static void AddLedgerResource(int cityId, ReinoResourceType type, int amount)
        {
            GetLedger(cityId).Add(type, amount);
        }

        private static int GetNextAvailableLotId()
        {
            int id = 1;

            while (m_LotDefinitions.ContainsKey(id))
                id++;

            return id;
        }

        public static string GetResourceLabel(ReinoResourceType type)
        {
            switch (type)
            {
                case ReinoResourceType.Wood: return "madeira";
                case ReinoResourceType.Iron: return "ferro";
                case ReinoResourceType.Cloth: return "tecido";
                case ReinoResourceType.Gold: return "moedas";
                default: return "recurso";
            }
        }

        public static string GetStatusLabel(ReinoLotStatus status)
        {
            switch (status)
            {
                case ReinoLotStatus.Locked: return "Bloqueado";
                case ReinoLotStatus.Available: return "Disponível";
                case ReinoLotStatus.UnderConstruction: return "Em construção";
                case ReinoLotStatus.Active: return "Ativo";
                case ReinoLotStatus.Abandoned: return "Abandonado";
                default: return "Desconhecido";
            }
        }

        public static bool TryParseCityId(string raw, out int cityId)
        {
            cityId = -1;

            if (String.IsNullOrWhiteSpace(raw))
                return false;

            string value = PlayerMobile.NormalizeOSUCityId(raw);

            for (int i = 0; i < 4; i++)
            {
                if (String.Equals(PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(i)), value, StringComparison.OrdinalIgnoreCase))
                {
                    cityId = i;
                    return true;
                }
            }

            int parsed;
            if (Int32.TryParse(raw, out parsed) && parsed >= 0 && parsed < 4)
            {
                cityId = parsed;
                return true;
            }

            return false;
        }

        public static bool TryParseResourceType(string raw, out ReinoResourceType type)
        {
            type = ReinoResourceType.None;

            if (String.IsNullOrWhiteSpace(raw))
                return false;

            switch (raw.Trim().ToLower())
            {
                case "wood":
                case "madeira":
                    type = ReinoResourceType.Wood;
                    return true;
                case "iron":
                case "ferro":
                    type = ReinoResourceType.Iron;
                    return true;
                case "cloth":
                case "tecido":
                    type = ReinoResourceType.Cloth;
                    return true;
                case "gold":
                case "coins":
                case "coin":
                case "moeda":
                case "moedas":
                    type = ReinoResourceType.Gold;
                    return true;
                default:
                    return false;
            }
        }

        public static List<ReinoAreaDefinition> GetAreasForCity(int cityId, ReinoAreaType type)
        {
            List<ReinoAreaDefinition> list = new List<ReinoAreaDefinition>();

            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition def = kv.Value;

                if (def == null || def.CityId != cityId || def.AreaType != type)
                    continue;

                list.Add(def);
            }

            list.Sort(delegate (ReinoAreaDefinition a, ReinoAreaDefinition b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.AreaId.CompareTo(b.AreaId);
            });

            return list;
        }

        public static ReinoAreaDefinition GetAreaDefinition(int areaId)
        {
            ReinoAreaDefinition def;
            m_AreaDefinitions.TryGetValue(areaId, out def);
            return def;
        }

        public static ReinoAreaState GetAreaState(int areaId)
        {
            if (areaId <= 0)
                return null;

            ReinoAreaState state;
            if (!m_AreaStates.TryGetValue(areaId, out state))
            {
                state = new ReinoAreaState(areaId);
                m_AreaStates[areaId] = state;
            }

            return state;
        }

        public static ReinoLotDefinition GetLotDefinition(int lotId)
        {
            ReinoLotDefinition def;
            m_LotDefinitions.TryGetValue(lotId, out def);
            return def;
        }

        public static ReinoLotState GetLotState(int lotId)
        {
            if (lotId <= 0)
                return null;

            ReinoLotState state;
            if (!m_LotStates.TryGetValue(lotId, out state))
            {
                state = new ReinoLotState(lotId);
                m_LotStates[lotId] = state;
            }

            return state;
        }

        public static ReinoLotDefinition FindLotAt(Point3D point, Map map)
        {
            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;

                if (lot == null || lot.Map != map)
                    continue;

                if (lot.Contains(point))
                    return lot;
            }

            return null;
        }

        public static bool AddKingdomArea(int cityId, Map map, Rectangle2D rect, int z, out int areaId, out string message)
        {
            return AddAreaInternal(cityId, map, rect, z, ReinoAreaType.Kingdom, 0, String.Empty, out areaId, out message);
        }

        public static bool AddDecorativeArea(int cityId, Map map, Rectangle2D rect, int z, int linkedLotId, out int areaId, out string message)
        {
            if (GetLotDefinition(linkedLotId) == null)
            {
                areaId = 0;
                message = "O lote vinculado não existe.";
                return false;
            }

            return AddAreaInternal(cityId, map, rect, z, ReinoAreaType.Decorative, linkedLotId, String.Empty, out areaId, out message);
        }

        public static bool AddWallArea(int cityId, Map map, Rectangle2D rect, int z, string name, out int areaId, out string message)
        {
            return AddAreaInternal(cityId, map, rect, z, ReinoAreaType.Wall, 0, name, out areaId, out message);
        }

        private static bool AddAreaInternal(int cityId, Map map, Rectangle2D rect, int z, ReinoAreaType type, int linkedLotId, string name, out int areaId, out string message)
        {
            areaId = 0;
            message = String.Empty;

            if (map == null || map == Map.Internal)
            {
                message = "Mapa inválido.";
                return false;
            }

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                message = "Área inválida.";
                return false;
            }

            areaId = m_NextAreaId++;

            if (String.IsNullOrWhiteSpace(name))
            {
                if (type == ReinoAreaType.Kingdom)
                    name = String.Format("Área de Reino {0}", areaId);
                else if (type == ReinoAreaType.Decorative)
                    name = String.Format("Área Decorativa {0}", areaId);
                else
                    name = String.Format("Muralha {0}", areaId);
            }

            ReinoAreaDefinition def = new ReinoAreaDefinition(areaId, cityId, map, rect, z, type, linkedLotId, name);
            m_AreaDefinitions[areaId] = def;

            if (type != ReinoAreaType.Kingdom)
                m_AreaStates[areaId] = new ReinoAreaState(areaId);

            message = String.Format("Área {0} criada para {1}.", areaId, ReinoElectionsSystem.GetCityName(cityId));
            return true;
        }

        public static bool ClearByRect(Map map, Rectangle2D rect, out string message)
        {
            message = String.Empty;

            if (map == null || map == Map.Internal)
            {
                message = "Mapa inválido.";
                return false;
            }

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                message = "Área inválida.";
                return false;
            }

            List<int> lotsToRemove = new List<int>();
            List<int> areasToRemove = new List<int>();

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                if (lot == null || lot.Map != map)
                    continue;

                if (RectsOverlap(lot.Rect, rect))
                    lotsToRemove.Add(lot.LotId);
            }

            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                if (area == null || area.Map != map)
                    continue;

                if (RectsOverlap(area.Rect, rect))
                    areasToRemove.Add(area.AreaId);
            }

            if (lotsToRemove.Count == 0 && areasToRemove.Count == 0)
            {
                message = "Nada foi encontrado nessa área.";
                return false;
            }

            int removedLots = 0;
            int removedAreas = 0;

            for (int i = 0; i < lotsToRemove.Count; i++)
            {
                if (DeleteLot(lotsToRemove[i]))
                    removedLots++;
            }

            for (int i = 0; i < areasToRemove.Count; i++)
            {
                if (DeleteArea(areasToRemove[i]))
                    removedAreas++;
            }

            message = String.Format("Remoção concluída. Lotes apagados: {0}. Áreas apagadas: {1}.", removedLots, removedAreas);
            return true;
        }

        private static bool DeleteArea(int areaId)
        {
            ReinoAreaDefinition def;
            if (!m_AreaDefinitions.TryGetValue(areaId, out def) || def == null)
                return false;

            ReinoAreaState st;
            if (m_AreaStates.TryGetValue(areaId, out st))
            {
                CleanupAreaWorldObjects(st);
                m_AreaStates.Remove(areaId);
            }

            m_AreaDefinitions.Remove(areaId);
            return true;
        }

        public static bool DeleteLot(int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null)
                return false;

            CleanupLotWorldObjects(state);
            DeleteLotSign(state);

            List<int> linkedAreas = new List<int>();
            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                if (area != null && area.AreaType == ReinoAreaType.Decorative && area.LinkedLotId == lotId)
                    linkedAreas.Add(area.AreaId);
            }

            for (int i = 0; i < linkedAreas.Count; i++)
                DeleteArea(linkedAreas[i]);

            m_LotStates.Remove(lotId);
            m_LotDefinitions.Remove(lotId);
            return true;
        }

        public static bool CreateLot(int cityId, Map map, Point3D northWest, int side, out int lotId, out string message)
        {
            lotId = 0;
            message = String.Empty;

            if (map == null || map == Map.Internal)
            {
                message = "Mapa inválido.";
                return false;
            }

            Rectangle2D rect = new Rectangle2D(northWest.X, northWest.Y, side, side);

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition other = kv.Value;
                if (other == null || other.Map != map)
                    continue;

                if (RectsOverlap(other.Rect, rect))
                {
                    message = "Já existe um lote sobrepondo essa área.";
                    return false;
                }
            }

            lotId = GetNextAvailableLotId();

            if (lotId >= m_NextLotId)
                m_NextLotId = lotId + 1;
            ReinoLotDefinition lot = new ReinoLotDefinition(lotId, cityId, map, northWest, side);
            lot.Name = String.Format("Lote {0}: {1}x{1}", lotId, side);
            lot.Objective = BuildDefaultObjective();
            m_LotDefinitions[lotId] = lot;
            m_LotStates[lotId] = new ReinoLotState(lotId);
            EnsureLotSign(lot, m_LotStates[lotId]);

            message = String.Format("Lote {0} criado para {1}.", lotId, ReinoElectionsSystem.GetCityName(cityId));
            return true;
        }

        private static ReinoObjectiveDefinition BuildDefaultObjective()
        {
            ReinoObjectiveDefinition def = new ReinoObjectiveDefinition();
            def.Type = ReinoObjectiveType.KillMob;
            def.DisplayName = "skeletons";
            def.TargetTypeNames = new string[] { "Skeleton" };
            def.RequiredAmount = 3;
            def.ResourceType = ReinoResourceType.None;
            return def;
        }

        public static List<ReinoLotDefinition> GetUnavailableLotsForCity(int cityId)
        {
            List<ReinoLotDefinition> list = new List<ReinoLotDefinition>();

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                ReinoLotState st = GetLotState(kv.Key);

                if (lot == null || st == null || lot.CityId != cityId)
                    continue;

                if (st.Status == ReinoLotStatus.Locked)
                    list.Add(lot);
            }

            list.Sort(delegate (ReinoLotDefinition a, ReinoLotDefinition b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.LotId.CompareTo(b.LotId);
            });

            return list;
        }

        public static List<ReinoLotDefinition> GetVisibleLeftLotsForCity(int cityId)
        {
            List<ReinoLotDefinition> list = new List<ReinoLotDefinition>();

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                ReinoLotState st = GetLotState(kv.Key);

                if (lot == null || st == null || lot.CityId != cityId)
                    continue;

                if (st.Status == ReinoLotStatus.Available || st.Status == ReinoLotStatus.UnderConstruction || st.Status == ReinoLotStatus.Active || st.Status == ReinoLotStatus.Abandoned)
                    list.Add(lot);
            }

            list.Sort(delegate (ReinoLotDefinition a, ReinoLotDefinition b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.LotId.CompareTo(b.LotId);
            });

            return list;
        }

        public static List<ReinoAreaDefinition> GetVisibleWallAreasForCity(int cityId)
        {
            List<ReinoAreaDefinition> list = new List<ReinoAreaDefinition>();

            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                ReinoAreaState st = GetAreaState(kv.Key);

                if (area == null || st == null || area.CityId != cityId || area.AreaType != ReinoAreaType.Wall)
                    continue;

                if (st.Status == ReinoLotStatus.Available || st.Status == ReinoLotStatus.UnderConstruction || st.Status == ReinoLotStatus.Active)
                    list.Add(area);
            }

            list.Sort(delegate (ReinoAreaDefinition a, ReinoAreaDefinition b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.AreaId.CompareTo(b.AreaId);
            });

            return list;
        }

        public static string GetLotListLabel(ReinoLotDefinition lot, ReinoLotState state)
        {
            if (lot == null)
                return "Lote inválido";

            if (state == null)
                return lot.Name;

            if (!String.IsNullOrWhiteSpace(state.ConstructionId))
            {
                ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
                string label = def != null ? def.Name : state.ConstructionId;

                if (state.Status == ReinoLotStatus.UnderConstruction)
                    label += " [em construção]";
                else if (state.Status == ReinoLotStatus.Abandoned)
                    label += " [abandonado]";

                return label;
            }

            return lot.Name;
        }

        public static string GetWallAreaLabel(ReinoAreaDefinition area, ReinoAreaState state)
        {
            if (area == null)
                return "Muralha inválida";

            string label = !String.IsNullOrWhiteSpace(area.Name) ? area.Name : ("Muralha " + area.AreaId);

            if (state != null)
            {
                if (state.Status == ReinoLotStatus.UnderConstruction)
                    label += " [em construção]";
                else if (state.Status == ReinoLotStatus.Active)
                    label += " [pronta]";
            }

            return label;
        }

        public static string BuildLotSignHtml(PlayerMobile viewer, ReinoLotDefinition lot, ReinoLotState state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            if (lot == null || state == null)
            {
                sb.Append("Lote inválido.");
                sb.Append("</BASEFONT>");
                return sb.ToString();
            }

            string cityName = ReinoElectionsSystem.GetCityName(lot.CityId);
            string citizenCity = viewer != null ? PlayerMobile.NormalizeOSUCityId(viewer.OSUCitizenCityId) : String.Empty;
            bool isCitizen = String.Equals(citizenCity, PlayerMobile.NormalizeOSUCityId(cityName), StringComparison.OrdinalIgnoreCase);

            sb.Append("<B>Reino:</B> ");
            sb.Append(cityName);
            sb.Append("<BR>");
            sb.Append("<B>Status:</B> ");
            sb.Append(GetStatusLabel(state.Status));
            sb.Append("<BR><BR>");

            if (state.Status == ReinoLotStatus.Locked)
            {
                sb.Append(ReinoExpansionDefinitions.FormatObjectiveHtml(lot, state));
                sb.Append("<BR><BR>");
                sb.Append(isCitizen ? "Você pertence a este reino. Seu esforço contará para libertar este terreno." : "Somente cidadãos deste reino podem ajudar a limpar este lote.");
            }
            else if (state.Status == ReinoLotStatus.Available)
            {
                sb.Append("O terreno já foi limpo. Agora ele aguarda uma decisão do governador.<BR><BR>");
                if (state.AvailableUntilUtc != DateTime.MinValue)
                {
                    TimeSpan remain = state.AvailableUntilUtc - DateTime.UtcNow;
                    if (remain.TotalSeconds < 0)
                        remain = TimeSpan.Zero;

                    sb.Append("<B>Tempo restante antes de resetar:</B> ");
                    sb.Append(FormatTime(remain));
                }
            }
            else if (state.Status == ReinoLotStatus.UnderConstruction)
            {
                sb.Append("As obras já começaram aqui. Pedras, madeira e andaimes tomam o terreno enquanto a construção avança.");
            }
            else if (state.Status == ReinoLotStatus.Active)
            {
                sb.Append("Uma construção do reino já funciona neste lote.");
            }
            else if (state.Status == ReinoLotStatus.Abandoned)
            {
                sb.Append("A construção deste lote foi abandonada por falta de recursos. O governador ainda pode reativá-la.");
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public static string BuildWallRequirementHtml(int cityId)
        {
            if (AreAllRegularLotsBuilt(cityId))
                return "<BASEFONT COLOR=#000000>Todos os lotes do reino já foram concluídos. A muralha pode ser iniciada.</BASEFONT>";

            return "<BASEFONT COLOR=#000000>A muralha só pode ser iniciada quando todos os lotes normais do reino já tiverem sido construídos pelo menos uma vez.</BASEFONT>";
        }

        public static bool TryConfirmLotConstruction(PlayerMobile from, int cityId, int lotId, string constructionId, out string message)
        {
            message = String.Empty;

            if (from == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(from, cityId))
            {
                message = "Somente o governador ou alguém com a chave do governador pode construir aqui.";
                return false;
            }

            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null || lot.CityId != cityId)
            {
                message = "Lote inválido para esse reino.";
                return false;
            }

            if (state.Status == ReinoLotStatus.Available)
            {
                ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(constructionId);
                if (def == null)
                {
                    message = "Selecione uma construção antes de confirmar.";
                    return false;
                }

                if (!def.SupportsLot(lot))
                {
                    message = "Essa construção não cabe nesse lote.";
                    return false;
                }

                if (!TryConsumeResources(cityId, def.BuildCosts, out message))
                    return false;

                StartLotConstruction(lot, state, def);
                message = "Construção iniciada.";
                return true;
            }

            if (state.Status == ReinoLotStatus.Abandoned)
            {
                ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
                if (def == null)
                {
                    message = "Essa construção abandonada não está configurada para reativação.";
                    return false;
                }

                StartLotReactivation(lot, state, def);
                message = "Reativação iniciada.";
                return true;
            }

            message = "Esse lote não está pronto para confirmar construção.";
            return false;
        }

        public static bool TryConfirmAreaConstruction(PlayerMobile from, int cityId, int areaId, string constructionId, out string message)
        {
            message = String.Empty;

            if (from == null)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(from, cityId))
            {
                message = "Somente o governador ou alguém com a chave do governador pode construir aqui.";
                return false;
            }

            ReinoAreaDefinition area = GetAreaDefinition(areaId);
            ReinoAreaState state = GetAreaState(areaId);

            if (area == null || state == null || area.CityId != cityId || area.AreaType != ReinoAreaType.Wall)
            {
                message = "Área inválida para essa construção.";
                return false;
            }

            if (state.Status != ReinoLotStatus.Available && state.Status != ReinoLotStatus.Abandoned)
            {
                message = "Essa área de muralha ainda não está pronta para construir.";
                return false;
            }

            ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(constructionId);
            if (def == null || !def.SupportsArea(area))
            {
                message = "Nenhuma construção de muralha válida foi selecionada.";
                return false;
            }

            if (state.Status == ReinoLotStatus.Available)
            {
                if (!TryConsumeResources(cityId, def.BuildCosts, out message))
                    return false;

                StartAreaConstruction(area, state, def);
                message = "Construção iniciada.";
                return true;
            }

            if (state.Status == ReinoLotStatus.Abandoned)
            {
                StartAreaReactivation(area, state, def);
                message = "Reativação iniciada.";
                return true;
            }

            message = "Não foi possível iniciar essa construção.";
            return false;
        }

        private static void StartLotConstruction(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def)
        {
            CleanupLotWorldObjects(state);
            DeleteLotSign(state);

            state.Status = ReinoLotStatus.UnderConstruction;
            state.ConstructionId = def.Id;
            state.CurrentStageIndex = 0;
            state.NextStageUtc = DateTime.UtcNow + def.StageDurations[0];
            state.AvailableUntilUtc = DateTime.MinValue;
            state.ReactivateReadyUtc = DateTime.MinValue;

            PlaceLotStageMulti(lot, state, def, 0);
        }

        private static void StartLotReactivation(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def)
        {
            CleanupLotWorldObjects(state);
            DeleteLotSign(state);

            state.Status = ReinoLotStatus.UnderConstruction;
            state.CurrentStageIndex = Math.Max(0, def.StageMultiIds.Length - 1);
            state.NextStageUtc = DateTime.UtcNow + def.ReactivateDuration;
            state.ReactivateReadyUtc = state.NextStageUtc;

            int stageIndex = Math.Max(0, def.StageMultiIds.Length - 1);
            PlaceLotStageMulti(lot, state, def, stageIndex);
        }

        private static void StartAreaConstruction(ReinoAreaDefinition area, ReinoAreaState state, ReinoConstructionDefinition def)
        {
            CleanupAreaWorldObjects(state);
            state.Status = ReinoLotStatus.UnderConstruction;
            state.ConstructionId = def.Id;
            state.CurrentStageIndex = 0;
            state.NextStageUtc = DateTime.UtcNow + def.StageDurations[0];
            PlaceAreaStageMulti(area, state, def, 0);
        }

        private static void StartAreaReactivation(ReinoAreaDefinition area, ReinoAreaState state, ReinoConstructionDefinition def)
        {
            CleanupAreaWorldObjects(state);
            state.Status = ReinoLotStatus.UnderConstruction;
            state.CurrentStageIndex = Math.Max(0, def.StageMultiIds.Length - 1);
            state.NextStageUtc = DateTime.UtcNow + def.ReactivateDuration;
            int stageIndex = Math.Max(0, def.StageMultiIds.Length - 1);
            PlaceAreaStageMulti(area, state, def, stageIndex);
        }

        private static void Pulse()
        {
            try
            {
                ResolveWorldReferences();
                UpdateDecorativeUnlocks();
                UpdateWallAvailability();

                foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
                {
                    ReinoLotDefinition lot = kv.Value;
                    ReinoLotState state = GetLotState(kv.Key);

                    if (lot == null || state == null)
                        continue;

                    if (state.Status == ReinoLotStatus.Available && state.AvailableUntilUtc != DateTime.MinValue && DateTime.UtcNow >= state.AvailableUntilUtc)
                    {
                        ResetLotInternal(lot, state);
                        continue;
                    }

                    if (state.Status == ReinoLotStatus.UnderConstruction && state.NextStageUtc != DateTime.MinValue && DateTime.UtcNow >= state.NextStageUtc)
                        AdvanceLotConstruction(lot, state);
                }

                foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
                {
                    ReinoAreaDefinition area = kv.Value;
                    ReinoAreaState state = GetAreaState(kv.Key);

                    if (area == null || state == null || area.AreaType != ReinoAreaType.Wall)
                        continue;

                    if (state.Status == ReinoLotStatus.UnderConstruction && state.NextStageUtc != DateTime.MinValue && DateTime.UtcNow >= state.NextStageUtc)
                        AdvanceAreaConstruction(area, state);
                }
            }
            catch
            {
            }
        }

        private static void MaintenancePulse()
        {
            try
            {
                foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
                {
                    ReinoLotDefinition lot = kv.Value;
                    ReinoLotState state = GetLotState(kv.Key);

                    if (lot == null || state == null || state.Status != ReinoLotStatus.Active)
                        continue;

                    ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
                    if (def == null || def.Permanent)
                        continue;

                    string reason;
                    if (!TryConsumeResources(lot.CityId, def.MaintenanceCosts, out reason))
                        ConvertLotToAbandoned(lot, state, def);
                }

                foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
                {
                    ReinoAreaDefinition area = kv.Value;
                    ReinoAreaState state = GetAreaState(kv.Key);

                    if (area == null || state == null || state.Status != ReinoLotStatus.Active)
                        continue;

                    ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
                    if (def == null || def.Permanent)
                        continue;

                    string reason;
                    if (!TryConsumeResources(area.CityId, def.MaintenanceCosts, out reason))
                        ConvertAreaToAbandoned(area, state, def);
                }
            }
            catch
            {
            }
        }

        private static void UpdateDecorativeUnlocks()
        {
            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                if (area == null || area.AreaType != ReinoAreaType.Decorative)
                    continue;

                ReinoAreaState state = GetAreaState(area.AreaId);
                ReinoLotDefinition lot = GetLotDefinition(area.LinkedLotId);
                ReinoLotState lotState = GetLotState(area.LinkedLotId);

                bool unlocked = lot != null && lotState != null && lotState.IsBuilt;
                state.Unlocked = unlocked;
            }
        }

        private static void UpdateWallAvailability()
        {
            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                if (area == null || area.AreaType != ReinoAreaType.Wall)
                    continue;

                ReinoAreaState state = GetAreaState(area.AreaId);
                bool canOpen = AreAllRegularLotsBuilt(area.CityId);

                if (canOpen && state.Status == ReinoLotStatus.Locked)
                {
                    state.Status = ReinoLotStatus.Available;
                    state.Unlocked = true;
                }
                else if (!canOpen && state.Status == ReinoLotStatus.Available && String.IsNullOrWhiteSpace(state.ConstructionId))
                {
                    state.Status = ReinoLotStatus.Locked;
                    state.Unlocked = false;
                }
            }
        }

        public static bool AreAllRegularLotsBuilt(int cityId)
        {
            bool foundAny = false;

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                ReinoLotState state = GetLotState(kv.Key);

                if (lot == null || state == null || lot.CityId != cityId)
                    continue;

                foundAny = true;

                if (!state.IsBuilt)
                    return false;
            }

            return foundAny;
        }

        private static void AdvanceLotConstruction(ReinoLotDefinition lot, ReinoLotState state)
        {
            ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
            if (def == null)
                return;

            int nextStage = state.CurrentStageIndex + 1;

            if (nextStage < def.StageMultiIds.Length)
            {
                state.CurrentStageIndex = nextStage;
                state.NextStageUtc = DateTime.UtcNow + def.StageDurations[nextStage];
                PlaceLotStageMulti(lot, state, def, nextStage);
                return;
            }

            CompleteLotConstruction(lot, state, def);
        }

        private static void AdvanceAreaConstruction(ReinoAreaDefinition area, ReinoAreaState state)
        {
            ReinoConstructionDefinition def = ReinoExpansionDefinitions.GetBuilding(state.ConstructionId);
            if (def == null)
                return;

            int nextStage = state.CurrentStageIndex + 1;

            if (nextStage < def.StageMultiIds.Length)
            {
                state.CurrentStageIndex = nextStage;
                state.NextStageUtc = DateTime.UtcNow + def.StageDurations[nextStage];
                PlaceAreaStageMulti(area, state, def, nextStage);
                return;
            }

            CompleteAreaConstruction(area, state, def);
        }

        private static void CompleteLotConstruction(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def)
        {
            CleanupLotWorldObjects(state);
            DeleteLotSign(state);

            state.Status = ReinoLotStatus.Active;
            state.CurrentStageIndex = def.StageMultiIds.Length;
            state.NextStageUtc = DateTime.MinValue;
            state.ReactivateReadyUtc = DateTime.MinValue;

            PlaceLotFinishedMulti(lot, state, def, false);
            SpawnNpcIfNeeded(lot, state, def);
        }

        private static void CompleteAreaConstruction(ReinoAreaDefinition area, ReinoAreaState state, ReinoConstructionDefinition def)
        {
            CleanupAreaWorldObjects(state);
            state.Status = ReinoLotStatus.Active;
            state.CurrentStageIndex = def.StageMultiIds.Length;
            state.NextStageUtc = DateTime.MinValue;

            PlaceAreaFinishedMulti(area, state, def, false);
        }

        private static void ConvertLotToAbandoned(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def)
        {
            CleanupLotWorldObjects(state);
            DeleteLotSign(state);
            state.Status = ReinoLotStatus.Abandoned;
            state.CurrentStageIndex = -1;
            state.NextStageUtc = DateTime.MinValue;
            state.ReactivateReadyUtc = DateTime.MinValue;
            PlaceLotFinishedMulti(lot, state, def, true);
        }

        private static void ConvertAreaToAbandoned(ReinoAreaDefinition area, ReinoAreaState state, ReinoConstructionDefinition def)
        {
            if (def.Permanent)
                return;

            CleanupAreaWorldObjects(state);
            state.Status = ReinoLotStatus.Abandoned;
            state.CurrentStageIndex = -1;
            state.NextStageUtc = DateTime.MinValue;
            PlaceAreaFinishedMulti(area, state, def, true);
        }

        private static void PlaceLotStageMulti(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def, int stageIndex)
        {
            EjectLotMobiles(lot);
            CleanupLotWorldObjects(state);
            DeleteLotSign(state);

            int multiId = def.StageMultiIds[stageIndex];
            Point3D anchor = GetAnchorForTopLeft(lot.NorthWest, multiId, null);
            ReinoConstructionMulti multi = new ReinoConstructionMulti(multiId, lot.LotId, def.Id, stageIndex);
            multi.Name = def.Name + " (fase " + (stageIndex + 1) + ")";
            multi.MoveToWorld(anchor, lot.Map);
            state.MultiSerial = multi.Serial.Value;
        }

        private static void PlaceLotFinishedMulti(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def, bool abandoned)
        {
            EjectLotMobiles(lot);
            CleanupLotWorldObjects(state);
            DeleteLotSign(state);

            int multiId = abandoned ? def.AbandonedMultiId : def.FinishedMultiId;
            Point3D anchor = GetAnchorForTopLeft(lot.NorthWest, multiId, null);
            ReinoConstructionMulti multi = new ReinoConstructionMulti(multiId, lot.LotId, def.Id, abandoned ? -2 : -1);
            multi.Name = abandoned ? def.Name + " abandonado" : def.Name;
            multi.MoveToWorld(anchor, lot.Map);
            state.MultiSerial = multi.Serial.Value;
        }

        private static void PlaceAreaStageMulti(ReinoAreaDefinition area, ReinoAreaState state, ReinoConstructionDefinition def, int stageIndex)
        {
            EjectAreaMobiles(area);
            CleanupAreaWorldObjects(state);

            int multiId = def.StageMultiIds[stageIndex];
            Point3D anchor = GetAnchorForTopLeft(area.GetNorthWestPoint(), multiId, null);
            ReinoConstructionMulti multi = new ReinoConstructionMulti(multiId, area.AreaId, def.Id, stageIndex);
            multi.Name = def.Name + " (fase " + (stageIndex + 1) + ")";
            multi.MoveToWorld(anchor, area.Map);
            state.MultiSerial = multi.Serial.Value;
        }

        private static void PlaceAreaFinishedMulti(ReinoAreaDefinition area, ReinoAreaState state, ReinoConstructionDefinition def, bool abandoned)
        {
            EjectAreaMobiles(area);
            CleanupAreaWorldObjects(state);

            int multiId = abandoned ? def.AbandonedMultiId : def.FinishedMultiId;
            Point3D anchor = GetAnchorForTopLeft(area.GetNorthWestPoint(), multiId, null);
            ReinoConstructionMulti multi = new ReinoConstructionMulti(multiId, area.AreaId, def.Id, abandoned ? -2 : -1);
            multi.Name = abandoned ? def.Name + " abandonada" : def.Name;
            multi.MoveToWorld(anchor, area.Map);
            state.MultiSerial = multi.Serial.Value;
        }

        private static Point3D GetAnchorForTopLeft(Point3D desiredTopLeft, int multiId, List<Point3D> skipOffsets)
        {
            MultiComponentList mcl = MultiData.GetComponents(multiId);
            if (mcl == null || mcl.List == null || mcl.List.Length == 0)
                return desiredTopLeft;

            bool found = false;
            int minX = 0;
            int minY = 0;

            for (int i = 0; i < mcl.List.Length; i++)
            {
                MultiTileEntry entry = mcl.List[i];

                if (entry.m_ItemID <= 0)
                    continue;

                if (ShouldSkipOffset(entry.m_OffsetX, entry.m_OffsetY, entry.m_OffsetZ, skipOffsets))
                    continue;

                if (!found)
                {
                    minX = entry.m_OffsetX;
                    minY = entry.m_OffsetY;
                    found = true;
                }
                else
                {
                    if (entry.m_OffsetX < minX)
                        minX = entry.m_OffsetX;

                    if (entry.m_OffsetY < minY)
                        minY = entry.m_OffsetY;
                }
            }

            if (!found)
                return desiredTopLeft;

            return new Point3D(desiredTopLeft.X - minX, desiredTopLeft.Y - minY, desiredTopLeft.Z);
        }

        private static bool ShouldSkipOffset(int x, int y, int z, List<Point3D> skipOffsets)
        {
            if (skipOffsets == null || skipOffsets.Count == 0)
                return false;

            for (int i = 0; i < skipOffsets.Count; i++)
            {
                Point3D p = skipOffsets[i];
                if (p.X == x && p.Y == y && p.Z == z)
                    return true;
            }

            return false;
        }

        private static void EjectLotMobiles(ReinoLotDefinition lot)
        {
            if (lot == null || lot.Map == null || lot.Map == Map.Internal)
                return;

            Point3D southExit = new Point3D(lot.NorthWest.X + (lot.Side / 2), lot.NorthWest.Y + lot.Side, lot.NorthWest.Z);
            Point3D westExit = new Point3D(lot.NorthWest.X - 1, lot.NorthWest.Y + (lot.Side / 2), lot.NorthWest.Z);
            Point3D center = lot.GetCenter(lot.NorthWest.Z);

            EjectMobilesFromRect(lot.Map, lot.Rect, southExit, westExit, center);
        }

        private static void EjectAreaMobiles(ReinoAreaDefinition area)
        {
            if (area == null || area.Map == null || area.Map == Map.Internal)
                return;

            Point3D southExit = new Point3D(area.Rect.Start.X + (area.Rect.Width / 2), area.Rect.Start.Y + area.Rect.Height, area.Z);
            Point3D westExit = new Point3D(area.Rect.Start.X - 1, area.Rect.Start.Y + (area.Rect.Height / 2), area.Z);
            Point3D center = area.GetCenterPoint(area.Z);

            EjectMobilesFromRect(area.Map, area.Rect, southExit, westExit, center);
        }

        private static void EjectMobilesFromRect(Map map, Rectangle2D rect, params Point3D[] preferredPoints)
        {
            if (map == null || map == Map.Internal || rect.Width <= 0 || rect.Height <= 0)
                return;

            List<Mobile> mobiles = new List<Mobile>();

            IPooledEnumerable eable = map.GetMobilesInBounds(rect);

            foreach (Mobile m in eable)
            {
                if (m == null || m.Deleted)
                    continue;

                if (m.AccessLevel >= AccessLevel.GameMaster)
                    continue;

                mobiles.Add(m);
            }

            eable.Free();

            for (int i = 0; i < mobiles.Count; i++)
            {
                Mobile m = mobiles[i];

                if (m == null || m.Deleted || m.Map != map || !rect.Contains(new Point2D(m.X, m.Y)))
                    continue;

                Point3D dest = FindBestEjectPoint(map, preferredPoints);

                if (dest != Point3D.Zero)
                    m.MoveToWorld(dest, map);
            }
        }

        private static Point3D FindBestEjectPoint(Map map, params Point3D[] preferredPoints)
        {
            if (map == null || map == Map.Internal)
                return Point3D.Zero;

            for (int i = 0; i < preferredPoints.Length; i++)
            {
                Point3D p = FindNearbyWalkablePoint(map, preferredPoints[i]);

                if (p != Point3D.Zero)
                    return p;
            }

            return Point3D.Zero;
        }

        private static Point3D FindNearbyWalkablePoint(Map map, Point3D origin)
        {
            if (map == null || map == Map.Internal)
                return Point3D.Zero;

            for (int range = 0; range <= 4; range++)
            {
                for (int x = origin.X - range; x <= origin.X + range; x++)
                {
                    for (int y = origin.Y - range; y <= origin.Y + range; y++)
                    {
                        int z = map.GetAverageZ(x, y);

                        if (map.CanSpawnMobile(x, y, z))
                            return new Point3D(x, y, z);
                    }
                }
            }

            return Point3D.Zero;
        }

        private static void SpawnNpcIfNeeded(ReinoLotDefinition lot, ReinoLotState state, ReinoConstructionDefinition def)
        {
            if (def == null || String.IsNullOrWhiteSpace(def.NpcTypeName))
                return;

            Type npcType = ScriptCompiler.FindTypeByFullName(def.NpcTypeName);
            if (npcType == null)
                npcType = ScriptCompiler.FindTypeByName(def.NpcTypeName);

            if (npcType == null || !typeof(Mobile).IsAssignableFrom(npcType))
                return;

            object obj = Activator.CreateInstance(npcType);
            Mobile mob = obj as Mobile;
            if (mob == null)
                return;

            Point3D p = new Point3D(lot.NorthWest.X + def.NpcOffset.X, lot.NorthWest.Y + def.NpcOffset.Y, lot.NorthWest.Z + def.NpcZOffset);
            mob.MoveToWorld(p, lot.Map);
            state.NpcSerial = mob.Serial.Value;
        }

        private static void EnsureLotSign(ReinoLotDefinition lot, ReinoLotState state)
        {
            if (lot == null || state == null)
                return;

            Item existing = state.SignSerial > 0 ? World.FindItem((Serial)state.SignSerial) : null;
            ReinoLotSign sign = existing as ReinoLotSign;

            if (sign == null || sign.Deleted)
            {
                sign = new ReinoLotSign(lot.LotId, lot.CityId);
                sign.MoveToWorld(lot.NorthWest, lot.Map);
                state.SignSerial = sign.Serial.Value;
            }
            else
            {
                sign.LotId = lot.LotId;
                sign.CityId = lot.CityId;
            }

            sign.Visible = !(state.Status == ReinoLotStatus.UnderConstruction || state.Status == ReinoLotStatus.Active || state.Status == ReinoLotStatus.Abandoned);
        }

        private static void DeleteLotSign(ReinoLotState state)
        {
            if (state == null || state.SignSerial <= 0)
                return;

            int serial = state.SignSerial;
            Item sign = World.FindItem((Serial)serial);
            if (sign != null && !sign.Deleted)
            {
                m_InternalLotSignDeletes.Add(serial);
                sign.Delete();
            }

            state.SignSerial = 0;
        }

        public static void OnLotSignDeleted(int signSerial, int lotId)
        {
            if (signSerial > 0 && m_InternalLotSignDeletes.Contains(signSerial))
            {
                m_InternalLotSignDeletes.Remove(signSerial);
                return;
            }

            if (lotId > 0)
                DeleteLot(lotId);
        }

        private static void ResolveWorldReferences()
        {
            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
                EnsureLotSign(kv.Value, GetLotState(kv.Key));
        }

        private static void CleanupLotWorldObjects(ReinoLotState state)
        {
            if (state == null)
                return;

            Item multi = state.MultiSerial > 0 ? World.FindItem((Serial)state.MultiSerial) : null;
            if (multi != null && !multi.Deleted)
                multi.Delete();
            state.MultiSerial = 0;

            Mobile npc = state.NpcSerial > 0 ? World.FindMobile((Serial)state.NpcSerial) : null;
            if (npc != null && !npc.Deleted)
                npc.Delete();
            state.NpcSerial = 0;

            if (state.DoorSerials != null)
            {
                for (int i = 0; i < state.DoorSerials.Count; i++)
                {
                    Item door = state.DoorSerials[i] > 0 ? World.FindItem((Serial)state.DoorSerials[i]) : null;
                    if (door != null && !door.Deleted)
                        door.Delete();
                }

                state.DoorSerials.Clear();
            }
        }

        private static void CleanupAreaWorldObjects(ReinoAreaState state)
        {
            if (state == null)
                return;

            Item multi = state.MultiSerial > 0 ? World.FindItem((Serial)state.MultiSerial) : null;
            if (multi != null && !multi.Deleted)
                multi.Delete();
            state.MultiSerial = 0;

            if (state.DoorSerials != null)
            {
                for (int i = 0; i < state.DoorSerials.Count; i++)
                {
                    Item door = state.DoorSerials[i] > 0 ? World.FindItem((Serial)state.DoorSerials[i]) : null;
                    if (door != null && !door.Deleted)
                        door.Delete();
                }

                state.DoorSerials.Clear();
            }
        }

        private static bool TryConsumeResources(int cityId, ReinoResourceCost[] costs, out string message)
        {
            message = String.Empty;
            ReinoResourceLedger ledger = GetLedger(cityId);

            if (costs != null)
            {
                for (int i = 0; i < costs.Length; i++)
                {
                    ReinoResourceCost cost = costs[i];
                    if (cost == null)
                        continue;

                    if (!ledger.Has(cost.Type, cost.Amount))
                    {
                        message = String.Format("Faltam {0} de {1} no tesouro virtual do reino.", cost.Amount, GetResourceLabel(cost.Type));
                        return false;
                    }
                }

                for (int i = 0; i < costs.Length; i++)
                {
                    ReinoResourceCost cost = costs[i];
                    if (cost == null)
                        continue;

                    ledger.Add(cost.Type, -cost.Amount);
                }
            }

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

                string citizenCity = PlayerMobile.NormalizeOSUCityId(killer.OSUCitizenCityId);
                if (String.IsNullOrWhiteSpace(citizenCity))
                    return;

                string killedTypeName = e.Creature.GetType().Name;

                ReinoLotDefinition targetLot = null;
                ReinoLotState targetState = null;

                foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
                {
                    ReinoLotDefinition lot = kv.Value;
                    ReinoLotState state = GetLotState(kv.Key);

                    if (lot == null || state == null)
                        continue;

                    if (state.Status != ReinoLotStatus.Locked)
                        continue;

                    if (!String.Equals(PlayerMobile.NormalizeOSUCityId(ReinoElectionsSystem.GetCityName(lot.CityId)), citizenCity, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (lot.Objective == null || lot.Objective.Type != ReinoObjectiveType.KillMob)
                        continue;

                    if (!MatchesAnyTypeName(killedTypeName, lot.Objective.TargetTypeNames))
                        continue;

                    if (targetLot == null || lot.LotId < targetLot.LotId)
                    {
                        targetLot = lot;
                        targetState = state;
                    }
                }

                if (targetLot != null && targetState != null)
                {
                    targetState.ObjectiveProgress++;
                    if (targetState.ObjectiveProgress > targetLot.Objective.RequiredAmount)
                        targetState.ObjectiveProgress = targetLot.Objective.RequiredAmount;

                    if (targetState.ObjectiveProgress >= targetLot.Objective.RequiredAmount)
                    {
                        targetState.Status = ReinoLotStatus.Available;
                        targetState.AvailableUntilUtc = DateTime.UtcNow + TimeSpan.FromDays(7.0);
                        killer.SendMessage("O {0} foi limpo e agora está disponível para construção.", targetLot.Name);
                    }
                    else
                    {
                        killer.SendMessage("Progresso do {0}: {1}/{2}.", targetLot.Name, targetState.ObjectiveProgress, targetLot.Objective.RequiredAmount);
                    }

                    EnsureLotSign(targetLot, targetState);
                }
            }
            catch
            {
            }
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

        public static bool SetLotProgress(int lotId, int value, out string message)
        {
            message = String.Empty;
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null)
            {
                message = "Lote inválido.";
                return false;
            }

            if (value < 0)
                value = 0;
            if (value > lot.Objective.RequiredAmount)
                value = lot.Objective.RequiredAmount;

            state.ObjectiveProgress = value;

            if (state.ObjectiveProgress >= lot.Objective.RequiredAmount && state.Status == ReinoLotStatus.Locked)
            {
                state.Status = ReinoLotStatus.Available;
                state.AvailableUntilUtc = DateTime.UtcNow + TimeSpan.FromDays(7.0);
            }
            else if (state.ObjectiveProgress < lot.Objective.RequiredAmount && state.Status == ReinoLotStatus.Available && String.IsNullOrWhiteSpace(state.ConstructionId))
            {
                state.Status = ReinoLotStatus.Locked;
            }

            EnsureLotSign(lot, state);
            message = String.Format("Progresso do lote {0} ajustado para {1}/{2}.", lotId, state.ObjectiveProgress, lot.Objective.RequiredAmount);
            return true;
        }

        public static bool ResetLot(int lotId, out string message)
        {
            message = String.Empty;
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            ReinoLotState state = GetLotState(lotId);

            if (lot == null || state == null)
            {
                message = "Lote inválido.";
                return false;
            }

            ResetLotInternal(lot, state);
            message = String.Format("Lote {0} resetado.", lotId);
            return true;
        }

        private static void ResetLotInternal(ReinoLotDefinition lot, ReinoLotState state)
        {
            CleanupLotWorldObjects(state);
            state.Status = ReinoLotStatus.Locked;
            state.ObjectiveProgress = 0;
            state.AvailableUntilUtc = DateTime.MinValue;
            state.ConstructionId = String.Empty;
            state.CurrentStageIndex = -1;
            state.NextStageUtc = DateTime.MinValue;
            state.ReactivateReadyUtc = DateTime.MinValue;
            EnsureLotSign(lot, state);
        }

        public static bool IsPointInsideKingdomArea(int cityId, Point3D point, Map map)
        {
            if (IsPointInsideAnyLot(cityId, point, map))
                return false;

            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition def = kv.Value;
                if (def == null || def.CityId != cityId || def.Map != map || def.AreaType != ReinoAreaType.Kingdom)
                    continue;

                if (def.Contains(point))
                    return true;
            }

            return false;
        }

        public static bool IsPointInsideAnyLot(int cityId, Point3D point, Map map)
        {
            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                if (lot == null || lot.CityId != cityId || lot.Map != map)
                    continue;

                if (lot.Contains(point))
                    return true;
            }

            return false;
        }

        public static bool IsPointInsideUnlockedDecorativeArea(int cityId, Point3D point, Map map)
        {
            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition def = kv.Value;
                ReinoAreaState st = GetAreaState(kv.Key);

                if (def == null || st == null || def.CityId != cityId || def.Map != map || def.AreaType != ReinoAreaType.Decorative)
                    continue;

                if (st.Unlocked && def.Contains(point))
                    return true;
            }

            return false;
        }

        public static bool IsPointInsideBuiltWallArea(int cityId, Point3D point, Map map)
        {
            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition def = kv.Value;
                ReinoAreaState st = GetAreaState(kv.Key);

                if (def == null || st == null || def.CityId != cityId || def.Map != map || def.AreaType != ReinoAreaType.Wall)
                    continue;

                if (st.IsBuilt && def.Contains(point))
                    return true;
            }

            return false;
        }

        public static bool CanPlaceGuardsAt(int cityId, Point3D point, Map map)
        {
            if (IsPointInsideKingdomArea(cityId, point, map))
                return true;

            if (IsPointInsideAnyLot(cityId, point, map))
                return true;

            if (IsPointInsideUnlockedDecorativeArea(cityId, point, map))
                return true;

            if (IsPointInsideBuiltWallArea(cityId, point, map))
                return true;

            return false;
        }

        public static bool IsInsideRentalHouse(Point3D point, Map map)
        {
            BaseHouse house = BaseHouse.FindHouseAt(point, map, 16);
            if (house == null)
                return false;

            return house is TownHouse;
        }

        public static void ClearPreview(Mobile from)
        {
            if (from == null)
                return;

            List<int> serials;
            if (!m_PreviewSerials.TryGetValue(from.Serial.Value, out serials) || serials == null)
                return;

            for (int i = 0; i < serials.Count; i++)
            {
                Item item = World.FindItem((Serial)serials[i]);
                if (item != null && !item.Deleted)
                    item.Delete();
            }

            serials.Clear();
        }

        public static void ShowKingdomAreas(Mobile from, int cityId)
        {
            ShowCityOverlay(from, cityId);
        }

        public static void ShowSingleArea(Mobile from, int areaId)
        {
            ReinoAreaDefinition area = GetAreaDefinition(areaId);
            if (area != null)
                ShowCityOverlay(from, area.CityId);
        }

        public static void ShowSingleLot(Mobile from, int lotId)
        {
            ReinoLotDefinition lot = GetLotDefinition(lotId);
            if (lot != null)
                ShowCityOverlay(from, lot.CityId);
        }

        public static void ShowLotsForCity(Mobile from, int cityId)
        {
            ShowCityOverlay(from, cityId);
        }

        public static void ShowCityOverlay(Mobile from, int cityId)
        {
            if (from == null)
                return;

            ClearPreview(from);

            Dictionary<int, Dictionary<long, ReinoPreviewKind>> cellsByMap = new Dictionary<int, Dictionary<long, ReinoPreviewKind>>();

            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                if (area == null || area.CityId != cityId)
                    continue;

                ReinoPreviewKind kind = area.AreaType == ReinoAreaType.Wall ? ReinoPreviewKind.WallArea :
                    area.AreaType == ReinoAreaType.Decorative ? ReinoPreviewKind.DecorativeArea :
                    ReinoPreviewKind.KingdomArea;

                AddRectToOverlay(cellsByMap, area.Map, area.Rect, kind);
            }

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                if (lot == null || lot.CityId != cityId)
                    continue;

                AddRectToOverlay(cellsByMap, lot.Map, lot.Rect, ReinoPreviewKind.Lot);
            }

            RenderOverlay(from, cellsByMap);
        }

        public static void ShowMapOverlay(Mobile from, Map map)
        {
            if (from == null || map == null || map == Map.Internal)
                return;

            ClearPreview(from);

            Dictionary<int, Dictionary<long, ReinoPreviewKind>> cellsByMap = new Dictionary<int, Dictionary<long, ReinoPreviewKind>>();

            foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
            {
                ReinoAreaDefinition area = kv.Value;
                if (area == null || area.Map != map)
                    continue;

                ReinoPreviewKind kind = area.AreaType == ReinoAreaType.Wall ? ReinoPreviewKind.WallArea :
                    area.AreaType == ReinoAreaType.Decorative ? ReinoPreviewKind.DecorativeArea :
                    ReinoPreviewKind.KingdomArea;

                AddRectToOverlay(cellsByMap, area.Map, area.Rect, kind);
            }

            foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
            {
                ReinoLotDefinition lot = kv.Value;
                if (lot == null || lot.Map != map)
                    continue;

                AddRectToOverlay(cellsByMap, lot.Map, lot.Rect, ReinoPreviewKind.Lot);
            }

            RenderOverlay(from, cellsByMap);
        }

        private static void AddRectToOverlay(Dictionary<int, Dictionary<long, ReinoPreviewKind>> cellsByMap, Map map, Rectangle2D rect, ReinoPreviewKind kind)
        {
            if (map == null || map == Map.Internal)
                return;

            int mapIndex = GetMapIndex(map);

            Dictionary<long, ReinoPreviewKind> cells;
            if (!cellsByMap.TryGetValue(mapIndex, out cells) || cells == null)
            {
                cells = new Dictionary<long, ReinoPreviewKind>();
                cellsByMap[mapIndex] = cells;
            }

            for (int x = rect.Start.X; x < rect.End.X; x++)
            {
                for (int y = rect.Start.Y; y < rect.End.Y; y++)
                {
                    long key = (((long)x) << 32) | (uint)y;
                    ReinoPreviewKind existing;
                    if (!cells.TryGetValue(key, out existing) || GetPreviewPriority(kind) >= GetPreviewPriority(existing))
                        cells[key] = kind;
                }
            }
        }

        private static void RenderOverlay(Mobile from, Dictionary<int, Dictionary<long, ReinoPreviewKind>> cellsByMap)
        {
            if (from == null || cellsByMap == null)
                return;

            List<int> serials;
            if (!m_PreviewSerials.TryGetValue(from.Serial.Value, out serials) || serials == null)
            {
                serials = new List<int>();
                m_PreviewSerials[from.Serial.Value] = serials;
            }

            foreach (KeyValuePair<int, Dictionary<long, ReinoPreviewKind>> mapEntry in cellsByMap)
            {
                Map map = GetMapByIndex(mapEntry.Key);
                if (map == null || map == Map.Internal)
                    continue;

                foreach (KeyValuePair<long, ReinoPreviewKind> cell in mapEntry.Value)
                {
                    int x = (int)(cell.Key >> 32);
                    int y = (int)(cell.Key & 0xFFFFFFFF);
                    CreatePreviewMarker(from, map, x, y, 0, 0x1766, GetPreviewHue(cell.Value), serials);
                }
            }
        }

        private static int GetMapIndex(Map map)
        {
            return map == null ? -1 : map.MapIndex;
        }

        private static Map GetMapByIndex(int index)
        {
            return Map.Maps != null && index >= 0 && index < Map.Maps.Length ? Map.Maps[index] : null;
        }

        private static int GetPreviewPriority(ReinoPreviewKind kind)
        {
            switch (kind)
            {
                case ReinoPreviewKind.KingdomArea: return 1;
                case ReinoPreviewKind.Lot: return 2;
                case ReinoPreviewKind.DecorativeArea: return 3;
                case ReinoPreviewKind.WallArea: return 4;
                default: return 0;
            }
        }

        private static int GetPreviewHue(ReinoPreviewKind kind)
        {
            switch (kind)
            {
                case ReinoPreviewKind.KingdomArea: return 67;   // verde
                case ReinoPreviewKind.Lot: return 53;           // amarelo
                case ReinoPreviewKind.DecorativeArea: return 3; // azul
                case ReinoPreviewKind.WallArea: return 32;      // vermelho
                default: return 0;
            }
        }

        private static void CreatePreviewMarker(Mobile owner, Map map, int x, int y, int zHint, int itemId, int hue, List<int> serials)
        {
            if (owner == null || map == null || map == Map.Internal)
                return;

            int z = map.GetAverageZ(x, y);
            if (z == 0 && zHint != 0)
                z = zHint;

            ReinoPreviewMarker marker = new ReinoPreviewMarker(owner.Serial.Value, itemId, hue);
            marker.MoveToWorld(new Point3D(x, y, z), map);
            serials.Add(marker.Serial.Value);
        }

        private static bool RectsOverlap(Rectangle2D a, Rectangle2D b)
        {
            return a.Start.X < b.End.X && a.End.X > b.Start.X && a.Start.Y < b.End.Y && a.End.Y > b.Start.Y;
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
                    bw.Write(2);
                    bw.Write(m_NextLotId);
                    bw.Write(m_NextAreaId);

                    bw.Write(m_Ledgers.Count);
                    foreach (KeyValuePair<int, ReinoResourceLedger> kv in m_Ledgers)
                    {
                        ReinoResourceLedger ledger = kv.Value;
                        bw.Write(kv.Key);
                        bw.Write(ledger.Wood);
                        bw.Write(ledger.Iron);
                        bw.Write(ledger.Cloth);
                        bw.Write(ledger.Gold);
                    }

                    bw.Write(m_AreaDefinitions.Count);
                    foreach (KeyValuePair<int, ReinoAreaDefinition> kv in m_AreaDefinitions)
                    {
                        ReinoAreaDefinition def = kv.Value;
                        ReinoAreaState st = GetAreaState(kv.Key);

                        bw.Write(def.AreaId);
                        bw.Write(def.CityId);
                        bw.Write(def.Map != null ? def.Map.MapID : 0);
                        bw.Write(def.Rect.Start.X);
                        bw.Write(def.Rect.Start.Y);
                        bw.Write(def.Rect.Width);
                        bw.Write(def.Rect.Height);
                        bw.Write(def.Z);
                        bw.Write((int)def.AreaType);
                        bw.Write(def.LinkedLotId);
                        bw.Write(def.Name ?? String.Empty);

                        bool hasState = def.AreaType != ReinoAreaType.Kingdom;
                        bw.Write(hasState);
                        if (hasState)
                        {
                            bw.Write(st.Unlocked);
                            bw.Write((int)st.Status);
                            bw.Write(st.ConstructionId ?? String.Empty);
                            bw.Write(st.CurrentStageIndex);
                            bw.Write(st.NextStageUtc.ToBinary());
                            bw.Write(st.MultiSerial);
                            int doorCount = st.DoorSerials != null ? st.DoorSerials.Count : 0;
                            bw.Write(doorCount);
                            for (int i = 0; i < doorCount; i++)
                                bw.Write(st.DoorSerials[i]);
                        }
                    }

                    bw.Write(m_LotDefinitions.Count);
                    foreach (KeyValuePair<int, ReinoLotDefinition> kv in m_LotDefinitions)
                    {
                        ReinoLotDefinition def = kv.Value;
                        ReinoLotState st = GetLotState(kv.Key);

                        bw.Write(def.LotId);
                        bw.Write(def.CityId);
                        bw.Write(def.Map != null ? def.Map.MapID : 0);
                        bw.Write(def.NorthWest.X);
                        bw.Write(def.NorthWest.Y);
                        bw.Write(def.NorthWest.Z);
                        bw.Write(def.Side);
                        bw.Write(def.Name ?? String.Empty);

                        bw.Write((int)def.Objective.Type);
                        bw.Write(def.Objective.DisplayName ?? String.Empty);
                        bw.Write(def.Objective.RequiredAmount);
                        bw.Write((int)def.Objective.ResourceType);
                        int targetCount = def.Objective.TargetTypeNames != null ? def.Objective.TargetTypeNames.Length : 0;
                        bw.Write(targetCount);
                        for (int i = 0; i < targetCount; i++)
                            bw.Write(def.Objective.TargetTypeNames[i] ?? String.Empty);

                        bw.Write((int)st.Status);
                        bw.Write(st.ObjectiveProgress);
                        bw.Write(st.AvailableUntilUtc.ToBinary());
                        bw.Write(st.ConstructionId ?? String.Empty);
                        bw.Write(st.CurrentStageIndex);
                        bw.Write(st.NextStageUtc.ToBinary());
                        bw.Write(st.ReactivateReadyUtc.ToBinary());
                        bw.Write(st.SignSerial);
                        bw.Write(st.MultiSerial);
                        bw.Write(st.NpcSerial);
                        int doorCount = st.DoorSerials != null ? st.DoorSerials.Count : 0;
                        bw.Write(doorCount);
                        for (int i = 0; i < doorCount; i++)
                            bw.Write(st.DoorSerials[i]);
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
                m_AreaDefinitions.Clear();
                m_AreaStates.Clear();
                m_LotDefinitions.Clear();
                m_LotStates.Clear();

                if (!File.Exists(FilePath))
                    return;

                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    m_NextLotId = br.ReadInt32();
                    m_NextAreaId = version >= 2 ? br.ReadInt32() : 1;

                    m_Ledgers.Clear();
                    int ledgerCount = br.ReadInt32();
                    for (int i = 0; i < ledgerCount; i++)
                    {
                        int cityId = br.ReadInt32();
                        ReinoResourceLedger ledger = new ReinoResourceLedger(cityId);
                        ledger.Wood = br.ReadInt32();
                        ledger.Iron = br.ReadInt32();
                        ledger.Cloth = br.ReadInt32();
                        ledger.Gold = version >= 2 ? br.ReadInt32() : 0;
                        m_Ledgers[cityId] = ledger;
                    }

                    int areaCount = version >= 2 ? br.ReadInt32() : 0;
                    for (int i = 0; i < areaCount; i++)
                    {
                        int areaId = br.ReadInt32();
                        int cityId = br.ReadInt32();
                        int mapId = br.ReadInt32();
                        int x = br.ReadInt32();
                        int y = br.ReadInt32();
                        int w = br.ReadInt32();
                        int h = br.ReadInt32();
                        int z = br.ReadInt32();
                        ReinoAreaType type = (ReinoAreaType)br.ReadInt32();
                        int linkedLotId = br.ReadInt32();
                        string name = br.ReadString();

                        Map map = mapId >= 0 && mapId < Map.Maps.Length ? Map.Maps[mapId] : Map.Felucca;
                        ReinoAreaDefinition def = new ReinoAreaDefinition(areaId, cityId, map, new Rectangle2D(x, y, w, h), z, type, linkedLotId, name);
                        m_AreaDefinitions[areaId] = def;

                        bool hasState = br.ReadBoolean();
                        if (hasState)
                        {
                            ReinoAreaState st = new ReinoAreaState(areaId);
                            st.Unlocked = br.ReadBoolean();
                            st.Status = (ReinoLotStatus)br.ReadInt32();
                            st.ConstructionId = br.ReadString();
                            st.CurrentStageIndex = br.ReadInt32();
                            st.NextStageUtc = DateTime.FromBinary(br.ReadInt64());
                            st.MultiSerial = br.ReadInt32();
                            int doorCount = br.ReadInt32();
                            for (int d = 0; d < doorCount; d++)
                                st.DoorSerials.Add(br.ReadInt32());
                            m_AreaStates[areaId] = st;
                        }
                    }

                    int lotCount = br.ReadInt32();
                    for (int i = 0; i < lotCount; i++)
                    {
                        int lotId = br.ReadInt32();
                        int cityId = br.ReadInt32();
                        int mapId = br.ReadInt32();
                        int x = br.ReadInt32();
                        int y = br.ReadInt32();
                        int z = br.ReadInt32();
                        int side = br.ReadInt32();
                        string name = br.ReadString();

                        Map map = mapId >= 0 && mapId < Map.Maps.Length ? Map.Maps[mapId] : Map.Felucca;
                        ReinoLotDefinition def = new ReinoLotDefinition(lotId, cityId, map, new Point3D(x, y, z), side);
                        def.Name = name;
                        def.Objective.Type = (ReinoObjectiveType)br.ReadInt32();
                        def.Objective.DisplayName = br.ReadString();
                        def.Objective.RequiredAmount = br.ReadInt32();
                        def.Objective.ResourceType = (ReinoResourceType)br.ReadInt32();
                        int targets = br.ReadInt32();
                        def.Objective.TargetTypeNames = new string[targets];
                        for (int t = 0; t < targets; t++)
                            def.Objective.TargetTypeNames[t] = br.ReadString();

                        ReinoLotState st = new ReinoLotState(lotId);
                        st.Status = (ReinoLotStatus)br.ReadInt32();
                        st.ObjectiveProgress = br.ReadInt32();
                        st.AvailableUntilUtc = DateTime.FromBinary(br.ReadInt64());
                        st.ConstructionId = br.ReadString();
                        st.CurrentStageIndex = br.ReadInt32();
                        st.NextStageUtc = DateTime.FromBinary(br.ReadInt64());
                        st.ReactivateReadyUtc = DateTime.FromBinary(br.ReadInt64());
                        st.SignSerial = br.ReadInt32();
                        st.MultiSerial = br.ReadInt32();
                        st.NpcSerial = br.ReadInt32();
                        int doorCount = br.ReadInt32();
                        for (int d = 0; d < doorCount; d++)
                            st.DoorSerials.Add(br.ReadInt32());

                        m_LotDefinitions[lotId] = def;
                        m_LotStates[lotId] = st;
                    }
                }

                EnsureDefaults();
            }
            catch
            {
            }
        }

        public static string FormatTime(TimeSpan ts)
        {
            if (ts.TotalSeconds < 0)
                ts = TimeSpan.Zero;

            if (ts.TotalDays >= 1.0)
                return String.Format("{0}d {1}h", (int)ts.TotalDays, ts.Hours);

            if (ts.TotalHours >= 1.0)
                return String.Format("{0}h {1}m", (int)ts.TotalHours, ts.Minutes);

            return String.Format("{0}m {1}s", Math.Max(0, ts.Minutes), Math.Max(0, ts.Seconds));
        }
    }
}
