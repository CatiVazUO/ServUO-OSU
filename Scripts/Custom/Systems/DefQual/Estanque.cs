using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Estanque : OSUDefQualDefinition
    {
        public override string Id => "estanque";
        public override string Name => "Estanque";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "hemofilico" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Estanque</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Seu corpo lida melhor com ferimentos abertos. Os <B>ticks de bleed</B> causam <B>10% menos dano</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Hemofílico</B>.
</BASEFONT>";


        public override int ModifyBleedTickDamage(PlayerMobile pm, int currentDamage)
        {
            return Math.Max(0, (int)Math.Round(currentDamage * 0.90));
        }

        public override double ModifyBleedSusceptibility(PlayerMobile pm, double current)
        {
            return current * 0.90;
        }
    }
}
