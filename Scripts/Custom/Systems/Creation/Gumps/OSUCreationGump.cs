using Server;
using Server.Custom.Systems.Creation.Cultures;
using Server.Custom.Systems.Creation.Engine;
using Server.Custom.Systems.Culture;
using Server.Custom.Systems.DefQual;
using Server.Custom.Systems.Religion;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Server.Custom.Systems.Creation.Engine.OSUCreationTexts;

namespace Server.Custom.Systems.Creation.Gumps
{
    public class OSUCreationGump : Gump
    {
        #region Enums

        private const int TotalPages = 7;

        private const int CultureSelectButtonBase = 500; // 500..505

        // ===== Page 4 IDs =====
        private const int AttrMinusBase = 600; // 600..605
        private const int AttrPlusBase = 610;  // 610..615
        private const int AttrMinusBasePlus = 650; // 600..605
        private const int AttrPlusBasePlus = 660;  // 610..615

        private const int AttrInfoBase = 620;  // 620..625 (botões informativos)

        private const int DQTabDefects = 700;
        private const int DQTabQualities = 701;
        private const int DQPrevPage = 702;
        private const int DQNextPage = 703;
        private const int DQSelectBase = 720;   // 720..727
        private const int DQBuyToggle = 750;

        private const int ReligionSelectBase = 800; // 800..809

        private const int PointsTotal = 100;
        private const int AttrStart = 15;
        private const int AttrMin = 10;
        private const int AttrMax = 115;

        private const int MinMajorCap = 40000;
        private const int MaxMajorCap = 70000;


        private readonly PlayerMobile _pm;
        private readonly int _page;
        private readonly OSUCreationInfoTopic _page1Topic;
        private readonly OSUCultureInfoTopic _page3Topic;

        // Page4 UI state (não vai pro char, só pro gump)
        private readonly OSUAttributeTopic _page4AttrTopic;
        private readonly bool _page4ShowDefects;
        private readonly int _page4DQPage;
        private readonly string _page4SelectedDQId;

       // Pagina 5 ReadOnly
        private readonly bool _p5ShowCombat;
        private readonly int _p5ListPage;
        private readonly SkillName? _p5InfoSkill;

        // ===== Page 5 IDs =====
        private const int P5_TabCombat = 5000;
        private const int P5_TabProf = 5001;
        private const int P5_TabSystem = 5002;

        private const int P5_SkillSelectBase = 9000; // base livre
        private const int P5_PrevPage = 6010;
        private const int P5_NextPage = 6011;
        private const int P5_RowsPerPage = 14;

        // ===== Page 6 IDs =====
        private const int P6_NameEntry = 10000;

        private const int P6_GenderMale = 10010;
        private const int P6_GenderFemale = 10011;

        private const int P6_Body1 = 10020;
        private const int P6_Body2 = 10021;

        // Pele: 16 botões (igual sua imagem)
        private const int P6_SkinBase = 10030; // +0..+15

        // Rostos (placeholders): 8 botões
        private const int P6_FaceBase = 10060; // +0..+7

        // Cabelo: 16 botões + 8 cores (placeholders)
        private const int P6_HairBase = 13050; // +0..+15
        private const int P6_HairColorBase = 10130; // +0..+7

        // Barba: 16 botões + 8 cores (só aparece se homem)
        private const int P6_BeardBase = 15160; // +0..+15
        private const int P6_BeardColorBase = 10190; // +0..+7

        private const int P6_TabHair = 11000;
        private const int P6_TabBeard = 11001;

        // ===== Page 7 IDs =====
        private const int P7_WeightEntry = 12000;
        private const int P7_HeightEntry = 12001;
        private const int P7_AgeEntry = 12002;
        private const int P7_HistoryEntry = 12003;
        private const int P7_TraitsEntry = 12004;

        private const int P7_AvatarPrev = 12010;
        private const int P7_AvatarNext = 12011;

        private const int P7_AvatarBtn1 = 12020;
        private const int P7_AvatarBtn2 = 12021;
        private const int P7_AvatarBtn3 = 12022;
        private const int P7_AvatarBtn4 = 12023;

        private const int P7_AvatarsPerPage = 4;
        private const int P7_ConfirmWeight = 12030;
        private const int P7_ConfirmHeight = 12031;
        private const int P7_ConfirmAge = 12032;

        private const int P7_ConfirmHistory = 12033;
        private const int P7_ConfirmTraits = 12034;


        private enum Buttons
        {
            Close = 0,

            Prev = 1,
            Next = 2,

            // Página 1 (informativos)
            InfoOSU = 10,
            InfoLore = 11,
            InfoRegras = 12,

            // Página 3 (informativos)
            CultureLore = 30,
            CultureFisico = 31,
            CulturePapeis = 32,
            CultureTradicoes = 33,
            CultureProverbios = 34
        }

        private enum Switches
        {
            // Página 2
            PathWarrior = 100,
            PathArtisan = 101,

            ModePvp = 110,
            ModeNoPvp = 111
        }

        #endregion

        #region Constructors

        public OSUCreationGump(
            PlayerMobile pm,
            int page = 1,
            OSUCreationInfoTopic page1Topic = OSUCreationInfoTopic.OSU,
            OSUCultureInfoTopic page3Topic = OSUCultureInfoTopic.Lore,
            OSUAttributeTopic page4AttrTopic = OSUAttributeTopic.Str,
            bool page4ShowDefects = true,
            int page4DQPage = 0,
            string page4SelectedDQId = null)
            : base(0, 0)
        {
            _pm = pm;
            _page = page < 1 ? 1 : (page > TotalPages ? TotalPages : page);
            _page1Topic = page1Topic;
            _page3Topic = page3Topic;

            _page4AttrTopic = page4AttrTopic;
            _page4ShowDefects = page4ShowDefects;
            _page4DQPage = page4DQPage;
            _page4SelectedDQId = page4SelectedDQId;


            bool page5ShowCombat = true;
            int page5ListPage = 0;
            SkillName? page5InfoSkill = null;

            _p5ShowCombat = page5ShowCombat;
            _p5ListPage = page5ListPage;
            _p5InfoSkill = page5InfoSkill;


            Closable = false;
            Disposable = false;
            Dragable = true;
            Resizable = false;

            if (_pm.OSUCreation == null)
                _pm.OSUCreation = new OSUCreationContext();

            BuildBase();
            BuildPage();
        }

        #endregion

        #region GumpBase
        private void BuildBase()
        {
            AddPage(0);

            AddImageTiled(268, 86, 1038, 778, 374);
            AddImage(1223, 763, 341);
            AddImage(255, 763, 342);
            AddImage(255, 56, 339);
            AddImage(1223, 56, 340);

            AddLabel(721, 113, 0x481, "Criação de Personagem");

            AddButton(1223, 806, 588, 588, (int)Buttons.Next, GumpButtonType.Reply, 0);
            AddButton(313, 807, 589, 589, (int)Buttons.Prev, GumpButtonType.Reply, 0);

            AddImageTiled(260, 180, 33, 583, 345);
            AddImageTiled(1300, 182, 33, 581, 346);
            AddImageTiled(359, 58, 869, 48, 355);
            AddImageTiled(365, 838, 860, 48, 356);
            AddImageTiled(307, 133, 974, 21, 471);
            AddImageTiled(307, 783, 974, 21, 471);

            AddLabel(372, 812, 0x481, "Pagina anterior");
            AddLabel(1124, 811, 0x481, "Proxima pagina");
            AddLabel(757, 811, 0x481, $"Pagina {_page}/{TotalPages}");
        }

        #endregion

        #region GumpLayout

        // ===== Página 1 =====
        private void BuildPage1_Info()
        {
            AddHtml(314, 246, 963, 526, OSUCreationTexts.GetPage1Html(_page1Topic), true, true);

            AddLabel(518, 200, 0x481, "OSU");
            AddButton(512, 162, 442, 440, (int)Buttons.InfoOSU, GumpButtonType.Reply, 0);

            AddLabel(755, 200, 0x481, "Lore Amanti");
            AddButton(770, 162, 442, 440, (int)Buttons.InfoLore, GumpButtonType.Reply, 0);

            AddLabel(1006, 200, 0x481, "Regras");
            AddButton(1008, 162, 442, 440, (int)Buttons.InfoRegras, GumpButtonType.Reply, 0);
        }

        // ===== Página 2 =====
        private void BuildPage2_PathAndMode()
        {
            AddLabel(763, 158, 0x481, "Caminhos");
            AddImageTiled(307, 178, 974, 21, 463);

            AddLabel(495, 244, 0x481, "Guerreiro");
            AddLabel(1002, 244, 0x481, "Artesão");

            AddHtml(312, 277, 459, 141, OSUCreationTexts.Page2_Warrior, true, true);
            AddHtml(809, 277, 459, 141, OSUCreationTexts.Page2_Artisan, true, true);

            AddGroup(1);
            bool warriorOn = (_pm.OSUCreation.Path == OSUCreationPath.Warrior);
            bool artisanOn = (_pm.OSUCreation.Path == OSUCreationPath.Artisan);

            AddRadio(504, 206, 442, 440, warriorOn, (int)Switches.PathWarrior);
            AddRadio(1007, 206, 442, 440, artisanOn, (int)Switches.PathArtisan);

            AddImageTiled(307, 434, 974, 21, 463);
            AddLabel(757, 459, 0x481, "Game Mode");
            AddImageTiled(307, 479, 974, 21, 463);

            AddLabel(502, 551, 0x481, "Char PVP");
            AddLabel(991, 552, 0x481, "Char Nao PVP");

            AddHtml(312, 587, 459, 181, OSUCreationTexts.Page2_Pvp, true, true);
            AddHtml(809, 587, 459, 177, OSUCreationTexts.Page2_NoPvp, true, true);

            AddGroup(2);
            bool pvpOn = (_pm.OSUCreation.GameMode == OSUCreationGameMode.Pvp);
            bool nopvpOn = (_pm.OSUCreation.GameMode == OSUCreationGameMode.NoPvp);

            AddRadio(512, 508, 442, 440, pvpOn, (int)Switches.ModePvp);
            AddRadio(1011, 508, 442, 440, nopvpOn, (int)Switches.ModeNoPvp);
        }

