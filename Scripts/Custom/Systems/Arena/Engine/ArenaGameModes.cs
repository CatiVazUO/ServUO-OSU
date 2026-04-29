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
using Server.Custom.Systems.Animations;

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
            public Point3D Knight1Start;
            public Point3D Knight2Start;
            public readonly Dictionary<int, DateTime> LanceCooldown = new Dictionary<int, DateTime>();

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
                    msg = "Um ou ambos cavaleiros não estão com full plate + shield + montaria + lança 0x26CA.";
                    return false;
                }

                Knight1.CantWalk = true;
                Knight2.CantWalk = true;

                msg = "Equipamentos corretos. Evento pronto para começar.";
                return true;
            }

            public void Play(ReinoLotDefinition lot)
            {
                if (Knight1 == null || Knight2 == null)
                    return;

                Running = true;
                RoundOpen = true;
                PlaceKnightsAtLanes(lot, false);
                Knight1.CantWalk = false;
                Knight2.CantWalk = false;
            }

            public void Stop(bool hard)
            {
                Running = false;
                RoundOpen = false;
                LanceCooldown.Clear();

                Release(Knight1);
                Release(Knight2);
                Knight1 = null;
                Knight2 = null;
            }

            private static bool IsHitWindow(Point3D a, Point3D b)
            {
                if (a.Z != b.Z)
                    return false;

                int dx = a.X - b.X;
                int dy = a.Y - b.Y;
                return Math.Abs(dx) <= 1 && Math.Abs(dy) >= 1 && Math.Abs(dy) <= 3;
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

                Item shieldItem = pm.FindItemOnLayer(Layer.TwoHanded);
                if (!(shieldItem is BaseShield))
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

            private static void Release(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                pm.CantWalk = false;
                pm.CloseGump(typeof(JoustHitGump));
            }

            private bool IsMoving(PlayerMobile pm)
            {
                if (pm == null)
                    return false;

                return Core.TickCount - pm.LastMoveTime <= 500;
            }

            private static bool IsFacingTarget(PlayerMobile from, PlayerMobile target)
            {
                if (from == null || target == null)
                    return false;

                Direction toward = from.GetDirectionTo(target);
                return (from.Direction & Direction.Mask) == (toward & Direction.Mask);
            }

            private static bool IsHorizontalDirection(PlayerMobile attacker)
            {
                if (attacker == null)
                    return false;

                Direction dir = attacker.Direction & Direction.Mask;
                return dir == Direction.East || dir == Direction.West;
            }

            public bool TryLanca(PlayerMobile attacker)
            {
                if (!Running || !RoundOpen || attacker == null)
                    return false;

                PlayerMobile defender = attacker == Knight1 ? Knight2 : attacker == Knight2 ? Knight1 : null;
                if (defender == null)
                    return false;

                DateTime next;
                if (LanceCooldown.TryGetValue(attacker.Serial.Value, out next) && next > DateTime.UtcNow)
                {
                    attacker.SendMessage("Aguarde para usar [lanca novamente.");
                    return true;
                }

                LanceCooldown[attacker.Serial.Value] = DateTime.UtcNow + TimeSpan.FromSeconds(3.0);

                bool hit = IsHitWindow(attacker.Location, defender.Location)
                    && IsMoving(attacker)
                    && IsHorizontalDirection(attacker)
                    && !IsFacingTarget(attacker, defender);

                if (!hit)
                {
                    attacker.PlaySound(0x238);
                    attacker.SendMessage("Sua lança foi esquivada.");
                    RefreshJoustGump(attacker);
                    return true;
                }

                attacker.PlaySound(0x13B);
                attacker.Emote(String.Format(WinEmotes[Utility.Random(WinEmotes.Length)], attacker.Name, defender.Name));
                defender.Emote(String.Format(LoseEmotes[Utility.Random(LoseEmotes.Length)], defender.Name));
                TryDismount(defender);
                OSUKnockdownSystem.KnockDown(defender, TimeSpan.FromSeconds(2.0));
                int damage = 20;
                if (GetShield(defender) == null)
                    damage += 10;

                damage += (CountBrokenArmorPieces(defender) * 5);
                defender.Damage(damage);
                LoseArmorDurability(defender);
                LoseLanceDurability(attacker);

                RefreshJoustGump(attacker);
                RefreshJoustGump(defender);

                if (defender.Hits <= 0)
                {
                    EmoteBoth(attacker.Name + " venceu a justa por nocaute!");
                    Stop(false);
                }
                return true;
            }

            private void PlaceKnightsAtLanes(ReinoLotDefinition lot, bool keepLocked)
            {
                if (lot == null || Knight1 == null || Knight2 == null)
                    return;

                ArenaDefinition def = ArenaSystem.GetJoustDefinition(Key);
                Point3D off = def != null ? def.JoustKnight1Offset : new Point3D(5, 13, 0);
                int westX = lot.NorthWest.X + off.X;
                int topY = lot.NorthWest.Y + off.Y;
                int laneLen = 17;

                Knight1Start = new Point3D(westX, topY, lot.NorthWest.Z + off.Z);
                Knight2Start = new Point3D(westX + laneLen, topY + 2, lot.NorthWest.Z + off.Z);

                Knight1.MoveToWorld(Knight1Start, lot.Map);
                Knight2.MoveToWorld(Knight2Start, lot.Map);
                Knight1.Direction = Direction.East;
                Knight2.Direction = Direction.West;
                Knight1.CantWalk = keepLocked;
                Knight2.CantWalk = keepLocked;
                RefreshJoustGump(Knight1);
                RefreshJoustGump(Knight2);
            }

            private static BaseWeapon GetLance(PlayerMobile pm)
            {
                if (pm == null)
                    return null;

                Item oneHand = pm.FindItemOnLayer(Layer.OneHanded);
                Item twoHand = pm.FindItemOnLayer(Layer.TwoHanded);
                BaseWeapon w = oneHand as BaseWeapon;
                if (w != null && w.ItemID == 0x26CA)
                    return w;

                w = twoHand as BaseWeapon;
                if (w != null && w.ItemID == 0x26CA)
                    return w;

                return null;
            }

            private static BaseShield GetShield(PlayerMobile pm)
            {
                return pm != null ? pm.FindItemOnLayer(Layer.TwoHanded) as BaseShield : null;
            }

            private static int CountBrokenArmorPieces(PlayerMobile pm)
            {
                if (pm == null)
                    return 0;

                Layer[] layers = new Layer[] { Layer.Helm, Layer.Neck, Layer.InnerTorso, Layer.Arms, Layer.Gloves, Layer.Pants };
                int broken = 0;
                for (int i = 0; i < layers.Length; i++)
                {
                    BaseArmor ba = pm.FindItemOnLayer(layers[i]) as BaseArmor;
                    if (ba == null || ba.HitPoints <= 0)
                        broken++;
                }

                return broken;
            }

            private static int GetArmorDurability(PlayerMobile pm, bool max)
            {
                if (pm == null)
                    return 0;

                Layer[] layers = new Layer[] { Layer.Helm, Layer.Neck, Layer.InnerTorso, Layer.Arms, Layer.Gloves, Layer.Pants };
                int val = 0;
                for (int i = 0; i < layers.Length; i++)
                {
                    BaseArmor ba = pm.FindItemOnLayer(layers[i]) as BaseArmor;
                    if (ba == null)
                        continue;

                    val += max ? ba.MaxHitPoints : Math.Max(0, ba.HitPoints);
                }

                return val;
            }

            private static void RefreshJoustGump(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                BaseWeapon lance = GetLance(pm);
                BaseShield shield = GetShield(pm);
                int lanceCur = lance != null ? Math.Max(0, lance.HitPoints) : 0;
                int lanceMax = lance != null ? lance.MaxHitPoints : 0;
                int shieldCur = shield != null ? Math.Max(0, shield.HitPoints) : 0;
                int shieldMax = shield != null ? shield.MaxHitPoints : 0;
                int armorCur = GetArmorDurability(pm, false);
                int armorMax = GetArmorDurability(pm, true);

                pm.CloseGump(typeof(JoustHitGump));
                pm.SendGump(new JoustHitGump(lanceCur, lanceMax, shieldCur, shieldMax, armorCur, armorMax));
            }

            private class KnightTarget : Target
            {
                private readonly JoustSession m_Session;
                private readonly int m_Slot;
                private readonly PlayerMobile m_Host;

                public KnightTarget(JoustSession session, int slot, PlayerMobile host) : base(-1, false, TargetFlags.None)
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

                    Server.Custom.Reinos.ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_Session.Key);
                    if (m_Session.Knight1 != null && m_Session.Knight2 != null && lot != null)
                        m_Session.PlaceKnightsAtLanes(lot, true);

                    from.SendMessage("Cavaleiro adicionado: {0}", pm.Name);
                    int city = 0;
                    if (lot != null) city = lot.CityId;
                    m_Host.SendGump(new ArenaJoustGump(m_Host, city, m_Session.Key));
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
            public bool WaitingForWaveClear;
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
                WaitingForWaveClear = false;
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
                WaitingForWaveClear = false;

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

                if (WaitingForWaveClear)
                {
                    if (HasAliveSpawn())
                    {
                        ScheduleNextWave(lot, TimeSpan.FromSeconds(5.0));
                        return;
                    }

                    WaitingForWaveClear = false;
                    ScheduleNextWave(lot, TimeSpan.FromSeconds(30.0));
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

                WaitingForWaveClear = true;
                ScheduleNextWave(lot, TimeSpan.FromSeconds(5.0));
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

            private static void EnsureNotMounted(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                IMount mount = pm.Mount;
                if (mount != null)
                    mount.Rider = null;
            }

            private class FighterTarget : Target
            {
                private readonly GladiatorSession m_Session;
                private readonly PlayerMobile m_Host;

                public FighterTarget(GladiatorSession session, PlayerMobile host) : base(-1, false, TargetFlags.None)
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

                    EnsureNotMounted(pm);

                    if (!m_Session.Fighters.Contains(pm) && m_Session.Fighters.Count < 3)
                        m_Session.Fighters.Add(pm);

                    int city = 0;
                    Server.Custom.Reinos.ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_Session.Key);
                    if (lot != null) city = lot.CityId;
                    m_Host.SendGump(new ArenaGladiatorGump(m_Host, city, m_Session.Key));
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
            public List<Item> Walls = new List<Item>();
            public List<Item> Crates = new List<Item>();
            public List<Item> Bonuses = new List<Item>();
            public Dictionary<int, int> RangeBonus = new Dictionary<int, int>();
            public Dictionary<int, DateTime> MultiBombUntil = new Dictionary<int, DateTime>();
            public Dictionary<int, int> Falls = new Dictionary<int, int>();
            public Dictionary<int, DateTime> DownUntil = new Dictionary<int, DateTime>();
            public int MaxFalls = 3;
            public Dictionary<int, int> StorageBags = new Dictionary<int, int>();

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
                if (lot == null)
                    return;

                Running = true;
                SpawnGrid(lot);
                SpawnCratesAndBonuses(lot);

                List<PlayerMobile> all = new List<PlayerMobile>();
                all.AddRange(Red);
                all.AddRange(Blue);

                ArenaState state = ArenaSystem.GetState(Key);
                Item storageChest = ArenaSystem.GetBombermanStorage(state);

                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] == null)
                        continue;

                    EnsureNotMounted(all[i]);
                    MoveLoadoutToStorage(storageChest, all[i]);
                    EquipTeamVest(all[i], Red.Contains(all[i]));
                    PositionPlayer(lot, all[i], Red.Contains(all[i]));
                    all[i].CantWalk = false;
                }
            }

            public void Stop(bool hard)
            {
                Running = false;
                ActiveBombs.Clear();

                DeleteItems(Walls);
                DeleteItems(Crates);
                DeleteItems(Bonuses);

                List<PlayerMobile> all = new List<PlayerMobile>();
                all.AddRange(Red);
                all.AddRange(Blue);

                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] != null)
                    {
                        RemoveTeamVest(all[i]);
                        RestoreLoadout(all[i]);
                        all[i].CantWalk = false;
                        all[i].Blessed = false;
                    }
                }

                Red.Clear();
                Blue.Clear();
                RangeBonus.Clear();
                MultiBombUntil.Clear();
                Falls.Clear();
                DownUntil.Clear();
                StorageBags.Clear();
            }



            private void PositionPlayer(ReinoLotDefinition lot, PlayerMobile pm, bool red)
            {
                if (pm == null || lot == null)
                    return;

                Point3D loc;
                ArenaDefinition def = ArenaSystem.GetJoustDefinition(Key);
                if (TeamMode)
                {
                    Point3D[] redSpawns = def != null ? def.BombermanRedSpawnOffsets : null;
                    Point3D[] blueSpawns = def != null ? def.BombermanBlueSpawnOffsets : null;
                    int rIdx = Red.IndexOf(pm);
                    int bIdx = Blue.IndexOf(pm);

                    Point3D rDef = new Point3D(2, rIdx == 0 ? 2 : 27, 0);
                    Point3D bDef = new Point3D(27, bIdx == 0 ? 2 : 27, 0);

                    Point3D ro = redSpawns != null && redSpawns.Length > 0
                        ? redSpawns[Math.Max(0, Math.Min(redSpawns.Length - 1, rIdx))]
                        : rDef;
                    Point3D bo = blueSpawns != null && blueSpawns.Length > 0
                        ? blueSpawns[Math.Max(0, Math.Min(blueSpawns.Length - 1, bIdx))]
                        : bDef;

                    loc = red
                        ? new Point3D(lot.NorthWest.X + ro.X, lot.NorthWest.Y + ro.Y, lot.NorthWest.Z + ro.Z)
                        : new Point3D(lot.NorthWest.X + bo.X, lot.NorthWest.Y + bo.Y, lot.NorthWest.Z + bo.Z);
                }
                else
                {
                    loc = red
                        ? new Point3D(lot.NorthWest.X + 2, lot.NorthWest.Y + 2, lot.NorthWest.Z)
                        : new Point3D(lot.NorthWest.X + 27, lot.NorthWest.Y + 27, lot.NorthWest.Z);
                }

                pm.MoveToWorld(loc, lot.Map);
            }

            private void SpawnGrid(ReinoLotDefinition lot)
            {
                DeleteItems(Walls);

                ArenaDefinition def = ArenaSystem.GetJoustDefinition(Key);
                int startX = def.BombermanGridStartX;
                int startY = def.BombermanGridStartY;
                int width = Math.Max(5, Math.Min(20, def.BombermanGridWidth));
                int height = Math.Max(5, Math.Min(20, def.BombermanGridHeight));

                for (int x = startX; x < startX + width; x++)
                {
                    for (int y = startY; y < startY + height; y++)
                    {
                        if ((x % 2 == 0) && (y % 2 == 0))
                        {
                            ArenaWallItem wall = new ArenaWallItem();
                            wall.MoveToWorld(new Point3D(lot.NorthWest.X + x, lot.NorthWest.Y + y, lot.NorthWest.Z), lot.Map);
                            Walls.Add(wall);
                        }
                    }
                }
            }

            private void SpawnCratesAndBonuses(ReinoLotDefinition lot)
            {
                DeleteItems(Crates);
                DeleteItems(Bonuses);

                ArenaDefinition defDim = ArenaSystem.GetJoustDefinition(Key);
                int startX = defDim.BombermanGridStartX;
                int startY = defDim.BombermanGridStartY;
                int width = Math.Max(5, Math.Min(20, defDim.BombermanGridWidth));
                int height = Math.Max(5, Math.Min(20, defDim.BombermanGridHeight));
                int maxX = startX + width - 1;
                int maxY = startY + height - 1;

                HashSet<string> blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                ArenaDefinition def = ArenaSystem.GetJoustDefinition(Key);
                AddBlockedOffsets(blocked, def != null ? def.BombermanRedSpawnOffsets : null);
                AddBlockedOffsets(blocked, def != null ? def.BombermanBlueSpawnOffsets : null);
                if (blocked.Count == 0)
                {
                    blocked.Add(startX + ":" + startY);
                    blocked.Add(startX + ":" + maxY);
                    blocked.Add(maxX + ":" + startY);
                    blocked.Add(maxX + ":" + maxY);
                }

                int tries = 0;
                while (Crates.Count < 20 && tries < 300)
                {
                    tries++;
                    int x = Utility.RandomMinMax(startX, maxX);
                    int y = Utility.RandomMinMax(startY, maxY);
                    string k = x + ":" + y;
                    if (blocked.Contains(k))
                        continue;

                    Point3D loc = new Point3D(lot.NorthWest.X + x, lot.NorthWest.Y + y, lot.NorthWest.Z);
                    if (HasBlockAt(loc, lot.Map))
                        continue;

                    ArenaCrateItem c = new ArenaCrateItem();
                    c.MoveToWorld(loc, lot.Map);
                    Crates.Add(c);
                }

                for (int i = 0; i < 12; i++)
                {
                    int x = Utility.RandomMinMax(startX, maxX);
                    int y = Utility.RandomMinMax(startY, maxY);
                    Point3D loc = new Point3D(lot.NorthWest.X + x, lot.NorthWest.Y + y, lot.NorthWest.Z);
                    if (HasBlockAt(loc, lot.Map))
                        continue;

                    Item b;
                    int r = Utility.Random(100);
                    if (r < 45) b = new ArenaMoveBonusItem();
                    else if (r < 80) b = new ArenaMultiBombBonusItem();
                    else b = new ArenaRangeBonusItem();

                    b.MoveToWorld(loc, lot.Map);
                    Bonuses.Add(b);
                }
            }

            private static void AddBlockedOffsets(HashSet<string> blocked, Point3D[] offsets)
            {
                if (blocked == null || offsets == null)
                    return;

                for (int i = 0; i < offsets.Length; i++)
                {
                    Point3D p = offsets[i];
                    blocked.Add(p.X + ":" + p.Y);
                }
            }

            private static void EnsureNotMounted(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                IMount mount = pm.Mount;
                if (mount != null)
                    mount.Rider = null;
            }

            private static void EquipTeamVest(PlayerMobile pm, bool red)
            {
                if (pm == null)
                    return;

                RemoveTeamVest(pm);
                pm.AddItem(new ArenaBombermanTeamVest(red));
            }

            private static void RemoveTeamVest(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                Item vest = pm.FindItemOnLayer(Layer.OuterTorso);
                if (vest is ArenaBombermanTeamVest)
                    vest.Delete();
            }

            private static bool HasBlockAt(Point3D loc, Map map)
            {
                IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
                foreach (Item item in eable)
                {
                    if (item == null || item.Deleted)
                        continue;

                    if (item.ItemID == 0x071E || item.ItemID == 0x0E3C)
                    {
                        eable.Free();
                        return true;
                    }
                }

                eable.Free();
                return false;
            }

            private static void DeleteItems(List<Item> items)
            {
                if (items == null)
                    return;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i] != null && !items[i].Deleted)
                        items[i].Delete();
                }

                items.Clear();
            }


            public int GetRange(PlayerMobile pm)
            {
                if (pm == null)
                    return 1;

                int add;
                RangeBonus.TryGetValue(pm.Serial.Value, out add);
                return Math.Max(1, Math.Min(4, 1 + add));
            }

            public int GetMaxBombs(PlayerMobile pm)
            {
                if (pm == null)
                    return 1;

                DateTime until;
                if (MultiBombUntil.TryGetValue(pm.Serial.Value, out until) && until > DateTime.UtcNow)
                    return 2;

                return 1;
            }


            public bool IsParticipant(PlayerMobile pm)
            {
                if (pm == null)
                    return false;

                return Red.Contains(pm) || Blue.Contains(pm);
            }


            public bool IsDown(PlayerMobile pm)
            {
                if (pm == null)
                    return false;

                DateTime until;
                if (!DownUntil.TryGetValue(pm.Serial.Value, out until))
                    return false;

                if (until <= DateTime.UtcNow)
                {
                    DownUntil.Remove(pm.Serial.Value);
                    return false;
                }

                return true;
            }

            public void NotifyPlayerHit(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                DateTime until;
                if (DownUntil.TryGetValue(pm.Serial.Value, out until) && until > DateTime.UtcNow)
                    return;

                int falls;
                Falls.TryGetValue(pm.Serial.Value, out falls);
                falls++;
                Falls[pm.Serial.Value] = falls;

                DownUntil[pm.Serial.Value] = DateTime.UtcNow + TimeSpan.FromSeconds(30.0);
                pm.Emote("*cai no chão*");
                pm.CantWalk = true;
                pm.Blessed = true;
                Timer.DelayCall(TimeSpan.FromSeconds(20.0), delegate { if (pm != null && !pm.Deleted) pm.Blessed = false; });
                Timer.DelayCall(TimeSpan.FromSeconds(30.0), delegate { if (pm != null && !pm.Deleted) pm.CantWalk = false; });

                CheckVictory();
            }

            public void CheckVictory()
            {
                if (!Running)
                    return;

                if (TeamMode)
                {
                    bool redStanding = HasStanding(Red);
                    bool blueStanding = HasStanding(Blue);

                    if (!redStanding && blueStanding)
                    {
                        Announce("Time Azul venceu o bomberman!");
                        Stop(false);
                    }
                    else if (!blueStanding && redStanding)
                    {
                        Announce("Time Vermelho venceu o bomberman!");
                        Stop(false);
                    }
                }
                else
                {
                    List<PlayerMobile> players = new List<PlayerMobile>();
                    players.AddRange(Red);
                    players.AddRange(Blue);

                    int alive = 0;
                    PlayerMobile last = null;
                    for (int i = 0; i < players.Count; i++)
                    {
                        PlayerMobile p = players[i];
                        if (p == null)
                            continue;

                        int falls;
                        Falls.TryGetValue(p.Serial.Value, out falls);
                        if (falls < MaxFalls)
                        {
                            alive++;
                            last = p;
                        }
                    }

                    if (alive <= 1)
                    {
                        if (last != null)
                            Announce(last.Name + " venceu o bomberman individual!");
                        else
                            Announce("Partida encerrada.");

                        Stop(false);
                    }
                }
            }

            private bool HasStanding(List<PlayerMobile> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    PlayerMobile p = list[i];
                    if (p == null)
                        continue;

                    if (!IsDown(p))
                        return true;
                }

                return false;
            }

            private static void Announce(string msg)
            {
                foreach (NetState ns in NetState.Instances)
                {
                    if (ns != null && ns.Mobile != null)
                        ns.Mobile.SendMessage(msg);
                }
            }

            public void ApplyBonus(PlayerMobile pm, int type)
            {
                if (pm == null)
                    return;

                if (type == 1)
                {
                    bool up = Utility.RandomBool();
                    pm.SendMessage(up ? "Movimento ++ por 10s" : "Movimento -- por 10s");
                }
                else if (type == 2)
                {
                    MultiBombUntil[pm.Serial.Value] = DateTime.UtcNow + TimeSpan.FromSeconds(15.0);
                    pm.SendMessage("Bombas: 2 por 15s.");
                }
                else if (type == 3)
                {
                    int val;
                    RangeBonus.TryGetValue(pm.Serial.Value, out val);
                    RangeBonus[pm.Serial.Value] = Math.Min(3, val + 1);
                    pm.SendMessage("Alcance permanente +1.");
                }
            }

            private void MoveLoadoutToStorage(Item storageChest, PlayerMobile pm)
            {
                if (pm == null || storageChest == null || !(storageChest is Container))
                    return;

                Container chest = (Container)storageChest;
                Bag bag = new Bag();
                bag.Name = "Equipamentos de " + pm.Name;
                chest.DropItem(bag);
                StorageBags[pm.Serial.Value] = bag.Serial.Value;

                if (pm.Backpack != null)
                {
                    List<Item> packItems = new List<Item>(pm.Backpack.Items);
                    for (int i = 0; i < packItems.Count; i++)
                    {
                        Item it = packItems[i];
                        if (it == null || it.Deleted)
                            continue;

                        bag.DropItem(it);
                    }
                }

                for (int layer = 0; layer < 32; layer++)
                {
                    Layer l = (Layer)layer;
                    if (l == Layer.Backpack || l == Layer.Bank)
                        continue;

                    Item equipped = pm.FindItemOnLayer(l);
                    if (equipped == null || equipped.Deleted)
                        continue;

                    bag.DropItem(equipped);
                }
            }

            private void RestoreLoadout(PlayerMobile pm)
            {
                if (pm == null)
                    return;

                int bagSerial;
                if (!StorageBags.TryGetValue(pm.Serial.Value, out bagSerial))
                    return;

                Container bag = World.FindItem((Serial)bagSerial) as Container;
                if (bag == null || bag.Deleted)
                    return;

                List<Item> items = new List<Item>(bag.Items);
                for (int i = 0; i < items.Count; i++)
                {
                    Item it = items[i];
                    if (it == null || it.Deleted)
                        continue;

                    if (pm.Backpack != null)
                        pm.Backpack.DropItem(it);
                    else
                        pm.AddToBackpack(it);
                }

                bag.Delete();
            }

            private class BombermanTarget : Target
            {
                private readonly BombermanSession m_Session;
                private readonly PlayerMobile m_Host;
                private readonly bool m_Red;

                public BombermanTarget(BombermanSession session, PlayerMobile host, bool red) : base(-1, false, TargetFlags.None)
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

                    EnsureNotMounted(pm);

                    if (m_Red)
                    {
                        if (!m_Session.Red.Contains(pm))
                            m_Session.Red.Add(pm);
                    }
                    else
                    {
                        if (!m_Session.Blue.Contains(pm))
                            m_Session.Blue.Add(pm);
                    }

                    pm.CantWalk = true;
                    pm.CloseGump(typeof(ArenaBombermanPlayerGump));
                    pm.SendGump(new ArenaBombermanPlayerGump());
                    int city = 0;
                    Server.Custom.Reinos.ReinoLotDefinition lot = ArenaSystem.GetLotFromConstructionKey(m_Session.Key);
                    if (lot != null) city = lot.CityId;
                    m_Host.SendGump(new ArenaBombermanGump(m_Host, city, m_Session.Key));
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

            if (!session.IsParticipant(from))
                return false;

            int count;
            session.ActiveBombs.TryGetValue(from.Serial.Value, out count);
            int max = session.GetMaxBombs(from);
            if (count >= max)
            {
                from.SendMessage("Você atingiu o limite de bombas ativas.");
                return true;
            }

            IPooledEnumerable itemsAt = from.Map.GetItemsInRange(from.Location, 0);
            foreach (Item it in itemsAt)
            {
                if (it == null || it.Deleted)
                    continue;

                if (it.ItemID == 0x071E || it.ItemID == 0x0E3C || it.ItemID == 0xA5B4)
                {
                    itemsAt.Free();
                    from.SendMessage("Não dá para colocar bomba nesse tile.");
                    return true;
                }
            }
            itemsAt.Free();

            ArenaBombItem bomb = new ArenaBombItem(from, session, session.GetRange(from));
            bomb.MoveToWorld(from.Location, from.Map);
            session.ActiveBombs[from.Serial.Value] = count + 1;
            return true;
        }

        public static bool HandleLancaCommand(PlayerMobile from)
        {
            if (from == null || from.Map == null)
                return false;

            string key;
            int city;
            ArenaDefinition def;
            ReinoLotDefinition lot;

            if (!ArenaSystem.TryResolveArenaAt(from.Location, from.Map, out key, out city, out def, out lot))
                return false;

            JoustSession session = GetOrCreateJoust(key);
            return session.TryLanca(from);
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
