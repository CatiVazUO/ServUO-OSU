
using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Mobiles;
using Server.Custom.Systems.Espetaculos.Gumps;

namespace Server.Custom.Systems.Espetaculos.Mobiles
{
    [CorpseName("um bilheteiro")]
    public class EspetaculoBilheteriaNpc : BaseNoTradeVendor
    {
        private int m_GovernmentCityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId
        {
            get { return m_GovernmentCityId; }
            set { m_GovernmentCityId = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey
        {
            get { return m_ConstructionKey; }
            set { m_ConstructionKey = value ?? String.Empty; }
        }

        [Constructable]
        public EspetaculoBilheteriaNpc() : base("bilheteiro")
        {
            CanMove = false;
            m_GovernmentCityId = -1;
            m_ConstructionKey = String.Empty;
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            return true;
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            if (e.Mobile == null || !e.Mobile.InRange(Location, 3))
                return;

            string speech = e.Speech != null ? e.Speech.ToLowerInvariant() : String.Empty;

            if (speech.IndexOf("comprar") >= 0)
            {
                HandleBuy(e.Mobile as PlayerMobile);
                e.Handled = true;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(Location, 3))
            {
                from.SendMessage("Chegue mais perto da bilheteria.");
                return;
            }

            from.SendMessage("Diga 'comprar' ao bilheteiro para adquirir um ingresso.");
        }

        private bool EnsureVenue()
        {
            if (!String.IsNullOrWhiteSpace(m_ConstructionKey) && m_GovernmentCityId >= 0)
                return true;

            string key;
            int cityId;
            EspetaculoVenueDefinition venue;
            if (!EspetaculoSystem.TryResolveVenueAt(Location, Map, out key, out cityId, out venue))
                return false;

            m_ConstructionKey = key;
            m_GovernmentCityId = cityId;
            return true;
        }

        private void HandleBuy(PlayerMobile from)
        {
            if (from == null)
                return;

            if (!EnsureVenue())
            {
                from.SendMessage("A bilheteria ainda não foi vinculada corretamente.");
                return;
            }

            string message;
            if (!EspetaculoSystem.TryBuyTicket(from, m_ConstructionKey, m_GovernmentCityId, out message))
                from.SendMessage(message);
            else
                from.SendMessage(message);
        }

        public EspetaculoBilheteriaNpc(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_GovernmentCityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_GovernmentCityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            CanMove = false;
        }
    }

    [CorpseName("um porteiro")]
    public class EspetaculoPortaNpc : BaseNoTradeVendor
    {
        private int m_GovernmentCityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId
        {
            get { return m_GovernmentCityId; }
            set { m_GovernmentCityId = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey
        {
            get { return m_ConstructionKey; }
            set { m_ConstructionKey = value ?? String.Empty; }
        }

        [Constructable]
        public EspetaculoPortaNpc() : base("porteiro")
        {
            CanMove = false;
            m_GovernmentCityId = -1;
            m_ConstructionKey = String.Empty;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(Location, 3))
            {
                pm.SendMessage("Chegue mais perto do porteiro.");
                return;
            }

            if (!EnsureVenue())
            {
                pm.SendMessage("A porta ainda não foi vinculada corretamente.");
                return;
            }

            EspetaculoReservation active = EspetaculoSystem.GetActiveReservation(m_ConstructionKey);

            if (active != null)
            {
                string message;
                if (!EspetaculoSystem.TryUseTicketGate(pm, m_ConstructionKey, m_GovernmentCityId, out message))
                    pm.SendMessage(message);
                else
                    pm.SendMessage(message);

                return;
            }

            pm.CloseGump(typeof(EspetaculoReservationGump));
            pm.SendGump(new EspetaculoReservationGump(pm, m_GovernmentCityId, m_ConstructionKey));
        }

        private bool EnsureVenue()
        {
            if (!String.IsNullOrWhiteSpace(m_ConstructionKey) && m_GovernmentCityId >= 0)
                return true;

            string key;
            int cityId;
            EspetaculoVenueDefinition venue;
            if (!EspetaculoSystem.TryResolveVenueAt(Location, Map, out key, out cityId, out venue))
                return false;

            m_ConstructionKey = key;
            m_GovernmentCityId = cityId;
            return true;
        }

        public EspetaculoPortaNpc(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_GovernmentCityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_GovernmentCityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            CanMove = false;
        }
    }
}
