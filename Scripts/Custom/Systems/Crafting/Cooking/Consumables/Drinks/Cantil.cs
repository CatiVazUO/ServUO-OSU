using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Custom.Systems.Needs;
using Server.Custom.Systems.Needs.Gumps;
using Server.Custom.Systems.Health;

namespace Server.Custom.Drinks
{
    public class Cantil : BaseBeverage
    {
        public override int MaxQuantity
        {
            get { return 5; }
        }

        public override int ComputeItemID()
        {
            return 0x13F3; // gráfico do flask
        }

        [Constructable]
        public Cantil()
        {
            Name = "cantil";
            Weight = 1.0;
            Hue = 0;
        }

        public Cantil(Serial serial) : base(serial)
        {
        }


        public override void Fill_OnTarget(Mobile from, object targ)
        {
            if (TryFillFromWetTarget(from, targ))
                return;

            base.Fill_OnTarget(from, targ);
        }

        private bool TryFillFromWetTarget(Mobile from, object targ)
        {
            if (from == null || from.Map == null || from.Map == Map.Internal)
                return false;

            Point3D p;
            Map map;

            if (!TryResolvePoint(from, targ, out p, out map))
                return false;

            if (!IsWetTile(map, p.X, p.Y))
                return false;

            Content = BeverageType.Water;
            Quantity = MaxQuantity;

            Item item = targ as Item;
            if (item != null && OSUHealthSystem.IsContaminated(item))
                OSUHealthSystem.CopyContamination(item, this);
            else
            {
                OSUDiseaseType disease;
                if (!OSUHealthSystem.TryGetAreaDiseaseAt(map, p.X, p.Y, out disease))
                    OSUHealthSystem.ClearContaminatedItem(this);
                else
                    OSUHealthSystem.ContaminateItem(this, disease, TimeSpan.FromHours(12), "água contaminada");
            }

            from.PlaySound(0x240);
            from.SendMessage("Você enche o cantil com água.");
            return true;
        }

        private static bool TryResolvePoint(Mobile from, object targ, out Point3D p, out Map map)
        {
            p = Point3D.Zero;
            map = from.Map;

            Item item = targ as Item;
            if (item != null)
            {
                p = item.GetWorldLocation();
                map = item.Map;
                return map != null && map != Map.Internal;
            }

            IPoint3D ip = targ as IPoint3D;
            if (ip != null)
            {
                p = new Point3D(ip.X, ip.Y, ip.Z);
                return map != null && map != Map.Internal;
            }

            return false;
        }

        private static bool IsWetTile(Map map, int x, int y)
        {
            LandTile land = map.Tiles.GetLandTile(x, y);
            if ((TileData.LandTable[land.ID & 0x3FFF].Flags & TileFlag.Wet) != 0)
                return true;

            StaticTile[] statics = map.Tiles.GetStaticTiles(x, y, true);
            for (int i = 0; i < statics.Length; i++)
            {
                int itemId = statics[i].ID & 0x3FFF;
                if ((TileData.ItemTable[itemId].Flags & TileFlag.Wet) != 0)
                    return true;
            }

            return false;
        }

        public override void Pour_OnTarget(Mobile from, object targ)
        {
            if (targ == from)
            {
                PlayerMobile pm = from as PlayerMobile;

                if (pm == null)
                    return;

                if (IsEmpty)
                {
                    pm.SendMessage("O cantil está vazio.");
                    return;
                }

                // 4 por gole = 20 no total quando o flask está cheio
                if (!OSUNeedsSystem.TryAddThirst(pm, 20))
                {
                    pm.SendMessage("Você está satisfeito demais para beber agora.");
                    return;
                }

                if (OSUHealthSystem.IsContaminated(this))
                    OSUHealthSystem.TryExposeFromItem(pm, this);

                pm.PlaySound(Utility.RandomList(0x30, 0x31, 0x2D6));
                pm.SendMessage("Você toma um gole de água.");

                Quantity -= 1;

                OSUNeedsGump.TryRefresh(pm);
                return;
            }

            from.PlaySound(0x025);
            base.Pour_OnTarget(from, targ);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
