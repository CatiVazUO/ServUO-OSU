using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class Zombie : BaseCreature
    {

        private bool m_RestoreLootOnThisCorpse;
        private List<Item> m_SavedFirstCorpseLoot;
        [Constructable]
        public Zombie()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a zombie";
            Body = 3;
            BaseSoundID = 471;

            SetStr(46, 70);
            SetDex(31, 50);
            SetInt(26, 40);

            SetHits(28, 42);

            SetDamage(3, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 5, 10);

            SetSkill(SkillName.MagicResist, 15.1, 40.0);
            SetSkill(SkillName.Tactics, 35.1, 50.0);
            SetSkill(SkillName.Wrestling, 35.1, 50.0);

            Fame = 600;
            Karma = -600;

            VirtualArmor = 18;

            PackBodyPartOrBones();

            // ===== OSU Lives (exemplo) =====
            OSULivesMax = 3;
            OSULives = 3;
            OSUReviveSeconds = 30; // 30s desmaiado e volta

        }

        public Zombie(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        private bool IsFirstIntermediateDeath
        {
            get { return OSULivesMax > 1 && OSULives == OSULivesMax; }
        }

        private void SaveLootForFirstCorpse()
        {
            if (Backpack == null || Backpack.Items.Count == 0)
                return;

            m_SavedFirstCorpseLoot = new List<Item>();

            List<Item> items = new List<Item>(Backpack.Items);
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (item == null || item.Deleted)
                    continue;

                try
                {
                    Backpack.RemoveItem(item);
                    item.Internalize();
                    m_SavedFirstCorpseLoot.Add(item);
                }
                catch
                {
                }
            }
        }

        private void RestoreSavedLootToCorpse(Container c)
        {
            if (c == null || m_SavedFirstCorpseLoot == null || m_SavedFirstCorpseLoot.Count == 0)
                return;

            for (int i = 0; i < m_SavedFirstCorpseLoot.Count; i++)
            {
                Item item = m_SavedFirstCorpseLoot[i];
                if (item == null || item.Deleted)
                    continue;

                try
                {
                    c.DropItem(item);
                }
                catch
                {
                    try { item.Delete(); } catch { }
                }
            }

            m_SavedFirstCorpseLoot.Clear();
        }

        public override bool IsEnemy(Mobile m)
        {
            if (Region.IsPartOf("Haven Island"))
            {
                return false;
            }

            return base.IsEnemy(m);
        }


        public override bool OnBeforeDeath()
        {
            if (IsFirstIntermediateDeath)
            {
                m_RestoreLootOnThisCorpse = true;
                SaveLootForFirstCorpse();
            }
            else
            {
                m_RestoreLootOnThisCorpse = false;
            }

            return base.OnBeforeDeath();
        }

        public override void OnDeath(Container c)
        {
            bool restoreLoot = m_RestoreLootOnThisCorpse;
            m_RestoreLootOnThisCorpse = false;

            base.OnDeath(c);

            if (restoreLoot)
                RestoreSavedLootToCorpse(c);
        }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            if (corpse != null && corpse.OSUKnockoutCorpse)
            {
                from.SendLocalizedMessage(500485);
                return;
            }

            base.OnCarve(from, corpse, with);
        }

        protected override void OnOSULifeLost(int livesRemaining)
        {
            // Exemplo: a cada vida perdida, fica mais fraco
            RawStr = Math.Max(10, RawStr - 5);
            RawDex = Math.Max(10, RawDex - 5);
            RawInt = Math.Max(5, RawInt - 2);

            VirtualArmor = Math.Max(0, VirtualArmor - 2);

            // Se essas propriedades existirem no seu fork, reduz dano também
            try
            {
                DamageMin = Math.Max(1, DamageMin - 1);
                DamageMax = Math.Max(DamageMin, DamageMax - 1);
            }
            catch
            {
            }
        }

        protected override void OnOSURevived(int livesRemaining)
        {
            Say("*groans*");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
            writer.Write(m_RestoreLootOnThisCorpse);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 1)
                m_RestoreLootOnThisCorpse = reader.ReadBool();
            else
                m_RestoreLootOnThisCorpse = false;

            m_SavedFirstCorpseLoot = null;
        }
    }
}
