using System;
using Server;

namespace Server.Items
{
    [Flipable(0x18BA, 0x18BB)]
    public class OhlmStatue : Item
    {
        [Constructable]
        public OhlmStatue() : this(false)
        {
        }

        [Constructable]
        public OhlmStatue(bool flipped) : base(flipped ? 0x18BB : 0x18BA)
        {
            Name = "estátua de Ohlm";
            Movable = false;
            Weight = 255.0;
        }

        public OhlmStatue(Serial serial) : base(serial)
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

            Name = "estátua de Ohlm";
            Movable = false;
            Weight = 255.0;
        }
    }

    [Flipable(0x18BE, 0x18BF)]
    public class LamoaStatue : Item
    {
        [Constructable]
        public LamoaStatue() : this(false)
        {
        }

        [Constructable]
        public LamoaStatue(bool flipped) : base(flipped ? 0x18BF : 0x18BE)
        {
            Name = "estátua de Lamoa";
            Movable = false;
            Weight = 255.0;
        }

        public LamoaStatue(Serial serial) : base(serial)
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

            Name = "estátua de Lamoa";
            Movable = false;
            Weight = 255.0;
        }
    }

    [Flipable(0x18C2, 0x18C3)]
    public class CailanStatue : Item
    {
        [Constructable]
        public CailanStatue() : this(false)
        {
        }

        [Constructable]
        public CailanStatue(bool flipped) : base(flipped ? 0x18C3 : 0x18C2)
        {
            Name = "estátua de Cailan";
            Movable = false;
            Weight = 255.0;
        }

        public CailanStatue(Serial serial) : base(serial)
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

            Name = "estátua de Cailan";
            Movable = false;
            Weight = 255.0;
        }
    }

    [Flipable(0x18C4, 0x18C5)]
    public class ElysiaStatue : Item
    {
        [Constructable]
        public ElysiaStatue() : this(false)
        {
        }

        [Constructable]
        public ElysiaStatue(bool flipped) : base(flipped ? 0x18C5 : 0x18C4)
        {
            Name = "estátua de Elysia";
            Movable = false;
            Weight = 255.0;
        }

        public ElysiaStatue(Serial serial) : base(serial)
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

            Name = "estátua de Elysia";
            Movable = false;
            Weight = 255.0;
        }
    }

    [Flipable(0x18C6, 0x18C7)]
    public class DortemStatue : Item
    {
        [Constructable]
        public DortemStatue() : this(false)
        {
        }

        [Constructable]
        public DortemStatue(bool flipped) : base(flipped ? 0x18C7 : 0x18C6)
        {
            Name = "estátua de Dortem";
            Movable = false;
            Weight = 255.0;
        }

        public DortemStatue(Serial serial) : base(serial)
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

            Name = "estátua de Dortem";
            Movable = false;
            Weight = 255.0;
        }
    }

    [Flipable(0x18C8, 0x18C9)]
    public class EverneStatue : Item
    {
        [Constructable]
        public EverneStatue() : this(false)
        {
        }

        [Constructable]
        public EverneStatue(bool flipped) : base(flipped ? 0x18C9 : 0x18C8)
        {
            Name = "estátua de Everne";
            Movable = false;
            Weight = 255.0;
        }

        public EverneStatue(Serial serial) : base(serial)
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

            Name = "estátua de Everne";
            Movable = false;
            Weight = 255.0;
        }
    }
}
