using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class ShortOrnadeGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F1; } }
        public int SuccessChance { get { return 100; } }

        public override int PlatformHeight { get { return 5; } }

        [Constructable]
        public ShortOrnadeGiantPlatform() : base(0x16F1)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            ShortOrnadeGiantPlatform item = new ShortOrnadeGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public ShortOrnadeGiantPlatform(Serial serial) : base(serial) { }

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


    public class GoldenGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Dourada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F3; } }
        public int SuccessChance { get { return 80; } }

        public override int PlatformHeight { get { return 10; } }

        [Constructable]
        public GoldenGiantPlatform() : base(0x16F3)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenGiantPlatform item = new GoldenGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenGiantPlatform(Serial serial) : base(serial) { }

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


    public class RusticGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Rústica"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F4; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 8; } }

        [Constructable]
        public RusticGiantPlatform() : base(0x16F4)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            RusticGiantPlatform item = new RusticGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public RusticGiantPlatform(Serial serial) : base(serial) { }

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


    public class RusticShortGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Rústica Baixa"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F5; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 6; } }

        [Constructable]
        public RusticShortGiantPlatform() : base(0x16F5)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            RusticShortGiantPlatform item = new RusticShortGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public RusticShortGiantPlatform(Serial serial) : base(serial) { }

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


    public class BrokenGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Quebrada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F6; } }
        public int SuccessChance { get { return 85; } }
        public override int PlatformHeight { get { return 7; } }

        [Constructable]
        public BrokenGiantPlatform() : base(0x16F6)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            BrokenGiantPlatform item = new BrokenGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public BrokenGiantPlatform(Serial serial) : base(serial) { }

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


    public class GoldenSimpleGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Base Dourada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F7; } }
        public int SuccessChance { get { return 80; } }

        public override int PlatformHeight { get { return 7; } }

        [Constructable]
        public GoldenSimpleGiantPlatform() : base(0x16F7)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenSimpleGiantPlatform item = new GoldenSimpleGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenSimpleGiantPlatform(Serial serial) : base(serial) { }

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


    public class SculptedGiantPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Esculpida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Giant; } }
        public int PreviewItemID { get { return 0x16F8; } }
        public int SuccessChance { get { return 80; } }

        public override int PlatformHeight { get { return 8; } }

        [Constructable]
        public SculptedGiantPlatform() : base(0x16F8)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Giant);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            SculptedGiantPlatform item = new SculptedGiantPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public SculptedGiantPlatform(Serial serial) : base(serial) { }

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
