using Server;
using Server.Custom.Systems.Postos;
using Server.Gumps;
using Server.Items;
using System;

namespace Server.Mobiles
{
    public class BasePostoNPC : BaseCreature
    {
        private string m_PostoId;

        [CommandProperty(AccessLevel.GameMaster)]
        public string PostoId
        {
            get { return m_PostoId; }
            set
            {
                m_PostoId = value ?? String.Empty;
                RefreshFromDefinition();
            }
        }
        public override bool IsInvulnerable
        {
            get { return true; }
        }

        public override bool ClickTitle
        {
            get { return !String.IsNullOrWhiteSpace(Title); }
        }

        [Constructable]
        public BasePostoNPC() : this(String.Empty)
        {
        }

        public BasePostoNPC(string postoId) : base(AIType.AI_Animal, FightMode.None, 10, 1, 0.1, 0.2)
        {
            NameHue = 0;
            SpeechHue = Utility.RandomDyedHue();
            Hue = Utility.RandomSkinHue();
            CantWalk = true;
            Blessed = true;
            Direction = Direction.South;
            Female = Utility.RandomBool();
            Body = Female ? 0x191 : 0x190;
            Name = "trabalhador do posto";

            m_PostoId = postoId ?? String.Empty;

            InitStats(100, 100, 25);
            SetSkill(SkillName.Anatomy, 25.0, 50.0);
            SetSkill(SkillName.Tactics, 25.0, 50.0);

            Utility.AssignRandomHair(this);
            AddItem(new Boots());
            AddItem(new Backpack());

            RefreshFromDefinition();
        }

        public virtual void RefreshFromDefinition()
        {
            PostoDefinition def = PostoSystem.GetDefinition(m_PostoId);

            if (def == null)
            {
                if (String.IsNullOrWhiteSpace(Name))
                    Name = "trabalhador do posto";

                return;
            }

            if (String.IsNullOrWhiteSpace(Name)
                || String.Equals(Name, "trabalhador do posto", StringComparison.OrdinalIgnoreCase)
                || Name.IndexOf(" do posto ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Name = def.GetNpcName();
            }

            EquipByResource(def.ResourceType);
        }

        private void EquipByResource(PostoResourceType type)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Item item = Items[i];

                if (item == null || item.Deleted)
                    continue;

                if (item.Layer == Layer.Hair || item.Layer == Layer.FacialHair)
                    continue;

                item.Delete();
            }

            AddItem(new Boots());
            AddItem(new Backpack());

            switch (type)
            {
                case PostoResourceType.Iron:
                    AddItem(new Shirt(0x47E));
                    AddItem(new LongPants(0x455));
                    AddItem(new HalfApron(0x835));
                    break;
                case PostoResourceType.Wood:
                    AddItem(new FancyShirt(0x59C));
                    AddItem(new LongPants(0x455));
                    AddItem(new HalfApron(0x3E0));
                    break;
                case PostoResourceType.Cotton:
                    AddItem(new FancyShirt(0x47E));
                    if (Female)
                        AddItem(new Skirt(0x47F));
                    else
                        AddItem(new LongPants(0x47F));
                    AddItem(new HalfApron(0x21));
                    break;
                default:
                    AddItem(new Shirt());
                    AddItem(new LongPants());
                    break;
            }

            Utility.AssignRandomHair(this);
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null || pm.Deleted)
                return;

            if (!from.InRange(Location, 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            pm.CloseGump(typeof(PostoGump));
            pm.SendGump(new PostoGump(pm, m_PostoId));
        }

        public static BasePostoNPC FindByPostoId(string postoId)
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                BasePostoNPC npc = m as BasePostoNPC;

                if (npc == null || npc.Deleted)
                    continue;

                if (String.Equals(npc.PostoId, postoId, StringComparison.OrdinalIgnoreCase))
                    return npc;
            }

            return null;
        }

        public BasePostoNPC(Serial serial) : base(serial)
        {
        }

        protected override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
            writer.Write(m_PostoId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_PostoId = reader.ReadString();
                    break;
                default:
                    m_PostoId = String.Empty;
                    break;
            }

            RefreshFromDefinition();
        }
    }
}
