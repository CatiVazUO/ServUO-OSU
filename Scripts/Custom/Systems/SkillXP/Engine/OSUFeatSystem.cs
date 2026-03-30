using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Custom.Systems.DefQual;

namespace Server.Custom.Systems.SkillXP.Engine
{
    public enum OSUFeatCategory
    {
        Combate = 0,
        Profissoes = 1
    }

    // ============================================================
    //  FEAT DEFINITION
    // ============================================================
    public class OSUFeatDefinition
    {
        public int Id;
        public SkillName Skill;
        public OSUFeatCategory Category;

        public string Name;
        public string Description;

        public int CostSkillXP;

        public string CommandName;   // ex: "disarm"
        public int IconID;

        // ✅ Requerimentos REAIS (por ID)
        public int RequiredFeatId;     // 0 = nenhum
        public int RequiredAbilityId;  // 0 = nenhum

        // ✅ Texto opcional pra UI (override)
        public string Requirement;

        public OSUFeatDefinition(
            int id,
            SkillName skill,
            string name,
            string desc,
            int costSkillXP,
            string commandName,
            int iconId,
            int requiredFeatId = 0,
            int requiredAbilityId = 0,
            string requirementTextOverride = "",
            OSUFeatCategory category = OSUFeatCategory.Combate)
        {
            Id = id;
            Skill = skill;
            Category = category;

            Name = name;
            Description = desc;

            CostSkillXP = costSkillXP;

            CommandName = commandName ?? "";
            IconID = iconId;

            RequiredFeatId = requiredFeatId;
            RequiredAbilityId = requiredAbilityId;

            Requirement = requirementTextOverride ?? "";
        }
    }

    // ============================================================
    //  FEAT SYSTEM
    // ============================================================
    public static class OSUFeatSystem
    {
        public const int OSUFeatTotalSpendCap = 80000;

        // Caps por categoria (invertível via PlayerMobile.OSUFeatCapsInverted)
        public const int OSUFeatCombatSpendCap = 50000;
        public const int OSUFeatProfessionSpendCap = 30000;

        private static readonly Dictionary<SkillName, List<OSUFeatDefinition>> _Feats =
            new Dictionary<SkillName, List<OSUFeatDefinition>>();

        // ✅ lookup por ID (pra requirement automático)
        private static readonly Dictionary<int, OSUFeatDefinition> _ById =
            new Dictionary<int, OSUFeatDefinition>();

        public static void Initialize()
        {
            OSUFeatRegistry.Initialize();
        }

        public static void AddFeat(OSUFeatDefinition feat)
        {
            if (feat == null)
                return;

            // evita duplicado global por Id
            if (_ById.ContainsKey(feat.Id))
                return;

            _ById[feat.Id] = feat;

            List<OSUFeatDefinition> list;
            if (!_Feats.TryGetValue(feat.Skill, out list))
            {
                list = new List<OSUFeatDefinition>();
                _Feats[feat.Skill] = list;
            }

            list.Add(feat);
        }

        public static OSUFeatDefinition GetFeatById(int id)
        {
            if (id <= 0)
                return null;

            foreach (var kv in _Feats)
            {
                var list = kv.Value;
                if (list == null) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var f = list[i];
                    if (f != null && f.Id == id)
                        return f;
                }
            }

            return null;
        }

        public static List<OSUFeatDefinition> GetFeats(SkillName skill)
        {
            List<OSUFeatDefinition> list;
            if (_Feats.TryGetValue(skill, out list) && list != null)
                return list;

            return new List<OSUFeatDefinition>();
        }

        public static List<OSUFeatDefinition> GetFeats(SkillName skill, OSUFeatCategory category)
        {
            List<OSUFeatDefinition> list = GetFeats(skill);
            if (list == null || list.Count == 0)
                return new List<OSUFeatDefinition>();

            List<OSUFeatDefinition> filtered = new List<OSUFeatDefinition>();

            for (int i = 0; i < list.Count; i++)
            {
                OSUFeatDefinition f = list[i];
                if (f != null && f.Category == category)
                    filtered.Add(f);
            }

            return filtered;
        }

        // ============================================================
        //  REQUIREMENT TEXT (para UI)
        // ============================================================
        public static string GetRequirementText(OSUFeatDefinition def)
        {
            if (def == null)
                return "";

            if (!string.IsNullOrEmpty(def.Requirement))
                return def.Requirement;

            List<string> parts = null;

            if (def.RequiredFeatId > 0)
            {
                OSUFeatDefinition reqFeat = GetFeatById(def.RequiredFeatId);
                string name = (reqFeat != null) ? reqFeat.Name : ("Feat " + def.RequiredFeatId);

                if (parts == null) parts = new List<string>();
                parts.Add(name);
            }

            if (def.RequiredAbilityId > 0)
            {
                OSUAbilityDefinition reqAb = OSUAbilitySystem.GetDefinitionById(def.RequiredAbilityId);
                string name = (reqAb != null) ? reqAb.Name : ("Ability " + def.RequiredAbilityId);

                if (parts == null) parts = new List<string>();
                parts.Add(name);
            }

            if (parts == null || parts.Count == 0)
                return "";

            return string.Join(", ", parts.ToArray());
        }

