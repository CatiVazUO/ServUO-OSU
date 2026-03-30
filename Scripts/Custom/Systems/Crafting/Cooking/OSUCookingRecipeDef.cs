using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.Crafting.Cooking
{
    // Grupo / categoria (define ferramenta/estação no futuro)
    public enum OSUCookingGroup
    {
        Preparos,
        Assados,
        Cozidos,
        Fritos,
        Bebidas,
        Fermentados,
        Moagem
    }

    // “Estação” onde transforma/produz (forno, moinho, etc.)
    public enum OSUCookingStation
    {
        None,
        Oven,
        Mill,
        Campfire,
        Cauldron
    }

    public class OSUIngredientDef
    {
        public Type TypeRes { get; set; }          // ex: typeof(FlourSack)
        public string NameRes { get; set; }        // ex: "Trigo"
        public int Amount { get; set; }            // ex: 3
    }

    public class OSUCookingRecipeDef
    {
        // Identidade do item (pra você organizar / pro futuro sistema)
        public string ID { get; set; }              // ex: "Dough"
        public Type TypeItem { get; set; }          // ex: typeof(Dough)
        public OSUCookingGroup Group { get; set; }  // ex: Preparos
        public string Name { get; set; }            // nome que aparece no jogo

        public double MinSkill { get; set; }
        public double MaxSkill { get; set; }

        // Tempo real pra “ficar pronto”
        public double CraftTimeMinutes { get; set; } // ex: 0.5

        // Estação/ferramenta
        public OSUCookingStation Station { get; set; }

        // Mensagem quando falta ingrediente
        // Use {0} para o nome do ingrediente faltando
        public string MissingResMessage { get; set; }

        // Ingredientes ilimitados
        public List<OSUIngredientDef> Ingredients { get; private set; } = new List<OSUIngredientDef>();
    }

    // Interface: todo item (comida/bebida) OSU pode expor uma Recipe
    public interface IOSUHasCookingRecipe
    {
        OSUCookingRecipeDef Recipe { get; }
    }
}
