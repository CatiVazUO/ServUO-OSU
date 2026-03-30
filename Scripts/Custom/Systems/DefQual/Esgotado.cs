using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Esgotado : OSUDefQualDefinition
    {
        public override string Id => "esgotado";
        public override string Name => "Esgotado";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "mistico" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Esgotado</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Sua energia espiritual demora mais a retornar. Você possui <B>regen de mana</B> mais lento.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Místico</B>.
</BASEFONT>";


        public override double ModifyManaRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 1.15;
        }
    }
}
