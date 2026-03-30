using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public enum OSUDefQualType
    {
        Defect = 1,
        Quality = 2
    }

    public abstract class OSUDefQualDefinition
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract OSUDefQualType Type { get; }
        public abstract int CapDelta { get; }
        public abstract string DescriptionHtml { get; }

        public virtual string[] BlocksIds => Array.Empty<string>();
        public virtual OSUCreationPath? RequiresPath => null;
        public virtual OSUCreationGameMode? RequiresGameMode => null;

        public virtual bool IsBlockedBySelection(HashSet<string> alreadySelected)
        {
            if (BlocksIds == null || BlocksIds.Length == 0 || alreadySelected == null)
                return false;

            for (int i = 0; i < BlocksIds.Length; i++)
            {
                if (alreadySelected.Contains(BlocksIds[i]))
                    return true;
            }

            return false;
        }

        public virtual int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            return currentMax;
        }

        public virtual int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            return currentMax;
        }
        public virtual bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            reason = null;

            if (RequiresPath.HasValue && ctx.Path != RequiresPath.Value)
            {
                reason = "Requer um caminho diferente.";
                return false;
            }

            if (RequiresGameMode.HasValue && ctx.GameMode != RequiresGameMode.Value)
            {
                reason = "Requer um modo de jogo diferente.";
                return false;
            }

            return true;
        }

        public virtual void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
        }

        public virtual void ApplyEffects(object player, OSUCreationContext ctx)
        {
        }

        // ===== Hooks de gameplay: deixe a regra no próprio Def/Qual =====

        public virtual int ModifyMaxWeight(PlayerMobile pm, int current) { return current; }
        public virtual int ModifyRunSpeed(PlayerMobile pm, int current, bool running) { return current; }
        public virtual int ModifyDisarmTrapBonus(PlayerMobile pm, int current) { return current; }
        public virtual double ModifySkillGainScalar(PlayerMobile pm, double current) { return current; }

        public virtual double ModifyHitsRegenRate(PlayerMobile pm, double currentSeconds) { return currentSeconds; }
        public virtual double ModifyStamRegenRate(PlayerMobile pm, double currentSeconds) { return currentSeconds; }
        public virtual double ModifyManaRegenRate(PlayerMobile pm, double currentSeconds) { return currentSeconds; }

        public virtual bool ShouldBreakMeditation(PlayerMobile pm) { return false; }

        public virtual bool BlocksOwnSpeech(PlayerMobile pm) { return false; }
        public virtual bool BlocksHearingSpeech(PlayerMobile listener, PlayerMobile speaker) { return false; }

        public virtual int ModifyFeatCap(PlayerMobile pm, int current) { return current; }

        public virtual int ModifyFeatCost(PlayerMobile pm, OSUFeatDefinition feat, int current) { return current; }

        public virtual double ModifyHungerRate(PlayerMobile pm, double current) { return current; }
        public virtual double ModifyThirstRate(PlayerMobile pm, double current) { return current; }

        public virtual int ModifyFinalBlowGold(PlayerMobile pm, int currentGold) { return currentGold; }

        public virtual int ModifyPoisonTickDamage(PlayerMobile pm, int currentDamage) { return currentDamage; }
        public virtual int ModifyBleedTickDamage(PlayerMobile pm, int currentDamage) { return currentDamage; }
        public virtual int ModifyColdClimateDamage(PlayerMobile pm, int currentDamage) { return currentDamage; }
        public virtual int ModifyHeatClimateDamage(PlayerMobile pm, int currentDamage) { return currentDamage; }

        public virtual double ModifyDiseaseSusceptibility(PlayerMobile pm, double current) { return current; }
        public virtual double ModifyPoisonSusceptibility(PlayerMobile pm, double current) { return current; }
        public virtual double ModifyBleedSusceptibility(PlayerMobile pm, double current) { return current; }
        public virtual double ModifyColdSusceptibility(PlayerMobile pm, double current) { return current; }
        public virtual double ModifyHeatSusceptibility(PlayerMobile pm, double current) { return current; }

        public virtual int ModifyUnconsciousDurationSeconds(PlayerMobile pm, int currentSeconds) { return currentSeconds; }
        public virtual double ModifyAddictionSusceptibility(PlayerMobile pm, double current) { return current; }

        public virtual bool CanReadAndWrite(PlayerMobile pm) { return true; }
        public virtual bool CanBuyLanguageSkills(PlayerMobile pm) { return true; }

        public virtual int ModifyStartingGold(PlayerMobile pm, int currentGold) { return currentGold; }
        public virtual double ModifyShrineBlessingScalar(PlayerMobile pm, double current) { return current; }
    }
}
