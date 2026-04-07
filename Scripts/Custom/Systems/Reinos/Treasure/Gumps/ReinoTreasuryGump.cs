using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonTreasuryCitizenTax = 59300;
        private const int ButtonTreasuryVendorTax = 59301;
        private const int ButtonTreasurySalaryTax = 59302;
        private const int ButtonTreasuryAuctionTax = 59303;
        private const int ButtonTreasuryReligiousTax = 59304;

        private const int EntryTreasuryCitizenTax = 59400;
        private const int EntryTreasuryVendorTax = 59401;
        private const int EntryTreasurySalaryTax = 59402;
        private const int EntryTreasuryAuctionTax = 59403;
        private const int EntryTreasuryReligiousTax = 59404;

        private void BuildTreasurePage()
        {
            AddPage(1);

            AddLabel(789, 173, 0, @"Tesouro");
            AddLabel(410, 299, 0, @"Percentagem de Vendas de Npcs:");
            AddLabel(410, 336, 0, @"Percentagem de Salários:");
            AddLabel(651, 486, 0, @"Semana Passada");
            AddLabel(410, 233, 0, @"Impostos Semanais:");
            AddImageTiled(407, 271, 396, 5, 367);
            AddTextEntry(544, 231, 193, 20, 0, EntryTreasuryCitizenTax, ReinoTreasurySystem.GetWeeklyCitizenTax(m_CityId).ToString());
            AddTextEntry(624, 295, 143, 20, 0, EntryTreasuryVendorTax, ReinoTreasurySystem.GetVendorSalesTaxPercent(m_CityId).ToString());
            AddTextEntry(583, 334, 184, 20, 0, EntryTreasurySalaryTax, ReinoTreasurySystem.GetSalaryTaxPercent(m_CityId).ToString());
            AddLabel(410, 408, 0, @"Percentagem de Doações Religiosas:");
            AddImageTiled(801, 219, 6, 470, 365);
            AddImageTiled(405, 510, 399, 5, 367);
            AddImageTiled(1037, 224, 6, 465, 365);
            AddLabel(825, 262, 0, @"Construções:");
            AddLabel(826, 289, 0, @"Postos");
            AddLabel(828, 388, 0, @"Doações:");
            AddLabel(893, 218, 0, @"Entrada");
            AddLabel(1126, 220, 0, @"Saída");
            AddLabel(849, 314, 0, @"Tecidos:");
            AddLabel(849, 338, 0, @"Ferro:");
            AddLabel(849, 363, 0, @"Madeira:");
            AddLabel(830, 490, 0, @"Leilões:");
            AddLabel(852, 414, 0, @"Tecidos:");
            AddLabel(852, 438, 0, @"Ferro:");
            AddLabel(852, 463, 0, @"Madeira:");
            AddLabel(1065, 262, 0, @"Manutenção:");
            AddLabel(1065, 364, 0, @"Salários:");
            AddLabel(1090, 288, 0, @"Tecidos:");
            AddLabel(1090, 312, 0, @"Ferro:");
            AddLabel(1090, 337, 0, @"Madeira:");
            AddLabel(1065, 395, 0, @"Guarda:");
            AddLabel(830, 515, 0, @"Diplomacia:");
            AddLabel(1065, 426, 0, @"Diplomacia:");
            AddImageTiled(809, 248, 430, 5, 367);
            AddLabel(1094, 456, 0, @"Tecidos:");
            AddLabel(1094, 480, 0, @"Ferro:");
            AddLabel(1094, 505, 0, @"Madeira:");
            AddLabel(414, 538, 0, @"Moedas:");
            AddLabel(414, 566, 0, @"Tecidos:");
            AddLabel(414, 596, 0, @"Ferro:");
            AddLabel(414, 627, 0, @"Madeira:");
            AddImageTiled(597, 479, 6, 212, 365);
            AddLabel(856, 540, 0, @"Tecidos:");
            AddLabel(856, 564, 0, @"Ferro:");
            AddLabel(856, 589, 0, @"Madeira:");
            AddLabel(474, 486, 0, @"Em Caixa");
            AddLabel(632, 537, 0, @"Moedas:");
            AddLabel(632, 565, 0, @"Tecidos:");
            AddLabel(632, 595, 0, @"Ferro:");
            AddLabel(632, 626, 0, @"Madeira:");
            AddLabel(830, 645, 0, @"Representante:");
            AddLabel(1064, 532, 0, @"Representante:");
            AddLabel(830, 619, 0, @"Vendas de Npcs:");

            AddButton(777, 236, 536, 435, ButtonTreasuryCitizenTax, GumpButtonType.Reply, 0);
            AddButton(777, 300, 536, 435, ButtonTreasuryVendorTax, GumpButtonType.Reply, 0);
            AddButton(777, 338, 536, 435, ButtonTreasurySalaryTax, GumpButtonType.Reply, 0);

            AddLabel(410, 372, 0, @"Percentagem de Ganhos em Leilões:");
            AddTextEntry(642, 369, 125, 20, 0, EntryTreasuryAuctionTax, ReinoTreasurySystem.GetAuctionTaxPercent(m_CityId).ToString());
            AddButton(777, 373, 536, 435, ButtonTreasuryAuctionTax, GumpButtonType.Reply, 0);


            AddTextEntry(649, 406, 119, 20, 0, EntryTreasuryReligiousTax, ReinoTreasurySystem.GetReligiousDonationTaxPercent(m_CityId).ToString());
            AddButton(777, 410, 536, 435, ButtonTreasuryReligiousTax, GumpButtonType.Reply, 0);

            ReinoTreasuryResourceBundle current = ReinoTreasurySystem.GetCombinedTreasuryResources(m_CityId);
            ReinoTreasuryResourceBundle snapshot = ReinoTreasurySystem.GetLastWeekSnapshot(m_CityId);
            ReinoTreasuryWeekRecord week = ReinoTreasurySystem.GetLastClosedWeek(m_CityId);

            AddLabel(510, 538, 0, current.Gold.ToString());
            AddLabel(510, 566, 0, current.Cloth.ToString());
            AddLabel(510, 596, 0, current.Iron.ToString());
            AddLabel(510, 627, 0, current.Wood.ToString());

            AddLabel(728, 537, 0, snapshot.Gold.ToString());
            AddLabel(728, 565, 0, snapshot.Cloth.ToString());
            AddLabel(728, 595, 0, snapshot.Iron.ToString());
            AddLabel(728, 626, 0, snapshot.Wood.ToString());

            AddLabel(950, 262, 0, week.ConstructionIncome.Gold.ToString() + @" moedas");
            AddLabel(950, 314, 0, week.PostoIncome.Cloth.ToString());
            AddLabel(950, 338, 0, week.PostoIncome.Iron.ToString());
            AddLabel(950, 363, 0, week.PostoIncome.Wood.ToString());
            AddLabel(950, 388, 0, week.DonationIncome.Gold.ToString() + @" moedas");
            AddLabel(950, 414, 0, week.DonationIncome.Cloth.ToString());
            AddLabel(950, 438, 0, week.DonationIncome.Iron.ToString());
            AddLabel(950, 463, 0, week.DonationIncome.Wood.ToString());
            AddLabel(950, 490, 0, week.AuctionIncome.Gold.ToString() + @" moedas");
            AddLabel(950, 515, 0, week.DiplomacyIncome.Gold.ToString() + @" moedas");
            AddLabel(950, 540, 0, week.DiplomacyIncome.Cloth.ToString());
            AddLabel(950, 564, 0, week.DiplomacyIncome.Iron.ToString());
            AddLabel(950, 589, 0, week.DiplomacyIncome.Wood.ToString());
            AddLabel(950, 619, 0, week.VendorIncome.Gold.ToString() + @" moedas");
            AddLabel(950, 645, 0, week.RepresentativeIncome.Gold.ToString() + @" moedas");

            AddLabel(1162, 262, 0, week.MaintenanceExpense.Gold.ToString() + @" moedas");
            AddLabel(1162, 288, 0, week.MaintenanceExpense.Cloth.ToString());
            AddLabel(1162, 312, 0, week.MaintenanceExpense.Iron.ToString());
            AddLabel(1162, 337, 0, week.MaintenanceExpense.Wood.ToString());
            AddLabel(1162, 364, 0, week.SalaryExpense.Gold.ToString() + @" moedas");
            AddLabel(1162, 395, 0, week.GuardExpense.Gold.ToString() + @" moedas");
            AddLabel(1162, 426, 0, week.DiplomacyExpense.Gold.ToString() + @" moedas");
            AddLabel(1162, 456, 0, week.DiplomacyExpense.Cloth.ToString());
            AddLabel(1162, 480, 0, week.DiplomacyExpense.Iron.ToString());
            AddLabel(1162, 505, 0, week.DiplomacyExpense.Wood.ToString());
            AddLabel(1162, 532, 0, week.RepresentativeExpense.Gold.ToString() + @" moedas");
        }

        private static int ParsePercent(TextRelay relay, int current)
        {
            if (relay == null || String.IsNullOrWhiteSpace(relay.Text))
                return current;

            int value;
            if (!Int32.TryParse(relay.Text.Trim(), out value))
                return current;

            if (value < 0)
                value = 0;
            if (value > 50)
                value = 50;

            return value;
        }

        private static int ParseNonNegative(TextRelay relay, int current)
        {
            if (relay == null || String.IsNullOrWhiteSpace(relay.Text))
                return current;

            int value;
            if (!Int32.TryParse(relay.Text.Trim(), out value))
                return current;

            return Math.Max(0, value);
        }

        private bool HandleTreasuryResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;

            if (button != ButtonTreasuryCitizenTax &&
                button != ButtonTreasuryVendorTax &&
                button != ButtonTreasurySalaryTax &&
                button != ButtonTreasuryAuctionTax &&
                button != ButtonTreasuryReligiousTax)
                return false;

            int weeklyCitizenTax = ReinoTreasurySystem.GetWeeklyCitizenTax(m_CityId);
            int vendorTax = ReinoTreasurySystem.GetVendorSalesTaxPercent(m_CityId);
            int salaryTax = ReinoTreasurySystem.GetSalaryTaxPercent(m_CityId);
            int auctionTax = ReinoTreasurySystem.GetAuctionTaxPercent(m_CityId);
            int religiousTax = ReinoTreasurySystem.GetReligiousDonationTaxPercent(m_CityId);

            switch (button)
            {
                case ButtonTreasuryCitizenTax:
                    weeklyCitizenTax = ParseNonNegative(info.GetTextEntry(EntryTreasuryCitizenTax), weeklyCitizenTax);
                    break;
                case ButtonTreasuryVendorTax:
                    vendorTax = ParsePercent(info.GetTextEntry(EntryTreasuryVendorTax), vendorTax);
                    break;
                case ButtonTreasurySalaryTax:
                    salaryTax = ParsePercent(info.GetTextEntry(EntryTreasurySalaryTax), salaryTax);
                    break;
                case ButtonTreasuryAuctionTax:
                    if (ReinoTreasurySystem.HasAuctionHouse(m_CityId))
                        auctionTax = ParsePercent(info.GetTextEntry(EntryTreasuryAuctionTax), auctionTax);
                    break;
                case ButtonTreasuryReligiousTax:
                    religiousTax = ParsePercent(info.GetTextEntry(EntryTreasuryReligiousTax), religiousTax);
                    break;
            }

            string message;
            ReinoTreasurySystem.UpdateConfiguration(from, m_CityId, weeklyCitizenTax, vendorTax, salaryTax, auctionTax, religiousTax, out message);
            from.SendMessage(message);
            from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, m_SelectedBuildingId, m_BuildingPage, 2));
            return true;
        }
    }
}
