using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class LuxuriousXXLPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma de Luxo"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.XXL; } }
        public int PreviewItemID { get { return 0x16EF; } }
        public int SuccessChance { get { return 75; } }

        public override int PlatformHeight { get { return 12; } }

        [Constructable]
        public LuxuriousXXLPlatform() : base(0x16EF)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            LuxuriousXXLPlatform item = new LuxuriousXXLPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public LuxuriousXXLPlatform(Serial serial) : base(serial) { }

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


    public class OrnateXXLPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.XXL; } }
        public int PreviewItemID { get { return 0x16F0; } }
        public int SuccessChance { get { return 75; } }

        public override int PlatformHeight { get { return 10; } }

        [Constructable]
        public OrnateXXLPlatform() : base(0x16F0)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            OrnateXXLPlatform item = new OrnateXXLPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public OrnateXXLPlatform(Serial serial) : base(serial) { }

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


    public class SimpleXXLPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Simples"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.XXL; } }
        public int PreviewItemID { get { return 0x16F2; } }
        public int SuccessChance { get { return 80; } }
        public override int PlatformHeight { get { return 12; } }

        [Constructable]
        public SimpleXXLPlatform() : base(0x16F2)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.XXL);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            SimpleXXLPlatform item = new SimpleXXLPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public SimpleXXLPlatform(Serial serial) : base(serial) { }

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
