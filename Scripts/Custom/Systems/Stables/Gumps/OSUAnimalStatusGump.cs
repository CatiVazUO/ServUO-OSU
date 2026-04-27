using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Custom.Systems.Stables.Engine;

namespace Server.Custom.Systems.Stables.Gumps
{
    public class OSUAnimalStatusGump : Gump
    {
        public OSUAnimalStatusGump(Mobile from, BaseCreature pet) : base(0, 0)
        {
            OSUStablePetSystem.EnsureInitialized(pet);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddStableFrame();

            AddPage(1);

            string petName = pet != null && !String.IsNullOrWhiteSpace(pet.Name) ? pet.Name : "Nome do Animal";
            AddLabel(390, 87, 1152, Safe(petName));
            AddHtml(209, 157, 158, 318, BuildAnimalInfoHtml(pet), false, false);
            AddHtml(398, 157, 250, 318, BuildAnimalInfoXHtml(pet), false, false);


        }

        private void AddStableFrame()
        {
            AddPage(0);
            AddImageTiled(171, 71, 520, 440, 388);
            AddImageTiled(172, 47, 523, 29, 634);
            AddImageTiled(143, 75, 37, 424, 635);
            AddImageTiled(676, 77, 37, 428, 635);
            AddImageTiled(171, 494, 518, 29, 634);
            AddImage(134, 38, 1361);
            AddImage(665, 38, 1361);
            AddImage(664, 483, 1361);
            AddImage(133, 484, 1361);
            AddImage(177, 110, 464);
        }
             private static string BuildAnimalInfoXHtml(BaseCreature pet)
        {
            if (pet == null)
                return "<BASEFONT COLOR=#FFFFFF>Animal inválido.</BASEFONT>";

            return "<BASEFONT COLOR=#FFFFFF>" +
                Safe(pet.Name) + "<BR><BR>" +
                pet.RawStr + " <BR>" +
                pet.RawDex + " <BR>" +
                pet.RawInt + " <BR>" +
                pet.OSUPetLevel + " <BR>" +
                pet.OSUPetXP + "/" + pet.OSUPetNextLevelXP + "<BR>" +
                pet.Loyalty + "/" + BaseCreature.MaxLoyalty + " (" + OSUStablePetSystem.GetLoyaltyLabel(pet) + ")<BR>" +
                (pet.OSUPetLastGainStr + pet.OSUPetLastGainDex + pet.OSUPetLastGainInt) + "<BR>" +
                (String.IsNullOrWhiteSpace(pet.OSUPetAbilitySlot5) ? "nenhuma" : Safe(pet.OSUPetAbilitySlot5)) + "<BR>" +
                (String.IsNullOrWhiteSpace(pet.OSUPetAbilitySlot10) ? "nenhuma" : Safe(pet.OSUPetAbilitySlot10)) + "<BR>" +
                (pet.OSUPetMarked ? "sim" : "não") + "<BR>" +
                (pet.OSUPetCastrated ? "sim" : "não") + "<BR>" +
                pet.OSUPetLivesRemaining + "/" + pet.OSUPetLivesMax + "<BR>" +
                pet.OSUPetBreedCount + "/" + pet.OSUPetBreedCountMax + "<BR>" +
                GetServiceLabel(pet) +
                "</BASEFONT>";
        }
        private static string BuildAnimalInfoHtml(BaseCreature pet)
        {
            if (pet == null)
                return "<BASEFONT COLOR=#FFFFFF>Animal inválido.</BASEFONT>";

            return "<BASEFONT COLOR=#FFFFFF>" +
                "<B>Nome:</B> <BR>" +
                "<B>Atributos:</B> <BR>" +
                "STR - <BR>" +
                "DEX - <BR>" +
                "INT - <BR>" +
                "<B>Nível:</B> <BR>" +
                "<B>XP para o próximo:</B> <BR>" +
                "<B>Lealdade:</B> <BR>" +
                "<B>Pontos redistribuíveis:</B> <BR>" +
                "<B>Habilidade lvl 5:</B> <BR>" +
                "<B>Habilidade lvl 10:</B> <BR>" +
                "<B>Marcado:</B> <BR>" +
                "<B>Castrado:</B><BR>" +
                "<B>Vidas:</B><BR>" +
                "<B>Cruzamentos:</B> <BR>" +
                "<B>Serviço atual:</B>" +
                "</BASEFONT>";
        }

        private static string GetServiceLabel(BaseCreature pet)
        {
            if (pet == null || pet.OSUPetServiceKind == 0)
                return "nenhum";

            string kind = ((OSUStableServiceKind)pet.OSUPetServiceKind).ToString();

            if (pet.OSUPetServiceReadyUtc != DateTime.MinValue)
            {
                TimeSpan wait = pet.OSUPetServiceReadyUtc - DateTime.UtcNow;
                if (wait.TotalSeconds > 0)
                    return kind + " — pronto em " + FormatTime(wait);
            }

            return kind + " — pronto para retirada";
        }

        private static string FormatTime(TimeSpan time)
        {
            if (time.TotalDays >= 1.0)
                return ((int)Math.Ceiling(time.TotalDays)) + " dia(s)";

            if (time.TotalHours >= 1.0)
                return ((int)Math.Ceiling(time.TotalHours)) + " hora(s)";

            if (time.TotalMinutes >= 1.0)
                return ((int)Math.Ceiling(time.TotalMinutes)) + " minuto(s)";

            return Math.Max(1, (int)Math.Ceiling(time.TotalSeconds)) + " segundo(s)";
        }

        private static string Safe(string text)
        {
            if (text == null)
                return String.Empty;

            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
