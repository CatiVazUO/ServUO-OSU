using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Mobiles;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Network;
using Server.Custom.Systems.SkillXP;

namespace Server.Custom.Systems.Skills.Abilities
{
    public class RidingIAbility : IOSUAbility, IOSUAbilityModule
    {

        public OSUAbilityDefinition Definition { get; private set; }

        // Riding I não tem requisito
        public string RequirementText { get { return ""; } }
        public string CommandText { get { return ""; } }


        public RidingIAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: 200001,
                name: "Riding I",
                desc: "Você sabe montar, mas cai com frequencia",
                costPicks: 1,
                commandText: "riding",
                requiredFeatId: 0,
                requiredAbilityId: 0,
                iconId: 0,
                requirementTextOverride: "Riding I"
            );
        }

        public bool CanPurchase(PlayerMobile pm, out string reason)
        {
            reason = null;
            if (pm == null)
            {
                reason = "Erro interno.";
                return false;
            }
            return true;
        }

        public void OnPurchased(PlayerMobile pm)
        {
            // Você pediu que não dependesse de skill real.
            // Então aqui não precisa mexer em Skills.
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
            // sem comando
        }

        // ============================================================
        //  LOGICA DO JOGO (hook) - TUDO DENTRO DO ARQUIVO Riding I
        // ============================================================

        private static bool _hooked;
        private static readonly Dictionary<Serial, int> _steps = new Dictionary<Serial, int>();
        private static Timer _mountBlockTimer;
        private static readonly Dictionary<Serial, DateTime> _lastWarn = new Dictionary<Serial, DateTime>();

        public void InitializeModule()
        {

            // garante que só hooka 1 vez
            if (_hooked)
                return;

            _hooked = true;

            // tenta bloquear montar "de verdade" (antes de montar) se existir evento Mount na base
            TryHookMountEvent();

            // fallback e também controla quedas
            EventSink.Movement += OnMovement;
            EventSink.Login += OnLogin; // garante desmontar se logar montado sem Riding I

            StartMountBlockTimer();

        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            if (pm.Mount != null && !pm.HasOSUAbility(200001))
            {
                ForceDismount(pm, "Você não sabe montar.");
            }
        }

        private static void OnMovement(MovementEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null || pm.Deleted)
                return;

            if (pm.Mount == null)
            {
                _steps.Remove(pm.Serial);
                return;
            }

            // ✅ Sem Riding I = não pode montar (mensagem como “não é dono”)
            if (!pm.HasOSUAbility(200001))
            {
                ForceDismount(pm, "Você não sabe montar.");
                return;
            }

            // ✅ Com Riding II = nunca cai
            if (pm.HasOSUAbility(200002))
            {
                _steps[pm.Serial] = 0;
                return;
            }

            // OSU: Riding I sem Riding II só pode cair se estiver CORRENDO.
            // Se estiver andando, nunca cai e também reseta a contagem de passos.
            bool running = (e.Direction & Direction.Running) != 0;

            if (!running)
            {
                _steps[pm.Serial] = 0;
                return;
            }

            // ✅ Riding I sem Riding II = só testa queda correndo
            HandleAlwaysFall(pm);
        }

        private static void HandleAlwaysFall(PlayerMobile pm)
        {
            int s;
            _steps.TryGetValue(pm.Serial, out s);
            s++;
            _steps[pm.Serial] = s;

            // a cada 10 passos, rola a chance
            if (s <= 9)
                return;

            // reseta para testar de novo depois de mais 10 passos
            _steps[pm.Serial] = 0;

            // ✅ 40% de chance de cair
            // Utility.RandomDouble() retorna 0.0 até 0.999...
            if (Utility.RandomDouble() >= 0.40)
                return;

            IMount mount = pm.Mount;
            if (mount != null)
                mount.Rider = null;

            // dano + emote
            try
            {
                Spells.SpellHelper.Damage(TimeSpan.FromTicks(1), pm, pm, Utility.RandomMinMax(1, 6));
            }
            catch
            {
                pm.Damage(Utility.RandomMinMax(1, 6));
            }

            pm.Emote("* Cai de sua montaria *");
        }


        private static void ForceDismount(PlayerMobile pm, string msg)
        {
            IMount mount = pm.Mount;
            if (mount != null)
                mount.Rider = null;

            pm.SendMessage(0x22, msg);
            _steps.Remove(pm.Serial);
        }

        // ------------------------------------------------------------
        // Tentativa de bloquear montar antes (sem “subir nem 1 segundo”)
        // Usa reflection pra não quebrar se sua base não tiver esse evento
        // ------------------------------------------------------------
        private static void TryHookMountEvent()
        {
            try
            {
                var evt = typeof(EventSink).GetEvent("Mount");
                if (evt == null)
                    return;

                var handler = typeof(RidingIAbility).GetMethod("OnMount_Reflection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var del = Delegate.CreateDelegate(evt.EventHandlerType, handler);
                evt.AddEventHandler(null, del);
            }
            catch
            {
                // se não existir, ok: o fallback do Movement já desmonta na hora
            }
        }

        private static void OnMount_Reflection(object e)
        {
            try
            {
                var t = e.GetType();

                var mobileProp = t.GetProperty("Mobile");
                var mountProp = t.GetProperty("Mount");

                Mobile m = mobileProp != null ? mobileProp.GetValue(e, null) as Mobile : null;
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null)
                    return;

                if (pm.HasOSUAbility(200001))
                    return;

                pm.SendMessage(0x22, "Você não sabe montar.");

                // tenta cancelar (nomes variam por base)
                SetBoolIfExists(e, t, "Blocked", true);
                SetBoolIfExists(e, t, "Cancel", true);
                SetBoolIfExists(e, t, "AllowMount", false);

                IMount mount = mountProp != null ? mountProp.GetValue(e, null) as IMount : null;
                if (mount != null && mount.Rider == pm)
                    mount.Rider = null;
            }
            catch
            {
            }
        }

        private static void SetBoolIfExists(object obj, Type t, string propName, bool value)
        {
            var p = t.GetProperty(propName);
            if (p != null && p.CanWrite && p.PropertyType == typeof(bool))
                p.SetValue(obj, value, null);
        }

        private static void StartMountBlockTimer()
        {
            if (_mountBlockTimer != null)
                return;

            // checa 4x por segundo (bem rápido, mas leve)
            _mountBlockTimer = Timer.DelayCall(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250), CheckMountedPlayers);
        }

        private static void CheckMountedPlayers()
        {
            // percorre todos os players online
            foreach (NetState ns in NetState.Instances)
            {
                if (ns == null)
                    continue;

                PlayerMobile pm = ns.Mobile as PlayerMobile;
                if (pm == null || pm.Deleted)
                    continue;

                if (pm.Mount == null)
                    continue;

                // sem Riding I: desmonta SEM PRECISAR ANDAR
                if (!pm.HasOSUAbility(200001))
                {
                    // evita spam de msg
                    DateTime last;
                    _lastWarn.TryGetValue(pm.Serial, out last);

                    if (DateTime.UtcNow - last > TimeSpan.FromSeconds(2))
                    {
                        _lastWarn[pm.Serial] = DateTime.UtcNow;
                        ForceDismount(pm, "Você não sabe montar.");
                    }
                    else
                    {
                        // desmonta mesmo sem mensagem repetida
                        IMount mount = pm.Mount;
                        if (mount != null)
                            mount.Rider = null;
                    }
                }
            }
        }

    }
}
