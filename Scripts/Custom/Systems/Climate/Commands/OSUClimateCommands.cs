using System;
using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Custom.Systems.Climate.Commands
{
    public static class OSUClimateCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("climeadd", AccessLevel.GameMaster, OnAdd);
            CommandSystem.Register("climeaddrect", AccessLevel.GameMaster, OnAddRect);
            CommandSystem.Register("climelist", AccessLevel.GameMaster, OnList);
            CommandSystem.Register("climeremove", AccessLevel.GameMaster, OnRemove);
            CommandSystem.Register("climehere", AccessLevel.GameMaster, OnHere);
            CommandSystem.Register("climeclear", AccessLevel.Administrator, OnClear);
            CommandSystem.Register("termico", AccessLevel.Player, OnTermico);
        }

        // Método 1: clicar 2 pontos (mantém o que você gosta)
        // Uso: [climeadd Nome TempBase Static(0/1)
        private static void OnAdd(CommandEventArgs e)
        {
            if (e.Length < 3)
            {
                e.Mobile.SendMessage("Uso: [climeadd <Nome> <TempBase> <Static 0/1>");
                return;
            }

            string name = e.GetString(0);
            int baseTemp = e.GetInt32(1);
            bool isStatic = (e.GetInt32(2) != 0);

            e.Mobile.SendMessage("Clique o 1º canto do retângulo.");
            e.Mobile.Target = new RectTarget(name, baseTemp, isStatic, true);
        }

        // Método 2: por coordenadas (para você preencher regiões grandes sem clicar)
        // Uso: [climeaddrect Nome TempBase Static(0/1) x1 y1 x2 y2
        private static void OnAddRect(CommandEventArgs e)
        {
            if (e.Length < 7)
            {
                e.Mobile.SendMessage("Uso: [climeaddrect <Nome> <TempBase> <Static 0/1> <x1> <y1> <x2> <y2>");
                return;
            }

            string name = e.GetString(0);
            int baseTemp = e.GetInt32(1);
            bool isStatic = (e.GetInt32(2) != 0);

            int x1 = e.GetInt32(3);
            int y1 = e.GetInt32(4);
            int x2 = e.GetInt32(5);
            int y2 = e.GetInt32(6);

            int mapIndex = e.Mobile.Map != null ? e.Mobile.Map.MapID : 0;

            var region = new Server.Custom.Systems.Climate.OSUClimateRegion(name, baseTemp, isStatic, mapIndex, x1, y1, x2, y2);

            string err;
            if (!Server.Custom.Systems.Climate.OSUClimateRegions.TryAddRegion(region, out err))
            {
                e.Mobile.SendMessage(0x22, "ERRO: " + err);
                return;
            }

            e.Mobile.SendMessage(0x3F, "Região criada: " + region.ToString());
        }

        private static void OnList(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Regiões: " + Server.Custom.Systems.Climate.OSUClimateRegions.Regions.Count);

            for (int i = 0; i < Server.Custom.Systems.Climate.OSUClimateRegions.Regions.Count; i++)
            {
                e.Mobile.SendMessage(Server.Custom.Systems.Climate.OSUClimateRegions.Regions[i].ToString());
            }
        }

        private static void OnRemove(CommandEventArgs e)
        {
            if (e.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [climeremove <Nome>");
                return;
            }

            string name = e.GetString(0);

            if (Server.Custom.Systems.Climate.OSUClimateRegions.RemoveByName(name))
                e.Mobile.SendMessage("Região removida: " + name);
            else
                e.Mobile.SendMessage("Não achei a região: " + name);
        }

        private static void OnClear(CommandEventArgs e)
        {
            Server.Custom.Systems.Climate.OSUClimateRegions.Clear();
            e.Mobile.SendMessage("Todas as regiões foram apagadas.");
        }

        private static void OnHere(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            int mapIndex = pm.Map != null ? pm.Map.MapID : 0;
            var loc = pm.Location;

            Server.Custom.Systems.Climate.OSUClimateRegion region;
            Server.Custom.Systems.WorldTime.OSUSeason season;

            int temp = Server.Custom.Systems.Climate.OSUClimateService.GetEffectiveTemperatureAt(loc.X, loc.Y, mapIndex, out region, out season);

            bool night = Server.Custom.Systems.Climate.OSUClimateService.IsNightNow();

            string regionName;

            if (region != null)
            {
                regionName = "NEW:" + region.Name;
            }
            else
            {
                // fallback: regiões antigas (OSUClimate._regions)
                var old = Server.Custom.Systems.WorldTime.OSUClimate.GetRegionAt(pm.Map, pm.Location);
                regionName = (old != null) ? ("OLD:" + old.Name) : "(sem região)";
            }

            pm.SendMessage("Estação: " + season + " | Noite: " + (night ? "sim" : "não"));
            pm.SendMessage("Região: " + regionName);
            pm.SendMessage("Temp base: " + ((region != null) ? region.BaseTemperature.ToString() : "0") + " | Temp efetiva: " + temp);
        }

        private static void OnTermico(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            int comfort = Server.Custom.Systems.Climate.OSUClimatePenaltySystem.GetThermalComfort(pm);

            if (comfort == 0)
                pm.SendMessage("Conforto térmico: 0 (confortável).");
            else if (comfort < 0)
                pm.SendMessage($"Conforto térmico: {comfort} (frio).");
            else
                pm.SendMessage($"Conforto térmico: +{comfort} (calor).");
        }

        // Para shards com mapas custom:
        // usamos o MapID do próprio ServUO (funciona para mapas oficiais e custom).

        private class RectTarget : Target
        {
            private readonly string _name;
            private readonly int _baseTemp;
            private readonly bool _isStatic;
            private readonly bool _first;

            private readonly int _x1;
            private readonly int _y1;

            public RectTarget(string name, int baseTemp, bool isStatic, bool first, int x1 = 0, int y1 = 0)
                : base(18, true, TargetFlags.None)
            {
                _name = name;
                _baseTemp = baseTemp;
                _isStatic = isStatic;
                _first = first;
                _x1 = x1;
                _y1 = y1;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                if (p == null)
                {
                    from.SendMessage("Clique no chão.");
                    return;
                }

                int mapIndex = from.Map != null ? from.Map.MapID : 0;

                if (_first)
                {
                    from.SendMessage("Agora clique o 2º canto do retângulo.");
                    from.Target = new RectTarget(_name, _baseTemp, _isStatic, false, p.X, p.Y);
                    return;
                }

                var region = new Server.Custom.Systems.Climate.OSUClimateRegion(_name, _baseTemp, _isStatic, mapIndex, _x1, _y1, p.X, p.Y);

                string err;
                if (!Server.Custom.Systems.Climate.OSUClimateRegions.TryAddRegion(region, out err))
                {
                    from.SendMessage(0x22, "ERRO: " + err);
                    return;
                }

                from.SendMessage(0x3F, "Região criada: " + region.ToString());
            }

        }
    }
}
