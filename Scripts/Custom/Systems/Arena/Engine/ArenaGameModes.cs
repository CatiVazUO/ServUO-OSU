using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Network;
using Server.Custom.Reinos;
using Server.Custom.Systems.Arena.Gumps;

namespace Server.Custom.Systems.Arena
{
    public static class ArenaGameModes
    {
        private static readonly Dictionary<string, JoustSession> m_Joust = new Dictionary<string, JoustSession>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, GladiatorSession> m_Gladiator = new Dictionary<string, GladiatorSession>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, BombermanSession> m_Bomberman = new Dictionary<string, BombermanSession>(StringComparer.OrdinalIgnoreCase);

        public static JoustSession GetOrCreateJoust(string key) { if (!m_Joust.ContainsKey(key)) m_Joust[key] = new JoustSession(key); return m_Joust[key]; }
        public static GladiatorSession GetOrCreateGladiator(string key) { if (!m_Gladiator.ContainsKey(key)) m_Gladiator[key] = new GladiatorSession(key); return m_Gladiator[key]; }
        public static BombermanSession GetOrCreateBomberman(string key) { if (!m_Bomberman.ContainsKey(key)) m_Bomberman[key] = new BombermanSession(key); return m_Bomberman[key]; }

        public static void StopAll(string key)
        {
            JoustSession j; if (m_Joust.TryGetValue(key, out j) && j != null) j.Stop(true);
            GladiatorSession g; if (m_Gladiator.TryGetValue(key, out g) && g != null) g.Stop(true);
            BombermanSession b; if (m_Bomberman.TryGetValue(key, out b) && b != null) b.Stop(true);
        }

        #region JUSTA
        public class JoustSession
        {
            public string Key;
            public PlayerMobile Knight1;
            public PlayerMobile Knight2;
            public bool Running;
            public bool RoundOpen;
            public bool DirectionFlip;
            public bool Knight1Clicked;
            public bool Knight2Clicked;
            public DateTime Click1Utc;
            public DateTime Click2Utc;
            public Point3D Click1Loc;
            public Point3D Click2Loc;
            public Timer RoundTimer;

            private static readonly string[] WinEmotes = new string[]
            {
                "*{0} acerta a lança no peito de {1}!*",
                "*{0} bate a lança nas costelas de {1}!*",
                "*{0} pega em cheio o ombro de {1}!*",
                "*{0} quebra a defesa e derruba {1}!*",
                "*{0} acerta o elmo de {1} com força!*",
                "*{0} empurra {1} para fora da sela!*",
                "*{0} encaixa uma justa perfeita contra {1}!*",
                "*{0} atinge o tronco de {1} e vence a passada!*",
                "*{0} domina o timing e derruba {1}!*",
                "*{0} quebra a lança no impacto em {1}!*",
                "*{0} atinge o escudo e desmonta {1}!*",
                "*{0} entra no ângulo e derruba {1}!*",
                "*{0} ganha a linha e acerta {1}!*",
                "*{0} crava a lança e desequilibra {1}!*",
                "*{0} vence no detalhe e desmonta {1}!*"
            };

            private static readonly string[] LoseEmotes = new string[]
            {
                "*{0} cai no chão com o impacto!*",
                "*{0} perde o equilíbrio e despenca da montaria!*",
                "*{0} é lançado para o lado e rola no chão!*",
                "*{0} recebe o golpe e cai pesado!*",
                "*{0} tenta segurar, mas cai da sela!*",
                "*{0} fica tonto e cai no piso da arena!*",
                "*{0} é desmontado com violência!*",
                "*{0} desaba no meio da passada!*",
                "*{0} escorrega da sela e cai!*",
                "*{0} sente o impacto e vai ao chão!*",
                "*{0} não segura a passada e cai!*",
                "*{0} é derrubado diante da plateia!*",
                "*{0} tenta reagir, mas cai!*",
                "*{0} perde o centro e desaba!*",
                "*{0} tomba no chão da arena!*"
            };

