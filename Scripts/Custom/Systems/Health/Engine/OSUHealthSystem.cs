
using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Reinos;
using Server.Custom.Systems.DefQual;
using Server.Custom.Systems.Health.Gumps;
using Server.Custom.OSUDrag;

namespace Server.Custom.Systems.Health
{
    public static class OSUHealthSystem
    {
        private static readonly object _sync = new object();
        private static readonly Dictionary<int, OSUHealthProfile> _profiles = new Dictionary<int, OSUHealthProfile>();
        private static readonly Dictionary<int, OSUContaminatedItemState> _contaminatedItems = new Dictionary<int, OSUContaminatedItemState>();
        private static readonly Dictionary<int, OSUSurgeryProgressState> _surgery = new Dictionary<int, OSUSurgeryProgressState>();
        private static readonly Dictionary<int, int> _weeklyHospitalSurgeries = new Dictionary<int, int>();
        private static DateTime _surgeryWeekStartUtc = DateTime.MinValue;

        private static string FilePath
        {
            get { return Path.Combine(Core.BaseDirectory, "Saves", "OSU_HealthSystem.bin"); }
        }

        public static void Initialize()
        {
            EventSink.WorldLoad += OnWorldLoad;
            EventSink.WorldSave += OnWorldSave;
            EventSink.Movement += OnMovement;
            EventSink.Disconnected += OnDisconnected;
            EventSink.Login += OnLogin;
            EventSink.Speech += OnSpeech;

            CommandSystem.Register("DarDoenca", AccessLevel.GameMaster, OnGiveDiseaseCommand);
            CommandSystem.Register("VerDoencas", AccessLevel.GameMaster, OnViewDiseasesCommand);
            CommandSystem.Register("ResetarSaude", AccessLevel.Owner, OnResetHealthCommand);
            CommandSystem.Register("ApagarSaude", AccessLevel.Owner, OnWipeHealthCommand);
            CommandSystem.Register("DarLesao", AccessLevel.GameMaster, OnGiveInjuryCommand);
            CommandSystem.Register("VerInfoSaude", AccessLevel.GameMaster, OnViewHealthInfoCommand);
            CommandSystem.Register("AbrirSaude", AccessLevel.GameMaster, OnOpenHealthGumpCommand);
            CommandSystem.Register("AbrirCirurgia", AccessLevel.GameMaster, OnOpenSurgeryGumpCommand);

            _surgeryWeekStartUtc = GetWeekStartUtc(DateTime.UtcNow);
            new OSUHealthPulseTimer().Start();
            new OSUSurgeryGumpRefreshTimer().Start();
            new OSUHealthAutosaveTimer().Start();
        }

        #region Definitions

        public class InjuryDefinition
        {
            public OSUInjuryType Type;
            public string Name;
            public string AppliedMessage;
            public string ZoneHitMessage;
            public OSUBodyZone Zone;
            public OSUInjurySeverity Severity;
            public bool RequiresSurgery;
            public bool HealsOnHospitalStretcher;
            public TimeSpan BaseDuration;
        }

        public class DiseaseDefinition
        {
            public OSUDiseaseType Type;
            public string Name;
            public string DiagnoseText;
            public string ContractedMessage;
            public TimeSpan Incubation;
            public TimeSpan Virulence;
            public int RecoveryTarget;
            public bool HealsOverTime;
        }

        private static readonly Dictionary<OSUInjuryType, InjuryDefinition> m_Injuries = BuildInjuries();
        private static readonly Dictionary<OSUDiseaseType, DiseaseDefinition> m_Diseases = BuildDiseases();

        public const int FeatExameMedico = 110101;
        public const int FeatCirurgia = 110102;

