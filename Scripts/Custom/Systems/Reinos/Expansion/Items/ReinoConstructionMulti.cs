using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;

namespace Server.Custom.Reinos
{
    // Implementação compatível com ServUO custom: o item principal é invisível,
    // e ele desenha os componentes do multi no mundo. Portas são convertidas em
    // BaseDoor funcionais e destrancadas, em vez de virar static duplicada.
    public class ReinoConstructionMulti : Item
    {
        private int m_MultiId;
        private int m_ReferenceId;
        private string m_ConstructionId;
        private int m_StageIndex;
        private List<Item> m_Components;
        private List<int> m_ComponentSerials;
        private bool m_ResolvingAfterLoad;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MultiId
        {
            get { return m_MultiId; }
            set
            {
                m_MultiId = value;
                RefreshComponents();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReferenceId
        {
            get { return m_ReferenceId; }
            set { m_ReferenceId = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionId
        {
            get { return m_ConstructionId; }
            set { m_ConstructionId = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StageIndex
        {
            get { return m_StageIndex; }
            set { m_StageIndex = value; }
        }

        public ReinoConstructionMulti(int multiId, int referenceId, string constructionId, int stageIndex)
            : base(1)
        {
            Visible = false;
            Movable = false;
            m_MultiId = multiId;
            m_ReferenceId = referenceId;
            m_ConstructionId = constructionId ?? String.Empty;
            m_StageIndex = stageIndex;
            m_Components = new List<Item>();
            Name = "reino construction marker";
        }

        public ReinoConstructionMulti(Serial serial) : base(serial)
        {
        }

        public override void OnMapChange()
        {
            base.OnMapChange();

            if (Deleted)
                return;

            if (m_ResolvingAfterLoad)
                return;

            if (m_Components == null)
                m_Components = new List<Item>();

            if (Map == null || Map == Map.Internal)
                return;

            if (m_Components.Count == 0)
                BuildComponents();
            else
            {
                for (int i = 0; i < m_Components.Count; i++)
                {
                    Item item = m_Components[i];

                    if (item != null && !item.Deleted)
                        item.Map = Map;
                }
            }
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);

            if (m_Components == null || Map == null || Map == Map.Internal)
                return;

            int xOffset = X - oldLocation.X;
            int yOffset = Y - oldLocation.Y;
            int zOffset = Z - oldLocation.Z;

            for (int i = 0; i < m_Components.Count; i++)
            {
                Item item = m_Components[i];

                if (item != null && !item.Deleted)
                    item.MoveToWorld(new Point3D(item.X + xOffset, item.Y + yOffset, item.Z + zOffset), Map);
            }
        }

        public override void OnAfterDelete()
        {
            ClearComponents();
            base.OnAfterDelete();
        }

        private void RefreshComponents()
        {
            if (Deleted)
                return;

            ClearComponents();

            if (Map == null || Map == Map.Internal)
                return;

            BuildComponents();
        }

        private void BuildComponents()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            if (m_Components == null)
                m_Components = new List<Item>();

            if (m_ComponentSerials == null)
                m_ComponentSerials = new List<int>();
            else
                m_ComponentSerials.Clear();

            MultiComponentList mcl = MultiData.GetComponents(m_MultiId);

            if (mcl == null || mcl.List == null || mcl.List.Length == 0)
            {
                Console.WriteLine("ReinoConstructionMulti: multi 0x{0:X} sem componentes no servidor.", m_MultiId);
                return;
            }

            List<BaseDoor> placedDoors = new List<BaseDoor>();
            HashSet<string> placedDoorOffsets = new HashSet<string>();

            for (int i = 0; i < mcl.List.Length; ++i)
            {
                MultiTileEntry entry = mcl.List[i];

                if (entry.m_ItemID <= 0)
                    continue;

                int itemID = entry.m_ItemID & TileData.MaxItemValue;
                ItemData data = TileData.ItemTable[itemID];
                bool functionalDoors = (m_StageIndex == -1);

                if (functionalDoors && (data.Flags & TileFlag.Door) != 0)
                {
                    string key = entry.m_OffsetX + ":" + entry.m_OffsetY + ":" + entry.m_OffsetZ;

                    if (placedDoorOffsets.Contains(key))
                        continue;

                    placedDoorOffsets.Add(key);
                }

                Item item = CreateComponent(entry);
                if (item == null)
                    continue;

                item.Movable = false;
                item.MoveToWorld(new Point3D(X + entry.m_OffsetX, Y + entry.m_OffsetY, Z + entry.m_OffsetZ), Map);
                m_Components.Add(item);
                m_ComponentSerials.Add(item.Serial.Value);

                BaseDoor door = item as BaseDoor;
                if (door != null)
                    placedDoors.Add(door);
            }

            LinkNearbyDoors(placedDoors);
        }

        private Item CreateComponent(MultiTileEntry entry)
        {
            int itemID = entry.m_ItemID & TileData.MaxItemValue;
            ItemData data = TileData.ItemTable[itemID];

            bool functionalDoors = (m_StageIndex == -1);

            if (functionalDoors && (data.Flags & TileFlag.Door) != 0)
            {
                BaseDoor door = CreateDoorFromMultiId(itemID);
                if (door != null)
                {
                    door.Locked = false;
                    door.KeyValue = 0;
                    return door;
                }
            }

            return new Static(itemID);
        }

        private static void LinkNearbyDoors(List<BaseDoor> doors)
        {
            if (doors == null)
                return;

            for (int i = 0; i < doors.Count; i++)
            {
                BaseDoor a = doors[i];
                if (a == null || a.Deleted)
                    continue;

                for (int j = i + 1; j < doors.Count; j++)
                {
                    BaseDoor b = doors[j];
                    if (b == null || b.Deleted)
                        continue;

                    if (a.Link == null && b.Link == null && a.InRange(b.Location, 1))
                    {
                        a.Link = b;
                        b.Link = a;
                        break;
                    }
                }
            }
        }

        private static BaseDoor CreateDoorFromMultiId(int itemID)
        {
            if (itemID >= 0x675 && itemID < 0x6F5)
            {
                int type = (itemID - 0x675) / 16;
                DoorFacing facing = (DoorFacing)(((itemID - 0x675) / 2) % 8);

                switch (type)
                {
                    case 0: return new GenericPublicDoor(facing, 0x675, 0xEC, 0xF3);
                    case 1: return new GenericPublicDoor(facing, 0x685, 0xEC, 0xF3);
                    case 2: return new GenericPublicDoor(facing, 0x695, 0xEB, 0xF2);
                    case 3: return new GenericPublicDoor(facing, 0x6A5, 0xEA, 0xF1);
                    case 4: return new GenericPublicDoor(facing, 0x6B5, 0xEA, 0xF1);
                    case 5: return new GenericPublicDoor(facing, 0x6C5, 0xEC, 0xF3);
                    case 6: return new GenericPublicDoor(facing, 0x6D5, 0xEA, 0xF1);
                    case 7: return new GenericPublicDoor(facing, 0x6E5, 0xEA, 0xF1);
                }
            }
            else if (itemID >= 0x314 && itemID < 0x364)
            {
                int type = (itemID - 0x314) / 16;
                DoorFacing facing = (DoorFacing)(((itemID - 0x314) / 2) % 8);
                return new GenericPublicDoor(facing, 0x314 + (type * 16), 0xED, 0xF4);
            }
            else if (itemID >= 0x824 && itemID < 0x834)
            {
                DoorFacing facing = (DoorFacing)(((itemID - 0x824) / 2) % 8);
                return new GenericPublicDoor(facing, 0x824, 0xEC, 0xF3);
            }
            else if (itemID >= 0x839 && itemID < 0x849)
            {
                DoorFacing facing = (DoorFacing)(((itemID - 0x839) / 2) % 8);
                return new GenericPublicDoor(facing, 0x839, 0xEB, 0xF2);
            }
            else if (itemID >= 0x84C && itemID < 0x85C)
            {
                DoorFacing facing = (DoorFacing)(((itemID - 0x84C) / 2) % 8);
                return new GenericPublicDoor(facing, 0x84C, 0xEC, 0xF3);
            }
            else if (itemID >= 0x866 && itemID < 0x876)
            {
                DoorFacing facing = (DoorFacing)(((itemID - 0x866) / 2) % 8);
                return new GenericPublicDoor(facing, 0x866, 0xEB, 0xF2);
            }
            else if (itemID >= 0xE8 && itemID < 0xF8)
            {
                DoorFacing facing = (DoorFacing)(((itemID - 0xE8) / 2) % 8);
                return new GenericPublicDoor(facing, 0xE8, 0xED, 0xF4);
            }
            else if (itemID >= 0x1FED && itemID < 0x1FFD)
            {
                DoorFacing facing = (DoorFacing)(((itemID - 0x1FED) / 2) % 8);
                return new GenericPublicDoor(facing, 0x1FED, 0xEC, 0xF3);
            }
            else if (itemID >= 0x241F && itemID < 0x2421)
            {
                return new GenericPublicDoor(DoorFacing.NorthCCW, 0x2415, -1, -1);
            }
            else if (itemID >= 0x2423 && itemID < 0x2425)
            {
                return new GenericPublicDoor(DoorFacing.WestCW, 0x2423, -1, -1);
            }
            else if (itemID >= 0x2A05 && itemID < 0x2A1D)
            {
                DoorFacing facing = (DoorFacing)((((itemID - 0x2A05) / 2) % 4) + 8);
                int sound = (itemID >= 0x2A0D && itemID < 0x2A15) ? 0x539 : -1;
                return new GenericPublicDoor(facing, 0x29F5 + (8 * ((itemID - 0x2A05) / 8)), sound, sound);
            }
            else if (itemID == 0x2D46)
            {
                return new GenericPublicDoor(DoorFacing.NorthCW, 0x2D46, 0xEA, 0xF1, false);
            }
            else if (itemID == 0x2D48 || itemID == 0x2FE2)
            {
                return new GenericPublicDoor(DoorFacing.SouthCCW, itemID, 0xEA, 0xF1, false);
            }
            else if (itemID >= 0x2D63 && itemID < 0x2D70)
            {
                DoorFacing facing = (DoorFacing)((((itemID - 0x2D63) / 2) % 4) + 8);
                return new GenericPublicDoor(facing, 0x2D63 + (8 * ((itemID - 0x2D63) / 8)), 0xEA, 0xF1, false);
            }
            else if (itemID == 0x319E)
            {
                return new GenericPublicDoor(DoorFacing.NorthCW, 0x319E, 0xEC, 0xF3);
            }
            else if (itemID == 0x31A0)
            {
                return new GenericPublicDoor(DoorFacing.SouthCCW, 0x31A0, 0xEC, 0xF3);
            }
            else if (itemID == 0x31A2)
            {
                return new GenericPublicDoor(DoorFacing.WestCW, 0x31A2, 0xEC, 0xF3);
            }
            else if (itemID == 0x31A4)
            {
                return new GenericPublicDoor(DoorFacing.EastCCW, 0x31A4, 0xEC, 0xF3);
            }

            return null;
        }

        private void ClearComponents()
        {
            if (m_Components == null)
                return;

            for (int i = 0; i < m_Components.Count; i++)
            {
                Item item = m_Components[i];

                if (item != null && !item.Deleted)
                    item.Delete();
            }

            m_Components.Clear();

            if (m_ComponentSerials != null)
                m_ComponentSerials.Clear();
        }

        private void ResolveComponentsAfterLoad()
        {
            m_ResolvingAfterLoad = false;

            if (Deleted)
                return;

            if (m_Components == null)
                m_Components = new List<Item>();
            else
                m_Components.Clear();

            if (m_ComponentSerials == null)
                m_ComponentSerials = new List<int>();

            for (int i = 0; i < m_ComponentSerials.Count; i++)
            {
                Item item = World.FindItem((Serial)m_ComponentSerials[i]);
                if (item != null && !item.Deleted)
                    m_Components.Add(item);
            }

            if (Map == null || Map == Map.Internal)
                return;

            if (m_Components.Count == 0)
                BuildComponents();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);
            writer.Write(m_MultiId);
            writer.Write(m_ReferenceId);
            writer.Write(m_ConstructionId ?? String.Empty);
            writer.Write(m_StageIndex);

            int count = m_Components != null ? m_Components.Count : 0;
            writer.Write(count);

            if (m_Components != null)
            {
                for (int i = 0; i < m_Components.Count; i++)
                {
                    Item item = m_Components[i];
                    writer.Write(item != null && !item.Deleted ? item.Serial.Value : 0);
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_MultiId = reader.ReadInt();
            m_ReferenceId = reader.ReadInt();
            m_ConstructionId = reader.ReadString();
            m_StageIndex = reader.ReadInt();

            m_Components = new List<Item>();
            m_ComponentSerials = new List<int>();
            Visible = false;
            Movable = false;

            if (version >= 1)
            {
                int count = reader.ReadInt();

                for (int i = 0; i < count; i++)
                    m_ComponentSerials.Add(reader.ReadInt());
            }

            m_ResolvingAfterLoad = true;
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerCallback(ResolveComponentsAfterLoad));
        }
    }

    public class GenericPublicDoor : BaseDoor
    {
        [Constructable]
        public GenericPublicDoor(DoorFacing facing, int baseItemID, int openedSound, int closedSound)
            : this(facing, baseItemID, openedSound, closedSound, true)
        {
        }

        [Constructable]
        public GenericPublicDoor(DoorFacing facing, int baseItemID, int openedSound, int closedSound, bool autoAdjust)
            : base(baseItemID + (autoAdjust ? (2 * (int)facing) : 0), baseItemID + 1 + (autoAdjust ? (2 * (int)facing) : 0), openedSound, closedSound, BaseDoor.GetOffset(facing))
        {
            Locked = false;
            KeyValue = 0;
        }

        public GenericPublicDoor(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            Locked = false;
            KeyValue = 0;
        }
    }
}
