using System;
using System.Reflection;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public abstract class BaseSculptingRecipeScroll : Item
    {
        public abstract string RecipeKey { get; }

        public virtual string RecipeLabel
        {
            get
            {
                ISculptureRecipeProvider recipe = StatueRecipeRegistry.FindSculptureByKey(RecipeKey);
                return recipe != null ? recipe.RecipeName : "Receita de Escultura";
            }
        }

        public BaseSculptingRecipeScroll() : base(0x138B)
        {
            Weight = 1.0;
            LootType = LootType.Regular;
            Name = "Receita: " + RecipeLabel;
            Hue = 0;
        }

        public BaseSculptingRecipeScroll(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendMessage("A receita precisa estar na sua mochila.");
                return;
            }

            if (SculptingLearnedRecipes.HasLearned(pm, RecipeKey))
            {
                pm.SendMessage("Você já conhece essa receita.");
                return;
            }

            if (!SculptingLearnedRecipes.Learn(pm, RecipeKey))
            {
                pm.SendMessage("Não foi possível aprender essa receita agora.");
                return;
            }

            pm.SendMessage("Você aprende permanentemente a receita: {0}.", RecipeLabel);
            Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (String.IsNullOrWhiteSpace(Name))
                Name = "Receita: " + RecipeLabel;
        }
    }

    public class AncientGuardianStatueRecipeScroll : BaseSculptingRecipeScroll
    {
        public override string RecipeKey { get { return AncientGuardianStatueRecipe.StaticRecipeKey; } }

        [Constructable]
        public AncientGuardianStatueRecipeScroll() : base()
        {
        }

        public AncientGuardianStatueRecipeScroll(Serial serial) : base(serial)
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

    public class AncientGuardianStatueRecipe : BaseFinishedSculptureItem, ISculptureRecipeProvider, ILearnableSculptureRecipe
    {
        public static readonly string StaticRecipeKey = "large.ancient_guardian";

        public string RecipeKey { get { return StaticRecipeKey; } }
        public override string RecipeName { get { return "Estátua do Guardião Antigo"; } }
        public override StatueCraftCategory SculptureCategory { get { return StatueCraftCategory.Large; } }
        public StatueCraftCategory Category { get { return SculptureCategory; } }
        public int SuccessChance { get { return 86; } }

        [Constructable]
        public AncientGuardianStatueRecipe() : base(0x2D0F)
        {
            Weight = 130.0;
        }

        public int GetMaterialCost(int materialId)
        {
            return 12;
        }

        public int GetPreviewBlockItemID()
        {
            return 0x10B6;
        }

        public double GetFinalWeight()
        {
            return 130.0;
        }

        public Item CreateItem(int materialId)
        {
            AncientGuardianStatueRecipe item = new AncientGuardianStatueRecipe();
            item.MaterialId = materialId;
            return item;
        }

        public SculptorRequirement[] GetExtraRequirements(int materialId)
        {
            return new SculptorRequirement[]
            {
                new SculptorRequirement(typeof(IronWire), 11, "Arame de Ferro"),
                new SculptorRequirement(typeof(Board), 6, "Madeira"),
                new SculptorRequirement(ResolveType("Server.Items.Resin", "Resin"), 3, "Resina")
            };
        }

        private static Type ResolveType(string fullName, string shortName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Type t = assemblies[i].GetType(fullName, false);
                if (t != null)
                    return t;
            }

            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;

                try { types = assemblies[i].GetTypes(); }
                catch { continue; }

                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j] != null && String.Equals(types[j].Name, shortName, StringComparison.OrdinalIgnoreCase))
                        return types[j];
                }
            }

            return null;
        }

        public AncientGuardianStatueRecipe(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
