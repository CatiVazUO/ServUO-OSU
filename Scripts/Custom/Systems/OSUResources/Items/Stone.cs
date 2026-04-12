using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class StoneChunk : BaseOSUChunk
    {
        [Constructable]
        public StoneChunk() : this(1) { }

        [Constructable]
        public StoneChunk(int amount) : base(0x1779, OSUMaterialIds.Stone)
        {
            Name = "pedra";
            Hue = 0xA7F;
            Stackable = true;
            Amount = amount;
        }

        public StoneChunk(Serial serial) : base(serial) { }

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

    public class StoneBlockSmall : BaseOSUBlock
    {
        [Constructable]
        public StoneBlockSmall() : this(1) { }

        [Constructable]
        public StoneBlockSmall(int amount) : base(0x10B2, OSUMaterialIds.Stone, 10.0)
        {
            Name = "bloco de pedra";
            Hue = 0xA7F;
            Stackable = true;
            Amount = amount;
        }

        public StoneBlockSmall(Serial serial) : base(serial) { }

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

    public class StoneBlockLarge : BaseOSUBlock
    {
        [Constructable]
        public StoneBlockLarge() : this(1) { }

        [Constructable]
        public StoneBlockLarge(int amount) : base(0x10B6, OSUMaterialIds.Stone, 130.0)
        {
            Name = "grande bloco de pedra";
            Hue = 0xA7F;
            Stackable = true;
            Amount = amount;
        }

        public StoneBlockLarge(Serial serial) : base(serial) { }

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
