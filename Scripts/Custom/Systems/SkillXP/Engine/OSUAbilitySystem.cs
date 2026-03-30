using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Custom.Systems.DefQual;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.SkillXP.Engine
{
    // ============================================================
    //  ABILITY DEFINITION (catálogo exibido no gump)
    // ============================================================
    public class OSUAbilityDefinition
    {
        public int Id;
        public string Name;
        public string Description;

        public int CostPicks;        // custo em OSUAbilityPicks
        public string CommandText;   // "" se passiva
        public int IconID;           // 0 se passiva

        // ✅ Requerimentos REAIS (por ID)
        public int RequiredAbilityId; // 0 = nenhum
        public int RequiredFeatId;    // 0 = nenhum

        // ✅ Mantemos também um "Requirement" string opcional, mas agora é só fallback/override de UI
        // Se você quiser forçar um texto específico, pode preencher.
        public string Requirement;

        // ✅ NOVO CTOR COMPLETO
        public OSUAbilityDefinition(
            int id,
            string name,
            string desc,
            int costPicks,
            string commandText,
            int iconId,
            int requiredAbilityId = 0,
            int requiredFeatId = 0,
            string requirementTextOverride = "")
        {
            Id = id;
            Name = name;
            Description = desc;

            CostPicks = costPicks;
            CommandText = commandText ?? "";
            IconID = iconId;

            RequiredAbilityId = requiredAbilityId;
            RequiredFeatId = requiredFeatId;
            Requirement = requirementTextOverride ?? "";
        }

        // ✅ CTOR LEGADO (pra não quebrar seus arquivos antigos que tinham: requirement string)
        public OSUAbilityDefinition(int id, string name, string desc, int costPicks, string requirement, int icon)
            : this(id, name, desc, costPicks, "", icon, 0, 0, reminder(requirement))
        {
        }

        private static string reminder(string requirement)
        {
            return requirement ?? "";
        }
    }

    // ============================================================
    //  ABILITY SYSTEM (registry + compra)
    // ============================================================
    public static class OSUAbilitySystem
    {
        public static void Initialize()
        {
            OSUAbilityRegistry.RegisterAll();
        }

        private static readonly List<IOSUAbility> _Abilities = new List<IOSUAbility>();
        private static readonly Dictionary<int, IOSUAbility> _ById = new Dictionary<int, IOSUAbility>();

        public static void AddAbility(IOSUAbility ability)
        {
            if (ability == null || ability.Definition == null)
                return;

            int id = ability.Definition.Id;

            if (_ById.ContainsKey(id))
                return;

            _Abilities.Add(ability);
            _ById[id] = ability;

            IOSUAbilityModule module = ability as IOSUAbilityModule;
            if (module != null)
                module.InitializeModule();
        }

        public static List<OSUAbilityDefinition> GetAll()
        {
            List<OSUAbilityDefinition> list = new List<OSUAbilityDefinition>();

            for (int i = 0; i < _Abilities.Count; i++)
            {
                IOSUAbility a = _Abilities[i];
                if (a != null && a.Definition != null)
                    list.Add(a.Definition);
            }

            return list;
        }

        public static OSUAbilityDefinition GetByIndex(int index)
        {
            if (index < 0 || index >= _Abilities.Count)
                return null;

            return _Abilities[index].Definition;
        }

        public static IOSUAbility GetAbilityByIndex(int index)
        {
            if (index < 0 || index >= _Abilities.Count)
                return null;

            return _Abilities[index];
        }

        public static IOSUAbility GetAbilityById(int id)
        {
            IOSUAbility ab;
            _ById.TryGetValue(id, out ab);
            return ab;
        }

        public static OSUAbilityDefinition GetDefinitionById(int id)
        {
            IOSUAbility ab;
            if (_ById.TryGetValue(id, out ab) && ab != null)
                return ab.Definition;

            return null;
        }

        // ============================================================
        //  REQUIREMENT TEXT (para UI)
        //  - se Requirement string estiver preenchido -> usa ele
        //  - senão monta automaticamente pelo(s) IDs
        // ============================================================
        public static string GetRequirementText(OSUAbilityDefinition def)
        {
            if (def == null)
                return "";

            if (!string.IsNullOrEmpty(def.Requirement))
                return def.Requirement;

            List<string> parts = null;

            if (def.RequiredAbilityId > 0)
            {
                OSUAbilityDefinition reqAb = GetDefinitionById(def.RequiredAbilityId);
                string name = (reqAb != null) ? reqAb.Name : ("Ability " + def.RequiredAbilityId);

                if (parts == null) parts = new List<string>();
                parts.Add(name);
            }

            if (def.RequiredFeatId > 0)
            {
                OSUFeatDefinition reqFeat = OSUFeatSystem.GetFeatById(def.RequiredFeatId);
                string name = (reqFeat != null) ? reqFeat.Name : ("Feat " + def.RequiredFeatId);

                if (parts == null) parts = new List<string>();
                parts.Add(name);
            }

            if (parts == null || parts.Count == 0)
                return "";

            // Ex: "Riding I, Feat X"
            return string.Join(", ", parts.ToArray());
        }

        // ============================================================
        //  COMPRA (validação)
        // ============================================================
        public static bool CanPurchase(PlayerMobile pm, OSUAbilityDefinition def, out string reason)
        {
            reason = null;

            if (pm == null || def == null)
            {
                reason = "Erro interno.";
                return false;
            }

            if (pm.HasOSUAbility(def.Id))
            {
                reason = "Você já possui esta habilidade.";
                return false;
            }

            // ✅ Requerimentos por IDs (regra real)
            if (def.RequiredAbilityId > 0 && !pm.HasOSUAbility(def.RequiredAbilityId))
            {
                OSUAbilityDefinition req = GetDefinitionById(def.RequiredAbilityId);
                reason = "Você precisa comprar " + ((req != null) ? req.Name : ("Ability " + def.RequiredAbilityId)) + " primeiro.";
                return false;
            }

            if (def.RequiredFeatId > 0 && !pm.HasOSUFeat(def.RequiredFeatId))
            {
                OSUFeatDefinition req = OSUFeatSystem.GetFeatById(def.RequiredFeatId);
                reason = "Você precisa comprar " + ((req != null) ? req.Name : ("Feat " + def.RequiredFeatId)) + " primeiro.";
                return false;
            }

            bool isLanguageAbility =
                def.Id == LanguageAbilityIds.SpeakCommon ||
                def.Id == LanguageAbilityIds.SpeakSarang ||
                def.Id == LanguageAbilityIds.SpeakKamay ||
                def.Id == LanguageAbilityIds.SpeakMatalun ||
                def.Id == LanguageAbilityIds.SpeakZorteros ||
                def.Id == LanguageAbilityIds.SpeakAludin ||
                def.Id == LanguageAbilityIds.SpeakTherok;

            if (isLanguageAbility && !OSUDefQualDispatcher.CanBuyLanguageSkills(pm))
            {
                reason = "Você não consegue aprender habilidades de idioma.";
                return false;
            }

            // ✅ Regras extras específicas (hardcoded da ability)
            IOSUAbility impl = GetAbilityById(def.Id);

            if (impl != null)
            {
                string r;
                if (!impl.CanPurchase(pm, out r))
                {
                    reason = string.IsNullOrEmpty(r) ? "Você não atende aos requisitos." : r;
                    return false;
                }
            }

            if (pm.OSUAbilityPicks < def.CostPicks)
            {
                reason = "Você não tem pontos de habilidade suficientes.";
                return false;
            }

            return true;
        }

        public static bool Purchase(PlayerMobile pm, OSUAbilityDefinition def, out string reason)
        {
            reason = null;

            if (pm == null || def == null)
            {
                reason = "Erro interno.";
                return false;
            }

            IOSUAbility ability = GetAbilityById(def.Id);
            if (ability == null)
            {
                reason = "Habilidade não encontrada.";
                return false;
            }

            // Revalida tudo
            if (!CanPurchase(pm, def, out reason))
                return false;

            pm.OSUAbilityPicks -= def.CostPicks;
            pm.AddOSUAbility(def.Id);

            ability.OnPurchased(pm);

            reason = "Habilidade comprada com sucesso!";
            return true;
        }
    }
}
