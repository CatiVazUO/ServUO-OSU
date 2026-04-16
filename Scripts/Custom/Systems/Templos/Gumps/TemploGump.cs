using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Religion;

namespace Server.Custom.Systems.Templos.Gumps
{
    public class TemploGump : Gump
    {
        private const int ButtonCycleGod = 1;
        private const int ButtonToggleDoors = 2;
        private const int ButtonStartRite = 3;
        private const int ButtonStartWedding = 4;
        private const int ButtonStartFuneral = 5;
        private const int ButtonEndEvent = 6;
        private const int ButtonCycleMusic = 7;

        private readonly int m_CityId;
        private readonly string m_ConstructionKey;

        private readonly int m_AltarSerial;
        private static readonly Dictionary<int, Timer> m_RangeTimers = new Dictionary<int, Timer>();

        public TemploGump(PlayerMobile from, int cityId, string constructionKey, int altarSerial) : base(0, 0)
        {
            m_CityId = cityId;
            m_ConstructionKey = constructionKey ?? String.Empty;

            m_AltarSerial = altarSerial;
            StartRangeTimer(from, altarSerial);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            OSUReligionDefinition religion = TemploSystem.GetSelectedReligion(m_ConstructionKey, m_CityId);
            string religionName = religion != null ? religion.Name : "Sem Deus";
            string musicName = TemploSystem.GetRiteMusicLabel(TemploSystem.GetSelectedRiteMusic(m_ConstructionKey, m_CityId));
            bool doorsClosed = TemploSystem.IsTempleClosedToPublic(m_ConstructionKey);
            int chestGold = TemploSystem.GetChestGold(m_ConstructionKey, m_CityId);
            TemploDonationBundle weekly = TemploSystem.GetWeeklyDonations(m_ConstructionKey, m_CityId);
            int totalCost = TemploSystem.GetDisplayedTotalCost(m_ConstructionKey, m_CityId);
            int hourlyCost = TemploSystem.GetDisplayedHourlyCost(m_ConstructionKey, m_CityId);
            TemploEventoTipo activeType = TemploSystem.GetActiveEventType(m_ConstructionKey, m_CityId);

            AddPage(0);
            AddImageTiled(388, 235, 347, 384, 400);
            AddImageTiled(393, 205, 336, 36, 596);
            AddImageTiled(357, 224, 38, 392, 597);
            AddImageTiled(389, 605, 348, 36, 596);
            AddImageTiled(723, 228, 38, 386, 597);
            AddImageTiled(397, 270, 326, 21, 463);
            AddLabel(542, 250, 0, "Templo");
            AddImageTiled(393, 205, 336, 36, 596);
            AddImage(349, 196, 1360);
            AddImage(714, 196, 1360);
            AddImage(713, 596, 1360);
            AddImage(350, 595, 1360);
            AddImageTiled(548, 481, 8, 127, 597);
            AddImageTiled(562, 537, 159, 9, 596);
            AddImageTiled(399, 396, 321, 9, 596);
            AddImageTiled(399, 468, 321, 9, 596);

            AddButton(405, 252, 455, 455, ButtonCycleGod, GumpButtonType.Reply, 0);
            AddLabel(430, 250, 0, "Deus:");
            AddLabelCropped(470, 250, 170, 20, 0, religionName);

            AddButton(405, 443, doorsClosed ? 434 : 455, doorsClosed ? 434 : 455, ButtonToggleDoors, GumpButtonType.Reply, 0);
            AddLabel(430, 441, 0, doorsClosed ? "Abrir Portas" : "Trancar Portas");

            AddButton(407, 298, 455, 455, ButtonStartRite, GumpButtonType.Reply, 0);
            AddLabel(432, 297, 0, "Iniciar Rito");
            AddLabel(611, 297, 0, "100/h");

            AddButton(407, 323, 455, 455, ButtonStartWedding, GumpButtonType.Reply, 0);
            AddLabel(432, 321, 0, "Iniciar Casamento");
            AddLabel(611, 321, 0, "700/h");

            AddButton(407, 348, 455, 455, ButtonStartFuneral, GumpButtonType.Reply, 0);
            AddLabel(432, 346, 0, "Iniciar Funeral");
            AddLabel(611, 346, 0, "400/h");

            AddButton(406, 375, 455, 455, ButtonEndEvent, GumpButtonType.Reply, 0);
            AddLabel(431, 373, 0, "Encerrar Evento");

            AddButton(405, 417, 455, 455, ButtonCycleMusic, GumpButtonType.Reply, 0);
            AddLabel(430, 415, 0, "Música Rito:");
            AddLabelCropped(607, 413, 105, 20, 0, musicName);

            AddLabel(424, 487, 0, "Doações da Semana");
            AddLabel(418, 517, 0, "Moedas:");
            AddLabel(418, 537, 0, "Ferro:");
            AddLabel(418, 557, 0, "Tecido:");
            AddLabel(418, 577, 0, "Madeira:");
            AddLabel(500, 517, 0, weekly != null ? weekly.Gold.ToString() : "0");
            AddLabel(500, 537, 0, weekly != null ? weekly.Iron.ToString() : "0");
            AddLabel(500, 557, 0, weekly != null ? weekly.Cloth.ToString() : "0");
            AddLabel(500, 577, 0, weekly != null ? weekly.Wood.ToString() : "0");

            AddLabel(613, 486, 0, "No Baú");
            AddLabel(573, 508, 0, "Moedas:");
            AddLabel(648, 508, 0, chestGold.ToString());

            AddLabel(592, 556, 0, "Custo Evento:");
            AddLabel(573, 581, 0, "Moedas:");
            AddLabel(648, 581, 0, totalCost.ToString());

     //       AddLabel(418, 602, 0, "Evento Atual:");
      //      AddLabel(500, 602, 0, TemploSystem.GetEventLabel(activeType));
      //      AddLabel(573, 602, 0, "Moedas/h:");
       //     AddLabel(648, 602, 0, hourlyCost.ToString());
        }


