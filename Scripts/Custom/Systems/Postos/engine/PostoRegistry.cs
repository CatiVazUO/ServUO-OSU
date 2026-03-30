using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.Postos
{
    public static class PostoRegistry
    {
        private static readonly Dictionary<string, PostoDefinition> m_ById = new Dictionary<string, PostoDefinition>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<PostoDefinition> m_All = new List<PostoDefinition>();

        public static IList<PostoDefinition> All
        {
            get { return m_All.AsReadOnly(); }
        }

        static PostoRegistry()
        {
            Register(new PostoDefinition(
                "aramute",
                "Aramute",
                "mineiro",
                PostoSize.Small,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "morcegos cavernícolas",
                new string[] { "MorcegoCavernicolo" },
                200,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Esse é o posto Aramute. Quando abrimos a nova galeria, demos de cara com um ninho inteiro de morcegos cavernícolas. Eles rasgam sacos, assustam as mulas e espalham poeira por tudo. Se seu reino nos ajudar a reduzir a infestação, entregaremos parte do ferro que sair daqui todos os dias."));

            Register(new PostoDefinition(
                "dorvok",
                "Dorvok",
                "mineiro",
                PostoSize.Small,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "ratos saqueadores",
                new string[] { "RatoSaqueador" },
                180,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Em Dorvok o problema não é a pedra, é quem vive dentro dela. Bandos de ratos saqueadores estão roubando ferramentas, comida e até as lanternas dos poceiros. Dê-nos alguns dias de paz e faremos um acordo honesto: proteção em troca de ferro despachado ao seu reino."));

            Register(new PostoDefinition(
                "selgard",
                "Selgard",
                "mineiro",
                PostoSize.Small,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "lodos férricos",
                new string[] { "LodoFerrico" },
                210,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>A água velha que escorre por Selgard criou poças grossas e vivas. Esses lodos férricos grudam nas botas, estragam veios rasos e adoecem quem trabalha perto demais. Se sua gente limpar os túneis, reservaremos uma parte do ferro produzido aqui para os seus despachantes."));

            Register(new PostoDefinition(
                "karstun",
                "Karstun",
                "mineiro",
                PostoSize.Small,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "xistos vivos",
                new string[] { "XistoVivo" },
                160,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Karstun era uma escavação tranquila até os veios mais fundos começarem a despertar a própria rocha. Agora xistos vivos destroem escoras e fecham passagens durante a madrugada. Se seu reino segurar essa ameaça, podemos pagar a ajuda com remessas diárias de ferro."));

            Register(new PostoDefinition(
                "vhalor",
                "Vhalor",
                "mineiro",
                PostoSize.Large,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "gárgulas da pedreira",
                new string[] { "GargulaPedreira" },
                480,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Vhalor é uma pedreira grande demais para ficar parada. O problema é que as gárgulas da pedreira começaram a pousar nas bordas do corte e atacar qualquer equipe que tente trabalhar as bancadas mais ricas. Se um reino conseguir manter essas criaturas sob controle, damos metade da produção pactuada em forma de ferro diário."));

            Register(new PostoDefinition(
                "nargesh",
                "Nargesh",
                "mineiro",
                PostoSize.Large,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "trolls das galerias",
                new string[] { "TrollGaleria" },
                420,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Nas galerias largas de Nargesh, trolls das galerias passam a descer das fendas à noite e quebrar tudo o que encontram: vagonetes, escoras e braços de mineiro. Precisamos de proteção constante, não de uma visita corajosa. Cumpram a limpeza e terão direito a uma grande remessa diária de ferro."));

            Register(new PostoDefinition(
                "tirak",
                "Tirak",
                "mineiro",
                PostoSize.Large,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "imps carvoeiros",
                new string[] { "ImpCarvoeiro" },
                500,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Tirak produz muito, mas também atrai coisa ruim. Imps carvoeiros entram pelas saídas de ar e tocam fogo em cordas, barris de óleo e depósitos de carvão. Se o seu reino provar que consegue manter a mina segura, nós topamos repartir uma fração bem maior do ferro extraído aqui."));

            Register(new PostoDefinition(
                "thorma",
                "Thorma",
                "mineiro",
                PostoSize.Large,
                PostoResourceType.Iron,
                PostoObjectiveType.KillMob,
                "besouros de magma",
                new string[] { "BesouroMagma" },
                450,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Thorma se expandiu demais e acabou furando bolsões quentes que deviam ter permanecido selados. Desde então, besouros de magma infestam os níveis baixos e queimam todo mundo que tenta minerar perto dos veios novos. Tire essa praga do nosso caminho e faremos um acordo lucrativo para o seu reino."));

            Register(new PostoDefinition(
                "cunhau",
                "Cunhau",
                "lenhador",
                PostoSize.Small,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "lobos do pinhal",
                new string[] { "LoboDoPinhal" },
                190,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>O posto Cunhau fica numa mata boa de corte, mas os lobos do pinhal já perderam o medo do machado. Eles rondam os acampamentos, atacam carroças e espantam os lenhadores antes do amanhecer. Se seu reino abrir passagem e mantiver a trilha segura, pagaremos em madeira despachada todos os dias."));

            Register(new PostoDefinition(
                "belorim",
                "Belorim",
                "lenhador",
                PostoSize.Small,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "hárpias do pinhal",
                new string[] { "HarpiaDoPinhal" },
                200,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>As hárpias do pinhal de Belorim fizeram ninho nos pinheiros mais altos e agora mergulham sobre qualquer equipe que tente derrubar as árvores maduras. Ninguém trabalha olhando para o céu o dia inteiro. Se sua gente limpar o alto do bosque, nós entregamos parte da madeira serrada ao reino que nos ajudar."));

            Register(new PostoDefinition(
                "valesca",
                "Valesca",
                "lenhador",
                PostoSize.Small,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "aranhas do emaranhado",
                new string[] { "AranhaEmaranhada" },
                175,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>A mata de Valesca está tomada por teias grossas entre um tronco e outro. As aranhas do emaranhado agarram lenhadores, cobrem serras e fazem até as mulas empacarem. Precisamos de gente disposta a abrir o emaranhado e caçar a ninhada. Em troca, reservamos madeira diária para o reino protetor."));

            Register(new PostoDefinition(
                "norvind",
                "Norvind",
                "lenhador",
                PostoSize.Small,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "ettins madeireiros",
                new string[] { "EttinMadeireiro" },
                160,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Em Norvind, ettins madeireiros descem do morro e roubam troncos já cortados antes que a madeira chegue à serraria. Perdemos dias de trabalho em uma única noite ruim. Se vocês segurarem esses brutamontes, terão um acordo simples e constante: defesa por madeira."));

            Register(new PostoDefinition(
                "talbrasa",
                "Talbrasa",
                "lenhador",
                PostoSize.Large,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "reapers talhadores",
                new string[] { "ReaperTalhador" },
                430,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Talbrasa é um grande posto florestal, e justamente por isso os reapers talhadores da área ficaram violentos. Eles arrancam estacas, derrubam torres de vigia e esmagam os pátios onde a madeira seca. Se um reino conseguir manter esse bosque sob controle, daremos uma parte muito maior da produção em madeira diária."));

            Register(new PostoDefinition(
                "rivenoak",
                "Rivenoak",
                "lenhador",
                PostoSize.Large,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "corpsers trilheiros",
                new string[] { "CorpserTrilheiro" },
                470,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>As trilhas de Rivenoak foram engolidas por corpsers trilheiros. As raízes vivas prendem carroças, viram homens do avesso e deixam a serraria isolada por dias. Se o seu reino cumprir a limpeza e garantir passagem, nós aceitamos repartir uma grande quota da madeira produzida aqui."));

            Register(new PostoDefinition(
                "galdrin",
                "Galdrin",
                "lenhador",
                PostoSize.Large,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "centauros hostis",
                new string[] { "CentauroHostil" },
                520,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Galdrin corta árvores antigas perto de rotas de caça, e os centauros hostis da região decidiram que isso não vai continuar sem sangue. Todo carregamento sai escoltado e mesmo assim quase sempre se perde alguém. Se vocês contiverem esses ataques, pagaremos o acordo com grandes remessas de madeira."));

            Register(new PostoDefinition(
                "ulmora",
                "Ulmora",
                "lenhador",
                PostoSize.Large,
                PostoResourceType.Wood,
                PostoObjectiveType.KillMob,
                "lobos da névoa",
                new string[] { "LoboDaNevoa" },
                500,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Em Ulmora, os ataques vêm na névoa, sempre antes do sol. Há quem diga que são só lobos grandes; quem viu de perto jura que havia coisa pior entre eles. Não me importa o nome da fera, só quero a mata de volta. Protejam o posto e repartiremos a produção de madeira com o seu reino."));

            Register(new PostoDefinition(
                "saial",
                "Saial",
                "fazendeiro",
                PostoSize.Small,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "ratos de tulha",
                new string[] { "RatoDeTulha" },
                180,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Saial produz algodão fino, mas os celeiros vivem tomados por ratos de tulha que rasgam fardos e espalham sementes por toda parte. Já perdemos colheita suficiente para um inverno inteiro. Se seu reino ajudar a conter a praga, podemos pagar com remessas diárias de algodão."));

            Register(new PostoDefinition(
                "iriande",
                "Iriande",
                "fazendeiro",
                PostoSize.Small,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "lamalinos dos canais",
                new string[] { "LamalinoDosCanais" },
                200,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Os canais de irrigação de Iriande estão cheios de lamalinos dos canais. Eles sujam a água, arrancam mudas novas e apodrecem o campo de dentro para fora. Se o seu povo limpar os valos e mantiver a margem segura, entregaremos parte do algodão produzido aqui ao reino aliado."));

            Register(new PostoDefinition(
                "belsara",
                "Belsara",
                "fazendeiro",
                PostoSize.Small,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "aranhas do algodoeiro",
                new string[] { "AranhaDoAlgodoeiro" },
                170,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>As aranhas de Belsara aprenderam que os pés de algodão são ótimos para esconder ovos. Quando chega a colheita, metade do campo já virou ninho. Se sua gente varrer essas pestes dos canteiros, firmamos um acordo estável de algodão diário."));

            Register(new PostoDefinition(
                "rosamar",
                "Rosamar",
                "fazendeiro",
                PostoSize.Small,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "vultos da cerca",
                new string[] { "VultoDaCerca" },
                160,
                100,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Rosamar tem terra boa e gente pouca. O problema é que grupos de vultos da cerca começaram a cruzar os campos ao entardecer, pisando plantio, quebrando cercas e levando pânico aos trabalhadores. Se vocês protegerem nossas lavouras, reservamos algodão para o seu reino todos os dias."));

            Register(new PostoDefinition(
                "dalvila",
                "Dalvila",
                "fazendeiro",
                PostoSize.Large,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "escorpiões do baixio",
                new string[] { "EscorpiaoDoBaixio" },
                420,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Dalvila é um baixio enorme de algodão, mas os escorpiões do baixio tomaram as faixas mais férteis. Toda semana alguém cai envenenado entre um sulco e outro. Se o seu reino quiser nosso acordo, terá de provar que consegue manter este campo inteiro respirando. Em troca, despachamos uma grande cota diária de algodão."));

            Register(new PostoDefinition(
                "orquessa",
                "Orquessa",
                "fazendeiro",
                PostoSize.Large,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "lagartos do vau",
                new string[] { "LagartoDoVau" },
                500,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>As plantações de Orquessa ficam perto de água parada, e os lagartos do vau descobriram isso. Eles roubam fardos, atacam batedores e quebram as rodas dos moinhos quando ninguém está olhando. Se vocês segurarem esses ataques por tempo suficiente, aceitamos repartir metade pactuada da produção em algodão."));

            Register(new PostoDefinition(
                "ventalva",
                "Ventalva",
                "fazendeiro",
                PostoSize.Large,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "hárpias do moinho",
                new string[] { "HarpiaDoMoinho" },
                460,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Ventalva depende do vento para secar o algodão, mas as hárpias do moinho se apoderaram das pás e fazem chover penas, pedras e ossos sobre os secadouros. Não basta expulsar uma ou duas; precisamos de paz duradoura. Se o seu reino conseguir isso, pagaremos generosamente em algodão."));

            Register(new PostoDefinition(
                "lumera",
                "Lumera",
                "fazendeiro",
                PostoSize.Large,
                PostoResourceType.Cotton,
                PostoObjectiveType.KillMob,
                "lodos do alagado",
                new string[] { "LodoDoAlagado" },
                480,
                300,
                TimeSpan.FromDays(4.0),
                "<BASEFONT COLOR=#000000>Lumera produz muito porque planta onde quase ninguém se arrisca. Só que o alagado começou a cuspir lodos do alagado que invadem os canteiros, dissolvem raízes e transformam o chão num lodo imprestável. Limpe esse lugar para nós e o seu reino receberá grandes despachos diários de algodão."));
        }

        private static void Register(PostoDefinition def)
        {
            if (def == null || String.IsNullOrWhiteSpace(def.Id))
                return;

            if (m_ById.ContainsKey(def.Id))
                return;

            m_ById[def.Id] = def;
            m_All.Add(def);
        }

        public static PostoDefinition Get(string postoId)
        {
            if (String.IsNullOrWhiteSpace(postoId))
                return null;

            PostoDefinition def;
            m_ById.TryGetValue(postoId.Trim(), out def);
            return def;
        }
    }
}
