using Server;
using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public abstract class SculptorScaffold : BaseTool
    {
        private StatueScaffoldFacing m_Facing;
        private PlacedSculptorScaffold m_PlacedScaffold;

        [CommandProperty(AccessLevel.GameMaster)]
        public PlacedSculptorScaffold PlacedScaffold
        {
            get { return m_PlacedScaffold; }
            set { m_PlacedScaffold = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public StatueScaffoldFacing Facing
        {
            get { return m_Facing; }
            set
            {
                m_Facing = value;
                ItemID = (m_Facing == StatueScaffoldFacing.East) ? 0x0E3E : 0X0E3F;
                Hue = 0;
                InvalidateProperties();
            }
        }

        public override CraftSystem CraftSystem
        {
            get { return null; }
        }

        protected SculptorScaffold(int uses, StatueScaffoldFacing facing) : base(uses, (facing == StatueScaffoldFacing.East) ? 0x0E3E : 0X0E3F)
        {
            Name = (facing == StatueScaffoldFacing.East) ? "caixa de andaime (leste)" : "caixa de andaime (sul)";
            Weight = 20.0;
            Hue = 0;
            Movable = true;
            Facing = facing;
        }

        public SculptorScaffold(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            bool canUse = false;

            if (IsChildOf(pm.Backpack) || Parent == pm)
                canUse = true;
            else if (RootParent == null && Map == pm.Map && pm.InRange(GetWorldLocation(), 2))
                canUse = true;

            if (!canUse)
            {
                pm.SendMessage("A caixa de andaime precisa estar com você ou no chão perto de você.");
                return;
            }

            if (m_PlacedScaffold != null && !m_PlacedScaffold.Deleted)
            {
                pm.SendMessage("Escolha o andaime para recolher.");
                pm.Target = new FoldTarget(this);
            }
            else
            {
                pm.SendMessage("Escolha onde montar o andaime. Alcance de 2 tiles.");
                pm.Target = new InternalTarget(this);
            }
        }

        private class FoldTarget : Target
        {
            private readonly SculptorScaffold m_Box;

            public FoldTarget(SculptorScaffold box) : base(2, false, TargetFlags.None)
            {
                m_Box = box;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;

                if (pm == null || m_Box == null || m_Box.Deleted)
                    return;

                PlacedSculptorScaffold scaffold = targeted as PlacedSculptorScaffold;

                if (scaffold == null || scaffold.Deleted)
                {
                    pm.SendMessage("Isso não é um andaime válido.");
                    return;
                }

                if (m_Box.PlacedScaffold == null || m_Box.PlacedScaffold.Deleted)
                {
                    pm.SendMessage("Essa caixa não possui andaime montado.");
                    return;
                }

                if (scaffold != m_Box.PlacedScaffold)
                {
                    pm.SendMessage("Esse andaime não pertence a essa caixa.");
                    return;
                }

                if (!pm.InRange(scaffold.GetWorldLocation(), 2) || !pm.InRange(m_Box.GetWorldLocation(), 2))
                {
                    pm.SendMessage("Você está longe demais da caixa ou do andaime.");
                    return;
                }

                scaffold.Delete();
                m_Box.PlacedScaffold = null;

                if (m_Box.UsesRemaining > 0)
                    m_Box.UsesRemaining--;

                if (m_Box.UsesRemaining <= 0)
                {
                    m_Box.Delete();
                    pm.SendMessage("A caixa de andaime se desgastou por completo.");
                    return;
                }

                m_Box.Movable = true;
                pm.SendMessage("Você desmonta o andaime e o guarda na caixa.");
            }
        }

        private class InternalTarget : Target
        {
            private readonly SculptorScaffold m_Box;

            public InternalTarget(SculptorScaffold box) : base(2, true, TargetFlags.None)
            {
                m_Box = box;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;

                if (pm == null || m_Box == null || m_Box.Deleted)
                    return;

                IPoint3D p = targeted as IPoint3D;
                if (p == null)
                    return;

                Point3D loc = new Point3D(p);

                Point3D origin;
                Map map;

                if (m_Box.RootParent == null && m_Box.Map != null && m_Box.Map != Map.Internal)
                {
                    origin = m_Box.GetWorldLocation();
                    map = m_Box.Map;
                }
                else
                {
                    origin = pm.Location;
                    map = pm.Map;
                }

                if (map == null || map == Map.Internal)
                    return;

                if (!Utility.InRange(origin, loc, 2))
                {
                    pm.SendMessage("Esse local está longe demais.");
                    return;
                }

                if (!map.CanFit(loc.X, loc.Y, loc.Z, 16, false, false))
                {
                    pm.SendMessage("Não há espaço para montar o andaime aí.");
                    return;
                }

                PlacedSculptorScaffold scaffold = new PlacedSculptorScaffold(m_Box, m_Box.Facing);
                scaffold.MoveToWorld(loc, map);

                m_Box.PlacedScaffold = scaffold;
                m_Box.Movable = false;

                if (m_Box.RootParent != null)
                {
                    if (pm.Backpack != null && m_Box.IsChildOf(pm.Backpack))
                        pm.Backpack.DropItem(m_Box);
                    else
                        m_Box.MoveToWorld(pm.Location, pm.Map);
                }

                pm.SendMessage("Andaime montado.");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write((int)m_Facing);
            writer.Write(m_PlacedScaffold);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Facing = (StatueScaffoldFacing)reader.ReadInt();

            if (version >= 1)
                m_PlacedScaffold = reader.ReadItem() as PlacedSculptorScaffold;

            ItemID = (m_Facing == StatueScaffoldFacing.East) ? 0x12B4 : 0x12AD;
            Hue = 0;
        }
    }

    public class SculptorScaffoldEast : SculptorScaffold
    {
        [Constructable]
        public SculptorScaffoldEast() : base(20, StatueScaffoldFacing.East)
        {
        }

        public SculptorScaffoldEast(Serial serial) : base(serial)
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
    }

    public class SculptorScaffoldSouth : SculptorScaffold
    {
        [Constructable]
        public SculptorScaffoldSouth() : base(20, StatueScaffoldFacing.South)
        {
        }

        public SculptorScaffoldSouth(Serial serial) : base(serial)
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
    }
}
