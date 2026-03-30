using System;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    public enum TextAlignMode
    {
        Left = 0,
        Center = 1
    }

    public enum TextColorMode
    {
        Black = 0,
        White = 1
    }

    /// <summary>
    /// Estilo da LINHA: apenas alinhamento.
    /// </summary>
    [Serializable]
    public struct LineStyle
    {
        public TextAlignMode Align;

        public LineStyle(TextAlignMode align)
        {
            Align = align;
        }

        public static LineStyle Default
        {
            get { return new LineStyle(TextAlignMode.Left); }
        }
    }

    /// <summary>
    /// Estilo da PALAVRA (última palavra no editor).
    /// </summary>
    [Serializable]
    public struct WordStyle
    {
        public bool Bold;
        public bool Italic;
        public bool Underline;
        public FontSizeMode FontSize;

        public WordStyle(FontSizeMode size)
        {
            Bold = false;
            Italic = false;
            Underline = false;
            FontSize = size;
        }

        public static WordStyle Default
        {
            get { return new WordStyle(FontSizeMode.Medium); }
        }
    }
}
