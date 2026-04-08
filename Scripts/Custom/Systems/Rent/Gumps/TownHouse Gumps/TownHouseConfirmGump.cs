using Server;
using Server.Custom.Systems.Rent;
using Server.Mobiles;
using Server.Multis;
using System;


namespace Server.Custom.Systems.Rent
{
    public class TownHouseConfirmGump : GumpPlusLight
    {
        private TownHouseSign c_Sign;
        private bool c_Items;

        public TownHouseConfirmGump(Mobile m, TownHouseSign sign) : base(m, 100, 100)
        {
            c_Sign = sign;
        }

        protected override void BuildGump()
        {
            bool cultureAllowed = c_Sign.IsCultureAllowed(Owner);
            string diplomacyReason;
            bool diplomacyAllowed = c_Sign.CanAcquireByDiplomacy(Owner, out diplomacyReason);
            bool canConfirm = cultureAllowed && diplomacyAllowed && c_Sign.CanBuyHouse(Owner) && c_Sign.CanOwnThisProperty(Owner);

            AddPage(0);

            // Moldura baseada no layout criado no GumpStudio
            AddImageTiled(275, 131, 223, 235, 398);
            AddImageTiled(489, 148, 25, 213, 369);
            AddImageTiled(254, 140, 26, 216, 370);
            AddImageTiled(269, 115, 230, 25, 371);
            AddImageTiled(281, 352, 207, 30, 372);
            AddImage(246, 107, 415);
            AddImage(453, 105, 414);
            AddImage(248, 316, 412);
            AddImage(452, 318, 413);

            // Título e informações
            string titulo = c_Sign.RentByTime == TimeSpan.Zero ? "Comprar casa?" : "Alugar casa?";
            AddHtml(286, 150, 200, 20, String.Format("<CENTER>{0}</CENTER>", titulo), false, false);

            string valorTexto;
            if (c_Sign.RentByTime == TimeSpan.Zero)
                valorTexto = String.Format("Valor: {0}", c_Sign.Free ? "Free" : c_Sign.Price.ToString());
            else if (c_Sign.RecurRent)
                valorTexto = String.Format("Valor {0}: {1}", c_Sign.PriceType, c_Sign.Price);
            else
                valorTexto = String.Format("Valor {0}: {1}", c_Sign.PriceTypeShort, c_Sign.Price);

            AddHtml(321, 205, 170, 20, valorTexto, false, false);
            AddHtml(321, 228, 170, 20, "Lockdowns: " + c_Sign.Locks, false, false);
            AddHtml(321, 254, 170, 20, "Secures: " + c_Sign.Secures, false, false);

            // Mantém a funcionalidade de itens, usando o checkbox do layout mockado
            if (c_Sign.KeepItems)
            {
                AddImageTiled(287, 174, 21, 21, 461);
                AddImageTiled(303, 174, 179, 21, 462);
                AddButton(290, 176, c_Items ? 0xD3 : 0xD2, c_Items ? 0xD3 : 0xD2, "", new GumpCallback(Items));
                AddHtml(320, 176, 160, 20, "Itens: " + c_Sign.ItemsPrice, false, false);
            }

            // Botões funcionais originais, apenas com o visual do mockup
            AddButton(296, 303, 544, 544, "", new GumpCallback(Cancel));

            if (canConfirm)
                AddButton(396, 302, 559, 559, "", new GumpCallback(Confirm));

            if (!c_Sign.CanOwnThisProperty(Owner))
                AddHtml(286, 276, 200, 40, "<CENTER>" + c_Sign.CannotOwnMessage(Owner) + "</CENTER>", false, false);
            else if (!cultureAllowed)
                AddHtml(286, 276, 200, 40, "<CENTER>Seu povo não pode adquirir esta propriedade.</CENTER>", false, false);
            else if (!diplomacyAllowed)
                AddHtml(286, 276, 200, 50, "<CENTER>" + diplomacyReason + "</CENTER>", false, false);
            else if (!c_Sign.CanBuyHouse(Owner))
                AddHtml(286, 276, 200, 40, "<CENTER>Você não atende aos requisitos desta propriedade.</CENTER>", false, false);
            else if (!c_Sign.PriceReady)
                AddHtml(286, 276, 200, 40, "<CENTER>O setup desta propriedade ainda não está completo.</CENTER>", false, false);
        }

        private void Items()
        {
            c_Items = !c_Items;

            NewGump();
        }

        private void Cancel()
        {
        }

        private void Confirm()
        {
            c_Sign.Purchase(Owner, c_Items);
        }
    }
}