            private static readonly string[] TieEmotes = new string[]
            {
                "*as lanças se cruzam e ninguém cai!*",
                "*ambos acertam ao mesmo tempo!*",
                "*os dois cavaleiros empatam a passada!*",
                "*as justas batem em sincronia perfeita!*",
                "*impacto duplo! empate total!*",
                "*os dois golpes saem juntos!*",
                "*a plateia grita: empate!*",
                "*nenhum cavaleiro leva vantagem!*",
                "*passada limpa, sem vencedor!*",
                "*choque de lanças, empate!*",
                "*os dois erram por muito pouco!*",
                "*as lanças raspam e ninguém cai!*",
                "*ambos acertam fraco, sem queda!*",
                "*sincronia total, sem derrubada!*",
                "*empate técnico na passada!*"
            };

            public JoustSession(string key) { Key = key; }

            public void AddKnight(PlayerMobile host, int slot)
            {
                if (host == null)
                    return;

                host.Target = new KnightTarget(this, slot, host);
                host.SendMessage(slot == 1 ? "Selecione o cavaleiro 1." : "Selecione o cavaleiro 2.");
            }

            public bool CheckGear(out string msg)
            {
                msg = String.Empty;
                if (Knight1 == null || Knight2 == null)
                {
                    msg = "Adicione os dois cavaleiros primeiro.";
                    return false;
                }

                if (!HasJoustGear(Knight1) || !HasJoustGear(Knight2))
                {
                    msg = "Um ou ambos cavaleiros não estão com full plate + montaria + lança 0x26CA.";
                    return false;
                }

                msg = "Equipamentos corretos. Evento pronto para começar.";
                return true;
            }

            public void Play(ReinoLotDefinition lot)
            {
                if (Knight1 == null || Knight2 == null)
                    return;

                Running = true;
                RoundOpen = true;
                Knight1Clicked = false;
                Knight2Clicked = false;

                ArenaSystem.ApplyJoustPlacement(lot, DirectionFlip, Knight1, Knight2);
                DirectionFlip = !DirectionFlip;

                SendHitGump(Knight1);
                SendHitGump(Knight2);

                if (RoundTimer != null)
                    RoundTimer.Stop();

                RoundTimer = Timer.DelayCall(TimeSpan.FromSeconds(4.0), EvaluateRound);
            }

            public void Click(PlayerMobile pm)
            {
                if (!RoundOpen || pm == null)
                    return;

                if (pm == Knight1 && !Knight1Clicked)
                {
                    Knight1Clicked = true;
                    Click1Utc = DateTime.UtcNow;
                    Click1Loc = pm.Location;
                }
                else if (pm == Knight2 && !Knight2Clicked)
                {
                    Knight2Clicked = true;
                    Click2Utc = DateTime.UtcNow;
                    Click2Loc = pm.Location;
                }
            }

            private void EvaluateRound()
            {
                RoundOpen = false;

                if (Knight1 == null || Knight2 == null)
                    return;

                if (Knight1Clicked && Knight2Clicked && Math.Abs((Click1Utc - Click2Utc).TotalMilliseconds) <= 250)
                {
                    EmoteBoth(String.Format(TieEmotes[Utility.Random(TieEmotes.Length)]));
                    return;
                }

                PlayerMobile winner = null;
                PlayerMobile loser = null;

                if (Knight1Clicked && IsHitWindow(Click1Loc, Click2Loc)) { winner = Knight1; loser = Knight2; }
                else if (Knight2Clicked && IsHitWindow(Click2Loc, Click1Loc)) { winner = Knight2; loser = Knight1; }

                if (winner != null && loser != null)
                {
                    winner.Emote(String.Format(WinEmotes[Utility.Random(WinEmotes.Length)], winner.Name, loser.Name));
                    loser.Emote(String.Format(LoseEmotes[Utility.Random(LoseEmotes.Length)], loser.Name));
                    TryDismount(loser);
                    loser.Damage(Utility.RandomMinMax(8, 20));
                    LoseArmorDurability(loser);
                    LoseLanceDurability(winner);
                }
            }

            public void Stop(bool hard)
            {
                Running = false;
                RoundOpen = false;

                if (RoundTimer != null)
                {
                    RoundTimer.Stop();
                    RoundTimer = null;
                }

                Release(Knight1);
                Release(Knight2);
                Knight1 = null;
                Knight2 = null;
            }

            private static bool IsHitWindow(Point3D a, Point3D b)
            {
                if (a.Z != b.Z)
                    return false;

                int dx = Math.Abs(a.X - b.X);
                int dy = Math.Abs(a.Y - b.Y);
                return dx <= 1 && dy == 1;
            }

