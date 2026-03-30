using Server;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using Server.Gumps;


namespace Server.SkillXp
{
    public static class SkillXPSystem
    {
        // ===== CONFIGURAÇÃO =====
        public const int OSUMaxLevel = 10;
        public const int BaseXPPerHit = 1;
        public const int OSUSkillXPCap = 10000;


        public static void AwardXP(PlayerMobile pm, SkillName skill, int amount)
        {
            if (pm == null || amount <= 0)
                return;

            // trava TOTAL pela regra do seu shard:
            // se a skill não estiver "UP", não ganha XP da skill nem XP geral por essa ação
            Skill sk = pm.Skills[skill];
            if (sk == null || sk.Lock != SkillLock.Up)
                return;

            // Se você ainda usa seu unlock OSU separado, mantenha:
            if (!pm.IsOSUSkillUnlocked(skill))
                return;

            AddSkillXP(pm, skill, amount);
            AddGeneralXP(pm, amount);

            // ===== XP PASSIVO =====

            // 1) Tactics: +1 XP a cada 10 XP ganho em skills de combate
            if (IsCombatSkill(skill))
            {
                pm.OSU_TacticsCarry += amount;

                int give = pm.OSU_TacticsCarry / 10;
                pm.OSU_TacticsCarry = pm.OSU_TacticsCarry % 10;

                if (give > 0)
                {
                    // só ganha se tactics estiver unlocked (igual suas outras regras)
                    if (pm.IsOSUSkillUnlocked(SkillName.Tactics))
                        AddSkillXP(pm, SkillName.Tactics, give);
                }
            }

            // 2) EvalInt: +1 XP a cada 10 XP ganho em Magery
            if (skill == SkillName.Magery)
            {
                pm.OSU_EvalCarry += amount;

                int give = pm.OSU_EvalCarry / 10;
                pm.OSU_EvalCarry = pm.OSU_EvalCarry % 10;

                if (give > 0)
                {
                    if (pm.IsOSUSkillUnlocked(SkillName.EvalInt))
                        AddSkillXP(pm, SkillName.EvalInt, give);
                }
            }

        }

        private static bool IsCombatSkill(SkillName sk)
        {
            return sk == SkillName.Swords ||
                   sk == SkillName.Macing ||
                   sk == SkillName.Fencing ||
                   sk == SkillName.Archery ||
                   sk == SkillName.Wrestling ||
                   sk == SkillName.Throwing;
        }

        #region OSU Dar XP Geral
        public static void AddGeneralXP(PlayerMobile pm, int amount)
        {
            if (pm == null || amount <= 0)
                return;

            if (pm.OSULevel >= OSUMaxLevel)
                return;

            pm.OSUGeneralXP += amount;

            CheckLevelUp(pm);
          //  pm.SendGump(new OSUSkillGump(pm, pm, 0, 0));

        }

        public static void AwardSkillAndGeneralXP(PlayerMobile pm, SkillName skill, int skillXP)
        {
            if (pm == null || skillXP <= 0)
                return;

            // Regra do seu design:
            // Só ganha XP (skill + geral) se a skill estiver DISPOSTA a subir.
            // Ou seja: precisa estar "Unlocked" no teu sistema E com Lock == Up no core.
            Skill sk = pm.Skills[skill];
            if (sk == null || sk.Lock != SkillLock.Up || !pm.IsOSUSkillUnlocked(skill))
                return;

            AddSkillXP(pm, skill, skillXP);
            AddGeneralXP(pm, skillXP);
        }

        #endregion

        #region OSU Checagem de Level

        public static int GetXPRequiredForLevel(int level)
        {
            // level 1 = 1000, level 2 = 2000, level 3 = 4000 ...
            if (level <= 1)
                return 1000;

            // 2^(level-1)
            int value = 1000;
            for (int i = 2; i <= level; i++)
            {
                // dobra a cada nível
                value *= 2;
            }

            return value;
        }

