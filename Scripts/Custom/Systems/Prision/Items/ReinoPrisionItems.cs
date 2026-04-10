using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Needs;

namespace Server.Custom.Reinos
{
    public class ReinoPrisonLocker : MetalChest
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [Constructable]
        public ReinoPrisonLocker() : this(0, String.Empty)
        {
        }

        public ReinoPrisonLocker(int cityId, string constructionKey)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Name = "baú de pertences da prisão";
            Movable = false;
            Hue = 0x835;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoPrisionSystem.CanAccessPrisonControl(pm, m_CityId))
            {
                pm.SendMessage("Você não tem acesso ao baú da prisão.");
                return;
            }

            base.OnDoubleClick(from);
        }

        public ReinoPrisonLocker(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            Movable = false;
        }
    }

    public class RefeicaoDoPreso : Item
    {
        [Constructable]
        public RefeicaoDoPreso() : base(0x09AF)
        {
            Name = "Refeição do preso";
            Movable = true;
            Timer.DelayCall(TimeSpan.FromMinutes(10.0), DeleteIfStillHere);
        }

        public RefeicaoDoPreso(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            bool ate = OSUNeedsSystem.TryAddHunger(pm, 20);
            bool drank = OSUNeedsSystem.TryAddThirst(pm, 20);

            if (!ate && !drank)
            {
                pm.SendMessage("Você não consegue consumir a refeição agora.");
                return;
            }

            pm.SendMessage("Você consome a refeição do preso.");
            Delete();
        }

        private void DeleteIfStillHere()
        {
            if (!Deleted)
                Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            Timer.DelayCall(TimeSpan.FromMinutes(10.0), DeleteIfStillHere);
        }
    }
 
}