            private static void EmoteBoth(string msg)
            {
                foreach (NetState ns in NetState.Instances)
                {
                    if (ns != null && ns.Mobile != null)
                        ns.Mobile.Emote(msg);
                }
            }

            private static void TryDismount(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                IMount mount = pm.Mount;
                if (mount != null)
                    mount.Rider = null;
            }

            private static bool HasJoustGear(PlayerMobile pm)
            {
                if (pm == null || pm.Mount == null)
                    return false;

                Item oneHand = pm.FindItemOnLayer(Layer.OneHanded);
                Item twoHand = pm.FindItemOnLayer(Layer.TwoHanded);
                Item lance = oneHand != null && oneHand.ItemID == 0x26CA ? oneHand : twoHand != null && twoHand.ItemID == 0x26CA ? twoHand : null;
                if (lance == null)
                    return false;

                return IsPlate(pm.FindItemOnLayer(Layer.Helm))
                    && IsPlate(pm.FindItemOnLayer(Layer.InnerTorso))
                    && IsPlate(pm.FindItemOnLayer(Layer.Arms))
                    && IsPlate(pm.FindItemOnLayer(Layer.Gloves))
                    && IsPlate(pm.FindItemOnLayer(Layer.Pants));
            }

            private static bool IsPlate(Item item)
            {
                if (item == null)
                    return false;

                string n = item.GetType().Name.ToLowerInvariant();
                return n.Contains("plate") || n.Contains("chaincoif") && n.Contains("plate");
            }

            private static void LoseArmorDurability(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                Item[] parts = new Item[]
                {
                    pm.FindItemOnLayer(Layer.Helm),
                    pm.FindItemOnLayer(Layer.InnerTorso),
                    pm.FindItemOnLayer(Layer.Arms),
                    pm.FindItemOnLayer(Layer.Gloves),
                    pm.FindItemOnLayer(Layer.Pants)
                };

                for (int i = 0; i < parts.Length; i++)
                {
                    BaseArmor ba = parts[i] as BaseArmor;
                    if (ba != null && ba.HitPoints > 0)
                        ba.HitPoints = Math.Max(0, ba.HitPoints - 1);
                }
            }

            private static void LoseLanceDurability(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                Item oneHand = pm.FindItemOnLayer(Layer.OneHanded);
                Item twoHand = pm.FindItemOnLayer(Layer.TwoHanded);
                BaseWeapon lance = oneHand as BaseWeapon;
                if (lance == null || lance.ItemID != 0x26CA)
                    lance = twoHand as BaseWeapon;

                if (lance == null || lance.ItemID != 0x26CA)
                    return;

                int pct = Utility.RandomMinMax(15, 20);
                int loss = Math.Max(1, (lance.MaxHitPoints * pct) / 100);
                lance.HitPoints = Math.Max(0, lance.HitPoints - loss);
            }

            private static void SendHitGump(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                pm.CloseGump(typeof(JoustHitGump));
                pm.SendGump(new JoustHitGump(pm));
            }

            private static void Release(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                pm.CantWalk = false;
                pm.CloseGump(typeof(JoustHitGump));
            }

            private class KnightTarget : Target
            {
                private readonly JoustSession m_Session;
                private readonly int m_Slot;
                private readonly PlayerMobile m_Host;

                public KnightTarget(JoustSession session, int slot, PlayerMobile host) : base(12, false, TargetFlags.None)
                {
                    m_Session = session;
                    m_Slot = slot;
                    m_Host = host;
                }

                protected override void OnTarget(Mobile from, object targeted)
                {
                    PlayerMobile pm = targeted as PlayerMobile;
                    if (pm == null)
                    {
                        from.SendMessage("Selecione um jogador.");
                        return;
                    }

                    if (m_Slot == 1)
                        m_Session.Knight1 = pm;
                    else
                        m_Session.Knight2 = pm;

                    pm.CantWalk = true;
                    from.SendMessage("Cavaleiro adicionado: {0}", pm.Name);
                    m_Host.SendGump(new ArenaJoustGump(m_Host, 0, m_Session.Key));
                }
            }
        }
        #endregion

        #region GLADIADORES
        public class GladiatorSession
        {
            public string Key;
            public bool Running;
            public bool Paused;
            public int Wave;
            public List<PlayerMobile> Fighters;
            public List<BaseCreature> Spawned;
            public Timer NextWave;

