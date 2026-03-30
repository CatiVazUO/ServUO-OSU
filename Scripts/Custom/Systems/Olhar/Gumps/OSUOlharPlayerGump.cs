using System;
using Server;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Custom.Systems.Olhar.Gumps
{
    public class OSUOlharPlayerGump : Gump
    {
        private readonly Mobile _viewer;
        private readonly PlayerMobile _target;

        public OSUOlharPlayerGump(Mobile viewer, PlayerMobile target) : base(0, 0)
        {
            _viewer = viewer;
            _target = target;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            // Segurança
            if (_target == null || _target.Deleted)
                return;

            int age = Server.Custom.Systems.WorldTime.OSUAgeHelper.GetCurrentAge(_target);
            int height = _target.OSURpHeightCm;
            int weight = _target.OSURpWeightKg;
            int avatarId = _target.OSUAvatarId;
            string traits = _target.OSURpTraitsPublic ?? "";

            AddPage(0);

            // ======= Layout baseado no seu gump "olhar" =======
            AddImageTiled(226, 93, 551, 440, 376);
            AddImage(214, 457, 359);
            AddImage(717, 457, 360);
            AddImage(717, 84, 361);
            AddImage(214, 84, 362);
            AddImageTiled(285, 516, 433, 30, 367);
            AddImageTiled(286, 86, 431, 30, 368);
            AddImageTiled(218, 169, 27, 290, 365);
            AddImageTiled(762, 167, 26, 292, 366);
            AddImageTiled(255, 148, 184, 21, 469);
            AddImageTiled(562, 148, 183, 21, 470);

            AddLabel(480, 128, 0, "Olhar");

            // Avatar (se tiver)
            if (avatarId > 0)
            {
                // No seu sistema o avatarId é o ID da imagem (ex: 667..986)
                AddImage(267, 179, avatarId);
            }

            AddLabel(287, 398, 0, $"Idade: {age} anos");
            AddLabel(287, 431, 0, $"Altura: {height} cm");
            AddLabel(287, 467, 0, $"Peso: {weight} kg");

            // Caixa do texto (traços/personalidade)
            AddHtml(498, 177, 236, 309, $"<BASEFONT COLOR=#FFFFFF>{traits}</BASEFONT>", false, true);
        }
    }
}
