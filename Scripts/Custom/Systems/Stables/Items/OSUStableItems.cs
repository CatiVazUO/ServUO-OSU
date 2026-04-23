using System;
using Server;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Custom.Systems.Stables.Engine;
using Server.Custom.Systems.Stables.Gumps;

namespace Server.Custom.Systems.Stables.Engine
{
    public class OSUAnimalMuzzle : BaseTool
    {
        private int m_BoundPetSerial;

        public override CraftSystem CraftSystem
        {
            get { return null; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BoundPetSerial
        {
            get { return m_BoundPetSerial; }
            set { m_BoundPetSerial = value; }
        }

        [Constructable]
        public OSUAnimalMuzzle() : base(40, 0x1374)
        {
            Name = "focinheira";
            Weight = 1.0;
        }

        public OSUAnimalMuzzle(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!BaseTool.CheckAccessible(this, from, true))
                return;

            from.Target = new InternalTarget(this);
            from.SendMessage("Escolha o animal que você quer analisar.");
        }

        private class InternalTarget : Target
        {
            private readonly OSUAnimalMuzzle _muzzle;

            public InternalTarget(OSUAnimalMuzzle muzzle) : base(2, false, TargetFlags.None)
            {
                _muzzle = muzzle;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature pet = targeted as BaseCreature;
                if (_muzzle == null || _muzzle.Deleted || pet == null)
                    return;

                string reason;
                if (!OSUStablePetSystem.CanAnalyzeWithMuzzle(_muzzle, pet, from, out reason))
                {
                    from.SendMessage(reason);
                    return;
                }

                if (_muzzle.BoundPetSerial == 0)
                {
                    _muzzle.BoundPetSerial = pet.Serial.Value;
                    from.SendMessage("Essa focinheira agora está vinculada a esse animal.");
                }
                else if (_muzzle.BoundPetSerial != pet.Serial.Value)
                {
                    from.SendMessage("Essa focinheira já está vinculada a outro animal.");
                    return;
                }

                if (_muzzle.UsesRemaining > 0)
                    _muzzle.UsesRemaining--;

                if (_muzzle.UsesRemaining <= 0)
                {
                    from.SendMessage("A focinheira se desgastou e não pode mais ser usada.");
                    _muzzle.Delete();
                }

                from.CloseGump(typeof(OSUAnimalStatusGump));
                from.SendGump(new OSUAnimalStatusGump(from, pet));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_BoundPetSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 1)
                m_BoundPetSerial = reader.ReadInt();
        }
    }

