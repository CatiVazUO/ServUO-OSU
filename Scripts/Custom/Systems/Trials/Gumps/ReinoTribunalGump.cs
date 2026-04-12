using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoTribunalGump : Gump
    {
        private readonly PlayerMobile m_From;
        private readonly int m_CityId;

        private const int ButtonStartSession = 1;
        private const int ButtonBangHammer = 2;
        private const int ButtonExpel = 3;
        private const int ButtonContempt = 4;
        private const int ButtonEndSession = 5;
        private const int ButtonSetSentence = 6;
        private const int ButtonSetFine = 7;

        public ReinoTribunalGump(PlayerMobile from, int cityId) : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReinoTrialSession st = ReinoTrialsSystem.GetSession(from, cityId);
            string acusado = String.IsNullOrWhiteSpace(st.AccusedName) ? "nenhum" : st.AccusedName;

            AddPage(0);
            AddImageTiled(337, 166, 274, 406, 375);
            AddImageTiled(317, 500, 78, 89, 359);
            AddImageTiled(557, 500, 74, 90, 360);
            AddImageTiled(557, 137, 74, 82, 361);
            AddImageTiled(317, 137, 74, 90, 362);
            AddImageTiled(321, 217, 26, 294, 365);
            AddImageTiled(602, 214, 26, 298, 366);
            AddImageTiled(386, 559, 174, 31, 367);
            AddImageTiled(383, 139, 201, 31, 368);
            AddImageTiled(395, 202, 180, 21, 470);
            AddImageTiled(366, 202, 178, 21, 469);
            AddLabel(440, 179, 1152, @"Tribunal");
            AddLabel(432, 226, 1152, @"Réu atual: " + acusado);
            AddLabel(384, 266, 1152, st.SessionActive ? @"Sessão: aberta" : @"Sessão: fechada");
            AddLabel(413, 295, 1152, @"Bater Martelo");
            AddButton(385, 295, 530, 248, ButtonBangHammer, GumpButtonType.Reply, 0);
            AddLabel(413, 325, 1152, @"Expulsar da Corte");
            AddButton(385, 325, 530, 248, ButtonExpel, GumpButtonType.Reply, 0);
            AddLabel(413, 355, 1152, @"Prender por Desacato");
            AddButton(385, 355, 530, 248, ButtonContempt, GumpButtonType.Reply, 0);
            AddLabel(384, 443, 1152, @"Decretar Pena em Dias");
            AddButton(541, 469, 530, 248, ButtonSetSentence, GumpButtonType.Reply, 0);
            AddTextEntry(384, 468, 137, 20, 1152, 1, st.PendingSentenceDays > 0 ? st.PendingSentenceDays.ToString() : String.Empty);
            AddImageTiled(357, 420, 233, 5, 368);
            AddLabel(384, 497, 1152, @"Decretar Multa");
            AddButton(542, 524, 530, 248, ButtonSetFine, GumpButtonType.Reply, 0);
            AddTextEntry(385, 523, 137, 20, 1152, 2, st.PendingFineGold > 0 ? st.PendingFineGold.ToString() : String.Empty);
            AddLabel(413, 266, 1152, @"Iniciar Sessão");
            AddButton(385, 266, 530, 248, ButtonStartSession, GumpButtonType.Reply, 0);
            AddLabel(413, 385, 1152, @"Encerrar Sessão");
            AddButton(385, 385, 530, 248, ButtonEndSession, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (!ReinoTrialsSystem.CanAccessTribunalControl(from, m_CityId))
                return;

            switch (info.ButtonID)
            {
                case ButtonStartSession:
                    from.Target = new ReinoTribunalStartSessionTarget(m_CityId);
                    from.SendMessage("Selecione o jogador que será julgado.");
                    break;
                case ButtonBangHammer:
                    ReinoTrialsSystem.BangHammer(from, m_CityId);
                    from.SendGump(new ReinoTribunalGump(from, m_CityId));
                    break;
                case ButtonExpel:
                    from.Target = new ReinoTribunalExpelTarget(m_CityId);
                    from.SendMessage("Selecione quem deve ser expulso da corte.");
                    break;
                case ButtonContempt:
                    from.Target = new ReinoTribunalContemptTarget(m_CityId);
                    from.SendMessage("Selecione quem será preso por desacato.");
                    break;
                case ButtonEndSession:
                    ReinoTrialsSystem.EndSession(from, m_CityId);
                    from.SendGump(new ReinoTribunalGump(from, m_CityId));
                    break;
                case ButtonSetSentence:
                    {
                        TextRelay tr = info.GetTextEntry(1);
                        int days;
                        if (tr == null || !Int32.TryParse((tr.Text ?? String.Empty).Trim(), out days) || days <= 0)
                            from.SendMessage("Digite uma quantidade válida de dias.");
                        else
                            from.SendMessage(ReinoTrialsSystem.SetPendingSentence(from, m_CityId, days));

                        from.SendGump(new ReinoTribunalGump(from, m_CityId));
                        break;
                    }
                case ButtonSetFine:
                    {
                        TextRelay tr = info.GetTextEntry(2);
                        int gold;
                        if (tr == null || !Int32.TryParse((tr.Text ?? String.Empty).Trim(), out gold) || gold < 0)
                            from.SendMessage("Digite um valor válido de multa.");
                        else
                            from.SendMessage(ReinoTrialsSystem.SetPendingFine(from, m_CityId, gold));

                        from.SendGump(new ReinoTribunalGump(from, m_CityId));
                        break;
                    }
            }
        }
    }
}
