using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom
{
    public enum OSUDeathAttribution
    {
        None,
        Player,
        Mob,
        Mixed
    }

    public static class OSUPvpRules
    {
        public const double PlayerDeathThreshold = 0.70;
        public const double MobDeathThreshold = 0.70;
        public const double RecentPlayerPressureThreshold = 0.30;

        public static readonly TimeSpan RecentPlayerPressureWindow = TimeSpan.FromSeconds(30);

        public static bool IsPvp(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            return pm != null && pm.OSUIsPvpChar;
        }

        public static Mobile ResolveDamageSource(Mobile from)
        {
            if (from == null)
                return null;

            if (from is PlayerMobile)
                return from;

            BaseCreature bc = from as BaseCreature;

            if (bc != null)
            {
                if (bc.ControlMaster is PlayerMobile)
                    return bc.ControlMaster;

                if (bc.SummonMaster is PlayerMobile)
                    return bc.SummonMaster;

                Mobile master = bc.GetMaster();

                if (master is PlayerMobile)
                    return master;
            }

            return from;
        }

        public static bool CanLootCorpse(Mobile looter, Corpse corpse)
        {
            if (looter == null || corpse == null)
                return false;

            if (looter.AccessLevel >= AccessLevel.GameMaster)
                return true;

            PlayerMobile looterPm = looter as PlayerMobile;
            PlayerMobile ownerPm = corpse.Owner as PlayerMobile;

            if (ownerPm == null)
                return true;

            if (looter == ownerPm)
                return true;

            if (looterPm == null)
                return false;

            // PvP looteia qualquer corpo de jogador.
            if (looterPm.OSUIsPvpChar)
                return true;

            // Não-PvP só looteia corpo de PvP.
            return ownerPm.OSUIsPvpChar;
        }
    }
}
