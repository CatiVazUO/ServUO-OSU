using System;
using System.Collections;
using Server;
using Server.Custom.Systems.Rent;
using Server.Multis;

namespace Server.Custom.Systems.Rent
{
    public class TownHousesGump : GumpPlusLight
    {
        public enum ListPage { Houses, Commercial, Tombs }

        public static void Initialize()
        {
            RUOVersion.AddCommand("OSUHouses", AccessLevel.GameMaster, new TownHouseCommandHandler(OnHouses));
            RUOVersion.AddCommand("DeleteRentalByName", AccessLevel.GameMaster, new TownHouseCommandHandler(OnDeleteRentalByName));
        }

        private static void OnHouses(CommandInfo info)
        {
            new TownHousesGump(info.Mobile);
        }

        private static void OnDeleteRentalByName(CommandInfo info)
        {
            string search = info.ArgString == null ? String.Empty : info.ArgString.Trim();

            if (String.IsNullOrWhiteSpace(search))
            {
                info.Mobile.SendMessage("Use: [DeleteRentalByName <nome da placa, nome do dono ou nome do túmulo>");
                return;
            }

            ArrayList signs = TownHouseSign.AllSigns;
            ArrayList matches = new ArrayList();

            for (int i = 0; i < signs.Count; i++)
            {
                TownHouseSign sign = signs[i] as TownHouseSign;
                if (sign == null || sign.Deleted)
                    continue;

                string signName = sign.Name ?? String.Empty;
                string ownerName = (sign.House != null && sign.House.Owner != null) ? sign.House.Owner.Name ?? String.Empty : String.Empty;
                string tombName = sign.IsTomb ? sign.GetTombDisplayName() ?? String.Empty : String.Empty;

                if (ContainsInsensitive(signName, search) || ContainsInsensitive(ownerName, search) || ContainsInsensitive(tombName, search))
                    matches.Add(sign);
            }

            if (matches.Count == 0)
            {
                info.Mobile.SendMessage("Nenhuma propriedade encontrada com esse nome.");
                return;
            }

            if (matches.Count > 1)
            {
                info.Mobile.SendMessage("Foram encontradas {0} propriedades. Seja mais específico.", matches.Count);

                for (int i = 0; i < matches.Count && i < 10; i++)
                {
                    TownHouseSign sign = matches[i] as TownHouseSign;
                    if (sign == null)
                        continue;

                    string signName = sign.Name ?? "-";
                    string ownerName = (sign.House != null && sign.House.Owner != null) ? sign.House.Owner.Name ?? "-" : "-";
                    string tombName = sign.IsTomb ? sign.GetTombDisplayName() ?? "-" : "-";

                    info.Mobile.SendMessage("#{0}: placa='{1}' dono='{2}' túmulo='{3}' loc={4}", i + 1, signName, ownerName, tombName, sign.Location);
                }

                return;
            }

            DeleteRental(matches[0] as TownHouseSign, info.Mobile);
        }

