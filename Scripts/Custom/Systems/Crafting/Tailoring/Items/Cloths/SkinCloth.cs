using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bandages;
using System;
using Server.Items;
using Server.Network;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bolts;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Cloths
{
    [Flipable(0x1766, 0x1768)]
    public class SkinCloth : Cloth, IScissorable, IDyable//, ICommodity
    {
        [Constructable]
        public SkinCloth()
            : this(1)
        {
            Name = "pele cortada";
            Stackable = true;
        }

        [Constructable]
        public SkinCloth(int amount)
            : base(0x1766)
        {
            Name = "pele cortada";
            Stackable = true;
            Amount = amount;
        }

        public SkinCloth(Serial serial)
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

            base.ScissorHelper(from, new SkinBandage(), 1);
            return true;
        }
    }

    public class CutUpSkinCloth : Item
    {
        public override int LabelNumber { get { return 1044458; } } // cut-up cloth

        [Constructable]
        public CutUpSkinCloth()
            : base(0x1767)
        {
        }

        public CutUpSkinCloth(Serial serial)
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

    public class CombineSkinCloth : Item
    {
        public override int LabelNumber { get { return 1044459; } } // combine cloth

        [Constructable]
        public CombineSkinCloth()
            : base(0x1767)
        {
        }

        public CombineSkinCloth(Serial serial)
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
                SkinCloth c = items[i] as SkinCloth;

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
                SkinCloth cloth = new SkinCloth();
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
