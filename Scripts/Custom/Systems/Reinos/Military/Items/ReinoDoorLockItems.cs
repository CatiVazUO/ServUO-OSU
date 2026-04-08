using Server;
using Server.Targeting;
using Server.Network;
using Server.Custom.Reinos;

namespace Server.Items
{
    public abstract class ReinoDoorLockKit : Item
    {
        private ReinoDoorLockMaterial m_Material;
        private int m_MaxUses;
        private int m_PickPenalty;

        public ReinoDoorLockMaterial Material { get { return m_Material; } }
        public int MaxUses { get { return m_MaxUses; } }
        public int PickPenalty { get { return m_PickPenalty; } }

        public override double DefaultWeight { get { return 1.0; } }

        public ReinoDoorLockKit(int itemID, ReinoDoorLockMaterial material)
            : base(itemID)
        {
            m_Material = material;
            m_MaxUses = ReinoDoorLockSystem.GetDefaultUses(material);
            m_PickPenalty = ReinoDoorLockSystem.GetPickPenalty(material);
            Name = "fechadura de " + ReinoDoorLockSystem.GetMaterialLabel(material).ToLowerInvariant();
            Hue = GetHue(material);
        }

        public ReinoDoorLockKit(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            from.SendMessage("Escolha a porta em que deseja instalar a fechadura.");
            from.Target = new InternalTarget(this);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Material: {0}", ReinoDoorLockSystem.GetMaterialLabel(m_Material));
            list.Add("Usos máximos: {0}", m_MaxUses);
            list.Add("Dificuldade extra: -{0}%", m_PickPenalty);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write((int)m_Material);
            writer.Write(m_MaxUses);
            writer.Write(m_PickPenalty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 1)
            {
                m_Material = (ReinoDoorLockMaterial)reader.ReadInt();
                m_MaxUses = reader.ReadInt();
                m_PickPenalty = reader.ReadInt();
                Name = "fechadura de " + ReinoDoorLockSystem.GetMaterialLabel(m_Material).ToLowerInvariant();
                Hue = GetHue(m_Material);
            }
        }

        private static int GetHue(ReinoDoorLockMaterial material)
        {
            switch (material)
            {
                case ReinoDoorLockMaterial.DullCopper: return 0x973;
                case ReinoDoorLockMaterial.Copper: return 0x96D;
                case ReinoDoorLockMaterial.Bronze: return 0x972;
                case ReinoDoorLockMaterial.Gold: return 0x8A5;
                case ReinoDoorLockMaterial.Agapite: return 0x979;
                case ReinoDoorLockMaterial.Verite: return 0x89F;
                case ReinoDoorLockMaterial.Valorite: return 0x8AB;
                default: return 0;
            }
        }

        private class InternalTarget : Target
        {
            private readonly ReinoDoorLockKit m_Kit;

            public InternalTarget(ReinoDoorLockKit kit)
                : base(2, false, TargetFlags.None)
            {
                m_Kit = kit;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseDoor door = targeted as BaseDoor;
                if (door == null)
                {
                    from.SendMessage("Isso não é uma porta válida.");
                    return;
                }

                string message;
                if (ReinoDoorLockSystem.TryInstallLock(from, door, m_Kit, out message))
                {
                    from.SendMessage(message);
                    m_Kit.Consume();
                }
                else
                {
                    from.SendMessage(message);
                }
            }
        }
    }

    public class IronDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public IronDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Iron) { }
        public IronDoorLockKit(Serial serial) : base(serial) { }
    }

    public class DullCopperDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public DullCopperDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.DullCopper) { }
        public DullCopperDoorLockKit(Serial serial) : base(serial) { }
    }

    public class CopperDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public CopperDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Copper) { }
        public CopperDoorLockKit(Serial serial) : base(serial) { }
    }

    public class BronzeDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public BronzeDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Bronze) { }
        public BronzeDoorLockKit(Serial serial) : base(serial) { }
    }

    public class GoldDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public GoldDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Gold) { }
        public GoldDoorLockKit(Serial serial) : base(serial) { }
    }

    public class AgapiteDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public AgapiteDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Agapite) { }
        public AgapiteDoorLockKit(Serial serial) : base(serial) { }
    }

    public class VeriteDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public VeriteDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Verite) { }
        public VeriteDoorLockKit(Serial serial) : base(serial) { }
    }

    public class ValoriteDoorLockKit : ReinoDoorLockKit
    {
        [Constructable]
        public ValoriteDoorLockKit() : base(0x14F0, ReinoDoorLockMaterial.Valorite) { }
        public ValoriteDoorLockKit(Serial serial) : base(serial) { }
    }
}
