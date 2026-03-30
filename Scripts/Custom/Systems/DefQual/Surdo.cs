using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Surdo : OSUDefQualDefinition
    {
        public override string Id => "surdo";
        public override string Name => "Surdo";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Surdo</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Você não consegue ouvir a fala comum dos outros. Mensagens de <B>say</B>, <B>whisper</B> ou <B>yell</B> não chegarão até você.
</BASEFONT>";

        public override bool BlocksHearingSpeech(PlayerMobile listener, PlayerMobile speaker)
        {
            return true;
        }
    }
}
