
using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Espetaculos.Gumps
{
    public class EspetaculoReservationGump : Gump
    {
        private readonly int m_CityId;
        private readonly string m_ConstructionKey;
        private readonly int m_SelectedSlot;
        private readonly int m_SelectedDuration;

        private const int ButtonConfirm = 1200;

        public EspetaculoReservationGump(PlayerMobile from, int cityId, string constructionKey)
            : this(from, cityId, constructionKey, -1, -1)
        {
        }

        public EspetaculoReservationGump(PlayerMobile from, int cityId, string constructionKey, int selectedSlot, int selectedDuration)
            : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            m_SelectedSlot = selectedSlot;
            m_SelectedDuration = selectedDuration;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            EspetaculoVenueDefinition venue = EspetaculoSystem.GetVenueDefinitionFromKey(m_ConstructionKey);
            List<EspetaculoSlotOption> slots = EspetaculoSystem.GetNextSlotOptions(m_ConstructionKey, m_CityId);
            EspetaculoDurationDefinition[] durations = EspetaculoSystem.GetDurations(m_ConstructionKey);

            AddPage(0);
            AddImageTiled(422, 206, 365, 326, 392);
            AddImageTiled(395, 209, 40, 323, 631);
            AddImageTiled(773, 197, 40, 340, 631);
            AddImageTiled(419, 177, 364, 37, 630);
            AddImageTiled(416, 515, 368, 37, 630);
            AddImage(391, 173, 1315);
            AddImage(756, 170, 1316);
            AddImage(758, 500, 1317);
            AddImage(391, 500, 1318);

            AddLabel(580, 217, 1152, venue != null ? venue.DisplayName : "Espaço");
            AddImageTiled(435, 234, 335, 13, 630);
            AddHtml(451, 262, 299, 99, EspetaculoSystem.GetReservationInfoHtml(m_ConstructionKey), false, false);

            for (int i = 0; i < 3; i++)
            {
                int y = 377 + (23 * i);
                string label = i < slots.Count && slots[i].Available ? slots[i].Label : "Sem vaga disponível";
                bool selected = m_SelectedSlot == i;

                AddButton(454, y, selected ? 536 : 437, 536, 1000 + i, GumpButtonType.Reply, 0);
                AddLabel(478, y - 3, 1152, (selected ? "[X] " : "") + label);
            }

            for (int i = 0; i < 3; i++)
            {
                int y = 379 + (23 * i);
                string label = i < durations.Length ? durations[i].Label : "--";
                bool selected = m_SelectedDuration == i;

                AddButton(640, y, selected ? 536 : 437, 536, 1100 + i, GumpButtonType.Reply, 0);
                AddLabel(664, y - 3, 1152, (selected ? "[X] " : "") + label);
            }

            AddImageTiled(435, 447, 335, 13, 630);
            AddButton(553, 476, 492, 248, ButtonConfirm, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID >= 1000 && info.ButtonID <= 1002)
            {
                from.SendGump(new EspetaculoReservationGump(from, m_CityId, m_ConstructionKey, info.ButtonID - 1000, m_SelectedDuration));
                return;
            }

            if (info.ButtonID >= 1100 && info.ButtonID <= 1102)
            {
                from.SendGump(new EspetaculoReservationGump(from, m_CityId, m_ConstructionKey, m_SelectedSlot, info.ButtonID - 1100));
                return;
            }

            if (info.ButtonID != ButtonConfirm)
                return;

            string message;
            if (!EspetaculoSystem.TryReserve(from, m_ConstructionKey, m_CityId, m_SelectedSlot, m_SelectedDuration, out message))
            {
                from.SendMessage(message);
                from.SendGump(new EspetaculoReservationGump(from, m_CityId, m_ConstructionKey, m_SelectedSlot, m_SelectedDuration));
                return;
            }

            from.SendMessage(message);
        }
    }

    public class EspetaculoControlGump : Gump
    {
        private readonly int m_CityId;
        private readonly string m_ConstructionKey;
        private readonly EspetaculoVenueType m_VenueType;
        private readonly int m_ControlSerial;

        private const int ButtonAudienceOff = 1;
        private const int ButtonAudienceOn = 2;
        private const int ButtonSetPiece1 = 3;
        private const int ButtonSetPiece2 = 4;
        private const int ButtonStageOn = 5;
        private const int ButtonStageOff = 6;
        private const int ButtonColorBlue = 10;
        private const int ButtonColorRed = 11;
        private const int ButtonColorGreen = 12;
        private const int ButtonColorPurple = 13;
        private const int ButtonColorWhite = 14;
        private const int ButtonColorYellow = 15;

        private static readonly Dictionary<int, Timer> m_RangeTimers = new Dictionary<int, Timer>();

        public EspetaculoControlGump(PlayerMobile from, int cityId, string constructionKey, EspetaculoVenueType venueType, int controlSerial)
            : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            m_VenueType = venueType;
            m_ControlSerial = controlSerial;

            StartRangeTimer(from, controlSerial);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            bool audienceDimmed = EspetaculoSystem.IsAudienceDimmed(m_ConstructionKey);
            bool stageOn = EspetaculoSystem.AreStageLightsOn(m_ConstructionKey);
            EspetaculoLightColor color = EspetaculoSystem.GetSelectedLightColor(m_ConstructionKey);
            bool piece1 = EspetaculoSystem.IsSetPieceOpen(m_ConstructionKey, 0);
            bool piece2 = EspetaculoSystem.IsSetPieceOpen(m_ConstructionKey, 1);

            AddPage(0);
            AddImageTiled(422, 206, 365, 326, 392);
            AddImageTiled(395, 209, 40, 323, 631);
            AddImageTiled(773, 197, 40, 340, 631);
            AddImageTiled(419, 177, 364, 37, 630);
            AddImageTiled(416, 515, 368, 37, 630);
            AddImage(391, 173, 1315);
            AddImage(756, 170, 1316);
            AddImage(758, 500, 1317);
            AddImage(391, 500, 1318);

            AddLabel(580, 217, 0, EspetaculoSystem.GetVenueLabel(m_VenueType));
            AddImageTiled(435, 234, 335, 13, 630);

            AddButton(449, 268, audienceDimmed ? 536 : 437, 536, ButtonAudienceOff, GumpButtonType.Reply, 0);
            AddLabel(473, 265, 0, "Apagar Luzes Plateia");

            AddButton(449, 291, !audienceDimmed ? 536 : 437, 536, ButtonAudienceOn, GumpButtonType.Reply, 0);
            AddLabel(473, 288, 0, "Acender Luzes Plateia");

            AddImageTiled(435, 372, 335, 13, 630);

            if (m_VenueType == EspetaculoVenueType.Theater)
            {
                AddButton(449, 314, piece1 && piece2 ? 536 : 437, 536, ButtonSetPiece1, GumpButtonType.Reply, 0);
                AddLabel(473, 311, 0, "Abrir Cortinas");
                AddButton(449, 338, !(piece1 || piece2) ? 536 : 437, 536, ButtonSetPiece2, GumpButtonType.Reply, 0);
                AddLabel(473, 335, 0, "Fechar Cortinas");
            }
            else
            {
                AddButton(449, 314, piece1 ? 536 : 437, 536, ButtonSetPiece1, GumpButtonType.Reply, 0);
                AddLabel(473, 311, 0, piece1 ? "Fechar Jaula 1" : "Abrir Jaula 1");
                AddButton(449, 338, piece2 ? 536 : 437, 536, ButtonSetPiece2, GumpButtonType.Reply, 0);
                AddLabel(473, 335, 0, piece2 ? "Fechar Jaula 2" : "Abrir Jaula 2");
            }

            AddButton(449, 404, stageOn ? 536 : 437, 536, ButtonStageOn, GumpButtonType.Reply, 0);
            AddLabel(473, 401, 0, "Acender Palco");

            AddButton(651, 405, !stageOn ? 536 : 437, 536, ButtonStageOff, GumpButtonType.Reply, 0);
            AddLabel(675, 402, 0, "Apagar Palco");

            AddButton(461, 438, color == EspetaculoLightColor.Blue ? 536 : 437, 536, ButtonColorBlue, GumpButtonType.Reply, 0);
            AddLabel(485, 435, 0, "Azul");

            AddButton(461, 461, color == EspetaculoLightColor.Red ? 536 : 437, 536, ButtonColorRed, GumpButtonType.Reply, 0);
            AddLabel(485, 458, 0, "Vermelho");

            AddButton(461, 484, color == EspetaculoLightColor.Green ? 536 : 437, 536, ButtonColorGreen, GumpButtonType.Reply, 0);
            AddLabel(485, 481, 0, "Verde");

            AddButton(563, 438, color == EspetaculoLightColor.Purple ? 536 : 437, 536, ButtonColorPurple, GumpButtonType.Reply, 0);
            AddLabel(587, 435, 0, "Roxo");

            AddButton(563, 461, color == EspetaculoLightColor.White ? 536 : 437, 536, ButtonColorWhite, GumpButtonType.Reply, 0);
            AddLabel(587, 458, 0, "Branco");

            AddButton(563, 484, color == EspetaculoLightColor.Yellow ? 536 : 437, 536, ButtonColorYellow, GumpButtonType.Reply, 0);
            AddLabel(587, 481, 0, "Amarelo");
        }

        private static void StartRangeTimer(PlayerMobile pm, int controlSerial)
        {
            if (pm == null)
                return;

            StopRangeTimer(pm);

            if (controlSerial <= 0 || pm.Deleted || pm.NetState == null)
                return;

            RangeTimer timer = new RangeTimer(pm, controlSerial);
            m_RangeTimers[pm.Serial.Value] = timer;
            timer.Start();
        }

        private static void StopRangeTimer(PlayerMobile pm)
        {
            if (pm == null)
                return;

            Timer timer;
            if (m_RangeTimers.TryGetValue(pm.Serial.Value, out timer) && timer != null)
                timer.Stop();

            m_RangeTimers.Remove(pm.Serial.Value);
        }

        private class RangeTimer : Timer
        {
            private readonly PlayerMobile m_From;
            private readonly int m_ControlSerial;

            public RangeTimer(PlayerMobile from, int controlSerial)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                m_From = from;
                m_ControlSerial = controlSerial;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                if (m_From == null || m_From.Deleted || m_From.NetState == null)
                {
                    StopRangeTimer(m_From);
                    return;
                }

                if (!m_From.HasGump(typeof(EspetaculoControlGump)))
                {
                    StopRangeTimer(m_From);
                    return;
                }

                Item item = World.FindItem((Serial)m_ControlSerial);
                if (item == null || item.Deleted)
                {
                    m_From.CloseGump(typeof(EspetaculoControlGump));
                    StopRangeTimer(m_From);
                    return;
                }

                if (m_From.Map != item.Map || !m_From.InRange(item.GetWorldLocation(), 3))
                {
                    m_From.CloseGump(typeof(EspetaculoControlGump));
                    StopRangeTimer(m_From);
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            string message = String.Empty;

            switch (info.ButtonID)
            {
                case ButtonAudienceOff:
                    EspetaculoSystem.SetAudienceLights(from, m_ConstructionKey, m_CityId, true, out message);
                    break;
                case ButtonAudienceOn:
                    EspetaculoSystem.SetAudienceLights(from, m_ConstructionKey, m_CityId, false, out message);
                    break;
                case ButtonSetPiece1:
                    if (m_VenueType == EspetaculoVenueType.Theater)
                        EspetaculoSystem.SetTheaterCurtains(from, m_ConstructionKey, m_CityId, true, out message);
                    else
                        EspetaculoSystem.ToggleCircusCage(from, m_ConstructionKey, m_CityId, 0, out message);
                    break;
                case ButtonSetPiece2:
                    if (m_VenueType == EspetaculoVenueType.Theater)
                        EspetaculoSystem.SetTheaterCurtains(from, m_ConstructionKey, m_CityId, false, out message);
                    else
                        EspetaculoSystem.ToggleCircusCage(from, m_ConstructionKey, m_CityId, 1, out message);
                    break;
                case ButtonStageOn:
                    EspetaculoSystem.SetStageLights(from, m_ConstructionKey, m_CityId, true, out message);
                    break;
                case ButtonStageOff:
                    EspetaculoSystem.SetStageLights(from, m_ConstructionKey, m_CityId, false, out message);
                    break;
                case ButtonColorBlue:
                    EspetaculoSystem.SetLightColor(from, m_ConstructionKey, m_CityId, EspetaculoLightColor.Blue, out message);
                    break;
                case ButtonColorRed:
                    EspetaculoSystem.SetLightColor(from, m_ConstructionKey, m_CityId, EspetaculoLightColor.Red, out message);
                    break;
                case ButtonColorGreen:
                    EspetaculoSystem.SetLightColor(from, m_ConstructionKey, m_CityId, EspetaculoLightColor.Green, out message);
                    break;
                case ButtonColorPurple:
                    EspetaculoSystem.SetLightColor(from, m_ConstructionKey, m_CityId, EspetaculoLightColor.Purple, out message);
                    break;
                case ButtonColorWhite:
                    EspetaculoSystem.SetLightColor(from, m_ConstructionKey, m_CityId, EspetaculoLightColor.White, out message);
                    break;
                case ButtonColorYellow:
                    EspetaculoSystem.SetLightColor(from, m_ConstructionKey, m_CityId, EspetaculoLightColor.Yellow, out message);
                    break;
                default:
                    return;
            }

            if (!String.IsNullOrWhiteSpace(message))
                from.SendMessage(message);

            from.SendGump(new EspetaculoControlGump(from, m_CityId, m_ConstructionKey, m_VenueType, m_ControlSerial));
        }
    }

    public class EspetaculoAnnouncementGump : Gump
    {
        public EspetaculoAnnouncementGump(PlayerMobile from, int cityId, EspetaculoVenueDefinition venue, EspetaculoReservation reservation, string cityName)
            : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            string title = venue != null ? venue.DisplayName : "Espetáculo";
            string verb = venue != null && venue.VenueType == EspetaculoVenueType.Circus ? "apresentado por" : "dirigida por";
            string body = "<BASEFONT COLOR=#000000>" +
                          "Dentro de <B>10 minutos</B> começará um espetáculo no <B>" + title + "</B> do reino de <B>" + cityName + "</B>.<BR><BR>" +
                          "Responsável: <B>" + (reservation != null ? reservation.RenterName : "desconhecido") + "</B>.<BR><BR>" +
                          "A apresentação será " + verb + " <B>" + (reservation != null ? reservation.RenterName : "desconhecido") + "</B>.<BR><BR>" +
                          "Horário: " + (reservation != null ? EspetaculoSystem.FormatLongDate(reservation.StartLocal) : "--") + ".</BASEFONT>";

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(303, 128, 0, title);
            AddHtml(221, 154, 377, 168, body, false, true);
            AddImage(535, 307, ReinoVisualSystem.GetSealGumpId(cityId));
        }
    }
}
