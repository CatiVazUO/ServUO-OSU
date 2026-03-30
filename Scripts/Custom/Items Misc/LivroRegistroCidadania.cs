using Server;
using Server.Custom.Systems.Rent;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections;

namespace Server.Custom.Systems.Reinos
{
    public class LivroRegistroCidadania : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get; set; }

        [Constructable]
        public LivroRegistroCidadania() : this(-1)
        {
        }

        [Constructable]
        public LivroRegistroCidadania(int cityId) : base(0xFEF)
        {
            Movable = false;
            Weight = 3.0;
            Hue = 1150;
            CityId = cityId;

            UpdateName();
        }

        public LivroRegistroCidadania(Serial serial) : base(serial)
        {
        }

        private void UpdateName()
        {
            string cityName = ReinoElectionsSystem.GetCityName(CityId);

            if (CityId >= 0)
                Name = "livro de registro de cidadania de " + cityName;
            else
                Name = "livro de registro de cidadania";
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais do livro de registro.");
                return;
            }

            if (CityId < 0)
            {
                pm.SendMessage("Este livro ainda não está configurado.");
                return;
            }

            string cityName = ReinoElectionsSystem.GetCityName(CityId);

            if (String.IsNullOrWhiteSpace(cityName) || cityName == "Cidade inválida")
            {
                pm.SendMessage("Cidade inválida.");
                return;
            }

            if (pm.IsCitizenOf(cityName))
            {
                pm.SendMessage("Você já é cidadão de " + cityName + ".");
                return;
            }

            TownHouseSign sign = FindValidResidence(pm, cityName);

            if (sign == null)
            {
                pm.SendMessage("Para registrar cidadania em " + cityName + ", você precisa possuir ou alugar uma casa residencial ligada a essa cidade.");
                return;
            }

            pm.OSUCitizenCityId = cityName;
            pm.SendMessage("Sua cidadania agora é: " + cityName + ".");
        }

        private TownHouseSign FindValidResidence(PlayerMobile pm, string cityName)
        {
            if (pm == null || String.IsNullOrWhiteSpace(cityName))
                return null;

            foreach (object obj in TownHouseSign.AllSigns)
            {
                TownHouseSign sign = obj as TownHouseSign;

                if (sign == null || sign.Deleted)
                    continue;

                if (sign.PropertyType != OSUPropertyType.House)
                    continue;

                if (String.IsNullOrWhiteSpace(sign.CitizenCityId))
                    continue;

                if (!String.Equals(sign.CitizenCityId, cityName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!sign.Owned || sign.House == null || sign.House.Deleted)
                    continue;

                if (sign.House.Owner == pm)
                    return sign;
            }

            return null;
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
        }
    }
}
