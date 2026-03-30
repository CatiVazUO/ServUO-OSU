using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Frugal : OSUDefQualDefinition
    {
        public override string Id => "frugal";
        public override string Name => "Frugal";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -3000;
        public override string[] BlocksIds => new[] { "voraz" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Frugal</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -3000<BR><BR>
Seu metabolismo consome alimento com mais calma. Você sente <B>fome</B> mais devagar.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Voraz</B>.
</BASEFONT>";


        public override double ModifyHungerRate(PlayerMobile pm, double current)
        {
            return current * 0.80;
        }
    }
}