        private static Dictionary<OSUInjuryType, InjuryDefinition> BuildInjuries()
        {
            Dictionary<OSUInjuryType, InjuryDefinition> d = new Dictionary<OSUInjuryType, InjuryDefinition>();

            AddInjury(d, OSUInjuryType.Winded, "Sem Fôlego", "Você está sem fôlego.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Light, false, true, TimeSpan.FromHours(1));

            AddInjury(d, OSUInjuryType.Bruised, "Roxo", "Você está com hematomas.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Light, false, true, TimeSpan.FromHours(4));

            AddInjury(d, OSUInjuryType.MinorCut, "Cortes Pequenos", "Você tem alguns poucos cortes.",
                "Você foi acertado nos braços.", OSUBodyZone.Arms, OSUInjurySeverity.Light, false, true, TimeSpan.FromHours(6));

            AddInjury(d, OSUInjuryType.MinorConcussion, "Contusão Leve", "Você sofreu uma contusão leve.",
                "Você foi acertado na cabeça.", OSUBodyZone.Head, OSUInjurySeverity.Light, false, true, TimeSpan.FromHours(6));

            AddInjury(d, OSUInjuryType.Bloodied, "Sangrando", "Você está sangrando.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Light, false, true, TimeSpan.FromHours(7));

            AddInjury(d, OSUInjuryType.Exhausted, "Exausto", "Você está exausto.",
                "Você foi acertado nas costas.", OSUBodyZone.Back, OSUInjurySeverity.Light, false, true, TimeSpan.FromHours(3));

            AddInjury(d, OSUInjuryType.MajorConcussion, "Contusão Grave", "Você está com uma enorme contusão.",
                "Você foi acertado na cabeça.", OSUBodyZone.Head, OSUInjurySeverity.Moderate, false, true, TimeSpan.FromHours(8));

            AddInjury(d, OSUInjuryType.FracturedLeftArm, "Braço Esquerdo Fraturado", "Seu braço esquerdo está fraturado.",
                "Você foi acertado nos braços.", OSUBodyZone.Arms, OSUInjurySeverity.Moderate, true, true, TimeSpan.FromHours(22));

            AddInjury(d, OSUInjuryType.FracturedRightArm, "Braço Direito Fraturado", "Seu braço direito está fraturado.",
                "Você foi acertado nos braços.", OSUBodyZone.Arms, OSUInjurySeverity.Moderate, true, true, TimeSpan.FromHours(22));

            AddInjury(d, OSUInjuryType.FracturedLeftLeg, "Perna Esquerda Fraturada", "Sua perna esquerda está fraturada.",
                "Você foi acertado nas pernas.", OSUBodyZone.Legs, OSUInjurySeverity.Moderate, true, true, TimeSpan.FromHours(28));

            AddInjury(d, OSUInjuryType.FracturedRightLeg, "Perna Direita Fraturada", "Sua perna direita está fraturada.",
                "Você foi acertado nas pernas.", OSUBodyZone.Legs, OSUInjurySeverity.Moderate, true, true, TimeSpan.FromHours(28));

            AddInjury(d, OSUInjuryType.FracturedRibs, "Costela Fraturada", "Sua costela está fraturada.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Moderate, true, true, TimeSpan.FromHours(30));

            AddInjury(d, OSUInjuryType.FracturedSkull, "Crânio Fraturado", "Seu crânio está fraturado.",
                "Você foi acertado na cabeça.", OSUBodyZone.Head, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(40));

            AddInjury(d, OSUInjuryType.DeepCut, "Corte Profundo", "Você está com cortes profundos.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Moderate, true, false, TimeSpan.FromHours(35));

            AddInjury(d, OSUInjuryType.InternalBleeding, "Sangramento Interno", "Você está sangrando internamente.",
                "Você foi acertado nas costas.", OSUBodyZone.Back, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(24));

            AddInjury(d, OSUInjuryType.LaceratedTorso, "Lacerações", "Seu torso está lacerado.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(28));

            AddInjury(d, OSUInjuryType.BrokenLeftArm, "Braço Esquerdo Quebrado", "Seu braço esquerdo está quebrado.",
                "Você foi acertado nos braços.", OSUBodyZone.Arms, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(44));

            AddInjury(d, OSUInjuryType.BrokenRightArm, "Braço Direito Quebrado", "Seu braço direito está quebrado.",
                "Você foi acertado nos braços.", OSUBodyZone.Arms, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(34));

            AddInjury(d, OSUInjuryType.BrokenLeftLeg, "Perna Esquerda Quebrada", "Sua perna esquerda está quebrada.",
                "Você foi acertado nas pernas.", OSUBodyZone.Legs, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(46));

            AddInjury(d, OSUInjuryType.BrokenRightLeg, "Perna Direita Quebrada", "Sua perna direita está quebrada.",
                "Você foi acertado nas pernas.", OSUBodyZone.Legs, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(46));

            AddInjury(d, OSUInjuryType.BrokenJaw, "Mandíbula Quebrada", "Sua mandíbula está quebrada.",
                "Você foi acertado na cabeça.", OSUBodyZone.Head, OSUInjurySeverity.Severe, true, false, TimeSpan.FromHours(46));

            AddInjury(d, OSUInjuryType.ChestTrauma, "Traumatismo Torácico Grave", "Seu tórax sofreu um traumatismo grave.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Deadly, true, false, TimeSpan.FromHours(70));

            AddInjury(d, OSUInjuryType.RupturedSpleen, "Ruptura do Baço", "Seu baço foi gravemente lesionado.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Deadly, true, false, TimeSpan.FromHours(70));

            AddInjury(d, OSUInjuryType.BrokenSkull, "Trauma Craniano Grave", "Você sofreu um trauma craniano grave.",
                "Você foi acertado na cabeça.", OSUBodyZone.Head, OSUInjurySeverity.Deadly, true, false, TimeSpan.FromHours(70));

            AddInjury(d, OSUInjuryType.MassiveBleeding, "Hemorragia Interna Extensa", "Você sofre uma hemorragia interna extensa.",
                "Você foi acertado no torso.", OSUBodyZone.Torso, OSUInjurySeverity.Deadly, true, false, TimeSpan.FromHours(70));


            return d;
        }

        private static Dictionary<OSUDiseaseType, DiseaseDefinition> BuildDiseases()
        {
            Dictionary<OSUDiseaseType, DiseaseDefinition> d = new Dictionary<OSUDiseaseType, DiseaseDefinition>();

            AddDisease(d, OSUDiseaseType.Influenza, "Influenza", "O paciente sofre de febre, suor frio, congestão nasal, vômito ocasional e delírio.",
                "Você se sente esquentado...", RandomBetweenMinutes(15, 25), RandomBetweenMinutes(25, 65), 20, true);

            AddDisease(d, OSUDiseaseType.HundredDaysCough, "Coqueluche", "O paciente sofre de acessos de tosse violentos e prolongados e tem dificuldade de respirar.",
                "Sua garganta começa a incomodar...", RandomBetweenMinutes(15, 25), RandomBetweenMinutes(15, 25), 200, true);

            AddDisease(d, OSUDiseaseType.Diptheria, "Difteria", "O paciente sofre de tosse crônica, dor de garganta e rosto e pescoço inchado.",
                "Está difícil respirar...", RandomBetweenMinutes(20, 45), RandomBetweenMinutes(40, 60), 20, true);

            AddDisease(d, OSUDiseaseType.Dysentery, "Disenteria", "O paciente sofre de cólicas abdominais, vômito e desidratação.",
                "Sua barriga dói...", RandomBetweenMinutes(40, 60), RandomBetweenMinutes(10, 60), 40, true);

            AddDisease(d, OSUDiseaseType.Consumption, "Tuberculose", "O paciente sofre de crises de tosse prolongadas, dificuldade de respiração e sangue ao expelir fluidos.",
                "Você se sente fatigado...", RandomBetweenMinutes(50, 70), RandomBetweenMinutes(20, 50), 150, false);

            AddDisease(d, OSUDiseaseType.WesternFever, "Febre Amarela", "O paciente sofre de tons amarelados na pele e olhos, suor intenso, delírio, fraqueza e dor nas juntas.",
                "Você se sente quente...", RandomBetweenMinutes(50, 70), RandomBetweenMinutes(30, 50), 60, true);

            AddDisease(d, OSUDiseaseType.Bile, "Cólera", "O paciente sofre de vômitos frequentes, diarreia, desidratação e mal nutrição.",
                "Você sente náuseas fortes...", RandomBetweenMinutes(15, 45), RandomBetweenMinutes(100, 200), 80, true);

            AddDisease(d, OSUDiseaseType.Leprosy, "Lepra", "O paciente sofre de caroços e infecções cutâneas aparentes e tem pouca reação à dor.",
                "Você está dormente...", RandomBetweenMinutes(15, 30), RandomBetweenMinutes(300, 500), 250, false);

            AddDisease(d, OSUDiseaseType.LoveDisease, "Sífilis", "O paciente sofre de lesões indolores nas mãos, rosto e outras áreas sensíveis, além de estar debilitado.",
                "Você se sente tonto...", RandomBetweenMinutes(100, 200), RandomBetweenMinutes(200, 400), 250, false);


            return d;
        }

        private static void AddInjury(Dictionary<OSUInjuryType, InjuryDefinition> map, OSUInjuryType type, string name, string applied, string zoneHit, OSUBodyZone zone, OSUInjurySeverity severity, bool surgery, bool healsOnStretcher, TimeSpan duration)
        {
            map[type] = new InjuryDefinition
            {
                Type = type,
                Name = name,
                AppliedMessage = applied,
                ZoneHitMessage = zoneHit,
                Zone = zone,
                Severity = severity,
                RequiresSurgery = surgery,
                HealsOnHospitalStretcher = healsOnStretcher,
                BaseDuration = duration
            };
        }

        private static void AddDisease(Dictionary<OSUDiseaseType, DiseaseDefinition> map, OSUDiseaseType type, string name, string diag, string contracted, TimeSpan incubation, TimeSpan virulence, int recoveryTarget, bool healsOverTime)
        {
            map[type] = new DiseaseDefinition
            {
                Type = type,
                Name = name,
                DiagnoseText = diag,
                ContractedMessage = contracted,
                Incubation = incubation,
                Virulence = virulence,
                RecoveryTarget = recoveryTarget,
                HealsOverTime = healsOverTime
            };
        }

        private static TimeSpan RandomBetweenMinutes(int min, int max)
        {
            return TimeSpan.FromMinutes(Utility.RandomMinMax(min, max));
        }

        public static InjuryDefinition GetInjuryDefinition(OSUInjuryType type)
        {
            InjuryDefinition def;
            m_Injuries.TryGetValue(type, out def);
            return def;
        }

        public static DiseaseDefinition GetDiseaseDefinition(OSUDiseaseType type)
        {
            DiseaseDefinition def;
            m_Diseases.TryGetValue(type, out def);
            return def;
        }

        #endregion

        #region Public UI Helpers

        public static void OpenHealthStatusGump(Mobile viewer, PlayerMobile target)
        {
            if (viewer == null || target == null)
                return;

            viewer.CloseGump(typeof(OSUHealthStatusGump));
            viewer.SendGump(new OSUHealthStatusGump(viewer, target));
        }

        public static void OpenSurgeryStatusGump(Mobile viewer, PlayerMobile patient)
        {
            if (viewer == null || patient == null)
                return;

            OSUSurgeryProgressState state = GetSurgeryState(patient);
            viewer.CloseGump(typeof(OSUSurgeryStatusGump));
            viewer.SendGump(new OSUSurgeryStatusGump(viewer, patient, state));
        }

        public static OSUSurgeryProgressState GetSurgeryState(PlayerMobile patient)
        {
            if (patient == null)
                return null;

            lock (_sync)
            {
                OSUSurgeryProgressState state;
                _surgery.TryGetValue(patient.Serial.Value, out state);
                return state;
            }
        }

        public static List<OSUInjuryState> GetInjuries(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            return profile != null ? profile.Injuries : new List<OSUInjuryState>();
        }

        public static List<OSUDiseaseState> GetDiseases(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            return profile != null ? profile.Diseases : new List<OSUDiseaseState>();
        }

        public static List<OSUImmunityState> GetImmunities(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            return profile != null ? profile.Immunities : new List<OSUImmunityState>();
        }

        #endregion

        #region Profiles

        public static OSUHealthProfile GetProfile(Mobile m, bool create)
        {
            if (m == null || m.Deleted)
                return null;

            int key = m.Serial.Value;

            lock (_sync)
            {
                OSUHealthProfile profile;
                if (_profiles.TryGetValue(key, out profile))
                    return profile;

                if (!create)
                    return null;

                profile = new OSUHealthProfile();
                profile.MobileSerial = key;
                _profiles[key] = profile;
                return profile;
            }
        }

        public static OSUHealthProfile GetProfileBySerial(int serial)
        {
            lock (_sync)
            {
                OSUHealthProfile profile;
                _profiles.TryGetValue(serial, out profile);
                return profile;
            }
        }

        public static void RemoveProfile(int serial)
        {
            lock (_sync)
            {
                _profiles.Remove(serial);
            }
        }

        #endregion

        #region Manual Hook Entry

        public static void OnPlayerKnockoutLostLife(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            double roll = Utility.RandomDouble();
            double chance = 0.70 * GetInjurySusceptibility(pm);

            if (roll > chance)
                return;

            OSUInjuryType chosen = ChooseKnockoutInjury(pm);

            if (chosen == OSUInjuryType.None)
                return;

            ApplyInjury(pm, chosen, true);

            InjuryDefinition def = GetInjuryDefinition(chosen);
            if (def != null && !String.IsNullOrWhiteSpace(def.ZoneHitMessage))
                pm.SendMessage(0x35, def.ZoneHitMessage);
        }

        private static OSUInjuryType ChooseKnockoutInjury(PlayerMobile pm)
        {
            if (pm == null)
                return OSUInjuryType.None;

            List<OSUInjuryType> bag = new List<OSUInjuryType>();
            int remainingLives = Math.Max(0, pm.OSULives);

            if (remainingLives >= 2)
            {
                AddBag(bag, OSUInjuryType.Winded, 14);
                AddBag(bag, OSUInjuryType.Bruised, 14);
                AddBag(bag, OSUInjuryType.MinorCut, 12);
                AddBag(bag, OSUInjuryType.MinorConcussion, 10);
                AddBag(bag, OSUInjuryType.Bloodied, 10);
                AddBag(bag, OSUInjuryType.Exhausted, 10);
                AddBag(bag, OSUInjuryType.MajorConcussion, 4);
                AddBag(bag, OSUInjuryType.FracturedRibs, 3);
                AddBag(bag, OSUInjuryType.DeepCut, 2);
            }
            else if (remainingLives == 1)
            {
                AddBag(bag, OSUInjuryType.Bruised, 8);
                AddBag(bag, OSUInjuryType.MinorConcussion, 8);
                AddBag(bag, OSUInjuryType.Bloodied, 8);
                AddBag(bag, OSUInjuryType.MajorConcussion, 8);
                AddBag(bag, OSUInjuryType.FracturedLeftArm, 4);
                AddBag(bag, OSUInjuryType.FracturedRightArm, 4);
                AddBag(bag, OSUInjuryType.FracturedLeftLeg, 4);
                AddBag(bag, OSUInjuryType.FracturedRightLeg, 4);
                AddBag(bag, OSUInjuryType.FracturedRibs, 5);
                AddBag(bag, OSUInjuryType.FracturedSkull, 2);
                AddBag(bag, OSUInjuryType.DeepCut, 4);
                AddBag(bag, OSUInjuryType.InternalBleeding, 2);
                AddBag(bag, OSUInjuryType.LaceratedTorso, 2);
            }
            else
            {
                AddBag(bag, OSUInjuryType.MajorConcussion, 8);
                AddBag(bag, OSUInjuryType.FracturedSkull, 6);
                AddBag(bag, OSUInjuryType.InternalBleeding, 6);
                AddBag(bag, OSUInjuryType.LaceratedTorso, 5);
                AddBag(bag, OSUInjuryType.BrokenLeftArm, 4);
                AddBag(bag, OSUInjuryType.BrokenRightArm, 4);
                AddBag(bag, OSUInjuryType.BrokenLeftLeg, 4);
                AddBag(bag, OSUInjuryType.BrokenRightLeg, 4);
                AddBag(bag, OSUInjuryType.BrokenJaw, 3);
                AddBag(bag, OSUInjuryType.ChestTrauma, 1);
                AddBag(bag, OSUInjuryType.RupturedSpleen, 1);
                AddBag(bag, OSUInjuryType.BrokenSkull, 1);
                AddBag(bag, OSUInjuryType.MassiveBleeding, 1);
            }

            if (bag.Count == 0)
                return OSUInjuryType.None;

            return bag[Utility.Random(bag.Count)];
        }

        private static void AddBag(List<OSUInjuryType> bag, OSUInjuryType type, int count)
        {
            for (int i = 0; i < count; i++)
                bag.Add(type);
        }

        private static bool HasOppositeLegDisable(OSUHealthProfile profile, OSUInjuryType type)
        {
            if (profile == null)
                return false;

            bool needLeft = (type == OSUInjuryType.FracturedRightLeg || type == OSUInjuryType.BrokenRightLeg);
            bool needRight = (type == OSUInjuryType.FracturedLeftLeg || type == OSUInjuryType.BrokenLeftLeg);

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (injury == null || injury.Cured)
                    continue;

                if (needLeft && (injury.Type == OSUInjuryType.FracturedLeftLeg || injury.Type == OSUInjuryType.BrokenLeftLeg))
                    return true;

                if (needRight && (injury.Type == OSUInjuryType.FracturedRightLeg || injury.Type == OSUInjuryType.BrokenRightLeg))
                    return true;
            }

            return false;
        }

        private static bool IsDeadlyCollapseCandidate(PlayerMobile pm, OSUHealthProfile profile)
        {
            return pm != null && profile != null && profile.DeadlyLocked && profile.ComaUntilUtc <= DateTime.UtcNow &&
                   profile.HospitalStretcherSerial == 0 && profile.SurgeryStretcherSerial == 0;
        }

        private static void TriggerTemporaryCollapse(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || !pm.Alive)
                return;

            pm.Emote("*perde as forças e desaba por um instante*");
            pm.Direction = Direction.East;
            pm.Frozen = true;
            OSUDragSystem.ForceLayDown(pm, null, 1);

            Timer.DelayCall(TimeSpan.FromSeconds(4.0), delegate
            {
                if (pm == null || pm.Deleted)
                    return;

                if (!ShouldRemainLying(pm))
                {
                    pm.Frozen = false;
                    OSUDragSystem.ReleaseForcedLay(pm);
                }
            });
        }

        private static double GetInjurySusceptibility(PlayerMobile pm)
        {
            double scalar = 1.0;

            if (pm == null)
                return scalar;

            if (pm.OSUDefQualFlags != null)
            {
                if (pm.OSUDefQualFlags.Contains("fragil"))
                    scalar *= 1.10;
                if (pm.OSUDefQualFlags.Contains("debilitado"))
                    scalar *= 1.10;
                if (pm.OSUDefQualFlags.Contains("enfermo"))
                    scalar *= 1.10;
                if (pm.OSUDefQualFlags.Contains("robusto"))
                    scalar *= 0.92;
                if (pm.OSUDefQualFlags.Contains("resiliente"))
                    scalar *= 0.90;
                if (pm.OSUDefQualFlags.Contains("saudavel"))
                    scalar *= 0.92;
            }

            return scalar;
        }

        #endregion

        #region Injury / Disease Apply

        public static bool ApplyInjury(Mobile m, OSUInjuryType type, bool sendMessage)
        {
            if (m == null || m.Deleted || type == OSUInjuryType.None)
                return false;

            InjuryDefinition def = GetInjuryDefinition(type);
            if (def == null)
                return false;

            OSUHealthProfile profile = GetProfile(m, true);

            if (HasInjury(profile, type))
                return false;

            if (HasOppositeLegDisable(profile, type))
                return false;

            TimeSpan duration = def.BaseDuration;
            if (m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;
                if (pm.OSUDefQualFlags != null)
                {
                    if (pm.OSUDefQualFlags.Contains("fragil"))
                        duration += TimeSpan.FromHours(2);
                    if (pm.OSUDefQualFlags.Contains("saudavel"))
                        duration -= TimeSpan.FromHours(1);
                    if (pm.OSUDefQualFlags.Contains("resiliente"))
                        duration -= TimeSpan.FromHours(1);
                }
            }

            if (duration < TimeSpan.FromHours(1))
                duration = TimeSpan.FromHours(1);

            OSUInjuryState state = new OSUInjuryState();
            state.Type = type;
            state.Severity = def.Severity;
            state.RequiresSurgery = def.RequiresSurgery;
            state.StartedUtc = DateTime.UtcNow;
            state.EndsUtc = DateTime.UtcNow + duration;
            state.Cured = false;

            profile.Injuries.Add(state);

            if (sendMessage)
                m.SendMessage(37, def.AppliedMessage);

            if (type == OSUInjuryType.BrokenRightArm)
            {
                TryDropHeldWeapon(m);
                EnforceBrokenArmEquipment(m);
            }

            if (type == OSUInjuryType.BrokenLeftArm)
            {
                TryDropShieldOrTwoHanded(m);
                EnforceBrokenArmEquipment(m);
            }

            if (def.Severity == OSUInjurySeverity.Deadly)
                BeginDeadlyState(m);

            return true;
        }

        public static bool ApplyDisease(Mobile m, OSUDiseaseType type, bool sendMessage)
        {
            if (m == null || m.Deleted || type == OSUDiseaseType.None)
                return false;

            DiseaseDefinition def = GetDiseaseDefinition(type);
            if (def == null)
                return false;

            if (m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;
                if (Utility.RandomDouble() > GetDiseaseSusceptibility(pm, type))
                    return false;
            }

            OSUHealthProfile profile = GetProfile(m, true);

            for (int i = 0; i < profile.Diseases.Count; i++)
            {
                if (!profile.Diseases[i].Cured && profile.Diseases[i].Type == type)
                    return false;
            }

            OSUDiseaseState state = new OSUDiseaseState();
            state.Type = type;
            state.ContractedUtc = DateTime.UtcNow;
            state.IncubationEndsUtc = DateTime.UtcNow + def.Incubation;
            state.NextPulseUtc = state.IncubationEndsUtc + def.Virulence;
            state.RecoveryCount = 0;
            state.Cured = false;

            profile.Diseases.Add(state);

            if (sendMessage)
                m.SendMessage(def.ContractedMessage);

            return true;
        }

        public static bool HasInjury(OSUHealthProfile profile, OSUInjuryType type)
        {
            if (profile == null)
                return false;

            for (int i = 0; i < profile.Injuries.Count; i++)
                if (!profile.Injuries[i].Cured && profile.Injuries[i].Type == type)
                    return true;

            return false;
        }

        public static bool HasAnyDeadlyInjury(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return false;

            if (profile.DeadlyLocked)
                return true;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState state = profile.Injuries[i];
                if (!state.Cured && state.Severity == OSUInjurySeverity.Deadly)
                    return true;
            }

            return false;
        }

        public static void CureInjury(Mobile m, OSUInjuryType type, bool sendMessage)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return;

            bool deadlyBefore = profile.DeadlyLocked;

            for (int i = profile.Injuries.Count - 1; i >= 0; i--)
            {
                if (profile.Injuries[i].Type == type)
                    profile.Injuries.RemoveAt(i);
            }

            if (sendMessage && m != null)
            {
                InjuryDefinition def = GetInjuryDefinition(type);
                if (def != null)
                    m.SendMessage("Você se recuperou de " + def.Name + ".");
            }

            if (deadlyBefore && !HasAnyDeadlyInjury(m))
                EndDeadlyState(m);
        }

        public static void CureDisease(Mobile m, OSUDiseaseType type, bool sendMessage)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return;

            for (int i = profile.Diseases.Count - 1; i >= 0; i--)
            {
                if (profile.Diseases[i].Type == type)
                    profile.Diseases.RemoveAt(i);
            }

            if (sendMessage && m != null)
                m.SendMessage("Você se sente mais saudável.");
        }

        public static void AddTimedImmunity(Mobile m, OSUDiseaseType disease, string sourceId, double reductionScalar, TimeSpan duration)
        {
            if (m == null || disease == OSUDiseaseType.None || duration <= TimeSpan.Zero)
                return;

            if (reductionScalar < 0.0)
                reductionScalar = 0.0;

            if (reductionScalar > 1.0)
                reductionScalar = 1.0;

            OSUHealthProfile profile = GetProfile(m, true);
            OSUImmunityState state = new OSUImmunityState();
            state.Disease = disease;
            state.SourceId = sourceId ?? String.Empty;
            state.ReductionScalar = reductionScalar;
            state.EndsUtc = DateTime.UtcNow + duration;
            profile.Immunities.Add(state);
        }

        public static double GetDiseaseSusceptibility(PlayerMobile pm, OSUDiseaseType disease)
        {
            double scalar = OSUDefQualDispatcher.ModifyDiseaseSusceptibility(pm, 1.0);

            OSUHealthProfile profile = GetProfile(pm, false);
            if (profile != null)
            {
                CleanupExpiredImmunities(profile);

                double immunity = 0.0;
                for (int i = 0; i < profile.Immunities.Count; i++)
                {
                    OSUImmunityState entry = profile.Immunities[i];
                    if (entry.Disease == disease)
                        immunity += entry.ReductionScalar;
                }

                if (immunity > 0.95)
                    immunity = 0.95;

                scalar *= (1.0 - immunity);
            }

            if (scalar < 0.05)
                scalar = 0.05;

            return scalar;
        }

        private static void CleanupExpiredImmunities(OSUHealthProfile profile)
        {
            if (profile == null)
                return;

            DateTime now = DateTime.UtcNow;
            for (int i = profile.Immunities.Count - 1; i >= 0; i--)
            {
                if (profile.Immunities[i].EndsUtc <= now)
                    profile.Immunities.RemoveAt(i);
            }
        }

        #endregion

        #region Deadly Injury State

        public static void BeginDeadlyState(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            OSUHealthProfile profile = GetProfile(pm, true);

            profile.DeadlyLocked = true;
            profile.DeadlyDeadlineUtc = DateTime.UtcNow + TimeSpan.FromHours(48.0);

            pm.Hidden = false;
            pm.SendMessage(37, "Suas lesões são mortais. Você ainda consegue andar, mas precisa de cirurgia urgente.");
        }

        public static void EndDeadlyState(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            OSUHealthProfile profile = GetProfile(pm, false);
            if (profile != null)
            {
                profile.DeadlyLocked = false;
                profile.DeadlyDeadlineUtc = DateTime.MinValue;
            }

            pm.Blessed = false;
            pm.SendMessage("Você não está mais em estado crítico.");
        }

        #endregion


        #region Contamination

        public static void ContaminateItem(Item item, OSUDiseaseType disease, TimeSpan duration, string sourceLabel)
        {
            if (item == null || item.Deleted || disease == OSUDiseaseType.None)
                return;

            OSUContaminatedItemState state = new OSUContaminatedItemState();
            state.ItemSerial = item.Serial.Value;
            state.Disease = disease;
            state.ExpiresUtc = DateTime.UtcNow + duration;
            state.SourceLabel = sourceLabel ?? String.Empty;

            lock (_sync)
            {
                _contaminatedItems[item.Serial.Value] = state;
            }

            item.OSUContaminated = true;
            item.OSUContamination = (int)disease;
            item.OSUContaminationEndsUtc = state.ExpiresUtc;
            item.OSUContaminationSource = state.SourceLabel;
            item.InvalidateProperties();
        }

        public static void ClearContaminatedItem(Item item)
        {
            if (item == null)
                return;

            lock (_sync)
            {
                _contaminatedItems.Remove(item.Serial.Value);
            }

            item.OSUContaminated = false;
            item.OSUContamination = 0;
            item.OSUContaminationEndsUtc = DateTime.MinValue;
            item.OSUContaminationSource = String.Empty;
            item.InvalidateProperties();
        }

        public static bool IsContaminated(Item item)
        {
            if (item == null)
                return false;

            if (item.OSUContaminated && item.OSUContamination != 0)
            {
                if (item.OSUContaminationEndsUtc == DateTime.MinValue || item.OSUContaminationEndsUtc > DateTime.UtcNow)
                    return true;

                ClearContaminatedItem(item);
                return false;
            }

            lock (_sync)
            {
                OSUContaminatedItemState state;
                if (_contaminatedItems.TryGetValue(item.Serial.Value, out state))
                {
                    if (state.ExpiresUtc > DateTime.UtcNow)
                    {
                        item.OSUContaminated = true;
                        item.OSUContamination = (int)state.Disease;
                        item.OSUContaminationEndsUtc = state.ExpiresUtc;
                        item.OSUContaminationSource = state.SourceLabel ?? String.Empty;
                        item.InvalidateProperties();
                        return true;
                    }

                    _contaminatedItems.Remove(item.Serial.Value);
                    item.InvalidateProperties();
                }
            }

            return false;
        }

        public static OSUDiseaseType GetItemContamination(Item item)
        {
            if (item == null)
                return OSUDiseaseType.None;

            if (IsContaminated(item))
                return (OSUDiseaseType)item.OSUContamination;

            lock (_sync)
            {
                OSUContaminatedItemState state;
                if (_contaminatedItems.TryGetValue(item.Serial.Value, out state))
                {
                    if (state.ExpiresUtc > DateTime.UtcNow)
                    {
                        item.OSUContaminated = true;
                        item.OSUContamination = (int)state.Disease;
                        item.OSUContaminationEndsUtc = state.ExpiresUtc;
                        item.OSUContaminationSource = state.SourceLabel ?? String.Empty;
                        item.InvalidateProperties();
                        return state.Disease;
                    }

                    _contaminatedItems.Remove(item.Serial.Value);
                    item.InvalidateProperties();
                }
            }

            return OSUDiseaseType.None;
        }

        public static void CopyContamination(Item source, Item target)
        {
            if (source == null || target == null)
                return;

            if (!IsContaminated(source))
            {
                ClearContaminatedItem(target);
                return;
            }

            DateTime ends = GetItemContaminationEndUtc(source);
            TimeSpan duration = ends > DateTime.UtcNow ? (ends - DateTime.UtcNow) : TimeSpan.FromHours(6);
            ContaminateItem(target, GetItemContamination(source), duration, GetItemContaminationSource(source));
        }

        public static bool TryContaminateItemFromLocation(Item item, Map map, int x, int y, TimeSpan duration, string sourceLabel)
        {
            if (item == null || map == null || map == Map.Internal)
                return false;

            OSUDiseaseType disease;
            if (!TryGetAreaDiseaseAt(map, x, y, out disease))
                return false;

            ContaminateItem(item, disease, duration, sourceLabel);
            return true;
        }

        public static bool TryGetAreaDiseaseAt(Map map, int x, int y, out OSUDiseaseType disease)
        {
            disease = OSUDiseaseType.None;

            if (map == null || map == Map.Internal)
                return false;

            IPooledEnumerable eable = map.GetItemsInRange(new Point3D(x, y, 0), 12);
            try
            {
                foreach (Item raw in eable)
                {
                    OSUDiseaseSource source = raw as OSUDiseaseSource;
                    if (source == null || source.Deleted || source.Map != map || !source.Active)
                        continue;

                    if (source.Disease == OSUDiseaseType.None)
                        continue;

                    if (!Utility.InRange(new Point3D(x, y, 0), source.GetWorldLocation(), source.Radius))
                        continue;

                    if (source.AffectsWetTilesOnly && !IsWetTile(map, x, y))
                        continue;

                    disease = source.Disease;
                    return true;
                }
            }
            finally
            {
                eable.Free();
            }

            return false;
        }

        public static bool TryExposeFromItem(Mobile m, Item item)
        {
            if (m == null || item == null)
                return false;

            OSUDiseaseType disease = GetItemContamination(item);
            if (disease == OSUDiseaseType.None)
                return false;

            return ApplyDisease(m, disease, true);
        }

        public static bool IsStandingInContaminatedArea(Mobile m)
        {
            if (m == null || m.Map == null || m.Map == Map.Internal)
                return false;

            OSUDiseaseType disease;
            if (!TryGetAreaDiseaseAt(m.Map, m.X, m.Y, out disease))
                return false;

            ApplyDisease(m, disease, true);
            return true;
        }

        public static bool IsWetTile(Map map, int x, int y)
        {
            if (map == null || map == Map.Internal)
                return false;

            LandTile land = map.Tiles.GetLandTile(x, y);
            if ((TileData.LandTable[land.ID & 0x3FFF].Flags & TileFlag.Wet) != 0)
                return true;

            StaticTile[] statics = map.Tiles.GetStaticTiles(x, y, true);
            for (int i = 0; i < statics.Length; i++)
            {
                int itemId = statics[i].ID & 0x3FFF;
                if ((TileData.ItemTable[itemId].Flags & TileFlag.Wet) != 0)
                    return true;
            }

            return false;
        }

        #endregion


#region Surgery

        private class SurgeryPlan
        {
            public bool AllowCut;
            public bool AllowHeat;
            public bool AllowBleed;
            public int CutMax;
            public int HeatMax;
            public int BleedMax;
        }

        private static SurgeryPlan BuildPlan(OSUInjuryType injury)
        {
            SurgeryPlan p = new SurgeryPlan();
            p.AllowCut = true;
            p.AllowHeat = true;
            p.AllowBleed = false;
            p.CutMax = 2;
            p.HeatMax = 2;
            p.BleedMax = 0;

            switch (injury)
            {
                case OSUInjuryType.InternalBleeding:
                case OSUInjuryType.MassiveBleeding:
                    p.AllowCut = false;
                    p.AllowHeat = true;
                    p.AllowBleed = false;
                    p.CutMax = 0;
                    p.HeatMax = 3;
                    p.BleedMax = 0;
                    break;
                case OSUInjuryType.RupturedSpleen:
                case OSUInjuryType.ChestTrauma:
                    p.AllowCut = true;
                    p.AllowHeat = true;
                    p.AllowBleed = true;
                    p.CutMax = 2;
                    p.HeatMax = 3;
                    p.BleedMax = 2;
                    break;
                case OSUInjuryType.BrokenSkull:
                case OSUInjuryType.FracturedSkull:
                    p.AllowCut = false;
                    p.AllowHeat = true;
                    p.AllowBleed = true;
                    p.CutMax = 0;
                    p.HeatMax = 2;
                    p.BleedMax = 2;
                    break;
                case OSUInjuryType.DeepCut:
                case OSUInjuryType.LaceratedTorso:
                    p.AllowCut = true;
                    p.AllowHeat = true;
                    p.AllowBleed = false;
                    p.CutMax = 3;
                    p.HeatMax = 2;
                    p.BleedMax = 0;
                    break;
                case OSUInjuryType.FracturedLeftArm:
                case OSUInjuryType.FracturedRightArm:
                case OSUInjuryType.FracturedLeftLeg:
                case OSUInjuryType.FracturedRightLeg:
                case OSUInjuryType.FracturedRibs:
                case OSUInjuryType.BrokenLeftArm:
                case OSUInjuryType.BrokenRightArm:
                case OSUInjuryType.BrokenLeftLeg:
                case OSUInjuryType.BrokenRightLeg:
                case OSUInjuryType.BrokenJaw:
                    p.AllowCut = true;
                    p.AllowHeat = false;
                    p.AllowBleed = false;
                    p.CutMax = 3;
                    p.HeatMax = 0;
                    p.BleedMax = 0;
                    break;
            }

            return p;
        }

        public static OSUInjurySeverity GetSurgerySeverity(OSUSurgeryProgressState progress)
        {
            if (progress == null)
                return OSUInjurySeverity.Light;

            InjuryDefinition def = GetInjuryDefinition(progress.Injury);
            return def != null ? def.Severity : OSUInjurySeverity.Light;
        }

        public static string GetSurgeryConditionLabel(OSUSurgeryProgressState state)
        {
            if (state == null)
                return "Sem cirurgia";

            if (!state.Anesthetized)
                return "Preparando";

            return "Paciente anestesiado";
        }

        private static TimeSpan GetBaseSurgeryTime(OSUInjurySeverity severity)
        {
            switch (severity)
            {
                case OSUInjurySeverity.Moderate: return TimeSpan.FromSeconds(30.0);
                case OSUInjurySeverity.Severe: return TimeSpan.FromSeconds(25.0);
                case OSUInjurySeverity.Critical:
                case OSUInjurySeverity.Deadly: return TimeSpan.FromSeconds(20.0);
                default: return TimeSpan.FromSeconds(30.0);
            }
        }

        private static OSUInjuryState GetNextSurgeryInjury(OSUHealthProfile profile)
        {
            if (profile == null)
                return null;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (!injury.Cured && injury.RequiresSurgery)
                    return injury;
            }

            return null;
        }

        private static bool IsPatientOnSurgeryStretcher(PlayerMobile patient)
        {
            if (patient == null)
                return false;

            OSUHealthProfile profile = GetProfile(patient, false);
            return profile != null && profile.SurgeryStretcherSerial != 0;
        }

        private static bool IsSurgeonBusy(PlayerMobile surgeon)
        {
            if (surgeon == null)
                return false;

            lock (_sync)
            {
                foreach (OSUSurgeryProgressState s in _surgery.Values)
                    if (s != null && s.SurgeonSerial == surgeon.Serial.Value)
                        return true;
            }

            return false;
        }

        private static void StartSurgery(PlayerMobile surgeon, PlayerMobile patient, Item sourceItem, OSUInjuryState injury, out string message)
        {
            message = String.Empty;
            if (surgeon == null || patient == null || injury == null)
            {
                message = "Cirurgião ou paciente inválido.";
                return;
            }

            if (IsSurgeonBusy(surgeon))
            {
                message = "Você já está conduzindo outra cirurgia.";
                return;
            }

            InjuryDefinition def = GetInjuryDefinition(injury.Type);
            if (def == null)
            {
                message = "A lesão escolhida não é válida.";
                return;
            }

            SurgeryPlan plan = BuildPlan(injury.Type);
            OSUSurgeryProgressState state = new OSUSurgeryProgressState();
            state.PatientSerial = patient.Serial.Value;
            state.SurgeonSerial = surgeon.Serial.Value;
            state.SourceCityId = GetCityIdFromHospitalItem(sourceItem);
            state.SourceConstructionKey = GetConstructionKeyFromHospitalItem(sourceItem);
            state.Injury = injury.Type;
            state.AllowCut = plan.AllowCut;
            state.AllowHeat = plan.AllowHeat;
            state.AllowBleed = plan.AllowBleed;
            state.AllowCool = false;
            state.TargetCutMin = Utility.RandomMinMax(0, plan.CutMax);
            state.TargetCutMax = state.TargetCutMin;
            state.TargetHeatMin = Utility.RandomMinMax(0, plan.HeatMax);
            state.TargetHeatMax = state.TargetHeatMin;
            state.TargetBleedMin = Utility.RandomMinMax(0, plan.BleedMax);
            state.TargetBleedMax = state.TargetBleedMin;
            state.Cut = 0;
            state.Heat = 0;
            state.Bleed = 0;
            state.StatusText = "O paciente foi anestesiado. Comece os procedimentos.";
            state.Anesthetized = true;
            state.StartedUtc = DateTime.UtcNow;
            state.DeadlineUtc = state.StartedUtc + GetBaseSurgeryTime(def.Severity);
            state.LastActionUtc = state.StartedUtc;

            lock (_sync)
                _surgery[patient.Serial.Value] = state;

            patient.Frozen = true;
            ReapplyStretcherLay(patient, GetProfile(patient, true));
            message = "Você anestesia o paciente e inicia a cirurgia de " + def.Name + ".";
        }

        private static int GetDistanceToTarget(int value, int target)
        {
            return Math.Abs(value - target);
        }

        private static string EvaluateAxisFeedback(string exactMessage, string helpedMessage, string worsenedMessage, int before, int after, int target)
        {
            if (after == target)
                return exactMessage;

            int d1 = GetDistanceToTarget(before, target);
            int d2 = GetDistanceToTarget(after, target);
            return d2 < d1 ? helpedMessage : worsenedMessage;
        }

        private static void SpillBlood(PlayerMobile patient)
        {
            if (patient == null)
                return;

            OSUHealthProfile profile = GetProfile(patient, false);
            Item anchor = null;
            if (profile != null && profile.SurgeryStretcherSerial != 0)
                anchor = World.FindItem(profile.SurgeryStretcherSerial);

            if (anchor == null || anchor.Deleted || anchor.Map == null || anchor.Map == Map.Internal)
                return;

            Static blood = new Static(0x122A);
            blood.Movable = false;
            blood.MoveToWorld(new Point3D(anchor.X + 1, anchor.Y, anchor.Z), anchor.Map);
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), delegate { if (blood != null && !blood.Deleted) blood.Delete(); });
        }

