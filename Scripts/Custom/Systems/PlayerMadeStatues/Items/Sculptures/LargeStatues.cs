using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class GenericLargeStatue : BaseFinishedSculptureItem, ISculptureRecipeProvider
    {
        public override string RecipeName { get { return "Estátua Grande"; } }
        public override StatueCraftCategory SculptureCategory { get { return StatueCraftCategory.Large; } }
        public StatueCraftCategory Category { get { return SculptureCategory; } }
        public int ItePreviewItemIDmID { get { return 0x1227; } }
        public int SuccessChance { get { return 87; } }

        [Constructable]
        public GenericLargeStatue() : base(0x1227) { Weight = 130.0; }
        public int GetMaterialCost(int materialId) { return 4; }
        public int GetPreviewBlockItemID() { return 0x10B6; }
        public double GetFinalWeight() { return 130.0; }
        public Item CreateItem(int materialId) { GenericLargeStatue item = new GenericLargeStatue(); item.MaterialId = materialId; return item; }
        public GenericLargeStatue(Serial serial) : base(serial) { }

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
