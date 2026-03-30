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
        }

        private static void OnHouses(CommandInfo info)
        {
            new TownHousesGump(info.Mobile);
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
            if (!(obj is TownHouseSign))
                return;

            new TownHouseSetupGump(Owner, (TownHouseSign)obj);
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
            sign.ItemID = 0x0BD2;
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
