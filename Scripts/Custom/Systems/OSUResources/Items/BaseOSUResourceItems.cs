using System;
using Server.Custom.Systems.OSUResources;
using Server.Network;

namespace Server.Items
{
    public abstract class BaseOSUMaterialItem : Item
    {
        private int m_OSUMaterialId;

        public int OSUMaterialId
        {
            get { return m_OSUMaterialId; }
        }

        public OSUMaterialDefinition MaterialDefinition
        {
            get { return OSUMaterialRegistry.GetById(m_OSUMaterialId); }
        }

        public override bool DisplayLootType
        {
            get { return false; }
        }

        public BaseOSUMaterialItem(int itemID, int materialId) : base(itemID)
        {
            m_OSUMaterialId = materialId;
            Stackable = true;
            Amount = 1;
        }

        public BaseOSUMaterialItem(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            OSUMaterialDefinition def = MaterialDefinition;

            if (def != null)
                list.Add("Material OSU: " + def.Name);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_OSUMaterialId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_OSUMaterialId = reader.ReadInt();
        }
    }

    public abstract class BaseOSUChunk : BaseOSUMaterialItem
    {
        public BaseOSUChunk(int itemID, int materialId) : base(itemID, materialId)
        {
            Weight = 2.0;
        }

        public BaseOSUChunk(Serial serial) : base(serial)
        {
        }

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

    public abstract class BaseOSUBlock : BaseOSUMaterialItem
    {
        public BaseOSUBlock(int itemID, int materialId, double weight) : base(itemID, materialId)
        {
            Weight = weight;
        }

        public BaseOSUBlock(Serial serial) : base(serial)
        {
        }

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

    public abstract class BaseOSUPowder : BaseOSUMaterialItem
    {
        public BaseOSUPowder(int itemID, int materialId) : base(itemID, materialId)
        {
            Weight = 1.0;
        }

        public BaseOSUPowder(Serial serial) : base(serial)
        {
        }

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

    public abstract class BaseOSUNugget : BaseOSUMaterialItem
    {
        public BaseOSUNugget(int itemID, int materialId) : base(itemID, materialId)
        {
            Weight = 0.2;
        }

        public BaseOSUNugget(Serial serial) : base(serial)
        {
        }

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
