using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.SkillXP.Engine;

namespace Server.Custom.Systems.HtmlBooks.Gumps
{
    public class HtmlLanguageSelectGump : Gump
    {
        private readonly PlayerMobile _pm;
        private readonly HtmlDocumentBase _doc;
        private readonly int _returnPage;
        private readonly int _selectedLine;

        private const int BtnConfirm = 1;

        public HtmlLanguageSelectGump(PlayerMobile pm, HtmlDocumentBase doc, int returnPage, int selectedLine)
            : base(0, 0)
        {
            _pm = pm;
            _doc = doc;
            _returnPage = returnPage;
            _selectedLine = selectedLine;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Moldura / fundo
            AddImageTiled(371, 295, 177, 295, 375);
            AddImageTiled(543, 298, 25, 285, 369);
            AddImageTiled(348, 303, 26, 280, 370);
            AddImageTiled(363, 278, 193, 25, 371);
            AddImageTiled(364, 584, 184, 30, 372);
            AddImage(340, 270, 402);
            AddImage(536, 268, 402);
            AddImage(341, 575, 402);
            AddImage(532, 573, 402);

            AddLabel(403, 306, 0, "Escolha um idioma");
            AddImageTiled(377, 325, 163, 21, 471);

            OSULanguage[] langs = GetSelectableLanguages(pm);

            int startX = 386;
            int startY = 355;
            int rowGap = 27;

            for (int i = 0; i < langs.Length && i < 7; i++)
            {
                int y = startY + (i * rowGap);

                AddRadio(startX, y, 535, 436, langs[i] == doc.Language, 100 + i);
                AddLabel(startX + 35, y, 0, GetDisplayName(langs[i]));
            }

            AddButton(419, 554, 559, 248, BtnConfirm, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_pm == null || _doc == null || _doc.Deleted)
                return;

            if (info.ButtonID != BtnConfirm)
            {
                _pm.SendGump(new HtmlWriteGump(_pm, _doc, _returnPage, _selectedLine));
                return;
            }

            OSULanguage[] langs = GetSelectableLanguages(_pm);

            for (int i = 0; i < langs.Length && i < 7; i++)
            {
                if (info.IsSwitched(100 + i))
                {
                    _doc.Language = langs[i];
                    break;
                }
            }

            _pm.SendGump(new HtmlWriteGump(_pm, _doc, _returnPage, _selectedLine));
        }

        private static OSULanguage[] GetSelectableLanguages(PlayerMobile pm)
        {
            List<OSULanguage> list = new List<OSULanguage>();

            if (pm == null)
                return new OSULanguage[0];

            // 1) idioma da cultura do personagem
            OSULanguage native = LanguageKnowledge.GetNativeLanguageForCulture(pm.OSUCultureId);
            AddIfMissing(list, native);

            // 2) comum se tiver INT >= 40 OU habilidade comprada
            if (pm.Int >= 40 || pm.HasOSUAbility(LanguageAbilityIds.SpeakCommon))
                AddIfMissing(list, OSULanguage.Common);

            // 3) outros idiomas só se habilidade comprada
            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakKamay))
                AddIfMissing(list, OSULanguage.Kamay);

            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakSarang))
                AddIfMissing(list, OSULanguage.Sarang);

            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakMatalun))
                AddIfMissing(list, OSULanguage.Matalun);

            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakZorteros))
                AddIfMissing(list, OSULanguage.Zorteros);

            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakTherok))
                AddIfMissing(list, OSULanguage.Therok);

            if (pm.HasOSUAbility(LanguageAbilityIds.SpeakAludin))
                AddIfMissing(list, OSULanguage.Aludin);

            return list.ToArray();
        }

        private static void AddIfMissing(List<OSULanguage> list, OSULanguage lang)
        {
            if (!list.Contains(lang))
                list.Add(lang);
        }

        private static string GetDisplayName(OSULanguage lang)
        {
            switch (lang)
            {
                case OSULanguage.Common:
                    return "Comum";
                case OSULanguage.Therok:
                    return "Therok";
                case OSULanguage.Kamay:
                    return "Kamay";
                case OSULanguage.Sarang:
                    return "Sarang";
                case OSULanguage.Matalun:
                    return "Matalun";
                case OSULanguage.Zorteros:
                    return "Zorteros";
                case OSULanguage.Aludin:
                    return "Aludin";
                default:
                    return lang.ToString();
            }
        }
    }
}
