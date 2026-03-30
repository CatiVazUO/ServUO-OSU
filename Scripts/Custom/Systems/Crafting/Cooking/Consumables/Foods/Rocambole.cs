using Server.Custom.Systems.Crafting.Cooking;
using Server.Items;
using System;
using System.Xml.Linq;

namespace Server.Custom.Systems.Crafting.Cooking.Consumables.Foods
{
    public class Rocambole : OSUBaseFood, IOSUHasCookingRecipe
    {
        public static readonly OSUCookingRecipeDef _Recipe = new OSUCookingRecipeDef
        {
            ID = "RocamboleDeCarne",
            TypeItem = typeof(Rocambole),
            Group = OSUCookingGroup.Assados,
            Name = "Rocambole de Carne",
            MinSkill = 40.0,
            MaxSkill = 100.0,

            CraftTimeMinutes = 1.0, // 1 minuto
            Station = OSUCookingStation.Oven,

            MissingResMessage = "Você não tem {0} para fazer um rocambole de carne."
        };

        static Rocambole()
        {
            _Recipe.Ingredients.Add(new OSUIngredientDef
            {
                TypeRes = typeof(RawRibs), // ajuste se o tipo for outro
                NameRes = "Costela crua",
                Amount = 2
            });

            _Recipe.Ingredients.Add(new OSUIngredientDef
            {
                TypeRes = typeof(Dough),
                NameRes = "Massa crua",
                Amount = 1
            });
        }

        public OSUCookingRecipeDef Recipe { get { return _Recipe; } }

        [Constructable]
        public Rocambole(int amount) : base(0x09C9)
        {
            Name = "rocambole de carne";
            Weight = 3.0;
            Hue = 0x3FC;
            FillFactor = 25;
            DecomposeDays = 3;

            Stackable = true;
            Amount = amount;

            HotHpPerTick = 5;
            HotStamPerTick = 0;
            HotManaPerTick = 0;
        }

        public Rocambole(Serial serial) : base(serial) { }

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
