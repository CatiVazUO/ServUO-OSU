using System;
using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;
using Server.Custom.Systems.Olhar.Gumps;

namespace Server.Custom.Systems.Olhar.Commands
{
    public static class OSUOlharCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("olhar", AccessLevel.Player, OnOlharCommand);
        }

        private static void OnOlharCommand(CommandEventArgs e)
        {
            var from = e.Mobile;

            if (from == null || from.Deleted)
                return;

            from.SendMessage("Escolha um jogador ou um objeto para olhar.");
            from.Target = new OlharTarget();
        }

        private class OlharTarget : Target
        {
            public OlharTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (from == null || from.Deleted)
                    return;

                // ======== SE CLICAR EM JOGADOR ========
                if (targeted is PlayerMobile pm)
                {
                    // Placeholder do disfarce:
                    if (OSUDisguiseHelper.IsDisguised(pm))
                    {
                        // comportamento futuro (vamos implementar depois)
                        from.SendMessage("Você não consegue identificar claramente essa pessoa (disfarce).");
                        return;
                    }

                    // abre sua ficha RP
                    from.SendGump(new OSUOlharPlayerGump(from, pm));
                    return;
                }

                // ======== SE CLICAR EM ITEM ========
                if (targeted is Item item)
                {
                    string txt = item.OlharTxt;

                    from.SendGump(new OSUOlharObjectGump(from, txt));
                    return;
                }

                // ======== QUALQUER OUTRA COISA ========
                from.SendGump(new OSUOlharObjectGump(from, null));
            }
        }
    }

    // Placeholder pro disfarce (por enquanto sempre falso)
    public static class OSUDisguiseHelper
    {
        public static bool IsDisguised(PlayerMobile pm)
        {
            // FUTURO: aqui você vai checar a skill/sistema de disfarce
            return false;
        }
    }
}
