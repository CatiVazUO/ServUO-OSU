
using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.Health;

namespace Server.Custom.Systems.Skills.Profissoes
{
    public class CirurgiaFeat : IOSUFeat
    {
        public OSUFeatDefinition Definition { get; private set; }

        public string CommandText
        {
            get { return OSU.OSUCommandDisplay.Prefix + (Definition.CommandName ?? ""); }
        }

        public string RequirementText { get { return Definition.Requirement ?? ""; } }

        public CirurgiaFeat()
        {
            Definition = new OSUFeatDefinition(
                id: OSUHealthSystem.FeatCirurgia,
                skill: SkillName.Healing,
                name: "Cirurgia",
                desc: "Permite usar os instrumentos da mesa cirúrgica para tratar lesões que exigem cirurgia.",
                costSkillXP: 5000,
                commandName: "",
                iconId: 20996,
                requiredFeatId: OSUHealthSystem.FeatExameMedico,
                requiredAbilityId: 0,
                requirementTextOverride: "Requer Exame Médico.",
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

            if (!pm.HasOSUFeat(OSUHealthSystem.FeatExameMedico))
            {
                reason = "Você precisa comprar Exame Médico primeiro.";
                return false;
            }

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
