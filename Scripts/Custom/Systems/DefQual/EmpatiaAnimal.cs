using System;
using System.Collections.Generic;
using Server;
using Server.Custom.Systems.Creation.Engine;

namespace Server.Custom.Systems.DefQual
{
    public class EmpatiaAnimal : OSUDefQualDefinition
    {
        public override string Id => "EmpatiaAnimal";
        public override string Name => "Empatia Animal";
        public override OSUDefQualType Type => OSUDefQualType.Quality;

        // Defeito dá “bônus” de cap (aumenta o cap maior)
        // Você escreveu “Custo: -7000”, mas como é defeito (bonus), aqui precisa ser +7000.
        public override int CapDelta => +7000;

        // Bloqueia "Robusto"
        public override string[] BlocksIds => new[] { "" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Empatia Animal</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +7000<BR><BR>
Você é empático com os animais e eles sentem isso. Com essa qualidade você pode andar entre os animais, sem ser atacado por eles, mesmo os mais agressivos<BR><BR>
<B>Bloqueios:</B> Se seu char for PVP
</BASEFONT>";

        public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
        {
            if (ctx.GameMode != OSUCreationGameMode.NoPvp)
            {
                reason = "Você só pode comprar isso se seu personagem for Não-PVP.";
                return false;
            }

            reason = null;
            return true;
        }

        public override void ApplyEffects(object player, OSUCreationContext ctx)
        {
            // Placeholder: quando o jogador entrar pelo portal, aqui você vai aplicar:
            // - reduzir cap de HP em 10, max 105
            // - aumentar chance/quebra de ossos etc.
        }
    }
}
