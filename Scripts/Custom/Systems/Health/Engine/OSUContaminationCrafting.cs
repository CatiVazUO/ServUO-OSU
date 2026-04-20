
using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Custom.Systems.Health
{
    public static class OSUContaminationCrafting
    {
        public static void InheritStrongestContamination(Item result, IEnumerable<Item> ingredients)
        {
            if (result == null || ingredients == null)
                return;

            OSUDiseaseType chosen = OSUDiseaseType.None;
            DateTime chosenEnd = DateTime.MinValue;
            string source = String.Empty;

            foreach (Item item in ingredients)
            {
                if (item == null || !OSUHealthSystem.IsContaminated(item))
                    continue;

                OSUDiseaseType disease = OSUHealthSystem.GetItemContamination(item);
                DateTime ends = OSUHealthSystem.GetItemContaminationEndUtc(item);

                if (chosen == OSUDiseaseType.None || ends > chosenEnd)
                {
                    chosen = disease;
                    chosenEnd = ends;
                    source = OSUHealthSystem.GetItemContaminationSource(item);
                }
            }

            if (chosen == OSUDiseaseType.None)
            {
                OSUHealthSystem.ClearContaminatedItem(result);
                return;
            }

            TimeSpan remaining = chosenEnd > DateTime.UtcNow ? (chosenEnd - DateTime.UtcNow) : TimeSpan.FromHours(6);
            OSUHealthSystem.ContaminateItem(result, chosen, remaining, source);
        }
    }
}
