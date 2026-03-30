using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Reinos;

namespace Server.Custom.Systems.Reinos
{
    public class PedraDoReino : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get; set; }

        [Constructable]
        public PedraDoReino() : this(-1)
        {
        }

        [Constructable]
        public PedraDoReino(int cityId) : base(0x136C)
        {
            Movable = false;
            Weight = 255.0;
            CityId = cityId;
            Name = "pedra do reino";
            Hue = 1153;

            UpdateName();
        }

        public PedraDoReino(Serial serial) : base(serial)
        {
        }

        private void UpdateName()
        {
            switch (CityId)
            {
                case 0: Name = "pedra do governo de Aurora"; break;
                case 1: Name = "pedra do governo de Xetá"; break;
                case 2: Name = "pedra do governo de Lurone"; break;
                case 3: Name = "pedra do governo de Willran"; break;
                default: Name = "pedra do reino"; break;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais da pedra do reino.");
                return;
            }

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(pm, CityId))
            {
                pm.SendMessage("Somente membros do povo " + ReinoElectionsSystem.GetCityPeopleName(CityId) + " podem governar " + ReinoElectionsSystem.GetCityName(CityId) + ".");
                return;
            }

            if (!ReinoAccessHelper.HasGovernmentAccess(pm, CityId))
            {
                pm.SendMessage("Somente o governador ou alguém com a chave do governador pode usar esta pedra.");
                return;
            }

            pm.CloseGump(typeof(PedraDoReinoGump));
            pm.SendGump(new PedraDoReinoGump(pm, this));
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
                Name = "pedra do reino";

        }
    }

    public class PedraDoReinoGump : Gump
    {
        private readonly PedraDoReino _stone;

        public PedraDoReinoGump(PlayerMobile from, PedraDoReino stone) : base(100, 100)
        {
            _stone = stone;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 420, 280, 9270);
            AddAlphaRegion(10, 10, 400, 260);

            AddLabel(145, 20, 1152, "Controle do Reino");
            AddLabel(30, 60, 0, "Cidade ID:");
            AddLabel(140, 60, 1153, stone.CityId.ToString());

            AddLabel(30, 90, 0, "Acesso:");
            AddLabel(140, 90, 1153, "Liberado");

            AddLabel(30, 130, 0, "Este gump é a base da pedra.");
            AddLabel(30, 150, 0, "Agora a gente ainda vai plugar os sistemas");
            AddLabel(30, 170, 0, "de construção, impostos, decretos, etc.");

            AddButton(150, 220, 247, 248, 0, GumpButtonType.Reply, 0);
            AddLabel(185, 220, 0, "Fechar");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }
}
