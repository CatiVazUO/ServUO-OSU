using System;
using System.Collections.Generic;

using Server;
using Server.Items;
using Server.Targeting;

namespace Server.Items
{
    public enum OSUContainerWearKind
    {
        Cloth,
        Leather,
        Wood,
        Metal,
        Stone
    }

    public static class OSUContainerResource
    {
        public static readonly CraftResource None = CraftResource.None;

        public static readonly CraftResource RegularWood = Get("RegularWood", "Wood");
        public static readonly CraftResource OakWood = Get("OakWood", "Oak", "RegularWood", "Wood");
        public static readonly CraftResource AshWood = Get("AshWood", "Ash", "OakWood", "RegularWood", "Wood");
        public static readonly CraftResource YewWood = Get("YewWood", "Yew", "AshWood", "OakWood", "RegularWood", "Wood");
        public static readonly CraftResource Heartwood = Get("Heartwood", "YewWood", "AshWood", "OakWood", "RegularWood", "Wood");
        public static readonly CraftResource Bloodwood = Get("Bloodwood", "Heartwood", "YewWood", "AshWood", "OakWood", "RegularWood", "Wood");
        public static readonly CraftResource Frostwood = Get("Frostwood", "Bloodwood", "Heartwood", "YewWood", "AshWood", "OakWood", "RegularWood", "Wood");

        public static readonly CraftResource RegularLeather = Get("RegularLeather", "Leather");
        public static readonly CraftResource SpinedLeather = Get("SpinedLeather", "RegularLeather", "Leather");
        public static readonly CraftResource HornedLeather = Get("HornedLeather", "SpinedLeather", "RegularLeather", "Leather");
        public static readonly CraftResource BarbedLeather = Get("BarbedLeather", "HornedLeather", "SpinedLeather", "RegularLeather", "Leather");

        public static readonly CraftResource Iron = Get("Iron");
        public static readonly CraftResource DullCopper = Get("DullCopper", "Iron");
        public static readonly CraftResource ShadowIron = Get("ShadowIron", "DullCopper", "Iron");
        public static readonly CraftResource Copper = Get("Copper", "ShadowIron", "DullCopper", "Iron");
        public static readonly CraftResource Bronze = Get("Bronze", "Copper", "ShadowIron", "DullCopper", "Iron");
        public static readonly CraftResource Gold = Get("Gold", "Bronze", "Copper", "ShadowIron", "DullCopper", "Iron");
        public static readonly CraftResource Agapite = Get("Agapite", "Gold", "Bronze", "Copper", "ShadowIron", "DullCopper", "Iron");
        public static readonly CraftResource Verite = Get("Verite", "Agapite", "Gold", "Bronze", "Copper", "ShadowIron", "DullCopper", "Iron");
        public static readonly CraftResource Valorite = Get("Valorite", "Verite", "Agapite", "Gold", "Bronze", "Copper", "ShadowIron", "DullCopper", "Iron");

        public static CraftResource Get(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                try
                {
                    return (CraftResource)Enum.Parse(typeof(CraftResource), names[i], false);
                }
                catch
                {
                }
            }

            return CraftResource.None;
        }

