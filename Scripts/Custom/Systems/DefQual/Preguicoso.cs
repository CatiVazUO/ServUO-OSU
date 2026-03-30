using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Preguicoso : OSUDefQualDefinition
    {
        public override string Id => "preguicoso";
        public override string Name => "Preguiçoso";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "resiliente" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Preguiçoso</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu corpo demora mais para recuperar o fôlego. Você possui <B>regen de stamina</B> mais lento.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Resiliente</B>.
</BASEFONT>";


        public override double ModifyStamRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 1.15;
        }
    }
}