        // ===== Página 3 =====
        private void BuildPage3_Cultures()
        {
            AddLabel(770, 158, 0x481, "Povos");
            AddImageTiled(307, 176, 974, 21, 463);

            AddImageTiled(429, 192, 29, 601, 480);

            List<OSUCultureDefinition> cultures = OSUCultureRegistry.GetOrdered(6);

            int[] labelY = { 358, 434, 509, 584, 659, 734 };
            int[] buttonY = { 349, 424, 499, 574, 649, 724 };

            for (int i = 0; i < 6; i++)
            {
                OSUCultureDefinition def = (i < cultures.Count) ? cultures[i] : null;

                string name = def != null ? def.DisplayName : "";
                AddLabel(367, labelY[i], 0x481, name);

                if (def == null)
                    continue;

                bool pselected = !String.IsNullOrWhiteSpace(_pm.OSUCreation.CultureId) &&
                    String.Equals(_pm.OSUCreation.CultureId, def.Id, StringComparison.OrdinalIgnoreCase);

                int normalID = pselected ? 440 : 442;
                int pressedID = 440;

                AddButton(315, buttonY[i], normalID, pressedID, CultureSelectButtonBase + i, GumpButtonType.Reply, 0);
            }

            AddButton(831, 240, 455, 454, (int)Buttons.CultureLore, GumpButtonType.Reply, 0);
            AddLabel(824, 216, 0x481, "Lore");

            AddButton(931, 240, 455, 454, (int)Buttons.CultureFisico, GumpButtonType.Reply, 0);
            AddLabel(918, 216, 0x481, "Físico");

            AddButton(1031, 240, 455, 454, (int)Buttons.CulturePapeis, GumpButtonType.Reply, 0);
            AddLabel(1015, 215, 0x481, "Papéis");

            AddButton(1131, 240, 455, 454, (int)Buttons.CultureTradicoes, GumpButtonType.Reply, 0);
            AddLabel(1106, 216, 0x481, "Tradições");

            AddButton(1231, 240, 455, 454, (int)Buttons.CultureProverbios, GumpButtonType.Reply, 0);
            AddLabel(1203, 216, 0x481, "Provérbios");

            OSUCultureDefinition selected = OSUCultureRegistry.GetById(_pm.OSUCreation.CultureId);

            string html;

            if (selected == null)
            {
                html = @"<BASEFONT COLOR=#FFFFFF>
                <CENTER><B>Escolha um povo</B></CENTER><BR><BR>
                Clique em um povo à esquerda para ver a Lore e a imagem.
                </BASEFONT>";
            }
            else
            {
                html = selected.GetHtml(_page3Topic);
            }

            AddHtml(817, 275, 457, 490, html, true, true);

            if (selected != null && selected.PortraitGumpId > 0)
            {
                AddImage(495, 287, selected.PortraitGumpId);
            }
        }

        // ===== Página 4 =====
        private void BuildPage4_Attributes_DefQual()
        {
            // =========================================================
            //  DEF & QUAL  (layout do 2º script, funcionalidade do 1º)
            // =========================================================
            EnsureAttrInitialized(_pm.OSUCreation);

            // Título / barra superior do bloco
            AddLabel(726, 156, 0x481, "Defeitos e Qualidades");
            AddImageTiled(307, 174, 974, 21, 463);

            bool showDefects = _page4ShowDefects;

            // Tabs (diamante / checkbox look)
            // (no 2º script tinha AddCheck; aqui uso AddButton p/ manter Reply e não mexer no OnResponse)
            AddButton(685, 203, showDefects ? 440 : 442, 440, DQTabDefects, GumpButtonType.Reply, 0);
            AddLabel(736, 211, 0x481, "Defeitos");

            AddButton(818, 203, !showDefects ? 440 : 442, 440, DQTabQualities, GumpButtonType.Reply, 0);
            AddLabel(869, 211, 0x481, "Qualidades");

            // Lista filtrada (igual 1º script)
            var all = OSUDefQualRegistry.GetAll();
            var filtered = new List<OSUDefQualDefinition>();

            var selectedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_pm.OSUCreation.SelectedDefQualIds != null)
            {
                for (int i = 0; i < _pm.OSUCreation.SelectedDefQualIds.Count; i++)
                    selectedSet.Add(_pm.OSUCreation.SelectedDefQualIds[i]);
            }

            for (int i = 0; i < all.Count; i++)
            {
                var d = all[i];
                if (d == null) continue;

                if (showDefects && d.Type != OSUDefQualType.Defect) continue;
                if (!showDefects && d.Type != OSUDefQualType.Quality) continue;

                filtered.Add(d);
            }

            // Paginação (no layout certo aparecem 6 itens)
            int perPage = 6;
            int maxPage = filtered.Count == 0 ? 0 : (filtered.Count - 1) / perPage;
            int page = _page4DQPage;
            if (page < 0) page = 0;
            if (page > maxPage) page = maxPage;

            int start = page * perPage;
            int end = Math.Min(start + perPage, filtered.Count);

            // Coordenadas exatas (2º script)
            int listX = 318;
            int labelX = 344;
            int[] rowY = { 245, 274, 303, 332, 362, 392 };

            for (int i = start; i < end; i++)
            {
                int row = i - start;
                int y = rowY[row];

                var d = filtered[i];
                bool already = selectedSet.Contains(d.Id);

                // botão de seleção (mostra descrição)
                AddButton(listX, y, 455, 454, DQSelectBase + row, GumpButtonType.Reply, 0);

                int hue = already ? 0x35 : 0x481;
                // no 2º script o label ficava 2px acima
                AddLabel(labelX, y - 2, hue, d.Name);
            }

            // Paginação (2º script: setas + rótulo "1/1")
            AddLabel(430, 431, 0x481, $"{page + 1}/{maxPage + 1}");

            if (page > 0)
                AddButton(392, 427, 453, 5541, DQPrevPage, GumpButtonType.Reply, 0);

            if (page < maxPage)
                AddButton(470, 427, 452, 5541, DQNextPage, GumpButtonType.Reply, 0);

            // Descrição
            OSUDefQualDefinition selectedDQ = OSUDefQualRegistry.GetById(_page4SelectedDQId);

            string dqHtml = selectedDQ != null
                ? selectedDQ.DescriptionHtml
                : @"<BASEFONT COLOR=#FFFFFF>Selecione um Defeito ou Qualidade para ver a descrição.</BASEFONT>";

            AddHtml(640, 260, 484, 188, dqHtml, true, true);

            // Cap (2º script)
            int majorCap = GetMajorCapAfterDefQual(_pm.OSUCreation);
            AddImageTiled(1143, 265, 133, 35, 393);
            AddLabel(1169, 271, 0x481, $"Caps: {majorCap}");

            // Compra / Remover / Bloqueado (mesma lógica do 1º script, posição do 2º)
            bool showBuyButton = false;
            string buyLabel = "COMPRA";

            if (selectedDQ != null)
            {
                bool already = selectedSet.Contains(selectedDQ.Id);

                if (already)
                {
                    buyLabel = "REMOVER";
                    showBuyButton = true;
                }
                else
                {
                    if (selectedDQ.IsBlockedBySelection(selectedSet))
                    {
                        buyLabel = "Nao pode Comprar";
                        showBuyButton = false;
                    }
                    else
                    {
                        buyLabel = "COMPRA";
                        showBuyButton = true;
                    }
                }
            }

            AddLabel(1160, 352, 0x481, buyLabel);
            if (showBuyButton)
                AddButton(1224, 344, 442, 440, DQBuyToggle, GumpButtonType.Reply, 0);

            // =========================================================
            //  ATRIBUTOS  (layout do 2º script, funcionalidade do 1º)
            // =========================================================

            AddImageTiled(307, 458, 974, 21, 463);
            AddLabel(542, 482, 0x481, "Atributos");
            AddImageTiled(307, 502, 974, 21, 463);

            // PONTOS RESTANTES (2º script embaixo)
            int remaining = RemainingAttrPoints(_pm.OSUCreation);
            AddLabel(340, 749, 0x481, $"Pontos Restantes: {remaining}/{PointsTotal}");

            // Valores atuais
            int vStr = _pm.OSUCreation.Attr_Str;
            int vDex = _pm.OSUCreation.Attr_Dex;
            int vInt = _pm.OSUCreation.Attr_Int;
            int vHP = _pm.OSUCreation.Attr_HP;
            int vVig = _pm.OSUCreation.Attr_Vit;
            int vMan = _pm.OSUCreation.Attr_Man;

            // FORÇA
            AddLabel(358, 529, 0x481, "FORÇA");
            AddImageTiled(359, 553, 49, 28, 399);
            AddLabel(373, 558, 0x481, vStr.ToString());
            AddButton(327, 555, 453, 453, AttrMinusBase + 0, GumpButtonType.Reply, 0);
            AddButton(420, 555, 452, 452, AttrPlusBase + 0, GumpButtonType.Reply, 0);

            AddButton(308, 551, 451, 451, AttrMinusBasePlus + 0, GumpButtonType.Reply, 0);
            AddButton(438, 551, 450, 450, AttrPlusBasePlus + 0, GumpButtonType.Reply, 0);

            // DESTREZA
            AddLabel(348, 600, 0x481, "DESTREZA");
            AddImageTiled(359, 624, 49, 28, 399);
            AddLabel(373, 628, 0x481, vDex.ToString());
            AddButton(327, 626, 453, 453, AttrMinusBase + 1, GumpButtonType.Reply, 0);
            AddButton(420, 626, 452, 452, AttrPlusBase + 1, GumpButtonType.Reply, 0);

            AddButton(308, 622, 451, 451, AttrMinusBasePlus + 1, GumpButtonType.Reply, 0);
            AddButton(438, 622, 450, 450, AttrPlusBasePlus + 1, GumpButtonType.Reply, 0);

            // INTELIGENCIA
            AddLabel(340, 672, 0x481, "INTELIGENCIA");
            AddImageTiled(359, 696, 49, 28, 399);
            AddLabel(373, 700, 0x481, vInt.ToString());
            AddButton(327, 698, 453, 453, AttrMinusBase + 2, GumpButtonType.Reply, 0);
            AddButton(420, 698, 452, 452, AttrPlusBase + 2, GumpButtonType.Reply, 0);

