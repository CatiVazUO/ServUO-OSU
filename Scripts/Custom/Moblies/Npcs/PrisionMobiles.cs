using Server;
using Server.Items;
using Server.Mobiles;
using System;
using static Server.Items.Uniforme44;

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
            AddItem(new UniformeUnderShirt  { Movable = false });
            AddUniform(cityId);
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();
            if (!Female)
            {
                FacialHairItemID = Race.RandomFacialHair(Female);
                FacialHairHue = Race.RandomHairHue();
            }
            m_NextSpeechUtc = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(25, 50));
        }

        private void AddUniform(int cityId)
        {
            Tunic tunic = new Tunic();
            tunic.Movable = false;

            string culture = ReinoEmploymentSystem.GetGovernmentCultureId(cityId);
            switch ((culture ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "sarangs":
                    tunic.ItemID = 0x227E;
                    break;
                case "kamay":
                    tunic.ItemID = 0x2281;
                    break;
                case "zorteros":
                case "zosteros":
                    tunic.ItemID = 0x228A;
                    break;
                case "matalun":
                    tunic.ItemID = 0x229C;
                    break;
                default:
                    tunic.ItemID = 0x1FA1;
                    break;
            }

            AddItem(tunic);
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
