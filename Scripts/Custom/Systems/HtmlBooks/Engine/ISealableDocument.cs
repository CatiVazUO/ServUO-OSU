using Server.Mobiles;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    public interface ISealableDocument
    {
        bool IsSealed { get; }

        // ID do selo exibido no gump de leitura.
        // 0 = selo genérico/invisível. 1..100 = selos custom.
        int SealId { get; set; }

        OSULanguage Language { get; set; }
        FontSizeMode FontSize { get; set; }

        int PageCount { get; }

        // área do HTML (serve para limite de chars)
        int HtmlWidth { get; }
        int HtmlHeight { get; }

        string GetPageHtml(int pageIndex);
        void SetPageHtml(int pageIndex, string html);

        void ClearAll();

        // sela definitivamente (sem volta)
        void Seal(PlayerMobile sealer);
    }
}
