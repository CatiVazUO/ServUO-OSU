
using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;
using Server.Custom.Systems.Common.Engine;
using Server.Custom.Systems.Espetaculos.Gumps;
using Server.Custom.Systems.Espetaculos.Items;

namespace Server.Custom.Systems.Espetaculos
{
    public static class EspetaculoSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_Espetaculos_v1.bin");
        private static readonly Dictionary<string, EspetaculoVenueState> m_States = new Dictionary<string, EspetaculoVenueState>(StringComparer.OrdinalIgnoreCase);
        private static int m_NextReservationId = 1;
        private static Timer m_PulseTimer;

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };

            if (m_PulseTimer != null)
                m_PulseTimer.Stop();

            m_PulseTimer = Timer.DelayCall(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0), Pulse);
        }

        public static EspetaculoVenueDefinition GetVenueDefinition(string constructionId)
        {
            if (String.IsNullOrWhiteSpace(constructionId))
                return null;

            if (String.Equals(constructionId, TeatroAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase))
                return TeatroAuroraDefinition.CreateVenue();

            if (String.Equals(constructionId, CircoAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase))
                return CircoAuroraDefinition.CreateVenue();

            return null;
        }

        public static EspetaculoVenueDefinition GetVenueDefinitionFromKey(string constructionKey)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(constructionKey);
            return info != null && info.Definition != null ? GetVenueDefinition(info.Definition.Id) : null;
        }

        public static bool TryResolveVenueAt(Point3D location, Map map, out string constructionKey, out int cityId, out EspetaculoVenueDefinition venue)
        {
            constructionKey = String.Empty;
            cityId = -1;
            venue = null;

            if (map == null || map == Map.Internal)
                return false;

            ReinoLotDefinition lot = ReinoExpansionSystem.FindLotAt(location, map);
            if (lot == null)
                return false;

            string key = ReinoMaintenanceSystem.BuildLotKey(lot.LotId);
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(key);

            if (info == null || info.Definition == null)
                return false;

            venue = GetVenueDefinition(info.Definition.Id);
            if (venue == null)
                return false;

            constructionKey = key;
            cityId = lot.CityId;
            return true;
        }

        public static EspetaculoVenueState EnsureState(string constructionKey, int cityId, EspetaculoVenueType venueType)
        {
            EspetaculoVenueState state;
            if (!m_States.TryGetValue(constructionKey ?? String.Empty, out state))
            {
                state = new EspetaculoVenueState();
                state.ConstructionKey = constructionKey ?? String.Empty;
                state.CityId = cityId;
                state.VenueType = venueType;
                m_States[state.ConstructionKey] = state;
            }

            state.CityId = cityId;
            state.VenueType = venueType;
            if (state.Reservations == null)
                state.Reservations = new List<EspetaculoReservation>();
            if (state.StageLightSerials == null)
                state.StageLightSerials = new List<int>();
            if (state.SetPieceSerials == null)
                state.SetPieceSerials = new List<int>();
            if (state.DoorSerials == null)
                state.DoorSerials = new List<int>();
            if (state.LightRestoreLevels == null)
                state.LightRestoreLevels = new Dictionary<int, int>();

            return state;
        }

        public static EspetaculoVenueState GetState(string constructionKey)
        {
            EspetaculoVenueState state;
            m_States.TryGetValue(constructionKey ?? String.Empty, out state);
            return state;
        }

        public static void RegisterControlItem(string constructionKey, int cityId, EspetaculoVenueType venueType, int serial)
        {
            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venueType);
            state.ControlItemSerial = serial;
        }

        public static void RegisterStageLight(string constructionKey, int cityId, EspetaculoVenueType venueType, int serial)
        {
            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venueType);
            if (!state.StageLightSerials.Contains(serial))
                state.StageLightSerials.Add(serial);
        }

        public static void RegisterSetPiece(string constructionKey, int cityId, EspetaculoVenueType venueType, int serial)
        {
            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venueType);
            if (!state.SetPieceSerials.Contains(serial))
                state.SetPieceSerials.Add(serial);
        }

        public static void RegisterDoor(string constructionKey, int cityId, EspetaculoVenueType venueType, int serial)
        {
            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venueType);
            if (!state.DoorSerials.Contains(serial))
                state.DoorSerials.Add(serial);
        }

        public static bool GetSetPieceOpenState(EspetaculoVenueState state, EspetaculoVenueDefinition venue, string setPieceId)
        {
            if (state == null || venue == null || String.IsNullOrWhiteSpace(setPieceId))
                return false;

            // Teatro: todas as cortinas usam o mesmo estado
            if (venue.VenueType == EspetaculoVenueType.Theater)
            {
                if (setPieceId.StartsWith("curtain_", StringComparison.OrdinalIgnoreCase))
                    return state.SetPieceState1;

                return false;
            }

            // Circo: cada jaula tem seu próprio estado
            if (String.Equals(setPieceId, "cage_1", StringComparison.OrdinalIgnoreCase))
                return state.SetPieceState1;

            if (String.Equals(setPieceId, "cage_2", StringComparison.OrdinalIgnoreCase))
                return state.SetPieceState2;

            return false;
        }

        public static void OnVenueMultiDeleted(string constructionKey)
        {
            if (String.IsNullOrWhiteSpace(constructionKey))
                return;

            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null)
                return;

            RestoreAudienceLights(state);
            DeleteItems(state.StageLightSerials);
            DeleteItems(state.SetPieceSerials);
            DeleteItems(state.DoorSerials);

            if (state.ControlItemSerial > 0)
            {
                Item control = World.FindItem((Serial)state.ControlItemSerial);
                if (control != null && !control.Deleted)
                    control.Delete();
            }

            state.Reservations.Clear();
            m_States.Remove(constructionKey);
        }

        public static List<EspetaculoSlotOption> GetNextSlotOptions(string constructionKey, int cityId)
        {
            List<EspetaculoSlotOption> list = new List<EspetaculoSlotOption>();
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null || venue.Slots == null)
                return list;

            DateTime now = DateTime.Now;

            for (int i = 0; i < venue.Slots.Length; i++)
            {
                DateTime next = GetNextAvailableSlot(constructionKey, venue, i, now);
                EspetaculoSlotOption opt = new EspetaculoSlotOption();
                opt.SlotIndex = i;
                opt.StartLocal = next;
                opt.Label = FormatSlotDate(next) + " - " + venue.Slots[i].Label;
                opt.Available = next != DateTime.MinValue;
                list.Add(opt);
            }

            return list;
        }

        private static DateTime GetNextAvailableSlot(string constructionKey, EspetaculoVenueDefinition venue, int slotIndex, DateTime now)
        {
            if (venue == null || venue.Slots == null || slotIndex < 0 || slotIndex >= venue.Slots.Length)
                return DateTime.MinValue;

            EspetaculoSlotDefinition slot = venue.Slots[slotIndex];

            DateTime candidate = new DateTime(now.Year, now.Month, now.Day, slot.Hour, slot.Minute, 0);
            while (candidate.DayOfWeek != slot.Day)
                candidate = candidate.AddDays(1);

            if (candidate < now.AddMinutes(1))
                candidate = candidate.AddDays(7);

            for (int week = 0; week < 12; week++)
            {
                DateTime test = candidate.AddDays(7 * week);
                if (!HasReservationConflict(constructionKey, test, TimeSpan.FromHours(2.0)))
                    return test;
            }

            return DateTime.MinValue;
        }

        public static EspetaculoDurationDefinition[] GetDurations(string constructionKey)
        {
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            return venue != null ? venue.Durations : new EspetaculoDurationDefinition[0];
        }

        public static string GetReservationInfoHtml(string constructionKey)
        {
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            return venue != null ? venue.ReservationHtml : "<BASEFONT COLOR=#FFFFFF>Este espaço não está configurado.</BASEFONT>";
        }

        public static bool TryReserve(PlayerMobile from, string constructionKey, int cityId, int slotIndex, int durationIndex, out string message)
        {
            message = String.Empty;

            if (from == null || from.Deleted)
            {
                message = "Jogador inválido.";
                return false;
            }

            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null)
            {
                message = "Esse espaço não está configurado.";
                return false;
            }

            if (slotIndex < 0 || slotIndex >= venue.Slots.Length)
            {
                message = "Selecione um horário.";
                return false;
            }

            if (durationIndex < 0 || durationIndex >= venue.Durations.Length)
            {
                message = "Selecione a duração.";
                return false;
            }

            if (GetOwnedUpcomingReservation(from, constructionKey) != null)
            {
                message = "Você já possui uma reserva futura neste espaço.";
                return false;
            }

            DateTime start = GetNextAvailableSlot(constructionKey, venue, slotIndex, DateTime.Now);
            if (start == DateTime.MinValue)
            {
                message = "Não foi encontrado um horário livre para essa opção.";
                return false;
            }

            EspetaculoDurationDefinition duration = venue.Durations[durationIndex];
            if (HasReservationConflict(constructionKey, start, duration.Duration))
            {
                message = "Esse horário já foi reservado por outra pessoa.";
                return false;
            }

            if (from.Backpack == null || !from.Backpack.ConsumeTotal(typeof(Gold), duration.GoldCost))
            {
                message = "Você precisa ter " + duration.GoldCost + " moedas na mochila.";
                return false;
            }

            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, duration.GoldCost);

            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venue.VenueType);
            EspetaculoReservation res = new EspetaculoReservation();
            res.ReservationId = m_NextReservationId++;
            res.ConstructionKey = constructionKey;
            res.CityId = cityId;
            res.VenueType = venue.VenueType;
            res.RenterSerial = from.Serial.Value;
            res.RenterName = from.Name;
            res.StartLocal = start;
            res.Duration = duration.Duration;
            res.RentalCostGold = duration.GoldCost;

            state.Reservations.Add(res);
            state.Reservations.Sort(delegate (EspetaculoReservation a, EspetaculoReservation b)
            {
                return a.StartLocal.CompareTo(b.StartLocal);
            });

            message = "Reserva confirmada para " + FormatLongDate(start) + " por " + duration.Label + ".";
            return true;
        }

        private static bool HasReservationConflict(string constructionKey, DateTime start, TimeSpan duration)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null || state.Reservations == null)
                return false;

            DateTime end = start + duration;

            for (int i = 0; i < state.Reservations.Count; i++)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res == null || res.Ended)
                    continue;

                if (start < res.EndLocal && end > res.StartLocal)
                    return true;
            }

            return false;
        }

        public static EspetaculoReservation GetActiveReservation(string constructionKey)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null || state.Reservations == null)
                return null;

            DateTime now = DateTime.Now;

            for (int i = 0; i < state.Reservations.Count; i++)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res != null && res.IsActive(now))
                    return res;
            }

            return null;
        }

        public static EspetaculoReservation GetUpcomingReservation(string constructionKey)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null || state.Reservations == null)
                return null;

            DateTime now = DateTime.Now;

            for (int i = 0; i < state.Reservations.Count; i++)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res != null && !res.Ended && res.StartLocal >= now)
                    return res;
            }

            return null;
        }

        public static EspetaculoReservation GetOwnedUpcomingReservation(PlayerMobile pm, string constructionKey)
        {
            if (pm == null)
                return null;

            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null || state.Reservations == null)
                return null;

            DateTime now = DateTime.Now;

            for (int i = 0; i < state.Reservations.Count; i++)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res == null || res.Ended)
                    continue;

                if (res.RenterSerial == pm.Serial.Value && res.EndLocal >= now)
                    return res;
            }

            return null;
        }

        public static bool CanAccessControl(PlayerMobile pm, int cityId, string constructionKey)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role != null && role.IsOccupied && !String.IsNullOrWhiteSpace(role.LinkedConstructionKey) &&
                String.Equals(role.LinkedConstructionKey, constructionKey, StringComparison.OrdinalIgnoreCase))
                return true;

            EspetaculoReservation owned = GetOwnedUpcomingReservation(pm, constructionKey);
            return owned != null;
        }

        public static bool IsAudienceDimmed(string constructionKey)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            return state != null && state.AudienceLightsDimmed;
        }

        public static bool AreStageLightsOn(string constructionKey)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            return state != null && state.StageLightsOn;
        }

        public static EspetaculoLightColor GetSelectedLightColor(string constructionKey)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            return state != null ? state.SelectedLightColor : EspetaculoLightColor.Blue;
        }

        public static bool IsSetPieceOpen(string constructionKey, int index)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null)
                return false;

            return index == 0 ? state.SetPieceState1 : state.SetPieceState2;
        }

        public static bool SetAudienceLights(PlayerMobile actor, string constructionKey, int cityId, bool dim, out string message)
        {
            message = String.Empty;

            if (!CanAccessControl(actor, cityId, constructionKey))
            {
                message = "Somente o responsável pelo espetáculo, o líder do reino ou o cargo ligado a esta construção pode usar isso.";
                return false;
            }

            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null)
            {
                message = "Espaço inválido.";
                return false;
            }

            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venue.VenueType);
            state.AudienceLightsDimmed = dim;

            if (!dim)
                RestoreAudienceLights(state);
            else
                ApplyAudienceLighting(state, true);

            message = dim ? "As luzes da plateia foram apagadas." : "As luzes da plateia voltaram ao normal.";
            return true;
        }

        public static bool SetStageLights(PlayerMobile actor, string constructionKey, int cityId, bool on, out string message)
        {
            message = String.Empty;

            if (!CanAccessControl(actor, cityId, constructionKey))
            {
                message = "Você não tem acesso a esse controle.";
                return false;
            }

            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null)
            {
                message = "Espaço inválido.";
                return false;
            }

            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venue.VenueType);
            state.StageLightsOn = on;
            SyncStageLights(state);
            message = on ? "As luzes do palco foram acesas." : "As luzes do palco foram apagadas.";
            return true;
        }

        public static bool SetLightColor(PlayerMobile actor, string constructionKey, int cityId, EspetaculoLightColor color, out string message)
        {
            message = String.Empty;

            if (!CanAccessControl(actor, cityId, constructionKey))
            {
                message = "Você não tem acesso a esse controle.";
                return false;
            }

            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null)
            {
                message = "Espaço inválido.";
                return false;
            }

            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venue.VenueType);
            state.SelectedLightColor = color;
            SyncStageLights(state);
            message = "A cor selecionada foi " + GetColorLabel(color) + ".";
            return true;
        }

        public static bool SetTheaterCurtains(PlayerMobile actor, string constructionKey, int cityId, bool open, out string message)
        {
            message = String.Empty;
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);

            if (venue == null || venue.VenueType != EspetaculoVenueType.Theater)
            {
                message = "Esse controle só existe para o teatro.";
                return false;
            }

            if (!CanAccessControl(actor, cityId, constructionKey))
            {
                message = "Você não tem acesso a esse controle.";
                return false;
            }

            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venue.VenueType);
            state.SetPieceState1 = open;
            state.SetPieceState2 = open;
            SyncSetPieces(state, venue);
            message = open ? "As cortinas foram abertas." : "As cortinas foram fechadas.";
            return true;
        }

        public static bool ToggleCircusCage(PlayerMobile actor, string constructionKey, int cityId, int cageIndex, out string message)
        {
            message = String.Empty;
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);

            if (venue == null || venue.VenueType != EspetaculoVenueType.Circus)
            {
                message = "Esse controle só existe para o circo.";
                return false;
            }

            if (!CanAccessControl(actor, cityId, constructionKey))
            {
                message = "Você não tem acesso a esse controle.";
                return false;
            }

            EspetaculoVenueState state = EnsureState(constructionKey, cityId, venue.VenueType);

            if (cageIndex == 0)
            {
                state.SetPieceState1 = !state.SetPieceState1;
                message = state.SetPieceState1 ? "A jaula 1 foi aberta." : "A jaula 1 foi fechada.";
            }
            else
            {
                state.SetPieceState2 = !state.SetPieceState2;
                message = state.SetPieceState2 ? "A jaula 2 foi aberta." : "A jaula 2 foi fechada.";
            }

            SyncSetPieces(state, venue);
            return true;
        }

        public static bool TryBuyTicket(PlayerMobile from, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null)
            {
                message = "Esse espaço não está configurado.";
                return false;
            }

            EspetaculoReservation sellable = GetSellableReservation(constructionKey, venue.TicketSellLeadMinutes);

            if (sellable == null)
            {
                message = "Não há espetáculo em bilheteria neste momento.";
                return false;
            }

            if (from.Backpack == null || !from.Backpack.ConsumeTotal(typeof(Gold), venue.TicketPriceGold))
            {
                message = "Você precisa ter " + venue.TicketPriceGold + " moedas na mochila.";
                return false;
            }

            EspetaculoTicket ticket = new EspetaculoTicket(cityId, constructionKey, sellable.ReservationId, venue.DisplayName, sellable.RenterName, sellable.StartLocal);
            from.Backpack.DropItem(ticket);
            CreditRenter(sellable, venue.TicketPriceGold);
            message = "Você comprou um ingresso para o " + venue.DisplayName.ToLower() + ".";
            return true;
        }

        public static bool TryUseTicketGate(PlayerMobile from, string constructionKey, int cityId, out string message)
        {
            message = String.Empty;
            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(constructionKey);
            if (venue == null)
            {
                message = "Espaço inválido.";
                return false;
            }

            EspetaculoReservation active = GetActiveReservation(constructionKey);
            if (active == null)
            {
                message = "Nenhum espetáculo está acontecendo agora.";
                return false;
            }

            if (CanAccessControl(from, cityId, constructionKey))
            {
                AdmitAndTeleport(from, active, venue);
                message = "Bem-vindo. Você entrou como responsável autorizado.";
                return true;
            }

            if (active.ContainsAdmitted(from.Serial.Value))
            {
                AdmitAndTeleport(from, active, venue);
                message = "Bem-vindo de volta ao espetáculo.";
                return true;
            }

            EspetaculoTicket ticket = FindValidTicket(from, constructionKey, active.ReservationId);
            if (ticket == null)
            {
                message = "Você precisa de um ingresso válido na mochila.";
                return false;
            }

            ticket.Delete();
            active.Admit(from.Serial.Value);
            AdmitAndTeleport(from, active, venue);
            message = "Bem-vindo ao espetáculo.";
            return true;
        }

        public static void AdmitAndTeleport(PlayerMobile from, EspetaculoReservation active, EspetaculoVenueDefinition venue)
        {
            if (from == null || from.Deleted || venue == null)
                return;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(active.ConstructionKey);
            if (info == null || info.Lot == null || info.Lot.Map == null)
                return;

            Point3D dest = new Point3D(
                info.Lot.NorthWest.X + venue.EntryTeleportOffset.X,
                info.Lot.NorthWest.Y + venue.EntryTeleportOffset.Y,
                info.Lot.NorthWest.Z + venue.EntryTeleportOffset.Z);

            from.MoveToWorld(dest, info.Lot.Map);
            from.SendMessage("Seja bem-vindo ao " + venue.DisplayName.ToLower() + ".");
        }

        public static bool CanUsePhysicalDoor(Mobile from, string constructionKey, int cityId)
        {
            if (from == null || from.Deleted)
                return false;

            EspetaculoReservation active = GetActiveReservation(constructionKey);
            if (active == null)
                return true;

            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return true;

            if (CanAccessControl(pm, cityId, constructionKey))
                return true;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(constructionKey);
            if (info == null || info.Lot == null)
                return true;

            if (info.Lot.Contains(pm.Location))
                return true; // permite sair

            return active.ContainsAdmitted(pm.Serial.Value);
        }

        private static EspetaculoReservation GetSellableReservation(string constructionKey, int leadMinutes)
        {
            EspetaculoVenueState state = GetState(constructionKey);
            if (state == null || state.Reservations == null)
                return null;

            DateTime now = DateTime.Now;

            for (int i = 0; i < state.Reservations.Count; i++)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res == null || res.Ended)
                    continue;

                if (res.IsActive(now))
                    return res;

                if (!res.Started && now >= res.StartLocal.AddMinutes(-leadMinutes) && now < res.StartLocal)
                    return res;
            }

            return null;
        }

        private static EspetaculoTicket FindValidTicket(PlayerMobile from, string constructionKey, int reservationId)
        {
            if (from == null || from.Backpack == null)
                return null;

            Item[] items = from.Backpack.FindItemsByType(typeof(EspetaculoTicket), true);
            for (int i = 0; i < items.Length; i++)
            {
                EspetaculoTicket ticket = items[i] as EspetaculoTicket;
                if (ticket == null || ticket.Deleted)
                    continue;

                if (String.Equals(ticket.ConstructionKey, constructionKey, StringComparison.OrdinalIgnoreCase) &&
                    ticket.ReservationId == reservationId)
                    return ticket;
            }

            return null;
        }

        private static void CreditRenter(EspetaculoReservation res, int amount)
        {
            if (res == null || amount <= 0)
                return;

            Mobile mob = World.FindMobile((Serial)res.RenterSerial);
            PlayerMobile pm = mob as PlayerMobile;
            if (pm == null || pm.BankBox == null)
                return;

            pm.BankBox.DropItem(new Gold(amount));
        }

        private static void Pulse()
        {
            DateTime now = DateTime.Now;
            List<string> keys = new List<string>(m_States.Keys);

            for (int k = 0; k < keys.Count; k++)
            {
                EspetaculoVenueState state = GetState(keys[k]);
                if (state == null)
                    continue;

                CleanupExpiredReservations(state, now);
                ProcessReservations(state, now);

                EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(state.ConstructionKey);
                if (venue != null)
                    SyncSetPieces(state, venue);

                SyncStageLights(state);

                if (state.AudienceLightsDimmed)
                    ApplyAudienceLighting(state, false);

                SyncDoorsForState(state);
            }
        }

        private static void CleanupExpiredReservations(EspetaculoVenueState state, DateTime now)
        {
            if (state == null || state.Reservations == null)
                return;

            for (int i = state.Reservations.Count - 1; i >= 0; i--)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res == null)
                {
                    state.Reservations.RemoveAt(i);
                    continue;
                }

                if (res.Ended && now > res.EndLocal.AddHours(6.0))
                    state.Reservations.RemoveAt(i);
            }
        }

        private static void ProcessReservations(EspetaculoVenueState state, DateTime now)
        {
            if (state == null || state.Reservations == null)
                return;

            EspetaculoVenueDefinition venue = GetVenueDefinitionFromKey(state.ConstructionKey);
            if (venue == null)
                return;

            for (int i = 0; i < state.Reservations.Count; i++)
            {
                EspetaculoReservation res = state.Reservations[i];
                if (res == null || res.Ended)
                    continue;

                if (!res.NoticeSent && now >= res.StartLocal.AddMinutes(-10.0) && now < res.StartLocal)
                {
                    BroadcastAnnouncement(state, venue, res);
                    res.NoticeSent = true;
                }

                if (!res.Started && now >= res.StartLocal)
                    StartReservation(state, venue, res);

                if (res.Started && !res.Ended)
                {
                    EnforceVenueAccess(state, venue, res);

                    if (now >= res.EndLocal)
                        EndReservation(state, venue, res);
                }
            }
        }

        private static void StartReservation(EspetaculoVenueState state, EspetaculoVenueDefinition venue, EspetaculoReservation res)
        {
            if (state == null || venue == null || res == null)
                return;

            res.Started = true;
            EjectUnauthorizedPlayers(state, venue, res);
            SyncDoorsForState(state);
        }

        private static void EndReservation(EspetaculoVenueState state, EspetaculoVenueDefinition venue, EspetaculoReservation res)
        {
            if (state == null || res == null)
                return;

            res.Ended = true;
            RestoreAudienceLights(state);
            SyncDoorsForState(state);
        }

        private static void EnforceVenueAccess(EspetaculoVenueState state, EspetaculoVenueDefinition venue, EspetaculoReservation res)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(state.ConstructionKey);
            if (info == null || info.Lot == null || info.Lot.Map == null)
                return;

            IPooledEnumerable eable = info.Lot.Map.GetMobilesInBounds(info.Lot.Rect);
            List<PlayerMobile> inside = new List<PlayerMobile>();

            foreach (Mobile m in eable)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.Map != info.Lot.Map)
                    continue;

                if (!info.Lot.Contains(pm.Location))
                    continue;

                inside.Add(pm);
            }

            eable.Free();

            for (int i = 0; i < inside.Count; i++)
            {
                PlayerMobile pm = inside[i];
                if (pm.AccessLevel >= AccessLevel.GameMaster)
                    continue;

                if (CanAccessControl(pm, state.CityId, state.ConstructionKey))
                    continue;

                if (res.ContainsAdmitted(pm.Serial.Value))
                    continue;

                MoveOutsideLot(info.Lot, pm);
                pm.SendMessage("O espetáculo está em andamento. A entrada só é permitida com ingresso.");
            }
        }

        private static void EjectUnauthorizedPlayers(EspetaculoVenueState state, EspetaculoVenueDefinition venue, EspetaculoReservation res)
        {
            EnforceVenueAccess(state, venue, res);
        }

        private static void MoveOutsideLot(ReinoLotDefinition lot, Mobile m)
        {
            if (lot == null || m == null || m.Deleted || lot.Map == null)
                return;

            Point3D southExit = new Point3D(lot.NorthWest.X + (lot.Side / 2), lot.NorthWest.Y + lot.Side + 1, lot.NorthWest.Z);
            Point3D westExit = new Point3D(lot.NorthWest.X - 1, lot.NorthWest.Y + (lot.Side / 2), lot.NorthWest.Z);

            Point3D dest = FindNearbyWalkablePoint(lot.Map, southExit);
            if (dest == Point3D.Zero)
                dest = FindNearbyWalkablePoint(lot.Map, westExit);

            if (dest != Point3D.Zero)
                m.MoveToWorld(dest, lot.Map);
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

        private static void BroadcastAnnouncement(EspetaculoVenueState state, EspetaculoVenueDefinition venue, EspetaculoReservation res)
        {
            string cityName = GetCityName(state.CityId);

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.NetState == null)
                    continue;

                pm.CloseGump(typeof(EspetaculoAnnouncementGump));
                pm.SendGump(new EspetaculoAnnouncementGump(pm, state.CityId, venue, res, cityName));
            }
        }

        private static void ApplyAudienceLighting(EspetaculoVenueState state, bool forceCapture)
        {
            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(state.ConstructionKey);
            if (state == null || info == null || info.Lot == null || info.Lot.Map == null)
                return;

            HashSet<int> insideNow = new HashSet<int>();

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted || pm.NetState == null || pm.Map != info.Lot.Map)
                    continue;

                if (!info.Lot.Contains(pm.Location))
                    continue;

                insideNow.Add(pm.Serial.Value);

                if (forceCapture || !state.LightRestoreLevels.ContainsKey(pm.Serial.Value))
                    state.LightRestoreLevels[pm.Serial.Value] = 1;

                // Para escurecer visualmente o lote, o que precisa ser elevado é o GLOBAL light.
                // PersonalLight, sozinho, quase não muda nada em períodos claros.
                OSULightOverrideSystem.SetOverride(pm, 26, 0);
            }

            List<int> restoreKeys = new List<int>(state.LightRestoreLevels.Keys);
            for (int i = 0; i < restoreKeys.Count; i++)
            {
                int serial = restoreKeys[i];
                if (insideNow.Contains(serial))
                    continue;

                Mobile m = World.FindMobile((Serial)serial);
                PlayerMobile pm = m as PlayerMobile;
                if (pm != null && !pm.Deleted)
                {
                    OSULightOverrideSystem.ClearOverride(pm);
                }

                state.LightRestoreLevels.Remove(serial);
            }
        }

        private static void RestoreAudienceLights(EspetaculoVenueState state)
        {
            if (state == null || state.LightRestoreLevels == null)
                return;

            List<int> keys = new List<int>(state.LightRestoreLevels.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Mobile m = World.FindMobile((Serial)keys[i]);
                PlayerMobile pm = m as PlayerMobile;
                if (pm != null && !pm.Deleted)
                {
                    OSULightOverrideSystem.ClearOverride(pm);
                }
            }

            state.LightRestoreLevels.Clear();
            state.AudienceLightsDimmed = false;
        }

        private static void SyncStageLights(EspetaculoVenueState state)
        {
            if (state == null || state.StageLightSerials == null)
                return;

            for (int i = 0; i < state.StageLightSerials.Count; i++)
            {
                EspetaculoStageLight light = World.FindItem((Serial)state.StageLightSerials[i]) as EspetaculoStageLight;
                if (light != null && !light.Deleted)
                    light.SetEnabled(state.StageLightsOn, state.SelectedLightColor);
            }
        }

        private static void SyncSetPieces(EspetaculoVenueState state, EspetaculoVenueDefinition venue)
        {
            if (state == null || venue == null || state.SetPieceSerials == null)
                return;

            for (int i = 0; i < state.SetPieceSerials.Count; i++)
            {
                EspetaculoSetPieceItem item = World.FindItem((Serial)state.SetPieceSerials[i]) as EspetaculoSetPieceItem;
                if (item == null || item.Deleted)
                    continue;

                bool open = GetSetPieceOpenState(state, venue, item.SetPieceId);
                item.SetOpen(open);
            }
        }

        private static void SyncDoorsForState(EspetaculoVenueState state)
        {
            if (state == null || state.DoorSerials == null)
                return;

            bool lockedToPublic = GetActiveReservation(state.ConstructionKey) != null;

            for (int i = 0; i < state.DoorSerials.Count; i++)
            {
                EspetaculoVenueDoor door = World.FindItem((Serial)state.DoorSerials[i]) as EspetaculoVenueDoor;
                if (door != null && !door.Deleted)
                    door.SyncLockedState(lockedToPublic);
            }
        }

        private static void DeleteItems(List<int> serials)
        {
            if (serials == null)
                return;

            for (int i = 0; i < serials.Count; i++)
            {
                Item item = World.FindItem((Serial)serials[i]);
                if (item != null && !item.Deleted)
                    item.Delete();
            }

            serials.Clear();
        }

        public static string GetCityName(int cityId)
        {
            try
            {
                if (cityId >= 0 && cityId < ReinoElectionsSystem.CityNames.Length)
                    return ReinoElectionsSystem.CityNames[cityId];
            }
            catch
            {
            }

            return "Reino";
        }

        public static string GetColorLabel(EspetaculoLightColor color)
        {
            switch (color)
            {
                case EspetaculoLightColor.Red: return "vermelho";
                case EspetaculoLightColor.Green: return "verde";
                case EspetaculoLightColor.Purple: return "roxo";
                case EspetaculoLightColor.White: return "branco";
                case EspetaculoLightColor.Yellow: return "amarelo";
                default: return "azul";
            }
        }

        public static int GetLightHue(EspetaculoLightColor color)
        {
            switch (color)
            {
                case EspetaculoLightColor.Red: return 33;
                case EspetaculoLightColor.Green: return 68;
                case EspetaculoLightColor.Purple: return 1167;
                case EspetaculoLightColor.White: return 0;
                case EspetaculoLightColor.Yellow: return 53;
                default: return 1152;
            }
        }

        public static string GetVenueLabel(EspetaculoVenueType type)
        {
            return type == EspetaculoVenueType.Circus ? "Circo" : "Teatro";
        }

        public static string FormatSlotDate(DateTime date)
        {
            return String.Format("{0:dd/MM HH:mm}", date);
        }

        public static string FormatLongDate(DateTime date)
        {
            return String.Format("{0:dd/MM/yyyy HH:mm}", date);
        }

        public static void Save()
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(fs, true);

                try
                {
                    writer.Write(0);
                    writer.Write(m_NextReservationId);
                    writer.Write(m_States.Count);

                    foreach (KeyValuePair<string, EspetaculoVenueState> kv in m_States)
                    {
                        EspetaculoVenueState state = kv.Value;
                        writer.Write(kv.Key ?? String.Empty);
                        writer.Write(state != null ? state.CityId : -1);
                        writer.Write(state != null ? (int)state.VenueType : 0);
                        writer.Write(state != null ? state.ControlItemSerial : 0);

                        WriteIntList(writer, state != null ? state.StageLightSerials : null);
                        WriteIntList(writer, state != null ? state.SetPieceSerials : null);
                        WriteIntList(writer, state != null ? state.DoorSerials : null);

                        writer.Write(state != null && state.AudienceLightsDimmed);
                        writer.Write(state != null ? (int)state.SelectedLightColor : 0);
                        writer.Write(state != null && state.StageLightsOn);
                        writer.Write(state != null && state.SetPieceState1);
                        writer.Write(state != null && state.SetPieceState2);

                        WriteLightRestore(writer, state != null ? state.LightRestoreLevels : null);

                        int resCount = state != null && state.Reservations != null ? state.Reservations.Count : 0;
                        writer.Write(resCount);

                        for (int i = 0; i < resCount; i++)
                        {
                            EspetaculoReservation res = state.Reservations[i];
                            writer.Write(res != null ? res.ReservationId : 0);
                            writer.Write(res != null ? res.ConstructionKey ?? String.Empty : String.Empty);
                            writer.Write(res != null ? res.CityId : -1);
                            writer.Write(res != null ? (int)res.VenueType : 0);
                            writer.Write(res != null ? res.RenterSerial : 0);
                            writer.Write(res != null ? res.RenterName ?? String.Empty : String.Empty);
                            writer.Write(res != null ? res.StartLocal : DateTime.MinValue);
                            writer.Write(res != null ? res.Duration : TimeSpan.Zero);
                            writer.Write(res != null ? res.RentalCostGold : 0);
                            writer.Write(res != null && res.NoticeSent);
                            writer.Write(res != null && res.Started);
                            writer.Write(res != null && res.Ended);
                            WriteIntList(writer, res != null ? res.AdmittedPlayerSerials : null);
                        }
                    }
                }
                finally
                {
                    writer.Close();
                }
            }
        }

        public static void Load()
        {
            m_States.Clear();

            if (!File.Exists(FilePath))
                return;

            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                BinaryFileReader reader = new BinaryFileReader(br);

                try
                {
                    int version = reader.ReadInt();
                    m_NextReservationId = reader.ReadInt();
                    int stateCount = reader.ReadInt();

                    for (int s = 0; s < stateCount; s++)
                    {
                        EspetaculoVenueState state = new EspetaculoVenueState();
                        state.ConstructionKey = reader.ReadString();
                        state.CityId = reader.ReadInt();
                        state.VenueType = (EspetaculoVenueType)reader.ReadInt();
                        state.ControlItemSerial = reader.ReadInt();

                        state.StageLightSerials = ReadIntList(reader);
                        state.SetPieceSerials = ReadIntList(reader);
                        state.DoorSerials = ReadIntList(reader);

                        state.AudienceLightsDimmed = reader.ReadBool();
                        state.SelectedLightColor = (EspetaculoLightColor)reader.ReadInt();
                        state.StageLightsOn = reader.ReadBool();
                        state.SetPieceState1 = reader.ReadBool();
                        state.SetPieceState2 = reader.ReadBool();
                        state.LightRestoreLevels = ReadLightRestore(reader);

                        int resCount = reader.ReadInt();
                        state.Reservations = new List<EspetaculoReservation>();

                        for (int i = 0; i < resCount; i++)
                        {
                            EspetaculoReservation res = new EspetaculoReservation();
                            res.ReservationId = reader.ReadInt();
                            res.ConstructionKey = reader.ReadString();
                            res.CityId = reader.ReadInt();
                            res.VenueType = (EspetaculoVenueType)reader.ReadInt();
                            res.RenterSerial = reader.ReadInt();
                            res.RenterName = reader.ReadString();
                            res.StartLocal = reader.ReadDateTime();
                            res.Duration = reader.ReadTimeSpan();
                            res.RentalCostGold = reader.ReadInt();
                            res.NoticeSent = reader.ReadBool();
                            res.Started = reader.ReadBool();
                            res.Ended = reader.ReadBool();
                            res.AdmittedPlayerSerials = ReadIntList(reader);
                            state.Reservations.Add(res);
                        }

                        m_States[state.ConstructionKey] = state;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            if (m_NextReservationId <= 0)
                m_NextReservationId = 1;
        }

        private static void WriteIntList(GenericWriter writer, List<int> list)
        {
            int count = list != null ? list.Count : 0;
            writer.Write(count);
            for (int i = 0; i < count; i++)
                writer.Write(list[i]);
        }

        private static List<int> ReadIntList(GenericReader reader)
        {
            int count = reader.ReadInt();
            List<int> list = new List<int>(count);

            for (int i = 0; i < count; i++)
                list.Add(reader.ReadInt());

            return list;
        }

        private static void WriteLightRestore(GenericWriter writer, Dictionary<int, int> restore)
        {
            int count = restore != null ? restore.Count : 0;
            writer.Write(count);

            if (restore == null)
                return;

            foreach (KeyValuePair<int, int> kv in restore)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
        }

        private static Dictionary<int, int> ReadLightRestore(GenericReader reader)
        {
            int count = reader.ReadInt();
            Dictionary<int, int> dict = new Dictionary<int, int>();

            for (int i = 0; i < count; i++)
                dict[reader.ReadInt()] = reader.ReadInt();

            return dict;
        }
    }
}
