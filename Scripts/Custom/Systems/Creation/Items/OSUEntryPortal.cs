using Server;
using Server.Mobiles;
using Server.Custom.Systems.Creation.Engine;

namespace Server.Custom.Systems.Creation.Items
{
    public class OSUEntryPortal : Item
    {
        // visual de “portal/moongate” (você pode trocar)
        [Constructable]
        public OSUEntryPortal() : base(0x1FD4)
        {
            Movable = false;
            Name = "Portal para Amanti";
        }

        public OSUEntryPortal(Serial serial) : base(serial) { }

        public override bool OnMoveOver(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null)
                return true;

            string reason;
            if (!OSUCreationFinalizer.TryEnterAmanti(pm, out reason))
            {
                pm.SendMessage(0x35, reason ?? "Você não pode entrar ainda.");
                return false;
            }

            return false; // impede “andar através” (porque a gente já teleportou)
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
