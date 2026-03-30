using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Atletico : OSUDefQualDefinition
    {
        public override string Id => "atletico";
        public override string Name => "Atlético";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -7000;
        public override string[] BlocksIds => new[] { "indisposto" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Atlético</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -7000<BR><BR>
Você possui melhor preparo físico. Essa qualidade aumenta o seu <B>Cap máximo de STAM</B> em <B>+10</B>, permitindo chegar até <B>125</B>. Você também corre mais rápido.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Indisposto</B>.
</BASEFONT>";

public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUStamCapMax < 125)
                pm.OSUStamCapMax = 125;

            if (pm.OSUBaseStam > pm.OSUStamCapMax)
                pm.OSUBaseStam = pm.OSUStamCapMax;
        }
public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Vit)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Vit)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int ModifyRunSpeed(PlayerMobile pm, int current, bool running)
        {
            if (!running)
                return current;

            int value = (int)(current * 0.90);

            return value < 1 ? 1 : value;
        }
    }
}
