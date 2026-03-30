using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Fraco : OSUDefQualDefinition
    {
        public override string Id => "fraco";
        public override string Name => "Fraco";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +7000;
        public override string[] BlocksIds => new[] { "forte" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Fraco</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você possui menos força física que o normal. Esse defeito faz com que seu <B>Cap máximo de STR</B> diminua em <B>-10</B>, só podendo chegar até <B>105</B>. Você também pode carregar menos peso.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Forte</B>.
</BASEFONT>";

public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Str)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Str)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }
public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (!base.CanBePurchased(ctx, alreadySelected, out reason))
                return false;

            if (ctx != null && ctx.Attr_Str > 105)
            {
                reason = "Você não pode comprar Fraco porque o atributo correspondente já foi definido acima de 105.";
                return false;
            }

            reason = null;
            return true;
        }
public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUStrCapMax > 105)
                pm.OSUStrCapMax = 105;

            if (pm.RawStr > pm.OSUStrCapMax)
                pm.RawStr = pm.OSUStrCapMax;
        }

        public override int ModifyMaxWeight(PlayerMobile pm, int current)
        {
            return current - 50;
        }
    }
}
