using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class GraniteChunk : BaseOSUChunk
    {
        [Constructable]
        public GraniteChunk() : this(1) { }

        [Constructable]
        public GraniteChunk(int amount) : base(0x1779, OSUMaterialIds.Granite)
        {
            Name = "granito";
            Hue = 0x835;
            Stackable = true;
            Amount = amount;
        }

        public GraniteChunk(Serial serial) : base(serial) { }

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

    public class GraniteBlockSmall : BaseOSUBlock
    {
        [Constructable]
        public GraniteBlockSmall() : this(1) { }

        [Constructable]
        public GraniteBlockSmall(int amount) : base(0x10B2, OSUMaterialIds.Granite, 10.0)
        {
            Name = "bloco de granito";
            Hue = 0x835;
            Stackable = true;
            Amount = amount;
        }

        public GraniteBlockSmall(Serial serial) : base(serial) { }

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

    public class GraniteBlockLarge : BaseOSUBlock
    {
        [Constructable]
        public GraniteBlockLarge() : this(1) { }

        [Constructable]
        public GraniteBlockLarge(int amount) : base(0x10B6, OSUMaterialIds.Granite, 130.0)
        {
            Name = "grande bloco de granito";
            Hue = 0x835;
            Stackable = true;
            Amount = amount;
        }

        public GraniteBlockLarge(Serial serial) : base(serial) { }

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
