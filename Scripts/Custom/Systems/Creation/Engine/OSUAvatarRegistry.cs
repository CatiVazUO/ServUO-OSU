using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Mobiles;

namespace Server.Custom.Systems.Creation.Engine
{
    public static class OSUAvatarRegistry
    {
        private static readonly object _sync = new object();
        private static HashSet<int> _used = new HashSet<int>();

        private static string FilePath
        {
            get { return Path.Combine(Core.BaseDirectory, "Saves", "OSU_Avatars.bin"); }
        }

        public static void Initialize()
        {
            EventSink.WorldLoad += OnWorldLoad;
            EventSink.WorldSave += OnWorldSave;
        }

        public static bool IsUsed(int avatarId)
        {
            if (avatarId <= 0) return false;

            lock (_sync)
                return _used.Contains(avatarId);
        }

        public static bool TryMarkUsed(int avatarId, out string reason)
        {
            reason = null;

            if (avatarId <= 0)
            {
                reason = "Avatar inválido.";
                return false;
            }

            lock (_sync)
            {
                if (_used.Contains(avatarId))
                {
                    reason = "Esse avatar já foi escolhido por outro jogador.";
                    return false;
                }

                _used.Add(avatarId);
                return true;
            }
        }

        public static bool UnmarkUsed(int avatarId)
        {
            if (avatarId <= 0)
                return false;

            lock (_sync)
                return _used.Remove(avatarId);
        }

        public static bool IsUsedByOther(PlayerMobile pm, int avatarId)
        {
            if (avatarId <= 0)
                return false;

            if (pm != null && pm.OSUAvatarId == avatarId)
                return false;

            return IsUsed(avatarId);
        }

        private static void OnWorldLoad()
        {
            lock (_sync)
            {
                _used.Clear();

                try
                {
                    if (!File.Exists(FilePath))
                        return;

                    using (FileStream fs = File.OpenRead(FilePath))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int version = br.ReadInt32();
                        int count = br.ReadInt32();

                        for (int i = 0; i < count; i++)
                            _used.Add(br.ReadInt32());
                    }
                }
                catch
                {
                    // se der ruim, a gente só volta vazio (não crasha)
                    _used.Clear();
                }
            }
        }

        private static void OnWorldSave(WorldSaveEventArgs e)
        {
            lock (_sync)
            {
                try
                {
                    string dir = Path.GetDirectoryName(FilePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    using (FileStream fs = File.Create(FilePath))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(1); // version
                        bw.Write(_used.Count);

                        foreach (int id in _used)
                            bw.Write(id);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
