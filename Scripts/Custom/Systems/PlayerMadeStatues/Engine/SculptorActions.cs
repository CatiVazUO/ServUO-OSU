using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Multis;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public static class SculptorActions
    {
        public static bool TryGetLiveModelData(PlayerMobile from, Mobile model, out StatueMobileProfile profile, out string message)
        {
            profile = null;
            message = null;

            if (from == null || model == null)
            {
                message = "Alvo inválido.";
                return false;
            }

            if (!StatueCraftSystem.HasSculptingHeight(from))
            {
                message = "Você precisa estar mais alto pra escupir uma escultura desse porte.";
                return false;
            }

            if (!StatueCraftSystem.ValidateLiveModel(from, model))
            {
                message = "Esse alvo não pode ser usado agora como modelo vivo.";
                return false;
            }

            profile = StatueMobileProfileReader.GetFrom(model);
            if (profile == null || !profile.IsValid)
            {
                message = "Não é possível usar esse mobile como modelo vivo.";
                return false;
            }

            return true;
        }

        private static Point3D GetPlatformWorkLocation(PlayerMobile from, StatuePlatformSize size)
        {
            return StatueCraftSystem.GetScaffoldWorkPoint(from, 2);
        }

        private static Point3D GetLargeStatueWorkLocation(PlayerMobile from)
        {
            return StatueCraftSystem.GetScaffoldWorkPoint(from, 2);
        }

        private static Point3D GetLiveModelWorkLocation(PlayerMobile from, BaseStatuePlatformItem platform, int statueZ)
        {
            Point3D p = GetLargeStatueWorkLocation(from);

            if (platform != null && !platform.Deleted)
                return new Point3D(p.X, p.Y, statueZ);

            return p;
        }

        private static void ForceFacingFromScaffold(PlayerMobile from)
        {
            if (from == null)
                return;

            from.Direction = StatueCraftSystem.GetScaffoldDirection(from);
        }

        private static bool IsOwnedRentalTile(PlayerMobile from, Point3D loc)
        {
            if (from == null || from.Map == null || from.Map == Map.Internal)
                return false;

            BaseHouse house = BaseHouse.FindHouseAt(loc, from.Map, 16);

            if (house == null)
                return false;

            return house.IsOwner(from);
        }

        private static bool CanBuildPlatformHere(PlayerMobile from)
        {
            if (from == null || from.Map == null || from.Map == Map.Internal)
                return false;

            Point3D work = GetPlatformWorkLocation(from, StatuePlatformSize.Small);

            if (!IsOwnedRentalTile(from, from.Location))
                return false;

            if (!IsOwnedRentalTile(from, work))
                return false;

            return true;
        }

        private static bool PlatformAcceptsStatue(StatuePlatformSize platformSize, StatuePlatformSize requiredSize)
        {
            return platformSize >= requiredSize;
        }

        private static bool TryValidateTargetPlatform(PlayerMobile from, object targeted, StatuePlatformSize requiredSize, out BaseStatuePlatformItem platform, out string message)
        {
            platform = targeted as BaseStatuePlatformItem;
            message = null;

            if (platform == null || platform.Deleted)
            {
                message = "Isso não é uma plataforma válida.";
                return false;
            }

            if (from == null || from.Map == null || platform.Map != from.Map)
            {
                message = "Essa plataforma não pode ser usada agora.";
                return false;
            }

            if (!from.InRange(platform.GetWorldLocation(), 12) || !from.InLOS(platform))
            {
                message = "A plataforma precisa estar perto de você e em linha de visão.";
                return false;
            }

            if (!PlatformAcceptsStatue(platform.PlatformSize, requiredSize))
            {
                message = "A estátua desse modelo não caberia nessa plataforma.";
                return false;
            }

            return true;
        }

        private static bool CheckPlatformRecipeResources(PlayerMobile from, IPlatformRecipeProvider recipe, int materialId)
        {
            if (!StatueCraftSystem.HasSmallMaterial(from, materialId, recipe.GetMaterialCost(materialId)))
                return false;

            if (!SculptorDef.HasExtraRequirements(from, recipe, materialId))
                return false;

            return true;
        }

        private static bool CheckSculptureRecipeResources(PlayerMobile from, ISculptureRecipeProvider recipe, int materialId)
        {
            if (recipe.Category == StatueCraftCategory.Small)
            {
                if (!StatueCraftSystem.HasSmallMaterial(from, materialId, recipe.GetMaterialCost(materialId)))
                    return false;
            }
            else
            {
                if (!StatueCraftSystem.HasLargeMaterial(from, materialId, recipe.GetMaterialCost(materialId)))
                    return false;
            }

            if (!SculptorDef.HasExtraRequirements(from, recipe, materialId))
                return false;

            return true;
        }

        private static bool CheckLiveModelResources(PlayerMobile from, Mobile model, StatueMobileProfile profile, int materialId)
        {
            int amount = profile.RequiredResourceAmount;

            if (model != null && model.Mounted)
                amount *= 2;

            if (!StatueCraftSystem.HasLargeMaterial(from, materialId, amount))
                return false;

            if (!SculptorDef.HasExtraRequirements(from, profile, materialId, model != null && model.Mounted))
                return false;

            return true;
        }

        private static bool ConsumePlatformRecipeResources(PlayerMobile from, IPlatformRecipeProvider recipe, int materialId)
        {
            if (!StatueCraftSystem.ConsumeSmallMaterial(from, materialId, recipe.GetMaterialCost(materialId)))
                return false;

            if (!SculptorDef.ConsumeExtraRequirements(from, recipe, materialId))
                return false;

            return true;
        }

        private static bool ConsumeSculptureRecipeResources(PlayerMobile from, ISculptureRecipeProvider recipe, int materialId)
        {
            if (recipe.Category == StatueCraftCategory.Small)
            {
                if (!StatueCraftSystem.ConsumeSmallMaterial(from, materialId, recipe.GetMaterialCost(materialId)))
                    return false;
            }
            else
            {
                if (!StatueCraftSystem.ConsumeLargeMaterial(from, materialId, recipe.GetMaterialCost(materialId)))
                    return false;
            }

            if (!SculptorDef.ConsumeExtraRequirements(from, recipe, materialId))
                return false;

            return true;
        }

        private static bool ConsumeLiveModelResources(PlayerMobile from, Mobile model, StatueMobileProfile profile, int materialId)
        {
            int amount = profile.RequiredResourceAmount;

            if (model != null && model.Mounted)
                amount *= 2;

            if (!StatueCraftSystem.ConsumeLargeMaterial(from, materialId, amount))
                return false;

            if (!SculptorDef.ConsumeExtraRequirements(from, profile, materialId, model != null && model.Mounted))
                return false;

            return true;
        }

        private static bool RollSuccess(PlayerMobile from, int chance)
        {
            if (chance < 0)
                chance = 0;
            if (chance > 100)
                chance = 100;

            bool success = Utility.Random(100) < chance;
            if (!success && from != null)
                from.SendMessage("Você falha ao concluir o trabalho. Os materiais são perdidos.");
            return success;
        }

        public static void BeginPlatform(PlayerMobile from, SculptorTools tool, IPlatformRecipeProvider recipe, int materialId, bool withSign)
        {
            if (!StatueCraftSystem.CanUseTools(from, tool))
                return;

            if (recipe == null)
            {
                from.SendMessage("Receita de plataforma inválida.");
                return;
            }

            if (!CanBuildPlatformHere(from))
            {
                from.SendMessage("Você só pode construir plataformas em uma área alugada que pertença a você.");
                return;
            }

            if (!CheckPlatformRecipeResources(from, recipe, materialId))
                return;

            if (!ConsumePlatformRecipeResources(from, recipe, materialId))
                return;

            ForceFacingFromScaffold(from);

            UnfinishedStatueBlock block = new UnfinishedStatueBlock(recipe.GetPreviewBlockItemID(), StatueMaterialOptions.GetHue(materialId));
            block.Sculptor = from;
            block.Category = StatueCraftCategory.Platform;
            block.MaterialId = materialId;
            block.PlannedName = recipe.RecipeName;
            block.MoveToWorld(GetPlatformWorkLocation(from, recipe.PlatformSize), from.Map);

            from.Frozen = true;
            from.CantWalk = true;

            TimeSpan duration = TimeSpan.FromSeconds(10.0);
            new SculptWorkTimer(from, duration).Start();

            Timer.DelayCall(duration, delegate
            {
                if (from != null && !from.Deleted)
                {
                    from.Frozen = false;
                    from.CantWalk = false;
                }

                if (block == null || block.Deleted)
                    return;

                if (!RollSuccess(from, SculptorDef.GetPlatformSuccessChance(recipe, materialId)))
                {
                    block.Delete();
                    StatueCraftSystem.ConsumeToolUse(tool);
                    return;
                }

                Item finished = recipe.CreateItem(materialId, withSign);
                if (finished == null)
                {
                    block.Delete();
                    return;
                }

                finished.MoveToWorld(block.Location, block.Map);
                block.Delete();
                StatueCraftSystem.ConsumeToolUse(tool);
            });
        }

        public static void BeginSmall(PlayerMobile from, SculptorTools tool, ISculptureRecipeProvider recipe, int materialId)
        {
            if (!StatueCraftSystem.CanUseTools(from, tool))
                return;

            if (recipe == null)
            {
                from.SendMessage("Receita inválida.");
                return;
            }

            if (!CheckSculptureRecipeResources(from, recipe, materialId))
                return;

            if (!ConsumeSculptureRecipeResources(from, recipe, materialId))
                return;

            ForceFacingFromScaffold(from);

            UnfinishedStatueBlock block = new UnfinishedStatueBlock(recipe.GetPreviewBlockItemID(), StatueMaterialOptions.GetHue(materialId));
            block.Sculptor = from;
            block.Category = StatueCraftCategory.Small;
            block.MaterialId = materialId;
            block.PlannedName = recipe.RecipeName;
            block.MoveToWorld(GetLargeStatueWorkLocation(from), from.Map);

            from.Frozen = true;
            from.CantWalk = true;

            TimeSpan duration = TimeSpan.FromSeconds(10.0);
            new SculptWorkTimer(from, duration).Start();

            Timer.DelayCall(duration, delegate
            {
                if (from != null && !from.Deleted)
                {
                    from.Frozen = false;
                    from.CantWalk = false;
                }

                if (block == null || block.Deleted)
                    return;

                if (!RollSuccess(from, SculptorDef.GetSculptureSuccessChance(recipe, materialId)))
                {
                    block.Delete();
                    StatueCraftSystem.ConsumeToolUse(tool);
                    return;
                }

                Item finished = recipe.CreateItem(materialId);
                if (finished == null)
                {
                    block.Delete();
                    return;
                }

                finished.MoveToWorld(block.Location, block.Map);
                block.Delete();
                StatueCraftSystem.ConsumeToolUse(tool);
            });
        }

        public static void BeginLarge(PlayerMobile from, SculptorTools tool, ISculptureRecipeProvider recipe, int materialId)
        {
            if (!StatueCraftSystem.CanUseTools(from, tool))
                return;

            if (!StatueCraftSystem.HasSculptingHeight(from))
            {
                from.SendMessage("Você precisa estar mais alto pra escupir uma escultura desse porte.");
                return;
            }

            if (recipe == null)
            {
                from.SendMessage("Receita inválida.");
                return;
            }

            if (!CheckSculptureRecipeResources(from, recipe, materialId))
                return;

            if (!ConsumeSculptureRecipeResources(from, recipe, materialId))
                return;

            ForceFacingFromScaffold(from);

            UnfinishedStatueBlock block = new UnfinishedStatueBlock(recipe.GetPreviewBlockItemID(), StatueMaterialOptions.GetHue(materialId));
            block.Sculptor = from;
            block.Category = StatueCraftCategory.Large;
            block.MaterialId = materialId;
            block.PlannedName = recipe.RecipeName;
            block.MoveToWorld(GetLargeStatueWorkLocation(from), from.Map);

            from.Frozen = true;
            from.CantWalk = true;

            TimeSpan duration = TimeSpan.FromSeconds(30.0);
            new SculptWorkTimer(from, duration).Start();

            Timer.DelayCall(duration, delegate
            {
                if (from != null && !from.Deleted)
                {
                    from.Frozen = false;
                    from.CantWalk = false;
                }

                if (block == null || block.Deleted)
                    return;

                if (!RollSuccess(from, SculptorDef.GetSculptureSuccessChance(recipe, materialId)))
                {
                    block.Delete();
                    StatueCraftSystem.ConsumeToolUse(tool);
                    return;
                }

                Item finished = recipe.CreateItem(materialId);
                if (finished == null)
                {
                    block.Delete();
                    return;
                }

                finished.MoveToWorld(block.Location, block.Map);
                block.Delete();
                StatueCraftSystem.ConsumeToolUse(tool);
            });
        }

        public static void BeginLiveModel(PlayerMobile from, SculptorTools tool, int materialId)
        {
            if (!StatueCraftSystem.CanUseTools(from, tool))
                return;

            if (!StatueCraftSystem.HasSculptorIIAbility(from))
            {
                from.SendMessage("Você precisa da habilidade Esculpir II para esculpir a partir de modelos vivos.");
                return;
            }

            if (!StatueCraftSystem.HasSculptingHeight(from))
            {
                from.SendMessage("Você precisa estar mais alto pra escupir uma escultura desse porte.");
                return;
            }

            from.SendMessage("Escolha o modelo vivo.");
            from.Target = new LiveModelTarget(tool, materialId);
        }

        public static void BeginLiveModelSelected(PlayerMobile from, SculptorTools tool, int materialId, Mobile model, BaseStatuePlatformItem platform)
        {
            StatueMobileProfile profile;
            string message;

            if (!StatueCraftSystem.HasSculptorIIAbility(from))
            {
                from.SendMessage("Você precisa da habilidade Esculpir II para esculpir a partir de modelos vivos.");
                return;
            }

            if (!StatueCraftSystem.CanUseTools(from, tool))
                return;

            if (!TryGetLiveModelData(from, model, out profile, out message))
            {
                if (!string.IsNullOrEmpty(message))
                    from.SendMessage(message);
                return;
            }

            StatuePlatformSize requiredPlatformSize = profile.PlatformSize;

            if (model != null && model.Mounted)
                requiredPlatformSize = StatuePlatformSize.Large;

            if (!TryValidateTargetPlatform(from, platform, requiredPlatformSize, out platform, out message))
            {
                if (!string.IsNullOrEmpty(message))
                    from.SendMessage(message);
                return;
            }

            if (!CheckLiveModelResources(from, model, profile, materialId))
                return;

            if (!ConsumeLiveModelResources(from, model, profile, materialId))
                return;

            int statueZ = platform.Z + platform.PlatformHeight;

            ForceFacingFromScaffold(from);

            UnfinishedStatueBlock block = new UnfinishedStatueBlock(StatueCraftSystem.LargeStoneBlock, StatueMaterialOptions.GetHue(materialId));
            block.Sculptor = from;
            block.Category = StatueCraftCategory.LiveModel;
            block.MaterialId = materialId;
            block.PlannedName = "estátua de " + (model.Name ?? "modelo");
            block.MoveToWorld(GetLiveModelWorkLocation(from, platform, statueZ), from.Map);

            from.Frozen = true;
            from.CantWalk = true;

            TimeSpan duration = TimeSpan.FromMinutes(1.0);
            new SculptWorkTimer(from, duration).Start();

            Timer.DelayCall(duration, delegate
            {
                if (from != null && !from.Deleted)
                {
                    from.Frozen = false;
                    from.CantWalk = false;
                }

                StatueCraftSystem.ConsumeToolUse(tool);

                if (block == null || block.Deleted)
                    return;

                if (!StatueCraftSystem.ValidateLiveModel(from, model))
                {
                    from.SendMessage("O modelo vivo não permaneceu válido até o fim. Os materiais foram perdidos.");
                    block.Delete();
                    return;
                }

                if (platform == null || platform.Deleted || platform.Map != from.Map)
                {
                    from.SendMessage("A plataforma escolhida não está mais disponível.");
                    block.Delete();
                    return;
                }

                if (!TryValidateTargetPlatform(from, platform, requiredPlatformSize, out platform, out message))
                {
                    from.SendMessage(message ?? "A plataforma escolhida não pode receber essa estátua.");
                    block.Delete();
                    return;
                }

                if (!RollSuccess(from, SculptorDef.GetLiveModelSuccessChance(profile)))
                {
                    block.Delete();
                    return;
                }

                LiveModelStatue statue = new LiveModelStatue(model, from, materialId, profile.Poses, requiredPlatformSize, profile.PlatformZOffset, platform);
                statue.MoveToWorld(new Point3D(platform.X, platform.Y, statueZ), platform.Map);
                block.Delete();
            });
        }

        private class SculptWorkTimer : Timer
        {
            private readonly PlayerMobile m_From;
            private readonly DateTime m_End;

            public SculptWorkTimer(PlayerMobile from, TimeSpan duration)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(5.0))
            {
                m_From = from;
                m_End = DateTime.UtcNow + duration;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                if (m_From == null || m_From.Deleted)
                {
                    Stop();
                    return;
                }

                if (DateTime.UtcNow >= m_End)
                {
                    Stop();
                    return;
                }

                StatueCraftSystem.PlaySculptorEffect(m_From);
            }
        }

        private class LiveModelTarget : Target
        {
            private readonly SculptorTools m_Tool;
            private readonly int m_MaterialId;

            public LiveModelTarget(SculptorTools tool, int materialId) : base(7, false, TargetFlags.None)
            {
                m_Tool = tool;
                m_MaterialId = materialId;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                Mobile model = targeted as Mobile;
                if (pm == null || model == null)
                    return;

                pm.SendMessage("Agora escolha a plataforma pronta onde a estátua será colocada.");
                pm.Target = new LiveModelPlatformTarget(m_Tool, m_MaterialId, model);
            }
        }

        private class LiveModelPlatformTarget : Target
        {
            private readonly SculptorTools m_Tool;
            private readonly int m_MaterialId;
            private readonly Mobile m_Model;

            public LiveModelPlatformTarget(SculptorTools tool, int materialId, Mobile model) : base(12, false, TargetFlags.None)
            {
                m_Tool = tool;
                m_MaterialId = materialId;
                m_Model = model;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                BaseStatuePlatformItem platform = targeted as BaseStatuePlatformItem;

                if (pm == null)
                    return;

                if (platform == null)
                {
                    pm.SendMessage("Isso não é uma plataforma válida.");
                    return;
                }

                BeginLiveModelSelected(pm, m_Tool, m_MaterialId, m_Model, platform);
            }
        }
    }
}
