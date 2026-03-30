using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.SkillXP;

namespace Server.Custom.Systems.Skills.Abilities
{
    /// <summary>
    /// Habilidade: Falar: Therok
    /// Por enquanto ela serve para leitura de livros/pergaminhos selados (HtmlBooksV2),
    /// mas você pode expandir mais tarde (fala, NPCs, crafting etc).
    /// </summary>
    public class SpeakTherokAbility : IOSUAbility
    {
        public OSUAbilityDefinition Definition { get; private set; }

        // Passiva
        public string CommandText { get { return ""; } }

        public SpeakTherokAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: 210007,
                name: "Falar: Therok",
                desc: "Permite ler livros/pergaminhos selados escritos em Therok (língua antiga).",
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

            // Therok não é natural; compra permitida sempre.


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
