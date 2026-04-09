using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;
using Server.Custom;
using Server.Custom.Systems.HtmlBooks.Engine;
using Server.Custom.Systems.HtmlBooks.Html.Readable;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public sealed class ReinoMilitaryArchivedReport
    {
        public DateTime GeneratedUtc;
        public string Title;
        public string SummaryHtml;
        public List<string> CrimePages = new List<string>();
        public List<string> PrisonPages = new List<string>();
        public List<string> WantedPages = new List<string>();
        public List<string> RecurringPages = new List<string>();
    }

    public static class ReinoMilitaryReportArchiveStore
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoMilitaryArchives_v1.bin");
        private static readonly Dictionary<int, List<ReinoMilitaryArchivedReport>> m_Archives = new Dictionary<int, List<ReinoMilitaryArchivedReport>>();

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };
        }

        public static List<ReinoMilitaryArchivedReport> GetArchives(int cityId)
        {
            List<ReinoMilitaryArchivedReport> list;
            if (!m_Archives.TryGetValue(cityId, out list))
            {
                list = new List<ReinoMilitaryArchivedReport>();
                m_Archives[cityId] = list;
            }

            return list;
        }

        public static ReinoMilitaryArchivedReport GetArchiveByIndex(int cityId, int index)
        {
            List<ReinoMilitaryArchivedReport> list = GetArchives(cityId);
            if (index < 0 || index >= list.Count)
                return null;
            return list[index];
        }

        public static void ArchiveCurrentReport(int cityId, string viewerName, int viewerSerial)
        {
            ReinoMilitaryArchivedReport report = BuildCurrentReport(cityId);
            if (report == null)
                return;

            List<ReinoMilitaryArchivedReport> list = GetArchives(cityId);
            list.Insert(0, report);
            while (list.Count > 10)
                list.RemoveAt(list.Count - 1);

            ReinoMilitaryReportState st = ReinoMilitarySystem.GetReportState(cityId);
            st.LastDeliveredUtc = DateTime.UtcNow;
            st.LastDeliveredTo = viewerName ?? String.Empty;
            st.LastDeliveredToSerial = viewerSerial;
        }

        public static string GetCurrentSummaryHtml(int cityId)
        {
            return BuildSummaryHtml(cityId, GetCurrentCrimes(cityId), GetCurrentPrisons(cityId));
        }

        public static int GetCurrentDetailCount(int cityId, int mode)
        {
            switch (mode)
            {
                case 1: return GetCurrentCrimePages(cityId).Count;
                case 2: return GetCurrentPrisonPages(cityId).Count;
                case 3: return GetCurrentWantedPages(cityId).Count;
                case 4: return GetCurrentRecurringPages(cityId).Count;
                default: return 0;
            }
        }

        public static string GetCurrentDetailHtml(int cityId, int mode, int index)
        {
            return GetPageFromList(GetPagesForCurrent(cityId, mode), index, "Nada registrado ainda.");
        }

        public static int GetArchiveDetailCount(ReinoMilitaryArchivedReport archive, int mode)
        {
            if (archive == null)
                return 0;

            switch (mode)
            {
                case 1: return archive.CrimePages.Count;
                case 2: return archive.PrisonPages.Count;
                case 3: return archive.WantedPages.Count;
                case 4: return archive.RecurringPages.Count;
                default: return 0;
            }
        }

        public static string GetArchiveDetailHtml(ReinoMilitaryArchivedReport archive, int mode, int index)
        {
            return GetPageFromList(GetPagesForArchive(archive, mode), index, "Nada registrado nesse relatório.");
        }

        public static string PrintReportBook(PlayerMobile from, int cityId, HtmlBook30 book, ReinoMilitaryArchivedReport archive)
        {
            if (from == null || from.Deleted || book == null || book.Deleted)
                return "Livro inválido.";

            if (!LanguageKnowledge.Understands(from, OSULanguage.Common))
                return "Você precisa falar a língua comum para copiar esse relatório para um livro.";

            if (book.GetWrittenPageCount() > 0)
                return "Esse livro não está em branco.";

            ReinoMilitaryArchivedReport src = archive ?? BuildCurrentReport(cityId);
            if (src == null)
                return "Não há relatório para copiar.";

            from.Frozen = true;
            Timer.DelayCall(TimeSpan.FromSeconds(5.0), delegate
            {
                if (from != null && !from.Deleted)
                    from.Frozen = false;
            });

            string title = String.IsNullOrWhiteSpace(src.Title) ? ("relatórios de " + DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy")) : src.Title.ToLowerInvariant();
            book.Name = title;
            book.DocumentTitle = title;
            book.Language = OSULanguage.Common;
            book.SetPageHtml(0, ToBookHtml(src.SummaryHtml));

            int page = 1;
            for (int i = 0; i < src.CrimePages.Count && page < 30; i++, page++)
                book.SetPageHtml(page, ToBookHtml(src.CrimePages[i]));

            for (int i = 0; i < src.PrisonPages.Count && page < 30; i++, page++)
                book.SetPageHtml(page, ToBookHtml(src.PrisonPages[i]));

            book.ForceSealAsCopy("Ofício Militar", 0);

            if (archive == null)
            {
                ReinoMilitaryReportState st = ReinoMilitarySystem.GetReportState(cityId);
                st.LastDeliveredUtc = DateTime.UtcNow;
                st.LastDeliveredTo = from.Name;
                st.LastDeliveredToSerial = from.Serial.Value;
            }

            return "O relatório será copiado para o livro após 5 segundos.";
        }

        public static ReinoMilitaryArchivedReport BuildCurrentReport(int cityId)
        {
            List<ReinoCrimeRecord> crimes = GetCurrentCrimes(cityId);
            List<ReinoPrisonRecord> prisons = GetCurrentPrisons(cityId);

            ReinoMilitaryArchivedReport r = new ReinoMilitaryArchivedReport();
            r.GeneratedUtc = DateTime.UtcNow;
            r.Title = "Relatorio " + r.GeneratedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            r.SummaryHtml = BuildSummaryHtml(cityId, crimes, prisons);
            r.CrimePages = BuildCrimePages(cityId, crimes);
            r.PrisonPages = BuildPrisonPages(prisons);
            r.WantedPages = BuildWantedPages(cityId);
            r.RecurringPages = BuildRecurringPages(cityId, crimes);
            return r;
        }

        private static List<string> GetPagesForCurrent(int cityId, int mode)
        {
            switch (mode)
            {
                case 1: return GetCurrentCrimePages(cityId);
                case 2: return GetCurrentPrisonPages(cityId);
                case 3: return GetCurrentWantedPages(cityId);
                case 4: return GetCurrentRecurringPages(cityId);
                default: return new List<string>();
            }
        }

        private static List<string> GetPagesForArchive(ReinoMilitaryArchivedReport archive, int mode)
        {
            if (archive == null)
                return new List<string>();

            switch (mode)
            {
                case 1: return archive.CrimePages;
                case 2: return archive.PrisonPages;
                case 3: return archive.WantedPages;
                case 4: return archive.RecurringPages;
                default: return new List<string>();
            }
        }

        private static string GetPageFromList(List<string> list, int index, string empty)
        {
            if (list == null || list.Count == 0)
                return "<BASEFONT COLOR=#000000>" + empty + "</BASEFONT>";
            if (index < 0) index = 0;
            if (index >= list.Count) index = list.Count - 1;
            return list[index];
        }

        private static List<ReinoCrimeRecord> GetCurrentCrimes(int cityId)
        {
            DateTime since = ReinoMilitarySystem.GetReportState(cityId).LastDeliveredUtc;
            List<ReinoCrimeRecord> src = ReinoMilitarySystem.GetCrimeList(cityId);
            List<ReinoCrimeRecord> list = new List<ReinoCrimeRecord>();
            for (int i = 0; i < src.Count; i++)
            {
                ReinoCrimeRecord r = src[i];
                if (r != null && r.Utc > since)
                    list.Add(r);
            }
            return list;
        }

        private static List<ReinoPrisonRecord> GetCurrentPrisons(int cityId)
        {
            DateTime since = ReinoMilitarySystem.GetReportState(cityId).LastDeliveredUtc;
            List<ReinoPrisonRecord> src = ReinoMilitarySystem.GetPrisonList(cityId);
            List<ReinoPrisonRecord> list = new List<ReinoPrisonRecord>();
            for (int i = 0; i < src.Count; i++)
            {
                ReinoPrisonRecord r = src[i];
                if (r != null && r.ArrestUtc > since)
                    list.Add(r);
            }
            return list;
        }

        private static bool IsAdministrativeCrimeRecord(ReinoCrimeRecord r)
        {
            return r != null && r.CriminalSerial == 0 && r.WitnessGuardSerial == 0;
        }

        private static string BuildSummaryHtml(int cityId, List<ReinoCrimeRecord> crimes, List<ReinoPrisonRecord> prisons)
        {
            ReinoMilitaryReportState st = ReinoMilitarySystem.GetReportState(cityId);
            int since = 0;
            for (int i = 0; i < crimes.Count; i++)
            {
                if (crimes[i] != null && !IsAdministrativeCrimeRecord(crimes[i]))
                    since++;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");
            sb.Append("<B>Crimes desde o último relatório:</B> ").Append(since).Append(".<BR>");
            sb.Append("<B>Prisões desde o último relatório:</B> ").Append(prisons.Count).Append(".<BR>");
            sb.Append("<B>Última entrega:</B> ").Append(st.LastDeliveredUtc == DateTime.MinValue ? "nunca" : st.LastDeliveredUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append(".<BR>");
            sb.Append("<B>Entregue a:</B> ").Append(String.IsNullOrWhiteSpace(st.LastDeliveredTo) ? "ninguém" : st.LastDeliveredTo).Append(".");
            sb.Append("</BASEFONT>");
            return sb.ToString();
        }

        private static List<string> GetCurrentCrimePages(int cityId)
        {
            return BuildCrimePages(cityId, GetCurrentCrimes(cityId));
        }

        private static List<string> GetCurrentPrisonPages(int cityId)
        {
            return BuildPrisonPages(GetCurrentPrisons(cityId));
        }

        private static List<string> GetCurrentWantedPages(int cityId)
        {
            return BuildWantedPages(cityId);
        }

        private static List<string> GetCurrentRecurringPages(int cityId)
        {
            return BuildRecurringPages(cityId, GetCurrentCrimes(cityId));
        }

        private static List<string> BuildCrimePages(int cityId, List<ReinoCrimeRecord> crimes)
        {
            List<string> pages = new List<string>();
            if (crimes.Count == 0)
            {
                pages.Add("<BASEFONT COLOR=#000000>Nenhum crime registrado.</BASEFONT>");
                return pages;
            }

            for (int i = 0; i < crimes.Count; i++)
            {
                ReinoCrimeRecord r = crimes[i];
                StringBuilder sb = new StringBuilder();
                sb.Append("<BASEFONT COLOR=#000000>");
                sb.Append("<B>Registro ").Append(i + 1).Append(" de ").Append(crimes.Count).Append("</B><BR><BR>");
                if (IsAdministrativeCrimeRecord(r))
                {
                    sb.Append("<B>Tipo:</B> Registro administrativo<BR>");
                    sb.Append("<B>Origem:</B> ").Append(r.WitnessGuardName).Append("<BR>");
                    sb.Append("<B>Quando:</B> ").Append(r.Utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                    if (!String.IsNullOrWhiteSpace(r.Notes))
                        sb.Append("<B>Detalhes:</B> ").Append(r.Notes);
                }
                else
                {
                    sb.Append("<B>Quem:</B> ").Append(r.CriminalName).Append("<BR>");
                    sb.Append("<B>Crime:</B> ").Append(ReinoMilitarySystem.GetLawLabel(r.Law)).Append("<BR>");
                    sb.Append("<B>Guarda:</B> ").Append(r.WitnessGuardName).Append("<BR>");
                    sb.Append("<B>Quando:</B> ").Append(r.Utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                    sb.Append("<B>Resultado:</B> ").Append(ReinoMilitarySystem.GetActionLabel(r.Result)).Append("<BR>");
                    sb.Append("<B>Guarda morto:</B> ").Append(r.GuardDied ? "sim" : "não").Append("<BR>");
                    sb.Append("<B>Personagem morto:</B> ").Append(r.CriminalDied ? "sim" : "não").Append("<BR>");
                    sb.Append("<B>Desmaiado:</B> ").Append(r.CriminalKnockedOut ? "sim" : "não").Append("<BR>");
                    sb.Append("<B>Itens no quartel:</B> ").Append(r.LootStoredInBarracks ? "sim" : "não").Append("<BR>");
                    sb.Append("<B>Preso:</B> ").Append(r.SentToPrison ? "sim" : "não").Append("<BR>");
                    if (!String.IsNullOrWhiteSpace(r.Notes))
                        sb.Append("<B>Observação:</B> ").Append(r.Notes);
                }
                sb.Append("</BASEFONT>");
                pages.Add(sb.ToString());
            }

            return pages;
        }

        private static List<string> BuildPrisonPages(List<ReinoPrisonRecord> prisons)
        {
            List<string> pages = new List<string>();
            if (prisons.Count == 0)
            {
                pages.Add("<BASEFONT COLOR=#000000>Nenhuma prisão registrada.</BASEFONT>");
                return pages;
            }

            for (int i = 0; i < prisons.Count; i++)
            {
                ReinoPrisonRecord r = prisons[i];
                StringBuilder sb = new StringBuilder();
                sb.Append("<BASEFONT COLOR=#000000>");
                sb.Append("<B>Prisão ").Append(i + 1).Append(" de ").Append(prisons.Count).Append("</B><BR><BR>");
                sb.Append("<B>Preso:</B> ").Append(r.PrisonerName).Append("<BR>");
                sb.Append("<B>Quem prendeu:</B> ").Append(r.ArrestedBy).Append("<BR>");
                sb.Append("<B>Crime:</B> ").Append(r.CrimeLabel).Append("<BR>");
                sb.Append("<B>Entrada:</B> ").Append(r.ArrestUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                sb.Append("<B>Saída:</B> ").Append(r.ReleaseUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                sb.Append("<B>Pena:</B> ").Append(r.DurationHours).Append(" horas<BR>");
                sb.Append("<B>Solto por:</B> ").Append(String.IsNullOrWhiteSpace(r.ReleasedBy) ? "ainda preso" : r.ReleasedBy).Append("<BR>");
                if (!String.IsNullOrWhiteSpace(r.Notes))
                    sb.Append("<B>Observação:</B> ").Append(r.Notes);
                sb.Append("</BASEFONT>");
                pages.Add(sb.ToString());
            }
            return pages;
        }

        private static List<string> BuildWantedPages(int cityId)
        {
            List<string> pages = new List<string>();
            List<ReinoWantedEntry> list = ReinoMilitarySystem.GetWantedList(cityId);
            if (list.Count == 0)
            {
                pages.Add("<BASEFONT COLOR=#000000>Nenhum procurado registrado.</BASEFONT>");
                return pages;
            }

            for (int i = 0; i < list.Count; i++)
            {
                ReinoWantedEntry e = list[i];
                StringBuilder sb = new StringBuilder();
                sb.Append("<BASEFONT COLOR=#000000>");
                sb.Append("<B>Procurado ").Append(i + 1).Append(" de ").Append(list.Count).Append("</B><BR><BR>");
                sb.Append("<B>Nome:</B> ").Append(e.PlayerName).Append("<BR>");
                sb.Append("<B>Ação:</B> ").Append(ReinoMilitarySystem.GetActionLabel(e.Action)).Append("<BR>");
                sb.Append("<B>Adicionado por:</B> ").Append(e.AddedByName).Append("<BR>");
                sb.Append("<B>Quando:</B> ").Append(e.AddedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Append("<BR>");
                sb.Append("</BASEFONT>");
                pages.Add(sb.ToString());
            }
            return pages;
        }

        private static List<string> BuildRecurringPages(int cityId, List<ReinoCrimeRecord> crimes)
        {
            Dictionary<string, int> recurring = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, HashSet<string>> labels = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < crimes.Count; i++)
            {
                ReinoCrimeRecord r = crimes[i];
                if (r == null || IsAdministrativeCrimeRecord(r) || String.IsNullOrWhiteSpace(r.CriminalName) || r.CriminalSerial == 0)
                    continue;

                int count;
                recurring.TryGetValue(r.CriminalName, out count);
                recurring[r.CriminalName] = count + 1;

                HashSet<string> set;
                if (!labels.TryGetValue(r.CriminalName, out set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    labels[r.CriminalName] = set;
                }
                set.Add(ReinoMilitarySystem.GetLawLabel(r.Law));
            }

            List<string> pages = new List<string>();
            if (recurring.Count == 0)
            {
                pages.Add("<BASEFONT COLOR=#000000>Nenhum criminoso recorrente ainda.</BASEFONT>");
                return pages;
            }

            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(recurring);
            list.Sort(delegate (KeyValuePair<string, int> a, KeyValuePair<string, int> b) { return b.Value.CompareTo(a.Value); });
            for (int i = 0; i < list.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<BASEFONT COLOR=#000000>");
                sb.Append("<B>Criminoso recorrente ").Append(i + 1).Append(" de ").Append(list.Count).Append("</B><BR><BR>");
                sb.Append("<B>Nome:</B> ").Append(list[i].Key).Append("<BR>");
                sb.Append("<B>Ocorrências:</B> ").Append(list[i].Value).Append("<BR>");
                HashSet<string> set = labels[list[i].Key];
                sb.Append("<B>Crimes:</B> ").Append(String.Join(", ", new List<string>(set).ToArray()));
                sb.Append("</BASEFONT>");
                pages.Add(sb.ToString());
            }
            return pages;
        }

        private static string ToBookHtml(string html)
        {
            string text = StripTags(html).Replace("\r", " ").Replace("\n", " ");
            text = text.Replace("•", "-");
            text = text.Replace("  ", " ");
            List<string> lines = WrapLines(text, 20);
            return String.Join("<BR>", lines.ToArray());
        }

        private static string StripTags(string html)
        {
            if (String.IsNullOrWhiteSpace(html))
                return String.Empty;
            StringBuilder sb = new StringBuilder();
            bool inside = false;
            for (int i = 0; i < html.Length; i++)
            {
                char c = html[i];
                if (c == '<')
                {
                    inside = true;
                    if (i + 3 < html.Length)
                    {
                        string sub = html.Substring(i).ToUpperInvariant();
                        if (sub.StartsWith("<BR"))
                            sb.Append('\n');
                    }
                    continue;
                }
                if (c == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static List<string> WrapLines(string text, int max)
        {
            List<string> lines = new List<string>();
            if (String.IsNullOrWhiteSpace(text))
            {
                lines.Add(String.Empty);
                return lines;
            }

            string[] rawLines = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int li = 0; li < rawLines.Length; li++)
            {
                string[] words = rawLines[li].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder line = new StringBuilder();
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];
                    if (line.Length == 0)
                    {
                        line.Append(word);
                    }
                    else if (line.Length + 1 + word.Length <= max)
                    {
                        line.Append(' ').Append(word);
                    }
                    else
                    {
                        lines.Add(line.ToString());
                        line.Length = 0;
                        line.Append(word);
                    }
                }
                if (line.Length > 0)
                    lines.Add(line.ToString());
            }

            if (lines.Count == 0)
                lines.Add(String.Empty);
            return lines;
        }

        private static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(0);
                bw.Write(m_Archives.Count);
                foreach (KeyValuePair<int, List<ReinoMilitaryArchivedReport>> kv in m_Archives)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value.Count);
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        ReinoMilitaryArchivedReport r = kv.Value[i];
                        bw.Write(r.GeneratedUtc.ToBinary());
                        bw.Write(r.Title ?? String.Empty);
                        bw.Write(r.SummaryHtml ?? String.Empty);
                        WriteStringList(bw, r.CrimePages);
                        WriteStringList(bw, r.PrisonPages);
                        WriteStringList(bw, r.WantedPages);
                        WriteStringList(bw, r.RecurringPages);
                    }
                }
            }
        }

        private static void Load()
        {
            m_Archives.Clear();
            if (!File.Exists(FilePath))
                return;

            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                int version = br.ReadInt32();
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int cityId = br.ReadInt32();
                    int listCount = br.ReadInt32();
                    List<ReinoMilitaryArchivedReport> list = new List<ReinoMilitaryArchivedReport>();
                    for (int x = 0; x < listCount; x++)
                    {
                        ReinoMilitaryArchivedReport r = new ReinoMilitaryArchivedReport();
                        r.GeneratedUtc = DateTime.FromBinary(br.ReadInt64());
                        r.Title = br.ReadString();
                        r.SummaryHtml = br.ReadString();
                        r.CrimePages = ReadStringList(br);
                        r.PrisonPages = ReadStringList(br);
                        r.WantedPages = ReadStringList(br);
                        r.RecurringPages = ReadStringList(br);
                        list.Add(r);
                    }
                    m_Archives[cityId] = list;
                }
            }
        }

        private static void WriteStringList(BinaryWriter bw, List<string> list)
        {
            bw.Write(list != null ? list.Count : 0);
            if (list == null)
                return;
            for (int i = 0; i < list.Count; i++)
                bw.Write(list[i] ?? String.Empty);
        }

        private static List<string> ReadStringList(BinaryReader br)
        {
            int count = br.ReadInt32();
            List<string> list = new List<string>();
            for (int i = 0; i < count; i++)
                list.Add(br.ReadString());
            return list;
        }
    }
}
