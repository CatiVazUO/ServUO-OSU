using Server.Custom.Correios;
using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    // Deixe PUBLIC para os itens sobrescreverem
    public class DocumentGumpLayout
    {
        // ====== IMAGEM DO LIVRO/PERGAMINHO (gump image id)
        public int BookImageID = 3509;
        public int BookImageX = 42;
        public int BookImageY = 260;

        // ====== PREVIEW HTML (página esquerda)
        public int PreviewLabelX = 191;
        public int PreviewLabelY = 269;
        public int LeftHtmlX = 111;
        public int HtmlY = 293;
        public int HtmlWidth = 213;
        public int HtmlHeight = 284;

        // Distância entre páginas (esquerda->direita)
        public int HtmlGap = 50;

        // ====== LABELS DE PÁGINA (1/10, 2/10)
        public int LeftPageLabelX = 208;
        public int LeftPageLabelY = 584;
        public int RightPageLabelX = 459;
        public int RightPageLabelY = 587;

        // ====== SETAS (botões) NO LIVRO
        public int PrevBtnX = 46;
        public int PrevBtnY = 430;
        public int PrevBtnUpID = 451;
        public int PrevBtnDownID = 451;

        public int NextBtnX = 621;
        public int NextBtnY = 430;
        public int NextBtnUpID = 450;
        public int NextBtnDownID = 450;

        // ====== SELO (gump 1823..1923)
        // Posição do selo quando o documento estiver selado.
        // Pode ser sobrescrito em cada item (GetLayout) para livros de tamanhos diferentes.
        public int SealX = 470;
        public int SealY = 295;

        // ====== PAINEL DIREITO (EDITOR) - POSIÇÃO/BASE
        // Se você quiser layouts diferentes por livro, altere aqui no item.
        public int EditorPanelX = 658;
        public int EditorPanelY = 223;

        // Largura/altura do painel (pode crescer em livros maiores)
        public int EditorPanelWidth = 570;
        public int EditorPanelHeight = 450;

        // ====== LINHAS (TextEntry)
        public int LineNumberX = 880;
        public int RadioX = 860;
        public int TextEntryX = 900;
        public int LinesStartY = 262;
        public int LineRowHeight = 25;

        public int TextEntryWidth = 285;
        public int TextEntryHeight = 20;

        // ====== BOTÕES (coluna da esquerda do painel direito)
        public int ButtonsX = 700;

        // Info "Linha X (máx ...)"
        public int LineInfoX = 867;
        public int LineInfoY = 230; 

        // Hue labels brancos
        public int LabelHue = 0x481;
    }
}