        private static void EndSurgerySuccess(PlayerMobile surgeon, PlayerMobile patient, OSUSurgeryProgressState state)
        {
            OSUHealthProfile profile = GetProfile(patient, false);
            if (profile != null)
            {
                profile.SurgeryFailureCount = 0;
                for (int i = profile.Injuries.Count - 1; i >= 0; i--)
                {
                    if (!profile.Injuries[i].Cured && profile.Injuries[i].Type == state.Injury)
                    {
                        profile.Injuries.RemoveAt(i);
                        break;
                    }
                }
            }

            IncrementWeeklySurgeryCount(state.SourceCityId);

            lock (_sync)
                _surgery.Remove(patient.Serial.Value);

            if (surgeon != null)
                surgeon.SendMessage("A cirurgia foi concluída com sucesso.");
            patient.SendMessage("Você sente que a cirurgia terminou bem.");
            if (surgeon != null)
                surgeon.CloseGump(typeof(OSUSurgeryStatusGump));
            patient.Frozen = false;
        }

        private static TimeSpan GetComaDurationForFailures(int count)
        {
            if (count <= 1)
                return TimeSpan.FromHours(1.0);
            if (count == 2)
                return TimeSpan.FromHours(3.0);
            if (count == 3)
                return TimeSpan.FromHours(12.0);
            return TimeSpan.FromHours(30.0);
        }

