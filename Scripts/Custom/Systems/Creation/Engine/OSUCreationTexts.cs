using Server.Engines.Quests;
using Server.Ethics;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;
using Ultima;

namespace Server.Custom.Systems.Creation.Engine
{
    public enum OSUCreationInfoTopic
    {
        OSU = 1,
        LoreAmanti = 2,
        Regras = 3
    }

    public static class OSUCreationTexts
    {
        // ===== Página 1 (informativa) =====
        // (Você pode trocar os textos depois sem mexer no gump)

        public static string Page1_OSU =>
            @"<BASEFONT COLOR=#FFFFFF>
            <CENTER><B>O que é a OSU</B></CENTER><BR>
            (Coloque aqui seu texto do shard...)
            </BASEFONT>";

        public static string Page1_LoreAmanti =>
            @"<BASEFONT COLOR=#FFFFFF>
            <CENTER><B>Lore de Amanti</B></CENTER><BR>
            (Coloque aqui a lore...)
            </BASEFONT>";

        public static string Page1_Regras =>
            @"<BASEFONT COLOR=#FFFFFF>
            <CENTER><B>Regras do Shard</B></CENTER><BR>
            (Coloque aqui as regras...)
            </BASEFONT>";


        // ===== Página 2 (Caminhos + Game Mode) =====

        public static string Page2_Pvp =>
            @"<BASEFONT COLOR=#FFFFFF>
            Caso você escolha ser um char PVP existem algumas regras que você precisa saber e seguir. Um char PVP, ao matar um jogador, faz com que ele perca 1 ponto de vida.
            Ao mesmo tempo que também perde um ponto de vida quando morto por um jogador PVP ou não PVP. Esse estilo de jogo são para jogadores que pretendem fazer RPs mais complexos e
            estão dispostos a viver totalmente a consequencia de suas ações, assim como aplicar a outros jogadores punições. A Staff NUNCA vai interferir no gameplay e matar um jogador,
            então fica a critério dos jogadores fazerem justiça, e criarem seus vilões. Somente chars que são PVP podem escolher perícias de ladrão, incluindo: se esconder,
            se disfarçar, roubar, olhar dentro de mochilas e arrombar fechaduras (portas). Outros jogadores não tem como saber qual escolha de jogo você fez ( se vc é ou não um jogador PVP)
            </BASEFONT>";

        public static string Page2_NoPvp =>
            @"<BASEFONT COLOR=#FFFFFF>
            Caso você escolha ser um char não PVP, ele nunca poderá tirar 1 ponto de vida de outro jogador e nunca perderá um ponto de vida ao ser morto por outro jogador.
            Esse jogador opta por esse estilo de jogo quando pretende ter uma experiência mais relaxada ou pvm. Ao escolher esse caminho o jogador não terá a opção de escolher perícias de ladrão.
            como se esconder, se disfarçar, roubar, olhar dentro de mochilas e arrombar fechaduras (portas). E não poderá ser quem pune os vilões de nossas histórias. Escolha com sabedoria.
            Outros jogadores não tem como saber qual escolha de jogo você fez ( se vc é ou não um jogador PVP)
            </BASEFONT>";

        public static string Page2_Warrior =>
            @"<BASEFONT COLOR=#FFFFFF>
            Guerreiros são personagens que estão focados em desenvolver suas perícias de combate. Esse caminho permite uma diversidade enorme de personagens e RPS, então o seu maior cap
            (Capacidade máxima de uso de pontos XP) pode ser usado na compra de especializações relacionadas a destruiçao de seus inimigos em batalha. Os guerreiros podem construir personagens
            de mago, combate a mão armada, arqueiros e domadores de animal, ou uma mistura de qualquer das perícias de combate. Os jogadores podem comprar especializações em qualquer perícias
            contanto que tenham os pontos necessários pra isso. Os XP são pontos que o jogador ganha ao usar uma perícias. Cada perícias tem um medidor de XP, e ao ter sucesso
            utilizando aquela perícia, o jogador junta pontos naquela perícia e pode gastar comprando especializações dela. Algumas perícias não estarão disponíveis para jogadores que escolheram
            ter Char não PVP.
            </BASEFONT>";