        // ============================================================
        //  COMPRA (validação)
        // ============================================================
        public static bool CanPurchase(PlayerMobile pm, OSUFeatDefinition feat, out string reason)
        {
            reason = null;

            if (pm == null || feat == null)
            {
                reason = "Erro interno.";
                return false;
            }

            if (pm.HasOSUFeat(feat.Id))
            {
                reason = "Você já comprou esta especialização.";
                return false;
            }

            // ✅ Requerimentos por IDs (regra real)
            if (feat.RequiredFeatId > 0 && !pm.HasOSUFeat(feat.RequiredFeatId))
            {
                OSUFeatDefinition req = GetFeatById(feat.RequiredFeatId);
                reason = "Você precisa comprar " + ((req != null) ? req.Name : ("Feat " + feat.RequiredFeatId)) + " primeiro.";
                return false;
            }

            if (feat.RequiredAbilityId > 0 && !pm.HasOSUAbility(feat.RequiredAbilityId))
            {
                OSUAbilityDefinition req = OSUAbilitySystem.GetDefinitionById(feat.RequiredAbilityId);
                reason = "Você precisa comprar " + ((req != null) ? req.Name : ("Ability " + feat.RequiredAbilityId)) + " primeiro.";
                return false;
            }

            int featCost = OSUDefQualDispatcher.ModifyFeatCost(pm, feat, feat.CostSkillXP);

            if (featCost < 1)
                featCost = 1;

            int xp = pm.GetSkillXP(feat.Skill);

            if (xp < featCost)
            {
                reason = "XP da skill insuficiente.";
                return false;
            }

            // ✅ cap total
            int totalCap = (pm.OSUFeatTotalCapCustom > 0) ? pm.OSUFeatTotalCapCustom : OSUFeatTotalSpendCap;

            if (pm.OSUFeatSpentXP + featCost > totalCap)
            {
                reason = "Você atingiu o limite total de XP gasto em especializações (" + totalCap + ").";
                return false;
            }

            int baseCombat = (pm.OSUFeatCombatCapCustom > 0) ? pm.OSUFeatCombatCapCustom : OSUFeatCombatSpendCap;
            int baseProf = (pm.OSUFeatProfessionCapCustom > 0) ? pm.OSUFeatProfessionCapCustom : OSUFeatProfessionSpendCap;

            int combatCap = pm.OSUFeatCapsInverted ? baseProf : baseCombat;
            int profCap = pm.OSUFeatCapsInverted ? baseCombat : baseProf;

            // ✅ cap por categoria (Combate / Profissões) - pode inverter depois via NPC
            //int combatCap = pm.OSUFeatCapsInverted ? OSUFeatProfessionSpendCap : OSUFeatCombatSpendCap;
            //int profCap   = pm.OSUFeatCapsInverted ? OSUFeatCombatSpendCap : OSUFeatProfessionSpendCap;

            if (feat.Category == OSUFeatCategory.Combate)
            {
                if (pm.OSUFeatSpentXPCombat + featCost > combatCap)
                {
                    reason = "Você atingiu o limite de XP gasto em especializações de combate (" + combatCap + ").";
                    return false;
                }
            }
            else
            {
                if (pm.OSUFeatSpentXPProf + featCost > profCap)
                {
                    reason = "Você atingiu o limite de XP gasto em especializações de profissão (" + profCap + ").";
                    return false;
                }
            }

            return true;
        }

        public static bool Purchase(PlayerMobile pm, OSUFeatDefinition feat, out string reason)
        {
            if (!CanPurchase(pm, feat, out reason))
                return false;

            int featCost = OSUDefQualDispatcher.ModifyFeatCost(pm, feat, feat.CostSkillXP);

            if (featCost < 1)
                featCost = 1;

            int xp = pm.GetSkillXP(feat.Skill);
            xp -= featCost;

            if (xp < 0)
                xp = 0;

            if (!pm.SkillXP.ContainsKey(feat.Skill))
                pm.SkillXP[feat.Skill] = 0;

            pm.SkillXP[feat.Skill] = xp;

            pm.AddOSUFeat(feat.Id);
            pm.OSUFeatSpentXP += featCost;

            if (feat.Category == OSUFeatCategory.Combate)
                pm.OSUFeatSpentXPCombat += featCost;
            else
                pm.OSUFeatSpentXPProf += featCost;

            reason = "Compra realizada!";
            return true;
        }
    }
}
