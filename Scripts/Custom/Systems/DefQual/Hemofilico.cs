using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Hemofilico : OSUDefQualDefinition
    {
        public override string Id => "hemofilico";
        public override string Name => "Hemofílico";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;
        public override string[] BlocksIds => new[] { "estanque" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Hemofílico</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Seu corpo lida pior com sangramentos. Você é mais suscetível a <B>bleed</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Estanque</B>.
</BASEFONT>";


        public override double ModifyBleedSusceptibility(PlayerMobile pm, double current)
        {
            return current * 1.10;
        }
    }
}
