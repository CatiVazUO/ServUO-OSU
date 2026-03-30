using Server;
using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.DefQual
{
    public class Limitado : OSUDefQualDefinition
    {
        public override string Id => "limitado";
        public override string Name => "Limitado";
        public override OSUDefQualType Type => OSUDefQualType.Defect;
        public override int CapDelta => +5000;
        public override string[] BlocksIds => new[] { "academico" };

        public override string DescriptionHtml =>
@"<BASEFONT COLOR=#FFFFFF>
<CENTER><B>Limitado</B></CENTER><BR><BR>
<B>Bônus de Cap:</B> +5000<BR><BR>
Você tem mais dificuldade para dominar os fundamentos da sua vocação principal.
Feats da sua <B>classe principal</B> custam <B>10% mais</B>.<BR><BR>
<B>Bloqueios:</B> Não pode comprar <B>Acadêmico</B>.
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

            int novo = (int)Math.Round(current * 1.10);

            if (novo < 1)
                novo = 1;

            return novo;
        }

    }
}
