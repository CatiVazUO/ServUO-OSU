using Server;
using Server.Items;
using System;

namespace Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bandages
{
    public class LinenBandage : Bandage
    {
        [Constructable]
        public LinenBandage() : this(1)
        {
        }

        [Constructable]
        public LinenBandage(int amount) : base(amount)
        {
            Name = "bandagem de linho";
        }

        public LinenBandage(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Cura em etapas: aplica uma parte da cura agora e o resto ao longo de 20 segundos.");
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
