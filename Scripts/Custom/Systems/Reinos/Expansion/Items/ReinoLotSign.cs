using System;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos
{
    public class ReinoLotSign : Item
    {
        private int m_LotId;
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int LotId
        {
            get { return m_LotId; }
            set { m_LotId = value; UpdateName(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId
        {
            get { return m_CityId; }
            set { m_CityId = value; UpdateName(); }
        }

        [Constructable]
        public ReinoLotSign() : this(0, -1)
        {
        }

        [Constructable]
        public ReinoLotSign(int lotId, int cityId) : base(0x0BD1)
        {
            m_LotId = lotId;
            m_CityId = cityId;
            Movable = false;
            Weight = 255.0;
            Hue = 0;
            UpdateName();
        }

        public ReinoLotSign(Serial serial) : base(serial)
        {
        }

        private void UpdateName()
        {
            string cityName = ReinoElectionsSystem.GetCityName(m_CityId);
            Name = String.Format("placa do lote {0} de {1}", m_LotId, cityName);
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 3))
            {
                pm.SendMessage("Chegue mais perto da placa do lote.");
                return;
            }

            pm.CloseGump(typeof(ReinoLotSignGump));
            pm.SendGump(new ReinoLotSignGump(pm, m_LotId));
        }

        public override bool HandlesOnSpeech
        {
            get { return true; }
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            if (e == null || e.Mobile == null || e.Mobile.Deleted)
                return;

            if (!e.Mobile.InRange(GetWorldLocation(), 3))
                return;

            string speech = e.Speech != null ? e.Speech.ToLower() : String.Empty;

            if (speech.StartsWith("lote") || speech.StartsWith("terreno"))
            {
                OnDoubleClick(e.Mobile);
                e.Handled = true;
            }
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            ReinoExpansionSystem.OnLotSignDeleted(Serial.Value, m_LotId);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_LotId);
            writer.Write(m_CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_LotId = reader.ReadInt();
            m_CityId = reader.ReadInt();
            Movable = false;
            UpdateName();
        }
    }
}
