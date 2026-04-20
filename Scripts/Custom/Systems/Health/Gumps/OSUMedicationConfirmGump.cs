
using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Reinos;


namespace Server.Custom.Systems.Health.Gumps
{
    public class OSUMedicationConfirmGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly OSUMedicationTub _tub;
        private readonly Bandage _bandage;
        private readonly int _total;

        public OSUMedicationConfirmGump(PlayerMobile from, OSUMedicationTub tub, Bandage bandage) : base(0, 0)
        {
            _from = from;
            _tub = tub;
            _bandage = bandage;
            _total = Math.Max(1, bandage != null ? bandage.Amount : 1) * (tub != null ? tub.CostPerBandage : 0);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(426, 203, 267, 193, 392);
            AddImageTiled(395, 206, 40, 197, 631);
            AddImageTiled(678, 206, 40, 197, 631);
            AddImageTiled(423, 177, 265, 37, 630);
            AddImageTiled(424, 390, 265, 37, 630);
            AddImage(391, 173, 1315);
            AddImage(661, 170, 1316);
            AddImage(663, 375, 1317);
            AddImage(391, 375, 1318);

            AddLabel(531, 219, 1152, "Remédio");
            AddButton(522, 363, 495, 495, 1, GumpButtonType.Reply, 0);

            string desc = tub != null ? tub.GetEffectDescription() : String.Empty;
            desc += "<br><br><BASEFONT COLOR=#FFFFFF><B>Custo por bandagem:</B> " + (tub != null ? tub.CostPerBandage : 0).ToString() + " moedas";
            desc += "<br><B>Total:</B> " + _total.ToString() + " moedas</BASEFONT>";

            AddHtml(441, 253, 230, 91, desc, false, true);
        }

        public override void OnResponse(Network.NetState sender, RelayInfo info)
        {
            if (_from == null || _tub == null || _bandage == null || _from.Deleted || _tub.Deleted || _bandage.Deleted)
                return;

            if (info.ButtonID != 1)
                return;

            _tub.TryDipBandages(_from, _bandage);
        }
    }
}
