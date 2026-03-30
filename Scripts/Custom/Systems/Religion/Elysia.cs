namespace Server.Custom.Systems.Religion
{
    public class Elysia : OSUReligionDefinition
    {
        public override string Id => "Elysia";
        public override string Name => "Elysia";
        public override int DisplayOrder => 1;

        public override int IconGumpId => 159; // exemplo: coloque o id real do Dortem


        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Elysia</B></CENTER><BR><BR>
		Filha de Ilhena. Deusa de tudo que não é material. Quando oramos por uma boa colheita, não é para Elysia que oramos, pois ela não criou nem a
        semente, nem a terra, nem a chuva, nem o fruto. Elysia criou a fé.
        </BASEFONT>";
    }
}
