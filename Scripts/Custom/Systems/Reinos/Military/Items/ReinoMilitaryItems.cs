using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class ReinoGuardPostMarker : Item
    {
        private int m_CityId;
        private int m_PostId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PostId { get { return m_PostId; } set { m_PostId = value; } }

        [Constructable]
        public ReinoGuardPostMarker() : this(0)
        {
        }

        public ReinoGuardPostMarker(int cityId) : base(0x0AC9)
        {
            m_CityId = cityId;
            m_PostId = 0;
            Movable = false;
            Visible = true;
            Name = "ponto de guarda";
            Hue = 0x44E;
        }

        public ReinoGuardPostMarker(Serial serial) : base(serial)
        {
        }

        public void MakeVisibleFor(TimeSpan duration)
        {
            SetTemporaryVisible(true, duration);
        }

        public void SetTemporaryVisible(bool visible, TimeSpan? duration)
        {
            Visible = visible;

            if (visible && duration.HasValue)
            {
                Timer.DelayCall(duration.Value, delegate
                {
                    if (!Deleted)
                        Visible = false;
                });
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_CityId);
            writer.Write(m_PostId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_PostId = version >= 1 ? reader.ReadInt() : 0;
        }
    }

    public class ReinoMilitaryRoutePoint : WayPoint
    {
        private int m_CityId;
        private int m_PostId;
        private int m_RouteHue;
        private bool m_Closed;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PostId { get { return m_PostId; } set { m_PostId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RouteHue { get { return m_RouteHue; } set { m_RouteHue = value; Hue = value <= 0 ? 0x59B : value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ClosedRoute { get { return m_Closed; } set { m_Closed = value; } }

        [Constructable]
        public ReinoMilitaryRoutePoint() : this(0)
        {
        }

        public ReinoMilitaryRoutePoint(int cityId) : base()
        {
            m_CityId = cityId;
            m_PostId = 0;
            m_RouteHue = 0x59B;
            m_Closed = false;
            Visible = true;
            Movable = false;
            Name = "ponto de rota militar";
            Hue = 0x59B;
            ItemID = 0x0AC9;
        }

        public ReinoMilitaryRoutePoint(Serial serial) : base(serial)
        {
        }

        public void MakeVisibleFor(TimeSpan duration)
        {
            SetTemporaryVisible(true, duration);
        }

        public void SetTemporaryVisible(bool visible, TimeSpan? duration)
        {
            Visible = visible;

            if (visible && duration.HasValue)
            {
                Timer.DelayCall(duration.Value, delegate
                {
                    if (!Deleted)
                        Visible = false;
                });
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_CityId);
            writer.Write(m_PostId);
            writer.Write(m_RouteHue);
            writer.Write(m_Closed);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_PostId = version >= 1 ? reader.ReadInt() : 0;
            m_RouteHue = version >= 1 ? reader.ReadInt() : 0x59B;
            m_Closed = version >= 1 && reader.ReadBool();
            Hue = m_RouteHue <= 0 ? 0x59B : m_RouteHue;
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

        public ReinoBarracksLocker(Serial serial) : base(serial)
        {
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

        public ReinoBarracksDesk(Serial serial) : base(serial)
        {
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

        public ReinoPrisonDesk(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoPrisionSystem.CanAccessPrisonControl(pm, m_CityId))
            {
                from.SendMessage("Você não tem acesso à administração da prisão.");
                return;
            }

            pm.CloseGump(typeof(ReinoPrisionGump));
            pm.SendGump(new ReinoPrisionGump(pm, m_CityId));
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

    public class ReinoBarracksBadge : Item
    {
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [Constructable]
        public ReinoBarracksBadge() : this(0)
        {
        }

        public ReinoBarracksBadge(int cityId) : base(0x171B)
        {
            m_CityId = cityId;
            Name = "insígnia do quartel";
            LootType = LootType.Blessed;
            Movable = true;
        }

        public ReinoBarracksBadge(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!ReinoMilitarySystem.CanAccessBarracksSubGump(pm, m_CityId))
            {
                pm.SendMessage("Você não ocupa um cargo ligado ao quartel deste reino.");
                return;
            }

            pm.SendGump(new ReinoMilitaryMiniGump(pm, m_CityId, ReinoMilitaryTab.Guards));
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
            LootType = LootType.Blessed;
        }
    }

    [Flipable(0x1389, 0x138A)]
    public class ReinoLawBoard : Item
    {
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [Constructable]
        public ReinoLawBoard() : this(0)
        {
        }

        public ReinoLawBoard(int cityId) : base(0x1389)
        {
            m_CityId = cityId;
            Name = "placa de leis do reino";
            Movable = false;
        }

        public ReinoLawBoard(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            pm.SendGump(new ReinoLawBoardGump(pm, m_CityId));
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

            ReinoBarracksLocker locker = FindWorldItem(m_LockerSerial) as ReinoBarracksLocker;
            if (locker == null)
            {
                locker = new ReinoBarracksLocker(cityId, key);
                locker.MoveToWorld(new Point3D(X + 1, Y + 1, Z), Map);
                m_LockerSerial = locker.Serial.Value;
            }

            ReinoBarracksDesk desk = FindWorldItem(m_DeskSerial) as ReinoBarracksDesk;
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
            Item item = FindWorldItem(m_LockerSerial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);

            item = FindWorldItem(m_DeskSerial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);
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
            Item item = FindWorldItem(m_LockerSerial);
            if (item != null) item.Delete();
            item = FindWorldItem(m_DeskSerial);
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
        private int m_LockerSerial;
        private int m_JailerSerial;
        private int m_GuardSerial;

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

            ReinoPrisonDesk desk = FindWorldItem(m_DeskSerial) as ReinoPrisonDesk;
            if (desk == null)
            {
                desk = new ReinoPrisonDesk(cityId, key);
                Point3D deskOffset = PrisaoAuroraDefinition.GetDeskOffset();
                desk.MoveToWorld(new Point3D(X + deskOffset.X, Y + deskOffset.Y, Z + deskOffset.Z), Map);
                m_DeskSerial = desk.Serial.Value;
            }

            ReinoPrisonLocker locker = FindWorldItem(m_LockerSerial) as ReinoPrisonLocker;
            if (locker == null)
            {
                locker = new ReinoPrisonLocker(cityId, key);
                Point3D lockerOffset = PrisaoAuroraDefinition.GetLockerOffset();
                locker.MoveToWorld(new Point3D(X + lockerOffset.X, Y + lockerOffset.Y, Z + lockerOffset.Z), Map);
                m_LockerSerial = locker.Serial.Value;
            }

            OSUCarcereiro jailer = FindWorldMobile(m_JailerSerial) as OSUCarcereiro;
            if (jailer == null)
            {
                jailer = new OSUCarcereiro(cityId);
                Point3D jailerOffset = PrisaoAuroraDefinition.GetJailerOffset();
                jailer.MoveToWorld(new Point3D(X + jailerOffset.X, Y + jailerOffset.Y, Z + jailerOffset.Z), Map);
                m_JailerSerial = jailer.Serial.Value;
            }

            OSUGuardaDePrisao guard = FindWorldMobile(m_GuardSerial) as OSUGuardaDePrisao;
            if (guard == null)
            {
                guard = new OSUGuardaDePrisao(cityId);
                Point3D guardOffset = PrisaoAuroraDefinition.GetPrisonGuardOffset();
                guard.MoveToWorld(new Point3D(X + guardOffset.X, Y + guardOffset.Y, Z + guardOffset.Z), Map);
                m_GuardSerial = guard.Serial.Value;
            }

            SyncNpcSerials();
        }

        private void MoveAux(int dx, int dy, int dz)
        {
            Item item = FindWorldItem(m_DeskSerial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);

            item = FindWorldItem(m_LockerSerial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);

            Mobile mob = FindWorldMobile(m_JailerSerial);
            if (mob != null && !mob.Deleted)
                mob.MoveToWorld(new Point3D(mob.X + dx, mob.Y + dy, mob.Z + dz), Map);

            mob = FindWorldMobile(m_GuardSerial);
            if (mob != null && !mob.Deleted)
                mob.MoveToWorld(new Point3D(mob.X + dx, mob.Y + dy, mob.Z + dz), Map);
        }

        private void SyncNpcSerials()
        {
            ReinoLotState state = ReinoExpansionSystem.GetLotState(ReferenceId);
            if (state == null)
                return;

            if (state.NpcSerials == null)
                state.NpcSerials = new System.Collections.Generic.List<int>();

            state.NpcSerials.Clear();

            if (m_JailerSerial > 0)
                state.NpcSerials.Add(m_JailerSerial);

            if (m_GuardSerial > 0)
                state.NpcSerials.Add(m_GuardSerial);
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

        private static Mobile FindWorldMobile(int serial)
        {
            Mobile mob;
            if (serial != 0 && World.Mobiles.TryGetValue(serial, out mob))
                return mob;
            return null;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            Item item = FindWorldItem(m_DeskSerial);
            if (item != null) item.Delete();

            item = FindWorldItem(m_LockerSerial);
            if (item != null) item.Delete();

            Mobile mob = FindWorldMobile(m_JailerSerial);
            if (mob != null) mob.Delete();

            mob = FindWorldMobile(m_GuardSerial);
            if (mob != null) mob.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_CityId);
            writer.Write(m_DeskSerial);
            writer.Write(m_LockerSerial);
            writer.Write(m_JailerSerial);
            writer.Write(m_GuardSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_DeskSerial = reader.ReadInt();
            m_LockerSerial = version >= 1 ? reader.ReadInt() : 0;
            m_JailerSerial = version >= 1 ? reader.ReadInt() : 0;
            m_GuardSerial = version >= 1 ? reader.ReadInt() : 0;
            Timer.DelayCall(TimeSpan.FromSeconds(2.0), EnsureAuxiliary);
        }
    }
}
