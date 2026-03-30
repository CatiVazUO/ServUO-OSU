using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP;



namespace Server.Custom.Systems.SkillXP.Engine
{
    public static class OSUFeatRegistry
    {
        private static readonly Dictionary<int, IOSUFeat> _ById = new Dictionary<int, IOSUFeat>();
        private static readonly Dictionary<string, IOSUFeat> _ByCommand = new Dictionary<string, IOSUFeat>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            // Register combat feats (Systems/Feats/Combate)
            Register(new Custom.Systems.Skills.Combate.DisarmeFeat());

            // Register profession feats (Systems/Feats/Profissoes)
            Register(new Custom.Systems.Skills.Profissoes.MiningProficiencyFeat());
        }

        public static IOSUFeat GetById(int id)
        {
            IOSUFeat feat;
            return _ById.TryGetValue(id, out feat) ? feat : null;
        }

        public static IOSUFeat GetByCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return null;

            IOSUFeat feat;
            return _ByCommand.TryGetValue(command, out feat) ? feat : null;
        }

        public static void Register(IOSUFeat feat)
        {
            if (feat == null || feat.Definition == null)
                return;

            OSUFeatDefinition def = feat.Definition;

            // Evita duplicidade por Id
            if (_ById.ContainsKey(def.Id))
                return;

            _ById[def.Id] = feat;

            // Alimenta o catálogo usado pelos gumps
            OSUFeatSystem.AddFeat(def);

            // Registra comando (se houver)
            if (!string.IsNullOrEmpty(def.CommandName))
            {
                if (!_ByCommand.ContainsKey(def.CommandName))
                {
                    _ByCommand[def.CommandName] = feat;
                    CommandSystem.Register(def.CommandName, AccessLevel.Player, new CommandEventHandler(OnFeatCommand));
                }
            }
        }

        private static void OnFeatCommand(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            // Em ServUO/RunUO normalmente existe e.Command com o nome do comando digitado
            string cmd = e.Command;

            IOSUFeat feat = GetByCommand(cmd);
            if (feat == null || feat.Definition == null)
            {
                pm.SendMessage(0x22, "Comando desconhecido.");
                return;
            }

            if (!pm.HasOSUFeat(feat.Definition.Id))
            {
                pm.SendMessage(0x22, "Você não possui a especialização " + feat.Definition.Name + ".");
                return;
            }

            try
            {
                feat.OnCommand(pm, e);
            }
            catch (Exception ex)
            {
                pm.SendMessage(0x22, "Erro ao executar o feat.");
                Console.WriteLine(ex);
            }
        }
    }
}
