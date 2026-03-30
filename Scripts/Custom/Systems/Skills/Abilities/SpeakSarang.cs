using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.SkillXP;

namespace Server.Custom.Systems.Skills.Abilities
{
    /// <summary>
    /// Habilidade: Falar: Língua do Povo 1
    /// Por enquanto ela serve para leitura de livros/pergaminhos selados (HtmlBooksV2),
    /// mas você pode expandir mais tarde (fala, NPCs, crafting etc).
    /// </summary>
    public class SpeakSarang : IOSUAbility
    {
        public OSUAbilityDefinition Definition { get; private set; }

        // Passiva
        public string CommandText { get { return ""; } }

        public SpeakSarang()
        {
            Definition = new OSUAbilityDefinition(
                id: 210002,
                name: "Falar: Língua dos Sarang",
                desc: "Permite ler livros/pergaminhos selados escritos na língua do Povo 1.",
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

            OSULanguage native = LanguageKnowledge.GetNativeLanguageForCulture(pm.OSUCultureId);

            if (native == OSULanguage.Sarang)
            {
                reason = "Você já fala essa língua naturalmente (sua cultura).";
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
