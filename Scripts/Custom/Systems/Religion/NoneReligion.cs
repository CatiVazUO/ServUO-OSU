namespace Server.Custom.Systems.Religion
{
    public class NoneReligion : OSUReligionDefinition
    {
        public override string Id => "none";
        public override string Name => "Sem Deus";
        public override int DisplayOrder => 8;

        public override int IconGumpId => 158;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Sem Deus</B></CENTER><BR><BR>
Você não segue nenhum deus. Isso também terá efeitos próprios no futuro. Você não será penalizado por não ter uma religião estabelecida.
</BASEFONT>";
    }
}
