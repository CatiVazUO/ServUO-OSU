using Server;
using Server.Custom.Systems.Crafting.Tailoring;
using Server.Custom.Systems.WorldTime;
using Server.Mobiles;
using System;
using Server.Custom.Systems.DefQual;

namespace Server.Custom.Systems.Climate
{
    // Penalidades climáticas baseadas no "desconforto térmico" do personagem.
    // Importante: NÃO drena stamina. Apenas:
    // 1) deixa o regen de stamina mais lento (via RegenRates.cs)
    // 2) paralyze por frio (a cada 30s, baseado no frio sentido)
    // 3) sede extra por calor (o NeedsSystem chama GetExtraThirstPerMinute)
    // 4) dano por extremos (frio/calor >= 10 de desconforto)
    public static class OSUClimatePenaltySystem
    {
        // ====== AJUSTES FÁCEIS ======
        public static TimeSpan TickInterval = TimeSpan.FromSeconds(20.0);

        // A partir de quanto de desconforto começa a paralisar / acelerar sede
        public const int DiscomfortStart = 4;

        // Frio: 4 => 50% ; 16 => 85%
        public const int ColdChanceMaxAt = 16;
        public const double ColdChanceAtStart = 0.50;
        public const double ColdChanceAtMax = 0.85;

        // Calor: 4 => 50% ; 16 => 85%
        public const int HeatChanceMaxAt = 16;
        public const double HeatChanceAtStart = 0.50;
        public const double HeatChanceAtMax = 0.85;

        // Dano por extremos (quando desconforto >= 5)
        // Regras novas:
        // - Entrou numa nova região extrema? Primeiro tick só AVISA.
        // - A partir do segundo tick (20s), começa a aplicar dano.
        public const int ExtremeThreshold = 5;

        // Estado por player (para controlar "1º tick avisa, 2º tick dá dano")
        private class ClimateTickState
        {
            public string LastRegionKey;
            public bool PendingExtremeDamage;
        }

        private static readonly System.Collections.Generic.Dictionary<Serial, ClimateTickState> _states
            = new System.Collections.Generic.Dictionary<Serial, ClimateTickState>();

        private static Timer _timer;

        public static void Initialize()
        {
            // roda enquanto o server está ligado; só afeta players logados
            _timer = Timer.DelayCall(TimeSpan.FromSeconds(10.0), TickInterval, Tick);
        }

        private static void Tick()
        {
            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null || pm.Deleted || pm.NetState == null || !pm.NetState.Running)
                    continue;

