using System.Collections.Generic;

namespace Server.Custom.Systems.Rent
{
    public static class TombstoneRegistry
    {
        private static readonly List<TombstoneDefinition> m_East = new List<TombstoneDefinition>();
        private static readonly List<TombstoneDefinition> m_South = new List<TombstoneDefinition>();

        public static List<TombstoneDefinition> East { get { return m_East; } }
        public static List<TombstoneDefinition> South { get { return m_South; } }

        static TombstoneRegistry()
        {
            // Exemplos iniciais. Ajuste e adicione os outros depois.
            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x1165,
                GumpID = 3579,
                ExtraCost = 0,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name,
                DateLayout = TombstoneDateLayout.Inline,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 0,
                NameX = 923,
                NameY = 306,
                NameWidth = 180,
                NameHeight = 20
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x1166,
                GumpID = 3579,
                ExtraCost = 0,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name,
                DateLayout = TombstoneDateLayout.Inline,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 0,
                NameX = 923,
                NameY = 306,
                NameWidth = 180,
                NameHeight = 20
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x1167,
                GumpID = 3577,
                ExtraCost = 50,
                TextColor = 0xFFFFFF,
                Fields = TombstoneFields.Name | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Inline,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 50,
                NameX = 913,
                NameY = 252,
                NameWidth = 180,
                NameHeight = 20,
                MessageX = 913,
                MessageY = 294,
                MessageWidth = 180,
                MessageHeight = 100
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x1168,
                GumpID = 3577,
                ExtraCost = 50,
                TextColor = 0xFFFFFF,
                Fields = TombstoneFields.Name | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Inline,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 50,
                NameX = 913,
                NameY = 252,
                NameWidth = 180,
                NameHeight = 20,
                MessageX = 913,
                MessageY = 294,
                MessageWidth = 180,
                MessageHeight = 100
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x1169,
                GumpID = 3573,
                ExtraCost = 100,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 0,
                NameX = 916,
                NameY = 267,
                NameWidth = 180,
                NameHeight = 20,
                DateX = 916,
                DateY = 320,
                DateWidth = 180,
                DateHeight = 50
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x116A,
                GumpID = 3573,
                ExtraCost = 100,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 0,
                NameX = 916,
                NameY = 267,
                NameWidth = 180,
                NameHeight = 20,
                DateX = 916,
                DateY = 320,
                DateWidth = 180,
                DateHeight = 50
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x116B,
                GumpID = 3569,
                ExtraCost = 200,
                TextColor = 0xFFFFFF,
                Fields = TombstoneFields.Name | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 50,
                NameX = 900,
                NameY = 260,
                NameWidth = 180,
                NameHeight = 20,
                MessageX = 909,
                MessageY = 305,
                MessageWidth = 160,
                MessageHeight = 100
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x116C,
                GumpID = 3569,
                ExtraCost = 200,
                TextColor = 0xFFFFFF,
                Fields = TombstoneFields.Name | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 42,
                MaxDateLength = 12,
                MaxMessageLength = 50,
                NameX = 900,
                NameY = 260,
                NameWidth = 180,
                NameHeight = 20,
                MessageX = 909,
                MessageY = 305,
                MessageWidth = 160,
                MessageHeight = 100
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x116D,
                GumpID = 3570,
                ExtraCost = 300,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 50,
                MaxDateLength = 12,
                MaxMessageLength = 70,
                NameX = 886,
                NameY = 323,
                NameWidth = 206,
                NameHeight = 20,
                MessageX = 862,
                MessageY = 363,
                MessageWidth = 255,
                MessageHeight = 60
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x116E,
                GumpID = 3570,
                ExtraCost = 300,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 50,
                MaxDateLength = 12,
                MaxMessageLength = 70,
                NameX = 886,
                NameY = 323,
                NameWidth = 206,
                NameHeight = 20,
                MessageX = 862,
                MessageY = 363,
                MessageWidth = 255,
                MessageHeight = 60
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x1171,
                GumpID = 3575,
                ExtraCost = 200,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 50,
                MaxDateLength = 12,
                MaxMessageLength = 120,
                NameX = 910,
                NameY = 186,
                NameWidth = 206,
                NameHeight = 20,
                DateX = 922,
                DateY = 224,
                DateWidth = 180,
                DateHeight = 50,
                MessageX = 906,
                MessageY = 293,
                MessageWidth = 210,
                MessageHeight = 128
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x1172,
                GumpID = 3575,
                ExtraCost = 200,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 50,
                MaxDateLength = 12,
                MaxMessageLength = 120,
                NameX = 910,
                NameY = 186,
                NameWidth = 206,
                NameHeight = 20,
                DateX = 922,
                DateY = 224,
                DateWidth = 180,
                DateHeight = 50,
                MessageX = 906,
                MessageY = 293,
                MessageWidth = 210,
                MessageHeight = 128
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x117F,
                GumpID = 3576,
                ExtraCost = 250,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 50,
                MaxDateLength = 12,
                MaxMessageLength = 120,
                NameX = 907,
                NameY = 274,
                NameWidth = 206,
                NameHeight = 20,
                DateX = 937,
                DateY = 213,
                DateWidth = 148,
                DateHeight = 40,
                MessageX = 917,
                MessageY = 312,
                MessageWidth = 184,
                MessageHeight = 116
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x1180,
                GumpID = 3576,
                ExtraCost = 250,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Stacked,
                MaxNameLength = 50,
                MaxDateLength = 12,
                MaxMessageLength = 120,
                NameX = 907,
                NameY = 274,
                NameWidth = 206,
                NameHeight = 20,
                DateX = 937,
                DateY = 213,
                DateWidth = 148,
                DateHeight = 40,
                MessageX = 917,
                MessageY = 312,
                MessageWidth = 184,
                MessageHeight = 116
            });

