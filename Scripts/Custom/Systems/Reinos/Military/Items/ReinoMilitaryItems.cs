using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class ReinoGuardPostMarker : Item
    {
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [Constructable]
        public ReinoGuardPostMarker() : this(0)
        {
        }

        public ReinoGuardPostMarker(int cityId) : base(0x0AC9)
        {
            m_CityId = cityId;
            Movable = false;
            Visible = true;
            Name = "ponto de guarda";
            Hue = 0x44E;
        }

        public void MakeVisibleFor(TimeSpan duration)
        {
            Visible = true;
            Timer.DelayCall(duration, delegate
            {
                if (!Deleted)
                    Visible = false;
            });
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
        }
    }

    public class ReinoMilitaryRoutePoint : WayPoint
    {
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [Constructable]
        public ReinoMilitaryRoutePoint() : this(0)
        {
        }

        public ReinoMilitaryRoutePoint(int cityId) : base()
        {
            m_CityId = cityId;
            Visible = true;
            Movable = false;
            Name = "ponto de rota militar";
            Hue = 0x59B;
            ItemID = 0x0AC9;
        }

        public void MakeVisibleFor(TimeSpan duration)
        {
            Visible = true;
            Timer.DelayCall(duration, delegate
            {
                if (!Deleted)
                    Visible = false;
            });
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
        }
    }

    public class ReinoBarracksLocker : MetalChest
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value; } }

        [Constructable]
        public ReinoBarracksLocker() : this(0, String.Empty)
        {
        }

        public ReinoBarracksLocker(int cityId, string constructionKey)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Movable = false;
            Name = "caixa de apreensões do quartel";
            Hue = 0x835;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoMilitarySystem.CanAccessBarracksSubGump(pm, m_CityId))
            {
                from.SendMessage("Somente o governo ou o cargo ligado ao quartel pode abrir essa caixa.");
                return;
            }

            base.OnDoubleClick(from);
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

    public class ReinoBarracksDesk : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [Constructable]
        public ReinoBarracksDesk() : this(0, String.Empty)
        {
        }

        public ReinoBarracksDesk(int cityId, string constructionKey) : base(0xB2D)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Name = "mesa do quartel";
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoMilitarySystem.CanAccessBarracksSubGump(pm, m_CityId))
            {
                from.SendMessage("Você não tem acesso ao comando do quartel.");
                return;
            }

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(pm);
            session.RestrictToBarracksView = true;
            if (session.Tab == ReinoMilitaryTab.Laws)
                session.Tab = ReinoMilitaryTab.Guards;

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

    public class ReinoPrisonDesk : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [Constructable]
        public ReinoPrisonDesk() : this(0, String.Empty)
        {
        }

        public ReinoPrisonDesk(int cityId, string constructionKey) : base(0xB2D)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Name = "mesa da prisão";
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoMilitarySystem.CanAccessMilitaryGovernmentPage(pm, m_CityId))
            {
                from.SendMessage("Você não tem acesso à administração da prisão.");
                return;
            }

            from.SendMessage("Placeholder: aqui depois entra o item do diretor da prisão para definir pena, soltar presos, criar chaves e refeições.");
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

    public class ReinoQuartelMulti : ReinoConstructionMulti
    {
        private bool m_AuxReady;
        private int m_CityId;
        private int m_LockerSerial;
        private int m_DeskSerial;

        public ReinoQuartelMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA7, referenceId, constructionId, stageIndex)
        {
            Name = "Quartel";
            Movable = false;
        }

        public ReinoQuartelMulti(Serial serial) : base(serial)
        {
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureAuxiliary();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            EnsureAuxiliary();
            MoveAux(X - oldLocation.X, Y - oldLocation.Y, Z - oldLocation.Z);
        }

        private void EnsureAuxiliary()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            int cityId = ResolveCityId();
            m_CityId = cityId;
            string key = ReinoMaintenanceSystem.BuildLotKey(ReferenceId);

            ReinoBarracksLocker locker = FindItem(m_LockerSerial) as ReinoBarracksLocker;
            if (locker == null)
            {
                locker = new ReinoBarracksLocker(cityId, key);
                locker.MoveToWorld(new Point3D(X + 1, Y + 1, Z), Map);
                m_LockerSerial = locker.Serial.Value;
            }

            ReinoBarracksDesk desk = FindItem(m_DeskSerial) as ReinoBarracksDesk;
            if (desk == null)
            {
                desk = new ReinoBarracksDesk(cityId, key);
                desk.MoveToWorld(new Point3D(X + 2, Y + 1, Z), Map);
                m_DeskSerial = desk.Serial.Value;
            }

            m_AuxReady = true;
        }

        private void MoveAux(int dx, int dy, int dz)
        {
            Item item = FindItem(m_LockerSerial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);

            item = FindItem(m_DeskSerial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);
        }

        private int ResolveCityId()
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            return lot != null ? lot.CityId : 0;
        }

        private static Item FindItem(int serial)
        {
            Item item;
            if (serial != 0 && World.Items.TryGetValue(serial, out item))
                return item;
            return null;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            Item item = FindItem(m_LockerSerial);
            if (item != null) item.Delete();
            item = FindItem(m_DeskSerial);
            if (item != null) item.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_AuxReady);
            writer.Write(m_CityId);
            writer.Write(m_LockerSerial);
            writer.Write(m_DeskSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_AuxReady = reader.ReadBool();
            m_CityId = reader.ReadInt();
            m_LockerSerial = reader.ReadInt();
            m_DeskSerial = reader.ReadInt();
            Timer.DelayCall(TimeSpan.FromSeconds(2.0), EnsureAuxiliary);
        }
    }

    public class ReinoPrisaoMulti : ReinoConstructionMulti
    {
        private int m_CityId;
        private int m_DeskSerial;

        public ReinoPrisaoMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA7, referenceId, constructionId, stageIndex)
        {
            Name = "Prisão";
            Movable = false;
        }

        public ReinoPrisaoMulti(Serial serial) : base(serial)
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
            Item desk = FindItem(m_DeskSerial);
            if (desk != null)
                desk.MoveToWorld(new Point3D(desk.X + (X - oldLocation.X), desk.Y + (Y - oldLocation.Y), desk.Z + (Z - oldLocation.Z)), Map);
        }

        private void EnsureDesk()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            int cityId = ResolveCityId();
            m_CityId = cityId;
            string key = ReinoMaintenanceSystem.BuildLotKey(ReferenceId);

            ReinoPrisonDesk desk = FindItem(m_DeskSerial) as ReinoPrisonDesk;
            if (desk == null)
            {
                desk = new ReinoPrisonDesk(cityId, key);
                desk.MoveToWorld(new Point3D(X + 1, Y + 1, Z), Map);
                m_DeskSerial = desk.Serial.Value;
            }
        }

        private int ResolveCityId()
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            return lot != null ? lot.CityId : 0;
        }

        private static Item FindItem(int serial)
        {
            Item item;
            if (serial != 0 && World.Items.TryGetValue(serial, out item))
                return item;
            return null;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            Item desk = FindItem(m_DeskSerial);
            if (desk != null) desk.Delete();
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
