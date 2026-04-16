namespace Server.Custom.Systems.Religion
{
    public class Ilhena : OSUReligionDefinition
    {
        public override string Id => "Ilhena";
        public override string Name => "Ilhena";
        public override int DisplayOrder => 2;
        public override int IconGumpId => 162;
        public override int[] TempleRiteItemIds => new int[] { 0x1223, 0x1224, 0x1F0B, 0x0B1D, 0x1F18 };
        public override int[] TempleWeddingItemIds => new int[] { 0x1223, 0x1224, 0x0B1D, 0x0B26, 0x0C3B, 0x0C3C };
        public override int TempleFuneralCoffinItemId => 0x1C42;
        public override int TempleStatueItemId => 0x18C0;  // 0x18C1

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Ilhena</B></CENTER><BR><BR>
		Deusa suprema, fagulha divina, inicio de toda a criação. Não é um ser sapiente pois é a própria sapiencia. Ela é o tudo e o nada. Ilhena
		existe por que qualquer coisa existe, e a ausência dela é o vazio completo. 
        </BASEFONT>";
    }
}
