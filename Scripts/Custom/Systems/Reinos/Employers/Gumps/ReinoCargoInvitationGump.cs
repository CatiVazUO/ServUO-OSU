using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server;
using Server.Items;

namespace Server.Custom.Reinos
{
    public class ReinoCargoInvitationGump : Gump
    {
        private readonly int m_CityId;
        private readonly int m_RoleId;
        private readonly string m_InviterName;
        private readonly bool m_AllowAccept;
        private readonly int m_SourceLetterSerial;

        public ReinoCargoInvitationGump(PlayerMobile from, int cityId, int roleId, string inviterName, bool allowAccept, int sourceLetterSerial) : base(0, 0)
        {
            m_CityId = cityId;
            m_RoleId = roleId;
            m_InviterName = inviterName ?? String.Empty;
            m_AllowAccept = allowAccept;
            m_SourceLetterSerial = sourceLetterSerial;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetRole(cityId, roleId);
            string html;

            if (role == null)
                html = "<BASEFONT COLOR=#000000>Este cargo não existe mais.</BASEFONT>";
            else
            {
                html = "<BASEFONT COLOR=#000000><BIG><B>" + role.Title + "</B></BIG><BR><BR>";
                if (!String.IsNullOrWhiteSpace(m_InviterName))
                    html += "Convidado por: " + m_InviterName + ".<BR><BR>";
                html += role.Description + "<BR><BR>";
                html += "Salário semanal: " + role.WeeklySalaryGold + " moedas.<BR>";
                html += "Hierarquia: " + role.Hierarchy + ".</BASEFONT>";
            }

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(303, 128, 0, @"Convite para Cargo Comissionado");
            AddHtml(221, 154, 377, 168, html, false, false);
            if (m_AllowAccept)
                AddButton(191, 359, 492, 492, 1, GumpButtonType.Reply, 0);
            AddImage(535, 307, 2923);
        }

        private void DeleteSourceLetter()
        {
            if (m_SourceLetterSerial <= 0)
                return;

            Item item = World.FindItem((Serial)m_SourceLetterSerial);

            if (item != null && !item.Deleted)
                item.Delete();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (info.ButtonID != 1 || !m_AllowAccept)
            {
                DeleteSourceLetter();
                return;
            }

            string message;
            ReinoEmploymentSystem.AcceptInvitation(from, m_CityId, m_RoleId, m_InviterName, out message);
            from.SendMessage(message);
            DeleteSourceLetter();
        }
    }
}
