using Server.Custom.Systems.PlayerMadeStatues;
using Server.Gumps;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public abstract class BaseStatuePlatformItem : Item
    {
        private int m_MaterialId;
        private bool m_HasSign;
        private string m_SignText;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaterialId { get { return m_MaterialId; } set { m_MaterialId = value; ApplyMaterial(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasSign { get { return m_HasSign; } set { m_HasSign = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string SignText
        {
            get { return m_SignText; }
            set
            {
                m_SignText = value;
                Name = m_SignText;
                InvalidateProperties();
            }
        }

        public override bool DisplayWeight { get { return false; } }

        public abstract string RecipeName { get; }
        public abstract StatuePlatformSize PlatformSize { get; }

        public abstract int PlatformHeight { get; }

        protected BaseStatuePlatformItem(int itemID) : base(itemID)
        {
            Movable = false;
            Weight = 50.0;
            Name = null;
            m_MaterialId = 1000;
            m_HasSign = false;
            m_SignText = null;
            ApplyMaterial();
        }

        public virtual void ApplyMaterial()
        {
            Hue = StatueMaterialOptions.GetHue(m_MaterialId);
        }

        public override void OnSingleClick(Mobile from)
        {
            if (!string.IsNullOrWhiteSpace(m_SignText))
                LabelTo(from, m_SignText);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            if (!string.IsNullOrWhiteSpace(m_SignText))
                list.Add(m_SignText);
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!m_HasSign)
            {
                from.SendMessage("Escolha uma placa para anexar à plataforma.");
                from.Target = new SignAttachTarget(this);
            }
            else
            {
                from.SendGump(new GenericSignGump());
            }
        }

        private class SignAttachTarget : Target
        {
            private readonly BaseStatuePlatformItem m_Platform;
            public SignAttachTarget(BaseStatuePlatformItem platform) : base(2, false, TargetFlags.None)
            {
                m_Platform = platform;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Platform == null || m_Platform.Deleted)
                    return;

                GenericSign sign = targeted as GenericSign;
                if (sign == null || sign.Deleted)
                {
                    from.SendMessage("Isso não é uma placa válida.");
                    return;
                }

                if (!sign.IsChildOf(from.Backpack) && !(sign.RootParent == null && sign.Map == from.Map && from.InRange(sign.GetWorldLocation(), 2)))
                {
                    from.SendMessage("A placa precisa estar na sua mochila ou no chão perto de você.");
                    return;
                }

                string signText = sign.Name;

                sign.Delete();
                m_Platform.m_HasSign = true;
                m_Platform.m_SignText = signText;
                m_Platform.Name = signText;
                m_Platform.InvalidateProperties();
                from.SendMessage("Placa anexada à plataforma.");
            }
        }

        public BaseStatuePlatformItem(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_MaterialId);
            writer.Write(m_HasSign);
            writer.Write(m_SignText);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_MaterialId = reader.ReadInt();
            m_HasSign = reader.ReadBool();

            if (version >= 1)
                m_SignText = reader.ReadString();
            else
                m_SignText = null;

            Name = m_SignText;
            ApplyMaterial();
        }
    }
}
