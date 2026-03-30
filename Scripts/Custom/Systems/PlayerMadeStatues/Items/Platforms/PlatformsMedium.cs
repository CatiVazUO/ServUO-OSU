using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class TallSimpleSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Simples Alta"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Medium; } }
        public int PreviewItemID { get { return 0x16D8; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 9; } }

        [Constructable]
        public TallSimpleSmallPlatform() : base(0x16D8)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            TallSimpleSmallPlatform item = new TallSimpleSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public TallSimpleSmallPlatform(Serial serial) : base(serial) { }

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


    public class GoldenMediumPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Dourada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Medium; } }
        public int PreviewItemID { get { return 0x16DC; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 9; } }

        [Constructable]
        public GoldenMediumPlatform() : base(0x16DC)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenMediumPlatform item = new GoldenMediumPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenMediumPlatform(Serial serial) : base(serial) { }

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

    public class OrnateMediumPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Medium; } }
        public int PreviewItemID { get { return 0x16DD; } }
        public int SuccessChance { get { return 90; } }
        public override int PlatformHeight { get { return 10; } }

        [Constructable]
        public OrnateMediumPlatform() : base(0x16DD)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Medium);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            OrnateMediumPlatform item = new OrnateMediumPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public OrnateMediumPlatform(Serial serial) : base(serial) { }

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
