using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Desastrado : OSUDefQualDefinition
    {
        public override string Id => "desastrado";
        public override string Name => "Desastrado";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +7000;
        public override string[] BlocksIds => new[] { "agil" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Desastrado</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você é naturalmente desastrado. Esse defeito faz com que seu <B>Cap máximo de DEX</B> diminua em <B>-10</B>, só podendo chegar até <B>105</B>. Você também recebe penalidade ao <B>desarmar armadilhas</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Ágil</B>.
</BASEFONT>";

public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Dex)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Dex)
                return currentMax > 105 ? 105 : currentMax;

            return currentMax;
        }
public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (!base.CanBePurchased(ctx, alreadySelected, out reason))
                return false;

            if (ctx != null && ctx.Attr_Dex > 105)
            {
                reason = "Você não pode comprar Desastrado porque o atributo correspondente já foi definido acima de 105.";
                return false;
            }

            reason = null;
            return true;
        }
public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUDexCapMax > 105)
                pm.OSUDexCapMax = 105;

            if (pm.RawDex > pm.OSUDexCapMax)
                pm.RawDex = pm.OSUDexCapMax;
        }

        public override int ModifyDisarmTrapBonus(PlayerMobile pm, int current)
        {
            return current - 10;
        }
    }
}
