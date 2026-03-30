using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public class PlacedSculptorScaffold : Item
    {
        private StatueScaffoldFacing m_Facing;
        private SculptorScaffold m_Box;

        [CommandProperty(AccessLevel.GameMaster)]
        public SculptorScaffold Box
        {
            get { return m_Box; }
            set { m_Box = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public StatueScaffoldFacing Facing
        {
            get { return m_Facing; }
            set
            {
                m_Facing = value;
                ItemID = (m_Facing == StatueScaffoldFacing.East) ? StatueCraftSystem.ScaffoldEast : StatueCraftSystem.ScaffoldSouth;
                InvalidateProperties();
            }
        }

        [Constructable]
        public PlacedSculptorScaffold() : this(null, StatueScaffoldFacing.South)
        {
        }

        [Constructable]
        public PlacedSculptorScaffold(SculptorScaffold box, StatueScaffoldFacing facing)
            : base(facing == StatueScaffoldFacing.East ? StatueCraftSystem.ScaffoldEast : StatueCraftSystem.ScaffoldSouth)
        {
            Name = "andaime";
            Movable = false;
            Weight = 65.0;
            m_Box = box;
            Facing = facing;
        }

        public PlacedSculptorScaffold(Serial serial) : base(serial)
        {
        }

        public override bool BlocksFit
        {
            get { return true; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (Map == null || Map == Map.Internal || Deleted)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais do andaime.");
                return;
            }

            Point3D loc = GetWorldLocation();
            pm.Location = new Point3D(loc.X, loc.Y, loc.Z + 6);
            pm.Direction = (m_Facing == StatueScaffoldFacing.East) ? Direction.East : Direction.South;
            pm.SendMessage("Você sobe no andaime.");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write((int)m_Facing);
            writer.Write(m_Box);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Facing = (StatueScaffoldFacing)reader.ReadInt();

            if (version >= 1)
                m_Box = reader.ReadItem() as SculptorScaffold;

            Facing = m_Facing;
            Movable = false;
        }
    }
}
