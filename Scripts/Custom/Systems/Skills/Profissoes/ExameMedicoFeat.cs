
using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.Health;

namespace Server.Custom.Systems.Skills.Profissoes
{
    public class ExameMedicoFeat : IOSUFeat
    {
        public OSUFeatDefinition Definition { get; private set; }

        public string CommandText
        {
            get { return OSU.OSUCommandDisplay.Prefix + (Definition.CommandName ?? ""); }
        }

        public string RequirementText { get { return Definition.Requirement ?? ""; } }

        public ExameMedicoFeat()
        {
            Definition = new OSUFeatDefinition(
                id: OSUHealthSystem.FeatExameMedico,
                skill: SkillName.Healing,
                name: "Exame Médico",
                desc: "Permite usar a maleta médica para examinar lesões, doenças e imunidades de um jogador.",
                costSkillXP: 3000,
                commandName: "",
                iconId: 20995,
                requiredFeatId: 0,
                requiredAbilityId: 0,
                requirementTextOverride: "",
                category: OSUFeatCategory.Profissoes
            );
        }

        public bool CanPurchase(PlayerMobile pm, out string reason)
        {
            reason = null;
            return true;
        }

        public void OnPurchased(PlayerMobile pm)
        {
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
        }
    }
}
