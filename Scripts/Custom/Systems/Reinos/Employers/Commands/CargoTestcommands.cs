using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    /*
     * COMANDOS ÚTEIS PARA TESTAR O REINO
     *
     * Fluxo mais útil para você testar rápido:
     *
     * 1) [ReinoDefinirPovo kamay
     * 2) [ReinoDefinirCidadania Aurora
     * 3) [ReinoVirarLider 0
     * 4) [ReinoCargoInfo 0
     * 5) [ReinoCargoForcar 0 <roleId> NomeDoJogador
     *
     * Lista de comandos deste arquivo:
     *
     * [ReinoDefinirPovo <kamay|matalun|sarangs|zosteros>
     * [ReinoDefinirCidadania <Aurora|Xetá|Lurone|Willran>
     * [ReinoVirarLider <cityId>
     * [ReinoDefinirLider <cityId> <nome do jogador>
     * [ReinoDarChave <cityId>
     * [ReinoCargoInfo <cityId>
     * [ReinoCargoForcar <cityId> <roleId> <nome do jogador>
     * [ReinoCargoLimpar <cityId> <roleId>
     * [ReinoCargosLimparTudo <cityId>
     * [ReinoCargoSalvar
     *
     * Observação importante:
     * Este arquivo procura automaticamente tanto ReinoGovernmentSystem
     * quanto ReinoCargosSystem. Então ele continua funcionando mesmo se você
     * renomear a classe principal do sistema de cargos.
     */
    public static class ReinoCargosTestCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReinoDefinirPovo", AccessLevel.GameMaster, OnSetCulture);
            CommandSystem.Register("ReinoDefinirCidadania", AccessLevel.GameMaster, OnSetCitizenship);
            CommandSystem.Register("ReinoVirarLider", AccessLevel.GameMaster, OnBecomeLeader);
            CommandSystem.Register("ReinoDefinirLider", AccessLevel.GameMaster, OnSetLeaderByName);
            CommandSystem.Register("ReinoDarChave", AccessLevel.GameMaster, OnGiveGovernorKey);
            CommandSystem.Register("ReinoCargoInfo", AccessLevel.GameMaster, OnCargoInfo);
            CommandSystem.Register("ReinoCargoForcar", AccessLevel.GameMaster, OnForceRole);
            CommandSystem.Register("ReinoCargoLimpar", AccessLevel.GameMaster, OnClearRole);
            CommandSystem.Register("ReinoCargosLimparTudo", AccessLevel.GameMaster, OnClearAllRoles);
            CommandSystem.Register("ReinoCargoSalvar", AccessLevel.GameMaster, OnSaveCargoSystem);
        }

        private static void OnSetCulture(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (e.Length != 1)
            {
                pm.SendMessage("Uso: [ReinoDefinirPovo <kamay|matalun|sarangs|zosteros>");
                return;
            }

            string culture = NormalizeCultureId(e.GetString(0));

            if (String.IsNullOrWhiteSpace(culture))
            {
                pm.SendMessage("Povo inválido. Use kamay, matalun, sarangs ou zosteros.");
                return;
            }

            pm.OSUCultureId = culture;
            pm.SendMessage("Seu povo foi definido como: {0}", culture);
        }

        private static void OnSetCitizenship(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (e.Length < 1)
            {
                pm.SendMessage("Uso: [ReinoDefinirCidadania <Aurora|Xetá|Lurone|Willran>");
                return;
            }

            string cityName = NormalizeCityName(JoinArguments(e, 0));
            int cityId = GetCityIdByName(cityName);

            if (cityId < 0)
            {
                pm.SendMessage("Cidade inválida. Use Aurora, Xetá, Lurone ou Willran.");
                return;
            }

            pm.OSUCitizenCityId = ReinoElectionsSystem.GetCityName(cityId);
            pm.SendMessage("Sua cidadania foi definida como: {0}", pm.OSUCitizenCityId);
        }

        private static void OnBecomeLeader(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (e.Length != 1)
            {
                pm.SendMessage("Uso: [ReinoVirarLider <cityId>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                pm.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(pm, cityId))
            {
                pm.SendMessage("Seu povo atual não pode governar {0}.", ReinoElectionsSystem.GetCityName(cityId));
                pm.SendMessage("Defina primeiro o povo correto com [ReinoDefinirPovo.");
                return;
            }

            ReinoElectionsSystem.SetGovernor(cityId, pm);
            ReinoElectionsSystem.Save();
            SaveCargoSystem();

            pm.SendMessage("Agora você é o líder de {0} e recebeu a chave do governador.", ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnSetLeaderByName(CommandEventArgs e)
        {
            PlayerMobile from = e.Mobile as PlayerMobile;

            if (from == null)
                return;

            if (e.Length < 2)
            {
                from.SendMessage("Uso: [ReinoDefinirLider <cityId> <nome do jogador>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                from.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            string name = JoinArguments(e, 1);
            PlayerMobile target = FindPlayerByName(name);

            if (target == null)
            {
                from.SendMessage("Jogador não encontrado online: {0}", name);
                return;
            }

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(target, cityId))
            {
                from.SendMessage("Esse jogador não pertence ao povo que pode governar {0}.", ReinoElectionsSystem.GetCityName(cityId));
                return;
            }

            ReinoElectionsSystem.SetGovernor(cityId, target);
            ReinoElectionsSystem.Save();
            SaveCargoSystem();

            from.SendMessage("{0} agora é o líder de {1}.", target.Name, ReinoElectionsSystem.GetCityName(cityId));

            if (target != from)
                target.SendMessage("Você agora é o líder de {0}.", ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnGiveGovernorKey(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (e.Length != 1)
            {
                pm.SendMessage("Uso: [ReinoDarChave <cityId>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                pm.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            ReinoAccessHelper.GrantGovernorAccess(pm, cityId, true);
            pm.SendMessage("Chave do governador de {0} entregue.", ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnCargoInfo(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (e.Length != 1)
            {
                pm.SendMessage("Uso: [ReinoCargoInfo <cityId>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                pm.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            IList roles = GetRoles(cityId);

            if (roles == null)
            {
                pm.SendMessage("Não consegui encontrar a classe principal do sistema de cargos.");
                pm.SendMessage("Se você renomeou a classe, me mande o nome final que eu ajusto o arquivo.");
                return;
            }

            pm.SendMessage("=== Cargos de {0} ===", ReinoElectionsSystem.GetCityName(cityId));

            for (int i = 0; i < roles.Count; i++)
            {
                object role = roles[i];
                if (role == null)
                    continue;

                int roleId = GetIntMember(role, "RoleId");
                int hierarchy = GetIntMember(role, "Hierarchy");
                bool essential = GetBoolMember(role, "IsEssential");
                string title = GetStringMember(role, "Title");
                string occupant = GetStringMember(role, "OccupantName");
                string linkedKey = GetStringMember(role, "LinkedConstructionKey");

                if (String.IsNullOrWhiteSpace(occupant))
                    occupant = "vazio";

                string extra = essential ? " | essencial" : String.Empty;

                if (!String.IsNullOrWhiteSpace(linkedKey))
                    extra += " | vinculado=" + linkedKey;

                pm.SendMessage("RoleId={0} | H={1} | {2} | Ocupante={3}{4}", roleId, hierarchy, title, occupant, extra);
            }
        }

        private static void OnForceRole(CommandEventArgs e)
        {
            PlayerMobile from = e.Mobile as PlayerMobile;

            if (from == null)
                return;

            if (e.Length < 3)
            {
                from.SendMessage("Uso: [ReinoCargoForcar <cityId> <roleId> <nome do jogador>");
                return;
            }

            int cityId;
            int roleId;

            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                from.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            if (!Int32.TryParse(e.GetString(1), out roleId) || roleId <= 0)
            {
                from.SendMessage("RoleId inválido.");
                return;
            }

            string targetName = JoinArguments(e, 2);
            PlayerMobile target = FindPlayerByName(targetName);

            if (target == null)
            {
                from.SendMessage("Jogador não encontrado online: {0}", targetName);
                return;
            }

            object role = GetRole(cityId, roleId);

            if (role == null)
            {
                from.SendMessage("Cargo não encontrado.");
                return;
            }

            if (!ReinoElectionsSystem.IsPlayerAllowedForCity(target, cityId))
            {
                from.SendMessage("Esse jogador não pertence ao povo que pode governar {0}.", ReinoElectionsSystem.GetCityName(cityId));
                return;
            }

            bool isLeaderRole = GetBoolMember(role, "IsLeaderRole");

            if (isLeaderRole)
            {
                ReinoElectionsSystem.SetGovernor(cityId, target);
                ReinoElectionsSystem.Save();
                SaveCargoSystem();
                from.SendMessage("Liderança de {0} passada para {1}.", ReinoElectionsSystem.GetCityName(cityId), target.Name);
                return;
            }

            ClearPlayerFromAllCommissionedRoles(target);

            SetMember(role, "OccupantSerial", target.Serial.Value);
            SetMember(role, "OccupantName", target.Name);

            SyncRoleDependentState(cityId);
            SaveCargoSystem();

            from.SendMessage("{0} agora ocupa o cargo {1} em {2}.", target.Name, GetStringMember(role, "Title"), ReinoElectionsSystem.GetCityName(cityId));
            if (target != from)
                target.SendMessage("Você foi colocado no cargo {0} de {1}.", GetStringMember(role, "Title"), ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnClearRole(CommandEventArgs e)
        {
            PlayerMobile from = e.Mobile as PlayerMobile;

            if (from == null)
                return;

            if (e.Length != 2)
            {
                from.SendMessage("Uso: [ReinoCargoLimpar <cityId> <roleId>");
                return;
            }

            int cityId;
            int roleId;

            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                from.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            if (!Int32.TryParse(e.GetString(1), out roleId) || roleId <= 0)
            {
                from.SendMessage("RoleId inválido.");
                return;
            }

            object role = GetRole(cityId, roleId);

            if (role == null)
            {
                from.SendMessage("Cargo não encontrado.");
                return;
            }

            bool isLeaderRole = GetBoolMember(role, "IsLeaderRole");
            string title = GetStringMember(role, "Title");

            if (isLeaderRole)
            {
                ReinoElectionsSystem.SetGovernor(cityId, null);
                ReinoElectionsSystem.Save();
                SaveCargoSystem();
                from.SendMessage("A liderança de {0} foi limpa.", ReinoElectionsSystem.GetCityName(cityId));
                return;
            }

            SetMember(role, "OccupantSerial", 0);
            SetMember(role, "OccupantName", String.Empty);

            SyncRoleDependentState(cityId);
            SaveCargoSystem();

            from.SendMessage("Cargo limpo: {0}.", title);
        }

        private static void OnClearAllRoles(CommandEventArgs e)
        {
            PlayerMobile from = e.Mobile as PlayerMobile;

            if (from == null)
                return;

            if (e.Length != 1)
            {
                from.SendMessage("Uso: [ReinoCargosLimparTudo <cityId>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.GetString(0), out cityId) || !IsValidCityId(cityId))
            {
                from.SendMessage("CityId inválido. Use 0, 1, 2 ou 3.");
                return;
            }

            IList roles = GetRoles(cityId);

            if (roles == null)
            {
                from.SendMessage("Não consegui encontrar a classe principal do sistema de cargos.");
                return;
            }

            int cleared = 0;

            for (int i = 0; i < roles.Count; i++)
            {
                object role = roles[i];
                if (role == null)
                    continue;

                if (GetBoolMember(role, "IsLeaderRole"))
                    continue;

                int serial = GetIntMember(role, "OccupantSerial");
                string occupant = GetStringMember(role, "OccupantName");

                if (serial <= 0 && String.IsNullOrWhiteSpace(occupant))
                    continue;

                SetMember(role, "OccupantSerial", 0);
                SetMember(role, "OccupantName", String.Empty);
                cleared++;
            }

            SyncRoleDependentState(cityId);
            SaveCargoSystem();

            from.SendMessage("{0} cargos comissionados foram limpos em {1}.", cleared, ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnSaveCargoSystem(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            SaveCargoSystem();

            if (pm != null)
                pm.SendMessage("Sistemas de reino salvos manualmente.");
        }

        private static string NormalizeCultureId(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;

            string v = value.Trim().ToLowerInvariant();

            switch (v)
            {
                case "kamay": return "kamay";
                case "matalun": return "matalun";
                case "sarang":
                case "sarangs": return "sarangs";
                case "zortero":
                case "zorteros":
                case "zosteros": return "zosteros";
                default: return String.Empty;
            }
        }

        private static string NormalizeCityName(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;

            string v = value.Trim().ToLowerInvariant();

            switch (v)
            {
                case "aurora": return "Aurora";
                case "xeta":
                case "xetá": return "Xetá";
                case "lurone": return "Lurone";
                case "willran": return "Willran";
                default: return value.Trim();
            }
        }

        private static int GetCityIdByName(string cityName)
        {
            if (String.IsNullOrWhiteSpace(cityName))
                return -1;

            for (int i = 0; i < ReinoElectionsSystem.CityNames.Length; i++)
            {
                string current = NormalizeCityName(ReinoElectionsSystem.CityNames[i]);
                if (String.Equals(current, NormalizeCityName(cityName), StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private static bool IsValidCityId(int cityId)
        {
            return cityId >= 0 && cityId < ReinoElectionsSystem.CityNames.Length;
        }

        private static string JoinArguments(CommandEventArgs e, int startIndex)
        {
            if (e == null || e.Arguments == null || startIndex < 0 || startIndex >= e.Arguments.Length)
                return String.Empty;

            List<string> parts = new List<string>();

            for (int i = startIndex; i < e.Arguments.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(e.Arguments[i]))
                    parts.Add(e.Arguments[i]);
            }

            return String.Join(" ", parts.ToArray());
        }

        private static PlayerMobile FindPlayerByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;

            name = name.Trim();

            foreach (Mobile m in World.Mobiles.Values)
            {
                PlayerMobile pm = m as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (String.Equals(pm.Name, name, StringComparison.OrdinalIgnoreCase))
                    return pm;
            }

            return null;
        }

        private static void ClearPlayerFromAllCommissionedRoles(PlayerMobile target)
        {
            if (target == null || target.Deleted)
                return;

            for (int cityId = 0; cityId < ReinoElectionsSystem.CityNames.Length; cityId++)
            {
                IList roles = GetRoles(cityId);
                if (roles == null)
                    continue;

                for (int i = 0; i < roles.Count; i++)
                {
                    object role = roles[i];
                    if (role == null)
                        continue;

                    if (GetBoolMember(role, "IsLeaderRole"))
                        continue;

                    if (GetIntMember(role, "OccupantSerial") != target.Serial.Value)
                        continue;

                    SetMember(role, "OccupantSerial", 0);
                    SetMember(role, "OccupantName", String.Empty);
                    SyncRoleDependentState(cityId);
                }
            }
        }

        private static Type GetCargoSystemType()
        {
            string[] candidates = new string[]
            {
                "Server.Custom.Systems.Reinos.ReinoCargosSystem",
                "Server.Custom.Systems.Reinos.ReinoCargoSystem",
                "Server.Custom.Systems.Reinos.ReinoGovernmentSystem"
            };

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int a = 0; a < assemblies.Length; a++)
            {
                Assembly asm = assemblies[a];

                for (int i = 0; i < candidates.Length; i++)
                {
                    Type t = asm.GetType(candidates[i], false, true);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }

        private static IList GetRoles(int cityId)
        {
            Type t = GetCargoSystemType();
            if (t == null)
                return null;

            MethodInfo mi = t.GetMethod("GetRoles", BindingFlags.Public | BindingFlags.Static);
            if (mi == null)
                return null;

            object result = mi.Invoke(null, new object[] { cityId });
            return result as IList;
        }

        private static object GetRole(int cityId, int roleId)
        {
            Type t = GetCargoSystemType();
            if (t == null)
                return null;

            MethodInfo mi = t.GetMethod("GetRole", BindingFlags.Public | BindingFlags.Static);
            if (mi == null)
                return null;

            return mi.Invoke(null, new object[] { cityId, roleId });
        }

        private static void SyncRoleDependentState(int cityId)
        {
            Type t = GetCargoSystemType();
            if (t == null)
                return;

            MethodInfo mi = t.GetMethod("SyncRoleDependentState", BindingFlags.Public | BindingFlags.Static);
            if (mi != null)
                mi.Invoke(null, new object[] { cityId });
        }

        private static void SaveCargoSystem()
        {
            Type t = GetCargoSystemType();
            if (t == null)
                return;

            MethodInfo mi = t.GetMethod("Save", BindingFlags.Public | BindingFlags.Static);
            if (mi != null)
                mi.Invoke(null, null);

            ReinoElectionsSystem.Save();
        }

        private static int GetIntMember(object obj, string name)
        {
            object value = GetMemberValue(obj, name);
            if (value == null)
                return 0;

            if (value is int)
                return (int)value;

            int parsed;
            return Int32.TryParse(value.ToString(), out parsed) ? parsed : 0;
        }

        private static bool GetBoolMember(object obj, string name)
        {
            object value = GetMemberValue(obj, name);
            if (value == null)
                return false;

            if (value is bool)
                return (bool)value;

            bool parsed;
            return Boolean.TryParse(value.ToString(), out parsed) && parsed;
        }

        private static string GetStringMember(object obj, string name)
        {
            object value = GetMemberValue(obj, name);
            return value == null ? String.Empty : value.ToString();
        }

        private static object GetMemberValue(object obj, string name)
        {
            if (obj == null || String.IsNullOrWhiteSpace(name))
                return null;

            Type t = obj.GetType();

            PropertyInfo pi = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null)
                return pi.GetValue(obj, null);

            FieldInfo fi = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fi != null)
                return fi.GetValue(obj);

            return null;
        }

        private static void SetMember(object obj, string name, object value)
        {
            if (obj == null || String.IsNullOrWhiteSpace(name))
                return;

            Type t = obj.GetType();

            PropertyInfo pi = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null && pi.CanWrite)
            {
                pi.SetValue(obj, value, null);
                return;
            }

            FieldInfo fi = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fi != null)
                fi.SetValue(obj, value);
        }
    }
}
