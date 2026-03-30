using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Devoto : OSUDefQualDefinition
    {
        public override string Id => "devoto";
        public override string Name => "Devoto";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -4000;
        public override string[] BlocksIds => new[] { "descrente" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Devoto</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -4000<BR><BR>
Sua fé é mais intensa. Você recebe bônus maiores das <B>shrines</B> do seu deus.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Descrente</B>.
</BASEFONT>";


        public override double ModifyShrineBlessingScalar(PlayerMobile pm, double current)
        {
            return current * 1.15;
        }
    }
}
