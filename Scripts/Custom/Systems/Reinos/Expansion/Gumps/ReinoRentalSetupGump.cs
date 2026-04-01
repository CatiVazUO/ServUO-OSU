using System;
using System.Collections.Generic;
using Server;
using Server.Custom.Systems.Culture;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Reinos
{
    public class ReinoRentalSetupGump : GumpPlusLight
    {
        private readonly ReinoRentalSign c_Sign;
        private string c_CulturesCsv;
        private TimeSpan c_RentTime;

        public ReinoRentalSetupGump(Mobile m, ReinoRentalSign sign)
            : this(m, sign, sign != null ? sign.AllowedCulturesCsv : "Todos", sign != null && sign.RentByTime != TimeSpan.Zero ? sign.RentByTime : TimeSpan.FromDays(7.0))
        {
        }

        public ReinoRentalSetupGump(Mobile m, ReinoRentalSign sign, string culturesCsv, TimeSpan rentTime)
            : base(m, 0, 0)
        {
            c_Sign = sign;
            c_CulturesCsv = ReinoRentalCultureHelper.NormalizeCsv(culturesCsv);
            c_RentTime = rentTime == TimeSpan.Zero ? TimeSpan.FromDays(7.0) : rentTime;

            m.CloseGump(typeof(ReinoRentalSetupGump));
        }

        protected override void BuildGump()
        {
            if (c_Sign == null)
                return;

            AddImageTiled(275, 131, 450, 640, 398);
            AddImageTiled(716, 138, 25, 607, 369);
            AddImageTiled(254, 140, 26, 618, 370);
            AddImageTiled(269, 115, 450, 25, 371);
            AddImageTiled(281, 764, 443, 30, 372);
            AddImage(246, 107, 415);
            AddImage(679, 105, 414);
            AddImage(249, 730, 412);
            AddImage(680, 728, 413);
            AddLabel(420, 147, 0, "Configurar Imóvel do Reino");
            AddImage(293, 163, 443);

            AddButton(601, 199, 559, 559, "Nome", new GumpCallback(SetName));
            AddTextField(415, 199, 186, 20, 0, 0, "Name", c_Sign.Name);
            AddLabel(305, 201, 0, "Nome do Imóvel:");

            AddLabel(307, 256, 0, "Cobrança:");
            AddButton(410, 252, 453, 453, "RentDown", new GumpCallback(RentDown));
            AddLabel(480, 256, 0, GetRentLabel());
            AddButton(570, 252, 452, 452, "RentUp", new GumpCallback(RentUp));

            AddLabel(315, 305, 0, "Valor:");
            AddTextField(369, 306, 208, 20, 0, 0, "Price", c_Sign.Price.ToString());
            AddButton(602, 306, 559, 559, "Price", new GumpCallback(SetPrice));

            AddImage(296, 348, 443);
            DrawCultureButton(379, 370, "Mataluns");
            DrawCultureButton(379, 418, "Kamay");
            DrawCultureButton(521, 372, "Sarangs");
            DrawCultureButton(521, 420, "Zorteros");
            DrawCultureButton(642, 374, "Todos");

            AddLabel(314, 380, 0, "Mataluns");
            AddLabel(316, 424, 0, "Kamay");
            AddLabel(455, 382, 0, "Sarangs");
            AddLabel(457, 426, 0, "Zorteros");
            AddLabel(596, 381, 0, "Todos");

            AddLabel(307, 480, 0, "Resumo:");
            AddHtml(380, 478, 240, 90, ReinoRentalCultureHelper.BuildDisplayLabel(c_CulturesCsv), false, true);

            AddImage(295, 583, 443);
            AddLabel(307, 603, 0, c_Sign.GovernorConfigured ? "Estado: configurado" : "Estado: aguardando liberação");
            AddLabel(307, 630, 0, "Tipo: " + c_Sign.PropertyType.ToString());

            AddButton(350, 734, 559, 559, "Salvar", new GumpCallback(SaveOnly));
            AddLabel(304, 734, 0, "Salvar");

            AddButton(520, 734, 559, 559, "SaveOpen", new GumpCallback(SaveAndOpenRent));
            AddLabel(430, 734, 0, "Salvar e Abrir Aluguel");
        }

        private void DrawCultureButton(int x, int y, string cultureId)
        {
            bool selected = ReinoRentalCultureHelper.ContainsCulture(c_CulturesCsv, cultureId);
            AddButton(x, y, selected ? 440 : 442, selected ? 440 : 442, "Culture " + cultureId, new GumpStateCallback(CultureToggle), cultureId);
        }

        private string GetRentLabel()
        {
            if (c_RentTime == TimeSpan.FromDays(1.0))
                return "Diária";

            if (c_RentTime == TimeSpan.FromDays(30.0))
                return "Mensal";

            return "Semanal";
        }

        private void RentDown()
        {
            if (c_RentTime == TimeSpan.FromDays(7.0))
                c_RentTime = TimeSpan.FromDays(1.0);
            else if (c_RentTime == TimeSpan.FromDays(30.0))
                c_RentTime = TimeSpan.FromDays(7.0);

            NewGump();
        }

        private void RentUp()
        {
            if (c_RentTime == TimeSpan.FromDays(1.0))
                c_RentTime = TimeSpan.FromDays(7.0);
            else if (c_RentTime == TimeSpan.FromDays(7.0))
                c_RentTime = TimeSpan.FromDays(30.0);

            NewGump();
        }

        private void CultureToggle(object obj)
        {
            string cultureId = obj as string;

            if (!String.IsNullOrWhiteSpace(cultureId))
                c_CulturesCsv = ReinoRentalCultureHelper.ToggleCulture(c_CulturesCsv, cultureId);

            NewGump();
        }

        private void SetName()
        {
            c_Sign.Name = String.IsNullOrWhiteSpace(GetTextField("Name")) ? "Imóvel do reino" : GetTextField("Name").Trim();
            Owner.SendMessage("Nome definido!");
            NewGump();
        }

        private void SetPrice()
        {
            int price = GetTextFieldInt("Price");
            if (price < 0)
                price = 0;

            c_Sign.Price = price;
            Owner.SendMessage("Valor definido!");
            NewGump();
        }

        private void SaveOnly()
        {
            ApplyValues();
            Owner.SendMessage("Imóvel configurado com sucesso.");
            NewGump();
        }

        private void SaveAndOpenRent()
        {
            ApplyValues();
            Owner.SendMessage("Imóvel configurado com sucesso.");
            c_Sign.OpenRentalOffer(Owner);
            NewGump();
        }

        private void ApplyValues()
        {
            c_Sign.Name = String.IsNullOrWhiteSpace(GetTextField("Name")) ? "Imóvel do reino" : GetTextField("Name").Trim();

            int price = GetTextFieldInt("Price");
            if (price < 0)
                price = 0;

            c_Sign.Price = price;
            c_Sign.RentByTime = c_RentTime;
            c_Sign.RecurRent = true;
            c_Sign.GovernorConfigured = true;
            c_Sign.AllowedCulturesCsv = c_CulturesCsv;
            c_Sign.AllowedCulture = "Todos";
        }
    }
}
