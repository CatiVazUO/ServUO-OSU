using System;
using System.Collections.Generic;
using Server;
using Server.Custom.Systems.Creation.Engine;

namespace Server.Custom.Systems.DefQual
{
    /*   public class Template : OSUDefQualDefinition
       {
           public override string Id => "NomeId";
           public override string Name => "Nome da skill que fica no gump";
           public override OSUDefQualType Type => OSUDefQualType.Defect ou OSUDefQualType.Quality;

           // Defeito dá “bônus” de cap (aumenta o cap maior)
           // Custo:  negativo para qualidades e positivo para defeitos
           public override int CapDelta => valor da skill;

           // Bloqueia e outro def/qual
           public override string[] BlocksIds => new[] { "NomeId de outro def/qual" };

           public override string DescriptionHtml =>
       @"<BASEFONT COLOR=#FFFFFF>
       <CENTER><B>Nome da Skill</B></CENTER><BR><BR>
       <B>Bônus de Cap:</B> Custo <BR><BR>
       Descrição <BR><BR>
       <B>Bloqueios:</B> Caso haja bloqueios <B>nome do bloqueio</B>.
       </BASEFONT>";


       //Aqui sao pra defeitos e qualidades q impedem atributos
           public override int GetAttributeMax(OSUCreationContext ctx, OSUCreationAttribute attr, int currentMax)
           {
               if (attr == OSUCreationAttribute.HP)
                   return currentMax > 105 ? 105 : currentMax;

               return currentMax;
           }


       //Aqui efetuas as restrições de compra

           public override bool CanBePurchased(OSUCreationContext ctx, HashSet<string> alreadySelected, out string reason)
           {
               // So pode ser comprada por char nao pvp

               if (ctx.GameMode != OSUCreationGameMode.NoPvp)
               {
                   reason = "Você só pode comprar isso se seu personagem for Não-PVP.";
                   return false;
               }

               //ou So pode ser comprada por char pvp

               if (ctx.GameMode == OSUCreationGameMode.Pvp)
               {
                   reason = "Você não pode comprar isso se seu personagem for PVP.";
                   return false;
               }

               // so pode ser comprar por warriors

               if (ctx.Path != OSUCreationPath.Warrior)
               {
                   reason = "Você só pode comprar isso se seu Caminho for Guerreiro.";
                   return false;
               }


               // so pode ser comprada por artesões

               if (ctx.Path == OSUCreationPath.Artisan)
               {
                   reason = "Você não pode comprar isso se seu Caminho for Artesão.";
                   return false;
               }


               // não pode comprar se algum atributo estiver estourando capacidade
               if (ctx != null && ctx.Attr_HP > 105)
               {
                   reason = "Você não pode comprar X porque seu Atributo já foi definido acima da capacidade.";
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
     */
}

