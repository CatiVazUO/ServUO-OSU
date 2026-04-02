using Server;
using Server.Multis;
using Server.Items;

namespace Server.Custom.Systems.Reinos
{
    public abstract class ReinoPlacedMultiBase : BaseMulti
    {
        private int m_ReferenceId;
        private string m_ConstructionId;
        private int m_StageIndex;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReferenceId { get { return m_ReferenceId; } set { m_ReferenceId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionId { get { return m_ConstructionId; } set { m_ConstructionId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StageIndex { get { return m_StageIndex; } set { m_StageIndex = value; } }

        protected ReinoPlacedMultiBase(int multiId, int referenceId, string constructionId, int stageIndex) : base(multiId)
        {
            m_ReferenceId = referenceId;
            m_ConstructionId = constructionId ?? string.Empty;
            m_StageIndex = stageIndex;
        }

        protected ReinoPlacedMultiBase(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_ReferenceId);
            writer.Write(m_ConstructionId);
            writer.Write(m_StageIndex);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_ReferenceId = reader.ReadInt();
            m_ConstructionId = reader.ReadString();
            m_StageIndex = reader.ReadInt();
        }
    }
}
