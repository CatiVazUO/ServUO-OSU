using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Reinos;
using Server.Custom.Systems.SkillXP.Engine;
using Server.SkillXp;
using Server.Custom.Systems.Stables.Engine;

namespace Server.Custom.Systems.Stables.Engine
{
    public enum OSUStableServiceKind
    {
        None = 0,
        Breeding = 1,
        Castration = 2
    }

    public enum OSUStableServiceStage
    {
        None = 0,
        BreedingParents = 1,
        BreedingOffspring = 2
    }

    public sealed class OSUStableBreedGroup
    {
        public string Id;
        public string DisplayName;
        public int DefaultMaxBreeds;
        public bool FarmAnimal;
        public bool MountAnimal;
        public Type[] AllowedTypes;

        public OSUStableBreedGroup(string id, string displayName, int defaultMaxBreeds, bool farmAnimal, bool mountAnimal, params Type[] allowedTypes)
        {
            Id = id ?? String.Empty;
            DisplayName = displayName ?? id ?? String.Empty;
            DefaultMaxBreeds = Math.Max(0, defaultMaxBreeds);
            FarmAnimal = farmAnimal;
            MountAnimal = mountAnimal;
            AllowedTypes = allowedTypes ?? new Type[0];
        }

        public bool Allows(BaseCreature pet)
        {
            if (pet == null || AllowedTypes == null)
                return false;

            Type t = pet.GetType();

            for (int i = 0; i < AllowedTypes.Length; i++)
            {
                Type allowed = AllowedTypes[i];
                if (allowed != null && allowed.IsAssignableFrom(t))
                    return true;
            }

            return false;
        }

        public Type ChooseOffspringType(BaseCreature a, BaseCreature b)
        {
            Type ta = a != null ? a.GetType() : null;
            Type tb = b != null ? b.GetType() : null;

            if (ta != null && tb != null)
                return Utility.RandomBool() ? ta : tb;

            return ta ?? tb;
        }
    }

    public static class OSUStableBreedRegistry
    {
        private static readonly List<OSUStableBreedGroup> _groups = new List<OSUStableBreedGroup>();
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _groups.Clear();

            Register(new OSUStableBreedGroup(
                id: "horse",
                displayName: "Equinos",
                defaultMaxBreeds: 8,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(Horse),
                    typeof(PackHorse),
                    typeof(Palomino),
                    typeof(DreadWarhorse),
                    typeof(FireSteed),
                    typeof(Nightmare),
                    typeof(HorseFrisio),
                    typeof(HorseMustang),
                    typeof(HorsePuroSangue),
                    typeof(HorseMangaLarga),
                    typeof(HorseAndaluz),
                    typeof(HorseShire),
                    typeof(HorseArdennes),
                    typeof(HorseClaydesdale),
                    typeof(HorseGypsy),
                    typeof(HorseBelga),
                    typeof(HorseFrisioCarga)
                }));