            AddButton(308, 693, 451, 451, AttrMinusBasePlus + 2, GumpButtonType.Reply, 0);
            AddButton(438, 693, 450, 450, AttrPlusBasePlus + 2, GumpButtonType.Reply, 0);

            // VIDA (HP)
            AddLabel(561, 529, 0x481, "VIDA");
            AddImageTiled(554, 553, 49, 28, 399);
            AddLabel(568, 557, 0x481, vHP.ToString());
            AddButton(521, 555, 453, 453, AttrMinusBase + 3, GumpButtonType.Reply, 0);
            AddButton(615, 555, 452, 452, AttrPlusBase + 3, GumpButtonType.Reply, 0);

            AddButton(502, 551, 451, 451, AttrMinusBasePlus + 3, GumpButtonType.Reply, 0);
            AddButton(634, 551, 450, 450, AttrPlusBasePlus + 3, GumpButtonType.Reply, 0);

            // VIGOR (VIT)
            AddLabel(556, 600, 0x481, "VIGOR");
            AddImageTiled(554, 624, 49, 28, 399);
            AddLabel(568, 628, 0x481, vVig.ToString());
            AddButton(521, 626, 453, 453, AttrMinusBase + 4, GumpButtonType.Reply, 0);
            AddButton(615, 626, 452, 452, AttrPlusBase + 4, GumpButtonType.Reply, 0);

            AddButton(502, 622, 451, 451, AttrMinusBasePlus + 4, GumpButtonType.Reply, 0);
            AddButton(634, 622, 450, 450, AttrPlusBasePlus + 4, GumpButtonType.Reply, 0);

            // MANA
            AddLabel(557, 672, 0x481, "MANA");
            AddImageTiled(554, 694, 49, 28, 399);
            AddLabel(568, 698, 0x481, vMan.ToString());
            AddButton(521, 696, 453, 453, AttrMinusBase + 5, GumpButtonType.Reply, 0);
            AddButton(615, 696, 452, 452, AttrPlusBase + 5, GumpButtonType.Reply, 0);

            AddButton(502, 693, 451, 451, AttrMinusBasePlus + 5, GumpButtonType.Reply, 0);
            AddButton(634, 693, 450, 450, AttrPlusBasePlus + 5, GumpButtonType.Reply, 0);

            // =========================================================
            //  INFO DE ATRIBUTOS (layout do 2º script)
            // =========================================================
            // Botões curtos: For / Vida / Des / Vig / Int / Mana
            // Mapear para seu tópico interno (_page4AttrTopic) via OnResponse.

            AddButton(729, 535, 455, 454, AttrInfoBase + 0, GumpButtonType.Reply, 0); // For
            AddLabel(720, 557, 0x481, "Força");

            AddButton(829, 535, 455, 454, AttrInfoBase + 3, GumpButtonType.Reply, 0); // Vida (HP)
            AddLabel(823, 557, 0x481, "Vida");

            AddButton(929, 535, 455, 454, AttrInfoBase + 1, GumpButtonType.Reply, 0); // Des
            AddLabel(908, 557, 0x481, "Destreza");

            AddButton(1029, 535, 455, 454, AttrInfoBase + 4, GumpButtonType.Reply, 0); // Vig (Vit)
            AddLabel(1019, 557, 0x481, "Vigor");

            AddButton(1129, 535, 455, 454, AttrInfoBase + 2, GumpButtonType.Reply, 0); // Int
            AddLabel(1101, 557, 0x481, "Inteligencia");

            AddButton(1229, 535, 455, 454, AttrInfoBase + 5, GumpButtonType.Reply, 0); // Mana
            AddLabel(1220, 557, 0x481, "Mana");

            AddHtml(699, 587, 570, 190, OSUCreationTexts.GetAttrHtml(_page4AttrTopic), true, true);

        }

        private void BuildPage5_Skills()
        {
            // =========================================================
            //  Layout: Religião (esquerda) + Skills (direita)
            //  (conforme o seu GumpStudio)
            // =========================================================

            AddLabel(1069, 162, 0x481, "Skills");
            AddImageTiled(308, 178, 974, 21, 463);

            // Divisória vertical (Religião | Skills)
            AddImageTiled(768, 150, 33, 643, 480);

            // Divisória vertical (Lista | Descrição de skills)
            AddImageTiled(962, 192, 33, 600, 480);

            // =========================================================
            //  RELIGIÃO (Página 5)
            // =========================================================

            AddLabel(520, 160, 0x481, "Religião");

            // Descrição da religião (painel superior esquerdo)
            var selRel = OSUReligionRegistry.GetById(_pm.OSUCreation.ReligionId);

            string LeftHtml;

            if (selRel != null)
            {
                // Se já escolheu um deus, mostra o texto daquele deus
                LeftHtml = selRel.DescriptionHtml;
            }
            else
            {
                // Se ainda não escolheu, mostra o texto geral de religião
                LeftHtml = OSUCreationTexts.Page5_Religion;
            }


            AddHtml(316, 212, 444, 193, LeftHtml, true, true);

            // Arte decorativa (igual seu layout do GumpStudio)
            int icon = (selRel != null) ? selRel.IconGumpId : 159;
            AddImage(465, 442, icon);


            // Lista de religiões (inferior esquerdo)
            var religions = OSUReligionRegistry.GetAll();
            int rx = 332;
            int ry = 501;
            int rStep = 25;

            if (religions.Count == 0)
            {
                AddLabel(rx, ry, 0x35, "Nenhum deus cadastrado (adicione arquivos em /Religion).");
            }
            else
            {
                for (int i = 0; i < religions.Count && i < 10; i++)
                {
                    var r = religions[i];

                    bool sel = !String.IsNullOrWhiteSpace(_pm.OSUCreation.ReligionId) &&
                               String.Equals(_pm.OSUCreation.ReligionId, r.Id, StringComparison.OrdinalIgnoreCase);

                    int normal = sel ? 454 : 455;
                    AddButton(rx, ry + (i * rStep), normal, 454, ReligionSelectBase + i, GumpButtonType.Reply, 0);

                    int hue = sel ? 0x35 : 0x481;
                    AddLabel(rx + 26, (ry + (i * rStep)) - 2, hue, r.Name);
                }
            }

            // =========================================================
            //  SKILLS (Página 5 - lado direito)
            // =========================================================

            // Linha separadora do bloco de skills (direita)
            AddImageTiled(788, 253, 494, 21, 463);

            // Tabs (O Sistema / Combate / Profissão)
            bool showCombat = _pm.OSUCreation.Page5ShowCombat;
            bool showSystem = String.IsNullOrWhiteSpace(_pm.OSUCreation.Page5InfoSkill);

            int sysNormal = showSystem ? 440 : 442;
            int combatNormal = (!showSystem && showCombat) ? 440 : 442;
            int profNormal = (!showSystem && !showCombat) ? 440 : 442;

            AddButton(829, 210, sysNormal, 440, P5_TabSystem, GumpButtonType.Reply, 0);
            AddLabel(875, 219, 0x481, "O Sistema");

            AddButton(986, 210, combatNormal, 440, P5_TabCombat, GumpButtonType.Reply, 0);
            AddLabel(1030, 219, 0x481, "Combate");

            AddButton(1129, 210, profNormal, 440, P5_TabProf, GumpButtonType.Reply, 0);
            AddLabel(1175, 219, 0x481, "Profissao");

            // Lista dinâmica (via SkillXPSystem)
            var group = showCombat ? Server.SkillXp.SkillXPSystem.OSUSkillGroup.Combat
                                   : Server.SkillXp.SkillXPSystem.OSUSkillGroup.Profession;

            List<SkillName> list = GetSkillsForGroup(group);

            int maxPage = (list.Count == 0) ? 0 : (list.Count - 1) / P5_RowsPerPage;
            int page = _pm.OSUCreation.Page5ListPage;
            if (page < 0) page = 0;
            if (page > maxPage) page = maxPage;
            _pm.OSUCreation.Page5ListPage = page;

            // Paginação (posicionada no lado direito)
            if (page > 0)
                AddButton(1040, 760, 453, 5541, P5_PrevPage, GumpButtonType.Reply, 0);
            if (page < maxPage)
                AddButton(1080, 760, 452, 5541, P5_NextPage, GumpButtonType.Reply, 0);

            int start = page * P5_RowsPerPage;
            int end = Math.Min(start + P5_RowsPerPage, list.Count);

            int btnX = 813;
            int lblX = 844;

            int y = 285;
            int step = 30;

            for (int i = start; i < end; i++)
            {
                SkillName sk = list[i];
                bool selected = IsSkillSelected(sk, showCombat);

                int normal = selected ? 454 : 455;
                int pressed = 454;

                AddButton(btnX, y + (i - start) * step, normal, pressed, P5_SkillSelectBase + (i - start), GumpButtonType.Reply, 0);
                AddLabel(lblX, y + (i - start) * step, 0x481, sk.ToString());
            }

            // Painel de descrição (direita)
            SkillName? infoSkill = null;
            if (!String.IsNullOrWhiteSpace(_pm.OSUCreation.Page5InfoSkill))
            {
                SkillName parsed;
                if (Enum.TryParse(_pm.OSUCreation.Page5InfoSkill, out parsed))
                    infoSkill = parsed;
            }

            string rightHtml = showSystem ? OSUCreationTexts.Page5_SystemHtml : BuildSkillInfoHtml(infoSkill);

            AddHtml(997, 305, 283, 447, rightHtml, true, true);
        }

