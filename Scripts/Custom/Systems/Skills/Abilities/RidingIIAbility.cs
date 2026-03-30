using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.SkillXP;

namespace Server.Custom.Systems.Skills.Abilities
{
    public class RidingIIAbility : IOSUAbility
    {
        public OSUAbilityDefinition Definition { get; private set; }

        // ✅ Se for ativa, coloque o comando completo aqui.
        public string CommandText { get { return ""; } }

        public string RequirementText { get { return ""; } }

        public RidingIIAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: 200002,
                name: "Riding II",
                desc: "Você não cai mais do cavalo.",
                costPicks: 1,
                commandText: "riding2",
                requiredFeatId: 0,
                requiredAbilityId: 200001,
                iconId: 0,
                requirementTextOverride: "Riding I"
            );
        }

        // ✅ Aqui você só precisa checar a REGRA REAL (ID)
        // (texto “Riding I” é só visual pro gump)
        public bool CanPurchase(PlayerMobile pm, out string reason)
        {
            reason = null;

            if (pm == null)
            {
                reason = "Erro interno.";
                return false;
            }

            // Riding I = 200001
            if (!pm.HasOSUAbility(200001))
            {
                reason = "Você precisa comprar Riding I primeiro.";
                return false;
            }

            return true;
        }

        public void OnPurchased(PlayerMobile pm)
        {
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
            if (pm == null)
                return;

            pm.SendMessage(0x55, "Riding II ativado! (efeito ainda não implementado)");
        }
    }
}
