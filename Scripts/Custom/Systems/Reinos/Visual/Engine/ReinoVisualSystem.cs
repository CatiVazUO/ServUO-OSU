using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class ReinoVisualCityState
    {
        public int CityId;
        public int SealGumpId;
        public int RingGumpId;
        public int BannerGumpId;

        public ReinoVisualCityState()
        {
        }

        public ReinoVisualCityState(int cityId, int sealGumpId, int ringGumpId, int bannerGumpId)
        {
            CityId = cityId;
            SealGumpId = sealGumpId;
            RingGumpId = ringGumpId;
            BannerGumpId = bannerGumpId;
        }
    }

    public class ReinoVisualSession
    {
        public int CityId;
        public int BrowseSealGumpId;
        public int BrowseRingGumpId;
        public int BrowseBannerGumpId;
        public int PendingSealGumpId;
        public int PendingRingGumpId;
        public int PendingBannerGumpId;
        public string InfoHtml;

        public ReinoVisualSession()
        {
            InfoHtml = String.Empty;
        }
    }

    public static class ReinoVisualSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoVisual.bin");
        private static readonly Dictionary<int, ReinoVisualCityState> m_States = new Dictionary<int, ReinoVisualCityState>();
        private static readonly Dictionary<int, ReinoVisualSession> m_Sessions = new Dictionary<int, ReinoVisualSession>();
        private static bool m_Loaded;
        private static bool m_SaveHooked;

        public const int SealMin = 2923;
        public const int SealMax = 2943;

        public const int RingMin = 3010;
        public const int RingMax = 3069;

        public const int BannerMin = 2535;
        public const int BannerMax = 2578;

        public const int BannerEastNoPoleMin = 0x3BBD;
        public const int BannerEastNoPoleMax = 0x3BE8;
        public const int BannerSouthNoPoleMin = 0x3BE9;
        public const int BannerSouthNoPoleMax = 0x3C14;
        public const int BannerEastPoleMin = 0x3C15;
        public const int BannerEastPoleMax = 0x3C40;
        public const int BannerSouthPoleMin = 0x3C41;
        public const int BannerSouthPoleMax = 0x3C6C;

        public const int SealCost = 200;
        public const int RingCost = 100;
        public const int BannerCost = 100;

        public const int AddBannerGoldCost = 30;
        public const int AddBannerClothCost = 20;

        private class LastPlacedBannerInfo
        {
            public Serial BannerSerial;
            public int GoldCost;
            public int ClothCost;
        }

        private static readonly Dictionary<int, LastPlacedBannerInfo> m_LastPlacedBannerByCity = new Dictionary<int, LastPlacedBannerInfo>();

        public static void Initialize()
        {
            EnsureLoaded();

            if (!m_SaveHooked)
            {
                m_SaveHooked = true;
                EventSink.WorldSave += delegate { Save(); };
            }
        }

        private static void EnsureLoaded()
        {
            if (m_Loaded)
                return;

            m_Loaded = true;
            EnsureDefaults();
            Load();
            EnsureDefaults();
        }

        private static void EnsureDefaults()
        {
            int count = ReinoElectionsSystem.CityNames != null ? ReinoElectionsSystem.CityNames.Length : 4;

            for (int cityId = 0; cityId < count; cityId++)
            {
                if (!m_States.ContainsKey(cityId))
                {
                    m_States[cityId] = new ReinoVisualCityState(
                        cityId,
                        GetDefaultSealGumpId(cityId),
                        GetDefaultRingGumpId(cityId),
                        GetDefaultBannerGumpId(cityId));
                }
                else
                {
                    ReinoVisualCityState state = m_States[cityId];
                    state.CityId = cityId;
                    state.SealGumpId = ClampSeal(state.SealGumpId, cityId);
                    state.RingGumpId = ClampRing(state.RingGumpId, cityId);
                    state.BannerGumpId = ClampBanner(state.BannerGumpId, cityId);
                }
            }
        }

        private static ReinoVisualCityState GetState(int cityId)
        {
            EnsureLoaded();

            ReinoVisualCityState state;
            if (!m_States.TryGetValue(cityId, out state))
            {
                state = new ReinoVisualCityState(cityId, GetDefaultSealGumpId(cityId), GetDefaultRingGumpId(cityId), GetDefaultBannerGumpId(cityId));
                m_States[cityId] = state;
            }

            return state;
        }

        private static int GetDefaultSealGumpId(int cityId)
        {
            int offset = cityId;
            if (offset < 0)
                offset = 0;
            if (offset > (SealMax - SealMin))
                offset = 0;

            return SealMin + offset;
        }

        private static int GetDefaultRingGumpId(int cityId)
        {
            int defaultBanner = GetDefaultBannerGumpId(cityId);
            int styleIndex = GetBannerStyleIndexFromGumpId(defaultBanner);

            if (styleIndex < 1)
                styleIndex = 1;
            if (styleIndex > (RingMax - RingMin + 1))
                styleIndex = 1;

            return RingMin + (styleIndex - 1);
        }

        private static int GetDefaultBannerGumpId(int cityId)
        {
            switch (cityId)
            {
                case 0: return 2538; // Aurora / Kamay -> Uniforme4
                case 1: return 2565; // Xetá / Matalun -> Uniforme31
                case 2: return 2535; // Lurone / Sarangs -> Uniforme1
                case 3: return 2547; // Willran / Zosteros -> Uniforme13
                default: return 2535;
            }
        }

        private static int ClampSeal(int value, int cityId)
        {
            if (value < SealMin || value > SealMax)
                return GetDefaultSealGumpId(cityId);

            return value;
        }

        private static int ClampRing(int value, int cityId)
        {
            if (value < RingMin || value > RingMax)
                return GetDefaultRingGumpId(cityId);

            return value;
        }

        private static int ClampBanner(int value, int cityId)
        {
            if (value < BannerMin || value > BannerMax)
                return GetDefaultBannerGumpId(cityId);

            return value;
        }

        public static int GetSealGumpId(int cityId)
        {
            return GetState(cityId).SealGumpId;
        }

        public static int GetRingGumpId(int cityId)
        {
            return GetState(cityId).RingGumpId;
        }

        public static int GetBannerGumpId(int cityId)
        {
            return GetState(cityId).BannerGumpId;
        }

        public static int GetSealGumpIdForPlayer(PlayerMobile pm)
        {
            int cityId = ResolvePlayerCityId(pm);
            return cityId >= 0 ? GetSealGumpId(cityId) : 2923;
        }

        private static void RememberLastPlacedBanner(int cityId, Item banner)
        {
            if (banner == null || banner.Deleted)
                return;

            m_LastPlacedBannerByCity[cityId] = new LastPlacedBannerInfo
            {
                BannerSerial = banner.Serial,
                GoldCost = AddBannerGoldCost,
                ClothCost = AddBannerClothCost
            };
        }

        public static bool TryUndoLastPlacedBanner(PlayerMobile from, int cityId, out string message)
        {
            message = null;

            LastPlacedBannerInfo info;
            if (!m_LastPlacedBannerByCity.TryGetValue(cityId, out info))
            {
                message = "Este reino ainda não tem um banner recente para desfazer.";
                return false;
            }

            Item banner = World.FindItem(info.BannerSerial);

            if (banner == null || banner.Deleted)
            {
                m_LastPlacedBannerByCity.Remove(cityId);
                message = "O último banner já não existe mais no mundo.";
                return false;
            }

            banner.Delete();

            RefundBannerPlacementCost(cityId, info.GoldCost, info.ClothCost);

            m_LastPlacedBannerByCity.Remove(cityId);

            message = "O último banner foi removido e os custos foram devolvidos ao tesouro.";
            return true;
        }

        private static void RefundBannerPlacementCost(int cityId, int gold, int cloth)
        {
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null)
                return;

            if (gold != 0)
                ledger.Add(ReinoResourceType.Gold, gold);

            if (cloth != 0)
                ledger.Add(ReinoResourceType.Cloth, cloth);
        }

        public static int ResolvePlayerCityId(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return -1;

            int cityId = ReinoEmploymentSystem.GetActingGovernmentCityId(pm);
            if (cityId >= 0)
                return cityId;

            if (ReinoExpansionSystem.TryParseCityId(pm.OSUCitizenCityId, out cityId))
                return cityId;

            return -1;
        }

        public static ReinoVisualSession GetSession(PlayerMobile pm, int cityId)
        {
            EnsureLoaded();

            if (pm == null || pm.Deleted)
                return null;

            ReinoVisualSession session;
            if (!m_Sessions.TryGetValue(pm.Serial.Value, out session) || session == null || session.CityId != cityId)
            {
                ReinoVisualCityState state = GetState(cityId);

                session = new ReinoVisualSession();
                session.CityId = cityId;
                session.BrowseSealGumpId = state.SealGumpId;
                session.BrowseRingGumpId = state.RingGumpId;
                session.BrowseBannerGumpId = state.BannerGumpId;
                session.PendingSealGumpId = state.SealGumpId;
                session.PendingRingGumpId = state.RingGumpId;
                session.PendingBannerGumpId = state.BannerGumpId;
                session.InfoHtml = GetDefaultInfoHtml();

                m_Sessions[pm.Serial.Value] = session;
            }

            NormalizeSession(session, cityId);
            return session;
        }

        public static void ResetSession(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return;

            ReinoVisualCityState state = GetState(cityId);

            ReinoVisualSession session = new ReinoVisualSession();
            session.CityId = cityId;
            session.BrowseSealGumpId = state.SealGumpId;
            session.BrowseRingGumpId = state.RingGumpId;
            session.BrowseBannerGumpId = state.BannerGumpId;
            session.PendingSealGumpId = state.SealGumpId;
            session.PendingRingGumpId = state.RingGumpId;
            session.PendingBannerGumpId = state.BannerGumpId;
            session.InfoHtml = GetDefaultInfoHtml();

            m_Sessions[pm.Serial.Value] = session;
        }

        private static void NormalizeSession(ReinoVisualSession session, int cityId)
        {
            if (session == null)
                return;

            session.BrowseSealGumpId = NormalizeSelection(session.BrowseSealGumpId, GetAvailableSealGumpIds(cityId), GetState(cityId).SealGumpId);
            session.BrowseRingGumpId = NormalizeSelection(session.BrowseRingGumpId, GetAvailableRingGumpIds(cityId), GetState(cityId).RingGumpId);
            session.BrowseBannerGumpId = NormalizeSelection(session.BrowseBannerGumpId, GetAvailableBannerGumpIds(cityId), GetState(cityId).BannerGumpId);

            session.PendingSealGumpId = ClampSeal(session.PendingSealGumpId, cityId);
            session.PendingRingGumpId = ClampRing(session.PendingRingGumpId, cityId);
            session.PendingBannerGumpId = ClampBanner(session.PendingBannerGumpId, cityId);

            if (String.IsNullOrWhiteSpace(session.InfoHtml))
                session.InfoHtml = GetDefaultInfoHtml();
        }

        private static int NormalizeSelection(int current, List<int> available, int fallback)
        {
            if (available == null || available.Count <= 0)
                return fallback;

            for (int i = 0; i < available.Count; i++)
            {
                if (available[i] == current)
                    return current;
            }

            for (int i = 0; i < available.Count; i++)
            {
                if (available[i] == fallback)
                    return fallback;
            }

            return available[0];
        }

        public static List<int> GetAvailableSealGumpIds(int cityId)
        {
            return BuildAvailableList(cityId, SealMin, SealMax, 0);
        }

        public static List<int> GetAvailableRingGumpIds(int cityId)
        {
            return BuildAvailableList(cityId, RingMin, RingMax, 1);
        }

        public static List<int> GetAvailableBannerGumpIds(int cityId)
        {
            return BuildAvailableList(cityId, BannerMin, BannerMax, 2);
        }

        private static List<int> BuildAvailableList(int cityId, int min, int max, int mode)
        {
            EnsureLoaded();

            List<int> list = new List<int>();

            for (int id = min; id <= max; id++)
            {
                bool used = false;

                foreach (KeyValuePair<int, ReinoVisualCityState> kv in m_States)
                {
                    if (kv.Key == cityId || kv.Value == null)
                        continue;

                    int otherValue = 0;

                    switch (mode)
                    {
                        case 0: otherValue = kv.Value.SealGumpId; break;
                        case 1: otherValue = kv.Value.RingGumpId; break;
                        case 2: otherValue = kv.Value.BannerGumpId; break;
                    }

                    if (otherValue == id)
                    {
                        used = true;
                        break;
                    }
                }

                if (!used)
                    list.Add(id);
            }

            ReinoVisualCityState state = GetState(cityId);
            int ownValue = mode == 0 ? state.SealGumpId : mode == 1 ? state.RingGumpId : state.BannerGumpId;
            if (!list.Contains(ownValue))
                list.Add(ownValue);

            list.Sort();
            return list;
        }

        public static int GetPreviousValue(List<int> list, int current)
        {
            if (list == null || list.Count <= 0)
                return current;

            int index = list.IndexOf(current);
            if (index <= 0)
                return current;

            return list[index - 1];
        }

        public static int GetNextValue(List<int> list, int current)
        {
            if (list == null || list.Count <= 0)
                return current;

            int index = list.IndexOf(current);
            if (index < 0 || index >= list.Count - 1)
                return current;

            return list[index + 1];
        }

        public static int GetSelectionCost(int currentValue, int pendingValue, int cost)
        {
            return currentValue != pendingValue ? cost : 0;
        }

        public static int GetPendingTotalCost(int cityId, ReinoVisualSession session)
        {
            if (session == null)
                return 0;

            ReinoVisualCityState state = GetState(cityId);

            return GetSelectionCost(state.SealGumpId, session.PendingSealGumpId, SealCost)
                + GetSelectionCost(state.RingGumpId, session.PendingRingGumpId, RingCost)
                + GetSelectionCost(state.BannerGumpId, session.PendingBannerGumpId, BannerCost);
        }

        public static string GetDefaultInfoHtml()
        {
            return "<BASEFONT COLOR=#000000>Escolha o visual desejado com as setas e pressione <B>OK</B> em cada seção que quiser alterar. As mudanças só entram em vigor quando o líder confirmar no botão final.</BASEFONT>";
        }

        public static string GetSealInfoHtml()
        {
            return "<BASEFONT COLOR=#000000>Os <B>selos do reino</B> mudam a marca visual dos avisos oficiais do governo, como convites de cargos, exonerações, aprovações e avisos de leis.</BASEFONT>";
        }

        public static string GetRingInfoHtml()
        {
            return "<BASEFONT COLOR=#000000>O <B>anel do reino</B> define qual modelo de anel o líder recebe ao assumir o mandato. Somente os <B>anéis de líder</B> são blessed.</BASEFONT>";
        }

        public static string GetBannerInfoHtml()
        {
            return "<BASEFONT COLOR=#000000>Mudar o <B>banner</B> altera o visual militar do reino: os uniformes dos guardas e os banners espalhados dentro dos lotes e áreas do reino passam a usar o novo modelo.</BASEFONT>";
        }

        public static bool CommitSession(PlayerMobile pm, int cityId, out string message)
        {
            message = String.Empty;

            if (pm == null || pm.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.IsCurrentGovernor(pm, cityId))
            {
                message = "Somente o líder atual do reino pode confirmar mudanças visuais.";
                return false;
            }

            ReinoVisualSession session = GetSession(pm, cityId);
            ReinoVisualCityState state = GetState(cityId);

            int newSeal = ClampSeal(session.PendingSealGumpId, cityId);
            int newRing = ClampRing(session.PendingRingGumpId, cityId);
            int newBanner = ClampBanner(session.PendingBannerGumpId, cityId);

            int sealCost = GetSelectionCost(state.SealGumpId, newSeal, SealCost);
            int ringCost = GetSelectionCost(state.RingGumpId, newRing, RingCost);
            int bannerCost = GetSelectionCost(state.BannerGumpId, newBanner, BannerCost);
            int totalCost = sealCost + ringCost + bannerCost;

            if (totalCost <= 0)
            {
                message = "Nenhuma mudança visual foi confirmada.";
                return false;
            }

            if (sealCost > 0 && !GetAvailableSealGumpIds(cityId).Contains(newSeal))
            {
                message = "Esse selo já foi escolhido por outro reino.";
                return false;
            }

            if (ringCost > 0 && !GetAvailableRingGumpIds(cityId).Contains(newRing))
            {
                message = "Esse anel já foi escolhido por outro reino.";
                return false;
            }

            if (bannerCost > 0 && !GetAvailableBannerGumpIds(cityId).Contains(newBanner))
            {
                message = "Esse banner já foi escolhido por outro reino.";
                return false;
            }

            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null || !ledger.Has(ReinoResourceType.Gold, totalCost))
            {
                message = "O tesouro do reino não possui moedas suficientes para essa mudança.";
                return false;
            }

            ledger.Add(ReinoResourceType.Gold, -totalCost);

            bool bannerChanged = state.BannerGumpId != newBanner;
            bool ringChanged = state.RingGumpId != newRing;

            state.SealGumpId = newSeal;
            state.RingGumpId = newRing;
            state.BannerGumpId = newBanner;

            if (bannerChanged)
                ApplyBannerVisual(cityId, newBanner);

            if (ringChanged)
                RefreshLeaderRing(cityId);

            Save();
            ResetSession(pm, cityId);

            message = "Visual do reino atualizado com sucesso. Custo total: " + totalCost + " moedas.";
            return true;
        }

        public static int GetBannerStyleIndexFromGumpId(int bannerGumpId)
        {
            if (bannerGumpId < BannerMin || bannerGumpId > BannerMax)
                return -1;

            return (bannerGumpId - BannerMin) + 1;
        }

        public static void ApplyBannerVisual(int cityId, int bannerGumpId)
        {
            int styleIndex = GetBannerStyleIndexFromGumpId(bannerGumpId);
            if (styleIndex <= 0)
                return;

            UpdateUniformizedMobiles(cityId);
            UpdateTerritoryBanners(cityId, styleIndex);
        }

        private static void UpdateUniformizedMobiles(int cityId)
        {
            List<OSUCityGuard> guards = new List<OSUCityGuard>();
            List<ReinoPrisionNpcBase> prisonNpcs = new List<ReinoPrisionNpcBase>();

            foreach (Mobile mobile in World.Mobiles.Values)
            {
                if (mobile == null || mobile.Deleted)
                    continue;

                OSUCityGuard guard = mobile as OSUCityGuard;
                if (guard != null && guard.CityId == cityId)
                {
                    guards.Add(guard);
                    continue;
                }

                ReinoPrisionNpcBase prisonNpc = mobile as ReinoPrisionNpcBase;
                if (prisonNpc != null && prisonNpc.CityId == cityId)
                    prisonNpcs.Add(prisonNpc);
            }

            for (int i = 0; i < guards.Count; i++)
                guards[i].ApplyUniform();

            for (int i = 0; i < prisonNpcs.Count; i++)
                prisonNpcs[i].ApplyUniform();
        }

        private static void UpdateTerritoryBanners(int cityId, int newStyleIndex)
        {
            if (newStyleIndex < 1 || newStyleIndex > 44)
                return;

            List<Item> items = new List<Item>(World.Items.Values);
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (item == null || item.Deleted || item.Map == null || item.Map == Map.Internal)
                    continue;

                if (!IsInsideCityTerritory(cityId, item.Location, item.Map))
                    continue;

                int rangeBase;
                if (!TryGetBannerRangeBase(item.ItemID, out rangeBase))
                    continue;

                item.ItemID = rangeBase + (newStyleIndex - 1);
                item.InvalidateProperties();
            }
        }

        private static bool TryGetBannerRangeBase(int itemId, out int rangeBase)
        {
            rangeBase = 0;

            if (itemId >= BannerEastNoPoleMin && itemId <= BannerEastNoPoleMax)
            {
                rangeBase = BannerEastNoPoleMin;
                return true;
            }

            if (itemId >= BannerSouthNoPoleMin && itemId <= BannerSouthNoPoleMax)
            {
                rangeBase = BannerSouthNoPoleMin;
                return true;
            }

            if (itemId >= BannerEastPoleMin && itemId <= BannerEastPoleMax)
            {
                rangeBase = BannerEastPoleMin;
                return true;
            }

            if (itemId >= BannerSouthPoleMin && itemId <= BannerSouthPoleMax)
            {
                rangeBase = BannerSouthPoleMin;
                return true;
            }

            return false;
        }

        private static bool IsInsideCityTerritory(int cityId, Point3D location, Map map)
        {
            List<ReinoLotDefinition> lots = ReinoExpansionSystem.GetAllLotsForCity(cityId);
            for (int i = 0; i < lots.Count; i++)
            {
                ReinoLotDefinition lot = lots[i];
                if (lot != null && lot.Map == map && lot.Contains(location))
                    return true;
            }

            if (IsInsideAreaList(ReinoExpansionSystem.GetAreasForCity(cityId, ReinoAreaType.Kingdom), location, map))
                return true;

            if (IsInsideAreaList(ReinoExpansionSystem.GetAreasForCity(cityId, ReinoAreaType.Decorative), location, map))
                return true;

            if (IsInsideAreaList(ReinoExpansionSystem.GetAreasForCity(cityId, ReinoAreaType.Wall), location, map))
                return true;

            return false;
        }

        private static bool IsInsideAreaList(List<ReinoAreaDefinition> list, Point3D location, Map map)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ReinoAreaDefinition area = list[i];
                if (area != null && area.Map == map && area.Contains(location))
                    return true;
            }

            return false;
        }

        public static int GetBannerItemId(int styleIndex, bool withPole, bool facingSouth)
        {
            if (styleIndex < 1 || styleIndex > 44)
                return 0;

            int baseId;

            if (withPole)
                baseId = facingSouth ? BannerSouthPoleMin : BannerEastPoleMin;
            else
                baseId = facingSouth ? BannerSouthNoPoleMin : BannerEastNoPoleMin;

            return baseId + (styleIndex - 1);
        }

        public static Item CreateBannerItemForCity(int cityId, bool withPole, bool facingSouth)
        {
            int styleIndex = GetBannerStyleIndexFromGumpId(GetBannerGumpId(cityId));
            if (styleIndex <= 0)
                return null;

            int itemId = GetBannerItemId(styleIndex, withPole, facingSouth);
            if (itemId <= 0)
                return null;

            Static banner = new Static(itemId);
            banner.Movable = false;
            banner.Name = "banner do reino";
            return banner;
        }

        public static bool TryConsumeBannerPlacementCost(int cityId, out string fail)
        {
            fail = null;

            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(cityId);
            if (ledger == null)
            {
                fail = "O tesouro do reino não está disponível.";
                return false;
            }

            if (AddBannerGoldCost > 0 && !ledger.Has(ReinoResourceType.Gold, AddBannerGoldCost))
            {
                fail = "O tesouro do reino não tem moedas suficientes.";
                return false;
            }

            if (AddBannerClothCost > 0 && !ledger.Has(ReinoResourceType.Cloth, AddBannerClothCost))
            {
                fail = "O tesouro do reino não tem tecido suficiente.";
                return false;
            }

            if (AddBannerGoldCost > 0)
                ledger.Add(ReinoResourceType.Gold, -AddBannerGoldCost);

            if (AddBannerClothCost > 0)
                ledger.Add(ReinoResourceType.Cloth, -AddBannerClothCost);

            return true;
        }

        public static bool TryPlaceBanner(PlayerMobile from, int cityId, bool withPole, bool facingSouth, int zOffset, Point3D worldLocation, Map map, out string message)
        {
            message = String.Empty;

            if (from == null || from.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            if (!ReinoAccessHelper.IsCurrentGovernor(from, cityId))
            {
                message = "Somente o líder atual do reino pode adicionar banners.";
                return false;
            }

            if (map == null || map == Map.Internal)
            {
                message = "Mapa inválido.";
                return false;
            }

            Point3D finalLocation = new Point3D(worldLocation.X, worldLocation.Y, worldLocation.Z + zOffset);

            if (!IsInsideCityTerritory(cityId, finalLocation, map))
            {
                message = "O banner precisa ser colocado dentro de lotes ou áreas do próprio reino.";
                return false;
            }

            string fail;
            if (!TryConsumeBannerPlacementCost(cityId, out fail))
            {
                message = fail;
                return false;
            }

            Item banner = CreateBannerItemForCity(cityId, withPole, facingSouth);
            if (banner == null)
            {
                message = "Não foi possível criar o banner do reino.";
                return false;
            }

            banner.MoveToWorld(finalLocation, map);
            RememberLastPlacedBanner(cityId, banner);
            message = "Banner do reino adicionado com sucesso.";
            return true;
        }

        public static Item CreateUniformForCity(int cityId)
        {
            int styleIndex = GetBannerStyleIndexFromGumpId(GetBannerGumpId(cityId));
            if (styleIndex <= 0)
                return new Tunic();

            switch (styleIndex)
            {
                case 1: return new Uniforme1();
                case 2: return new Uniforme2();
                case 3: return new Uniforme3();
                case 4: return new Uniforme4();
                case 5: return new Uniforme5();
                case 6: return new Uniforme6();
                case 7: return new Uniforme7();
                case 8: return new Uniforme8();
                case 9: return new Uniforme9();
                case 10: return new Uniforme10();
                case 11: return new Uniforme11();
                case 12: return new Uniforme12();
                case 13: return new Uniforme13();
                case 14: return new Uniforme14();
                case 15: return new Uniforme15();
                case 16: return new Uniforme16();
                case 17: return new Uniforme17();
                case 18: return new Uniforme18();
                case 19: return new Uniforme19();
                case 20: return new Uniforme20();
                case 21: return new Uniforme21();
                case 22: return new Uniforme22();
                case 23: return new Uniforme23();
                case 24: return new Uniforme24();
                case 25: return new Uniforme25();
                case 26: return new Uniforme26();
                case 27: return new Uniforme27();
                case 28: return new Uniforme28();
                case 29: return new Uniforme29();
                case 30: return new Uniforme30();
                case 31: return new Uniforme31();
                case 32: return new Uniforme32();
                case 33: return new Uniforme33();
                case 34: return new Uniforme34();
                case 35: return new Uniforme35();
                case 36: return new Uniforme36();
                case 37: return new Uniforme37();
                case 38: return new Uniforme38();
                case 39: return new Uniforme39();
                case 40: return new Uniforme40();
                case 41: return new Uniforme41();
                case 42: return new Uniforme42();
                case 43: return new Uniforme43();
                case 44: return new Uniforme44();
                default: return new Tunic();
            }
        }

        public static void AssignLeaderRing(PlayerMobile pm, int cityId)
        {
            EnsureLoaded();

            if (pm == null || pm.Deleted)
                return;

            DeleteLeaderRings(cityId);

            NobleRingBase ring = NobleRingFactory.Create(GetRingGumpId(cityId), true, cityId);
            if (ring == null)
                return;

            if (pm.Backpack != null)
                pm.Backpack.DropItem(ring);
            else if (pm.BankBox != null)
                pm.BankBox.DropItem(ring);
            else
                ring.MoveToWorld(pm.Location, pm.Map);
        }

        public static void RefreshLeaderRing(int cityId)
        {
            DeleteLeaderRings(cityId);

            ReinoCityData city;
            if (!ReinoElectionsSystem._cities.TryGetValue(cityId, out city) || city == null || city.GovernorSerial <= 0)
                return;

            PlayerMobile governor = ReinoElectionsSystem.FindPlayer(city.GovernorSerial);
            if (governor == null || governor.Deleted)
                return;

            AssignLeaderRing(governor, cityId);
        }

        public static void DeleteLeaderRings(int cityId)
        {
            List<Item> toDelete = new List<Item>();

            foreach (Item item in World.Items.Values)
            {
                NobleRingBase ring = item as NobleRingBase;
                if (ring != null && !ring.Deleted && ring.IsLeaderRing && ring.LeaderCityId == cityId)
                    toDelete.Add(ring);
            }

            for (int i = 0; i < toDelete.Count; i++)
                toDelete[i].Delete();
        }

        private static void Save()
        {
            EnsureLoaded();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(0); // version
                    bw.Write(m_States.Count);

                    foreach (KeyValuePair<int, ReinoVisualCityState> kv in m_States)
                    {
                        ReinoVisualCityState state = kv.Value;
                        bw.Write(kv.Key);
                        bw.Write(state != null ? state.SealGumpId : GetDefaultSealGumpId(kv.Key));
                        bw.Write(state != null ? state.RingGumpId : GetDefaultRingGumpId(kv.Key));
                        bw.Write(state != null ? state.BannerGumpId : GetDefaultBannerGumpId(kv.Key));
                    }
                }
            }
            catch
            {
            }
        }

        private static void Load()
        {
            if (!File.Exists(FilePath))
                return;

            try
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    int count = br.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        int cityId = br.ReadInt32();
                        int seal = br.ReadInt32();
                        int ring = br.ReadInt32();
                        int banner = br.ReadInt32();

                        m_States[cityId] = new ReinoVisualCityState(cityId, seal, ring, banner);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
