using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class QuartzChunk : BaseOSUChunk
    {
        [Constructable]
        public QuartzChunk() : this(1) { }

        [Constructable]
        public QuartzChunk(int amount) : base(0x1779, OSUMaterialIds.Quartz)
        {
            Name = "quartzo";
            Hue = 0x47F;
            Stackable = true;
            Amount = amount;
        }

        public QuartzChunk(Serial serial) : base(serial) { }

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

    public class QuartzPowder : BaseOSUPowder
    {
        [Constructable]
        public QuartzPowder() : this(1) { }

        [Constructable]
        public QuartzPowder(int amount) : base(0x103D, OSUMaterialIds.Quartz)
        {
            Name = "pó de quartzo";
            Hue = 0x47F;
            Stackable = true;
            Amount = amount;
        }

        public QuartzPowder(Serial serial) : base(serial) { }

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
