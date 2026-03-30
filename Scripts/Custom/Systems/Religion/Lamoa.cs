namespace Server.Custom.Systems.Religion
{
    public class Lamoa : OSUReligionDefinition
    {
        public override string Id => "Lamoa";
        public override string Name => "Lamoa";
        public override int DisplayOrder => 4;
        public override int IconGumpId => 166; // exemplo: coloque o id real do Dortem
        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Lamoa</B></CENTER><BR><BR>
        Lamoa foi o primeiro filho de Elysia, e sua mais perfeita criação. O mundo só existe hoje porque Lamoa é perfeita. Deusa do bem e do mal. 
        Podemos escolher acreditar que fazemos nossas próprias escolhas. Que escolhemos sobre qual lado vamos alimentar, mas os contos dizem o contrário. Tudo se
        equilibra tanto dentro quanto fora de nós  
        </BASEFONT>";
    }
}
