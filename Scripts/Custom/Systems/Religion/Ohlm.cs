namespace Server.Custom.Systems.Religion
{
    public class Ohlm : OSUReligionDefinition
    {
        public override string Id => "Ohlm";
        public override string Name => "Ohlm";
        public override int DisplayOrder => 3;
        public override int IconGumpId => 167; // exemplo: coloque o id real do Dortem
        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Ohlm</B></CENTER><BR><BR>
		Filho de Ilhena. Deus de tudo que é material. Ele criou todos os seres. Dos pássaros que vooam mais perto da luz, até os seres que se escondem
		dela nos lugares mais profundos. Ohlm é sem dúvidas o Deus mais adorado de Umanti até os dias de hoje. 
        </BASEFONT>";
    }
}
