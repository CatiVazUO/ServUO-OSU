using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Focado : OSUDefQualDefinition
    {
        public override string Id => "focado";
        public override string Name => "Focado";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -7000;
        public override string[] BlocksIds => new[] { "distraido" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Focado</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -7000<BR><BR>
Você mantém a concentração com facilidade. Essa qualidade aumenta o seu <B>Cap máximo de MANA</B> em <B>+10</B>, permitindo chegar até <B>125</B>. Você também medita com mais eficiência.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Distraído</B>.
</BASEFONT>";

public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUManaCapMax < 125)
                pm.OSUManaCapMax = 125;

            if (pm.OSUBaseMana > pm.OSUManaCapMax)
                pm.OSUBaseMana = pm.OSUManaCapMax;
        }
public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Mana)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Mana)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override double ModifyManaRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 0.90;
        }
    }
}
