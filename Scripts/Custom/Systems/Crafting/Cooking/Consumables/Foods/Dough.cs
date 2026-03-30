using Server.Custom.Systems.Crafting.Cooking;
using Server.Items;
using System;

namespace Server.Custom.Systems.Crafting.Cooking.Consumables.Foods
{
    public class Dough : OSUBaseFood, IOSUHasCookingRecipe
    {
        public static readonly OSUCookingRecipeDef _Recipe = new OSUCookingRecipeDef
        {
            ID = "Dough",
            TypeItem = typeof(Dough),
            Group = OSUCookingGroup.Preparos,
            Name = "Massa crua",
            MinSkill = 0.0,
            MaxSkill = 100.0,

            CraftTimeMinutes = 0.5, // 30 segundos
            Station = OSUCookingStation.Oven,

            MissingResMessage = "Você não tem {0} para sovar essa massa."
        };

        static Dough()
        {
            _Recipe.Ingredients.Add(new OSUIngredientDef
            {
                TypeRes = typeof(SackFlour), // ajuste pro tipo real do seu shard
                NameRes = "Trigo",
                Amount = 3
            });

            _Recipe.Ingredients.Add(new OSUIngredientDef
            {
                TypeRes = typeof(Eggs), // ajuste pro tipo real
                NameRes = "Ovos",
                Amount = 2
            });
        }

        public OSUCookingRecipeDef Recipe { get { return _Recipe; } }

        [Constructable]
        public Dough() : base(0x103d)
        {
            Name = "Massa crua";
            Weight = 1.0;
            Hue = 0;
            FillFactor = 2;        // exemplo
            DecomposeDays = 3;     // exemplo

            // Sem efeito (cru, simples)
            HotHpPerTick = 0;
            HotStamPerTick = 0;
            HotManaPerTick = 0;
        }

        public Dough(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
