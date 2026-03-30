using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class VulneravelAoVeneno : OSUDefQualDefinition
    {
        public override string Id => "vulneravel_veneno";
        public override string Name => "Vulnerável ao Veneno";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "tolerante_veneno" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Vulnerável ao Veneno</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu corpo é mais afetado por venenos. Você é mais suscetível a <B>poison</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Tolerante ao Veneno</B>.
</BASEFONT>";


        public override double ModifyPoisonSusceptibility(PlayerMobile pm, double current)
        {
            return current * 1.10;
        }
    }
}