        public static string GetDisplayName(CraftResource resource)
        {
            if (resource == CraftResource.None)
                return "sem resource";

            string name = resource.ToString();

            // Deixa nomes do ServUO um pouco mais legíveis na tooltip.
            name = name.Replace("Regular", "Comum ");
            name = name.Replace("Wood", " Madeira");
            name = name.Replace("Leather", " Couro");

            while (name.Contains("  "))
                name = name.Replace("  ", " ");

            return name.Trim();
        }
    }

    public abstract class OSUContainerBase : BaseContainer
    {
        private int m_MaxUses;
        private int m_UsesRemaining;
        private int m_BrokenOpenAttempts;
        private CraftResource m_Resource;
        private OSUContainerWearKind m_WearKind;

        public override int DefaultMaxItems { get { return OSUDefaultMaxItems; } }
        public override int DefaultMaxWeight { get { return OSUDefaultMaxWeight; } }

        public virtual int OSUDefaultMaxItems { get { return 125; } }
        public virtual int OSUDefaultMaxWeight { get { return 400; } }
        public virtual string OSUContainerName { get { return "container"; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxUses
        {
            get { return m_MaxUses; }
            set
            {
                m_MaxUses = Math.Max(1, value);

                if (m_UsesRemaining > m_MaxUses)
                    m_UsesRemaining = m_MaxUses;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set
            {
                m_UsesRemaining = Math.Max(0, value);
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource ContainerResource
        {
            get { return m_Resource; }
            set
            {
                m_Resource = value;
                RebuildUsesFromResource(true);
                ApplyResourceHue();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUContainerWearKind WearKind
        {
            get { return m_WearKind; }
            set
            {
                m_WearKind = value;
                RebuildUsesFromResource(true);
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BrokenOpenAttempts
        {
            get { return m_BrokenOpenAttempts; }
            set { m_BrokenOpenAttempts = Math.Max(0, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Broken
        {
            get { return m_UsesRemaining <= 0; }
        }

        public OSUContainerBase(int itemID)
            : this(itemID, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
        }

        public OSUContainerBase(int itemID, CraftResource resource, OSUContainerWearKind wearKind)
            : base(itemID)
        {
            m_Resource = resource;
            m_WearKind = wearKind;
            RebuildUsesFromResource(false);
            ApplyResourceHue();
        }

        public OSUContainerBase(Serial serial)
            : base(serial)
        {
        }

        private void RebuildUsesFromResource(bool keepDamagePercent)
        {
            int oldMax = m_MaxUses;
            int oldRemaining = m_UsesRemaining;

            m_MaxUses = GetUsesForResource(m_Resource, m_WearKind);

            if (keepDamagePercent && oldMax > 0)
            {
                double percent = Math.Max(0.0, Math.Min(1.0, (double)oldRemaining / oldMax));
                m_UsesRemaining = Math.Max(1, (int)Math.Round(m_MaxUses * percent));
            }
            else
            {
                m_UsesRemaining = m_MaxUses;
            }

            m_BrokenOpenAttempts = 0;
        }

        private void ApplyResourceHue()
        {
            int hue = GetHueForResource(m_Resource);

            if (hue > 0)
                Hue = hue;
        }

        private static int GetHueForResource(CraftResource resource)
        {
            // Evita depender de métodos auxiliares específicos de uma versão do ServUO.
            string name = resource.ToString();

            switch (name)
            {
                case "DullCopper": return 0x973;
                case "ShadowIron": return 0x966;
                case "Copper": return 0x96D;
                case "Bronze": return 0x972;
                case "Gold": return 0x8A5;
                case "Agapite": return 0x979;
                case "Verite": return 0x89F;
                case "Valorite": return 0x8AB;
                case "SpinedLeather": return 0x8AC;
                case "HornedLeather": return 0x845;
                case "BarbedLeather": return 0x851;
                case "OakWood": return 0x7DA;
                case "AshWood": return 0x4A7;
                case "YewWood": return 0x4A8;
                case "Heartwood": return 0x4A9;
                case "Bloodwood": return 0x4AA;
                case "Frostwood": return 0x47F;
            }

            return 0;
        }

        public static int GetUsesForResource(CraftResource resource, OSUContainerWearKind kind)
        {
            string name = resource.ToString();

            switch (name)
            {
                // Madeiras do ServUO, da comum para a mais rara.
                case "RegularWood":
                case "Wood":
                    return 320;
                case "OakWood":
                case "Oak":
                    return 420;
                case "AshWood":
                case "Ash":
                    return 520;
                case "YewWood":
                case "Yew":
                    return 650;
                case "Heartwood":
                    return 800;
                case "Bloodwood":
                    return 950;
                case "Frostwood":
                    return 1150;

                // Couros do ServUO.
                case "RegularLeather":
                case "Leather":
                    return 240;
                case "SpinedLeather":
                    return 360;
                case "HornedLeather":
                    return 520;
                case "BarbedLeather":
                    return 700;

                // Metais do ServUO, da ordem comum de mineração.
                case "Iron":
                    return 550;
                case "DullCopper":
                    return 640;
                case "ShadowIron":
                    return 720;
                case "Copper":
                    return 800;
                case "Bronze":
                    return 900;
                case "Gold":
                    return 1000;
                case "Agapite":
                    return 1120;
                case "Verite":
                    return 1260;
                case "Valorite":
                    return 1450;

                // Escamas, caso você futuramente queira containers especiais reforçados por escamas.
                case "RedScales":
                case "YellowScales":
                    return 780;
                case "BlackScales":
                case "GreenScales":
                    return 900;
                case "WhiteScales":
                    return 1020;
                case "BlueScales":
                    return 1150;
            }

            // Fallback por tipo de desgaste quando o item não tem resource real do CraftResource.
            switch (kind)
            {
                case OSUContainerWearKind.Cloth:
                    return 160;
                case OSUContainerWearKind.Leather:
                    return 240;
                case OSUContainerWearKind.Metal:
                    return 550;
                case OSUContainerWearKind.Stone:
                    return 900;
                case OSUContainerWearKind.Wood:
                default:
                    return 320;
            }
        }

        private static CraftResource MapOldMaterialToResource(int oldMaterial)
        {
            // Compatibilidade com a primeira versão do patch, caso algum item já tenha sido salvo.
            // A conversão é por índice antigo, mas o resultado agora é sempre CraftResource real.
            switch (oldMaterial)
            {
                case 1:
                    return OSUContainerResource.RegularLeather;
                case 2:
                    return OSUContainerResource.HornedLeather;
                case 6:
                    return OSUContainerResource.OakWood;
                case 7:
                    return OSUContainerResource.Heartwood;
                case 8:
                case 9:
                    return OSUContainerResource.Iron;
                case 10:
                    return OSUContainerResource.Gold;
                case 11:
                    return OSUContainerResource.Valorite;
                case 0:
                case 12:
                    return CraftResource.None;
                case 3:
                case 4:
                case 5:
                default:
                    return OSUContainerResource.RegularWood;
            }
        }

        private static OSUContainerWearKind MapOldMaterialToKind(int oldMaterial)
        {
            switch (oldMaterial)
            {
                case 0:
                    return OSUContainerWearKind.Cloth;
                case 1:
                case 2:
                    return OSUContainerWearKind.Leather;
                case 8:
                case 9:
                case 10:
                case 11:
                    return OSUContainerWearKind.Metal;
                case 12:
                    return OSUContainerWearKind.Stone;
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                default:
                    return OSUContainerWearKind.Wood;
            }
        }

        public virtual bool AcceptsItem(Item item)
        {
            return true;
        }

        public virtual string GetRefuseMessage(Item item)
        {
            return "Esse container não foi feito para guardar esse tipo de item.";
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            if (Broken)
            {
                if (message && m != null)
                    m.SendMessage("Esse container não abre mais e não pode receber novos itens.");

                return false;
            }

            if (item != null && !AcceptsItem(item))
            {
                if (message && m != null)
                    m.SendMessage(GetRefuseMessage(item));

                return false;
            }

            return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
        }

        public override void SendFullItemsMessage(Mobile to, Item item)
        {
            if (to != null)
                to.SendMessage("Esse container não cabe mais itens.");
        }

        public override void SendFullWeightMessage(Mobile to, Item item)
        {
            if (to != null)
                to.SendMessage("Esse container não aguenta mais peso.");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null)
                return;

            if (from.AccessLevel == AccessLevel.Player && !from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            if (Broken)
            {
                HandleBrokenOpen(from);
                return;
            }

            if (from.AccessLevel == AccessLevel.Player)
                ConsumeUse(from);

            if (!Broken)
                Open(from);
        }

        private void ConsumeUse(Mobile from)
        {
            if (m_UsesRemaining <= 0)
                return;

            m_UsesRemaining--;
            InvalidateProperties();

            if (m_UsesRemaining <= 0)
            {
                if (from != null)
                    from.SendMessage("O container se rompeu e não abre mais. Escolha outro container para receber os itens.");

                BeginTransferTarget(from);
                return;
            }

            if (m_UsesRemaining <= Math.Max(1, m_MaxUses / 10))
                SendLowDurabilityWarning(from);
        }

        private void SendLowDurabilityWarning(Mobile from)
        {
            if (from == null)
                return;

            switch (m_WearKind)
            {
                case OSUContainerWearKind.Cloth:
                case OSUContainerWearKind.Leather:
                    from.SendMessage("Você percebe que o container está rasgando. Ele está perto de se inutilizar.");
                    break;
                case OSUContainerWearKind.Metal:
                    from.SendMessage("Você percebe que o container está enferrujando e já não parece confiável.");
                    break;
                case OSUContainerWearKind.Stone:
                    from.SendMessage("Você percebe rachaduras no container. Ele está perto de ceder.");
                    break;
                case OSUContainerWearKind.Wood:
                default:
                    from.SendMessage("Você percebe que o container está rangendo. A madeira está perto de ceder.");
                    break;
            }
        }

        private void HandleBrokenOpen(Mobile from)
        {
            m_BrokenOpenAttempts++;
            InvalidateProperties();

            if (m_BrokenOpenAttempts >= 3)
            {
                from.SendMessage("O container cede de vez. Tudo que estava dentro cai no chão.");
                DumpContentsToGround();
                Delete();
                return;
            }

            from.SendMessage("Esse container não abre mais. Clique em outro container para tentar passar os itens para ele.");
            from.SendMessage(String.Format("Aviso: se você insistir sem transferir, na terceira tentativa os itens cairão no chão. Tentativa {0}/3.", m_BrokenOpenAttempts));
            BeginTransferTarget(from);
        }

        private void BeginTransferTarget(Mobile from)
        {
            if (from != null)
                from.Target = new OSUContainerTransferTarget(this);
        }

        private void DumpContentsToGround()
        {
            Point3D loc = GetWorldLocation();
            Map map = Map;

            if (map == null || map == Map.Internal)
            {
                object root = RootParent;
                Mobile mob = root as Mobile;

                if (mob != null)
                {
                    loc = mob.Location;
                    map = mob.Map;
                }
            }

            if (map == null)
                map = Map.Internal;

            List<Item> list = new List<Item>(Items);

            for (int i = 0; i < list.Count; i++)
            {
                Item item = list[i];

                if (item != null && !item.Deleted)
                    item.MoveToWorld(loc, map);
            }
        }

        public bool TryTransferContentsTo(Mobile from, Container target, out string reason)
        {
            reason = null;

            if (target == null || target.Deleted)
            {
                reason = "Você precisa escolher outro container válido.";
                return false;
            }

            if (target == this || target.IsChildOf(this))
            {
                reason = "Você não pode transferir os itens para o próprio container quebrado, nem para algo que está dentro dele.";
                return false;
            }

            if (!target.IsAccessibleTo(from))
            {
                reason = "Esse container não está acessível.";
                return false;
            }

            List<Item> list = new List<Item>(Items);
            int plusItems = 0;
            int plusWeight = 0;

            for (int i = 0; i < list.Count; i++)
            {
                Item item = list[i];

                if (item == null || item.Deleted)
                    continue;

                if (!target.CheckHold(from, item, false, true, plusItems, plusWeight))
                {
                    reason = "O container escolhido não tem espaço, peso livre ou tipo adequado para receber tudo.";
                    return false;
                }

                plusItems += item.TotalItems + (item.IsVirtualItem ? 0 : 1);
                plusWeight += item.TotalWeight + item.PileWeight;
            }

            for (int i = 0; i < list.Count; i++)
            {
                Item item = list[i];

                if (item != null && !item.Deleted)
                    target.DropItem(item);
            }

            Delete();
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(String.Format("Limite realista: {0} itens / {1} stones", MaxItems, MaxWeight));
            list.Add(String.Format("Material: {0}", OSUContainerResource.GetDisplayName(m_Resource)));
            list.Add(String.Format("Uso: {0}/{1}", m_UsesRemaining, m_MaxUses));

            if (Broken)
                list.Add("Quebrado: precisa transferir os itens");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version
            writer.Write((int)m_Resource);
            writer.Write((int)m_WearKind);
            writer.Write(m_MaxUses);
            writer.Write(m_UsesRemaining);
            writer.Write(m_BrokenOpenAttempts);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_Resource = (CraftResource)reader.ReadInt();
                    m_WearKind = (OSUContainerWearKind)reader.ReadInt();
                    m_MaxUses = reader.ReadInt();
                    m_UsesRemaining = reader.ReadInt();
                    m_BrokenOpenAttempts = reader.ReadInt();
                    break;
                case 0:
                    int oldMaterial = reader.ReadInt();
                    m_Resource = MapOldMaterialToResource(oldMaterial);
                    m_WearKind = MapOldMaterialToKind(oldMaterial);
                    m_MaxUses = reader.ReadInt();
                    m_UsesRemaining = reader.ReadInt();
                    m_BrokenOpenAttempts = reader.ReadInt();
                    break;
            }

            if (m_MaxUses <= 0)
                m_MaxUses = GetUsesForResource(m_Resource, m_WearKind);

            if (m_UsesRemaining < 0)
                m_UsesRemaining = m_MaxUses;
        }

        private class OSUContainerTransferTarget : Target
        {
            private readonly OSUContainerBase m_Source;

            public OSUContainerTransferTarget(OSUContainerBase source)
                : base(12, false, TargetFlags.None)
            {
                m_Source = source;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Source == null || m_Source.Deleted)
                    return;

                Container target = targeted as Container;
                string reason;

                if (m_Source.TryTransferContentsTo(from, target, out reason))
                {
                    from.SendMessage("Os itens foram transferidos para o novo container. O container quebrado foi descartado.");
                }
                else
                {
                    if (String.IsNullOrEmpty(reason))
                        reason = "Não foi possível transferir os itens.";

                    from.SendMessage(reason);
                    from.SendMessage("Os itens continuam presos no container quebrado.");
                }
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                if (m_Source != null && !m_Source.Deleted && from != null)
                    from.SendMessage("Transferência cancelada. Os itens continuam presos no container quebrado.");
            }
        }
    }

    public abstract class OSUClothingContainerBase : OSUContainerBase
    {
        public OSUClothingContainerBase(int itemID, CraftResource resource, OSUContainerWearKind wearKind)
            : base(itemID, resource, wearKind)
        {
        }

        public OSUClothingContainerBase(Serial serial)
            : base(serial)
        {
        }

        public override bool AcceptsItem(Item item)
        {
            if (item == null)
                return false;

            return OSUContainerTypeRules.IsClothing(item);
        }

        public override string GetRefuseMessage(Item item)
        {
            return "Esse guarda-roupas só aceita roupas, sapatos, chapéus e peças de vestuário.";
        }
    }

    public abstract class OSUBookContainerBase : OSUContainerBase
    {
        public OSUBookContainerBase(int itemID, CraftResource resource, OSUContainerWearKind wearKind)
            : base(itemID, resource, wearKind)
        {
        }

        public OSUBookContainerBase(Serial serial)
            : base(serial)
        {
        }

        public override bool AcceptsItem(Item item)
        {
            if (item == null)
                return false;

            return OSUContainerTypeRules.IsBook(item);
        }

        public override string GetRefuseMessage(Item item)
        {
            return "Essa estante só aceita livros, grimórios, tomos e objetos claramente literários.";
        }
    }

    public static class OSUContainerTypeRules
    {
        public static bool IsClothing(Item item)
        {
            if (item == null)
                return false;

            if (item is BaseClothing)
                return true;

            string typeName = item.GetType().Name.ToLower();

            return typeName.Contains("robe") || typeName.Contains("dress") || typeName.Contains("shirt") ||
                   typeName.Contains("pants") || typeName.Contains("skirt") || typeName.Contains("kilt") ||
                   typeName.Contains("hat") || typeName.Contains("cap") || typeName.Contains("sandals") ||
                   typeName.Contains("shoes") || typeName.Contains("boots") || typeName.Contains("cloak") ||
                   typeName.Contains("sash") || typeName.Contains("apron") || typeName.Contains("tunic");
        }

        public static bool IsBook(Item item)
        {
            if (item == null)
                return false;

            if (item is BaseBook)
                return true;

            string typeName = item.GetType().Name.ToLower();

            return typeName.Contains("book") || typeName.Contains("spellbook") || typeName.Contains("runebook") ||
                   typeName.Contains("tome") || typeName.Contains("codex") || typeName.Contains("grimoire") ||
                   typeName.Contains("atlas") || typeName.Contains("journal");
        }
    }
}
