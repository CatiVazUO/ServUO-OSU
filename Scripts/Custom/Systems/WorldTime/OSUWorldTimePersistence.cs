using System;
using System.IO;
using Server;

namespace Server.Custom.Systems.WorldTime
{
    public static class OSUWorldTimePersistence
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_WorldTime.bin");

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += e => Save();
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                OSUWorldTime.GetBase(out var baseWorld, out var baseRealUtc, out var paused);

                using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(1); // version

                    bw.Write(baseWorld.ToBinary());
                    bw.Write(baseRealUtc.ToBinary());
                    bw.Write(paused);

                    bw.Write(OSUWorldTime.SpringRealDays);
                    bw.Write(OSUWorldTime.SummerRealDays);
                    bw.Write(OSUWorldTime.AutumnRealDays);
                    bw.Write(OSUWorldTime.WinterRealDays);
                }
            }
            catch
            {
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return;

                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();

                    if (version >= 1)
                    {
                        var baseWorld = DateTime.FromBinary(br.ReadInt64());
                        var baseRealUtc = DateTime.FromBinary(br.ReadInt64());
                        bool paused = br.ReadBoolean();

                        OSUWorldTime.SetBase(baseWorld, baseRealUtc, paused);

                        OSUWorldTime.SpringRealDays = br.ReadInt32();
                        OSUWorldTime.SummerRealDays = br.ReadInt32();
                        OSUWorldTime.AutumnRealDays = br.ReadInt32();
                        OSUWorldTime.WinterRealDays = br.ReadInt32();
                    }
                }
            }
            catch
            {
            }
        }
    }
}
