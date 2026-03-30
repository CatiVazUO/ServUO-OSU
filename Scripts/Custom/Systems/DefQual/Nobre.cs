using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Nobre : OSUDefQualDefinition
    {
        public override string Id => "nobre";
        public override string Name => "Nobre";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -6000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Nobre</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -6000<BR><BR>
Você vem de uma família nobre. Começa com mais dinheiro e possui um anel de família que futuramente poderá dar regalias.
</BASEFONT>";


        public override int ModifyStartingGold(PlayerMobile pm, int currentGold)
        {
            return currentGold + 1000;
        }
    }
}
