using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Arena
{
    public class ArenaGateBlockItem : Item
    {
        private bool m_IsEntryDoor;

        [Constructable]
        public ArenaGateBlockItem(bool entryDoor) : base(0x071E)
        {
            m_IsEntryDoor = entryDoor;
            Movable = false;
            Name = m_IsEntryDoor ? "Porta de Entrada da Arena" : "Portão da Arena";
            Hue = m_IsEntryDoor ? 1150 : 2406;
        }

        public ArenaGateBlockItem(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_IsEntryDoor);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_IsEntryDoor = reader.ReadBool();
            Movable = false;
            Name = m_IsEntryDoor ? "Porta de Entrada da Arena" : "Portão da Arena";
            Hue = m_IsEntryDoor ? 1150 : 2406;
        }
    }

    public class ArenaWallItem : Item
    {
        [Constructable]
        public ArenaWallItem() : base(0x071E) { Movable = false; Name = "Parede da Arena"; }
        public ArenaWallItem(Serial s) : base(s) { }
        public override bool BlocksFit { get { return true; } }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); Movable = false; Name = "Parede da Arena"; }
    }

    public class ArenaBombermanTeamVest : Item
    {
        private bool m_Red;

        public ArenaBombermanTeamVest(bool red) : base(0x1F03)
        {
            m_Red = red;
            Layer = Layer.OuterTorso;
            Name = red ? "Colete do Time Vermelho" : "Colete do Time Azul";
            Hue = red ? 33 : 1152;
            Movable = false;
        }

        public ArenaBombermanTeamVest(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_Red);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Red = reader.ReadBool();
            Layer = Layer.OuterTorso;
            Name = m_Red ? "Colete do Time Vermelho" : "Colete do Time Azul";
            Hue = m_Red ? 33 : 1152;
            Movable = false;
        }
    }

    public class ArenaCrateItem : Item
    {
        [Constructable]
        public ArenaCrateItem() : base(0x0E3C) { Movable = false; }
        public ArenaCrateItem(Serial s) : base(s) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); Movable = false; }
    }

    public abstract class ArenaBonusItemBase : Item
    {
        protected ArenaBonusItemBase(int itemId) : base(itemId) { Movable = false; }
        public ArenaBonusItemBase(Serial s) : base(s) { }
        protected abstract int BonusType { get; }

        public override bool OnMoveOver(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm != null)
            {
                string key;
                int city;
                ArenaDefinition def;
                Server.Custom.Reinos.ReinoLotDefinition lot;
                if (ArenaSystem.TryResolveArenaAt(pm.Location, pm.Map, out key, out city, out def, out lot))
                {
                    ArenaGameModes.BombermanSession s = ArenaGameModes.GetOrCreateBomberman(key);
                    s.ApplyBonus(pm, BonusType);
                }

                Delete();
            }

            return true;
        }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); Movable = false; }
    }

    public class ArenaMoveBonusItem : ArenaBonusItemBase
    {
        [Constructable]
        public ArenaMoveBonusItem() : base(0x170B) { }
        public ArenaMoveBonusItem(Serial s) : base(s) { }
        protected override int BonusType { get { return 1; } }
    }

    public class ArenaMultiBombBonusItem : ArenaBonusItemBase
    {
        [Constructable]
        public ArenaMultiBombBonusItem() : base(0x0E74) { }
        public ArenaMultiBombBonusItem(Serial s) : base(s) { }
        protected override int BonusType { get { return 2; } }
    }

    public class ArenaRangeBonusItem : ArenaBonusItemBase
    {
        [Constructable]
        public ArenaRangeBonusItem() : base(0x36BE) { }
        public ArenaRangeBonusItem(Serial s) : base(s) { }
        protected override int BonusType { get { return 3; } }
    }

    public class ArenaBombItem : Item
    {
        private Mobile m_Owner;
        private ArenaGameModes.BombermanSession m_Session;
        private int m_Range;
        private string m_ConstructionKey;

        public ArenaBombItem(Mobile owner, ArenaGameModes.BombermanSession session, int range) : base(0xA5B4)
        {
            m_Owner = owner;
            m_Session = session;
            m_Range = Math.Max(1, Math.Min(4, range));
            m_ConstructionKey = session != null ? session.Key : String.Empty;
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

            DamageTile(c, map, m_Session);
            for (int i = 1; i <= m_Range; i++)
            {
                DamageTile(new Point3D(c.X + i, c.Y, c.Z), map, m_Session);
                DamageTile(new Point3D(c.X - i, c.Y, c.Z), map, m_Session);
                DamageTile(new Point3D(c.X, c.Y + i, c.Z), map, m_Session);
                DamageTile(new Point3D(c.X, c.Y - i, c.Z), map, m_Session);
            }

            if (m_Owner != null && m_Session != null)
            {
                int count;
                m_Session.ActiveBombs.TryGetValue(m_Owner.Serial.Value, out count);
                count = Math.Max(0, count - 1);
                m_Session.ActiveBombs[m_Owner.Serial.Value] = count;
            }

            Delete();
        }

        private static void DamageTile(Point3D p, Map map, ArenaGameModes.BombermanSession session)
        {
            if (map == null || map == Map.Internal)
                return;

            int[] frames = new int[] { 0x370A, 0x370B, 0x370C, 0x370D, 0x370E, 0x370F, 0x3710, 0x3711 };
            for (int i = 0; i < frames.Length; i++)
                Effects.SendLocationEffect(p, map, frames[i], 10, 0, 0);

            IPooledEnumerable eable = map.GetMobilesInRange(p, 0);
            foreach (Mobile m in eable)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || session == null || !session.IsParticipant(pm))
                    continue;

                session.NotifyPlayerHit(pm);
            }
            eable.Free();

            IPooledEnumerable items = map.GetItemsInRange(p, 0);
            foreach (Item item in items)
            {
                if (item == null || item.Deleted)
                    continue;

                if (item.ItemID == 0x0E3C)
                    item.Delete();
            }
            items.Free();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_Owner);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write(m_Range);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Owner = reader.ReadMobile();
            m_ConstructionKey = reader.ReadString();
            m_Range = reader.ReadInt();

            if (!String.IsNullOrWhiteSpace(m_ConstructionKey))
                m_Session = ArenaGameModes.GetOrCreateBomberman(m_ConstructionKey);

            Delete();
        }
    }
}