    public class OSUMilkingBucket : Item
    {
        private int m_MilkUnits;
        private DateTime m_SpoilsAtUtc;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MilkUnits { get { return m_MilkUnits; } set { m_MilkUnits = Math.Max(0, Math.Min(5, value)); InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SpoilsAtUtc { get { return m_SpoilsAtUtc; } set { m_SpoilsAtUtc = value; InvalidateProperties(); } }

        [Constructable]
        public OSUMilkingBucket() : base(0x14E0)
        {
            Name = "balde de ordenha";
            Weight = 3.0;
        }

        public OSUMilkingBucket(Serial serial) : base(serial)
        {
        }

        public bool IsFilled
        {
            get { return m_MilkUnits > 0; }
        }

        public bool IsSpoiled
        {
            get { return IsFilled && m_SpoilsAtUtc != DateTime.MinValue && DateTime.UtcNow >= m_SpoilsAtUtc; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("O balde precisa estar na sua mochila.");
                return;
            }

            if (IsSpoiled)
            {
                from.SendMessage("O leite nesse balde estragou.");
                m_MilkUnits = 0;
                m_SpoilsAtUtc = DateTime.MinValue;
                InvalidateProperties();
                return;
            }

            if (!IsFilled)
            {
                from.Target = new MilkAnimalTarget(this);
                from.SendMessage("Escolha a vaca ou cabra fêmea que você quer ordenhar.");
            }
            else
            {
                from.Target = new UseMilkTarget(this);
                from.SendMessage("Escolha uma pitcher para despejar o leite, você mesmo para beber, ou um alvo de culinária.");
            }
        }

        private void FillBucket(Mobile from)
        {
            m_MilkUnits = 5;
            m_SpoilsAtUtc = DateTime.UtcNow + TimeSpan.FromDays(3.0);
            InvalidateProperties();
            from.SendMessage("Você encheu o balde de ordenha com leite.");
        }

        private bool CheckFresh(Mobile from)
        {
            if (!IsSpoiled)
                return true;

            from.SendMessage("O leite nesse balde estragou.");
            m_MilkUnits = 0;
            m_SpoilsAtUtc = DateTime.MinValue;
            InvalidateProperties();
            return false;
        }

        private class MilkAnimalTarget : Target
        {
            private readonly OSUMilkingBucket _bucket;

            public MilkAnimalTarget(OSUMilkingBucket bucket) : base(2, false, TargetFlags.None)
            {
                _bucket = bucket;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_bucket == null || _bucket.Deleted || _bucket.IsFilled)
                    return;

                if (targeted is Cow)
                {
                    Cow cow = (Cow)targeted;
                    if (!cow.Female)
                    {
                        from.SendMessage("Somente vacas fêmeas dão leite.");
                        return;
                    }

                    if (cow.TryMilk(from))
                        _bucket.FillBucket(from);

                    return;
                }

                if (targeted is Goat)
                {
                    Goat goat = (Goat)targeted;
                    if (!goat.Female)
                    {
                        from.SendMessage("Somente cabras fêmeas dão leite.");
                        return;
                    }

                    if (goat.TryMilk(from))
                        _bucket.FillBucket(from);

                    return;
                }

                from.SendMessage("Esse balde só serve para tirar leite de vaca ou cabra.");
            }
        }

        private class UseMilkTarget : Target
        {
            private readonly OSUMilkingBucket _bucket;

            public UseMilkTarget(OSUMilkingBucket bucket) : base(2, false, TargetFlags.None)
            {
                _bucket = bucket;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_bucket == null || _bucket.Deleted || !_bucket.CheckFresh(from) || !_bucket.IsFilled)
                    return;

                if (targeted == from || targeted == _bucket)
                {
                    _bucket.DrinkOne(from);
                    return;
                }

                if (targeted is OSUSpoilingMilkPitcher || targeted is Pitcher)
                {
                    _bucket.PourIntoPitcher(from, targeted as Item);
                    return;
                }

                if (_bucket.TryHandleCookingPlaceholder(from, targeted as Item))
                    return;

                from.SendMessage("Por enquanto esse leite só pode ser despejado em uma pitcher ou usado em processos de cozinha.");
            }
        }

        private void DrinkOne(Mobile from)
        {
            if (!CheckFresh(from) || !IsFilled)
                return;

            PlayerMobile pm = from as PlayerMobile;
            if (pm != null)
            {
                pm.OSUThirst = Math.Min(100, pm.OSUThirst + 5);
                pm.OSUHunger = Math.Min(100, pm.OSUHunger + 2);
            }

            m_MilkUnits--;
            if (m_MilkUnits <= 0)
                m_SpoilsAtUtc = DateTime.MinValue;

            InvalidateProperties();
            from.SendMessage("Você bebe um gole do leite.");
        }

        private void PourIntoPitcher(Mobile from, Item item)
        {
            if (item == null)
                return;

            OSUSpoilingMilkPitcher pitcher = item as OSUSpoilingMilkPitcher;
            if (pitcher == null)
                pitcher = ConvertPitcher(item);

            if (pitcher == null)
            {
                from.SendMessage("Essa pitcher não pode receber esse leite.");
                return;
            }

            if (pitcher.IsSpoiled)
                pitcher.EmptyOut();

            if (!pitcher.IsEmpty && pitcher.Content != BeverageType.Milk)
            {
                from.SendMessage("Essa pitcher já contém outra bebida.");
                return;
            }

            int transferable = Math.Min(Math.Max(0, m_MilkUnits - 1), pitcher.MaxQuantity - pitcher.Quantity);
            if (transferable <= 0)
            {
                from.SendMessage("Não há espaço suficiente na pitcher.");
                return;
            }

            pitcher.Content = BeverageType.Milk;
            pitcher.Quantity += transferable;
            pitcher.SpoilsAtUtc = m_SpoilsAtUtc;
            m_MilkUnits -= transferable;

            if (m_MilkUnits <= 0)
                m_SpoilsAtUtc = DateTime.MinValue;

            InvalidateProperties();
            from.SendMessage("Você despejou leite na pitcher.");
        }

