using System;
using System.Collections.Generic;
using Server.Items;
using Server.Custom.Systems.Espetaculos;
using Server.Custom.Systems.Espetaculos.Items;
using Server.Custom.Reinos;

namespace Server.Custom.Systems.Reinos.Expansion.Multis
{
    public abstract class ReinoEspetaculoMultiBase : ReinoConstructionMulti
    {
        private int m_CityId;
        private int m_ControlSerial;
        private int[] m_StageLightSerials;
        private int[] m_SetPieceSerials;
        private int[] m_DoorSerials;

        protected ReinoEspetaculoMultiBase(int multiId, int referenceId, string constructionId, int stageIndex)
            : base(multiId, referenceId, constructionId, stageIndex)
        {
            Movable = false;
        }

        public ReinoEspetaculoMultiBase(Serial serial) : base(serial)
        {
        }

        protected abstract EspetaculoVenueDefinition GetVenueDefinition();

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureAuxiliary();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            EnsureAuxiliary();
        }

        public override void OnAfterDelete()
        {
            string key = GetConstructionKey();
            DeleteAuxiliary();
            base.OnAfterDelete();
            EspetaculoSystem.OnVenueMultiDeleted(key);
        }

        private void EnsureAuxiliary()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            if (lot == null || lot.Map == null || lot.Map == Map.Internal)
                return;

            EspetaculoVenueDefinition venue = GetVenueDefinition();
            if (venue == null)
                return;

            m_CityId = lot.CityId;
            string key = GetConstructionKey();
            EspetaculoVenueState state = EspetaculoSystem.EnsureState(key, m_CityId, venue.VenueType);

            state.StageLightSerials.Clear();
            state.SetPieceSerials.Clear();
            state.DoorSerials.Clear();

