using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.Skills.Combate
{
    // ✅ 1 arquivo por feat
    public class DisarmeFeat : IOSUFeat
    {
        public OSUFeatDefinition Definition { get; private set; }

        // ✅ Comando completo (com prefixo)
        public string CommandText
        {
            get
            {
                // Se você usa prefixo padrão:
                return OSU.OSUCommandDisplay.Prefix + (Definition.CommandName ?? "");
            }
        }

        // ✅ Texto do requerimento pro gump.
        // Aqui usamos o Requirement que está no Definition.
        public string RequirementText { get { return Definition.Requirement ?? ""; } }

        public DisarmeFeat()
        {
            Definition = new OSUFeatDefinition(
                id: 100001,
                skill: SkillName.Swords,
                name: "Disarme",
                desc: "Desarma o alvo (efeito a implementar).",
                costSkillXP: 3500,
                commandName: "disarme",
                iconId: 20996,
                requiredFeatId: 0, // ex: "Precisa da feat X" ou "" se não tiver
                requiredAbilityId: 0,
                requirementTextOverride: "",
                category: OSUFeatCategory.Combate
            );
        }

        // ✅ Se tiver regra EXTRA além do RequirementText, faz aqui.
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
            //ver se tem requisito
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
            if (pm == null)
                return;

            pm.SendMessage(0x55, "Disarme ativado! (efeito ainda não implementado)");
        }
    }
}
