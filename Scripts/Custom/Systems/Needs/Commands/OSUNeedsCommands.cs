using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.Needs.Gumps;

namespace Server.Custom.Systems.Needs.Commands
{
    public static class OSUNeedsCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("fomesede", AccessLevel.Player, OnNeeds);
            CommandSystem.Register("needs", AccessLevel.Player, OnNeeds);
        }

        private static void OnNeeds(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            if (pm.HasGump(typeof(OSUNeedsGump)))
                pm.CloseGump(typeof(OSUNeedsGump));

            pm.SendGump(new OSUNeedsGump(pm));
        }
    }
}
