using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server;

namespace Server.Custom.Reinos
{
    public class ReinoCargoLetterSetupGump : Gump
    {
        private readonly PlayerMobile m_From;
        private readonly int m_CityId;
        private readonly bool m_ForHire;
        private readonly int m_Page;
        private readonly int m_SelectedRoleId;
        private readonly int m_SourceLetterSerial;

        public ReinoCargoLetterSetupGump(PlayerMobile from, int cityId, bool forHire, int page, int selectedRoleId, string typedName, int sourceLetterSerial) : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;
            m_ForHire = forHire;
            m_Page = page < 0 ? 0 : page;
            m_SelectedRoleId = selectedRoleId;
            m_SourceLetterSerial = sourceLetterSerial;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRolesBelowActor(from, cityId, forHire);
            int pageCount = Math.Max(1, (roles.Count + 3) / 4);
            int currentPage = m_Page >= pageCount ? pageCount - 1 : m_Page;
            int start = currentPage * 4;
            int[] y = new int[] { 180, 210, 241, 271 };

            AddPage(0);
            AddImage(113, 55, 3557);
            AddLabel(303, 128, 0, forHire ? @"Convite para Cargo Comissionado" : @"Carta de exoneração");
            AddButton(191, 359, 492, 492, 1, GumpButtonType.Reply, 0);
            AddImage(535, 307, ReinoVisualSystem.GetSealGumpId(m_CityId));

            for (int i = 0; i < 4; i++)
            {
                int idx = start + i;
                if (idx >= roles.Count)
                    break;

                ReinoCargoEntry role = roles[idx];
                AddLabel(267, y[i], 0, role.Title);
                AddButton(234, y[i], m_SelectedRoleId == role.RoleId ? 528 : 531, 528, 100 + role.RoleId, GumpButtonType.Reply, 0);
            }

            AddTextEntry(319, 305, 200, 20, 0, 5, typedName ?? String.Empty);
            if (currentPage > 0)
                AddButton(429, 162, 498, 498, 2, GumpButtonType.Reply, 0);
            if ((start + 4) < roles.Count)
                AddButton(542, 162, 499, 499, 3, GumpButtonType.Reply, 0);
            AddLabel(501, 165, 0, (currentPage + 1) + "/" + pageCount);
            AddLabel(266, 307, 0, @"Nome:");
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

            TextRelay tr = info.GetTextEntry(5);
            string typedName = tr != null ? tr.Text : String.Empty;

            if (info.ButtonID == 2)
            {
                from.SendGump(new ReinoCargoLetterSetupGump(from, m_CityId, m_ForHire, m_Page - 1, m_SelectedRoleId, typedName, m_SourceLetterSerial));
                return;
            }

            if (info.ButtonID == 3)
            {
                from.SendGump(new ReinoCargoLetterSetupGump(from, m_CityId, m_ForHire, m_Page + 1, m_SelectedRoleId, typedName, m_SourceLetterSerial));
                return;
            }

            if (info.ButtonID >= 100)
            {
                int roleId = info.ButtonID - 100;
                from.SendGump(new ReinoCargoLetterSetupGump(from, m_CityId, m_ForHire, m_Page, roleId, typedName, m_SourceLetterSerial));
                return;
            }

            if (info.ButtonID != 1)
                return;

            if (m_SelectedRoleId <= 0)
            {
                from.SendMessage("Selecione um cargo.");
                from.SendGump(new ReinoCargoLetterSetupGump(from, m_CityId, m_ForHire, m_Page, m_SelectedRoleId, typedName, m_SourceLetterSerial));
                return;
            }

            string message;
            if (!ReinoEmploymentSystem.CanActorManageLowerRole(from, m_CityId, m_SelectedRoleId, m_ForHire, out message))
            {
                from.SendMessage(message);
                return;
            }

            PlayerMobile target;
            if (!ReinoEmploymentSystem.FindPlayerByName(typedName, out target))
            {
                from.SendMessage("Jogador não encontrado.");
                from.SendGump(new ReinoCargoLetterSetupGump(from, m_CityId, m_ForHire, m_Page, m_SelectedRoleId, typedName, m_SourceLetterSerial));
                return;
            }

            if (m_ForHire)
            {
                ReinoEmploymentSystem.DeliverInvitationLetter(from, target, m_CityId, m_SelectedRoleId);
                DeleteSourceLetter();
                from.SendMessage("Carta de convite enviada.");
                return;
            }

            ReinoCargoEntry role = ReinoEmploymentSystem.GetRole(m_CityId, m_SelectedRoleId);
            if (role == null || !role.IsOccupied || !String.Equals(role.OccupantName, target.Name, StringComparison.OrdinalIgnoreCase))
            {
                from.SendMessage("Esse jogador não ocupa o cargo selecionado.");
                from.SendGump(new ReinoCargoLetterSetupGump(from, m_CityId, m_ForHire, m_Page, m_SelectedRoleId, typedName, m_SourceLetterSerial));
                return;
            }

            ReinoEmploymentSystem.RemoveRoleOccupant(from, m_CityId, m_SelectedRoleId, true, out message);
            DeleteSourceLetter();
            from.SendMessage(message);
        }
    }
}
