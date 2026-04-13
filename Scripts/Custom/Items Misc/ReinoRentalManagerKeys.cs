using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Reinos
{
    public abstract class ReinoRentalManagerKeyBase : Item
    {
        private int m_CityId;
        private string m_ConstructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return m_CityId; } set { m_CityId = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return m_ConstructionKey; } set { m_ConstructionKey = value ?? String.Empty; InvalidateProperties(); } }

        protected abstract OSUPropertyType ManagedPropertyType { get; }
        protected abstract string KeyDisplayName { get; }

        protected ReinoRentalManagerKeyBase(int itemId, int cityId, string constructionKey) : base(itemId)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;
            LootType = LootType.Blessed;
            Movable = true;
            Weight = 1.0;
            Name = KeyDisplayName;
        }

        public ReinoRentalManagerKeyBase(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            pm.SendMessage("Use esta chave em uma placa governamental da área correta para abrir a configuração reduzida.");
        }

        public virtual bool GrantsAccess(TownHouseSign sign)
        {
            if (sign == null || sign.Deleted)
                return false;

            if (sign.GovernmentCityId != m_CityId)
                return false;

            string signKey = ReinoEmploymentSystem.FindConstructionKeyByRentalSign(sign);
            if (String.IsNullOrWhiteSpace(signKey) || !String.Equals(signKey, m_ConstructionKey, StringComparison.OrdinalIgnoreCase))
                return false;

            return sign.PropertyType == ManagedPropertyType;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_CityId);
            writer.Write(m_ConstructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CityId = reader.ReadInt();
            m_ConstructionKey = reader.ReadString();
            LootType = LootType.Blessed;
            Name = KeyDisplayName;
        }
    }

    public class ReinoResidentialManagerKey : ReinoRentalManagerKeyBase
    {
        protected override OSUPropertyType ManagedPropertyType { get { return OSUPropertyType.House; } }
        protected override string KeyDisplayName { get { return "chave de administração residencial"; } }

        [Constructable]
        public ReinoResidentialManagerKey() : this(0, String.Empty)
        {
        }

        public ReinoResidentialManagerKey(int cityId, string constructionKey) : base(0x1010, cityId, constructionKey)
        {
        }

        public ReinoResidentialManagerKey(Serial serial) : base(serial)
        {
        }
    }

    public class ReinoCommercialManagerKey : ReinoRentalManagerKeyBase
    {
        protected override OSUPropertyType ManagedPropertyType { get { return OSUPropertyType.Commercial; } }
        protected override string KeyDisplayName { get { return "chave de administração comercial"; } }

        [Constructable]
        public ReinoCommercialManagerKey() : this(0, String.Empty)
        {
        }

        public ReinoCommercialManagerKey(int cityId, string constructionKey) : base(0x1010, cityId, constructionKey)
        {
            Hue = 0x59B;
        }

        public ReinoCommercialManagerKey(Serial serial) : base(serial)
        {
        }
    }

    public static class ReinoRentalManagerKeyHelper
    {
        public static bool HasAccess(PlayerMobile pm, TownHouseSign sign)
        {
            if (pm == null || pm.Deleted || sign == null || sign.Deleted)
                return false;

            List<Item> items = new List<Item>();
            CollectItems(pm.Items, items);
            if (pm.Backpack != null)
                CollectContainer(pm.Backpack, items);
            if (pm.BankBox != null)
                CollectContainer(pm.BankBox, items);

            for (int i = 0; i < items.Count; i++)
            {
                ReinoRentalManagerKeyBase key = items[i] as ReinoRentalManagerKeyBase;
                if (key != null && !key.Deleted && key.GrantsAccess(sign))
                    return true;
            }

            return false;
        }

        public static void DeleteKeys(PlayerMobile pm, int cityId, string constructionKey)
        {
            if (pm == null || pm.Deleted)
                return;

            List<Item> items = new List<Item>();
            CollectItems(pm.Items, items);
            if (pm.Backpack != null)
                CollectContainer(pm.Backpack, items);
            if (pm.BankBox != null)
                CollectContainer(pm.BankBox, items);

            for (int i = 0; i < items.Count; i++)
            {
                ReinoRentalManagerKeyBase key = items[i] as ReinoRentalManagerKeyBase;
                if (key == null || key.Deleted)
                    continue;

                if (key.CityId == cityId && String.Equals(key.ConstructionKey, constructionKey ?? String.Empty, StringComparison.OrdinalIgnoreCase))
                    key.Delete();
            }
        }

        public static void EnsureKeys(PlayerMobile pm, int cityId, string constructionKey, bool residential, bool commercial)
        {
            if (pm == null || pm.Deleted || pm.Backpack == null || String.IsNullOrWhiteSpace(constructionKey))
                return;

            if (residential && !HasSpecificKey<ReinoResidentialManagerKey>(pm, cityId, constructionKey))
                pm.Backpack.DropItem(new ReinoResidentialManagerKey(cityId, constructionKey));

            if (commercial && !HasSpecificKey<ReinoCommercialManagerKey>(pm, cityId, constructionKey))
                pm.Backpack.DropItem(new ReinoCommercialManagerKey(cityId, constructionKey));
        }

        private static bool HasSpecificKey<T>(PlayerMobile pm, int cityId, string constructionKey) where T : ReinoRentalManagerKeyBase
        {
            List<Item> items = new List<Item>();
            CollectItems(pm.Items, items);
            if (pm.Backpack != null)
                CollectContainer(pm.Backpack, items);
            if (pm.BankBox != null)
                CollectContainer(pm.BankBox, items);

            for (int i = 0; i < items.Count; i++)
            {
                T key = items[i] as T;
                if (key != null && !key.Deleted && key.CityId == cityId && String.Equals(key.ConstructionKey, constructionKey ?? String.Empty, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void CollectItems(List<Item> source, List<Item> buffer)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                Item item = source[i];
                if (item != null && !item.Deleted)
                    buffer.Add(item);
            }
        }

        private static void CollectContainer(Container container, List<Item> buffer)
        {
            if (container == null || container.Deleted)
                return;

            Item[] items = container.Items.ToArray();
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item == null || item.Deleted)
                    continue;

                buffer.Add(item);

                Container sub = item as Container;
                if (sub != null)
                    CollectContainer(sub, buffer);
            }
        }
    }
}
