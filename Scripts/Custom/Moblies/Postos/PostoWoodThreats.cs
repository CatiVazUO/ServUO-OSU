using Server;

namespace Server.Mobiles
{
    [CorpseName("um cadáver de lobo do pinhal")]
    public class LoboDoPinhal : BasePostoThreat
    {
        [Constructable]
        public LoboDoPinhal() : base("um lobo do pinhal", 225, 0xE5, PostoThreatTier.SmallWood, AIType.AI_Melee, FightMode.Closest, -1, false, false)
        {
        }

        public LoboDoPinhal(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de hárpia do pinhal")]
    public class HarpiaDoPinhal : BasePostoThreat
    {
        [Constructable]
        public HarpiaDoPinhal() : base("uma hárpia do pinhal", 30, 402, PostoThreatTier.SmallWood, AIType.AI_Melee, FightMode.Closest, 2411, true, false)
        {
        }

        public HarpiaDoPinhal(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de aranha do emaranhado")]
    public class AranhaEmaranhada : BasePostoThreat
    {
        [Constructable]
        public AranhaEmaranhada() : base("uma aranha do emaranhado", 28, 0x388, PostoThreatTier.SmallWood, AIType.AI_Melee, FightMode.Closest, 0, false, false)
        {
            SetDamageType(ResistanceType.Physical, 80);
            SetDamageType(ResistanceType.Poison, 20);
            SetResistance(ResistanceType.Poison, 25, 35);
        }

        public AranhaEmaranhada(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de ettin madeireiro")]
    public class EttinMadeireiro : BasePostoThreat
    {
        [Constructable]
        public EttinMadeireiro() : base("um ettin madeireiro", 18, 367, PostoThreatTier.SmallWood, AIType.AI_Melee, FightMode.Closest, 0, false, true)
        {
            SetStr(120, 145);
            SetDex(65, 85);
            SetDamage(7, 10);
            VirtualArmor = 34;
        }

        public EttinMadeireiro(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de reaper talhador")]
    public class ReaperTalhador : BasePostoThreat
    {
        [Constructable]
        public ReaperTalhador() : base("um reaper talhador", 47, 442, PostoThreatTier.LargeWood, AIType.AI_Mage, FightMode.Closest, 2213, false, false)
        {
            SetDamageType(ResistanceType.Physical, 80);
            SetDamageType(ResistanceType.Poison, 20);
            SetResistance(ResistanceType.Poison, 25, 35);
        }

        public ReaperTalhador(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de corpser trilheiro")]
    public class CorpserTrilheiro : BasePostoThreat
    {
        [Constructable]
        public CorpserTrilheiro() : base("um corpser trilheiro", 8, 684, PostoThreatTier.LargeWood, AIType.AI_Melee, FightMode.Closest, 2212, false, false)
        {
            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Poison, 25);
            SetResistance(ResistanceType.Poison, 20, 30);
        }

        public CorpserTrilheiro(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de centauro hostil")]
    public class CentauroHostil : BasePostoThreat
    {
        [Constructable]
        public CentauroHostil() : base("um centauro hostil", 101, 679, PostoThreatTier.LargeWood, AIType.AI_Melee, FightMode.Aggressor, 0, false, false)
        {
            SetDex(75, 95);
            SetDamage(8, 14);
        }

        public CentauroHostil(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de lobo da névoa")]
    public class LoboDaNevoa : BasePostoThreat
    {
        [Constructable]
        public LoboDaNevoa() : base("um lobo da névoa", 23, 0xE5, PostoThreatTier.LargeWood, AIType.AI_Melee, FightMode.Closest, 1150, false, false)
        {
            SetDex(80, 100);
        }

        public LoboDaNevoa(Serial serial) : base(serial)
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