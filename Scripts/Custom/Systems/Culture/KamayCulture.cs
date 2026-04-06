using Server.Custom.Systems.Culture;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Creation.Cultures
{
    public class KamayCulture : OSUCultureDefinition
    {
        public override string Id => "kamay";
        public override string DisplayName => "Kamay";
        public override int DisplayOrder => 4;
        public override int PortraitGumpId => 117;
        public override int[] MaleHairGumpIds => new[] {
            54061, //1
            54067, //2
            54076, //3
            54142, //4
            54144, //5
            54207, //6
            54092, //7 
            54055, //8
            54096, //9 
            54070, //10
            54103, //11
            54100, //12
            54104, //13
            54068, //14
            54069, //15 
            54188 }; //16 
        public override int[] FemaleHairGumpIds => new[] {
            64174, //1 
            64081, //2
            64164, //3
            64095, //4
            64096, //5
            64101, //6
            64080, //7
            64186, //8
            64189, //9 
            64192, //10
            64097, //11
            64099, //12
            64203, //13
            64193, //14
            64194, //15
            64204 }; //16

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
            53545, // 
            53543, // costeleta
            53544, // barbicha minuscula
            53547, // curly beard eyebrowns
            53549, // tied beard eyebrowns
            53550, // three piece beard eyebrowns
            53539  // beard no barbicha
            };

        public override int[] SkinHues => new[] { 1001, 1002, 1003, 1008, 1009, 1010, 1015, 1016, 1017, 1022, 1023, 1024, 1029, 1030, 1031, 1036, 1037, 1038, 1044, 1045 };
        public override string CapitalCityId => "Aurora";
        public override string CapitalCityName => "Aurora";

        // Troque depois pelas coords reais
        public override Point3D StartLocation => new Point3D(1693, 1612, 10);
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
            var hue = 0x2A9;
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


        //public override string CapitalCity => "Aurora";
        //public override string Economy => "Capitalista, centrado na Educação";
        //public override string GovernedBy => "Ministérios (voto obrigatório)";

        public override string LoreHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Os Kamay</B></CENTER><BR><BR>
    Após a calamidade... chamados de “cúmplices”...<BR><BR>
    Construíram cidade próxima às ruínas de Antares e se dedicaram ao estudo... centro de conhecimento, livros, pergaminhos, biblioteca e escola de magia...<BR><BR>
    Sociedade baseada em meritocracia e educação... política externa pacífica...<BR><BR>
    Economia especializada em conhecimento mágico e aprendizado... muitos jovens de outros povos vão estudar com eles...<BR><BR>
    <B>Cidade Capital:</B> Aurora<BR>
    <B>Economia:</B> Capitalista, centrado na Educação<BR>
    <B>Regido por:</B> Governo por Ministérios (voto obrigatório)
    </BASEFONT>";

        public override string ProverbiosHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Provérbios</B></CENTER><BR><BR>
    &quot;O poder do conhecimento é inestimável, mas seu uso é que determina seu valor.&quot;<BR><BR>
    &quot;As escolhas de hoje moldam o destino de amanhã.&quot;<BR><BR>
    &quot;Aquele que sabe demais pode ser temido, mas aquele que compartilha seu conhecimento é admirado.&quot;<BR><BR>
    &quot;A humildade é a marca dos sábios.&quot;
    </BASEFONT>";

        public override string TradicoesHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Tradições</B></CENTER><BR><BR>
    <B>Fortalecimento da Comunidade:</B> eventos de coesão social e celebrações...<BR><BR>
    <B>Dia da Graduação:</B> quando um aluno consegue ensinar 10 coisas diferentes a seus mestres...<BR><BR>
    <B>Dia do Outro:</B> cerimônia oferecendo presentes a alguém de outro povo...
    </BASEFONT>";

        public override string PapeisHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Papéis na sociedade</B></CENTER><BR><BR>
    <B>Magos:</B> papel vital, considerados os mais importantes... rituais e feitiços de proteção...<BR><BR>
    <B>Guerreiros:</B> papel menos proeminente por ser uma sociedade pacífica...<BR><BR>
    <B>Artesãos:</B> produzem necessidades do dia a dia, mas status social não é exaltado...
    </BASEFONT>";

        public override string FisicoHtml =>
    @"<BASEFONT COLOR=#FFFFFF>
    <CENTER><B>Características físicas</B></CENTER><BR><BR>
    Postura imponente, expressão determinada, traços marcantes. Roupas resistentes e confortáveis em cores neutras; jóias como símbolo de poder e status. Aparência pode ser variada pela presença de visitantes.<BR><BR>
    </BASEFONT>";
    }
}
