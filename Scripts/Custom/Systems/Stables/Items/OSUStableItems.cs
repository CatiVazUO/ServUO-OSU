using Server;
using Server.Custom.Drinks;
using Server.Custom.Systems.Stables.Engine;
using Server.Custom.Systems.Stables.Gumps;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;

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

                from.Emote("*coloca uma focinheira no animal e o examina*");
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
        private OSUMilkKind m_MilkKind;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MilkUnits
        {
            get { return m_MilkUnits; }
            set { m_MilkUnits = Math.Max(0, Math.Min(5, value));
                InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SpoilsAtUtc
        {
            get { return m_SpoilsAtUtc; }
            set { m_SpoilsAtUtc = value;
                InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUMilkKind MilkKind
        {
            get { return m_MilkKind; }
            set { m_MilkKind = value;
                InvalidateProperties(); }
        }

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

            if (m_MilkUnits < 5)
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

        private void AddMilkUnit(Mobile from, OSUMilkKind kind)
        {
            if (kind == OSUMilkKind.None)
            {
                from.SendMessage("Tipo de leite inválido.");
                return;
            }

            if (m_MilkUnits > 0 && m_MilkKind != kind)
            {
                from.SendMessage("Esse balde já contém outro tipo de leite.");
                return;
            }

            if (m_MilkUnits < 5)
                m_MilkUnits++;

            m_MilkKind = kind;

            if (m_MilkUnits > 0 && m_SpoilsAtUtc == DateTime.MinValue)
                m_SpoilsAtUtc = DateTime.UtcNow + TimeSpan.FromDays(3.0);

            InvalidateProperties();

            from.Emote("*ordena cuidadosamente o animal*");
            from.SendMessage("Você colocou " + GetMilkKindLabel(kind) + " no balde. Agora ele está com " + m_MilkUnits + "/5.");
        }

        private string GetMilkKindLabel(OSUMilkKind kind)
        {
            switch (kind)
            {
                case OSUMilkKind.Cow: return "leite de vaca";
                case OSUMilkKind.Goat: return "leite de cabra";
                default: return "leite";
            }
        }

        private bool CheckFresh(Mobile from)
        {
            if (!IsSpoiled)
                return true;

            from.SendMessage("O leite nesse balde estragou.");
            m_MilkUnits = 0;
            m_SpoilsAtUtc = DateTime.MinValue;
            m_MilkKind = OSUMilkKind.None;
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
                if (_bucket == null || _bucket.Deleted || _bucket.MilkUnits >= 5)
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
                        _bucket.AddMilkUnit(from, OSUMilkKind.Cow);

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
                        _bucket.AddMilkUnit(from, OSUMilkKind.Goat);

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

                if (targeted is OSUCowMilkPitcher || targeted is OSUGoatMilkPitcher)
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
            {
                m_SpoilsAtUtc = DateTime.MinValue;
                m_MilkKind = OSUMilkKind.None;
            }

            InvalidateProperties();
            from.SendMessage("Você bebe um gole do leite.");
        }

        private void PourIntoPitcher(Mobile from, Item item)
        {
            if (item == null)
                return;

            OSUBaseMilkPitcher pitcher = item as OSUBaseMilkPitcher;
            if (pitcher == null)
            {
                from.SendMessage("Essa pitcher não pode receber esse leite.");
                return;
            }

            if (m_MilkUnits <= 0 || m_MilkKind == OSUMilkKind.None)
            {
                from.SendMessage("O balde está vazio.");
                return;
            }

            if (pitcher.MilkKind != m_MilkKind)
            {
                from.SendMessage("Essa pitcher é de outro tipo de leite.");
                return;
            }

            int transferable = Math.Min(Math.Max(0, m_MilkUnits - 1), pitcher.MaxQuantity - pitcher.Quantity);

            if (transferable <= 0)
            {
                from.SendMessage("Não há espaço suficiente na pitcher.");
                return;
            }

            int added = pitcher.TryAddUnits(transferable, m_SpoilsAtUtc);
            m_MilkUnits -= added;

            if (m_MilkUnits <= 0)
            {
                m_MilkUnits = 0;
                m_MilkKind = OSUMilkKind.None;
                m_SpoilsAtUtc = DateTime.MinValue;
            }

            InvalidateProperties();
            from.SendMessage("Você despejou " + GetMilkKindLabel(pitcher.MilkKind) + " na pitcher.");
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
            writer.Write(1);
            writer.Write(m_MilkUnits);
            writer.Write(m_SpoilsAtUtc);
            writer.Write((int)m_MilkKind);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_MilkUnits = reader.ReadInt();
            m_SpoilsAtUtc = reader.ReadDateTime();

            if (version >= 1)
                m_MilkKind = (OSUMilkKind)reader.ReadInt();
            else
                m_MilkKind = OSUMilkKind.None;
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
    public class OSUAnimalBrush : Item
    {
        [Constructable]
        public OSUAnimalBrush() : base(0x1373)
        {
            Name = "escova de cuidar animais";
            Weight = 1.0;
        }

        public OSUAnimalBrush(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            from.Target = new InternalTarget(this);
            from.SendMessage("Escolha o animal que você quer escovar.");
        }

        private class InternalTarget : Target
        {
            private readonly OSUAnimalBrush _brush;

            public InternalTarget(OSUAnimalBrush brush) : base(2, false, TargetFlags.None)
            {
                _brush = brush;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature pet = targeted as BaseCreature;
                if (_brush == null || _brush.Deleted || pet == null)
                    return;

                string reason;
                if (OSUStablePetSystem.TryGainLoyaltyFromCare(pet, from, 4, "brush", TimeSpan.FromHours(12.0), out reason))
                    from.Emote("*escova o animal com cuidado*");

                from.SendMessage(reason);
            }
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

    public class OSUHoofCareKit : Item
    {
        private int m_UsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set { m_UsesRemaining = Math.Max(0, value); InvalidateProperties(); }
        }

        [Constructable]
        public OSUHoofCareKit() : base(0xFB6)
        {
            Name = "kit de ferraduras";
            Weight = 2.0;
            m_UsesRemaining = 10;
        }

        public OSUHoofCareKit(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            from.Target = new InternalTarget(this);
            from.SendMessage("Escolha a montaria que terá as ferraduras revisadas.");
        }

        private class InternalTarget : Target
        {
            private readonly OSUHoofCareKit _kit;

            public InternalTarget(OSUHoofCareKit kit) : base(2, false, TargetFlags.None)
            {
                _kit = kit;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_kit == null || _kit.Deleted || _kit.UsesRemaining <= 0)
                    return;

                BaseCreature pet = targeted as BaseCreature;
                if (pet == null)
                    return;

                if (!(pet is BaseMount))
                {
                    from.SendMessage("Esse cuidado só faz sentido em montarias.");
                    return;
                }

                string reason;
                if (OSUStablePetSystem.TryGainLoyaltyFromCare(pet, from, 7, "hoof", TimeSpan.FromDays(3.0), out reason))
                {
                    _kit.UsesRemaining--;
                    from.Emote("*limpa os cascos e troca as ferraduras gastas*");
                    pet.Stam = pet.StamMax;

                    if (_kit.UsesRemaining <= 0)
                    {
                        from.SendMessage("O kit de ferraduras acabou.");
                        _kit.Delete();
                    }
                }

                from.SendMessage(reason);
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Usos restantes: {0}", m_UsesRemaining);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_UsesRemaining = reader.ReadInt();
        }
    }
}
