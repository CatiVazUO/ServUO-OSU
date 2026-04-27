using Server;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public abstract class ArenaGladiatorMobBase : BaseCreature
    {
        protected ArenaGladiatorMobBase(string name, int body, int hue, int minHits, int maxHits, int minDmg, int maxDmg, int minSkill, int maxSkill)
            : base(AIType.AI_Melee, FightMode.Closest, 12, 1, 0.1, 0.25)
        {
            Name = name;
            Body = body;
            Hue = hue;
            SetStr(minHits, maxHits);
            SetDex(60, 120);
            SetInt(25, 60);
            SetHits(minHits, maxHits);
            SetDamage(minDmg, maxDmg);
            SetSkill(SkillName.Wrestling, minSkill, maxSkill);
            SetSkill(SkillName.Tactics, minSkill, maxSkill);
            SetSkill(SkillName.MagicResist, minSkill - 10, maxSkill - 5);
            Fame = 0;
            Karma = -2500;
            VirtualArmor = 20;
        }

        public override bool DeleteCorpseOnDeath { get { return true; } }

        public override void GenerateLoot() { }

        public ArenaGladiatorMobBase(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }

        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class ArenaT1Wolf : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT1Wolf() : base("lobo da arena", 23, 0, 70, 90, 4, 7, 45, 60) { }
        public ArenaT1Wolf(Serial serial) : base(serial) { }
    }

    public class ArenaT1Boar : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT1Boar() : base("javali da arena", 290, 0, 75, 95, 4, 8, 45, 60) { }
        public ArenaT1Boar(Serial serial) : base(serial) { }
    }

    public class ArenaT1Panther : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT1Panther() : base("pantera da arena", 214, 0, 70, 88, 5, 8, 50, 65) { }
        public ArenaT1Panther(Serial serial) : base(serial) { }
    }

    public class ArenaT1Hound : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT1Hound() : base("cão feroz da arena", 98, 0, 72, 92, 4, 7, 48, 62) { }
        public ArenaT1Hound(Serial serial) : base(serial) { }
    }

    public class ArenaT1Bear : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT1Bear() : base("urso jovem da arena", 211, 0, 85, 110, 5, 9, 50, 65) { }
        public ArenaT1Bear(Serial serial) : base(serial) { }
    }

    public class ArenaT2DireWolf : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT2DireWolf() : base("lobo terrível da arena", 225, 0, 120, 150, 7, 11, 65, 78) { }
        public ArenaT2DireWolf(Serial serial) : base(serial) { }
    }

    public class ArenaT2Raptor : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT2Raptor() : base("raptor da arena", 217, 0, 125, 155, 8, 12, 66, 80) { }
        public ArenaT2Raptor(Serial serial) : base(serial) { }
    }

    public class ArenaT2WarBoar : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT2WarBoar() : base("javali de guerra da arena", 290, 1175, 130, 160, 8, 12, 66, 80) { }
        public ArenaT2WarBoar(Serial serial) : base(serial) { }
    }

    public class ArenaT2Lion : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT2Lion() : base("leão da arena", 216, 0, 120, 150, 8, 12, 68, 82) { }
        public ArenaT2Lion(Serial serial) : base(serial) { }
    }

    public class ArenaT2Bear : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT2Bear() : base("urso feroz da arena", 212, 0, 135, 170, 8, 13, 68, 82) { }
        public ArenaT2Bear(Serial serial) : base(serial) { }
    }

    public class ArenaT3SavageWolf : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT3SavageWolf() : base("lobo selvagem da arena", 225, 1157, 190, 240, 10, 14, 80, 95) { }
        public ArenaT3SavageWolf(Serial serial) : base(serial) { }
    }

    public class ArenaT3Tiger : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT3Tiger() : base("tigre da arena", 214, 1175, 200, 250, 10, 15, 82, 96) { }
        public ArenaT3Tiger(Serial serial) : base(serial) { }
    }

    public class ArenaT3Bull : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT3Bull() : base("touro da arena", 232, 0, 210, 260, 11, 15, 82, 97) { }
        public ArenaT3Bull(Serial serial) : base(serial) { }
    }

    public class ArenaT3Rhino : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT3Rhino() : base("rinoceronte da arena", 208, 0, 220, 270, 11, 16, 84, 98) { }
        public ArenaT3Rhino(Serial serial) : base(serial) { }
    }

    public class ArenaT3WarBear : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT3WarBear() : base("urso de guerra da arena", 212, 1150, 210, 260, 11, 16, 85, 100) { }
        public ArenaT3WarBear(Serial serial) : base(serial) { }
    }

    public class ArenaT4BruteWolf : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT4BruteWolf() : base("lobo brutal da arena", 225, 33, 280, 340, 13, 18, 98, 110) { }
        public ArenaT4BruteWolf(Serial serial) : base(serial) { }
    }

    public class ArenaT4DreadLion : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT4DreadLion() : base("leão sanguinário da arena", 216, 33, 285, 350, 13, 18, 100, 112) { }
        public ArenaT4DreadLion(Serial serial) : base(serial) { }
    }

    public class ArenaT4HornedBeast : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT4HornedBeast() : base("fera chifruda da arena", 232, 33, 300, 360, 14, 19, 100, 114) { }
        public ArenaT4HornedBeast(Serial serial) : base(serial) { }
    }

    public class ArenaT4ClawRipper : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT4ClawRipper() : base("estripador da arena", 217, 33, 300, 365, 14, 20, 102, 114) { }
        public ArenaT4ClawRipper(Serial serial) : base(serial) { }
    }

    public class ArenaT4DireBear : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT4DireBear() : base("urso monstruoso da arena", 212, 33, 315, 380, 15, 20, 104, 116) { }
        public ArenaT4DireBear(Serial serial) : base(serial) { }
    }

    public class ArenaT5NightReaver : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT5NightReaver() : base("ceifador noturno da arena", 214, 1175, 380, 460, 17, 23, 116, 125) { }
        public ArenaT5NightReaver(Serial serial) : base(serial) { }
    }

    public class ArenaT5DoomWolf : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT5DoomWolf() : base("lobo da ruína da arena", 225, 1175, 390, 470, 17, 24, 118, 126) { }
        public ArenaT5DoomWolf(Serial serial) : base(serial) { }
    }

    public class ArenaT5GoreKing : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT5GoreKing() : base("rei sangrento da arena", 208, 1175, 420, 510, 18, 25, 120, 128) { }
        public ArenaT5GoreKing(Serial serial) : base(serial) { }
    }

    public class ArenaT5TitanBear : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT5TitanBear() : base("titã da arena", 212, 1175, 430, 520, 19, 26, 122, 129) { }
        public ArenaT5TitanBear(Serial serial) : base(serial) { }
    }

    public class ArenaT5ExecutionerBeast : ArenaGladiatorMobBase
    {
        [Constructable] public ArenaT5ExecutionerBeast() : base("fera executora da arena", 217, 1175, 440, 540, 19, 27, 123, 130) { }
        public ArenaT5ExecutionerBeast(Serial serial) : base(serial) { }
    }

}