            Register(new OSUStableBreedGroup(
                id: "llama",
                displayName: "Lhamas",
                defaultMaxBreeds: 7,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(Llama),
                    typeof(PackLlama),
                    typeof(RidableLlama)
                }));

            Register(new OSUStableBreedGroup(
                id: "ostard",
                displayName: "Ostards",
                defaultMaxBreeds: 7,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(DesertOstard),
                    typeof(ForestOstard),
                    typeof(FrenziedOstard)
                }));

            Register(new OSUStableBreedGroup(
                id: "ridgeback",
                displayName: "Ridgebacks",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(Ridgeback),
                    typeof(SavageRidgeback)
                }));

            Register(new OSUStableBreedGroup(
                id: "swamp_dragon",
                displayName: "Dragões do Pântano",
                defaultMaxBreeds: 4,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(SwampDragon),
                    typeof(ScaledSwampDragon)
                }));

            Register(new OSUStableBreedGroup(
                id: "kirin_unicorn",
                displayName: "Unicórnios e Kirins",
                defaultMaxBreeds: 4,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(Kirin),
                    typeof(Unicorn)
                }));

            Register(new OSUStableBreedGroup(
                id: "bovine",
                displayName: "Bovinos",
                defaultMaxBreeds: 6,
                farmAnimal: true,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Cow),
                    typeof(Bull)
                }));

            Register(new OSUStableBreedGroup(
                id: "goat",
                displayName: "Caprinos",
                defaultMaxBreeds: 6,
                farmAnimal: true,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Goat),
                    typeof(MountainGoat)
                }));

            Register(new OSUStableBreedGroup(
                id: "sheep",
                displayName: "Ovinos",
                defaultMaxBreeds: 6,
                farmAnimal: true,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Sheep),
                    typeof(OsseinRam)
                }));

            Register(new OSUStableBreedGroup(
                id: "pig",
                displayName: "Suínos",
                defaultMaxBreeds: 6,
                farmAnimal: true,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Pig),
                    typeof(Boar)
                }));

            Register(new OSUStableBreedGroup(
                id: "chicken",
                displayName: "Aves de terreiro",
                defaultMaxBreeds: 8,
                farmAnimal: true,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Chicken)
                }));

            Register(new OSUStableBreedGroup(
                id: "chicken_lizard",
                displayName: "Lagartos-galinha",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(BattleChickenLizard),
                    typeof(ChickenLizard)
                }));

            Register(new OSUStableBreedGroup(
                id: "deer",
                displayName: "Cervídeos",
                defaultMaxBreeds: 6,
                farmAnimal: true,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Hind),
                    typeof(GreatHart)
                }));

            Register(new OSUStableBreedGroup(
                id: "bear",
                displayName: "Ursos",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(BlackBear),
                    typeof(BrownBear),
                    typeof(GrizzlyBear),
                    typeof(PolarBear)
                }));

            Register(new OSUStableBreedGroup(
                id: "canine",
                displayName: "Caninos",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Dog),
                    typeof(GreyWolf),
                    typeof(TimberWolf),
                    typeof(WhiteWolf),
                    typeof(DireWolf),
                    typeof(TsukiWolf),
                    typeof(HellHound),
                    typeof(IceHound),
                    typeof(DragonWolf),
                    typeof(CuSidhe),
                    typeof(BloodFox)
                }));

            Register(new OSUStableBreedGroup(
                id: "feline",
                displayName: "Felinos",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Cat),
                    typeof(HellCat),
                    typeof(PredatorHellCat),
                    typeof(Cougar),
                    typeof(Panther),
                    typeof(SnowLeopard),
                    typeof(Lion),
                    typeof(SabertoothedTiger),
                    typeof(WildTiger),
                    typeof(SkeletalCat)
                }));

            Register(new OSUStableBreedGroup(
                id: "dragon",
                displayName: "Dragões e dracos",
                defaultMaxBreeds: 3,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Dragon),
                    typeof(GreaterDragon),
                    typeof(Drake),
                    typeof(ColdDrake),
                    typeof(CrimsonDrake),
                    typeof(FrostDragon),
                    typeof(PlatinumDrake),
                    typeof(StygianDrake),
                    typeof(ShadowWyrm),
                    typeof(WhiteWyrm),
                    typeof(SerpentineDragon),
                    typeof(Reptalon),
                    typeof(Hiryu),
                    typeof(LesserHiryu),
                    typeof(Skree)
                }));

            Register(new OSUStableBreedGroup(
                id: "beetle",
                displayName: "Besouros",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Beetle),
                    typeof(DeathwatchBeetle),
                    typeof(FireBeetle),
                    typeof(IronBeetle),
                    typeof(RuneBeetle)
                }));

            Register(new OSUStableBreedGroup(
                id: "spider",
                displayName: "Aranhas",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(DreadSpider),
                    typeof(FrostSpider),
                    typeof(GiantSpider),
                    typeof(WolfSpider)
                }));

            Register(new OSUStableBreedGroup(
                id: "bird",
                displayName: "Aves",
                defaultMaxBreeds: 6,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Bird),
                    typeof(TropicalBird),
                    typeof(Eagle),
                    typeof(Parrot),
                    typeof(Phoenix)
                }));

            Register(new OSUStableBreedGroup(
                id: "rodent",
                displayName: "Roedores e pequenos mamíferos",
                defaultMaxBreeds: 8,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(GiantRat),
                    typeof(Rat),
                    typeof(Sewerrat),
                    typeof(Rabbit),
                    typeof(JackRabbit),
                    typeof(Squirrel),
                    typeof(Ferret)
                }));

            Register(new OSUStableBreedGroup(
                id: "frog",
                displayName: "Rãs e sapos",
                defaultMaxBreeds: 6,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(BullFrog),
                    typeof(GiantToad)
                }));

            Register(new OSUStableBreedGroup(
                id: "reptile",
                displayName: "Répteis",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Alligator),
                    typeof(LavaLizard),
                    typeof(Slith),
                    typeof(StoneSlith),
                    typeof(Snake)
                }));

            Register(new OSUStableBreedGroup(
                id: "bat",
                displayName: "Mongbats",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Mongbat),
                    typeof(GreaterMongbat),
                    typeof(StrongMongbat)
                }));

            Register(new OSUStableBreedGroup(
                id: "boura",
                displayName: "Bouras",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(LowlandBoura),
                    typeof(RuddyBoura),
                    typeof(HighPlainsBoura)
                }));

            Register(new OSUStableBreedGroup(
                id: "crab",
                displayName: "Caranguejos",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(CoconutCrab),
                    typeof(HungryCoconutCrab)
                }));

            Register(new OSUStableBreedGroup(
                id: "ooze",
                displayName: "Limos",
                defaultMaxBreeds: 8,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Slime),
                    typeof(CorrosiveSlime)
                }));

            Register(new OSUStableBreedGroup(
                id: "scorpion",
                displayName: "Escorpiões",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Scorpion)
                }));

            Register(new OSUStableBreedGroup(
                id: "worm_mite",
                displayName: "Vermes e ácaros",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(GiantIceWorm),
                    typeof(FrostMite)
                }));

            Register(new OSUStableBreedGroup(
                id: "daemon",
                displayName: "Criaturas infernais",
                defaultMaxBreeds: 3,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Imp),
                    typeof(BakeKitsune)
                }));

            Register(new OSUStableBreedGroup(
                id: "gaman",
                displayName: "Gaman",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Gaman)
                }));

            Register(new OSUStableBreedGroup(
                id: "gorilla",
                displayName: "Gorilas",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Gorilla)
                }));

            Register(new OSUStableBreedGroup(
                id: "eowmu",
                displayName: "Eowmus",
                defaultMaxBreeds: 4,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Eowmu)
                }));

            Register(new OSUStableBreedGroup(
                id: "lasher",
                displayName: "Lashers",
                defaultMaxBreeds: 4,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Lasher)
                }));

            Register(new OSUStableBreedGroup(
                id: "skittering_hopper",
                displayName: "Skittering Hoppers",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(SkitteringHopper)
                }));

            Register(new OSUStableBreedGroup(
                id: "triceratops",
                displayName: "Triceratops",
                defaultMaxBreeds: 3,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Triceratops)
                }));

            Register(new OSUStableBreedGroup(
                id: "triton",
                displayName: "Tritões",
                defaultMaxBreeds: 3,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Triton)
                }));

            Register(new OSUStableBreedGroup(
                id: "walrus",
                displayName: "Morsas",
                defaultMaxBreeds: 5,
                farmAnimal: false,
                mountAnimal: false,
                allowedTypes: new Type[]
                {
                    typeof(Walrus)
                }));

            Register(new OSUStableBreedGroup(
                id: "windrunner",
                displayName: "Windrunners",
                defaultMaxBreeds: 4,
                farmAnimal: false,
                mountAnimal: true,
                allowedTypes: new Type[]
                {
                    typeof(Windrunner)
                }));

        }

        private static void Register(OSUStableBreedGroup group)
        {
            if (group != null)
                _groups.Add(group);
        }

        public static OSUStableBreedGroup GetGroup(BaseCreature pet)
        {
            Initialize();

            if (pet == null)
                return null;

            for (int i = 0; i < _groups.Count; i++)
            {
                OSUStableBreedGroup g = _groups[i];
                if (g != null && g.Allows(pet))
                    return g;
            }

            return null;
        }
    }

    public static class OSUStablePetSystem
    {
        public const int DefaultPlayerFollowerSlots = 2;
        public const int DefaultPetLives = 3;
        public const int TrainingFeatId = 110201;
        public const int BreedingFeatId = 110202;
        public const int CastrationFeatId = 110203;
        public const int BrandingFeatId = 110204;

        public const int TrainingCostGold = 150;
        public const int BreedingCostGold = 350;
        public const int CastrationCostGold = 800;
        public const int BrandingCostGold = 600;
        public const int LateClaimFeeGold = 50;

        public static readonly TimeSpan BreedingParentsDuration = TimeSpan.FromSeconds(30.0);
        public static readonly TimeSpan BreedingOffspringDuration = TimeSpan.FromSeconds(30.0);
        public static readonly TimeSpan CastrationDuration = TimeSpan.FromSeconds(15.0);
        public static readonly TimeSpan OffspringGraceWindow = TimeSpan.FromSeconds(20.0);
        public static readonly TimeSpan DownedLifetime = TimeSpan.FromHours(24.0);
        public static readonly TimeSpan CommandCooldownWhenLowInt = TimeSpan.FromSeconds(3.0);
        public static readonly TimeSpan FarmPastureDelay = TimeSpan.FromMinutes(1.0);
        public static readonly TimeSpan FarmPassiveXPDelay = TimeSpan.FromDays(1.0);

        private static readonly Dictionary<int, DateTime> m_FarmPassiveXPNextUtc = new Dictionary<int, DateTime>();
        private static readonly Dictionary<string, DateTime> m_LoyaltyCareCooldowns = new Dictionary<string, DateTime>();

        private static string FarmPassiveXPFilePath
        {
            get { return Path.Combine(Core.BaseDirectory, "Saves", "OSU_FarmPassiveXP.bin"); }
        }

        private static readonly string[] m_AbilityPool = new string[]
        {
            "Fôlego de Guerra",
            "Pele Resistente",
            "Passo Veloz",
            "Instinto de Caça",
            "Comando Firme",
            "Musculatura Densa"
        };

        public static void Initialize()
        {
            OSUStableBreedRegistry.Initialize();
            EventSink.Login += OnLogin;
            EventSink.CharacterCreated += OnCharacterCreated;
            EventSink.WorldLoad += OnWorldLoad;
            EventSink.WorldSave += OnWorldSave;
            LoadFarmPassiveXP();
            Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), PulseWorldState);
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            ApplyFollowerCap(pm);

            foreach (Mobile m in World.Mobiles.Values)
            {
                BaseCreature bc = m as BaseCreature;
                if (bc == null || bc.Deleted)
                    continue;

                if (bc.ControlMaster == pm)
                    EnsureInitialized(bc);
            }
        }

        private static void OnCharacterCreated(CharacterCreatedEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm != null)
                ApplyFollowerCap(pm);
        }

        private static void OnWorldLoad()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), PulseWorldState);
        }


        private static void OnWorldSave(WorldSaveEventArgs e)
        {
            SaveFarmPassiveXP();
        }

        private static void LoadFarmPassiveXP()
        {
            m_FarmPassiveXPNextUtc.Clear();

            try
            {
                if (!File.Exists(FarmPassiveXPFilePath))
                    return;

                using (FileStream fs = File.OpenRead(FarmPassiveXPFilePath))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    int count = br.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        int serial = br.ReadInt32();
                        long ticks = br.ReadInt64();

                        if (serial != 0 && ticks > 0)
                            m_FarmPassiveXPNextUtc[serial] = new DateTime(ticks, DateTimeKind.Utc);
                    }
                }
            }
            catch
            {
                m_FarmPassiveXPNextUtc.Clear();
            }
        }

        private static void SaveFarmPassiveXP()
        {
            try
            {
                string dir = Path.GetDirectoryName(FarmPassiveXPFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = File.Create(FarmPassiveXPFilePath))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(1);
                    bw.Write(m_FarmPassiveXPNextUtc.Count);

                    foreach (KeyValuePair<int, DateTime> kv in m_FarmPassiveXPNextUtc)
                    {
                        bw.Write(kv.Key);
                        bw.Write(kv.Value.ToUniversalTime().Ticks);
                    }
                }
            }
            catch
            {
            }
        }

        private static bool HasSkillToCommand(BaseCreature pet, Mobile from)
        {
            if (pet == null || from == null)
                return false;

            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            double required = pet.CurrentTameSkill;

            if (required <= 0.0)
                return true;

            return from.Skills[SkillName.AnimalTaming].Value >= required
                || from.Skills[SkillName.AnimalLore].Value >= required;
        }

        private static void ProcessFarmPassiveXP(BaseCreature pet)
        {
            if (pet == null || pet.Deleted || !pet.Controlled || !IsFarmAnimal(pet))
                return;

            if (pet.ControlMaster == null && !pet.OSUPetMarked)
                return;

            EnsureInitialized(pet);

            int serial = pet.Serial.Value;
            DateTime now = DateTime.UtcNow;

            DateTime next;
            if (!m_FarmPassiveXPNextUtc.TryGetValue(serial, out next) || next == DateTime.MinValue)
            {
                m_FarmPassiveXPNextUtc[serial] = now + FarmPassiveXPDelay;
                return;
            }

            int safety = 0;

            while (now >= next && safety < 60)
            {
                int required = Math.Max(1, pet.OSUPetNextLevelXP);
                int xp = Math.Max(1, (int)Math.Ceiling(required / 30.0));

                AddPetXP(pet, xp, false);

                next = next + FarmPassiveXPDelay;
                safety++;
            }

            m_FarmPassiveXPNextUtc[serial] = next;
        }

        public static TimeSpan GetFarmProductionDelay(BaseCreature pet, TimeSpan baseDelay)
        {
            if (pet == null || baseDelay <= TimeSpan.Zero)
                return baseDelay;

            EnsureInitialized(pet);

            int level = Math.Max(1, Math.Min(10, pet.OSUPetLevel));
            double t = (level - 1) / 9.0;

            // Nível 1 = tempo normal. Nível 10 = metade do tempo.
            double factor = 1.0 - (0.50 * t);

            return TimeSpan.FromSeconds(Math.Max(60.0, baseDelay.TotalSeconds * factor));
        }

        public static int ScaleFarmDeathResource(BaseCreature pet, int baseAmount, string resourceKind)
        {
            if (pet == null || baseAmount <= 0)
                return baseAmount;

            EnsureInitialized(pet);

            if (!IsFarmAnimal(pet))
                return baseAmount;

            int level = Math.Max(1, Math.Min(10, pet.OSUPetLevel));
            double t = (level - 1) / 9.0;

            if ((pet is Pig || pet is Boar) && String.Equals(resourceKind, "meat", StringComparison.OrdinalIgnoreCase))
                return Math.Max(baseAmount, (int)Math.Round(2.0 + (28.0 * t)));

            if (pet is Chicken && String.Equals(resourceKind, "meat", StringComparison.OrdinalIgnoreCase))
                return Math.Max(baseAmount, (int)Math.Round(2.0 + (8.0 * t)));

            if (pet is Chicken && String.Equals(resourceKind, "feathers", StringComparison.OrdinalIgnoreCase))
                return Math.Max(baseAmount, (int)Math.Round(baseAmount * (1.0 + (2.0 * t))));

            double multiplier = 1.0 + (2.5 * t); // Nível 10 = 3.5x nos recursos de carcaça.
            return Math.Max(baseAmount, (int)Math.Round(baseAmount * multiplier));
        }

        private static void PulseWorldState()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                BaseCreature pet = m as BaseCreature;
                if (pet == null || pet.Deleted || !pet.Tamable)
                    continue;

                EnsureInitialized(pet);
                ProcessPastureState(pet);
                ProcessFarmPassiveXP(pet);
            }
        }

        public static void ApplyFollowerCap(PlayerMobile pm)
        {
            if (pm == null || pm.AccessLevel > AccessLevel.Player)
                return;

            pm.FollowersMax = DefaultPlayerFollowerSlots;
        }

        public static void EnsureInitialized(BaseCreature pet)
        {
            if (pet == null || pet.Deleted || !pet.Tamable)
                return;

            if (!pet.OSUPetInitialized)
            {
                pet.OSUPetInitialized = true;
                pet.OSUPetLevel = pet.OSUPetLevel > 0 ? pet.OSUPetLevel : 1;
                pet.OSUPetXP = Math.Max(0, pet.OSUPetXP);
                pet.OSUPetNextLevelXP = pet.OSUPetNextLevelXP > 0 ? pet.OSUPetNextLevelXP : GetRequiredXPForLevel(1);
                pet.OSUPetLevelOneStr = pet.OSUPetLevelOneStr > 0 ? pet.OSUPetLevelOneStr : pet.RawStr;
                pet.OSUPetLevelOneDex = pet.OSUPetLevelOneDex > 0 ? pet.OSUPetLevelOneDex : pet.RawDex;
                pet.OSUPetLevelOneInt = pet.OSUPetLevelOneInt > 0 ? pet.OSUPetLevelOneInt : pet.RawInt;
                pet.OSUPetLivesMax = pet.OSUPetLivesMax > 0 ? pet.OSUPetLivesMax : DefaultPetLives;
                pet.OSUPetLivesRemaining = pet.OSUPetLivesRemaining > 0 ? pet.OSUPetLivesRemaining : pet.OSUPetLivesMax;
                pet.OSUPetLastGainLevel = Math.Max(0, pet.OSUPetLastGainLevel);
                pet.IsBonded = false;

                OSUStableBreedGroup group = OSUStableBreedRegistry.GetGroup(pet);
                if (group != null)
                {
                    pet.OSUPetBreedGroup = group.Id;
                    if (pet.OSUPetBreedCountMax <= 0)
                        pet.OSUPetBreedCountMax = group.DefaultMaxBreeds;
                }
            }

            if (pet.OSUPetMarked && IsFarmAnimal(pet))
            {
                if (pet.OSUPetStoredControlSlots <= 0)
                    pet.OSUPetStoredControlSlots = Math.Max(1, pet.ControlSlots);

                pet.ControlSlots = 0;
            }

            if (pet.OSUPetAwaitingResurrection && pet.OSUPetDownedUntilUtc != DateTime.MinValue && DateTime.UtcNow >= pet.OSUPetDownedUntilUtc)
                PermanentKillExpiredPet(pet);

            ProcessPastureState(pet);
        }

        public static void AfterDeserialize(BaseCreature pet)
        {
            EnsureInitialized(pet);
        }

        public static int GetRequiredXPForLevel(int currentLevel)
        {
            if (currentLevel <= 1)
                return 2000;

            int value = 2000;
            for (int i = 2; i <= currentLevel; i++)
                value *= 2;

            return value;
        }

        public static bool CanGainMoreLevels(BaseCreature pet)
        {
            if (pet == null)
                return false;

            int cap = 10;
            PlayerMobile owner = pet.ControlMaster as PlayerMobile;

            if (owner == null)
            {
                if (IsFarmAnimal(pet) && pet.OSUPetMarked)
                    return pet.OSUPetLevel < 10;

                return false;
            }

            if (owner.AccessLevel <= AccessLevel.Player && !owner.HasOSUFeat(TrainingFeatId))
                cap = 7;

            return pet.OSUPetLevel < cap;
        }

        public static void HandleKillXP(BaseCreature victim, int baseXp)
        {
            if (victim == null || victim.Deleted || baseXp <= 0 || victim.DamageEntries == null)
                return;

            int totalDamage = 0;

            for (int i = 0; i < victim.DamageEntries.Count; i++)
            {
                DamageEntry de = victim.DamageEntries[i];
                if (de == null || de.Damager == null || de.DamageGiven <= 0)
                    continue;

                totalDamage += de.DamageGiven;
            }

            if (totalDamage <= 0)
                return;

            Dictionary<PlayerMobile, int> ownerAwarded = new Dictionary<PlayerMobile, int>();
            Dictionary<BaseCreature, int> petAwarded = new Dictionary<BaseCreature, int>();

            for (int i = 0; i < victim.DamageEntries.Count; i++)
            {
                DamageEntry de = victim.DamageEntries[i];
                BaseCreature pet = de != null ? de.Damager as BaseCreature : null;

                if (pet == null || pet.Deleted || !pet.Controlled || pet.ControlMaster == null || de.DamageGiven <= 0)
                    continue;

                EnsureInitialized(pet);

                PlayerMobile owner = pet.ControlMaster as PlayerMobile;
                if (owner == null)
                    continue;

                double pct = (double)de.DamageGiven / (double)totalDamage;
                double required = (pet is IMount) ? 0.30 : 0.40;

                if (pct < required)
                    continue;

                int petShare = (int)Math.Round(baseXp * pct);
                petShare = Math.Max(1, petShare);
                int ownerShare = Math.Max(0, baseXp - petShare);

                if (ownerAwarded.ContainsKey(owner))
                    ownerAwarded[owner] = Math.Max(ownerAwarded[owner], ownerShare);
                else
                    ownerAwarded[owner] = ownerShare;

                if (petAwarded.ContainsKey(pet))
                    petAwarded[pet] = Math.Max(petAwarded[pet], petShare);
                else
                    petAwarded[pet] = petShare;
            }

            foreach (KeyValuePair<PlayerMobile, int> kv in ownerAwarded)
            {
                if (kv.Key != null && kv.Value > 0)
                    SkillXPSystem.AddGeneralXP(kv.Key, kv.Value);
            }

            foreach (KeyValuePair<BaseCreature, int> kv in petAwarded)
            {
                if (kv.Key != null && kv.Value > 0)
                    AddPetXP(kv.Key, kv.Value, true);
            }
        }

        public static void AddPetXP(BaseCreature pet, int amount, bool notifyOwner)
        {
            if (pet == null || pet.Deleted || amount <= 0)
                return;

            EnsureInitialized(pet);

            if (!CanGainMoreLevels(pet))
                return;

            pet.OSUPetXP += amount;

            PlayerMobile owner = pet.ControlMaster as PlayerMobile;
            if (notifyOwner && owner != null)
                owner.SendMessage(0x44, "{0} ganhou {1} XP animal.", pet.Name, amount);

            while (CanGainMoreLevels(pet) && pet.OSUPetXP >= pet.OSUPetNextLevelXP)
            {
                pet.OSUPetXP -= pet.OSUPetNextLevelXP;
                pet.OSUPetLevel++;
                pet.OSUPetNextLevelXP = GetRequiredXPForLevel(pet.OSUPetLevel);
                RollLevelGain(pet);

                if (owner != null)
                    owner.SendMessage(0x35, "{0} chegou ao nível {1}.", pet.Name, pet.OSUPetLevel);
            }
        }

        private static void RollLevelGain(BaseCreature pet)
        {
            int points = Utility.RandomMinMax(2, 7);

            if (pet.OSUPetCastrated)
                points++;

            int bucket = Utility.Random(3);
            int addStr = 0;
            int addDex = 0;
            int addInt = 0;

            if (bucket == 0)
                addStr = points;
            else if (bucket == 1)
                addDex = points;
            else
                addInt = points;

            if (pet.OSUPetLastTrainedLevel >= pet.OSUPetLastGainLevel)
            {
                pet.OSUPetLastGainStr = 0;
                pet.OSUPetLastGainDex = 0;
                pet.OSUPetLastGainInt = 0;
            }

            pet.RawStr += addStr;
            pet.RawDex += addDex;
            pet.RawInt += addInt;

            if (addStr > 0)
            {
                pet.DamageMin = Math.Max(1, pet.DamageMin + Math.Max(0, addStr / 2));
                pet.DamageMax = Math.Max(pet.DamageMin, pet.DamageMax + addStr);
            }

            pet.Hits = Math.Min(pet.HitsMax, pet.Hits + addStr);
            pet.Stam = Math.Min(pet.StamMax, pet.Stam + addDex);
            pet.Mana = Math.Min(pet.ManaMax, pet.Mana + addInt);

            pet.OSUPetLastGainStr += addStr;
            pet.OSUPetLastGainDex += addDex;
            pet.OSUPetLastGainInt += addInt;
            pet.OSUPetLastGainLevel = pet.OSUPetLevel;
            pet.InvalidateProperties();
        }

        public static bool HasTrainablePoints(BaseCreature pet)
        {
            return pet != null
                && pet.OSUPetLastGainLevel == pet.OSUPetLevel
                && pet.OSUPetLastTrainedLevel != pet.OSUPetLevel
                && (pet.OSUPetLastGainStr + pet.OSUPetLastGainDex + pet.OSUPetLastGainInt) > 0;
        }

        public static bool TryRedistributeLastLevel(BaseCreature pet, int newStr, int newDex, int newInt, PlayerMobile owner, out string reason)
        {
            reason = null;

            if (pet == null || owner == null)
            {
                reason = "Pet inválido.";
                return false;
            }

            EnsureInitialized(pet);

            if (!HasTrainablePoints(pet))
            {
                reason = "Esse animal não tem pontos do último nível para redistribuir.";
                return false;
            }

            int oldTotal = pet.OSUPetLastGainStr + pet.OSUPetLastGainDex + pet.OSUPetLastGainInt;
            int newTotal = Math.Max(0, newStr) + Math.Max(0, newDex) + Math.Max(0, newInt);

            if (oldTotal != newTotal)
            {
                reason = "A soma precisa ser exatamente " + oldTotal + " pontos.";
                return false;
            }

            if (!WithdrawGoldAndDeposit(owner, GetGovernmentCityIdFromOwnerPet(pet), TrainingCostGold, out reason))
                return false;

            pet.RawStr -= pet.OSUPetLastGainStr;
            pet.RawDex -= pet.OSUPetLastGainDex;
            pet.RawInt -= pet.OSUPetLastGainInt;

            pet.RawStr += Math.Max(0, newStr);
            pet.RawDex += Math.Max(0, newDex);
            pet.RawInt += Math.Max(0, newInt);

            pet.OSUPetLastGainStr = Math.Max(0, newStr);
            pet.OSUPetLastGainDex = Math.Max(0, newDex);
            pet.OSUPetLastGainInt = Math.Max(0, newInt);
            pet.OSUPetLastTrainedLevel = pet.OSUPetLevel;

            TryRollSpecialAbility(pet, owner);
            pet.InvalidateProperties();
            reason = "Treino aplicado.";
            return true;
        }

        private static void TryRollSpecialAbility(BaseCreature pet, PlayerMobile owner)
        {
            if (pet == null)
                return;

            bool atFive = pet.OSUPetLevel >= 5 && String.IsNullOrWhiteSpace(pet.OSUPetAbilitySlot5);
            bool atTen = pet.OSUPetLevel >= 10 && String.IsNullOrWhiteSpace(pet.OSUPetAbilitySlot10);

            if (!atFive && !atTen)
                return;

            if (Utility.RandomDouble() > 0.50)
                return;

            string pick = m_AbilityPool[Utility.Random(m_AbilityPool.Length)];

            if (atFive)
            {
                pet.OSUPetAbilitySlot5 = pick;
                if (owner != null)
                    owner.SendMessage(0x59, "{0} despertou a habilidade especial: {1}.", pet.Name, pick);
                return;
            }

            if (atTen)
            {
                pet.OSUPetAbilitySlot10 = pick;
                if (owner != null)
                    owner.SendMessage(0x59, "{0} despertou a habilidade especial: {1}.", pet.Name, pick);
            }
        }

        public static bool CheckCommandGate(BaseCreature pet, Mobile from, ref bool result)
        {
            if (pet == null || from == null)
                return false;

            EnsureInitialized(pet);

            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                result = true;
                return true;
            }

            if (pet.ControlMaster != from)
                return false;

            if (!HasSkillToCommand(pet, from))
            {
                from.SendMessage(0x22, "Você possui este animal, mas não tem Animal Taming ou Animal Lore suficiente para que ele obedeça.");
                result = false;
                return true;
            }

            if (pet.OSUPetMarked && IsFarmAnimal(pet))
            {
                from.SendMessage(0x22, "Animais de fazenda marcados não obedecem comandos.");
                result = false;
                return true;
            }

            if (pet.RawInt >= 50)
                return false;

            if (pet.OSUPetLastCommandUtc > DateTime.UtcNow)
            {
                from.SendMessage(0x22, "Esse animal ainda está processando o último comando.");
                result = false;
                return true;
            }

            pet.OSUPetLastCommandUtc = DateTime.UtcNow + CommandCooldownWhenLowInt;

            double chance = 0.20 + (Math.Min(50, Math.Max(0, pet.RawInt)) / 50.0) * 0.75;

            if (Utility.RandomDouble() <= chance)
            {
                result = true;
                return true;
            }

            from.SendMessage(0x22, "O animal parece confuso e ignora a ordem.");
            result = false;
            return true;
        }

        public static void AppendSingleClickInfo(BaseCreature pet, Mobile from)
        {
            if (pet == null || from == null)
                return;

            EnsureInitialized(pet);

            if (pet.OSUPetMarked && !String.IsNullOrWhiteSpace(pet.OSUPetBrandOwnerName))
                pet.PrivateOverheadMessage(MessageType.Regular, 0x59, false, "Marcado para " + pet.OSUPetBrandOwnerName, from.NetState);

            if (pet.Controlled)
                pet.PrivateOverheadMessage(MessageType.Regular, 0x44, false, String.Format("Nv {0} - XP {1}/{2} - Vidas {3}/{4}", pet.OSUPetLevel, pet.OSUPetXP, pet.OSUPetNextLevelXP, pet.OSUPetLivesRemaining, pet.OSUPetLivesMax), from.NetState);
        }

