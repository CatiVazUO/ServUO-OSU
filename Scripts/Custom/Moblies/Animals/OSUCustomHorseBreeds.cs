using Server.ContextMenus;
using Server.Items;
using System;
using Server.Custom.Systems.Stables.Engine;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public abstract class OSUBaseHorseBreed : BaseMount
    {
        private readonly int[] _possibleHues;

        public override int Meat { get { return 3; } }
        public override int Hides { get { return 10; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

        protected OSUBaseHorseBreed(
            string displayName,
            int body,
            int itemId,
            int[] possibleHues,
            int strMin,
            int strMax,
            int dexMin,
            int dexMax,
            int intMin,
            int intMax,
            int hitsMin,
            int hitsMax,
            int damageMin,
            int damageMax,
            double minTameSkill)
            : base(displayName, body, itemId, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            _possibleHues = possibleHues ?? new int[0];

            BaseSoundID = 0xA8;
            Female = Utility.RandomBool();

            if (_possibleHues.Length > 0)
                Hue = _possibleHues[Utility.Random(_possibleHues.Length)];
            else
                Hue = 0;

            SetStr(strMin, strMax);
            SetDex(dexMin, dexMax);
            SetInt(intMin, intMax);

            SetHits(hitsMin, hitsMax);
            SetMana(0);
            SetDamage(damageMin, damageMax);

            SetDamageType(ResistanceType.Physical, 100);
            SetResistance(ResistanceType.Physical, 15, 25);

            SetSkill(SkillName.MagicResist, 25.1, 35.0);
            SetSkill(SkillName.Tactics, 29.3, 49.0);
            SetSkill(SkillName.Wrestling, 29.3, 49.0);

            Fame = 300;
            Karma = 300;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = minTameSkill;
        }

        protected OSUBaseHorseBreed(Serial serial)
            : base(serial)
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

    // Hues naturais placeholder. Se alguma não agradar visualmente, basta trocar os números.
    public static class OSUHorseNaturalHues
    {
        public static readonly int[] Dark = new int[] { 0, 1811, 1812, 1813 };
        public static readonly int[] Brown = new int[] { 0, 1820, 1821, 1822 };
        public static readonly int[] Light = new int[] { 0, 1814, 1815, 1816 };
        public static readonly int[] Grey = new int[] { 0, 1801, 1802, 1803 };
        public static readonly int[] Cream = new int[] { 0, 1817, 1818, 1819 };
    }

    [CorpseName("a horse corpse")]
    public class HorseFrisio : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseFrisio() : base("um frisio", 0xC8, 0x3E9F, OSUHorseNaturalHues.Dark, 70, 90, 55, 70, 8, 14, 42, 55, 4, 5, 39.1)
        {
        }

        public HorseFrisio(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseMustang : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseMustang() : base("um mustang", 0xCC, 0x3EA2, OSUHorseNaturalHues.Brown, 55, 80, 65, 85, 6, 12, 38, 50, 3, 5, 29.1)
        {
        }

        public HorseMustang(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorsePuroSangue : OSUBaseHorseBreed
    {
        [Constructable]
        public HorsePuroSangue() : base("um puro sangue", 0xE2, 0x3EA0, OSUHorseNaturalHues.Grey, 60, 85, 70, 90, 8, 14, 40, 52, 4, 5, 49.1)
        {
        }

        public HorsePuroSangue(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseMangaLarga : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseMangaLarga() : base("um manga larga", 0xE2, 0x3EA0, OSUHorseNaturalHues.Cream, 50, 72, 60, 82, 8, 12, 36, 48, 3, 4, 34.1)
        {
        }

        public HorseMangaLarga(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseAndaluz : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseAndaluz() : base("um andaluz", 0xE4, 0x3EA1, OSUHorseNaturalHues.Light, 58, 82, 62, 84, 8, 14, 39, 52, 3, 5, 44.1)
        {
        }

        public HorseAndaluz(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseShire : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseShire() : base("um shire", 1600, 0x3EE3, null, 85, 110, 45, 60, 8, 12, 48, 62, 4, 6, 59.1)
        {
        }

        public HorseShire(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseArdennes : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseArdennes() : base("um ardennes", 1601, 0x3EE4, null, 90, 115, 40, 58, 8, 12, 50, 64, 4, 6, 64.1)
        {
        }

        public HorseArdennes(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseClaydesdale : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseClaydesdale() : base("um claydesdale", 1602, 0x3EE5, null, 88, 112, 44, 60, 8, 12, 49, 63, 4, 6, 62.1)
        {
        }

        public HorseClaydesdale(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseGypsy : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseGypsy() : base("um gypsy", 1603, 0x3EE6, null, 68, 88, 58, 75, 8, 14, 42, 55, 4, 5, 44.1)
        {
        }

        public HorseGypsy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseBelga : OSUBaseHorseBreed
    {
        [Constructable]
        public HorseBelga() : base("um belga", 1604, 0x3EE7, null, 92, 118, 38, 54, 8, 12, 51, 66, 4, 6, 69.1)
        {
        }

        public HorseBelga(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a horse corpse")]
    public class HorseFrisioCarga : OSUBaseHorseBreed
    {
        private int m_CargoBagUsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CargoBagUsesRemaining
        {
            get { return m_CargoBagUsesRemaining; }
            set
            {
                m_CargoBagUsesRemaining = Math.Max(0, value);
                InvalidateProperties();
            }
        }

        [Constructable]
        public HorseFrisioCarga()
            : base("um frisio de carga", 0xC8, 0x3E9F, OSUHorseNaturalHues.Dark, 70, 90, 55, 70, 8, 14, 42, 55, 4, 5, 39.1)
        {
            EnsureCargoBackpack();
            m_CargoBagUsesRemaining = 50;
        }

        public HorseFrisioCarga(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAutoStable
        {
            get { return (Backpack == null || Backpack.Items.Count == 0) && base.CanAutoStable; }
        }

        public int GetCargoMaxWeight()
        {
            return Math.Max(200, Math.Min(500, RawStr * 4));
        }

        public int GetCargoFreeWeight()
        {
            Container pack = Backpack;
            int total = pack != null ? (int)pack.TotalWeight : 0;
            return Math.Max(0, GetCargoMaxWeight() - total);
        }

        public void EnsureCargoBackpack()
        {
            if (Backpack is OSUFrisioCargoBackpack)
            {
                Backpack.Movable = false;
                return;
            }

            Container old = Backpack;
            OSUFrisioCargoBackpack pack = new OSUFrisioCargoBackpack();
            pack.Movable = false;
            AddItem(pack);

            if (old != null && old != pack && !old.Deleted)
            {
                while (old.Items.Count > 0)
                {
                    Item item = old.Items[0];
                    if (item == null || item.Deleted)
                        continue;

                    pack.DropItem(item);
                }

                old.Delete();
            }
        }

        public void TryOpenCargoPack(Mobile from)
        {
            if (IsDeadPet)
                return;

            if (!PackAnimal.CheckAccess(this, from))
            {
                from.SendMessage("Esse cavalo não permite que você abra a carga dele.");
                return;
            }

            if (Rider != null)
            {
                from.SendMessage("Desmonte o cavalo antes de abrir a carga.");
                return;
            }

            EnsureCargoBackpack();

            if (m_CargoBagUsesRemaining <= 0)
            {
                from.SendMessage("A bolsa de carga se desgastou totalmente.");
                OSUFrisioCargoUtility.CollapseCargoHorse(this);
                return;
            }

            m_CargoBagUsesRemaining--;
            InvalidateProperties();

            if (m_CargoBagUsesRemaining <= 0)
            {
                from.SendMessage("Ao tentar abrir a carga, a bolsa se rompe e o cavalo volta à forma normal.");
                OSUFrisioCargoUtility.CollapseCargoHorse(this);
                return;
            }

            from.Use(Backpack);
        }

        public override bool OnBeforeDeath()
        {
            if (!base.OnBeforeDeath())
                return false;

            PackAnimal.CombineBackpacks(this);
            return true;
        }

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            return DeathMoveResult.MoveToCorpse;
        }

        public override bool IsSnoop(Mobile from)
        {
            if (PackAnimal.CheckAccess(this, from))
                return false;

            return base.IsSnoop(from);
        }

        public override bool OnDragDrop(Mobile from, Item item)
        {
            if (CheckFeed(from, item))
                return true;

            if (PackAnimal.CheckAccess(this, from))
            {
                EnsureCargoBackpack();
                return AddToBackpack(item);
            }

            return base.OnDragDrop(from, item);
        }

        public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
        {
            return PackAnimal.CheckAccess(this, from);
        }

        public override bool CheckNonlocalLift(Mobile from, Item item)
        {
            return PackAnimal.CheckAccess(this, from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsDeadPet && from.InRange(this, 1) && PackAnimal.CheckAccess(this, from))
            {
                if (GetCargoFreeWeight() < 150)
                {
                    from.SendMessage("Esse cavalo está carregado demais para levar um cavaleiro. Ele precisa ter pelo menos 150 stones livres.");
                    return;
                }
            }

            base.OnDoubleClick(from);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (PackAnimal.CheckAccess(this, from))
                list.Add(new OSUCargoHorseBackpackEntry(this, from));
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            Container pack = Backpack;
            int itemCount = pack != null ? pack.Items.Count : 0;
            int totalWeight = pack != null ? (int)pack.TotalWeight : 0;
            list.Add("Usos da bolsa: " + m_CargoBagUsesRemaining);
            list.Add("Capacidade da bolsa: " + itemCount + "/30 itens, " + totalWeight + "/" + GetCargoMaxWeight() + " stones");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CargoBagUsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CargoBagUsesRemaining = reader.ReadInt();
            EnsureCargoBackpack();
        }
    }

    public class OSUFrisioCargoBackpack : StrongBackpack
    {
        public OSUFrisioCargoBackpack()
        {
            Movable = false;
        }

        public OSUFrisioCargoBackpack(Serial serial)
            : base(serial)
        {
        }

        public override int DefaultMaxWeight
        {
            get
            {
                HorseFrisioCarga horse = RootParent as HorseFrisioCarga;
                if (horse != null)
                    return horse.GetCargoMaxWeight();

                return 200;
            }
        }

        public override int DefaultMaxItems
        {
            get { return 30; }
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

    public class OSUCargoHorseBackpackEntry : ContextMenuEntry
    {
        private readonly HorseFrisioCarga m_Horse;
        private readonly Mobile m_From;

        public OSUCargoHorseBackpackEntry(HorseFrisioCarga horse, Mobile from)
            : base(6145, 3)
        {
            m_Horse = horse;
            m_From = from;

            if (horse == null || horse.IsDeadPet)
                Enabled = false;
        }

        public override void OnClick()
        {
            if (m_Horse != null && !m_Horse.Deleted)
                m_Horse.TryOpenCargoPack(m_From);
        }
    }
}
