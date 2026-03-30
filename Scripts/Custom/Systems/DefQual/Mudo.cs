using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Mudo : OSUDefQualDefinition
    {
        public override string Id => "mudo";
        public override string Name => "Mudo";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +4000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Mudo</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +4000<BR><BR>
Você não consegue se expressar pela fala comum. Tudo o que tentar dizer em <B>say</B>, <B>whisper</B> ou <B>yell</B> não será ouvido por ninguém.
</BASEFONT>";

        public override bool BlocksOwnSpeech(PlayerMobile pm)
        {
            return true;
        }
    }
}
