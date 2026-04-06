using System;
using Server;
using Server.Items;
using Server.Custom.Reinos.Expansion.Engine;

namespace Server.Mobiles
{
    public static class ReinoLotThreatHelper
    {
        public static void EnsureBackpack(BaseCreature m)
        {
            if (m.Backpack == null)
                m.AddItem(new Backpack());
        }

        public static void AddHumanoidClothes(BaseCreature m, int hue, bool boots)
        {
            if (m.FindItemOnLayer(Layer.Pants) == null)
            {
                LongPants p = new LongPants();
                p.Hue = hue;
                p.Movable = false;
                m.AddItem(p);
            }

            if (m.FindItemOnLayer(Layer.Shirt) == null)
            {
                Shirt s = new Shirt();
                s.Hue = hue;
                s.Movable = false;
                m.AddItem(s);
            }

            if (boots && m.FindItemOnLayer(Layer.Shoes) == null)
            {
                Boots b = new Boots();
                b.Hue = hue;
                b.Movable = false;
                m.AddItem(b);
            }
        }

        public static void AddRobe(BaseCreature m, int hue)
        {
            if (m.FindItemOnLayer(Layer.OuterTorso) == null)
            {
                Robe robe = new Robe();
                robe.Hue = hue;
                robe.Movable = false;
                m.AddItem(robe);
            }
        }

        public static void AddHat(BaseCreature m, Item hat, int hue)
        {
            if (hat == null)
                return;

            hat.Hue = hue;
            hat.Movable = false;
            m.AddItem(hat);
        }

        public static void AddWeapon(BaseCreature m, Item weapon, int hue)
        {
            if (weapon == null)
                return;

            weapon.Hue = hue;
            weapon.Movable = false;
            m.AddItem(weapon);
        }

        public static void PackBasicLoot(BaseCreature m, int goldMin, int goldMax, int potionChance, int gemChance)
        {
            EnsureBackpack(m);
            m.PackGold(goldMin, goldMax);

            if (Utility.Random(100) < potionChance)
                m.PackItem(Loot.RandomPotion());

            if (Utility.Random(100) < gemChance)
                m.PackItem(Loot.RandomGem());
        }

        public static void PackMageLoot(BaseCreature m, int goldMin, int goldMax, int regMin, int regMax, int scrollChance)
        {
            EnsureBackpack(m);
            m.PackGold(goldMin, goldMax);
            m.PackReg(regMin, regMax);

            if (Utility.RandomBool())
                m.PackItem(new Spellbook());

            if (Utility.Random(100) < scrollChance)
                m.PackItem(Loot.RandomScroll(1, 5, SpellbookType.Regular));
        }

        public static void PackBanditLoot(BaseCreature m, int goldMin, int goldMax, int bandageMin, int bandageMax, int arrowMin, int arrowMax)
        {
            EnsureBackpack(m);
            m.PackGold(goldMin, goldMax);

            if (bandageMax > 0)
                m.PackItem(new Bandage(Utility.RandomMinMax(bandageMin, bandageMax)));

            if (arrowMax > 0)
                m.PackItem(new Arrow(Utility.RandomMinMax(arrowMin, arrowMax)));

            if (Utility.RandomBool())
                m.PackItem(Loot.RandomGem());
        }
    }

    public class FreshSkeleton : BaseReinoLotThreat
    {
        [Constructable]
        public FreshSkeleton() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.22, 0.44)
        {
            Name = "fresh skeleton";
            Body = 50;
            BaseSoundID = 451;
            Hue = 1109;

            SetStr(80, 100);
            SetDex(55, 75);
            SetInt(25, 40);

            SetHits(65, 85);
            SetDamage(5, 9);

            SetSkill(SkillName.Wrestling, 50.0, 70.0);
            SetSkill(SkillName.Tactics, 50.0, 70.0);
            SetSkill(SkillName.MagicResist, 35.0, 50.0);

            Fame = 700;
            Karma = -700;
            VirtualArmor = 18;

            ReinoLotThreatHelper.PackBasicLoot(this, 20, 45, 20, 20);
        }

        public FreshSkeleton(Serial serial) : base(serial) { }

