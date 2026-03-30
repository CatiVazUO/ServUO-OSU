using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class BronzeBlock : BaseOSUBlock
    {
        [Constructable]
        public BronzeBlock() : this(1) { }

        [Constructable]
        public BronzeBlock(int amount) : base(0x1BEF, OSUMaterialIds.Bronze, 5.0)
        {
            Name = "bloco de bronze";
            Hue = 0x972;
            Stackable = true;
            Amount = amount;
        }

        public BronzeBlock(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
