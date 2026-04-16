namespace Server.Custom.Systems.Religion
{
    public class Lamoa : OSUReligionDefinition
    {
        public override string Id => "Lamoa";
        public override string Name => "Lamoa";
        public override int DisplayOrder => 4;
        public override int IconGumpId => 166;
        public override int[] TempleRiteItemIds => new int[] { 0x0B17, 0x0B18, 0x1F14, 0x1F1C, 0x0E24 };
        public override int[] TempleWeddingItemIds => new int[] { 0x0C3B, 0x0C3C, 0x0B17, 0x0B18, 0x0B26, 0x0E2D };
        public override int TempleFuneralCoffinItemId => 0x1C4F;
        public override int TempleStatueItemId => 0x18BE; // ou 0x18BF
        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Lamoa</B></CENTER><BR><BR>
        Lamoa foi o primeiro filho de Elysia, e sua mais perfeita criação. O mundo só existe hoje porque Lamoa é perfeita. Deusa do bem e do mal. 
        Podemos escolher acreditar que fazemos nossas próprias escolhas. Que escolhemos sobre qual lado vamos alimentar, mas os contos dizem o contrário. Tudo se
        equilibra tanto dentro quanto fora de nós  
        </BASEFONT>";
    }
}
