using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Debilitado : OSUDefQualDefinition
    {
        public override string Id => "debilitado";
        public override string Name => "Debilitado";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Debilitado</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu organismo é mais frágil contra enfermidades. Você é mais suscetível a <B>doenças</B>.
</BASEFONT>";


        public override double ModifyDiseaseSusceptibility(PlayerMobile pm, double current)
        {
            return current * 1.10;
        }
    }
}
