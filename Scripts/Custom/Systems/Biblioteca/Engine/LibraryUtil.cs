using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Items;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Server.Custom.Biblioteca.Engine
{
    public static class LibraryUtil
    {
        public static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            string formD = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);

            for (int i = 0; i < formD.Length; i++)
            {
                char c = formD[i];
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static bool IsLoosePage(Item item)
        {
            // Seu sistema tem HtmlLoosePage; se o nome mudar, isso ainda pega por contains
            string n = item.GetType().Name;
            return n.IndexOf("Loose", StringComparison.OrdinalIgnoreCase) >= 0
                   && n.IndexOf("Page", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool TryGetPublicationInfo(Item item, out string title, out OSULanguage language, out string authorDisplay, out bool isAnonymous, out bool isCompilation)
        {
            title = null;
            language = OSULanguage.Common;
            authorDisplay = null;
            isAnonymous = false;
            isCompilation = false;

            if (item == null || item.Deleted)
                return false;

            // HtmlDocumentBase
            HtmlDocumentBase doc = item as HtmlDocumentBase;
            if (doc != null)
            {
                if (!doc.IsSealed)
                    return false;

                title = (doc.DocumentTitle ?? string.Empty).Trim();
                language = doc.Language;

                // autor real
                string sealedBy = (doc.SealedBy ?? string.Empty).Trim();

                // flag anônimo pode existir (você adicionou), então pegamos por reflection pra compatibilidade
                bool showAuthor = true;
                PropertyInfo p = doc.GetType().GetProperty("ShowAuthorOnTooltip");
                if (p != null && p.PropertyType == typeof(bool))
                {
                    try { showAuthor = (bool)p.GetValue(doc, null); }
                    catch { showAuthor = true; }
                }

                isAnonymous = !showAuthor;
                authorDisplay = showAuthor ? sealedBy : "Anônimo";
                if (string.IsNullOrWhiteSpace(authorDisplay))
                    authorDisplay = "Anônimo";

                isCompilation = false;
                return !string.IsNullOrWhiteSpace(title);
            }

            // HtmlCompilationBook
            HtmlCompilationBook comp = item as HtmlCompilationBook;
            if (comp != null)
            {
                if (!comp.IsSealed)
                    return false;

                title = (comp.DocumentTitle ?? string.Empty).Trim();
                language = comp.Language;

                string compiledBy = (comp.CompiledBy ?? string.Empty).Trim();

                bool showAuthor = true;
                PropertyInfo p = comp.GetType().GetProperty("ShowAuthorOnTooltip");
                if (p != null && p.PropertyType == typeof(bool))
                {
                    try { showAuthor = (bool)p.GetValue(comp, null); }
                    catch { showAuthor = true; }
                }

                isAnonymous = !showAuthor;
                authorDisplay = showAuthor ? compiledBy : "Anônimo";
                if (string.IsNullOrWhiteSpace(authorDisplay))
                    authorDisplay = "Anônimo";

                isCompilation = true;
                return !string.IsNullOrWhiteSpace(title);
            }

            return false;
        }

        public static string GetLanguageName(OSULanguage lang)
        {
            return OSULanguageNames.GetName(lang);
        }
    }
}
