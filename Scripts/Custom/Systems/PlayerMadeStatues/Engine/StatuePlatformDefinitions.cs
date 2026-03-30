using System.Collections.Generic;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public sealed class StatuePlatformDefinition
    {
        public StatuePlatformSize Size { get; private set; }
        public int ItemID { get; private set; }
        public int Cost { get; private set; }
        public int PreviewBlockItemID { get; private set; }
        public double Weight { get; private set; }
        public string DisplayName { get; private set; }
        public int Height { get; private set; }

        public StatuePlatformDefinition(StatuePlatformSize size, int itemID, int cost, int previewBlockItemID, double weight, string displayName, int height)
        {
            Size = size;
            ItemID = itemID;
            Cost = cost;
            PreviewBlockItemID = previewBlockItemID;
            Weight = weight;
            DisplayName = displayName;
            Height = height;
        }
    }

    public static class StatuePlatformDefinitions
    {
        private static readonly Dictionary<StatuePlatformSize, StatuePlatformDefinition> m_Table = new Dictionary<StatuePlatformSize, StatuePlatformDefinition>()
        {
            { StatuePlatformSize.Small, new StatuePlatformDefinition(StatuePlatformSize.Small, 0x16D7, 2, 0x10B2, 20.0, "plataforma pequena", 4) },
            { StatuePlatformSize.Medium, new StatuePlatformDefinition(StatuePlatformSize.Medium, 0x16DD, 3, 0x10B7, 30.0, "plataforma média", 10) },
            { StatuePlatformSize.Large, new StatuePlatformDefinition(StatuePlatformSize.Large, 0x16E3, 4, 0x10B6, 40.0, "plataforma grande", 6) },
            { StatuePlatformSize.Giant, new StatuePlatformDefinition(StatuePlatformSize.Giant, 0x16F4, 7, 0x10B6, 50.0, "plataforma gigante", 7) },
            { StatuePlatformSize.XXL, new StatuePlatformDefinition(StatuePlatformSize.XXL, 0x16F2, 10, 0x10B6, 60.0, "plataforma xxl", 14) }
        };

        public static StatuePlatformDefinition Get(StatuePlatformSize size)
        {
            StatuePlatformDefinition def;
            return m_Table.TryGetValue(size, out def) ? def : null;
        }
    }
}
