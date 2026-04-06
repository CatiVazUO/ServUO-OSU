using Server;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Server.Custom.Reinos
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

        private static ReinoLotSpawnPointDefinition SpawnAt(int x, int y, int z, int range, int weight)
        {
            return new ReinoLotSpawnPointDefinition(x, y, z, range, weight);
        }

        private static ReinoLotMobSpawnEntry Killable(string typeName, string displayName, int hue, int weight, int x, int y, int z, int range, int spawnWeight)
        {
            return new ReinoLotMobSpawnEntry(typeName, displayName, hue, weight, SpawnAt(x, y, z, range, spawnWeight));
        }

        private static ReinoLotCollectibleSpawnEntry Collectable(string typeName, string displayName, int itemId, int hue, string requiredToolTypeName, int weight, int x, int y, int z, int range, int spawnWeight)
        {
            return new ReinoLotCollectibleSpawnEntry(typeName, displayName, itemId, hue, requiredToolTypeName, weight, SpawnAt(x, y, z, range, spawnWeight));
        }

        private static ReinoLotConfigDefinition KillConfig(int configId, int side, string name, int multiId, Point3D encounterOffset, Point3D spawnOffset, int spawnRange, int objectiveAmount, int spawnCount, string displayName, string[] spawnTypeNames, string[] objectiveTypeNames)
        {
            return KillConfig(configId, side, name, multiId, encounterOffset, spawnOffset, spawnRange, objectiveAmount, spawnCount, displayName, spawnTypeNames, objectiveTypeNames, 12.0, null);
        }

        private static ReinoLotConfigDefinition KillConfig(int configId, int side, string name, int multiId, Point3D encounterOffset, Point3D spawnOffset, int spawnRange, int objectiveAmount, int spawnCount, string displayName, string[] spawnTypeNames, string[] objectiveTypeNames, double respawnSeconds, ReinoLotMobSpawnEntry[] entries)
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
            def.ObjectiveAmount = objectiveAmount;
            def.SpawnCount = spawnCount;
            def.RespawnDelay = TimeSpan.FromSeconds(respawnSeconds <= 0 ? 12.0 : respawnSeconds);

            if (entries != null && entries.Length > 0)
            {
                def.MobEntries = entries;

                List<string> targets = new List<string>();
                for (int i = 0; i < entries.Length; i++)
                {
                    ReinoLotMobSpawnEntry entry = entries[i];
                    if (entry == null || String.IsNullOrWhiteSpace(entry.TypeName))
                        continue;

                    if (!targets.Contains(entry.TypeName))
                        targets.Add(entry.TypeName);
                }

                def.ObjectiveTargetTypeNames = objectiveTypeNames ?? targets.ToArray();
                return def;
            }

            def.ObjectiveTargetTypeNames = objectiveTypeNames ?? spawnTypeNames ?? new string[0];

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
            return CollectConfig(configId, side, name, multiId, encounterOffset, spawnOffset, spawnRange, objectiveAmount, spawnCount, displayName, entries, 12.0);
        }

        private static ReinoLotConfigDefinition CollectConfig(int configId, int side, string name, int multiId, Point3D encounterOffset, Point3D spawnOffset, int spawnRange, int objectiveAmount, int spawnCount, string displayName, ReinoLotCollectibleSpawnEntry[] entries, double respawnSeconds)
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
            def.RespawnDelay = TimeSpan.FromSeconds(respawnSeconds <= 0 ? 12.0 : respawnSeconds);

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
            Register(KillConfig(1501, 15, "Acampamento de saqueadores", 0x0064, new Point3D(0, 0, 0), Point3D.Zero, 4, 8, 4, "saqueadores",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("FreshBandit", "saqueador fresco", 2101, 3, -2, 1, 0, 2, 1),
                Killable("BoneCaptain", "capitão de ossos", 2106, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1502, 15, "Ruína infestada", 0x0066, new Point3D(0, 0, 0), Point3D.Zero, 4, 10, 4, "zumbis de ruína",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("RuinZombie", "zumbi de ruína", 2125, 3, -2, 1, 0, 2, 1),
                Killable("RuinZombie", "zumbi de ruína", 2125, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1503, 15, "Ossário recente", 0x0068, new Point3D(0, 0, 0), Point3D.Zero, 4, 10, 5, "fresh skeletons",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("FreshSkeleton", "esqueleto fresco", 2102, 3, -2, 1, 0, 2, 1),
                Killable("FreshSkeleton", "esqueleto fresco", 2102, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1504, 15, "Poleiro das harpias", 0x006A, new Point3D(0, 0, 0), Point3D.Zero, 4, 8, 3, "harpias cinzentas",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("AshHarpy", "harpia de cinza", 2202, 3, -2, 1, 0, 2, 1),
                Killable("AshHarpy", "harpia de cinza", 2202, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1505, 15, "Casebre dos magos", 0x006C, new Point3D(0, 0, 0), Point3D.Zero, 4, 8, 3, "magos perdidos",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("LostHedgeMage", "mago perdido", 2301, 3, -2, 1, 0, 2, 1),
                Killable("LostHedgeMage", "mago perdido", 2301, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1506, 15, "Covil de goblins", 0x006E, new Point3D(0, 0, 0), Point3D.Zero, 4, 12, 5, "goblins esfomeados",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("FeralGoblin", "goblin esfomeado", 2211, 3, -2, 1, 0, 2, 1),
                Killable("FeralGoblin", "goblin esfomeado", 2211, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1507, 15, "Toca de morcegos", 0x00A0, new Point3D(0, 0, 0), Point3D.Zero, 4, 14, 6, "mongbats doentios",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("BlightMongbat", "mongbat doentio", 1365, 3, -2, 1, 0, 2, 1),
                Killable("BlightMongbat", "mongbat doentio", 1365, 1, 4, 0, 0, 3, 1)
                    }));

            Register(KillConfig(1508, 15, "Ninho de ratmen", 0x00A2, new Point3D(0, 0, 0), Point3D.Zero, 4, 10, 4, "ratmen saqueadores",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("RuinRatman", "ratman da ruína", 2401, 3, -2, 1, 0, 2, 1),
                Killable("RuinRatman", "ratman da ruína", 2401, 1, 4, 0, 0, 3, 1)
                    }));

            Register(CollectConfig(1509, 15, "Bosque de galhos secos", 0x0064, Point3D.Zero, Point3D.Zero, 4, 10, 4, "galhos secos",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("DryBrambles", "galho seco", 0x0D15, 2101, "Scythe", 1, -2, 1, 0, 2, 1),
            Collectable("DryBrambles", "galho seco", 0x0D15, 2101, "Scythe", 1, 4, 0, 0, 3, 1)
                },
                40.0));

            Register(CollectConfig(1510, 15, "Jardim de fungos", 0x0066, Point3D.Zero, Point3D.Zero, 4, 10, 4, "fungos tóxicos",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("ToxicMushroom", "fungo tóxico", 0x0D16, 1272, "Torch", 1, -2, 1, 0, 2, 1),
            Collectable("ToxicMushroom", "fungo tóxico", 0x0D16, 1272, "Torch", 1, 4, 0, 0, 3, 1)
                },
                40.0));

            Register(CollectConfig(1511, 15, "Pedreira rasa", 0x0068, Point3D.Zero, Point3D.Zero, 4, 12, 4, "nódulos de minério ruim",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("ShallowOreNode", "nódulo de minério", 0x19B8, 2419, "Pickaxe", 1, -2, 1, 0, 2, 1),
            Collectable("ShallowOreNode", "nódulo de minério", 0x19B8, 2419, "Pickaxe", 1, 4, 0, 0, 3, 1)
                },
                40.0));

            Register(CollectConfig(1512, 15, "Matagal de raízes", 0x006A, Point3D.Zero, Point3D.Zero, 4, 12, 5, "raízes entranhadas",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("SnaredRoot", "raiz entranhada", 0x0C95, 2206, "Scythe", 1, -2, 1, 0, 2, 1),
            Collectable("SnaredRoot", "raiz entranhada", 0x0C95, 2206, "Scythe", 1, 4, 0, 0, 3, 1)
                },
                40.0));

            Register(CollectConfig(1513, 15, "Ninho de ovos ruins", 0x006C, Point3D.Zero, Point3D.Zero, 4, 8, 3, "ovos corrompidos",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("RottenEggNest", "ninho podre", 0x09B5, 2117, "Torch", 1, -2, 1, 0, 2, 1),
            Collectable("RottenEggNest", "ninho podre", 0x09B5, 2117, "Torch", 1, 4, 0, 0, 3, 1)
                },
                40.0));

            Register(CollectConfig(1514, 15, "Campina espinhosa", 0x006E, Point3D.Zero, Point3D.Zero, 4, 14, 5, "sarças agressivas",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("Hookthorn", "sarça agressiva", 0x0CA8, 2118, "Scythe", 1, -2, 1, 0, 2, 1),
            Collectable("Hookthorn", "sarça agressiva", 0x0CA8, 2118, "Scythe", 1, 4, 0, 0, 3, 1)
                },
                40.0));

            Register(CollectConfig(1515, 15, "Cristais do lodo", 0x00A0, Point3D.Zero, Point3D.Zero, 4, 10, 4, "cristais de lodo",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("SludgeCrystal", "cristal de lodo", 0x1F19, 1260, "Pickaxe", 1, -2, 1, 0, 2, 1),
            Collectable("SludgeCrystal", "cristal de lodo", 0x1F19, 1260, "Pickaxe", 1, 4, 0, 0, 3, 1)
                },
                40.0));
        }

        private static void Register20()
        {
            Register(KillConfig(2001, 20, "Ruína dos necromantes", 0x0074, Point3D.Zero, Point3D.Zero, 6, 16, 6, "capatazes ósseos",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("BoneCaptain", "capitão de ossos", 2106, 2, -2, 1, 0, 3, 1),
                Killable("FreshSkeleton", "esqueleto fresco", 2102, 1, 4, 0, 0, 4, 1)
                    }));

            Register(KillConfig(2002, 20, "Ruas tomadas", 0x0076, Point3D.Zero, Point3D.Zero, 6, 18, 6, "goblins ferozes",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("FeralGoblin", "goblin feroz", 2211, 3, -2, 1, 0, 3, 1),
                Killable("FeralGoblin", "goblin feroz", 2211, 1, 4, 0, 0, 4, 1)
                    }));

            Register(KillConfig(2003, 20, "Praça das harpias", 0x0078, Point3D.Zero, Point3D.Zero, 6, 14, 5, "harpias de cinza",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("AshHarpy", "harpia de cinza", 2202, 3, -2, 1, 0, 3, 1),
                Killable("AshHarpy", "harpia de cinza", 2202, 1, 4, 0, 0, 4, 1)
                    }));

            Register(KillConfig(2004, 20, "Acampamento ardente", 0x008C, Point3D.Zero, Point3D.Zero, 6, 16, 6, "bandidos incendiários",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("FirebrandBandit", "bandido incendiário", 1359, 2, -2, 1, 0, 3, 1),
                Killable("FreshBandit", "saqueador fresco", 2101, 1, 4, 0, 0, 4, 1)
                    }));

            Register(KillConfig(2005, 20, "Covil do lodo", 0x0096, Point3D.Zero, Point3D.Zero, 6, 16, 5, "ratos da ruína",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("RuinRatman", "ratman da ruína", 2401, 2, -2, 1, 0, 3, 1),
                Killable("RuinZombie", "zumbi de ruína", 2125, 1, 4, 0, 0, 4, 1)
                    }));

            Register(KillConfig(2006, 20, "Anel dos magos", 0x0098, Point3D.Zero, Point3D.Zero, 6, 12, 4, "magos perdidos",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("LostHedgeMage", "mago perdido", 2301, 3, -2, 1, 0, 3, 1),
                Killable("LostHedgeMage", "mago perdido", 2301, 1, 4, 0, 0, 4, 1)
                    }));

            Register(CollectConfig(2007, 20, "Campo de tocos", 0x009A, Point3D.Zero, Point3D.Zero, 6, 18, 6, "tocos retorcidos",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("WitheredStump", "toco retorcido", 0x0E57, 1109, "Axe", 1, -2, 1, 0, 3, 1),
            Collectable("WitheredStump", "toco retorcido", 0x0E57, 1109, "Axe", 1, 4, 0, 0, 4, 1)
                },
                40.0));

            Register(CollectConfig(2008, 20, "Lodaçal fúngico", 0x009C, Point3D.Zero, Point3D.Zero, 6, 16, 5, "fungos gordurosos",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("GreaseMushroom", "fungo gorduroso", 0x0D16, 1270, "Torch", 1, -2, 1, 0, 3, 1),
            Collectable("GreaseMushroom", "fungo gorduroso", 0x0D16, 1270, "Torch", 1, 4, 0, 0, 4, 1)
                },
                40.0));

            Register(CollectConfig(2009, 20, "Cascalho antigo", 0x009E, Point3D.Zero, Point3D.Zero, 6, 16, 5, "blocos de cascalho",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("AncientGravel", "bloco de cascalho", 0x1363, 2407, "Pickaxe", 1, -2, 1, 0, 3, 1),
            Collectable("AncientGravel", "bloco de cascalho", 0x1363, 2407, "Pickaxe", 1, 4, 0, 0, 4, 1)
                },
                40.0));

            Register(CollectConfig(2010, 20, "Jardim de espinhos", 0x00A0, Point3D.Zero, Point3D.Zero, 6, 18, 6, "espinhos endurecidos",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("HardenedThorn", "espinho endurecido", 0x0CA7, 2213, "Scythe", 1, -2, 1, 0, 3, 1),
            Collectable("HardenedThorn", "espinho endurecido", 0x0CA7, 2213, "Scythe", 1, 4, 0, 0, 4, 1)
                },
                40.0));

            Register(CollectConfig(2011, 20, "Taludes em brasa", 0x00A2, Point3D.Zero, Point3D.Zero, 6, 14, 4, "bolsões de gás incendiável",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("FireGasPod", "bolsão inflamável", 0x0F8B, 1359, "Torch", 1, -2, 1, 0, 3, 1),
            Collectable("FireGasPod", "bolsão inflamável", 0x0F8B, 1359, "Torch", 1, 4, 0, 0, 4, 1)
                },
                40.0));

            Register(CollectConfig(2012, 20, "Jazida rachada", 0x0064, Point3D.Zero, Point3D.Zero, 6, 18, 6, "cristas minerais rachadas",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("CrackedMineral", "crista mineral", 0x1367, 2418, "Pickaxe", 1, -2, 1, 0, 3, 1),
            Collectable("CrackedMineral", "crista mineral", 0x1367, 2418, "Pickaxe", 1, 4, 0, 0, 4, 1)
                },
                40.0));
        }

        private static void Register30()
        {
            Register(KillConfig(3001, 30, "Pátio dos capitães", 0x007A, Point3D.Zero, Point3D.Zero, 8, 24, 8, "capitães ósseos",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("BoneCaptain", "capitão de ossos", 2106, 2, -2, 1, 0, 4, 1),
                Killable("FreshSkeleton", "esqueleto fresco", 2102, 1, 4, 0, 0, 5, 1)
                    }));

            Register(KillConfig(3002, 30, "Encruzilhada bruta", 0x0074, Point3D.Zero, Point3D.Zero, 8, 24, 8, "ogros da ruína",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("RuinOgre", "ogro da ruína", 2415, 2, -2, 1, 0, 4, 1),
                Killable("FreshBandit", "saqueador fresco", 2101, 1, 4, 0, 0, 5, 1)
                    }));

            Register(KillConfig(3003, 30, "Torres do eco", 0x0076, Point3D.Zero, Point3D.Zero, 8, 20, 6, "arcanistas do eco",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("SmolderMage", "mago em brasa", 1353, 2, -2, 1, 0, 4, 1),
                Killable("LostHedgeMage", "mago perdido", 2301, 1, 4, 0, 0, 5, 1)
                    }));

            Register(KillConfig(3004, 30, "Círculo das harpias", 0x0078, Point3D.Zero, Point3D.Zero, 8, 22, 7, "harpias de fuligem",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("AshHarpy", "harpia de fuligem", 2202, 3, -2, 1, 0, 4, 1),
                Killable("AshHarpy", "harpia de fuligem", 2202, 1, 4, 0, 0, 5, 1)
                    }));

            Register(KillConfig(3005, 30, "Pântano dos ratmen", 0x008C, Point3D.Zero, Point3D.Zero, 8, 24, 8, "ratmen da ruína",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("RuinRatman", "ratman da ruína", 2401, 2, -2, 1, 0, 4, 1),
                Killable("RuinZombie", "zumbi de ruína", 2125, 1, 4, 0, 0, 5, 1)
                    }));

            Register(CollectConfig(3006, 30, "Bosque calcinado", 0x0096, Point3D.Zero, Point3D.Zero, 8, 22, 7, "troncos calcinados",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("CharredLog", "tronco calcinado", 0x1BDD, 1102, "Axe", 1, -2, 1, 0, 4, 1),
            Collectable("CharredLog", "tronco calcinado", 0x1BDD, 1102, "Axe", 1, 4, 0, 0, 5, 1)
                },
                40.0));

            Register(CollectConfig(3007, 30, "Caverna de fungos", 0x0098, Point3D.Zero, Point3D.Zero, 8, 20, 6, "fungos flamejantes",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("BlazingMushroom", "fungo flamejante", 0x0D16, 1353, "Torch", 1, -2, 1, 0, 4, 1),
            Collectable("BlazingMushroom", "fungo flamejante", 0x0D16, 1353, "Torch", 1, 4, 0, 0, 5, 1)
                },
                40.0));

            Register(CollectConfig(3008, 30, "Pedreira quebrada", 0x009A, Point3D.Zero, Point3D.Zero, 8, 24, 8, "placas de minério pesado",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("HeavyOrePlate", "placa de minério", 0x1779, 2404, "Pickaxe", 1, -2, 1, 0, 4, 1),
            Collectable("HeavyOrePlate", "placa de minério", 0x1779, 2404, "Pickaxe", 1, 4, 0, 0, 5, 1)
                },
                40.0));

            Register(CollectConfig(3009, 30, "Vale das raízes antigas", 0x009C, Point3D.Zero, Point3D.Zero, 8, 24, 8, "raízes antigas",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("AncientRoot", "raiz antiga", 0x0C9F, 2216, "Scythe", 1, -2, 1, 0, 4, 1),
            Collectable("AncientRoot", "raiz antiga", 0x0C9F, 2216, "Scythe", 1, 4, 0, 0, 5, 1)
                },
                40.0));
        }

        private static void Register40()
        {
            Register(KillConfig(4001, 40, "Fortim dos condenados", 0x007C, Point3D.Zero, Point3D.Zero, 10, 32, 10, "condenados da ruína",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("DreadChampion", "campeão da ruína", 1175, 2, -2, 1, 0, 5, 1),
                Killable("BoneCaptain", "capitão de ossos", 2106, 1, 4, 0, 0, 6, 1),
                Killable("RuinOgre", "ogro da ruína", 2415, 1, 0, -3, 0, 5, 1)
                    }));

            Register(KillConfig(4002, 40, "Bastião das chamas", 0x007E, Point3D.Zero, Point3D.Zero, 10, 32, 10, "arcanistas em brasa",
                null, null, 40.0,
                    new ReinoLotMobSpawnEntry[]
                    {
                Killable("SmolderMage", "mago em brasa", 1353, 2, -2, 1, 0, 5, 1),
                Killable("FirebrandBandit", "bandido incendiário", 1359, 1, 4, 0, 0, 6, 1),
                Killable("AshHarpy", "harpia de cinza", 2202, 1, 0, -3, 0, 5, 1)
                    }));

            Register(CollectConfig(4003, 40, "Ruína coberta por vinhas", 0x007C, Point3D.Zero, Point3D.Zero, 10, 30, 10, "vinhas petrificadas",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("PetrifiedVine", "vinha petrificada", 0x0CA8, 2202, "Scythe", 1, -2, 1, 0, 5, 1),
            Collectable("PetrifiedVine", "vinha petrificada", 0x0CA8, 2202, "Scythe", 1, 4, 0, 0, 6, 1)
                },
                40.0));

            Register(CollectConfig(4004, 40, "Campo de geodos", 0x007E, Point3D.Zero, Point3D.Zero, 10, 30, 10, "geodos de mineração",
                new ReinoLotCollectibleSpawnEntry[]
                {
            Collectable("MineGeode", "geodo de mineração", 0x1363, 2415, "Pickaxe", 1, -2, 1, 0, 5, 1),
            Collectable("MineGeode", "geodo de mineração", 0x1363, 2415, "Pickaxe", 1, 4, 0, 0, 6, 1)
                },
                40.0));
        }
    }
}
