using System;
using Server.Custom.Reinos;
using Server.Mobiles;

namespace Server.Custom.Reinos.Expansion.Engine
{
    public abstract class BaseReinoLotThreat : BaseCreature
    {
        private int m_LotId;
        private int m_ConfigId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int LotId
        {
            get { return m_LotId; }
            set { m_LotId = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ConfigId
        {
            get { return m_ConfigId; }
            set { m_ConfigId = value; }
        }

        public override bool DeleteCorpseOnDeath
        {
            get { return false; }
        }

        public override bool AlwaysMurderer
        {
            get { return true; }
        }

        protected BaseReinoLotThreat(AIType ai, FightMode mode, int rangePerception, int rangeFight, double activeSpeed, double passiveSpeed)
            : base(ai, mode, rangePerception, rangeFight, activeSpeed, passiveSpeed)
        {
            Tamable = false;
            ControlSlots = 0;
            Fame = 0;
            Karma = 0;
        }

        public BaseReinoLotThreat(Serial serial) : base(serial)
        {
        }

        public override void GenerateLoot()
        {
        }

        public override bool OnBeforeDeath()
        {
            return base.OnBeforeDeath();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_LotId);
            writer.Write(m_ConfigId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_LotId = reader.ReadInt();
            m_ConfigId = reader.ReadInt();
        }
    }
}
