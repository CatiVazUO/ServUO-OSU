using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Agil : OSUDefQualDefinition
    {
        public override string Id => "agil";
        public override string Name => "Ágil";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -7000;
        public override string[] BlocksIds => new[] { "desastrado" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Ágil</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -7000<BR><BR>
Você possui movimentos mais precisos e leves. Essa qualidade aumenta o seu <B>Cap máximo de DEX</B> em <B>+10</B>, permitindo chegar até <B>125</B>. Você também recebe bônus ao <B>desarmar armadilhas</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Desastrado</B>.
</BASEFONT>";

public override void ApplyToPlayer(PlayerMobile pm, OSUCreationContext ctx)
        {
            if (pm == null)
                return;

            if (pm.OSUDexCapMax < 125)
                pm.OSUDexCapMax = 125;

            if (pm.RawDex > pm.OSUDexCapMax)
                pm.RawDex = pm.OSUDexCapMax;
        }
public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Dex)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int GetAttributeMax(PlayerMobile pm, OSUCreationAttribute attr, int currentMax)
        {
            if (attr == OSUCreationAttribute.Dex)
                return currentMax > 125 ? 125 : currentMax;

            return currentMax;
        }

        public override int ModifyDisarmTrapBonus(PlayerMobile pm, int current)
        {
            return current + 10;
        }
    }
}