public static bool TryCreateKnockoutState(BaseCreature pet)
{
    if (pet == null || pet.Deleted || !pet.Controlled || pet.ControlMaster == null || !pet.Tamable)
        return false;

    EnsureInitialized(pet);

    if (pet.OSUPetLivesRemaining <= 1)
    {
        pet.OSUPetLivesRemaining = Math.Max(0, pet.OSUPetLivesRemaining - 1);
        pet.CorpseNameOverride = BuildMarkedCorpseName(pet);
        return false;
    }

    pet.OSUPetLivesRemaining = Math.Max(0, pet.OSUPetLivesRemaining - 1);
    pet.CorpseNameOverride = BuildMarkedCorpseName(pet);

    Corpse corpse = new Corpse(pet, new List<Item>());
    corpse.OSUKnockoutCorpse = true;
    corpse.Name = pet.Name;
    corpse.BeginDecay(DownedLifetime);
    corpse.MoveToWorld(pet.Location, pet.Map);
    pet.Corpse = corpse;

    pet.Hits = 0;
    pet.Stam = 0;
    pet.Mana = 0;
    pet.Poison = null;
    pet.Combatant = null;
    pet.Warmode = false;
    pet.IsDeadPet = true;
    pet.Hidden = true;
    pet.OSUPetAwaitingResurrection = true;
    pet.OSUPetDownedUntilUtc = DateTime.UtcNow + DownedLifetime;
    pet.Internalize();

    PlayerMobile owner = pet.ControlMaster as PlayerMobile;
    if (owner != null)
        owner.SendMessage(0x22, "{0} caiu em combate. Você tem 24 horas para reanimá-lo antes da morte definitiva.", pet.Name);

    return true;
}

