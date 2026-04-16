namespace Server.Custom.Systems.Religion
{
    public class Elysia : OSUReligionDefinition
    {
        public override string Id => "Elysia";
        public override string Name => "Elysia";
        public override int DisplayOrder => 1;
        public override int IconGumpId => 159;
        public override int[] TempleRiteItemIds => new int[] { 0x1F14, 0x1F1C, 0xFEF, 0x0E34, 0x122A };
        public override int[] TempleWeddingItemIds => new int[] { 0x0C37, 0x0C38, 0x0B26, 0x122A, 0x0E2D, 0x1F13 };
        public override int TempleFuneralCoffinItemId => 0x1C41;
        public override int TempleStatueItemId => 0x18C4; // ou 0x18C5

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Elysia</B></CENTER><BR><BR>
		Filha de Ilhena. Deusa de tudo que não é material. Quando oramos por uma boa colheita, não é para Elysia que oramos, pois ela não criou nem a
        semente, nem a terra, nem a chuva, nem o fruto. Elysia criou a fé.
        </BASEFONT>";
    }
}
