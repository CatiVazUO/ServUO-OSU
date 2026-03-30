using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom
{
    public static class OSUHairGrowthSystem
    {
        private const int HairItemBase = 13050;
        private const int HairGumpMaleBase = 54000;
        private const int HairGumpFemaleBase = 64000;
        private const int BeardItemBase = 15160;
        private const int BeardGumpBase = 53500;

        private const int MaleLongHairGumpId = 54013;
        private const int FemaleLongHairGumpId = 64002;
        private const int LongBeardGumpId = 53500;

        private static readonly Dictionary<int, DateTime> _lastHairCutBySerial = new Dictionary<int, DateTime>();
        private static readonly Dictionary<int, DateTime> _lastBeardTrimBySerial = new Dictionary<int, DateTime>();
        private static readonly object _sync = new object();
        private static Timer _timer;

        private static string FilePath
        {
            get { return Path.Combine(Core.BaseDirectory, "Saves", "OSU_HairGrowth.bin"); }
        }

        public static TimeSpan HairGrowthDelay
        {
            get { return TimeSpan.FromDays(40.0); }
        }

        public static TimeSpan BeardGrowthDelay
        {
            get { return TimeSpan.FromDays(25.0); }
        }

        public static TimeSpan CheckInterval
        {
            get { return TimeSpan.FromHours(12.0); }
        }

        public static void Initialize()
        {
            EventSink.WorldLoad += OnWorldLoad;
            EventSink.WorldSave += OnWorldSave;
            EventSink.Login += OnLogin;

            if (_timer == null)
                _timer = Timer.DelayCall(CheckInterval, CheckInterval, CheckOnlinePlayers);
        }

        public static void ResetHairGrowthTimer(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            lock (_sync)
                _lastHairCutBySerial[pm.Serial.Value] = DateTime.UtcNow;
        }

        public static void ResetBeardGrowthTimer(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            lock (_sync)
                _lastBeardTrimBySerial[pm.Serial.Value] = DateTime.UtcNow;
        }

        public static DateTime GetLastHairCutUtc(PlayerMobile pm)
        {
            if (pm == null)
                return DateTime.UtcNow;

            lock (_sync)
            {
                DateTime dt;
                if (_lastHairCutBySerial.TryGetValue(pm.Serial.Value, out dt))
                    return dt;
            }

            return DateTime.UtcNow;
        }

        public static DateTime GetLastBeardTrimUtc(PlayerMobile pm)
        {
            if (pm == null)
                return DateTime.UtcNow;

            lock (_sync)
            {
                DateTime dt;
                if (_lastBeardTrimBySerial.TryGetValue(pm.Serial.Value, out dt))
                    return dt;
            }

            return DateTime.UtcNow;
        }

        public static void EnsureTracked(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            lock (_sync)
            {
                if (!_lastHairCutBySerial.ContainsKey(pm.Serial.Value))
                    _lastHairCutBySerial[pm.Serial.Value] = DateTime.UtcNow;

                if (!pm.Female && !_lastBeardTrimBySerial.ContainsKey(pm.Serial.Value))
                    _lastBeardTrimBySerial[pm.Serial.Value] = DateTime.UtcNow;
            }
        }

        public static bool TryApplyGrowth(PlayerMobile pm)
        {
            bool hairChanged = TryApplyHairGrowth(pm);
            bool beardChanged = TryApplyBeardGrowth(pm);
            return hairChanged || beardChanged;
        }

        public static bool TryApplyHairGrowth(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return false;

            EnsureTracked(pm);

            DateTime lastCut;
            lock (_sync)
                lastCut = _lastHairCutBySerial[pm.Serial.Value];

            if ((DateTime.UtcNow - lastCut) < HairGrowthDelay)
                return false;

            if (pm.HairItemID <= 0)
            {
                lock (_sync)
                    _lastHairCutBySerial[pm.Serial.Value] = DateTime.UtcNow;
                return false;
            }

            int desiredGump = pm.Female ? FemaleLongHairGumpId : MaleLongHairGumpId;
            int desiredItem = HairGumpToItem(pm.Female, desiredGump);

            if (pm.HairItemID == desiredItem)
                return false;

            pm.HairItemID = desiredItem;
            pm.Delta(MobileDelta.Hair);
            pm.ProcessDelta();
            RefreshPaperdoll(pm);

            if (pm.NetState != null)
                pm.SendMessage(0x59, "Seu cabelo cresceu.");

            return true;
        }

        public static bool TryApplyBeardGrowth(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.Female)
                return false;

            EnsureTracked(pm);

            DateTime lastTrim;
            lock (_sync)
                lastTrim = _lastBeardTrimBySerial[pm.Serial.Value];

            if ((DateTime.UtcNow - lastTrim) < BeardGrowthDelay)
                return false;

            if (pm.FacialHairItemID <= 0)
            {
                lock (_sync)
                    _lastBeardTrimBySerial[pm.Serial.Value] = DateTime.UtcNow;
                return false;
            }

            int desiredItem = BeardGumpToItem(LongBeardGumpId);

            if (pm.FacialHairItemID == desiredItem)
                return false;

            pm.FacialHairItemID = desiredItem;
            pm.Delta(MobileDelta.FacialHair);
            pm.ProcessDelta();
            RefreshPaperdoll(pm);

            if (pm.NetState != null)
                pm.SendMessage(0x59, "Sua barba cresceu.");

            return true;
        }

        public static int HairGumpToItem(bool female, int gumpId)
        {
            if (gumpId <= 0)
                return 0;

            int offset = gumpId - (female ? HairGumpFemaleBase : HairGumpMaleBase);
            if (offset < 0)
                return 0;

            return HairItemBase + offset;
        }

        public static int HairItemToGump(bool female, int itemId)
        {
            if (itemId <= 0)
                return 0;

            int offset = itemId - HairItemBase;
            if (offset < 0)
                return 0;

            return (female ? HairGumpFemaleBase : HairGumpMaleBase) + offset;
        }

        public static int BeardGumpToItem(int gumpId)
        {
            if (gumpId <= 0)
                return 0;

            int offset = gumpId - BeardGumpBase;
            if (offset < 0)
                return 0;

            return BeardItemBase + offset;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            EnsureTracked(pm);
            TryApplyGrowth(pm);
        }

        private static void CheckOnlinePlayers()
        {
            foreach (NetState ns in NetState.Instances)
            {
                if (ns == null)
                    continue;

                PlayerMobile pm = ns.Mobile as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                TryApplyGrowth(pm);
            }
        }

        private static void RefreshPaperdoll(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.NetState == null)
                return;

            int bodyVariant = 0;
            int faceIndex = 0;

            TryReadCustomInt(pm, "OSUBodyVariant", ref bodyVariant);
            TryReadCustomInt(pm, "OSUFaceIndex", ref faceIndex);

            string title = (pm.Name ?? String.Empty) + String.Format(" [OSUPD:{0}:{1}]", bodyVariant, faceIndex);
            pm.NetState.Send(new DisplayPaperdoll(pm, title, true));
        }

        private static void TryReadCustomInt(object obj, string propName, ref int value)
        {
            if (obj == null || String.IsNullOrEmpty(propName))
                return;

            try
            {
                var prop = obj.GetType().GetProperty(propName);
                if (prop == null || !prop.CanRead)
                    return;

                object raw = prop.GetValue(obj, null);
                if (raw is int)
                    value = (int)raw;
            }
            catch
            {
            }
        }

        private static void OnWorldLoad()
        {
            lock (_sync)
            {
                _lastHairCutBySerial.Clear();
                _lastBeardTrimBySerial.Clear();

                try
                {
                    if (!File.Exists(FilePath))
                        return;

                    using (FileStream fs = File.OpenRead(FilePath))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int version = br.ReadInt32();
                        int hairCount = br.ReadInt32();

                        for (int i = 0; i < hairCount; i++)
                        {
                            int serial = br.ReadInt32();
                            long ticks = br.ReadInt64();
                            _lastHairCutBySerial[serial] = new DateTime(ticks, DateTimeKind.Utc);
                        }

                        if (version >= 2)
                        {
                            int beardCount = br.ReadInt32();

                            for (int i = 0; i < beardCount; i++)
                            {
                                int serial = br.ReadInt32();
                                long ticks = br.ReadInt64();
                                _lastBeardTrimBySerial[serial] = new DateTime(ticks, DateTimeKind.Utc);
                            }
                        }
                    }
                }
                catch
                {
                    _lastHairCutBySerial.Clear();
                    _lastBeardTrimBySerial.Clear();
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
                        bw.Write(2);
                        bw.Write(_lastHairCutBySerial.Count);

                        foreach (KeyValuePair<int, DateTime> kv in _lastHairCutBySerial)
                        {
                            bw.Write(kv.Key);
                            bw.Write(kv.Value.ToUniversalTime().Ticks);
                        }

                        bw.Write(_lastBeardTrimBySerial.Count);

                        foreach (KeyValuePair<int, DateTime> kv in _lastBeardTrimBySerial)
                        {
                            bw.Write(kv.Key);
                            bw.Write(kv.Value.ToUniversalTime().Ticks);
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
