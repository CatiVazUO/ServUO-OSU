using System;
using System.Text.RegularExpressions;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    public enum FontSizeMode
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }

    /// <summary>
    /// Config simples e EDITÁVEL para:
    /// - quantos caracteres cabem por LINHA (depende do HtmlWidth)
    /// - quantas LINHAS cabem por página (depende do HtmlHeight)
    /// </summary>
    public static class DocumentLayoutConfig
    {
        // ✅ Ajustável: quantos caracteres cabem em 20px de largura
        public static int BaseCharsSmall_20pxWidth = 5;
        public static int BaseCharsMedium_20pxWidth = 5;
        public static int BaseCharsLarge_20pxWidth = 3;

        // ✅ Ajustável: altura de uma linha "visual" no gump (em pixels)
        public static int LinePixelHeight = 17;

        // ✅ Ajustável: folga (0.70 = 30% de sobra)
        public static double ReserveFactor = 0.60;

        // ✅ Ajustável: arredonda para múltiplos desse número
        public static int RoundToMultipleOf = 5;

        private static readonly Regex _StripTags = new Regex(@"<[^>]+>", RegexOptions.Compiled);

        /// <summary>
        /// Novo nome (mais claro): quantas linhas cabem por página.
        /// </summary>
        public static int GetLinesPerPage(int htmlHeight)
        {
            if (htmlHeight <= 0 || LinePixelHeight <= 0)
                return 1;

            int raw = Math.Max(1, htmlHeight / LinePixelHeight);

            // 1 linha de folga pra não raspar
            int safe = Math.Max(1, raw - 2);

            return safe;
        }

        /// <summary>
        /// ✅ Compatibilidade com código antigo (se algum arquivo ainda chama GetMaxLines).
        /// </summary>
        public static int GetMaxLines(int htmlHeight)
        {
            return GetLinesPerPage(htmlHeight);
        }

        public static int GetMaxCharsPerLine(int htmlWidth, FontSizeMode size)
        {
            if (htmlWidth <= 0)
                return 1;

            int blocksW = Math.Max(1, htmlWidth / 20);

            int baseCount;
            switch (size)
            {
                default:
                case FontSizeMode.Small: baseCount = BaseCharsSmall_20pxWidth; break;
                case FontSizeMode.Medium: baseCount = BaseCharsMedium_20pxWidth; break;
                case FontSizeMode.Large: baseCount = BaseCharsLarge_20pxWidth; break;
            }

            double raw = (double)(blocksW * baseCount);
            int capped = (int)Math.Floor(raw * ReserveFactor);

            if (RoundToMultipleOf > 1)
                capped = (capped / RoundToMultipleOf) * RoundToMultipleOf;

            return Math.Max(5, capped);
        }

        public static int CountContentChars(string html)
        {
            if (string.IsNullOrEmpty(html))
                return 0;

            string stripped = _StripTags.Replace(html, string.Empty);
            return stripped.Length;
        }

        public static int GetMaxContentChars(int htmlWidth, int htmlHeight, FontSizeMode size)
        {
            int lines = GetLinesPerPage(htmlHeight);
            int perLine = GetMaxCharsPerLine(htmlWidth, size);

            // Total base
            int total = lines * perLine;

            // Uma folga extra (pra bold/itálico e evitar cortar palavra)
            // Ajuste esse fator se você achar que está muito apertado ou muito folgado.
            total = (int)(total * 0.90);

            return Math.Max(10, total);
        }
    }
}
