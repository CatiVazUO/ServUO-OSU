using System;
using Server.Items;
using Server.Network;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Cloths;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bolts
{
    [Flipable(0xF95, 0xF96, 0xF97, 0xF98, 0xF99, 0xF9A, 0xF9B, 0xF9C)]
    public class BoltOfLinen : Item, IScissorable, IDyable//, ICommodity
    {
        [Constructable]
        public BoltOfLinen()
            : this(1)
        {
        }

        [Constructable]
        public BoltOfLinen(int amount)
            : base(0xF95)
        {
            Stackable = true;
            Weight = 5.0;
            Amount = amount;
            Name = "rolo de linho";
        }

        public BoltOfLinen(Serial serial)
            : base(serial)
        {
        }

     /*   TextDefinition ICommodity.Description
        {
            get
            {
                return LabelNumber;
            }
        }
        bool ICommodity.IsDeedable
        {
            get
            {
                return true;
            }
        }*/
        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public bool Scissor(Mobile from, Scissors scissors)
        {
            if (Deleted || !from.CanSee(this))
                return false;

            base.ScissorHelper(from, new LinenCloth(), 50);

            return true;
        }

        public override void OnSingleClick(Mobile from)
        {
            int number = Amount == 1 ? 1049122 : 1049121;

            from.Send(new MessageLocalized(Serial, ItemID, MessageType.Label, 0x3B2, 3, number, "", (Amount * 50).ToString()));
        }
    }
}
