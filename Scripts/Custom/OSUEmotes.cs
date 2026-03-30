using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Custom.Systems.Needs.Gumps;
using Server.Custom.Systems.Animations;

namespace Server.Custom.Systems.Emotes
{
    public static class OSUEmotesCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("e", AccessLevel.Player, OnEmoteCommand);
            CommandSystem.Register("emote", AccessLevel.Player, OnEmoteCommand);
            CommandSystem.Register("emotes", AccessLevel.Player, OnEmoteCommand);
        }

        [Usage("e [nomeDoEmote]")]
        [Description("Abre o menu de emotes ou executa um emote direto.")]
        private static void OnEmoteCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null)
                return;

            string arg = (e.ArgString ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(arg))
            {
                from.CloseGump(typeof(OSUEmotesGump));
                from.SendGump(new OSUEmotesGump(from, 0));
                return;
            }

            int emoteId;
            if (OSUEmoteSystem.TryGetEmoteId(arg, out emoteId))
            {
                OSUEmoteSystem.Execute(from, emoteId);
                return;
            }

            from.SendMessage("Emote não reconhecido. Abrindo a lista de emotes.");
            from.CloseGump(typeof(OSUEmotesGump));
            from.SendGump(new OSUEmotesGump(from, 0));
        }
    }

    public sealed class OSUEmoteDefinition
    {
        public int Id { get; private set; }
        public string Label { get; private set; }
        public string[] Aliases { get; private set; }

        public OSUEmoteDefinition(int id, string label, params string[] aliases)
        {
            Id = id;
            Label = label;
            Aliases = aliases ?? new string[0];
        }
    }

    public static class OSUEmoteSystem
    {
        public const int PerPage = 15;

        private static readonly OSUEmoteDefinition[] _entries = new OSUEmoteDefinition[]
        {
            new OSUEmoteDefinition(1,  "Ah", "ah"),
            new OSUEmoteDefinition(2,  "Ah ha!", "ah-ha", "ah ha", "ahha"),
            new OSUEmoteDefinition(3,  "Aplaudir", "aplaudir", "aplaude", "aplaudir"),
            new OSUEmoteDefinition(4,  "Assoar", "assoar", "assoar", "assoar nariz"),
            new OSUEmoteDefinition(5,  "Reverenciar", "reverenciar", "reverencia", "curvar", "bow"),
            new OSUEmoteDefinition(6,  "Tosse ironica", "tosseironica", "toss ironica"),
            new OSUEmoteDefinition(7,  "Arrotar", "arrotar", "burp"),
            new OSUEmoteDefinition(8,  "Limpar garganta", "limpargarganta", "limpar garganta"),
            new OSUEmoteDefinition(9,  "Tossir", "tossir", "tosse", "cof"),
            new OSUEmoteDefinition(10, "Chorar", "chorar", "chora"),
            new OSUEmoteDefinition(11, "Desmaiar", "desmaiar", "desmaia", "faint"),
            new OSUEmoteDefinition(12, "Peidar", "peidar", "peida"),
            new OSUEmoteDefinition(13, "Arfar", "arfar", "arfa"),
            new OSUEmoteDefinition(14, "Rir", "rir", "ri", "laugh"),
            new OSUEmoteDefinition(15, "Gemer", "gemer", "geme"),
            new OSUEmoteDefinition(16, "Rugir", "rugir", "ruge", "grrr"),
            new OSUEmoteDefinition(17, "Hey", "hey"),
            new OSUEmoteDefinition(18, "Soluçar", "solucar", "soluçar", "soluca", "soluça"),
            new OSUEmoteDefinition(19, "Huh?", "huh?", "huh", "hein"),
            new OSUEmoteDefinition(20, "Beijar", "beijar", "beija", "kiss"),
            new OSUEmoteDefinition(21, "Gargalhar", "gargalhar", "gargalha"),
            new OSUEmoteDefinition(22, "No", "no", "nao", "não"),
            new OSUEmoteDefinition(23, "Oh!", "oh!", "oh"),
            new OSUEmoteDefinition(24, "Oooh", "oooh", "ooo"),
            new OSUEmoteDefinition(25, "Oops", "oops"),
            new OSUEmoteDefinition(26, "Vomitar", "vomitar", "vomita", "puke"),
            new OSUEmoteDefinition(27, "Esmurrar", "esmurrar", "esmurra", "socar"),
            new OSUEmoteDefinition(28, "Urrar", "urrar", "urra"),
            new OSUEmoteDefinition(29, "Shhh!", "calar", "shhh", "shhh!", "silencio", "silêncio"),
            new OSUEmoteDefinition(30, "Suspirar", "suspirar", "suspira"),
            new OSUEmoteDefinition(31, "Estapear", "estapear", "estapeia", "slap"),
            new OSUEmoteDefinition(32, "Espirrar", "espirar", "espirrar", "espirra"),
            new OSUEmoteDefinition(33, "Choramingar", "choromingar", "choramingar", "choraminga", "snif"),
            new OSUEmoteDefinition(34, "Roncar", "roncar", "ronca"),
            new OSUEmoteDefinition(35, "Cuspir", "cuspir", "cospe"),
            new OSUEmoteDefinition(36, "Estirar língua", "estirarlingua", "estirar lingua", "mostrar lingua", "mostrar língua"),
            new OSUEmoteDefinition(37, "Bater pé", "baterpe", "bater pe", "bate o pe", "bate o pé"),
            new OSUEmoteDefinition(38, "Assoviar", "assoviar", "assovia"),
            new OSUEmoteDefinition(39, "Woohoo", "woohoo"),
            new OSUEmoteDefinition(40, "Bocejar", "bocejar", "boceja"),
            new OSUEmoteDefinition(41, "Yeah!", "yeah!", "yeah"),
            new OSUEmoteDefinition(42, "Gritar", "gritar", "grita")
        };

        private static readonly Dictionary<string, int> _lookup = BuildLookup();

        public static OSUEmoteDefinition[] Entries
        {
            get { return _entries; }
        }

        public static int TotalPages
        {
            get { return (_entries.Length + PerPage - 1) / PerPage; }
        }

        private static Dictionary<string, int> BuildLookup()
        {
            Dictionary<string, int> table = new Dictionary<string, int>();

            for (int i = 0; i < _entries.Length; i++)
            {
                OSUEmoteDefinition entry = _entries[i];
                AddAlias(table, entry.Label, entry.Id);

                for (int j = 0; j < entry.Aliases.Length; j++)
                    AddAlias(table, entry.Aliases[j], entry.Id);
            }

            return table;
        }

        private static void AddAlias(Dictionary<string, int> table, string text, int id)
        {
            string key = Normalize(text);

            if (string.IsNullOrEmpty(key))
                return;

            if (!table.ContainsKey(key))
                table.Add(key, id);
        }

        public static bool TryGetEmoteId(string text, out int id)
        {
            return _lookup.TryGetValue(Normalize(text), out id);
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = text.Trim().ToLowerInvariant();
            text = text.Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a");
            text = text.Replace("é", "e").Replace("ê", "e");
            text = text.Replace("í", "i");
            text = text.Replace("ó", "o").Replace("ô", "o").Replace("õ", "o");
            text = text.Replace("ú", "u");
            text = text.Replace("ç", "c");
            text = text.Replace("-", string.Empty).Replace(" ", string.Empty);
            text = text.Replace("!", string.Empty).Replace("?", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty);
            text = text.Replace("*", string.Empty).Replace("'", string.Empty).Replace("\"", string.Empty);

            return text;
        }

        public static void Execute(Mobile from, int emoteId)
        {
            if (from == null || from.Deleted)
                return;

            switch (emoteId)
            {
                case 1: PlayAndSay(from, from.Female ? 778 : 1049, "*ah!*"); break;
                case 2: PlayAndSay(from, from.Female ? 779 : 1050, "*ah-ha!*"); break;
                case 3: PlayAndSay(from, from.Female ? 780 : 1051, "*aplaude*"); break;
                case 4:
                    PlayAndSay(from, from.Female ? 781 : 1052, "*assoa o nariz*");
                    TryAnimate(from, 34);
                    break;
                case 5:
                    from.Say("*reverencia*");
                    TryAnimate(from, 32);
                    break;
                case 6: PlayAndSay(from, from.Female ? 786 : 1057, "*tosse ironicamente*"); break;
                case 7:
                    PlayAndSay(from, from.Female ? 782 : 1053, "*burp!*");
                    TryAnimate(from, 33);
                    break;
                case 8:
                    PlayAndSay(from, from.Female ? 0x310 : 1055, "*limpa a garganta*");
                    TryAnimate(from, 33);
                    break;
                case 9:
                    PlayAndSay(from, from.Female ? 785 : 1056, "*cof cof!*");
                    TryAnimate(from, 33);
                    break;
                case 10: PlayAndSay(from, from.Female ? 787 : 1058, "*chora*"); break;
                case 11: DoFaint(from); break;
                case 12: PlayAndSay(from, from.Female ? 792 : 1064, "*peida*"); break;
                case 13: PlayAndSay(from, from.Female ? 793 : 1065, "*arfa!*"); break;
                case 14: PlayAndSay(from, from.Female ? 794 : 1066, "*ri*"); break;
                case 15: PlayAndSay(from, from.Female ? 795 : 1067, "*geme*"); break;
                case 16: PlayAndSay(from, from.Female ? 796 : 1068, "*grrr*"); break;
                case 17: PlayAndSay(from, from.Female ? 797 : 1069, "*hey!*"); break;
                case 18: PlayAndSay(from, from.Female ? 798 : 1070, "*soluça!*"); break;
                case 19: PlayAndSay(from, from.Female ? 799 : 1071, "*huh?*"); break;
                case 20: PlayAndSay(from, from.Female ? 800 : 1072, "*beija*"); break;
                case 21: PlayAndSay(from, from.Female ? 801 : 1073, "*gargalha*"); break;
                case 22: PlayAndSay(from, from.Female ? 802 : 1074, "*no!*"); break;
                case 23: PlayAndSay(from, from.Female ? 803 : 1075, "*oh!*"); break;
                case 24: PlayAndSay(from, from.Female ? 811 : 1085, "*oooh*"); break;
                case 25: PlayAndSay(from, from.Female ? 812 : 1086, "*oops*"); break;
                case 26: DoVomit(from); break;
                case 27:
                    PlayAndSay(from, 315, "*esmurra*");
                    TryAnimate(from, 31);
                    break;
                case 28: PlayAndSay(from, from.Female ? 814 : 1088, "*ahhhh!*"); break;
                case 29: PlayAndSay(from, from.Female ? 815 : 1089, "*shhh!*"); break;
                case 30: PlayAndSay(from, from.Female ? 816 : 1090, "*suspira*"); break;
                case 31:
                    PlayAndSay(from, 948, "*estapeia*");
                    TryAnimate(from, 11);
                    break;
                case 32:
                    PlayAndSay(from, from.Female ? 817 : 1091, "*aahh-tchin!*");
                    TryAnimate(from, 32);
                    break;
                case 33:
                    PlayAndSay(from, from.Female ? 818 : 1092, "*snif*");
                    TryAnimate(from, 34);
                    break;
                case 34: PlayAndSay(from, from.Female ? 819 : 1093, "*ronca*"); break;
                case 35:
                    PlayAndSay(from, from.Female ? 820 : 1094, "*cospe*");
                    TryAnimate(from, 6);
                    break;
                case 36: PlayAndSay(from, 792, "*estira a língua*"); break;
                case 37:
                    PlayAndSay(from, 874, "*bate o pé*");
                    TryAnimate(from, 38);
                    break;
                case 38:
                    PlayAndSay(from, from.Female ? 821 : 1095, "*assovia*");
                    TryAnimate(from, 5);
                    break;
                case 39: PlayAndSay(from, from.Female ? 783 : 1054, "*woohoo!*"); break;
                case 40:
                    PlayAndSay(from, from.Female ? 822 : 1096, "*boceja*");
                    TryAnimate(from, 17);
                    break;
                case 41: PlayAndSay(from, from.Female ? 823 : 1097, "*yeah!*"); break;
                case 42: PlayAndSay(from, from.Female ? 0x338 : 1098, "*grita*"); break;
            }
        }

        private static void PlayAndSay(Mobile from, int sound, string text)
        {
            from.PlaySound(sound);
            from.Say(text);
        }

        private static void TryAnimate(Mobile from, int action)
        {
            if (from.Mounted)
                return;

            from.Animate(action, 5, 1, true, false, 0);
        }

        private static void DoFaint(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            from.PlaySound(from.Female ? 791 : 1063);
            from.Say("*desmaia*");

            if (from.Mounted)
                return;

            OSUKnockdownSystem.KnockDown(from, TimeSpan.FromSeconds(5.0));
        }

        private static void DoVomit(Mobile from)
        {

            from.PlaySound(from.Female ? 813 : 1087);
            from.Say("*vomita*");

            PlayerMobile pm = from as PlayerMobile;

            if (pm != null)
            {
                pm.OSUHunger = Math.Max(0, pm.OSUHunger - 10);
                pm.OSUThirst = Math.Max(0, pm.OSUThirst - 10);
                OSUNeedsGump.TryRefresh(pm);
            }

            TryAnimate(from, 32);

            if (from.Map == null || from.Map == Map.Internal)
                return;

            Point3D p = new Point3D(from.Location);

            switch (from.Direction)
            {
                case Direction.North: p.Y--; break;
                case Direction.South: p.Y++; break;
                case Direction.East: p.X++; break;
                case Direction.West: p.X--; break;
                case Direction.Right: p.X++; p.Y--; break;
                case Direction.Down: p.X++; p.Y++; break;
                case Direction.Left: p.X--; p.Y++; break;
                case Direction.Up: p.X--; p.Y--; break;
            }

            p.Z = from.Map.GetAverageZ(p.X, p.Y);

            if (SpellHelper.AdjustField(ref p, from.Map, 12, false))
            {
                Puke puke = new Puke();
                puke.MoveToWorld(p, from.Map);
            }
        }
    }

    public class OSUEmotesGump : Gump
    {
        private const int ButtonPrev = 1;
        private const int ButtonNext = 2;
        private const int ButtonClose = 3;
        private const int ButtonEmoteBase = 1000;

        private readonly Mobile _from;
        private readonly int _pageIndex;

        public OSUEmotesGump(Mobile from, int pageIndex) : base(0, 0)
        {
            _from = from;
            _pageIndex = pageIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            AddImageTiled(638, 251, 156, 481, 398);
            AddImageTiled(612, 257, 28, 474, 593);
            AddImageTiled(640, 229, 144, 28, 592);
            AddImageTiled(778, 255, 27, 474, 593);
            AddImageTiled(641, 718, 144, 28, 592);
            AddImage(618, 235, 1325);
            AddImage(746, 236, 1326);
            AddImage(747, 687, 1327);
            AddImage(619, 687, 1328);

            AddLabel(651, 262, 0, "Emotes");
            AddImageTiled(640, 278, 144, 13, 592);
            AddLabel(744, 261, 0, string.Format("{0}/{1}", _pageIndex + 1, OSUEmoteSystem.TotalPages));

            DrawEntries();
            DrawFooter();
        }

        private void DrawEntries()
        {
            int start = _pageIndex * OSUEmoteSystem.PerPage;
            int end = Math.Min(start + OSUEmoteSystem.PerPage, OSUEmoteSystem.Entries.Length);

            int y = 293;

            for (int i = start; i < end; i++)
            {
                OSUEmoteDefinition entry = OSUEmoteSystem.Entries[i];

                AddButton(649, y + 2, 437, 248, ButtonEmoteBase + entry.Id, GumpButtonType.Reply, 0);
                AddLabel(674, y, 0, entry.Label);

                y += 25;
            }
        }

        private void DrawFooter()
        {
            AddImageTiled(642, 668, 144, 13, 592);

            if (_pageIndex > 0)
            {
                AddButton(650, 685, 589, 589, ButtonPrev, GumpButtonType.Reply, 0);
        //        AddLabel(674, 684, 0, "Anterior");
            }

            if (_pageIndex < OSUEmoteSystem.TotalPages - 1)
            {
                AddButton(727, 685, 588, 588, ButtonNext, GumpButtonType.Reply, 0);
        //        AddLabel(748, 684, 0, "Próxima");
            }

       //     AddButton(649, 705, 4017, 4019, ButtonClose, GumpButtonType.Reply, 0);
       //     AddLabel(674, 703, 0, "Fechar");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_from == null || _from.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 0:
                case ButtonClose:
                    return;
                case ButtonPrev:
                    _from.SendGump(new OSUEmotesGump(_from, Math.Max(0, _pageIndex - 1)));
                    return;
                case ButtonNext:
                    _from.SendGump(new OSUEmotesGump(_from, Math.Min(OSUEmoteSystem.TotalPages - 1, _pageIndex + 1)));
                    return;
                default:
                    if (info.ButtonID >= ButtonEmoteBase)
                    {
                        int emoteId = info.ButtonID - ButtonEmoteBase;
                        OSUEmoteSystem.Execute(_from, emoteId);

                        int reopenPage = (emoteId - 1) / OSUEmoteSystem.PerPage;
                        _from.SendGump(new OSUEmotesGump(_from, reopenPage));
                    }
                    return;
            }
        }
    }

    public class ItemRemovalTimer : Timer
    {
        private readonly Item _item;

        public ItemRemovalTimer(Item item) : base(TimeSpan.FromSeconds(180.0))
        {
            Priority = TimerPriority.OneSecond;
            _item = item;
        }

        protected override void OnTick()
        {
            if (_item != null && !_item.Deleted)
                _item.Delete();

            Stop();
        }
    }

    public class Puke : Item
    {
        private Timer _timer;

        [Constructable]
        public Puke() : base(Utility.RandomList(0x0F3B, 0x0F3C))
        {
            Name = "Vômito";
            Hue = 0x236;
            Movable = false;

            _timer = new ItemRemovalTimer(this);
            _timer.Start();
        }

        public Puke(Serial serial) : base(serial)
        {
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (_timer != null)
                _timer.Stop();
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, Name);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Delete(); // evita sobrar no mundo após restart
        }
    }
}