        private static void StartRangeTimer(PlayerMobile pm, int altarSerial)
        {
            if (pm == null)
                return;

            StopRangeTimer(pm);

            if (altarSerial <= 0 || pm.Deleted || pm.NetState == null)
                return;

            RangeTimer timer = new RangeTimer(pm, altarSerial);
            m_RangeTimers[pm.Serial.Value] = timer;
            timer.Start();
        }

        private static void StopRangeTimer(PlayerMobile pm)
        {
            if (pm == null)
                return;

            Timer timer;
            if (m_RangeTimers.TryGetValue(pm.Serial.Value, out timer) && timer != null)
                timer.Stop();

            m_RangeTimers.Remove(pm.Serial.Value);
        }

        private class RangeTimer : Timer
        {
            private readonly PlayerMobile m_From;
            private readonly int m_AltarSerial;

            public RangeTimer(PlayerMobile from, int altarSerial)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                m_From = from;
                m_AltarSerial = altarSerial;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                if (m_From == null || m_From.Deleted || m_From.NetState == null)
                {
                    StopRangeTimer(m_From);
                    return;
                }

                if (!m_From.HasGump(typeof(TemploGump)))
                {
                    StopRangeTimer(m_From);
                    return;
                }

                Item altar;
                if (!World.Items.TryGetValue(m_AltarSerial, out altar) || altar == null || altar.Deleted)
                {
                    m_From.CloseGump(typeof(TemploGump));
                    StopRangeTimer(m_From);
                    return;
                }

                if (m_From.Map != altar.Map || !m_From.InRange(altar.GetWorldLocation(), 3))
                {
                    m_From.CloseGump(typeof(TemploGump));
                    StopRangeTimer(m_From);
                }
            }
        }
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null || from.Deleted)
                return;

            string msg = String.Empty;

            switch (info.ButtonID)
            {
                case ButtonCycleGod:
                    TemploSystem.CycleReligion(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                case ButtonToggleDoors:
                    TemploSystem.ToggleDoors(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                case ButtonStartRite:
                    TemploSystem.StartRite(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                case ButtonStartWedding:
                    TemploSystem.StartWedding(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                case ButtonStartFuneral:
                    TemploSystem.StartFuneral(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                case ButtonEndEvent:
                    TemploSystem.EndEvent(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                case ButtonCycleMusic:
                    TemploSystem.CycleRiteMusic(from, m_ConstructionKey, m_CityId, out msg);
                    break;
                default:
                    return;
            }

            if (!String.IsNullOrWhiteSpace(msg))
                from.SendMessage(msg);

            from.CloseGump(typeof(TemploGump));
            from.SendGump(new TemploGump(from, m_CityId, m_ConstructionKey, m_AltarSerial));
        }
    }
}
