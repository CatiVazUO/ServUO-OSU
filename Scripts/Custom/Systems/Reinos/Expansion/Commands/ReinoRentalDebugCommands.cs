using System;
using System.Collections;
using Server;
using Server.Commands;
using Server.Custom.Systems.Rent;
using Server.Targeting;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoRentalDebugCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReinoShowRentalArea", AccessLevel.GameMaster, new CommandEventHandler(OnShowRentalArea));
        }

        private static void OnShowRentalArea(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Clique na placa de aluguel.");
            e.Mobile.Target = new RentalAreaTarget();
        }

        private class RentalAreaTarget : Target
        {
            public RentalAreaTarget() : base(20, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                TownHouseSign sign = targeted as TownHouseSign;

                if (sign == null)
                {
                    from.SendMessage("Isso não é uma TownHouseSign.");
                    return;
                }

                from.SendMessage("Nome: {0}", sign.Name);
                from.SendMessage("SignLoc: {0}", sign.SignLoc);
                from.SendMessage("BanLoc: {0}", sign.BanLoc);
                from.SendMessage("MinZ: {0}", sign.MinZ);
                from.SendMessage("MaxZ: {0}", sign.MaxZ);
                from.SendMessage("Blocks: {0}", sign.Blocks != null ? sign.Blocks.Count : 0);

                if (sign.Blocks != null)
                {
                    for (int i = 0; i < sign.Blocks.Count; i++)
                    {
                        Rectangle2D rect = (Rectangle2D)sign.Blocks[i];
                        from.SendMessage("Block {0}: ({1},{2}) -> ({3},{4})  W:{5} H:{6}",
                            i + 1,
                            rect.Start.X,
                            rect.Start.Y,
                            rect.End.X,
                            rect.End.Y,
                            rect.Width,
                            rect.Height);
                    }
                }

                sign.ShowAreaPreview(from);
            }
        }
    }
}
