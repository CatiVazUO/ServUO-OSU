using Server;
using Server.Custom.Reinos;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public static class ReinoAccessHelper
    {
        public static bool HasGovernmentAccess(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (!Server.Custom.Reinos.ReinoElectionsSystem.IsPlayerAllowedForCity(pm, cityId))
                return false;

            if (IsCurrentGovernor(pm, cityId))
                return true;

            if (HasGovernorKey(pm, cityId))
                return true;

            return false;
        }

        public static bool IsCurrentGovernor(PlayerMobile pm, int cityId)
        {
            if (pm == null || pm.Deleted)
                return false;

            ReinoCityData city;
            if (!ReinoElectionsSystem._cities.TryGetValue(cityId, out city) || city == null)
                return false;

            return city.GovernorSerial == pm.Serial.Value;
        }

        public static int GetGovernorCityId(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return -1;

            foreach (KeyValuePair<int, ReinoCityData> kv in ReinoElectionsSystem._cities)
            {
                ReinoCityData city = kv.Value;
                if (city != null && city.GovernorSerial == pm.Serial.Value)
                    return kv.Key;
            }

            return -1;
        }

        public static bool HasGovernorKey(PlayerMobile pm, int cityId)
        {
            if (pm == null)
                return false;

            if (HasGovernorKeyInContainer(pm.Backpack, cityId))
                return true;

            if (pm.BankBox != null && HasGovernorKeyInContainer(pm.BankBox, cityId))
                return true;

            for (int i = 0; i < pm.Items.Count; i++)
            {
                Item item = pm.Items[i];

                if (item is ChaveDoGovernador)
                {
                    ChaveDoGovernador chave = (ChaveDoGovernador)item;

                    if (!chave.Deleted && chave.CityId == cityId)
                        return true;
                }

                Container cont = item as Container;

                if (cont != null && HasGovernorKeyInContainer(cont, cityId))
                    return true;
            }

            return false;
        }

        private static bool HasGovernorKeyInContainer(Container c, int cityId)
        {
            if (c == null)
                return false;

            for (int i = 0; i < c.Items.Count; i++)
            {
                Item item = c.Items[i];

                if (item is ChaveDoGovernador)
                {
                    ChaveDoGovernador chave = (ChaveDoGovernador)item;

                    if (!chave.Deleted && chave.CityId == cityId)
                        return true;
                }

                Container sub = item as Container;

                if (sub != null && HasGovernorKeyInContainer(sub, cityId))
                    return true;
            }

            return false;
        }

        public static void GrantGovernorAccess(PlayerMobile pm, int cityId, bool giveKey)
        {
            if (pm == null || pm.Deleted)
                return;

            if (giveKey)
            {
                DeleteAllGovernorKeysInWorld(cityId);

                ChaveDoGovernador chave = new ChaveDoGovernador(cityId);

                if (pm.Backpack != null)
                    pm.Backpack.DropItem(chave);
                else if (pm.BankBox != null)
                    pm.BankBox.DropItem(chave);
                else
                    chave.MoveToWorld(pm.Location, pm.Map);
            }
        }

        public static void RevokeGovernorAccess(PlayerMobile pm, int cityId)
        {
            DeleteAllGovernorKeysInWorld(cityId);
        }

        public static void DeleteGovernorKeys(Mobile m, int cityId)
        {
            if (m == null || m.Deleted)
                return;

            DeleteGovernorKeysOnItemList(m.Items, cityId);

            if (m.Backpack != null)
                DeleteGovernorKeysInContainer(m.Backpack, cityId);

            if (m.BankBox != null)
                DeleteGovernorKeysInContainer(m.BankBox, cityId);
        }

        public static void DeleteAllGovernorKeysInWorld(int cityId)
        {
            List<Item> toDelete = new List<Item>();

            foreach (Item item in World.Items.Values)
            {
                ChaveDoGovernador chave = item as ChaveDoGovernador;

                if (chave != null && !chave.Deleted && chave.CityId == cityId)
                    toDelete.Add(chave);
            }

            for (int i = 0; i < toDelete.Count; i++)
                toDelete[i].Delete();
        }

        private static void DeleteGovernorKeysOnItemList(List<Item> items, int cityId)
        {
            if (items == null)
                return;

            Item[] copy = items.ToArray();

            for (int i = 0; i < copy.Length; i++)
            {
                Item item = copy[i];

                if (item == null || item.Deleted)
                    continue;

                if (item is ChaveDoGovernador)
                {
                    ChaveDoGovernador chave = (ChaveDoGovernador)item;

                    if (chave.CityId == cityId)
                        chave.Delete();
                }

                Container cont = item as Container;

                if (cont != null)
                    DeleteGovernorKeysInContainer(cont, cityId);
            }
        }

        private static void DeleteGovernorKeysInContainer(Container c, int cityId)
        {
            if (c == null || c.Deleted)
                return;

            Item[] items = c.Items.ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];

                if (item == null || item.Deleted)
                    continue;

                if (item is ChaveDoGovernador)
                {
                    ChaveDoGovernador chave = (ChaveDoGovernador)item;

                    if (chave.CityId == cityId)
                    {
                        chave.Delete();
                        continue;
                    }
                }

                Container sub = item as Container;

                if (sub != null)
                    DeleteGovernorKeysInContainer(sub, cityId);
            }
        }
    }
}
