using Server;
using Server.Custom.Systems.Needs.Gumps;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using Server.Custom.Systems.DefQual;

namespace Server.Custom.Systems.Needs
{
    public static class OSUNeedsSystem
    {
        // ===== CONFIGURÁVEL =====
        // Tick do sistema (1 minuto para cair "suave")
        public static TimeSpan TickInterval = TimeSpan.FromMinutes(1.0);

        // Queda por minuto:
        // - sede: 1 por minuto
        public static int ThirstDropPerMinute = 1;

        // - fome: 0.5 por minuto = 1 a cada 2 minutos
        public static int HungerDropEveryMinutes = 2;

        // Debuff aplica a cada 20 minutos logado quando <10
        public static int DebuffIntervalMinutes = 20;

        // Limiar para começar o debuff
        public static int DebuffStartsAt = 10;

        // Mensagens (limiares)
        public static int WarnHungry = 40;
        public static int WarnVeryHungry = 20;
        public static int WarnDebuff = 10;

        // Debuff por intervalo (20min)
        public static int DebuffPerInterval = 5;

        // Stats nunca abaixo disso
        public static int MinStat = 10;

        // ===== CONTROLE INTERNO =====
        private static readonly Dictionary<Serial, Timer> _timers = new Dictionary<Serial, Timer>();

        // Para fome cair 1 a cada 2 min
        private static readonly Dictionary<Serial, int> _hungerMinuteCounter = new Dictionary<Serial, int>();

        // Para debuff acontecer só a cada 20 min
        private static readonly Dictionary<Serial, int> _debuffMinuteCounter = new Dictionary<Serial, int>();

        // Anti-spam de mensagens (guarda o "nível" atual por necessidade)
        // 0=ok, 1=<=40, 2=<=20, 3=<=10
        private static readonly Dictionary<Serial, int> _hungerWarnLevel = new Dictionary<Serial, int>();
        private static readonly Dictionary<Serial, int> _thirstWarnLevel = new Dictionary<Serial, int>();

