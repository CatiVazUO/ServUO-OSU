using Server;
using Server.Custom.Systems.Postos;
using Server.Mobiles;
using System;

namespace Server.Items
{
    public class PostoResourceChest : Item
    {
        private string m_PostoId;

        [CommandProperty(AccessLevel.GameMaster)]
        public string PostoId
        {
            get { return m_PostoId; }
            set
            {
                m_PostoId = value ?? String.Empty;
                RefreshState();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StoredAmount
        {
            get { return PostoSystem.GetStoredAmount(m_PostoId); }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string OwnerCity
        {
            get
            {
                PostoState st = PostoSystem.GetState(m_PostoId);
                return PostoSystem.GetOwnerLabel(st);
            }
            set { }
        }

        public override bool ForceShowProperties
        {
            get { return ObjectPropertyList.Enabled; }
        }

        [Constructable]
        public PostoResourceChest() : this(String.Empty)
        {
        }

        public PostoResourceChest(string postoId) : base(0xE43)
        {
            Movable = false;
            LootType = LootType.Blessed;
            Weight = 10.0;

            m_PostoId = postoId ?? String.Empty;
            RefreshFromDefinition();
        }

        private void RefreshFromDefinition()
        {
            PostoDefinition def = PostoSystem.GetDefinition(m_PostoId);

            if (def == null)
            {
                Name = "baú de despacho";
                Hue = 0;
                return;
            }

            Name = def.GetChestName();

            switch (def.ResourceType)
            {
                case PostoResourceType.Iron:
                    Hue = 0x835;
                    break;
                case PostoResourceType.Wood:
                    Hue = 0x455;
                    break;
                case PostoResourceType.Cotton:
                    Hue = 0x47F;
                    break;
                default:
                    Hue = 0;
                    break;
            }
        }

        public void RefreshState()
        {
            RefreshFromDefinition();
            InvalidateProperties();

            if (Parent == null && Map != null)
                Delta(ItemDelta.Update);
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null || pm.Deleted)
                return;

            if (!from.InRange(Location, 2))
            {
                from.SendLocalizedMessage(500446);
                return;
            }

            string msg;
            int amount;

            if (PostoSystem.TryDispatch(pm, m_PostoId, out msg, out amount))
                pm.SendMessage(msg);
            else
                pm.SendMessage(msg);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            PostoDefinition def = PostoSystem.GetDefinition(m_PostoId);
            PostoState state = PostoSystem.GetState(m_PostoId);

            if (def == null || state == null)
                return;

            int amount = PostoSystem.GetStoredAmount(m_PostoId);
            list.Add("Armazenando: {0} de {1}", amount, PostoSystem.GetResourceDisplayName(def.ResourceType));
            list.Add("Reino dono: {0}", PostoSystem.GetOwnerLabel(state));
        }

        public PostoResourceChest(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
            writer.Write(m_PostoId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_PostoId = reader.ReadString();

            RefreshState();
        }
    }
}