        private static void EnterComa(PlayerMobile patient, TimeSpan duration)
        {
            OSUHealthProfile profile = GetProfile(patient, true);
            profile.ComaUntilUtc = DateTime.UtcNow + duration;
            profile.SurgeryBlockedUntilUtc = profile.ComaUntilUtc;
            patient.Frozen = true;
            ReapplyStretcherLay(patient, profile);
        }

        public static void CancelSurgeryForPatient(PlayerMobile patient, string reason)
        {
            if (patient == null)
                return;

            OSUSurgeryProgressState state = GetSurgeryState(patient);
            if (state == null)
                return;

            lock (_sync)
                _surgery.Remove(patient.Serial.Value);

            PlayerMobile surgeon = World.FindMobile(state.SurgeonSerial) as PlayerMobile;
            if (surgeon != null && !String.IsNullOrWhiteSpace(reason))
                surgeon.SendMessage(reason);
            if (surgeon != null)
                surgeon.CloseGump(typeof(OSUSurgeryStatusGump));
        }

        private static void EndSurgeryFailure(PlayerMobile surgeon, PlayerMobile patient, OSUSurgeryProgressState state, string reason)
        {
            lock (_sync)
                _surgery.Remove(patient.Serial.Value);

            OSUHealthProfile profile = GetProfile(patient, true);
            profile.SurgeryFailureCount++;
            TimeSpan coma = GetComaDurationForFailures(profile.SurgeryFailureCount);
            EnterComa(patient, coma);

            if (surgeon != null)
                surgeon.SendMessage(reason + " O paciente entrou em coma por " + ((int)coma.TotalHours) + " horas.");
            patient.SendMessage("Seu corpo entra em coma após a cirurgia falhar.");

            IncrementWeeklySurgeryCount(state.SourceCityId);
            if (surgeon != null)
                surgeon.CloseGump(typeof(OSUSurgeryStatusGump));
        }

