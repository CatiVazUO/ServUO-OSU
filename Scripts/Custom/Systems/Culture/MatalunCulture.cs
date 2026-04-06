using Server.Custom.Systems.Culture;
using Server.Engines.TreasuresOfDoom;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Creation.Cultures
{
    public class MatalunCulture : OSUCultureDefinition
    {
        public override string Id => "matalun";
        public override string DisplayName => "Matalun";
        public override int DisplayOrder => 3;
        public override int PortraitGumpId => 116;
        public override int[] MaleHairGumpIds => new[] {
            54136, //1
            54092, //2
            54147, //3 
            54141, //4
            54159, //5
            54149, //6 
            54198, //7
            54217, //8
            54025, //9
            54063, //10
            54160, //11
            54133, //12
            54050, //13
            54173, //14
            54054, //15
            54058 }; //16
        public override int[] FemaleHairGumpIds => new[] {
            64094, //1
            64054, //2
            64095, // 3
            64198, //4
            64217, //5
            64102, //6
            64114, //7
            64115, //8
            64116, //9
            64119, //10
            64122, //11
            64123, //12
            64112, //13
            64185, //14
            64186, //15
            64191 }; //16

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
            53526, // long beard shaggy
            53532, // small mustache
            53534, // mustache skinny
            53536, // long beard small mustache
            53538, // barba rala
            53542  // up mustache 
            };

        public override int[] SkinHues => new[] { 1001, 1002, 1003, 1008, 1009, 1010, 1015, 1016, 1017, 1022, 1023, 1024, 1029, 1030, 1031, 1036, 1037, 1038, 1044, 1045 };
        public override string CapitalCityId => "Xetá";
        public override string CapitalCityName => "Xetá";

        // Troque depois pelas coords reais
        public override Point3D StartLocation => new Point3D(1654, 1612, 10);
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
            var hue = 0x1FC;
            if (pm == null || pm.Deleted)
                return;

            // limpa só o que estiver equipado se você quiser (opcional)
            // (eu NÃO recomendo limpar aqui se você já limpa na pedra)

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


        //public override string CapitalCity => "Xetá";
        //public override string Economy => "Socialismo, centrado na agricultura";
        //public override string GovernedBy => "Sacerdotisa (processo seletivo a cada 10 anos)";

        public override string LoreHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Os Matalun</B></CENTER><BR><BR>
    Cidade pequena e recém-estabelecida, conhecida por resistência e coragem...<BR><BR>
    Economia gira em torno da agricultura... fé é o que faz a cidade se mover... pequenos altares e capelas...<BR><BR>
    Liderados por uma profetisa espiritual (sempre mulher)... ninguém pode manter riquezas próprias...<BR><BR>
    Rigorosos com novos moradores, mas abertos a visitantes... missionários viajam pregando o “canto”...<BR><BR>
    <B>Cidade Capital:</B> Xetá<BR>
    <B>Economia:</B> Socialismo, centrado na agricultura<BR>
    <B>Regido por:</B> Teocracia Coletiva
    </BASEFONT>";

        public override string ProverbiosHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Provérbios</B></CENTER><BR><BR>
    &quot;A disciplina é a chave para a sabedoria.&quot;<BR><BR>
    &quot;O corpo é a prisão da mente, mas Deus é a chave para a liberdade.&quot;<BR><BR>
    &quot;O medo é um veneno para a alma, a fé é o antídoto.&quot;<BR><BR>
    &quot;As sementes devem ser regadas com disciplina e cuidado para florescerem.&quot;
    </BASEFONT>";

        public override string TradicoesHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Tradições</B></CENTER><BR><BR>
    <B>Cerimônia de iniciação:</B> jovens passam por ritual para se tornarem membros plenos...<BR><BR>
    <B>Peregrinação anual:</B> renovam fé e conexão com seus deuses...<BR><BR>
    <B>Canto em uníssono:</B> todo dia 1 de cada mês...<BR><BR>
    <B>Silêncio da colheita:</B> silêncio por horas após a colheita...<BR><BR>
    <B>Dia de limpeza:</B> todos trabalham juntos pela limpeza da cidade...<BR><BR>
    <B>Noite de preces:</B> crianças ouvem histórias sobre Deuses e Deusas...
    </BASEFONT>";

        public override string PapeisHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Papéis na sociedade</B></CENTER><BR><BR>
    Guerreiros, magos e artesãos são importantes e valorizados, mas nenhum é superior...<BR><BR>
    <B>Guerreiros:</B> treinados para defender a cidade e acompanhar orquestras...<BR><BR>
    <B>Magos:</B> valorizados pelo conhecimento, vistos como conhecimento sagrado ou audaciosos...<BR><BR>
    <B>Artesãos:</B> habilidades manuais vistas como arte e autoexpressão...
    </BASEFONT>";

        public override string FisicoHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Características físicas</B></CENTER><BR><BR>
    Corpos fortes e bem desenvolvidos pelo trabalho na agricultura. Rostos marcados por linhas profundas, olhos intensos e penetrantes...<BR><BR>
    </BASEFONT>";
    }
}
