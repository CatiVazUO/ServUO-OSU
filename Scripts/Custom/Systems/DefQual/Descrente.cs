using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Descrente : OSUDefQualDefinition
    {
        public override string Id => "descrente";
        public override string Name => "Descrente";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "devoto" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Descrente</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Você tem pouca ou nenhuma fé. Recebe bônus menores das <B>shrines</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Devoto</B>.
</BASEFONT>";


        public override double ModifyShrineBlessingScalar(PlayerMobile pm, double current)
        {
            return current * 0.85;
        }
    }
}
