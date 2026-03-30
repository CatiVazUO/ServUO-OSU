using System;
using Server.Custom.Systems.OSUResources;

namespace Server.Items
{
    public class SoapstoneChunk : BaseOSUChunk
    {
        [Constructable]
        public SoapstoneChunk() : this(1) { }

        [Constructable]
        public SoapstoneChunk(int amount) : base(0x1779, OSUMaterialIds.Soapstone)
        {
            Name = "pedra-sabão";
            Hue = 0x97B;
            Stackable = true;
            Amount = amount;
        }

        public SoapstoneChunk(Serial serial) : base(serial) { }

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

    public class SoapstoneBlockSmall : BaseOSUBlock
    {
        [Constructable]
        public SoapstoneBlockSmall() : this(1) { }

        [Constructable]
        public SoapstoneBlockSmall(int amount) : base(0x10B2, OSUMaterialIds.Soapstone, 10.0)
        {
            Name = "bloco de pedra-sabão";
            Hue = 0x97B;
            Stackable = true;
            Amount = amount;
        }

        public SoapstoneBlockSmall(Serial serial) : base(serial) { }

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

    public class SoapstoneBlockLarge : BaseOSUBlock
    {
        [Constructable]
        public SoapstoneBlockLarge() : this(1) { }

        [Constructable]
        public SoapstoneBlockLarge(int amount) : base(0x10B6, OSUMaterialIds.Soapstone, 130.0)
        {
            Name = "grande bloco de pedra-sabão";
            Hue = 0x97B;
            Stackable = true;
            Amount = amount;
        }

        public SoapstoneBlockLarge(Serial serial) : base(serial) { }

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
