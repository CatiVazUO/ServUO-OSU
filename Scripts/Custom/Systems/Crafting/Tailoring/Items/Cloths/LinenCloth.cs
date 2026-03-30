using System;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bandages;
using Server.Items;
using Server.Network;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bolts;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Cloths
{
    [Flipable(0x1766, 0x1768)]
    public class LinenCloth : Cloth, IScissorable, IDyable//, ICommodity
    {
        [Constructable]
        public LinenCloth()
            : this(1)
        {
            Name = "linho cortado";
            Stackable = true;
        }

        [Constructable]
        public LinenCloth(int amount)
            : base(0x1766)
        {
            Stackable = true;
            Amount = amount;
            Name = "linho cortado";
        }

        public LinenCloth(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }
        /*  TextDefinition ICommodity.Description
          {
              get
              {
                  return this.LabelNumber;
              }
          }
          bool ICommodity.IsDeedable
          {
              get
              {
                  return true;
              }
          }*/

        public new bool Dye(Mobile from, DyeTub sender)
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

        public override void OnSingleClick(Mobile from)
        {
            int number = Amount == 1 ? 1049124 : 1049123;

            from.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", Amount.ToString()));
        }

        bool IScissorable.Scissor(Mobile from, Scissors scissors)
        {
            if (Deleted || !from.CanSee(this))
                return false;

            base.ScissorHelper(from, new LinenBandage(), 1);
            return true;
        }
    }

    public class CutUpLinenCloth : Item
    {
        public override int LabelNumber { get { return 1044458; } } // cut-up cloth

        [Constructable]
        public CutUpLinenCloth()
            : base(0x1767)
        {
        }

        public CutUpLinenCloth(Serial serial)
            : base(serial)
        {
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

        public void CutUp(Mobile from, Item[] items)
        {
            //Container backpack = from.Backpack;

            for (int i = 0; i < items.Length; i++)
            {
                BoltOfWool boc = items[i] as BoltOfWool;

                if (boc != null)
                    boc.Scissor(from, null);
            }
        }
    }

    public class CombineLinenCloth : Item
    {
        public override int LabelNumber { get { return 1044459; } } // combine cloth

        [Constructable]
        public CombineLinenCloth()
            : base(0x1767)
        {
        }

        public CombineLinenCloth(Serial serial)
            : base(serial)
        {
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

        public static bool CheckHue(int hue, int[] hues, out int count)
        {
            int result = 0;
            bool success = true;

            for (int i = 0; i < hues.Length; i++)
            {
                if (hues[i] == hue)
                {
                    result = i;
                    success = false;
                }
            }

            count = result;

            return success;
        }

        public void Combine(Mobile from, Item[] items)
        {
            Container backpack = from.Backpack;

            int[] hues = new int[backpack.Items.Count];
            int[] amounts = new int[backpack.Items.Count];

            for (int i = 0; i < items.Length; i++)
            {
                LinenCloth c = items[i] as LinenCloth;

                if (c != null)
                {
                    int count;

                    if (CheckHue(c.Hue, hues, out count))
                    {
                        hues[i] = c.Hue;
                        amounts[i] = c.Amount;
                    }
                    else
                    {
                        amounts[count] += c.Amount;
                    }

                    c.Delete();
                }
            }

            for (int i = 0; i < hues.Length; i++)
            {
                LinenCloth cloth = new LinenCloth();
                cloth.Hue = hues[i];
                cloth.Amount = amounts[i];

                if (cloth.Amount > 0)
                    backpack.DropItem(cloth);
                else
                    cloth.Delete();
            }
        }
    }
}
