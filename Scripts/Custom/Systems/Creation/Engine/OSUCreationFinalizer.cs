using Server;
using Server.Custom.Systems.Creation.Cultures;
using Server.Custom.Systems.Culture;
using Server.Custom.Systems.DefQual;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;


namespace Server.Custom.Systems.Creation.Engine
{
    public static class OSUCreationFinalizer
    {
        // DESTINO PLACEHOLDER (você troca depois)
        public static Point3D EntryLocation = new Point3D(1000, 1000, 0);
        public static Map EntryMap = Map.Trammel;

        public static bool TryEnterAmanti(PlayerMobile pm, out string reason)
        {
            reason = null;
            OSUCultureDefinition culture = null;

            pm.StrLock = StatLockType.Locked;
            pm.DexLock = StatLockType.Locked;
            pm.IntLock = StatLockType.Locked;

            // 1) valida pm
            if (pm == null || pm.Deleted)
            {
                reason = "Personagem inválido.";
                return false;
            }

            // 2) pega contexto
            OSUCreationContext ctx = pm.OSUCreation;
            if (ctx == null)
            {
                reason = "Você ainda não iniciou a criação.";
                return false;
            }

            culture = OSUCultureRegistry.GetById(ctx.CultureId);

            // ===== validações mínimas (mesmas do gump) =====

            if (ctx.Path != OSUCreationPath.Warrior && ctx.Path != OSUCreationPath.Artisan)
            {
                reason = "Você precisa escolher um Caminho.";
                return false;
            }

            if (ctx.GameMode != OSUCreationGameMode.Pvp && ctx.GameMode != OSUCreationGameMode.NoPvp)
            {
                reason = "Você precisa escolher um Game Mode.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(ctx.CultureId))
            {
                reason = "Você precisa escolher um Povo.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(ctx.ReligionId))
            {
                reason = "Você precisa escolher uma Religião (ou Sem Deus).";
                return false;
            }

            int c = ctx.StartingCombatSkills == null ? 0 : ctx.StartingCombatSkills.Count;
            int p = ctx.StartingProfessionSkills == null ? 0 : ctx.StartingProfessionSkills.Count;

            if (c != 2 || p != 2)
            {
                reason = "Você precisa escolher 2 skills de Combate e 2 de Profissão.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(ctx.ChosenName))
            {
                reason = "Você precisa escolher um Nome.";
                return false;
            }

            if (ctx.RpWeightKg < 1 || ctx.RpWeightKg > 140)
            {
                reason = "Peso inválido. Use 1 a 140 kg.";
                return false;
            }

            if (ctx.RpHeightCm < 1 || ctx.RpHeightCm > 200)
            {
                reason = "Altura inválida. Use 1 a 200 cm.";
                return false;
            }

            if (ctx.RpAge < 1 || ctx.RpAge > 65)
            {
                reason = "Idade inválida. Use 1 a 70 anos.";
                return false;
            }

            if (ctx.RpAvatarId <= 0)
            {
                reason = "Você precisa escolher um Avatar.";
                return false;
            }

            // Nome único (igual UO: ninguém repete nome)
            if (IsNameTaken(pm, ctx.ChosenName))
            {
                reason = "Esse nome já está sendo usado. Escolha outro.";
                return false;
            }

            // Avatar único. Se o personagem já tinha um avatar antigo, ele é liberado ao trocar.
            int previousAvatarId = pm.OSUAvatarId;

            if (previousAvatarId > 0 && previousAvatarId != ctx.RpAvatarId)
                OSUAvatarRegistry.UnmarkUsed(previousAvatarId);

            if (previousAvatarId != ctx.RpAvatarId)
            {
                if (!OSUAvatarRegistry.TryMarkUsed(ctx.RpAvatarId, out reason))
                {
                    if (previousAvatarId > 0)
                        OSUAvatarRegistry.TryMarkUsed(previousAvatarId, out _);

                    return false;
                }
            }
            else if (ctx.RpAvatarId > 0 && !OSUAvatarRegistry.IsUsed(ctx.RpAvatarId))
            {
                if (!OSUAvatarRegistry.TryMarkUsed(ctx.RpAvatarId, out reason))
                    return false;
            }

            // ===== grava flags permanentes no PlayerMobile =====
            pm.OSUCreationCompleted = true;

            // modo pvp
            pm.OSUIsPvpChar = (ctx.GameMode == OSUCreationGameMode.Pvp);

            // cultura/religião/avatar
            pm.OSUCultureId = ctx.CultureId;

            if (culture != null)
            {
                // cidadão nasce na capital da própria cultura
                pm.OSUCitizenCityId = culture.CapitalCityId;

                // entrada no mundo baseada na cultura
                OSUStarterItems.GiveAll(pm, ctx);
                ApplyAppearance(pm, ctx);
                MoveToCultureEntry(pm, ctx);

            }
            else
            {
                // fallback
                OSUStarterItems.GiveAll(pm, ctx);
                ApplyAppearance(pm, ctx);
                MoveToCultureEntry(pm, ctx);
            }

            pm.OSUReligionId = ctx.ReligionId;
            pm.OSUAvatarId = ctx.RpAvatarId;

            // ficha rp
            pm.OSURpWeightKg = ctx.RpWeightKg;
            pm.OSURpHeightCm = ctx.RpHeightCm;
            pm.OSURpAge = ctx.RpAge;
            pm.OSURpTraitsPublic = ctx.RpTraitsPublic ?? "";
            pm.OSURpHistoryStaff = ctx.RpHistoryStaff ?? "";

            // ===== Atributos escolhidos no gump viram valores reais =====
            pm.RawStr = ctx.Attr_Str;
            pm.RawDex = ctx.Attr_Dex;
            pm.RawInt = ctx.Attr_Int;

            // defaults dos caps (antes de aplicar Def/Qual)
            // defaults dos caps (antes de aplicar Def/Qual)
            pm.OSUHpCapMax = 115;
            pm.OSUStamCapMax = 115;
            pm.OSUManaCapMax = 115;

            pm.OSUStrCapMax = 115;
            pm.OSUDexCapMax = 115;
            pm.OSUIntCapMax = 115;

            // zera/atualiza flags de DefQual no player
            pm.OSUDefQualFlags = new List<string>();

            // aplica flags + efeitos de cada Def/Qual escolhido
            if (ctx.SelectedDefQualIds != null)
            {
                for (int i = 0; i < ctx.SelectedDefQualIds.Count; i++)
                {
                    string id = ctx.SelectedDefQualIds[i];
                    if (String.IsNullOrWhiteSpace(id))
                        continue;

                    // grava a flag permanente (pra injuries e etc)
                    pm.OSUDefQualFlags.Add(id);

                    // aplica o efeito real
                    var def = OSUDefQualRegistry.GetById(id);


                    // defqual
                    int extraGold = OSUDefQualDispatcher.ModifyStartingGold(pm, 0);
                    if (extraGold > 0)
                        pm.AddToBackpack(new Gold(extraGold));

                    if (pm.HasOSUDefQual("nobre"))
                        pm.AddToBackpack(new OSUFamilyRing());

                    if (def != null)
                        def.ApplyToPlayer(pm, ctx);
                }
            }

            // agora coloca os atributos “HP/Vigor/Mana” escolhidos no player
            pm.OSUBaseHP = ctx.Attr_HP;
            pm.OSUBaseStam = ctx.Attr_Vit;
            pm.OSUBaseMana = ctx.Attr_Man;

            if (pm.OSUBaseHP > pm.OSUHpCapMax)
                pm.OSUBaseHP = pm.OSUHpCapMax;

            if (pm.OSUBaseStam > pm.OSUStamCapMax)
                pm.OSUBaseStam = pm.OSUStamCapMax;

            if (pm.OSUBaseMana > pm.OSUManaCapMax)
                pm.OSUBaseMana = pm.OSUManaCapMax;

            if (pm.RawStr > pm.OSUStrCapMax)
                pm.RawStr = pm.OSUStrCapMax;

            if (pm.RawDex > pm.OSUDexCapMax)
                pm.RawDex = pm.OSUDexCapMax;

            if (pm.RawInt > pm.OSUIntCapMax)
                pm.RawInt = pm.OSUIntCapMax;

            // atualiza valores atuais para o máximo
            pm.Hits = pm.HitsMax;
            pm.Stam = pm.StamMax;
            pm.Mana = pm.ManaMax;


            // ===== caps (def/qual mexe no cap MAIOR) =====
            // garante 50k/30k no ctx
            ctx.ApplyPathCaps();

            int baseCombat = ctx.CombatCap; // warrior 50k / artisan 30k
            int baseProf = ctx.ProfCap;     // warrior 30k / artisan 50k

            // soma delta dos Def/Qual
            int delta = 0;
            if (ctx.SelectedDefQualIds != null)
            {
                for (int i = 0; i < ctx.SelectedDefQualIds.Count; i++)
                {
                    var dq = OSUDefQualRegistry.GetById(ctx.SelectedDefQualIds[i]);
                    if (dq != null)
                        delta += dq.CapDelta;
                }
            }

            bool warrior = (ctx.Path == OSUCreationPath.Warrior);

            // ===== caps de SKILL por caminho (combat x craft) =====
            if (warrior)
            {
                pm.OSUCombatSkillCap = 400.0;
                pm.OSUCraftSkillCap = 300.0;
            }
            else
            {
                pm.OSUCombatSkillCap = 300.0;
                pm.OSUCraftSkillCap = 400.0;
            }



            int majorBase = warrior ? baseCombat : baseProf;
            int minorBase = warrior ? baseProf : baseCombat;

            int majorFinal = majorBase + delta;

            // clamp 40k..70k
            if (majorFinal < 40000) majorFinal = 40000;
            if (majorFinal > 70000) majorFinal = 70000;

            // grava caps finais no player
            pm.OSUFeatCombatCapCustom = warrior ? majorFinal : minorBase;
            pm.OSUFeatProfessionCapCustom = warrior ? minorBase : majorFinal;

            pm.OSUFeatTotalCapCustom = pm.OSUFeatCombatCapCustom + pm.OSUFeatProfessionCapCustom;

            // esse bool você usa no seu sistema (mantém)
            pm.OSUFeatCapsInverted = (ctx.Path == OSUCreationPath.Artisan);

            // ===== skills: trava tudo, libera só 4 e seta 30.0 =====
            pm.OSUInitialSkillsLocked = true;

            LockAllSkills(pm);
            ApplyStartingSkills(pm, ctx.StartingCombatSkills);
            ApplyStartingSkills(pm, ctx.StartingProfessionSkills);

            // Nome vira real aqui
            pm.Name = ctx.ChosenName.Trim();

            pm.SendMessage(0x35, "Você atravessou o portal e entrou em Amanti. Bem-vindo!");
            return true;
        }

        private static void LockAllSkills(PlayerMobile pm)
        {
            if (pm == null)
                return;

            // trava todas as OSU skills pelo seu sistema (não usa Skill.Lock)
            Array values = Enum.GetValues(typeof(SkillName));
            for (int i = 0; i < values.Length; i++)
            {
                SkillName sk = (SkillName)values.GetValue(i);
                pm.TryLockOSUSkill(sk);
            }
        }
        private static void ApplyStartingSkills(PlayerMobile pm, List<string> list)
        {
            if (pm == null || list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(list[i]))
                    continue;

                SkillName sk;

                // importante: ignoreCase = true
                if (!Enum.TryParse(list[i].Trim(), true, out sk))
                    continue;

                try
                {
                    // 1) libera no seu sistema OSU
                    pm.UnlockOSUSkill(sk);

                    // 2) seta valor inicial
                    Skill s = pm.Skills[sk];
                    if (s != null)
                    {
                        s.Base = 30.0;

                        // 3) destrava o lock do UO/client
                        s.SetLockNoRelay(SkillLock.Up);
                        s.Update();
                    }
                }
                catch { }
            }
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

        // ===== Aparência (pele + cabelo + barba) =====
        public static void ApplyAppearance(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.OSUCreation == null)
                return;

            ApplyAppearance(pm, pm.OSUCreation);
        }

        public static void ApplyAppearance(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null || pm.Deleted || ctx == null)
                return;

            // 1) Gênero + Body padrão humano
            pm.Female = ctx.GenderFemale;
            pm.Body = pm.Female ? 0x191 : 0x190;

            // 2) Pele
            // Se o gump só fez preview no corpo (pm.Hue) e não gravou no ctx, capturamos aqui:
            if (ctx.SkinHue <= 0 && pm.Hue > 0)
                ctx.SkinHue = pm.Hue;

            if (ctx.SkinHue > 0)
                pm.Hue = ctx.SkinHue;

            // 3) Cultura
            var culture = OSUCultureRegistry.GetById(ctx.CultureId);

            // 4) Cabelo: sempre deriva do GUMP escolhido na cultura.
            int hairGumpId = ctx.HairGumpId;

            if (culture != null)
            {
                if (hairGumpId <= 0)
                    hairGumpId = culture.GetHairGumpId(pm.Female, ctx.HairIndex);
            }
            else
            {
                int fallbackBase = pm.Female ? 64000 : 54000;
                hairGumpId = fallbackBase + Math.Max(0, ctx.HairIndex);
            }

            int hairItemId = 0;

            if (culture != null)
                hairItemId = culture.MapHairGumpToItemId(hairGumpId, pm.Female);
            else if (hairGumpId > 0)
                hairItemId = 13050 + (hairGumpId - (pm.Female ? 64000 : 54000));

            ctx.HairGumpId = hairGumpId;
            ctx.HairItemId = hairItemId;

            pm.HairItemID = hairItemId;
            pm.HairHue = ctx.HairHue;

            // 5) Barba (só homem)
            if (!pm.Female)
            {
                int beardGumpId = 0;

                if (culture != null)
                    beardGumpId = culture.GetBeardGumpId(ctx.BeardIndex);
                else
                    beardGumpId = 53500 + Math.Max(0, ctx.BeardIndex);

                int beardItemId = 0;

                if (culture != null)
                    beardItemId = culture.MapBeardGumpToItemId(beardGumpId);
                else if (beardGumpId > 0)
                    beardItemId = 15160 + (beardGumpId - 53500);

                ctx.BeardItemId = beardItemId;
                pm.FacialHairItemID = beardItemId;
                pm.FacialHairHue = ctx.BeardHue;
            }
            else
            {
                ctx.BeardItemId = 0;
                pm.FacialHairItemID = 0;
                pm.FacialHairHue = 0;
            }

            // força update no client
            pm.Delta(MobileDelta.Hair | MobileDelta.FacialHair);
            pm.ProcessDelta();
        }

        private static int GetCultureIndex(string cultureId)
        {
            if (String.IsNullOrWhiteSpace(cultureId))
                return 0;

            // IDs que você me passou:
            // "zorteros", "sarangs", "matalun", "kamay"
            if (cultureId.Equals("zorteros", StringComparison.OrdinalIgnoreCase)) return 0;
            if (cultureId.Equals("sarangs", StringComparison.OrdinalIgnoreCase)) return 1;
            if (cultureId.Equals("matalun", StringComparison.OrdinalIgnoreCase)) return 2;
            if (cultureId.Equals("kamay", StringComparison.OrdinalIgnoreCase)) return 3;

            return 0;
        }

        private static void MoveToCultureEntry(PlayerMobile pm, OSUCreationContext ctx)
        {
            var culture = OSUCultureRegistry.GetById(ctx.CultureId);

            if (culture != null)
            {
                // Ajuste os nomes conforme seu OSUCultureDefinition:
                // Ex: culture.EntryMap / culture.EntryLocation
                Map map = culture.StartMap;
                Point3D loc = culture.StartLocation;

                pm.OSURpAgeBase = ctx.RpAge;   // ou o nome real do campo no ctx
                if (pm.OSURpBirthWorldTime == default(DateTime))
                    pm.OSURpBirthWorldTime = Server.Custom.Systems.WorldTime.OSUWorldTime.WorldNow;


                if (map != null && loc != Point3D.Zero)
                {
                    pm.MoveToWorld(loc, map);
                    return;
                }
            }

            // fallback global
            pm.OSURpAgeBase = ctx.RpAge;   // ou o nome real do campo no ctx
            if (pm.OSURpBirthWorldTime == default(DateTime))
                pm.OSURpBirthWorldTime = Server.Custom.Systems.WorldTime.OSUWorldTime.WorldNow;


            pm.MoveToWorld(EntryLocation, EntryMap);
        }


    }
}
