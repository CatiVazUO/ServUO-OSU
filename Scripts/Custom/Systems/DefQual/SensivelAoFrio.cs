using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class SensivelAoFrio : OSUDefQualDefinition
    {
        public override string Id => "sensivel_frio";
        public override string Name => "Sensível ao Frio";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "aclimatado_frio" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Sensível ao Frio</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu corpo sofre mais com o frio. Você é mais suscetível a <B>dano de frio</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Aclimatado ao Frio</B>.
</BASEFONT>";


        public override double ModifyColdSusceptibility(PlayerMobile pm, double current)
        {
            return current * 1.10;
        }
    }
}
