using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Crafting.Tailoring
{
    // OSUFabricType:
    // 0 = Algodão (0)
    // 1 = Lã (+1)
    // 2 = Peles (+2)
    // 3 = Linho (-1)
    // 4 = Seda (-2)
    // 5 = algodao novo
    public static class OSUFabrics
    {
        public static int GetFabricThermalBonus(int fabricType)
        {
            switch (fabricType)
            {
                case 1: return +1; // lã
                case 2: return +2; // peles
                case 3: return -1; // linho
                case 4: return -2; // seda
                case 5: return 0; // algodao
                default: return 0; // algodão
            }
        }

        // Soma o bônus térmico de todas as roupas equipadas
        public static int GetTotalThermalBonus(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return 0;

            int total = 0;

            for (int i = 0; i < pm.Items.Count; i++)
            {
                Item it = pm.Items[i];
                BaseClothing bc = it as BaseClothing;

                if (bc == null)
                    continue;

                total += GetFabricThermalBonus(bc.OSUFabricType);
            }

            return total;
        }
    }
}
