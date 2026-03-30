using System;
using System.Reflection;
using Server.Mobiles;

namespace Server.Custom.Systems.PlayerMadeStatues
{
    public sealed class StatuePoseDefinition
    {
        public string Name { get; private set; }
        public int Animation { get; private set; }
        public int FrameCount { get; private set; }
        public bool Forward { get; private set; }

        public StatuePoseDefinition(string name, int animation, int frameCount, bool forward)
        {
            Name = name ?? "Idle";
            Animation = animation;
            FrameCount = frameCount;
            Forward = forward;
        }
    }

    public sealed class StatueMobileProfile
    {
        public bool Enabled { get; private set; }
        public int RequiredResourceAmount { get; private set; }
        public StatuePlatformSize PlatformSize { get; private set; }
        public int PlatformZOffset { get; private set; }
        public StatuePoseDefinition[] Poses { get; private set; }
        public int SuccessChance { get; private set; }
        public SculptorRequirement[] ExtraRequirements { get; private set; }

        public bool IsValid { get { return Enabled && Poses != null && Poses.Length > 0 && PlatformSize != StatuePlatformSize.None; } }

        public StatueMobileProfile(bool enabled, int requiredResourceAmount, StatuePlatformSize platformSize, int platformZOffset, StatuePoseDefinition[] poses, int successChance, SculptorRequirement[] extraRequirements)
        {
            Enabled = enabled;
            RequiredResourceAmount = requiredResourceAmount;
            PlatformSize = platformSize;
            PlatformZOffset = platformZOffset;
            Poses = poses ?? new StatuePoseDefinition[0];
            SuccessChance = successChance;
            ExtraRequirements = extraRequirements ?? new SculptorRequirement[0];
        }
    }

    public static class StatueMobileProfileReader
    {
        public static StatueMobileProfile GetFrom(Mobile m)
        {
            if (m == null)
                return null;

            bool enabled = ReadBool(m, "StatueCanBeLiveModel", false);
            int amount = ReadInt(m, "StatueMaterialAmount", 0);
            StatuePlatformSize size = ReadEnum<StatuePlatformSize>(m, "StatuePlatformSize", StatuePlatformSize.None);
            int zOffset = ReadInt(m, "StatuePlatformZOffset", 0);
            StatuePoseDefinition[] poses = ReadPoses(m, "StatueAllowedPoses");
            int successChance = ReadInt(m, "StatueSuccessChance", 80);
            SculptorRequirement[] extraRequirements = ReadRequirements(m, "StatueExtraRequirements");

            return new StatueMobileProfile(enabled, amount, size, zOffset, poses, successChance, extraRequirements);
        }

        private static bool ReadBool(object obj, string propName, bool fallback)
        {
            try
            {
                PropertyInfo pi = obj.GetType().GetProperty(propName);
                if (pi == null || !pi.CanRead)
                    return fallback;
                object val = pi.GetValue(obj, null);
                if (val is bool)
                    return (bool)val;
            }
            catch { }
            return fallback;
        }

        private static int ReadInt(object obj, string propName, int fallback)
        {
            try
            {
                PropertyInfo pi = obj.GetType().GetProperty(propName);
                if (pi == null || !pi.CanRead)
                    return fallback;
                object val = pi.GetValue(obj, null);
                if (val is int)
                    return (int)val;
            }
            catch { }
            return fallback;
        }

        private static T ReadEnum<T>(object obj, string propName, T fallback)
        {
            try
            {
                PropertyInfo pi = obj.GetType().GetProperty(propName);
                if (pi == null || !pi.CanRead)
                    return fallback;
                object val = pi.GetValue(obj, null);
                if (val is T)
                    return (T)val;
            }
            catch { }
            return fallback;
        }

        private static StatuePoseDefinition[] ReadPoses(object obj, string propName)
        {
            try
            {
                PropertyInfo pi = obj.GetType().GetProperty(propName);
                if (pi == null || !pi.CanRead)
                    return new StatuePoseDefinition[0];
                StatuePoseDefinition[] poses = pi.GetValue(obj, null) as StatuePoseDefinition[];
                return poses ?? new StatuePoseDefinition[0];
            }
            catch { return new StatuePoseDefinition[0]; }
        }
        private static SculptorRequirement[] ReadRequirements(object obj, string propName)
        {
            try
            {
                PropertyInfo pi = obj.GetType().GetProperty(propName);
                if (pi == null || !pi.CanRead)
                    return new SculptorRequirement[0];
                SculptorRequirement[] reqs = pi.GetValue(obj, null) as SculptorRequirement[];
                return reqs ?? new SculptorRequirement[0];
            }
            catch { return new SculptorRequirement[0]; }
        }

    }
}
