using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.SkillXP;

namespace Server.Custom.Systems.Skills.Abilities
{
    /// <summary>
    /// Habilidade: Falar: Língua Comum
    /// Por enquanto ela serve para leitura de livros/pergaminhos selados (HtmlBooksV2),
    /// mas você pode expandir mais tarde (fala, NPCs, crafting etc).
    /// </summary>
    public class SpeakCommonAbility : IOSUAbility
    {
        public OSUAbilityDefinition Definition { get; private set; }

        // Passiva
        public string CommandText { get { return ""; } }

        public SpeakCommonAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: 210001,
                name: "Falar: Língua Comum",
                desc: "Permite ler livros/pergaminhos selados escritos na Língua Comum. Habilidade que é ganha gratuitamente se vc tiver mais de 40 de inteligência",
                costPicks: 1,
                commandText: "",
                requiredFeatId: 0,
                requiredAbilityId: 0,
                iconId: 0,
                requirementTextOverride: ""
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

            // Se INT >= 40, a língua comum é natural (não deve poder comprar).
            if (pm.Int >= 40)
            {
                reason = "Você já fala a língua comum naturalmente (INT 40+).";
                return false;
            }

            // Se já possui por compra (redundante, mas dá mensagem melhor)
            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakCommon))
            {
                reason = "Você já possui esta habilidade.";
                return false;
            }


            return true;
        }

        public void OnPurchased(PlayerMobile pm)
        {
            // Nada a fazer aqui por enquanto.
            // O OSUAbilitySystem já adiciona o ID na lista (pm.AddOSUAbility).
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
            // Sem comando.
        }
    }
}
