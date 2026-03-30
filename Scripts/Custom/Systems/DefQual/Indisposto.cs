using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Indisposto : OSUDefQualDefinition
    {
        public override string Id => "indisposto";
        public override string Name => "Indisposto";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +7000;
        public override string[] BlocksIds => new[] { "atletico" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Indisposto</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você se cansa mais facilmente. Esse defeito faz com que seu <B>Cap máximo de STAM</B> diminua em <B>-10</B>, só podendo chegar até <B>105</B>. Você também corre mais devagar.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Atlético</B>.
</BASEFONT>";

public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Vit)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Vit)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }
public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (!base.CanBePurchased(ctx, alreadySelected, out reason))
                return false;

            if (ctx != null && ctx.Attr_Vit > 105)
            {
                reason = "Você não pode comprar Indisposto porque o atributo correspondente já foi definido acima de 105.";
                return false;
            }

            reason = null;
            return true;
        }
public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUStamCapMax > 105)
                pm.OSUStamCapMax = 105;

            if (pm.OSUBaseStam > pm.OSUStamCapMax)
                pm.OSUBaseStam = pm.OSUStamCapMax;
        }

        public override int ModifyRunSpeed(PlayerMobile pm, int current, bool running)
        {
            if (!running)
                return current;

            int value = (int)(current * 1.10);

            return value < 1 ? 1 : value;
        }
    }
}
