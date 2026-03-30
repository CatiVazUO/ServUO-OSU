using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.Mobiles;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public interface ILearnableSculptureRecipe
    {
        string RecipeKey { get; }
    }

    public static class SculptingLearnedRecipes
    {
        private const string TagPrefix = "OSU.SculptingRecipes.";

        private static string GetTagName(PlayerMobile from)
        {
            return from == null ? null : TagPrefix + from.Serial.Value.ToString();
        }

        private static HashSet<string> ReadKeys(PlayerMobile from)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (from == null)
                return set;

            Account acct = from.Account as Account;
            if (acct == null)
                return set;

            string raw = acct.GetTag(GetTagName(from));
            if (String.IsNullOrWhiteSpace(raw))
                return set;

            string[] parts = raw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string key = parts[i].Trim();
                if (!String.IsNullOrWhiteSpace(key))
                    set.Add(key);
            }

            return set;
        }

        private static void WriteKeys(PlayerMobile from, HashSet<string> set)
        {
            if (from == null)
                return;

            Account acct = from.Account as Account;
            if (acct == null)
                return;

            string[] keys = new string[set.Count];
            set.CopyTo(keys);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            acct.SetTag(GetTagName(from), String.Join(",", keys));
        }

        public static bool HasLearned(PlayerMobile from, string recipeKey)
        {
            if (from == null || String.IsNullOrWhiteSpace(recipeKey))
                return false;

            return ReadKeys(from).Contains(recipeKey);
        }

        public static bool Learn(PlayerMobile from, string recipeKey)
        {
            if (from == null || String.IsNullOrWhiteSpace(recipeKey))
                return false;

            HashSet<string> set = ReadKeys(from);
            if (!set.Add(recipeKey))
                return false;

            WriteKeys(from, set);
            return true;
        }

        public static bool IsRecipeVisible(PlayerMobile from, ISculptureRecipeProvider recipe)
        {
            ILearnableSculptureRecipe learnable = recipe as ILearnableSculptureRecipe;
            if (learnable == null)
                return true;

            return HasLearned(from, learnable.RecipeKey);
        }
    }
}
