using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.SkillXP.Engine
{
    /// <summary>
    /// IDs únicos das habilidades de idioma.
    /// Ajuste se você já usa esse range no seu shard.
    /// </summary>
    public static class LanguageAbilityIds
    {
        public const int SpeakCommon  = 210001;

        // 4 povos (mapeados para OSULanguage.People1..People4)
        public const int SpeakSarang = 210002; // Ex.: Sarang
        public const int SpeakKamay = 210003; // Ex.: Kamay
        public const int SpeakMatalun = 210004; // Ex.: Matalun
        public const int SpeakZorteros = 210005; // Ex.: Zorteros

        // Antigas
        public const int SpeakAludin  = 210006;
        public const int SpeakTherok  = 210007;

        public static int ForLanguage(OSULanguage lang)
        {
            switch (lang)
            {
                default:
                case OSULanguage.Common:  return SpeakCommon;
                case OSULanguage.Sarang: return SpeakSarang;
                case OSULanguage.Kamay: return SpeakKamay;
                case OSULanguage.Matalun: return SpeakMatalun;
                case OSULanguage.Zorteros: return SpeakZorteros;
                case OSULanguage.Aludin:  return SpeakAludin;
                case OSULanguage.Therok:  return SpeakTherok;
            }
        }
    }
}