        private void BuildPage6_Appearance()
        {
            // defaults: inicia Homem + Corpo 1
            if (_pm.OSUCreation.BodyVariant != 0 && _pm.OSUCreation.BodyVariant != 1)
                _pm.OSUCreation.BodyVariant = 0;

            // se nunca escolheu pele, começa em 1001
            if (_pm.OSUCreation.SkinHue == 0)
                _pm.OSUCreation.SkinHue = 1001;

            // ====== Cultura (para opções de pele/cabelo/cores) ======
            var cultureDef = OSUCultureRegistry.GetById(_pm.OSUCreation.CultureId);
            bool showBeard = (!_pm.OSUCreation.GenderFemale && _pm.OSUCreation.ShowBeardTab);

            int[] skinHues = (cultureDef != null && cultureDef.SkinHues != null && cultureDef.SkinHues.Length > 0)
                ? cultureDef.SkinHues
                : new[] { 1001 };

            int[] HairColorHues = (cultureDef != null && cultureDef.HairColorHues != null && cultureDef.HairColorHues.Length > 0)
                ? cultureDef.HairColorHues
                : new[] { 1102 };

            int[] BeardColorHues = (cultureDef != null && cultureDef.BeardColorHues != null && cultureDef.BeardColorHues.Length > 0)
                ? cultureDef.BeardColorHues
                : HairColorHues;


            // ====== Fundo da área de preview (seu layout) ======
            AddImageTiled(940, 500, 225, 274, 375);

            // ====== Cabeçalhos / linhas (seu layout) ======
            AddLabel(511, 157, 0x481, "Nome");
            AddImageTiled(308, 175, 974, 21, 463);

            AddImageTiled(308, 248, 974, 21, 463);

            AddLabel(897, 157, 0x481, "Genero");
            AddImageTiled(777, 145, 33, 647, 480);
            AddImageTiled(1031, 145, 33, 113, 480);

            AddLabel(1145, 157, 0x481, "Corpos");

            AddLabel(514, 277, 0x481, "Pele");
            AddImageTiled(308, 297, 972, 21, 463);

            AddLabel(1028, 277, 0x481, "Rostos");
            AddImageTiled(796, 459, 483, 21, 463);

            AddImageTiled(309, 508, 483, 21, 463);
            AddImageTiled(309, 563, 483, 21, 463);

            // ====== Nome (TextEntry) ======
            AddImageTiled(361, 199, 360, 42, 400);
            AddTextEntry(366, 206, 350, 28, 0x481, P6_NameEntry, _pm.OSUCreation.ChosenName ?? "");

            // ====== Botões Gênero ======
            AddButton(825, 216, _pm.OSUCreation.GenderFemale ? 455 : 454, 454, P6_GenderMale, GumpButtonType.Reply, 0);
            AddLabel(854, 215, 0x481, "Homem");

            AddButton(939, 216, _pm.OSUCreation.GenderFemale ? 454 : 455, 454, P6_GenderFemale, GumpButtonType.Reply, 0);
            AddLabel(968, 215, 0x481, "Mulher");

            // ====== Botões Corpo 1 / Corpo 2 ======
            AddButton(1077, 216, _pm.OSUCreation.BodyVariant == 0 ? 454 : 455, 454, P6_Body1, GumpButtonType.Reply, 0);
            AddLabel(1106, 215, 0x481, "Corpo 1");

            AddButton(1182, 216, _pm.OSUCreation.BodyVariant == 1 ? 454 : 455, 454, P6_Body2, GumpButtonType.Reply, 0);
            AddLabel(1211, 215, 0x481, "Corpo 2");

            // ====== Tabs Cabelo / Barba ======
            int hairTabPressed = (!_pm.OSUCreation.ShowBeardTab ? 441 : 441);
            AddButton(391, 527, 442, 441, P6_TabHair, GumpButtonType.Reply, 0);
            AddLabel(437, 537, 0x481, "Cabelo");

            if (!_pm.OSUCreation.GenderFemale) // só aparece se homem
            {
                int beardTabPressed = (_pm.OSUCreation.ShowBeardTab ? 441 : 441);
                AddButton(576, 527, 442, beardTabPressed, P6_TabBeard, GumpButtonType.Reply, 0);
                AddLabel(621, 537, 0x481, "Barba");
            }


            // ====== Pele (20 botões no layout: 4 colunas x 5 linhas) ======
            int[] colX = { 317, 430, 545, 669 };
            int[] rowY = { 341, 372, 405, 435, 467 };

            int skinIndex = 0;
            for (int ry = 0; ry < rowY.Length; ry++)
            {
                for (int cx = 0; cx < colX.Length; cx++)
                {
                    int hue = (skinIndex < skinHues.Length) ? skinHues[skinIndex] : skinHues[skinHues.Length - 1];

                    bool skinSelected = (_pm.OSUCreation.SkinHue == hue);
                    AddButton(colX[cx], rowY[ry], skinSelected ? 454 : 455, 454, P6_SkinBase + skinIndex, GumpButtonType.Reply, 0);

                    // IMPORTANTE: o texto fica pintado com a cor da pele
                    AddLabel(colX[cx] + 29, rowY[ry] - 1, hue, hue.ToString());

                    skinIndex++;
                }
            }

            // ====== Rostos (8 botões no layout) ======
            // posições: 4 colunas x 2 linhas
            int[] faceX = { 823, 935, 1051, 1173 };
            int[] faceY = { 353, 399 };

            int faceBtn = 0;
            for (int fy = 0; fy < faceY.Length; fy++)
            {
                for (int fx = 0; fx < faceX.Length; fx++)
                {
                    bool faceSelected = (_pm.OSUCreation.FaceIndex == faceBtn);
                    AddButton(faceX[fx], faceY[fy], faceSelected ? 454 : 455, 454, P6_FaceBase + faceBtn, GumpButtonType.Reply, 0);
                    AddLabel(faceX[fx] + 29, faceY[fy] - 1, 0x481, $"Rosto {faceBtn + 1}");
                    faceBtn++;
                }
            }

            bool female = _pm.OSUCreation.GenderFemale;

            // 16 cabelos: puxa direto do arquivo da cultura (IDs de GUMP).
            int[] hairList = null;
            if (cultureDef != null)
                hairList = female ? cultureDef.FemaleHairGumpIds : cultureDef.MaleHairGumpIds;

            // fallback (mantém a matemática antiga caso cultura esteja faltando)
            if (hairList == null || hairList.Length == 0)
            {
                string cid = _pm.OSUCreation.CultureId ?? "";

                int hairStart = 54000 + (female ? 64000 : 54000);

                hairList = new int[16];
                for (int i = 0; i < 16; i++)
                    hairList[i] = hairStart + i;
            }

            // 16 barbas: por padrão todas as culturas usam a mesma lista.
            int[] beardList = (cultureDef != null) ? cultureDef.MaleBeardGumpIds : null;

            if (beardList == null || beardList.Length == 0)
            {
                beardList = new int[16];
                for (int i = 0; i < 16; i++)
                    beardList[i] = 53500 + i;
            }

            // ====== CORES POR CULTURA ======
            // Aqui o gump pega as cores da cultura
            int[] colorList = showBeard ? BeardColorHues : HairColorHues;

            // area dos botões (16 slots)
            int[] hairRowY = { 589, 614, 639, 664, 689, 714, 739, 764 };
            int leftX = 338;
            int rightX = 496;

            for (int i = 0; i < 16; i++)
            {
                int x = (i < 8) ? leftX : rightX;
                int y = hairRowY[i % 8];

                bool selected = (!showBeard && _pm.OSUCreation.HairIndex == i) ||
                                (showBeard && _pm.OSUCreation.BeardIndex == i);

                int normal = selected ? 454 : 455;

                AddButton(x, y, normal, 454, (showBeard ? P6_BeardBase : P6_HairBase) + i, GumpButtonType.Reply, 0);

                if (!showBeard)
                    AddLabel(x + 26, y - 2, 0x481, $"Cabelo{i + 1}");
                else
                    AddLabel(x + 26, y - 2, 0x481, $"Barba{i + 1}");
            }

            // ====== Botões de cor (8 slots do layout) ======
            int colorX = 672;

            int colorCount = Math.Min(8, colorList.Length);

            for (int i = 0; i < colorCount; i++)
            {
                int hue = colorList[i];

                bool colorSelected = showBeard
                    ? (_pm.OSUCreation.BeardHue == hue)
                    : (_pm.OSUCreation.HairHue == hue);

                AddButton(colorX, hairRowY[i], colorSelected ? 454 : 455, 454, (showBeard ? P6_BeardColorBase : P6_HairColorBase) + i, GumpButtonType.Reply, 0);

                // label pintado com a cor real
                AddLabel(colorX + 26, hairRowY[i] - 2, hue, hue.ToString());
            }


            // ====== Preview: corpo + rosto + cabelo + barba ======
            int bodyGump = GetPreviewBodyGumpId();
            int faceGump = GetPreviewFaceGumpId(_pm.OSUCreation.FaceIndex);

            int px = 940;
            int py = 476;

            AddImage(px, py, bodyGump, _pm.OSUCreation.SkinHue);

            if (faceGump > 0)
                AddImage(px, py, faceGump, _pm.OSUCreation.SkinHue);

            int hairGump = GetPreviewHairGumpId(hairList, _pm.OSUCreation.HairIndex);
            if (hairGump > 0)
                AddImage(px, py, hairGump, _pm.OSUCreation.HairHue);

            // IMPORTANTE: barba aparece sempre no preview se for homem, mesmo na aba cabelo
            if (!_pm.OSUCreation.GenderFemale)
            {
                int beardGump = GetPreviewHairGumpId(beardList, _pm.OSUCreation.BeardIndex);
                if (beardGump > 0)
                    AddImage(px, py, beardGump, _pm.OSUCreation.BeardHue);
            }
        }

