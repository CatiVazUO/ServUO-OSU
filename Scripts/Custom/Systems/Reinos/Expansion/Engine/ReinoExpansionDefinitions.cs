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
            // Adicione novas construções aqui conforme for criando novos arquivos.
            // Ex.: Register(BancoAuroraDefinition.Create());
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

            if (lot.Objective.Type == ReinoObjectiveType.KillMob)
            {
                sb.Append("Essa área está infestada de esqueletos. Seria impossível construir aqui dessa maneira.<BR><BR>");
                sb.Append("<B>Objetivo:</B> eliminar as ameaças dentro do lote.<BR>");
                sb.Append("<B>Progresso:</B> ");
                sb.Append(state.ObjectiveProgress);
                sb.Append("/");
                sb.Append(lot.Objective.RequiredAmount);
                sb.Append(" skeletons abatidos.");
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
