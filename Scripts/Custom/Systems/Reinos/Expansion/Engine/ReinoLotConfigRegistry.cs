using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoLotConfigRegistry
    {
        private static readonly Dictionary<int, ReinoLotConfigDefinition> m_Configs = new Dictionary<int, ReinoLotConfigDefinition>();
        private static readonly Dictionary<int, List<int>> m_ConfigIdsBySide = new Dictionary<int, List<int>>();
        private static bool m_Initialized;

        public static void EnsureInitialized()
        {
            if (m_Initialized)
                return;

            m_Initialized = true;
            RegisterDefaults();
        }

        public static ReinoLotConfigDefinition Get(int configId)
        {
            EnsureInitialized();

            ReinoLotConfigDefinition def;
            m_Configs.TryGetValue(configId, out def);
            return def;
        }

        public static List<int> GetConfigIdsForSide(int side)
        {
            EnsureInitialized();

            List<int> list;
            if (!m_ConfigIdsBySide.TryGetValue(side, out list))
                return new List<int>();

            return new List<int>(list);
        }

        public static int GetRandomConfigIdForSide(int side)
        {
            EnsureInitialized();

            List<int> ids;
            if (!m_ConfigIdsBySide.TryGetValue(side, out ids) || ids == null || ids.Count == 0)
                return 0;

            return ids[Utility.Random(ids.Count)];
        }

        private static void Register(ReinoLotConfigDefinition def)
        {
            if (def == null)
                return;

            m_Configs[def.ConfigId] = def;

            if (def.ConfigId == 0)
                return;

            List<int> list;
            if (!m_ConfigIdsBySide.TryGetValue(def.Side, out list))
            {
                list = new List<int>();
                m_ConfigIdsBySide[def.Side] = list;
            }

            if (!list.Contains(def.ConfigId))
                list.Add(def.ConfigId);
        }

        private static ReinoLotConfigDefinition KillConfig(int configId, int side, string name, int multiId, Point3D encounterOffset, Point3D spawnOffset, int spawnRange, int objectiveAmount, int spawnCount, string displayName, string[] spawnTypeNames, string[] objectiveTypeNames)
        {
            ReinoLotConfigDefinition def = new ReinoLotConfigDefinition();
            def.ConfigId = configId;
            def.Side = side;
            def.Name = name;
            def.EncounterMultiId = multiId;
            def.EncounterOffset = encounterOffset;
            def.SpawnOffset = spawnOffset;
            def.SpawnRange = spawnRange;
            def.ObjectiveType = ReinoObjectiveType.KillMob;
            def.ObjectiveDisplayName = displayName;
            def.ObjectiveTargetTypeNames = objectiveTypeNames ?? spawnTypeNames ?? new string[0];
            def.ObjectiveAmount = objectiveAmount;
            def.SpawnCount = spawnCount;

            if (spawnTypeNames != null && spawnTypeNames.Length > 0)
            {
                def.MobEntries = new ReinoLotMobSpawnEntry[spawnTypeNames.Length];

                for (int i = 0; i < spawnTypeNames.Length; i++)
                    def.MobEntries[i] = new ReinoLotMobSpawnEntry(spawnTypeNames[i], 1);
            }

            return def;
        }

        private static ReinoLotConfigDefinition CollectConfig(int configId, int side, string name, int multiId, Point3D encounterOffset, Point3D spawnOffset, int spawnRange, int objectiveAmount, int spawnCount, string displayName, ReinoLotCollectibleSpawnEntry[] entries)
        {
            ReinoLotConfigDefinition def = new ReinoLotConfigDefinition();
            def.ConfigId = configId;
            def.Side = side;
            def.Name = name;
            def.EncounterMultiId = multiId;
            def.EncounterOffset = encounterOffset;
            def.SpawnOffset = spawnOffset;
            def.SpawnRange = spawnRange;
            def.ObjectiveType = ReinoObjectiveType.CollectItem;
            def.ObjectiveDisplayName = displayName;
            def.ObjectiveAmount = objectiveAmount;
            def.SpawnCount = spawnCount;
            def.CollectibleEntries = entries ?? new ReinoLotCollectibleSpawnEntry[0];

            List<string> targets = new List<string>();
            for (int i = 0; i < def.CollectibleEntries.Length; i++)
            {
                ReinoLotCollectibleSpawnEntry entry = def.CollectibleEntries[i];
                if (entry == null || String.IsNullOrWhiteSpace(entry.TypeName))
                    continue;

                if (!targets.Contains(entry.TypeName))
                    targets.Add(entry.TypeName);
            }

            def.ObjectiveTargetTypeNames = targets.ToArray();
            return def;
        }

        private static ReinoLotCollectibleSpawnEntry Collectible(string typeName, string displayName, int itemId, int hue, string requiredToolTypeName)
        {
            return new ReinoLotCollectibleSpawnEntry(typeName, displayName, itemId, hue, requiredToolTypeName, 1);
        }

        private static void RegisterDefaults()
        {
            Register(new ReinoLotConfigDefinition
            {
                ConfigId = 0,
                Side = 0,
                Name = "Lote limpo",
                EncounterMultiId = 0,
                EncounterOffset = Point3D.Zero,
                SpawnOffset = Point3D.Zero,
                SpawnRange = 0,
                ObjectiveType = ReinoObjectiveType.None,
                ObjectiveDisplayName = "sem ameaça",
                ObjectiveTargetTypeNames = new string[0],
                ObjectiveAmount = 0,
                SpawnCount = 0
            });

            Register15();
            Register20();
            Register30();
            Register40();
        }

        private static void Register15()
        {
            Register(KillConfig(1501, 15, "Acampamento de saqueadores", 0x0064, new Point3D(0, 0, 0), Point3D.Zero, 4, 8, 4, "saqueadores", new string[] { "FreshBandit" }, null));
            Register(KillConfig(1502, 15, "Ruína infestada", 0x0066, new Point3D(0, 0, 0), Point3D.Zero, 4, 10, 4, "zumbis de ruína", new string[] { "RuinZombie" }, null));
            Register(KillConfig(1503, 15, "Ossário recente", 0x0068, new Point3D(0, 0, 0), Point3D.Zero, 4, 10, 5, "fresh skeletons", new string[] { "FreshSkeleton" }, null));
            Register(KillConfig(1504, 15, "Poleiro das harpias", 0x006A, new Point3D(0, 0, 0), Point3D.Zero, 4, 8, 3, "harpias cinzentas", new string[] { "AshHarpy" }, null));
            Register(KillConfig(1505, 15, "Casebre dos magos", 0x006C, new Point3D(0, 0, 0), Point3D.Zero, 4, 8, 3, "magos perdidos", new string[] { "LostHedgeMage" }, null));
            Register(KillConfig(1506, 15, "Covil de goblins", 0x006E, new Point3D(0, 0, 0), Point3D.Zero, 4, 12, 5, "goblins esfomeados", new string[] { "FeralGoblin" }, null));
            Register(KillConfig(1507, 15, "Toca de morcegos", 0x00A0, new Point3D(0, 0, 0), Point3D.Zero, 4, 14, 6, "mongbats doentios", new string[] { "BlightMongbat" }, null));
            Register(KillConfig(1508, 15, "Ninho de ratmen", 0x00A2, new Point3D(0, 0, 0), Point3D.Zero, 4, 10, 4, "ratmen saqueadores", new string[] { "RuinRatman" }, null));

            Register(CollectConfig(1509, 15, "Bosque de galhos secos", 0x0064, Point3D.Zero, Point3D.Zero, 4, 10, 4, "galhos secos",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("DryBrambles", "galho seco", 0x0D15, 2101, "Scythe")
                }));

            Register(CollectConfig(1510, 15, "Jardim de fungos", 0x0066, Point3D.Zero, Point3D.Zero, 4, 10, 4, "fungos tóxicos",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("ToxicMushroom", "fungo tóxico", 0x0D16, 1272, "Torch")
                }));

            Register(CollectConfig(1511, 15, "Pedreira rasa", 0x0068, Point3D.Zero, Point3D.Zero, 4, 12, 4, "nódulos de minério ruim",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("ShallowOreNode", "nódulo de minério", 0x19B8, 2419, "Pickaxe")
                }));

            Register(CollectConfig(1512, 15, "Matagal de raízes", 0x006A, Point3D.Zero, Point3D.Zero, 4, 12, 5, "raízes entranhadas",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("SnaredRoot", "raiz entranhada", 0x0C95, 2206, "Scythe")
                }));

            Register(CollectConfig(1513, 15, "Ninho de ovos ruins", 0x006C, Point3D.Zero, Point3D.Zero, 4, 8, 3, "ovos corrompidos",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("RottenEggNest", "ninho podre", 0x09B5, 2117, "Torch")
                }));

            Register(CollectConfig(1514, 15, "Campina espinhosa", 0x006E, Point3D.Zero, Point3D.Zero, 4, 14, 5, "sarças agressivas",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("Hookthorn", "sarça agressiva", 0x0CA8, 2118, "Scythe")
                }));

            Register(CollectConfig(1515, 15, "Cristais do lodo", 0x00A0, Point3D.Zero, Point3D.Zero, 4, 10, 4, "cristais de lodo",
                new ReinoLotCollectibleSpawnEntry[]
                {
                    Collectible("SludgeCrystal", "cristal de lodo", 0x1F19, 1260, "Pickaxe")
                }));
        }

        private static void Register20()
        {
            Register(KillConfig(2001, 20, "Ruína dos necromantes", 0x0074, Point3D.Zero, Point3D.Zero, 6, 16, 6, "capatazes ósseos", new string[] { "BoneCaptain", "FreshSkeleton" }, new string[] { "BoneCaptain", "FreshSkeleton" }));
            Register(KillConfig(2002, 20, "Ruas tomadas", 0x0076, Point3D.Zero, Point3D.Zero, 6, 18, 6, "goblins ferozes", new string[] { "FeralGoblin", "FeralGoblin" }, null));
            Register(KillConfig(2003, 20, "Praça das harpias", 0x0078, Point3D.Zero, Point3D.Zero, 6, 14, 5, "harpias de cinza", new string[] { "AshHarpy", "AshHarpy" }, null));
            Register(KillConfig(2004, 20, "Acampamento ardente", 0x008C, Point3D.Zero, Point3D.Zero, 6, 16, 6, "bandidos incendiários", new string[] { "FirebrandBandit", "FreshBandit" }, new string[] { "FirebrandBandit", "FreshBandit" }));
            Register(KillConfig(2005, 20, "Covil do lodo", 0x0096, Point3D.Zero, Point3D.Zero, 6, 16, 5, "ratos da ruína", new string[] { "RuinRatman", "RuinZombie" }, new string[] { "RuinRatman", "RuinZombie" }));
            Register(KillConfig(2006, 20, "Anel dos magos", 0x0098, Point3D.Zero, Point3D.Zero, 6, 12, 4, "magos perdidos", new string[] { "LostHedgeMage" }, null));

            Register(CollectConfig(2007, 20, "Campo de tocos", 0x009A, Point3D.Zero, Point3D.Zero, 6, 18, 6, "tocos retorcidos",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("WitheredStump", "toco retorcido", 0x0E57, 1109, "Axe") }));

            Register(CollectConfig(2008, 20, "Lodaçal fúngico", 0x009C, Point3D.Zero, Point3D.Zero, 6, 16, 5, "fungos gordurosos",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("GreaseMushroom", "fungo gorduroso", 0x0D16, 1270, "Torch") }));

            Register(CollectConfig(2009, 20, "Cascalho antigo", 0x009E, Point3D.Zero, Point3D.Zero, 6, 16, 5, "blocos de cascalho",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("AncientGravel", "bloco de cascalho", 0x1363, 2407, "Pickaxe") }));

            Register(CollectConfig(2010, 20, "Jardim de espinhos", 0x00A0, Point3D.Zero, Point3D.Zero, 6, 18, 6, "espinhos endurecidos",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("HardenedThorn", "espinho endurecido", 0x0CA7, 2213, "Scythe") }));

            Register(CollectConfig(2011, 20, "Taludes em brasa", 0x00A2, Point3D.Zero, Point3D.Zero, 6, 14, 4, "bolsões de gás incendiável",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("FireGasPod", "bolsão inflamável", 0x0F8B, 1359, "Torch") }));

            Register(CollectConfig(2012, 20, "Jazida rachada", 0x0064, Point3D.Zero, Point3D.Zero, 6, 18, 6, "cristas minerais rachadas",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("CrackedMineral", "crista mineral", 0x1367, 2418, "Pickaxe") }));
        }

        private static void Register30()
        {
            Register(KillConfig(3001, 30, "Pátio dos capitães", 0x007A, Point3D.Zero, Point3D.Zero, 8, 24, 8, "capitães ósseos", new string[] { "BoneCaptain", "FreshSkeleton" }, new string[] { "BoneCaptain", "FreshSkeleton" }));
            Register(KillConfig(3002, 30, "Encruzilhada bruta", 0x0074, Point3D.Zero, Point3D.Zero, 8, 24, 8, "ogros da ruína", new string[] { "RuinOgre", "FreshBandit" }, new string[] { "RuinOgre", "FreshBandit" }));
            Register(KillConfig(3003, 30, "Torres do eco", 0x0076, Point3D.Zero, Point3D.Zero, 8, 20, 6, "arcanistas do eco", new string[] { "SmolderMage", "LostHedgeMage" }, new string[] { "SmolderMage", "LostHedgeMage" }));
            Register(KillConfig(3004, 30, "Círculo das harpias", 0x0078, Point3D.Zero, Point3D.Zero, 8, 22, 7, "harpias de fuligem", new string[] { "AshHarpy", "AshHarpy" }, null));
            Register(KillConfig(3005, 30, "Pântano dos ratmen", 0x008C, Point3D.Zero, Point3D.Zero, 8, 24, 8, "ratmen da ruína", new string[] { "RuinRatman", "RuinRatman", "RuinZombie" }, new string[] { "RuinRatman", "RuinZombie" }));

            Register(CollectConfig(3006, 30, "Bosque calcinado", 0x0096, Point3D.Zero, Point3D.Zero, 8, 22, 7, "troncos calcinados",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("CharredLog", "tronco calcinado", 0x1BDD, 1102, "Axe") }));

            Register(CollectConfig(3007, 30, "Caverna de fungos", 0x0098, Point3D.Zero, Point3D.Zero, 8, 20, 6, "fungos flamejantes",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("BlazingMushroom", "fungo flamejante", 0x0D16, 1353, "Torch") }));

            Register(CollectConfig(3008, 30, "Pedreira quebrada", 0x009A, Point3D.Zero, Point3D.Zero, 8, 24, 8, "placas de minério pesado",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("HeavyOrePlate", "placa de minério", 0x1779, 2404, "Pickaxe") }));

            Register(CollectConfig(3009, 30, "Vale das raízes antigas", 0x009C, Point3D.Zero, Point3D.Zero, 8, 24, 8, "raízes antigas",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("AncientRoot", "raiz antiga", 0x0C9F, 2216, "Scythe") }));
        }

        private static void Register40()
        {
            Register(KillConfig(4001, 40, "Fortim dos condenados", 0x007C, Point3D.Zero, Point3D.Zero, 10, 32, 10, "condenados da ruína", new string[] { "DreadChampion", "BoneCaptain", "RuinOgre" }, new string[] { "DreadChampion", "BoneCaptain", "RuinOgre" }));
            Register(KillConfig(4002, 40, "Bastião das chamas", 0x007E, Point3D.Zero, Point3D.Zero, 10, 32, 10, "arcanistas em brasa", new string[] { "SmolderMage", "FirebrandBandit", "AshHarpy" }, new string[] { "SmolderMage", "FirebrandBandit", "AshHarpy" }));

            Register(CollectConfig(4003, 40, "Ruína coberta por vinhas", 0x007C, Point3D.Zero, Point3D.Zero, 10, 30, 10, "vinhas petrificadas",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("PetrifiedVine", "vinha petrificada", 0x0CA8, 2202, "Scythe") }));

            Register(CollectConfig(4004, 40, "Campo de geodos", 0x007E, Point3D.Zero, Point3D.Zero, 10, 30, 10, "geodos de mineração",
                new ReinoLotCollectibleSpawnEntry[] { Collectible("MineGeode", "geodo de mineração", 0x1363, 2415, "Pickaxe") }));
        }
    }
}
