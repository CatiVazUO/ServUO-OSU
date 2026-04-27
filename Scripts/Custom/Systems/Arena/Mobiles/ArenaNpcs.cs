using System;
using Server;
using Server.Mobiles;
using Server.Custom.Mobiles;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Arena.Mobiles
{
    [CorpseName("um bilheteiro da arena")]
    public class ArenaBilheteriaNpc : BaseNoTradeVendor
    {
        private int m_GovernmentCityId;
        private string m_ConstructionKey;

        [Constructable]
        public ArenaBilheteriaNpc() : base("bilheteiro")
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

            if (speech.IndexOf("comprar") >= 0 || speech.IndexOf("ingresso") >= 0)
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

            from.SendMessage("Diga 'comprar' para comprar ingresso por 100 moedas.");
        }

        private bool EnsureArena()
        {
            if (!String.IsNullOrWhiteSpace(m_ConstructionKey) && m_GovernmentCityId >= 0)
                return true;

            string key;
            int cityId;
            ArenaDefinition def;
            ReinoLotDefinition lot;

            if (!ArenaSystem.TryResolveArenaAt(Location, Map, out key, out cityId, out def, out lot))
                return false;

            m_ConstructionKey = key;
            m_GovernmentCityId = cityId;
            return true;
        }

        private void HandleBuy(PlayerMobile pm)
        {
            if (pm == null)
                return;

            if (!EnsureArena())
            {
                pm.SendMessage("Bilheteria da arena não vinculada ainda.");
                return;
            }

            string message;
            ArenaSystem.TryBuyTicket(pm, m_GovernmentCityId, m_ConstructionKey, out message);
            pm.SendMessage(message);
        }

        public ArenaBilheteriaNpc(Serial serial) : base(serial)
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

    [CorpseName("um porteiro da arena")]
    public class ArenaPorteiroNpc : BaseNoTradeVendor
    {
        private int m_GovernmentCityId;
        private string m_ConstructionKey;

        [Constructable]
        public ArenaPorteiroNpc() : base("porteiro")
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

            string key;
            int cityId;
            ArenaDefinition def;
            ReinoLotDefinition lot;

            if (!ArenaSystem.TryResolveArenaAt(Location, Map, out key, out cityId, out def, out lot))
            {
                pm.SendMessage("Arena não vinculada ainda.");
                return;
            }

            string message;
            Point3D inside = new Point3D(lot.NorthWest.X + def.PublicoTeleportOffset.X, lot.NorthWest.Y + def.PublicoTeleportOffset.Y, lot.NorthWest.Z + def.PublicoTeleportOffset.Z);

            ArenaSystem.TryUseGate(pm, key, cityId, inside, out message);
            pm.SendMessage(message);
        }

        public ArenaPorteiroNpc(Serial serial) : base(serial)
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
