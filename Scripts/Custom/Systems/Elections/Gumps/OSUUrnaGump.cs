using System;
using System.Text;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class OSUUrnaGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly UrnaDoReino _urna;
        private readonly int _cityId;

        private const int ButtonCandidate = 1000;
        private const int ButtonVoteBase = 2000;

        public OSUUrnaGump(PlayerMobile from, UrnaDoReino urna) : base(0, 0)
        {
            _from = from;
            _urna = urna;
            _cityId = urna.CityId;

            bool mandatoryKamayVote = IsMandatoryKamayVotePending();

            Closable = !mandatoryKamayVote;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            AddImageTiled(356, 200, 379, 406, 377);
            AddImageTiled(333, 542, 78, 89, 359);
            AddImageTiled(685, 542, 74, 90, 360);
            AddImageTiled(685, 182, 74, 82, 361);
            AddImageTiled(333, 182, 74, 90, 362);
            AddImageTiled(337, 270, 26, 276, 365);
            AddImageTiled(730, 259, 26, 289, 366);
            AddImageTiled(402, 601, 289, 31, 367);
            AddImageTiled(399, 184, 289, 31, 368);
            AddImageTiled(540, 248, 181, 21, 466);
            AddImageTiled(367, 248, 189, 21, 465);

            AddLabel(513, 226, 1152, "Eleições");

            DrawBody();
        }

        private void DrawBody()
        {
            ReinoFase phase = ReinoElectionsSystem.GetCurrentPhase();

            if (phase == ReinoFase.Candidatura)
            {
                DrawCandidatePage();
                return;
            }

            if (phase == ReinoFase.Votacao)
            {
                DrawVotePage();
                return;
            }

            DrawClosedPage();
        }

        private void DrawCandidatePage()
        {
            ReinoCityData city = ReinoElectionsSystem.GetCityData(_cityId);
            bool alreadyCandidate = IsAlreadyCandidate();

            AddLabel(500, 275, 1152, "Candidatura");

            AddHtml(389, 315, 318, 175, BuildCandidateHtml(city), false, true);

            string reason;
            bool canRegister = ReinoElectionsSystem.CanRegisterCandidate(_from, _cityId, out reason);

            if (alreadyCandidate)
            {
                AddButton(396, 524, 520, 520, ButtonCandidate, GumpButtonType.Reply, 0);
                AddLabel(433, 529, 1152, "Cancelar Candidatura");
                return;
            }

            if (canRegister)
            {
                AddButton(396, 524, 520, 520, ButtonCandidate, GumpButtonType.Reply, 0);
                AddLabel(433, 529, 1152, "Se Candidatar");
            }
            else
            {
                AddImageTiled(396, 524, 26, 26, 2624);
                AddLabel(445, 532, 1152, "Se Candidatar");
                AddHtml(389, 500, 318, 40, Colorize(reason, "#FFFFFF"), false, false);
            }
        }

        private void DrawVotePage()
        {
            ReinoCityData city = ReinoElectionsSystem.GetCityData(_cityId);

            AddLabel(522, 275, 1152, "Votação");

            AddHtml(389, 315, 318, 120, BuildVoteInfoHtml(city), false, true);

            string reason;
            bool canAnyVote = CanVoteInThisCity(out reason);

            if (!canAnyVote)
            {
             //   AddHtml(389, 455, 318, 95, Colorize(reason, "#FFFFFF"), false, true);
                return;
            }

            int shown = 0;

            for (int i = 0; i < city.Candidates.Count && i < 10; i++)
            {
                ReinoCandidateEntry entry = city.Candidates[i];

                int col = (i < 5) ? 0 : 1;
                int row = (i % 5);

                int x = (col == 0) ? 397 : 557;
                int y = 450 + (row * 28);
                int labelX = x + 28;

                AddButton(x, y, 537, 537, ButtonVoteBase + i, GumpButtonType.Reply, 0);
                AddLabel(labelX, y - 2, 1152, entry.Nome);
                shown++;
            }

         //   if (shown == 0)
          //      AddHtml(389, 455, 318, 80, Colorize("Não há candidatos nesta cidade nesta eleição.", "#FFFFFF"), false, false);
        }

        private void DrawClosedPage()
        {
            AddLabel(498, 275, 1152, "Urna Fechada");

            StringBuilder sb = new StringBuilder();

            sb.Append("<BASEFONT COLOR=#FFFFFF>");
            sb.Append("A urna não está recebendo candidaturas nem votos neste momento.<br><br>");
            sb.Append("As candidaturas ocorrem entre os dias 1 e 4 de cada mês eleitoral.<br>");
            sb.Append("A votação acontece entre os dias 5 e 20.<br><br>");
            sb.Append("Os resultados são anunciados separadamente e os mandatos duram 1 ano. (2 meses em tempo real)");
            sb.Append("</BASEFONT>");

            AddHtml(389, 315, 318, 180, sb.ToString(), false, true);
        }

        private bool IsAlreadyCandidate()
        {
            ReinoCityData city = ReinoElectionsSystem.GetCityData(_cityId);

            if (city == null)
                return false;

            for (int i = 0; i < city.Candidates.Count; i++)
            {
                if (city.Candidates[i].Serial == _from.Serial.Value)
                    return true;
            }

            return false;
        }

        private bool IsMandatoryKamayVotePending()
        {
            if (_from == null || _from.Deleted)
                return false;

            if (ReinoElectionsSystem.GetCurrentPhase() != ReinoFase.Votacao)
                return false;

            if (!String.Equals(_from.OSUCultureId, "kamay", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!_from.IsCitizenOf(ReinoElectionsSystem.GetCityName(_cityId)))
                return false;

            if (ReinoElectionsSystem.HasPlayerVoted(_from))
                return false;

            return true;
        }

        private bool CanVoteInThisCity(out string reason)
        {
            reason = null;

            ReinoCityData city = ReinoElectionsSystem.GetCityData(_cityId);

            if (city == null)
            {
                reason = "Cidade inválida.";
                return false;
            }

            if (city.Candidates.Count <= 0)
            {
               // reason = "Ainda não existem candidatos nesta cidade.";
                return false;
            }

            if (ReinoElectionsSystem.HasPlayerVoted(_from))
            {
                reason = "Seu voto já foi computado nesta eleição.";
                return false;
            }

            ReinoCandidateEntry first = city.Candidates[0];
            return ReinoElectionsSystem.CanVote(_from, _cityId, first.Serial, out reason);
        }

        private string BuildCandidateHtml(ReinoCityData city)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<BASEFONT COLOR=#FFFFFF>");



            switch (_cityId)
            {
                case 0: // Aurora / Kamay
                    sb.Append("Em Aurora, os cidadãos elegem o Primeiro Ministro. O governo kamay funciona por ministérios: além do Primeiro Ministro, três ministros participam da administração e toda decisão relevante deve passar por ao menos um deles.<br><br>");
                    break;

                case 1: // Xetá / Matalun
                    sb.Append("Em Xetá, os cidadãos elegem a Sacerdotiza que guiará a cidade. A liderança matalun é espiritual e política ao mesmo tempo, e somente personagens femininas podem assumir este posto.<br><br>");
                    break;

                case 2: // Lurone / Sarangs
                    sb.Append("Em Lurone, os cidadãos elegem democraticamente o Líder Soberano. O soberano governa em nome do povo sarang durante todo o mandato e conduz as decisões da cidade até a próxima eleição.<br><br>");
                    break;

                case 3: // Willran / Zosteros
                    sb.Append("Em Willran, os cidadãos elegem o Chefe do Conselho de Anciões. O mais votado torna-se o Chefe do Conselho, mas as decisões do governo só se confirmam quando aprovadas pelos demais membros do conselho.<br><br>");
                    break;

                default:
                    sb.Append("Cada cidade elege sua própria forma de governo.<br><br>");
                    break;
            }

            sb.Append("As eleições acontecem em meses alternados do calendário real do servidor. As candidaturas ficam abertas entre os dias 1 e 4, a votação acontece entre os dias 5 e 20, e o mandato dura 1 ano (2 meses de tempo real).<br><br>");
            sb.Append("Somente cidadãos da cidade e membros do nativos de Lurone porem se candidatar.");

            if (city != null && city.Candidates.Count > 0)
            {
                sb.Append("<br><br>Candidatos já inscritos:<br>");
                for (int i = 0; i < city.Candidates.Count; i++)
                {
                    sb.Append("- ");
                    sb.Append(city.Candidates[i].Nome);
                    sb.Append("<br>");
                }
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        private string BuildVoteInfoHtml(ReinoCityData city)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<BASEFONT COLOR=#FFFFFF>");

            switch (_cityId)
            {
                case 0:
                    sb.Append("Aurora elege o Primeiro Ministro, que governa com o apoio de três ministros. Todo morador cidadão da cidade tem direito a voto. Cada cidadão possui apenas 1 voto por eleição e não pode alterá-lo depois de confirmado.<br><br>");
                    sb.Append("Para os Kamay, o voto é obrigatório. Não votar até o fim do período de votação resultará em multa de 2000 moedas de ouro retirada do banco.");
                    break;

                case 1:
                    sb.Append("Xetá elege sua Sacerdotiza. Todo morador cidadão da cidade tem direito a voto. Cada cidadão possui apenas 1 voto por eleição e, depois de votar, não pode mudar sua escolha.");
                    break;

                case 2:
                    sb.Append("Lurone elege democraticamente seu Líder Soberano. Todo morador cidadão da cidade tem direito a voto. Cada cidadão possui apenas 1 voto por eleição e não pode mudar de voto depois da confirmação.");
                    break;

                case 3:
                    sb.Append("Willran escolhe o Chefe do Conselho de Anciões, e o mais votado torna-se o Chefe do Conselho. Todo morador cidadão da cidade tem direito a voto. Cada cidadão possui apenas 1 voto por eleição e a escolha não pode ser alterada depois.");
                    break;

                default:
                    sb.Append("Todo cidadão da cidade tem direito a um único voto por eleição, e não é possível mudar o voto depois de confirmado.");
                    break;
            }

         //   if (city == null || city.Candidates.Count == 0)
          //      sb.Append("<br><br>Ainda não há candidatos cadastrados.");

            sb.Append("</BASEFONT>");

            return sb.ToString();
        }

        private string Colorize(string text, string color)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            return String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", color, text);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_from == null || _from.Deleted || _urna == null || _urna.Deleted)
                return;

            if (!_from.InRange(_urna.GetWorldLocation(), 2))
            {
                _from.SendMessage("Você está longe demais da urna.");
                return;
            }

            int button = info.ButtonID;

            if (button == 0)
            {
                if (IsMandatoryKamayVotePending())
                {
                    _from.SendMessage("O voto dos cidadãos kamay é obrigatório. Escolha um candidato para registrar seu voto.");
                    _from.CloseGump(typeof(OSUUrnaGump));
                    _from.SendGump(new OSUUrnaGump(_from, _urna));
                }

                return;
            }

            if (button == ButtonCandidate)
            {
                if (IsAlreadyCandidate())
                {
                    string cancelReason;

                    if (ReinoElectionsSystem.CancelCandidate(_from, _cityId, out cancelReason))
                        _from.SendMessage("Sua candidatura em {0} foi cancelada.", ReinoElectionsSystem.GetCityName(_cityId));
                    else
                        _from.SendMessage(cancelReason);
                }
                else
                {
                    string reason;

                    if (ReinoElectionsSystem.RegisterCandidate(_from, _cityId, out reason))
                        _from.SendMessage("Seu nome foi registrado na eleição de {0}.", ReinoElectionsSystem.GetCityName(_cityId));
                    else
                        _from.SendMessage(reason);
                }

                _from.CloseGump(typeof(OSUUrnaGump));
                _from.SendGump(new OSUUrnaGump(_from, _urna));
                return;
            }

            if (button >= ButtonVoteBase && button < ButtonVoteBase + 10)
            {
                ReinoCityData city = ReinoElectionsSystem.GetCityData(_cityId);

                if (city == null)
                    return;

                int index = button - ButtonVoteBase;

                if (index < 0 || index >= city.Candidates.Count)
                {
                    _from.SendMessage("Candidato inválido.");
                    return;
                }

                ReinoCandidateEntry entry = city.Candidates[index];
                string reason;

                if (ReinoElectionsSystem.RegisterVote(_from, _cityId, entry.Serial, out reason))
                {
                    _from.SendMessage("Seu voto em {0} foi registrado.", entry.Nome);
                    _from.CloseGump(typeof(OSUUrnaGump));
                }
                else
                {
                    _from.SendMessage(reason);
                    _from.CloseGump(typeof(OSUUrnaGump));
                    _from.SendGump(new OSUUrnaGump(_from, _urna));
                }
            }
        }
    }
}
