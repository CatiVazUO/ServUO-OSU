using Server.Custom.Systems.Culture;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Creation.Cultures
{
    public class ZorterosCulture : OSUCultureDefinition
    {
        public override string Id => "zorteros";
        public override string DisplayName => "Zorteros";
        public override int DisplayOrder => 1;
        public override int PortraitGumpId => 114;
        public override int[] MaleHairGumpIds => new[] {
            54056, //1
            54060, //2
            54177, //3
            54197, //4
            54199, //5
            54000, //6 
            54209, //7
            54140, //8
            54009, //9
            54019, //10
            54143, //11
            54065, //12
            54208, //13
            54101, //14
            54037, //15
            54119 }; //16
        public override int[] FemaleHairGumpIds => new[] {
            64196, //1
            64197, //2
            64212, //3 
            64216, //4
            64035, //5
            64008, //6
            64190, //7
            64179, //8
            64021, //9
            64022, //10
            64038, //11
            64053, //12
            64136, //13
            64213, //14
            64214, //15
            64218 }; //16

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
            53502, // full beard straight
            53503, // full beard small shaggy
            53514, // full beard 3 pieces
            53517, // full beard shaggy
            53519, // full beard full mustache
            53524  // long beard 
            };


        public override int[] SkinHues => new[] { 1001, 1002, 1003, 1008, 1009, 1010, 1015, 1016, 1017, 1022, 1023, 1024, 1029, 1030, 1031, 1036, 1037, 1038, 1044, 1045 };
        public override string CapitalCityId => "Willran";
        public override string CapitalCityName => "Willran";

        // Troque depois pelas coords reais
        public override Point3D StartLocation => new Point3D(1673, 1612, 10);

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
            var hue = 0x2E;
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


        //public override string CapitalCity => "Willran";
        //public override string Economy => "Capitalista, centrada no comércio";
        //public override string GovernedBy => "Conselho de anciões";

        public override string LoreHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Os Zorteros</B></CENTER><BR><BR>
    Após a calamidade que devastou Antares, a guarda da cidade foi dividida em duas facções...<BR><BR>
    Os covardes, como os outros povos os chamavam, se refugiaram numa carverna próxima a Antares...<BR><BR>
    Com o passar dos anos, a cidade dos covardes se tornou um centro comercial importante...<BR><BR>
    Os Zorteros mantinham relações amistosas com os outros povos...<BR><BR>
    A política da cidade dos Zorteros era baseada na valorização da sabedoria e da experiência de vida...<BR><BR>
    <B>Cidade Capital:</B> Willran<BR>
    <B>Economia:</B> Capitalista, centrada no comércio<BR>
    <B>Governado por:</B> Conselho de anciões
    </BASEFONT>";

        public override string ProverbiosHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Provérbios</B></CENTER><BR><BR>
    &quot;Um covarde não é aquele que teme o perigo, mas aquele que o enfrenta sem uma estratégia.&quot;<BR><BR>
    &quot;Não é a força que prevalece, mas a inteligência para superar o inimigo.&quot;<BR><BR>
    &quot;O medo não é uma fraqueza, é uma ferramenta para a sobrevivência.&quot;
    </BASEFONT>";

        public override string TradicoesHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Tradições</B></CENTER><BR><BR>
    <B>Concurso de Histórias:</B> competição anual de narrativas...<BR><BR>
    <B>Jogo da Estratégia:</B> jogo de tabuleiro que simula situações de risco...<BR><BR>
    <B>Sabedoria dos Anciões:</B> ritual quando um novo conselheiro é escolhido...
    </BASEFONT>";

        public override string PapeisHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Papéis na sociedade</B></CENTER><BR><BR>
    <B>Guerreiros:</B> posição ambígua, necessários para a proteção, mas vistos como um grupo potencialmente perigoso...<BR><BR>
    <B>Magos:</B> posição de respeito, fascinados com o estudo e uso da magia como forma de proteção...<BR><BR>
    <B>Artesãos:</B> altamente valorizados por criar objetos úteis e bonitos...
    </BASEFONT>";

        public override string FisicoHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Características físicas</B></CENTER><BR><BR>
    Geralmente menores em estatura, magros, com força surpreendente. Agilidade e flexibilidade notáveis... olhos grandes e expressivos... roupas coloridas e bem adornadas.<BR><BR>
    </BASEFONT>";
    }
}