            public GladiatorSession(string key)
            {
                Key = key;
                Fighters = new List<PlayerMobile>();
                Spawned = new List<BaseCreature>();
            }

            public void AddFighter(PlayerMobile host)
            {
                host.Target = new FighterTarget(this, host);
                host.SendMessage("Selecione o lutador.");
            }

            public void Play(ReinoLotDefinition lot)
            {
                if (Running)
                    return;

                Running = true;
                Paused = false;
                Wave = 0;
                ScheduleNextWave(lot, TimeSpan.FromSeconds(30.0));
            }

            public void TogglePause(ReinoLotDefinition lot)
            {
                Paused = !Paused;
                if (!Paused && Running)
                    ScheduleNextWave(lot, TimeSpan.FromSeconds(30.0));
            }

            public void Stop(bool hard)
            {
                Running = false;
                Paused = false;
                Wave = 0;

                if (NextWave != null)
                {
                    NextWave.Stop();
                    NextWave = null;
                }

                for (int i = Spawned.Count - 1; i >= 0; i--)
                {
                    if (Spawned[i] != null && !Spawned[i].Deleted)
                        Spawned[i].Delete();
                }

                Spawned.Clear();
                Fighters.Clear();
            }

            private void ScheduleNextWave(ReinoLotDefinition lot, TimeSpan delay)
            {
                if (NextWave != null)
                    NextWave.Stop();

                NextWave = Timer.DelayCall(delay, delegate { DoWave(lot); });
            }

            private void DoWave(ReinoLotDefinition lot)
            {
                if (!Running || Paused || lot == null)
                    return;

                if (HasAliveSpawn())
                {
                    ScheduleNextWave(lot, TimeSpan.FromSeconds(5.0));
                    return;
                }

                Wave++;
                if (Wave > 5)
                {
                    Running = false;
                    return;
                }

                Type t = ArenaMobFactory.GetRandomTierType(Wave);
                Point3D[] spawns = ArenaSystem.GetGladiatorSpawnPoints(lot);

                for (int i = 0; i < spawns.Length; i++)
                {
                    BaseCreature bc = Activator.CreateInstance(t) as BaseCreature;
                    if (bc == null)
                        continue;

                    bc.MoveToWorld(spawns[i], lot.Map);
                    Spawned.Add(bc);
                }

                ScheduleNextWave(lot, TimeSpan.FromSeconds(30.0));
            }

            private bool HasAliveSpawn()
            {
                for (int i = Spawned.Count - 1; i >= 0; i--)
                {
                    BaseCreature bc = Spawned[i];
                    if (bc == null || bc.Deleted || bc.Hits <= 0)
                    {
                        Spawned.RemoveAt(i);
                        continue;
                    }

                    return true;
                }

                return false;
            }

            private class FighterTarget : Target
            {
                private readonly GladiatorSession m_Session;
                private readonly PlayerMobile m_Host;

                public FighterTarget(GladiatorSession session, PlayerMobile host) : base(12, false, TargetFlags.None)
                {
                    m_Session = session;
                    m_Host = host;
                }

                protected override void OnTarget(Mobile from, object targeted)
                {
                    PlayerMobile pm = targeted as PlayerMobile;
                    if (pm == null)
                    {
                        from.SendMessage("Selecione um jogador.");
                        return;
                    }

                    if (!m_Session.Fighters.Contains(pm) && m_Session.Fighters.Count < 3)
                        m_Session.Fighters.Add(pm);

                    m_Host.SendGump(new ArenaGladiatorGump(m_Host, 0, m_Session.Key));
                }
            }
        }
        #endregion

        #region BOMBERMAN
        public class BombermanSession
        {
            public string Key;
            public bool TeamMode = true;
            public bool Running;
            public List<PlayerMobile> Red = new List<PlayerMobile>();
            public List<PlayerMobile> Blue = new List<PlayerMobile>();
            public Dictionary<int, int> ActiveBombs = new Dictionary<int, int>();

            public BombermanSession(string key) { Key = key; }

            public void ToggleMode()
            {
                if (Running)
                    return;

                TeamMode = !TeamMode;
                Red.Clear();
                Blue.Clear();
            }

