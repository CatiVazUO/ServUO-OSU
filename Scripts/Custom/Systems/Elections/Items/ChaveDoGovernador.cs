using Server;
using Server.Mobiles;
using System;

namespace Server.Custom.Reinos
{
    public class ChaveDoGovernador : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get; set; }

        [Constructable]
        public ChaveDoGovernador() : this(-1)
        {
        }

        [Constructable]
        public ChaveDoGovernador(int cityId) : base(0x1012)
        {
            LootType = LootType.Blessed;
            Weight = 1.0;
            CityId = cityId;
            Name = "chave da cidade";
            Hue = 1154;

            UpdateName();
        }

        private void UpdateName()
        {
            switch (CityId)
            {
                case 0: Name = "chave de Aurora"; break;
                case 1: Name = "chave de Xetá"; break;
                case 2: Name = "chave de Lurone"; break;
                case 3: Name = "chave de Willran"; break;
                default: Name = "chave do governador"; break;
            }
        }

        public ChaveDoGovernador(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return;

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(pm, CityId))
            {
                pm.SendMessage("Você não pertence ao povo permitido para usar esta chave de governo.");
                return;
            }

            pm.SendMessage("Esta chave concede acesso emergencial ao governo de " + ReinoElectionsSystem.GetCityName(CityId) + ".");
            pm.CloseGump(typeof(ReinoExpansionGump));
            pm.SendGump(new ReinoExpansionGump(pm, CityId));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
            writer.Write(CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            CityId = reader.ReadInt();
            UpdateName();

            if (Name == null || Name == "")
                Name = "chave do governador";
        }
    }
}
