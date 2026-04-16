namespace Server.Custom.Systems.Religion
{
    public class Cailan : OSUReligionDefinition
    {
        public override string Id => "Cailan";
        public override string Name => "Cailan";
        public override int DisplayOrder => 5;
        public override int IconGumpId => 160;
        public override int[] TempleRiteItemIds => new int[] { 0x0F87, 0x0F88, 0x1E1D, 0x1E20, 0x1F0B };
        public override int[] TempleWeddingItemIds => new int[] { 0x0C37, 0x0C38, 0x0F87, 0x0F88, 0x0B26, 0x1E1D };
        public override int TempleFuneralCoffinItemId => 0x1C50;
        public override int TempleStatueItemId => 0x18C2; // ou 0x18C3

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Cailan</B></CENTER><BR><BR>
        Cailan foi o último filho de Ohlm e o único criado por puro egoísmo. Ele queria continuar criando cada vez mais e enchendo Umanti de seres.
        Na época, seres ainda inanimados, mas uma criação infinita num espaço limitado não se sustenta. Foi aí que Ohlm teve a ideia de Cailan,
        Deus da vida e da morte. Para que suas criações ganhassem vida também precisariam morrer.
        </BASEFONT>";
    }
}
