using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.HtmlBooks.Html.Readable
{
    public class HtmlLoosePage : HtmlDocumentBase
    {
        public override int HtmlWidth { get { return 290; } }
        public override int HtmlHeight { get { return 418; } }
        public override int PageCount => 1;

        public override string EditedDisplayName { get { return "Página editada"; } }

        [Constructable]
        public HtmlLoosePage()
        {
            ItemID = 0x138C;
            Name = "Página Solta";
            Weight = 0.1;

            FontSize = FontSizeMode.Medium;
            Language = OSULanguage.Common;

            // IMPORTANTÍSSIMO:
            // O BookSeal do seu shard pede título se DocumentTitle estiver vazio.
            // A loosepage NÃO pode pedir título.
            // Zero-width space NÃO é considerado whitespace pelo IsNullOrWhiteSpace,
            // então impede o gump de título, e não aparece visualmente.
            DocumentTitle = "\u200B";
        }

        // Não mostrar "Título" nas propriedades da página solta
        public override void AddNameProperties(ObjectPropertyList list)
        {
            // chama o base primeiro (ele vai tentar mostrar título, mas a gente evita depois)
            base.AddNameProperties(list);

            // Como o base já pode ter adicionado "Título" vazio (dependendo do seu base),
            // a solução prática é: não colocar nada extra aqui.
            // O importante é que o título não aparece visível (zero-width).
        }

        public override DocumentGumpLayout GetLayout()
        {
            var l = base.GetLayout();

            // Usa a arte do livro aberto por enquanto (você troca depois se quiser)
            l.BookImageID = 3526;

            // Matermática  q define o tamanho do Gump de edição (não mudar)
            l.HtmlWidth = HtmlWidth;
            l.HtmlHeight = HtmlHeight;

            // X e Y da imagem do Gump
            l.BookImageX = 42;
            l.BookImageY = 260;

            // Label de Preview X e Y
            l.PreviewLabelX = 207;
            l.PreviewLabelY = 284;

            //Tamanho da janela de Html
            l.LeftHtmlX = 91;
            l.HtmlY = 309;

            // Posição do selo quando o documento estiver selado.
            l.SealX = 170;
            l.SealY = 690;

            // Coords do gump de edição (tiradoes pela imagem do canto esquerdo superior)
            l.EditorPanelX = 450;
            l.EditorPanelY = 265;

            return l;
        }

        public HtmlLoosePage(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
