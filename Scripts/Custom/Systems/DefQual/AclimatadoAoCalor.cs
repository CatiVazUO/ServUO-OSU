using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class AclimatadoAoCalor : OSUDefQualDefinition
    {
        public override string Id => "aclimatado_calor";
        public override string Name => "Aclimatado ao Calor";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "sensivel_calor" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Aclimatado ao Calor</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Seu corpo tolera melhor o calor. Os danos do <B>sistema de clima quente</B> causam <B>10% menos dano</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Sensível ao Calor</B>.
</BASEFONT>";


        public override int ModifyHeatClimateDamage(PlayerMobile pm, int currentDamage)
        {
            return Math.Max(0, (int)Math.Round(currentDamage * 0.90));
        }

        public override double ModifyHeatSusceptibility(PlayerMobile pm, double current)
        {
            return current * 0.90;
        }
    }
}
