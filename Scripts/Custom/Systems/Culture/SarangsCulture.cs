using Server.Custom.Systems.Culture;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Creation.Cultures
{
    public class SarangsCulture : OSUCultureDefinition
    {
        public override string Id => "sarangs";
        public override string DisplayName => "Sarangs";
        public override int DisplayOrder => 2;
        public override int PortraitGumpId => 115;
        public override int[] MaleHairGumpIds => new[] {
            54163, //1
            54168, //2
            54081, //3
            54187, //4
            54090, //5
            54206, //6
            54160, //7
            54044, //8
            54059, //9
            54007, //10
            54091, //11
            54106, //12
            54139, //13
            54002, //14
            54004, //15
            54205 }; //16

        public override int[] FemaleHairGumpIds => new[] {
            64055, //1...
            64163, //2
            64060, //3
            64044, //4
            64117, //5
            64129, //6
            64135, //7
            64118, //8
            64070, //9
            64210, //10
            64149, //11
            64125, //12 
            64131, //13 
            64162, //14
            64216, //15
            64178 }; //16

        public override int[] MaleBeardGumpIds => new[] {
            0,
            53515,
            53522,
            53562,
            53523,
            53588,
            53537,
            53533,
            53529,
            53545,
            53569, // bigode anos 60
            53574, // barba curta mas cheia
            53575, // barba curta e shaggy
            53577, // long beard kinda crazy
            53587, // bigode fininho longo
            53590  // barbicha com bigode fininho
            };

        public override int[] SkinHues => new[] { 1001, 1002, 1003, 1008, 1009, 1010, 1015, 1016, 1017, 1022, 1023, 1024, 1029, 1030, 1031, 1036, 1037, 1038, 1044, 1045 };
        public override string CapitalCityId => "Lurone";
        public override string CapitalCityName => "Lurone";

        // Troque depois pelas coords reais
        public override Point3D StartLocation => new Point3D(1673, 1603, 10);
        public override Map StartMap => Map.Trammel;

        public override int[] HairColorHues => new int[]
            {
                1102, 1108, 1110, 1116, 1125, 1130, 1135, 1140
            };

        public override int[] BeardHues => new int[]
            {
            1102, 1108, 1110, 1116, 1125, 1130, 1135, 1140
};
        public override void GiveStartingOutfit(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            // limpa só o que estiver equipado se você quiser (opcional)
            // (eu NÃO recomendo limpar aqui se você já limpa na pedra)
            var hue = 0x1B4;
            if (pm.Female)
            {
                pm.TryEquipItem(new FancyDress(hue));
                pm.TryEquipItem(new Sandals());
            }
            else
            {
                pm.TryEquipItem(new Shirt(hue));
                pm.TryEquipItem(new LongPants(hue));
                pm.TryEquipItem(new Shoes());
            }
        }

        public override void GiveStartingItems(PlayerMobile pm)
        {
            if (pm?.Backpack == null) return;

            // item cultural (exemplo)
            pm.AddToBackpack(new Candle());
        }


        //public override string CapitalCity => "Sa-Ra-Ang";
        //public override string Economy => "Capitalista, centrada na prestação de serviços";
        //public override string GovernedBy => "Um líder escolhido democraticamente";

        public override string LoreHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Os Sarangs</B></CENTER><BR><BR>
    Nos 100 anos seguintes à calamidade que destruiu Antares... foi saqueando, roubando e enganando que os Sarangs se tornaram o que são hoje...<BR><BR>
    Relação ambígua com os outros povos...<BR><BR>
    Política baseada em hierarquia... economia baseada na exploração de recursos e venda de serviços...<BR><BR>
    Vivem de forma hedonista e pragmática, valorizando riqueza, poder e influência...<BR><BR>
    <B>Cidade Capital:</B> Lurone <BR>
    <B>Economia:</B> Capitalista, centrada na prestação de serviços<BR>
    <B>Governado por:</B> Um líder escolhido democraticamente
    </BASEFONT>";

        public override string ProverbiosHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Provérbios</B></CENTER><BR><BR>
    &quot;Quem espera pelo momento perfeito nunca sai do lugar.&quot;<BR><BR>
    &quot;Aqueles que são rápidos em agir muitas vezes colhem as maiores recompensas.&quot;<BR><BR>
    &quot;Quando a sorte sorri, é preciso sorrir de volta.&quot;<BR><BR>
    &quot;Se a porta da oportunidade se fecha, é preciso procurar uma janela.&quot;
    </BASEFONT>";

        public override string TradicoesHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Tradições</B></CENTER><BR><BR>
    <B>Dia do Lote Vago:</B> dia para reivindicar legalmente propriedade abandonada...<BR><BR>
    <B>Dia dos antepassados:</B> velas acesas durante 3 dias...<BR><BR>
    <B>Caçada noturna:</B> na primeira lua cheia do ano...<BR><BR>
    <B>Torneio de luta:</B> torneio anual para determinar o campeão...
    </BASEFONT>";

        public override string PapeisHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Papéis na sociedade</B></CENTER><BR><BR>
    <B>Guerreiros:</B> muito valorizados, vistos como fortes e capazes...<BR><BR>
    <B>Magos:</B> vistos com desconfiança, mas reconhecem a importância da magia...<BR><BR>
    <B>Artesãos:</B> grande papel, criam e reparam armas e armaduras...
    </BASEFONT>";

        public override string FisicoHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Características físicas</B></CENTER><BR><BR>
    Aparência austera e prática, roupas funcionais em tons escuros. Tendem a ser fortes e resistentes, astutos e calculistas; também vaidosos.<BR><BR>
    </BASEFONT>";
    }
}