        private static bool ValidateSurgeryAccess(PlayerMobile surgeon, PlayerMobile patient, out string message)
        {
            message = String.Empty;

            if (surgeon == null || patient == null)
            {
                message = "Cirurgião ou paciente inválido.";
                return false;
            }

            if (!CanUseSurgery(surgeon))
            {
                message = "Você precisa da feat Cirurgia.";
                return false;
            }

            if (IsSurgeonBusy(surgeon))
            {
                OSUSurgeryProgressState busy = null;
                lock (_sync)
                {
                    foreach (OSUSurgeryProgressState s in _surgery.Values)
                    {
                        if (s != null && s.SurgeonSerial == surgeon.Serial.Value)
                        {
                            busy = s;
                            break;
                        }
                    }
                }

                if (busy != null && busy.PatientSerial != patient.Serial.Value)
                {
                    message = "Você já está conduzindo outra cirurgia.";
                    return false;
                }
            }

            if (!IsPatientOnSurgeryStretcher(patient))
            {
                message = "O paciente precisa estar na maca cirúrgica.";
                return false;
            }

            OSUHealthProfile profile = GetProfile(patient, false);
            if (profile != null && profile.SurgeryBlockedUntilUtc > DateTime.UtcNow)
            {
                message = "O paciente ainda está muito frágil para uma nova cirurgia.";
                return false;
            }

            OSUInjuryState injury = GetNextSurgeryInjury(profile);
            if (injury == null)
            {
                message = "Esse paciente não tem nenhuma lesão cirúrgica pendente.";
                return false;
            }

            return true;
        }

        public static bool TryUseSurgeryTool(PlayerMobile surgeon, PlayerMobile patient, OSUSurgeryToolType tool, Item sourceItem, out string message)
        {
            message = String.Empty;

            if (!ValidateSurgeryAccess(surgeon, patient, out message))
                return false;

            OSUHealthProfile profile = GetProfile(patient, false);
            OSUInjuryState injury = GetNextSurgeryInjury(profile);
            if (injury == null)
            {
                message = "Esse paciente não tem nenhuma lesão cirúrgica pendente.";
                return false;
            }

            OSUSurgeryProgressState state = GetSurgeryState(patient);
            if (tool == OSUSurgeryToolType.Anestesico)
            {
                if (state != null)
                {
                    message = "Essa cirurgia já foi anestesiada e está em andamento.";
                    return false;
                }

                StartSurgery(surgeon, patient, sourceItem, injury, out message);
                return true;
            }

            if (state == null)
            {
                message = "Você precisa começar pela anestesia.";
                return false;
            }

            if (state.SurgeonSerial != surgeon.Serial.Value)
            {
                message = "Outro cirurgião já está tratando esse paciente.";
                return false;
            }

            if (DateTime.UtcNow >= state.DeadlineUtc)
            {
                EndSurgeryFailure(surgeon, patient, state, "O tempo da cirurgia acabou.");
                message = "O tempo da cirurgia acabou.";
                return false;
            }

            if (!surgeon.InRange(patient.Location, 2))
            {
                EndSurgeryFailure(surgeon, patient, state, "Você se afastou demais do paciente.");
                message = "Você se afastou demais do paciente.";
                return false;
            }

            int beforeCut = state.Cut;
            int beforeHeat = state.Heat;
            int beforeBleed = state.Bleed;
            string status = String.Empty;

            switch (tool)
            {
                case OSUSurgeryToolType.Tesoura:
                    if (!state.AllowCut)
                        status = "A incisão foi desnecessária e piorou o estado do paciente.";
                    else
                    {
                        state.Cut += 1;
                        SpillBlood(patient);
                        status = EvaluateAxisFeedback("Você fez a incisão perfeita.", "A incisão ajudou o procedimento.", "A incisão foi profunda e piorou o estado do paciente.", beforeCut, state.Cut, state.TargetCutMin);
                    }
                    break;
                case OSUSurgeryToolType.CuteloCirurgico:
                    if (!state.AllowCut)
                        status = "O corte pesado foi desnecessário e piorou o estado do paciente.";
                    else
                    {
                        state.Cut += 3;
                        SpillBlood(patient);
                        status = EvaluateAxisFeedback("Você fez a incisão perfeita.", "O corte amplo ajudou o procedimento.", "O corte amplo piorou o estado do paciente.", beforeCut, state.Cut, state.TargetCutMin);
                    }
                    break;
                case OSUSurgeryToolType.Gazes:
                    state.Cut = Math.Max(0, state.Cut - 1);
                    status = EvaluateAxisFeedback("Você fez a incisão perfeita.", "A gaze conteve o corte na medida certa.", "A gaze conteve demais o corte e atrapalhou o procedimento.", beforeCut, state.Cut, state.TargetCutMin);
                    break;
                case OSUSurgeryToolType.VelaCauterizadora:
                    if (!state.AllowHeat)
                        status = "O calor foi desnecessário e piorou o estado do paciente.";
                    else
                    {
                        state.Heat += 1;
                        status = EvaluateAxisFeedback("O tecido está morto.", "A cauterização ajudou o procedimento.", "A cauterização passou do ponto e piorou o estado do paciente.", beforeHeat, state.Heat, state.TargetHeatMin);
                    }
                    break;
                case OSUSurgeryToolType.BrasaCauterizadora:
                    if (!state.AllowHeat)
                        status = "O calor intenso foi desnecessário e piorou o estado do paciente.";
                    else
                    {
                        state.Heat += 3;
                        status = EvaluateAxisFeedback("O tecido está morto.", "A brasa cauterizou com firmeza e ajudou o procedimento.", "A brasa cauterizou demais e piorou o estado do paciente.", beforeHeat, state.Heat, state.TargetHeatMin);
                    }
                    break;
                case OSUSurgeryToolType.AguaEsteril:
                    state.Heat = Math.Max(0, state.Heat - 1);
                    status = EvaluateAxisFeedback("O tecido está morto.", "A água estabilizou a cauterização.", "A água resfriou demais a área e piorou o estado do paciente.", beforeHeat, state.Heat, state.TargetHeatMin);
                    break;
                case OSUSurgeryToolType.Sanguessuga:
                    if (!state.AllowBleed)
                        status = "A drenagem foi desnecessária e piorou o estado do paciente.";
                    else
                    {
                        state.Bleed += 1;
                        status = EvaluateAxisFeedback("O ferimento foi totalmente drenado.", "A drenagem ajudou o procedimento.", "A drenagem passou do ponto e piorou o estado do paciente.", beforeBleed, state.Bleed, state.TargetBleedMin);
                    }
                    break;
                case OSUSurgeryToolType.AdagaSangria:
                    if (!state.AllowBleed)
                        status = "A drenagem agressiva foi desnecessária e piorou o estado do paciente.";
                    else
                    {
                        state.Bleed += 3;
                        status = EvaluateAxisFeedback("O ferimento foi totalmente drenado.", "A drenagem agressiva ajudou o procedimento.", "A drenagem agressiva passou do ponto e piorou o estado do paciente.", beforeBleed, state.Bleed, state.TargetBleedMin);
                    }
                    break;
                case OSUSurgeryToolType.AlcoolCirurgico:
                case OSUSurgeryToolType.TochaCauterizadora:
                    state.Bleed = Math.Max(0, state.Bleed - 1);
                    status = EvaluateAxisFeedback("O ferimento foi totalmente drenado.", "O álcool estabilizou a drenagem.", "O álcool removeu drenagem demais e piorou o estado do paciente.", beforeBleed, state.Bleed, state.TargetBleedMin);
                    break;
                case OSUSurgeryToolType.LinhaSutura:
                    bool ok = state.Cut == state.TargetCutMin && state.Heat == state.TargetHeatMin && state.Bleed == state.TargetBleedMin;
                    if (ok)
                    {
                        state.StatusText = "A sutura encerrou a cirurgia com sucesso.";
                        EndSurgerySuccess(surgeon, patient, state);
                        message = state.StatusText;
                        return true;
                    }

                    EndSurgeryFailure(surgeon, patient, state, "A sutura final foi feita no momento errado e a cirurgia falhou.");
                    message = "A sutura final foi feita no momento errado e a cirurgia falhou.";
                    return false;
                default:
                    message = "Esse instrumento não faz parte do novo procedimento cirúrgico.";
                    return false;
            }

            state.LastActionUtc = DateTime.UtcNow;
            state.StatusText = status;
            message = status;
            return true;
        }

#endregion

#region Ticks

        private static void ProcessOnlinePlayer(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted)
                return;

            OSUHealthProfile profile = GetProfile(pm, false);
            if (profile == null)
                return;

            EnforceBrokenArmEquipment(pm);
            CleanupExpiredImmunities(profile);
            DateTime now = DateTime.UtcNow;

            if (_surgeryWeekStartUtc == DateTime.MinValue || now >= _surgeryWeekStartUtc + TimeSpan.FromDays(7.0))
            {
                _surgeryWeekStartUtc = GetWeekStartUtc(now);
                _weeklyHospitalSurgeries.Clear();
            }

            OSUSurgeryProgressState activeSurgery = GetSurgeryState(pm);
            if (activeSurgery != null && profile.SurgeryStretcherSerial != 0)
            {
                pm.Frozen = true;
                pm.Direction = Direction.East;
                ReapplyStretcherLay(pm, profile);
            }

            if (activeSurgery != null)
            {
                PlayerMobile surgeon = World.FindMobile(activeSurgery.SurgeonSerial) as PlayerMobile;
                if (now >= activeSurgery.DeadlineUtc)
                    EndSurgeryFailure(surgeon, pm, activeSurgery, "O tempo da cirurgia acabou.");
                else if (surgeon == null || surgeon.Deleted || !surgeon.InRange(pm.Location, 2))
                    EndSurgeryFailure(surgeon, pm, activeSurgery, "O cirurgião se afastou demais do paciente.");
            }

            if (profile.ComaUntilUtc > now)
            {
                pm.Frozen = true;
                ReapplyStretcherLay(pm, profile);
                return;
            }
            else if (profile.ComaUntilUtc != DateTime.MinValue && profile.ComaUntilUtc <= now)
            {
                profile.ComaUntilUtc = DateTime.MinValue;
                profile.SurgeryBlockedUntilUtc = DateTime.MinValue;
                CleanupExpiredStatesAfterComa(pm, profile, now);
                if (!ShouldRemainLying(pm))
                {
                    pm.Frozen = false;
                    OSUDragSystem.ReleaseForcedLay(pm);
                }
            }

