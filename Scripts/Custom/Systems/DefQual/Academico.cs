using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.DefQual
{
    public class Academico : OSUDefQualDefinition
    {
        public override string Id => "academico";
        public override string Name => "Acadêmico";
        public override OSUDefQualType Type => OSUDefQualType.Quality;
        public override int CapDelta => -5000;
        public override string[] BlocksIds => new[] { "limitado" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Acadêmico</B></CENTER><BR><BR>
<B>Custo de Cap:</B> -5000<BR><BR>
Você aprende com mais facilidade os fundamentos da sua vocação principal.
Feats da sua <B>classe principal</B> custam <B>10% menos</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Limitado</B>.
</BASEFONT>";


        public override int ModifyFeatCost(PlayerMobile pm, OSUFeatDefinition feat, int current)
        {
            if (pm == null || feat == null)
                return current;

            bool principalEhProfissao = pm.OSUFeatCapsInverted;
            bool featEhPrincipal =
                (principalEhProfissao && feat.Category == OSUFeatCategory.Profissoes) ||
                (!principalEhProfissao && feat.Category == OSUFeatCategory.Combate);

            if (!featEhPrincipal)
                return current;

            int novo = (int)Math.Round(current * 0.90);

            if (novo < 1)
                novo = 1;

            return novo;
        }
    }
}
