
using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Reinos;
using Server.Network;

namespace Server.Custom.Systems.Espetaculos.Items
{
    public class EspetaculoTicket : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;
        private int m_ReservationId;
        private string m_VenueName;
        private string m_RenterName;
        private DateTime m_EventStart;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReservationId { get { return m_ReservationId; } set { m_ReservationId = value; } }

        [Constructable]
        public EspetaculoTicket() : this(-1, String.Empty, 0, "Espetáculo", String.Empty, DateTime.MinValue)
        {
        }

        public EspetaculoTicket(int cityId, string constructionKey, int reservationId, string venueName, string renterName, DateTime eventStart)
            : base(0xE17)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            m_ReservationId = reservationId;
            m_VenueName = venueName ?? "Espetáculo";
            m_RenterName = renterName ?? String.Empty;
            m_EventStart = eventStart;
            Name = "ingresso de " + m_VenueName.ToLower();
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public EspetaculoTicket(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("O ingresso precisa estar na sua mochila.");
                return;
            }

            from.SendMessage("Esse ingresso é válido para " + m_VenueName.ToLower() + " em " + EspetaculoSystem.FormatLongDate(m_EventStart) + ".");
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!String.IsNullOrWhiteSpace(m_RenterName))
                list.Add("Apresentação de " + m_RenterName);

            if (m_EventStart > DateTime.MinValue)
                list.Add("Data: " + EspetaculoSystem.FormatLongDate(m_EventStart));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write(m_ReservationId);
            writer.Write(m_VenueName ?? String.Empty);
            writer.Write(m_RenterName ?? String.Empty);
            writer.Write(m_EventStart);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            m_ReservationId = reader.ReadInt();
            m_VenueName = reader.ReadString();
            m_RenterName = reader.ReadString();
            m_EventStart = reader.ReadDateTime();
            LootType = LootType.Blessed;
        }
    }

    public class EspetaculoControlMarker : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;
        private EspetaculoVenueType m_VenueType;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public EspetaculoVenueType VenueType { get { return m_VenueType; } set { m_VenueType = value; } }

        [Constructable]
        public EspetaculoControlMarker() : this(-1, String.Empty, EspetaculoVenueType.Theater)
        {
        }

        public EspetaculoControlMarker(int cityId, string constructionKey, EspetaculoVenueType venueType)
            : base(0x1B72)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            m_VenueType = venueType;
            Name = "controle de " + EspetaculoSystem.GetVenueLabel(venueType).ToLower();
            Movable = false;
            Visible = true;
        }

        public EspetaculoControlMarker(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 3))
            {
                pm.SendMessage("Chegue mais perto do controle.");
                return;
            }

            if (!EspetaculoSystem.CanAccessControl(pm, m_CityId, m_ConstructionKey))
            {
                pm.SendMessage("Somente o responsável pelo espetáculo, o líder do reino ou o cargo ligado a esta construção pode usar esse controle.");
                return;
            }

            pm.CloseGump(typeof(Server.Custom.Systems.Espetaculos.Gumps.EspetaculoControlGump));
            pm.SendGump(new Server.Custom.Systems.Espetaculos.Gumps.EspetaculoControlGump(pm, m_CityId, m_ConstructionKey, m_VenueType, this.Serial.Value));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write((int)m_VenueType);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            m_VenueType = (EspetaculoVenueType)reader.ReadInt();
            Movable = false;
            Visible = true;
        }
    }

    public class EspetaculoStageLight : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;
        private bool m_IsOn;
        private EspetaculoLightColor m_LightColor;
        private bool m_HiddenEmitter;

        [CommandProperty(AccessLevel.GameMaster)]
        public EspetaculoLightColor LightColor { get { return m_LightColor; } set { m_LightColor = value; ApplyVisual(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HiddenEmitter { get { return m_HiddenEmitter; } set { m_HiddenEmitter = value; ApplyVisual(); } }

        [Constructable]
        public EspetaculoStageLight() : this(-1, String.Empty, EspetaculoLightColor.Blue, false)
        {
        }

        public EspetaculoStageLight(int cityId, string constructionKey, EspetaculoLightColor lightColor)
            : this(cityId, constructionKey, lightColor, false)
        {
        }

        public EspetaculoStageLight(int cityId, string constructionKey, EspetaculoLightColor lightColor, bool hiddenEmitter)
            : base(GetItemId(lightColor))
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            m_IsOn = false;
            m_LightColor = lightColor;
            m_HiddenEmitter = hiddenEmitter;
            Movable = !hiddenEmitter;
            Visible = true;
            Name = hiddenEmitter ? "emissor de luz do palco" : ("luz de palco " + EspetaculoSystem.GetColorLabel(lightColor));
            ApplyVisual();
        }

        public EspetaculoStageLight(Serial serial) : base(serial)
        {
        }

        public void SetEnabled(bool on, EspetaculoLightColor selectedColor)
        {
            m_IsOn = on && selectedColor == m_LightColor;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            ItemID = GetItemId(m_LightColor);

            // IMPORTANTE:
            // fica visível para o servidor/client receberem o item e a luz,
            // mas o cliente patchado não desenha quando Hue == 1.
            Visible = true;
            Movable = !m_HiddenEmitter;
            Hue = m_HiddenEmitter ? 1 : 0;

            Light = m_IsOn ? LightType.Circle300 : LightType.Empty;
            InvalidateProperties();
        }

        public static int GetItemId(EspetaculoLightColor color)
        {
            switch (color)
            {
                case EspetaculoLightColor.Red: return 0x40FE;
                case EspetaculoLightColor.Green: return 0x4100;
                case EspetaculoLightColor.Purple: return 0x4101;
                case EspetaculoLightColor.White: return 0x0A15;
                case EspetaculoLightColor.Yellow: return 0x1647;
                default: return 0x40FF; // azul
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write(m_IsOn);
            writer.Write((int)m_LightColor);
            writer.Write(m_HiddenEmitter);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            m_IsOn = reader.ReadBool();

            if (version >= 1)
            {
                m_LightColor = (EspetaculoLightColor)reader.ReadInt();
            }
            else
            {
                int oldHue = reader.ReadInt();
                m_LightColor = EspetaculoLightColor.Blue;
            }

            if (version >= 2)
                m_HiddenEmitter = reader.ReadBool();
            else
                m_HiddenEmitter = false;

            Name = m_HiddenEmitter ? "emissor de luz do palco" : ("luz de palco " + EspetaculoSystem.GetColorLabel(m_LightColor));
            ApplyVisual();
        }
    }

    // Itens constructables de luz colorida para staff/testes.
    public class OSUStageLightBlue : EspetaculoStageLight
    {
        [Constructable]
        public OSUStageLightBlue() : base(-1, String.Empty, EspetaculoLightColor.Blue, false) { Name = "luz de palco azul"; Movable = true; }
        public OSUStageLightBlue(Serial serial) : base(serial) { }
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

    public class OSUStageLightRed : EspetaculoStageLight
    {
        [Constructable]
        public OSUStageLightRed() : base(-1, String.Empty, EspetaculoLightColor.Red, false) { Name = "luz de palco vermelha"; Movable = true; }
        public OSUStageLightRed(Serial serial) : base(serial) { }
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

    public class OSUStageLightGreen : EspetaculoStageLight
    {
        [Constructable]
        public OSUStageLightGreen() : base(-1, String.Empty, EspetaculoLightColor.Green, false) { Name = "luz de palco verde"; Movable = true; }
        public OSUStageLightGreen(Serial serial) : base(serial) { }
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

    public class OSUStageLightPurple : EspetaculoStageLight
    {
        [Constructable]
        public OSUStageLightPurple() : base(-1, String.Empty, EspetaculoLightColor.Purple, false) { Name = "luz de palco roxa"; Movable = true; }
        public OSUStageLightPurple(Serial serial) : base(serial) { }
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

    public class OSUStageLightWhite : EspetaculoStageLight
    {
        [Constructable]
        public OSUStageLightWhite() : base(-1, String.Empty, EspetaculoLightColor.White, false) { Name = "luz de palco branca"; Movable = true; }
        public OSUStageLightWhite(Serial serial) : base(serial) { }
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

    public class OSUStageLightYellow : EspetaculoStageLight
    {
        [Constructable]
        public OSUStageLightYellow() : base(-1, String.Empty, EspetaculoLightColor.Yellow, false) { Name = "luz de palco amarela"; Movable = true; }
        public OSUStageLightYellow(Serial serial) : base(serial) { }
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

    public class EspetaculoSetPieceItem : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;
        private string m_SetPieceId;
        private Point3D m_ClosedOffset;
        private Point3D m_OpenOffset;
        private bool m_Open;

        [CommandProperty(AccessLevel.GameMaster)]
        public string SetPieceId { get { return m_SetPieceId; } }

        [Constructable]
        public EspetaculoSetPieceItem() : this(String.Empty, -1, String.Empty, 0x1224, Point3D.Zero, Point3D.Zero)
        {
        }

        public EspetaculoSetPieceItem(string setPieceId, int cityId, string constructionKey, int itemId, Point3D closedOffset, Point3D openOffset)
            : base(itemId)
        {
            m_SetPieceId = setPieceId ?? String.Empty;
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            m_ClosedOffset = closedOffset;
            m_OpenOffset = openOffset;
            Movable = false;
            m_Open = false;
            Name = null;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            // vazio de propósito: evita tooltip automático com nome
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            // vazio de propósito: evita tooltip ao passar o mouse
        }

        public EspetaculoSetPieceItem(Serial serial) : base(serial)
        {
        }

        public void SetOpen(bool open)
        {
            m_Open = open;

            ReinoConstructionRuntimeInfo info = ReinoMaintenanceSystem.GetConstruction(m_ConstructionKey);
            if (info == null || info.Lot == null || info.Lot.Map == null)
                return;

            Point3D anchor = info.Lot.NorthWest;
            Point3D offset = m_Open ? m_OpenOffset : m_ClosedOffset;
            MoveToWorld(new Point3D(anchor.X + offset.X, anchor.Y + offset.Y, anchor.Z + offset.Z), info.Lot.Map);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write(m_SetPieceId ?? String.Empty);
            writer.Write(m_ClosedOffset);
            writer.Write(m_OpenOffset);
            writer.Write(m_Open);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            m_SetPieceId = reader.ReadString();
            m_ClosedOffset = reader.ReadPoint3D();
            m_OpenOffset = reader.ReadPoint3D();
            m_Open = reader.ReadBool();
            Movable = false;
            SetOpen(m_Open);
        }
    }

    public class EspetaculoVenueDoor : BaseDoor
    {
        private int m_CityId;
        private string m_ConstructionKey;
        private bool m_EventRestricted;

        [Constructable]
        public EspetaculoVenueDoor() : this(0x675, 0x676, 0xEC, 0xF3, Point3D.Zero, -1, String.Empty)
        {
        }

        public EspetaculoVenueDoor(int closedId, int openedId, int openedSound, int closedSound, Point3D offset, int cityId, string constructionKey)
            : base(closedId, openedId, openedSound, closedSound, offset)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            Movable = false;
            Locked = false;
            KeyValue = 0;
            m_EventRestricted = false;
        }

        public EspetaculoVenueDoor(Serial serial) : base(serial)
        {
        }

        public override bool UseLocks()
        {
            return false;
        }

        public void SyncLockedState(bool restricted)
        {
            m_EventRestricted = restricted;
            Locked = false;
            KeyValue = 0;
            InvalidateProperties();
        }

        public override void Use(Mobile from)
        {
            if (m_EventRestricted && !EspetaculoSystem.CanUsePhysicalDoor(from, m_ConstructionKey, m_CityId))
            {
                from.SendMessage("A entrada só é permitida para quem tem ingresso válido.");
                return;
            }

            base.Use(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
            writer.Write(m_EventRestricted);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            m_EventRestricted = reader.ReadBool();
            Movable = false;
            Locked = false;
            KeyValue = 0;
        }
    }
}
