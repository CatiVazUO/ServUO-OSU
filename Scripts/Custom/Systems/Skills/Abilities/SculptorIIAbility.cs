using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.SkillXP;

namespace Server.Custom.Systems.Skills.Abilities
{
    public class SculptorIIAbility : IOSUAbility
    {
        public const int AbilityId = 200005;

        public OSUAbilityDefinition Definition { get; private set; }
        public string CommandText { get { return ""; } }

        public SculptorIIAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: AbilityId,
                name: "Esculpir II",
                desc: "Permite esculpir a partir de modelos vivos.",
                costPicks: 1,
                commandText: "",
                iconId: 2278,
                requiredAbilityId: 200004,
                requiredFeatId: 0,
                requirementTextOverride: "Esculpir"
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

            if (!pm.HasOSUAbility(200004))
            {
                reason = "Você precisa comprar Escultor primeiro.";
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
