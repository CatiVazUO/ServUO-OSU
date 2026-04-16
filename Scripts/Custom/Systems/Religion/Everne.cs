namespace Server.Custom.Systems.Religion
{
    public class Everne : OSUReligionDefinition
    {
        public override string Id => "Everne";
        public override string Name => "Everne";
        public override int DisplayOrder => 7;
        public override int IconGumpId => 164;
        public override int[] TempleRiteItemIds => new int[] { 0x0B1D, 0x143B, 0x143C, 0x1F17, 0x1F1D };
        public override int[] TempleWeddingItemIds => new int[] { 0x0B1D, 0x0B26, 0x0C3B, 0x0C3C, 0x143B, 0x143C };
        public override int TempleFuneralCoffinItemId => 0x1C41;
        public override int TempleStatueItemId => 0x18C8; // ou 0x18C9

        public override string DescriptionHtml =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Everne</B></CENTER><BR><BR>
		É muito importante não confundir o alcance de Everne. Deusa do dia e da noite, do sol e da lua, da luz e da escuridão. Muitos cometem o erro de achar que Everne
        controla o tempo, mas não, ela controla apenas os seus domínios. O tempo é muito maior do que a duração das coisas. Everne é quem trás as estações e quem define
        as marés.
        </BASEFONT>";
    }
}
