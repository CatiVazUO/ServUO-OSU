using Server.Custom.Systems.Olhar;

namespace Server.Custom.Systems.Olhar.Items
{
    public class EstatuaEspecial : OSUOlharItem
    {
        [Constructable]
        public EstatuaEspecial() : base(0x1223) // ID exemplo
        {
            Name = "Estátua Antiga";
            OlharTxt = "Uma estátua antiga, com símbolos apagados pelo tempo.";
        }

        public EstatuaEspecial(Serial serial) : base(serial) { }

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
