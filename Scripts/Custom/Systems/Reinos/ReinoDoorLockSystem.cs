using System;
using System.Collections.Generic;
using System.IO;
using Server.Custom.Reinos;
using Server.Custom.Systems.Rent;
using Server.Multis;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.Systems.Reinos
{
    public enum ReinoDoorLockMaterial
    {
        Iron = 0,
        DullCopper,
        Copper,
        Bronze,
        Gold,
        Agapite,
        Verite,
        Valorite
    }

    public class ReinoDoorLockData
    {
        public ReinoDoorLockMaterial Material;
        public int MaxUses;
        public int RemainingUses;
        public int PickPenalty;
    }

    public static class ReinoDoorLockSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoDoorLocks_v1.bin");
        private static readonly Dictionary<int, ReinoDoorLockData> m_Data = new Dictionary<int, ReinoDoorLockData>();
        private static readonly Dictionary<int, LockpickAttemptState> m_Attempts = new Dictionary<int, LockpickAttemptState>();

        private class LockpickAttemptState
        {
            public int PickerSerial;
            public int DoorSerial;
            public int PickSerial;
            public Point3D StartLocation;
            public Map StartMap;
            public DateTime FinishUtc;
        }

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };
        }

        public static string GetMaterialLabel(ReinoDoorLockMaterial material)
        {
            switch (material)
            {
                case ReinoDoorLockMaterial.DullCopper: return "Dull Copper";
                case ReinoDoorLockMaterial.Copper: return "Copper";
                case ReinoDoorLockMaterial.Bronze: return "Bronze";
                case ReinoDoorLockMaterial.Gold: return "Gold";
                case ReinoDoorLockMaterial.Agapite: return "Agapite";
                case ReinoDoorLockMaterial.Verite: return "Verite";
                case ReinoDoorLockMaterial.Valorite: return "Valorite";
                default: return "Ferro";
            }
        }

        public static int GetDefaultUses(ReinoDoorLockMaterial material)
        {
            switch (material)
            {
                case ReinoDoorLockMaterial.DullCopper: return 550;
                case ReinoDoorLockMaterial.Copper: return 600;
                case ReinoDoorLockMaterial.Bronze: return 650;
                case ReinoDoorLockMaterial.Gold: return 700;
                case ReinoDoorLockMaterial.Agapite: return 750;
                case ReinoDoorLockMaterial.Verite: return 800;
                case ReinoDoorLockMaterial.Valorite: return 850;
                default: return 500;
            }
        }

        public static int GetPickPenalty(ReinoDoorLockMaterial material)
        {
            switch (material)
            {
                case ReinoDoorLockMaterial.DullCopper: return 3;
                case ReinoDoorLockMaterial.Copper: return 6;
                case ReinoDoorLockMaterial.Bronze: return 9;
                case ReinoDoorLockMaterial.Gold: return 12;
                case ReinoDoorLockMaterial.Agapite: return 15;
                case ReinoDoorLockMaterial.Verite: return 18;
                case ReinoDoorLockMaterial.Valorite: return 21;
                default: return 0;
            }
        }

        public static bool TryInstallLock(Mobile from, BaseDoor door, ReinoDoorLockKit kit, out string message)
        {
            message = string.Empty;

            if (from == null || door == null || kit == null || kit.Deleted)
            {
                message = "Fechadura inválida.";
                return false;
            }

            if (from.Map != door.Map || !from.InRange(door.GetWorldLocation(), 2))
            {
                message = "Você precisa estar ao lado da porta.";
                return false;
            }

            ReinoDoorLockData existing;
            if (m_Data.TryGetValue(door.Serial.Value, out existing) && existing != null && existing.RemainingUses > 0)
            {
                message = "Essa porta já possui uma fechadura instalada. Espere ela quebrar para trocar.";
                return false;
            }

            m_Data[door.Serial.Value] = new ReinoDoorLockData
            {
                Material = kit.Material,
                MaxUses = kit.MaxUses,
                RemainingUses = kit.MaxUses,
                PickPenalty = kit.PickPenalty
            };

            door.InvalidateProperties();
            door.Delta(ItemDelta.Update);

            if (door.Link != null && !door.Link.Deleted)
            {
                door.Link.InvalidateProperties();
                door.Link.Delta(ItemDelta.Update);
            }

            message = "Você instala uma nova fechadura na porta.";
            return true;
        }

        public static void OnDoorOpened(BaseDoor door)
        {
            if (door == null || door.Deleted)
                return;

            ReinoDoorLockData data;
            if (!m_Data.TryGetValue(door.Serial.Value, out data) || data == null)
                return;

            if (data.RemainingUses > 0)
                data.RemainingUses--;

            if (data.RemainingUses == 30)
                door.PublicOverheadMessage(MessageType.Regular, 0x55, false, "*sua fechadura esta rangindo*");

            if (data.RemainingUses <= 0)
            {
                m_Data.Remove(door.Serial.Value);
                door.PublicOverheadMessage(MessageType.Regular, 0x22, false, "*sua fechadura quebrou*");
            }
        }

        public static void RemoveDoorLock(BaseDoor door)
        {
            if (door == null)
                return;

            m_Data.Remove(door.Serial.Value);

            door.InvalidateProperties();
            door.Delta(ItemDelta.Update);

            if (door.Link != null && !door.Link.Deleted)
            {
                m_Data.Remove(door.Link.Serial.Value);
                door.Link.InvalidateProperties();
                door.Link.Delta(ItemDelta.Update);
            }
        }

        public static void AddDoorProperties(BaseDoor door, ObjectPropertyList list)
        {
            if (door == null || list == null)
                return;

            ReinoDoorLockData data;
            if (!m_Data.TryGetValue(door.Serial.Value, out data) || data == null)
                return;

            list.Add("Fechadura: {0}", GetMaterialLabel(data.Material));
            list.Add("Desgaste: {0}", GetWearLabel(data));
          //  list.Add("Usos restantes: {0}", data.RemainingUses);
        }

        private static string GetWearLabel(ReinoDoorLockData data)
        {
            if (data == null || data.MaxUses <= 0)
                return "sem dados";

            double pct = data.RemainingUses / (double)data.MaxUses;
            if (pct <= 0.20)
                return "enferrujada";
            if (pct <= 0.60)
                return "desgastada";
            return "nova";
        }


        private static TownHouse FindRentalHouseForDoor(BaseDoor door)
        {
            if (door == null || door.Deleted || door.Map == null || door.Map == Map.Internal)
                return null;

            foreach (TownHouse house in TownHouse.AllTownHouses)
            {
                if (house == null || house.Deleted || house.Map != door.Map)
                    continue;

                try
                {
                    if (house.IsInside(door.Location, 16))
                        return house;
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool HasRentalAccess(TownHouse house, Mobile from)
        {
            if (house == null || from == null)
                return false;

            if (house.Owner == from)
                return true;

            try
            {
                if (house.IsCoOwner(from) || house.IsFriend(from))
                    return true;
            }
            catch
            {
            }

            return false;
        }

        private static bool ConsumeLockpicks(Lockpick pick, int amount)
        {
            if (pick == null || pick.Deleted || amount <= 0)
                return false;

            if (pick.Amount < amount)
                return false;

            pick.Consume(amount);
            return true;
        }

        public static bool TryHandleLockpickTarget(Mobile from, Lockpick pick, object targeted)
        {
            BaseDoor door = targeted as BaseDoor;
            if (door == null)
                return false;

            if (from == null || pick == null || pick.Deleted || door.Deleted)
                return true;

            TownHouse rentalHouse = FindRentalHouseForDoor(door);
            if (rentalHouse == null)
                return false;

            if (from.Map != door.Map || !from.InRange(door.GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446);
                return true;
            }

            if (HasRentalAccess(rentalHouse, from))
            {
                from.SendMessage(0x35, "Você não pode arrombar uma porta de uma casa à qual já possui acesso.");
                return true;
            }

            if (!door.Locked)
            {
                from.SendLocalizedMessage(502069);
                return true;
            }

            if (pick.Amount < 2)
            {
                from.SendMessage(0x35, "Você precisa de 2 gazuas para tentar arrombar essa porta.");
                return true;
            }

            double skill = from.Skills[SkillName.Lockpicking].Value;
            if (skill < 80.0)
            {
                from.SendMessage(0x35, "Você precisa de ao menos 80.0 de Lockpicking para tentar arrombar portas.");
                return true;
            }

            ReinoMilitarySystem.NotifyLockpicking(from, door);

            int durationSeconds = Math.Max(6, 8 + (int)Math.Ceiling((100.0 - skill) / 5.0));
            from.SendMessage(0x55, "Você começa a trabalhar na fechadura. Não se mova.");
            from.PlaySound(0x241);

            LockpickAttemptState state = new LockpickAttemptState
            {
                PickerSerial = from.Serial.Value,
                DoorSerial = door.Serial.Value,
                PickSerial = pick.Serial.Value,
                StartLocation = from.Location,
                StartMap = from.Map,
                FinishUtc = DateTime.UtcNow + TimeSpan.FromSeconds(durationSeconds)
            };

            m_Attempts[from.Serial.Value] = state;
            int serial = from.Serial.Value;
            Timer.DelayCall(TimeSpan.FromSeconds(durationSeconds), delegate { EndLockpickAttempt(serial); });
            return true;
        }

        private static void EndLockpickAttempt(int serial)
        {
            LockpickAttemptState state;
            if (!m_Attempts.TryGetValue(serial, out state) || state == null)
                return;

            m_Attempts.Remove(serial);

            Mobile from = World.FindMobile((Serial)state.PickerSerial);
            BaseDoor door = World.FindItem((Serial)state.DoorSerial) as BaseDoor;
            Lockpick pick = World.FindItem((Serial)state.PickSerial) as Lockpick;

            if (from == null || from.Deleted || door == null || door.Deleted || pick == null || pick.Deleted)
                return;

            if (from.Map != state.StartMap || from.Location != state.StartLocation)
            {
                from.SendMessage(0x22, "Você se moveu e quebrou a gazua.");
                from.PlaySound(0x3A4);
                ConsumeLockpicks(pick, 2);
                return;
            }

            if (from.Map != door.Map || !from.InRange(door.GetWorldLocation(), 2))
            {
                from.SendMessage(0x22, "Você se afastou demais da porta.");
                ConsumeLockpicks(pick, 2);
                return;
            }

            double skill = from.Skills[SkillName.Lockpicking].Value;
            int baseChance = 20 + Math.Max(0, (int)Math.Floor(skill - 80.0));

            ReinoDoorLockData data;
            int penalty = m_Data.TryGetValue(door.Serial.Value, out data) && data != null ? data.PickPenalty : 0;
            int chance = Math.Max(0, Math.Min(40, baseChance) - penalty);

            ConsumeLockpicks(pick, 2);

            if (Utility.Random(100) < chance)
            {
                from.SendMessage(0x55, "A fechadura cede e a porta se abre.");
                from.PlaySound(0x4A);

                door.Locked = false;
                door.Open = true;
                if (door.Link != null && !door.Link.Deleted)
                    door.Link.Open = true;
                door.Locked = true;
            }
            else
            {
                from.SendMessage(0x22, "Você falha em arrombar a fechadura.");
                from.PlaySound(0x3A4);
            }
        }

        private static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(1);
                    bw.Write(m_Data.Count);
                    foreach (KeyValuePair<int, ReinoDoorLockData> kv in m_Data)
                    {
                        bw.Write(kv.Key);
                        bw.Write((int)kv.Value.Material);
                        bw.Write(kv.Value.MaxUses);
                        bw.Write(kv.Value.RemainingUses);
                        bw.Write(kv.Value.PickPenalty);
                    }
                }
            }
            catch
            {
            }
        }

        private static void Load()
        {
            m_Data.Clear();

            if (!File.Exists(FilePath))
                return;

            try
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        int serial = br.ReadInt32();
                        ReinoDoorLockData data = new ReinoDoorLockData();
                        data.Material = (ReinoDoorLockMaterial)br.ReadInt32();
                        data.MaxUses = br.ReadInt32();
                        data.RemainingUses = br.ReadInt32();
                        data.PickPenalty = br.ReadInt32();
                        m_Data[serial] = data;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
