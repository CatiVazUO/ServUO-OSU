using Server.Custom.Systems.PlayerMadeStatues;
using Server.Spells.SkillMasteries;

namespace Server.Items
{
    public class SimpleSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Simples"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16D7; } }
        public int SuccessChance { get { return 95; } }
        public int RequiredAmount { get { return 5; } }

        public override int PlatformHeight { get { return 4; } }

        public SculptorRequirement[] GetExtraRequirements(int materialId) { return new SculptorRequirement[]
            {
                new SculptorRequirement(typeof(GoldIngot), 0, "Ingots de Ferro")
            };}

        [Constructable]
        public SimpleSmallPlatform() : base(0x16D7)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            SimpleSmallPlatform item = new SimpleSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public SimpleSmallPlatform(Serial serial) : base(serial) { }

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

    public class PolishedSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Polida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16DE; } }
        public int SuccessChance { get { return 95; } }
        public override int PlatformHeight { get { return 4; } }

        [Constructable]
        public PolishedSmallPlatform() : base(0x16DE)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            PolishedSmallPlatform item = new PolishedSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public PolishedSmallPlatform(Serial serial) : base(serial) { }

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

    public class OrnateSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16DA; } }
        public int SuccessChance { get { return 95; } }
        public override int PlatformHeight { get { return 3; } }

        [Constructable]
        public OrnateSmallPlatform() : base(0x16DA)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            OrnateSmallPlatform item = new OrnateSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public OrnateSmallPlatform(Serial serial) : base(serial) { }

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

    public class GoldenBaseSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Ornada Alta"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16D6; } }
        public int SuccessChance { get { return 90; } }
        public override int PlatformHeight { get { return 6; } }

        [Constructable]
        public GoldenBaseSmallPlatform() : base(0x16D6)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenBaseSmallPlatform item = new GoldenBaseSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenBaseSmallPlatform(Serial serial) : base(serial) { }

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


    public class TallSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16D9; } }
        public int SuccessChance { get { return 95; } }

        public override int PlatformHeight { get { return 8; } }

        [Constructable]
        public TallSmallPlatform() : base(0x16D9)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            TallSmallPlatform item = new TallSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public TallSmallPlatform(Serial serial) : base(serial) { }

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

    public class SculptedTallSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Esculpida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16DB; } }
        public int SuccessChance { get { return 90; } }
        public override int PlatformHeight { get { return 8; } }

        [Constructable]
        public SculptedTallSmallPlatform() : base(0x16DB)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            SculptedTallSmallPlatform item = new SculptedTallSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public SculptedTallSmallPlatform(Serial serial) : base(serial) { }

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

    public class GoldenTallSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16E1; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 5; } }

        [Constructable]
        public GoldenTallSmallPlatform() : base(0x16E1)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenTallSmallPlatform item = new GoldenTallSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenTallSmallPlatform(Serial serial) : base(serial) { }

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

    public class PolishedTallSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Polida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16DF; } }
        public int SuccessChance { get { return 90; } }
        public override int PlatformHeight { get { return 7; } }

        [Constructable]
        public PolishedTallSmallPlatform() : base(0x16DF)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            PolishedTallSmallPlatform item = new PolishedTallSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public PolishedTallSmallPlatform(Serial serial) : base(serial) { }

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


    public class PolishedOrnateSmallPlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Polida Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Small; } }
        public int PreviewItemID { get { return 0x16E0; } }
        public int SuccessChance { get { return 90; } }
        public override int PlatformHeight { get { return 3; } }

        [Constructable]
        public PolishedOrnateSmallPlatform() : base(0x16E0)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Small).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Small);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            PolishedOrnateSmallPlatform item = new PolishedOrnateSmallPlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public PolishedOrnateSmallPlatform(Serial serial) : base(serial) { }

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
