using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Forte : OSUDefQualDefinition
    {
        public override string Id => "forte";
        public override string Name => "Forte";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -7000;
        public override string[] BlocksIds => new[] { "fraco" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Forte</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -7000<BR><BR>
Você possui força acima do normal. Essa qualidade aumenta o seu <B>Cap máximo de STR</B> em <B>+10</B>, permitindo chegar até <B>125</B>. Você também pode carregar mais peso.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Fraco</B>.
</BASEFONT>";

public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUStrCapMax < 125)
                pm.OSUStrCapMax = 125;

            if (pm.RawStr > pm.OSUStrCapMax)
                pm.RawStr = pm.OSUStrCapMax;
        }
public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Str)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Str)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int ModifyMaxWeight(PlayerMobile pm, int current)
        {
            return current + 50;
        }
    }
}
