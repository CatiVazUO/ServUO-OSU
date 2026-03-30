using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Robusto : OSUDefQualDefinition
    {
        public override string Id => "robusto";
        public override string Name => "Robusto";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -7000;
        public override string[] BlocksIds => new[] { "fragil" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Robusto</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -7000<BR><BR>
Você é mais resistente que uma pessoa comum. Essa qualidade aumenta o seu <B>Cap máximo de HP</B> em <B>+10</B>, permitindo chegar até <B>125</B>. Você também se recupera um pouco melhor.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Frágil</B>.
</BASEFONT>";

public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUHpCapMax < 125)
                pm.OSUHpCapMax = 125;

            if (pm.OSUBaseHP > pm.OSUHpCapMax)
                pm.OSUBaseHP = pm.OSUHpCapMax;
        }
public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.HP)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.HP)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override double ModifyHitsRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 0.95;
        }
    }
}
