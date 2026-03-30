using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Saudavel : OSUDefQualDefinition
    {
        public override string Id => "saudavel";
        public override string Name => "Saudável";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "enfermo" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Saudável</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Seu corpo se recupera mais rapidamente. Você possui <B>regen de HP</B> mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Enfermo</B>.
</BASEFONT>";


        public override double ModifyHitsRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 0.85;
        }
    }
}
