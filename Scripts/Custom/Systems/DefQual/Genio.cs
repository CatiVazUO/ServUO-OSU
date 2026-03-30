using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Genio : OSUDefQualDefinition
    {
        public override string Id => "genio";
        public override string Name => "Gênio";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -7000;
        public override string[] BlocksIds => new[] { "ignorante" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Gênio</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -7000<BR><BR>
Sua mente aprende com mais facilidade. Essa qualidade aumenta o seu <B>Cap máximo de INT</B> em <B>+10</B>, permitindo chegar até <B>125</B>. Você também aprende skills mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Ignorante</B>.
</BASEFONT>";

public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUIntCapMax < 125)
                pm.OSUIntCapMax = 125;

            if (pm.RawInt > pm.OSUIntCapMax)
                pm.RawInt = pm.OSUIntCapMax;
        }
public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Int)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Int)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override double ModifySkillGainScalar(PlayerMobile pm, double current)
        {
            return current * 1.15;
        }
    }
}
