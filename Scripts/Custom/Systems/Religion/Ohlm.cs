namespace Server.Custom.Systems.Religion
{
    public class Ohlm : OSUReligionDefinition
    {
        public override string Id => "Ohlm";
        public override string Name => "Ohlm";
        public override int DisplayOrder => 3;
        public override int IconGumpId => 167;
        public override int[] TempleRiteItemIds => new int[] { 0x1EBD, 0x1EBC, 0x0F0E, 0x0FA0, 0x1F2E };
        public override int[] TempleWeddingItemIds => new int[] { 0x0B26, 0x0B1D, 0x0C90, 0x0C91, 0x1EBC, 0x1EBD };
        public override int TempleFuneralCoffinItemId => 0x1C43;
        public override int TempleStatueItemId => 0x18BA; // ou 0x18BB 

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Ohlm</B></CENTER><BR><BR>
		Filho de Ilhena. Deus de tudo que é material. Ele criou todos os seres. Dos pássaros que vooam mais perto da luz, até os seres que se escondem
		dela nos lugares mais profundos. Ohlm é sem dúvidas o Deus mais adorado de Umanti até os dias de hoje. 
        </BASEFONT>";
    }
}
