using System;
using Server.Accounting;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Systems.Creation.Items
{
    public class OSUPermaDeathPortal : Item
    {
        [Constructable]
        public OSUPermaDeathPortal() : base(0x1F04) // itemid de "portal" (troque se quiser)
        {
            Name = "Portal do Recomeço";
            Movable = false;
            Hue = 1153; // opcional
        }

        public OSUPermaDeathPortal(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            TryOpen(from);
        }

        public override bool OnMoveOver(Mobile m)
        {
            // Ao atravessar/pisar, abre confirmação também
            TryOpen(m);
            return true;
        }

        private void TryOpen(Mobile from)
        {
            if (from == null || from.Deleted)
                return;

            if (!(from is PlayerMobile pm))
            {
                from.SendMessage(0x22, "Apenas jogadores podem usar este portal.");
                return;
            }

            // Regras: só perma-dead pode usar
            // Ajuste o nome do campo se você usou outro
            if (!pm.OSUPermaDead)
            {
                pm.SendMessage(0x22, "Você não pode usar este portal.");
                return;
            }

            // Evita spam de gumps
            pm.CloseGump(typeof(OSUPermaDeathConfirmGump));
            pm.SendGump(new OSUPermaDeathConfirmGump(pm));
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

        private class OSUPermaDeathConfirmGump : Gump
        {
            private readonly PlayerMobile _pm;

            public OSUPermaDeathConfirmGump(PlayerMobile pm) : base(50, 50)
            {
                _pm = pm;

                Closable = true;
                Dragable = true;
                Resizable = false;

                AddPage(0);
                AddBackground(0, 0, 420, 240, 9270);

                AddLabel(30, 20, 0x34, "Portal do Recomeço");
                AddHtml(30, 55, 360, 110,
                    "<BASEFONT COLOR=#FFFFFF>" +
                    "Ao confirmar, este personagem será <B>deletado permanentemente</B> e " +
                    "você poderá criar um novo personagem nesta conta.<BR><BR>" +
                    "Se você acha que houve erro na morte, clique em <B>Cancelar</B> e chame a staff." +
                    "</BASEFONT>", false, true);

                AddButton(60, 185, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddLabel(95, 185, 0x44, "Confirmar (Deletar)");

                AddButton(240, 185, 4017, 4019, 0, GumpButtonType.Reply, 0);
                AddLabel(275, 185, 0x44, "Cancelar");
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (_pm == null || _pm.Deleted)
                    return;

                if (info.ButtonID != 1)
                {
                    _pm.SendMessage(0x22, "Ação cancelada. Aguarde a staff se necessário.");
                    return;
                }

                // Segurança final
                if (!_pm.OSUPermaDead)
                {
                    _pm.SendMessage(0x22, "Você não pode usar este portal.");
                    return;
                }

                try
                {
                    RemoveCharacterFromAccount(_pm);

                    // Deleta o mobile
                    _pm.Delete();

                    // Desconecta para voltar ao login/seleção
                    sender?.Dispose();
                }
                catch (Exception ex)
                {
                    // Se der algo errado, não queremos crashar
                    Console.WriteLine("OSUPermaDeathPortal: erro ao deletar personagem: " + ex);
                    _pm.SendMessage(0x22, "Erro ao deletar personagem. Chame a staff.");
                }
            }

            private static void RemoveCharacterFromAccount(PlayerMobile pm)
            {
                Account acct = pm.Account as Account;
                if (acct == null)
                    return;

                for (int i = 0; i < acct.Length; i++)
                {
                    if (acct[i] == pm)
                    {
                        acct[i] = null;
                        break;
                    }
                }
            }
        }
    }
}
