using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class KaolinChunk : BaseOSUChunk
    {
        [Constructable]
        public KaolinChunk() : this(1) { }

        [Constructable]
        public KaolinChunk(int amount) : base(0x1779, OSUMaterialIds.Kaolin)
        {
            Name = "caolim";
            Hue = 0x47F;
            Stackable = true;
            Amount = amount;
        }

        public KaolinChunk(Serial serial) : base(serial) { }

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

    public class KaolinPowder : BaseOSUPowder
    {
        [Constructable]
        public KaolinPowder() : this(1) { }

        [Constructable]
        public KaolinPowder(int amount) : base(0x103D, OSUMaterialIds.Kaolin)
        {
            Name = "pó de caolim";
            Hue = 0x47F;
            Stackable = true;
            Amount = amount;
        }

        public KaolinPowder(Serial serial) : base(serial) { }

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
