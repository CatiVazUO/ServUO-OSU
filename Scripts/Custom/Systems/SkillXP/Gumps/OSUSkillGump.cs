using Server;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.Hotbar;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.SkillXp;
using System;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class OSUSkillGump : OSUBaseGump
    {
        // ============================================================
        //  CONFIG / CONSTANTES
        // ============================================================
        public const int SkillXPCap = 10000;
        public const int SkillsPerPage = 8;
        private const int AbilitiesPerPage = 8;

        // Button ranges
        private const int BuySkillButtonBase = 1000;        // + (int)SkillName
        private const int LockSkillButtonBase = 3000;       // + (int)SkillName (lock/unlock)
        private const int AbilitySelectButtonBase = 5000;   // + index em OSUAbilitySystem.GetAll()

        // ============================================================
        //  HUB VIEW
        // ============================================================
        public enum HubView
        {
            Skills = 0,
            FeatBuy = 1,
            AbilitiesList = 2,
            AbilityBuy = 3
        }

        // ============================================================
        //  STATE
        // ============================================================
        private readonly Mobile m_Viewer;
        private readonly PlayerMobile m_Target;

        private readonly HubView m_View;

        // Skills view
        private readonly int m_SkillsTab;   // 0=Combate, 1=Profissão
        private readonly int m_SkillsPage;

        // Feat buy
        private readonly SkillName m_FeatSkill;
        private readonly int m_FeatIndex;
        private readonly bool m_FeatConfirm;     // ✅ agora existe de verdade

        // Abilities list
        private readonly int m_AbilitiesPage;

        // Ability buy
        private readonly int m_AbilityIndex;
        private readonly bool m_AbilityConfirm;

        // ============================================================
        //  CTOR
        // ============================================================
        public OSUSkillGump(
            Mobile viewer,
            PlayerMobile target,

            int skillsTab = 0,
            int skillsPage = 0,

            HubView view = HubView.Skills,

            // feat buy
            SkillName featSkill = SkillName.Alchemy,
            int featIndex = 0,
            bool featConfirm = false,          // ✅ ADICIONADO

            // abilities
            int abilitiesPage = 0,

            // ability buy
            int abilityIndex = 0,
            bool abilityConfirm = false
        )
            : base(0, 0)
        {
            m_Viewer = viewer;
            m_Target = target;

            m_View = view;

            m_SkillsTab = skillsTab;
            m_SkillsPage = skillsPage;

            m_FeatSkill = featSkill;
            m_FeatIndex = featIndex;
            m_FeatConfirm = featConfirm;

            m_AbilitiesPage = abilitiesPage;

            m_AbilityIndex = abilityIndex;
            m_AbilityConfirm = abilityConfirm;

            if (m_Viewer == null || m_Target == null)
                return;

            Build();
        }

        // ============================================================
        //  REFRESH
        // ============================================================
        public static void Refresh(PlayerMobile pm)
        {
            if (pm == null)
                return;

            if (!pm.HasGump(typeof(OSUSkillGump)))
                return;

            pm.CloseGump(typeof(OSUSkillGump));
            pm.SendGump(new OSUSkillGump(pm, pm, 0, 0, HubView.Skills));
        }

        // ============================================================
        //  DESCRIPTION das Habilidades e Feats
        // ============================================================

        private static string BuildReqDesc(string requirement, string description)
        {
            string desc = description ?? "";

            if (!string.IsNullOrEmpty(requirement))
                return "Requerimento: " + requirement + "\n\nDescrição: " + desc;

            return "Descrição: " + desc;
        }

        // ============================================================
        //  BUILD
        // ============================================================
        private void Build()
        {
            if (m_View == HubView.Skills)
            {
                if (m_SkillsTab == 0)
                {
                    AddLabel(712, 310, LabelHue, "Skills de Combate");
                }
                else
                    AddLabel(703, 310, LabelHue, "Skills de Profissões");
            }
            else if (m_View == HubView.AbilitiesList)
            {
                AddLabel(729, 310, LabelHue, "Habilidades");
            }
            else if (m_View == HubView.FeatBuy)
            {
                AddLabel(712, 310, LabelHue, "Compra de Feats");
            }
            else if (m_View == HubView.AbilityBuy)
            {
                AddLabel(698, 310, LabelHue, "Compra de Habilidades");
            }

            // Tabs só em telas de lista
            if (m_View == HubView.Skills || m_View == HubView.AbilitiesList)
                DrawTabs();

            // Rodapé fixo
            DrawFooter();

            // Conteúdo
            switch (m_View)
            {
                case HubView.Skills:
                    DrawSkillsView();
                    break;

                case HubView.FeatBuy:
                    DrawFeatBuyView();
                    break;

                case HubView.AbilitiesList:
                    DrawAbilitiesListView();
                    break;

                case HubView.AbilityBuy:
                    DrawAbilityBuyView();
                    break;
            }
        }

        // ============================================================
        //  UI: TABS
        // ============================================================
        private void DrawTabs()
        {
            bool combateAtivo = (m_View == HubView.Skills && m_SkillsTab == 0);
            bool profissaoAtivo = (m_View == HubView.Skills && m_SkillsTab == 1);
            bool habilidadeAtivo = (m_View == HubView.AbilitiesList);

            int combateArt = combateAtivo ? 440 : 442;
            int profissaoArt = profissaoAtivo ? 440 : 442;
            int habilidadeArt = habilidadeAtivo ? 440 : 442;

            AddButton(564, 360, combateArt, combateArt, (int)Buttons.TabCombate, GumpButtonType.Reply, 0);
            AddLabel(609, 370, LabelHue, "Combate");

            AddButton(702, 360, profissaoArt, profissaoArt, (int)Buttons.TabProfissao, GumpButtonType.Reply, 0);
            AddLabel(747, 370, LabelHue, "Profissao");

            AddButton(847, 360, habilidadeArt, habilidadeArt, (int)Buttons.TabHabilidade, GumpButtonType.Reply, 0);
            AddLabel(892, 370, LabelHue, "Habilidade");
        }

        // ============================================================
        //  UI: FOOTER
        // ============================================================

        private void DrawFooter()
        {
            int nextlvl = m_Target.OSULevel + 1;

            AddLabel(629, 822, LabelHue, "Nível: " + m_Target.OSULevel);
            AddLabel(737, 822, LabelHue, "Nivel " + nextlvl + ": " + m_Target.OSUGeneralXP + "/" + m_Target.OSUNextLevelXP);

            // Pontos gastos por categoria (mostrar somente a categoria do tab/view atual)
            bool showCombat = false;
            bool showProf = false;

            if (m_View == HubView.Skills)
            {
                showCombat = (m_SkillsTab == 0);
                showProf = (m_SkillsTab == 1);

                var group = (m_SkillsTab == 0) ? SkillXPSystem.OSUSkillGroup.Combat : SkillXPSystem.OSUSkillGroup.Profession;

                double total = SkillXPSystem.GetTotalSkillValueForGroup(m_Target, group);
                double cap = SkillXPSystem.GetGroupCap(group);

                string label = (group == SkillXPSystem.OSUSkillGroup.Combat)
                    ? $"Cap Comb: {total:F1}/{cap:F1}"
                    : $"Cap Prof: {total:F1}/{cap:F1}";

                AddLabel(564, 772, LabelHue, label);
            }
            else if (m_View == HubView.FeatBuy)
            {
                // No buy de feat, mostra a categoria correspondente à skill atual
                OSUFeatCategory cat = OSUFeatCategory.Combate;
                var list = OSUFeatSystem.GetFeats(m_FeatSkill);
                if (list != null && list.Count > 0)
                    cat = list[0].Category;

                showCombat = (cat == OSUFeatCategory.Combate);
                showProf = (cat == OSUFeatCategory.Profissoes);
            }

            if (showCombat || showProf)
            {
                int combatCap = (m_Target.OSUFeatCombatCapCustom > 0)
                    ? m_Target.OSUFeatCombatCapCustom
                    : OSUFeatSystem.OSUFeatCombatSpendCap;

                int profCap = (m_Target.OSUFeatProfessionCapCustom > 0)
                    ? m_Target.OSUFeatProfessionCapCustom
                    : OSUFeatSystem.OSUFeatProfessionSpendCap;

                if (showCombat)
                    AddLabel(741, 772, LabelHue, "XP Gasto: " + m_Target.OSUFeatSpentXPCombat + "/" + combatCap);
                else
                    AddLabel(741, 772, LabelHue, "XP Gasto: " + m_Target.OSUFeatSpentXPProf + "/" + profCap);
            }

            if (m_Target is PlayerMobile pm && pm.OSUPendingStatPoints > 0)
            {
                AddButton(681, 818, 525, 525, (int)Buttons.LevelUpStats, GumpButtonType.Reply, 0);
            }

        }

        // ============================================================
        //  VIEW: SKILLS
        // ============================================================
        private void DrawSkillsView()
        {
            AddLabel(582, 430, LabelHue, "Nome");
            AddLabel(721, 430, LabelHue, "Skill");
            AddLabel(823, 430, LabelHue, "XP");
            AddLabel(902, 430, LabelHue, "Compra");

            List<Skill> list = BuildSkillListForTab(m_Target, m_SkillsTab);

            int totalPages = (list.Count + SkillsPerPage - 1) / SkillsPerPage;
            if (totalPages < 1) totalPages = 1;

            int page = m_SkillsPage;
            if (page < 0) page = 0;
            if (page >= totalPages) page = totalPages - 1;

            AddLabel(911, 772, LabelHue, "Pg: " + (page + 1) + "/" + totalPages);

            if (page > 0)
                AddButton(563, 815, 448, 448, (int)Buttons.Prev, GumpButtonType.Reply, 0);

            if (page < totalPages - 1)
                AddButton(917, 815, 449, 449, (int)Buttons.Next, GumpButtonType.Reply, 0);

            int start = page * SkillsPerPage;
            int end = start + SkillsPerPage;
            if (end > list.Count) end = list.Count;

            int y = 479;

            for (int i = start; i < end; i++)
            {
                Skill sk = list[i];
                if (sk == null) continue;

                AddLabel(582, y, LabelHue, sk.Name);
                AddLabel(721, y, LabelHue, sk.Base.ToString("F1"));

                int xp = m_Target.GetSkillXP(sk.SkillName);
                AddLabel(806, y, LabelHue, xp.ToString() + "/" + SkillXPCap);

                List<OSUFeatDefinition> feats = OSUFeatSystem.GetFeats(sk.SkillName);
                bool hasAnyFeat = (feats != null && feats.Count > 0);

                if (hasAnyFeat)
                    AddButton(938, y, 452, 452, BuySkillButtonBase + (int)sk.SkillName, GumpButtonType.Reply, 0);

                bool unlocked = m_Target.IsOSUSkillUnlocked(sk.SkillName);
                int lockArt = unlocked ? 454 : 455;
                AddButton(556, y, lockArt, lockArt, LockSkillButtonBase + (int)sk.SkillName, GumpButtonType.Reply, 0);

                y += 34;
            }
        }

        private static List<Skill> BuildSkillListForTab(PlayerMobile target, int tab)
        {
            List<Skill> list = new List<Skill>();

            for (int i = 0; i < target.Skills.Length; i++)
            {
                Skill sk = target.Skills[i];
                if (sk == null) continue;

                if (!target.OSUIsPvpChar && (sk.SkillName == SkillName.Stealing || sk.SkillName == SkillName.Lockpicking))
                    continue;

                var group = SkillXPSystem.GetSkillGroup(sk.SkillName);


                int combatCap = target.OSUFeatCapsInverted ? OSUFeatSystem.OSUFeatProfessionSpendCap : OSUFeatSystem.OSUFeatCombatSpendCap;
                int profCap = target.OSUFeatCapsInverted ? OSUFeatSystem.OSUFeatCombatSpendCap : OSUFeatSystem.OSUFeatProfessionSpendCap;

                if (tab == 0 && group != SkillXPSystem.OSUSkillGroup.Combat)               
                    continue;

                 

                if (tab == 1 && group != SkillXPSystem.OSUSkillGroup.Profession)
                    continue;
                   

                list.Add(sk);
            }

            list.Sort(delegate (Skill a, Skill b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return list;
        }

        // ============================================================
        //  VIEW: FEAT BUY
        // ============================================================
        private void DrawFeatBuyView()
        {
            AddLabel(580, 371, LabelHue, "Feats de " + m_FeatSkill);

            AddLabel(582, 430, LabelHue, "Nome");
            AddLabel(708, 430, LabelHue, "XP Disponível");
            AddLabel(898, 430, LabelHue, "Custo");

            List<OSUFeatDefinition> feats = OSUFeatSystem.GetFeats(m_FeatSkill);

            if (feats == null || feats.Count == 0)
            {
                AddLabel(690, 524, LabelHue, "Nenhuma especialização para esta skill.");
                AddButton(563, 815, 448, 448, (int)Buttons.BackToSkills, GumpButtonType.Reply, 0);
                return;
            }

            int idx = m_FeatIndex;
            if (idx < 0) idx = 0;
            if (idx >= feats.Count) idx = feats.Count - 1;

            OSUFeatDefinition feat = feats[idx];

            int xp = m_Target.GetSkillXP(m_FeatSkill);
            if (xp > SkillXPSystem.OSUSkillXPCap)
                xp = SkillXPSystem.OSUSkillXPCap;

            int y = 479;

            AddLabel(582, y, LabelHue, feat.Name);
            AddLabel(708, y, LabelHue, xp.ToString());
            AddLabel(898, y, LabelHue, feat.CostSkillXP.ToString());

            // ✅ HTML = SÓ descrição
            AddHtml(578, 511, 374, 150, FormatHtml(BuildReqDesc(OSUFeatSystem.GetRequirementText(feat), feat.Description)), false, true);


            // ✅ Caixa de COMANDO (sempre visível)
            DrawCommandBox(GetFeatCommandText(feat));

            // ✅ Ícone / hotbar (clicável)
            if (feat.IconID > 0)
                AddButton(894, 680, feat.IconID, feat.IconID, (int)Buttons.HotbarAddFeat, GumpButtonType.Reply, 0);

            // Navegação
            AddButton(563, 815, 448, 448, (int)Buttons.BackToSkills, GumpButtonType.Reply, 0);

            if (idx > 0)
                AddButton(620, 815, 448, 448, (int)Buttons.FeatPrev, GumpButtonType.Reply, 0);

            if (idx < feats.Count - 1)
                AddButton(917, 815, 449, 449, (int)Buttons.FeatNext, GumpButtonType.Reply, 0);

            // ✅ Compra: botão + mensagem (no lugar do “clique 2x”)
            string msg;
            bool showBuy = CanShowBuyForFeat(feat, xp, out msg);

            AddLabel(654, 740, LabelHue, msg); // <-- você ajusta o X/Y depois

            if (showBuy)
                AddButton(600, 728, 442, 440, (int)Buttons.FeatBuy, GumpButtonType.Reply, 0);
        }

        // ============================================================
        //  VIEW: ABILITIES LIST
        // ============================================================
        private void DrawAbilitiesListView()
        {
            AddLabel(582, 430, LabelHue, "Nome");
            AddLabel(806, 430, LabelHue, "XP Geral");
            AddLabel(902, 430, LabelHue, "Compra");

            List<OSUAbilityDefinition> list = OSUAbilitySystem.GetAll();

            int totalPages = (list.Count + AbilitiesPerPage - 1) / AbilitiesPerPage;
            if (totalPages < 1) totalPages = 1;

            int page = m_AbilitiesPage;
            if (page < 0) page = 0;
            if (page >= totalPages) page = totalPages - 1;

            AddLabel(873, 772, LabelHue, "Página: " + (page + 1) + "/" + totalPages);

            if (page > 0)
                AddButton(563, 815, 448, 448, (int)Buttons.AbPrevPage, GumpButtonType.Reply, 0);

            if (page < totalPages - 1)
                AddButton(917, 815, 449, 449, (int)Buttons.AbNextPage, GumpButtonType.Reply, 0);

            int start = page * AbilitiesPerPage;
            int end = start + AbilitiesPerPage;
            if (end > list.Count) end = list.Count;

            int y = 479;

            for (int i = start; i < end; i++)
            {
                OSUAbilityDefinition ab = list[i];
                if (ab == null) continue;

                AddLabel(576, y, LabelHue, ab.Name);
                AddLabel(806, y, LabelHue, m_Target.OSUAbilityPicks.ToString());

                AddButton(925, y, 452, 452, AbilitySelectButtonBase + i, GumpButtonType.Reply, 0);

                y += 34;
            }
        }

        // ============================================================
        //  VIEW: ABILITY BUY
        // ============================================================
        private void DrawAbilityBuyView()
        {
            OSUAbilityDefinition ab = OSUAbilitySystem.GetByIndex(m_AbilityIndex);
            if (ab == null)
            {
                AddLabel(600, 500, LabelHue, "Habilidade inválida.");
                AddButton(563, 815, 448, 448, (int)Buttons.AbBackToList, GumpButtonType.Reply, 0);
                return;
            }

            AddLabel(640, 370, LabelHue, "Habilidade: " + ab.Name);

            AddLabel(582, 430, LabelHue, "Nome");
            AddLabel(806, 430, LabelHue, "XP Geral");
            AddLabel(902, 430, LabelHue, "Custo");

            int y = 479;

            AddLabel(576, y, LabelHue, ab.Name);
            AddLabel(806, y, LabelHue, m_Target.OSUAbilityPicks.ToString());
            AddLabel(902, y, LabelHue, ab.CostPicks.ToString());

            // ✅ HTML = SÓ descrição
            AddHtml(578, 511, 374, 150, FormatHtml(BuildReqDesc(OSUAbilitySystem.GetRequirementText(ab), ab.Description)), false, true);

            // ✅ Caixa de COMANDO (sempre visível)
            DrawCommandBox(GetAbilityCommandText(m_AbilityIndex));

            // Hotbar icon
            if (ab.IconID > 0)
                AddButton(925, 735, ab.IconID, ab.IconID, (int)Buttons.HotbarAddAbility, GumpButtonType.Reply, 0);

            // Voltar pra lista (mantém página)
            AddButton(563, 815, 448, 448, (int)Buttons.AbBackToList, GumpButtonType.Reply, 0);

            // ✅ Compra: botão + mensagem (no lugar do “clique 2x”)
            string msg;
            bool showBuy = CanShowBuyForAbility(m_AbilityIndex, out msg);

            AddLabel(650, 740, LabelHue, msg); // <-- você ajusta X/Y depois

            if (showBuy)
                AddButton(600, 728, 442, 440, (int)Buttons.AbBuy, GumpButtonType.Reply, 0);
        }

        // ============================================================
        //  COMMAND BOX (VISUAL PADRÃO DOS BUYS)
        //  (você mexe nas coords depois)
        // ============================================================
        private void DrawCommandBox(string cmdText)
        {
            // caixa para comandos
            AddImageTiled(587, 680, 279, 35, 377);

            AddLabelCropped(602, 687, 250, 20, LabelHue, cmdText ?? "");
        }

        private string GetFeatCommandText(OSUFeatDefinition feat)
        {
            if (feat == null)
                return "Comando -";

            // Prefix do seu sistema
            string prefix = Server.Custom.OSU.OSUCommandDisplay.Prefix;

            if (!string.IsNullOrEmpty(feat.CommandName))
                return "Comando - " + prefix + feat.CommandName;

            return "Comando -";
        }

        private string GetAbilityCommandText(int abilityIndex)
        {
            IOSUAbility obj = OSUAbilitySystem.GetAbilityByIndex(abilityIndex);
            if (obj != null && !string.IsNullOrEmpty(obj.CommandText))
                return "Comando - " + obj.CommandText;

            return "Comando -";
        }

        // ============================================================
        //  BUY RULES + MESSAGE
        // ============================================================
        private bool CanShowBuyForFeat(OSUFeatDefinition feat, int availableSkillXP, out string message)
        {
            message = "";

            if (feat == null)
            {
                message = "Inválido.";
                return false;
            }

            if (m_Viewer != m_Target)
            {
                message = "Somente o dono pode comprar.";
                return false;
            }

            if (m_Target.HasOSUFeat(feat.Id))
            {
                message = "Você já possui esta feat.";
                return false;
            }

            // custo
            if (availableSkillXP < feat.CostSkillXP)
            {
                message = "Você não tem XP suficiente.";
                return false;
            }

            // confirmação
            if (!m_FeatConfirm)
            {
                message = "Clique 2x para comprar esta especialização.";
                return true;
            }

            message = "Clique de novo para CONFIRMAR a compra.";
            return true;
        }

        private bool CanShowBuyForAbility(int abilityIndex, out string message)
        {
            message = "";

            OSUAbilityDefinition ab = OSUAbilitySystem.GetByIndex(abilityIndex);
            if (ab == null)
            {
                message = "Inválido.";
                return false;
            }

            if (m_Target.HasOSUAbility(ab.Id))
            {
                message = "Você já possui esta habilidade.";
                return false;
            }

            // checa regra real (requerimentos e etc)
            IOSUAbility obj = OSUAbilitySystem.GetAbilityByIndex(abilityIndex);
            if (obj != null)
            {
                string reason;
                if (!obj.CanPurchase(m_Target, out reason))
                {
                    // isso cobre "não tem requerimento"
                    message = string.IsNullOrEmpty(reason) ? "Você não atende aos requisitos." : reason;
                    return false;
                }
            }

            // custo
            if (m_Target.OSUAbilityPicks < ab.CostPicks)
            {
                message = "Você não tem pontos suficientes.";
                return false;
            }

            // confirmação
            if (!m_AbilityConfirm)
            {
                message = "Clique 2x para comprar esta habilidade.";
                return true;
            }

            message = "Clique de novo para CONFIRMAR a compra.";
            return true;
        }

        // ============================================================
        //  HTML FORMAT
        // ============================================================
        private static string FormatHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return "<BASEFONT COLOR=#FFFFFF>" + text.Replace("\n", "<BR>") + "</BASEFONT>";
        }

        // ============================================================
        //  BUTTONS
        // ============================================================
        private enum Buttons
        {
            TabCombate = 1,
            TabProfissao = 2,
            TabHabilidade = 3,

            Prev = 10,
            Next = 11,

            BackToSkills = 20,
            FeatPrev = 21,
            FeatNext = 22,
            FeatBuy = 23,

            AbPrevPage = 30,
            AbNextPage = 31,

            AbBackToList = 40,
            AbBuy = 41,

            HotbarAddFeat = 90,
            HotbarAddAbility = 91,
            LevelUpStats = 92

        }

        // ============================================================
        //  ONRESPONSE
        // ============================================================
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Viewer == null || m_Target == null)
                return;

            int id = info.ButtonID;


            // ---------------- LVL UP ----------------
            if (id == (int)Buttons.LevelUpStats)
            {
                PlayerMobile pm = m_Target as PlayerMobile;
                if (pm != null && pm.OSUPendingStatPoints > 0)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new Server.Custom.Systems.SkillXP.Gumps.OSULevelUpStatsGump(pm));
                }
                else
                {
                    m_Viewer.SendMessage(0x35, "Você não tem pontos de atributos pendentes.");
                }
                return;
            }

            // ---------------- TABS ----------------
            if (id == (int)Buttons.TabCombate)
            {
                m_Viewer.CloseGump(typeof(OSUSkillGump));
                m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, 0, 0, HubView.Skills));
                return;
            }

            if (id == (int)Buttons.TabProfissao)
            {
                m_Viewer.CloseGump(typeof(OSUSkillGump));
                m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, 1, 0, HubView.Skills));
                return;
            }

            if (id == (int)Buttons.TabHabilidade)
            {
                if (m_Viewer != m_Target)
                {
                    m_Viewer.SendMessage(0x22, "Somente o dono do personagem pode abrir habilidades.");
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.Skills));
                    return;
                }

                m_Viewer.CloseGump(typeof(OSUSkillGump));
                m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.AbilitiesList, abilitiesPage: 0));
                return;
            }

            // ---------------- SKILLS VIEW ----------------
            if (m_View == HubView.Skills)
            {
                if (id == (int)Buttons.Next)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage + 1, HubView.Skills));
                    return;
                }

                if (id == (int)Buttons.Prev)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage - 1, HubView.Skills));
                    return;
                }

                if (id >= LockSkillButtonBase && id < LockSkillButtonBase + 2000)
                {
                    if (m_Viewer != m_Target)
                    {
                        m_Viewer.SendMessage(0x22, "Somente o dono do personagem pode bloquear/desbloquear skills.");
                        m_Viewer.CloseGump(typeof(OSUSkillGump));
                        m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.Skills));
                        return;
                    }

                    SkillName sn = (SkillName)(id - LockSkillButtonBase);

                    if (!m_Target.OSUIsPvpChar && (sn == SkillName.Stealing || sn == SkillName.Lockpicking))
                    {
                        m_Target.SendMessage(0x35, "Apenas personagens PvP podem aprender esta skill.");
                        m_Viewer.CloseGump(typeof(OSUSkillGump));
                        m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.Skills));
                        return;
                    }

                    Skill sk = m_Target.Skills[sn];
                    if (sk == null)
                    {
                        m_Target.SendMessage(0x22, "Skill inválida.");
                    }
                    else
                    {
                        // Toggle real: Up <-> Locked (sem Down)
                        if (sk.Lock == SkillLock.Locked)
                        {
                            sk.SetLockNoRelay(SkillLock.Up);
                            sk.Update();

                            // seu sistema OSU (unlocked lógico, se você ainda estiver usando)
                            m_Target.UnlockOSUSkill(sn);

                            m_Target.SendMessage(0x55, "Skill desbloqueada: " + sn);
                        }
                        else
                        {
                            sk.SetLockNoRelay(SkillLock.Locked);
                            sk.Update();

                            // seu sistema OSU (unlocked lógico, se você ainda estiver usando)
                            m_Target.TryLockOSUSkill(sn);

                            m_Target.SendMessage(0x55, "Skill bloqueada: " + sn);
                        }
                    }


                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.Skills));
                    return;
                }

                if (id >= BuySkillButtonBase && id < BuySkillButtonBase + 2000)
                {
                    SkillName sn = (SkillName)(id - BuySkillButtonBase);

                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.FeatBuy,
                        featSkill: sn,
                        featIndex: 0,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: 0,
                        abilityConfirm: false
                    ));
                    return;
                }
            }

            // ---------------- FEAT BUY ----------------
            if (m_View == HubView.FeatBuy)
            {
                if (id == (int)Buttons.BackToSkills)
                {
                    // ✅ volta pra mesma tab/página
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.Skills));
                    return;
                }

                if (id == (int)Buttons.FeatPrev)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.FeatBuy,
                        featSkill: m_FeatSkill, featIndex: m_FeatIndex - 1, featConfirm: false,
                        abilitiesPage: m_AbilitiesPage, abilityIndex: m_AbilityIndex, abilityConfirm: m_AbilityConfirm));
                    return;
                }

                if (id == (int)Buttons.FeatNext)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.FeatBuy,
                        featSkill: m_FeatSkill, featIndex: m_FeatIndex + 1, featConfirm: false,
                        abilitiesPage: m_AbilitiesPage, abilityIndex: m_AbilityIndex, abilityConfirm: m_AbilityConfirm));
                    return;
                }

                if (id == (int)Buttons.FeatBuy)
                {
                    List<OSUFeatDefinition> feats = OSUFeatSystem.GetFeats(m_FeatSkill);
                    if (feats == null || feats.Count == 0)
                        return;

                    int idx = m_FeatIndex;
                    if (idx < 0) idx = 0;
                    if (idx >= feats.Count) idx = feats.Count - 1;

                    OSUFeatDefinition feat = feats[idx];

                    int available = m_Target.GetSkillXP(m_FeatSkill);
                    if (available > SkillXPSystem.OSUSkillXPCap) available = SkillXPSystem.OSUSkillXPCap;

                    string msg;
                    bool showBuy = CanShowBuyForFeat(feat, available, out msg);

                    // se não pode, só refresca
                    if (!showBuy)
                    {
                        m_Viewer.CloseGump(typeof(OSUSkillGump));
                        m_Viewer.SendGump(new OSUSkillGump(m_Viewer, m_Target, m_SkillsTab, m_SkillsPage, HubView.FeatBuy,
                            featSkill: m_FeatSkill,
                            featIndex: idx,
                            featConfirm: false,
                            abilitiesPage: m_AbilitiesPage,
                            abilityIndex: m_AbilityIndex,
                            abilityConfirm: m_AbilityConfirm));
                        return;
                    }

                    // ✅ CONFIRMAÇÃO 2x
                    if (!m_FeatConfirm)
                    {
                        // 1º clique: liga confirmação
                        m_Viewer.CloseGump(typeof(OSUSkillGump));
                        m_Viewer.SendGump(new OSUSkillGump(
                            m_Viewer, m_Target,
                            m_SkillsTab, m_SkillsPage,
                            HubView.FeatBuy,
                            featSkill: m_FeatSkill,
                            featIndex: idx,
                            featConfirm: true,
                            abilitiesPage: m_AbilitiesPage,
                            abilityIndex: m_AbilityIndex,
                            abilityConfirm: m_AbilityConfirm
                        ));
                        return;
                    }

                    // 2º clique: tenta comprar de verdade
                    string reason;
                    bool ok = OSUFeatSystem.Purchase(m_Target, feat, out reason);
                    m_Target.SendMessage(ok ? 0x55 : 0x22, reason);

                    // volta pra mesma tela, resetando confirmação
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.FeatBuy,
                        featSkill: m_FeatSkill,
                        featIndex: idx,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: m_AbilityConfirm
                    ));
                    return;
                }

                if (id == (int)Buttons.HotbarAddFeat)
                {
                    List<OSUFeatDefinition> feats = OSUFeatSystem.GetFeats(m_FeatSkill);
                    if (feats != null && feats.Count > 0)
                    {
                        int idx = m_FeatIndex;
                        if (idx < 0) idx = 0;
                        if (idx >= feats.Count) idx = feats.Count - 1;

                        OSUFeatDefinition feat = feats[idx];

                        // comando padrão da feat: prefix + CommandName
                        string cmd = Server.Custom.OSU.OSUCommandDisplay.Prefix + (feat.CommandName ?? "");

                        string msg;
                        bool ok = OSUHotBar.TryAddNext(m_Target, cmd, out msg);
                        m_Target.SendMessage(ok ? 0x55 : 0x22, msg);
                    }

                    // refresca mantendo tudo
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.FeatBuy,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: m_FeatConfirm,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: m_AbilityConfirm
                    ));
                    return;
                }
            }

            // ---------------- ABILITIES LIST ----------------
            if (m_View == HubView.AbilitiesList)
            {
                if (id == (int)Buttons.AbPrevPage)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.AbilitiesList,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage - 1,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: false
                    ));
                    return;
                }

                if (id == (int)Buttons.AbNextPage)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.AbilitiesList,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage + 1,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: false
                    ));
                    return;
                }

                if (id >= AbilitySelectButtonBase)
                {
                    int index = id - AbilitySelectButtonBase;

                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.AbilityBuy,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: index,
                        abilityConfirm: false
                    ));
                    return;
                }
            }

            // ---------------- ABILITY BUY ----------------
            if (m_View == HubView.AbilityBuy)
            {
                if (id == (int)Buttons.AbBackToList)
                {
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.AbilitiesList,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: false
                    ));
                    return;
                }

                if (id == (int)Buttons.AbBuy)
                {
                    OSUAbilityDefinition ab = OSUAbilitySystem.GetByIndex(m_AbilityIndex);
                    if (ab == null)
                        return;

                    // Revalida se pode comprar (pontos + requisitos)
                    string msg;
                    bool showBuy = CanShowBuyForAbility(m_AbilityIndex, out msg);

                    if (!showBuy)
                    {
                        // só refresca (sem confirmar)
                        m_Viewer.CloseGump(typeof(OSUSkillGump));
                        m_Viewer.SendGump(new OSUSkillGump(
                            m_Viewer, m_Target,
                            m_SkillsTab, m_SkillsPage,
                            HubView.AbilityBuy,
                            featSkill: m_FeatSkill,
                            featIndex: m_FeatIndex,
                            featConfirm: false,
                            abilitiesPage: m_AbilitiesPage,
                            abilityIndex: m_AbilityIndex,
                            abilityConfirm: false
                        ));
                        return;
                    }

                    // ✅ CONFIRMAÇÃO 2x
                    if (!m_AbilityConfirm)
                    {
                        // 1º clique: liga confirmação
                        m_Viewer.CloseGump(typeof(OSUSkillGump));
                        m_Viewer.SendGump(new OSUSkillGump(
                            m_Viewer, m_Target,
                            m_SkillsTab, m_SkillsPage,
                            HubView.AbilityBuy,
                            featSkill: m_FeatSkill,
                            featIndex: m_FeatIndex,
                            featConfirm: false,
                            abilitiesPage: m_AbilitiesPage,
                            abilityIndex: m_AbilityIndex,
                            abilityConfirm: true
                        ));
                        return;
                    }

                    // 2º clique: compra
                    string reason;
                    bool ok = OSUAbilitySystem.Purchase(m_Target, ab, out reason);
                    m_Target.SendMessage(ok ? 0x55 : 0x22, reason);

                    // volta pra mesma tela, resetando confirmação
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.AbilityBuy,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: false
                    ));
                    return;
                }

                if (id == (int)Buttons.HotbarAddAbility)
                {
                    IOSUAbility obj = OSUAbilitySystem.GetAbilityByIndex(m_AbilityIndex);

                    // se tiver CommandText, usa ele; senão, salva token ABILITY:id
                    string token = null;
                    if (obj != null && !string.IsNullOrEmpty(obj.CommandText))
                        token = obj.CommandText;
                    else
                    {
                        OSUAbilityDefinition ab = OSUAbilitySystem.GetByIndex(m_AbilityIndex);
                        token = (ab != null) ? ("ABILITY:" + ab.Id) : "ABILITY:0";
                    }

                    string msg;
                    bool ok = OSUHotBar.TryAddNext(m_Target, token, out msg);
                    m_Target.SendMessage(ok ? 0x55 : 0x22, msg);

                    // refresca mantendo confirmação
                    m_Viewer.CloseGump(typeof(OSUSkillGump));
                    m_Viewer.SendGump(new OSUSkillGump(
                        m_Viewer, m_Target,
                        m_SkillsTab, m_SkillsPage,
                        HubView.AbilityBuy,
                        featSkill: m_FeatSkill,
                        featIndex: m_FeatIndex,
                        featConfirm: false,
                        abilitiesPage: m_AbilitiesPage,
                        abilityIndex: m_AbilityIndex,
                        abilityConfirm: m_AbilityConfirm
                    ));
                    return;
                }
            }
        }
    }
}

