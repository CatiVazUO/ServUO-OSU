using System;
using System.Text;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class OSUResultadoEleicaoGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly int _cityId;
        private readonly string _winnerName;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        public OSUResultadoEleicaoGump(PlayerMobile from, int cityId, string winnerName, DateTime startDate, DateTime endDate) : base(0, 0)
        {
            _from = from;
            _cityId = cityId;
            _winnerName = winnerName;
            _startDate = startDate;
            _endDate = endDate;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            AddImage(254, 239, 3557);
            AddLabel(454, 315, 0, "Resultado das Eleições de " + ReinoElectionsSystem.GetCityName(_cityId));
            AddHtml(363, 344, 377, 157, BuildHtml(), false, false);
        }

        private string BuildHtml()
        {
            StringBuilder sb = new StringBuilder();

         /*   sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("As eleições de ");
            sb.Append(ReinoElectionsSystem.GetCityName(_cityId));
            sb.Append(" foram encerradas.<br><br>");*/

            sb.Append("O novo ");
            sb.Append(GetOfficeTitle());
            sb.Append(" de ");
            sb.Append(ReinoElectionsSystem.GetCityName(_cityId));
            sb.Append(" é <b>");
            sb.Append(_winnerName);
            sb.Append("</b>.<br><br>");
            sb.Append("Este governante receberá as chaves da cidade e passa a responder pelas decisões do reino até o próximo ciclo eleitoral.<br><br>");

            sb.Append("O mandato de 1 ano começa no primeiro dia de primavera (");
            sb.Append(_startDate.ToString("dd/MM"));
            sb.Append(") e termina após o próximo inverno (");
            sb.Append(_endDate.ToString("dd/MM"));
            sb.Append(")<br><br>");

            sb.Append("</BASEFONT>");


            return sb.ToString();
        }

        private string GetOfficeTitle()
        {
            switch (_cityId)
            {
                case 0: return "Primeiro Ministro";
                case 1: return "Sacerdotiza";
                case 2: return "Líder Soberano";
                case 3: return "Chefe do Conselho";
                default: return "governante";
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }
}
