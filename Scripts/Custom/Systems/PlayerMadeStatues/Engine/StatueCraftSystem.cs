using System;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Skills.Abilities;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public static class StatueCraftSystem
    {
        public const int SmallStoneBlockSouth = 0x10B2;
        public const int SmallStoneBlockEast = 0x10B4;
        public const int LargeStoneBlock = 0x10B6;
        public const int ScaffoldEast = 0x12B4;
        public const int ScaffoldSouth = 0x12AD;

        public static bool HasSculptorAbility(PlayerMobile pm)
        {
            return pm != null && pm.HasOSUAbility(SculptorAbility.AbilityId);
        }

        public static bool HasSculptorIIAbility(PlayerMobile pm)
        {
            return pm != null && pm.HasOSUAbility(SculptorIIAbility.AbilityId);
        }

        public static bool CanUseTools(PlayerMobile pm, BaseTool tool)
        {
            if (pm == null || tool == null || tool.Deleted)
                return false;

            if (!HasSculptorAbility(pm))
            {
                pm.SendMessage("Você não possui a habilidade Escultor.");
                return false;
            }

            if (pm.Mounted)
            {
                pm.SendMessage("Você não pode esculpir montado.");
                return false;
            }

            if (tool.UsesRemaining <= 0)
            {
                pm.SendMessage("Essa ferramenta não possui mais usos.");
                tool.Delete();
                return false;
            }

            if (!tool.IsChildOf(pm.Backpack) && tool.Parent != pm)
            {
                pm.SendMessage("A ferramenta precisa estar com você.");
                return false;
            }

            return true;
        }

        public static bool HasSmallMaterial(PlayerMobile from, int materialId, int amount)
        {
            return HasMaterial(from, StatueMaterialOptions.GetSmallMaterialType(materialId), amount, StatueMaterialOptions.GetName(materialId));
        }

        public static bool HasLargeMaterial(PlayerMobile from, int materialId, int amount)
        {
            return HasMaterial(from, StatueMaterialOptions.GetLargeMaterialType(materialId), amount, StatueMaterialOptions.GetName(materialId));
        }

        public static bool ConsumeSmallMaterial(PlayerMobile from, int materialId, int amount)
        {
            return ConsumeMaterial(from, StatueMaterialOptions.GetSmallMaterialType(materialId), amount, StatueMaterialOptions.GetName(materialId));
        }

        public static bool ConsumeLargeMaterial(PlayerMobile from, int materialId, int amount)
        {
            return ConsumeMaterial(from, StatueMaterialOptions.GetLargeMaterialType(materialId), amount, StatueMaterialOptions.GetName(materialId));
        }

        public static BaseStatuePlatformItem FindPlatform(PlayerMobile from, StatuePlatformSize size)
        {
            if (from == null || from.Backpack == null)
                return null;

            Item[] items = from.Backpack.FindItemsByType(typeof(BaseStatuePlatformItem), true);

            for (int i = 0; i < items.Length; i++)
            {
                BaseStatuePlatformItem p = items[i] as BaseStatuePlatformItem;

                if (p != null && !p.Deleted && p.PlatformSize == size)
                    return p;
            }

            return null;
        }

        public static bool HasPlatform(PlayerMobile from, StatuePlatformSize size)
        {
            return FindPlatform(from, size) != null;
        }

        private static bool HasMaterial(PlayerMobile from, Type type, int amount, string name)
        {
            if (from == null || from.Backpack == null || type == null)
                return false;

            if (from.Backpack.GetAmount(type) < amount)
            {
                from.SendMessage("Você precisa de {0}x {1} para isso.", amount, name);
                return false;
            }

            return true;
        }

        private static bool ConsumeMaterial(PlayerMobile from, Type type, int amount, string name)
        {
            if (from == null || from.Backpack == null || type == null)
                return false;

            if (!from.Backpack.ConsumeTotal(type, amount))
            {
                from.SendMessage("Você precisa de {0}x {1} para isso.", amount, name);
                return false;
            }

            return true;
        }

        public static void ConsumeToolUse(BaseTool tool)
        {
            if (tool == null || tool.Deleted)
                return;
            tool.UsesRemaining--;
            if (tool.UsesRemaining <= 0)
                tool.Delete();
            else
                tool.InvalidateProperties();
        }

        public static void PlaySculptorEffect(Mobile from)
        {
            if (from == null)
                return;
            from.Animate(11, 5, 1, true, false, 0);
            from.PlaySound(0x2A);
        }

        public static Point3D GetPointInFront(Mobile from)
        {
            if (from == null || from.Map == null || from.Map == Map.Internal)
                return Point3D.Zero;

            Point3D p = from.Location;

            int x = p.X + 1;
            int y = p.Y;

            int low = 0;
            int avg = 0;
            int top = 0;
            from.Map.GetAverageZ(x, y, ref low, ref avg, ref top);

            return new Point3D(x, y, avg);
        }

        public static bool ValidateLiveModel(Mobile sculptor, Mobile model)
        {
            if (sculptor == null || model == null || model.Deleted)
                return false;
            if (!model.Alive)
                return false;
            if (!sculptor.InRange(model.Location, 7))
                return false;
            if (!sculptor.InLOS(model))
                return false;
            return true;
        }


        public static bool HasSculptingHeight(PlayerMobile pm)
        {
            return FindActiveScaffold(pm) != null;
        }

        public static bool IsOnScaffoldHeight(PlayerMobile pm, PlacedSculptorScaffold scaffold)
        {
            if (pm == null || scaffold == null || scaffold.Deleted || pm.Map != scaffold.Map)
                return false;

            return FindActiveScaffold(pm) == scaffold;
        }
        public static PlacedSculptorScaffold FindActiveScaffold(PlayerMobile pm)
        {
            if (pm == null || pm.Map == null || pm.Map == Map.Internal)
                return null;

            IPooledEnumerable eable = pm.Map.GetItemsInRange(pm.Location, 0);

            try
            {
                foreach (Item item in eable)
                {
                    PlacedSculptorScaffold scaffold = item as PlacedSculptorScaffold;

                    if (scaffold == null || scaffold.Deleted)
                        continue;

                    if (scaffold.X == pm.X && scaffold.Y == pm.Y && pm.Z == (scaffold.Z + 6))
                        return scaffold;
                }
            }
            finally
            {
                eable.Free();
            }

            return null;
        }

        public static bool IsSouthScaffold(PlayerMobile pm)
        {
            PlacedSculptorScaffold scaffold = FindActiveScaffold(pm);
            return scaffold != null && scaffold.Facing == StatueScaffoldFacing.South;
        }

        public static Direction GetScaffoldDirection(PlayerMobile pm)
        {
            return IsSouthScaffold(pm) ? Direction.South : Direction.East;
        }

        public static Point3D GetScaffoldWorkPoint(PlayerMobile pm, int distance)
        {
            if (pm == null || pm.Map == null || pm.Map == Map.Internal)
                return Point3D.Zero;

            int x = pm.X;
            int y = pm.Y;

            if (IsSouthScaffold(pm))
                y += distance;
            else
                x += distance;

            int low = 0;
            int avg = 0;
            int top = 0;
            pm.Map.GetAverageZ(x, y, ref low, ref avg, ref top);

            return new Point3D(x, y, avg);
        }
    }
}
