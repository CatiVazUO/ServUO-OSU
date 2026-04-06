using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonGovTopSelectBase = 51000;
        private const int ButtonGovSalaryBase = 52000;
        private const int ButtonGovNominateBase = 53000;
        private const int ButtonGovExonerateBase = 54000;
        private const int ButtonGovRemoveEmptyBase = 55000;
        private const int ButtonGovTopPrev = 56000;
        private const int ButtonGovTopNext = 56001;
        private const int ButtonGovShowAdd = 56002;

        private const int ButtonGovAddSelectBase = 56100;
        private const int ButtonGovAddConfirm = 56200;
        private const int ButtonGovAddRepresentative = 56201;
        private const int ButtonGovOpenCreate = 56202;

        private const int ButtonGovBottomPrev = 56210;
        private const int ButtonGovBottomNext = 56211;

        private const int ButtonGovCreateToggleFinancial = 57001;
        private const int ButtonGovCreateToggleMilitary = 57002;
        private const int ButtonGovCreateToggleHire = 57003;
        private const int ButtonGovCreateToggleFire = 57004;
        private const int ButtonGovCreateHierarchyDown = 57005;
        private const int ButtonGovCreateHierarchyUp = 57006;
        private const int ButtonGovCreateConstructionSelectBase = 57100;
        private const int ButtonGovCreateConstructionPrev = 57200;
        private const int ButtonGovCreateConstructionNext = 57201;
        private const int ButtonGovCreateSubmit = 57202;

        private const int EntryGovSalaryEdit = 5;
        private const int EntryGovCreateName = 1;
        private const int EntryGovCreateSalary = 2;
        private const int EntryGovCreateDescription = 4;

        private void BuildGovernmentPage()
        {
            AddPage(0);
            AddLabel(779, 173, 0, @"Cargos");
            AddLabel(413, 231, 0, @"Cargos");
            AddImageTiled(407, 261, 825, 5, 367);
            AddImageTiled(407, 455, 825, 5, 367);
            AddImageTiled(407, 641, 825, 5, 367);
            AddImageTiled(653, 655, 6, 36, 365);

            ReinoEmploymentSession session = ReinoEmploymentSystem.GetSession(m_From, m_CityId);
            List<ReinoCargoEntry> roles = ReinoEmploymentSystem.GetRoles(m_CityId);
            int topPageCount = Math.Max(1, (roles.Count + 3) / 4);

            if (session.TopPage < 0)
                session.TopPage = 0;
            if (session.TopPage >= topPageCount)
                session.TopPage = topPageCount - 1;

            int topStart = session.TopPage * 4;
            int[] topY = new int[] { 284, 314, 345, 375 };
            ReinoCargoEntry selectedTop = null;
            ReinoCargoEntry firstVisibleTop = null;

            for (int i = 0; i < 4; i++)
            {
                int realIndex = topStart + i;
                if (realIndex >= roles.Count)
                    break;

                ReinoCargoEntry role = roles[realIndex];
                if (role == null)
                    continue;

                if (firstVisibleTop == null)
                    firstVisibleTop = role;
                if (role.RoleId == session.SelectedTopRoleId)
                    selectedTop = role;

                int y = topY[i];
                bool selected = role.RoleId == session.SelectedTopRoleId && !session.PreferBottomSelection;

                AddButton(415, y + 1, selected ? 528 : 531, 528, ButtonGovTopSelectBase + role.RoleId, GumpButtonType.Reply, 0);
                AddLabel(447, y, 0, role.Title + ":");
                AddLabel(598, y, 0, role.IsOccupied ? role.OccupantName : "Vago");

                if (!role.IsLeaderRole && !role.IsOccupied)
                {
                    AddButton(676, y + 2, 531, 531, ButtonGovNominateBase + role.RoleId, GumpButtonType.Reply, 0);
                    AddLabel(707, y, 0, @"Nomear");

                    if (role.IsRemovable)
                    {
                        AddButton(804, y + 2, 531, 531, ButtonGovRemoveEmptyBase + role.RoleId, GumpButtonType.Reply, 0);
                        AddLabel(836, y, 0, @"Remover");
                    }
                }
                else if (!role.IsLeaderRole && role.IsOccupied)
                {
                    AddButton(804, y + 2, 531, 531, ButtonGovExonerateBase + role.RoleId, GumpButtonType.Reply, 0);
                    AddLabel(836, y, 0, @"Exonerar");
                }

                AddButton(940, y + 2, 531, 531, ButtonGovSalaryBase + role.RoleId, GumpButtonType.Reply, 0);
                AddLabel(969, y, 0, @"Salário:");

                if (session.EditingSalaryRoleId == role.RoleId)
                    AddTextEntry(1028, y, 90, 20, 0, EntryGovSalaryEdit, role.WeeklySalaryGold.ToString());
                else
                    AddLabel(1035, y, 0, role.WeeklySalaryGold.ToString());

                AddLabel(1130, y, 0, @"Hierarquia: " + role.Hierarchy);
            }

            if (selectedTop == null)
                selectedTop = firstVisibleTop;

            if (session.TopPage > 0)
                AddButton(1050, 420, 498, 498, ButtonGovTopPrev, GumpButtonType.Reply, 0);

            if ((topStart + 4) < roles.Count)
                AddButton(1163, 420, 499, 499, ButtonGovTopNext, GumpButtonType.Reply, 0);

            AddLabel(1122, 423, 0, (session.TopPage + 1) + "/" + topPageCount);

            AddLabel(447, 426, 0, @"Adicionar");
            AddButton(415, 425, session.ShowAddList ? 528 : 531, 528, ButtonGovShowAdd, GumpButtonType.Reply, 0);

            ReinoCargoEntry selectedBottom = null;
            if (session.ShowAddList)
                selectedBottom = BuildGovernmentAddList(session);

            AddLabel(413, 665, 0, @"Representante Comercial");
            AddLabel(705, 665, 0, @"Custo Semanal: " + ReinoEmploymentSystem.GetRepresentativeSalary(m_CityId) + " Moedas");
            AddButton(587, 666, 529, 529, ButtonGovAddRepresentative, GumpButtonType.Reply, 0);
            AddLabel(1138, 665, 0, @"Criar Cargo");
            AddButton(1108, 665, 529, 529, ButtonGovOpenCreate, GumpButtonType.Reply, 0);

            ReinoCargoEntry htmlRole = session.ShowAddList && session.PreferBottomSelection && selectedBottom != null ? selectedBottom : selectedTop;
            AddHtml(669, 477, 453, 145, BuildGovernmentRoleHtml(htmlRole), false, true);

            string missing = ReinoEmploymentSystem.GetMissingEssentialsMessage(m_CityId);
            if (!String.IsNullOrWhiteSpace(missing))
                AddHtml(669, 620, 500, 50, "<BASEFONT COLOR=#8B0000>" + missing + "</BASEFONT>", false, false);
        }

        private ReinoCargoEntry BuildGovernmentAddList(ReinoEmploymentSession session)
        {
            List<ReinoCargoEntry> options = ReinoEmploymentSystem.GetAddableRoleTemplates(m_CityId);
            int pageCount = Math.Max(1, (options.Count + 3) / 4);

            if (session.BottomPage < 0)
                session.BottomPage = 0;
            if (session.BottomPage >= pageCount)
                session.BottomPage = pageCount - 1;

            int start = session.BottomPage * 4;
            int[] y = new int[] { 477, 507, 537, 567 };
            ReinoCargoEntry selected = null;
            ReinoCargoEntry firstVisible = null;

            for (int i = 0; i < 4; i++)
            {
                int real = start + i;
                if (real >= options.Count)
                    break;

                ReinoCargoEntry role = options[real];
                if (role == null)
                    continue;

                if (firstVisible == null)
                    firstVisible = role;
                if (role.RoleId == session.SelectedBottomIndex)
                    selected = role;

                bool isSelected = role.RoleId == session.SelectedBottomIndex && session.PreferBottomSelection;
                AddButton(415, y[i], isSelected ? 528 : 531, 528, ButtonGovAddSelectBase + i, GumpButtonType.Reply, 0);
                AddLabel(447, y[i], 0, role.Title);
            }

            if (selected == null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (options[i] != null && options[i].RoleId == session.SelectedBottomIndex)
                    {
                        selected = options[i];
                        break;
                    }
                }

                if (selected == null)
                    selected = firstVisible;
            }

            if (session.BottomPage > 0)
                AddButton(440, 603, 498, 498, ButtonGovBottomPrev, GumpButtonType.Reply, 0);

            if ((start + 4) < options.Count)
                AddButton(553, 603, 499, 499, ButtonGovBottomNext, GumpButtonType.Reply, 0);

            AddLabel(512, 606, 0, (session.BottomPage + 1) + "/" + pageCount);
            AddButton(1141, 533, 492, 492, ButtonGovAddConfirm, GumpButtonType.Reply, 0);
            return selected;
        }

        private string BuildGovernmentRoleHtml(ReinoCargoEntry role)
        {
            if (role == null)
                return "<BASEFONT COLOR=#000000>Selecione um cargo para ver os detalhes.</BASEFONT>";

            int count = ReinoEmploymentSystem.GetRoleSlotCount(m_CityId, role.Title);
            string html = "<BASEFONT COLOR=#000000><BIG><B>" + role.Title + "</B></BIG><BR><BR>";
            html += role.Description + "<BR><BR>";
            html += "Salário semanal: " + role.WeeklySalaryGold + " moedas.<BR>";
            html += "Hierarquia: " + role.Hierarchy + ".<BR>";
            html += "Quantidade existente: " + count + ".<BR>";
            html += role.IsOccupied ? ("Ocupante atual: " + role.OccupantName + ".") : "Cargo atualmente vago.";
            html += "</BASEFONT>";
            return html;
        }

        private void BuildCreateRolePage()
        {
            AddPage(0);
            AddLabel(417, 232, 0, @"Nome do Cargo:");
            AddImageTiled(414, 363, 825, 5, 367);
            AddLabel(416, 262, 0, @"Salário:");
            AddLabel(417, 302, 0, @"Hierarquia:");
            AddLabel(436, 381, 0, @"Ligado a Construção:");
            AddLabel(773, 172, 0, @"Criar Cargos");

            ReinoEmploymentSession session = ReinoEmploymentSystem.GetSession(m_From, m_CityId);

            AddTextEntry(525, 231, 200, 20, 0, EntryGovCreateName, session.CreateName ?? String.Empty);
            AddTextEntry(525, 260, 200, 20, 0, EntryGovCreateSalary, session.CreateSalary ?? "0");
            AddButton(544, 299, 583, 583, ButtonGovCreateHierarchyDown, GumpButtonType.Reply, 0);
            AddButton(670, 297, 582, 582, ButtonGovCreateHierarchyUp, GumpButtonType.Reply, 0);
            AddLabel(614, 301, 0, session.CreateHierarchy ?? "3");

            AddLabel(675, 381, 0, @"Características do Cargo:");
            AddImageTiled(413, 413, 825, 5, 367);
            AddImageTiled(604, 380, 6, 314, 365);

            AddLabel(642, 437, 0, @"Pode tomar decisoões financeiras?");
            AddButton(877, 433, session.CreateCanFinancial ? 438 : 439, session.CreateCanFinancial ? 438 : 439, ButtonGovCreateToggleFinancial, GumpButtonType.Reply, 0);
            AddLabel(642, 477, 0, @"Pode tomar decisoões militares?");
            AddButton(877, 473, session.CreateCanMilitary ? 438 : 439, session.CreateCanMilitary ? 438 : 439, ButtonGovCreateToggleMilitary, GumpButtonType.Reply, 0);
            AddLabel(643, 518, 0, @"Pode contratar?");
            AddButton(878, 514, session.CreateCanHire ? 438 : 439, session.CreateCanHire ? 438 : 439, ButtonGovCreateToggleHire, GumpButtonType.Reply, 0);
            AddLabel(643, 558, 0, @"Pode exonerar?");
            AddButton(878, 554, session.CreateCanFire ? 438 : 439, session.CreateCanFire ? 438 : 439, ButtonGovCreateToggleFire, GumpButtonType.Reply, 0);

            AddTextEntry(788, 256, 434, 90, 0, EntryGovCreateDescription, session.CreateDescription ?? String.Empty);
            AddLabel(946, 222, 0, @"Descrição do Cargo:");

            BuildCreateRoleConstructionList(session);

            AddHtml(941, 442, 288, 237, GetCreateRoleInfoHtml(session), false, true);
            AddLabel(661, 667, 0, @"Criar do Cargo:");
            AddButton(772, 663, 492, 492, ButtonGovCreateSubmit, GumpButtonType.Reply, 0);
        }

        private void BuildCreateRoleConstructionList(ReinoEmploymentSession session)
        {
            List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);
            int pageCount = Math.Max(1, (active.Count + 5) / 6);

            if (session.SelectedConstructionPage < 0)
                session.SelectedConstructionPage = 0;
            if (session.SelectedConstructionPage >= pageCount)
                session.SelectedConstructionPage = pageCount - 1;

            int start = session.SelectedConstructionPage * 6;
            int[] y = new int[] { 435, 465, 495, 525, 555, 585 };

            for (int i = 0; i < 6; i++)
            {
                int real = start + i;
                if (real >= active.Count)
                    break;

                ReinoConstructionRuntimeInfo info = active[real];
                bool selected = String.Equals(session.CreateLinkedConstructionKey, info.Key, StringComparison.OrdinalIgnoreCase);
                AddLabel(476, y[i], 0, info.Name);
                AddButton(452, y[i], selected ? 433 : 434, selected ? 433 : 434, ButtonGovCreateConstructionSelectBase + i, GumpButtonType.Reply, 0);
            }

            if (session.SelectedConstructionPage > 0)
                AddButton(415, 668, 498, 498, ButtonGovCreateConstructionPrev, GumpButtonType.Reply, 0);
            if ((start + 6) < active.Count)
                AddButton(528, 668, 499, 499, ButtonGovCreateConstructionNext, GumpButtonType.Reply, 0);

            AddLabel(487, 671, 0, (session.SelectedConstructionPage + 1) + "/" + pageCount);
        }

        private string GetCreateRoleInfoHtml(ReinoEmploymentSession session)
        {
            if (session == null)
                return "<BASEFONT COLOR=#000000>Selecione uma característica ou uma construção para ver a explicação.</BASEFONT>";

            if (session.CreateInfoIsPermission)
            {
                switch ((session.CreateInfoKey ?? String.Empty).Trim().ToLowerInvariant())
                {
                    case "financial":
                        return "<BASEFONT COLOR=#000000><B>Decisões financeiras</B><BR><BR>Permite que o ocupante tome decisões econômicas do reino. Sem vínculo de construção, o cargo atua nas decisões financeiras gerais. Com vínculo de construção, ele atua apenas naquela construção.</BASEFONT>";
                    case "military":
                        return "<BASEFONT COLOR=#000000><B>Decisões militares</B><BR><BR>Permite que o ocupante tome decisões militares do reino. Sem vínculo de construção, o cargo atua nas decisões militares gerais. Com vínculo de construção, ele atua apenas onde esse vínculo permitir.</BASEFONT>";
                    case "hire":
                        return "<BASEFONT COLOR=#000000><B>Pode contratar</B><BR><BR>Permite enviar convites para cargos de hierarquia mais baixa que a do próprio cargo. Nunca permite contratar cargos da mesma hierarquia ou acima dela.</BASEFONT>";
                    case "fire":
                        return "<BASEFONT COLOR=#000000><B>Pode exonerar</B><BR><BR>Permite exonerar cargos de hierarquia mais baixa que a do próprio cargo. Nunca permite exonerar cargos da mesma hierarquia ou acima dela.</BASEFONT>";
                }
            }

            return "<BASEFONT COLOR=#000000>" + ReinoEmploymentSystem.GetConstructionRoleDescription(session.CreateLinkedConstructionKey) + "</BASEFONT>";
        }

        private void UpdateCreateSessionFromResponse(ReinoEmploymentSession session, RelayInfo info)
        {
            if (session == null || info == null)
                return;

            TextRelay tr;

            tr = info.GetTextEntry(EntryGovCreateName);
            if (tr != null)
                session.CreateName = tr.Text;

            tr = info.GetTextEntry(EntryGovCreateSalary);
            if (tr != null)
                session.CreateSalary = tr.Text;

            tr = info.GetTextEntry(EntryGovCreateDescription);
            if (tr != null)
                session.CreateDescription = tr.Text;
        }

        private int GetSalaryEntryValue(RelayInfo info, int roleId, int current)
        {
            TextRelay tr = info.GetTextEntry(EntryGovSalaryEdit);
            if (tr == null || String.IsNullOrWhiteSpace(tr.Text))
                return current;

            int value;
            if (Int32.TryParse(tr.Text.Trim(), out value))
                return Math.Max(0, value);

            return current;
        }

        private bool HandleGovernmentResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;
            ReinoEmploymentSession session = ReinoEmploymentSystem.GetSession(from, m_CityId);
            UpdateCreateSessionFromResponse(session, info);

            if (button >= ButtonGovTopSelectBase && button < ButtonGovSalaryBase)
            {
                session.SelectedTopRoleId = button - ButtonGovTopSelectBase;
                session.PreferBottomSelection = false;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                return true;
            }

            if (button >= ButtonGovSalaryBase && button < ButtonGovNominateBase)
            {
                int roleId = button - ButtonGovSalaryBase;
                ReinoCargoEntry role = ReinoEmploymentSystem.GetRole(m_CityId, roleId);
                if (role != null)
                {
                    if (session.EditingSalaryRoleId == roleId)
                    {
                        int newSalary = GetSalaryEntryValue(info, roleId, role.WeeklySalaryGold);
                        string message;
                        ReinoEmploymentSystem.UpdateRoleSalary(from, m_CityId, roleId, newSalary, out message);
                        from.SendMessage(message);
                        session.EditingSalaryRoleId = 0;
                    }
                    else
                    {
                        session.EditingSalaryRoleId = roleId;
                    }
                }

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                return true;
            }

            if (button >= ButtonGovNominateBase && button < ButtonGovExonerateBase)
            {
                int roleId = button - ButtonGovNominateBase;
                ReinoCargoEntry role = ReinoEmploymentSystem.GetRole(m_CityId, roleId);
                if (role != null)
                {
                    from.Target = new ReinoGovernmentNominationTarget(from, m_CityId, roleId);
                    from.SendMessage("Selecione o jogador que receberá o convite para o cargo de " + role.Title + ".");
                }
                return true;
            }

            if (button >= ButtonGovExonerateBase && button < ButtonGovRemoveEmptyBase)
            {
                int roleId = button - ButtonGovExonerateBase;
                string message;
                ReinoEmploymentSystem.RemoveRoleOccupant(from, m_CityId, roleId, true, out message);
                from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                return true;
            }

            if (button >= ButtonGovRemoveEmptyBase && button < ButtonGovTopPrev)
            {
                int roleId = button - ButtonGovRemoveEmptyBase;
                string message;
                ReinoEmploymentSystem.RemoveEmptyRole(from, m_CityId, roleId, out message);
                from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                return true;
            }

            switch (button)
            {
                case ButtonGovTopPrev:
                    session.TopPage--;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                    return true;
                case ButtonGovTopNext:
                    session.TopPage++;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                    return true;
                case ButtonGovShowAdd:
                    session.ShowAddList = !session.ShowAddList;
                    if (!session.ShowAddList)
                        session.PreferBottomSelection = false;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                    return true;
                case ButtonGovAddConfirm:
                    {
                        string message;
                        ReinoEmploymentSystem.AddRoleFromTemplate(from, m_CityId, session.SelectedBottomIndex, out message);
                        from.SendMessage(message);
                        from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                        return true;
                    }
                case ButtonGovAddRepresentative:
                    {
                        string message;
                        ReinoEmploymentSystem.SpawnRepresentative(from, m_CityId, out message);
                        from.SendMessage(message);
                        from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                        return true;
                    }
                case ButtonGovOpenCreate:
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;

                case ButtonGovBottomPrev:
                    session.BottomPage--;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                    return true;

                case ButtonGovBottomNext:
                    session.BottomPage++;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                    return true;

                case ButtonGovCreateHierarchyDown:
                    {
                        int h;
                        if (!Int32.TryParse(session.CreateHierarchy, out h))
                            h = 3;
                        session.CreateHierarchy = Math.Max(3, h - 1).ToString();
                        from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                        return true;
                    }
                case ButtonGovCreateHierarchyUp:
                    {
                        int h;
                        if (!Int32.TryParse(session.CreateHierarchy, out h))
                            h = 3;
                        session.CreateHierarchy = Math.Min(99, h + 1).ToString();
                        from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                        return true;
                    }
                case ButtonGovCreateToggleFinancial:
                    session.CreateCanFinancial = !session.CreateCanFinancial;
                    session.CreateInfoIsPermission = true;
                    session.CreateInfoKey = "financial";
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;
                case ButtonGovCreateToggleMilitary:
                    session.CreateCanMilitary = !session.CreateCanMilitary;
                    session.CreateInfoIsPermission = true;
                    session.CreateInfoKey = "military";
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;
                case ButtonGovCreateToggleHire:
                    session.CreateCanHire = !session.CreateCanHire;
                    session.CreateInfoIsPermission = true;
                    session.CreateInfoKey = "hire";
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;
                case ButtonGovCreateToggleFire:
                    session.CreateCanFire = !session.CreateCanFire;
                    session.CreateInfoIsPermission = true;
                    session.CreateInfoKey = "fire";
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;
                case ButtonGovCreateConstructionPrev:
                    session.SelectedConstructionPage--;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;
                case ButtonGovCreateConstructionNext:
                    session.SelectedConstructionPage++;
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                    return true;
                case ButtonGovCreateSubmit:
                    {
                        int salary;
                        int hierarchy;
                        if (!Int32.TryParse(session.CreateSalary, out salary))
                            salary = 0;
                        if (!Int32.TryParse(session.CreateHierarchy, out hierarchy))
                            hierarchy = 3;

                        string createdTitle = session.CreateName;
                        string message;
                        ReinoEmploymentSystem.CreateCustomRole(from, m_CityId, createdTitle, session.CreateDescription, salary, hierarchy, session.CreateCanFinancial, session.CreateCanMilitary, session.CreateCanHire, session.CreateCanFire, session.CreateLinkedConstructionKey, out message);
                        from.SendMessage(message);
                        if (message.IndexOf("sucesso", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            session.CreateName = String.Empty;
                            session.CreateSalary = "0";
                            session.CreateHierarchy = "3";
                            session.CreateDescription = String.Empty;
                            session.CreateLinkedConstructionKey = String.Empty;
                            session.CreateCanFinancial = false;
                            session.CreateCanMilitary = false;
                            session.CreateCanHire = false;
                            session.CreateCanFire = false;
                            session.ShowAddList = true;

                            List<ReinoCargoEntry> addable = ReinoEmploymentSystem.GetAddableRoleTemplates(m_CityId);
                            for (int i = 0; i < addable.Count; i++)
                            {
                                if (String.Equals(addable[i].Title, createdTitle, StringComparison.OrdinalIgnoreCase))
                                {
                                    session.SelectedBottomIndex = addable[i].RoleId;
                                    session.PreferBottomSelection = true;
                                    break;
                                }
                            }

                            from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                            return true;
                        }

                        from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                        return true;
                    }
            }

            if (button >= ButtonGovAddSelectBase && button < ButtonGovAddSelectBase + 10)
            {
                int visibleIndex = button - ButtonGovAddSelectBase;
                List<ReinoCargoEntry> options = ReinoEmploymentSystem.GetAddableRoleTemplates(m_CityId);
                int real = session.BottomPage * 4 + visibleIndex;

                if (real >= 0 && real < options.Count && options[real] != null)
                {
                    session.SelectedBottomIndex = options[real].RoleId;
                    session.PreferBottomSelection = true;
                }
                else
                {
                    session.PreferBottomSelection = false;
                }

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                return true;
            }

            if (button >= ButtonGovCreateConstructionSelectBase && button < ButtonGovCreateConstructionSelectBase + 10)
            {
                int visibleIndex = button - ButtonGovCreateConstructionSelectBase;
                List<ReinoConstructionRuntimeInfo> active = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);
                int real = session.SelectedConstructionPage * 6 + visibleIndex;
                if (real >= 0 && real < active.Count)
                {
                    session.CreateLinkedConstructionKey = active[real].Key;
                    session.CreateInfoIsPermission = false;
                    session.CreateInfoKey = active[real].Key;
                }

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 17));
                return true;
            }

            return false;
        }
    }

    public class ReinoGovernmentNominationTarget : Target
    {
        private readonly PlayerMobile m_Actor;
        private readonly int m_CityId;
        private readonly int m_RoleId;

        public ReinoGovernmentNominationTarget(PlayerMobile actor, int cityId, int roleId) : base(12, false, TargetFlags.None)
        {
            m_Actor = actor;
            m_CityId = cityId;
            m_RoleId = roleId;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            string message;
            Mobile mob = targeted as Mobile;

            if (!ReinoEmploymentSystem.CanNominateFromEmploymentPage(m_Actor, m_CityId, m_RoleId, mob, out message))
            {
                m_Actor.SendMessage(message);
                return;
            }

            PlayerMobile target = mob as PlayerMobile;
            ReinoCargoEntry role = ReinoEmploymentSystem.GetRole(m_CityId, m_RoleId);
            if (target == null || role == null)
                return;

            target.CloseGump(typeof(ReinoCargoInvitationGump));
            target.SendGump(new ReinoCargoInvitationGump(target, m_CityId, m_RoleId, m_Actor.Name, true));
            m_Actor.SendMessage(target.Name + " recebeu o convite para o cargo de " + role.Title + ".");
            target.SendMessage("Você recebeu um convite para o cargo de " + role.Title + ".");
        }
    }
}
