using System;

namespace Server.Custom.Systems.Postos
{
    public class PostoDefinition
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string ProfessionLabel { get; private set; }
        public PostoSize Size { get; private set; }
        public PostoResourceType ResourceType { get; private set; }
        public PostoObjectiveType ObjectiveType { get; private set; }
        public string ObjectiveDisplayName { get; private set; }
        public string[] ObjectiveTypeNames { get; private set; }
        public int ObjectiveAmount { get; private set; }
        public int DailyYield { get; private set; }
        public TimeSpan ProtectionDelay { get; private set; }
        public string StoryHtml { get; private set; }

        public PostoDefinition(
            string id,
            string name,
            string professionLabel,
            PostoSize size,
            PostoResourceType resourceType,
            PostoObjectiveType objectiveType,
            string objectiveDisplayName,
            string[] objectiveTypeNames,
            int objectiveAmount,
            int dailyYield,
            TimeSpan protectionDelay,
            string storyHtml)
        {
            Id = id ?? String.Empty;
            Name = name ?? String.Empty;
            ProfessionLabel = professionLabel ?? String.Empty;
            Size = size;
            ResourceType = resourceType;
            ObjectiveType = objectiveType;
            ObjectiveDisplayName = objectiveDisplayName ?? String.Empty;
            ObjectiveTypeNames = objectiveTypeNames ?? new string[0];
            ObjectiveAmount = objectiveAmount;
            DailyYield = dailyYield;
            ProtectionDelay = protectionDelay;
            StoryHtml = storyHtml ?? String.Empty;
        }

        public string GetNpcName()
        {
            return ProfessionLabel + " do posto " + Name;
        }

        public string GetChestName()
        {
            return "baú de despacho do posto " + Name;
        }
    }
}
