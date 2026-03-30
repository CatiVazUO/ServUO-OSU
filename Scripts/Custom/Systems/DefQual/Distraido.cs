using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Distraido : OSUDefQualDefinition
    {
        public override string Id => "distraido";
        public override string Name => "Distraído";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +7000;
        public override string[] BlocksIds => new[] { "focado" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Distraído</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você perde a concentração com facilidade. Esse defeito faz com que seu <B>Cap máximo de MANA</B> diminua em <B>-10</B>, só podendo chegar até <B>105</B>. Você também medita mais devagar e pode perder a meditação.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Focado</B>.
</BASEFONT>";

public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Mana)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Mana)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }
public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (!base.CanBePurchased(ctx, alreadySelected, out reason))
                return false;

            if (ctx != null && ctx.Attr_Man > 105)
            {
                reason = "Você não pode comprar Distraído porque o atributo correspondente já foi definido acima de 105.";
                return false;
            }

            reason = null;
            return true;
        }
public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUManaCapMax > 105)
                pm.OSUManaCapMax = 105;

            if (pm.OSUBaseMana > pm.OSUManaCapMax)
                pm.OSUBaseMana = pm.OSUManaCapMax;
        }

        public override double ModifyManaRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 1.15;
        }

        public override bool ShouldBreakMeditation(PlayerMobile pm)
        {
            return pm != null && pm.Meditating && Utility.RandomDouble() < 0.08;
        }
    }
}
