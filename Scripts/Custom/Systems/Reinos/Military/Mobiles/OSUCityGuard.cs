using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

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

            Female = Utility.RandomBool();
            Body = Female ? 0x191 : 0x190;
            Name = Female ? NameList.RandomName("female") : NameList.RandomName("male");
            Title = "guarda do reino";
            Hue = Race.RandomSkinHue();
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();
            if (!Female)
                FacialHairItemID = Race.RandomFacialHair(Female);

            Blessed = false;
            CantWalk = false;

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
            return kind == ReinoGuardKind.Oficial ? FightMode.None : FightMode.Closest;
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
            {
                ReinoMilitarySystem.RegisterGuardOutcome(this, target, m_CurrentLaw, false, false, false, false);
            }

            return base.OnBeforeDeath();
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Deleted || Map == null || Map == Map.Internal)
                return;

            if (IsOfficial)
            {
                Combatant = null;
                Warmode = false;
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
            }

            if (Combatant == null)
            {
                if (Hits < HitsMax && DateTime.UtcNow >= m_NextBandage)
                {
                    PublicOverheadMessage(MessageType.Emote, 0x3B2, false, "*se curando de ferimentos*");
                    Hits = Math.Min(HitsMax, Hits + Utility.RandomMinMax(8, 14));
                    m_NextBandage = DateTime.UtcNow + TimeSpan.FromSeconds(8.0);
                }

                if (CurrentWayPoint == null && !InRange(m_PostLocation, 1))
                    MoveToWorld(m_PostLocation, Map);

                if (CurrentWayPoint == null)
                    Direction = ReinoMilitarySystem.NormalizeFacing(Direction);
            }
        }

        public void BeginAttack(Mobile target, ReinoMilitaryLaw law, bool arrestMode)
        {
            if (Deleted || target == null || target.Deleted || IsOfficial)
                return;

            m_CurrentLaw = law;
            m_ArrestMode = arrestMode;
            Combatant = target;
            Warmode = true;
            CurrentWayPoint = null;
        }

        private void HandleTargetDown(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            bool permanentDeath = pm.OSUPermaDead;
            bool storedLoot = LootKnockoutCorpse(pm);
            bool prisoned = false;

            if (!permanentDeath && m_ArrestMode)
            {
                prisoned = ReinoMilitarySystem.TrySendToPrison(pm, m_CityId, this, m_CurrentLaw);
            }

            ReinoMilitarySystem.RegisterGuardOutcome(this, pm, m_CurrentLaw, !permanentDeath, permanentDeath, storedLoot, prisoned);

            Combatant = null;
            Warmode = false;
            Hits = HitsMax;
            Stam = StamMax;
            Mana = ManaMax;
            MoveToWorld(m_PostLocation, Map);
        }

        private bool LootKnockoutCorpse(PlayerMobile pm)
        {
            if (pm == null || pm.Corpse == null)
                return false;

            bool movedAny = false;
            Corpse corpse = pm.Corpse as Corpse;
            if (corpse == null)
                return false;

            List<Item> items = new List<Item>();
            for (int i = 0; i < corpse.Items.Count; i++)
            {
                Item item = corpse.Items[i];
                if (item != null && !item.Deleted)
                    items.Add(item);
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (ReinoMilitarySystem.StoreLootInBarracks(m_CityId, items[i]))
                    movedAny = true;
            }

            return movedAny;
        }

        public void ConfigureRouteSpeed(ReinoRouteSpeed speed)
        {
            switch (speed)
            {
                default:
                case ReinoRouteSpeed.Short:
                    ActiveSpeed = 0.20;
                    PassiveSpeed = 0.40;
                    break;
                case ReinoRouteSpeed.Medium:
                    ActiveSpeed = 0.30;
                    PassiveSpeed = 0.55;
                    break;
                case ReinoRouteSpeed.Long:
                    ActiveSpeed = 0.45;
                    PassiveSpeed = 0.75;
                    break;
            }
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
            InitStatsForKind();
            EquipBaseClothes();
            EquipRoleItems();
            ApplyUniform();
        }

        public void ApplyUniform()
        {
            Item tunic = FindItemOnLayer(Layer.InnerTorso);
            if (tunic == null)
                return;

            if (!m_Uniformized)
            {
                tunic.ItemID = 0x1FA1;
                return;
            }

            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(m_CityId);
            switch ((culture ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "sarangs": tunic.ItemID = 0x227E; break;
                case "kamay": tunic.ItemID = 0x2281; break;
                case "zorteros":
                case "zosteros": tunic.ItemID = 0x228A; break;
                case "matalun": tunic.ItemID = 0x229C; break;
                default: tunic.ItemID = 0x1FA1; break;
            }
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
            Tunic tunic = new Tunic();
            tunic.Movable = false;
            AddItem(tunic);

            Item boots = new Boots();
            boots.Movable = false;
            AddItem(boots);

            Item pants = new LongPants();
            pants.Movable = false;
            AddItem(pants);
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
                    CantWalk = false;
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
        }
    }
}
