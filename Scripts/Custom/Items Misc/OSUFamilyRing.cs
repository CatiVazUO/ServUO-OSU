using Server;

namespace Server.Items
{
    public class OSUFamilyRing : GoldRing
    {
        [Constructable]
        public OSUFamilyRing()
        {
            Name = "anel de família";
            Hue = 1153;
            LootType = LootType.Blessed;
            Movable = true;
        }

        public OSUFamilyRing(Serial serial) : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("símbolo de linhagem nobre");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            LootType = LootType.Blessed;
        }
    }
}
