using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos
{
    public class ReinoLotSignGump : Gump
    {
        public ReinoLotSignGump(PlayerMobile from, int lotId) : base(0, 0)
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(lotId);
            ReinoLotState state = ReinoExpansionSystem.GetLotState(lotId);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(353, 211, 410, 319, 394);
            AddImageTiled(355, 186, 406, 35, 638);
            AddImageTiled(321, 218, 39, 311, 639);
            AddImageTiled(355, 518, 405, 35, 638);
            AddImageTiled(744, 214, 39, 311, 639);
            AddImage(309, 176, 1359);
            AddImage(733, 175, 1359);
            AddImage(732, 509, 1359);
            AddImage(309, 513, 1359);

            if (lot == null || state == null)
            {
                AddLabel(430, 225, 0, "Lote inválido");
                AddHtml(371, 277, 362, 132, "<BASEFONT COLOR=#000000>Não foi possível carregar este lote.</BASEFONT>", false, false);
                return;
            }

            AddLabel(470, 225, 0, lot.Name);
            AddImage(352, 240, 443);
            AddHtml(371, 277, 362, 190, ReinoExpansionSystem.BuildLotSignHtml(from, lot, state), false, false);
            AddLabel(377, 486, 0, "Reino");
            AddLabel(472, 486, 0, ReinoElectionsSystem.GetCityName(lot.CityId));
            AddLabel(590, 486, 0, "Status");
            AddLabel(655, 486, 0, ReinoExpansionSystem.GetStatusLabel(state.Status));
        }
    }
}
