using System;
using System.Collections.Generic;
using Server.Custom.Systems.Postos;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Reinos
{
    public partial class ReinoExpansionGump
    {
        private const int ButtonGeneralConstructionBase = 59000;
        private const int ButtonGeneralPostoBase = 59100;
        private const int ButtonGeneralResign = 59200;

        private void BuildGeneralPage()
        {
            AddPage(1);

            AddLabel(741, 173, 0, @"Visão Geral");
            AddLabel(473, 224, 0, @"Tesouro");
            AddImageTiled(406, 251, 819, 5, 367);
            AddLabel(410, 273, 0, @"Moedas:");
            AddImageTiled(598, 222, 6, 470, 365);
            AddLabel(410, 298, 0, @"Tecidos:");
            AddLabel(410, 322, 0, @"Ferro:");
            AddLabel(410, 347, 0, @"Madeira:");
            AddImageTiled(406, 407, 194, 5, 367);
            AddLabel(468, 379, 0, @"Entrada");
            AddLabel(410, 429, 0, @"Moedas:");
            AddLabel(410, 454, 0, @"Tecidos:");
            AddLabel(410, 478, 0, @"Ferro:");
            AddLabel(410, 503, 0, @"Madeira:");
            AddImageTiled(406, 567, 194, 5, 367);
            AddLabel(476, 541, 0, @"Saída");
            AddLabel(412, 590, 0, @"Moedas:");
            AddLabel(412, 615, 0, @"Tecidos:");
            AddLabel(412, 639, 0, @"Ferro:");
            AddLabel(412, 664, 0, @"Madeira:");
            AddLabel(628, 228, 0, @"Construções Ativas");
            AddImageTiled(772, 225, 6, 470, 365);
            AddImageTiled(951, 226, 6, 470, 365);
            AddLabel(802, 228, 0, @"Áreas Conquistadas");
            AddLabel(1072, 227, 0, @"Mandato");
            AddImageTiled(956, 386, 268, 5, 367);
            AddLabel(1052, 361, 0, @"Trabalhadores");
            AddImageTiled(956, 561, 268, 5, 367);
            AddLabel(1041, 538, 0, @"Doações Totais");
            AddLabel(1043, 170, 0, @"Resignar Liderança do Reino");
            AddButton(1013, 170, 529, 529, ButtonGeneralResign, GumpButtonType.Reply, 0);

            ReinoTreasuryResourceBundle current = ReinoTreasurySystem.GetCombinedTreasuryResources(m_CityId);
            ReinoTreasuryResourceBundle income = ReinoTreasurySystem.GetRecurringIncomeBaseline(m_CityId);
            ReinoTreasuryResourceBundle expense = ReinoTreasurySystem.GetRecurringExpenseBaseline(m_CityId);
            ReinoTreasuryResourceBundle donations = ReinoTreasurySystem.GetAllTimeDonations(m_CityId);

            AddLabel(490, 273, 0, current.Gold.ToString());
            AddLabel(490, 298, 0, current.Cloth.ToString());
            AddLabel(490, 322, 0, current.Iron.ToString());
            AddLabel(490, 347, 0, current.Wood.ToString());

            AddLabel(490, 429, 0, income.Gold.ToString());
            AddLabel(490, 454, 0, income.Cloth.ToString());
            AddLabel(490, 478, 0, income.Iron.ToString());
            AddLabel(490, 503, 0, income.Wood.ToString());

            AddLabel(490, 590, 0, expense.Gold.ToString());
            AddLabel(490, 615, 0, expense.Cloth.ToString());
            AddLabel(490, 639, 0, expense.Iron.ToString());
            AddLabel(490, 664, 0, expense.Wood.ToString());

            List<ReinoConstructionRuntimeInfo> constructions = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);
            int[] constructionY = new int[] { 272, 297, 321, 346, 371, 396 };
            for (int i = 0; i < constructions.Count && i < constructionY.Length; i++)
            {
                AddButton(618, constructionY[i] + 3, 536, 435, ButtonGeneralConstructionBase + i, GumpButtonType.Reply, 0);
                AddLabel(642, constructionY[i], 0, constructions[i].Name);
            }

            List<PostoDefinition> postos = ReinoMaintenanceSystem.GetActivePostos(m_CityId);
            int[] postoY = new int[] { 272, 297, 321, 346, 371 };
            for (int i = 0; i < postos.Count && i < postoY.Length; i++)
            {
                AddButton(794, postoY[i] + 3, 536, 435, ButtonGeneralPostoBase + i, GumpButtonType.Reply, 0);
                AddLabel(818, postoY[i], 0, postos[i].Name);
            }

            ReinoCityData city = null;
            ReinoElectionsSystem._cities.TryGetValue(m_CityId, out city);

            DateTime shardNow = ReinoElectionsSystem.GetShardNow();
            DateTime nextElection = GetNextElectionDate(shardNow);
            TimeSpan elapsed = TimeSpan.Zero;

            if (city != null && city.GovernorSinceUtc != DateTime.MinValue)
                elapsed = DateTime.UtcNow - city.GovernorSinceUtc;

            TimeSpan remaining = nextElection - shardNow;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            AddLabel(978, 268, 0, @"Tempo Decorrido:");
            AddLabel(978, 293, 0, @"Tempo Restante:");
            AddLabel(978, 317, 0, @"Próxima Eleição:");
            AddLabel(1110, 268, 0, FormatTimeSpan(elapsed));
            AddLabel(1110, 293, 0, FormatTimeSpan(remaining));
            AddLabel(1110, 317, 0, nextElection.ToString("dd/MM/yyyy"));

            AddLabel(974, 409, 0, @"Salários Guardas:");
            AddLabel(974, 434, 0, @"Salários Cargos:");
            AddLabel(974, 461, 0, @"Número de Guardas:");
            AddLabel(974, 488, 0, @"Cargos do Governo:");
            AddLabel(1120, 409, 0, @"0");
            AddLabel(1120, 434, 0, ReinoTreasurySystem.GetNetWeeklyCommissionSalary(m_CityId).ToString());
            AddLabel(1120, 461, 0, @"0");
            AddLabel(1120, 488, 0, ReinoTreasurySystem.GetGovernmentRoleSlotCount(m_CityId).ToString());

            AddLabel(981, 584, 0, @"Moedas:");
            AddLabel(981, 609, 0, @"Tecidos:");
            AddLabel(981, 633, 0, @"Ferro:");
            AddLabel(981, 658, 0, @"Madeira:");
            AddLabel(1060, 584, 0, donations.Gold.ToString());
            AddLabel(1060, 609, 0, donations.Cloth.ToString());
            AddLabel(1060, 633, 0, donations.Iron.ToString());
            AddLabel(1060, 658, 0, donations.Wood.ToString());
        }

        private static DateTime GetNextElectionDate(DateTime now)
        {
            return new DateTime(now.Year, now.Month, 1).AddMonths(1);
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts < TimeSpan.Zero)
                ts = TimeSpan.Zero;

            int days = Math.Max(0, ts.Days);
            return days + " dias";
        }

        private bool HandleGeneralResponse(PlayerMobile from, RelayInfo info)
        {
            int button = info.ButtonID;

            if (button == ButtonGeneralResign)
            {
                string message;
                ReinoTreasurySystem.ResignLeadership(from, m_CityId, out message);
                from.SendMessage(message);
                from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, String.Empty, 0, 1));
                return true;
            }

            List<ReinoConstructionRuntimeInfo> constructions = ReinoMaintenanceSystem.GetActiveConstructions(m_CityId);
            if (button >= ButtonGeneralConstructionBase && button < ButtonGeneralConstructionBase + 100)
            {
                int index = button - ButtonGeneralConstructionBase;
                if (index >= 0 && index < constructions.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, constructions[index].Key, m_BuildingPage, 9));

                return true;
            }

            List<PostoDefinition> postos = ReinoMaintenanceSystem.GetActivePostos(m_CityId);
            if (button >= ButtonGeneralPostoBase && button < ButtonGeneralPostoBase + 100)
            {
                int index = button - ButtonGeneralPostoBase;
                if (index >= 0 && index < postos.Count)
                    from.SendGump(new ReinoExpansionGump(from, m_CityId, m_SelectedLotId, m_SelectedWallAreaId, "P:" + postos[index].Id, m_BuildingPage, 10));

                return true;
            }

            return false;
        }
    }
}
