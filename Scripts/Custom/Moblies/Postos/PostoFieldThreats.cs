using Server;

namespace Server.Mobiles
{
    [CorpseName("um cadáver de rato de tulha")]
    public class RatoDeTulha : BasePostoThreat
    {
        [Constructable]
        public RatoDeTulha() : base("um rato de tulha", 0xD7, 0x188, PostoThreatTier.SmallCotton, AIType.AI_Melee, FightMode.Closest, 2409, false, true)
        {
        }

        public RatoDeTulha(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de lamalino dos canais")]
    public class LamalinoDosCanais : BasePostoThreat
    {
        [Constructable]
        public LamalinoDosCanais() : base("um lamalino dos canais", 779, 422, PostoThreatTier.SmallCotton, AIType.AI_Melee, FightMode.Closest, 2207, false, false)
        {
            SetResistance(ResistanceType.Cold, 15, 25);
            SetResistance(ResistanceType.Poison, 15, 25);
        }

        public LamalinoDosCanais(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de aranha do algodoeiro")]
    public class AranhaDoAlgodoeiro : BasePostoThreat
    {
        [Constructable]
        public AranhaDoAlgodoeiro() : base("uma aranha do algodoeiro", 28, 0x388, PostoThreatTier.SmallCotton, AIType.AI_Melee, FightMode.Closest, -1, false, false)
        {
            SetDamageType(ResistanceType.Physical, 85);
            SetDamageType(ResistanceType.Poison, 15);
            SetResistance(ResistanceType.Poison, 20, 30);
        }

        public AranhaDoAlgodoeiro(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de vulto da cerca")]
    public class VultoDaCerca : BasePostoThreat
    {
        [Constructable]
        public VultoDaCerca() : base("um vulto da cerca", 31, 0x39D, PostoThreatTier.SmallCotton, AIType.AI_Melee, FightMode.Closest, -1, false, false)
        {
            Hue = Utility.RandomSkinHue() & 0x7FFF;
            SetDamage(5, 8);
        }

        public VultoDaCerca(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de escorpião do baixio")]
    public class EscorpiaoDoBaixio : BasePostoThreat
    {
        [Constructable]
        public EscorpiaoDoBaixio() : base("um escorpião do baixio", 48, 397, PostoThreatTier.LargeCotton, AIType.AI_Melee, FightMode.Closest, -1, false, false)
        {
            SetDamageType(ResistanceType.Physical, 60);
            SetDamageType(ResistanceType.Poison, 40);
            SetResistance(ResistanceType.Poison, 35, 45);
        }

        public EscorpiaoDoBaixio(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de lagarto do vau")]
    public class LagartoDoVau : BasePostoThreat
    {
        [Constructable]
        public LagartoDoVau() : base("um lagarto do vau", Utility.RandomList(35, 36), 417, PostoThreatTier.LargeCotton, AIType.AI_Melee, FightMode.Closest, 2206, false, true)
        {
            SetDex(75, 95);
        }

        public LagartoDoVau(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de hárpia do moinho")]
    public class HarpiaDoMoinho : BasePostoThreat
    {
        [Constructable]
        public HarpiaDoMoinho() : base("uma hárpia do moinho", 73, 402, PostoThreatTier.LargeCotton, AIType.AI_Melee, FightMode.Closest, 2411, true, false)
        {
            SetDex(80, 100);
            SetResistance(ResistanceType.Physical, 40, 50);
            VirtualArmor = 42;
        }

        public HarpiaDoMoinho(Serial serial) : base(serial)
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

    [CorpseName("um cadáver de lodo do alagado")]
    public class LodoDoAlagado : BasePostoThreat
    {
        [Constructable]
        public LodoDoAlagado() : base("um lodo do alagado", 51, 456, PostoThreatTier.LargeCotton, AIType.AI_Melee, FightMode.Closest, 2208, false, false)
        {
            SetDamageType(ResistanceType.Physical, 70);
            SetDamageType(ResistanceType.Poison, 30);
            SetResistance(ResistanceType.Poison, 30, 40);
        }

        public LodoDoAlagado(Serial serial) : base(serial)
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