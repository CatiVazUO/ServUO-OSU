using Server;

namespace Server.Mobiles
{
    public enum PostoThreatTier
    {
        SmallCotton,
        SmallIron,
        SmallWood,
        LargeWood,
        LargeIron,
        LargeCotton
    }

    public abstract class BasePostoThreat : BaseCreature
    {
        private PostoThreatTier m_Tier;
        private bool m_CanFly;
        private bool m_CanRummage;

        protected BasePostoThreat(string name, int body, int baseSoundId, PostoThreatTier tier, AIType aiType = AIType.AI_Melee, FightMode fightMode = FightMode.Closest, int hue = -1, bool canFly = false, bool canRummage = false)
            : base(aiType, fightMode, 10, 1, 0.2, 0.4)
        {
            Name = name;
            Body = body;
            BaseSoundID = baseSoundId;
            m_Tier = tier;
            m_CanFly = canFly;
            m_CanRummage = canRummage;

            if (hue >= 0)
                Hue = hue;

            Tamable = false;
            ApplyTier(tier);
        }

        public override bool CanFly
        {
            get { return m_CanFly; }
        }

        public override bool CanRummageCorpses
        {
            get { return m_CanRummage; }
        }

        protected virtual void ApplyTier(PostoThreatTier tier)
        {
            SetDamageType(ResistanceType.Physical, 100);

            switch (tier)
            {
                case PostoThreatTier.SmallCotton:
                    SetStr(80, 100);
                    SetDex(70, 90);
                    SetInt(20, 40);
                    SetHits(48, 60);
                    SetDamage(4, 6);
                    SetResistance(ResistanceType.Physical, 20, 25);
                    SetResistance(ResistanceType.Fire, 5, 10);
                    SetResistance(ResistanceType.Cold, 10, 15);
                    SetResistance(ResistanceType.Poison, 10, 20);
                    SetResistance(ResistanceType.Energy, 5, 10);
                    SetSkill(SkillName.MagicResist, 35.1, 50.0);
                    SetSkill(SkillName.Tactics, 45.1, 60.0);
                    SetSkill(SkillName.Wrestling, 45.1, 60.0);
                    Fame = 1200;
                    Karma = -1200;
                    VirtualArmor = 22;
                    break;
                case PostoThreatTier.SmallIron:
                    SetStr(95, 120);
                    SetDex(80, 100);
                    SetInt(30, 50);
                    SetHits(58, 72);
                    SetDamage(5, 7);
                    SetResistance(ResistanceType.Physical, 25, 30);
                    SetResistance(ResistanceType.Fire, 10, 20);
                    SetResistance(ResistanceType.Cold, 10, 20);
                    SetResistance(ResistanceType.Poison, 10, 20);
                    SetResistance(ResistanceType.Energy, 10, 20);
                    SetSkill(SkillName.MagicResist, 50.1, 65.0);
                    SetSkill(SkillName.Tactics, 60.1, 80.0);
                    SetSkill(SkillName.Wrestling, 55.1, 75.0);
                    Fame = 1600;
                    Karma = -1600;
                    VirtualArmor = 28;
                    break;
                case PostoThreatTier.SmallWood:
                    SetStr(110, 135);
                    SetDex(85, 105);
                    SetInt(30, 55);
                    SetHits(65, 80);
                    SetDamage(6, 9);
                    SetResistance(ResistanceType.Physical, 25, 35);
                    SetResistance(ResistanceType.Fire, 10, 20);
                    SetResistance(ResistanceType.Cold, 10, 20);
                    SetResistance(ResistanceType.Poison, 15, 25);
                    SetResistance(ResistanceType.Energy, 10, 20);
                    SetSkill(SkillName.MagicResist, 50.1, 70.0);
                    SetSkill(SkillName.Tactics, 60.1, 85.0);
                    SetSkill(SkillName.Wrestling, 60.1, 85.0);
                    Fame = 2000;
                    Karma = -2000;
                    VirtualArmor = 32;
                    break;
                case PostoThreatTier.LargeWood:
                    SetStr(125, 150);
                    SetDex(60, 85);
                    SetInt(30, 55);
                    SetHits(78, 92);
                    SetDamage(7, 15);
                    SetResistance(ResistanceType.Physical, 30, 35);
                    SetResistance(ResistanceType.Fire, 10, 20);
                    SetResistance(ResistanceType.Cold, 20, 30);
                    SetResistance(ResistanceType.Poison, 10, 20);
                    SetResistance(ResistanceType.Energy, 10, 20);
                    SetSkill(SkillName.MagicResist, 40.1, 55.0);
                    SetSkill(SkillName.Tactics, 50.1, 70.0);
                    SetSkill(SkillName.Wrestling, 50.1, 70.0);
                    Fame = 2600;
                    Karma = -2600;
                    VirtualArmor = 36;
                    break;
                case PostoThreatTier.LargeIron:
                    SetStr(136, 165);
                    SetDex(56, 75);
                    SetInt(31, 55);
                    SetHits(82, 99);
                    SetDamage(7, 17);
                    SetResistance(ResistanceType.Physical, 35, 40);
                    SetResistance(ResistanceType.Fire, 15, 25);
                    SetResistance(ResistanceType.Cold, 25, 35);
                    SetResistance(ResistanceType.Poison, 15, 25);
                    SetResistance(ResistanceType.Energy, 15, 25);
                    SetSkill(SkillName.MagicResist, 45.1, 60.0);
                    SetSkill(SkillName.Tactics, 50.1, 70.0);
                    SetSkill(SkillName.Wrestling, 50.1, 70.0);
                    Fame = 3000;
                    Karma = -3000;
                    VirtualArmor = 38;
                    break;
                case PostoThreatTier.LargeCotton:
                    SetStr(150, 180);
                    SetDex(65, 85);
                    SetInt(40, 65);
                    SetHits(90, 110);
                    SetDamage(8, 18);
                    SetResistance(ResistanceType.Physical, 35, 45);
                    SetResistance(ResistanceType.Fire, 15, 25);
                    SetResistance(ResistanceType.Cold, 20, 30);
                    SetResistance(ResistanceType.Poison, 20, 30);
                    SetResistance(ResistanceType.Energy, 15, 25);
                    SetSkill(SkillName.MagicResist, 50.1, 70.0);
                    SetSkill(SkillName.Tactics, 55.1, 75.0);
                    SetSkill(SkillName.Wrestling, 55.1, 75.0);
                    Fame = 3400;
                    Karma = -3400;
                    VirtualArmor = 40;
                    break;
            }
        }

        public override void GenerateLoot()
        {
            switch (m_Tier)
            {
                case PostoThreatTier.SmallCotton:
                case PostoThreatTier.SmallIron:
                case PostoThreatTier.SmallWood:
                    AddLoot(LootPack.Poor);
                    AddLoot(LootPack.Poor);
                    break;
                case PostoThreatTier.LargeWood:
                case PostoThreatTier.LargeIron:
                case PostoThreatTier.LargeCotton:
                    AddLoot(LootPack.Meager);
                    AddLoot(LootPack.Meager);
                    break;
            }
        }

        public BasePostoThreat(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write((int)m_Tier);
            writer.Write(m_CanFly);
            writer.Write(m_CanRummage);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
            {
                m_Tier = (PostoThreatTier)reader.ReadInt();
                m_CanFly = reader.ReadBool();
                m_CanRummage = reader.ReadBool();
            }
        }
    }
}
