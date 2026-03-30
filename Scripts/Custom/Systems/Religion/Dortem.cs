namespace Server.Custom.Systems.Religion
{
    internal class Dortem : OSUReligionDefinition
    {
        public override string Id => "Dortem";
        public override string Name => "Dortem";
        public override int DisplayOrder => 6;

        public override int IconGumpId => 165; // exemplo: coloque o id real do Dortem

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Dortem</B></CENTER><BR><BR>
		Elysia é muitas coisas que regem o mundo invísível e esse mundo é muito  maior do que imaginamos. Por mais que ela seja conhecida por seu perfeccionismo, 
		nossa Deusa maior também é conhecida por seu senso de humor. Para a diversão dos Deuses Elysia pariu Dortem, Deus da sorte e do azar. Ter a sorte de
        Dortem pode ser uma das maiores dádivas que o homem recebe. Dortem não espalha seu azar com facildiade.
        </BASEFONT>";
    }
}