            public void AddSide(PlayerMobile host, bool red)
            {
                host.Target = new BombermanTarget(this, host, red);
                host.SendMessage("Selecione o jogador.");
            }

            public void Play(ReinoLotDefinition lot)
            {
                Running = true;
                List<PlayerMobile> all = new List<PlayerMobile>();
                all.AddRange(Red);
                all.AddRange(Blue);

                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] != null)
                        all[i].CantWalk = false;
                }
            }

            public void Stop(bool hard)
            {
                Running = false;
                ActiveBombs.Clear();

                List<PlayerMobile> all = new List<PlayerMobile>();
                all.AddRange(Red);
                all.AddRange(Blue);

                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] != null)
                        all[i].CantWalk = false;
                }

                Red.Clear();
                Blue.Clear();
            }

            private class BombermanTarget : Target
            {
                private readonly BombermanSession m_Session;
                private readonly PlayerMobile m_Host;
                private readonly bool m_Red;

                public BombermanTarget(BombermanSession session, PlayerMobile host, bool red) : base(12, false, TargetFlags.None)
                {
                    m_Session = session;
                    m_Host = host;
                    m_Red = red;
                }

                protected override void OnTarget(Mobile from, object targeted)
                {
                    PlayerMobile pm = targeted as PlayerMobile;
                    if (pm == null)
                        return;

                    if (m_Red)
                    {
                        if (!m_Session.Red.Contains(pm))
                            m_Session.Red.Add(pm);

                        pm.Hue = 33;
                    }
                    else
                    {
                        if (!m_Session.Blue.Contains(pm))
                            m_Session.Blue.Add(pm);

                        pm.Hue = 1152;
                    }

                    pm.CantWalk = true;
                    pm.CloseGump(typeof(ArenaBombermanPlayerGump));
                    pm.SendGump(new ArenaBombermanPlayerGump());
                    m_Host.SendGump(new ArenaBombermanGump(m_Host, 0, m_Session.Key));
                }
            }
        }

        public static bool HandleBombCommand(PlayerMobile from)
        {
            if (from == null || from.Map == null)
                return false;

            string key;
            int city;
            ArenaDefinition def;
            ReinoLotDefinition lot;

            if (!ArenaSystem.TryResolveArenaAt(from.Location, from.Map, out key, out city, out def, out lot))
                return false;

            BombermanSession session = GetOrCreateBomberman(key);
            if (!session.Running)
                return false;

            int count;
            session.ActiveBombs.TryGetValue(from.Serial.Value, out count);
            if (count >= 1)
            {
                from.SendMessage("Aguarde sua bomba atual explodir.");
                return true;
            }

            ArenaBombItem bomb = new ArenaBombItem(from, session);
            bomb.MoveToWorld(from.Location, from.Map);
            session.ActiveBombs[from.Serial.Value] = count + 1;
            return true;
        }
        #endregion
    }

    public static class ArenaMobFactory
    {
        private static readonly Type[][] Tiers = new Type[][]
        {
            new Type[] { typeof(ArenaT1Wolf), typeof(ArenaT1Boar), typeof(ArenaT1Panther), typeof(ArenaT1Hound), typeof(ArenaT1Bear) },
            new Type[] { typeof(ArenaT2DireWolf), typeof(ArenaT2Raptor), typeof(ArenaT2WarBoar), typeof(ArenaT2Lion), typeof(ArenaT2Bear) },
            new Type[] { typeof(ArenaT3SavageWolf), typeof(ArenaT3Tiger), typeof(ArenaT3Bull), typeof(ArenaT3Rhino), typeof(ArenaT3WarBear) },
            new Type[] { typeof(ArenaT4BruteWolf), typeof(ArenaT4DreadLion), typeof(ArenaT4HornedBeast), typeof(ArenaT4ClawRipper), typeof(ArenaT4DireBear) },
            new Type[] { typeof(ArenaT5NightReaver), typeof(ArenaT5DoomWolf), typeof(ArenaT5GoreKing), typeof(ArenaT5TitanBear), typeof(ArenaT5ExecutionerBeast) }
        };

        public static Type GetRandomTierType(int tier)
        {
            int idx = Math.Max(1, Math.Min(5, tier)) - 1;
            Type[] list = Tiers[idx];
            return list[Utility.Random(list.Length)];
        }
    }
}
