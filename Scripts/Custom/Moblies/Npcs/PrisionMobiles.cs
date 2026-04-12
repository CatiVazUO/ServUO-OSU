using Server;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;


namespace Server.Custom.Reinos
{
    public abstract class ReinoPrisionNpcBase : BaseCreature
    {
        private DateTime m_NextSpeechUtc;
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; } }

        protected abstract string[] SpeechLines { get; }

        protected ReinoPrisionNpcBase(int cityId, string title)
            : base(AIType.AI_Animal, FightMode.None, 10, 1, 0.2, 0.4)
        {
            m_CityId = cityId;
            Title = title;

            Female = Utility.RandomBool();
            Body = Female ? 0x191 : 0x190;
            Hue = Utility.RandomSkinHue();
            CantWalk = true;
            InitStats(80, 80, 25);
            SpeechHue = Utility.RandomDyedHue();
            Name = NameList.RandomName(Female ? "female" : "male");
            AddItem(new Boots { Movable = false });
            AddItem(new LongPants { Movable = false });
            AddItem(new UniformeUnderShirt { Movable = false });
            ApplyUniform();
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();
            if (!Female)
            {
                FacialHairItemID = Race.RandomFacialHair(Female);
                FacialHairHue = Race.RandomHairHue();
            }
            m_NextSpeechUtc = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(25, 50));
        }

        public void ApplyUniform()
        {
            List<Item> toDelete = new List<Item>();

            for (int i = 0; i < Items.Count; i++)
            {
                Item item = Items[i];
                if (item != null && item.Layer == Layer.MiddleTorso)
                    toDelete.Add(item);
            }

            for (int i = 0; i < toDelete.Count; i++)
                toDelete[i].Delete();

            Item uniform = ReinoVisualSystem.CreateUniformForCity(m_CityId);
            if (uniform == null)
                uniform = new Tunic();

            uniform.Movable = false;
            AddItem(uniform);
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Deleted || SpeechLines == null || SpeechLines.Length == 0)
                return;

            if (DateTime.UtcNow >= m_NextSpeechUtc)
            {
                Say(SpeechLines[Utility.Random(SpeechLines.Length)]);
                m_NextSpeechUtc = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(35, 80));
            }
        }

        public ReinoPrisionNpcBase(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_NextSpeechUtc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_NextSpeechUtc = reader.ReadDateTime();
            CantWalk = true;
            Blessed = true;
            ApplyUniform();
        }
    }

    public class OSUCarcereiro : ReinoPrisionNpcBase
    {
        protected override string[] SpeechLines
        {
            get
            {
                return new string[]
                {
                    "Silêncio nas celas.",
                    "Ninguém sai sem ordem.",
                    "A comida já vai chegar.",
                    "Mantenham a disciplina.",
                    "A prisão observa tudo."
                };
            }
        }

        [Constructable]
        public OSUCarcereiro() : this(0)
        {
        }

        public OSUCarcereiro(int cityId) : base(cityId, "cacereiro")
        {
        }

        public OSUCarcereiro(Serial serial) : base(serial)
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

    public class OSUGuardaDePrisao : ReinoPrisionNpcBase
    {
        protected override string[] SpeechLines
        {
            get
            {
                return new string[]
                {
                    "De olho nas grades.",
                    "Nada de confusão por aqui.",
                    "A ordem do reino será mantida.",
                    "Fiquem onde estão.",
                    "A cela ficará fechada até nova ordem."
                };
            }
        }

        [Constructable]
        public OSUGuardaDePrisao() : this(0)
        {
        }

        public OSUGuardaDePrisao(int cityId) : base(cityId, "guarda de prisão")
        {
        }

        public OSUGuardaDePrisao(Serial serial) : base(serial)
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
}
