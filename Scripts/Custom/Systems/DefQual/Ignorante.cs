using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Ignorante : OSUDefQualDefinition
    {
        public override string Id => "ignorante";
        public override string Name => "Ignorante";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +7000;
        public override string[] BlocksIds => new[] { "genio" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Ignorante</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você aprende com mais dificuldade. Esse defeito faz com que seu <B>Cap máximo de INT</B> diminua em <B>-10</B>, só podendo chegar até <B>105</B>. Você também ganha skills mais devagar.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Gênio</B>.
</BASEFONT>";

public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Int)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Int)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }
public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (!base.CanBePurchased(ctx, alreadySelected, out reason))
                return false;

            if (ctx != null && ctx.Attr_Int > 105)
            {
                reason = "Você não pode comprar Ignorante porque o atributo correspondente já foi definido acima de 105.";
                return false;
            }

            reason = null;
            return true;
        }
public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUIntCapMax > 105)
                pm.OSUIntCapMax = 105;

            if (pm.RawInt > pm.OSUIntCapMax)
                pm.RawInt = pm.OSUIntCapMax;
        }

        public override double ModifySkillGainScalar(PlayerMobile pm, double current)
        {
            return current * 0.85;
        }
    }
}