            if (profile.DeadlyLocked)
            {
                pm.Blessed = false;
                pm.AddStatMod(new StatMod(StatType.Str, "[OSU][Deadly][Str]", -15, TimeSpan.FromMinutes(1.0)));

                if (profile.DeadlyDeadlineUtc != DateTime.MinValue && now >= profile.DeadlyDeadlineUtc)
                {
                    profile.DeadlyLocked = false;
                    pm.Blessed = false;
                    pm.OSULives = 0;
                    pm.OSUPermaDead = true;
                    pm.Kill();
                    return;
                }

                if (IsDeadlyCollapseCandidate(pm, profile) && Utility.RandomDouble() < 0.05)
                    TriggerTemporaryCollapse(pm);
            }

            bool onHospitalStretcher = profile.HospitalStretcherSerial != 0;
            bool stillHasHealableContent = false;
            bool hadTrackedRecoveryContent = false;

            for (int i = profile.Injuries.Count - 1; i >= 0; i--)
            {
                OSUInjuryState injury = profile.Injuries[i];
                InjuryDefinition def = GetInjuryDefinition(injury.Type);
                if (def == null)
                    continue;

                ApplyInjuryPulse(pm, injury);

                if (!injury.RequiresSurgery)
                {
                    hadTrackedRecoveryContent = true;
                    if (onHospitalStretcher && def.HealsOnHospitalStretcher)
                        injury.EndsUtc -= TimeSpan.FromSeconds(30.0);

                    if (now >= injury.EndsUtc)
                    {
                        profile.Injuries.RemoveAt(i);
                        pm.SendMessage("Você se recuperou de " + def.Name + ".");
                        continue;
                    }

                    stillHasHealableContent = true;
                }
            }

            if (profile.DeadlyLocked && !HasAnyDeadlyInjury(pm))
                EndDeadlyState(pm);

            for (int i = profile.Diseases.Count - 1; i >= 0; i--)
            {
                OSUDiseaseState disease = profile.Diseases[i];
                DiseaseDefinition def = GetDiseaseDefinition(disease.Type);
                if (def == null)
                    continue;

                if (now < disease.IncubationEndsUtc)
                    continue;

                if (now >= disease.NextPulseUtc)
                {
                    ApplyDiseasePulse(pm, disease);
                    disease.NextPulseUtc = now + def.Virulence;

                    if (def.HealsOverTime)
                    {
                        hadTrackedRecoveryContent = true;
                        disease.RecoveryCount += onHospitalStretcher ? 2 : 1;
                        stillHasHealableContent = true;
                    }

                    if (disease.RecoveryCount > def.RecoveryTarget)
                    {
                        pm.SendMessage("Você se sente mais saudável.");
                        AddTimedImmunity(pm, disease.Type, "recuperacao_natural", 0.10, TimeSpan.FromHours(12));
                        profile.Diseases.RemoveAt(i);
                    }
                }
            }

            if (profile.HospitalStretcherSerial != 0 && hadTrackedRecoveryContent && !stillHasHealableContent)
            {
                Item item = World.FindItem(profile.HospitalStretcherSerial);
                OSUHospitalRecoveryStretcher stretcher = item as OSUHospitalRecoveryStretcher;
                if (stretcher != null && !stretcher.Deleted)
                    stretcher.ForceStandUp(pm);
            }
        }

        private static void ApplyInjuryPulse(PlayerMobile pm, OSUInjuryState injury)
        {
            switch (injury.Type)
            {
                case OSUInjuryType.Bloodied:
                case OSUInjuryType.DeepCut:
                    pm.Damage(Utility.RandomMinMax(1, 3), pm);
                    break;
                case OSUInjuryType.InternalBleeding:
                    pm.Damage(Utility.RandomMinMax(2, 4), pm);
                    break;
                case OSUInjuryType.MassiveBleeding:
                    pm.Damage(Utility.RandomMinMax(3, 6), pm);
                    break;
                case OSUInjuryType.Exhausted:
                    pm.Stam = Math.Max(0, pm.Stam - Utility.RandomMinMax(1, 2));
                    pm.Mana = Math.Max(0, pm.Mana - 1);
                    break;
                case OSUInjuryType.Winded:
                case OSUInjuryType.FracturedRibs:
                    pm.Stam = Math.Max(0, pm.Stam - 1);
                    break;
                case OSUInjuryType.MinorConcussion:
                case OSUInjuryType.MajorConcussion:
                case OSUInjuryType.FracturedSkull:
                case OSUInjuryType.BrokenSkull:
                    pm.Mana = Math.Max(0, pm.Mana - 1);
                    break;
            }
        }

        private static void CleanupExpiredStatesAfterComa(PlayerMobile pm, OSUHealthProfile profile, DateTime now)
        {
            if (profile == null)
                return;

            for (int i = profile.Injuries.Count - 1; i >= 0; i--)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (injury != null && !injury.Cured && injury.EndsUtc <= now)
                    profile.Injuries.RemoveAt(i);
            }

            for (int i = profile.Diseases.Count - 1; i >= 0; i--)
            {
                OSUDiseaseState disease = profile.Diseases[i];
                if (disease != null)
                {
                    DiseaseDefinition def = GetDiseaseDefinition(disease.Type);
                    if (def != null && def.HealsOverTime && disease.RecoveryCount >= def.RecoveryTarget)
                        profile.Diseases.RemoveAt(i);
                }
            }

