using Server.Custom.Correios;
using Server.Custom.Mobiles;
using Server.Gumps;
using Server.Mobiles;
using System;

namespace Server.Custom.Correios
{
    [CorpseName("um carteiro")]
    public class CorreioNPC : BaseNoTradeVendor
    {
        [Constructable]
        public CorreioNPC() : base("carteiro")
        {
            CanMove = false;
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
            CorreioStorage.Ensure();

            from.CloseGump(typeof(CorreiosGump));
            from.SendGump(new CorreiosGump(from));

            CorreiosGump.OpenSubscriptionNoticeIfAny(from);
        }

        public CorreioNPC(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int v = reader.ReadInt();
        }
    }
}
