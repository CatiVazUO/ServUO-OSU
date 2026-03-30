using System;
using Server;
using Server.Items;

namespace Server.Custom.Systems.Olhar
{
    public class OSUOlharItem : Item
    {
        private string _OlharTxt;

        [CommandProperty(AccessLevel.GameMaster)]
        public new string OlharTxt
        {
            get => _OlharTxt;
            set => _OlharTxt = value;
        }

        public OSUOlharItem(int itemID) : base(itemID)
        {
        }

        public OSUOlharItem(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
            writer.Write(_OlharTxt);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            _OlharTxt = reader.ReadString();
        }
    }
}
