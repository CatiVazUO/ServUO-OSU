using System;

namespace Server.Custom.Systems.OSUResources
{
    public class OSUMaterialDefinition
    {
        public int Id { get; private set; }
        public string Key { get; private set; }
        public string Name { get; private set; }
        public OSUMaterialCategory Category { get; private set; }
        public int Hue { get; private set; }

        public bool Mineable { get; private set; }
        public bool Sculptable { get; private set; }
        public bool SupportsSmallStatues { get; private set; }
        public bool SupportsLargeStatues { get; private set; }
        public bool SupportsLiveModelStatues { get; private set; }

        public Type RawType { get; private set; }
        public Type SmallSculptType { get; private set; }
        public Type LargeSculptType { get; private set; }
        public Type PowderType { get; private set; }
        public Type RefinedType { get; private set; }

        public string Notes { get; private set; }

        public OSUMaterialDefinition(
            int id,
            string key,
            string name,
            OSUMaterialCategory category,
            int hue,
            bool mineable,
            bool sculptable,
            bool supportsSmallStatues,
            bool supportsLargeStatues,
            bool supportsLiveModelStatues,
            Type rawType,
            Type smallSculptType,
            Type largeSculptType,
            Type powderType,
            Type refinedType,
            string notes)
        {
            Id = id;
            Key = key ?? string.Empty;
            Name = name ?? string.Empty;
            Category = category;
            Hue = hue;
            Mineable = mineable;
            Sculptable = sculptable;
            SupportsSmallStatues = supportsSmallStatues;
            SupportsLargeStatues = supportsLargeStatues;
            SupportsLiveModelStatues = supportsLiveModelStatues;
            RawType = rawType;
            SmallSculptType = smallSculptType;
            LargeSculptType = largeSculptType;
            PowderType = powderType;
            RefinedType = refinedType;
            Notes = notes ?? string.Empty;
        }
    }
}
