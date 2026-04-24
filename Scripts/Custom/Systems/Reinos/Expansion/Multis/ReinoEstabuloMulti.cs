using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Custom.Reinos;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos.Expansion.Multis
{
    public class ReinoEstabuloMulti : ReinoConstructionMulti
    {
        private int[] m_StablePostSerials;

        public static void Initialize()
        {
            EventSink.WorldLoad += OnWorldLoad;
        }

        private static void OnWorldLoad()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), RepairAllLoadedStableMultis);
        }

        private static void RepairAllLoadedStableMultis()
        {
            RepairStableLotStates();

            foreach (Item item in World.Items.Values)
            {
                ReinoEstabuloMulti multi = item as ReinoEstabuloMulti;
                if (multi == null || multi.Deleted)
                    continue;

                multi.RepairMaintenanceRegistration();
                multi.EnsureStablePosts();
            }
        }

        public void EnsureStablePostsForCity(int cityId)
        {
            if (Deleted || Map == null || Map == Map.Internal)
                return;

            if (m_StablePostSerials == null || m_StablePostSerials.Length != EstabuloAuroraDefinition.StablePostOffsets.Length)
                m_StablePostSerials = new int[EstabuloAuroraDefinition.StablePostOffsets.Length];

            for (int i = 0; i < EstabuloAuroraDefinition.StablePostOffsets.Length; i++)
            {
                Point3D off = EstabuloAuroraDefinition.StablePostOffsets[i];
                Point3D loc = new Point3D(Location.X + off.X, Location.Y + off.Y, Location.Z + off.Z);

                StablePost post = World.FindItem((Serial)m_StablePostSerials[i]) as StablePost;

                if (post == null || post.Deleted)
                {
                    post = new StablePost();
                    post.GovernmentCityId = cityId;
                    post.Movable = false;
                    post.Visible = true;
                    post.Name = "poste do estábulo";
                    post.MoveToWorld(loc, Map);
                    m_StablePostSerials[i] = post.Serial.Value;
                }
                else
                {
                    post.GovernmentCityId = cityId;
                    post.Movable = false;
                    post.Visible = true;

                    if (post.Location != loc || post.Map != Map)
                        post.MoveToWorld(loc, Map);
                }
            }
        }

        private static void RepairStableLotStates()
        {
            for (int cityId = 0; cityId < ReinoElectionsSystem.CityNames.Length; cityId++)
            {
                List<ReinoLotDefinition> lots = ReinoExpansionSystem.GetAllLotsForCity(cityId);
                for (int i = 0; i < lots.Count; i++)
                {
                    ReinoLotDefinition lot = lots[i];
                    ReinoLotState state = lot != null ? ReinoExpansionSystem.GetLotState(lot.LotId) : null;
                    if (lot == null || state == null)
                        continue;

                    bool isStableState = String.Equals(state.ConstructionId, EstabuloAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase);
                    bool hasStableMulti = state.MultiSerial > 0 && World.FindItem((Serial)state.MultiSerial) is ReinoEstabuloMulti;

                    if (!isStableState && !hasStableMulti)
                        continue;

                    if (String.IsNullOrWhiteSpace(state.ConstructionId))
                        state.ConstructionId = EstabuloAuroraDefinition.BUILDING_ID;

                    if (state.Status != ReinoLotStatus.Active && state.Status != ReinoLotStatus.Abandoned && state.Status != ReinoLotStatus.UnderConstruction)
                        state.Status = ReinoLotStatus.Active;
                }
            }
        }

        [Constructable]
        public ReinoEstabuloMulti() : this(0, EstabuloAuroraDefinition.BUILDING_ID, -1)
        {
        }

        public ReinoEstabuloMulti(int referenceId, string constructionId, int stageIndex)
            : base(0xA7, referenceId, constructionId, stageIndex)
        {
            Name = "Estábulo Aurora";
            Movable = false;
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), EnsureStablePosts);
        }

        public ReinoEstabuloMulti(Serial serial) : base(serial)
        {
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureStablePosts();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            EnsureStablePosts();
        }

        public override void OnAfterDelete()
        {
            DeleteStablePosts();
            base.OnAfterDelete();
        }

        public void EnsureStablePosts()
        {
            EnsureStablePostsForCity(ResolveGovernmentCityId());
        }

        private int ResolveGovernmentCityId()
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            if (lot != null)
                return lot.CityId;

            return ReinoMilitarySystem.ResolveCityIdAt(Location, Map);
        }

        private void RepairMaintenanceRegistration()
        {
            ReinoLotDefinition lot = ReinoExpansionSystem.GetLotDefinition(ReferenceId);
            ReinoLotState state = ReinoExpansionSystem.GetLotState(ReferenceId);

            if (lot == null || state == null)
                return;

            if (String.IsNullOrWhiteSpace(state.ConstructionId))
                state.ConstructionId = EstabuloAuroraDefinition.BUILDING_ID;

            if (state.Status != ReinoLotStatus.Active && state.Status != ReinoLotStatus.Abandoned)
                state.Status = ReinoLotStatus.Active;
        }

        private void DeleteStablePosts()
        {
            if (m_StablePostSerials == null)
                return;

            for (int i = 0; i < m_StablePostSerials.Length; i++)
            {
                Item item = World.FindItem((Serial)m_StablePostSerials[i]);
                if (item != null && !item.Deleted)
                    item.Delete();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            int len = m_StablePostSerials != null ? m_StablePostSerials.Length : 0;
            writer.Write(len);
            for (int i = 0; i < len; i++)
                writer.Write(m_StablePostSerials[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 1)
            {
                int len = reader.ReadInt();
                m_StablePostSerials = new int[len];
                for (int i = 0; i < len; i++)
                    m_StablePostSerials[i] = reader.ReadInt();
            }

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), EnsureStablePosts);
        }
    }
}
