using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public abstract class ReinoCargoLetterBase : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get; set; }

        protected ReinoCargoLetterBase() : base(0x14ED)
        {
            Weight = 1.0;
            Name = "carta";
        }

        public ReinoCargoLetterBase(Serial serial) : base(serial)
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

    public class ReinoCargoInvitationLetterBlank : ReinoCargoLetterBase
    {
        [Constructable]
        public ReinoCargoInvitationLetterBlank()
        {
            Name = "Carta de convite de cargo";
        }

        public ReinoCargoInvitationLetterBlank(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            int cityId = CityId >= 0 ? CityId : ReinoEmploymentSystem.GetActingGovernmentCityId(pm);
            if (cityId < 0)
            {
                pm.SendMessage("Esta carta ainda não está ligada a um reino.");
                return;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(pm, cityId) && !ReinoEmploymentSystem.PlayerHasAnyCommissionedRole(pm, cityId))
            {
                pm.SendMessage("Você não pode usar esta carta.");
                return;
            }

            pm.SendGump(new ReinoCargoLetterSetupGump(pm, cityId, true, 0, 0, String.Empty, Serial.Value));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            CityId = reader.ReadInt();
        }
    }

    public class ReinoCargoDismissalLetterBlank : ReinoCargoLetterBase
    {
        [Constructable]
        public ReinoCargoDismissalLetterBlank()
        {
            Name = "Carta de exoneração";
        }

        public ReinoCargoDismissalLetterBlank(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            int cityId = CityId >= 0 ? CityId : ReinoEmploymentSystem.GetActingGovernmentCityId(pm);
            if (cityId < 0)
            {
                pm.SendMessage("Esta carta ainda não está ligada a um reino.");
                return;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(pm, cityId) && !ReinoEmploymentSystem.PlayerHasAnyCommissionedRole(pm, cityId))
            {
                pm.SendMessage("Você não pode usar esta carta.");
                return;
            }

            pm.SendGump(new ReinoCargoLetterSetupGump(pm, cityId, false, 0, 0, String.Empty, Serial.Value)); pm.SendGump(new ReinoCargoLetterSetupGump(pm, cityId, false, 0, 0, String.Empty, 0));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            CityId = reader.ReadInt();
        }
    }

    public class ReinoCargoInvitationLetter : ReinoCargoLetterBase
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int RoleId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TargetSerial { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string InviterName { get; set; }

        [Constructable]
        public ReinoCargoInvitationLetter() : this(-1, 0, 0, String.Empty)
        {
        }

        public ReinoCargoInvitationLetter(int cityId, int roleId, int targetSerial, string inviterName)
        {
            CityId = cityId;
            RoleId = roleId;
            TargetSerial = targetSerial;
            InviterName = inviterName ?? String.Empty;
            Name = "Carta de convite de cargo";
        }

        public ReinoCargoInvitationLetter(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            bool allowAccept = pm.Serial.Value == TargetSerial;
            pm.SendGump(new ReinoCargoInvitationGump(pm, CityId, RoleId, InviterName, allowAccept, Serial.Value));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(CityId);
            writer.Write(RoleId);
            writer.Write(TargetSerial);
            writer.Write(InviterName ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            CityId = reader.ReadInt();
            RoleId = reader.ReadInt();
            TargetSerial = reader.ReadInt();
            InviterName = reader.ReadString();
        }
    }

    public class ReinoCargoDismissalLetter : ReinoCargoLetterBase
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public string RoleTitle { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string DismissedBy { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TargetSerial { get; set; }

        [Constructable]
        public ReinoCargoDismissalLetter() : this(-1, String.Empty, String.Empty, 0)
        {
        }

        public ReinoCargoDismissalLetter(int cityId, string roleTitle, string dismissedBy, int targetSerial)
        {
            CityId = cityId;
            RoleTitle = roleTitle ?? String.Empty;
            DismissedBy = dismissedBy ?? String.Empty;
            TargetSerial = targetSerial;
            Name = "Carta de exoneração";
        }

        public ReinoCargoDismissalLetter(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            string body = "<BASEFONT COLOR=#000000>Você foi exonerado do cargo de <B>" + RoleTitle + "</B>.";
            if (!String.IsNullOrWhiteSpace(DismissedBy))
                body += "<BR><BR>Responsável pela exoneração: " + DismissedBy + ".";
            body += "</BASEFONT>";

            from.SendGump(new ReinoCargoDismissalNoticeGump("Carta de exoneração", body, Serial.Value));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(CityId);
            writer.Write(RoleTitle ?? String.Empty);
            writer.Write(DismissedBy ?? String.Empty);
            writer.Write(TargetSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            CityId = reader.ReadInt();
            RoleTitle = reader.ReadString();
            DismissedBy = reader.ReadString();
            TargetSerial = reader.ReadInt();
        }
    }
}
