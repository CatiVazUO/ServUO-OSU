using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public class ReinoTribunalLawConfigGump : Gump
    {
        private readonly PlayerMobile m_From;
        private readonly int m_CityId;
        private readonly ReinoMilitaryLaw m_Law;

        private const int ButtonSetSentence = 1;
        private const int ButtonSetFine = 2;
        private const int ButtonToggleNobles = 3;

        public ReinoTribunalLawConfigGump(PlayerMobile from, int cityId, ReinoMilitaryLaw law) : base(0, 0)
        {
            m_From = from;
            m_CityId = cityId;
            m_Law = law;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            bool nobles = ReinoTrialsSystem.DoesLawApplyToNobles(cityId, law);

            AddPage(0);
            AddImageTiled(344, 156, 318, 378, 387);
            AddImageTiled(317, 471, 78, 89, 359);
            AddImageTiled(608, 471, 74, 90, 360);
            AddImageTiled(608, 137, 74, 82, 361);
            AddImageTiled(317, 137, 74, 90, 362);
            AddImageTiled(321, 217, 26, 262, 365);
            AddImageTiled(653, 214, 26, 260, 366);
            AddImageTiled(386, 530, 222, 31, 367);
            AddImageTiled(383, 139, 230, 31, 368);
            AddImageTiled(449, 202, 180, 21, 470);
            AddImageTiled(366, 202, 178, 21, 469);
            AddLabel(454, 179, 0, ReinoMilitarySystem.GetLawLabel(law));
            AddLabel(374, 418, 0, @"Pena");
            AddButton(602, 415, 530, 248, ButtonSetSentence, GumpButtonType.Reply, 0);
            AddHtml(370, 239, 261, 155, ReinoTrialsSystem.GetLawDefinitionHtml(cityId, law), false, false);
            AddTextEntry(434, 413, 154, 20, 0, 1, ReinoTrialsSystem.GetLawDefaultHours(cityId, law).ToString());
            AddLabel(374, 451, 0, @"Multa");
            AddButton(602, 448, 530, 248, ButtonSetFine, GumpButtonType.Reply, 0);
            AddTextEntry(434, 446, 154, 20, 0, 2, ReinoTrialsSystem.GetLawDefaultFine(cityId, law).ToString());
            AddButton(498, 484, nobles ? 530 : 531, nobles ? 530 : 531, ButtonToggleNobles, GumpButtonType.Reply, 0);
            AddLabel(374, 483, 0, @"Vale para Nobres:");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            if (!ReinoTrialsSystem.CanAccessLawSettings(from, m_CityId))
                return;

            switch (info.ButtonID)
            {
                case ButtonSetSentence:
                    {
                        TextRelay tr = info.GetTextEntry(1);
                        int hours;

                        if (tr == null || !Int32.TryParse((tr.Text ?? String.Empty).Trim(), out hours) || hours <= 0)
                            from.SendMessage("Digite uma quantidade válida de horas.");
                        else
                            from.SendMessage(ReinoTrialsSystem.SetLawDefaultHours(from, m_CityId, m_Law, hours));

                        from.SendGump(new ReinoTribunalLawConfigGump(from, m_CityId, m_Law));
                        break;
                    }

                case ButtonSetFine:
                    {
                        TextRelay tr = info.GetTextEntry(2);
                        int fine;

                        if (tr == null || !Int32.TryParse((tr.Text ?? String.Empty).Trim(), out fine) || fine < 0)
                            from.SendMessage("Digite um valor válido de multa.");
                        else
                            from.SendMessage(ReinoTrialsSystem.SetLawDefaultFine(from, m_CityId, m_Law, fine));

                        from.SendGump(new ReinoTribunalLawConfigGump(from, m_CityId, m_Law));
                        break;
                    }

                case ButtonToggleNobles:
                    {
                        bool current = ReinoTrialsSystem.DoesLawApplyToNobles(m_CityId, m_Law);
                        from.SendMessage(ReinoTrialsSystem.SetLawAppliesToNobles(from, m_CityId, m_Law, !current));
                        from.SendGump(new ReinoTribunalLawConfigGump(from, m_CityId, m_Law));
                        break;
                    }
            }
        }
    }
}
