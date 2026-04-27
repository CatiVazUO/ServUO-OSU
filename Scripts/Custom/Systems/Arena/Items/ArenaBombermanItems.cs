using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Arena
{
    public class ArenaBombItem : Item
    {
        private readonly Mobile m_Owner;
        private readonly ArenaGameModes.BombermanSession m_Session;

        public ArenaBombItem(Mobile owner, ArenaGameModes.BombermanSession session) : base(0xA5B4)
        {
            m_Owner = owner;
            m_Session = session;
            Movable = false;
            Timer.DelayCall(TimeSpan.FromSeconds(5.0), Explode);
        }

        public ArenaBombItem(Serial serial) : base(serial) { }

        private void Explode()
        {
            if (Deleted)
                return;

            Point3D c = Location;
            Map map = Map;

            int[] frames = new int[] { 0x370A, 0x370B, 0x370C, 0x370D, 0x370E, 0x370F, 0x3710, 0x3711 };
            for (int i = 0; i < frames.Length; i++)
                Effects.SendLocationEffect(c, map, frames[i], 10, 0, 0);

            DamageTile(new Point3D(c.X, c.Y, c.Z), map);
            DamageTile(new Point3D(c.X + 1, c.Y, c.Z), map);
            DamageTile(new Point3D(c.X - 1, c.Y, c.Z), map);
            DamageTile(new Point3D(c.X, c.Y + 1, c.Z), map);
            DamageTile(new Point3D(c.X, c.Y - 1, c.Z), map);

            if (m_Owner != null && m_Session != null)
            {
                int count;
                m_Session.ActiveBombs.TryGetValue(m_Owner.Serial.Value, out count);
                count = Math.Max(0, count - 1);
                m_Session.ActiveBombs[m_Owner.Serial.Value] = count;
            }

            Delete();
        }

        private static void DamageTile(Point3D p, Map map)
        {
            if (map == null || map == Map.Internal)
                return;

            IPooledEnumerable eable = map.GetMobilesInRange(p, 0);
            foreach (Mobile m in eable)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null)
                    continue;

                pm.Emote("*cai no chão*");
                pm.CantWalk = true;
                Timer.DelayCall(TimeSpan.FromSeconds(30.0), delegate { if (pm != null && !pm.Deleted) pm.CantWalk = false; });
            }
            eable.Free();

            IPooledEnumerable items = map.GetItemsInRange(p, 0);
            foreach (Item item in items)
            {
                if (item != null && !item.Deleted && item.ItemID == 0x0E3C)
                    item.Delete();
            }
            items.Free();
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
            Delete();
        }
    }
}
