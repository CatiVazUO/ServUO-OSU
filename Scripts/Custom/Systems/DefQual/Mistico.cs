using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Mistico : OSUDefQualDefinition
    {
        public override string Id => "mistico";
        public override string Name => "Místico";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "esgotado" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Místico</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Sua energia espiritual flui com mais facilidade. Você possui <B>regen de mana</B> mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Esgotado</B>.
</BASEFONT>";


        public override double ModifyManaRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 0.85;
        }
    }
}