            if (!HasAnyDeadlyInjury(pm))
                EndDeadlyState(pm);
        }

        private static void ApplyDiseasePulse(PlayerMobile pm, OSUDiseaseState disease)
        {
            switch (disease.Type)
            {
                case OSUDiseaseType.Influenza:
                    pm.Emote("*sua de febre*");
                    pm.Stam = Math.Max(0, pm.Stam - 2);
                    pm.Damage(Utility.RandomMinMax(1, 3), pm);
                    break;
                case OSUDiseaseType.HundredDaysCough:
                    pm.Emote("*tosse violentamente*");
                    pm.Stam = Math.Max(0, pm.Stam - 3);
                    pm.Damage(Utility.RandomMinMax(1, 2), pm);
                    break;
                case OSUDiseaseType.Diptheria:
                    pm.Emote("*respira com dificuldade*");
                    pm.AddStatMod(new StatMod(StatType.Dex, "[OSU][Diptheria]", -(Math.Max(3, pm.Dex / 5)), TimeSpan.FromMinutes(2)));
                    break;
                case OSUDiseaseType.Dysentery:
                    pm.Emote("*se dobra de dor*");
                    pm.Hunger = Math.Max(0, pm.Hunger - 3);
                    pm.Thirst = Math.Max(0, pm.Thirst - 3);
                    pm.Damage(Utility.RandomMinMax(1, 3), pm);
                    break;
                case OSUDiseaseType.Consumption:
                    pm.Emote("*cospe sangue*");
                    pm.Damage(Utility.RandomMinMax(2, 4), pm);
                    pm.Stam = Math.Max(0, pm.Stam - 4);
                    break;
                case OSUDiseaseType.WesternFever:
                    pm.Emote("*está molhado de suor*");
                    pm.Stam = Math.Max(0, pm.Stam - 4);
                    pm.AddStatMod(new StatMod(StatType.Str, "[OSU][WesternFever]", -(Math.Max(2, pm.Str / 8)), TimeSpan.FromMinutes(2)));
                    break;
                case OSUDiseaseType.Bile:
                    pm.Emote("*tem fortes náuseas*");
                    pm.Hunger = Math.Max(0, pm.Hunger - 5);
                    pm.Thirst = Math.Max(0, pm.Thirst - 5);
                    pm.Damage(Utility.RandomMinMax(2, 4), pm);
                    break;
                case OSUDiseaseType.Leprosy:
                    pm.Emote("*sente o corpo dormente*");
                    pm.AddStatMod(new StatMod(StatType.Dex, "[OSU][Leprosy]", -5, TimeSpan.FromMinutes(5)));
                    break;
                case OSUDiseaseType.LoveDisease:
                    pm.Emote("*parece debilitado*");
                    pm.AddStatMod(new StatMod(StatType.Str, "[OSU][LoveDisease][Str]", -3, TimeSpan.FromMinutes(5)));
                    pm.AddStatMod(new StatMod(StatType.Dex, "[OSU][LoveDisease][Dex]", -3, TimeSpan.FromMinutes(5)));
                    break;
            }
        }

        #endregion

        #region Utils

        private static void TryDropHeldWeapon(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive)
                return;

            Item item = m.FindItemOnLayer(Layer.OneHanded);
            if (item == null)
                item = m.FindItemOnLayer(Layer.TwoHanded);

            if (item == null || item.Deleted)
                return;

            item.MoveToWorld(m.Location, m.Map);
            m.PlaySound(item.GetDropSound());
        }

        private static void TryDropShieldOrTwoHanded(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive)
                return;

            Item shield = m.FindItemOnLayer(Layer.TwoHanded);
            if (shield == null || shield.Deleted)
                return;

            shield.MoveToWorld(m.Location, m.Map);
            m.PlaySound(shield.GetDropSound());
        }

        public static string GetDisplayName(OSUInjuryType type)
        {
            InjuryDefinition def = GetInjuryDefinition(type);
            return def != null ? def.Name : type.ToString();
        }

        public static string GetDisplayName(OSUDiseaseType type)
        {
            DiseaseDefinition def = GetDiseaseDefinition(type);
            return def != null ? def.Name : type.ToString();
        }

        public static string GetSeverityDisplayName(OSUInjurySeverity severity)
        {
            switch (severity)
            {
                case OSUInjurySeverity.Light: return "Leve";
                case OSUInjurySeverity.Moderate: return "Moderada";
                case OSUInjurySeverity.Severe: return "Severa";
                case OSUInjurySeverity.Critical: return "Crítica";
                case OSUInjurySeverity.Deadly: return "Fatal";
                default: return severity.ToString();
            }
        }

        public static bool CanUseMedicalExam(Mobile m)
        {
            if (m == null)
                return false;

            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            PlayerMobile pm = m as PlayerMobile;
            return pm != null && pm.HasOSUFeat(FeatExameMedico);
        }

        public static bool CanUseSurgery(Mobile m)
        {
            if (m == null)
                return false;

            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            PlayerMobile pm = m as PlayerMobile;
            return pm != null && pm.HasOSUFeat(FeatCirurgia);
        }

        public static bool ShouldRemainLying(Mobile m)
        {
            if (m == null)
                return false;

            OSUHealthProfile profile = GetProfile(m, false);
            if (profile != null && profile.ComaUntilUtc > DateTime.UtcNow)
                return true;

            return false;
        }

        public static bool HasBothLegsDisabled(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return false;

            bool left = false;
            bool right = false;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (injury.Cured)
                    continue;

                switch (injury.Type)
                {
                    case OSUInjuryType.FracturedLeftLeg:
                    case OSUInjuryType.BrokenLeftLeg:
                        left = true;
                        break;
                    case OSUInjuryType.FracturedRightLeg:
                    case OSUInjuryType.BrokenRightLeg:
                        right = true;
                        break;
                }
            }

            return left && right;
        }

        public static bool HasLegRunBlock(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return false;

            if (profile.DeadlyLocked)
                return true;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (injury.Cured)
                    continue;

                if (injury.Type == OSUInjuryType.FracturedLeftLeg || injury.Type == OSUInjuryType.FracturedRightLeg || injury.Type == OSUInjuryType.BrokenLeftLeg || injury.Type == OSUInjuryType.BrokenRightLeg)
                    return true;
            }

            return false;
        }

        public static bool HasJawSpeechBlock(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return false;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (!injury.Cured && injury.Type == OSUInjuryType.BrokenJaw)
                    return true;
            }

            return false;
        }

        public static bool HasLeftArmArcheryBlock(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return false;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];

                if (injury == null || injury.Cured)
                    continue;

                if (injury.Type == OSUInjuryType.FracturedLeftArm || injury.Type == OSUInjuryType.BrokenLeftArm)
                    return true;
            }

            return false;
        }

        public static bool HasAnyBrokenArm(Mobile m)
        {
            OSUHealthProfile profile = GetProfile(m, false);
            if (profile == null)
                return false;

            for (int i = 0; i < profile.Injuries.Count; i++)
            {
                OSUInjuryState injury = profile.Injuries[i];
                if (injury == null || injury.Cured)
                    continue;

                if (injury.Type == OSUInjuryType.BrokenLeftArm || injury.Type == OSUInjuryType.BrokenRightArm)
                    return true;
            }

            return false;
        }

        private static void EnforceBrokenArmEquipment(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive)
                return;

            if (!HasAnyBrokenArm(m))
                return;

            Item twoHanded = m.FindItemOnLayer(Layer.TwoHanded);
            if (twoHanded == null || twoHanded.Deleted)
                return;

            twoHanded.MoveToWorld(m.Location, m.Map);
            m.PlaySound(twoHanded.GetDropSound());

            PlayerMobile pm = m as PlayerMobile;
            if (pm != null)
                pm.SendMessage("Com o braço quebrado, você não consegue usar armas de duas mãos.");
        }

        public static bool BlocksOwnSpeech(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null)
                return false;

            OSUHealthProfile profile = GetProfile(pm, false);
            if (profile != null && profile.ComaUntilUtc > DateTime.UtcNow)
                return true;

            OSUSurgeryProgressState surgery = GetSurgeryState(pm);
            return surgery != null && surgery.Anesthetized;
        }

        public static bool BlocksHearingSpeech(Mobile listener, Mobile speaker)
        {
            PlayerMobile pm = listener as PlayerMobile;
            if (pm == null)
                return false;

            OSUHealthProfile profile = GetProfile(pm, false);
            if (profile != null && profile.ComaUntilUtc > DateTime.UtcNow)
                return true;

            OSUSurgeryProgressState surgery = GetSurgeryState(pm);
            return surgery != null && surgery.Anesthetized;
        }

        private static void ReapplyStretcherLay(PlayerMobile pm, OSUHealthProfile profile)
        {
            if (pm == null || pm.Deleted || profile == null)
                return;

            if (profile.SurgeryStretcherSerial != 0)
            {
                Item surgeryStretcher = World.FindItem(profile.SurgeryStretcherSerial);
                if (surgeryStretcher != null && !surgeryStretcher.Deleted)
                {
                    pm.Direction = Direction.East;
                    OSUDragSystem.ForceLayDown(pm, surgeryStretcher, 1, 0, 2);
                    return;
                }
            }

            if (profile.HospitalStretcherSerial != 0)
            {
                Item hospitalStretcher = World.FindItem(profile.HospitalStretcherSerial);
                if (hospitalStretcher != null && !hospitalStretcher.Deleted)
                {
                    pm.Direction = Direction.East;
                    OSUDragSystem.ForceLayDown(pm, hospitalStretcher, 1, 0, 0);
                    return;
                }
            }

            pm.Direction = Direction.East;
            OSUDragSystem.ForceLayDown(pm, null, 1);
        }

        private static DateTime GetWeekStartUtc(DateTime utc)
        {
            int diff = ((int)utc.DayOfWeek + 6) % 7;
            DateTime d = utc.Date.AddDays(-diff);
            return DateTime.SpecifyKind(d, DateTimeKind.Utc);
        }

        private static void IncrementWeeklySurgeryCount(int cityId)
        {
            if (cityId < 0)
                return;

            DateTime now = DateTime.UtcNow;
            if (_surgeryWeekStartUtc == DateTime.MinValue || now >= _surgeryWeekStartUtc + TimeSpan.FromDays(7.0))
            {
                _surgeryWeekStartUtc = GetWeekStartUtc(now);
                _weeklyHospitalSurgeries.Clear();
            }

            int value;
            _weeklyHospitalSurgeries.TryGetValue(cityId, out value);
            _weeklyHospitalSurgeries[cityId] = value + 1;
        }

        public static void AddDynamicWeeklyCosts(ReinoConstructionRuntimeInfo info, List<ReinoResourceCost> list)
        {
            if (info == null || info.Definition == null || list == null)
                return;

            if (!String.Equals(info.Definition.Id, HospitalAuroraDefinition.BUILDING_ID, StringComparison.OrdinalIgnoreCase))
                return;

            if (_surgeryWeekStartUtc == DateTime.MinValue || DateTime.UtcNow >= _surgeryWeekStartUtc + TimeSpan.FromDays(7.0))
            {
                _surgeryWeekStartUtc = GetWeekStartUtc(DateTime.UtcNow);
                _weeklyHospitalSurgeries.Clear();
            }

            int count;
            if (!_weeklyHospitalSurgeries.TryGetValue(info.CityId, out count) || count <= 0)
                return;

            list.Add(new ReinoResourceCost(ReinoResourceType.Gold, count * 50));
            list.Add(new ReinoResourceCost(ReinoResourceType.Cloth, count * 10));
        }

        public static DateTime GetItemContaminationEndUtc(Item item)
        {
            if (item == null)
                return DateTime.MinValue;

            if (IsContaminated(item))
                return item.OSUContaminationEndsUtc;

            lock (_sync)
            {
                OSUContaminatedItemState state;
                if (_contaminatedItems.TryGetValue(item.Serial.Value, out state))
                    return state.ExpiresUtc;
            }

            return DateTime.MinValue;
        }
        public static string GetItemContaminationSource(Item item)
        {
            if (item == null)
                return String.Empty;

            if (IsContaminated(item))
                return item.OSUContaminationSource ?? String.Empty;

            lock (_sync)
            {
                OSUContaminatedItemState state;
                if (_contaminatedItems.TryGetValue(item.Serial.Value, out state))
                    return state.SourceLabel ?? String.Empty;
            }

            return String.Empty;
        }

        public static int GetCityIdFromHospitalItem(Item item)
        {
            if (item == null)
                return -1;

            IHospitalBoundItem bound = item as IHospitalBoundItem;
            if (bound != null)
                return bound.CityId;

            return -1;
        }

        public static string GetConstructionKeyFromHospitalItem(Item item)
        {
            if (item == null)
                return String.Empty;

            IHospitalBoundItem bound = item as IHospitalBoundItem;
            if (bound != null)
                return bound.ConstructionKey ?? String.Empty;

            return String.Empty;
        }

        #endregion

                #region Saves

        private static void LoadStateFile()
        {
            lock (_sync)
            {
                _profiles.Clear();
                _contaminatedItems.Clear();
                _surgery.Clear();
                _weeklyHospitalSurgeries.Clear();
                _surgeryWeekStartUtc = GetWeekStartUtc(DateTime.UtcNow);

                try
                {
                    if (!File.Exists(FilePath))
                        return;

                    using (FileStream fs = File.OpenRead(FilePath))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int version = br.ReadInt32();

                        int profileCount = br.ReadInt32();
                        for (int i = 0; i < profileCount; i++)
                        {
                            OSUHealthProfile profile = new OSUHealthProfile();
                            profile.MobileSerial = br.ReadInt32();
                            profile.DeadlyLocked = br.ReadBoolean();
                            profile.DeadlyDeadlineUtc = DateTime.FromBinary(br.ReadInt64());
                            profile.PortableStretcherSerial = br.ReadInt32();
                            profile.HospitalStretcherSerial = br.ReadInt32();
                            profile.LastCarrierSerial = br.ReadInt32();
                            if (version >= 2)
                            {
                                profile.SurgeryBlockedUntilUtc = DateTime.FromBinary(br.ReadInt64());
                                profile.ComaUntilUtc = DateTime.FromBinary(br.ReadInt64());
                                profile.SurgeryStretcherSerial = br.ReadInt32();
                            }
                            if (version >= 3)
                                profile.SurgeryFailureCount = br.ReadInt32();

                            int injuryCount = br.ReadInt32();
                            for (int n = 0; n < injuryCount; n++)
                            {
                                OSUInjuryState injury = new OSUInjuryState();
                                injury.Type = (OSUInjuryType)br.ReadInt32();
                                injury.Severity = (OSUInjurySeverity)br.ReadInt32();
                                injury.StartedUtc = DateTime.FromBinary(br.ReadInt64());
                                injury.EndsUtc = DateTime.FromBinary(br.ReadInt64());
                                injury.RequiresSurgery = br.ReadBoolean();
                                injury.Cured = br.ReadBoolean();
                                profile.Injuries.Add(injury);
                            }

                            int diseaseCount = br.ReadInt32();
                            for (int n = 0; n < diseaseCount; n++)
                            {
                                OSUDiseaseState disease = new OSUDiseaseState();
                                disease.Type = (OSUDiseaseType)br.ReadInt32();
                                disease.ContractedUtc = DateTime.FromBinary(br.ReadInt64());
                                disease.IncubationEndsUtc = DateTime.FromBinary(br.ReadInt64());
                                disease.NextPulseUtc = DateTime.FromBinary(br.ReadInt64());
                                disease.RecoveryCount = br.ReadInt32();
                                disease.Cured = br.ReadBoolean();
                                profile.Diseases.Add(disease);
                            }

                            int immunityCount = br.ReadInt32();
                            for (int n = 0; n < immunityCount; n++)
                            {
                                OSUImmunityState immunity = new OSUImmunityState();
                                immunity.Disease = (OSUDiseaseType)br.ReadInt32();
                                immunity.SourceId = br.ReadString();
                                immunity.ReductionScalar = br.ReadDouble();
                                immunity.EndsUtc = DateTime.FromBinary(br.ReadInt64());
                                profile.Immunities.Add(immunity);
                            }

                            _profiles[profile.MobileSerial] = profile;
                        }

                        int contaminatedCount = br.ReadInt32();
                        for (int i = 0; i < contaminatedCount; i++)
                        {
                            OSUContaminatedItemState state = new OSUContaminatedItemState();
                            state.ItemSerial = br.ReadInt32();
                            state.Disease = (OSUDiseaseType)br.ReadInt32();
                            state.ExpiresUtc = DateTime.FromBinary(br.ReadInt64());
                            state.SourceLabel = br.ReadString();
                            _contaminatedItems[state.ItemSerial] = state;
                        }

                        if (version >= 2)
                        {
                            _surgeryWeekStartUtc = DateTime.FromBinary(br.ReadInt64());
                            int weeklyCount = br.ReadInt32();
                            for (int i = 0; i < weeklyCount; i++)
                                _weeklyHospitalSurgeries[br.ReadInt32()] = br.ReadInt32();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[OSUHealth] Falha ao carregar save de health: " + ex);
                }
            }
        }

        private static void SaveStateFile()
        {
            lock (_sync)
            {
                try
                {
                    string dir = Path.GetDirectoryName(FilePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    using (FileStream fs = File.Create(FilePath))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(3);

                        bw.Write(_profiles.Count);
                        foreach (KeyValuePair<int, OSUHealthProfile> kv in _profiles)
                        {
                            OSUHealthProfile profile = kv.Value;
                            bw.Write(profile.MobileSerial);
                            bw.Write(profile.DeadlyLocked);
                            bw.Write(profile.DeadlyDeadlineUtc.ToBinary());
                            bw.Write(profile.PortableStretcherSerial);
                            bw.Write(profile.HospitalStretcherSerial);
                            bw.Write(profile.LastCarrierSerial);
                            bw.Write(profile.SurgeryBlockedUntilUtc.ToBinary());
                            bw.Write(profile.ComaUntilUtc.ToBinary());
                            bw.Write(profile.SurgeryStretcherSerial);
                            bw.Write(profile.SurgeryFailureCount);

                            bw.Write(profile.Injuries.Count);
                            for (int i = 0; i < profile.Injuries.Count; i++)
                            {
                                OSUInjuryState injury = profile.Injuries[i];
                                bw.Write((int)injury.Type);
                                bw.Write((int)injury.Severity);
                                bw.Write(injury.StartedUtc.ToBinary());
                                bw.Write(injury.EndsUtc.ToBinary());
                                bw.Write(injury.RequiresSurgery);
                                bw.Write(injury.Cured);
                            }

                            bw.Write(profile.Diseases.Count);
                            for (int i = 0; i < profile.Diseases.Count; i++)
                            {
                                OSUDiseaseState disease = profile.Diseases[i];
                                bw.Write((int)disease.Type);
                                bw.Write(disease.ContractedUtc.ToBinary());
                                bw.Write(disease.IncubationEndsUtc.ToBinary());
                                bw.Write(disease.NextPulseUtc.ToBinary());
                                bw.Write(disease.RecoveryCount);
                                bw.Write(disease.Cured);
                            }

                            bw.Write(profile.Immunities.Count);
                            for (int i = 0; i < profile.Immunities.Count; i++)
                            {
                                OSUImmunityState immunity = profile.Immunities[i];
                                bw.Write((int)immunity.Disease);
                                bw.Write(immunity.SourceId ?? String.Empty);
                                bw.Write(immunity.ReductionScalar);
                                bw.Write(immunity.EndsUtc.ToBinary());
                            }
                        }

                        bw.Write(_contaminatedItems.Count);
                        foreach (KeyValuePair<int, OSUContaminatedItemState> kv in _contaminatedItems)
                        {
                            OSUContaminatedItemState state = kv.Value;
                            bw.Write(state.ItemSerial);
                            bw.Write((int)state.Disease);
                            bw.Write(state.ExpiresUtc.ToBinary());
                            bw.Write(state.SourceLabel ?? String.Empty);
                        }

                        bw.Write(_surgeryWeekStartUtc.ToBinary());
                        bw.Write(_weeklyHospitalSurgeries.Count);
                        foreach (KeyValuePair<int, int> kv in _weeklyHospitalSurgeries)
                        {
                            bw.Write(kv.Key);
                            bw.Write(kv.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[OSUHealth] Falha ao salvar save de health: " + ex);
                }
            }
        }

        private static void OnWorldLoad()
        {
            LoadStateFile();
        }

        private static void OnWorldSave(WorldSaveEventArgs e)
        {
            SaveStateFile();
        }

        #endregion

        #region Event Handlers

        private static void OnMovement(MovementEventArgs e)
        {
            if (e == null || e.Mobile == null || e.Blocked)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            OSUHealthProfile profile = GetProfile(pm, false);

            EnforceBrokenArmEquipment(pm);

            bool running = (e.Direction & Direction.Running) != 0;
            if (running && HasLegRunBlock(pm))
            {
                pm.SendMessage("Suas lesões não permitem correr.");
                e.Blocked = true;
                return;
            }

            if (profile != null)
            {
                if (profile.ComaUntilUtc > DateTime.UtcNow)
                    e.Blocked = true;
            }

            IsStandingInContaminatedArea(pm);
        }

        private static void OnSpeech(SpeechEventArgs e)
        {
            if (e == null || e.Mobile == null || String.IsNullOrWhiteSpace(e.Speech))
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            OSUHealthProfile profile = GetProfile(pm, false);
            bool isEmote = e.Speech.StartsWith("*") && e.Speech.EndsWith("*");

            if (BlocksOwnSpeech(pm) && !isEmote)
            {
                e.Blocked = true;
                pm.SendMessage("Você não consegue falar nesse estado. Você ainda pode usar emotes.");
                return;
            }

            if (HasJawSpeechBlock(pm) && !isEmote)
            {
                e.Blocked = true;
                pm.SendMessage("Sua mandíbula quebrada impede você de falar. Você ainda pode usar emotes.");
            }
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), delegate
            {
                if (pm == null || pm.Deleted)
                    return;

                OSUHealthProfile profile = GetProfile(pm, false);
                if (profile != null && (profile.HospitalStretcherSerial != 0 || profile.SurgeryStretcherSerial != 0 || profile.ComaUntilUtc > DateTime.UtcNow))
                {
                    ReapplyStretcherLay(pm, profile);
                    pm.Frozen = profile.ComaUntilUtc > DateTime.UtcNow;
                }

                ProcessOnlinePlayer(pm);
            });
        }

        private static void OnDisconnected(DisconnectedEventArgs e)
        {
            if (e == null || e.Mobile == null)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            if (GetProfile(pm, false) != null)
                SaveStateFile();
        }

        private class OSUHealthPulseTimer : Timer
        {
            public OSUHealthPulseTimer() : base(TimeSpan.FromSeconds(15.0), TimeSpan.FromSeconds(15.0))
            {
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                List<PlayerMobile> list = new List<PlayerMobile>();

                foreach (Mobile m in World.Mobiles.Values)
                {
                    PlayerMobile pm = m as PlayerMobile;
                    if (pm != null && !pm.Deleted && pm.NetState != null)
                        list.Add(pm);
                }

                for (int i = 0; i < list.Count; i++)
                    ProcessOnlinePlayer(list[i]);
            }
        }

        private class OSUHealthAutosaveTimer : Timer
        {
            public OSUHealthAutosaveTimer() : base(TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(2.0))
            {
                Priority = TimerPriority.OneMinute;
            }

            protected override void OnTick()
            {
                SaveStateFile();
            }
        }

        private class OSUSurgeryGumpRefreshTimer : Timer
        {
            public OSUSurgeryGumpRefreshTimer() : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                List<OSUSurgeryProgressState> states = new List<OSUSurgeryProgressState>();
                lock (_sync)
                    states.AddRange(_surgery.Values);

                for (int i = 0; i < states.Count; i++)
                {
                    OSUSurgeryProgressState state = states[i];
                    if (state == null)
                        continue;

                    PlayerMobile surgeon = World.FindMobile(state.SurgeonSerial) as PlayerMobile;
                    PlayerMobile patient = World.FindMobile(state.PatientSerial) as PlayerMobile;
                    if (patient == null || patient.Deleted)
                        continue;

                    if (DateTime.UtcNow >= state.DeadlineUtc)
                    {
                        EndSurgeryFailure(surgeon, patient, state, "O tempo da cirurgia acabou.");
                        continue;
                    }

                    if (surgeon == null || surgeon.Deleted || surgeon.NetState == null)
                        continue;

                    if (!surgeon.InRange(patient.Location, 2))
                    {
                        EndSurgeryFailure(surgeon, patient, state, "O cirurgião se afastou demais do paciente.");
                        continue;
                    }

                    surgeon.CloseGump(typeof(OSUSurgeryStatusGump));
                    surgeon.SendGump(new OSUSurgeryStatusGump(surgeon, patient, state));
                }
            }
        }

        #endregion

        #region Commands

        private static void OnGiveDiseaseCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [DarDoenca nomeDaDoenca");
                return;
            }

            OSUDiseaseType disease;
            if (!Enum.TryParse<OSUDiseaseType>(e.Arguments[0], true, out disease) || disease == OSUDiseaseType.None)
            {
                e.Mobile.SendMessage("Doença inválida.");
                return;
            }

            e.Mobile.Target = new GiveDiseaseTarget(disease);
            e.Mobile.SendMessage("Escolha um jogador, item ou fonte.");
        }

        private static void OnViewDiseasesCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            e.Mobile.Target = new ViewDiseaseTarget();
            e.Mobile.SendMessage("Escolha um jogador.");
        }

        private static void OnResetHealthCommand(CommandEventArgs e)
        {
            lock (_sync)
            {
                _profiles.Clear();
                _contaminatedItems.Clear();
                _surgery.Clear();
            }

            e.Mobile.SendMessage("Todos os dados de saúde foram resetados.");
        }

        private static void OnWipeHealthCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            e.Mobile.Target = new WipeHealthTarget();
            e.Mobile.SendMessage("Escolha um jogador.");
        }

        private static void OnGiveInjuryCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [DarLesao nomeDaLesao");
                return;
            }

            OSUInjuryType injury;
            if (!Enum.TryParse<OSUInjuryType>(e.Arguments[0], true, out injury) || injury == OSUInjuryType.None)
            {
                e.Mobile.SendMessage("Lesão inválida.");
                return;
            }

            e.Mobile.Target = new GiveInjuryTarget(injury);
            e.Mobile.SendMessage("Escolha um jogador.");
        }

        private static void OnViewHealthInfoCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            e.Mobile.Target = new ViewHealthInfoTarget();
            e.Mobile.SendMessage("Escolha um jogador.");
        }

        private static void OnOpenHealthGumpCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            e.Mobile.Target = new OpenHealthGumpTarget();
            e.Mobile.SendMessage("Escolha um jogador.");
        }

        private static void OnOpenSurgeryGumpCommand(CommandEventArgs e)
        {
            if (e.Mobile == null)
                return;

            e.Mobile.Target = new OpenSurgeryGumpTarget();
            e.Mobile.SendMessage("Escolha um paciente.");
        }

        private class GiveDiseaseTarget : Target
        {
            private readonly OSUDiseaseType _disease;

            public GiveDiseaseTarget(OSUDiseaseType disease) : base(12, false, TargetFlags.None)
            {
                _disease = disease;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Mobile mob = targeted as Mobile;
                if (mob != null)
                {
                    if (ApplyDisease(mob, _disease, true))
                        from.SendMessage("Doença aplicada.");
                    else
                        from.SendMessage("A doença não foi aplicada.");
                    return;
                }

                Item item = targeted as Item;
                if (item != null)
                {
                    ContaminateItem(item, _disease, TimeSpan.FromHours(12), "comando");
                    from.SendMessage("Item contaminado por 12 horas.");
                    return;
                }

                from.SendMessage("Alvo inválido.");
            }
        }

        private class GiveInjuryTarget : Target
        {
            private readonly OSUInjuryType _injury;

            public GiveInjuryTarget(OSUInjuryType injury) : base(12, false, TargetFlags.None)
            {
                _injury = injury;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Mobile mob = targeted as Mobile;
                if (mob == null)
                {
                    from.SendMessage("Escolha um jogador.");
                    return;
                }

                if (ApplyInjury(mob, _injury, true))
                    from.SendMessage("Lesão aplicada.");
                else
                    from.SendMessage("A lesão não foi aplicada.");
            }
        }

        private class ViewDiseaseTarget : Target
        {
            public ViewDiseaseTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;
                if (pm == null)
                {
                    from.SendMessage("Escolha um jogador.");
                    return;
                }

                OpenHealthStatusGump(from, pm);
            }
        }

        private class ViewHealthInfoTarget : Target
        {
            public ViewHealthInfoTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;
                if (pm == null)
                {
                    from.SendMessage("Escolha um jogador.");
                    return;
                }

                OpenHealthStatusGump(from, pm);
            }
        }

        private class OpenHealthGumpTarget : Target
        {
            public OpenHealthGumpTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;
                if (pm == null)
                {
                    from.SendMessage("Escolha um jogador.");
                    return;
                }

                OpenHealthStatusGump(from, pm);
            }
        }

        private class OpenSurgeryGumpTarget : Target
        {
            public OpenSurgeryGumpTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;
                if (pm == null)
                {
                    from.SendMessage("Escolha um paciente.");
                    return;
                }

                OpenSurgeryStatusGump(from, pm);
            }
        }

        private class WipeHealthTarget : Target
        {
            public WipeHealthTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Mobile m = targeted as Mobile;
                if (m == null)
                {
                    from.SendMessage("Escolha um jogador.");
                    return;
                }

                RemoveProfile(m.Serial.Value);
                from.SendMessage("Dados de saúde apagados.");
            }
        }

        #endregion
    }

    public interface IHospitalBoundItem
    {
        int CityId { get; }
        string ConstructionKey { get; }
    }
}
