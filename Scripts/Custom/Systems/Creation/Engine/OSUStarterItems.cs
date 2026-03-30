using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Creation.Cultures;
using Server.Custom.Systems.Culture;

namespace Server.Custom.Systems.Creation.Engine
{
    public static class OSUStarterItems
    {
        // ===== Itens que TODO mundo recebe =====
        private static void GiveBaseItems(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            // garante mochila
            if (pm.Backpack == null)
                pm.AddItem(new Backpack());

            // exemplos de base (ajuste como quiser)
            pm.AddToBackpack(new Gold(100));
            pm.AddToBackpack(new Bandage(20));
            pm.AddToBackpack(new Torch());
        }

        // ===== Itens por skill (igual o AddSkillItems do CharacterCreation) =====
        // Você pode ir aumentando essa lista com o tempo.
        private static readonly Dictionary<SkillName, Action<PlayerMobile>> _skillItems =
            new Dictionary<SkillName, Action<PlayerMobile>>()
            {
                // Combate
                { SkillName.Swords, pm => GiveWeapon(pm, new Katana()) },
                { SkillName.Macing, pm => GiveWeapon(pm, new WarMace()) },
                { SkillName.Fencing, pm => GiveWeapon(pm, new Kryss()) },
                { SkillName.Archery, pm => { GiveWeapon(pm, new Bow()); pm.AddToBackpack(new Arrow(50)); } },
                { SkillName.Wrestling, pm => { /* sem arma */ } },
                { SkillName.Throwing, pm => GiveWeapon(pm, new Boomerang()) },

                // Magia
                { SkillName.Magery, pm => { pm.AddToBackpack(new Spellbook()); GiveRegs(pm); } },

                // Profissões / suporte
                { SkillName.Healing, pm => pm.AddToBackpack(new Bandage(50)) },
                { SkillName.AnimalTaming, pm => GiveWeapon(pm, new ShepherdsCrook()) },

                { SkillName.Mining, pm => pm.AddToBackpack(new Pickaxe()) },
                { SkillName.Lumberjacking, pm => pm.AddToBackpack(new Hatchet()) },

                { SkillName.Blacksmith, pm => { pm.AddToBackpack(new Tongs()); pm.AddToBackpack(new SmithHammer()); } },
                { SkillName.Carpentry, pm => pm.AddToBackpack(new Saw()) },
                { SkillName.Tailoring, pm => { pm.AddToBackpack(new SewingKit()); pm.AddToBackpack(new Scissors()); } },

                { SkillName.Alchemy, pm => pm.AddToBackpack(new MortarPestle()) },
                { SkillName.Cooking, pm => pm.AddToBackpack(new Skillet()) },
            };

        private static void GiveRegs(PlayerMobile pm)
        {
            if (pm?.Backpack == null) return;

            pm.AddToBackpack(new BlackPearl(20));
            pm.AddToBackpack(new Bloodmoss(20));
            pm.AddToBackpack(new Garlic(20));
            pm.AddToBackpack(new Ginseng(20));
            pm.AddToBackpack(new MandrakeRoot(20));
            pm.AddToBackpack(new Nightshade(20));
            pm.AddToBackpack(new SulfurousAsh(20));
            pm.AddToBackpack(new SpidersSilk(20));
        }

        private static void GiveWeapon(PlayerMobile pm, Item weapon)
        {
            if (pm == null || pm.Deleted || weapon == null)
                return;

            // você pode escolher: equipar ou colocar na mochila
            // aqui eu tento equipar; se não der, vai pra mochila
            if (!pm.TryEquipItem(weapon))
                pm.AddToBackpack(weapon);
        }

        private static void GiveSkillItems(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null || pm.Deleted || ctx == null)
                return;

            // junta as 4 skills iniciais (2 combate + 2 prof)
            List<string> all = new List<string>();

            if (ctx.StartingCombatSkills != null) all.AddRange(ctx.StartingCombatSkills);
            if (ctx.StartingProfessionSkills != null) all.AddRange(ctx.StartingProfessionSkills);

            for (int i = 0; i < all.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(all[i]))
                    continue;

                SkillName sk;
                if (!Enum.TryParse(all[i].Trim(), true, out sk))
                    continue;

                Action<PlayerMobile> give;
                if (_skillItems.TryGetValue(sk, out give) && give != null)
                    give(pm);
            }
        }

        // ===== Roupa por cultura (fica nos arquivos das culturas) =====
        private static void GiveCultureOutfit(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null || pm.Deleted || ctx == null)
                return;

            OSUCultureDefinition culture = OSUCultureRegistry.GetById(ctx.CultureId);
            if (culture != null)
                culture.GiveStartingOutfit(pm);
        }

        // ===== Itens extras por cultura (se quiser) =====
        private static void GiveCultureItems(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null || pm.Deleted || ctx == null)
                return;

            OSUCultureDefinition culture = OSUCultureRegistry.GetById(ctx.CultureId);
            if (culture != null)
                culture.GiveStartingItems(pm);
        }

        // ===== CHAMADA ÚNICA usada pelo Finalizer =====
        public static void GiveAll(PlayerMobile pm, OSUCreationContext ctx)
        {
            GiveBaseItems(pm);
            GiveCultureOutfit(pm, ctx);
            GiveCultureItems(pm, ctx);
            GiveSkillItems(pm, ctx);
        }
    }
}
