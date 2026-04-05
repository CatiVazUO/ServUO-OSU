using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Items;

namespace Server.Custom.Systems.Reinos
{
    public static class ReinoExpansionDefinitions
    {
        private static readonly List<ReinoConstructionDefinition> m_Buildings = new List<ReinoConstructionDefinition>();
        private static bool m_Initialized;

        public static void EnsureInitialized()
        {
            if (m_Initialized)
                return;

            m_Initialized = true;
            m_Buildings.Clear();
            Register(CorreiosAuroraDefinition.Create());
            Register(BibliotecaAuroraTesteDefinition.Create());
            Register(ResidencialAuroraTesteDefinition.Create());
            // Adicione novas construções aqui conforme for criando novos arquivos.
        }

        public static IEnumerable<ReinoConstructionDefinition> AllBuildings
        {
            get
            {
                EnsureInitialized();
                return m_Buildings;
            }
        }

        public static ReinoConstructionDefinition GetBuilding(string id)
        {
            EnsureInitialized();

            if (String.IsNullOrWhiteSpace(id))
                return null;

            for (int i = 0; i < m_Buildings.Count; i++)
            {
                ReinoConstructionDefinition def = m_Buildings[i];

                if (def != null && String.Equals(def.Id, id, StringComparison.OrdinalIgnoreCase))
                    return def;
            }

            return null;
        }

        public static List<ReinoConstructionDefinition> GetBuildingsForLot(ReinoLotDefinition lot)
        {
            EnsureInitialized();

            List<ReinoConstructionDefinition> list = new List<ReinoConstructionDefinition>();

            if (lot == null)
                return list;

            for (int i = 0; i < m_Buildings.Count; i++)
            {
                ReinoConstructionDefinition def = m_Buildings[i];

                if (def != null && def.SupportsLot(lot))
                    list.Add(def);
            }

            return list;
        }

        public static List<ReinoConstructionDefinition> GetBuildingsForArea(ReinoAreaDefinition area)
        {
            EnsureInitialized();

            List<ReinoConstructionDefinition> list = new List<ReinoConstructionDefinition>();

            if (area == null)
                return list;

            for (int i = 0; i < m_Buildings.Count; i++)
            {
                ReinoConstructionDefinition def = m_Buildings[i];

                if (def != null && def.SupportsArea(area))
                    list.Add(def);
            }

            return list;
        }

        private static void Register(ReinoConstructionDefinition def)
        {
            if (def != null)
                m_Buildings.Add(def);
        }

        public static string FormatConstructionHtml(ReinoConstructionDefinition def)
        {
            if (def == null)
                return "<BASEFONT COLOR=#000000>Nenhuma construção selecionada.</BASEFONT>";

            return def.DescriptionHtml ?? "<BASEFONT COLOR=#000000>Sem descrição.</BASEFONT>";
        }

        public static string FormatObjectiveHtml(ReinoLotDefinition lot, ReinoLotState state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BASEFONT COLOR=#000000>");

            if (lot == null || state == null || lot.Objective == null)
            {
                sb.Append("Sem objetivo configurado.");
                sb.Append("</BASEFONT>");
                return sb.ToString();
            }

            ReinoLotConfigDefinition config = ReinoLotConfigRegistry.Get(lot.LotConfigId);

            if (lot.Objective.Type == ReinoObjectiveType.KillMob)
            {
                sb.Append("Esse lote precisa ser limpo antes que qualquer obra comece.<BR><BR>");
                if (config != null && !String.IsNullOrWhiteSpace(config.Name))
                {
                    sb.Append("<B>Cenário:</B> ");
                    sb.Append(config.Name);
                    sb.Append("<BR>");
                }

                sb.Append("<B>Objetivo:</B> derrotar ");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(" ");
                sb.Append(String.IsNullOrWhiteSpace(lot.Objective.DisplayName) ? "ameaças" : lot.Objective.DisplayName);
                sb.Append(".<BR>");
                sb.Append("<B>Progresso:</B> ");
                sb.Append(state.ObjectiveProgress);
                sb.Append("/");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(".");
            }
            else if (lot.Objective.Type == ReinoObjectiveType.CollectItem)
            {
                sb.Append("Esse lote está tomado por entulho vivo, raízes ou focos de contaminação.<BR><BR>");
                if (config != null && !String.IsNullOrWhiteSpace(config.Name))
                {
                    sb.Append("<B>Cenário:</B> ");
                    sb.Append(config.Name);
                    sb.Append("<BR>");
                }

                sb.Append("<B>Objetivo:</B> remover ");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(" ");
                sb.Append(String.IsNullOrWhiteSpace(lot.Objective.DisplayName) ? "ameaças" : lot.Objective.DisplayName);
                sb.Append(".<BR>");

                if (config != null && config.CollectibleEntries != null && config.CollectibleEntries.Length > 0)
                {
                    string tool = config.CollectibleEntries[0] != null ? config.CollectibleEntries[0].RequiredToolTypeName : String.Empty;
                    if (!String.IsNullOrWhiteSpace(tool))
                    {
                        sb.Append("<B>Ferramenta:</B> ");
                        sb.Append(tool);
                        sb.Append(".<BR>");
                    }
                }

                sb.Append("<B>Progresso:</B> ");
                sb.Append(state.ObjectiveProgress);
                sb.Append("/");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(".");
            }
            else if (lot.Objective.Type == ReinoObjectiveType.DeliverVirtualResource)
            {
                sb.Append("Essa área ainda precisa de suprimentos antes de receber qualquer construção.<BR><BR>");
                sb.Append("<B>Objetivo:</B> entregar ");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(" de ");
                sb.Append(ReinoExpansionSystem.GetResourceLabel(lot.Objective.ResourceType));
                sb.Append(".<BR>");
                sb.Append("<B>Progresso:</B> ");
                sb.Append(state.ObjectiveProgress);
                sb.Append("/");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(".");
            }
            else
            {
                sb.Append("Sem objetivo configurado.");
            }

            sb.Append("</BASEFONT>");
            return sb.ToString();
        }
    }
}
