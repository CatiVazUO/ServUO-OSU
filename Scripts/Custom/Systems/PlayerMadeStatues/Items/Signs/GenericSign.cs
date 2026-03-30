using Server.Gumps;

namespace Server.Items
{
    public class GenericSign : Item
    {
        [Constructable]
        public GenericSign() : base(0x0BD2)
        {
            Name = "placa genérica";
            Weight = 1.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendGump(new GenericSignGump());
        }

        public GenericSign(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
