
using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.Stables.Engine;

namespace Server.Custom.Systems.Skills.Profissoes
{
    public class AnimalBreedingFeat : IOSUFeat
    {
        public OSUFeatDefinition Definition { get; private set; }

        public string CommandText
        {
            get { return OSU.OSUCommandDisplay.Prefix + (Definition.CommandName ?? ""); }
        }

        public string RequirementText { get { return Definition.Requirement ?? ""; } }

        public AnimalBreedingFeat()
        {
            Definition = new OSUFeatDefinition(
                id: OSUStablePetSystem.BreedingFeatId,
                skill: SkillName.AnimalTaming,
                name: "Cruzar Animais",
                desc: "Permite usar o serviço de cruzamento do estábulo.",
                costSkillXP: 5000,
                commandName: "",
                iconId: 20995,
                requiredFeatId: 0,
                requirementTextOverride: "Requer Animal Taming",
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
