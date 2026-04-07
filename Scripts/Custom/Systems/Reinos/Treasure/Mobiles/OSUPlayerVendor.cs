using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Mobiles
{
    public class OSUPlayerVendor : PlayerVendor
    {
        [Constructable]
        public OSUPlayerVendor(Mobile owner, BaseHouse house) : base(owner, house)
        {
        }

        public OSUPlayerVendor(Serial serial) : base(serial)
        {
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

namespace Server.Items
{
    public class OSUContractOfEmployment : ContractOfEmployment
    {
        [Constructable]
        public OSUContractOfEmployment() : base()
        {
            Name = "contrato de emprego osu";
        }

        public OSUContractOfEmployment(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            BaseHouse house = BaseHouse.FindHouseAt(from);
            if (house == null)
            {
                from.SendLocalizedMessage(503240);
                return;
            }

            if (!BaseHouse.NewVendorSystem && !house.IsFriend(from))
            {
                from.SendLocalizedMessage(503242);
                return;
            }

            if (BaseHouse.NewVendorSystem && !house.IsOwner(from))
            {
                from.SendLocalizedMessage(1062423);
                return;
            }

            if (!house.Public || !house.CanPlaceNewVendor())
            {
                from.SendLocalizedMessage(503241);
                return;
            }

            bool vendor, contract;
            BaseHouse.IsThereVendor(from.Location, from.Map, out vendor, out contract);

            if (vendor)
            {
                from.SendLocalizedMessage(1062677);
                return;
            }

            if (contract)
            {
                from.SendLocalizedMessage(1062678);
                return;
            }

            Mobile v = new Server.Mobiles.OSUPlayerVendor(from, house);
            v.Direction = from.Direction & Direction.Mask;
            v.MoveToWorld(from.Location, from.Map);
            v.SayTo(from, 503246);
            EventSink.InvokePlacePlayerVendor(new PlacePlayerVendorEventArgs(from, v));
            Delete();
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
