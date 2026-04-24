using System;
using Server.Custom.Systems.Stables.Engine;

namespace Server.Mobiles
{
    [CorpseName("a goat corpse")]
    public class Goat : BaseCreature
    {
        [Constructable]
        public Goat()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a goat";
            this.Body = 0xD1;
            this.Female = Utility.RandomBool();
            this.BaseSoundID = 0x99;

            this.SetStr(19);
            this.SetDex(15);
            this.SetInt(5);

            this.SetHits(12);
            this.SetMana(0);

            this.SetDamage(3, 4);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 5, 15);

            this.SetSkill(SkillName.MagicResist, 5.0);
            this.SetSkill(SkillName.Tactics, 5.0);
            this.SetSkill(SkillName.Wrestling, 5.0);

            this.Fame = 150;
            this.Karma = 0;

            this.VirtualArmor = 10;

            this.Tamable = true;
            OSUPetBreedGroup = "goat";
            if (OSUPetBreedCountMax <= 0)
                OSUPetBreedCountMax = 6;
            this.ControlSlots = 1;
            this.MinTameSkill = 11.1;
        }

        public Goat(Serial serial)
            : base(serial)
        {
        }


        private DateTime m_MilkedOn;
        private int m_Milk;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime MilkedOn
        {
            get { return m_MilkedOn; }
            set { m_MilkedOn = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Milk
        {
            get { return m_Milk; }
            set { m_Milk = value; }
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
                from.SendLocalizedMessage(1080400);
                return false;
            }

            bool authorizedMarkedOwner = OSUPetMarked && OSUPetBrandOwnerSerial == from.Serial.Value;

            if (Controlled && ControlMaster != from && !authorizedMarkedOwner)
            {
                from.SendLocalizedMessage(1071182);
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

        public override int Meat
        {
            get
            {
                return OSUStablePetSystem.ScaleFarmDeathResource(this, 2, "meat");
            }
        }
        public override int Hides
        {
            get
            {
                return OSUStablePetSystem.ScaleFarmDeathResource(this, 8, "hides");
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay | FoodType.FruitsAndVegies;
            }
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

            if (version >= 1)
            {
                m_MilkedOn = reader.ReadDateTime();
                m_Milk = reader.ReadInt();
            }
        }
    }
}
