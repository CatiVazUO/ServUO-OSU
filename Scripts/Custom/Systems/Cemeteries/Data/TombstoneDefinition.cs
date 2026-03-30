using System;

namespace Server.Custom.Systems.Rent
{
    [Flags]
    public enum TombstoneFields
    {
        None = 0,
        Name = 1,
        Date = 2,
        Message = 4
    }

    public enum TombstoneDateLayout
    {
        Inline = 0,
        Stacked = 1
    }

    public class TombstoneDefinition
    {
        public int ItemID { get; set; }
        public int GumpID { get; set; }
        public int ExtraCost { get; set; }
        public int TextColor { get; set; }

        public TombstoneFields Fields { get; set; }
        public TombstoneDateLayout DateLayout { get; set; }

        public int MaxNameLength { get; set; }
        public int MaxDateLength { get; set; }
        public int MaxMessageLength { get; set; }

        public int NameX { get; set; }
        public int NameY { get; set; }
        public int NameWidth { get; set; }
        public int NameHeight { get; set; }

        public int DateX { get; set; }
        public int DateY { get; set; }
        public int DateWidth { get; set; }
        public int DateHeight { get; set; }

        public int MessageX { get; set; }
        public int MessageY { get; set; }
        public int MessageWidth { get; set; }
        public int MessageHeight { get; set; }

        public bool HasName { get { return (Fields & TombstoneFields.Name) != 0; } }
        public bool HasDate { get { return (Fields & TombstoneFields.Date) != 0; } }
        public bool HasMessage { get { return (Fields & TombstoneFields.Message) != 0; } }

        public string TextColorHtml
        {
            get { return String.Format("#{0:X6}", TextColor & 0xFFFFFF); }
        }
    }
}
