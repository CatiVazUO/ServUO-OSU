using System;
using System.Collections.Generic;
using Server.Custom.Systems.Postos;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonDiplomacyTargetBase = 60000;
        private const int ButtonDiplomacyRelationBase = 60100;
        private const int ButtonDiplomacyActionBase = 60200;
        private const int ButtonDiplomacyConfirm = 60300;
        private const int ButtonDiplomacyCancelAgreement = 60301;
        private const int ButtonDiplomacyPostoBase = 60400;
        private const int ButtonDiplomacyBorderBase = 60500;
        private const int ButtonDiplomacyBlockadeBase = 60600;
        private const int ButtonDiplomacyTributeFreqBase = 60700;

        private const int EntryDonateGold = 100;
        private const int EntryDonateWood = 101;
        private const int EntryDonateIron = 102;
        private const int EntryDonateCloth = 103;
        private const int EntryAgreementSendGold = 110;
        private const int EntryAgreementSendWood = 111;
        private const int EntryAgreementSendIron = 112;
        private const int EntryAgreementSendCloth = 113;
        private const int EntryAgreementReceiveGold = 120;
        private const int EntryAgreementReceiveWood = 121;
        private const int EntryAgreementReceiveIron = 122;
        private const int EntryAgreementReceiveCloth = 123;
        private const int EntryTributeGold = 130;
        private const int EntryTributeWood = 131;
        private const int EntryTributeIron = 132;
        private const int EntryTributeCloth = 133;

        private void BuildDiplomacyPage()
        {
            AddPage(1);

            ReinoDiplomacySession session = ReinoDiplomacySystem.GetSession(m_From, m_CityId);
            List<int> otherCities = ReinoDiplomacySystem.GetOtherCityIds(m_CityId);
            if (session.TargetCityId < 0 && otherCities.Count > 0)
                ReinoDiplomacySystem.ResetSessionSelection(m_From, m_CityId, otherCities[0]);

            session = ReinoDiplomacySystem.GetSession(m_From, m_CityId);
            int targetCityId = session.TargetCityId;
            ReinoDiplomacyRelationStatus currentRelation = targetCityId >= 0 ? ReinoDiplomacySystem.GetRelation(m_CityId, targetCityId) : ReinoDiplomacyRelationStatus.Neutral;
            ReinoDiplomacyRelationStatus shownRelation = session.DraftRelation ?? currentRelation;

            AddLabel(790, 173, 0, @"Diplomacia");
            AddImageTiled(407, 261, 825, 5, 367);
            AddImageTiled(596, 278, 6, 175, 365);
            AddImageTiled(407, 471, 825, 5, 367);
            AddImageTiled(407, 521, 825, 5, 367);

            int[] topXs = new int[] { 440, 769, 1105 };
            int[] topLabelXs = new int[] { 467, 797, 1133 };

            for (int i = 0; i < otherCities.Count && i < 3; i++)
            {
                int cityId = otherCities[i];
                bool selected = cityId == targetCityId;
                int art = selected ? 528 : 531;
                AddButton(topXs[i], 229, art, art, ButtonDiplomacyTargetBase + cityId, GumpButtonType.Reply, 0);
                AddLabel(topLabelXs[i], 229, 0, ReinoElectionsSystem.GetCityPeopleName(cityId));
            }

            DrawDiplomacyStatusButtons(currentRelation, shownRelation);
            DrawDiplomacyActions(shownRelation, session.SelectedAction);

            List<ReinoDiplomacyActionKind> shownActions = ReinoDiplomacySystem.GetAvailableActionsForRelation(shownRelation);

            string infoHtml = targetCityId >= 0
                ? (session.SelectedAction != ReinoDiplomacyActionKind.None && shownActions.Contains(session.SelectedAction)
                    ? ReinoDiplomacySystem.BuildActionDescription(m_CityId, targetCityId, session.SelectedAction)
                    : ReinoDiplomacySystem.BuildStatusDescription(m_CityId, targetCityId, shownRelation))
                : "<BASEFONT COLOR=#000000>Selecione um reino para iniciar a diplomacia.</BASEFONT>";

            AddHtml(808, 279, 407, 171, infoHtml, false, false);

            if (targetCityId < 0)
                return;

            if (shownRelation != currentRelation)
            {
                AddLabel(662, 576, 0, "Alterar a relação com " + ReinoElectionsSystem.GetCityPeopleName(targetCityId) + " para " + ReinoDiplomacySystem.GetRelationLabel(shownRelation) + "?");
                AddButton(773, 615, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
                return;
            }

            if (session.SelectedAction == ReinoDiplomacyActionKind.None || !shownActions.Contains(session.SelectedAction))
                return;

            switch (session.SelectedAction)
            {
                case ReinoDiplomacyActionKind.DonateResources:
                    DrawDonateResourcesSection(session);
                    break;
                case ReinoDiplomacyActionKind.DonatePosto:
                    DrawDonatePostoSection(session);
                    break;
                case ReinoDiplomacyActionKind.ProposeAgreement:
                    DrawAgreementSection(session);
                    break;
                case ReinoDiplomacyActionKind.CloseBorders:
                    DrawBordersSection(session, targetCityId);
                    break;
                case ReinoDiplomacyActionKind.CommercialBlockade:
                    DrawBlockadeSection(session);
                    break;
                case ReinoDiplomacyActionKind.DemandTribute:
                    DrawTributeSection(session);
                    break;
            }
        }

        private void DrawDiplomacyStatusButtons(ReinoDiplomacyRelationStatus currentRelation, ReinoDiplomacyRelationStatus shownRelation)
        {
            AddLabel(455, 298, 0, @"Aliados");
            AddLabel(455, 337, 0, @"Neutro");
            AddLabel(455, 374, 0, @"Inimigo");
            AddLabel(455, 413, 0, @"Guerra");

            AddDiplomacyStatusButton(510, 297, ReinoDiplomacyRelationStatus.Allied, currentRelation, shownRelation);
            AddDiplomacyStatusButton(510, 336, ReinoDiplomacyRelationStatus.Neutral, currentRelation, shownRelation);
            AddDiplomacyStatusButton(510, 373, ReinoDiplomacyRelationStatus.Enemy, currentRelation, shownRelation);
            AddDiplomacyStatusButton(510, 412, ReinoDiplomacyRelationStatus.War, currentRelation, shownRelation);
        }

        private void AddDiplomacyStatusButton(int x, int y, ReinoDiplomacyRelationStatus thisStatus, ReinoDiplomacyRelationStatus currentRelation, ReinoDiplomacyRelationStatus shownRelation)
        {
            int normalId = 531;
            int pressedId = 531;

            if (thisStatus == currentRelation)
            {
                normalId = (thisStatus == ReinoDiplomacyRelationStatus.War) ? 534 : 530;
                pressedId = normalId;
            }
            else if (thisStatus == shownRelation)
            {
                normalId = 528;
                pressedId = 528;
            }

            AddButton(x, y, normalId, pressedId, ButtonDiplomacyRelationBase + (int)thisStatus, GumpButtonType.Reply, 0);
        }

        private void DrawDiplomacyActions(ReinoDiplomacyRelationStatus shownRelation, ReinoDiplomacyActionKind selectedAction)
        {
            List<ReinoDiplomacyActionKind> actions = ReinoDiplomacySystem.GetAvailableActionsForRelation(shownRelation);
            int[] ys = new int[] { 296, 321, 346, 370, 395, 420 };

            for (int i = 0; i < actions.Count && i < ys.Length; i++)
            {
                ReinoDiplomacyActionKind action = actions[i];
                int normalId = selectedAction == action ? 536 : 437;
                int pressedId = normalId;

                AddButton(643, ys[i], normalId, pressedId, ButtonDiplomacyActionBase + (int)action, GumpButtonType.Reply, 0);
                AddLabel(667, ys[i] - 3, 0, ReinoDiplomacySystem.GetActionLabel(action));
            }
        }

        private void DrawDonateResourcesSection(ReinoDiplomacySession session)
        {
            ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(m_CityId);

            AddLabel(772, 492, 0, @"Doar Recursos");
            AddLabel(430, 567, 0, @"Moedas:");
            AddLabel(430, 595, 0, @"Madeira:");
            AddLabel(430, 625, 0, @"Ferro:");
            AddLabel(430, 655, 0, @"Tecido:");
            AddTextEntry(516, 562, 200, 20, 0, EntryDonateGold, session.DraftDonation.Gold.ToString());
            AddTextEntry(516, 592, 200, 20, 0, EntryDonateWood, session.DraftDonation.Wood.ToString());
            AddTextEntry(515, 622, 200, 20, 0, EntryDonateIron, session.DraftDonation.Iron.ToString());
            AddTextEntry(516, 652, 200, 20, 0, EntryDonateCloth, session.DraftDonation.Cloth.ToString());
            AddImageTiled(806, 536, 6, 158, 365);
            AddImageTiled(1051, 536, 6, 158, 365);
            AddImageTiled(818, 558, 229, 5, 367);
            AddLabel(903, 534, 0, @"Tesouro");
            AddLabel(858, 578, 0, "Moedas: " + (ledger != null ? ledger.Gold : 0));
            AddLabel(858, 606, 0, "Tecidos: " + (ledger != null ? ledger.Cloth : 0));
            AddLabel(858, 636, 0, "Ferro: " + (ledger != null ? ledger.Iron : 0));
            AddLabel(858, 667, 0, "Madeira: " + (ledger != null ? ledger.Wood : 0));
            AddButton(1105, 593, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
        }

        private void DrawDonatePostoSection(ReinoDiplomacySession session)
        {
            List<PostoDefinition> list = ReinoDiplomacySystem.GetDonatablePostos(m_CityId);
            AddLabel(791, 493, 0, @"Doar Posto");

            int[] leftY = new int[] { 555, 580, 605, 629, 654 };
            int[] rightY = new int[] { 554, 579, 604, 628, 653 };

            for (int i = 0; i < list.Count && i < 10; i++)
            {
                int xButton = i < 5 ? 428 : 646;
                int xLabel = i < 5 ? 452 : 670;
                int y = i < 5 ? leftY[i] : rightY[i - 5];
                PostoDefinition def = list[i];
                bool selected = String.Equals(def.Id, session.SelectedPostoId, StringComparison.OrdinalIgnoreCase);
                int art = selected ? 528 : 531;
                AddButton(xButton, y, art, art, ButtonDiplomacyPostoBase + i, GumpButtonType.Reply, 0);
                AddLabel(xLabel, y - 3, 0, def.Name);
            }

            AddButton(1105, 593, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
        }

        private void DrawAgreementSection(ReinoDiplomacySession session)
        {
            ReinoDiplomacyAgreement agreement = ReinoDiplomacySystem.GetAgreement(m_CityId, session.TargetCityId);

            AddLabel(750, 490, 0, @"Estabelecer Acordo");
            AddLabel(546, 536, 0, @"Enviar");
            AddLabel(877, 535, 0, @"Receber");
            AddImageTiled(723, 536, 6, 158, 365);
            AddImageTiled(1051, 536, 6, 158, 365);

            AddLabel(419, 575, 0, @"Moedas:");
            AddLabel(419, 603, 0, @"Madeira:");
            AddLabel(419, 633, 0, @"Ferro:");
            AddLabel(419, 663, 0, @"Tecido:");
            AddTextEntry(503, 570, 100, 20, 0, EntryAgreementSendGold, session.DraftAgreementSend.Gold.ToString());
            AddTextEntry(503, 600, 100, 20, 0, EntryAgreementSendWood, session.DraftAgreementSend.Wood.ToString());
            AddTextEntry(502, 630, 100, 20, 0, EntryAgreementSendIron, session.DraftAgreementSend.Iron.ToString());
            AddTextEntry(503, 660, 100, 20, 0, EntryAgreementSendCloth, session.DraftAgreementSend.Cloth.ToString());

            AddLabel(759, 575, 0, @"Moedas:");
            AddLabel(759, 603, 0, @"Madeira:");
            AddLabel(759, 633, 0, @"Ferro:");
            AddLabel(759, 663, 0, @"Tecido:");
            AddTextEntry(843, 570, 100, 20, 0, EntryAgreementReceiveGold, session.DraftAgreementReceive.Gold.ToString());
            AddTextEntry(843, 600, 100, 20, 0, EntryAgreementReceiveWood, session.DraftAgreementReceive.Wood.ToString());
            AddTextEntry(842, 630, 100, 20, 0, EntryAgreementReceiveIron, session.DraftAgreementReceive.Iron.ToString());
            AddTextEntry(843, 660, 100, 20, 0, EntryAgreementReceiveCloth, session.DraftAgreementReceive.Cloth.ToString());

            if (agreement != null)
            {
                AddLabel(632, 572, 0, agreement.SendFromSource.Gold.ToString());
                AddLabel(632, 600, 0, agreement.SendFromSource.Wood.ToString());
                AddLabel(632, 630, 0, agreement.SendFromSource.Iron.ToString());
                AddLabel(632, 660, 0, agreement.SendFromSource.Cloth.ToString());
                AddLabel(972, 572, 0, agreement.SendFromTarget.Gold.ToString());
                AddLabel(973, 599, 0, agreement.SendFromTarget.Wood.ToString());
                AddLabel(972, 630, 0, agreement.SendFromTarget.Iron.ToString());
                AddLabel(973, 659, 0, agreement.SendFromTarget.Cloth.ToString());
                AddButton(1108, 620, 493, 493, ButtonDiplomacyCancelAgreement, GumpButtonType.Reply, 0);
            }

            AddButton(1108, 560, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
        }

        private void DrawBordersSection(ReinoDiplomacySession session, int targetCityId)
        {
            AddLabel(638, 491, 0, @"Fechar Fronteiras");
            AddLabel(435, 610, 0, @"Não Permitir:");
            AddLabel(621, 555, 0, "Entrada de cidadãos do reino " + ReinoElectionsSystem.GetCityPeopleName(targetCityId));
            AddLabel(622, 593, 0, "Entrada de qualquer nativo do povo " + ReinoElectionsSystem.GetCityPeopleName(targetCityId));
            AddLabel(622, 630, 0, "Entrada de qualquer aliado do reino " + ReinoElectionsSystem.GetCityPeopleName(targetCityId));
            AddLabel(622, 670, 0, @"Permitir a entrada");

            AddToggleButton(586, 554, session.DraftBorders.BlockEnemyCitizens, ButtonDiplomacyBorderBase + 1);
            AddToggleButton(586, 593, session.DraftBorders.BlockEnemyCulture, ButtonDiplomacyBorderBase + 2);
            AddToggleButton(586, 630, session.DraftBorders.BlockEnemyAllies, ButtonDiplomacyBorderBase + 3);
            AddToggleButton(586, 669, session.DraftBorders.AllowEntry, ButtonDiplomacyBorderBase + 4);
            AddImageTiled(1051, 536, 6, 158, 365);
            AddButton(1105, 593, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
        }

        private void DrawBlockadeSection(ReinoDiplomacySession session)
        {
            AddLabel(772, 492, 0, @"Bloqueio Comercial");
            AddLabel(429, 606, 0, @"Não Permitir:");
            AddLabel(621, 555, 0, @"Compra e venda no representante comercial");
            AddLabel(622, 593, 0, @"Acordos vigentes");
            AddLabel(622, 630, 0, @"Receber doação de postos ou recursos");
            AddLabel(622, 670, 0, @"Vendedores particulares de fazer comércio");

            AddToggleButton(586, 554, session.DraftBlockade.BlockRepresentative, ButtonDiplomacyBlockadeBase + 1);
            AddToggleButton(586, 593, session.DraftBlockade.CancelAgreements, ButtonDiplomacyBlockadeBase + 2);
            AddToggleButton(586, 630, session.DraftBlockade.CancelDonations, ButtonDiplomacyBlockadeBase + 3);
            AddToggleButton(586, 669, session.DraftBlockade.BlockPlayerVendors, ButtonDiplomacyBlockadeBase + 4);
            AddImageTiled(1051, 536, 6, 158, 365);
            AddButton(1105, 593, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
        }

        private void DrawTributeSection(ReinoDiplomacySession session)
        {
            AddLabel(772, 492, 0, @"Exigir Tributo");
            AddLabel(437, 563, 0, @"Moedas:");
            AddLabel(437, 591, 0, @"Madeira:");
            AddLabel(437, 621, 0, @"Ferro:");
            AddLabel(437, 651, 0, @"Tecido:");
            AddTextEntry(508, 558, 200, 20, 0, EntryTributeGold, session.DraftTribute.Gold.ToString());
            AddTextEntry(508, 588, 200, 20, 0, EntryTributeWood, session.DraftTribute.Wood.ToString());
            AddTextEntry(507, 618, 200, 20, 0, EntryTributeIron, session.DraftTribute.Iron.ToString());
            AddTextEntry(508, 648, 200, 20, 0, EntryTributeCloth, session.DraftTribute.Cloth.ToString());
            AddImageTiled(816, 536, 6, 158, 365);
            AddImageTiled(1051, 536, 6, 158, 365);

            AddTributeFrequencyButton(850, 548, ReinoDiplomacyTributeFrequency.Once, session.DraftTributeFrequency);
            AddTributeFrequencyButton(850, 587, ReinoDiplomacyTributeFrequency.Daily, session.DraftTributeFrequency);
            AddTributeFrequencyButton(850, 624, ReinoDiplomacyTributeFrequency.Weekly, session.DraftTributeFrequency);
            AddTributeFrequencyButton(850, 663, ReinoDiplomacyTributeFrequency.Monthly, session.DraftTributeFrequency);

            AddLabel(879, 548, 0, @"Uma Vez");
            AddLabel(879, 587, 0, @"Diariamente");
            AddLabel(879, 624, 0, @"Semanalmente");
            AddLabel(879, 663, 0, @"Mensalmente");
            AddButton(1105, 593, 492, 492, ButtonDiplomacyConfirm, GumpButtonType.Reply, 0);
        }

        private void AddToggleButton(int x, int y, bool selected, int buttonId)
        {
            int art = selected ? 528 : 531;
            AddButton(x, y, art, art, buttonId, GumpButtonType.Reply, 0);
        }

        private void AddTributeFrequencyButton(int x, int y, ReinoDiplomacyTributeFrequency option, ReinoDiplomacyTributeFrequency selected)
        {
            int art = option == selected ? 528 : 531;
            AddButton(x, y, art, art, ButtonDiplomacyTributeFreqBase + (int)option, GumpButtonType.Reply, 0);
        }

        private int ParseEntryInt(RelayInfo info, int entryId, int fallback)
        {
            if (info == null || info.TextEntries == null)
                return fallback;

            TextRelay relay = info.GetTextEntry(entryId);
            if (relay == null || String.IsNullOrWhiteSpace(relay.Text))
                return 0;

            int value;
            if (!Int32.TryParse(relay.Text.Trim(), out value))
                return fallback;

            return Math.Max(0, value);
        }

        private void LoadDraftNumbers(ReinoDiplomacySession session, RelayInfo info)
        {
            session.DraftDonation.Gold = ParseEntryInt(info, EntryDonateGold, session.DraftDonation.Gold);
            session.DraftDonation.Wood = ParseEntryInt(info, EntryDonateWood, session.DraftDonation.Wood);
            session.DraftDonation.Iron = ParseEntryInt(info, EntryDonateIron, session.DraftDonation.Iron);
            session.DraftDonation.Cloth = ParseEntryInt(info, EntryDonateCloth, session.DraftDonation.Cloth);

            session.DraftAgreementSend.Gold = ParseEntryInt(info, EntryAgreementSendGold, session.DraftAgreementSend.Gold);
            session.DraftAgreementSend.Wood = ParseEntryInt(info, EntryAgreementSendWood, session.DraftAgreementSend.Wood);
            session.DraftAgreementSend.Iron = ParseEntryInt(info, EntryAgreementSendIron, session.DraftAgreementSend.Iron);
            session.DraftAgreementSend.Cloth = ParseEntryInt(info, EntryAgreementSendCloth, session.DraftAgreementSend.Cloth);

            session.DraftAgreementReceive.Gold = ParseEntryInt(info, EntryAgreementReceiveGold, session.DraftAgreementReceive.Gold);
            session.DraftAgreementReceive.Wood = ParseEntryInt(info, EntryAgreementReceiveWood, session.DraftAgreementReceive.Wood);
            session.DraftAgreementReceive.Iron = ParseEntryInt(info, EntryAgreementReceiveIron, session.DraftAgreementReceive.Iron);
            session.DraftAgreementReceive.Cloth = ParseEntryInt(info, EntryAgreementReceiveCloth, session.DraftAgreementReceive.Cloth);

            session.DraftTribute.Gold = ParseEntryInt(info, EntryTributeGold, session.DraftTribute.Gold);
            session.DraftTribute.Wood = ParseEntryInt(info, EntryTributeWood, session.DraftTribute.Wood);
            session.DraftTribute.Iron = ParseEntryInt(info, EntryTributeIron, session.DraftTribute.Iron);
            session.DraftTribute.Cloth = ParseEntryInt(info, EntryTributeCloth, session.DraftTribute.Cloth);
        }

        private void SendDiplomacyRefresh(PlayerMobile from)
        {
            from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 6));
        }

        private bool HandleDiplomacyResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;
            if (button < ButtonDiplomacyTargetBase || button > ButtonDiplomacyTributeFreqBase + 10)
                return false;

            ReinoDiplomacySession session = ReinoDiplomacySystem.GetSession(from, m_CityId);
            LoadDraftNumbers(session, info);

            if (button >= ButtonDiplomacyTargetBase && button < ButtonDiplomacyRelationBase)
            {
                int targetCityId = button - ButtonDiplomacyTargetBase;
                ReinoDiplomacySystem.ResetSessionSelection(from, m_CityId, targetCityId);
                SendDiplomacyRefresh(from);
                return true;
            }

            if (button >= ButtonDiplomacyRelationBase && button < ButtonDiplomacyActionBase)
            {
                session.DraftRelation = (ReinoDiplomacyRelationStatus)(button - ButtonDiplomacyRelationBase);
                session.SelectedAction = ReinoDiplomacyActionKind.None;
                SendDiplomacyRefresh(from);
                return true;
            }

            if (button >= ButtonDiplomacyActionBase && button < ButtonDiplomacyConfirm)
            {
                ReinoDiplomacyActionKind action = (ReinoDiplomacyActionKind)(button - ButtonDiplomacyActionBase);
                ReinoDiplomacyRelationStatus relationForActions = session.DraftRelation ?? ReinoDiplomacySystem.GetRelation(m_CityId, session.TargetCityId);

                if (ReinoDiplomacySystem.GetAvailableActionsForRelation(relationForActions).Contains(action))
                    session.SelectedAction = action;
                else
                    session.SelectedAction = ReinoDiplomacyActionKind.None;

                SendDiplomacyRefresh(from);
                return true;
            }

            if (button >= ButtonDiplomacyPostoBase && button < ButtonDiplomacyBorderBase)
            {
                List<PostoDefinition> postos = ReinoDiplomacySystem.GetDonatablePostos(m_CityId);
                int postoIndex = button - ButtonDiplomacyPostoBase;

                if (postoIndex >= 0 && postoIndex < postos.Count)
                    session.SelectedPostoId = postos[postoIndex].Id;

                SendDiplomacyRefresh(from);
                return true;
            }

            if (button >= ButtonDiplomacyBorderBase && button < ButtonDiplomacyBlockadeBase)
            {
                int option = button - ButtonDiplomacyBorderBase;
                if (option == 1)
                {
                    session.DraftBorders.BlockEnemyCitizens = !session.DraftBorders.BlockEnemyCitizens;
                    if (session.DraftBorders.BlockEnemyCitizens)
                        session.DraftBorders.AllowEntry = false;
                }
                else if (option == 2)
                {
                    session.DraftBorders.BlockEnemyCulture = !session.DraftBorders.BlockEnemyCulture;
                    if (session.DraftBorders.BlockEnemyCulture)
                        session.DraftBorders.AllowEntry = false;
                }
                else if (option == 3)
                {
                    session.DraftBorders.BlockEnemyAllies = !session.DraftBorders.BlockEnemyAllies;
                    if (session.DraftBorders.BlockEnemyAllies)
                        session.DraftBorders.AllowEntry = false;
                }
                else if (option == 4)
                {
                    session.DraftBorders.AllowEntry = !session.DraftBorders.AllowEntry;
                    if (session.DraftBorders.AllowEntry)
                    {
                        session.DraftBorders.BlockEnemyCitizens = false;
                        session.DraftBorders.BlockEnemyCulture = false;
                        session.DraftBorders.BlockEnemyAllies = false;
                    }
                }

                SendDiplomacyRefresh(from);
                return true;
            }

            if (button >= ButtonDiplomacyBlockadeBase && button < ButtonDiplomacyTributeFreqBase)
            {
                int option = button - ButtonDiplomacyBlockadeBase;
                if (option == 1) session.DraftBlockade.BlockRepresentative = !session.DraftBlockade.BlockRepresentative;
                else if (option == 2) session.DraftBlockade.CancelAgreements = !session.DraftBlockade.CancelAgreements;
                else if (option == 3) session.DraftBlockade.CancelDonations = !session.DraftBlockade.CancelDonations;
                else if (option == 4) session.DraftBlockade.BlockPlayerVendors = !session.DraftBlockade.BlockPlayerVendors;
                SendDiplomacyRefresh(from);
                return true;
            }

            if (button >= ButtonDiplomacyTributeFreqBase && button < ButtonDiplomacyTributeFreqBase + 10)
            {
                session.DraftTributeFrequency = (ReinoDiplomacyTributeFrequency)(button - ButtonDiplomacyTributeFreqBase);
                SendDiplomacyRefresh(from);
                return true;
            }

            if (button == ButtonDiplomacyCancelAgreement)
            {
                string cancelMessage;
                ReinoDiplomacySystem.CancelAgreement(from, m_CityId, session.TargetCityId, out cancelMessage);
                from.SendMessage(cancelMessage);
                SendDiplomacyRefresh(from);
                return true;
            }

            if (button == ButtonDiplomacyConfirm)
            {
                string message = String.Empty;
                if (session.SelectedAction == ReinoDiplomacyActionKind.None)
                {
                    ReinoDiplomacyRelationStatus relationToApply = session.DraftRelation ?? ReinoDiplomacySystem.GetRelation(m_CityId, session.TargetCityId);
                    ReinoDiplomacySystem.SubmitRelationChange(from, m_CityId, session.TargetCityId, relationToApply, out message);
                }
                else if (session.SelectedAction == ReinoDiplomacyActionKind.DonateResources)
                {
                    ReinoDiplomacySystem.SubmitResourceDonation(from, m_CityId, session.TargetCityId, session.DraftDonation, out message);
                }
                else if (session.SelectedAction == ReinoDiplomacyActionKind.DonatePosto)
                {
                    ReinoDiplomacySystem.SubmitPostoDonation(from, m_CityId, session.TargetCityId, session.SelectedPostoId, out message);
                }
                else if (session.SelectedAction == ReinoDiplomacyActionKind.ProposeAgreement)
                {
                    ReinoDiplomacySystem.SubmitAgreement(from, m_CityId, session.TargetCityId, session.DraftAgreementSend, session.DraftAgreementReceive, out message);
                }
                else if (session.SelectedAction == ReinoDiplomacyActionKind.CloseBorders)
                {
                    ReinoDiplomacySystem.SubmitBorders(from, m_CityId, session.TargetCityId, session.DraftBorders, out message);
                }
                else if (session.SelectedAction == ReinoDiplomacyActionKind.CommercialBlockade)
                {
                    ReinoDiplomacySystem.SubmitBlockade(from, m_CityId, session.TargetCityId, session.DraftBlockade, out message);
                }
                else if (session.SelectedAction == ReinoDiplomacyActionKind.DemandTribute)
                {
                    ReinoDiplomacySystem.SubmitTribute(from, m_CityId, session.TargetCityId, session.DraftTribute, session.DraftTributeFrequency, out message);
                }

                if (!String.IsNullOrWhiteSpace(message))
                    from.SendMessage(message);

                SendDiplomacyRefresh(from);
                return true;
            }

            return false;
        }
    }
}
