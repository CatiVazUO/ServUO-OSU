using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Templos;
using Server.Custom.Systems.Templos.Gumps;
using Server.Custom.Systems.Templos.Items;

namespace Server.Custom.Reinos
{
    public class ReinoTemploAltar : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [Constructable]
        public ReinoTemploAltar() : this(0, String.Empty)
        {
        }

        public ReinoTemploAltar(int cityId, string constructionKey) : base(0x1185)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Movable = false;
            Name = "altar do templo";
        }

        public ReinoTemploAltar(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais do altar.");
                return;
            }

            if (!TemploSystem.CanAccessTemple(pm, m_CityId, m_ConstructionKey))
            {
                pm.SendMessage("Somente o líder ou o cargo ligado ao templo pode usar esse altar.");
                return;
            }

            pm.CloseGump(typeof(TemploGump));
            pm.SendGump(new TemploGump(pm, m_CityId, m_ConstructionKey, this.Serial.Value));
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

    public class ReinoTemploStatua : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [Constructable]
        public ReinoTemploStatua() : this(0, String.Empty, 0x1223)
        {
        }

        public ReinoTemploStatua(int cityId, string constructionKey, int itemId) : base(itemId)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Movable = false;
            Name = "estátua do deus";
        }

        public ReinoTemploStatua(Serial serial) : base(serial)
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

    public class ReinoTemploDoor : BaseDoor
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [Constructable]
        public ReinoTemploDoor() : this(0x675, 0x676, 0xEC, 0xF3, Point3D.Zero, 0, String.Empty)
        {
        }

        public ReinoTemploDoor(int closedId, int openedId, int openedSound, int closedSound, Point3D offset, int cityId, string constructionKey)
            : base(closedId, openedId, openedSound, closedSound, offset)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Locked = false;
            KeyValue = 0;
            Movable = false;
        }

        public ReinoTemploDoor(Serial serial) : base(serial)
        {
        }

        public override bool UseLocks()
        {
            return false;
        }

        public override void Use(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm != null &&
                TemploSystem.IsTempleClosedToPublic(m_ConstructionKey) &&
                !TemploSystem.CanAccessTemple(pm, m_CityId, m_ConstructionKey) &&
                !TemploSystem.IsInsideTempleLot(m_ConstructionKey, pm))
            {
                pm.SendMessage("O templo está fechado ao público.");
                return;
            }

            base.Use(from);
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
            Locked = false;
            KeyValue = 0;
            Movable = false;
        }
    }

    public class ReinoTemploMulti : ReinoConstructionMulti
    {
        private int m_CityId;
        private int m_AltarSerial;
        private int m_ChestSerial;
        private int m_StatueSerial;

        public ReinoTemploMulti(int referenceId, string constructionId, int stageIndex)
            : base(0x147B, referenceId, constructionId, stageIndex)
        {
            Name = "Templo";
            Movable = false;
        }

        public ReinoTemploMulti(Serial serial) : base(serial)
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
            string key = ReinoMaintenanceSystem.BuildLotKey(ReferenceId);
            m_CityId = cityId;

            if (m_ChestSerial == 0)
                m_ChestSerial = TemploSystem.GetRegisteredChestSerial(key);

            ReinoTemploAltar altar = FindWorldItem(m_AltarSerial) as ReinoTemploAltar;
            if (altar == null)
            {
                altar = new ReinoTemploAltar(cityId, key);
                Point3D offset = TemploAuroraDefinition.GetAltarOffset();
                altar.MoveToWorld(new Point3D(X + offset.X, Y + offset.Y, Z + offset.Z), Map);
                m_AltarSerial = altar.Serial.Value;
            }
            else
            {
                altar.CityId = cityId;
                altar.ConstructionKey = key;
            }

            BauDoacoesTemplo chest = FindWorldItem(m_ChestSerial) as BauDoacoesTemplo;
            if (chest == null)
            {
                Point3D offset = TemploAuroraDefinition.GetDonationChestOffset();

                chest = new BauDoacoesTemplo(cityId);
                chest.MoveToWorld(new Point3D(X + offset.X, Y + offset.Y, Z + offset.Z), Map);

                m_ChestSerial = chest.Serial.Value;
            }
            else
            {
                chest.CityId = cityId;
            }

            TemploSystem.SyncPlacedAssets(key, cityId, m_AltarSerial, m_ChestSerial, m_StatueSerial);
            TemploSystem.RefreshTempleStatue(key);
            RefreshStatueSerial(key);
            ReplaceTempleDoors(key, cityId);
            TemploSystem.SyncPlacedAssets(key, cityId, m_AltarSerial, m_ChestSerial, m_StatueSerial);
        }

        private void RefreshStatueSerial(string key)
        {
            ReinoTemploStatua nearest = null;
            int best = Int32.MaxValue;

            foreach (Item item in World.Items.Values)
            {
                ReinoTemploStatua statue = item as ReinoTemploStatua;
                if (statue == null || statue.Deleted || statue.Map != Map)
                    continue;

                if (!String.Equals(statue.ConstructionKey, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                int dist = GetDistanceSquared(statue.Location, Location);
                if (dist < best)
                {
                    best = dist;
                    nearest = statue;
                }
            }

            m_StatueSerial = nearest != null ? nearest.Serial.Value : 0;
        }

        private void MoveAux(int dx, int dy, int dz)
        {
            MoveOne(m_AltarSerial, dx, dy, dz);
            MoveOne(m_ChestSerial, dx, dy, dz);
            MoveOne(m_StatueSerial, dx, dy, dz);
        }

        private void MoveOne(int serial, int dx, int dy, int dz)
        {
            Item item = FindWorldItem(serial);
            if (item != null && !item.Deleted)
                item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);
        }

        private void ReplaceTempleDoors(string constructionKey, int cityId)
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            if (lot == null || lot.Map != Map)
                return;

            List<BaseDoor> originals = new List<BaseDoor>();

            foreach (Item item in World.Items.Values)
            {
                BaseDoor door = item as BaseDoor;
                if (door == null || door.Deleted || door.Map != Map)
                    continue;

                if (!lot.Contains(door.Location))
                    continue;

                if (door is ReinoTemploDoor)
                {
                    ReinoTemploDoor td = (ReinoTemploDoor)door;
                    td.CityId = cityId;
                    td.ConstructionKey = constructionKey;
                    continue;
                }

                originals.Add(door);
            }

            if (originals.Count == 0)
                return;

            Dictionary<int, ReinoTemploDoor> map = new Dictionary<int, ReinoTemploDoor>();

            for (int i = 0; i < originals.Count; i++)
            {
                BaseDoor old = originals[i];
                ReinoTemploDoor created = new ReinoTemploDoor(old.ClosedID, old.OpenedID, old.OpenedSound, old.ClosedSound, old.Offset, cityId, constructionKey);
                created.MoveToWorld(old.Location, old.Map);
                created.Locked = false;
                created.KeyValue = 0;

                if (old.Open)
                {
                    created.Open = true;
                    created.MoveToWorld(old.Location, old.Map);
                }

                map[old.Serial.Value] = created;
            }

            for (int i = 0; i < originals.Count; i++)
            {
                BaseDoor old = originals[i];
                ReinoTemploDoor created;
                if (!map.TryGetValue(old.Serial.Value, out created))
                    continue;

                BaseDoor oldLink = old.Link;
                if (oldLink != null)
                {
                    ReinoTemploDoor newLink;
                    if (map.TryGetValue(oldLink.Serial.Value, out newLink))
                        created.Link = newLink;
                }
            }

            for (int i = 0; i < originals.Count; i++)
                originals[i].Delete();
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

        private static int GetDistanceSquared(Point3D a, Point3D b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            int dz = a.Z - b.Z;
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        public override void OnAfterDelete()
        {
            string key = ReinoMaintenanceSystem.BuildLotKey(ReferenceId);

            base.OnAfterDelete();

            Item item = FindWorldItem(m_AltarSerial);
            if (item != null)
                item.Delete();

            // NÃO apaga o baú
            // item = FindWorldItem(m_ChestSerial);
            // if (item != null) item.Delete();

            item = FindWorldItem(m_StatueSerial);
            if (item != null)
                item.Delete();

            TemploSystem.OnTempleMultiDeleted(key);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_AltarSerial);
            writer.Write(m_ChestSerial);
            writer.Write(m_StatueSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_AltarSerial = reader.ReadInt();
            m_ChestSerial = reader.ReadInt();
            m_StatueSerial = reader.ReadInt();

            Timer.DelayCall(TimeSpan.FromSeconds(2.0), EnsureAuxiliary);
        }
    }
}
