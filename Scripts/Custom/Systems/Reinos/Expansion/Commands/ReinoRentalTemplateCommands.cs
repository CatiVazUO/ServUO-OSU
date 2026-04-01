using Server;
using Server.Commands;
using Server.Custom.Systems.Rent;
using Server.Items;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoRentalTemplateCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReinoDumpRentalTemplate", AccessLevel.GameMaster, OnDumpRentalTemplate);
        }

        private static void OnDumpRentalTemplate(CommandEventArgs e)
        {
            string templateId = e.Arguments != null && e.Arguments.Length > 0 ? e.Arguments[0] : "casa01";
            e.Mobile.SendMessage("Clique no ITEM do multi pronto de teste, não no piso.");
            e.Mobile.Target = new AnchorTarget(templateId);
        }

        private class AnchorTarget : Target
        {
            private readonly string _templateId;

            public AnchorTarget(string templateId) : base(20, true, TargetFlags.None)
            {
                _templateId = templateId;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Point3D anchor;

                Item item = targeted as Item;
                if (item != null)
                {
                    anchor = item.Location;
                }
                else
                {
                    IPoint3D p = targeted as IPoint3D;
                    if (p == null)
                    {
                        from.SendMessage("Alvo inválido.");
                        return;
                    }

                    anchor = new Point3D(p);
                }

                from.SendMessage("Agora clique na placa de aluguel já configurada.");
                from.Target = new SignTarget(_templateId, anchor);
            }
        }

        private class SignTarget : Target
        {
            private readonly string _templateId;
            private readonly Point3D _anchor;

            public SignTarget(string templateId, Point3D anchor) : base(20, false, TargetFlags.None)
            {
                _templateId = templateId;
                _anchor = anchor;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                TownHouseSign sign = targeted as TownHouseSign;
                if (sign == null)
                {
                    from.SendMessage("Isso não é uma TownHouseSign.");
                    return;
                }

                try
                {
                    string dir = Path.Combine(Core.BaseDirectory, "Data", "ReinoRentalExports");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    string file = Path.Combine(dir, String.Format("{0}_{1}.txt", _templateId, sign.Serial.Value));
                    File.WriteAllText(file, BuildSnippet(sign, _templateId, _anchor));
                    from.SendMessage("Template exportado para: {0}", file);
                }
                catch (Exception ex)
                {
                    from.SendMessage("Erro ao exportar: {0}", ex.Message);
                }
            }
        }

        private static string BuildSnippet(TownHouseSign sign, string templateId, Point3D anchor)
        {
            StringBuilder sb = new StringBuilder();
            Point3D exportBanLoc = GetExportBanLoc(sign);

            sb.AppendLine("new ReinoRentalTemplate");
            sb.AppendLine("{");
            sb.AppendLine("    TemplateId = \"" + Escape(templateId) + "\",");
            sb.AppendLine("    DisplayName = \"" + Escape(sign.Name) + "\",");
            sb.AppendLine("    PropertyType = OSUPropertyType." + sign.PropertyType + ",");
            sb.AppendLine("    GroupTag = \"Residential\",");
            sb.AppendLine(String.Format("    SignOffset = new Point3D({0}, {1}, {2}),", sign.X - anchor.X, sign.Y - anchor.Y, sign.Z - anchor.Z));
            sb.AppendLine(String.Format("    BanLocOffset = new Point3D({0}, {1}, {2}),", exportBanLoc.X - anchor.X, exportBanLoc.Y - anchor.Y, exportBanLoc.Z - anchor.Z));

            sb.AppendLine("    BlockOffsets = new ReinoRentalRectOffset[]");
            sb.AppendLine("    {");

            ArrayList blocks = sign.Blocks;
            if (blocks != null)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    Rectangle2D rect = (Rectangle2D)blocks[i];

                    sb.AppendLine(String.Format(
                        "        new ReinoRentalRectOffset({0}, {1}, {2}, {3}),",
                        rect.Start.X - anchor.X,
                        rect.Start.Y - anchor.Y,
                        rect.Width,
                        rect.Height));
                }
            }

            sb.AppendLine("    },");
            AppendDoorTemplates(sb, sign, anchor);
            sb.AppendLine(String.Format("    MinZOffset = {0},", sign.MinZ - anchor.Z));
            sb.AppendLine(String.Format("    MaxZOffset = {0},", sign.MaxZ - anchor.Z));
            sb.AppendLine(String.Format("    Lockdowns = {0},", sign.Locks));
            sb.AppendLine(String.Format("    Secures = {0},", sign.Secures));
            sb.AppendLine(String.Format("    DefaultPrice = {0},", sign.Price));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "    DefaultRentByTime = TimeSpan.FromDays({0:0.0}),", sign.RentByTime.TotalDays));
            sb.AppendLine("    DefaultAllowedCulturesCsv = \"" + Escape(GetAllowedCultureCsv(sign)) + "\",");
            sb.AppendLine("    GovernorManaged = true,");
            sb.AppendLine("    StartConfigured = false");
            sb.AppendLine("};");

            return sb.ToString();
        }

        private static Point3D GetExportBanLoc(TownHouseSign sign)
        {
            if (sign == null)
                return Point3D.Zero;

            Point3D ban = sign.BanLoc;

            if (ban == Point3D.Zero)
                return sign.Location;

            int dx = Math.Abs(ban.X - sign.X);
            int dy = Math.Abs(ban.Y - sign.Y);

            if (dx > 50 || dy > 50)
                return sign.Location;

            return ban;
        }

        private static string GetAllowedCultureCsv(TownHouseSign sign)
        {
            ReinoRentalSign reinoSign = sign as ReinoRentalSign;

            if (reinoSign != null && !String.IsNullOrWhiteSpace(reinoSign.AllowedCulturesCsv))
                return reinoSign.AllowedCulturesCsv;

            if (!String.IsNullOrWhiteSpace(sign.AllowedCulture))
                return sign.AllowedCulture;

            return "Todos";
        }

        private static void AppendDoorTemplates(StringBuilder sb, TownHouseSign sign, Point3D anchor)
        {
            List<BaseDoor> doors = CollectDoors(sign);

            sb.AppendLine("    DoorTemplates = new ReinoRentalDoorTemplate[]");
            sb.AppendLine("    {");

            for (int i = 0; i < doors.Count; i++)
            {
                BaseDoor door = doors[i];

                sb.AppendLine(String.Format(
                    "        new ReinoRentalDoorTemplate({0}, {1}, {2}, {3}, {4}, {5}, {6}, new Point3D({7}, {8}, {9})),",
                    door.X - anchor.X,
                    door.Y - anchor.Y,
                    door.Z - anchor.Z,
                    door.ClosedID,
                    door.OpenedID,
                    door.OpenedSound,
                    door.ClosedSound,
                    door.Offset.X,
                    door.Offset.Y,
                    door.Offset.Z));
            }

            sb.AppendLine("    },");
        }

        private static string Escape(string text)
        {
            return (text ?? String.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
        private static List<BaseDoor> CollectDoors(TownHouseSign sign)
        {
            List<BaseDoor> list = new List<BaseDoor>();
            List<int> seen = new List<int>();

            if (sign == null || sign.Map == null || sign.Map == Map.Internal || sign.Blocks == null)
                return list;

            ArrayList blocks = sign.Blocks;

            for (int i = 0; i < blocks.Count; i++)
            {
                Rectangle2D rect = (Rectangle2D)blocks[i];
                IPooledEnumerable eable = sign.Map.GetItemsInBounds(rect);

                foreach (Item item in eable)
                {
                    BaseDoor door = item as BaseDoor;

                    if (door == null || door.Deleted || door.RootParent != null)
                        continue;

                    if (seen.Contains(door.Serial.Value))
                        continue;

                    if (!rect.Contains(new Point2D(door.X, door.Y)))
                        continue;

                    if (door.Z < sign.MinZ || door.Z > sign.MaxZ)
                        continue;

                    seen.Add(door.Serial.Value);
                    list.Add(door);
                }

                eable.Free();
            }

            return list;
        }
    }
}
