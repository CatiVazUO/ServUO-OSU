using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Custom.Reinos
{
    public class ReinoAddBannerGump : Gump
    {
        private readonly int m_CityId;

        // valor mostrado no gump: 0 ou 20
        private readonly int m_DisplayZ;

        private readonly bool m_WithPole;
        private readonly bool m_FacingSouth;

        private const int ButtonZ0 = 61000;
        private const int ButtonZ20 = 61001;

        private const int ButtonWithPole = 61010;
        private const int ButtonWithoutPole = 61011;

        private const int ButtonFacingSouth = 61020;
        private const int ButtonFacingEast = 61021;

        private const int ButtonConfirm = 61030;
        private const int ButtonUndoLast = 61031;

        public ReinoAddBannerGump(PlayerMobile from, int cityId)
            : this(from, cityId, 0, true, true)
        {
        }

        public ReinoAddBannerGump(PlayerMobile from, int cityId, int displayZ, bool withPole, bool facingSouth)
            : base(0, 0)
        {
            m_CityId = cityId;
            m_DisplayZ = displayZ;
            m_WithPole = withPole;
            m_FacingSouth = facingSouth;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(159, 119, 282, 335, 386);
            AddImageTiled(135, 384, 78, 89, 359);
            AddImageTiled(387, 384, 74, 90, 360);
            AddImageTiled(387, 100, 74, 82, 361);
            AddImageTiled(135, 100, 74, 90, 362);
            AddImageTiled(139, 180, 26, 206, 365);
            AddImageTiled(432, 177, 26, 216, 366);
            AddImageTiled(204, 443, 204, 31, 367);
            AddImageTiled(201, 102, 189, 31, 368);
            AddImageTiled(223, 165, 183, 21, 470);
            AddImageTiled(184, 165, 178, 21, 469);


            AddLabel(229, 143, 1152, @"Adicionar Banners");

            AddLabel(200, 201, 1152, @"Z = 0");
            AddButton(251, 201, m_DisplayZ == 0 ? 530 : 531, m_DisplayZ == 0 ? 530 : 531, ButtonZ0, GumpButtonType.Reply, 0);

            AddLabel(321, 201, 1152, @"Z = 20");
            AddButton(372, 201, m_DisplayZ == 20 ? 530 : 531, m_DisplayZ == 20 ? 530 : 531, ButtonZ20, GumpButtonType.Reply, 0);

            AddImageTiled(176, 231, 245, 5, 367);

            AddLabel(180, 248, 1152, @"Com Haste");
            AddButton(261, 248, m_WithPole ? 530 : 531, m_WithPole ? 530 : 531, ButtonWithPole, GumpButtonType.Reply, 0);

            AddLabel(313, 248, 1152, @"Sem Haste");
            AddButton(395, 248, !m_WithPole ? 530 : 531, !m_WithPole ? 530 : 531, ButtonWithoutPole, GumpButtonType.Reply, 0);

            AddImageTiled(176, 280, 245, 5, 367);

            AddLabel(180, 298, 1152, @"Pro Sul");
            AddButton(261, 298, m_FacingSouth ? 530 : 531, m_FacingSouth ? 530 : 531, ButtonFacingSouth, GumpButtonType.Reply, 0);

            AddLabel(313, 298, 1152, @"Pro Leste");
            AddButton(395, 298, !m_FacingSouth ? 530 : 531, !m_FacingSouth ? 530 : 531, ButtonFacingEast, GumpButtonType.Reply, 0);

            AddImageTiled(176, 328, 245, 5, 367);
            AddImageTiled(294, 345, 5, 90, 366);

            AddButton(318, 357, 492, 492, ButtonConfirm, GumpButtonType.Reply, 0);
            AddLabel(213, 350, 1152, @"Custo");
            AddLabel(191, 377, 1152, @"Moedas:   " + ReinoVisualSystem.AddBannerGoldCost);
            AddLabel(191, 397, 1152, @"Tecido:   " + ReinoVisualSystem.AddBannerClothCost);

            AddButton(318, 401, 578, 579, ButtonUndoLast, GumpButtonType.Reply, 0);
            AddLabel(351, 405, 0, @"Desfazer");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            int displayZ = m_DisplayZ;
            bool withPole = m_WithPole;
            bool facingSouth = m_FacingSouth;

            switch (info.ButtonID)
            {
                case 0:
                    return;

                case ButtonZ0:
                    displayZ = 0;
                    break;

                case ButtonZ20:
                    displayZ = 20;
                    break;

                case ButtonWithPole:
                    withPole = true;
                    break;

                case ButtonWithoutPole:
                    withPole = false;
                    break;

                case ButtonFacingSouth:
                    facingSouth = true;
                    break;

                case ButtonFacingEast:
                    facingSouth = false;
                    break;

                case ButtonUndoLast:
                    {
                        if (!ReinoAccessHelper.IsCurrentGovernor(from, m_CityId))
                        {
                            from.SendMessage("Somente o líder atual do reino pode desfazer o último banner.");
                            return;
                        }

                        string undoMessage;
                        if (!ReinoVisualSystem.TryUndoLastPlacedBanner(from, m_CityId, out undoMessage))
                        {
                            from.SendMessage(undoMessage);
                            from.SendGump(new ReinoAddBannerGump(from, m_CityId, m_DisplayZ, m_WithPole, m_FacingSouth));
                            return;
                        }

                        from.SendMessage(undoMessage);
                        from.SendGump(new ReinoAddBannerGump(from, m_CityId, m_DisplayZ, m_WithPole, m_FacingSouth));
                        return;
                    }

                case ButtonConfirm:
                    if (!ReinoAccessHelper.IsCurrentGovernor(from, m_CityId))
                    {
                        from.SendMessage("Somente o líder atual do reino pode adicionar banners.");
                        return;
                    }


                    // Conversão interna:
                    // com haste: 0 / 20
                    // sem haste: 14 / 34
                    int realZOffset = withPole ? displayZ : (displayZ + 14);

                    from.SendMessage("Selecione o chão dentro do território do reino onde o banner será colocado.");
                    from.Target = new ReinoAddBannerTarget(m_CityId, displayZ, realZOffset, withPole, facingSouth);
                    from.SendGump(new ReinoAddBannerGump(from, m_CityId, displayZ, withPole, facingSouth));
                    return;
            }

            from.SendGump(new ReinoAddBannerGump(from, m_CityId, displayZ, withPole, facingSouth));
        }

        private class ReinoAddBannerTarget : Target
        {
            private readonly int m_CityId;
            private readonly int m_DisplayZ;
            private readonly int m_RealZOffset;
            private readonly bool m_WithPole;
            private readonly bool m_FacingSouth;

            public ReinoAddBannerTarget(int cityId, int displayZ, int realZOffset, bool withPole, bool facingSouth)
                : base(12, true, TargetFlags.None)
            {
                m_CityId = cityId;
                m_DisplayZ = displayZ;
                m_RealZOffset = realZOffset;
                m_WithPole = withPole;
                m_FacingSouth = facingSouth;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || pm.Deleted)
                    return;

                Point3D worldLoc;
                Map map;

                if (!TryResolveWorldPoint(targeted, pm, out worldLoc, out map))
                {
                    pm.SendMessage("Escolha um ponto válido no chão.");
                    pm.SendGump(new ReinoAddBannerGump(pm, m_CityId, m_DisplayZ, m_WithPole, m_FacingSouth));
                    return;
                }

                string message;
                if (!ReinoVisualSystem.TryPlaceBanner(pm, m_CityId, m_WithPole, m_FacingSouth, m_RealZOffset, worldLoc, map, out message))
                {
                    pm.SendMessage(message);
                    pm.SendGump(new ReinoAddBannerGump(pm, m_CityId, m_DisplayZ, m_WithPole, m_FacingSouth));
                    return;
                }

                pm.SendMessage(message);
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || pm.Deleted)
                    return;

                pm.SendGump(new ReinoAddBannerGump(pm, m_CityId, m_DisplayZ, m_WithPole, m_FacingSouth));
            }

            private static bool TryResolveWorldPoint(object targeted, PlayerMobile from, out Point3D worldLoc, out Map map)
            {
                worldLoc = Point3D.Zero;
                map = from.Map;

                if (targeted == null)
                    return false;

                Item item = targeted as Item;
                if (item != null)
                {
                    if (item.RootParent != null)
                        return false;

                    worldLoc = item.GetWorldLocation();
                    map = item.Map;
                    return map != null && map != Map.Internal;
                }

                IPoint3D p = targeted as IPoint3D;
                if (p != null)
                {
                    worldLoc = new Point3D(p.X, p.Y, p.Z);
                    map = from.Map;
                    return map != null && map != Map.Internal;
                }

                return false;
            }
        }
    }
}
