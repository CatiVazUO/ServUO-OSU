using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public class UrnaDoReino : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get; set; }

        [Constructable]
        public UrnaDoReino() : this(0)
        {
        }

        [Constructable]
        public UrnaDoReino(int cityId) : base(0x0ED4)
        {
            Movable = false;
            Weight = 1.0;
            Hue = 0;
            CityId = cityId;

            UpdateName();
        }

        public UrnaDoReino(Serial serial) : base(serial)
        {
        }

        private void UpdateName()
        {
            Name = "urna de " + ReinoElectionsSystem.GetCityName(CityId);
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais da urna.");
                return;
            }

            ReinoFase phase = ReinoElectionsSystem.GetCurrentPhase();

            if (phase == ReinoFase.Votacao && ReinoElectionsSystem.HasPlayerVoted(pm))
            {
                pm.SendMessage("Seu voto já foi computado nesta eleição.");
                return;
            }

            if (phase == ReinoFase.Candidatura || phase == ReinoFase.Votacao)
            {
                pm.CloseGump(typeof(OSUUrnaGump));
                pm.SendGump(new OSUUrnaGump(pm, this));
                return;
            }

            if (ReinoElectionsSystem.HasVisibleResultForCity(CityId))
            {
                pm.CloseGump(typeof(OSUResultadoEleicaoGump));
                pm.SendGump(ReinoElectionsSystem.CreateResultGump(pm, CityId));
                return;
            }

            pm.CloseGump(typeof(OSUUrnaGump));
            pm.SendGump(new OSUUrnaGump(pm, this));
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
