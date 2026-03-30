using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Afortunado : OSUDefQualDefinition
    {
        public override string Id => "afortunado";
        public override string Name => "Afortunado";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -5000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Afortunado</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -5000<BR><BR>
A sorte sorri para você. Quando dá o <B>golpe final</B> em um monstro, tende a encontrar um pouco mais de dinheiro.
</BASEFONT>";


        public override int ModifyFinalBlowGold(PlayerMobile pm, int currentGold)
        {
            return currentGold + 10;
        }
    }
}
