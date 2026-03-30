using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.Skills.Profissoes
{
    // Example of a profession feat (same pattern as combat feats).
    // One file per proficiency in this folder.
    public class MiningProficiencyFeat : IOSUFeat
    {
        public OSUFeatDefinition Definition { get; private set; }
        public string CommandText
        {
            get
            {
                // Se você usa prefixo padrão:
                return OSU.OSUCommandDisplay.Prefix + (Definition.CommandName ?? "");
            }
        }
        public string RequirementText { get { return Definition.Requirement ?? ""; } }

        public MiningProficiencyFeat()
        {
            Definition = new OSUFeatDefinition(
                id: 110001,
                skill: SkillName.Mining,
                name: "Mining Proficiency I",
                desc: "Increases your mining efficiency (example placeholder).",
                costSkillXP: 2500,
                commandName: "miningpro",
                iconId: 20997,
                requiredFeatId: 0, // ex: "Precisa da feat X" ou "" se não tiver
                requiredAbilityId: 0,
                requirementTextOverride: "",
                category: OSUFeatCategory.Profissoes
            );
        }

        public bool CanPurchase(PlayerMobile pm, out string reason)
        {
            reason = null;
            if (pm == null)
            {
                reason = "Erro interno.";
                return false;
            }

            return true;
        }

        public void OnPurchased(PlayerMobile pm)
        {
            // Normalmente nada em feat
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
            if (pm == null)
                return;

            pm.SendMessage(0x55, "Mining Proficienfy ativado! (efeito ainda não implementado)");
        }
    }
}