                HandleRegionEnterWarnings(pm);
                ApplyExtremeDamage(pm);
                ApplyColdParalyze(pm);
            }
        }

        private static ClimateTickState GetState(PlayerMobile pm)
        {
            ClimateTickState st;

            if (!_states.TryGetValue(pm.Serial, out st) || st == null)
            {
                st = new ClimateTickState();
                _states[pm.Serial] = st;
            }

            return st;
        }

        private static void HandleRegionEnterWarnings(PlayerMobile pm)
        {
            // Descobre a região "atual" (pode vir do sistema novo OU do antigo)
            string regionKey;
            int effectiveTemp;
            GetEffectiveTemperatureAndRegion(pm, out effectiveTemp, out regionKey);

            ClimateTickState st = GetState(pm);

            // Se mudou de região (ou é a primeira vez), registra e decide se precisa avisar
            if (!string.Equals(st.LastRegionKey, regionKey, StringComparison.OrdinalIgnoreCase))
            {
                st.LastRegionKey = regionKey;

                // Se estiver em extremo, 1º tick avisa e "arma" a espera do dano
                int comfort = GetThermalComfortRaw(pm);
                int cold = comfort < 0 ? -comfort : 0;
                int heat = comfort > 0 ? comfort : 0;
                int d = Math.Max(cold, heat);

                if (d >= ExtremeThreshold)
                {
                    st.PendingExtremeDamage = true;

                    if (cold >= heat)
                        pm.SendMessage("Você está com muito frio! Tente se agasalhar ou sair desta área.");
                    else
                        pm.SendMessage("Você está com muito calor! Tente se hidratar ou sair desta área.");
                }
                else
                {
                    st.PendingExtremeDamage = false;
                }
            }
        }

        // ============================================================
        //  REGEN DE STAMINA (o RegenRates.cs usa isso como multiplicador)
        // ============================================================
        public static double GetStamRegenMultiplier(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return 1.0;

            int cold = GetColdDiscomfort(pm);
            int heat = GetHeatDiscomfort(pm);

            int d = Math.Max(cold, heat);

            // muito perceptível:
            // 0..2 normal
            if (d <= 2) return 1.0;

            // 3..4 levemente mais lento
            if (d <= 4) return 0.50;

            // 5..6 lento
            if (d <= 6) return 0.30;

            // 7..8 bem lento
            if (d <= 8) return 0.10;

            // 9..10 quase nada (ex: frio -10)
            if (d <= 10) return 0.05;

            // 11..12 praticamente zero
            if (d <= 12) return 0.03;

            // 13+ "quase nunca"
            return 0.01;
        }

        // ============================================================
        //  FRIO: PARALYZE (baseado no frio sentido)
        // ============================================================
        private static void ApplyColdParalyze(PlayerMobile pm)
        {
            int cold = GetColdDiscomfort(pm);

            if (cold < DiscomfortStart)
                return;

            double chance = GetLinearChance(cold, DiscomfortStart, ColdChanceMaxAt, ColdChanceAtStart, ColdChanceAtMax);

            if (Utility.RandomDouble() > chance)
                return;

            // se já está travado por qualquer motivo, não reaplica
            if (pm.Frozen || pm.Paralyzed)
                return;

            int seconds = Utility.RandomMinMax(1, 5);

            // Em vez de pm.Paralyze(...) (que pode ter feedback/efeitos),
            // aplica "travado" diretamente.
            pm.Frozen = true;
            pm.SendMessage("O frio intenso trava seus músculos!");

            // Garantia extra: se algum outro trecho tiver pintando o player,
            // isso limpa o hue mod/override (não muda a cor base do personagem).
            pm.HueMod = -1;
            pm.SolidHueOverride = -1;

            Timer.DelayCall(TimeSpan.FromSeconds(seconds), () =>
            {
                if (pm != null && !pm.Deleted)
                    pm.Frozen = false;
            });
        }

        // ============================================================
        //  CALOR: SEDE MAIS RÁPIDA (o NeedsSystem chama 1x por minuto)
        //  - retorno 0 = normal
        //  - retorno 1 = perde +1 sede (total 2x)
        //  - retorno 2 = perde +2 sede (total 3x)
        // ============================================================
        public static int GetExtraThirstPerMinute(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return 0;

            int heat = GetHeatDiscomfort(pm);

            if (heat < DiscomfortStart)
                return 0;

            double chance = GetLinearChance(heat, DiscomfortStart, HeatChanceMaxAt, HeatChanceAtStart, HeatChanceAtMax);

            if (Utility.RandomDouble() > chance)
                return 0;

            // calor muito alto = triplo (extra 2)
            if (heat >= 12)
                return 2;

            // calor médio = dobro (extra 1)
            return 1;
        }

        // ============================================================
        //  EXTREMOS: DANO (frio/calor >= 10 de desconforto)
        //  roda a cada 30 segundos, então o dano fica bem "notável"
        // ============================================================
        private static void ApplyExtremeDamage(PlayerMobile pm)
        {
            int cold = GetColdDiscomfort(pm);
            int heat = GetHeatDiscomfort(pm);

            ClimateTickState st = GetState(pm);

            int d = Math.Max(cold, heat);

            // não está extremo
            if (d < ExtremeThreshold)
            {
                st.PendingExtremeDamage = false;
                return;
            }

            // 1º tick após entrar em região extrema: só avisa (não dá dano)
            if (st.PendingExtremeDamage)
            {
                st.PendingExtremeDamage = false;
                return;
            }

            // Dano cresce de 5 -> 1 dano, até 16 -> 20 dano (a cada 20s)
            int dmg = GetExtremeDamage(d);

            if (cold >= heat)
            {
                double scalar = OSUDefQualDispatcher.ModifyColdSusceptibility(pm, 1.0);
                dmg = (int)Math.Round(dmg * scalar);
                dmg = OSUDefQualDispatcher.ModifyColdClimateDamage(pm, dmg);
            }
            else
            {
                double scalar = OSUDefQualDispatcher.ModifyHeatSusceptibility(pm, 1.0);
                dmg = (int)Math.Round(dmg * scalar);
                dmg = OSUDefQualDispatcher.ModifyHeatClimateDamage(pm, dmg);
            }

            if (dmg < 0)
                dmg = 0;

            pm.Damage(dmg);

            if (cold >= heat)
                pm.SendMessage("O frio extremo fere seu corpo!");
            else
                pm.SendMessage("O calor extremo queima seu corpo!");
        }

        // ============================================================
        //  DESCONFORTO TÉRMICO (placeholder para roupas)
        //  Hoje: desconforto = temperatura efetiva (frio/ calor)
        //  Depois: você implementa GetWarmthFromClothes / GetCoolingFromClothes
        // ============================================================
        private static int GetThermalComfortRaw(PlayerMobile pm)
        {
            // Temperatura efetiva do lugar (já com estação)
            // IMPORTANTE: agora lê tanto regiões do sistema novo (OSUClimateRegions)
            // quanto as regiões antigas (OSUClimate._regions).
            int temp;
            string regionKey;
            GetEffectiveTemperatureAndRegion(pm, out temp, out regionKey);

            // Bônus das roupas (peles/lã sobem; seda/linho descem)
            int bonus = OSUFabrics.GetTotalThermalBonus(pm);

            int comfort = temp + bonus;

            // limita na faixa -16..+16
            if (comfort < -16) comfort = -16;
            if (comfort > 16) comfort = 16;

            return comfort;
        }

        // Retorna a temperatura efetiva SEMPRE (considerando estação), tentando:
        // 1) Sistema novo: OSUClimateRegions (comandos [clime...)
        // 2) Sistema antigo: OSUClimate._regions (regiões pré-setadas antigas)
        private static void GetEffectiveTemperatureAndRegion(PlayerMobile pm, out int effectiveTemp, out string regionKey)
        {
            regionKey = "(sem região)";
            effectiveTemp = 0;

            if (pm == null || pm.Deleted || pm.Map == null)
                return;

            WorldTime.OSUSeason season = Server.Custom.Systems.Climate.OSUClimateTimeAdapter.SeasonNow;
            int mapId = pm.Map.MapID;

            // 1) Sistema novo
            var rNew = Server.Custom.Systems.Climate.OSUClimateRegions.FindAt(pm.X, pm.Y, mapId);
            if (rNew != null)
            {
                regionKey = "NEW:" + rNew.Name;

                int baseTemp = rNew.BaseTemperature;

                if (rNew.IsStatic)
                    effectiveTemp = Clamp(baseTemp, OSUClimateMath.MinTemp, OSUClimateMath.MaxTemp);
                else
                    effectiveTemp = OSUClimateMath.ApplySeasonToBase(baseTemp, season);

                return;
            }

            // 2) Sistema antigo
            var rOld = Server.Custom.Systems.WorldTime.OSUClimate.GetRegionAt(pm.Map, pm.Location);
            if (rOld != null)
            {
                regionKey = "OLD:" + rOld.Name;

                int baseTemp = rOld.BaseTemp;

                if (rOld.StaticClimate)
                    effectiveTemp = Clamp(baseTemp, OSUClimateMath.MinTemp, OSUClimateMath.MaxTemp);
                else
                    effectiveTemp = OSUClimateMath.ApplySeasonToBase(baseTemp, season);

                return;
            }

            // Sem região em nenhum sistema
            regionKey = "NONE";
            effectiveTemp = 0;
        }

        private static int GetExtremeDamage(int discomfort)
        {
            // 5 => 1 dano; 16 => 20 dano
            if (discomfort <= ExtremeThreshold)
                return 3;

            if (discomfort >= 16)
                return 20;

            // linear: 1 + (d-5) * 19/11
            int num = (discomfort - ExtremeThreshold) * 19;
            int add = num / 11;
            return 3 + add;
        }

        private static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static int GetColdDiscomfort(PlayerMobile pm)
        {
            int comfort = GetThermalComfortRaw(pm);

            // negativo = frio
            if (comfort < 0)
                return -comfort;

            return 0;
        }

        private static int GetHeatDiscomfort(PlayerMobile pm)
        {
            int comfort = GetThermalComfortRaw(pm);

            // positivo = calor
            if (comfort > 0)
                return comfort;

            return 0;
        }

        private static double GetLinearChance(int level, int startLevel, int maxLevel, double startChance, double maxChance)
        {
            if (level <= startLevel)
                return startChance;

            if (level >= maxLevel)
                return maxChance;

            int span = maxLevel - startLevel;
            double t = (double)(level - startLevel) / (double)span;
            return startChance + (maxChance - startChance) * t;
        }

        // Retorna conforto térmico em escala -16..+16
        // negativo = frio sentido, positivo = calor sentido, 0 = confortável
        public static int GetThermalComfort(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return 0;

            // Usa o mesmo cálculo interno do sistema (já considerando roupas quando existirem)
            int cold = GetColdDiscomfort(pm);
            if (cold > 0)
                return -Math.Min(16, cold);

            int heat = GetHeatDiscomfort(pm);
            if (heat > 0)
                return Math.Min(16, heat);

            return 0;
        }
    }
}
