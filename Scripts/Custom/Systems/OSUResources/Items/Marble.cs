using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class MarbleChunk : BaseOSUChunk
    {
        [Constructable]
        public MarbleChunk() : this(1) { }

        [Constructable]
        public MarbleChunk(int amount) : base(0x1779, OSUMaterialIds.Marble)
        {
            Name = "mármore";
            Hue = 0x47E;
            Stackable = true;
            Amount = amount;
        }

        public MarbleChunk(Serial serial) : base(serial) { }

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

    public class MarbleBlockSmall : BaseOSUBlock
    {
        [Constructable]
        public MarbleBlockSmall() : this(1) { }

        [Constructable]
        public MarbleBlockSmall(int amount) : base(0x10B2, OSUMaterialIds.Marble, 10.0)
        {
            Name = "bloco de mármore";
            Hue = 0x47E;
            Stackable = true;
            Amount = amount;
        }

        public MarbleBlockSmall(Serial serial) : base(serial) { }

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

    public class MarbleBlockLarge : BaseOSUBlock
    {
        [Constructable]
        public MarbleBlockLarge() : this(1) { }

        [Constructable]
        public MarbleBlockLarge(int amount) : base(0x10B6, OSUMaterialIds.Marble, 130.0)
        {
            Name = "grande bloco de mármore";
            Hue = 0x47E;
            Stackable = true;
            Amount = amount;
        }

        public MarbleBlockLarge(Serial serial) : base(serial) { }

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

    public class MarblePowder : BaseOSUPowder
    {
        [Constructable]
        public MarblePowder() : this(1) { }

        [Constructable]
        public MarblePowder(int amount) : base(0x103D, OSUMaterialIds.Marble)
        {
            Name = "pó de mármore";
            Hue = 0x47E;
            Stackable = true;
            Amount = amount;
        }

        public MarblePowder(Serial serial) : base(serial) { }
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