public static void OnKnockoutCorpseDeleted(Corpse corpse)
{
    if (corpse == null)
        return;

    BaseCreature pet = corpse.Owner as BaseCreature;
    if (pet == null || pet.Deleted)
        return;

    EnsureInitialized(pet);

    if (pet.OSUPetAwaitingResurrection)
        PermanentKillExpiredPet(pet);
}

public static string BuildMarkedCorpseName(BaseCreature pet)
{
            if (pet == null)
                return null;

            if (pet.OSUPetMarked && !String.IsNullOrWhiteSpace(pet.OSUPetBrandOwnerName))
                return String.Format("corpo de {0} (animal marcado de {1})", pet.Name, pet.OSUPetBrandOwnerName);

            return null;
        }

        public static bool TryResurrectFromCorpse(Corpse corpse, Mobile from)
        {
            if (corpse == null || from == null)
                return false;

            BaseCreature pet = corpse.Owner as BaseCreature;
            if (pet == null)
                return false;

            EnsureInitialized(pet);

            if (!pet.OSUPetAwaitingResurrection)
                return false;

            PlayerMobile owner = pet.ControlMaster as PlayerMobile;
            if (owner == null || owner != from)
            {
                from.SendMessage(0x22, "Somente o dono pode reanimar esse animal.");
                return true;
            }

            if (pet.OSUPetDownedUntilUtc != DateTime.MinValue && DateTime.UtcNow >= pet.OSUPetDownedUntilUtc)
            {
                PermanentKillExpiredPet(pet);
                if (!corpse.Deleted)
                    corpse.Delete();
                from.SendMessage(0x22, "Você demorou demais. O animal morreu de forma definitiva.");
                return true;
            }

            if (from.Backpack == null || !from.Backpack.ConsumeTotal(typeof(Bandage), 10))
            {
                from.SendMessage(0x22, "Você precisa de 10 bandagens para reanimar esse animal caído.");
                return true;
            }

            Point3D loc = corpse.Location;
            Map map = corpse.Map;

            pet.MoveToWorld(loc, map);
            pet.Hidden = false;
            pet.ResurrectPet();
            pet.Hits = Math.Max(10, pet.HitsMax / 3);
            pet.OSUPetAwaitingResurrection = false;
            pet.OSUPetDownedUntilUtc = DateTime.MinValue;
            pet.CorpseNameOverride = null;
            pet.Corpse = null;

            if (!corpse.Deleted)
                corpse.Delete();

            from.SendMessage(0x59, "Você conseguiu reanimar seu animal.");
            return true;
        }

        private static void PermanentKillExpiredPet(BaseCreature pet)
        {
            if (pet == null || pet.Deleted)
                return;

            pet.OSUPetLivesRemaining = 0;
            pet.OSUPetAwaitingResurrection = false;
            pet.Delete();
        }

        public static bool BlockCarveIfProtected(BaseCreature pet, Corpse corpse, Mobile from)
        {
            if (pet == null || corpse == null || from == null)
                return false;

            EnsureInitialized(pet);

            if (pet.OSUPetAwaitingResurrection && pet.OSUPetLivesRemaining > 0)
            {
                from.SendMessage(0x22, "Você não pode esquartejar um animal que ainda tem vidas restantes.");
                return true;
            }

            return false;
        }

        public static bool IsFarmAnimal(BaseCreature pet)
        {
            OSUStableBreedGroup group = OSUStableBreedRegistry.GetGroup(pet);
            return group != null && group.FarmAnimal;
        }

        public static bool IsMountAnimal(BaseCreature pet)
        {
            OSUStableBreedGroup group = OSUStableBreedRegistry.GetGroup(pet);
            return group != null && group.MountAnimal;
        }

        public static bool CanPlayerMarkMoreFarmAnimals(PlayerMobile pm)
        {
            if (pm == null)
                return false;

            int count = 0;
            foreach (Mobile m in World.Mobiles.Values)
            {
                BaseCreature bc = m as BaseCreature;
                if (bc == null || bc.Deleted)
                    continue;

                if (!bc.OSUPetMarked || !IsFarmAnimal(bc))
                    continue;

                if (bc.OSUPetBrandOwnerSerial == pm.Serial.Value)
                    count++;
            }

            return count < 5;
        }

        public static bool CanUseStableService(PlayerMobile pm, int featId)
        {
            if (pm == null)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

            return featId <= 0 || pm.HasOSUFeat(featId);
        }

        public static bool WithdrawGoldAndDeposit(PlayerMobile from, int cityId, int amount, out string reason)
        {
            reason = null;

            if (from == null || amount <= 0)
                return true;

            if (!Banker.Withdraw(from, amount, true))
            {
                reason = "Você não tem ouro suficiente no banco.";
                return false;
            }

            if (cityId >= 0)
                ReinoTreasurySystem.RecordDonationToKingdom(cityId, amount, 0, 0, 0);

            return true;
        }

        public static int GetGovernmentCityIdFromOwnerPet(BaseCreature pet)
        {
            if (pet == null || pet.Map == null || pet.Map == Map.Internal)
                return -1;

            return ReinoMilitarySystem.ResolveCityIdAt(pet.Location, pet.Map);
        }

        public static string GetLoyaltyLabel(BaseCreature pet)
        {
            if (pet == null)
                return "desconhecida";

            int loyalty = pet.Loyalty;

            if (loyalty >= 100)
                return "perfeitamente leal";
            if (loyalty >= 90)
                return "muito leal";
            if (loyalty >= 70)
                return "leal";
            if (loyalty >= 50)
                return "insegura";
            if (loyalty >= 30)
                return "desconfiada";
            if (loyalty > 0)
                return "quase fugindo";

            return "sem lealdade";
        }

        public static bool TryGainLoyaltyFromCare(BaseCreature pet, Mobile from, int amount, string actionId, TimeSpan cooldown, out string reason)
        {
            reason = null;

            if (pet == null || from == null || pet.Deleted)
            {
                reason = "Animal inválido.";
                return false;
            }

            EnsureInitialized(pet);

            if (!pet.Controlled || pet.ControlMaster != from)
            {
                reason = "Você só pode cuidar de um animal domado por você.";
                return false;
            }

            if (pet.IsDeadPet || !pet.Alive)
            {
                reason = "Esse animal não pode receber esse cuidado agora.";
                return false;
            }

            if (!from.InRange(pet, 2))
            {
                reason = "Você está longe demais do animal.";
                return false;
            }

            if (amount <= 0)
                amount = 1;

            string key = pet.Serial.Value.ToString() + ":" + (actionId ?? "care");
            DateTime next;

            if (m_LoyaltyCareCooldowns.TryGetValue(key, out next) && DateTime.UtcNow < next)
            {
                TimeSpan left = next - DateTime.UtcNow;
                reason = "Esse animal já recebeu esse cuidado recentemente. Tente novamente em " + Math.Max(1, (int)Math.Ceiling(left.TotalMinutes)) + " minuto(s).";
                return false;
            }

            m_LoyaltyCareCooldowns[key] = DateTime.UtcNow + cooldown;

            int before = pet.Loyalty;
            pet.Loyalty = Math.Min(BaseCreature.MaxLoyalty, pet.Loyalty + amount);

            if (pet.Loyalty > before)
                reason = "A lealdade do animal aumentou para " + pet.Loyalty + "/" + BaseCreature.MaxLoyalty + " (" + GetLoyaltyLabel(pet) + ").";
            else
                reason = "O animal já está no máximo de lealdade.";

            pet.PlaySound(pet.GetIdleSound());
            pet.InvalidateProperties();
            return true;
        }

        public static bool CanAnalyzeWithMuzzle(Item muzzle, BaseCreature pet, Mobile from, out string reason)
        {
            reason = null;
            if (muzzle == null || pet == null || from == null)
            {
                reason = "Alvo inválido.";
                return false;
            }

            EnsureInitialized(pet);

            if (!pet.Controlled || pet.ControlMaster != from)
            {
                reason = "A focinheira só pode analisar um animal já domado por você.";
                return false;
            }

            return true;
        }

        public static void OnFarmResourceProduced(BaseCreature pet, Mobile actor, int units)
        {
            if (pet == null || units <= 0)
                return;

            EnsureInitialized(pet);
            AddPetXP(pet, Math.Max(1, units * 10), false);
        }

        public static bool CanTakeFarmResources(BaseCreature pet, Mobile actor, out string reason)
        {
            reason = null;

            if (pet == null || actor == null)
                return true;

            EnsureInitialized(pet);

            if (pet.OSUPetMarked && pet.OSUPetBrandOwnerSerial != actor.Serial.Value)
            {
                reason = "Somente o dono da marca pode recolher recursos desse animal.";
                return false;
            }

            return true;
        }

        public static bool TryMarkAnimal(PlayerMobile pm, BaseCreature pet, int cityId, out string reason)
        {
            reason = null;

            if (pm == null || pet == null)
            {
                reason = "Animal inválido.";
                return false;
            }

            EnsureInitialized(pet);

            if (!pet.Controlled || pet.ControlMaster != pm)
            {
                reason = "Você só pode marcar um animal já domado e sob seu controle.";
                return false;
            }

            if (!IsFarmAnimal(pet) && !IsMountAnimal(pet))
            {
                reason = "Por enquanto só é possível marcar montarias e animais de fazenda.";
                return false;
            }

            if (pet.OSUPetMarked)
            {
                reason = "Esse animal já está marcado.";
                return false;
            }

            if (IsFarmAnimal(pet) && !CanPlayerMarkMoreFarmAnimals(pm))
            {
                reason = "Você já atingiu o limite atual de 5 animais de fazenda marcados.";
                return false;
            }

            if (!WithdrawGoldAndDeposit(pm, cityId, BrandingCostGold, out reason))
                return false;

            pet.OSUPetMarked = true;
            pet.OSUPetBrandOwnerSerial = pm.Serial.Value;
            pet.OSUPetBrandOwnerName = pm.Name;
            pet.CorpseNameOverride = BuildMarkedCorpseName(pet);

            if (IsFarmAnimal(pet))
            {
                if (pet.ControlMaster != null)
                    pet.RemoveFollowers();

                if (pet.OSUPetStoredControlSlots <= 0)
                    pet.OSUPetStoredControlSlots = Math.Max(1, pet.ControlSlots);

                pet.ControlSlots = 0;
                pet.OSUPetPastureAtUtc = DateTime.UtcNow + FarmPastureDelay;

                pm.Emote("*marca o animal com cuidado*");
                pm.SendMessage(0x59, "Esse animal foi marcado. Em 10 minutos ele vai se soltar para começar a pastar.");
            }
            else
            {
                pm.Emote("*marca o animal*");
            }

            pet.InvalidateProperties();
            reason = "Animal marcado com sucesso.";
            return true;
        }

        public static bool TryStartCastration(PlayerMobile pm, BaseCreature pet, int cityId, out string reason)
        {
            reason = null;

            if (pm == null || pet == null)
            {
                reason = "Animal inválido.";
                return false;
            }

            EnsureInitialized(pet);

            if (!pet.Controlled || pet.ControlMaster != pm)
            {
                reason = "Você só pode castrar um animal domado por você.";
                return false;
            }

            if (pet.OSUPetCastrated)
            {
                reason = "Esse animal já foi castrado.";
                return false;
            }

            if (pet.OSUPetBreedCount > 0)
            {
                reason = "Esse animal já cruzou pelo menos uma vez. A castração só pode ser feita antes do primeiro cruzamento.";
                return false;
            }

            if (pet.OSUPetServiceKind != (int)OSUStableServiceKind.None)
            {
                reason = "Esse animal já está em outro serviço do estábulo.";
                return false;
            }

            if (!HasCastrationItems(pm))
            {
                reason = "Você precisa levar os itens de castração corretos: kit de sutura de castração, faca de castração e bandagens de castração.";
                return false;
            }

            if (!WithdrawGoldAndDeposit(pm, cityId, CastrationCostGold, out reason))
                return false;

            ConsumeCastrationItems(pm);
            StartService(pet, pm, OSUStableServiceKind.Castration, cityId, CastrationDuration);
            reason = "Castramento iniciado.";
            return true;
        }

        private static bool HasCastrationItems(PlayerMobile pm)
        {
            if (pm == null || pm.Backpack == null)
                return false;

            return pm.Backpack.GetAmount(typeof(OSUStableCastrationKnife)) > 0
                && pm.Backpack.GetAmount(typeof(OSUStableCastrationSewingKit)) > 0
                && pm.Backpack.GetAmount(typeof(OSUStableCastrationBandagePack)) > 0;
        }

        private static void ConsumeCastrationItems(PlayerMobile pm)
        {
            if (pm == null || pm.Backpack == null)
                return;

            pm.Backpack.ConsumeTotal(typeof(OSUStableCastrationKnife), 1);
            pm.Backpack.ConsumeTotal(typeof(OSUStableCastrationSewingKit), 1);
            pm.Backpack.ConsumeTotal(typeof(OSUStableCastrationBandagePack), 1);
        }

        private static void StartService(BaseCreature pet, PlayerMobile owner, OSUStableServiceKind kind, int cityId, TimeSpan duration)
        {
            if (pet == null || owner == null)
                return;

            pet.OSUPetServiceKind = (int)kind;
            pet.OSUPetServiceOwnerSerial = owner.Serial.Value;
            pet.OSUPetServiceCityId = cityId;
            pet.OSUPetServiceReadyUtc = DateTime.UtcNow + duration;
            pet.OSUPetServiceClaimFromUtc = kind == OSUStableServiceKind.Castration ? DateTime.MinValue : pet.OSUPetServiceReadyUtc;
            pet.OSUPetServiceRoomIndex = -1;
            pet.OSUPetServiceStage = (int)OSUStableServiceStage.None;

            if (kind == OSUStableServiceKind.Castration)
                pet.Internalize();
        }

        public static bool TryStartBreeding(PlayerMobile pm, Mobile stableNpc, BaseCreature first, BaseCreature second, int cityId, out string reason)
        {
            reason = null;

            if (pm == null || stableNpc == null || first == null || second == null || first == second)
            {
                reason = "Seleção inválida.";
                return false;
            }

            EnsureInitialized(first);
            EnsureInitialized(second);

            if (!first.Controlled || !second.Controlled || first.ControlMaster != pm || second.ControlMaster != pm)
            {
                reason = "Os dois animais precisam estar domados por você.";
                return false;
            }

            if (first.OSUPetServiceKind != 0 || second.OSUPetServiceKind != 0)
            {
                reason = "Um dos animais já está em outro serviço do estábulo.";
                return false;
            }

            if (first.Female == second.Female)
            {
                reason = "Você precisa selecionar um macho e uma fêmea.";
                return false;
            }

            if (first.OSUPetCastrated || second.OSUPetCastrated)
            {
                reason = "Animais castrados não podem cruzar.";
                return false;
            }

            if (first.OSUPetSterile || second.OSUPetSterile)
            {
                reason = "Um dos animais é estéril.";
                return false;
            }

            if (first.OSUPetBreedCount >= first.OSUPetBreedCountMax || second.OSUPetBreedCount >= second.OSUPetBreedCountMax)
            {
                reason = "Um dos animais já atingiu o limite de cruzamentos.";
                return false;
            }

            OSUStableBreedGroup a = OSUStableBreedRegistry.GetGroup(first);
            OSUStableBreedGroup b = OSUStableBreedRegistry.GetGroup(second);

            if (a == null || b == null || !String.Equals(a.Id, b.Id, StringComparison.OrdinalIgnoreCase))
            {
                reason = "Esses animais não pertencem ao mesmo tipo de cruzamento.";
                return false;
            }

            if (!WithdrawGoldAndDeposit(pm, cityId, BreedingCostGold, out reason))
                return false;

            BaseCreature female = first.Female ? first : second;
            BaseCreature male = first.Female ? second : first;

            int roomIndex;
            if (!TryFindAvailableBreedingRoom(cityId, out roomIndex))
            {
                reason = "Todos os quartinhos de cruzamento desse estábulo estão ocupados no momento.";
                return false;
            }

            female.OSUPetBreedCount++;
            male.OSUPetBreedCount++;

            ConfigurePendingOffspring(female, male, a);

            StartService(female, pm, OSUStableServiceKind.Breeding, cityId, BreedingParentsDuration);
            StartService(male, pm, OSUStableServiceKind.Breeding, cityId, BreedingParentsDuration);

            female.OSUPetServiceStage = (int)OSUStableServiceStage.BreedingParents;
            male.OSUPetServiceStage = (int)OSUStableServiceStage.BreedingParents;
            female.OSUPetServiceRoomIndex = roomIndex;
            male.OSUPetServiceRoomIndex = roomIndex;
            female.OSUPetServicePartnerSerial = male.Serial.Value;
            male.OSUPetServicePartnerSerial = female.Serial.Value;

            MovePetToBreedingRoom(female, pm, stableNpc, roomIndex, true);
            MovePetToBreedingRoom(male, pm, stableNpc, roomIndex, false);

            reason = "Cruzamento iniciado. Os dois animais foram levados ao quartinho do estábulo.";
            return true;
        }

        private static bool TryFindAvailableBreedingRoom(int cityId, out int roomIndex)
        {
            int count = Math.Min(
                Math.Min(EstabuloAuroraDefinition.BreedingRoomFemaleOffsets.Length, EstabuloAuroraDefinition.BreedingRoomMaleOffsets.Length),
                EstabuloAuroraDefinition.BreedingRoomOffspringOffsets.Length);

            for (int i = 0; i < count; i++)
            {
                if (!IsBreedingRoomOccupied(cityId, i))
                {
                    roomIndex = i;
                    return true;
                }
            }

            roomIndex = -1;
            return false;
        }

        private static bool IsBreedingRoomOccupied(int cityId, int roomIndex)
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                BaseCreature pet = m as BaseCreature;
                if (pet == null || pet.Deleted)
                    continue;

                if (pet.OSUPetServiceKind == (int)OSUStableServiceKind.Breeding
                    && pet.OSUPetServiceCityId == cityId
                    && pet.OSUPetServiceRoomIndex == roomIndex)
                    return true;
            }

            return false;
        }

        private static void MovePetToBreedingRoom(BaseCreature pet, PlayerMobile owner, Mobile stableNpc, int roomIndex, bool female)
        {
            if (pet == null)
                return;

            StoreAndReleaseFollowerSlots(pet);

            pet.ControlTarget = null;
            pet.ControlOrder = OrderType.None;
            pet.Controlled = false;
            pet.Frozen = true;
            pet.Combatant = null;
            pet.Warmode = false;
            pet.Hidden = false;
            pet.Home = pet.Location;
            pet.RangeHome = 0;

            Point3D loc = female ? GetBreedingFemaleLocation(stableNpc, roomIndex) : GetBreedingMaleLocation(stableNpc, roomIndex);
            pet.MoveToWorld(loc, stableNpc.Map);
        }

        private static Point3D GetBreedingFemaleLocation(Mobile stableNpc, int roomIndex)
        {
            Point3D p = EstabuloAuroraDefinition.BreedingRoomFemaleOffsets[Math.Max(0, Math.Min(roomIndex, EstabuloAuroraDefinition.BreedingRoomFemaleOffsets.Length - 1))];
            return new Point3D(stableNpc.X + p.X, stableNpc.Y + p.Y, stableNpc.Z + p.Z);
        }

        private static Point3D GetBreedingMaleLocation(Mobile stableNpc, int roomIndex)
        {
            Point3D p = EstabuloAuroraDefinition.BreedingRoomMaleOffsets[Math.Max(0, Math.Min(roomIndex, EstabuloAuroraDefinition.BreedingRoomMaleOffsets.Length - 1))];
            return new Point3D(stableNpc.X + p.X, stableNpc.Y + p.Y, stableNpc.Z + p.Z);
        }

        private static Point3D GetBreedingOffspringLocation(Mobile stableNpc, int roomIndex)
        {
            Point3D p = EstabuloAuroraDefinition.BreedingRoomOffspringOffsets[Math.Max(0, Math.Min(roomIndex, EstabuloAuroraDefinition.BreedingRoomOffspringOffsets.Length - 1))];
            return new Point3D(stableNpc.X + p.X, stableNpc.Y + p.Y, stableNpc.Z + p.Z);
        }

        private static Point3D GetBreedingReleaseLocation(Mobile stableNpc, int roomIndex)
        {
            Point3D p = EstabuloAuroraDefinition.BreedingRoomReleaseOffsets[Math.Max(0, Math.Min(roomIndex, EstabuloAuroraDefinition.BreedingRoomReleaseOffsets.Length - 1))];
            return new Point3D(stableNpc.X + p.X, stableNpc.Y + p.Y, stableNpc.Z + p.Z);
        }

        private static void ConfigurePendingOffspring(BaseCreature female, BaseCreature male, OSUStableBreedGroup group)
        {
            Type offspringType = group != null ? group.ChooseOffspringType(female, male) : female.GetType();

            if (offspringType == typeof(HorseFrisioCarga))
                offspringType = typeof(HorseFrisio);
            int str = (female.OSUPetLevelOneStr + male.OSUPetLevelOneStr) / 2;
            int dex = (female.OSUPetLevelOneDex + male.OSUPetLevelOneDex) / 2;
            int intel = (female.OSUPetLevelOneInt + male.OSUPetLevelOneInt) / 2;

            ApplyEliteParentBonus(female, male, ref str, ref dex, ref intel);

            int maxBreeds = Math.Min(female.OSUPetBreedCountMax, male.OSUPetBreedCountMax) / 2;
            bool sterile = maxBreeds <= 0;

            female.OSUPetPendingOffspringTypeName = offspringType != null ? offspringType.FullName : female.GetType().FullName;
            female.OSUPetPendingOffspringFemale = Utility.RandomBool();
            female.OSUPetPendingOffspringStr = Math.Max(1, str);
            female.OSUPetPendingOffspringDex = Math.Max(1, dex);
            female.OSUPetPendingOffspringInt = Math.Max(1, intel);
            female.OSUPetPendingOffspringBreedMax = Math.Max(0, maxBreeds);
            female.OSUPetPendingOffspringGroup = group != null ? group.Id : String.Empty;
            female.OSUPetSterile = female.OSUPetSterile || false;
            if (sterile)
                female.OSUPetPendingOffspringBreedMax = 0;
        }

        private static void ApplyEliteParentBonus(BaseCreature a, BaseCreature b, ref int str, ref int dex, ref int intel)
        {
            int bonus = 0;
            bonus += GetEliteParentBonus(a);
            bonus += GetEliteParentBonus(b);

            if (bonus <= 0)
                return;

            int best = 0;
            int bestValue = str;

            if (dex > bestValue)
            {
                best = 1;
                bestValue = dex;
            }

            if (intel > bestValue)
                best = 2;

            if (best == 0)
                str += bonus;
            else if (best == 1)
                dex += bonus;
            else
                intel += bonus;
        }

        private static int GetEliteParentBonus(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            if (pet.OSUPetLevel >= 10)
                return 6;
            if (pet.OSUPetLevel >= 9)
                return 4;
            if (pet.OSUPetLevel >= 8)
                return 2;

            return 0;
        }

        public static List<BaseMount> GetReadyServicePets(PlayerMobile pm, int cityId)
        {
            List<BaseMount> list = new List<BaseMount>();

            if (pm == null)
                return list;

            foreach (Mobile m in World.Mobiles.Values)
            {
                BaseMount pet = m as BaseMount;
                if (pet == null || pet.Deleted)
                    continue;

                if (pet.OSUPetServiceOwnerSerial != pm.Serial.Value || pet.OSUPetServiceKind == 0)
                    continue;

                if (cityId >= 0 && pet.OSUPetServiceCityId != cityId)
                    continue;

                if (DateTime.UtcNow < pet.OSUPetServiceReadyUtc)
                    continue;

                if (pet.OSUPetServiceKind == (int)OSUStableServiceKind.Breeding)
                {
                    if (pet.OSUPetServiceStage == (int)OSUStableServiceStage.BreedingParents)
                    {
                        if (!pet.Female)
                            continue;
                    }
                    else if (pet.OSUPetServiceStage != (int)OSUStableServiceStage.BreedingOffspring)
                        continue;
                }

                list.Add(pet);
            }

            list.Sort(delegate (BaseMount a, BaseMount b)
            {
                if (a == null && b == null)
                    return 0;

                if (a == null)
                    return 1;

                if (b == null)
                    return -1;

                int cmp = DateTime.Compare(a.OSUPetServiceReadyUtc, b.OSUPetServiceReadyUtc);
                if (cmp != 0)
                    return cmp;

                return a.Serial.Value.CompareTo(b.Serial.Value);
            });

            return list;
        }

        public static int GetLateFee(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            if (pet.OSUPetServiceKind == (int)OSUStableServiceKind.Breeding && pet.OSUPetServiceStage != (int)OSUStableServiceStage.BreedingOffspring)
                return 0;

            if (pet.OSUPetServiceClaimFromUtc == DateTime.MinValue || DateTime.UtcNow <= pet.OSUPetServiceClaimFromUtc)
                return 0;

            TimeSpan late = DateTime.UtcNow - pet.OSUPetServiceClaimFromUtc;
            int steps = Math.Max(1, (int)Math.Floor(late.TotalSeconds / Math.Max(1.0, OffspringGraceWindow.TotalSeconds)));
            return steps * LateClaimFeeGold;
        }

        public static bool TryClaimReadyService(PlayerMobile pm, Mobile stableNpc, BaseMount pet, out string reason)
        {
            reason = null;

            if (pm == null || stableNpc == null || pet == null)
            {
                reason = "Serviço inválido.";
                return false;
            }

            if (pet.OSUPetServiceOwnerSerial != pm.Serial.Value)
            {
                reason = "Esse serviço não pertence a você.";
                return false;
            }

            if (DateTime.UtcNow < pet.OSUPetServiceReadyUtc)
            {
                reason = "Esse serviço ainda não ficou pronto.";
                return false;
            }

            int lateFee = GetLateFee(pet);
            if (lateFee > 0 && !WithdrawGoldAndDeposit(pm, pet.OSUPetServiceCityId, lateFee, out reason))
                return false;

            if (pet.OSUPetServiceKind == (int)OSUStableServiceKind.Castration)
                return ClaimCastration(pm, stableNpc, pet, out reason);

            if (pet.OSUPetServiceKind == (int)OSUStableServiceKind.Breeding)
            {
                if (pet.OSUPetServiceStage == (int)OSUStableServiceStage.BreedingParents)
                    return ClaimBreedingParents(pm, stableNpc, pet, out reason);

                if (pet.OSUPetServiceStage == (int)OSUStableServiceStage.BreedingOffspring)
                    return ClaimBreedingOffspring(pm, stableNpc, pet, out reason);
            }

            reason = "Serviço desconhecido.";
            return false;
        }

        private static bool ClaimCastration(PlayerMobile pm, Mobile stableNpc, BaseCreature pet, out string reason)
        {
            reason = null;
            pet.OSUPetCastrated = true;
            pet.OSUPetServiceKind = 0;
            pet.OSUPetServiceOwnerSerial = 0;
            pet.OSUPetServiceReadyUtc = DateTime.MinValue;
            pet.OSUPetServiceClaimFromUtc = DateTime.MinValue;
            pet.OSUPetServiceCityId = -1;
            ReleasePetNearNpc(pet, stableNpc, pm);
            pet.InvalidateProperties();
            reason = "Seu animal foi devolvido após a castração.";
            return true;
        }

        private static bool SpawnBreedingOffspringFromPending
            (PlayerMobile owner, Mobile stableNpc, BaseMount female, BaseMount male, int cityId,
            int roomIndex, out BaseMount baby, out string reason)
        {
            reason = null;
            baby = null;

            Type babyType = ResolveMobileType(female.OSUPetPendingOffspringTypeName);
            if (babyType == null || !typeof(BaseCreature).IsAssignableFrom(babyType))
            {
                reason = "O tipo do filhote não pôde ser resolvido.";
                return false;
            }

            baby = Activator.CreateInstance(babyType) as BaseMount;
            if (baby == null)
            {
                reason = "Não foi possível criar o filhote.";
                return false;
            }

            CopyHorseVisualFromParent(baby, female, male);

            baby.Female = female.OSUPetPendingOffspringFemale;
            baby.RawStr = Math.Max(1, female.OSUPetPendingOffspringStr);
            baby.RawDex = Math.Max(1, female.OSUPetPendingOffspringDex);
            baby.RawInt = Math.Max(1, female.OSUPetPendingOffspringInt);
            baby.Hits = baby.HitsMax;
            baby.Stam = baby.StamMax;
            baby.Mana = baby.ManaMax;

            baby.Tamable = false;
            baby.Controlled = false;
            baby.ControlMaster = null;
            baby.ControlTarget = null;
            baby.ControlOrder = OrderType.None;
            baby.Frozen = true;
            baby.Blessed = true;

            baby.OSUPetInitialized = true;
            baby.OSUPetLevel = 1;
            baby.OSUPetXP = 0;
            baby.OSUPetNextLevelXP = GetRequiredXPForLevel(1);
            baby.OSUPetLevelOneStr = baby.RawStr;
            baby.OSUPetLevelOneDex = baby.RawDex;
            baby.OSUPetLevelOneInt = baby.RawInt;
            baby.OSUPetBreedCount = 0;
            baby.OSUPetBreedCountMax = Math.Max(0, female.OSUPetPendingOffspringBreedMax);
            baby.OSUPetSterile = baby.OSUPetBreedCountMax <= 0;
            baby.OSUPetBreedGroup = female.OSUPetPendingOffspringGroup ?? String.Empty;
            baby.OSUPetLivesMax = DefaultPetLives;
            baby.OSUPetLivesRemaining = DefaultPetLives;

            baby.OSUPetServiceKind = (int)OSUStableServiceKind.Breeding;
            baby.OSUPetServiceStage = (int)OSUStableServiceStage.BreedingOffspring;
            baby.OSUPetServiceOwnerSerial = owner.Serial.Value;
            baby.OSUPetServiceCityId = cityId;
            baby.OSUPetServiceReadyUtc = DateTime.UtcNow + BreedingOffspringDuration;
            baby.OSUPetServiceClaimFromUtc = baby.OSUPetServiceReadyUtc;
            baby.OSUPetServiceRoomIndex = roomIndex;

            Point3D babyLoc = GetBreedingOffspringLocation(stableNpc, roomIndex);
            baby.MoveToWorld(babyLoc, stableNpc.Map);

            return true;
        }

        private static bool ClaimBreedingParents(PlayerMobile pm, Mobile stableNpc, BaseMount female, out string reason)
        {
            reason = null;

            BaseMount male = World.FindMobile((Serial)female.OSUPetServicePartnerSerial) as BaseMount;
            if (male == null || male.Deleted)
            {
                reason = "O parceiro desse cruzamento não foi encontrado.";
                return false;
            }

            int roomIndex = female.OSUPetServiceRoomIndex;
            int cityId = female.OSUPetServiceCityId;

            BaseMount baby;
            if (!SpawnBreedingOffspringFromPending(pm, stableNpc, female, male, cityId, roomIndex, out baby, out reason))
                return false;

            ResetServiceFlags(male);
            RestoreFollowerSlots(male);
            male.Controlled = true;
            male.ControlTarget = pm;
            male.ControlOrder = OrderType.Follow;
            male.Frozen = false;
            male.Blessed = false;

            if (male.ControlMaster != pm)
                male.ControlMaster = pm;
            else
                male.AddFollowers();

            ReleasePetFromBreedingRoom(male, stableNpc, pm, roomIndex);

            ResetServiceFlags(female);
            RestoreFollowerSlots(female);
            female.Controlled = true;
            female.ControlTarget = pm;
            female.ControlOrder = OrderType.Follow;
            female.Frozen = false;
            female.Blessed = false;

            if (female.ControlMaster != pm)
                female.ControlMaster = pm;
            else
                female.AddFollowers();

            ReleasePetFromBreedingRoom(female, stableNpc, pm, roomIndex);

            OpenBreedingRoomDoorsForClaim(stableNpc, roomIndex);

            reason = "Os pais foram liberados. O filhote agora está no quartinho aguardando o segundo prazo.";
            return true;
        }

        private static bool ClaimBreedingOffspring(PlayerMobile pm, Mobile stableNpc, BaseCreature baby, out string reason)
        {
            reason = null;

            if (pm.Followers + Math.Max(1, baby.ControlSlots) > pm.FollowersMax)
            {
                reason = "Você não tem slots livres para pegar o filhote.";
                return false;
            }

            int roomIndex = baby.OSUPetServiceRoomIndex;

            baby.Tamable = true;
            baby.ControlMaster = pm;
            baby.Controlled = true;
            baby.ControlTarget = pm;
            baby.ControlOrder = OrderType.Follow;
            baby.Frozen = false;
            baby.Blessed = false;
            baby.Hidden = false;
            RestoreFollowerSlots(baby);
            ResetServiceFlags(baby);
            ReleasePetFromBreedingRoom(baby, stableNpc, pm, roomIndex);

            OpenBreedingRoomDoorsForClaim(stableNpc, roomIndex);
            reason = "O filhote foi entregue para você.";
            return true;
        }

        private static void OpenBreedingRoomDoorsForClaim(Mobile stableNpc, int roomIndex)
        {
            if (stableNpc == null || stableNpc.Map == null || stableNpc.Map == Map.Internal)
                return;

            SetBreedingRoomDoors(stableNpc, roomIndex, false, true);

            Timer.DelayCall(TimeSpan.FromMinutes(10.0), delegate
            {
                bool occupied = false;

                foreach (Mobile m in World.Mobiles.Values)
                {
                    BaseCreature pet = m as BaseCreature;
                    if (pet == null || pet.Deleted)
                        continue;

                    if (pet.OSUPetServiceKind == (int)OSUStableServiceKind.Breeding &&
                        pet.OSUPetServiceRoomIndex == roomIndex &&
                        pet.OSUPetServiceCityId >= 0 &&
                        stableNpc.Map == pet.Map)
                    {
                        occupied = true;
                        break;
                    }
                }

                SetBreedingRoomDoors(stableNpc, roomIndex, occupied, false);
            });
        }

        private static void SetBreedingRoomDoors(Mobile stableNpc, int roomIndex, bool locked, bool open)
        {
            if (stableNpc == null || stableNpc.Map == null || stableNpc.Map == Map.Internal)
                return;

            Point3D center = GetBreedingOffspringLocation(stableNpc, roomIndex);
            IPooledEnumerable eable = stableNpc.Map.GetItemsInRange(center, 3);

            foreach (object obj in eable)
            {
                BaseDoor door = obj as BaseDoor;
                if (door == null || door.Deleted)
                    continue;

                door.Locked = locked;

                if (open && !door.Open)
                    door.Open = true;
                else if (!open && door.Open)
                    door.Open = false;
            }

            eable.Free();
        }

        private static void ResetServiceFlags(BaseCreature pet)
        {
            if (pet == null)
                return;

            pet.OSUPetServiceKind = 0;
            pet.OSUPetServiceOwnerSerial = 0;
            pet.OSUPetServiceCityId = -1;
            pet.OSUPetServiceReadyUtc = DateTime.MinValue;
            pet.OSUPetServiceClaimFromUtc = DateTime.MinValue;
            pet.OSUPetServicePartnerSerial = 0;
            pet.OSUPetServiceRoomIndex = -1;
            pet.OSUPetServiceStage = (int)OSUStableServiceStage.None;
        }

        private static bool IsCustomHorseBreed(BaseCreature pet)
        {
            return pet is HorseFrisio
                || pet is HorseMustang
                || pet is HorsePuroSangue
                || pet is HorseMangaLarga
                || pet is HorseAndaluz
                || pet is HorseShire
                || pet is HorseArdennes
                || pet is HorseClaydesdale
                || pet is HorseGypsy
                || pet is HorseBelga;
        }

        private static void CopyHorseVisualFromParent(BaseMount baby, BaseMount female, BaseMount male)
        {
            if (baby == null || female == null || male == null)
                return;

            if (!IsCustomHorseBreed(female) || !IsCustomHorseBreed(male))
                return;

            BaseMount source = null;

            if (female.GetType() == baby.GetType() && male.GetType() == baby.GetType())
                source = Utility.RandomBool() ? female : male;
            else if (female.GetType() == baby.GetType())
                source = female;
            else if (male.GetType() == baby.GetType())
                source = male;
            else
                source = Utility.RandomBool() ? female : male;

            if (source == null)
                return;

            baby.Body = source.Body;
            baby.ItemID = source.ItemID;
            baby.Hue = source.Hue;
            baby.BaseSoundID = source.BaseSoundID;
        }

        private static void ReleasePetFromBreedingRoom(BaseCreature pet, Mobile stableNpc, PlayerMobile owner, int roomIndex)
        {
            if (pet == null || stableNpc == null)
                return;

            Point3D loc = GetBreedingReleaseLocation(stableNpc, roomIndex);
            Map map = stableNpc.Map;
            pet.MoveToWorld(loc, map);
            pet.Hidden = false;
            pet.Frozen = false;
            pet.Blessed = false;
            pet.IsDeadPet = false;
            pet.OSUPetAwaitingResurrection = false;
            pet.OSUPetDownedUntilUtc = DateTime.MinValue;
            pet.CorpseNameOverride = BuildMarkedCorpseName(pet);
        }

        private static void ReleasePetNearNpc(BaseCreature pet, Mobile stableNpc, PlayerMobile owner)
        {
            if (pet == null || stableNpc == null)
                return;

            Map map = stableNpc.Map;
            Point3D baseLoc = stableNpc.Location;
            Point3D loc = new Point3D(baseLoc.X + Utility.RandomMinMax(-1, 1), baseLoc.Y + Utility.RandomMinMax(-1, 1), baseLoc.Z);

            pet.MoveToWorld(loc, map);
            pet.Hidden = false;
            pet.Frozen = false;
            pet.Blessed = false;
            pet.IsDeadPet = false;
            pet.OSUPetAwaitingResurrection = false;
            pet.OSUPetDownedUntilUtc = DateTime.MinValue;
            pet.CorpseNameOverride = BuildMarkedCorpseName(pet);
            pet.Frozen = false;
            pet.Blessed = false;

            if (owner != null)
            {
                pet.ControlMaster = owner;
                pet.Controlled = true;
                pet.ControlTarget = owner;
                pet.ControlOrder = OrderType.Follow;
                RestoreFollowerSlots(pet);
            }
        }


        private static void StoreAndReleaseFollowerSlots(BaseCreature pet)
        {
            if (pet == null)
                return;

            if (pet.ControlMaster != null)
                pet.RemoveFollowers();

            if (pet.OSUPetStoredControlSlots <= 0)
                pet.OSUPetStoredControlSlots = Math.Max(1, pet.ControlSlots);

            pet.ControlSlots = 0;
        }

        private static void RestoreFollowerSlots(BaseCreature pet)
        {
            if (pet == null)
                return;

            int restore = pet.OSUPetStoredControlSlots > 0 ? pet.OSUPetStoredControlSlots : Math.Max(1, pet.ControlSlots);
            pet.ControlSlots = Math.Max(1, restore);
        }

        private static void ProcessPastureState(BaseCreature pet)
        {
            if (pet == null || pet.Deleted || !pet.OSUPetMarked || !IsFarmAnimal(pet))
                return;

            if (pet.OSUPetPastureAtUtc == DateTime.MinValue || DateTime.UtcNow < pet.OSUPetPastureAtUtc)
                return;

            if (pet.OSUPetStoredControlSlots <= 0)
                pet.OSUPetStoredControlSlots = Math.Max(1, pet.ControlSlots);

            pet.ControlSlots = 0;
            pet.ControlMaster = null;
            pet.Controlled = true;
            pet.ControlTarget = null;
            pet.ControlOrder = OrderType.None;
            pet.Home = pet.Location;
            pet.RangeHome = 10;
            pet.OSUPetPastureAtUtc = DateTime.MinValue;
            pet.InvalidateProperties();
        }

        private static Type ResolveMobileType(string typeName)
        {
            if (String.IsNullOrWhiteSpace(typeName))
                return null;

            Type t = ScriptCompiler.FindTypeByFullName(typeName);
            if (t == null)
                t = ScriptCompiler.FindTypeByName(typeName);

            return t;
        }

        public static void WriteStableData(GenericWriter writer, BaseCreature pet)
        {
            writer.Write(pet.OSUPetInitialized);
            writer.Write(pet.OSUPetLevel);
            writer.Write(pet.OSUPetXP);
            writer.Write(pet.OSUPetNextLevelXP);
            writer.Write(pet.OSUPetLastGainStr);
            writer.Write(pet.OSUPetLastGainDex);
            writer.Write(pet.OSUPetLastGainInt);
            writer.Write(pet.OSUPetLastGainLevel);
            writer.Write(pet.OSUPetLevelOneStr);
            writer.Write(pet.OSUPetLevelOneDex);
            writer.Write(pet.OSUPetLevelOneInt);
            writer.Write(pet.OSUPetCastrated);
            writer.Write(pet.OSUPetSterile);
            writer.Write(pet.OSUPetMarked);
            writer.Write(pet.OSUPetBrandOwnerSerial);
            writer.Write(pet.OSUPetBrandOwnerName ?? String.Empty);
            writer.Write(pet.OSUPetLivesRemaining);
            writer.Write(pet.OSUPetLivesMax);
            writer.Write(pet.OSUPetAwaitingResurrection);
            writer.Write(pet.OSUPetDownedUntilUtc);
            writer.Write(pet.OSUPetLastCommandUtc);
            writer.Write(pet.OSUPetBreedCount);
            writer.Write(pet.OSUPetBreedCountMax);
            writer.Write(pet.OSUPetBreedGroup ?? String.Empty);
            writer.Write(pet.OSUPetAbilitySlot5 ?? String.Empty);
            writer.Write(pet.OSUPetAbilitySlot10 ?? String.Empty);
            writer.Write(pet.OSUPetServiceOwnerSerial);
            writer.Write(pet.OSUPetServiceKind);
            writer.Write(pet.OSUPetServiceCityId);
            writer.Write(pet.OSUPetServiceReadyUtc);
            writer.Write(pet.OSUPetServiceClaimFromUtc);
            writer.Write(pet.OSUPetServicePartnerSerial);
            writer.Write(pet.OSUPetPendingOffspringTypeName ?? String.Empty);
            writer.Write(pet.OSUPetPendingOffspringFemale);
            writer.Write(pet.OSUPetPendingOffspringStr);
            writer.Write(pet.OSUPetPendingOffspringDex);
            writer.Write(pet.OSUPetPendingOffspringInt);
            writer.Write(pet.OSUPetPendingOffspringBreedMax);
            writer.Write(pet.OSUPetPendingOffspringGroup ?? String.Empty);
            writer.Write(pet.OSUPetStoredControlSlots);
            writer.Write(pet.OSUPetServiceRoomIndex);
            writer.Write(pet.OSUPetServiceStage);
            writer.Write(pet.OSUPetLastTrainedLevel);
            writer.Write(pet.OSUPetPastureAtUtc);
        }

        public static void ReadStableDataV31(GenericReader reader, BaseCreature pet)
        {
            pet.OSUPetInitialized = reader.ReadBool();
            pet.OSUPetLevel = reader.ReadInt();
            pet.OSUPetXP = reader.ReadInt();
            pet.OSUPetNextLevelXP = reader.ReadInt();
            pet.OSUPetLastGainStr = reader.ReadInt();
            pet.OSUPetLastGainDex = reader.ReadInt();
            pet.OSUPetLastGainInt = reader.ReadInt();
            pet.OSUPetLastGainLevel = reader.ReadInt();
            pet.OSUPetLevelOneStr = reader.ReadInt();
            pet.OSUPetLevelOneDex = reader.ReadInt();
            pet.OSUPetLevelOneInt = reader.ReadInt();
            pet.OSUPetCastrated = reader.ReadBool();
            pet.OSUPetSterile = reader.ReadBool();
            pet.OSUPetMarked = reader.ReadBool();
            pet.OSUPetBrandOwnerSerial = reader.ReadInt();
            pet.OSUPetBrandOwnerName = reader.ReadString();
            pet.OSUPetLivesRemaining = reader.ReadInt();
            pet.OSUPetLivesMax = reader.ReadInt();
            pet.OSUPetAwaitingResurrection = reader.ReadBool();
            pet.OSUPetDownedUntilUtc = reader.ReadDateTime();
            pet.OSUPetLastCommandUtc = reader.ReadDateTime();
            pet.OSUPetBreedCount = reader.ReadInt();
            pet.OSUPetBreedCountMax = reader.ReadInt();
            pet.OSUPetBreedGroup = reader.ReadString();
            pet.OSUPetAbilitySlot5 = reader.ReadString();
            pet.OSUPetAbilitySlot10 = reader.ReadString();
            pet.OSUPetServiceOwnerSerial = reader.ReadInt();
            pet.OSUPetServiceKind = reader.ReadInt();
            pet.OSUPetServiceCityId = reader.ReadInt();
            pet.OSUPetServiceReadyUtc = reader.ReadDateTime();
            pet.OSUPetServiceClaimFromUtc = reader.ReadDateTime();
            pet.OSUPetServicePartnerSerial = reader.ReadInt();
            pet.OSUPetPendingOffspringTypeName = reader.ReadString();
            pet.OSUPetPendingOffspringFemale = reader.ReadBool();
            pet.OSUPetPendingOffspringStr = reader.ReadInt();
            pet.OSUPetPendingOffspringDex = reader.ReadInt();
            pet.OSUPetPendingOffspringInt = reader.ReadInt();
            pet.OSUPetPendingOffspringBreedMax = reader.ReadInt();
            pet.OSUPetPendingOffspringGroup = reader.ReadString();
            pet.OSUPetStoredControlSlots = reader.ReadInt();
            pet.OSUPetServiceRoomIndex = reader.ReadInt();
            pet.OSUPetServiceStage = reader.ReadInt();
            pet.OSUPetLastTrainedLevel = reader.ReadInt();
            pet.OSUPetPastureAtUtc = reader.ReadDateTime();
        }

        public static void ReadStableDataV30(GenericReader reader, BaseCreature pet)
        {
            pet.OSUPetInitialized = reader.ReadBool();
            pet.OSUPetLevel = reader.ReadInt();
            pet.OSUPetXP = reader.ReadInt();
            pet.OSUPetNextLevelXP = reader.ReadInt();
            pet.OSUPetLastGainStr = reader.ReadInt();
            pet.OSUPetLastGainDex = reader.ReadInt();
            pet.OSUPetLastGainInt = reader.ReadInt();
            pet.OSUPetLastGainLevel = reader.ReadInt();
            pet.OSUPetLevelOneStr = reader.ReadInt();
            pet.OSUPetLevelOneDex = reader.ReadInt();
            pet.OSUPetLevelOneInt = reader.ReadInt();
            pet.OSUPetCastrated = reader.ReadBool();
            pet.OSUPetSterile = reader.ReadBool();
            pet.OSUPetMarked = reader.ReadBool();
            pet.OSUPetBrandOwnerSerial = reader.ReadInt();
            pet.OSUPetBrandOwnerName = reader.ReadString();
            pet.OSUPetLivesRemaining = reader.ReadInt();
            pet.OSUPetLivesMax = reader.ReadInt();
            pet.OSUPetAwaitingResurrection = reader.ReadBool();
            pet.OSUPetDownedUntilUtc = reader.ReadDateTime();
            pet.OSUPetLastCommandUtc = reader.ReadDateTime();
            pet.OSUPetBreedCount = reader.ReadInt();
            pet.OSUPetBreedCountMax = reader.ReadInt();
            pet.OSUPetBreedGroup = reader.ReadString();
            pet.OSUPetAbilitySlot5 = reader.ReadString();
            pet.OSUPetAbilitySlot10 = reader.ReadString();
            pet.OSUPetServiceOwnerSerial = reader.ReadInt();
            pet.OSUPetServiceKind = reader.ReadInt();
            pet.OSUPetServiceCityId = reader.ReadInt();
            pet.OSUPetServiceReadyUtc = reader.ReadDateTime();
            pet.OSUPetServiceClaimFromUtc = reader.ReadDateTime();
            pet.OSUPetServicePartnerSerial = reader.ReadInt();
            pet.OSUPetPendingOffspringTypeName = reader.ReadString();
            pet.OSUPetPendingOffspringFemale = reader.ReadBool();
            pet.OSUPetPendingOffspringStr = reader.ReadInt();
            pet.OSUPetPendingOffspringDex = reader.ReadInt();
            pet.OSUPetPendingOffspringInt = reader.ReadInt();
            pet.OSUPetPendingOffspringBreedMax = reader.ReadInt();
            pet.OSUPetPendingOffspringGroup = reader.ReadString();
            pet.OSUPetStoredControlSlots = reader.ReadInt();

            pet.OSUPetServiceRoomIndex = -1;
            pet.OSUPetServiceStage = 0;
            pet.OSUPetLastTrainedLevel = 0;
            pet.OSUPetPastureAtUtc = DateTime.MinValue;
        }
    }
}
