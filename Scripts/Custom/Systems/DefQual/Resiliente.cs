using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Resiliente : OSUDefQualDefinition
    {
        public override string Id => "resiliente";
        public override string Name => "Resiliente";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "preguicoso" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Resiliente</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Seu corpo recupera o fôlego com mais rapidez. Você possui <B>regen de stamina</B> mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Preguiçoso</B>.
</BASEFONT>";


        public override double ModifyStamRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 0.85;
        }
    }
}
