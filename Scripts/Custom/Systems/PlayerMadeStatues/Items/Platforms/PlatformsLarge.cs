using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class EsculptedLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Esculpida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E2; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 5; } }

        [Constructable]
        public EsculptedLargePlatform() : base(0x16E2)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            EsculptedLargePlatform item = new EsculptedLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public EsculptedLargePlatform(Serial serial) : base(serial) { }

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


    public class SimpleLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Simples"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E3; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 5; } }

        [Constructable]
        public SimpleLargePlatform() : base(0x16E3)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            SimpleLargePlatform item = new SimpleLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public SimpleLargePlatform(Serial serial) : base(serial) { }

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


    public class OrnateLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E4; } }
        public int SuccessChance { get { return 85; } }
        public override int PlatformHeight { get { return 5; } }

        [Constructable]
        public OrnateLargePlatform() : base(0x16E4)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            OrnateLargePlatform item = new OrnateLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public OrnateLargePlatform(Serial serial) : base(serial) { }

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


    public class PolishedLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Polida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E5; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 4; } }

        [Constructable]
        public PolishedLargePlatform() : base(0x16E5)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            PolishedLargePlatform item = new PolishedLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public PolishedLargePlatform(Serial serial) : base(serial) { }

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


    public class TallLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E6; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 6; } }

        [Constructable]
        public TallLargePlatform() : base(0x16E6)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            TallLargePlatform item = new TallLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public TallLargePlatform(Serial serial) : base(serial) { }

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


    public class TallPolishedLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Polida"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E7; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 8; } }

        [Constructable]
        public TallPolishedLargePlatform() : base(0x16E7)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            TallPolishedLargePlatform item = new TallPolishedLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public TallPolishedLargePlatform(Serial serial) : base(serial) { }

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


    public class TallGoldenLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Dourada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E8; } }
        public int SuccessChance { get { return 80; } }

        public override int PlatformHeight { get { return 8; } }

        [Constructable]
        public TallGoldenLargePlatform() : base(0x16E8)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            TallGoldenLargePlatform item = new TallGoldenLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public TallGoldenLargePlatform(Serial serial) : base(serial) { }

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

    public class GoldenBaseLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Dourada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16E9; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 4; } }

        [Constructable]
        public GoldenBaseLargePlatform() : base(0x16E9)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenBaseLargePlatform item = new GoldenBaseLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenBaseLargePlatform(Serial serial) : base(serial) { }

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


    public class GoldenOrnateLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Dourada Ornada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16EA; } }
        public int SuccessChance { get { return 80; } }

        public override int PlatformHeight { get { return 9; } }

        [Constructable]
        public GoldenOrnateLargePlatform() : base(0x16EA)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenOrnateLargePlatform item = new GoldenOrnateLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenOrnateLargePlatform(Serial serial) : base(serial) { }

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


    public class RusticLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Rústica"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16EB; } }
        public int SuccessChance { get { return 90; } }

        public override int PlatformHeight { get { return 5; } }

        [Constructable]
        public RusticLargePlatform() : base(0x16EB)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            RusticLargePlatform item = new RusticLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public RusticLargePlatform(Serial serial) : base(serial) { }

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


    public class GoldenBasePolishedLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Polida Dourada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16EC; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 4; } }

        [Constructable]
        public GoldenBasePolishedLargePlatform() : base(0x16EC)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            GoldenBasePolishedLargePlatform item = new GoldenBasePolishedLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public GoldenBasePolishedLargePlatform(Serial serial) : base(serial) { }

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


    public class TallDecoratedLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Alta Decorada"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16ED; } }
        public int SuccessChance { get { return 80; } }

        public override int PlatformHeight { get { return 7; } }

        [Constructable]
        public TallDecoratedLargePlatform() : base(0x16ED)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            TallDecoratedLargePlatform item = new TallDecoratedLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public TallDecoratedLargePlatform(Serial serial) : base(serial) { }

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


    public class PolishedBrokenLargePlatform : BaseStatuePlatformItem, IPlatformRecipeProvider
    {
        public override string RecipeName { get { return "Plataforma Polida Velha"; } }
        public override StatuePlatformSize PlatformSize { get { return StatuePlatformSize.Large; } }
        public int PreviewItemID { get { return 0x16EE; } }
        public int SuccessChance { get { return 85; } }

        public override int PlatformHeight { get { return 6; } }

        [Constructable]
        public PolishedBrokenLargePlatform() : base(0x16EE)
        {
            Weight = StatuePlatformDefinitions.Get(StatuePlatformSize.Large).Weight;
        }

        public int GetMaterialCost(int materialId)
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0 : def.Cost;
        }

        public int GetPreviewBlockItemID()
        {
            StatuePlatformDefinition def = StatuePlatformDefinitions.Get(StatuePlatformSize.Large);
            return def == null ? 0x10B4 : def.PreviewBlockItemID;
        }

        public Item CreateItem(int materialId, bool withSign)
        {
            PolishedBrokenLargePlatform item = new PolishedBrokenLargePlatform();
            item.MaterialId = materialId;
            item.HasSign = withSign;
            return item;
        }

        public PolishedBrokenLargePlatform(Serial serial) : base(serial) { }

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
