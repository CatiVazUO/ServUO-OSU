using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Voraz : OSUDefQualDefinition
    {
        public override string Id => "voraz";
        public override string Name => "Voraz";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +3000;
        public override string[] BlocksIds => new[] { "frugal" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Voraz</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +3000<BR><BR>
Seu metabolismo consome alimento mais depressa. Você sente <B>fome</B> mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Frugal</B>.
</BASEFONT>";


        public override double ModifyHungerRate(PlayerMobile pm, double current)
        {
            return current * 1.20;
        }
    }
}
