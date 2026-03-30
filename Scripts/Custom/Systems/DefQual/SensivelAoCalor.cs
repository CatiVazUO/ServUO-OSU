using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class SensivelAoCalor : OSUDefQualDefinition
    {
        public override string Id => "sensivel_calor";
        public override string Name => "Sensível ao Calor";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "aclimatado_calor" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Sensível ao Calor</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu corpo sofre mais com o calor. Você é mais suscetível a <B>dano de calor</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Aclimatado ao Calor</B>.
</BASEFONT>";


        public override double ModifyHeatSusceptibility(PlayerMobile pm, double current)
        {
            return current * 1.10;
        }
    }
}
