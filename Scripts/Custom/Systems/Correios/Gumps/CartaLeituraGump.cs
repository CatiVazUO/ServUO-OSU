using Server.Gumps;
using Server.Network;

namespace Server.Custom.Correios
{
    public class CartaLeituraGump : Gump
    {
        private readonly string _texto;

        public CartaLeituraGump(string texto) : base(50, 50)
        {
            _texto = texto ?? "";

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Pergaminho (você pediu "igual ao do pergaminho editável")
            // Aqui estou usando a imagem 1250 como base. Depois você troca se quiser.
            AddImage(0, 0, 1250);

            // Área do texto em cima do pergaminho
            // (ajuste de X/Y/W/H se quiser encaixar melhor na sua arte)
            AddHtml(25, 25, 370, 240, _texto, true, true);

            // Botão fechar (opcional)
            AddButton(360, 270, 4017, 4019, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            // nada (fechar)
        }
    }
}
