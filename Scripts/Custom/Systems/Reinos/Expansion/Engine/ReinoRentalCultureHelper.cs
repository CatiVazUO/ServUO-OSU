using System;
using System.Collections.Generic;
using Server.Custom.Systems.Culture;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoRentalCultureHelper
    {
        public static List<OSUCultureDefinition> GetCultureOptions()
        {
            try
            {
                return OSUCultureRegistry.GetOrdered(16);
            }
            catch
            {
                return new List<OSUCultureDefinition>();
            }
        }

        public static string NormalizeCsv(string raw)
        {
            if (String.IsNullOrWhiteSpace(raw))
                return "Todos";

            List<string> parts = new List<string>();
            string[] split = raw.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                string value = split[i] != null ? split[i].Trim() : String.Empty;
                if (String.IsNullOrWhiteSpace(value))
                    continue;

                if (String.Equals(value, "Todos", StringComparison.OrdinalIgnoreCase))
                    return "Todos";

                bool exists = false;
                for (int j = 0; j < parts.Count; j++)
                {
                    if (String.Equals(parts[j], value, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    parts.Add(value);
            }

            return parts.Count == 0 ? "Todos" : String.Join(",", parts.ToArray());
        }

        public static bool ContainsCulture(string csv, string cultureId)
        {
            if (String.IsNullOrWhiteSpace(csv) || String.Equals(csv, "Todos", StringComparison.OrdinalIgnoreCase))
                return true;

            if (String.IsNullOrWhiteSpace(cultureId))
                return false;

            string[] split = csv.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                if (String.Equals(split[i].Trim(), cultureId.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static string ToggleCulture(string csv, string cultureId)
        {
            if (String.IsNullOrWhiteSpace(cultureId))
                return NormalizeCsv(csv);

            List<string> values = new List<string>();
            string normalized = NormalizeCsv(csv);

            if (!String.Equals(normalized, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                string[] split = normalized.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                    values.Add(split[i].Trim());
            }

            int index = -1;
            for (int i = 0; i < values.Count; i++)
            {
                if (String.Equals(values[i], cultureId, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
                values.RemoveAt(index);
            else
                values.Add(cultureId);

            return values.Count == 0 ? "Todos" : NormalizeCsv(String.Join(",", values.ToArray()));
        }

        public static string BuildDisplayLabel(string csv)
        {
            string normalized = NormalizeCsv(csv);
            if (String.Equals(normalized, "Todos", StringComparison.OrdinalIgnoreCase))
                return "Todos";

            List<OSUCultureDefinition> defs = GetCultureOptions();
            string[] split = normalized.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> labels = new List<string>();

            for (int i = 0; i < split.Length; i++)
            {
                string id = split[i].Trim();
                string label = id;

                for (int j = 0; j < defs.Count; j++)
                {
                    if (defs[j] != null && String.Equals(defs[j].Id, id, StringComparison.OrdinalIgnoreCase))
                    {
                        label = defs[j].DisplayName;
                        break;
                    }
                }

                labels.Add(label);
            }

            return labels.Count == 0 ? "Todos" : String.Join(", ", labels.ToArray());
        }

        public static bool IsAllowedFor(PlayerMobile pm, string csv)
        {
            if (pm == null)
                return false;

            return ContainsCulture(csv, pm.OSUCultureId);
        }
    }
}
