namespace Server.Custom.Systems.Religion
{
    internal class Dortem : OSUReligionDefinition
    {
        public override string Id => "Dortem";
        public override string Name => "Dortem";
        public override int DisplayOrder => 6;
        public override int IconGumpId => 165;
        public override int[] TempleRiteItemIds => new int[] { 0x1F14, 0x1F18, 0x0E34, 0x0E2D, 0x0F5E };
        public override int[] TempleWeddingItemIds => new int[] { 0x0C90, 0x0C91, 0x0B26, 0x0F5E, 0x0E2D, 0x1F18 };
        public override int TempleFuneralCoffinItemId => 0x1C51;
        public override int TempleStatueItemId => 0x18C6; // ou 0x18C7

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Dortem</B></CENTER><BR><BR>
		Elysia é muitas coisas que regem o mundo invísível e esse mundo é muito  maior do que imaginamos. Por mais que ela seja conhecida por seu perfeccionismo, 
		nossa Deusa maior também é conhecida por seu senso de humor. Para a diversão dos Deuses Elysia pariu Dortem, Deus da sorte e do azar. Ter a sorte de
        Dortem pode ser uma das maiores dádivas que o homem recebe. Dortem não espalha seu azar com facildiade.
        </BASEFONT>";
    }
}
