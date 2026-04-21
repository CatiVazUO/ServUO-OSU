
using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.Crafting.Tailoring.Fabrics.Bandages;

namespace Server.Custom.Systems.Health
{
    public static class OSUBandageUtility
    {
        private class PendingBandageBonus
        {
            public OSUMedicatedBandageType Type;
            public int HealBonus;
            public int PoisonBleedBonus;
            public double SpeedBonusSeconds;
            public int PatientSerial;
            public int DelayedHealPerPulse;
            public int DelayedHealPulses;
            public bool DelayedHealScheduled;
            public double InfectionChance;
            public DateTime ExpiresUtc;
        }

        private class DelayedHealTimer : Timer
        {
            private readonly int _healerSerial;
            private readonly int _patientSerial;
            private readonly int _healPerPulse;
            private int _remaining;

            public DelayedHealTimer(int healerSerial, int patientSerial, int healPerPulse, int pulses)
                : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                _healerSerial = healerSerial;
                _patientSerial = patientSerial;
                _healPerPulse = healPerPulse;
                _remaining = pulses;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                PlayerMobile patient = World.FindMobile(_patientSerial) as PlayerMobile;
                Mobile healer = World.FindMobile(_healerSerial);

                if (patient == null || patient.Deleted || !patient.Alive || _remaining <= 0)
                {
                    Stop();
                    return;
                }

                int amount = Math.Max(1, _healPerPulse);
                patient.Heal(amount, healer, false);
                patient.SendMessage(68, "A bandagem continua fazendo efeito.");

                _remaining--;
                if (_remaining <= 0)
                    Stop();
            }
        }

        private static readonly Dictionary<int, PendingBandageBonus> _pending = new Dictionary<int, PendingBandageBonus>();

        public static void PrepareBandageUse(Mobile healer, Mobile patient, Item bandage)
        {
            if (healer == null || bandage == null)
                return;

            PendingBandageBonus bonus = new PendingBandageBonus();
            bonus.Type = OSUMedicatedBandageType.None;
            bonus.HealBonus = 0;
            bonus.PoisonBleedBonus = 0;
            bonus.SpeedBonusSeconds = 0.0;
            bonus.PatientSerial = patient != null ? patient.Serial.Value : 0;
            bonus.DelayedHealPerPulse = 0;
            bonus.DelayedHealPulses = 0;
            bonus.DelayedHealScheduled = false;
            bonus.InfectionChance = 0.0;
            bonus.ExpiresUtc = DateTime.UtcNow + TimeSpan.FromSeconds(15.0);

            OSUMedicatedBandage med = bandage as OSUMedicatedBandage;
            if (med != null)
            {
                bonus.Type = med.MedicineType;
                bonus.HealBonus = med.ExtraHealBonus;
                bonus.PoisonBleedBonus = med.ExtraPoisonBleedChance;

                if (med.MedicineType == OSUMedicatedBandageType.SpeedBonus)
                    bonus.SpeedBonusSeconds = 1.0;

                _pending[healer.Serial.Value] = bonus;
                return;
            }

            if (bandage is SilkBandage)
            {
                bonus.HealBonus = 1;
                bonus.SpeedBonusSeconds = 2.0;
                _pending[healer.Serial.Value] = bonus;
                return;
            }

            if (bandage is WoolBandage)
            {
                bonus.HealBonus = 5;
                bonus.SpeedBonusSeconds = -1.0;
                _pending[healer.Serial.Value] = bonus;
                return;
            }

            if (bandage is LinenBandage)
            {
                bonus.HealBonus = 2;
                bonus.DelayedHealPerPulse = 2;
                bonus.DelayedHealPulses = 2;
                _pending[healer.Serial.Value] = bonus;
                return;
            }

            if (bandage is SkinBandage)
            {
                bonus.HealBonus = 2;
                bonus.SpeedBonusSeconds = 1.0;
                bonus.InfectionChance = 0.08;
                _pending[healer.Serial.Value] = bonus;
                return;
            }

            _pending.Remove(healer.Serial.Value);
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

            if (bonus.DelayedHealPulses > 0 && !bonus.DelayedHealScheduled && bonus.PatientSerial != 0)
            {
                bonus.DelayedHealScheduled = true;
                _pending[healer.Serial.Value] = bonus;
                new DelayedHealTimer(healer.Serial.Value, bonus.PatientSerial, bonus.DelayedHealPerPulse, bonus.DelayedHealPulses).Start();
            }

            if (bonus.InfectionChance > 0.0 && bonus.PatientSerial != 0 && Utility.RandomDouble() < bonus.InfectionChance)
            {
                Mobile patient = World.FindMobile(bonus.PatientSerial);
                if (patient != null && !patient.Deleted)
                    OSUHealthSystem.ApplyDisease(patient, OSUDiseaseType.Infeccao, true);
            }

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

            return bonus.SpeedBonusSeconds;
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
