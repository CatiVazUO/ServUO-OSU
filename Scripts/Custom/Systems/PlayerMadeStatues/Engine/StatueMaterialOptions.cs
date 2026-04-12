using System;
using System.Collections.Generic;
using Server.Custom.Systems.OSUResources;
using Server.Items;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public sealed class StatueMaterialOption
    {
        public int MaterialId { get; private set; }
        public string Name { get; private set; }
        public int Hue { get; private set; }
        public Type SmallMaterialType { get; private set; }
        public Type LargeMaterialType { get; private set; }
        public bool AllowPlatforms { get; private set; }

        public StatueMaterialOption(int materialId, string name, int hue, Type smallMaterialType, Type largeMaterialType, bool allowPlatforms)
        {
            MaterialId = materialId;
            Name = name;
            Hue = hue;
            SmallMaterialType = smallMaterialType;
            LargeMaterialType = largeMaterialType;
            AllowPlatforms = allowPlatforms;
        }
    }

    public static class StatueMaterialOptions
    {
        private static readonly StatueMaterialOption[] m_All = new StatueMaterialOption[]
        {
            new StatueMaterialOption(OSUMaterialIds.Stone, "Pedra", 0xA7F, typeof(StoneBlockSmall), typeof(StoneBlockLarge), true),
            new StatueMaterialOption(OSUMaterialIds.Marble, "Mármore", 0x0455, typeof(MarbleBlockSmall), typeof(MarbleBlockLarge), true),
            new StatueMaterialOption(OSUMaterialIds.Granite, "Granito", 0x0835, typeof(GraniteBlockSmall), typeof(GraniteBlockLarge), true),
            new StatueMaterialOption(OSUMaterialIds.Soapstone, "Pedra-sabão", 0x097B, typeof(SoapstoneBlockSmall), typeof(SoapstoneBlockLarge), true),
            new StatueMaterialOption(OSUMaterialIds.Bronze, "Bronze", 0x0972, typeof(BronzeBlock), typeof(BronzeBlock), true),
            new StatueMaterialOption(OSUMaterialIds.Clay, "Argila", 0x096F, typeof(ClayPileSmall), typeof(ClayPileLarge), true),
            new StatueMaterialOption(OSUMaterialIds.Kaolin, "Porcelana", 0x0481, typeof(KaolinPowder), typeof(KaolinPowder), true)
        };

        public static StatueMaterialOption[] All { get { return m_All; } }

        public static StatueMaterialOption[] GetForUse(StatueMaterialUse use)
        {
            List<StatueMaterialOption> list = new List<StatueMaterialOption>();
            for (int i = 0; i < m_All.Length; i++)
            {
                if (use == StatueMaterialUse.Sculpture || m_All[i].AllowPlatforms)
                    list.Add(m_All[i]);
            }
            return list.ToArray();
        }

        public static StatueMaterialOption GetById(int materialId)
        {
            for (int i = 0; i < m_All.Length; i++)
            {
                if (m_All[i].MaterialId == materialId)
                    return m_All[i];
            }
            return m_All[0];
        }

        public static int GetHue(int materialId) { return GetById(materialId).Hue; }
        public static string GetName(int materialId) { return GetById(materialId).Name; }
        public static Type GetSmallMaterialType(int materialId) { return GetById(materialId).SmallMaterialType; }
        public static Type GetLargeMaterialType(int materialId) { return GetById(materialId).LargeMaterialType; }
        public static bool AllowsPlatforms(int materialId) { return GetById(materialId).AllowPlatforms; }
    }
}
