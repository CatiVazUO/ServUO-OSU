using Server;
using Server.Items;
using System;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bandages
{
    public class WoolBandage : Bandage
    {
        [Constructable]
        public WoolBandage() : this(1)
        {
        }

        [Constructable]
        public WoolBandage(int amount) : base(amount)
        {
            Name = "bandagem de lã";
        }

        public WoolBandage(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Bandagem espessa: cura mais, mas age mais devagar.");
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
        }
    }
}
