using Server.Commands;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Mobiles;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public class SculptorAbility : IOSUAbility
    {
        public const int AbilityId = 200004;

        public OSUAbilityDefinition Definition { get; private set; }
        public string CommandText { get { return ""; } }

        public SculptorAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: AbilityId,
                name: "Esculpir",
                desc: "Permite usar ferramentas de escultor e criar esculturas em pedra, mármore, granito, pedra-sabão, argila e caulim.",
                costPicks: 1,
                commandText: "",
                iconId: 2278,
                requiredAbilityId: 0,
                requiredFeatId: 0,
                requirementTextOverride: "Nenhum"
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
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
        }
    }
}
