using System;
using Server.Custom.Systems.Stables.Engine;

namespace Server.Mobiles
{
    [CorpseName("a cow corpse")]
    public class Cow : BaseCreature
    {
        private DateTime m_MilkedOn;
        private int m_Milk;
        [Constructable]
        public Cow()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a cow";
            Body = Utility.RandomList(0xD8, 0xE7);
            Female = (Body == 0xD8);
            BaseSoundID = 0x78;

            SetStr(30);
            SetDex(15);
            SetInt(5);

            SetHits(18);
            SetMana(0);

            SetDamage(1, 4);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 5, 15);

            SetSkill(SkillName.MagicResist, 5.5);
            SetSkill(SkillName.Tactics, 5.5);
            SetSkill(SkillName.Wrestling, 5.5);

            Fame = 300;
            Karma = 0;

            VirtualArmor = 10;

            Tamable = true;
            OSUPetBreedGroup = "bovine";
            if (OSUPetBreedCountMax <= 0)
                OSUPetBreedCountMax = 6;
            ControlSlots = 1;
            MinTameSkill = 11.1;

            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public Cow(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime MilkedOn
        {
            get
            {
                return m_MilkedOn;
            }
            set
            {
                m_MilkedOn = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Milk
        {
            get
            {
                return m_Milk;
            }
            set
            {
                m_Milk = value;
            }
        }
        public override int Meat
        {
            get
            {
                return OSUStablePetSystem.ScaleFarmDeathResource(this, 8, "meat");
            }
        }
        public override int Hides
        {
            get
            {
                return OSUStablePetSystem.ScaleFarmDeathResource(this, 12, "hides");
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);

            int random = Utility.Random(100);

            if (random < 5)
                Tip();
            else if (random < 20)
                PlaySound(120);
            else if (random < 40)
                PlaySound(121);
        }

        public void Tip()
        {
            PlaySound(121);
            Animate(8, 0, 3, true, false, 0);
        }

        private void RefreshMilk()
        {
            if (m_MilkedOn == DateTime.MinValue)
            {
                m_Milk = Math.Max(m_Milk, 5);
                m_MilkedOn = DateTime.UtcNow;
                return;
            }

            TimeSpan delay = OSUStablePetSystem.GetFarmProductionDelay(this, TimeSpan.FromHours(12.0));
            TimeSpan elapsed = DateTime.UtcNow - m_MilkedOn;
            int produced = (int)(elapsed.TotalSeconds / Math.Max(1.0, delay.TotalSeconds));

            if (produced <= 0)
                return;

            m_Milk = Math.Min(5, m_Milk + produced);
            m_MilkedOn = m_MilkedOn + TimeSpan.FromSeconds(delay.TotalSeconds * produced);
        }

        public bool TryMilk(Mobile from)
        {
            string reason;
            if (!OSUStablePetSystem.CanTakeFarmResources(this, from, out reason))
            {
                from.SendMessage(reason);
                return false;
            }

            if (!from.InLOS(this) || !from.InRange(Location, 2))
            {
                from.SendLocalizedMessage(1080400); // You can not milk the cow from this location.
                return false;
            }

            bool authorizedMarkedOwner = OSUPetMarked && OSUPetBrandOwnerSerial == from.Serial.Value;

            if (Controlled && ControlMaster != from && !authorizedMarkedOwner)
            {
                from.SendLocalizedMessage(1071182); // The cow nimbly escapes your attempts to milk it.
                return false;
            }

            RefreshMilk();

            if (m_Milk <= 0)
            {
                from.SendLocalizedMessage(1080198);
                return false;
            }

            m_Milk--;

            OSUStablePetSystem.OnFarmResourceProduced(this, from, 1);
            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.Write((DateTime)m_MilkedOn);
            writer.Write((int)m_Milk);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version > 0)
            {
                m_MilkedOn = reader.ReadDateTime();
                m_Milk = reader.ReadInt();
            }
        }
    }
}
