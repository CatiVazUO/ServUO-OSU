using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;
using Server.Custom.Systems.PlayerMadeStatues;

namespace Server.Items
{
    public class SculptorTools : BaseTool
    {
        public override CraftSystem CraftSystem { get { return null; } }

        [Constructable]
        public SculptorTools() : this(50)
        {
        }

        [Constructable]
        public SculptorTools(int uses) : base(uses, 0x1027)
        {
            Name = "ferramentas de escultor";
            Weight = 2.0;
        }

        public SculptorTools(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!StatueCraftSystem.CanUseTools(pm, this))
                return;

            pm.CloseGump(typeof(SculptorMainGump));
            pm.SendGump(new SculptorMainGump(pm, this, 1000));
        }
    }
}