        public static string Page2_Artisan =>
            @"<BASEFONT COLOR=#FFFFFF>
            Os Artesões são personagens que estão focados em desenvolver suas perícias de trabalho. Todos os jogadores tem acesso as perícias de combate e de trabalho. A diferença entre um artesão e
            um guerreiro é somente a quantidade máxima de pontos que cada um pode gastar em suas categorias específicas. O cap dos pontos de combate e pontos de profissão são invertidos e o jogador
            pode gastar um número muito maior pontos de XP em especializações relacionadas a sua profissão. Os jogadores podem comprar especializações em qualquer perícias
            contanto que tenham os pontos necessários pra isso. Os pontos de XP são pontos que o jogador ganha ao usar uma perícias. Cada perícias tem um medidor de XP, e ao ter sucesso
            utilizando aquela perícia, o jogador junta pontos naquela perícia e pode gastar comprando especializações dela. Algumas perícias não estarão disponíveis para jogadores que escolheram
            ter Char não PVP.
            </BASEFONT>";

        public static string Page5_Religion =>
        @"<BASEFONT COLOR=#FFFFFF>
        <CENTER><B>Religiao</B></CENTER><BR><BR>
        Na criação do seu personagem, você poderá escolher uma religião.A maior parte do mundo possui crenças e rituais religiosos, e é comum que os cidadãos de Umanti pratiquem
        alguma fé — seja por tradição, cultura, família ou convicção pessoal. Na prática, as religiões existem para fortalecer a identidade e a história do seu personagem. Cada
        religião tem características, símbolos, datas especiais e rituais próprios, e isso pode combinar mais naturalmente com algumas classes e estilos de jogo do que com outros.
        Importante: nenhuma religião é mais forte ou mais fraca. Por exemplo: Os benefícios (buffs) são os mesmos para todas os deuses, mudando apenas onde ficam os templos
        que concedem esses efeitos — cada fé tem seus santuários em lugares diferentes do mapa.O impacto é principalmente de RP, e a escolha serve para dar cor, propósito e
        conexão ao mundo.
        </BASEFONT>";

        public static string Page5_SystemHtml =>
            @" < BASEFONT COLOR=#FFFFFF>
            <CENTER><B>O Sistema</B></CENTER><BR><BR>
            • Todas as skills começam <B>Locked</B>.<BR>
            • Skills ganhas não podem ser baixadas.<BR>
            • Cada skill ganha um XP diferente quando acerta/tem sucesso.<BR>
            • Com o XP especial de cada skill você compra <B>Feats</B> (especializações).<BR><BR>
            Você pode escolher <B>4 skills</B> para começar com elas <B>Unlocked</B> e com <B>30.0</B>:<BR>
            • <B>2 de Combate</B><BR>
            • <B>2 de Profissão</B><BR><BR>
            Habilidades são ganhas com o passar de nível, assim como pontos de atributos.
            </BASEFONT>";

        public enum OSUAttributeTopic
        {
            Str = 1,
            Dex = 2,
            Int = 3,
            Man = 4,
            Vit = 5,
            Cha = 6
        }

        public static string Attr_Str =>
        @"<BASEFONT COLOR=#FFFFFF><B>Força</B><BR>Afeta ... (edite aqui)</BASEFONT>";

        public static string Attr_Dex =>
        @"<BASEFONT COLOR=#FFFFFF><B>Destreza</B><BR>Afeta ...</BASEFONT>";

        public static string Attr_Int =>
        @"<BASEFONT COLOR=#FFFFFF><B>Intelecto</B><BR>Afeta ...</BASEFONT>";

        public static string Attr_HP =>
        @"<BASEFONT COLOR=#FFFFFF><B>HP</B><BR>Afeta ...</BASEFONT>";

        public static string Attr_Vit =>
        @"<BASEFONT COLOR=#FFFFFF><B>Vitalidade</B><BR>Afeta ...</BASEFONT>";

        public static string Attr_Man =>
        @"<BASEFONT COLOR=#FFFFFF><B>Mana</B><BR>Afeta ...</BASEFONT>";

        public static string GetAttrHtml(OSUAttributeTopic t)
        {
            switch (t)
            {
                default:
                case OSUAttributeTopic.Str: return Attr_Str;
                case OSUAttributeTopic.Dex: return Attr_Dex;
                case OSUAttributeTopic.Int: return Attr_Int;
                case OSUAttributeTopic.Man: return Attr_HP;
                case OSUAttributeTopic.Vit: return Attr_Vit;
                case OSUAttributeTopic.Cha: return Attr_Man;
            }
        }

        public static string GetPage1Html(OSUCreationInfoTopic topic)
        {
            switch (topic)
            {
                default:
                case OSUCreationInfoTopic.OSU: return Page1_OSU;
                case OSUCreationInfoTopic.LoreAmanti: return Page1_LoreAmanti;
                case OSUCreationInfoTopic.Regras: return Page1_Regras;
            }
        }
    }
}
