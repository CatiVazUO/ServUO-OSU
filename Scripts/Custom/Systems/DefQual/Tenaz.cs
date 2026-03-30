using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Tenaz : OSUDefQualDefinition
    {
        public override string Id => "tenaz";
        public override string Name => "Tenaz";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Tenaz</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Você se recompõe mais depressa quando perde os sentidos. Quando <B>desmaia</B>, passa menos tempo desacordado.
</BASEFONT>";


        public override int ModifyUnconsciousDurationSeconds(PlayerMobile pm, int currentSeconds)
        {
            return (int)Math.Round(currentSeconds * 0.80);
        }
    }
}
