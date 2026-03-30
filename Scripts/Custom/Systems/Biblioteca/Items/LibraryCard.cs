using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Biblioteca.Library
{
    public class LibraryCard : Item
    {
        private DateTime _lastFeePaidUtc;

        public override string DefaultName
        {
            get { return "Cartão da Biblioteca"; }
        }

        [Constructable]
        public LibraryCard() : base(0x12AC) // deed-ish
        {
            LootType = LootType.Blessed;
            Movable = true;
            _lastFeePaidUtc = DateTime.UtcNow;
        }

        public LibraryCard(Serial serial) : base(serial)
        {
        }

        public bool EnsureWeeklyFee(PlayerMobile pm, out string failReason)
        {
            failReason = null;

            if (pm == null)
            {
                failReason = "Jogador inválido.";
                return false;
            }

            // cobra 10 moedas por semana, retirando do banco
            TimeSpan elapsed = DateTime.UtcNow - _lastFeePaidUtc;
            if (elapsed.TotalDays < 7.0)
                return true;

            int weeks = (int)Math.Floor(elapsed.TotalDays / 7.0);
            if (weeks < 1) weeks = 1;

            int cost = weeks * 10;

            if (!Banker.Withdraw(pm, cost))
            {
                failReason = string.Format("Você precisa de {0} moedas no banco para manter o cartão ativo.", cost);
                return false;
            }

            _lastFeePaidUtc = _lastFeePaidUtc.AddDays(weeks * 7.0);
            InvalidateProperties();
            return true;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1060662, "{0}\t{1}", "Taxa semanal", "10 moedas (banco)");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
            writer.Write(_lastFeePaidUtc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            _lastFeePaidUtc = reader.ReadDateTime();
            LootType = LootType.Newbied;
        }
    }
}
