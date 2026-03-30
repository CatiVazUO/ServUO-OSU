using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class PropensoAVicios : OSUDefQualDefinition
    {
        public override string Id => "propenso_vicios";
        public override string Name => "Propenso a Vícios";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Propenso a Vícios</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu organismo e sua mente cedem mais facilmente a dependências. Você é mais suscetível a <B>vícios</B>.
</BASEFONT>";


        public override double ModifyAddictionSusceptibility(PlayerMobile pm, double current)
        {
            return current * 1.15;
        }
    }
}
