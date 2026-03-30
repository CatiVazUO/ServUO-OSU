using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Enfermo : OSUDefQualDefinition
    {
        public override string Id => "enfermo";
        public override string Name => "Enfermo";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "saudavel" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Enfermo</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu corpo se recupera mais lentamente. Você possui <B>regen de HP</B> mais lento.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Saudável</B>.
</BASEFONT>";


        public override double ModifyHitsRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 1.15;
        }
    }
}
