using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class TinNugget : BaseOSUNugget
    {
        [Constructable]
        public TinNugget() : this(1) { }

        [Constructable]
        public TinNugget(int amount) : base(0x19B9, OSUMaterialIds.Tin)
        {
            Name = "pepita de estanho";
            Hue = 0x973;
            Stackable = true;
            Amount = amount;
        }

        public TinNugget(Serial serial) : base(serial) { }

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
