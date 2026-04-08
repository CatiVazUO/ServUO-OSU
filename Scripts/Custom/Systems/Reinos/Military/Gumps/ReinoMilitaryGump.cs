using System;
using System.Collections.Generic;
using System.Text;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Items;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonMilitaryTabLaws = 58000;
        private const int ButtonMilitaryTabGuards = 58001;
        private const int ButtonMilitaryTabRoutes = 58002;
        private const int ButtonMilitaryTabTraining = 58003;

        private const int ButtonWantedKill = 58010;
        private const int ButtonWantedArrest = 58011;
        private const int ButtonWantedAdd = 58012;
        private const int ButtonWantedRemove = 58013;

        private const int ButtonCrimeReport = 58020;
        private const int ButtonCrimeArrest = 58021;

        private const int ButtonLawBase = 58100;

        private const int ButtonGuardKindBase = 58200;
        private const int ButtonAddGuardPoint = 58220;
        private const int ButtonRemoveGuardPoint = 58221;
        private const int ButtonCycleFacing = 58222;
        private const int ButtonAddGuard = 58223;
        private const int ButtonMiniGump = 58224;

        private const int ButtonCreateRoutePoint = 58300;
        private const int ButtonLinkRoutePoints = 58301;
        private const int ButtonLinkRouteToGuard = 58302;
        private const int ButtonRemoveRoutePoint = 58303;
        private const int ButtonActivateRoute = 58304;
        private const int ButtonRouteSpeedShort = 58305;
        private const int ButtonRouteSpeedMedium = 58306;
        private const int ButtonRouteSpeedLong = 58307;
        private const int ButtonRouteSchedule = 58308;
        private const int ButtonResetRoute = 58309;
        private const int ButtonRevealRoutes = 58310;
        private const int ButtonResetRouteConfig = 58311;

        private const int ButtonTrainingBase = 58400;
        private const int ButtonTrainingPrev = 58490;
        private const int ButtonTrainingNext = 58491;

        private const int TextWantedAdd = 20;
        private const int TextWantedRemove = 21;

        private void BuildMilitaryPage()
        {
            AddPage(1);

            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(m_From);

            AddLabel(774, 173, 0, @"Militar");
            AddImageTiled(407, 261, 825, 5, 367);

            if (session.RestrictToBarracksView && session.Tab == ReinoMilitaryTab.Laws)
                session.Tab = ReinoMilitaryTab.Guards;

            if (!session.RestrictToBarracksView)
            {
                AddLabel(467, 229, 0, @"Leis");
                AddButton(440, 229, session.Tab == ReinoMilitaryTab.Laws ? 534 : 531, session.Tab == ReinoMilitaryTab.Laws ? 534 : 531, ButtonMilitaryTabLaws, GumpButtonType.Reply, 0);
            }

            AddLabel(679, 229, 0, @"Guardas");
            AddLabel(905, 229, 0, @"Rotas");
            AddLabel(1125, 229, 0, @"Treinamento");

            AddButton(651, 229, session.Tab == ReinoMilitaryTab.Guards ? 534 : 531, session.Tab == ReinoMilitaryTab.Guards ? 534 : 531, ButtonMilitaryTabGuards, GumpButtonType.Reply, 0);
            AddButton(877, 229, session.Tab == ReinoMilitaryTab.Routes ? 534 : 531, session.Tab == ReinoMilitaryTab.Routes ? 534 : 531, ButtonMilitaryTabRoutes, GumpButtonType.Reply, 0);
            AddButton(1097, 230, session.Tab == ReinoMilitaryTab.Training ? 534 : 531, session.Tab == ReinoMilitaryTab.Training ? 534 : 531, ButtonMilitaryTabTraining, GumpButtonType.Reply, 0);

            switch (session.Tab)
            {
                case ReinoMilitaryTab.Guards:
                    BuildMilitaryGuardsPage(session);
                    break;
                case ReinoMilitaryTab.Routes:
                    BuildMilitaryRoutesPage(session);
                    break;
                case ReinoMilitaryTab.Training:
                    BuildMilitaryTrainingPage(session);
                    break;
                default:
                    BuildMilitaryLawsPage(session);
                    break;
            }
        }

        private void BuildMilitaryLawsPage(ReinoMilitarySession session)
        {
            AddLabel(412, 302, 0, @"Ação Para Procurados:");
            AddButton(577, 302, session.SelectedWantedAction == ReinoGuardAction.Kill ? 530 : 531, session.SelectedWantedAction == ReinoGuardAction.Kill ? 530 : 531, ButtonWantedKill, GumpButtonType.Reply, 0);
            AddLabel(603, 303, 0, @"Matar");

            if (ReinoMilitarySystem.HasPrison(m_CityId))
            {
                AddButton(663, 302, session.SelectedWantedAction == ReinoGuardAction.Arrest ? 530 : 531, session.SelectedWantedAction == ReinoGuardAction.Arrest ? 530 : 531, ButtonWantedArrest, GumpButtonType.Reply, 0);
                AddLabel(689, 303, 0, @"Prender");
            }

            AddLabel(412, 351, 0, @"Adicionar Por Nome:");
            AddTextEntry(553, 346, 200, 20, 0, TextWantedAdd, String.Empty);
            AddButton(767, 346, 531, 531, ButtonWantedAdd, GumpButtonType.Reply, 0);

            AddLabel(412, 379, 0, @"Remover Por Nome:");
            AddTextEntry(553, 376, 200, 20, 0, TextWantedRemove, String.Empty);
            AddButton(767, 376, 531, 531, ButtonWantedRemove, GumpButtonType.Reply, 0);

            AddLabel(963, 275, 0, @"Lista de Procurados");
            AddHtml(806, 301, 416, 107, ReinoMilitarySystem.GetWantedHtml(m_CityId), false, false);

            AddLabel(429, 438, 0, @"Ação Para Atos Criminais:");
            ReinoMilitaryPolicy policy = ReinoMilitarySystem.GetPolicy(m_CityId);
            AddButton(690, 437, policy.CrimeDefaultAction == ReinoGuardAction.Report ? 530 : 531, policy.CrimeDefaultAction == ReinoGuardAction.Report ? 530 : 531, ButtonCrimeReport, GumpButtonType.Reply, 0);
            AddLabel(716, 438, 0, @"Reportar");

            if (ReinoMilitarySystem.HasPrison(m_CityId))
            {
                AddButton(822, 437, policy.CrimeDefaultAction == ReinoGuardAction.Arrest ? 530 : 531, policy.CrimeDefaultAction == ReinoGuardAction.Arrest ? 530 : 531, ButtonCrimeArrest, GumpButtonType.Reply, 0);
                AddLabel(848, 438, 0, @"Prender");
            }

            AddImageTiled(405, 419, 825, 5, 367);
            AddImageTiled(405, 473, 825, 5, 367);
            AddImageTiled(405, 525, 825, 5, 367);

            AddLabel(702, 493, 0, @"Considerar Atos Criminais");
            AddHtml(420, 270, 360, 22, "<BASEFONT COLOR=#000000>" + ReinoMilitarySystem.GetLawSummaryHtml(m_CityId) + "</BASEFONT>", false, false);

            ReinoMilitaryLaw[] left = new ReinoMilitaryLaw[]
            {
                ReinoMilitaryLaw.HoodedWalk,
                ReinoMilitaryLaw.Stealing,
                ReinoMilitaryLaw.Snooping,
                ReinoMilitaryLaw.LootKnockedOut,
                ReinoMilitaryLaw.Lockpicking
            };

            ReinoMilitaryLaw[] middle = new ReinoMilitaryLaw[]
            {
                ReinoMilitaryLaw.Fighting,
                ReinoMilitaryLaw.AnimalTaming,
                ReinoMilitaryLaw.AnimalKilling,
                ReinoMilitaryLaw.ForeignPlanting,
                ReinoMilitaryLaw.ForeignHarvesting
            };

            ReinoMilitaryLaw[] right = new ReinoMilitaryLaw[]
            {
                ReinoMilitaryLaw.DrugUse,
                ReinoMilitaryLaw.DrunkWalk,
                ReinoMilitaryLaw.TakingFruit,
                ReinoMilitaryLaw.FenceJumping,
                ReinoMilitaryLaw.ArmedWalk
            };

            int[] y = new int[] { 553, 581, 611, 641, 671 };
            for (int i = 0; i < left.Length; i++)
            {
                AddButton(415, y[i], ReinoMilitarySystem.IsLawEnabled(m_CityId, left[i]) ? 530 : 531, ReinoMilitarySystem.IsLawEnabled(m_CityId, left[i]) ? 530 : 531, ButtonLawBase + (int)left[i], GumpButtonType.Reply, 0);
                AddLabel(443, y[i] + 2, 0, ReinoMilitarySystem.GetLawLabel(left[i]));
            }

            for (int i = 0; i < middle.Length; i++)
            {
                AddButton(704, y[i], ReinoMilitarySystem.IsLawEnabled(m_CityId, middle[i]) ? 530 : 531, ReinoMilitarySystem.IsLawEnabled(m_CityId, middle[i]) ? 530 : 531, ButtonLawBase + (int)middle[i], GumpButtonType.Reply, 0);
                AddLabel(734, y[i] + 2, 0, ReinoMilitarySystem.GetLawLabel(middle[i]));
            }

            for (int i = 0; i < right.Length; i++)
            {
                AddButton(1042, y[i], ReinoMilitarySystem.IsLawEnabled(m_CityId, right[i]) ? 530 : 531, ReinoMilitarySystem.IsLawEnabled(m_CityId, right[i]) ? 530 : 531, ButtonLawBase + (int)right[i], GumpButtonType.Reply, 0);
                AddLabel(1071, y[i] + 2, 0, ReinoMilitarySystem.GetLawLabel(right[i]));
            }
        }

        private void BuildMilitaryGuardsPage(ReinoMilitarySession session)
        {
            AddLabel(740, 285, 0, @"Adicionar Guardas");
            AddImageTiled(406, 317, 825, 5, 367);
            AddImageTiled(584, 338, 6, 357, 365);
            AddLabel(443, 335, 0, @"Tipos de Guarda");
            AddImageTiled(1018, 337, 6, 357, 365);
            AddLabel(1087, 337, 0, @"Por Guarda / Semana");
            AddLabel(1087, 523, 0, @"Total Semanal do Quartel");
            AddImageTiled(1049, 366, 183, 5, 367);
            AddImageTiled(1047, 553, 183, 5, 367);

            ReinoGuardKind[] kinds = new ReinoGuardKind[]
            {
                ReinoGuardKind.Vigia,
                ReinoGuardKind.Rua,
                ReinoGuardKind.Armado,
                ReinoGuardKind.Arqueiro,
                ReinoGuardKind.CavalariaArmada,
                ReinoGuardKind.CavalariaArqueira,
                ReinoGuardKind.Oficial
            };

            int[] y = new int[] { 386, 416, 446, 476, 536, 566, 621 };
            for (int i = 0; i < kinds.Length; i++)
            {
                bool selected = session.SelectedGuardKind == kinds[i];
                AddButton(406, y[i], selected ? 530 : 531, selected ? 530 : 531, ButtonGuardKindBase + (int)kinds[i], GumpButtonType.Reply, 0);
                AddLabel(436, y[i] + 1, 0, ReinoMilitarySystem.GetGuardKindLabel(kinds[i]));
            }

            AddHtml(615, 345, 386, 166, ReinoMilitarySystem.GetGuardDescriptionHtml(session.SelectedGuardKind), false, true);

            int hireGold, hireCloth, hireIron, hireWood, wkGold, wkCloth, wkIron, wkWood;
            ReinoMilitarySystem.GetGuardCosts(session.SelectedGuardKind, out hireGold, out hireCloth, out hireIron, out hireWood, out wkGold, out wkCloth, out wkIron, out wkWood);
            int totalGold, totalCloth, totalIron, totalWood;
            ReinoMilitarySystem.GetTotalWeeklyGuardCost(m_CityId, out totalGold, out totalCloth, out totalIron, out totalWood);

            AddLabel(1050, 390, 0, @"Moedas:"); AddLabel(1130, 390, 0, wkGold.ToString());
            AddLabel(1050, 418, 0, @"Tecidos:"); AddLabel(1130, 418, 0, wkCloth.ToString());
            AddLabel(1050, 448, 0, @"Ferro:"); AddLabel(1130, 448, 0, wkIron.ToString());
            AddLabel(1050, 479, 0, @"Madeira:"); AddLabel(1130, 479, 0, wkWood.ToString());

            AddLabel(1050, 577, 0, @"Moedas:"); AddLabel(1130, 577, 0, totalGold + " (+" + wkGold + ")");
            AddLabel(1050, 605, 0, @"Tecidos:"); AddLabel(1130, 605, 0, totalCloth + " (+" + wkCloth + ")");
            AddLabel(1050, 635, 0, @"Ferro:"); AddLabel(1130, 635, 0, totalIron + " (+" + wkIron + ")");
            AddLabel(1050, 666, 0, @"Madeira:"); AddLabel(1130, 666, 0, totalWood + " (+" + wkWood + ")");

            AddLabel(668, 534, 0, @"Adicionar Ponto de Guarda");
            AddLabel(668, 561, 0, @"Remover Ponto de Guarda");
            AddLabel(668, 592, 0, @"Direção");
            AddLabel(740, 592, 0, ReinoMilitarySystem.GetFacingLabel(session.FacingIndex));
            AddLabel(668, 622, 0, @"Adicionar Guarda");
            AddLabel(668, 670, 0, @"Gump Mini");
            AddButton(638, 533, 531, 531, ButtonAddGuardPoint, GumpButtonType.Reply, 0);
            AddButton(638, 563, 531, 531, ButtonRemoveGuardPoint, GumpButtonType.Reply, 0);
            AddButton(638, 593, 531, 531, ButtonCycleFacing, GumpButtonType.Reply, 0);
            AddButton(638, 623, 531, 531, ButtonAddGuard, GumpButtonType.Reply, 0);
            AddButton(638, 671, 531, 531, ButtonMiniGump, GumpButtonType.Reply, 0);
        }

        private void BuildMilitaryRoutesPage(ReinoMilitarySession session)
        {
            AddLabel(740, 285, 0, @"Criar Rotas");
            AddImageTiled(406, 317, 825, 5, 367);

            StringBuilder html = new StringBuilder();
            html.Append("<BASEFONT COLOR=#000000>");
            html.Append("Crie um ponto de guarda primeiro. Depois vá passando pelos locais onde o guarda deve andar e crie pontos de rota. ");
            html.Append("Use <B>ligar pontos de rota</B> em um ponto, depois em outro, para formar a sequência. Em seguida fique sobre o ponto de guarda e clique em <B>ligar a um ponto de guarda</B>.<BR><BR>");
            html.Append("<B>Selecionar rota</B> torna pontos próximos visíveis por 1 minuto. <B>Resetar rota</B> devolve o padrão: tempo curto e rota infinita.<BR><BR>");
            html.Append("<B>Velocidade atual:</B> ").Append(ReinoMilitarySystem.GetRouteSpeedLabel(session.SelectedRouteSpeed));
            html.Append("<BR><B>Agendamento atual:</B> ").Append(ReinoMilitarySystem.GetRouteScheduleLabel(session.SelectedRouteSchedule));
            html.Append("</BASEFONT>");

            AddHtml(425, 341, 786, 168, html.ToString(), false, true);

            AddLabel(479, 540, 0, @"Criar Ponto de Rota");
            AddLabel(479, 567, 0, @"Ligar Pontos de Rota");
            AddLabel(479, 598, 0, @"Ligar a Um Ponto de Guarda");
            AddLabel(479, 628, 0, @"Remover Ponto de Rota");
            AddButton(449, 539, 531, 531, ButtonCreateRoutePoint, GumpButtonType.Reply, 0);
            AddButton(449, 569, 531, 531, ButtonLinkRoutePoints, GumpButtonType.Reply, 0);
            AddButton(449, 599, 531, 531, ButtonLinkRouteToGuard, GumpButtonType.Reply, 0);
            AddButton(449, 629, 531, 531, ButtonRemoveRoutePoint, GumpButtonType.Reply, 0);
            AddLabel(479, 659, 0, @"Acionar Rota");
            AddButton(449, 660, 531, 531, ButtonActivateRoute, GumpButtonType.Reply, 0);

            AddLabel(764, 540, 0, @"Tempo de Rota Curto");
            AddLabel(764, 567, 0, @"Tempo de Rota Médio");
            AddLabel(764, 598, 0, @"Tempo de Rota Longo");
            AddButton(734, 539, session.SelectedRouteSpeed == ReinoRouteSpeed.Short ? 530 : 531, session.SelectedRouteSpeed == ReinoRouteSpeed.Short ? 530 : 531, ButtonRouteSpeedShort, GumpButtonType.Reply, 0);
            AddButton(734, 569, session.SelectedRouteSpeed == ReinoRouteSpeed.Medium ? 530 : 531, session.SelectedRouteSpeed == ReinoRouteSpeed.Medium ? 530 : 531, ButtonRouteSpeedMedium, GumpButtonType.Reply, 0);
            AddButton(734, 599, session.SelectedRouteSpeed == ReinoRouteSpeed.Long ? 530 : 531, session.SelectedRouteSpeed == ReinoRouteSpeed.Long ? 530 : 531, ButtonRouteSpeedLong, GumpButtonType.Reply, 0);

            AddLabel(1076, 544, 0, @"Rota Por Tempo");
            AddLabel(1076, 560, 0, ReinoMilitarySystem.GetRouteScheduleLabel(session.SelectedRouteSchedule));
            AddButton(1046, 545, 531, 531, ButtonRouteSchedule, GumpButtonType.Reply, 0);
            AddLabel(1076, 575, 0, @"Resetar Rota");
            AddButton(1046, 576, 531, 531, ButtonResetRoute, GumpButtonType.Reply, 0);
            AddLabel(766, 658, 0, @"Mostrar/Ocultar Pontos");
            AddButton(736, 659, 531, 531, ButtonRevealRoutes, GumpButtonType.Reply, 0);
            AddLabel(1076, 606, 0, @"Resetar Config de Rota");
            AddButton(1046, 607, 531, 531, ButtonResetRouteConfig, GumpButtonType.Reply, 0);
            AddImageTiled(680, 534, 6, 154, 365);
            AddImageTiled(990, 536, 6, 154, 365);
        }

        private void BuildMilitaryTrainingPage(ReinoMilitarySession session)
        {
            AddLabel(739, 282, 0, @"Área de Treinamento");
            AddImageTiled(406, 317, 825, 5, 367);
            AddImageTiled(406, 357, 825, 5, 367);
            AddImageTiled(406, 397, 825, 5, 367);
            AddImageTiled(406, 437, 825, 5, 367);
            AddImageTiled(406, 477, 825, 5, 367);
            AddImageTiled(406, 517, 825, 5, 367);
            AddImageTiled(406, 557, 825, 5, 367);
            AddImageTiled(574, 321, 6, 240, 365);
            AddImageTiled(739, 321, 6, 240, 365);
            AddImageTiled(874, 321, 6, 238, 365);
            AddImageTiled(975, 321, 6, 240, 365);
            AddImageTiled(1105, 321, 6, 240, 365);

            AddLabel(417, 334, 0, @"Nome do Guarda");
            AddLabel(589, 334, 0, @"Tipo de Guarda");
            AddLabel(758, 333, 0, @"Localização");
            AddLabel(895, 333, 0, @"Nível");
            AddLabel(995, 333, 0, @"Custo ");
            AddLabel(1125, 332, 0, @"Iniciar");

            List<ReinoGuardPostInfo> entries = ReinoMilitarySystem.GetTrainingEntries(m_CityId);
            int start = session.PendingTrainingPage * 5;
            int[] rowY = new int[] { 370, 407, 446, 486, 527 };

            for (int i = 0; i < 5; i++)
            {
                int real = start + i;
                if (real >= entries.Count)
                    break;

                ReinoGuardPostInfo post = entries[real];
                OSUCityGuard guard = ReinoMilitarySystem.FindGuard(post);
                string guardName = post.Training ? ("Posto #" + post.Id + " - Em treinamento") : (guard != null ? ("Posto #" + post.Id + " - " + guard.Name) : ("Posto #" + post.Id + " - Sem guarda"));
                string location = post.Location.X + ", " + post.Location.Y;
                int cost = ReinoMilitarySystem.GetGuardTrainingCost(post.GuardKind, post.Level);

                AddLabel(417, rowY[i], 0, guardName);
                AddLabel(589, rowY[i], 0, ReinoMilitarySystem.GetGuardKindLabel(post.GuardKind));
                AddLabel(758, rowY[i], 0, location);
                AddLabel(895, rowY[i], 0, post.Level.ToString());
                AddLabel(995, rowY[i], 0, cost.ToString());

                if (!post.Training)
                {
                    AddLabel(1157, rowY[i], 0, @"Treinar");
                    AddButton(1127, rowY[i], 531, 531, ButtonTrainingBase + post.Id, GumpButtonType.Reply, 0);
                }
                else
                {
                    AddLabel(1125, rowY[i], 33, @"Treinando");
                }
            }

            AddButton(724, 673, 498, 498, ButtonTrainingPrev, GumpButtonType.Reply, 0);
            AddButton(837, 673, 499, 499, ButtonTrainingNext, GumpButtonType.Reply, 0);

            AddHtml(416, 580, 808, 75,
                "<BASEFONT COLOR=#000000>Durante os testes, quando um guarda começa o treinamento ele some do posto por <B>1 minuto</B>. Ao voltar, ganha atributos e skills melhores, mas não muda de tipo. O nível máximo é <B>5</B>.</BASEFONT>",
                false, false);
        }

        private bool HandleMilitaryResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;
            ReinoMilitarySession session = ReinoMilitarySystem.GetSession(from);

            if (button == ButtonMilitaryTabLaws || button == ButtonMilitaryTabGuards || button == ButtonMilitaryTabRoutes || button == ButtonMilitaryTabTraining)
            {
                bool canOpen = session.RestrictToBarracksView
                    ? ReinoMilitarySystem.CanAccessBarracksSubGump(from, m_CityId)
                    : ReinoMilitarySystem.CanAccessMilitaryGovernmentPage(from, m_CityId);

                if (!canOpen)
                {
                    from.SendMessage("Você não tem acesso militar a esta página.");
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 5));
                    return true;
                }

                if (button == ButtonMilitaryTabLaws)
                {
                    if (session.RestrictToBarracksView)
                        session.Tab = ReinoMilitaryTab.Guards;
                    else
                        session.Tab = ReinoMilitaryTab.Laws;
                }
                else if (button == ButtonMilitaryTabGuards) session.Tab = ReinoMilitaryTab.Guards;
                else if (button == ButtonMilitaryTabRoutes) session.Tab = ReinoMilitaryTab.Routes;
                else session.Tab = ReinoMilitaryTab.Training;

                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 8));
                return true;
            }

            if (button == ButtonWantedKill)
            {
                session.SelectedWantedAction = ReinoGuardAction.Kill;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonWantedArrest)
            {
                if (ReinoMilitarySystem.HasPrison(m_CityId))
                    session.SelectedWantedAction = ReinoGuardAction.Arrest;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonCrimeReport || button == ButtonCrimeArrest)
            {
                if (!ReinoMilitarySystem.CanManageWantedList(from, m_CityId))
                    from.SendMessage("Você não pode mudar a política militar do reino.");
                else if (button == ButtonCrimeArrest && !ReinoMilitarySystem.HasPrison(m_CityId))
                    from.SendMessage("É preciso ter uma prisão construída para usar a ação de prender.");
                else
                    ReinoMilitarySystem.GetPolicy(m_CityId).CrimeDefaultAction = (button == ButtonCrimeReport ? ReinoGuardAction.Report : ReinoGuardAction.Arrest);

                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonWantedAdd)
            {
                TextRelay relay = info.GetTextEntry(TextWantedAdd);
                from.SendMessage(ReinoMilitarySystem.AddWanted(from, m_CityId, relay != null ? relay.Text : String.Empty, session.SelectedWantedAction));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonWantedRemove)
            {
                TextRelay relay = info.GetTextEntry(TextWantedRemove);
                from.SendMessage(ReinoMilitarySystem.RemoveWanted(from, m_CityId, relay != null ? relay.Text : String.Empty));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button >= ButtonLawBase && button < ButtonLawBase + 100)
            {
                ReinoMilitaryLaw law = (ReinoMilitaryLaw)(button - ButtonLawBase);
                if (!ReinoMilitarySystem.CanManageWantedList(from, m_CityId))
                    from.SendMessage("Você não pode mudar as leis militares do reino.");
                else
                    ReinoMilitarySystem.ToggleLaw(m_CityId, law);

                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button >= ButtonGuardKindBase && button < ButtonGuardKindBase + 20)
            {
                session.SelectedGuardKind = (ReinoGuardKind)(button - ButtonGuardKindBase);
                session.Tab = ReinoMilitaryTab.Guards;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonAddGuardPoint)
            {
                Direction d = ReinoMilitarySystem.GetFacingByIndex(session.FacingIndex);
                from.SendMessage(ReinoMilitarySystem.AddGuardPost(from, m_CityId, d));
                session.Tab = ReinoMilitaryTab.Guards;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonRemoveGuardPoint)
            {
                from.SendMessage(ReinoMilitarySystem.RemoveGuardPost(from, m_CityId));
                session.Tab = ReinoMilitaryTab.Guards;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonCycleFacing)
            {
                session.FacingIndex = (session.FacingIndex + 1) % 4;
                session.Tab = ReinoMilitaryTab.Guards;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonAddGuard)
            {
                Direction d = ReinoMilitarySystem.GetFacingByIndex(session.FacingIndex);
                from.SendMessage(ReinoMilitarySystem.AddGuardToCurrentPost(from, m_CityId, session.SelectedGuardKind, d));
                session.Tab = ReinoMilitaryTab.Guards;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonMiniGump)
            {
                from.SendGump(new ReinoMilitaryMiniGump(from, m_CityId, ReinoMilitaryTab.Guards));
                return true;
            }

            if (button == ButtonCreateRoutePoint)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                from.SendMessage(ReinoMilitarySystem.CreateRoutePoint(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonLinkRoutePoints)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                from.SendMessage(ReinoMilitarySystem.LinkRoutePoint(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonLinkRouteToGuard)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                from.SendMessage(ReinoMilitarySystem.LinkRouteToGuardPost(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonRemoveRoutePoint)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                from.SendMessage(ReinoMilitarySystem.RemoveRoutePoint(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonActivateRoute)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                from.SendMessage(ReinoMilitarySystem.ActivateRouteAtCurrentPoint(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonRouteSpeedShort || button == ButtonRouteSpeedMedium || button == ButtonRouteSpeedLong)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                session.SelectedRouteSpeed = button == ButtonRouteSpeedShort ? ReinoRouteSpeed.Short : button == ButtonRouteSpeedMedium ? ReinoRouteSpeed.Medium : ReinoRouteSpeed.Long;
                from.SendMessage(ReinoMilitarySystem.SetRouteSpeedAtCurrentPoint(from, m_CityId, session.SelectedRouteSpeed));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonRouteSchedule)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                session.SelectedRouteSchedule = NextRouteSchedule(session.SelectedRouteSchedule);
                from.SendMessage(ReinoMilitarySystem.SetRouteScheduleAtCurrentPoint(from, m_CityId, session.SelectedRouteSchedule));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonResetRoute)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                session.SelectedRouteSchedule = ReinoRouteSchedule.Infinite;
                session.SelectedRouteSpeed = ReinoRouteSpeed.Short;
                from.SendMessage(ReinoMilitarySystem.ResetRouteAtCurrentPoint(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonResetRouteConfig)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                session.SelectedRouteSchedule = ReinoRouteSchedule.Infinite;
                session.SelectedRouteSpeed = ReinoRouteSpeed.Short;
                from.SendMessage(ReinoMilitarySystem.ResetRouteConfigAtCurrentPoint(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }
            if (button == ButtonRevealRoutes)
            {
                session.Tab = ReinoMilitaryTab.Routes;
                from.SendMessage(ReinoMilitarySystem.RevealRoutePoints(from, m_CityId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button >= ButtonTrainingBase && button < ButtonTrainingBase + 80)
            {
                session.Tab = ReinoMilitaryTab.Training;
                int postId = button - ButtonTrainingBase;
                from.SendMessage(ReinoMilitarySystem.StartGuardTraining(from, m_CityId, postId));
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonTrainingPrev)
            {
                session.Tab = ReinoMilitaryTab.Training;
                if (session.PendingTrainingPage > 0)
                    session.PendingTrainingPage--;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            if (button == ButtonTrainingNext)
            {
                session.Tab = ReinoMilitaryTab.Training;
                session.PendingTrainingPage++;
                from.SendGump(new ReinoExpansionGump(from, m_CityId, -1, -1, String.Empty, 0, 8));
                return true;
            }

            return false;
        }

        private static ReinoRouteSchedule NextRouteSchedule(ReinoRouteSchedule current)
        {
            switch (current)
            {
                case ReinoRouteSchedule.Every15Minutes: return ReinoRouteSchedule.Every30Minutes;
                case ReinoRouteSchedule.Every30Minutes: return ReinoRouteSchedule.Every45Minutes;
                case ReinoRouteSchedule.Every45Minutes: return ReinoRouteSchedule.Every60Minutes;
                case ReinoRouteSchedule.Every60Minutes: return ReinoRouteSchedule.DawnOnly;
                case ReinoRouteSchedule.DawnOnly: return ReinoRouteSchedule.Infinite;
                default: return ReinoRouteSchedule.Every15Minutes;
            }
        }
    }
}