        private OSUSpoilingMilkPitcher ConvertPitcher(Item item)
        {
            OSUSpoilingMilkPitcher existing = item as OSUSpoilingMilkPitcher;
            if (existing != null)
                return existing;

            Pitcher pitcher = item as Pitcher;
            if (pitcher == null || pitcher.Deleted)
                return null;

            if (!pitcher.IsEmpty && pitcher.Content != BeverageType.Milk)
                return null;

            OSUSpoilingMilkPitcher replacement = new OSUSpoilingMilkPitcher();
            replacement.Content = pitcher.IsEmpty ? BeverageType.Milk : pitcher.Content;
            replacement.Quantity = pitcher.Quantity;

            Container parent = pitcher.Parent as Container;
            if (parent != null)
            {
                parent.DropItem(replacement);
                replacement.Location = pitcher.Location;
            }
            else if (pitcher.Map != null && pitcher.Map != Map.Internal)
            {
                replacement.MoveToWorld(pitcher.Location, pitcher.Map);
            }

            pitcher.Delete();
            return replacement;
        }

        private bool TryHandleCookingPlaceholder(Mobile from, Item item)
        {
            if (item == null)
                return false;

            // Placeholder para integrar depois com processos de cooking/queijos.
            return false;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_MilkUnits > 0)
                list.Add("Leite: " + m_MilkUnits + "/5");

            if (IsSpoiled)
                list.Add("Leite estragado");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_MilkUnits);
            writer.Write(m_SpoilsAtUtc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_MilkUnits = reader.ReadInt();
            m_SpoilsAtUtc = reader.ReadDateTime();
        }
    }

    public class OSUSpoilingMilkPitcher : Pitcher
    {
        private DateTime m_SpoilsAtUtc;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SpoilsAtUtc { get { return m_SpoilsAtUtc; } set { m_SpoilsAtUtc = value; InvalidateProperties(); } }

        public bool IsSpoiled
        {
            get { return !IsEmpty && Content == BeverageType.Milk && m_SpoilsAtUtc != DateTime.MinValue && DateTime.UtcNow >= m_SpoilsAtUtc; }
        }

        [Constructable]
        public OSUSpoilingMilkPitcher()
            : base(BeverageType.Milk)
        {
            Name = "pitcher de leite";
            Quantity = 0;
        }

        public OSUSpoilingMilkPitcher(Serial serial) : base(serial)
        {
        }

        public void EmptyOut()
        {
            Quantity = 0;
            m_SpoilsAtUtc = DateTime.MinValue;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsSpoiled)
            {
                from.SendMessage("O leite nessa pitcher estragou.");
                EmptyOut();
                return;
            }

            base.OnDoubleClick(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_SpoilsAtUtc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_SpoilsAtUtc = reader.ReadDateTime();
        }
    }

    public class OSUStableCastrationKnife : Item
    {
        [Constructable]
        public OSUStableCastrationKnife() : base(0x13F6)
        {
            Name = "faca de castração";
            Weight = 1.0;
        }

        public OSUStableCastrationKnife(Serial serial) : base(serial)
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

    public class OSUStableCastrationSewingKit : Item
    {
        [Constructable]
        public OSUStableCastrationSewingKit() : base(0xF9D)
        {
            Name = "kit de sutura de castração";
            Weight = 1.0;
        }

        public OSUStableCastrationSewingKit(Serial serial) : base(serial)
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

    public class OSUStableCastrationBandagePack : Item
    {
        [Constructable]
        public OSUStableCastrationBandagePack() : base(0xE21)
        {
            Name = "bandagens de castração";
            Weight = 1.0;
        }

        public OSUStableCastrationBandagePack(Serial serial) : base(serial)
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