        public override int Meat { get { return 0; } }
        public override int Feathers { get { return 0; } }
        public override int Hides { get { return 0; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class RuinZombie : BaseReinoLotThreat
    {
        [Constructable]
        public RuinZombie() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.24, 0.48)
        {
            Name = "ruin zombie";
            Body = 3;
            BaseSoundID = 471;
            Hue = 2101;

            SetStr(95, 120);
            SetDex(40, 55);
            SetInt(30, 45);

            SetHits(80, 105);
            SetDamage(6, 11);

            SetSkill(SkillName.Wrestling, 55.0, 75.0);
            SetSkill(SkillName.Tactics, 55.0, 75.0);
            SetSkill(SkillName.MagicResist, 35.0, 50.0);

            Fame = 900;
            Karma = -900;
            VirtualArmor = 22;

            ReinoLotThreatHelper.PackBasicLoot(this, 30, 60, 25, 25);
        }

        public RuinZombie(Serial serial) : base(serial) { }

        public override int Meat { get { return 1; } }
        public override int Hides { get { return 0; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class FreshBandit : BaseReinoLotThreat
    {
        [Constructable]
        public FreshBandit() : base(AIType.AI_Archer, FightMode.Closest, 10, 6, 0.20, 0.40)
        {
            Name = "road bandit";
            Body = Utility.RandomBool() ? 0x190 : 0x191;
            Hue = 0;

            SetStr(90, 110);
            SetDex(70, 90);
            SetInt(35, 50);

            SetHits(80, 100);
            SetDamage(5, 10);

            SetSkill(SkillName.Fencing, 55.0, 75.0);
            SetSkill(SkillName.Archery, 55.0, 75.0);
            SetSkill(SkillName.Tactics, 60.0, 80.0);
            SetSkill(SkillName.MagicResist, 35.0, 50.0);

            Fame = 1100;
            Karma = -1100;
            VirtualArmor = 24;

            ReinoLotThreatHelper.AddHumanoidClothes(this, 1102, true);
            ReinoLotThreatHelper.AddWeapon(this, Utility.RandomBool() ? (Item)new Bow() : new Kryss(), 0);

            ReinoLotThreatHelper.PackBanditLoot(this, 45, 90, 3, 8, 8, 20);
        }

        public FreshBandit(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class AshHarpy : BaseReinoLotThreat
    {
        [Constructable]
        public AshHarpy() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.20, 0.40)
        {
            Name = "ash harpy";
            Body = 30;
            BaseSoundID = 402;
            Hue = 1150;

            SetStr(105, 130);
            SetDex(80, 100);
            SetInt(40, 55);

            SetHits(85, 110);
            SetDamage(7, 12);

            SetSkill(SkillName.Wrestling, 65.0, 85.0);
            SetSkill(SkillName.Tactics, 65.0, 85.0);
            SetSkill(SkillName.MagicResist, 45.0, 60.0);

            Fame = 1400;
            Karma = -1400;
            VirtualArmor = 26;

            ReinoLotThreatHelper.PackBasicLoot(this, 40, 80, 30, 30);
        }

        public AshHarpy(Serial serial) : base(serial) { }

        public override int Meat { get { return 1; } }
        public override int Feathers { get { return 25; } }
        public override int Hides { get { return 6; } }
        public override HideType HideType { get { return HideType.Spined; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class LostHedgeMage : BaseReinoLotThreat
    {
        [Constructable]
        public LostHedgeMage() : base(AIType.AI_Mage, FightMode.Closest, 10, 10, 0.18, 0.36)
        {
            Name = "lost hedge mage";
            Body = Utility.RandomBool() ? 0x190 : 0x191;
            Hue = 0;

            SetStr(70, 90);
            SetDex(55, 75);
            SetInt(95, 120);

            SetHits(70, 95);
            SetDamage(4, 8);

            SetSkill(SkillName.Magery, 70.0, 90.0);
            SetSkill(SkillName.EvalInt, 70.0, 90.0);
            SetSkill(SkillName.Meditation, 60.0, 80.0);
            SetSkill(SkillName.MagicResist, 60.0, 80.0);
            SetSkill(SkillName.Wrestling, 40.0, 55.0);

            Fame = 1600;
            Karma = -1600;
            VirtualArmor = 26;

            ReinoLotThreatHelper.AddHumanoidClothes(this, 1157, true);
            ReinoLotThreatHelper.AddRobe(this, 1153);
            ReinoLotThreatHelper.AddHat(this, new WizardsHat(), 1153);
            ReinoLotThreatHelper.AddWeapon(this, new QuarterStaff(), 0);

            ReinoLotThreatHelper.PackMageLoot(this, 60, 110, 5, 10, 45);
        }

        public LostHedgeMage(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class FeralGoblin : BaseReinoLotThreat
    {
        [Constructable]
        public FeralGoblin() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.20, 0.40)
        {
            Name = "feral goblin";
            Body = 334;
            BaseSoundID = 0x45A;
            Hue = 1271;

            SetStr(90, 115);
            SetDex(70, 90);
            SetInt(35, 50);

            SetHits(80, 105);
            SetDamage(6, 10);

            SetSkill(SkillName.Swords, 55.0, 75.0);
            SetSkill(SkillName.Tactics, 60.0, 80.0);
            SetSkill(SkillName.MagicResist, 35.0, 50.0);

            Fame = 1200;
            Karma = -1200;
            VirtualArmor = 24;

            ReinoLotThreatHelper.AddWeapon(this, Utility.RandomBool() ? (Item)new Scimitar() : new Hatchet(), 0);
            ReinoLotThreatHelper.PackBasicLoot(this, 35, 70, 20, 20);
        }

        public FeralGoblin(Serial serial) : base(serial) { }

        public override int Meat { get { return 1; } }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class BlightMongbat : BaseReinoLotThreat
    {
        [Constructable]
        public BlightMongbat() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.20, 0.40)
        {
            Name = "blight mongbat";
            Body = 39;
            BaseSoundID = 422;
            Hue = 1272;

            SetStr(50, 70);
            SetDex(70, 90);
            SetInt(25, 40);

            SetHits(45, 65);
            SetDamage(3, 7);

            SetSkill(SkillName.Wrestling, 45.0, 65.0);
            SetSkill(SkillName.Tactics, 45.0, 65.0);
            SetSkill(SkillName.MagicResist, 20.0, 35.0);

            Fame = 450;
            Karma = -450;
            VirtualArmor = 14;

            ReinoLotThreatHelper.PackBasicLoot(this, 10, 30, 15, 10);
        }

        public BlightMongbat(Serial serial) : base(serial) { }

        public override int Meat { get { return 1; } }
        public override int Feathers { get { return 8; } }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class RuinRatman : BaseReinoLotThreat
    {
        [Constructable]
        public RuinRatman() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.20, 0.40)
        {
            Name = "ruin ratman";
            Body = 42;
            BaseSoundID = 437;
            Hue = 2105;

            SetStr(95, 120);
            SetDex(60, 80);
            SetInt(35, 50);

            SetHits(85, 110);
            SetDamage(6, 10);

            SetSkill(SkillName.Wrestling, 60.0, 80.0);
            SetSkill(SkillName.Tactics, 60.0, 80.0);
            SetSkill(SkillName.MagicResist, 35.0, 50.0);

            Fame = 1000;
            Karma = -1000;
            VirtualArmor = 22;

            ReinoLotThreatHelper.AddWeapon(this, Utility.RandomBool() ? (Item)new Spear() : new Club(), 0);
            ReinoLotThreatHelper.PackBasicLoot(this, 30, 65, 20, 20);
        }

        public RuinRatman(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class BoneCaptain : BaseReinoLotThreat
    {
        [Constructable]
        public BoneCaptain() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.18, 0.36)
        {
            Name = "bone captain";
            Body = 50;
            BaseSoundID = 451;
            Hue = 2411;

            SetStr(140, 170);
            SetDex(75, 95);
            SetInt(55, 75);

            SetHits(130, 165);
            SetDamage(9, 14);

            SetSkill(SkillName.Swords, 75.0, 95.0);
            SetSkill(SkillName.Tactics, 75.0, 95.0);
            SetSkill(SkillName.MagicResist, 55.0, 75.0);

            Fame = 2400;
            Karma = -2400;
            VirtualArmor = 34;

            ReinoLotThreatHelper.AddWeapon(this, new VikingSword(), 0);
            ReinoLotThreatHelper.PackBasicLoot(this, 90, 150, 35, 40);
        }

        public BoneCaptain(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class FirebrandBandit : BaseReinoLotThreat
    {
        [Constructable]
        public FirebrandBandit() : base(AIType.AI_Mage, FightMode.Closest, 10, 8, 0.18, 0.36)
        {
            Name = "firebrand bandit";
            Body = Utility.RandomBool() ? 0x190 : 0x191;
            Hue = 0;

            SetStr(95, 120);
            SetDex(70, 90);
            SetInt(70, 95);

            SetHits(95, 120);
            SetDamage(6, 11);

            SetSkill(SkillName.Magery, 60.0, 80.0);
            SetSkill(SkillName.EvalInt, 60.0, 80.0);
            SetSkill(SkillName.Swords, 60.0, 80.0);
            SetSkill(SkillName.Tactics, 65.0, 85.0);
            SetSkill(SkillName.MagicResist, 50.0, 70.0);

            Fame = 1800;
            Karma = -1800;
            VirtualArmor = 28;

            ReinoLotThreatHelper.AddHumanoidClothes(this, 1358, true);
            ReinoLotThreatHelper.AddRobe(this, 1358);
            ReinoLotThreatHelper.AddWeapon(this, Utility.RandomBool() ? (Item)new Longsword() : new Katana(), 0);

            ReinoLotThreatHelper.PackBanditLoot(this, 70, 120, 4, 10, 0, 0);
            PackItem(Loot.RandomPotion());
            PackReg(3, 7);
        }

        public FirebrandBandit(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class RuinOgre : BaseReinoLotThreat
    {
        [Constructable]
        public RuinOgre() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.24, 0.48)
        {
            Name = "ruin ogre";
            Body = 1;
            BaseSoundID = 427;
            Hue = 1810;

            SetStr(160, 200);
            SetDex(55, 75);
            SetInt(30, 45);

            SetHits(150, 190);
            SetDamage(10, 16);

            SetSkill(SkillName.Macing, 70.0, 90.0);
            SetSkill(SkillName.Tactics, 70.0, 90.0);
            SetSkill(SkillName.MagicResist, 40.0, 55.0);

            Fame = 2600;
            Karma = -2600;
            VirtualArmor = 36;

            ReinoLotThreatHelper.AddWeapon(this, new Club(), 0);
            ReinoLotThreatHelper.PackBasicLoot(this, 100, 170, 25, 35);
        }

        public RuinOgre(Serial serial) : base(serial) { }

        public override int Meat { get { return 2; } }
        public override int Hides { get { return 12; } }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class SmolderMage : BaseReinoLotThreat
    {
        [Constructable]
        public SmolderMage() : base(AIType.AI_Mage, FightMode.Closest, 10, 10, 0.18, 0.36)
        {
            Name = "smolder mage";
            Body = Utility.RandomBool() ? 0x190 : 0x191;
            Hue = 0;

            SetStr(80, 100);
            SetDex(60, 80);
            SetInt(110, 140);

            SetHits(85, 115);
            SetDamage(6, 10);

            SetSkill(SkillName.Magery, 80.0, 100.0);
            SetSkill(SkillName.EvalInt, 80.0, 100.0);
            SetSkill(SkillName.Meditation, 70.0, 90.0);
            SetSkill(SkillName.MagicResist, 70.0, 90.0);
            SetSkill(SkillName.Wrestling, 45.0, 60.0);

            Fame = 2400;
            Karma = -2400;
            VirtualArmor = 30;

            ReinoLotThreatHelper.AddHumanoidClothes(this, 1645, true);
            ReinoLotThreatHelper.AddRobe(this, 1645);
            ReinoLotThreatHelper.AddHat(this, new WizardsHat(), 1645);
            ReinoLotThreatHelper.AddWeapon(this, new BlackStaff(), 0);

            ReinoLotThreatHelper.PackMageLoot(this, 90, 150, 6, 12, 55);
            PackItem(Loot.RandomPotion());
        }

        public SmolderMage(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
