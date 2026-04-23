using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.Stables.Engine;

namespace Server.Custom.Systems.Skills.Profissoes
{
    public class BrandingFeat : IOSUFeat
    {
        public OSUFeatDefinition Definition { get; private set; }

        public string CommandText
        {
            get { return OSU.OSUCommandDisplay.Prefix + (Definition.CommandName ?? ""); }
        }

        public string RequirementText { get { return Definition.Requirement ?? ""; } }

        public BrandingFeat()
        {
            Definition = new OSUFeatDefinition(
                id: OSUStablePetSystem.BrandingFeatId,
                skill: SkillName.Veterinary,
                name: "Marcar Animais",
                desc: "Permite marcar montarias e animais de fazenda no estábulo.",
                costSkillXP: 3000,
                commandName: "",
                iconId: 20995,
                requiredFeatId: 0,
                requirementTextOverride: "Requer Veterinary",
                category: OSUFeatCategory.Profissoes
            );
        }

        public bool CanPurchase(PlayerMobile pm, out string reason)
        { reason = null;
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