        private static bool ContainsInsensitive(string value, string search)
        {
            if (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(search))
                return false;

            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void DeleteRental(TownHouseSign sign, Mobile from)
        {
            if (sign == null || sign.Deleted)
            {
                from.SendMessage("A placa já não existe.");
                return;
            }

            TownHouse house = sign.House;

            string signName = sign.Name ?? "-";
            string ownerName = (house != null && house.Owner != null) ? house.Owner.Name ?? "-" : "-";
            string tombName = sign.IsTomb ? sign.GetTombDisplayName() ?? "-" : "-";

            Item hiddenHouseSign = null;

            if (house != null && house.Sign != null && !house.Sign.Deleted)
                hiddenHouseSign = house.Sign as Item;

            if (hiddenHouseSign != null && !hiddenHouseSign.Deleted)
                hiddenHouseSign.Delete();

            if (house != null && !house.Deleted)
                house.Delete();

            if (!sign.Deleted)
                sign.Delete();

            from.SendMessage("Propriedade apagada. Placa='{0}' Dono='{1}' Túmulo='{2}'", signName, ownerName, tombName);
        }

        private ListPage c_ListPage;
        private int c_Page;
        private const int PerPage = 4;

        public TownHousesGump(Mobile m) : base(m, 0, 0)
        {
            m.CloseGump(typeof(TownHousesGump));
        }

        protected override void BuildGump()
        {
            AddImageTiled(275, 131, 450, 640, 398);
            AddImageTiled(716, 138, 25, 607, 369);
            AddImageTiled(254, 140, 26, 618, 370);
            AddImageTiled(269, 115, 450, 25, 371);
            AddImageTiled(281, 764, 443, 30, 372);
            AddImage(246, 107, 415);
            AddImage(679, 105, 414);
            AddImage(249, 730, 412);
            AddImage(680, 728, 413);
            AddLabel(453, 147, 0, "OSU Houses");
            AddImage(293, 163, 443);

            ArrayList list = GetCurrentList();
            int countH = CountByType(OSUPropertyType.House);
            int countC = CountByType(OSUPropertyType.Commercial);
            int countT = CountByType(OSUPropertyType.Tomb);

            AddButton(303, 198, 451, 451, "Prev Type", new GumpCallback(PrevType));
            AddButton(663, 196, 450, 450, "Next Type", new GumpCallback(NextType));

            if (c_ListPage == ListPage.Houses)
            {
                AddLabel(413, 205, 0, "Quantidade de Casas: " + countH);
                AddLabel(440, 269, 0, "Lista de Casas");
                AddLabel(448, 727, 0, "Adicionar Nova Casa");
            }
            else if (c_ListPage == ListPage.Commercial)
            {
                AddLabel(413, 205, 0, "Quantidade de Lojas: " + countC);
                AddLabel(440, 269, 0, "Lista de Lojas");
                AddLabel(448, 727, 0, "Adicionar Nova Loja");
            }
            else
            {
                AddLabel(416, 205, 0, "Quantidade Lápides: " + countT);
                AddLabel(436, 269, 0, "Lista de Lápides");
                AddLabel(439, 727, 0, "Adicionar Nova Lápide");
            }

            AddImage(294, 238, 443);

            int start = c_Page * PerPage;
            int y = 299;
            for (int i = start; i < start + PerPage && i < list.Count; i++)
            {
                TownHouseSign sign = (TownHouseSign)list[i];
                AddLabel(327, y, 0, sign.Name);
                AddButton(300, y + 2, 543, 248, "Editar", new GumpStateCallback(EditSign), sign);
                y += 22;
            }

            AddButton(303, 686 + (c_ListPage == ListPage.Houses ? 0 : (c_ListPage == ListPage.Commercial ? 4 : 5)), 451, 451, "Page Down", new GumpCallback(PageDown));
            AddButton(663, 684 + (c_ListPage == ListPage.Houses ? 0 : (c_ListPage == ListPage.Commercial ? 5 : 5)), 450, 450, "Page Up", new GumpCallback(PageUp));
            AddButton(c_ListPage == ListPage.Houses ? 416 : (c_ListPage == ListPage.Commercial ? 383 : 407), 727 + (c_ListPage == ListPage.Houses ? 0 : 2), 535, 535, "New", new GumpCallback(New));
        }

        private ArrayList GetCurrentList()
        {
            ArrayList list = new ArrayList();
            foreach (TownHouseSign sign in TownHouseSign.AllSigns)
            {
                if (c_ListPage == ListPage.Houses && sign.PropertyType == OSUPropertyType.House)
                    list.Add(sign);
                else if (c_ListPage == ListPage.Commercial && OSUHousingConfig.EnableCommercialProperties && sign.PropertyType == OSUPropertyType.Commercial)
                    list.Add(sign);
                else if (c_ListPage == ListPage.Tombs && OSUHousingConfig.EnableTombs && sign.PropertyType == OSUPropertyType.Tomb)
                    list.Add(sign);
            }

            list.Sort(new InternalSort());
            return list;
        }

        private int CountByType(OSUPropertyType type)
        {
            int count = 0;
            foreach (TownHouseSign sign in TownHouseSign.AllSigns)
            {
                if (sign.PropertyType == type)
                    count++;
            }
            return count;
        }

        private void EditSign(object obj)
        {
            TownHouseSign sign = obj as TownHouseSign;
            if (sign == null)
                return;

            bool governmentReduced = sign.GovernmentManaged && sign.IsGovernmentManager(Owner) && Owner.AccessLevel == AccessLevel.Player;
            new TownHouseSetupGump(Owner, sign, governmentReduced);
        }

        private void PrevType()
        {
            if (c_ListPage == ListPage.Houses)
                c_ListPage = OSUHousingConfig.EnableTombs ? ListPage.Tombs : ListPage.Commercial;
            else if (c_ListPage == ListPage.Commercial)
                c_ListPage = ListPage.Houses;
            else
                c_ListPage = OSUHousingConfig.EnableCommercialProperties ? ListPage.Commercial : ListPage.Houses;

            c_Page = 0;
            NewGump();
        }

        private void NextType()
        {
            if (c_ListPage == ListPage.Houses)
                c_ListPage = OSUHousingConfig.EnableCommercialProperties ? ListPage.Commercial : (OSUHousingConfig.EnableTombs ? ListPage.Tombs : ListPage.Houses);
            else if (c_ListPage == ListPage.Commercial)
                c_ListPage = OSUHousingConfig.EnableTombs ? ListPage.Tombs : ListPage.Houses;
            else
                c_ListPage = ListPage.Houses;

            c_Page = 0;
            NewGump();
        }

        private void New()
        {
            TownHouseSign sign = new TownHouseSign();
            sign.PropertyType = c_ListPage == ListPage.Commercial ? OSUPropertyType.Commercial : (c_ListPage == ListPage.Tombs ? OSUPropertyType.Tomb : OSUPropertyType.House);
            sign.Flip = false; // leste = 0x0BD2
            sign.ItemID = 0x18B7;
            Owner.AddToBackpack(sign);
            Owner.SendMessage("Uma nova placa foi colocada na sua mochila. Durante o setup ela se move sozinha.");
            new TownHouseSetupGump(Owner, sign);
        }

        private void PageUp()
        {
            ArrayList list = GetCurrentList();
            if ((c_Page + 1) * PerPage < list.Count)
                c_Page++;
            NewGump();
        }

        private void PageDown()
        {
            if (c_Page > 0)
                c_Page--;
            NewGump();
        }

        private class InternalSort : IComparer
        {
            public int Compare(object x, object y)
            {
                TownHouseSign a = x as TownHouseSign;
                TownHouseSign b = y as TownHouseSign;
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                return Insensitive.Compare(a.Name, b.Name);
            }
        }
    }
}
