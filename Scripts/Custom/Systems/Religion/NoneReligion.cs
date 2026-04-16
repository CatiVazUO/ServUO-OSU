namespace Server.Custom.Systems.Religion
{
    public class NoneReligion : OSUReligionDefinition
    {
        public override string Id => "none";
        public override string Name => "Sem Deus";
        public override int DisplayOrder => 8;
        public override int IconGumpId => 158;
        public override int[] TempleRiteItemIds => new int[0];
        public override int[] TempleWeddingItemIds => new int[] { 0x0B26, 0x0B1D, 0x0C3B, 0x0C3C, 0x0C37, 0x0C38 };
        public override int TempleFuneralCoffinItemId => 0x1C41;
        public override int TempleStatueItemId => 0;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Sem Deus</B></CENTER><BR><BR>
Você não segue nenhum deus. Isso também terá efeitos próprios no futuro. Você não será penalizado por não ter uma religião estabelecida.
</BASEFONT>";
    }
}
