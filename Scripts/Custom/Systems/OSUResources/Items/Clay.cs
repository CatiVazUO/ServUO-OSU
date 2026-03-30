using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class ClayChunk : BaseOSUChunk
    {
        [Constructable]
        public ClayChunk() : this(1) { }

        [Constructable]
        public ClayChunk(int amount) : base(0x1BF2, OSUMaterialIds.Clay)
        {
            Name = "argila";
            Hue = 0x96F;
            Stackable = true;
            Amount = amount;
        }

        public ClayChunk(Serial serial) : base(serial) { }

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

    public class ClayPileSmall : BaseOSUBlock
    {
        [Constructable]
        public ClayPileSmall() : this(1) { }

        [Constructable]
        public ClayPileSmall(int amount) : base(0x10B2, OSUMaterialIds.Clay, 10.0)
        {
            Name = "monte de argila";
            Hue = 0x96F;
            Stackable = true;
            Amount = amount;
        }

        public ClayPileSmall(Serial serial) : base(serial) { }

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

    public class ClayPileLarge : BaseOSUBlock
    {
        [Constructable]
        public ClayPileLarge() : this(1) { }

        [Constructable]
        public ClayPileLarge(int amount) : base(0x10B6, OSUMaterialIds.Clay, 130.0)
        {
            Name = "grande monte de argila";
            Hue = 0x96F;
            Stackable = true;
            Amount = amount;
        }

        public ClayPileLarge(Serial serial) : base(serial) { }

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
