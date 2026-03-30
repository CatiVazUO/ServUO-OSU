using Server;
using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
    /*
     * ============================================================
     * SKILL XP SYSTEM - COMANDOS (GM)
     * ============================================================
     *
     * [SkillXpHelp
     *  - Lista todos os comandos desse arquivo
     *
     * [AddSkillXP <SkillName> <Quantidade>
     *  - Dá XP de uma skill específica para um jogador (seleciona no target)
     *
     * [AddGeneralXP <Quantidade>
     *  - Dá XP Geral para um jogador (seleciona no target) e força check de level up
     *
     * [OSUSkills
     *  - Abre o gump de Skills+XP de um jogador (seleciona no target)
     *
     * [osugmfeats
     *  - (FUTURO GUMP) Ver feats do jogador (seleciona no target)
     *
     * [osugmabs
     *  - (FUTURO GUMP) Ver habilidades do jogador (seleciona no target)
     *
     * [osugivefeat <featId>
     *  - Dá uma feat para um jogador (seleciona no target)
     *
     * [osugiveab <abilityId>
     *  - Dá uma habilidade para um jogador (seleciona no target)
     *  
     *  [GmGump 
     *  - Abre gump do gm
     *
     * ============================================================
     */

    public static class SkillXpCommands
    {
        public static void Initialize()
        {
            // LvlUp
            CommandSystem.Register("lvlup", AccessLevel.Player, OnLvlUp);

            // Help
            CommandSystem.Register("SkillXpHelp", AccessLevel.GameMaster, OnHelp);

            // XP
            CommandSystem.Register("AddSkillXP", AccessLevel.GameMaster, OnAddSkillXP);
            CommandSystem.Register("AddGeneralXP", AccessLevel.GameMaster, OnAddGeneralXP);

            // Gump skills
            CommandSystem.Register("OSUSkills", AccessLevel.GameMaster, OnOSUSkills);

            // GM view (gump depois)
            CommandSystem.Register("OSUGmFeat", AccessLevel.GameMaster, OnGMFeats);
            CommandSystem.Register("OSUGmHab", AccessLevel.GameMaster, OnGMAbilities);

            // GM give
            CommandSystem.Register("OSUGiveFeat", AccessLevel.GameMaster, OnGiveFeat);
            CommandSystem.Register("OSUGiveHab", AccessLevel.GameMaster, OnGiveAbility);
            // GM Gump
            CommandSystem.Register("gmgumpfeats", AccessLevel.GameMaster, OnGMGumpFeats);
            CommandSystem.Register("gmgumphabs", AccessLevel.GameMaster, OnGMGumpHabs);
            CommandSystem.Register("setskillcaps", AccessLevel.GameMaster, OnSetSkillCaps);
        }
            
        private static void OnLvlUp(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            if (pm.OSUPendingStatPoints <= 0)
            {
                pm.SendMessage(0x35, "Você não tem pontos de atributos pendentes.");
                return;
            }

            pm.CloseGump(typeof(Server.Custom.Systems.SkillXP.Gumps.OSULevelUpStatsGump));
            pm.SendGump(new Server.Custom.Systems.SkillXP.Gumps.OSULevelUpStatsGump(pm));
        }

        private static void OnSetSkillCaps(CommandEventArgs e)
        {
            var from = e.Mobile;

            if (from == null)
                return;

            // Padrão: warrior
            double combat = 400.0;
            double craft = 300.0;

            // Arg1 pode ser "warrior" / "artisan" ou número
            if (e.Arguments != null && e.Arguments.Length > 0)
            {
                var a0 = (e.Arguments[0] ?? "").Trim().ToLowerInvariant();

                if (a0 == "warrior" || a0 == "guerreiro")
                {
                    combat = 400.0;
                    craft = 300.0;
                }
                else if (a0 == "artisan" || a0 == "artesao" || a0 == "artesão")
                {
                    combat = 300.0;
                    craft = 400.0;
                }
                else
                {
                    // tenta ler como números: [setskillcaps 400 300
                    if (!Double.TryParse(e.Arguments[0], out combat))
                    {
                        from.SendMessage("Uso: [setskillcaps warrior|artisan  OU  [setskillcaps <combat> <craft>");
                        return;
                    }

                    if (e.Arguments.Length < 2 || !Double.TryParse(e.Arguments[1], out craft))
                    {
                        from.SendMessage("Uso: [setskillcaps <combat> <craft>  (ex: [setskillcaps 400 300)");
                        return;
                    }
                }
            }

            from.SendMessage("Escolha o jogador para aplicar os caps.");
            from.Target = new ApplyCapsTarget(combat, craft);
        }

        private class ApplyCapsTarget : Target
        {
            private readonly double _combat;
            private readonly double _craft;

            public ApplyCapsTarget(double combat, double craft) : base(12, false, TargetFlags.None)
            {
                _combat = combat;
                _craft = craft;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                var pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage("Isso não é um jogador.");
                    return;
                }

                pm.OSUCombatSkillCap = _combat;
                pm.OSUCraftSkillCap = _craft;

                pm.InvalidateProperties();

                from.SendMessage($"Caps aplicados em {pm.Name}: Combat={_combat} / Craft={_craft}");
                pm.SendMessage($"Seus caps foram ajustados: Combat={_combat} / Craft={_craft}");
            }
        }

        private static void OnGMGumpFeats(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendMessage(0x55, "Selecione o jogador para abrir o GM Gump (FEATS).");
            from.Target = new GMGumpTarget(OSUGMGump.Mode.Feats);
        }

        private static void OnGMGumpHabs(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendMessage(0x55, "Selecione o jogador para abrir o GM Gump (HABS).");
            from.Target = new GMGumpTarget(OSUGMGump.Mode.Abilities);
        }

        private class GMGumpTarget : Target
        {
            private readonly OSUGMGump.Mode _mode;

            public GMGumpTarget(OSUGMGump.Mode mode) : base(12, false, TargetFlags.None)
            {
                _mode = mode;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage(0x22, "Isso não é um jogador.");
                    return;
                }

                from.CloseGump(typeof(OSUGMGump));
                from.SendGump(new OSUGMGump(from, pm, _mode, 0));
            }
        }

        // ------------------------------------------------------------
        // HELP
        // ------------------------------------------------------------
        private static void OnHelp(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            from.SendMessage(0x55, "=== SkillXP Commands (GM) ===");
            from.SendMessage(0x55, "[SkillXpHelp - Lista comandos");
            from.SendMessage(0x55, "[AddSkillXP <SkillName> <Qtd> - Dá XP de skill (target player)");
            from.SendMessage(0x55, "[AddGeneralXP <Qtd> - Dá XP geral (target player)");
            from.SendMessage(0x55, "[OSUSkills - Abre gump Skills+XP (target player)");
            from.SendMessage(0x55, "[osugmfeats - Ver feats do player (target player) (gump depois)");
            from.SendMessage(0x55, "[osugmabs - Ver habilidades do player (target player) (gump depois)");
            from.SendMessage(0x55, "[osugivefeat <featId> - Dá feat (target player)");
            from.SendMessage(0x55, "[osugiveab <abilityId> - Dá habilidade (target player)");
        }

        // ------------------------------------------------------------
        // ADD SKILL XP
        // ------------------------------------------------------------
        private static void OnAddSkillXP(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Arguments.Length < 2)
            {
                from.SendMessage(0x22, "Uso: [AddSkillXP <SkillName> <Quantidade>");
                from.SendMessage(0x22, "Exemplo: [AddSkillXP Magery 500");
                return;
            }

            SkillName skill;
            if (!Enum.TryParse(e.Arguments[0], true, out skill))
            {
                from.SendMessage(0x22, "Skill inválida. Ex: Magery, Swords, Carpentry...");
                return;
            }

            int amount;
            if (!int.TryParse(e.Arguments[1], out amount) || amount <= 0)
            {
                from.SendMessage(0x22, "Quantidade inválida. Use um número inteiro > 0.");
                return;
            }

            from.SendMessage(0x55, "Clique no jogador que vai receber XP em {0} (+{1}).", skill, amount);
            from.Target = new SkillXPTarget(skill, amount);
        }

        private class SkillXPTarget : Target
        {
            private readonly SkillName m_Skill;
            private readonly int m_Amount;

            public SkillXPTarget(SkillName skill, int amount) : base(12, false, TargetFlags.None)
            {
                m_Skill = skill;
                m_Amount = amount;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage(0x22, "Isso não é um jogador.");
                    return;
                }

                Server.SkillXp.SkillXPSystem.AddSkillXP(pm, m_Skill, m_Amount);
                from.SendMessage(0x55, "OK: {0} recebeu +{1} XP em {2}.", pm.Name, m_Amount, m_Skill);
            }
        }

        // ------------------------------------------------------------
        // ADD GENERAL XP
        // ------------------------------------------------------------
        private static void OnAddGeneralXP(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Arguments.Length < 1)
            {
                from.SendMessage(0x22, "Uso: [AddGeneralXP <Quantidade>");
                from.SendMessage(0x22, "Exemplo: [AddGeneralXP 500");
                return;
            }

            int amount;
            if (!int.TryParse(e.Arguments[0], out amount) || amount <= 0)
            {
                from.SendMessage(0x22, "Quantidade inválida. Use um número inteiro > 0.");
                return;
            }

            from.SendMessage(0x55, "Clique no jogador que vai receber XP Geral (+{0}).", amount);
            from.Target = new GeneralXPTarget(amount);
        }

        private class GeneralXPTarget : Target
        {
            private readonly int m_Amount;

            public GeneralXPTarget(int amount) : base(12, false, TargetFlags.None)
            {
                m_Amount = amount;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage(0x22, "Isso não é um jogador.");
                    return;
                }

                Server.SkillXp.SkillXPSystem.AddGeneralXP(pm, m_Amount);
                Server.SkillXp.SkillXPSystem.ForceCheckLevelUp(pm);
                from.SendMessage(0x55, "OK: {0} recebeu +{1} XP Geral.", pm.Name, m_Amount);
            }
        }

        // ------------------------------------------------------------
        // OSU SKILLS GUMP
        // ------------------------------------------------------------
        private static void OnOSUSkills(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            from.SendMessage(0x55, "Clique no jogador para ver Skills + XP.");
            from.Target = new OSUSkillsTarget();
        }

        private class OSUSkillsTarget : Target
        {
            public OSUSkillsTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile target = targeted as PlayerMobile;

                if (target == null)
                {
                    from.SendMessage(0x22, "Isso não é um jogador.");
                    return;
                }

                // Se você renomeou o gump para OSUSkillGuyGump, troque aqui:
                // from.CloseGump(typeof(OSUSkillGuyGump));
                // from.SendGump(new OSUSkillGuyGump(from, target, 0, 0));

                from.CloseGump(typeof(OSUSkillGump));
                from.SendGump(new OSUSkillGump(from, target, 0, 0));
            }
        }

        // ------------------------------------------------------------
        // GM VIEW (GUMP DEPOIS)
        // ------------------------------------------------------------
        private static void OnGMFeats(CommandEventArgs e)
        {
            e.Mobile.SendMessage(0x55, "Selecione o jogador para ver as FEATS. (Gump depois)");
            // e.Mobile.Target = new ViewTarget(ViewTargetKind.Feats);
        }

        private static void OnGMAbilities(CommandEventArgs e)
        {
            e.Mobile.SendMessage(0x55, "Selecione o jogador para ver as HABILIDADES. (Gump depois)");
            // e.Mobile.Target = new ViewTarget(ViewTargetKind.Abilities);
        }

        // ------------------------------------------------------------
        // GM GIVE
        // ------------------------------------------------------------
        private static void OnGiveFeat(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Arguments.Length < 1)
            {
                from.SendMessage(0x22, "Uso: [osugivefeat <featId>");
                return;
            }

            int id;
            if (!int.TryParse(e.Arguments[0], out id))
            {
                from.SendMessage(0x22, "featId inválido.");
                return;
            }

            from.SendMessage(0x55, "Selecione o jogador para RECEBER a feat " + id + ".");
            from.Target = new GiveTarget(GiveTargetKind.Feat, id);
        }

        private static void OnGiveAbility(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Arguments.Length < 1)
            {
                from.SendMessage(0x22, "Uso: [osugiveab <abilityId>");
                return;
            }

            int id;
            if (!int.TryParse(e.Arguments[0], out id))
            {
                from.SendMessage(0x22, "abilityId inválido.");
                return;
            }

            from.SendMessage(0x55, "Selecione o jogador para RECEBER a habilidade " + id + ".");
            from.Target = new GiveTarget(GiveTargetKind.Ability, id);
        }

        private enum GiveTargetKind { Feat, Ability }

        private class GiveTarget : Target
        {
            private readonly GiveTargetKind _kind;
            private readonly int _id;

            public GiveTarget(GiveTargetKind kind, int id) : base(12, false, TargetFlags.None)
            {
                _kind = kind;
                _id = id;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage(0x22, "Isso não é um jogador.");
                    return;
                }

                if (_kind == GiveTargetKind.Feat)
                {
                    if (pm.HasOSUFeat(_id))
                    {
                        from.SendMessage(0x22, "O jogador já possui esta feat.");
                        return;
                    }

                    pm.AddOSUFeat(_id);
                    from.SendMessage(0x55, "Feat " + _id + " adicionada em " + pm.Name + ".");
                    pm.SendMessage(0x55, "Você recebeu uma feat (GM).");
                }
                else
                {
                    if (pm.HasOSUAbility(_id))
                    {
                        from.SendMessage(0x22, "O jogador já possui esta habilidade.");
                        return;
                    }

                    pm.AddOSUAbility(_id);
                    from.SendMessage(0x55, "Habilidade " + _id + " adicionada em " + pm.Name + ".");
                    pm.SendMessage(0x55, "Você recebeu uma habilidade (GM).");
                }
            }
        }
    }
}
