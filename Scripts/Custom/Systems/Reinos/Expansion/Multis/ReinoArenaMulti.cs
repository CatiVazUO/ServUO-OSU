using System;
using Server;
using Server.Items;
using Server.Custom.Reinos;
using Server.Custom.Systems.Arena;
using Server.Custom.Systems.Arena.Items;

namespace Server.Custom.Systems.Reinos.Expansion.Multis
{
    public class ReinoArenaMulti : ReinoConstructionMulti
    {
        private int m_CityId;
        private int m_ControlSerial;

        [Constructable]
        public ReinoArenaMulti() : this(0, ArenaAuroraDefinition.BUILDING_ID, -1)
        {
        }

        public ReinoArenaMulti(int referenceId, string constructionId, int stageIndex)
            : base(0x147B, referenceId, constructionId, stageIndex)
        {
            Name = "arena do reino";
            Movable = false;
        }

        public ReinoArenaMulti(Serial serial) : base(serial)
        {
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureControl();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            EnsureControl();
        }

        public override void OnAfterDelete()
        {
            DeleteControl();
            base.OnAfterDelete();
        }

        private void EnsureControl()
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            if (lot == null)
                return;

            m_CityId = lot.CityId;
            string key = ReinoMaintenanceSystem.BuildLotKey(lot.LotId);
            ArenaDefinition def = ArenaSystem.GetDefinitionByConstructionId(ArenaAuroraDefinition.BUILDING_ID);
            if (def == null)
                return;

            Point3D controlLoc = new Point3D(lot.NorthWest.X + def.ControlOffset.X, lot.NorthWest.Y + def.ControlOffset.Y, lot.NorthWest.Z + def.ControlOffset.Z);
            ArenaControlItem item = FindItem(m_ControlSerial) as ArenaControlItem;

            if (item == null)
            {
                item = new ArenaControlItem(m_CityId, key);
                item.MoveToWorld(controlLoc, lot.Map);
                m_ControlSerial = item.Serial.Value;
            }
            else
            {
                item.CityId = m_CityId;
                item.ConstructionKey = key;

                if (item.Map != lot.Map || item.Location != controlLoc)
                    item.MoveToWorld(controlLoc, lot.Map);
            }
        }

        private void DeleteControl()
        {
            Item item = FindItem(m_ControlSerial);
            if (item != null && !item.Deleted)
                item.Delete();

            m_ControlSerial = 0;
        }

        private static Item FindItem(int serial)
        {
            return serial > 0 ? World.FindItem((Serial)serial) : null;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ControlSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ControlSerial = reader.ReadInt();
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), EnsureControl);
        }
    }
}
