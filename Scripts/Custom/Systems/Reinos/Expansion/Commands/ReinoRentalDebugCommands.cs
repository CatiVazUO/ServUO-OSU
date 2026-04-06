using Server;
using Server.Commands;
using Server.Custom.Systems.Rent;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoRentalDebugCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReinoShowRentalArea", AccessLevel.GameMaster, new CommandEventHandler(OnShowRentalArea));
          //  CommandSystem.Register("ReinoDeleteRentalByName", AccessLevel.GameMaster, new CommandEventHandler(OnDeleteRentalByName));
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
        /*
        private static void OnDeleteRentalByName(CommandEventArgs e)
        {
            string search = e.ArgString == null ? String.Empty : e.ArgString.Trim();

            if (String.IsNullOrWhiteSpace(search))
            {
                e.Mobile.SendMessage("Use: [ReinoDeleteRentalByName <nome da placa, nome do dono ou nome do túmulo>");
                return;
            }

            ArrayList signs = TownHouseSign.AllSigns;
            List<TownHouseSign> matches = new List<TownHouseSign>();

            for (int i = 0; i < signs.Count; i++)
            {
                TownHouseSign sign = signs[i] as TownHouseSign;
                if (sign == null || sign.Deleted)
                    continue;

                string signName = sign.Name ?? String.Empty;
                string ownerName = (sign.House != null && sign.House.Owner != null) ? sign.House.Owner.Name ?? String.Empty : String.Empty;
                string tombName = sign.IsTomb ? sign.GetTombDisplayName() ?? String.Empty : String.Empty;

                if (ContainsInsensitive(signName, search) || ContainsInsensitive(ownerName, search) || ContainsInsensitive(tombName, search))
                    matches.Add(sign);
            }

            if (matches.Count == 0)
            {
                e.Mobile.SendMessage("Nenhuma propriedade encontrada com esse nome.");
                return;
            }

            if (matches.Count > 1)
            {
                e.Mobile.SendMessage("Foram encontradas {0} propriedades. Seja mais específico.", matches.Count);

                for (int i = 0; i < matches.Count && i < 10; i++)
                {
                    TownHouseSign sign = matches[i];
                    string signName = sign.Name ?? "-";
                    string ownerName = (sign.House != null && sign.House.Owner != null) ? sign.House.Owner.Name ?? "-" : "-";
                    string tombName = sign.IsTomb ? sign.GetTombDisplayName() ?? "-" : "-";

                    e.Mobile.SendMessage("#{0}: placa='{1}' dono='{2}' túmulo='{3}' loc={4}", i + 1, signName, ownerName, tombName, sign.Location);
                }

                return;
            }

            TownHouseSign target = matches[0];
            DeleteRental(target, e.Mobile);
        }

        private static bool ContainsInsensitive(string value, string search)
        {
            if (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(search))
                return false;

            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void DeleteRental(TownHouseSign sign, Mobile from)
        {
            if (sign == null || sign.Deleted)
            {
                from.SendMessage("A placa já não existe.");
                return;
            }

            TownHouse house = sign.House;

            string signName = sign.Name ?? "-";
            string ownerName = (house != null && house.Owner != null) ? house.Owner.Name ?? "-" : "-";
            string tombName = sign.IsTomb ? sign.GetTombDisplayName() ?? "-" : "-";

            Item hiddenHouseSign = null;

            if (house != null && house.Sign != null && !house.Sign.Deleted)
                hiddenHouseSign = house.Sign as Item;

            if (hiddenHouseSign != null && !hiddenHouseSign.Deleted)
                hiddenHouseSign.Delete();

            if (house != null && !house.Deleted)
                house.Delete();

            if (!sign.Deleted)
                sign.Delete();

            from.SendMessage("Propriedade apagada. Placa='{0}' Dono='{1}' Túmulo='{2}'", signName, ownerName, tombName);
        }
        */
    }
}
