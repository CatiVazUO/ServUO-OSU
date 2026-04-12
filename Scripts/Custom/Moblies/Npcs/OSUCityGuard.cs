using Server;
using Server.Items;
using Server.Items.Resource;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public class OSUCityGuard : BaseCreature
    {
        private int m_CityId;
        private int m_PostId;
        private Point3D m_PostLocation;
        private int m_GuardLevel;
        private ReinoGuardKind m_GuardKind;
        private bool m_Uniformized;
        private string m_ConstructionKey;
        private ReinoMilitaryLaw m_CurrentLaw;
        private bool m_ArrestMode;
        private DateTime m_NextBandage;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PostId { get { return m_PostId; } set { m_PostId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D PostLocation { get { return m_PostLocation; } set { m_PostLocation = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GuardLevel { get { return m_GuardLevel; } set { m_GuardLevel = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ReinoGuardKind GuardKind { get { return m_GuardKind; } set { m_GuardKind = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Uniformized { get { return m_Uniformized; } set { m_Uniformized = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value; } }

        public bool IsOfficial { get { return m_GuardKind == ReinoGuardKind.Oficial; } }

        public bool IsConstructionOfficial
        {
            get { return IsOfficial && m_PostId <= 0 && !String.IsNullOrWhiteSpace(m_ConstructionKey); }
        }

        [Constructable]
        public OSUCityGuard() : this(0, ReinoGuardKind.Vigia)
        {
        }

        public OSUCityGuard(int cityId, ReinoGuardKind kind)
            : base(GetAI(kind), GetFightMode(kind), 10, 1, 0.20, 0.40)
        {
            m_CityId = cityId;
            m_GuardKind = kind;
            m_GuardLevel = 1;
            m_ConstructionKey = String.Empty;
            m_Uniformized = true;

            Female = Utility.RandomBool();
            Body = Female ? 0x191 : 0x190;
            Name = Female ? NameList.RandomName("female") : NameList.RandomName("male");
            Title = "guarda do reino";
            Hue = Race.RandomSkinHue();
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();
            if (!Female)
            {
                FacialHairItemID = Race.RandomFacialHair(Female);
                FacialHairHue = Race.RandomHairHue();
            }

            Blessed = false;
            CantWalk = false;

            EnsureSupplyBackpack();
            ApplyLoadout();
        }

        private static AIType GetAI(ReinoGuardKind kind)
        {
            switch (kind)
            {
                case ReinoGuardKind.Arqueiro:
                case ReinoGuardKind.CavalariaArqueira:
                    return AIType.AI_Archer;
                case ReinoGuardKind.Oficial:
                    return AIType.AI_Vendor;
                default:
                    return AIType.AI_Melee;
            }
        }

        private static FightMode GetFightMode(ReinoGuardKind kind)
        {
            return FightMode.None;
        }

        public override bool ClickTitle { get { return true; } }
        public override bool CanTeach { get { return false; } }
        public override bool HandlesOnSpeech(Mobile from) { return true; }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            if (!IsOfficial || e == null || e.Mobile == null || e.Mobile.Deleted)
                return;

            if (!e.Mobile.InRange(this.Location, 4))
                return;

            string speech = (e.Speech ?? String.Empty).ToLowerInvariant();
            if (speech.Contains("relatorio") || speech.Contains("relatório"))
            {
                PlayerMobile pm = e.Mobile as PlayerMobile;
                if (pm != null && ReinoMilitarySystem.CanManageWantedList(pm, m_CityId))
                    pm.SendGump(new ReinoMilitaryReportsGump(pm, m_CityId));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);

            if (!IsOfficial)
                return;

            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (pm.InRange(this.Location, 4) && ReinoMilitarySystem.CanManageWantedList(pm, m_CityId))
                pm.SendGump(new ReinoMilitaryReportsGump(pm, m_CityId));
        }

        public override bool OnBeforeDeath()
        {
            Mobile target = Combatant as Mobile;

            if (target != null)
                ReinoMilitarySystem.RegisterGuardOutcome(this, target, m_CurrentLaw, false, false, false, false);

            ReinoMilitarySystem.NotifyGuardKilled(this);
            return base.OnBeforeDeath();
        }

        public override void OnThink()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            if (IsOfficial)
            {
                Combatant = null;
                Warmode = false;
                CurrentWayPoint = null;
                CantWalk = true;
                Direction = (Direction)(FindPostFacing());
                if (!InRange(m_PostLocation, 0))
                    MoveToWorld(m_PostLocation, Map);
                return;
            }

            if (Combatant != null)
            {
                PlayerMobile pm = Combatant as PlayerMobile;
                if (pm != null && !pm.Alive)
                {
                    HandleTargetDown(pm);
                    return;
                }

                if (IsConstructionOfficial && !ReinoMilitarySystem.IsInsideConstructionBounds(m_ConstructionKey, Location, Map))
                {
                    Combatant = null;
                    Warmode = false;
                    FightMode = FightMode.None;
                    CurrentWayPoint = null;
                    MoveToWorld(m_PostLocation, Map);
                    return;
                }

                Home = m_PostLocation;
                RangeHome = 0;
                FightMode = FightMode.Closest;
                Warmode = true;
                CantWalk = false;
                base.OnThink();
                return;
            }

            if (CurrentWayPoint != null)
            {
                base.OnThink();
                return;
            }

            Mobile hostile = ReinoMilitarySystem.FindMonsterThreatForGuard(this);
            if (hostile != null)
            {
                BeginAttack(hostile, ReinoMilitaryLaw.Fighting, false);
                base.OnThink();
                return;
            }

            Warmode = false;
            Combatant = null;
            FightMode = FightMode.None;
            Home = m_PostLocation;
            RangeHome = 0;
            Direction = (Direction)(FindPostFacing());

            if (!InRange(m_PostLocation, 0))
                MoveToWorld(m_PostLocation, Map);

            if (Hits < HitsMax && DateTime.UtcNow >= m_NextBandage)
            {
                if (Backpack == null)
                    EnsureSupplyBackpack();

                if (ConsumeGuardBandage())
                {
                    PublicOverheadMessage(MessageType.Emote, 0x3B2, false, "*se curando de ferimentos*");
                    Hits = Math.Min(HitsMax, Hits + Utility.RandomMinMax(8, 14));
                    m_NextBandage = DateTime.UtcNow + TimeSpan.FromSeconds(8.0);
                }
                else
                {
                    EnsureSupplyBackpack();
                }
            }
        }

        public void BeginAttack(Mobile target, ReinoMilitaryLaw law, bool arrestMode)
        {
            if (Deleted || target == null || target.Deleted || IsOfficial)
                return;

            m_CurrentLaw = law;
            m_ArrestMode = arrestMode;
            FightMode = FightMode.Closest;
            Combatant = target;
            Warmode = true;
            CantWalk = false;
            CurrentWayPoint = null;
            RangeHome = 0;

            PlayerMobile pm = target as PlayerMobile;
            if (pm != null && arrestMode && ReinoMilitarySystem.HasPrison(m_CityId))
            {
                pm.CloseGump(typeof(ReinoGuardSurrenderGump));
                pm.SendGump(new ReinoGuardSurrenderGump(pm, m_CityId, this.Serial.Value, law));
            }
        }

        private void HandleTargetDown(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            bool permanentDeath = pm.OSUPermaDead;
            bool storedLoot = false;
            bool prisoned = false;

            // Só recolhe os itens se NÃO for a última vida
            if (!permanentDeath)
                storedLoot = LootKnockoutCorpse(pm);

            // Se o modo for prender, depois do desmaio ele vai para a prisão
            if (!permanentDeath && m_ArrestMode)
                prisoned = ReinoMilitarySystem.TrySendToPrison(pm, m_CityId, this, m_CurrentLaw);

            ReinoMilitarySystem.RegisterGuardOutcome(this, pm, m_CurrentLaw, !permanentDeath, permanentDeath, storedLoot, prisoned);

            Combatant = null;
            Warmode = false;
            FightMode = FightMode.None;
            Hits = HitsMax;
            Stam = StamMax;
            Mana = ManaMax;
            MoveToWorld(m_PostLocation, Map);
        }

        private int FindPostFacing()
        {
            ReinoGuardPostInfo post = ReinoMilitarySystem.FindPostById(m_CityId, m_PostId);
            return post != null ? post.Facing : (int)Direction;
        }

        private bool LootKnockoutCorpse(PlayerMobile pm)
        {
            if (pm == null || pm.Corpse == null)
                return false;

            Corpse corpse = pm.Corpse as Corpse;
            if (corpse == null)
                return false;

            List<Item> items = new List<Item>();
            for (int i = 0; i < corpse.Items.Count; i++)
            {
                Item item = corpse.Items[i];
                if (item == null || item.Deleted)
                    continue;

                if (item is BaseClothing)
                    continue;

                items.Add(item);
            }

            if (items.Count == 0)
                return false;

            Bag lootBag = new Bag();
            lootBag.Name = "pertences de " + pm.Name;
            lootBag.Movable = true;

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (item != null && !item.Deleted)
                    lootBag.DropItem(item);
            }

            if (lootBag.Items.Count == 0)
            {
                lootBag.Delete();
                return false;
            }

            if (!ReinoMilitarySystem.StoreLootInBarracks(m_CityId, lootBag))
            {
                lootBag.Delete();
                return false;
            }

            return true;
        }

        public void ConfigureRouteSpeed(ReinoRouteSpeed speed)
        {
            switch (speed)
            {
                default:
                case ReinoRouteSpeed.Short:
                    ActiveSpeed = 0.50;
                    PassiveSpeed = 0.50;
                    break;

                case ReinoRouteSpeed.Medium:
                    ActiveSpeed = 1.00;
                    PassiveSpeed = 1.00;
                    break;

                case ReinoRouteSpeed.Long:
                    ActiveSpeed = 2.00;
                    PassiveSpeed = 2.00;
                    break;
            }

            if (Combatant != null || Warmode)
                CurrentSpeed = ActiveSpeed;
            else
                CurrentSpeed = PassiveSpeed;
        }

        public List<SkillName> GetTrainableSkills()
        {
            List<SkillName> list = new List<SkillName>();
            AddIfPositive(list, SkillName.Swords);
            AddIfPositive(list, SkillName.Macing);
            AddIfPositive(list, SkillName.Fencing);
            AddIfPositive(list, SkillName.Archery);
            AddIfPositive(list, SkillName.Tactics);
            AddIfPositive(list, SkillName.Anatomy);
            AddIfPositive(list, SkillName.Healing);
            AddIfPositive(list, SkillName.Parry);
            AddIfPositive(list, SkillName.Focus);
            AddIfPositive(list, SkillName.Wrestling);
            return list;
        }

        private void AddIfPositive(List<SkillName> list, SkillName name)
        {
            if (Skills[name] != null && Skills[name].Base > 0.0)
                list.Add(name);
        }

        public void ApplyLoadout()
        {
            DeleteEquipment();
            EnsureSupplyBackpack();
            InitStatsForKind();
            EquipBaseClothes();
            EquipRoleItems();
            ApplyUniform();
            EnsureSupplyBackpack();
        }

        public void ApplyUniform()
        {
            List<Item> toDelete = new List<Item>();

            for (int i = 0; i < Items.Count; i++)
            {
                Item item = Items[i];
                if (item == null)
                    continue;

                if (item.Layer == Layer.MiddleTorso || item is BaseMiddleTorso)
                    toDelete.Add(item);
            }

            for (int i = 0; i < toDelete.Count; i++)
            {
                if (toDelete[i] != null && !toDelete[i].Deleted)
                    toDelete[i].Delete();
            }

            Item uniform = CreateUniformForCity();
            if (uniform == null)
                uniform = new Tunic();

            uniform.Movable = false;
            AddItem(uniform);
        }

        private Item CreateUniformForCity()
        {
            Item uniform = ReinoVisualSystem.CreateUniformForCity(m_CityId);
            return uniform ?? new Tunic();
        }

        private void DeleteEquipment()
        {
            List<Item> toDelete = new List<Item>();
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] != null)
                    toDelete.Add(Items[i]);
            }

            for (int i = 0; i < toDelete.Count; i++)
                toDelete[i].Delete();
        }

        private void EquipBaseClothes()
        {
            Item shirt = new UniformeUnderShirt();
            shirt.Movable = true;
            AddItem(shirt);

            Item boots = new Boots();
            boots.Movable = true;
            AddItem(boots);

            bool usesLegArmor =
                m_GuardKind == ReinoGuardKind.Rua ||
                m_GuardKind == ReinoGuardKind.Armado ||
                m_GuardKind == ReinoGuardKind.Arqueiro ||
                m_GuardKind == ReinoGuardKind.CavalariaArmada ||
                m_GuardKind == ReinoGuardKind.CavalariaArqueira;

            if (!usesLegArmor)
            {
                Item pants = new LongPants();
                pants.Movable = true;
                AddItem(pants);
            }
        }

        private void EquipRoleItems()
        {
            switch (m_GuardKind)
            {
                case ReinoGuardKind.Vigia:
                    AddHeld(new Club());
                    break;
                case ReinoGuardKind.Rua:
                    AddArmor(new LeatherChest(), new LeatherArms(), new LeatherGloves(), new LeatherGorget(), new LeatherLegs());
                    AddHeld(RandomMeleeWeapon(false));
                    AddShield(new WoodenShield());
                    break;
                case ReinoGuardKind.Armado:
                    AddArmor(new PlateChest(), new PlateArms(), new PlateGloves(), new PlateLegs(), new PlateGorget());
                    AddHeld(RandomMeleeWeapon(false));
                    AddShield(new HeaterShield());
                    break;
                case ReinoGuardKind.Arqueiro:
                    AddArmor(new StuddedChest(), new StuddedArms(), new StuddedGloves(), new StuddedGorget(), new StuddedLegs());
                    AddHeld(new Bow());
                    PackItem(new Arrow(100));
                    break;
                case ReinoGuardKind.CavalariaArmada:
                    AddArmor(new ChainChest(), new ChainCoif(), new ChainLegs());
                    AddHeld(Utility.RandomBool() ? (BaseWeapon)new Spear() : new Halberd());
                    EnsureHorse();
                    break;
                case ReinoGuardKind.CavalariaArqueira:
                    AddArmor(new ChainChest(), new ChainLegs());
                    AddHeld(new Crossbow());
                    PackItem(new Bolt(100));
                    EnsureHorse();
                    break;
                case ReinoGuardKind.Oficial:
                    AddItem(new StuddedGloves() { Movable = false });
                    break;
            }
        }

        private void EnsureHorse()
        {
            if (Mount == null)
            {
                Horse horse = new Horse();
                horse.Tamable = false;
                horse.Controlled = false;
                horse.Rider = this;
            }
        }

        private void AddArmor(params BaseArmor[] armors)
        {
            for (int i = 0; i < armors.Length; i++)
            {
                if (armors[i] != null)
                {
                    armors[i].Movable = false;
                    AddItem(armors[i]);
                }
            }
        }

        private void EnsureSupplyBackpack()
        {
            if (Backpack == null)
            {
                Backpack pack = new Backpack();
                pack.Movable = false;
                AddItem(pack);
            }

            Bandage bandage = Backpack.FindItemByType(typeof(Bandage)) as Bandage;

            if (bandage == null)
            {
                bandage = new Bandage(50);
                bandage.Movable = false;
                Backpack.DropItem(bandage);
            }
            else if (bandage.Amount < 20)
            {
                bandage.Amount = 50;
            }
        }

        private bool ConsumeGuardBandage()
        {
            if (Backpack == null)
                return false;

            Bandage bandage = Backpack.FindItemByType(typeof(Bandage)) as Bandage;

            if (bandage == null || bandage.Amount <= 0)
                return false;

            bandage.Consume(1);
            return true;
        }

        private void AddHeld(Item weapon)
        {
            if (weapon == null)
                return;

            weapon.Movable = false;
            AddItem(weapon);
        }

        private void AddShield(BaseShield shield)
        {
            if (shield == null)
                return;

            shield.Movable = false;
            AddItem(shield);
        }

        private BaseWeapon RandomMeleeWeapon(bool strong)
        {
            if (!strong)
            {
                switch (Utility.Random(4))
                {
                    default:
                    case 0: return new Broadsword();
                    case 1: return new Kryss();
                    case 2: return new Axe();
                    case 3: return new WarMace();
                }
            }

            switch (Utility.Random(4))
            {
                default:
                case 0: return new Broadsword();
                case 1: return new Kryss();
                case 2: return new Axe();
                case 3: return new WarMace();
            }
        }

        private void InitStatsForKind()
        {
            Title = "guarda do reino";
            SetSkill(SkillName.Healing, 40.0);
            SetSkill(SkillName.Anatomy, 40.0);
            SetSkill(SkillName.Tactics, 50.0);
            SetSkill(SkillName.MagicResist, 40.0);
            SetSkill(SkillName.Focus, 35.0);
            VirtualArmor = 20;

            switch (m_GuardKind)
            {
                default:
                case ReinoGuardKind.Vigia:
                    SetStr(55, 70);
                    SetDex(45, 60);
                    SetInt(20, 30);
                    SetHits(75, 90);
                    SetDamage(4, 7);
                    SetSkill(SkillName.Macing, 45.0, 60.0);
                    SetSkill(SkillName.Parry, 30.0, 40.0);
                    break;
                case ReinoGuardKind.Rua:
                    SetStr(75, 90);
                    SetDex(60, 75);
                    SetInt(25, 35);
                    SetHits(95, 115);
                    SetDamage(5, 9);
                    SetSkill(SkillName.Swords, 60.0, 75.0);
                    SetSkill(SkillName.Macing, 60.0, 75.0);
                    SetSkill(SkillName.Fencing, 60.0, 75.0);
                    SetSkill(SkillName.Parry, 50.0, 65.0);
                    VirtualArmor = 28;
                    break;
                case ReinoGuardKind.Armado:
                    SetStr(95, 110);
                    SetDex(70, 85);
                    SetInt(30, 40);
                    SetHits(120, 145);
                    SetDamage(7, 11);
                    SetSkill(SkillName.Swords, 78.0, 88.0);
                    SetSkill(SkillName.Macing, 78.0, 88.0);
                    SetSkill(SkillName.Fencing, 78.0, 88.0);
                    SetSkill(SkillName.Parry, 70.0, 80.0);
                    VirtualArmor = 36;
                    break;
                case ReinoGuardKind.Arqueiro:
                    SetStr(85, 95);
                    SetDex(85, 100);
                    SetInt(30, 40);
                    SetHits(110, 130);
                    SetDamage(6, 10);
                    SetSkill(SkillName.Archery, 82.0, 95.0);
                    SetSkill(SkillName.Anatomy, 60.0, 70.0);
                    VirtualArmor = 30;
                    break;
                case ReinoGuardKind.CavalariaArmada:
                    SetStr(115, 130);
                    SetDex(80, 95);
                    SetInt(35, 45);
                    SetHits(150, 175);
                    SetDamage(9, 13);
                    SetSkill(SkillName.Fencing, 90.0, 100.0);
                    SetSkill(SkillName.Swords, 90.0, 100.0);
                    SetSkill(SkillName.Parry, 80.0, 92.0);
                    VirtualArmor = 42;
                    break;
                case ReinoGuardKind.CavalariaArqueira:
                    SetStr(105, 120);
                    SetDex(90, 105);
                    SetInt(35, 45);
                    SetHits(140, 165);
                    SetDamage(8, 12);
                    SetSkill(SkillName.Archery, 92.0, 102.0);
                    SetSkill(SkillName.Tactics, 88.0, 98.0);
                    VirtualArmor = 38;
                    break;
                case ReinoGuardKind.Oficial:
                    Title = "oficial do quartel";
                    SetStr(115, 130);
                    SetDex(80, 95);
                    SetInt(45, 60);
                    SetHits(150, 175);
                    SetDamage(1, 2);
                    SetSkill(SkillName.Swords, 90.0, 100.0);
                    SetSkill(SkillName.Parry, 80.0, 92.0);
                    VirtualArmor = 28;
                    break;
            }

            Fame = 2500;
            Karma = 2500;
            Hits = HitsMax;
            Stam = StamMax;
        }

        public OSUCityGuard(Serial serial)
    : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_PostId);
            writer.Write(m_PostLocation);
            writer.Write(m_GuardLevel);
            writer.Write((int)m_GuardKind);
            writer.Write(m_Uniformized);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write((int)m_CurrentLaw);
            writer.Write(m_ArrestMode);
            writer.Write(m_NextBandage);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_PostId = reader.ReadInt();
            m_PostLocation = reader.ReadPoint3D();
            m_GuardLevel = reader.ReadInt();
            m_GuardKind = (ReinoGuardKind)reader.ReadInt();
            m_Uniformized = reader.ReadBool();
            m_ConstructionKey = reader.ReadString();
            m_CurrentLaw = (ReinoMilitaryLaw)reader.ReadInt();
            m_ArrestMode = reader.ReadBool();
            m_NextBandage = reader.ReadDateTime();

            EnsureSupplyBackpack();
            ApplyUniform();
            Home = m_PostLocation;
            RangeHome = 0;
            FightMode = FightMode.None;
            Combatant = null;
            Warmode = false;
        }
    }
}
