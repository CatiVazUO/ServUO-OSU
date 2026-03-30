using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Sedento : OSUDefQualDefinition
    {
        public override string Id => "sedento";
        public override string Name => "Sedento";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +3000;
        public override string[] BlocksIds => new[] { "hidratado" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Sedento</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +3000<BR><BR>
Seu corpo pede líquidos com mais frequência. Você sente <B>sede</B> mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Hidratado</B>.
</BASEFONT>";


        public override double ModifyThirstRate(PlayerMobile pm, double current)
        {
            return current * 1.20;
        }
    }
}
