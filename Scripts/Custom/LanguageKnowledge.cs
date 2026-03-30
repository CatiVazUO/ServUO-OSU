using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Custom.Systems.Skills.Abilities;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.DefQual;

namespace Server.Custom
{
    /// <summary>
    /// Checagem de "o player fala/entende a língua".
    ///
    /// ✅ Usa OSUAbilities.
    /// - Língua nativa: baseada em PlayerMobile.OSUCultureId (mapeamento abaixo).
    /// - Línguas compradas: PlayerMobile.HasOSUAbility(id).
    /// - Comum: todo mundo com INT >= 40 entende automaticamente; abaixo disso, pode comprar a ability.
    /// </summary>
    public static class LanguageKnowledge
    {
        public static OSULanguage GetNativeLanguageForCulture(string cultureId)
        {
            if (string.IsNullOrEmpty(cultureId))
                return OSULanguage.Common;

            cultureId = cultureId.ToLowerInvariant();

            switch (cultureId)
            {
                default:
                    return OSULanguage.Common;

                case "sarangs": return OSULanguage.Sarang;   // Sarang
                case "kamay":   return OSULanguage.Kamay;   // Kamay
                case "matalun": return OSULanguage.Matalun;   // Matalun
                case "zorteros":return OSULanguage.Zorteros;   // Zorteros
            }
        }

        public static bool Understands(PlayerMobile pm, OSULanguage lang)
        {
            if (pm == null)
                return false;

            if (!OSUDefQualDispatcher.CanReadAndWrite(pm))
                return false;

            if (lang == OSULanguage.Common)
            {
                if (pm.Int >= 40)
                    return true;

                return pm.HasOSUAbility(LanguageAbilityIds.SpeakCommon);
            }

            OSULanguage native = GetNativeLanguageForCulture(pm.OSUCultureId);
            if (native == lang)
                return true;

            int abilityId = LanguageAbilityIds.ForLanguage(lang);
            return pm.HasOSUAbility(abilityId);
        }

        /// <summary>
        /// Lista de idiomas que o player pode escolher no editor.
        /// Regra: mostra apenas os idiomas que ele entende (inclusive a nativa) + Comum se INT>=40 ou se comprou.
        /// </summary>
        public static OSULanguage[] GetKnownLanguages(PlayerMobile pm)
        {
            if (pm == null)
                return new OSULanguage[] { OSULanguage.Common };

            List<OSULanguage> list = new List<OSULanguage>();

            // sempre inclui a nativa (se não for Common)
            OSULanguage native = GetNativeLanguageForCulture(pm.OSUCultureId);
            if (native != OSULanguage.Common)
                list.Add(native);

            // comum se entende
            if (Understands(pm, OSULanguage.Common) && !list.Contains(OSULanguage.Common))
                list.Insert(0, OSULanguage.Common);

            // outras (compradas)
            AddIfKnows(pm, list, OSULanguage.Sarang);
            AddIfKnows(pm, list, OSULanguage.Kamay);
            AddIfKnows(pm, list, OSULanguage.Matalun);
            AddIfKnows(pm, list, OSULanguage.Zorteros);
            AddIfKnows(pm, list, OSULanguage.Aludin);
            AddIfKnows(pm, list, OSULanguage.Therok);

            return list.ToArray();
        }

        private static void AddIfKnows(PlayerMobile pm, List<OSULanguage> list, OSULanguage lang)
        {
            if (list.Contains(lang))
                return;

            if (Understands(pm, lang))
                list.Add(lang);
        }
    }
}
