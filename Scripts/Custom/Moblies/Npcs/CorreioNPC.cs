using Server.Custom.Correios;
using Server.Commands;
using Server.Custom.Mobiles;
using Server.Gumps;
using Server.Mobiles;
using System;
using Server.Custom.Reinos;

namespace Server.Custom.Correios
{
    [CorpseName("um carteiro")]
    public class CorreioNPC : BaseNoTradeVendor
    {
        private int m_GovernmentCityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId
        {
            get { return m_GovernmentCityId; }
            set { m_GovernmentCityId = value; InvalidateProperties(); }
        }

        [Constructable]
        public CorreioNPC() : base("carteiro")
        {
            CanMove = false;
            m_GovernmentCityId = -1;
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            return true;
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            if (e.Mobile == null)
                return;

            if (!e.Mobile.InRange(Location, 3))
                return;

            if (e.Speech != null && e.Speech.ToLower().StartsWith("correio"))
            {
                OpenGump(e.Mobile);
                e.Handled = true;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null)
                return;

            if (!from.InRange(Location, 3))
            {
                from.SendMessage("Chegue mais perto do carteiro.");
                return;
            }

            OpenGump(from);
        }

        private void OpenGump(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm != null)
            {
                string reason;
                if (!ReinoDiplomacySystem.CanUsePostOffice(pm, m_GovernmentCityId, out reason))
                {
                    pm.SendMessage(reason);
                    return;
                }
            }

            CorreioStorage.Ensure();

            CorreioSession session = CorreiosGump.GetSession(from);
            if (session != null)
                session.ContextNpcSerial = this.Serial.Value;

            from.CloseGump(typeof(CorreiosGump));
            from.SendGump(new CorreiosGump(from));

            CorreiosGump.OpenSubscriptionNoticeIfAny(from);
        }

        public CorreioNPC(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_GovernmentCityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int v = reader.ReadInt();

            if (v >= 1)
                m_GovernmentCityId = reader.ReadInt();
            else
                m_GovernmentCityId = -1;
        }
    }
}
