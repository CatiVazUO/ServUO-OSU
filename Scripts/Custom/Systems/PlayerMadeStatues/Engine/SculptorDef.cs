using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public class SculptorRequirement
    {
        private readonly Type m_Type;
        private readonly int m_Amount;
        private readonly string m_DisplayName;

        public Type ItemType
        {
            get { return m_Type; }
        }

        public int Amount
        {
            get { return m_Amount; }
        }

        public string DisplayName
        {
            get { return m_DisplayName; }
        }

        public SculptorRequirement(Type itemType, int amount, string displayName)
        {
            m_Type = itemType;
            m_Amount = amount;
            m_DisplayName = displayName;
        }
    }

    public static class SculptorDef
    {
        public static int GetPlatformSuccessChance(IPlatformRecipeProvider recipe, int materialId)
        {
            int value = ReadSuccessChance(recipe, materialId);
            return value > 0 ? value : 95;
        }

        public static int GetSculptureSuccessChance(ISculptureRecipeProvider recipe, int materialId)
        {
            int value = ReadSuccessChance(recipe, materialId);
            if (value > 0)
                return value;

            if (recipe == null)
                return 80;

            return recipe.Category == StatueCraftCategory.Small ? 92 : 87;
        }

        public static int GetLiveModelSuccessChance(StatueMobileProfile profile)
        {
            if (profile == null)
                return 80;

            return profile.SuccessChance > 0 ? profile.SuccessChance : 80;
        }

        public static SculptorRequirement[] GetExtraRequirements(object source, int materialId)
        {
            if (source == null)
                return new SculptorRequirement[0];

            if (source is StatueMobileProfile)
                return ((StatueMobileProfile)source).ExtraRequirements ?? new SculptorRequirement[0];

            Type t = source.GetType();

            try
            {
                MethodInfo mi = t.GetMethod("GetExtraRequirements", new Type[] { typeof(int) });
                if (mi != null)
                {
                    object val = mi.Invoke(source, new object[] { materialId });
                    SculptorRequirement[] direct = val as SculptorRequirement[];
                    if (direct != null)
                        return direct;

                    IEnumerable enumerable = val as IEnumerable;
                    if (enumerable != null)
                    {
                        List<SculptorRequirement> list = new List<SculptorRequirement>();
                        foreach (object o in enumerable)
                        {
                            SculptorRequirement req = o as SculptorRequirement;
                            if (req != null)
                                list.Add(req);
                        }
                        return list.ToArray();
                    }
                }
            }
            catch { }

            try
            {
                PropertyInfo pi = t.GetProperty("ExtraRequirements");
                if (pi != null && pi.CanRead)
                {
                    object val = pi.GetValue(source, null);
                    SculptorRequirement[] arr = val as SculptorRequirement[];
                    if (arr != null)
                        return arr;
                }
            }
            catch { }

            return new SculptorRequirement[0];
        }

        public static bool HasExtraRequirements(PlayerMobile from, object source, int materialId, bool mountedDouble)
        {
            SculptorRequirement[] reqs = GetExtraRequirements(source, materialId);
            if (reqs == null || reqs.Length == 0)
                return true;

            if (from == null || from.Backpack == null)
                return false;

            for (int i = 0; i < reqs.Length; i++)
            {
                SculptorRequirement req = reqs[i];
                if (req == null || req.ItemType == null || req.Amount <= 0)
                    continue;

                if (typeof(BaseStatuePlatformItem).IsAssignableFrom(req.ItemType))
                    continue;

                int needed = mountedDouble ? (req.Amount * 2) : req.Amount;

                if (from.Backpack.GetAmount(req.ItemType) < needed)
                {
                    from.SendMessage("Você precisa de {0}x {1} para isso.", needed, req.DisplayName);
                    return false;
                }
            }

            return true;
        }

        public static bool ConsumeExtraRequirements(PlayerMobile from, object source, int materialId, bool mountedDouble)
        {
            SculptorRequirement[] reqs = GetExtraRequirements(source, materialId);
            if (reqs == null || reqs.Length == 0)
                return true;

            if (from == null || from.Backpack == null)
                return false;

            for (int i = 0; i < reqs.Length; i++)
            {
                SculptorRequirement req = reqs[i];
                if (req == null || req.ItemType == null || req.Amount <= 0)
                    continue;

                if (typeof(BaseStatuePlatformItem).IsAssignableFrom(req.ItemType))
                    continue;

                int needed = mountedDouble ? (req.Amount * 2) : req.Amount;

                if (!from.Backpack.ConsumeTotal(req.ItemType, needed))
                {
                    from.SendMessage("Você precisa de {0}x {1} para isso.", needed, req.DisplayName);
                    return false;
                }
            }

            return true;
        }

        public static bool HasExtraRequirements(PlayerMobile from, object source, int materialId)
        {
            return HasExtraRequirements(from, source, materialId, false);
        }

        public static bool ConsumeExtraRequirements(PlayerMobile from, object source, int materialId)
        {
            return ConsumeExtraRequirements(from, source, materialId, false);
        }

        public static List<SculptorRequirement> BuildRequirementList(StatueMobileProfile profile, int materialId, bool mountedDouble)
        {
            List<SculptorRequirement> list = new List<SculptorRequirement>();
            if (profile == null)
                return list;

            int amount = mountedDouble ? (profile.RequiredResourceAmount * 2) : profile.RequiredResourceAmount;

            list.Add(new SculptorRequirement(
                StatueMaterialOptions.GetLargeMaterialType(materialId),
                amount,
                StatueMaterialOptions.GetName(materialId)));

            SculptorRequirement[] extras = profile.ExtraRequirements ?? new SculptorRequirement[0];
            for (int i = 0; i < extras.Length; i++)
            {
                if (extras[i] == null)
                    continue;

                int extraAmount = mountedDouble ? (extras[i].Amount * 2) : extras[i].Amount;
                list.Add(new SculptorRequirement(extras[i].ItemType, extraAmount, extras[i].DisplayName));
            }

            StatuePlatformSize size = mountedDouble ? StatuePlatformSize.Giant : profile.PlatformSize;

            if (size != StatuePlatformSize.None)
            {
                string platformName = "Plataforma";

                switch (size)
                {
                    case StatuePlatformSize.Small: platformName = "Plataforma Pequena"; break;
                    case StatuePlatformSize.Medium: platformName = "Plataforma Média"; break;
                    case StatuePlatformSize.Large: platformName = "Plataforma Grande"; break;
                    case StatuePlatformSize.Giant: platformName = "Plataforma Gigante"; break;
                    case StatuePlatformSize.XXL: platformName = "Plataforma XXL"; break;
                }

                list.Add(new SculptorRequirement(null, 1, platformName + " (selecionada no chão)"));
            }

            return list;
        }

        public static List<SculptorRequirement> BuildRequirementList(IPlatformRecipeProvider recipe, int materialId)
        {
            List<SculptorRequirement> list = new List<SculptorRequirement>();
            if (recipe == null)
                return list;

            list.Add(new SculptorRequirement(StatueMaterialOptions.GetSmallMaterialType(materialId), recipe.GetMaterialCost(materialId), StatueMaterialOptions.GetName(materialId)));
            AppendExtras(list, GetExtraRequirements(recipe, materialId));
            return list;
        }

        public static List<SculptorRequirement> BuildRequirementList(ISculptureRecipeProvider recipe, int materialId)
        {
            List<SculptorRequirement> list = new List<SculptorRequirement>();
            if (recipe == null)
                return list;

            Type baseType = recipe.Category == StatueCraftCategory.Small ? StatueMaterialOptions.GetSmallMaterialType(materialId) : StatueMaterialOptions.GetLargeMaterialType(materialId);
            list.Add(new SculptorRequirement(baseType, recipe.GetMaterialCost(materialId), StatueMaterialOptions.GetName(materialId)));
            AppendExtras(list, GetExtraRequirements(recipe, materialId));
            return list;
        }

        public static List<SculptorRequirement> BuildRequirementList(StatueMobileProfile profile, int materialId)
        {
            List<SculptorRequirement> list = new List<SculptorRequirement>();
            if (profile == null)
                return list;

            list.Add(new SculptorRequirement(
                StatueMaterialOptions.GetLargeMaterialType(materialId),
                profile.RequiredResourceAmount,
                StatueMaterialOptions.GetName(materialId)));

            AppendExtras(list, profile.ExtraRequirements);

            if (profile.PlatformSize != StatuePlatformSize.None)
            {
                string platformName = "Plataforma";

                switch (profile.PlatformSize)
                {
                    case StatuePlatformSize.Small:
                        platformName = "Plataforma Pequena";
                        break;
                    case StatuePlatformSize.Medium:
                        platformName = "Plataforma Média";
                        break;
                    case StatuePlatformSize.Large:
                        platformName = "Plataforma Grande";
                        break;
                    case StatuePlatformSize.Giant:
                        platformName = "Plataforma Gigante";
                        break;
                    case StatuePlatformSize.XXL:
                        platformName = "Plataforma XXL";
                        break;
                }

                list.Add(new SculptorRequirement(null, 1, platformName + " (selecionada no chão)"));
            }

            return list;
        }

        private static void AppendExtras(List<SculptorRequirement> list, SculptorRequirement[] extras)
        {
            if (extras == null)
                return;

            for (int i = 0; i < extras.Length; i++)
            {
                if (extras[i] != null)
                    list.Add(extras[i]);
            }
        }

        private static int ReadSuccessChance(object source, int materialId)
        {
            if (source == null)
                return 0;

            Type t = source.GetType();
            try
            {
                MethodInfo mi = t.GetMethod("GetSuccessChance", new Type[] { typeof(int) });
                if (mi != null)
                {
                    object val = mi.Invoke(source, new object[] { materialId });
                    if (val is int)
                        return (int)val;
                }
            }
            catch { }

            try
            {
                PropertyInfo pi = t.GetProperty("SuccessChance");
                if (pi != null && pi.CanRead)
                {
                    object val = pi.GetValue(source, null);
                    if (val is int)
                        return (int)val;
                }
            }
            catch { }

            return 0;
        }
    }
}
