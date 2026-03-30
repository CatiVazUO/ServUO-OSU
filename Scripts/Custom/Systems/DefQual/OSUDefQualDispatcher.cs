using Server.Mobiles;
using System;
using System.Collections.Generic;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.DefQual
{
    public static class OSUDefQualDispatcher
    {
        public static List<OSUDefQualDefinition> GetActive(PlayerMobile pm)
        {
            List<OSUDefQualDefinition> list = new List<OSUDefQualDefinition>();

            if (pm == null || pm.OSUDefQualFlags == null)
                return list;

            for (int i = 0; i < pm.OSUDefQualFlags.Count; i++)
            {
                string id = pm.OSUDefQualFlags[i];

                if (String.IsNullOrWhiteSpace(id))
                    continue;

                OSUDefQualDefinition def = OSUDefQualRegistry.GetById(id);

                if (def != null)
                    list.Add(def);
            }

            return list;
        }

        public static int ModifyMaxWeight(PlayerMobile pm, int current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyMaxWeight(pm, current);

            return current;
        }

        public static int ModifyRunSpeed(PlayerMobile pm, int current, bool running)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyRunSpeed(pm, current, running);

            return current;
        }

        public static int ModifyDisarmTrapBonus(PlayerMobile pm, int current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyDisarmTrapBonus(pm, current);

            return current;
        }

        public static double ModifySkillGainScalar(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifySkillGainScalar(pm, current);

            return current;
        }

        public static double ModifyHitsRegenRate(PlayerMobile pm, double currentSeconds)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentSeconds = defs[i].ModifyHitsRegenRate(pm, currentSeconds);

            return currentSeconds;
        }

        public static double ModifyStamRegenRate(PlayerMobile pm, double currentSeconds)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentSeconds = defs[i].ModifyStamRegenRate(pm, currentSeconds);

            return currentSeconds;
        }

        public static double ModifyManaRegenRate(PlayerMobile pm, double currentSeconds)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentSeconds = defs[i].ModifyManaRegenRate(pm, currentSeconds);

            return currentSeconds;
        }

        public static bool ShouldBreakMeditation(PlayerMobile pm)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
            {
                if (defs[i].ShouldBreakMeditation(pm))
                    return true;
            }

            return false;
        }

        public static bool BlocksOwnSpeech(PlayerMobile pm)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
            {
                if (defs[i].BlocksOwnSpeech(pm))
                    return true;
            }

            return false;
        }

        public static bool BlocksHearingSpeech(PlayerMobile listener, PlayerMobile speaker)
        {
            List<OSUDefQualDefinition> defs = GetActive(listener);

            for (int i = 0; i < defs.Count; i++)
            {
                if (defs[i].BlocksHearingSpeech(listener, speaker))
                    return true;
            }

            return false;
        }

        public static int ModifyFeatCap(PlayerMobile pm, int current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyFeatCap(pm, current);

            return current;
        }

        public static int ModifyFeatCost(PlayerMobile pm, OSUFeatDefinition feat, int current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyFeatCost(pm, feat, current);

            return current;
        }

        public static double ModifyHungerRate(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyHungerRate(pm, current);

            return current;
        }

        public static double ModifyThirstRate(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyThirstRate(pm, current);

            return current;
        }

        public static int ModifyFinalBlowGold(PlayerMobile pm, int currentGold)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentGold = defs[i].ModifyFinalBlowGold(pm, currentGold);

            return currentGold;
        }

        public static int ModifyPoisonTickDamage(PlayerMobile pm, int currentDamage)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentDamage = defs[i].ModifyPoisonTickDamage(pm, currentDamage);

            return currentDamage;
        }

        public static int ModifyBleedTickDamage(PlayerMobile pm, int currentDamage)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentDamage = defs[i].ModifyBleedTickDamage(pm, currentDamage);

            return currentDamage;
        }

        public static int ModifyColdClimateDamage(PlayerMobile pm, int currentDamage)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentDamage = defs[i].ModifyColdClimateDamage(pm, currentDamage);

            return currentDamage;
        }

        public static int ModifyHeatClimateDamage(PlayerMobile pm, int currentDamage)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentDamage = defs[i].ModifyHeatClimateDamage(pm, currentDamage);

            return currentDamage;
        }

        public static double ModifyDiseaseSusceptibility(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyDiseaseSusceptibility(pm, current);

            return current;
        }

        public static double ModifyPoisonSusceptibility(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyPoisonSusceptibility(pm, current);

            return current;
        }

        public static double ModifyBleedSusceptibility(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyBleedSusceptibility(pm, current);

            return current;
        }

        public static double ModifyColdSusceptibility(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyColdSusceptibility(pm, current);

            return current;
        }

        public static double ModifyHeatSusceptibility(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyHeatSusceptibility(pm, current);

            return current;
        }

        public static int ModifyUnconsciousDurationSeconds(PlayerMobile pm, int currentSeconds)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentSeconds = defs[i].ModifyUnconsciousDurationSeconds(pm, currentSeconds);

            return currentSeconds;
        }

        public static double ModifyAddictionSusceptibility(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyAddictionSusceptibility(pm, current);

            return current;
        }

        public static bool CanReadAndWrite(PlayerMobile pm)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
            {
                if (!defs[i].CanReadAndWrite(pm))
                    return false;
            }

            return true;
        }

        public static bool CanBuyLanguageSkills(PlayerMobile pm)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
            {
                if (!defs[i].CanBuyLanguageSkills(pm))
                    return false;
            }

            return true;
        }

        public static int ModifyStartingGold(PlayerMobile pm, int currentGold)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                currentGold = defs[i].ModifyStartingGold(pm, currentGold);

            return currentGold;
        }

        public static double ModifyShrineBlessingScalar(PlayerMobile pm, double current)
        {
            List<OSUDefQualDefinition> defs = GetActive(pm);

            for (int i = 0; i < defs.Count; i++)
                current = defs[i].ModifyShrineBlessingScalar(pm, current);

            return current;
        }
    }
}
