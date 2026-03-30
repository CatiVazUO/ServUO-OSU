using System;
using System.Collections.Generic;
using System.Reflection;
using Server.Items;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public interface IPlatformRecipeProvider
    {
        string RecipeName { get; }
        StatuePlatformSize PlatformSize { get; }
        int ItemID { get; }
        int GetMaterialCost(int materialId);
        int GetPreviewBlockItemID();
        Item CreateItem(int materialId, bool withSign);
    }

    public interface ISculptureRecipeProvider
    {
        string RecipeName { get; }
        StatueCraftCategory Category { get; }
        int ItemID { get; }
        int GetMaterialCost(int materialId);
        int GetPreviewBlockItemID();
        double GetFinalWeight();
        Item CreateItem(int materialId);
    }

    public static class StatueRecipeRegistry
    {
        private static List<IPlatformRecipeProvider> m_Platforms;
        private static List<ISculptureRecipeProvider> m_Sculptures;

        public static List<IPlatformRecipeProvider> Platforms
        {
            get
            {
                if (m_Platforms == null)
                    Build();
                return m_Platforms;
            }
        }

        public static List<ISculptureRecipeProvider> Sculptures
        {
            get
            {
                if (m_Sculptures == null)
                    Build();
                return m_Sculptures;
            }
        }

        private static void Build()
        {
            m_Platforms = new List<IPlatformRecipeProvider>();
            m_Sculptures = new List<ISculptureRecipeProvider>();
            Assembly asm = Assembly.GetExecutingAssembly();
            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type t = types[i];
                if (t.IsAbstract)
                    continue;

                if (typeof(IPlatformRecipeProvider).IsAssignableFrom(t))
                {
                    try { m_Platforms.Add((IPlatformRecipeProvider)Activator.CreateInstance(t)); } catch { }
                }

                if (typeof(ISculptureRecipeProvider).IsAssignableFrom(t))
                {
                    try { m_Sculptures.Add((ISculptureRecipeProvider)Activator.CreateInstance(t)); } catch { }
                }
            }

            m_Platforms.Sort(delegate(IPlatformRecipeProvider a, IPlatformRecipeProvider b)
            {
                int c = a.PlatformSize.CompareTo(b.PlatformSize);
                return c != 0 ? c : string.Compare(a.RecipeName, b.RecipeName, StringComparison.OrdinalIgnoreCase);
            });

            m_Sculptures.Sort(delegate(ISculptureRecipeProvider a, ISculptureRecipeProvider b)
            {
                int c = a.Category.CompareTo(b.Category);
                return c != 0 ? c : string.Compare(a.RecipeName, b.RecipeName, StringComparison.OrdinalIgnoreCase);
            });
        }

        public static ISculptureRecipeProvider FindSculptureByKey(string recipeKey)
        {
            if (String.IsNullOrWhiteSpace(recipeKey))
                return null;

            for (int i = 0; i < Sculptures.Count; i++)
            {
                ILearnableSculptureRecipe learnable = Sculptures[i] as ILearnableSculptureRecipe;
                if (learnable != null && String.Equals(learnable.RecipeKey, recipeKey, StringComparison.OrdinalIgnoreCase))
                    return Sculptures[i];
            }

            return null;
        }

    }
}
