using System;
using Server;
using Server.Items;
using Server.Network;

namespace Server.Custom.Items
{
    public class BlessedBag : Pouch
    {
        [Constructable]
        public BlessedBag()
        {
            Name = "Blessed Bag";
            Hue = 0; // se quiser cor, troque
            LootType = LootType.Blessed; // a bag é newbie/blessed
            Weight = 2.0;
        }

        public BlessedBag(Serial serial) : base(serial)
        {
        }

        // Regra: só aceita itens blessed/newbied
        private static bool IsBlessedItem(Item item)
        {
            if (item == null || item.Deleted)
                return false;

            // Alguns shards usam Newbied, outros só Blessed
            return item.LootType == LootType.Blessed || item.LootType == LootType.Newbied;
        }

        private bool CanAccept(Mobile from, Item dropped)
        {
            if (dropped == null)
                return false;

            // Bloqueia se não for blessed/newbied
            if (!IsBlessedItem(dropped))
            {
                from.SendMessage(0x22, "A Blessed Bag só aceita itens blessed/newbied.");
                return false;
            }

            return true;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!CanAccept(from, dropped))
                return false;

            return base.OnDragDrop(from, dropped);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!CanAccept(from, item))
                return false;

            return base.OnDragDropInto(from, item, p);
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

            // garante que continue blessed mesmo se alguém mexer
            LootType = LootType.Blessed;
        }
    }
}
