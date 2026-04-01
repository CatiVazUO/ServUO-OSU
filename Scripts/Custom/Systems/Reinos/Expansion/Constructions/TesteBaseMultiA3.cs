using Server;
using Server.Multis;
using Server.Multis.Deeds;

namespace Server.Custom.Testes
{
    public class TesteCasaA3 : BaseHouse
    {
        public static Rectangle2D[] AreaArray = new Rectangle2D[]
        {
            new Rectangle2D(-8, -8, 17, 17)
        };

        [Constructable]
        public TesteCasaA3() : this(null)
        {
        }

        public TesteCasaA3(Mobile owner)
            : base(0xA3, owner, 0, 0)
        {
            RestrictDecay = true;
            Public = true;

            // AJUSTAR estes offsets depois do teste
            SetSign(1, 8, 16);
            AddSouthDoors(-1, 3, 7);
        }

        public TesteCasaA3(Serial serial)
            : base(serial)
        {
        }

        public override Rectangle2D[] Area
        {
            get { return AreaArray; }
        }

        public override Point3D BaseBanLocation
        {
            get { return new Point3D(0, 8, 0); }
        }

        public override HouseDeed GetDeed()
        {
            return null;
        }

        public override int DefaultPrice
        {
            get { return 0; }
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
