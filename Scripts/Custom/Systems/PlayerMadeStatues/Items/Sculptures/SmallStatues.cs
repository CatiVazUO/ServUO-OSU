using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class SimpleBustStatue : BaseFinishedSculptureItem, ISculptureRecipeProvider
    {
        public override string RecipeName { get { return "Busto Simples"; } }
        public override StatueCraftCategory SculptureCategory { get { return StatueCraftCategory.Small; } }
        public StatueCraftCategory Category { get { return SculptureCategory; } }
        public int PreviewItemID { get { return 0x1225; } }
        public int SuccessChance { get { return 95; } }
        public SculptorRequirement[] GetExtraRequirements(int materialId)
        {
            return new SculptorRequirement[]
            {
        new SculptorRequirement(typeof(IronWire), 8, "Arame de Ferro"),
        new SculptorRequirement(typeof(IronIngot), 2, "Ingots de Ferro")
            };
        }

        [Constructable]
        public SimpleBustStatue() : base(0x1225) { Weight = 20.0; }
        public int GetMaterialCost(int materialId) { return 2; }
        public int GetPreviewBlockItemID() { return 0x10B4; }
        public double GetFinalWeight() { return 20.0; }
        public Item CreateItem(int materialId) { SimpleBustStatue item = new SimpleBustStatue(); item.MaterialId = materialId; return item; }
        public SimpleBustStatue(Serial serial) : base(serial) { }

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

    public class KneelingFigureStatue : BaseFinishedSculptureItem, ISculptureRecipeProvider
    {
        public override string RecipeName { get { return "Figura Ajoelhada"; } }
        public override StatueCraftCategory SculptureCategory { get { return StatueCraftCategory.Small; } }
        public StatueCraftCategory Category { get { return SculptureCategory; } }
        public int PreviewItemID { get { return 0x139A; } }
        public int SuccessChance { get { return 90; } }

        [Constructable]
        public KneelingFigureStatue() : base(0x139A) { Weight = 25.0; }
        public int GetMaterialCost(int materialId) { return 3; }
        public int GetPreviewBlockItemID() { return 0x10B4; }
        public double GetFinalWeight() { return 25.0; }
        public Item CreateItem(int materialId) { KneelingFigureStatue item = new KneelingFigureStatue(); item.MaterialId = materialId; return item; }
        public KneelingFigureStatue(Serial serial) : base(serial) { }

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

    public class MaleBustStatue : BaseFinishedSculptureItem, ISculptureRecipeProvider
    {
        public override string RecipeName { get { return "Busto Homem"; } }
        public override StatueCraftCategory SculptureCategory { get { return StatueCraftCategory.Small; } }
        public StatueCraftCategory Category { get { return SculptureCategory; } }
        public int PreviewItemID { get { return 0x1226; } }
        public int SuccessChance { get { return 93; } }

        [Constructable]
        public MaleBustStatue() : base(0x1226) { Weight = 22.0; }
        public int GetMaterialCost(int materialId) { return 2; }
        public int GetPreviewBlockItemID() { return 0x10B4; }
        public double GetFinalWeight() { return 22.0; }
        public Item CreateItem(int materialId) { MaleBustStatue item = new MaleBustStatue(); item.MaterialId = materialId; return item; }
        public MaleBustStatue(Serial serial) : base(serial) { }

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
