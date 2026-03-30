using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Hidratado : OSUDefQualDefinition
    {
        public override string Id => "hidratado";
        public override string Name => "Hidratado";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -3000;
        public override string[] BlocksIds => new[] { "sedento" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Hidratado</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -3000<BR><BR>
Seu corpo tolera melhor a falta de água. Você sente <B>sede</B> mais devagar.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Sedento</B>.
</BASEFONT>";


        public override double ModifyThirstRate(PlayerMobile pm, double current)
        {
            return current * 0.80;
        }
    }
}
