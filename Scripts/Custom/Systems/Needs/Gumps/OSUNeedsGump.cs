using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Systems.Needs.Gumps
{
    public class OSUNeedsGump : Gump
    {
        public static void TryRefresh(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.NetState == null)
                return;

            if (!pm.HasGump(typeof(OSUNeedsGump)))
                return;

            pm.CloseGump(typeof(OSUNeedsGump));
            pm.SendGump(new OSUNeedsGump(pm));
        }

        private readonly PlayerMobile _pm;

        public OSUNeedsGump(PlayerMobile pm) : base(0, 0)
        {
            _pm = pm;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Fundo do seu gump
            AddImage(23, 26, 428);

            DrawBars();
        }

        private void DrawBars()
        {
            int hunger = Math.Max(0, Math.Min(100, _pm.OSUHunger));
            int thirst = Math.Max(0, Math.Min(100, _pm.OSUThirst));

            // ======= FOME / SEDE (como você já tinha) =======
            int hungerX = 30;
            int thirstX = 52;

            int barY = 39;
            int barHW = 10;
            int barTW = 10;
            int barHH = 93;
            int barTH = 94;

            int hungerFillH = (hunger * barHH) / 100;
            int thirstFillH = (thirst * barTH) / 100;

            if (hungerFillH > 0)
            {
                int yStart = barY + (barHH - hungerFillH);
                AddImageTiled(hungerX, yStart, barHW, hungerFillH, 429);
            }

            if (thirstFillH > 0)
            {
                int yStart = barY + (barTH - thirstFillH);
                AddImageTiled(thirstX, yStart, barTW, thirstFillH, 430);
            }

            // ======= TERMICO (nova coluna) =======
            // Layout do seu gump exportado:
            // calor: x=73 y=39 altura=47 usa imagem 431 (preenche para cima)
            // frio : x=73 y=87 altura=46 usa imagem 432 (preenche para baixo)
            int thermoX = 73;

            int heatY = 39;
            int heatH = 47;
            int heatW = 11;

            int coldY = 87;
            int coldH = 46;
            int coldW = 11;

            // Conforto: -16..+16
            int comfort = Server.Custom.Systems.Climate.OSUClimatePenaltySystem.GetThermalComfort(_pm);

            int heat = 0;
            int cold = 0;

            if (comfort > 0) heat = Math.Min(16, comfort);
            else if (comfort < 0) cold = Math.Min(16, -comfort);

            // calor: mostra acima da linha (cresce "de baixo pra cima")
            if (heat > 0)
            {
                int fill = (heat * heatH) / 16;
                if (fill < 1) fill = 1;

                int yStart = heatY + (heatH - fill);
                AddImageTiled(thermoX, yStart, heatW, fill, 431);
            }

            // frio: mostra abaixo da linha (cresce "de cima pra baixo")
            if (cold > 0)
            {
                int fill = (cold * coldH) / 16;
                if (fill < 1) fill = 1;

                AddImageTiled(thermoX, coldY, coldW, fill, 432);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }
}