            m_East.Add(new TombstoneDefinition
            {
                ItemID = 0x117B,
                GumpID = 3571,
                ExtraCost = 350,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Inline,
                MaxNameLength = 30,
                MaxDateLength = 12,
                MaxMessageLength = 95,
                NameX = 932,
                NameY = 209,
                NameWidth = 151,
                NameHeight = 20,
                DateX = 932,
                DateY = 243,
                DateWidth = 151,
                DateHeight = 20,
                MessageX = 936,
                MessageY = 284,
                MessageWidth = 146,
                MessageHeight = 100
            });

            m_South.Add(new TombstoneDefinition
            {
                ItemID = 0x117C,
                GumpID = 3571,
                ExtraCost = 350,
                TextColor = 0x000000,
                Fields = TombstoneFields.Name | TombstoneFields.Date | TombstoneFields.Message,
                DateLayout = TombstoneDateLayout.Inline,
                MaxNameLength = 30,
                MaxDateLength = 12,
                MaxMessageLength = 95,
                NameX = 932,
                NameY = 209,
                NameWidth = 151,
                NameHeight = 20,
                DateX = 932,
                DateY = 243,
                DateWidth = 151,
                DateHeight = 20,
                MessageX = 936,
                MessageY = 284,
                MessageWidth = 146,
                MessageHeight = 100
            });
        }

        public static List<TombstoneDefinition> GetByBaseItemID(int itemID)
        {
            return IsEastFamily(itemID) ? m_East : m_South;
        }

        public static TombstoneDefinition Find(int itemID)
        {
            int i;

            for (i = 0; i < m_East.Count; i++)
            {
                if (m_East[i].ItemID == itemID)
                    return m_East[i];
            }

            for (i = 0; i < m_South.Count; i++)
            {
                if (m_South[i].ItemID == itemID)
                    return m_South[i];
            }

            return null;
        }

        public static bool IsEastFamily(int itemID)
        {
            switch (itemID)
            {
                case 0x1165:
                case 0x1167:
                case 0x1169:
                case 0x116B:
                case 0x116D:
                case 0x116F:
                case 0x1171:
                case 0x1173:
                case 0x1177:
                case 0x117B:
                case 0x117F:
                    return true;
            }

            return false;
        }
    }
}