        public static void Initialize()
        {
            EventSink.Login += OnLogin;
            EventSink.Logout += OnLogout;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            Start(pm);

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), delegate
            {
                if (pm == null || pm.Deleted || pm.NetState == null || !pm.NetState.Running)
                    return;

                pm.CloseGump(typeof(OSUNeedsGump));
                pm.SendGump(new OSUNeedsGump(pm));
            });
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            Stop(pm);
        }

        private static void Start(PlayerMobile pm)
        {
            Stop(pm);

            Timer t = Timer.DelayCall(TickInterval, TickInterval, delegate
            {
                // C# 7.3 safe checks
                if (pm == null || pm.Deleted || pm.NetState == null || !pm.NetState.Running)
                {
                    Stop(pm);
                    return;
                }

                DoTick(pm);
            });

            _timers[pm.Serial] = t;
        }

        private static void Stop(PlayerMobile pm)
        {
            if (pm == null)
                return;

            Timer t;
            if (_timers.TryGetValue(pm.Serial, out t))
            {
                t.Stop();
                _timers.Remove(pm.Serial);
            }

            _hungerMinuteCounter.Remove(pm.Serial);
            _debuffMinuteCounter.Remove(pm.Serial);
            _hungerWarnLevel.Remove(pm.Serial);
            _thirstWarnLevel.Remove(pm.Serial);
        }

        private static void DoTick(PlayerMobile pm)
        {
            // Defqual
            double thirstRate = OSUDefQualDispatcher.ModifyThirstRate(pm, 1.0);
            if (thirstRate < 0.1)
                thirstRate = 0.1;

            double hungerRate = OSUDefQualDispatcher.ModifyHungerRate(pm, 1.0);
            if (hungerRate < 0.1)
                hungerRate = 0.1;

            // ===== 1) Queda de sede (1 por minuto) =====
            if (ThirstDropPerMinute > 0)
            {
                int thirstDrop = (int)Math.Round(ThirstDropPerMinute * thirstRate);
                if (thirstDrop < 1)
                    thirstDrop = 1;

                pm.OSUThirst = Math.Max(0, pm.OSUThirst - thirstDrop);
            }

            int extra = Climate.OSUClimatePenaltySystem.GetExtraThirstPerMinute(pm);
            if (extra > 0)
                pm.OSUThirst = Math.Max(0, pm.OSUThirst - extra);

            // ===== 2) Queda de fome (1 a cada 2 minutos) =====
            int hcount;
            if (!_hungerMinuteCounter.TryGetValue(pm.Serial, out hcount))
                hcount = 0;

            hcount++;

            int hungerEvery = (int)Math.Round(HungerDropEveryMinutes / hungerRate);
            if (hungerEvery < 1)
                hungerEvery = 1;

            if (hcount >= hungerEvery)
            {
                pm.OSUHunger = Math.Max(0, pm.OSUHunger - 1);
                hcount = 0;
            }

            OSUNeedsGump.TryRefresh(pm);
            _hungerMinuteCounter[pm.Serial] = hcount;

            // ===== 3) Mensagens (40/20/10) sem spam =====
            HandleWarnings(pm);

            // ===== 4) Debuff somente a cada 20 min logado =====
            int dcount;
            if (!_debuffMinuteCounter.TryGetValue(pm.Serial, out dcount))
                dcount = 0;

            dcount++;
            bool timeToDebuff = (dcount >= Math.Max(1, DebuffIntervalMinutes));

            if (timeToDebuff)
            {
                dcount = 0;

                // Debuff separado:
                // fome <10 => -5
                // sede <10 => -5
                // ambos => -10
                int step = 0;

                if (pm.OSUHunger < DebuffStartsAt)
                    step += DebuffPerInterval;

                if (pm.OSUThirst < DebuffStartsAt)
                    step += DebuffPerInterval;

                if (step > 0)
                {
                    ApplyDebuffStep(pm, step);
                }
                else
                {
                    // voltou ao normal: remove penalidades (recomendado)
                    ClearDebuffs(pm);
                }
            }

            _debuffMinuteCounter[pm.Serial] = dcount;

            pm.InvalidateProperties();
        }
        public static int GetMinutesUntilCanEat(PlayerMobile pm, int fill)
        {
            if (pm == null || pm.Deleted || fill <= 0)
                return 0;

            int limit = 100 - fill;
            int deficit = pm.OSUHunger - limit;

            if (deficit <= 0)
                return 0;

            // fome cai 0,5/min -> 1 ponto a cada 2 minutos
            // minutos = deficit * 2
            return deficit * 2;
        }

        public static int GetMinutesUntilCanDrink(PlayerMobile pm, int fill)
        {
            if (pm == null || pm.Deleted || fill <= 0)
                return 0;

            int limit = 100 - fill;
            int deficit = pm.OSUThirst - limit;

            if (deficit <= 0)
                return 0;

            // sede cai 1/min
            return deficit;
        }

        private static void HandleWarnings(PlayerMobile pm)
        {
            // FOME
            int newHLevel = GetWarnLevel(pm.OSUHunger);
            int oldHLevel;
            if (!_hungerWarnLevel.TryGetValue(pm.Serial, out oldHLevel))
                oldHLevel = 0;

            if (newHLevel > oldHLevel)
            {
                // cruzou pra baixo (piorou)
                if (newHLevel == 1) pm.SendMessage("Você está com fome.");
                else if (newHLevel == 2) pm.SendMessage("Você está com muita fome.");
                else if (newHLevel == 3) pm.SendMessage("Você começa a sentir os efeitos da fome.");
            }

            _hungerWarnLevel[pm.Serial] = newHLevel;

            // SEDE
            int newTLevel = GetWarnLevel(pm.OSUThirst);
            int oldTLevel;
            if (!_thirstWarnLevel.TryGetValue(pm.Serial, out oldTLevel))
                oldTLevel = 0;

            if (newTLevel > oldTLevel)
            {
                if (newTLevel == 1) pm.SendMessage("Você está com sede.");
                else if (newTLevel == 2) pm.SendMessage("Você está com muita sede.");
                else if (newTLevel == 3) pm.SendMessage("Você começa a sentir os efeitos da sede.");
            }

            _thirstWarnLevel[pm.Serial] = newTLevel;
        }

        // 0=ok, 1=<=40, 2=<=20, 3=<=10
        private static int GetWarnLevel(int value)
        {
            if (value <= WarnDebuff) return 3;
            if (value <= WarnVeryHungry) return 2;
            if (value <= WarnHungry) return 1;
            return 0;
        }

        private static void ApplyDebuffStep(PlayerMobile pm, int step)
        {
            // step é positivo (5 ou 10). Vamos acumulando como negativo.
            pm.OSUNeedsStrPenalty = ClampPenalty(pm, pm.OSUNeedsStrPenalty - step, StatType.Str);
            pm.OSUNeedsDexPenalty = ClampPenalty(pm, pm.OSUNeedsDexPenalty - step, StatType.Dex);
            pm.OSUNeedsIntPenalty = ClampPenalty(pm, pm.OSUNeedsIntPenalty - step, StatType.Int);

            ReapplyStatMods(pm);
        }

        private static int ClampPenalty(PlayerMobile pm, int desiredPenalty, StatType st)
        {
            int current = GetCurrentStat(pm, st);
            int minAllowedPenalty = -(Math.Max(0, current - MinStat));

            if (desiredPenalty < minAllowedPenalty)
                return minAllowedPenalty;

            return desiredPenalty;
        }

        private static int GetCurrentStat(PlayerMobile pm, StatType st)
        {
            if (st == StatType.Str) return pm.Str;
            if (st == StatType.Dex) return pm.Dex;
            if (st == StatType.Int) return pm.Int;
            return MinStat;
        }

        private static void ReapplyStatMods(PlayerMobile pm)
        {
            pm.RemoveStatMod("OSUNeedsStr");
            pm.RemoveStatMod("OSUNeedsDex");
            pm.RemoveStatMod("OSUNeedsInt");

            if (pm.OSUNeedsStrPenalty != 0)
                pm.AddStatMod(new StatMod(Server.StatType.Str, "OSUNeedsStr", pm.OSUNeedsStrPenalty, TimeSpan.Zero));

            if (pm.OSUNeedsDexPenalty != 0)
                pm.AddStatMod(new StatMod(Server.StatType.Dex, "OSUNeedsDex", pm.OSUNeedsDexPenalty, TimeSpan.Zero));

            if (pm.OSUNeedsIntPenalty != 0)
                pm.AddStatMod(new StatMod(Server.StatType.Int, "OSUNeedsInt", pm.OSUNeedsIntPenalty, TimeSpan.Zero));
        }

        public static void ClearDebuffs(PlayerMobile pm)
        {
            pm.OSUNeedsStrPenalty = 0;
            pm.OSUNeedsDexPenalty = 0;
            pm.OSUNeedsIntPenalty = 0;

            pm.RemoveStatMod("OSUNeedsStr");
            pm.RemoveStatMod("OSUNeedsDex");
            pm.RemoveStatMod("OSUNeedsInt");
        }

        // ===== Chamados pelas comidas/bebidas =====

        public static bool TryAddHunger(PlayerMobile pm, int fill)
        {
            if (pm == null || pm.Deleted || fill <= 0)
                return false;

            if (pm.OSUHunger > 100 - fill)
            {
                int mins = GetMinutesUntilCanEat(pm, fill);
                pm.SendMessage($"Você ainda está cheio e precisa esperar {mins} minutos para ingerir essa comida.");
                return false;
            }

            pm.OSUHunger = Math.Min(100, pm.OSUHunger + fill);

            // se voltou ao normal, remove penalidades
            if (pm.OSUHunger >= DebuffStartsAt && pm.OSUThirst >= DebuffStartsAt)
                ClearDebuffs(pm);

            pm.InvalidateProperties();
            return true;
        }

        public static bool TryAddThirst(PlayerMobile pm, int fill)
        {
            if (pm == null || pm.Deleted || fill <= 0)
                return false;

            if (pm.OSUThirst > 100 - fill)
            {
                int mins = GetMinutesUntilCanDrink(pm, fill);
                pm.SendMessage($"Você ainda está cheio e precisa esperar {mins} minutos para ingerir essa bebida.");
                return false;
            }

            pm.OSUThirst = Math.Min(100, pm.OSUThirst + fill);

            if (pm.OSUHunger >= DebuffStartsAt && pm.OSUThirst >= DebuffStartsAt)
                ClearDebuffs(pm);

            pm.InvalidateProperties();
            return true;
        }

        private enum StatType
        {
            Str,
            Dex,
            Int
        }
    }
}
