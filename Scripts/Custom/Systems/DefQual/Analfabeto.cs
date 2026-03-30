using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.DefQual
{
    public class Analfabeto : OSUDefQualDefinition
    {
        public override string Id => "analfabeto";
        public override string Name => "Analfabeto";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +6000;

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Analfabeto</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +6000<BR><BR>
Você não consegue ler nem escrever, independentemente da Int, e também não pode comprar habilidades de língua.
</BASEFONT>";


        public override bool CanReadAndWrite(PlayerMobile pm)
        {
            return false;
        }

        public override bool CanBuyLanguageSkills(PlayerMobile pm)
        {
            return false;
        }
    }
}