        private void BuildPage7_RP()
        {
            AddLabel(763, 157, 0x481, "Ficha RP");
            AddImageTiled(308, 174, 974, 21, 463);

            AddImageTiled(308, 463, 974, 21, 463);
            AddImageTiled(308, 508, 974, 21, 463);

            AddLabel(760, 487, 0x481, "Avatar");

            // divisórias verticais
            AddImageTiled(513, 190, 33, 281, 480);
            AddImageTiled(890, 189, 33, 280, 480);

            // labels topo
            AddLabel(381, 212, 0x481, "Peso (kg)");
            AddLabel(378, 285, 0x481, "Altura (cm)");
            AddLabel(394, 355, 0x481, "Idade");

            AddLabel(649, 208, 0x481, "Historia (so staff)");
            AddLabel(999, 208, 0x481, "Traços e Personalidade (visivel)");

            // entries (com valores atuais do context)
            AddTextEntry(319, 245, 151, 29, 0x481, P7_WeightEntry, _pm.OSUCreation.RpWeightKg > 0 ? _pm.OSUCreation.RpWeightKg.ToString() : "");
            AddTextEntry(319, 318, 151, 29, 0x481, P7_HeightEntry, _pm.OSUCreation.RpHeightCm > 0 ? _pm.OSUCreation.RpHeightCm.ToString() : "");
            AddTextEntry(319, 388, 151, 29, 0x481, P7_AgeEntry, _pm.OSUCreation.RpAge > 0 ? _pm.OSUCreation.RpAge.ToString() : "");

            // Botões de confirmação de peso, altura e idade
            AddButton(487, 249, _pm.OSUCreation.RpWeightSet ? 454 : 455, 454, P7_ConfirmWeight, GumpButtonType.Reply, 0);
            AddButton(487, 323, _pm.OSUCreation.RpHeightSet ? 454 : 455, 454, P7_ConfirmHeight, GumpButtonType.Reply, 0);
            AddButton(487, 394, _pm.OSUCreation.RpAgeSet ? 454 : 455, 454, P7_ConfirmAge, GumpButtonType.Reply, 0);

            // Historia e traços e personalidade
            AddTextEntry(551, 244, 304, 201, 0x481, P7_HistoryEntry, _pm.OSUCreation.RpHistoryStaff ?? "");
            AddButton(869, 325, _pm.OSUCreation.RpHistorySet ? 454 : 455, 454, P7_ConfirmHistory, GumpButtonType.Reply, 0);
            AddTextEntry(936, 245, 304, 201, 0x481, P7_TraitsEntry, _pm.OSUCreation.RpTraitsPublic ?? "");
            AddButton(1253, 325, _pm.OSUCreation.RpTraitsSet ? 454 : 455, 454, P7_ConfirmTraits, GumpButtonType.Reply, 0);

            if (!_pm.OSUCreation.RpAgeSet)
                return; // não mostra avatar até confirmar idade

            // paginação dos avatares
            AddButton(313, 637, 453, 5541, P7_AvatarPrev, GumpButtonType.Reply, 0);
            AddButton(1254, 637, 452, 5541, P7_AvatarNext, GumpButtonType.Reply, 0);

            // monta lista disponível (sem buracos)
            List<int> avatars = GetAvailableAvatarsForCurrentAgeAndGender();

            int maxPage = (avatars.Count == 0) ? 0 : (avatars.Count - 1) / P7_AvatarsPerPage;
            int page = _pm.OSUCreation.RpAvatarPage;
            if (page < 0) page = 0;
            if (page > maxPage) page = maxPage;
            _pm.OSUCreation.RpAvatarPage = page;

            int start = page * P7_AvatarsPerPage;

            // slots (posições do seu layout)
            int[] imgX = { 359, 579, 799, 1019 };
            int imgY = 545;

            int[] btnX = { 459, 682, 905, 1128 };
            int btnY = 761;

            // desenha até 4 avatares, sem slots vazios no meio
            for (int i = 0; i < 4; i++)
            {
                int idx = start + i;
                if (idx >= avatars.Count)
                    break;

                int avatarId = avatars[idx];

                AddImage(imgX[i], imgY, avatarId);

                bool selected = (_pm.OSUCreation.RpAvatarId == avatarId);
                int normal = selected ? 454 : 455;
                int pressed = 454;

                int btnId = (i == 0) ? P7_AvatarBtn1 :
                           (i == 1) ? P7_AvatarBtn2 :
                           (i == 2) ? P7_AvatarBtn3 : P7_AvatarBtn4;

                AddButton(btnX[i], btnY, normal, pressed, btnId, GumpButtonType.Reply, 0);
            }

            AddLabel(1158, 811, 0x481, "Finalizar");
        }

#endregion

