using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Custom.Systems.PlayerMadeStatues;
using Server.Mobiles;

namespace Server.Mobiles
{
    public class LiveModelStatue : Mobile
    {
        private Mobile m_Sculptor;
        private DateTime m_CustomizationExpire;
        private bool m_CustomizationDone;
        private List<StatuePoseDefinition> m_Poses;
        private int m_PreviewPoseIndex;
        private int m_PreviewDirectionIndex;
        private int m_MaterialHue;
        private bool m_MountedLook;
        private int m_OSUBodyVariant;
        private int m_OSUFaceIndex;
        private int m_PlacementZOffset;
        private StatuePlatformSize m_RequiredPlatformSize;
        private BaseStatuePlatformItem m_Platform;
        private static readonly List<LiveModelStatue> m_AllStatues = new List<LiveModelStatue>();
        private static InternalReapplyTimer m_Timer;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Sculptor { get { return m_Sculptor; } set { m_Sculptor = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime CustomizationExpire { get { return m_CustomizationExpire; } set { m_CustomizationExpire = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CustomizationDone { get { return m_CustomizationDone; } set { m_CustomizationDone = value; } }

        public int PoseCount
        {
            get { return m_Poses == null ? 0 : m_Poses.Count; }
        }

        public int CurrentPlacementOffset
        {
            get { return m_PlacementZOffset; }
        }

        private static readonly StatuePoseDefinition[] m_MountedDefaultPoses = new StatuePoseDefinition[]
        {
            new StatuePoseDefinition("Pose 1", 23, 1, true),
            new StatuePoseDefinition("Pose 2", 23, 2, true),
            new StatuePoseDefinition("Pose 3", 26, 1, true),
            new StatuePoseDefinition("Pose 4", 28, 5, true)
        };

        private class InternalReapplyTimer : Timer
        {
            public InternalReapplyTimer() : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                for (int i = m_AllStatues.Count - 1; i >= 0; i--)
                {
                    LiveModelStatue s = m_AllStatues[i];

                    if (s == null || s.Deleted || s.Map == null || s.Map == Map.Internal)
                    {
                        m_AllStatues.RemoveAt(i);
                        continue;
                    }

                    s.ApplyPreview(s.m_PreviewPoseIndex, s.m_PreviewDirectionIndex);
                }
            }
        }

        [Constructable]
        public LiveModelStatue() : base()
        {
            Blessed = true;
            Frozen = true;
            CantWalk = true;
            Name = null;
            m_Poses = new List<StatuePoseDefinition>();
            m_OSUBodyVariant = -1;
            m_OSUFaceIndex = 0;
            RegisterSelf();
        }

        public LiveModelStatue(Mobile source, Mobile sculptor, int materialId, Server.Custom.Systems.PlayerMadeStatues.StatuePoseDefinition[] poses, StatuePlatformSize requiredPlatformSize, int placementZOffset, BaseStatuePlatformItem platform) : this()
        {
            if (source != null)
            {
                CloneBody(source);
                m_MountedLook = source.Mounted;
                CloneClothes(source);
                Name = null;

                PlayerMobile pm = source as PlayerMobile;

                if (pm != null && pm.OSUCreation != null && pm.OSUCreationCompleted)
                {
                    m_OSUBodyVariant = pm.OSUCreation.BodyVariant;
                    m_OSUFaceIndex = pm.OSUCreation.FaceIndex;
                }
                else
                {
                    m_OSUBodyVariant = -1;
                    m_OSUFaceIndex = 0;
                }
            }

            m_MaterialHue = StatueMaterialOptions.GetHue(materialId);
            m_Sculptor = sculptor;
            m_PlacementZOffset = placementZOffset;
            m_RequiredPlatformSize = requiredPlatformSize;
            m_Platform = platform;
            m_CustomizationExpire = DateTime.UtcNow + TimeSpan.FromHours(1.0);
            m_CustomizationDone = false;
            m_PreviewPoseIndex = 0;
            m_PreviewDirectionIndex = 4;
            m_Poses = new List<StatuePoseDefinition>(poses ?? new StatuePoseDefinition[0]);
            StatuePoseDefinition[] finalPoses = poses ?? new StatuePoseDefinition[0];

            if (m_MountedLook)
                finalPoses = m_MountedDefaultPoses;

            m_Poses = new List<StatuePoseDefinition>(finalPoses);

            InvalidateHues();
            ApplyPreview(0, 4);
        }

        public override bool CanBeDamaged()
        {
            return false;
        }


        public override void OnSingleClick(Mobile from)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
        }

        public static void EnsureTimer()
        {
            if (m_Timer == null)
            {
                m_Timer = new InternalReapplyTimer();
                m_Timer.Start();
            }
        }

        private void RegisterSelf()
        {
            if (!m_AllStatues.Contains(this))
                m_AllStatues.Add(this);

            EnsureTimer();
        }

        private void UnregisterSelf()
        {
            m_AllStatues.Remove(this);
        }

        public override void DisplayPaperdollTo(Mobile to)
        {
            string title = Name;

            if (String.IsNullOrWhiteSpace(title))
                title = "";

            if ((Body == 0x190 || Body == 0x191 || Body == 0x192 || Body == 0x193) && m_OSUBodyVariant >= 0)
            {
                int bodyVariant = m_OSUBodyVariant;
                int faceIndex = m_OSUFaceIndex;

                if (bodyVariant < 0)
                    bodyVariant = 0;
                else if (bodyVariant > 1)
                    bodyVariant = 1;

                if (faceIndex < 0)
                    faceIndex = 0;
                else if (faceIndex > 8)
                    faceIndex = 8;

                string tag = String.Format("[OSUPD:{0}:{1}:1] ", bodyVariant, faceIndex);

                int maxTitleLen = 60 - tag.Length;

                if (maxTitleLen < 0)
                    maxTitleLen = 0;

                if (title.Length > maxTitleLen)
                    title = title.Substring(0, maxTitleLen);

                title = tag + title;
            }

            to.Send(new DisplayPaperdoll(this, title, false));

            if (to.ViewOPL)
            {
                List<Item> items = Items;

                for (int i = 0; i < items.Count; ++i)
                    to.Send(items[i].OPLPacket);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_CustomizationDone)
            {
                from.SendMessage("Esta estátua já foi configurada.");
                return;
            }

            if (from != m_Sculptor)
            {
                from.SendMessage("Somente o escultor desta estátua pode configurá-la.");
                return;
            }

            if (DateTime.UtcNow > m_CustomizationExpire)
            {
                from.SendMessage("O tempo para configurar esta estátua expirou. Ela ficará na pose idle.");
                m_CustomizationDone = true;
                ApplyPreview(0, 4);
                return;
            }

            from.CloseGump(typeof(LiveStatueCustomizeGump));
            from.SendGump(new LiveStatueCustomizeGump(from as PlayerMobile, this, m_PreviewDirectionIndex, m_PreviewPoseIndex));
        }

        public string GetPoseName(int index)
        {
            if (m_Poses == null || index < 0 || index >= m_Poses.Count)
                return "Idle";

            return m_Poses[index].Name;
        }

        public void ApplyPreview(int poseIndex, int directionIndex)
        {
            if (m_Poses == null || m_Poses.Count == 0)
                return;

            if (poseIndex < 0 || poseIndex >= m_Poses.Count)
                poseIndex = 0;

            if (directionIndex < 0 || directionIndex > 7)
                directionIndex = 4;

            m_PreviewPoseIndex = poseIndex;
            m_PreviewDirectionIndex = directionIndex;

            Direction = GetDirection(directionIndex);

            Server.Custom.Systems.PlayerMadeStatues.StatuePoseDefinition pose = m_Poses[poseIndex];
            SendAnimationToClients(pose.Animation, pose.FrameCount);
        }

        public bool TryAdjustPlacement(int delta)
        {
            int next = m_PlacementZOffset + delta;

            if (next > 8 || next < -4)
                return false;

            m_PlacementZOffset = next;

            if (m_Platform != null && !m_Platform.Deleted && m_Platform.Map != null && m_Platform.Map != Map.Internal)
                MoveToWorld(new Point3D(m_Platform.X, m_Platform.Y, m_Platform.Z + m_Platform.PlatformHeight + m_PlacementZOffset), m_Platform.Map);
            else
                Location = new Point3D(X, Y, Z + delta);

            ApplyPreview(m_PreviewPoseIndex, m_PreviewDirectionIndex);
            return true;
        }

        public void ConfirmPreview()
        {
            m_CustomizationDone = true;
            Frozen = true;
            CantWalk = true;
            Blessed = true;

            if (m_Platform != null && !m_Platform.Deleted && m_Platform.Map != null && m_Platform.Map != Map.Internal)
                MoveToWorld(new Point3D(m_Platform.X, m_Platform.Y, m_Platform.Z + m_Platform.PlatformHeight + m_PlacementZOffset), m_Platform.Map);

            ApplyPreview(m_PreviewPoseIndex, m_PreviewDirectionIndex);
        }

        private Direction GetDirection(int index)
        {
            switch (index)
            {
                case 0: return Direction.Up;
                case 1: return Direction.North;
                case 2: return Direction.Right;
                case 3: return Direction.East;
                case 4: return Direction.Down;
                case 5: return Direction.South;
                case 6: return Direction.Left;
                case 7: return Direction.West;
                default: return Direction.South;
            }
        }

        private void CloneBody(Mobile from)
        {
            Name = from.Name;

            Body = from.Body;
            BodyValue = from.BodyValue;
            BodyMod = 0;
            Hue = from.Hue;

            Female = from.Female;
            HairItemID = from.HairItemID;
            FacialHairItemID = from.FacialHairItemID;
            HairHue = from.HairHue;
            FacialHairHue = from.FacialHairHue;

            if (from is PlayerMobile && this is Mobile)
            {
                try
                {
                    Race = from.Race;
                }
                catch
                {
                }
            }
        }

        private void CloneClothes(Mobile from)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
                Items[i].Delete();

            for (int i = from.Items.Count - 1; i >= 0; i--)
            {
                Item item = from.Items[i];
                if (item.Layer == Layer.Backpack || item.Layer == Layer.Bank)
                    continue;

                if (item.Layer == Layer.Mount)
                    continue;

                Item cloned = new Item(item.ItemID);
                cloned.Layer = item.Layer;
                cloned.Name = item.Name;
                cloned.Weight = item.Weight;
                cloned.Movable = false;
                AddItem(cloned);
            }

            if (from.Mounted && from.Mount != null)
            {
                BaseMount bm = from.Mount as BaseMount;

                if (bm != null)
                {
                    Item mountStatue = new Item(bm.ItemID);
                    mountStatue.Layer = Layer.Mount;
                    mountStatue.Movable = false;
                    mountStatue.Hue = m_MaterialHue;
                    AddItem(mountStatue);
                }
            }
        }

        private void InvalidateHues()
        {
            Hue = m_MaterialHue;
            HueMod = m_MaterialHue;
            SolidHueOverride = m_MaterialHue;
            HairHue = m_MaterialHue;

            if (FacialHairItemID > 0)
                FacialHairHue = m_MaterialHue;

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Items[i].Hue = m_MaterialHue;
            }
        }

