namespace Server.Custom.Systems.Religion
{
    public class Ilhena : OSUReligionDefinition
    {
        public override string Id => "Ilhena";
        public override string Name => "Ilhena";
        public override int DisplayOrder => 2;

        public override int IconGumpId => 162; // exemplo: coloque o id real do Dortem


        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Ilhena</B></CENTER><BR><BR>
		Deusa suprema, fagulha divina, inicio de toda a criação. Não é um ser sapiente pois é a própria sapiencia. Ela é o tudo e o nada. Ilhena
		existe por que qualquer coisa existe, e a ausência dela é o vazio completo. 
        </BASEFONT>";
    }
}
