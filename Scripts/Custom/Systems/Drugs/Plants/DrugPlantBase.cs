using System;
using Server.Mobiles;
using Server.Custom.Reinos;
using Server.Spells;

namespace Server.Items
{
    public abstract class DrugPlantBase : Item
    {
        protected abstract Type YieldType { get; }
        protected abstract int MinSkillFixed { get; }
        protected abstract int BonusYield { get; }

        protected DrugPlantBase(int itemID)
            : base(itemID)
        {
            Movable = false;
        }

        public DrugPlantBase(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            if (!from.InRange(GetWorldLocation(), 1))
            {
                from.SendMessage("Você está muito distante.");
                return;
            }

            if (from.Mounted)
            {
                from.SendMessage("Você não pode fazer isso enquanto estiver montado.");
                return;
            }

            ReinoMilitarySystem.NotifyForeignHarvesting(from, this);

            int herbalFixed = 0;
            try
            {
                herbalFixed = from.Skills[(SkillName)36].Fixed;
            }
            catch
            {
                herbalFixed = (int)(from.Skills[SkillName.Alchemy].Value * 10.0);
            }

            if (herbalFixed < MinSkillFixed)
            {
                from.SendMessage("Você não tem conhecimento suficiente para colher essa planta.");
                return;
            }

            SpellHelper.Turn(from, this);
            from.Animate(32, 5, 1, true, false, 0);
            from.PlaySound(79);

            double chance = ((double)(herbalFixed - MinSkillFixed + 500)) / 1000.0;
            if (chance < 0.20)
                chance = 0.20;
            if (chance > 0.95)
                chance = 0.95;

            if (Utility.RandomDouble() > chance)
            {
                from.SendMessage("Você tenta colher a planta, mas acaba estragando o material.");
                Delete();
                return;
            }

            Item yield = Activator.CreateInstance(YieldType) as Item;
            if (yield == null)
            {
                from.SendMessage("Algo deu errado ao colher a planta.");
                return;
            }

            int amount = 1 + Utility.Random(BonusYield + 1);
            if (yield.Stackable)
                yield.Amount = amount;
            else if (amount > 1)
            {
                from.AddToBackpack(yield);
                for (int i = 1; i < amount; i++)
                    from.AddToBackpack(Activator.CreateInstance(YieldType) as Item);
                from.SendMessage("Você colhe a planta com cuidado.");
                Delete();
                return;
            }

            from.AddToBackpack(yield);
            from.SendMessage("Você colhe a planta com cuidado.");
            Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
