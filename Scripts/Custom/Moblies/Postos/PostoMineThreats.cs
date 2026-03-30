using Server;

namespace Server.Mobiles
{
    [CorpseName("um cadáver de morcego cavernícola")]
    public class MorcegoCavernicolo : BasePostoThreat
    {
        [Constructable]
        public MorcegoCavernicolo() : base("um morcego cavernícola", 39, 422, PostoThreatTier.SmallIron, AIType.AI_Melee, FightMode.Closest, 1109, true, false)
        {
        }

        public MorcegoCavernicolo(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de rato saqueador")]
    public class RatoSaqueador : BasePostoThreat
    {
        [Constructable]
        public RatoSaqueador() : base("um rato saqueador", 42, 437, PostoThreatTier.SmallIron, AIType.AI_Melee, FightMode.Closest, 2419, false, true)
        {
            SetDex(85, 105);
        }

        public RatoSaqueador(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de lodo férrico")]
    public class LodoFerrico : BasePostoThreat
    {
        [Constructable]
        public LodoFerrico() : base("um lodo férrico", 51, 456, PostoThreatTier.SmallIron, AIType.AI_Melee, FightMode.Closest, 2413, false, false)
        {
            SetDamageType(ResistanceType.Physical, 70);
            SetDamageType(ResistanceType.Poison, 30);
            SetResistance(ResistanceType.Poison, 25, 35);
        }

        public LodoFerrico(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de xisto vivo")]
    public class XistoVivo : BasePostoThreat
    {
        [Constructable]
        public XistoVivo() : base("um xisto vivo", 14, 268, PostoThreatTier.SmallIron, AIType.AI_Melee, FightMode.Closest, 2401, false, false)
        {
            SetStr(105, 130);
            SetDex(65, 85);
            SetDamage(6, 10);
            SetResistance(ResistanceType.Physical, 30, 35);
            VirtualArmor = 30;
        }

        public XistoVivo(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de gárgula da pedreira")]
    public class GargulaPedreira : BasePostoThreat
    {
        [Constructable]
        public GargulaPedreira() : base("uma gárgula da pedreira", 4, 372, PostoThreatTier.LargeIron, AIType.AI_Melee, FightMode.Closest, 2406, true, false)
        {
        }

        public GargulaPedreira(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de troll das galerias")]
    public class TrollGaleria : BasePostoThreat
    {
        [Constructable]
        public TrollGaleria() : base("um troll das galerias", Utility.RandomList(53, 54), 461, PostoThreatTier.LargeIron, AIType.AI_Melee, FightMode.Closest, 0, false, true)
        {
            SetStr(150, 180);
            SetDamage(8, 15);
            VirtualArmor = 40;
        }

        public TrollGaleria(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de imp carvoeiro")]
    public class ImpCarvoeiro : BasePostoThreat
    {
        [Constructable]
        public ImpCarvoeiro() : base("um imp carvoeiro", 74, 422, PostoThreatTier.LargeIron, AIType.AI_Mage, FightMode.Closest, 2407, true, false)
        {
            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Fire, 50);
            SetResistance(ResistanceType.Fire, 30, 40);
        }

        public ImpCarvoeiro(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de besouro de magma")]
    public class BesouroMagma : BasePostoThreat
    {
        [Constructable]
        public BesouroMagma() : base("um besouro de magma", 0xA9, 0x21D, PostoThreatTier.LargeIron, AIType.AI_Melee, FightMode.Closest, 0x489, false, false)
        {
            SetDamageType(ResistanceType.Physical, 25);
            SetDamageType(ResistanceType.Fire, 75);
            SetResistance(ResistanceType.Fire, 45, 55);
            SetResistance(ResistanceType.Cold, 10, 20);
        }

        public BesouroMagma(Serial serial) : base(serial)
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