using System;
using Server.Custom.Systems.Reinos;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos.Expansion.Engine
{
    public class ReinoLotCollectible : Item
    {
        private int m_LotId;
        private int m_ConfigId;
        private string m_CollectibleTypeName;
        private string m_RequiredToolTypeName;

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

        [CommandProperty(AccessLevel.GameMaster)]
        public string CollectibleTypeName
        {
            get { return m_CollectibleTypeName; }
            set { m_CollectibleTypeName = value ?? string.Empty; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string RequiredToolTypeName
        {
            get { return m_RequiredToolTypeName; }
            set { m_RequiredToolTypeName = value ?? string.Empty; }
        }

        [Constructable]
        public ReinoLotCollectible() : base(0x0D15)
        {
            Movable = false;
            Name = "ameaça do lote";
            m_LotId = 0;
            m_ConfigId = 0;
            m_CollectibleTypeName = string.Empty;
            m_RequiredToolTypeName = string.Empty;
        }

        public ReinoLotCollectible(int itemId, int hue, string name, int lotId, int configId, string collectibleTypeName, string requiredToolTypeName)
            : base(itemId)
        {
            Movable = false;
            Hue = hue;
            Name = name ?? "ameaça do lote";
            m_LotId = lotId;
            m_ConfigId = configId;
            m_CollectibleTypeName = collectibleTypeName ?? string.Empty;
            m_RequiredToolTypeName = requiredToolTypeName ?? string.Empty;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null || pm.Deleted)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendLocalizedMessage(500446);
                return;
            }

            if (!ReinoExpansionSystem.ValidateLotCollectibleTool(pm, m_RequiredToolTypeName, out var toolMessage))
            {
                if (!string.IsNullOrWhiteSpace(toolMessage))
                    pm.SendMessage(toolMessage);

                return;
            }

            if (pm.Mounted)
            {
                pm.SendMessage("Você não pode coletar isso enquanto estiver montado.");
                return;
            }

            if (ReinoExpansionSystem.NotifyLotCollectibleUsed(pm, m_LotId, m_CollectibleTypeName))
            {
                pm.Animate(14, 5, 1, true, false, 0);
                Delete();
            }
        }

        public ReinoLotCollectible(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_LotId);
            writer.Write(m_ConfigId);
            writer.Write(m_CollectibleTypeName);
            writer.Write(m_RequiredToolTypeName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_LotId = reader.ReadInt();
            m_ConfigId = reader.ReadInt();
            m_CollectibleTypeName = reader.ReadString();
            m_RequiredToolTypeName = reader.ReadString();
            Movable = false;
        }
    }
}
