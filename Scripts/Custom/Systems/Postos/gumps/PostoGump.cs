using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Custom.Systems.Postos
{
    public class PostoGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly string _postoId;

        public PostoGump(PlayerMobile from, string postoId) : base(0, 0)
        {
            _from = from;
            _postoId = postoId ?? String.Empty;

            PostoDefinition def = PostoSystem.GetDefinition(_postoId);
            PostoState state = PostoSystem.GetState(_postoId);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(353, 211, 410, 319, 394);
            AddImageTiled(355, 186, 406, 35, 638);
            AddImageTiled(321, 218, 39, 311, 639);
            AddImageTiled(355, 518, 405, 35, 638);
            AddImageTiled(744, 214, 39, 311, 639);
            AddImage(309, 176, 1359);
            AddImage(733, 175, 1359);
            AddImage(732, 509, 1359);
            AddImage(309, 513, 1359);

            if (def == null || state == null)
            {
                AddLabel(430, 225, 0, "Posto inválido");
                AddHtml(371, 277, 362, 132, "<BASEFONT COLOR=#000000>Não foi possível carregar as informações deste posto.", false, false);
                return;
            }

            string progressText = PostoSystem.GetObjectiveProgressText(def, state);
            string htmlText = PostoSystem.BuildMainHtml(_from, def, state);
            string objectiveTarget = def.ObjectiveDisplayName;

            AddLabel(504, 225, 0, "Posto " + def.Name);
            AddImage(352, 240, 443);
            AddLabel(377, 486, 0, objectiveTarget);
            AddLabel(663, 486, 0, progressText);
            AddHtml(371, 277, 362, 132, htmlText, false, false);

            string buttonLabel;
            string reason;
            PostoActionType action = PostoSystem.GetAvailableAction(_from, def, state, out buttonLabel, out reason);

            if (action != PostoActionType.None)
            {
                AddButton(690, 426, 506, 506, (int)action, GumpButtonType.Reply, 0);
                AddHtml(500, 432, 170, 22, buttonLabel, false, false);
            }
            else
            {
                AddHtml(400, 430, 290, 40, reason, false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;

            if (from == null || from.Deleted)
                return;

            string message;

            switch ((PostoActionType)info.ButtonID)
            {
                case PostoActionType.AcceptAgreement:
                    if (PostoSystem.TryAcceptAgreement(from, _postoId, out message))
                        from.SendMessage(message);
                    else
                        from.SendMessage(message);

                    from.CloseGump(typeof(PostoGump));
                    from.SendGump(new PostoGump(from, _postoId));
                    break;

                case PostoActionType.Conquer:
                    if (PostoSystem.TryConquer(from, _postoId, out message))
                        from.SendMessage(message);
                    else
                        from.SendMessage(message);

                    from.CloseGump(typeof(PostoGump));
                    from.SendGump(new PostoGump(from, _postoId));
                    break;
            }
        }
    }
}
