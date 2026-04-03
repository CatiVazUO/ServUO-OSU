using System.Text;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Systems.Postos
{
    public class PostoContestAlertGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly string _postoName;
        private readonly string _challengerKingdom;
        private readonly string _defenderKingdom;

        public PostoContestAlertGump(PlayerMobile from, string postoName, string challengerKingdom, string defenderKingdom) : base(0, 0)
        {
            _from = from;
            _postoName = postoName ?? string.Empty;
            _challengerKingdom = challengerKingdom ?? string.Empty;
            _defenderKingdom = defenderKingdom ?? string.Empty;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(254, 239, 3557);
            AddLabel(426, 315, 0, "Aviso de Disputa de Posto");
            AddHtml(363, 344, 377, 157, BuildHtml(), false, false);
        }

        private string BuildHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("O posto <b>");
            sb.Append(_postoName);
            sb.Append("</b>, atualmente vinculado ao reino de <b>");
            sb.Append(_defenderKingdom);
            sb.Append("</b>, entrou em disputa com o reino de <b>");
            sb.Append(_challengerKingdom);
            sb.Append("</b>.<br><br>");
            sb.Append("Pelos próximos 3 dias, o posto continuará produzindo para o reino defensor. Ao fim do prazo, permanecerá com o reino que mais contiver a ameaça local.<br><br>");
            sb.Append("Se julgar necessário, convoque seus cidadãos para defender o posto antes que o acordo mude de mãos.");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }
}