        private static void CheckLevelUp(PlayerMobile pm)
        {
            while (pm.OSULevel < OSUMaxLevel && pm.OSUGeneralXP >= pm.OSUNextLevelXP)
            {
                pm.OSUGeneralXP -= pm.OSUNextLevelXP;
                pm.OSULevel++;

                // Próximo nível dobra sempre
                pm.OSUNextLevelXP = GetXPRequiredForLevel(pm.OSULevel);

                // 1 compra de habilidade por nível
                pm.OSUAbilityPicks++;
                pm.OSUPendingStatPoints += 20;

                // Abre o gump AGORA (toda vez que subir)
                pm.CloseGump(typeof(Server.Custom.Systems.SkillXP.Gumps.OSULevelUpStatsGump));
                pm.SendGump(new Server.Custom.Systems.SkillXP.Gumps.OSULevelUpStatsGump(pm));

                pm.SendMessage(
                    0x35,
                    "Você chegou ao nível {0}! Você ganhou 1 escolha de habilidade e 20 pontos de atributos. Total: {1}. Próximo nível: {2} XP.",
                    pm.OSULevel, pm.OSUAbilityPicks, pm.OSUNextLevelXP

                );
            }
        }

        public static void ForceCheckLevelUp(PlayerMobile pm)
        {
            if (pm == null)
                return;

            CheckLevelUp(pm);
        }

        #endregion

        #region OSU Dar XP por Skill
        public static void AddSkillXP(PlayerMobile pm, SkillName skill, int amount)
        {
            if (pm == null || amount <= 0)
                return;

            // OSU: skill precisa estar desbloqueada
            if (!pm.IsOSUSkillUnlocked(skill))
            {
                pm.SendMessage(0x22, "Essa skill está bloqueada. Desbloqueie no gump de skills."); //comentar dps
                return;
            }

            if (!pm.SkillXP.ContainsKey(skill))
                pm.SkillXP[skill] = 0;

            pm.SkillXP[skill] += amount;

            if (pm.SkillXP[skill] > OSUSkillXPCap)
                pm.SkillXP[skill] = OSUSkillXPCap;

            pm.SendMessage(0x55, $"Você ganhou {amount} XP em {skill}."); //comentar dps
         //   pm.SendGump(new OSUSkillGump(pm, pm, 0, 0));

        }
        #endregion

        #region OSU XP Por Craft

        // % do MinSkill para virar XP base
        public const double OSUCraftXPPercentOfMinSkill = 0.10;

        // Garantir mínimo (pra não virar 0 XP em receitas fáceis)
        public const int OSUCraftMinXP = 1;

        // Bônus de exceptional: aqui eu fiz como +50% do XP base (você pode mudar depois)
        public const double OSUCraftExceptionalBonusPercent = 0.50;

        // Bônus por material (você pode preencher aos poucos)
        private static readonly Dictionary<CraftResource, int> _OSUCraftMaterialBonus = new Dictionary<CraftResource, int>()
        {
            // Metais (exemplos)
            { CraftResource.DullCopper, 1 },
            { CraftResource.ShadowIron, 2 },
            { CraftResource.Copper, 3 },
            { CraftResource.Bronze, 4 },
            { CraftResource.Gold, 5 },
            { CraftResource.Agapite, 6 },
            { CraftResource.Verite, 7 },
            { CraftResource.Valorite, 8 },

            // Madeiras (exemplos)
            { CraftResource.OakWood, 1 },
            { CraftResource.AshWood, 2 },
            { CraftResource.YewWood, 3 },
            { CraftResource.Heartwood, 4 },
            { CraftResource.Bloodwood, 5 },
            { CraftResource.Frostwood, 6 },

            // Couros etc (exemplos — só coloque se existir no seu shard)
            { CraftResource.SpinedLeather, 2 },
            { CraftResource.HornedLeather, 4 },
            { CraftResource.BarbedLeather, 6 },

            // Escamas
            { CraftResource.RedScales, 1 },
            { CraftResource.YellowScales, 2 },
            { CraftResource.BlackScales, 3 },
            { CraftResource.GreenScales, 4 },
            { CraftResource.WhiteScales, 5 },
            { CraftResource.BlueScales, 6 },

         };

        // Bônus por “receita específica” (por tipo do item craftado)
       // Server.SkillXp.SkillXPSystem.SetCraftRecipeXPBonus(typeof(nome do item magico), o numero de bonus) // <<<< ADICIONAR ISSO A ITENS RAROS PRA DAR XP MAIOR
        private static readonly Dictionary<Type, int> _OSUCraftRecipeBonus = new Dictionary<Type, int>();

        public static void SetCraftRecipeXPBonus(Type itemType, int bonusXP)
        {
            if (itemType == null)
                return;

            if (bonusXP <= 0)
            {
                if (_OSUCraftRecipeBonus.ContainsKey(itemType))
                    _OSUCraftRecipeBonus.Remove(itemType);

                return;
            }

            _OSUCraftRecipeBonus[itemType] = bonusXP;
        }

