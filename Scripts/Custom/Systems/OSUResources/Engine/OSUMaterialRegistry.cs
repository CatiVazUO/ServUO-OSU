using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Custom.Systems.OSUResources
{
    public static class OSUMaterialRegistry
    {
        private static readonly List<OSUMaterialDefinition> m_All = new List<OSUMaterialDefinition>();
        private static readonly Dictionary<int, OSUMaterialDefinition> m_ById = new Dictionary<int, OSUMaterialDefinition>();
        private static readonly Dictionary<string, OSUMaterialDefinition> m_ByKey = new Dictionary<string, OSUMaterialDefinition>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<Type, OSUMaterialDefinition> m_ByItemType = new Dictionary<Type, OSUMaterialDefinition>();

        public static IList<OSUMaterialDefinition> All
        {
            get { return m_All.AsReadOnly(); }
        }

        static OSUMaterialRegistry()
        {
            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Stone, "stone", "Pedra", OSUMaterialCategory.Stone, 0xA7F,
                true, true, true, true, true,
                typeof(StoneChunk), typeof(StoneBlockSmall), typeof(StoneBlockLarge), null, null,
                "Pedra comum. O chunk serve para crafts gerais. Os blocos servem para escultura."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Granite, "granite", "Granito", OSUMaterialCategory.Stone, 0x835,
                true, true, true, true, true,
                typeof(GraniteChunk), typeof(GraniteBlockSmall), typeof(GraniteBlockLarge), null, null,
                "Granito ornamental. O chunk serve para crafts gerais. Os blocos servem para escultura."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Marble, "marble", "Mármore", OSUMaterialCategory.Stone, 0x47E,
                true, true, true, true, true,
                typeof(MarbleChunk), typeof(MarbleBlockSmall), typeof(MarbleBlockLarge), typeof(MarblePowder), null,
                "Mármore. O pó pode ser usado depois em remédios, papel, tintas e outros crafts."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Soapstone, "soapstone", "Pedra-sabão", OSUMaterialCategory.Stone, 0x97B,
                true, true, true, true, true,
                typeof(SoapstoneChunk), typeof(SoapstoneBlockSmall), typeof(SoapstoneBlockLarge), null, null,
                "Pedra-sabão. O chunk serve para crafts gerais. Os blocos servem para escultura."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Clay, "clay", "Argila", OSUMaterialCategory.Clay, 0x96F,
                true, true, true, true, true,
                typeof(ClayChunk), typeof(ClayPileSmall), typeof(ClayPileLarge), null, null,
                "Argila. O chunk é o material bruto. Os montes servem para modelagem e escultura."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Kaolin, "kaolin", "Caolim", OSUMaterialCategory.Clay, 0x47F,
                true, false, false, false, false,
                typeof(KaolinChunk), null, null, typeof(KaolinPowder), null,
                "Caolim. Precisa ser processado em pó para porcelana e outros crafts."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Quartz, "quartz", "Quartzo", OSUMaterialCategory.Mineral, 0x47F,
                true, false, false, false, false,
                typeof(QuartzChunk), null, null, typeof(QuartzPowder), null,
                "Quartzo. Precisa ser processado em pó para porcelana e outros crafts."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Tin, "tin", "Estanho", OSUMaterialCategory.Metal, 0x973,
                true, false, false, false, false,
                typeof(TinNugget), null, null, null, typeof(BronzeBlock),
                "Pepita de estanho. Deve ser fundida com cobre para virar bronze."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Copper, "copper", "Cobre", OSUMaterialCategory.Metal, 0x96D,
                true, false, false, false, false,
                typeof(CopperNugget), null, null, null, typeof(BronzeBlock),
                "Pepita de cobre. Deve ser fundida com estanho para virar bronze."));

            Register(new OSUMaterialDefinition(
                OSUMaterialIds.Bronze, "bronze", "Bronze", OSUMaterialCategory.Metal, 0x972,
                false, false, false, false, false,
                typeof(BronzeBlock), null, null, null, null,
                "Bronze refinado. Placeholder para processos futuros de fundição e craft."));
        }

        public static void Register(OSUMaterialDefinition def)
        {
            if (def == null)
                return;

            if (m_ById.ContainsKey(def.Id))
                return;

            m_All.Add(def);
            m_ById[def.Id] = def;
            m_ByKey[def.Key] = def;

            RegisterType(def.RawType, def);
            RegisterType(def.SmallSculptType, def);
            RegisterType(def.LargeSculptType, def);
            RegisterType(def.PowderType, def);
            RegisterType(def.RefinedType, def);
        }

        private static void RegisterType(Type t, OSUMaterialDefinition def)
        {
            if (t == null || def == null)
                return;

            m_ByItemType[t] = def;
        }

        public static OSUMaterialDefinition GetById(int id)
        {
            OSUMaterialDefinition def;
            m_ById.TryGetValue(id, out def);
            return def;
        }

        public static OSUMaterialDefinition GetByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            OSUMaterialDefinition def;
            m_ByKey.TryGetValue(key, out def);
            return def;
        }

        public static OSUMaterialDefinition GetByItem(Item item)
        {
            if (item == null)
                return null;

            return GetByType(item.GetType());
        }

        public static OSUMaterialDefinition GetByType(Type type)
        {
            if (type == null)
                return null;

            OSUMaterialDefinition def;
            m_ByItemType.TryGetValue(type, out def);
            return def;
        }
    }
}
