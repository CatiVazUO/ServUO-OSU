using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Fragil : OSUDefQualDefinition
    {
        public override string Id => "fragil";
        public override string Name => "Frágil";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +7000;
        public override string[] BlocksIds => new[] { "robusto" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Frágil</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você é um pouco mais frágil que uma pessoa comum. Esse defeito faz com que seu <B>Cap máximo de HP</B> diminua em <B>-10</B>, só podendo chegar até <B>105</B>. Você também se recupera um pouco mais devagar.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Robusto</B>.
</BASEFONT>";

public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.HP)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.HP)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }
public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (!base.CanBePurchased(ctx, alreadySelected, out reason))
                return false;

            if (ctx != null && ctx.Attr_HP > 105)
            {
                reason = "Você não pode comprar Frágil porque o atributo correspondente já foi definido acima de 105.";
                return false;
            }

            reason = null;
            return true;
        }
public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUHpCapMax > 105)
                pm.OSUHpCapMax = 105;

            if (pm.OSUBaseHP > pm.OSUHpCapMax)
                pm.OSUBaseHP = pm.OSUHpCapMax;
        }

        public override double ModifyHitsRegenRate(PlayerMobile pm, double currentSeconds)
        {
            return currentSeconds * 1.05;
        }
    }
}