            EnsureControlItem(lot, venue, state, key);
            EnsureStageLights(lot, venue, state, key);
            EnsureSetPieces(lot, venue, state, key);
            EnsureDoors(lot, venue, state, key);
        }

        private void EnsureControlItem(ReinoLotDefinition lot, EspetaculoVenueDefinition venue, EspetaculoVenueState state, string key)
        {
            Point3D loc = AddOffset(lot.NorthWest, venue.ControlItemOffset);
            EspetaculoControlMarker item = FindItem(m_ControlSerial) as EspetaculoControlMarker;

            if (item == null)
            {
                item = new EspetaculoControlMarker(m_CityId, key, venue.VenueType);
                item.MoveToWorld(loc, lot.Map);
                m_ControlSerial = item.Serial.Value;
            }
            else
            {
                item.CityId = m_CityId;
                item.ConstructionKey = key;
                item.VenueType = venue.VenueType;
                MoveIfNeeded(item, loc, lot.Map);
            }

            EspetaculoSystem.RegisterControlItem(key, m_CityId, venue.VenueType, item.Serial.Value);
        }

        private void EnsureStageLights(ReinoLotDefinition lot, EspetaculoVenueDefinition venue, EspetaculoVenueState state, string key)
        {
            int baseCount = venue.StageLights != null ? venue.StageLights.Length : 0;
            EspetaculoLightColor[] palette = new EspetaculoLightColor[]
            {
        EspetaculoLightColor.Blue,
        EspetaculoLightColor.Red,
        EspetaculoLightColor.Green,
        EspetaculoLightColor.Purple,
        EspetaculoLightColor.White,
        EspetaculoLightColor.Yellow
            };

            int count = baseCount * palette.Length;
            EnsureArrayLength(ref m_StageLightSerials, count);

            for (int i = 0; i < baseCount; i++)
            {
                EspetaculoStageLightDefinition def = venue.StageLights[i];
                Point3D loc = AddOffset(lot.NorthWest, def.Offset);

                for (int c = 0; c < palette.Length; c++)
                {
                    int index = i * palette.Length + c;
                    EspetaculoLightColor color = palette[c];
                    EspetaculoStageLight item = FindItem(m_StageLightSerials[index]) as EspetaculoStageLight;

                    if (item == null || item.LightColor != color)
                    {
                        if (item != null)
                            item.Delete();

                        item = new EspetaculoStageLight(m_CityId, key, color, true);
                        item.MoveToWorld(loc, lot.Map);
                        m_StageLightSerials[index] = item.Serial.Value;
                    }
                    else
                    {
                        MoveIfNeeded(item, loc, lot.Map);
                        item.HiddenEmitter = true;
                    }

                    item.HiddenEmitter = true;
                    item.SetEnabled(state.StageLightsOn, state.SelectedLightColor);
                    EspetaculoSystem.RegisterStageLight(key, m_CityId, venue.VenueType, item.Serial.Value);
                }
            }
        }

        private void EnsureSetPieces(ReinoLotDefinition lot, EspetaculoVenueDefinition venue, EspetaculoVenueState state, string key)
        {
            int count = venue.SetPieces != null ? venue.SetPieces.Length : 0;
            EnsureArrayLength(ref m_SetPieceSerials, count);

            for (int i = 0; i < count; i++)
            {
                EspetaculoSetPieceDefinition def = venue.SetPieces[i];
                bool open = EspetaculoSystem.GetSetPieceOpenState(state, venue, def.Id);
                Point3D loc = AddOffset(lot.NorthWest, open ? def.OpenOffset : def.ClosedOffset);
                EspetaculoSetPieceItem item = FindItem(m_SetPieceSerials[i]) as EspetaculoSetPieceItem;

                if (item == null || item.ItemID != def.ItemId || !string.Equals(item.SetPieceId, def.Id, StringComparison.OrdinalIgnoreCase))
                {
                    if (item != null)
                        item.Delete();

                    item = new EspetaculoSetPieceItem(def.Id, m_CityId, key, def.ItemId, def.ClosedOffset, def.OpenOffset);
                    item.Name = def.Name;
                    item.Hue = def.Hue;
                    item.MoveToWorld(loc, lot.Map);
                    m_SetPieceSerials[i] = item.Serial.Value;
                }
                else
                {
                    item.Name = def.Name;
                    item.Hue = def.Hue;
                    MoveIfNeeded(item, loc, lot.Map);
                }

                item.SetOpen(open);
                EspetaculoSystem.RegisterSetPiece(key, m_CityId, venue.VenueType, item.Serial.Value);
            }
        }

        private void EnsureDoors(ReinoLotDefinition lot, EspetaculoVenueDefinition venue, EspetaculoVenueState state, string key)
        {
            int count = venue.Doors != null ? venue.Doors.Length : 0;
            EnsureArrayLength(ref m_DoorSerials, count);

            Dictionary<string, EspetaculoVenueDoor> byOffset = new Dictionary<string, EspetaculoVenueDoor>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < count; i++)
            {
                EspetaculoDoorDefinition def = venue.Doors[i];
                Point3D loc = AddOffset(lot.NorthWest, def.Offset);
                EspetaculoVenueDoor item = FindItem(m_DoorSerials[i]) as EspetaculoVenueDoor;

                if (item == null || item.ClosedID != def.ClosedId || item.OpenedID != def.OpenedId)
                {
                    if (item != null)
                        item.Delete();

                    item = new EspetaculoVenueDoor(def.ClosedId, def.OpenedId, def.OpenedSound, def.ClosedSound, def.LinkOffset, m_CityId, key);
                    item.Name = def.Name;
                    item.MoveToWorld(loc, lot.Map);
                    m_DoorSerials[i] = item.Serial.Value;
                }
                else
                {
                    item.Name = def.Name;
                    MoveIfNeeded(item, loc, lot.Map);
                }

                item.SyncLockedState(EspetaculoSystem.GetActiveReservation(key) != null);
                EspetaculoSystem.RegisterDoor(key, m_CityId, venue.VenueType, item.Serial.Value);
                byOffset[GetOffsetKey(def.Offset)] = item;
            }

            for (int i = 0; i < count; i++)
            {
                EspetaculoDoorDefinition def = venue.Doors[i];
                string selfKey = GetOffsetKey(def.Offset);
                string linkKey = GetOffsetKey(new Point3D(def.Offset.X + def.LinkOffset.X, def.Offset.Y + def.LinkOffset.Y, def.Offset.Z + def.LinkOffset.Z));

                EspetaculoVenueDoor self;
                EspetaculoVenueDoor link;
                if (byOffset.TryGetValue(selfKey, out self) && byOffset.TryGetValue(linkKey, out link) && self != null && link != null)
                    self.Link = link;
            }
        }

        private void DeleteAuxiliary()
        {
            DeleteItem(m_ControlSerial);
            DeleteItems(m_StageLightSerials);
            DeleteItems(m_SetPieceSerials);
            DeleteItems(m_DoorSerials);
        }

        private string GetConstructionKey()
        {
            return ReinoMaintenanceSystem.BuildLotKey(ReferenceId);
        }

        private static void DeleteItem(int serial)
        {
            Item item = FindItem(serial);
            if (item != null && !item.Deleted)
                item.Delete();
        }

        private static void DeleteItems(int[] serials)
        {
            if (serials == null)
                return;

            for (int i = 0; i < serials.Length; i++)
                DeleteItem(serials[i]);
        }

        private static Item FindItem(int serial)
        {
            return serial > 0 ? World.FindItem((Serial)serial) : null;
        }

        private static void EnsureArrayLength(ref int[] array, int count)
        {
            if (count <= 0)
            {
                DeleteItems(array);
                array = new int[0];
                return;
            }

            if (array == null)
            {
                array = new int[count];
                return;
            }

            if (array.Length == count)
                return;

            if (array.Length > count)
            {
                for (int i = count; i < array.Length; i++)
                    DeleteItem(array[i]);
            }

            int[] resized = new int[count];
            for (int i = 0; i < resized.Length && i < array.Length; i++)
                resized[i] = array[i];

            array = resized;
        }

        private static void MoveIfNeeded(Item item, Point3D loc, Map map)
        {
            if (item == null || item.Deleted)
                return;

            if (item.Map != map || item.Location != loc)
                item.MoveToWorld(loc, map);
        }

        private static Point3D AddOffset(Point3D origin, Point3D offset)
        {
            return new Point3D(origin.X + offset.X, origin.Y + offset.Y, origin.Z + offset.Z);
        }

        private static string GetOffsetKey(Point3D offset)
        {
            return offset.X + ":" + offset.Y + ":" + offset.Z;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ControlSerial);
            WriteIntArray(writer, m_StageLightSerials);
            WriteIntArray(writer, m_SetPieceSerials);
            WriteIntArray(writer, m_DoorSerials);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ControlSerial = reader.ReadInt();
            m_StageLightSerials = ReadIntArray(reader);
            m_SetPieceSerials = ReadIntArray(reader);
            m_DoorSerials = ReadIntArray(reader);
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), EnsureAuxiliary);
        }

        private static void WriteIntArray(GenericWriter writer, int[] values)
        {
            int count = values != null ? values.Length : 0;
            writer.Write(count);

            for (int i = 0; i < count; i++)
                writer.Write(values[i]);
        }

        private static int[] ReadIntArray(GenericReader reader)
        {
            int count = reader.ReadInt();
            int[] values = new int[count];

            for (int i = 0; i < count; i++)
                values[i] = reader.ReadInt();

            return values;
        }
    }

    public class ReinoTeatroMulti : ReinoEspetaculoMultiBase
    {
        [Constructable]
        public ReinoTeatroMulti() : this(0, TeatroAuroraDefinition.BUILDING_ID, -1)
        {
        }

        public ReinoTeatroMulti(int referenceId, string constructionId, int stageIndex)
            : base(0x147B, referenceId, constructionId, stageIndex)
        {
            Name = "teatro do reino";
        }

        public ReinoTeatroMulti(Serial serial) : base(serial)
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
            int version = reader.ReadInt();
        }

        protected override EspetaculoVenueDefinition GetVenueDefinition()
        {
            return TeatroAuroraDefinition.CreateVenue();
        }
    }

    public class ReinoCircoMulti : ReinoEspetaculoMultiBase
    {
        [Constructable]
        public ReinoCircoMulti() : this(0, CircoAuroraDefinition.BUILDING_ID, -1)
        {
        }

        public ReinoCircoMulti(int referenceId, string constructionId, int stageIndex)
            : base(0x147B, referenceId, constructionId, stageIndex)
        {
            Name = "circo do reino";
        }

        public ReinoCircoMulti(Serial serial) : base(serial)
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
            int version = reader.ReadInt();
        }

        protected override EspetaculoVenueDefinition GetVenueDefinition()
        {
            return CircoAuroraDefinition.CreateVenue();
        }
    }
}