        private void SendAnimationToClients(int animation, int frameCount)
        {
            if (Map == null)
                return;

            ProcessDelta();

            Packet p = null;
            IPooledEnumerable eable = Map.GetClientsInRange(Location);

            foreach (NetState state in eable)
            {
                if (state.Mobile != null)
                    state.Mobile.ProcessDelta();

                if (p == null)
                    p = Packet.Acquire(new UpdateStatueAnimation(this, 1, animation, frameCount));

                state.Send(p);
            }

            Packet.Release(p);
            eable.Free();
        }

        public LiveModelStatue(Serial serial) : base(serial)
        {
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            UnregisterSelf();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(3);

            writer.Write(m_Sculptor);
            writer.Write(m_CustomizationExpire);
            writer.Write(m_CustomizationDone);
            writer.Write(m_PreviewPoseIndex);
            writer.Write(m_PreviewDirectionIndex);
            writer.Write(m_MaterialHue);
            writer.Write(m_MountedLook);

            writer.Write(m_PlacementZOffset);
            writer.Write((int)m_RequiredPlatformSize);
            writer.Write(m_Platform);

            writer.Write(m_OSUBodyVariant);
            writer.Write(m_OSUFaceIndex);

            int count = m_Poses == null ? 0 : m_Poses.Count;
            writer.Write(count);

            for (int i = 0; i < count; i++)
            {
                writer.Write(m_Poses[i].Name);
                writer.Write(m_Poses[i].Animation);
                writer.Write(m_Poses[i].FrameCount);
                writer.Write(m_Poses[i].Forward);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Sculptor = reader.ReadMobile();
            m_CustomizationExpire = reader.ReadDateTime();
            m_CustomizationDone = reader.ReadBool();
            m_PreviewPoseIndex = reader.ReadInt();
            m_PreviewDirectionIndex = reader.ReadInt();
            m_MaterialHue = reader.ReadInt();

            if (version >= 1)
                m_MountedLook = reader.ReadBool();
            else
                m_MountedLook = false;

            if (version >= 2)
            {
                m_PlacementZOffset = reader.ReadInt();
                m_RequiredPlatformSize = (StatuePlatformSize)reader.ReadInt();
                m_Platform = reader.ReadItem() as BaseStatuePlatformItem;
            }
            else
            {
                m_PlacementZOffset = 0;
                m_RequiredPlatformSize = StatuePlatformSize.None;
                m_Platform = null;
            }

            if (version >= 3)
            {
                m_OSUBodyVariant = reader.ReadInt();
                m_OSUFaceIndex = reader.ReadInt();
            }
            else
            {
                m_OSUBodyVariant = -1;
                m_OSUFaceIndex = 0;
            }

            int count = reader.ReadInt();
            m_Poses = new List<Server.Custom.Systems.PlayerMadeStatues.StatuePoseDefinition>();

            for (int i = 0; i < count; i++)
            {
                string name = reader.ReadString();
                int animation = reader.ReadInt();
                int frameCount = reader.ReadInt();
                bool forward = reader.ReadBool();

                m_Poses.Add(new Server.Custom.Systems.PlayerMadeStatues.StatuePoseDefinition(name, animation, frameCount, forward));
            }

            Blessed = true;
            Frozen = true;
            CantWalk = true;
            InvalidateHues();
            ApplyPreview(m_PreviewPoseIndex, m_PreviewDirectionIndex);
            RegisterSelf();
        }
    }
}

