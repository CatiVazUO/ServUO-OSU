using System;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    // 4 línguas de povos + Comum + 2 antigas
    public enum OSULanguage
    {
        Common = 0,

        // MAPEAMENTO PADRÃO (AJUSTE PARA O SEU SHARD):
        // People1 = Sarang
        // People2 = Kamay
        // People3 = Matalun
        // People4 = Zorteros
        Sarang = 1,
        Kamay = 2,
        Matalun = 3,
        Zorteros = 4,

        Aludin = 5,
        Therok = 6
    }

    public static class OSULanguageNames
    {
        public static string GetName(OSULanguage lang)
        {
            switch (lang)
            {
                default:
                case OSULanguage.Common: return "Comum";
                case OSULanguage.Sarang: return "Sarang";
                case OSULanguage.Kamay: return "Kamay";
                case OSULanguage.Matalun: return "Matalun";
                case OSULanguage.Zorteros: return "Zorteros";
                case OSULanguage.Aludin: return "Aludin";
                case OSULanguage.Therok: return "Therok";
            }
        }
    }
}
