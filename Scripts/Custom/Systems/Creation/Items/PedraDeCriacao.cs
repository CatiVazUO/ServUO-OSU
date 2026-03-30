using System;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Creation.Gumps;

namespace Server.Custom.Items
{
    public class PedraDeCriacao : Item
    {
        [Constructable]
        public PedraDeCriacao() : base(0x0ED4)
        {
            Name = "Pedra de Criação";
            Movable = false;
        }

        public PedraDeCriacao(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            if (!(from is PlayerMobile pm))
            {
                from.SendMessage("Apenas jogadores podem usar isso.");
                return;
            }

            // Apaga TODOS os itens equipados (exceto a mochila)
            for (int i = 0; i < 256; i++)
            {
                Layer layer = (Layer)i;

                if (layer == Layer.Backpack)
                    continue;

                Item it = pm.FindItemOnLayer(layer);

                if (it != null && !it.Deleted)
                    it.Delete();
            }

            // Apaga tudo que está na mochila do jogador
            if (pm.Backpack != null)
            {
                pm.Backpack.Delete();
                pm.AddItem(new Backpack()); // cria uma mochila nova vazia
            }

            // 1) Reset TOTAL do personagem (como você pediu)
            pm.ResetForOSUCreation();

            // Reset visual (cabelo/barba/cor)
            pm.HairItemID = 0;
            pm.HairHue = 0;
            pm.FacialHairItemID = 0;
            pm.FacialHairHue = 0;

            // opcional: reset da pele (pra gump voltar ao default 1001)
            pm.Hue = 0;


            // 2) Abre o gump de criação na Página 1
            pm.CloseGump(typeof(OSUCreationGump));
            pm.SendGump(new OSUCreationGump(pm, 1));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
