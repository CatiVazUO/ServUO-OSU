using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Custom.Systems.Health;

namespace Server.Custom.Reinos
{
    public class ReinoHospitalMulti : ReinoPlacedMultiBase
    {
        private int[] _auxSerials;

        [Constructable]
        public ReinoHospitalMulti() : this(0, HospitalAuroraDefinition.BUILDING_ID)
        {
        }

        public ReinoHospitalMulti(int referenceId, string constructionId) : base(0x147B, referenceId, constructionId, -1)
        {
            Movable = false;
            Name = "hospital do reino";
            EnsureAux();
        }

        public ReinoHospitalMulti(Serial serial) : base(serial)
        {
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);

            if (_auxSerials == null)
                return;

            int dx = X - oldLocation.X;
            int dy = Y - oldLocation.Y;
            int dz = Z - oldLocation.Z;

            for (int i = 0; i < _auxSerials.Length; i++)
            {
                Item item = World.FindItem(_auxSerials[i]);
                if (item != null && !item.Deleted)
                    item.MoveToWorld(new Point3D(item.X + dx, item.Y + dy, item.Z + dz), Map);
            }
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureAux();
        }

        public override void OnAfterDelete()
        {
            DeleteAux();
            base.OnAfterDelete();
        }

        private void EnsureAux()
        {
            if (Map == null || Map == Map.Internal)
                return;

            int cityId = 0;
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            if (lot != null)
                cityId = lot.CityId;

            string key = ConstructionId ?? HospitalAuroraDefinition.BUILDING_ID;
            List<Item> items = new List<Item>();

            Point3D[] tubOffsets = HospitalAuroraDefinition.GetMedicationTubOffsets();
            for (int i = 0; i < tubOffsets.Length; i++)
            {
                OSUMedicatedBandageType type = OSUMedicatedBandageType.HealingBonus;
                int hue = 1150;

                if (i == 1)
                {
                    type = OSUMedicatedBandageType.SpeedBonus;
                    hue = 1266;
                }
                else if (i == 2)
                {
                    type = OSUMedicatedBandageType.Antiseptic;
                    hue = 2117;
                }

                items.Add(EnsureItem(i, new OSUMedicationTub(cityId, key, type, hue, 10), tubOffsets[i]));
            }

            int index = tubOffsets.Length;
            Point3D[] stretcherOffsets = HospitalAuroraDefinition.GetHospitalStretcherOffsets();
            for (int i = 0; i < stretcherOffsets.Length; i++, index++)
                items.Add(EnsureItem(index, new OSUHospitalRecoveryStretcher(cityId, key), stretcherOffsets[i]));

            Point3D[] surgeryStretchers = HospitalAuroraDefinition.GetSurgeryStretcherOffsets();
            for (int i = 0; i < surgeryStretchers.Length; i++, index++)
                items.Add(EnsureItem(index, new OSUSurgeryStretcher(cityId, key), surgeryStretchers[i]));

            Point3D[] tableOrigins = HospitalAuroraDefinition.GetSurgeryTableOrigins();
            for (int i = 0; i < tableOrigins.Length; i++)
            {
                Point3D o = tableOrigins[i];
                items.Add(EnsureItem(index++, new OSUMesaCirurgicaNorte(cityId, key), o));
                items.Add(EnsureItem(index++, new OSUMesaCirurgicaCentro(cityId, key), new Point3D(o.X, o.Y + 1, o.Z)));
                items.Add(EnsureItem(index++, new OSUMesaCirurgicaCentro(cityId, key), new Point3D(o.X, o.Y + 2, o.Z)));
                items.Add(EnsureItem(index++, new OSUMesaCirurgicaSul(cityId, key), new Point3D(o.X, o.Y + 3, o.Z)));

                items.Add(EnsureItem(index++, new OSUBrasaCauterizadora(cityId, key), new Point3D(o.X, o.Y + 3, o.Z + 4)));
                items.Add(EnsureItem(index++, new OSUTesouraCirurgica(cityId, key), new Point3D(o.X, o.Y + 3, o.Z + 3)));
                items.Add(EnsureItem(index++, new OSUVelaCauterizadora(cityId, key), new Point3D(o.X, o.Y + 3, o.Z + 10)));

                items.Add(EnsureItem(index++, new OSUAdagaDeSangria(cityId, key), new Point3D(o.X, o.Y + 2, o.Z + 4)));
                items.Add(EnsureItem(index++, new OSUSanguessugaCirurgica(cityId, key), new Point3D(o.X, o.Y + 2, o.Z + 6)));
                items.Add(EnsureItem(index++, new OSUAguaEsterilCirurgica(cityId, key), new Point3D(o.X, o.Y + 2, o.Z + 10)));

                items.Add(EnsureItem(index++, new OSULinhaDeSutura(cityId, key), new Point3D(o.X, o.Y + 1, o.Z + 2)));
                items.Add(EnsureItem(index++, new OSUGazesCirurgicas(cityId, key), new Point3D(o.X, o.Y + 1, o.Z + 6)));
                items.Add(EnsureItem(index++, new OSUCuteloCirurgico(cityId, key), new Point3D(o.X, o.Y + 1, o.Z + 10)));

                items.Add(EnsureItem(index++, new OSUAnestesicoCirurgico(cityId, key), new Point3D(o.X, o.Y, o.Z + 2)));
                items.Add(EnsureItem(index++, new OSUAlcoolCirurgico(cityId, key), new Point3D(o.X, o.Y, o.Z + 8)));
            }

            if (_auxSerials != null && _auxSerials.Length > items.Count)
            {
                for (int i = items.Count; i < _auxSerials.Length; i++)
                {
                    Item extra = World.FindItem(_auxSerials[i]);
                    if (extra != null && !extra.Deleted)
                        extra.Delete();
                }
            }

            _auxSerials = new int[items.Count];
            for (int i = 0; i < items.Count; i++)
                _auxSerials[i] = items[i] != null ? items[i].Serial.Value : 0;
        }

        private Item EnsureItem(int index, Item newItem, Point3D offset)
        {
            if (_auxSerials != null && index < _auxSerials.Length)
            {
                Item existing = World.FindItem(_auxSerials[index]);
                if (existing != null && !existing.Deleted)
                {
                    if (existing.GetType() == newItem.GetType() && existing.ItemID == newItem.ItemID)
                        return existing;

                    existing.Delete();
                }
            }

            newItem.MoveToWorld(new Point3D(X + offset.X, Y + offset.Y, Z + offset.Z), Map);
            return newItem;
        }

        private void DeleteAux()
        {
            if (_auxSerials == null)
                return;

            for (int i = 0; i < _auxSerials.Length; i++)
            {
                Item item = World.FindItem(_auxSerials[i]);
                if (item != null && !item.Deleted)
                    item.Delete();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            int count = _auxSerials != null ? _auxSerials.Length : 0;
            writer.Write(count);
            for (int i = 0; i < count; i++)
                writer.Write(_auxSerials[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            int count = reader.ReadInt();
            _auxSerials = new int[count];
            for (int i = 0; i < count; i++)
                _auxSerials[i] = reader.ReadInt();
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), EnsureAux);
        }
    }
}
