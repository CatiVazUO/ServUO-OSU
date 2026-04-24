using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Drinks;

namespace Server.Custom.Drinks
{
    public enum OSUMilkKind
    {
        None = 0,
        Cow = 1,
        Goat = 2
    }
}

namespace Server.Items
{
    public abstract class OSUBaseMilkPitcher : Item
    {
        private int m_Quantity;
        private DateTime m_SpoilsAtUtc;

        public abstract string MilkDisplayName { get; }
        public abstract OSUMilkKind MilkKind { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Quantity
        {
            get { return m_Quantity; }
            set
            {
                m_Quantity = Math.Max(0, Math.Min(5, value));
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SpoilsAtUtc
        {
            get { return m_SpoilsAtUtc; }
            set
            {
                m_SpoilsAtUtc = value;
                InvalidateProperties();
            }
        }

        public bool IsEmpty { get { return m_Quantity <= 0; } }
        public int MaxQuantity { get { return 5; } }

        public bool IsSpoiled
        {
            get { return !IsEmpty && m_SpoilsAtUtc != DateTime.MinValue && DateTime.UtcNow >= m_SpoilsAtUtc; }
        }

        protected OSUBaseMilkPitcher(int itemID) : base(itemID)
        {
            Weight = 2.0;
            Name = "pitcher de " + MilkDisplayName;
        }

        public OSUBaseMilkPitcher(Serial serial) : base(serial)
        {
        }

        public void EmptyOut()
        {
            m_Quantity = 0;
            m_SpoilsAtUtc = DateTime.MinValue;
            InvalidateProperties();
        }

        public int TryAddUnits(int amount, DateTime spoilsAtUtc)
        {
            if (amount <= 0)
                return 0;

            if (IsSpoiled)
                EmptyOut();

            int room = MaxQuantity - m_Quantity;
            int added = Math.Min(room, amount);

            if (added <= 0)
                return 0;

            m_Quantity += added;

            if (m_SpoilsAtUtc == DateTime.MinValue || spoilsAtUtc < m_SpoilsAtUtc)

                m_SpoilsAtUtc = spoilsAtUtc;

            InvalidateProperties();
            return added;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("A pitcher precisa estar na sua mochila.");
                return;
            }

            if (IsSpoiled)
            {
                from.SendMessage("O leite nessa pitcher estragou.");
                EmptyOut();
                return;
            }

            if (IsEmpty)
            {
                from.SendMessage("Essa pitcher está vazia.");
                return;
            }

            PlayerMobile pm = from as PlayerMobile;
            if (pm != null)
            {
                pm.OSUThirst = Math.Min(100, pm.OSUThirst + 5);
                pm.OSUHunger = Math.Min(100, pm.OSUHunger + 2);
            }

            m_Quantity--;

            if (m_Quantity <= 0)
                m_SpoilsAtUtc = DateTime.MinValue;

            InvalidateProperties();
            from.SendMessage("Você bebe um gole de " + MilkDisplayName + ".");
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(MilkDisplayName + ": " + m_Quantity + "/5");

            if (IsSpoiled)
                list.Add("Leite estragado");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_Quantity);
            writer.Write(m_SpoilsAtUtc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Quantity = reader.ReadInt();
            m_SpoilsAtUtc = reader.ReadDateTime();
        }
    }

    public class OSUCowMilkPitcher : OSUBaseMilkPitcher
    {
        public override string MilkDisplayName { get { return "leite de vaca"; } }
        public override OSUMilkKind MilkKind { get { return OSUMilkKind.Cow; } }

        [Constructable]
        public OSUCowMilkPitcher() : base(0x9F0)
        {
            Name = "pitcher de leite de vaca";
        }

        public OSUCowMilkPitcher(Serial serial) : base(serial)
        {
        }
    }

    public class OSUGoatMilkPitcher : OSUBaseMilkPitcher
    {
        public override string MilkDisplayName { get { return "leite de cabra"; } }
        public override OSUMilkKind MilkKind { get { return OSUMilkKind.Goat; } }

        [Constructable]
        public OSUGoatMilkPitcher() : base(0x9F0)
        {
            Name = "pitcher de leite de cabra";
        }

        public OSUGoatMilkPitcher(Serial serial) : base(serial)
        {
        }
    }
}
