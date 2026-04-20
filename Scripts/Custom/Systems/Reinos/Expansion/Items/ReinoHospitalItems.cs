
using System;
using Server;
using Server.Items;

namespace Server.Custom.Reinos
{
    public class ReinoHospitalAccessMarker : Item
    {
        private int _cityId;
        private string _constructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return _cityId; } set { _cityId = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return _constructionKey; } set { _constructionKey = value ?? String.Empty; } }

        [Constructable]
        public ReinoHospitalAccessMarker() : this(0, String.Empty)
        {
        }

        public ReinoHospitalAccessMarker(int cityId, string constructionKey) : base(0x1B72)
        {
            Visible = false;
            Movable = false;
            _cityId = cityId;
            _constructionKey = constructionKey ?? String.Empty;
            Name = "hospital access marker";
        }

        public ReinoHospitalAccessMarker(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(_cityId);
            writer.Write(_constructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _cityId = reader.ReadInt();
            _constructionKey = reader.ReadString();
            Visible = false;
            Movable = false;
        }
    }
}