        public static int GetCraftRecipeXPBonus(Type itemType)
        {
            if (itemType == null)
                return 0;

            int v;
            if (_OSUCraftRecipeBonus.TryGetValue(itemType, out v))
                return v;

            return 0;
        }

        public static int GetCraftMaterialBonus(CraftResource res)
        {
            int v;
            if (_OSUCraftMaterialBonus.TryGetValue(res, out v))
                return v;

            return 0;
        }

        // Calcula XP base a partir do MinSkill
        public static int CalculateCraftBaseXP(double minSkill)
        {
            // 10% do MinSkill
            int xp = (int)Math.Ceiling(minSkill * OSUCraftXPPercentOfMinSkill);

            if (xp < OSUCraftMinXP)
                xp = OSUCraftMinXP;

            return xp;
        }
        #endregion

        #region OSU Skill Groups

        public enum OSUSkillGroup
        {
            Combat,
            Profession,
            Ability
        }

        // Caps separados (em "pontos de skill", então 700.0 = 7000 no sistema interno do UO, mas vamos trabalhar em double)
        public const double OSUCombatSkillCap = 400.0;
        public const double OSUProfessionSkillCap = 300.0;

        // Exemplos (você vai trocar depois)
        private static readonly SkillName[] _CombatSkills = new SkillName[]
        {
    SkillName.Magery,
    SkillName.Swords,
    SkillName.Macing,
    SkillName.Archery,
    SkillName.Wrestling,
    SkillName.Fencing,
    SkillName.Tactics,
    SkillName.Healing,
    SkillName.Parry,
    SkillName.AnimalTaming,
    SkillName.Throwing,
    SkillName.AnimalLore,
    SkillName.Meditation
        };

        private static readonly SkillName[] _ProfessionSkills = new SkillName[]
        {
    SkillName.Carpentry,
    SkillName.Tinkering,
    SkillName.Cooking,
    SkillName.Alchemy,
    SkillName.Blacksmith,
    SkillName.Fletching,
    SkillName.Cooking,
    SkillName.Fishing,
    SkillName.Forensics,
    SkillName.Herding,
    SkillName.Inscribe,
    SkillName.Mining,
    SkillName.Lumberjacking,
    SkillName.Tinkering,
    SkillName.Tailoring










        };

        // Tudo que não estiver acima entra como "Ability"
        public static OSUSkillGroup GetSkillGroup(SkillName skill)
        {
            for (int i = 0; i < _CombatSkills.Length; i++)
            {
                if (_CombatSkills[i] == skill)
                    return OSUSkillGroup.Combat;
            }

            for (int i = 0; i < _ProfessionSkills.Length; i++)
            {
                if (_ProfessionSkills[i] == skill)
                    return OSUSkillGroup.Profession;
            }

            return OSUSkillGroup.Ability;
        }

        // Soma total das skills de um grupo
        public static double GetTotalSkillValueForGroup(PlayerMobile pm, OSUSkillGroup group)
        {
            if (pm == null)
                return 0.0;

            double total = 0.0;

            for (int i = 0; i < pm.Skills.Length; i++)
            {
                Skill sk = pm.Skills[i];

                if (sk == null)
                    continue;

                if (GetSkillGroup(sk.SkillName) == group)
                    total += sk.Base;
            }

            return total;
        }

        // Retorna o cap do grupo
        public static double GetGroupCap(OSUSkillGroup group)
        {
            if (group == OSUSkillGroup.Combat)
                return OSUCombatSkillCap;

            if (group == OSUSkillGroup.Profession)
                return OSUProfessionSkillCap;

            // Ability por enquanto sem cap próprio aqui (você quer travar o progresso, então cap não importa ainda)
            return 0.0;
        }

        // Cap do grupo respeitando o personagem (fallback pros defaults)
        public static double GetGroupCap(PlayerMobile pm, OSUSkillGroup group)
        {
            if (pm != null)
            {
                if (group == OSUSkillGroup.Combat && pm.OSUCombatSkillCap > 0)
                    return pm.OSUCombatSkillCap;

                if (group == OSUSkillGroup.Profession && pm.OSUCraftSkillCap > 0)
                    return pm.OSUCraftSkillCap;
            }

            return GetGroupCap(group);
        }

        #endregion

    }
}
