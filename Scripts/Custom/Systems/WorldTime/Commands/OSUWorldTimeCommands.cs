using System;
using Server.Commands;

namespace Server.Custom.Systems.WorldTime.Commands
{
    public static class OSUWorldTimeCommands
    {
        public static void Initialize()
        {
            // Player
            CommandSystem.Register("tempo", AccessLevel.Player, OnTime);
            CommandSystem.Register("estacao", AccessLevel.Player, OnSeason);

            // GM
            CommandSystem.Register("timeset", AccessLevel.GameMaster, OnTimeSet);
            CommandSystem.Register("timeadd", AccessLevel.GameMaster, OnTimeAdd);
            CommandSystem.Register("timepause", AccessLevel.GameMaster, OnTimePause);
            CommandSystem.Register("seasoncfg", AccessLevel.GameMaster, OnSeasonCfg);
        }

        private static void OnTime(CommandEventArgs e)
        {
            var now = OSUWorldTime.WorldNow;
            e.Mobile.SendMessage($"Tempo do mundo: {now:dd/MM/yyyy HH:mm}");
        }

        private static void OnSeason(CommandEventArgs e)
        {
            var s = OSUWorldTime.GetSeason();
            e.Mobile.SendMessage($"Estação atual: {s}");
        }

        // [timeset 1500-10-10 08:00
        private static void OnTimeSet(CommandEventArgs e)
        {
            if (e.Arguments.Length < 2)
            {
                e.Mobile.SendMessage("Uso: [timeset AAAA-MM-DD HH:MM");
                return;
            }

            if (!DateTime.TryParse($"{e.Arguments[0]} {e.Arguments[1]}", out var dt))
            {
                e.Mobile.SendMessage("Formato inválido. Ex: [timeset 1500-10-10 08:00");
                return;
            }

            OSUWorldTime.SetWorldNow(dt);
            OSUPropertyRefresh.RefreshAllPlayersBackpacks();
            e.Mobile.SendMessage($"WorldNow setado para {dt:dd/MM/yyyy HH:mm}");
        }

        // [timeadd 3y | 10d | 6h | 30m
        private static void OnTimeAdd(CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [timeadd 3y | 10d | 6h | 30m | 45s");
                return;
            }

            string arg = e.Arguments[0].Trim().ToLowerInvariant();
            if (arg.Length < 2)
            {
                e.Mobile.SendMessage("Uso: [timeadd 3y | 10d | 6h | 30m | 45s");
                return;
            }

            char unit = arg[arg.Length - 1];
            if (!double.TryParse(arg.Substring(0, arg.Length - 1), out var val))
            {
                e.Mobile.SendMessage("Valor inválido.");
                return;
            }

            TimeSpan delta;

            switch (unit)
            {
                case 'y':
                    // Adds full calendar years in world time
                    var now = OSUWorldTime.WorldNow;
                    OSUWorldTime.SetWorldNow(now.AddYears((int)val));
                    OSUPropertyRefresh.RefreshAllPlayersBackpacks();
                    e.Mobile.SendMessage($"Avançou {(int)val} ano(s). Agora: {OSUWorldTime.WorldNow:dd/MM/yyyy HH:mm}");
                    return;

                case 'd': delta = TimeSpan.FromDays(val); break;
                case 'h': delta = TimeSpan.FromHours(val); break;
                case 'm': delta = TimeSpan.FromMinutes(val); break;
                case 's': delta = TimeSpan.FromSeconds(val); break;

                default:
                    e.Mobile.SendMessage("Unidade inválida. Use y/d/h/m/s.");
                    return;
            }

            OSUWorldTime.AddWorldTime(delta);
            OSUPropertyRefresh.RefreshAllPlayersBackpacks();
            e.Mobile.SendMessage($"Avançou {delta}. Agora: {OSUWorldTime.WorldNow:dd/MM/yyyy HH:mm}");
        }

        private static void OnTimePause(CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [timepause on|off");
                return;
            }

            string a = e.Arguments[0].Trim().ToLowerInvariant();
            bool paused = a == "on" || a == "true" || a == "1";

            OSUWorldTime.SetPaused(paused);
            e.Mobile.SendMessage(paused ? "Tempo do mundo PAUSADO." : "Tempo do mundo retomado.");
        }

        // [seasoncfg 10 20 15 15  (dias reais por estação dentro do ano real de 60 dias)
        private static void OnSeasonCfg(CommandEventArgs e)
        {
            if (e.Arguments.Length < 4)
            {
                e.Mobile.SendMessage("Uso: [seasoncfg <primavera> <verao> <outono> <inverno>  (dias reais; ano=60)");
                return;
            }

            if (!int.TryParse(e.Arguments[0], out var sp) ||
                !int.TryParse(e.Arguments[1], out var su) ||
                !int.TryParse(e.Arguments[2], out var au) ||
                !int.TryParse(e.Arguments[3], out var wi))
            {
                e.Mobile.SendMessage("Valores inválidos.");
                return;
            }

            OSUWorldTime.SpringRealDays = Math.Max(0, sp);
            OSUWorldTime.SummerRealDays = Math.Max(0, su);
            OSUWorldTime.AutumnRealDays = Math.Max(0, au);
            OSUWorldTime.WinterRealDays = Math.Max(0, wi);

            int total = OSUWorldTime.SpringRealDays + OSUWorldTime.SummerRealDays + OSUWorldTime.AutumnRealDays + OSUWorldTime.WinterRealDays;

            if (total != (int)OSUWorldTime.RealDaysPerWorldYear)
                e.Mobile.SendMessage($"Aviso: soma {total} != 60. O sistema normaliza automaticamente.");

            e.Mobile.SendMessage($"Config estações (dias reais): P={OSUWorldTime.SpringRealDays} V={OSUWorldTime.SummerRealDays} O={OSUWorldTime.AutumnRealDays} I={OSUWorldTime.WinterRealDays}");
        }
    }
}
