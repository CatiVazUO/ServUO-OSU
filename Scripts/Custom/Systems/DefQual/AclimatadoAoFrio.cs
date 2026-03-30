using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class AclimatadoAoFrio : OSUDefQualDefinition
    {
        public override string Id => "aclimatado_frio";
        public override string Name => "Aclimatado ao Frio";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "sensivel_frio" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Aclimatado ao Frio</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Seu corpo tolera melhor o frio. Os danos do <B>sistema de clima frio</B> causam <B>10% menos dano</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Sensível ao Frio</B>.
</BASEFONT>";


        public override int ModifyColdClimateDamage(PlayerMobile pm, int currentDamage)
        {
            return Math.Max(0, (int)Math.Round(currentDamage * 0.90));
        }

        public override double ModifyColdSusceptibility(PlayerMobile pm, double current)
        {
            return current * 0.90;
        }
    }
}
