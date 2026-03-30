namespace Server.Custom.Systems.Religion
{
    public class Cailan : OSUReligionDefinition
    {
        public override string Id => "Cailan";
        public override string Name => "Cailan";
        public override int DisplayOrder => 5;

        public override int IconGumpId => 160; // exemplo: coloque o id real do Dortem

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Cailan</B></CENTER><BR><BR>
        Cailan foi o último filho de Ohlm e o único criado por puro egoísmo. Ele queria continuar criando cada vez mais e enchendo Umanti de seres.
        Na época, seres ainda inanimados, mas uma criação infinita num espaço limitado não se sustenta. Foi aí que Ohlm teve a ideia de Cailan,
        Deus da vida e da morte. Para que suas criações ganhassem vida também precisariam morrer.
        </BASEFONT>";
    }
}
