using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class ToleranteAoVeneno : OSUDefQualDefinition
    {
        public override string Id => "tolerante_veneno";
        public override string Name => "Tolerante ao Veneno";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "VulneravelAoVeneno" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Tolerante ao Veneno</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Seu corpo possui resistência natural a venenos. Os <B>ticks de poison</B> causam <B>10% menos dano</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Vulnerável ao Veneno</B>.
</BASEFONT>";


        public override int ModifyPoisonTickDamage(PlayerMobile pm, int currentDamage)
        {
            return Math.Max(0, (int)Math.Round(currentDamage * 0.90));
        }

        public override double ModifyPoisonSusceptibility(PlayerMobile pm, double current)
        {
            return current * 0.90;
        }
    }
}
