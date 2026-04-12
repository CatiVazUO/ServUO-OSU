using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class ReinoTribunalDesk : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [Constructable]
        public ReinoTribunalDesk() : this(0, String.Empty)
        {
        }

        public ReinoTribunalDesk(int cityId, string constructionKey) : base(0xB2D)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Name = "mesa do tribunal";
            Movable = false;
        }

        public ReinoTribunalDesk(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoTrialsSystem.CanAccessLawSettings(pm, m_CityId))
            {
                pm.SendMessage("Somente o líder, um cargo de hierarquia 2 permitido, ou o cargo ligado ao tribunal pode usar essa mesa.");
                return;
            }

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(pm);
            session.RestrictToBarracksView = false;
            session.Tab = ReinoMilitaryTab.Laws;

            pm.SendGump(new ReinoExpansionGump(pm, m_CityId, -1, -1, String.Empty, 0, 8));
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

    public class ReinoTribunalHammer : Item
    {
        private int m_CityId;
        private string m_RoleTitle;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string RoleTitle { get { return m_RoleTitle; } set { m_RoleTitle = value ?? String.Empty; UpdateName(); } }

        [Constructable]
        public ReinoTribunalHammer() : this(0, String.Empty)
        {
        }

        public ReinoTribunalHammer(int cityId, string roleTitle) : base(0x13E3)
        {
            m_CityId = cityId;
            m_RoleTitle = roleTitle ?? String.Empty;
            Layer = Layer.OneHanded;
            LootType = LootType.Blessed;
            Movable = true;
            Weight = 1.0;
            UpdateName();
        }

        public ReinoTribunalHammer(Serial serial) : base(serial)
        {
        }

        private void UpdateName()
        {
            Name = String.IsNullOrWhiteSpace(m_RoleTitle) ? "martelo do tribunal" : ("martelo do " + m_RoleTitle);
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoTrialsSystem.CanAccessTribunalControl(pm, m_CityId))
            {
                pm.SendMessage("Você não ocupa um cargo ligado ao tribunal deste reino.");
                return;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(pm, m_CityId)
                && pm.FindItemOnLayer(Layer.OneHanded) != this
                && pm.FindItemOnLayer(Layer.TwoHanded) != this)
            {
                pm.SendMessage("Você precisa estar empunhando o martelo para abrir o tribunal.");
                return;
            }

            pm.SendGump(new ReinoTribunalGump(pm, m_CityId));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_RoleTitle ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_RoleTitle = reader.ReadString();
            LootType = LootType.Blessed;
            Layer = Layer.OneHanded;
            UpdateName();
        }
    }

    public class ReinoTribunalMulti : ReinoConstructionMulti
    {
        private int m_CityId;
        private int m_DeskSerial;

        public ReinoTribunalMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA7, referenceId, constructionId, stageIndex)
        {
            Name = "Tribunal";
            Movable = false;
        }

        public ReinoTribunalMulti(Serial serial) : base(serial)
        {
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureDesk();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            EnsureDesk();

            Item desk = FindWorldItem(m_DeskSerial);
            if (desk != null && !desk.Deleted)
                desk.MoveToWorld(new Point3D(desk.X + (X - oldLocation.X), desk.Y + (Y - oldLocation.Y), desk.Z + (Z - oldLocation.Z)), Map);
        }

        private void EnsureDesk()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            int cityId = ResolveCityId();
            m_CityId = cityId;
            string key = ReinoMaintenanceSystem.BuildLotKey(ReferenceId);

            ReinoTribunalDesk desk = FindWorldItem(m_DeskSerial) as ReinoTribunalDesk;
            if (desk == null)
            {
                desk = new ReinoTribunalDesk(cityId, key);
                desk.MoveToWorld(new Point3D(X + TribunalAuroraDefinition.DESK_OFFSET.X, Y + TribunalAuroraDefinition.DESK_OFFSET.Y, Z + TribunalAuroraDefinition.DESK_OFFSET.Z), Map);
                m_DeskSerial = desk.Serial.Value;
            }
        }

        private int ResolveCityId()
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            return lot != null ? lot.CityId : 0;
        }

        private static Item FindWorldItem(int serial)
        {
            Item item;
            if (serial != 0 && World.Items.TryGetValue(serial, out item))
                return item;
            return null;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            Item desk = FindWorldItem(m_DeskSerial);
            if (desk != null)
                desk.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_DeskSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_DeskSerial = reader.ReadInt();
            Timer.DelayCall(TimeSpan.FromSeconds(2.0), EnsureDesk);
        }
    }
}
