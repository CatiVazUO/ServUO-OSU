namespace Server.Custom.Systems.Religion
{
    public class Everne : OSUReligionDefinition
    {
        public override string Id => "Everne";
        public override string Name => "Everne";
        public override int DisplayOrder => 7;
        public override int IconGumpId => 164; // exemplo: coloque o id real do Dortem


        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Everne</B></CENTER><BR><BR>
		É muito importante não confundir o alcance de Everne. Deusa do dia e da noite, do sol e da lua, da luz e da escuridão. Muitos cometem o erro de achar que Everne
        controla o tempo, mas não, ela controla apenas os seus domínios. O tempo é muito maior do que a duração das coisas. Everne é quem trás as estações e quem define
        as marés.
        </BASEFONT>";
    }
}