        #region OnResponse

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_pm == null || _pm.Deleted)
                return;

            int bid = info.ButtonID;

            #region Pages

            // ===== Página 1 - botões =====
            if (bid == (int)Buttons.InfoOSU)
            {
                _pm.SendGump(new OSUCreationGump(_pm, 1, OSUCreationInfoTopic.OSU, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                return;
            }
            if (bid == (int)Buttons.InfoLore)
            {
                _pm.SendGump(new OSUCreationGump(_pm, 1, OSUCreationInfoTopic.LoreAmanti, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                return;
            }
            if (bid == (int)Buttons.InfoRegras)
            {
                _pm.SendGump(new OSUCreationGump(_pm, 1, OSUCreationInfoTopic.Regras, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                return;
            }

            // ===== Página 3 - seleção de povo =====
            if (_page == 3 && bid >= CultureSelectButtonBase && bid < CultureSelectButtonBase + 6)
            {
                int idx = bid - CultureSelectButtonBase;
                List<OSUCultureDefinition> cultures = OSUCultureRegistry.GetOrdered(6);

                if (idx >= 0 && idx < cultures.Count)
                {
                    _pm.OSUCreation.CultureId = cultures[idx].Id;
                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, OSUCultureInfoTopic.Lore, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
            }

            // ===== Página 3 - tópicos =====
            if (_page == 3)
            {
                if (bid == (int)Buttons.CultureLore)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, OSUCultureInfoTopic.Lore, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
                if (bid == (int)Buttons.CultureFisico)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, OSUCultureInfoTopic.Fisico, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
                if (bid == (int)Buttons.CulturePapeis)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, OSUCultureInfoTopic.Papeis, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
                if (bid == (int)Buttons.CultureTradicoes)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, OSUCultureInfoTopic.Tradicoes, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
                if (bid == (int)Buttons.CultureProverbios)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, OSUCultureInfoTopic.Proverbios, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
            }

            // ===== Página 4 - atributos (+ / -) =====
            if (_page == 4)
            {
                // INFO de atributo
                if (bid >= AttrInfoBase && bid < AttrInfoBase + 6)
                {
                    int idx = bid - AttrInfoBase;

                    // Mantém seu esquema: OSUAttributeTopic começa em 1
                    OSUAttributeTopic topic = (OSUAttributeTopic)(idx + 1);

                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, topic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // Minus
                if (bid >= AttrMinusBase && bid < AttrMinusBase + 6)
                {
                    int idx = bid - AttrMinusBase;
                    AdjustAttribute(idx, -1);
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // Plus
                if (bid >= AttrPlusBase && bid < AttrPlusBase + 6)
                {
                    int idx = bid - AttrPlusBase;
                    AdjustAttribute(idx, +1);
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // Minus grande (-5)
                if (bid >= AttrMinusBasePlus && bid < AttrMinusBasePlus + 6)
                {
                    int idx = bid - AttrMinusBasePlus;
                    AdjustAttribute(idx, -5);
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // Plus grande (+5)
                if (bid >= AttrPlusBasePlus && bid < AttrPlusBasePlus + 6)
                {
                    int idx = bid - AttrPlusBasePlus;
                    AdjustAttribute(idx, +5);
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }


                // Tabs DefQual
                if (bid == DQTabDefects)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, true, 0, null));
                    return;
                }
                if (bid == DQTabQualities)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, false, 0, null));
                    return;
                }

                // paginação DefQual
                if (bid == DQPrevPage)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage - 1, _page4SelectedDQId));
                    return;
                }
                if (bid == DQNextPage)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage + 1, _page4SelectedDQId));
                    return;
                }

                // selecionar DefQual na lista (AGORA 6 por página)
                if (bid >= DQSelectBase && bid < DQSelectBase + 6)
                {
                    int row = bid - DQSelectBase;

                    var all = OSUDefQualRegistry.GetAll();
                    var filtered = new List<OSUDefQualDefinition>();

                    for (int i = 0; i < all.Count; i++)
                    {
                        var d = all[i];
                        if (d == null) continue;

                        if (_page4ShowDefects && d.Type != OSUDefQualType.Defect) continue;
                        if (!_page4ShowDefects && d.Type != OSUDefQualType.Quality) continue;

                        filtered.Add(d);
                    }

                    int perPage = 6; // <-- AQUI
                    int start = _page4DQPage * perPage;
                    int idx = start + row;

                    if (idx >= 0 && idx < filtered.Count)
                    {
                        _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, filtered[idx].Id));
                        return;
                    }
                }

                // comprar/remover DefQual selecionado
                if (bid == DQBuyToggle)
                {
                    ToggleDefQual(_page4SelectedDQId);
                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }


            }

            // ===== Página 5 - Skills =====
            if (_page == 5)
            {
                if (bid >= ReligionSelectBase && bid < ReligionSelectBase + 10)
                {
                    int idx = bid - ReligionSelectBase;
                    var list = OSUReligionRegistry.GetAll();

                    if (idx >= 0 && idx < list.Count)
                    {
                        _pm.OSUCreation.ReligionId = list[idx].Id;

                        // Reabre a Página 5 para atualizar o HTML
                        _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }
                }


                // voltar para "O Sistema" (limpa o painel de skill selecionada)
                if (bid == P5_TabSystem)
                {
                    _pm.OSUCreation.Page5InfoSkill = null;
                    _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // trocar aba
                if (bid == P5_TabCombat)
                {
                    _pm.OSUCreation.Page5ShowCombat = true;
                    _pm.OSUCreation.Page5ListPage = 0;
                    _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P5_TabProf)
                {
                    _pm.OSUCreation.Page5ShowCombat = false;
                    _pm.OSUCreation.Page5ListPage = 0;
                    _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // paginação
                if (bid == P5_PrevPage)
                {
                    _pm.OSUCreation.Page5ListPage--;
                    _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P5_NextPage)
                {
                    _pm.OSUCreation.Page5ListPage++;
                    _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // clique em skill
                if (bid >= P5_SkillSelectBase && bid < P5_SkillSelectBase + P5_RowsPerPage)
                {
                    int row = bid - P5_SkillSelectBase;

                    bool showCombat = _pm.OSUCreation.Page5ShowCombat;

                    var group = showCombat ? Server.SkillXp.SkillXPSystem.OSUSkillGroup.Combat
                                           : Server.SkillXp.SkillXPSystem.OSUSkillGroup.Profession;

                    List<SkillName> list = GetSkillsForGroup(group);

                    int index = _pm.OSUCreation.Page5ListPage * P5_RowsPerPage + row;
                    if (index >= 0 && index < list.Count)
                    {
                        SkillName sk = list[index];

                        ToggleSkill(sk, showCombat);

                        // salva skill para mostrar no painel direito
                        _pm.OSUCreation.Page5InfoSkill = sk.ToString();

                        _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }
                }
            }

            // ===== Página 6 - Looks =====
            if (_page == 6)
            {
                bool showBeard = (!_pm.OSUCreation.GenderFemale && _pm.OSUCreation.ShowBeardTab);

                // salvar nome se veio
                var te = info.GetTextEntry(P6_NameEntry);
                if (te != null)
                    _pm.OSUCreation.ChosenName = te.Text;

                if (bid == P6_GenderMale)
                {
                    _pm.OSUCreation.GenderFemale = false;
                    _pm.OSUCreation.FaceIndex = 0;
                    _pm.OSUCreation.HairIndex = 0;
                    _pm.OSUCreation.BeardIndex = 0;
                    _pm.OSUCreation.HairGumpId = 0;
                    _pm.OSUCreation.ShowBeardTab = true;
                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P6_GenderFemale)
                {
                    _pm.OSUCreation.GenderFemale = true;
                    _pm.OSUCreation.FaceIndex = 0;
                    _pm.OSUCreation.HairIndex = 0;
                    _pm.OSUCreation.BeardIndex = 0;
                    _pm.OSUCreation.HairGumpId = 0;
                    _pm.OSUCreation.ShowBeardTab = false; // mulher não usa barba
                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P6_Body1)
                {
                    _pm.OSUCreation.BodyVariant = 0;
                    _pm.OSUCreation.FaceIndex = 0; // porque muda o range de gumps
                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    RefreshPaperdoll();
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P6_Body2)
                {
                    _pm.OSUCreation.BodyVariant = 1;
                    _pm.OSUCreation.FaceIndex = 0;
                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    RefreshPaperdoll();
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // pele
                if (bid >= P6_SkinBase && bid < P6_SkinBase + 20)
                {
                    int idx = bid - P6_SkinBase;

                    var cultureDef = OSUCultureRegistry.GetById(_pm.OSUCreation.CultureId);

                    // Usa os hues da raça/cultura; se não tiver, não faz nada
                    var skinHues = (cultureDef != null) ? cultureDef.SkinHues : null;

                    if (skinHues != null && idx >= 0 && idx < skinHues.Length)
                        _pm.OSUCreation.SkinHue = skinHues[idx];

                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // rosto (placeholder: por enquanto só guarda índice, sem gump real)
                // rosto
                if (bid >= P6_FaceBase && bid < P6_FaceBase + 8)
                {
                    _pm.OSUCreation.FaceIndex = bid - P6_FaceBase;

                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    RefreshPaperdoll();

                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // cabelo e barba

                if (bid >= P6_HairBase && bid < P6_HairBase + 16)
                {
                    int idx = bid - P6_HairBase;
                    _pm.OSUCreation.HairIndex = idx;

                    var culture = OSUCultureRegistry.GetById(_pm.OSUCreation.CultureId);
                    bool femaleHair = _pm.OSUCreation.GenderFemale;
                    var list = (culture != null) ? (femaleHair ? culture.FemaleHairGumpIds : culture.MaleHairGumpIds) : null;

                    if (list != null && idx >= 0 && idx < list.Length)
                        _pm.OSUCreation.HairGumpId = list[idx];
                    else
                        _pm.OSUCreation.HairGumpId = (femaleHair ? 64000 : 54000) + idx;

                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (!_pm.OSUCreation.GenderFemale && bid >= P6_BeardBase && bid < P6_BeardBase + 16)
                {
                    int idx = bid - P6_BeardBase;
                    _pm.OSUCreation.BeardIndex = idx;
                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }


                if (bid == P6_TabHair)
                {
                    _pm.OSUCreation.ShowBeardTab = false;
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P6_TabBeard && !_pm.OSUCreation.GenderFemale)
                {
                    _pm.OSUCreation.ShowBeardTab = true;
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // cor cabelo
                if (bid >= P6_HairColorBase && bid < P6_HairColorBase + 8)
                {
                    int idx = bid - P6_HairColorBase;

                    var cultureDef = OSUCultureRegistry.GetById(_pm.OSUCreation.CultureId);

                    int[] colorList = showBeard ? cultureDef.BeardColorHues : cultureDef.HairColorHues;

                    if (idx >= 0 && idx < colorList.Length)
                    {
                        if (showBeard)
                            _pm.OSUCreation.BeardHue = colorList[idx];
                        else
                            _pm.OSUCreation.HairHue = colorList[idx];
                    }

                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // barba só se homem
                if (bid >= P6_BeardColorBase && bid < P6_BeardColorBase + 8)
                {
                    int idx = bid - P6_BeardColorBase;

                    var culture = OSUCultureRegistry.GetById(_pm.OSUCreation.CultureId);
                    int[] beardHues = (culture != null && culture.BeardColorHues != null && culture.BeardColorHues.Length > 0)
                        ? culture.BeardColorHues
                        : culture.BeardHues; // fallback se você tiver

                    if (idx >= 0 && idx < beardHues.Length)
                        _pm.OSUCreation.BeardHue = beardHues[idx];

                    _pm.OSUCreation.ShowBeardTab = true; // mantém aba de barba
                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
            }

            // RP
            if (_page == 7)
            {
                // salva textos sempre que clicar em qualquer coisa na page 7
                SavePage7Entries(info);

                if (bid == P7_ConfirmWeight)
                {
                    SavePage7Entries(info);

                    if (_pm.OSUCreation.RpWeightKg < 1 || _pm.OSUCreation.RpWeightKg > 140)
                        _pm.SendMessage(0x35, "Peso inválido. Use 1 a 140 kg.");
                    else
                        _pm.OSUCreation.RpWeightSet = true;

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                    return;
                }

                if (bid == P7_ConfirmHeight)
                {
                    SavePage7Entries(info);

                    if (_pm.OSUCreation.RpHeightCm < 1 || _pm.OSUCreation.RpHeightCm > 200)
                        _pm.SendMessage(0x35, "Altura inválida. Use 1 a 200 cm.");
                    else
                        _pm.OSUCreation.RpHeightSet = true;

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                    return;
                }

                if (bid == P7_ConfirmAge)
                {
                    SavePage7Entries(info);

                    if (_pm.OSUCreation.RpAge < 1 || _pm.OSUCreation.RpAge > 70)
                    {
                        _pm.SendMessage(0x35, "Idade inválida. Use 1 a 70 anos.");
                    }
                    else
                    {
                        _pm.OSUCreation.RpAgeSet = true;
                        _pm.OSUCreation.RpAvatarPage = 0; // reseta paginação porque a lista muda por idade
                        _pm.OSUCreation.RpAvatarId = 0;   // força escolher avatar novamente
                    }

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                    return;
                }

                if (bid == P7_ConfirmHistory)
                {
                    SavePage7Entries(info);

                    if (string.IsNullOrWhiteSpace(_pm.OSUCreation.RpHistoryStaff))
                        _pm.SendMessage(0x35, "Preencha a história antes de confirmar.");
                    else
                        _pm.OSUCreation.RpHistorySet = true;

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                    return;
                }

                if (bid == P7_ConfirmTraits)
                {
                    SavePage7Entries(info);

                    if (string.IsNullOrWhiteSpace(_pm.OSUCreation.RpTraitsPublic))
                        _pm.SendMessage(0x35, "Preencha os traços antes de confirmar.");
                    else
                        _pm.OSUCreation.RpTraitsSet = true;

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                    return;
                }


                // paginação
                if (bid == P7_AvatarPrev)
                {
                    _pm.OSUCreation.RpAvatarPage--;
                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (bid == P7_AvatarNext)
                {
                    _pm.OSUCreation.RpAvatarPage++;
                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                // selecionar avatar (slot 1..4)
                if (bid == P7_AvatarBtn1 || bid == P7_AvatarBtn2 || bid == P7_AvatarBtn3 || bid == P7_AvatarBtn4)
                {
                    List<int> avatars = GetAvailableAvatarsForCurrentAgeAndGender();

                    int page = _pm.OSUCreation.RpAvatarPage;
                    if (page < 0) page = 0;

                    int start = page * P7_AvatarsPerPage;
                    int slot = (bid == P7_AvatarBtn1) ? 0 :
                               (bid == P7_AvatarBtn2) ? 1 :
                               (bid == P7_AvatarBtn3) ? 2 : 3;

                    int idx = start + slot;
                    if (idx >= 0 && idx < avatars.Count)
                        _pm.OSUCreation.RpAvatarId = avatars[idx];

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
            }

            #endregion

            #region Navegação

            // ===== Navegação: Voltar =====
            if (bid == (int)Buttons.Prev)
            {
                if (_page == 1)
                {
                    _pm.CloseGump(typeof(OSUCreationGump));
                    _pm.SendMessage(0x35, "Seu personagem não foi criado, você não vai conseguir entrar em Amanti.");
                    return;
                }

                _pm.SendGump(new OSUCreationGump(_pm, _page - 1, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                return;
            }

            // ===== Navegação: Avançar =====
            if (bid == (int)Buttons.Next)
            {
                if (_page == 1)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, 2, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (_page == 2)
                {
                    bool choseWarrior = info.IsSwitched((int)Switches.PathWarrior);
                    bool choseArtisan = info.IsSwitched((int)Switches.PathArtisan);
                    bool chosePvp = info.IsSwitched((int)Switches.ModePvp);
                    bool choseNoPvp = info.IsSwitched((int)Switches.ModeNoPvp);

                    if ((!choseWarrior && !choseArtisan) || (!chosePvp && !choseNoPvp))
                    {
                        _pm.SendMessage(0x35, "Você precisa escolher um Caminho e um Game Mode para continuar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 2, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    _pm.OSUCreation.Path = choseWarrior ? OSUCreationPath.Warrior : OSUCreationPath.Artisan;
                    _pm.OSUCreation.GameMode = chosePvp ? OSUCreationGameMode.Pvp : OSUCreationGameMode.NoPvp;
                    _pm.OSUCreation.ApplyPathCaps();

                    _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (_page == 3)
                {
                    if (String.IsNullOrWhiteSpace(_pm.OSUCreation.CultureId))
                    {
                        _pm.SendMessage(0x35, "Você precisa escolher um Povo para continuar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 3, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (_page == 4)
                {
                    // Obrigatório: distribuir TODOS os pontos
                    if (RemainingAttrPoints(_pm.OSUCreation) != 0)
                    {
                        _pm.SendMessage(0x35, "Você precisa distribuir todos os {PointsTotal} pontos de atributos para continuar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 4, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }
                    _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (_page == 5)
                {
                    int c = _pm.OSUCreation.StartingCombatSkills == null ? 0 : _pm.OSUCreation.StartingCombatSkills.Count;
                    int p = _pm.OSUCreation.StartingProfessionSkills == null ? 0 : _pm.OSUCreation.StartingProfessionSkills.Count;

                    if (c != 2 || p != 2)
                    {
                        _pm.SendMessage(0x35, "Você precisa escolher 2 skills de Combate e 2 de Profissão para continuar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    
                    // Obrigatório: escolher religião (inclui Sem Deus)
                    if (String.IsNullOrWhiteSpace(_pm.OSUCreation.ReligionId))
                    {
                        _pm.SendMessage(0x35, "Você precisa escolher um deus (ou Sem Deus) para continuar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 5, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }



                    _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }

                if (_page == 6)
                {
                    TextRelay te = info.GetTextEntry(P6_NameEntry);
                    if (te != null)
                        _pm.OSUCreation.ChosenName = te.Text;

                    // OBRIGATÓRIO: nome
                    if (String.IsNullOrWhiteSpace(_pm.OSUCreation.ChosenName))
                    {
                        _pm.SendMessage(0x35, "Você precisa escolher um nome para continuar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 6, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    if (IsNameTaken(_pm, _pm.OSUCreation.ChosenName))
                    {
                        _pm.SendMessage(0x35, "Esse nome já está sendo usado. Escolha outro.");
                        return;
                    }

                    OSUCreationFinalizer.ApplyAppearance(_pm, _pm.OSUCreation);

                    _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                    return;
                }


                if (_page == 7)
                {
                    SavePage7Entries(info);

                    SavePage7Entries(info);

                    if (!_pm.OSUCreation.RpWeightSet || !_pm.OSUCreation.RpHeightSet || !_pm.OSUCreation.RpAgeSet)
                    {
                        _pm.SendMessage(0x35, "Confirme Peso, Altura e Idade antes de finalizar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                        return;
                    }

                    if (!_pm.OSUCreation.RpHistorySet)
                    {
                        _pm.SendMessage(0x35, "Confirme a História antes de finalizar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                        return;
                    }

                    if (!_pm.OSUCreation.RpTraitsSet)
                    {
                        _pm.SendMessage(0x35, "Confirme os Traços e Personalidade antes de finalizar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                        return;
                    }

                    if (_pm.OSUCreation.RpAvatarId <= 0)
                    {
                        _pm.SendMessage(0x35, "Escolha um avatar para finalizar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic));
                        return;
                    }

                    // valida limites
                    if (_pm.OSUCreation.RpWeightKg < 1 || _pm.OSUCreation.RpWeightKg > 140)
                    {
                        _pm.SendMessage(0x35, "Peso inválido. Use 1 a 140 kg.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    if (_pm.OSUCreation.RpHeightCm < 1 || _pm.OSUCreation.RpHeightCm > 200)
                    {
                        _pm.SendMessage(0x35, "Altura inválida. Use 1 a 200 cm.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    if (_pm.OSUCreation.RpAge < 1 || _pm.OSUCreation.RpAge > 70)
                    {
                        _pm.SendMessage(0x35, "Idade inválida. Use 1 a 70 anos.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    if (_pm.OSUCreation.RpAvatarId <= 0)
                    {
                        _pm.SendMessage(0x35, "Você precisa escolher um avatar para finalizar.");
                        _pm.SendGump(new OSUCreationGump(_pm, 7, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                        return;
                    }

                    // por enquanto só confirma (portal vem depois)
                    _pm.SendMessage(0x35, "Ficha RP salva. Para entrar em Amanti, vá até o portal.");
                    _pm.CloseGump(typeof(OSUCreationGump));
                    return;
                }

                if (_page < TotalPages)
                {
                    _pm.SendGump(new OSUCreationGump(_pm, _page + 1, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                    return;
                }
              
                _pm.SendMessage(0x35, "Criação ainda não finalizada (páginas futuras não implementadas).");
                _pm.SendGump(new OSUCreationGump(_pm, TotalPages, _page1Topic, _page3Topic, _page4AttrTopic, _page4ShowDefects, _page4DQPage, _page4SelectedDQId));
                return;

            }

            #endregion
        }

        #endregion

        #region Helpers

        //----------------------------------------------------------------------
        //============================   HELPERS   =============================
        //----------------------------------------------------------------------

        private void SavePage7Entries(RelayInfo info)
        {
            int w, h, a;

            TextRelay tw = info.GetTextEntry(P7_WeightEntry);
            if (tw != null && Int32.TryParse(tw.Text, out w))
                _pm.OSUCreation.RpWeightKg = w;

            TextRelay th = info.GetTextEntry(P7_HeightEntry);
            if (th != null && Int32.TryParse(th.Text, out h))
                _pm.OSUCreation.RpHeightCm = h;

            TextRelay ta = info.GetTextEntry(P7_AgeEntry);
            if (ta != null && Int32.TryParse(ta.Text, out a))
                _pm.OSUCreation.RpAge = a;

            TextRelay hist = info.GetTextEntry(P7_HistoryEntry);
            if (hist != null)
                _pm.OSUCreation.RpHistoryStaff = hist.Text;

            TextRelay tr = info.GetTextEntry(P7_TraitsEntry);
            if (tr != null)
                _pm.OSUCreation.RpTraitsPublic = tr.Text;
        }

        private void BuildPage()
        {
            switch (_page)
            {
                case 1: BuildPage1_Info(); break;
                case 2: BuildPage2_PathAndMode(); break;
                case 3: BuildPage3_Cultures(); break;
                case 4: BuildPage4_Attributes_DefQual(); break;
                case 5: BuildPage5_Skills(); break;
                case 6: BuildPage6_Appearance(); break;
                case 7: BuildPage7_RP(); break;
            }
        }

        // ===== Helpers Page4 =====
        private int CostForValue(int v)
        {
            if (v <= 0) return 0;
            if (v <= 100) return v;
            return 100 + (v - 100) * 3;
        }

        private int TotalAttrCost(OSUCreationContext ctx)
        {
            // custo relativo: (custo atual - custo do valor inicial 15)
            int baseCost = CostForValue(AttrStart);

            return
                (CostForValue(ctx.Attr_Str) - baseCost) +
                (CostForValue(ctx.Attr_Dex) - baseCost) +
                (CostForValue(ctx.Attr_Int) - baseCost) +
                (CostForValue(ctx.Attr_HP) - baseCost) +
                (CostForValue(ctx.Attr_Vit) - baseCost) +
                (CostForValue(ctx.Attr_Man) - baseCost);
        }

        private static bool IsNameTaken(PlayerMobile me, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return true;

            name = name.Trim();

            foreach (var m in World.Mobiles.Values)
            {
                var pm = m as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (pm == me)
                    continue;

                if (String.Equals(pm.Name, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        private int RemainingAttrPoints(OSUCreationContext ctx)
        {
            int used = TotalAttrCost(ctx);
            int rem = PointsTotal - used;
            return rem < 0 ? 0 : rem;
        }

        private int GetMajorCapBase(OSUCreationContext ctx)
        {
            // cap maior é o de 50k dependendo do caminho
            return (ctx.Path == OSUCreationPath.Warrior) ? ctx.CombatCap : ctx.ProfCap;
        }

        private int GetMajorCapAfterDefQual(OSUCreationContext ctx)
        {
            int baseCap = GetMajorCapBase(ctx);
            int delta = 0;

            if (ctx.SelectedDefQualIds != null)
            {
                for (int i = 0; i < ctx.SelectedDefQualIds.Count; i++)
                {
                    var def = OSUDefQualRegistry.GetById(ctx.SelectedDefQualIds[i]);
                    if (def != null)
                        delta += def.CapDelta;
                }
            }

            return baseCap + delta;
        }
        private List<int> GetAvailableAvatarsForCurrentAgeAndGender()
        {
            // idade padrão se ainda não preencheu
            int age = _pm.OSUCreation.RpAge;
            if (age <= 0) age = 18;
            if (age > 70) age = 70;

            bool female = _pm.OSUCreation.GenderFemale;

            int startId, endId;

            if (!female)
            {
                if (age <= 35) { startId = 667; endId = 746; }
                else if (age <= 55) { startId = 747; endId = 796; }
                else { startId = 797; endId = 826; }
            }
            else
            {
                // feminino começa em 827 e repete os mesmos blocos (+160 no total)
                // jovens: 827..906 (80)
                // meia idade: 907..956 (50)
                // velhas: 957..986 (30)
                if (age <= 35) { startId = 827; endId = 906; }
                else if (age <= 55) { startId = 907; endId = 956; }
                else { startId = 957; endId = 986; }
            }

            List<int> list = new List<int>();

            for (int id = startId; id <= endId; id++)
            {
                // aqui é onde no futuro a gente vai filtrar "já foi usado no mundo"
                // por enquanto, deixo um hook:
                if (OSUAvatarRegistry.IsUsedByOther(_pm, id))
                    continue;

                list.Add(id);
            }

            return list;
        }

        private int GetPreviewBodyGumpId()
        {
            // Seus IDs finais:
            // masculino: corpo1=122, corpo2=131
            // feminino:  corpo1=140, corpo2=149
            bool female = _pm.OSUCreation.GenderFemale;
            bool alt = (_pm.OSUCreation.BodyVariant == 1);

            if (female)
                return alt ? 149 : 140;

            return alt ? 131 : 122;
        }

        private int GetPreviewFaceGumpId(int faceIndex)
        {
            if (faceIndex < 0) faceIndex = 0;
            if (faceIndex > 7) faceIndex = 7;

            bool female = _pm.OSUCreation.GenderFemale;
            bool alt = (_pm.OSUCreation.BodyVariant == 1);

            // masculino: type1 123..130, type2 132..139
            // feminino:  type1 141..148, type2 150..157
            int baseId;

            if (!female)
                baseId = alt ? 132 : 123;
            else
                baseId = alt ? 150 : 141;

            return baseId + faceIndex;
        }

        private int GetPreviewHairGumpId(int[] list, int idx)
        {
            if (list == null || list.Length == 0)
                return 0;

            if (idx < 0) idx = 0;
            if (idx >= list.Length) idx = 0;

            return list[idx];
        }
        private void EnsureAttrInitialized(OSUCreationContext ctx)
        {
            // se ainda não inicializou (ou veio zerado), começa tudo em 15
            if (ctx.Attr_Str < AttrMin) ctx.Attr_Str = AttrStart;
            if (ctx.Attr_Dex < AttrMin) ctx.Attr_Dex = AttrStart;
            if (ctx.Attr_Int < AttrMin) ctx.Attr_Int = AttrStart;
            if (ctx.Attr_HP < AttrMin) ctx.Attr_HP = AttrStart;
            if (ctx.Attr_Vit < AttrMin) ctx.Attr_Vit = AttrStart;
            if (ctx.Attr_Man < AttrMin) ctx.Attr_Man = AttrStart;

            // só por segurança
            if (ctx.Attr_Str > AttrMax) ctx.Attr_Str = AttrMax;
            if (ctx.Attr_Dex > AttrMax) ctx.Attr_Dex = AttrMax;
            if (ctx.Attr_Int > AttrMax) ctx.Attr_Int = AttrMax;
            if (ctx.Attr_HP > AttrMax) ctx.Attr_HP = AttrMax;
            if (ctx.Attr_Vit > AttrMax) ctx.Attr_Vit = AttrMax;
            if (ctx.Attr_Man > AttrMax) ctx.Attr_Man = AttrMax;
        }

        private List<SkillName> GetSkillsForGroup(Server.SkillXp.SkillXPSystem.OSUSkillGroup group)
        {
            List<SkillName> list = new List<SkillName>();

            Array values = Enum.GetValues(typeof(SkillName));
            for (int i = 0; i < values.Length; i++)
            {
                SkillName sk = (SkillName)values.GetValue(i);

                if (Server.SkillXp.SkillXPSystem.GetSkillGroup(sk) == group)
                    list.Add(sk);
            }

            // ordena por nome
            list.Sort((a, b) => String.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase));
            return list;
        }
       

        private List<string> GetSelectedList(bool combat)
        {
            if (combat)
            {
                if (_pm.OSUCreation.StartingCombatSkills == null)
                    _pm.OSUCreation.StartingCombatSkills = new List<string>();
                return _pm.OSUCreation.StartingCombatSkills;
            }
            else
            {
                if (_pm.OSUCreation.StartingProfessionSkills == null)
                    _pm.OSUCreation.StartingProfessionSkills = new List<string>();
                return _pm.OSUCreation.StartingProfessionSkills;
            }
        }

        private bool IsSkillSelected(SkillName sk, bool combat)
        {
            var list = GetSelectedList(combat);
            string key = sk.ToString();

            for (int i = 0; i < list.Count; i++)
                if (string.Equals(list[i], key, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private void ToggleSkill(SkillName sk, bool combat)
        {
            var list = GetSelectedList(combat);
            string key = sk.ToString();

            // se já tem, remove
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    list.RemoveAt(i);
                    return;
                }
            }

            // se não tem, tenta adicionar (máx 2)
            if (list.Count >= 2)
            {
                _pm.SendMessage(0x35, "Você só pode escolher 2 skills nesta categoria.");
                return;
            }

            list.Add(key);
        }

        private string BuildSkillInfoHtml(SkillName? skill)
        {
            if (!skill.HasValue)
            {
                return @"<BASEFONT COLOR=#FFFFFF>
                 Clique em uma skill para ver a descrição e as feats disponíveis.
                 </BASEFONT>";
            }

            SkillName sk = skill.Value;

            // pega feats do teu sistema SkillXP
            // (isso funciona porque o OSUFeatSystem guarda feats por SkillName)
            var feats = Server.Custom.Systems.SkillXP.Engine.OSUFeatSystem.GetFeats(sk);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<BASEFONT COLOR=#FFFFFF>");
            sb.Append("<CENTER><B>").Append(sk.ToString()).Append("</B></CENTER><BR><BR>");

            sb.Append("<B>Feats disponíveis:</B><BR>");

            if (feats == null || feats.Count == 0)
            {
                sb.Append("Nenhuma feat cadastrada para esta skill.<BR>");
            }
            else
            {
                for (int i = 0; i < feats.Count; i++)
                {
                    var f = feats[i];
                    if (f != null)
                        sb.Append("• ").Append(f.Name).Append("<BR>");
                }
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        private int GetEffectiveAttrMax(OSUCreationAttribute attr)
        {
            int max = AttrMax; // seu max padrão (115)

            var list = _pm.OSUCreation.SelectedDefQualIds;
            if (list == null)
                return max;

            for (int i = 0; i < list.Count; i++)
            {
                var dq = OSUDefQualRegistry.GetById(list[i]);
                if (dq != null)
                    max = dq.GetAttributeMax(_pm.OSUCreation, attr, max);
            }

            return max;
        }

        private OSUCreationAttribute MapAttr(int idx)
        {
            switch (idx)
            {
                default:
                case 0: return OSUCreationAttribute.Str;
                case 1: return OSUCreationAttribute.Dex;
                case 2: return OSUCreationAttribute.Int;
                case 3: return OSUCreationAttribute.HP;
                case 4: return OSUCreationAttribute.Vit;
                case 5: return OSUCreationAttribute.Mana;
            }
        }

        private int CostForOneStep(int currentValue)
        {
            // regra: se o valor atual já está em 100 ou mais, o próximo +1 custa 3
            return (currentValue >= 100) ? 3 : 1;
        }

        private int TotalIncreaseCost(int startValue, int steps)
        {
            int cost = 0;
            for (int i = 0; i < steps; i++)
                cost += CostForOneStep(startValue + i);

            return cost;
        }

        private void AdjustAttribute(int idx, int delta)
        {
            int v = GetAttrValue(idx);

            if (delta > 0)
            {
                int rem = RemainingAttrPoints(_pm.OSUCreation);

                OSUCreationAttribute a = MapAttr(idx);
                int max = GetEffectiveAttrMax(a);

                if (v >= max)
                    return;

                int stepsWanted = delta; // 1 ou 5
                int stepsPossibleByMax = max - v;
                int steps = stepsWanted > stepsPossibleByMax ? stepsPossibleByMax : stepsWanted;

                if (steps <= 0)
                    return;

                int cost = TotalIncreaseCost(v, steps);

                if (rem < cost)
                {
                    // mensagem amigável: explica por que não deu pra subir 5
                    _pm.SendMessage(0x35, $"Você não tem pontos suficientes para aumentar {stepsWanted} pontos aqui. (Custo: {cost}, Restantes: {rem})");
                    return;
                }

                SetAttrValue(idx, v + steps);
                return;
            }

            if (delta < 0)
            {
                int stepsWanted = -delta; // 1 ou 5
                int newVal = v - stepsWanted;

                if (newVal < AttrMin)
                    newVal = AttrMin;

                if (newVal == v)
                    return;

                SetAttrValue(idx, newVal);
            }
        }



        private int GetAttrValue(int idx)
        {
            switch (idx)
            {
                default:
                case 0: return _pm.OSUCreation.Attr_Str;
                case 1: return _pm.OSUCreation.Attr_Dex;
                case 2: return _pm.OSUCreation.Attr_Int;
                case 3: return _pm.OSUCreation.Attr_HP;
                case 4: return _pm.OSUCreation.Attr_Vit;
                case 5: return _pm.OSUCreation.Attr_Man;
            }
        }

        private void SetAttrValue(int idx, int val)
        {
            if (val < AttrMin) val = AttrMin;
            if (val > AttrMax) val = AttrMax;

            switch (idx)
            {
                case 0: _pm.OSUCreation.Attr_Str = val; break;
                case 1: _pm.OSUCreation.Attr_Dex = val; break;
                case 2: _pm.OSUCreation.Attr_Int = val; break;
                case 3: _pm.OSUCreation.Attr_HP = val; break;
                case 4: _pm.OSUCreation.Attr_Vit = val; break;
                case 5: _pm.OSUCreation.Attr_Man = val; break;
            }
        }


        private void ToggleDefQual(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return;

            var def = OSUDefQualRegistry.GetById(id);
            if (def == null)
                return;

            if (_pm.OSUCreation.SelectedDefQualIds == null)
                _pm.OSUCreation.SelectedDefQualIds = new List<string>();

            var set = new HashSet<string>(_pm.OSUCreation.SelectedDefQualIds, StringComparer.OrdinalIgnoreCase);

            // remover
            if (set.Contains(id))
            {
                _pm.OSUCreation.SelectedDefQualIds.RemoveAll(x => String.Equals(x, id, StringComparison.OrdinalIgnoreCase));
                return;
            }

            // bloqueado por oposto -> nem tenta comprar (e normalmente o botão já nem aparece)
            if (def.IsBlockedBySelection(set))
            {
                _pm.SendMessage(0x35, "Você não pode comprar isso porque é incompatível com outra escolha.");
                return;
            }

            // requisitos do próprio def/qual (e.g. HP > 105 no Frágil)
            string reason;
            if (!def.CanBePurchased(_pm.OSUCreation, set, out reason))
            {
                _pm.SendMessage(0x35, String.IsNullOrWhiteSpace(reason) ? "Você não pode comprar isso." : reason);
                return;
            }

            // regra global de caps: após compra precisa ficar entre 40k e 70k
            int after = GetMajorCapAfterDefQual(_pm.OSUCreation) + def.CapDelta;

            if (after < MinMajorCap)
            {
                _pm.SendMessage(0x35, "Você não pode ficar com menos de 40k de cap.");
                return;
            }

            if (after > MaxMajorCap)
            {
                _pm.SendMessage(0x35, "Você não pode ficar com mais de 70k de cap.");
                return;
            }

            // compra
            _pm.OSUCreation.SelectedDefQualIds.Add(id);
        }

        private void RefreshPaperdoll()
        {
            if (_pm == null || _pm.NetState == null)
                return;

            string title = (_pm.Name ?? "") + String.Format(" [OSUPD:{0}:{1}]",
                _pm.OSUCreation.BodyVariant,
                _pm.OSUCreation.FaceIndex);

            _pm.NetState.Send(new DisplayPaperdoll(_pm, title, true));
        }

        #endregion

    }
}
