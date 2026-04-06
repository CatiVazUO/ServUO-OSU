using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public class ReinoExpansionCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReinoAreaAdd", AccessLevel.GameMaster, OnAreaAdd);
            CommandSystem.Register("ReinoAreaClear", AccessLevel.GameMaster, OnAreaClear);
            CommandSystem.Register("ReinoAreaVer", AccessLevel.GameMaster, OnAreaVer);
            CommandSystem.Register("ReinoAreaInvis", AccessLevel.GameMaster, OnAreaInvis);

            CommandSystem.Register("ReinoLotAdd", AccessLevel.GameMaster, OnLotAdd);
            CommandSystem.Register("ReinoLotVer", AccessLevel.GameMaster, OnLotVer);
            CommandSystem.Register("ReinoLotesVer", AccessLevel.GameMaster, OnLotesVer);

            CommandSystem.Register("ReinoDecorAreaAdd", AccessLevel.GameMaster, OnDecorAreaAdd);
            CommandSystem.Register("ReinoDecorAreaClear", AccessLevel.GameMaster, OnDecorAreaClear);
            CommandSystem.Register("ReinoMuralhaAreaAdd", AccessLevel.GameMaster, OnWallAreaAdd);
            CommandSystem.Register("ReinoMuralhaAreaClear", AccessLevel.GameMaster, OnWallAreaClear);

            CommandSystem.Register("ReinoRecursosAdd", AccessLevel.GameMaster, OnAddResources);
            CommandSystem.Register("ReinoRecursosInfo", AccessLevel.GameMaster, OnResourcesInfo);
            CommandSystem.Register("ReinoLotInfo", AccessLevel.GameMaster, OnLotInfo);
            CommandSystem.Register("ReinoLotProgress", AccessLevel.GameMaster, OnLotProgress);
            CommandSystem.Register("ReinoLotReset", AccessLevel.GameMaster, OnLotReset);
            CommandSystem.Register("ReinoLotClean", AccessLevel.GameMaster, OnLotClean);
            CommandSystem.Register("ReinoRecursosAddAll", AccessLevel.GameMaster, OnAddAllResources);
            CommandSystem.Register("ReinoMaintenanceNow", AccessLevel.GameMaster, OnMaintenanceNow);
            CommandSystem.Register("ReinoDeleteNearbyMulti", AccessLevel.GameMaster, OnDeleteNearbyMulti);
            CommandSystem.Register("ReinoListNearbyMulti", AccessLevel.GameMaster, OnListNearbyMulti);
        }

        private static bool TryParseCity(string raw, out int cityId)
        {
            return ReinoExpansionSystem.TryParseCityId(raw, out cityId);
        }

        private static void OnListNearbyMulti(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null || from.Map == null || from.Map == Map.Internal)
                return;

            int range = 12;

            if (e.Arguments.Length > 0)
            {
                int parsed;
                if (Int32.TryParse(e.Arguments[0], out parsed) && parsed > 0)
                    range = parsed;
            }

            IPooledEnumerable eable = from.Map.GetItemsInRange(from.Location, range);
            int count = 0;

            foreach (Item item in eable)
            {
                if (item == null || item.Deleted)
                    continue;

                if (item is ReinoConstructionMulti || item is BaseMulti || item is ReinoPlacedMultiBase)
                {
                    count++;
                    from.SendMessage("#{0}: {1} serial={2} loc={3}", count, item.GetType().Name, item.Serial, item.Location);
                }
            }

            eable.Free();

            if (count == 0)
                from.SendMessage("Nenhum multi encontrado no alcance.");
        }


        private static void OnDeleteNearbyMulti(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null || from.Map == null || from.Map == Map.Internal)
                return;

            int range = 12;

            if (e.Arguments.Length > 0)
            {
                int parsed;
                if (Int32.TryParse(e.Arguments[0], out parsed) && parsed > 0)
                    range = parsed;
            }

            IPooledEnumerable eable = from.Map.GetItemsInRange(from.Location, range);
            List<Item> found = new List<Item>();

            foreach (Item item in eable)
            {
                if (item == null || item.Deleted)
                    continue;

                if (item is ReinoConstructionMulti || item is BaseMulti || item is ReinoPlacedMultiBase)
                    found.Add(item);
            }

            eable.Free();

            if (found.Count == 0)
            {
                from.SendMessage("Nenhum multi encontrado no alcance.");
                return;
            }

            for (int i = 0; i < found.Count; i++)
            {
                Item item = found[i];
                from.SendMessage("Deletando multi: {0} serial={1}", item.GetType().Name, item.Serial);
                item.Delete();
            }

            from.SendMessage("{0} multi(s) deletado(s).", found.Count);
        }

        private static void OnMaintenanceNow(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length == 0)
            {
                ReinoMaintenanceSystem.RunWeeklyMaintenanceNow();
                e.Mobile.SendMessage("Cobrança semanal forçada para todos os reinos.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida. Use Aurora, Xetá, Lurone ou Willran.");
                return;
            }

            ReinoMaintenanceSystem.RunWeeklyMaintenanceNow(cityId);
            e.Mobile.SendMessage("Cobrança semanal forçada para {0}.", ReinoElectionsSystem.GetCityName(cityId));
        }
        private static void OnAddAllResources(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 2)
            {
                e.Mobile.SendMessage("Use [ReinoRecursosAddAll <cidade> <valor>.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            int amount;
            if (!Int32.TryParse(e.Arguments[1], out amount))
            {
                e.Mobile.SendMessage("Valor inválido.");
                return;
            }

            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Wood, amount);
            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Iron, amount);
            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Cloth, amount);
            ReinoExpansionSystem.AddLedgerResource(cityId, ReinoResourceType.Gold, amount);
            e.Mobile.SendMessage("Recursos ajustados. {0}: {1}", ReinoElectionsSystem.GetCityName(cityId), ReinoExpansionSystem.GetLedger(cityId).GetDebugLine());
        }

        private static void OnLotClean(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoLotClean <idDoLote>.");
                return;
            }

            int lotId;
            if (!Int32.TryParse(e.Arguments[0], out lotId))
            {
                e.Mobile.SendMessage("ID inválido.");
                return;
            }

            string message;
            if (!ReinoExpansionSystem.CleanLotForTesting(lotId, out message))
            {
                e.Mobile.SendMessage(message);
                return;
            }

            e.Mobile.SendMessage(message);
        }
        private static void OnAreaAdd(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoAreaAdd <cidade>. Ex: [ReinoAreaAdd Aurora");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida. Use Aurora, Xetá, Lurone ou Willran.");
                return;
            }

            ReinoExpansionSystem.ShowCityOverlay(e.Mobile, cityId);
            e.Mobile.SendMessage("Clique no canto noroeste da área de reino.");
            e.Mobile.Target = new AreaRectTarget(cityId, ReinoAreaType.Kingdom, 0);
        }

        private static void OnDecorAreaAdd(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoDecorAreaAdd <cidade>.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            ReinoExpansionSystem.ShowCityOverlay(e.Mobile, cityId);
            e.Mobile.SendMessage("Clique no canto noroeste da área decorativa.");
            e.Mobile.Target = new AreaRectTarget(cityId, ReinoAreaType.Decorative, 0);
        }

        private static void OnWallAreaAdd(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoMuralhaAreaAdd <cidade>.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            ReinoExpansionSystem.ShowCityOverlay(e.Mobile, cityId);
            e.Mobile.SendMessage("Clique no canto noroeste da área da muralha.");
            e.Mobile.Target = new AreaRectTarget(cityId, ReinoAreaType.Wall, 0);
        }

        private static void OnAreaClear(CommandEventArgs e)
        {
            ReinoExpansionSystem.ShowMapOverlay(e.Mobile, e.Mobile.Map);
            e.Mobile.SendMessage("Clique no canto noroeste da área que você quer apagar.");
            e.Mobile.Target = new AreaClearTarget();
        }

        private static void OnAreaVer(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoAreaVer <cidade>.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            ReinoExpansionSystem.ShowKingdomAreas(e.Mobile, cityId);
            e.Mobile.SendMessage("Mostrando as áreas de reino de {0}.", ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnAreaInvis(CommandEventArgs e)
        {
            ReinoExpansionSystem.ClearPreview(e.Mobile);
            e.Mobile.SendMessage("Marcadores ocultados.");
        }

        private static void OnDecorAreaClear(CommandEventArgs e)
        {
            OnAreaClear(e);
        }

        private static void OnWallAreaClear(CommandEventArgs e)
        {
            OnAreaClear(e);
        }

        private static void OnLotAdd(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 2)
            {
                e.Mobile.SendMessage("Use [ReinoLotAdd <cidade> <15|20|30|40>. Ex: [ReinoLotAdd Aurora 15");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            int side;
            if (!Int32.TryParse(e.Arguments[1], out side) || (side != 15 && side != 20 && side != 30 && side != 40))
            {
                e.Mobile.SendMessage("Tamanho inválido. Use 15, 20, 30 ou 40.");
                return;
            }

            ReinoExpansionSystem.ShowCityOverlay(e.Mobile, cityId);
            e.Mobile.SendMessage("Clique no tile superior esquerdo do lote.");
            e.Mobile.Target = new LotCornerTarget(cityId, side);
        }

        private static void OnLotVer(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoLotVer <idDoLote>.");
                return;
            }

            int lotId;
            if (!Int32.TryParse(e.Arguments[0], out lotId))
            {
                e.Mobile.SendMessage("ID inválido.");
                return;
            }

            ReinoExpansionSystem.ShowSingleLot(e.Mobile, lotId);
            e.Mobile.SendMessage("Mostrando o lote {0}.", lotId);
        }

        private static void OnLotesVer(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoLotesVer <cidade>.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            ReinoExpansionSystem.ShowLotsForCity(e.Mobile, cityId);
            e.Mobile.SendMessage("Mostrando todos os lotes de {0}.", ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnAddResources(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 3)
            {
                e.Mobile.SendMessage("Use [ReinoRecursosAdd <cidade> <wood|iron|cloth|gold> <valor>.");
                return;
            }

            int cityId;
            if (!TryParseCity(e.Arguments[0], out cityId))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            ReinoResourceType type;
            if (!ReinoExpansionSystem.TryParseResourceType(e.Arguments[1], out type))
            {
                e.Mobile.SendMessage("Recurso inválido. Use wood, iron, cloth ou gold.");
                return;
            }

            int amount;
            if (!Int32.TryParse(e.Arguments[2], out amount))
            {
                e.Mobile.SendMessage("Valor inválido.");
                return;
            }

            ReinoExpansionSystem.AddLedgerResource(cityId, type, amount);
            e.Mobile.SendMessage("Recurso ajustado. {0}: {1}", ReinoElectionsSystem.GetCityName(cityId), ReinoExpansionSystem.GetLedger(cityId).GetDebugLine());
        }

        private static void OnResourcesInfo(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                for (int cityId = 0; cityId < 4; cityId++)
                    e.Mobile.SendMessage("{0}: {1}", ReinoElectionsSystem.GetCityName(cityId), ReinoExpansionSystem.GetLedger(cityId).GetDebugLine());
                return;
            }

            int city;
            if (!TryParseCity(e.Arguments[0], out city))
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            e.Mobile.SendMessage("{0}: {1}", ReinoElectionsSystem.GetCityName(city), ReinoExpansionSystem.GetLedger(city).GetDebugLine());
        }

        private static void OnLotInfo(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoLotInfo <idDoLote>.");
                return;
            }

            int lotId;
            if (!Int32.TryParse(e.Arguments[0], out lotId))
            {
                e.Mobile.SendMessage("ID inválido.");
                return;
            }

            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(lotId);
            ReinoLotState st = ReinoExpansionSystem.GetLotState(lotId);

            if (lot == null || st == null)
            {
                e.Mobile.SendMessage("Lote inválido.");
                return;
            }

            e.Mobile.SendMessage("{0}", lot.Name);
            e.Mobile.SendMessage("Cidade: {0}", ReinoElectionsSystem.GetCityName(lot.CityId));
            e.Mobile.SendMessage("Mapa: {0}", lot.Map != null ? lot.Map.Name : "null");
            e.Mobile.SendMessage("Canto NW: {0}", lot.NorthWest);
            e.Mobile.SendMessage("Status: {0}", ReinoExpansionSystem.GetStatusLabel(st.Status));
            e.Mobile.SendMessage("Progresso: {0}/{1}", st.ObjectiveProgress, lot.Objective.RequiredAmount);
            e.Mobile.SendMessage("Construção: {0}", String.IsNullOrWhiteSpace(st.ConstructionId) ? "nenhuma" : st.ConstructionId);
            e.Mobile.SendMessage("Recursos do reino: {0}", ReinoExpansionSystem.GetLedger(lot.CityId).GetDebugLine());
        }

        private static void OnLotProgress(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 2)
            {
                e.Mobile.SendMessage("Use [ReinoLotProgress <idDoLote> <valor>.");
                return;
            }

            int lotId;
            int value;
            if (!Int32.TryParse(e.Arguments[0], out lotId) || !Int32.TryParse(e.Arguments[1], out value))
            {
                e.Mobile.SendMessage("Parâmetros inválidos.");
                return;
            }

            string message;
            if (ReinoExpansionSystem.SetLotProgress(lotId, value, out message))
                e.Mobile.SendMessage(message);
            else
                e.Mobile.SendMessage(message);
        }

        private static void OnLotReset(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Use [ReinoLotReset <idDoLote>.");
                return;
            }

            int lotId;
            if (!Int32.TryParse(e.Arguments[0], out lotId))
            {
                e.Mobile.SendMessage("ID inválido.");
                return;
            }

            string message;
            if (ReinoExpansionSystem.ResetLot(lotId, out message))
                e.Mobile.SendMessage(message);
            else
                e.Mobile.SendMessage(message);
        }

        private class AreaRectTarget : Target
        {
            private readonly int m_CityId;
            private readonly ReinoAreaType m_Type;
            private Point3D m_Start;
            private bool m_HasFirst;

            public AreaRectTarget(int cityId, ReinoAreaType type, int ignored) : base(20, true, TargetFlags.None)
            {
                m_CityId = cityId;
                m_Type = type;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                if (p == null)
                    return;

                if (!m_HasFirst)
                {
                    m_HasFirst = true;
                    m_Start = new Point3D(p);
                    from.SendMessage("Agora clique no canto sudeste da área.");
                    from.Target = this;
                    return;
                }

                Point3D end = new Point3D(p);
                Utility.FixPoints(ref m_Start, ref end);
                Rectangle2D rect = new Rectangle2D(m_Start, new Point3D(end.X + 1, end.Y + 1, end.Z));
                string message;
                int areaId;

                if (m_Type == ReinoAreaType.Kingdom)
                {
                    if (ReinoExpansionSystem.AddKingdomArea(m_CityId, from.Map, rect, m_Start.Z, out areaId, out message))
                    {
                        from.SendMessage(message);
                        ReinoExpansionSystem.ShowCityOverlay(from, m_CityId);
                    }
                    else
                        from.SendMessage(message);
                }
                else if (m_Type == ReinoAreaType.Wall)
                {
                    if (ReinoExpansionSystem.AddWallArea(m_CityId, from.Map, rect, m_Start.Z, String.Empty, out areaId, out message))
                    {
                        from.SendMessage(message);
                        ReinoExpansionSystem.ShowCityOverlay(from, m_CityId);
                    }
                    else
                        from.SendMessage(message);
                }
                else
                {
                    from.SendMessage("Agora clique em qualquer ponto do lote ao qual essa área decorativa ficará atrelada.");
                    from.Target = new DecorativeLinkTarget(m_CityId, rect, m_Start.Z);
                }
            }
        }

        private class DecorativeLinkTarget : Target
        {
            private readonly int m_CityId;
            private readonly Rectangle2D m_Rect;
            private readonly int m_Z;

            public DecorativeLinkTarget(int cityId, Rectangle2D rect, int z) : base(20, true, TargetFlags.None)
            {
                m_CityId = cityId;
                m_Rect = rect;
                m_Z = z;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                if (p == null)
                    return;

                ReinoLotDefinition lot = ReinoExpansionSystem.FindLotAt(new Point3D(p), from.Map);
                if (lot == null)
                {
                    from.SendMessage("Nenhum lote foi encontrado nesse ponto.");
                    return;
                }

                if (lot.CityId != m_CityId)
                {
                    from.SendMessage("Esse lote pertence a outro reino.");
                    return;
                }

                int areaId;
                string message;
                if (ReinoExpansionSystem.AddDecorativeArea(m_CityId, from.Map, m_Rect, m_Z, lot.LotId, out areaId, out message))
                {
                    from.SendMessage(message);
                    ReinoExpansionSystem.ShowCityOverlay(from, m_CityId);
                }
                else
                    from.SendMessage(message);
            }
        }

        private class AreaClearTarget : Target
        {
            private Point3D m_Start;
            private bool m_HasFirst;

            public AreaClearTarget() : base(20, true, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                if (p == null)
                    return;

                if (!m_HasFirst)
                {
                    m_HasFirst = true;
                    m_Start = new Point3D(p);
                    from.SendMessage("Agora clique no canto sudeste da área que você quer apagar.");
                    from.Target = this;
                    return;
                }

                Point3D end = new Point3D(p);
                Utility.FixPoints(ref m_Start, ref end);
                Rectangle2D rect = new Rectangle2D(m_Start, new Point3D(end.X + 1, end.Y + 1, end.Z));

                string message;
                if (ReinoExpansionSystem.ClearByRect(from.Map, rect, out message))
                {
                    from.SendMessage(message);
                    ReinoExpansionSystem.ShowMapOverlay(from, from.Map);
                }
                else
                    from.SendMessage(message);
            }
        }

        private class LotCornerTarget : Target
        {
            private readonly int m_CityId;
            private readonly int m_Side;

            public LotCornerTarget(int cityId, int side) : base(20, true, TargetFlags.None)
            {
                m_CityId = cityId;
                m_Side = side;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                if (p == null)
                    return;

                int lotId;
                string message;
                Point3D point = new Point3D(p);
                if (ReinoExpansionSystem.CreateLot(m_CityId, from.Map, point, m_Side, out lotId, out message))
                {
                    from.SendMessage(message);
                    ReinoExpansionSystem.ShowCityOverlay(from, m_CityId);
                }
                else
                    from.SendMessage(message);
            }
        }
    }
}
