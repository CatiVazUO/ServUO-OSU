
using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Health
{
    public static class OSUBandageUtility
    {
        private class PendingBandageBonus
        {
            public OSUMedicatedBandageType Type;
            public int HealBonus;
            public int PoisonBleedBonus;
            public DateTime ExpiresUtc;
        }

        private static readonly Dictionary<int, PendingBandageBonus> _pending = new Dictionary<int, PendingBandageBonus>();

        public static void PrepareBandageUse(Mobile healer, Mobile patient, Item bandage)
        {
            if (healer == null || bandage == null)
                return;

            OSUMedicatedBandage med = bandage as OSUMedicatedBandage;
            if (med == null)
                return;

            PendingBandageBonus bonus = new PendingBandageBonus();
            bonus.Type = med.MedicineType;
            bonus.HealBonus = med.ExtraHealBonus;
            bonus.PoisonBleedBonus = med.ExtraPoisonBleedChance;
            bonus.ExpiresUtc = DateTime.UtcNow + TimeSpan.FromSeconds(15.0);

            _pending[healer.Serial.Value] = bonus;
        }

        public static int PullHealBonus(Mobile healer)
        {
            PendingBandageBonus bonus;
            if (healer == null || !_pending.TryGetValue(healer.Serial.Value, out bonus))
                return 0;

            if (bonus.ExpiresUtc <= DateTime.UtcNow)
            {
                _pending.Remove(healer.Serial.Value);
                return 0;
            }

            if (bonus.Type != OSUMedicatedBandageType.HealingBonus)
                return 0;

            return bonus.HealBonus;
        }

        public static double PullSpeedBonusSeconds(Mobile healer)
        {
            PendingBandageBonus bonus;

            if (healer == null || !_pending.TryGetValue(healer.Serial.Value, out bonus))
                return 0.0;

            if (bonus.ExpiresUtc <= DateTime.UtcNow)
            {
                _pending.Remove(healer.Serial.Value);
                return 0.0;
            }

            if (bonus.Type != OSUMedicatedBandageType.SpeedBonus)
                return 0.0;

            return 2.0; // só 2 segundos mais rápido
        }

        public static int PullPoisonBleedBonus(Mobile healer)
        {
            PendingBandageBonus bonus;
            if (healer == null || !_pending.TryGetValue(healer.Serial.Value, out bonus))
                return 0;

            if (bonus.ExpiresUtc <= DateTime.UtcNow)
            {
                _pending.Remove(healer.Serial.Value);
                return 0;
            }

            if (bonus.Type != OSUMedicatedBandageType.Antiseptic)
                return 0;

            return bonus.PoisonBleedBonus;
        }

        public static void ClearPending(Mobile healer)
        {
            if (healer == null)
                return;

            _pending.Remove(healer.Serial.Value);
        }

        public static void DropBloodyBandage(Mobile healer, Mobile patient, Item bandageUsed)
        {
            if (healer == null || healer.Deleted || bandageUsed == null)
                return;

            Container pack = healer.Backpack;

            if (pack != null && !pack.Deleted)
            {
                foreach (Item item in pack.Items)
                {
                    OSUBloodyBandage existing = item as OSUBloodyBandage;
                    if (existing != null && !existing.Deleted)
                    {
                        existing.Amount += 1;
                        return;
                    }
                }

                pack.DropItem(new OSUBloodyBandage(1));
                return;
            }

            OSUBloodyBandage dirty = new OSUBloodyBandage(1);
            dirty.MoveToWorld(healer.Location, healer.Map);
        }
    }
}
