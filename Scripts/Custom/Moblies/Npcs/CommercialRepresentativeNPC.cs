using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class ReinoCommercialRepresentative : BaseCreature
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId { get; set; }

        public override bool IsInvulnerable
        {
            get { return true; }
        }

        [Constructable]
        public ReinoCommercialRepresentative() : this(-1)
        {
        }

        [Constructable]
        public ReinoCommercialRepresentative(int cityId) : base(AIType.AI_Animal, FightMode.None, 10, 1, 0.1, 0.2)
        {
            GovernmentCityId = cityId;
            Blessed = true;
            CantWalk = true;
            Direction = Direction.South;
            Female = Utility.RandomBool();
            Body = Female ? 0x191 : 0x190;
            Name = "representante comercial";
            Title = "do reino";
            Hue = Utility.RandomSkinHue();
            SpeechHue = 0;
            NameHue = 0;

            Utility.AssignRandomHair(this);
            AddItem(new Boots());
            AddItem(new Shirt());
            AddItem(new LongPants());
            AddItem(new Surcoat(GetKingdomHue(cityId)));

            RefreshName();
        }

        private int GetKingdomHue(int cityId)
        {
            switch (cityId)
            {
                case 0: return 0x89F;
                case 1: return 0x59B;
                case 2: return 0x482;
                case 3: return 0x847;
                default: return 0;
            }
        }

        public void RefreshName()
        {
            string city = ReinoElectionsSystem.GetCityName(GovernmentCityId);
            Name = "representante comercial";
            Title = "de " + city;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            if (!from.InRange(Location, 2))
            {
                from.SendLocalizedMessage(500446);
                return;
            }

            pm.CloseGump(typeof(ReinoCommercialRepresentativeTradeGump));
            pm.CloseGump(typeof(ReinoCommercialRepresentativeConfigGump));

            string diplomacyReason;
            if (!ReinoDiplomacySystem.CanUseCommercialRepresentative(pm, GovernmentCityId, out diplomacyReason))
            {
                if (!String.IsNullOrWhiteSpace(diplomacyReason))
                    pm.SendMessage(diplomacyReason);
                return;
            }

            pm.SendGump(new ReinoCommercialRepresentativeTradeGump(pm, GovernmentCityId));

            if (ReinoEmploymentSystem.CanUseCommercialRepresentative(pm, GovernmentCityId))
                pm.SendGump(new ReinoCommercialRepresentativeConfigGump(pm, GovernmentCityId));
        }

        public ReinoCommercialRepresentative(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(GovernmentCityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            GovernmentCityId = reader.ReadInt();
            RefreshName();
        }
    }
}
