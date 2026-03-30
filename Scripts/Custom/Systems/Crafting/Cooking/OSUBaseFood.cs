using System;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.WorldTime;
using Server.Custom.Systems.Needs;

namespace Server.Custom.Systems.Crafting.Cooking
{
    public enum OSUFoodFreshness
    {
        Fresh = 0,
        Moldy = 1,
        Spoiled = 2
    }

    // Base para TODAS as comidas do OSU.
    // Não mexe no Food.cs padrão do core.
    public abstract class OSUBaseFood : Item
    {
        private int _fillFactor;
        private int _hotHpPerTick;
        private int _hotStamPerTick;
        private int _hotManaPerTick;
        private int _decomposeDays;

        private DateTime _createdWorldTime;
        private OSUFoodFreshness _freshness;

        [CommandProperty(AccessLevel.GameMaster)]
        public int FillFactor
        {
            get => _fillFactor;
            set => _fillFactor = Math.Max(0, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HotHpPerTick
        {
            get => _hotHpPerTick;
            set => _hotHpPerTick = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HotStamPerTick
        {
            get => _hotStamPerTick;
            set => _hotStamPerTick = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HotManaPerTick
        {
            get => _hotManaPerTick;
            set => _hotManaPerTick = value;
        }

        // Dias até estragar (0 = nunca estraga)
        [CommandProperty(AccessLevel.GameMaster)]
        public int DecomposeDays
        {
            get => _decomposeDays;
            set => _decomposeDays = Math.Max(0, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUFoodFreshness Freshness => _freshness;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime CreatedWorldTime => _createdWorldTime;

        protected OSUBaseFood(int itemID) : base(itemID)
        {
            _createdWorldTime = OSUWorldTime.WorldNow;
            _freshness = OSUFoodFreshness.Fresh;
            StartDecayTimer();
        }

        public OSUBaseFood(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (_freshness == OSUFoodFreshness.Moldy)
                list.Add("Mofado");
            else if (_freshness == OSUFoodFreshness.Spoiled)
                list.Add("Estragado");

            if (_fillFactor > 0)
                list.Add($"Enche: {_fillFactor}");

            if (_decomposeDays > 0)
            {
                var remaining = GetRemaining();
                if (remaining > TimeSpan.Zero)
                    list.Add($"Estraga em: {Math.Ceiling(remaining.TotalDays)} dia(s)");
                else
                    list.Add("Estragado");
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsAccessibleTo(from))
                return;

            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            UpdateFreshness();

            // Regra de "espaço"
            if (_fillFactor > 0)
            {
                if (!OSUNeedsSystem.TryAddHunger(pm, _fillFactor))
                {
                    pm.SendMessage("Você está cheio demais para comer isso agora.");
                    return;
                }
            }

            // Se estragado: 50% chance de doença (placeholder)
            if (_freshness == OSUFoodFreshness.Spoiled)
            {
                if (OSUDiseaseHooks.ShouldBecomeSick(pm, this))
                {
                    pm.SendMessage("Você se sente mal após comer essa comida estragada...");
                    OSUDiseaseHooks.NotifySpoiledConsumed(pm, this);
                }
            }

            // Efeito over time (tick 5 min) por FillFactor minutos
            if (_fillFactor > 0 && (_hotHpPerTick != 0 || _hotStamPerTick != 0 || _hotManaPerTick != 0))
            {
                var duration = TimeSpan.FromMinutes(_fillFactor);
                new HotTimer(pm, duration, _hotHpPerTick, _hotStamPerTick, _hotManaPerTick).Start();
            }

            pm.SendMessage("Você come a comida.");
            Consume();
        }

        public new void Consume()
        {
            if (Stackable && Amount > 1)
                Amount--;
            else
                Delete();
        }

        private void StartDecayTimer()
        {
            if (_decomposeDays <= 0)
                return;

            // checa a cada 30 min
            Timer.DelayCall(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30), UpdateFreshness);
        }

        private TimeSpan GetRemaining()
        {
            if (_decomposeDays <= 0)
                return TimeSpan.MaxValue;

            var endWorld = _createdWorldTime.AddDays(_decomposeDays);
            return endWorld - OSUWorldTime.WorldNow;
        }

        private void UpdateFreshness()
        {
            if (Deleted)
                return;

            if (_decomposeDays <= 0)
                return;

            var remaining = GetRemaining();

            var newState = _freshness;

            if (remaining <= TimeSpan.Zero)
                newState = OSUFoodFreshness.Spoiled;
            else if (remaining <= TimeSpan.FromDays(1))
                newState = OSUFoodFreshness.Moldy;
            else
                newState = OSUFoodFreshness.Fresh;

            if (newState != _freshness)
            {
                _freshness = newState;
                InvalidateProperties();
            }
        }


        private static DateTime ConvertUtcToWorld(DateTime createdUtc)
        {
            // Converte "idade real" do item para o relógio do mundo:
            // worldCreated = WorldNow - (UtcNow - createdUtc)
            try
            {
                TimeSpan ageReal = DateTime.UtcNow - createdUtc;
                return OSUWorldTime.WorldNow - ageReal;
            }
            catch
            {
                return OSUWorldTime.WorldNow;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write(_fillFactor);
            writer.Write(_hotHpPerTick);
            writer.Write(_hotStamPerTick);
            writer.Write(_hotManaPerTick);
            writer.Write(_decomposeDays);
            writer.Write(_createdWorldTime);
            writer.Write((int)_freshness);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            _fillFactor = reader.ReadInt();
            _hotHpPerTick = reader.ReadInt();
            _hotStamPerTick = reader.ReadInt();
            _hotManaPerTick = reader.ReadInt();
            _decomposeDays = reader.ReadInt();

            if (version >= 1)
            {
                _createdWorldTime = reader.ReadDateTime();
            }
            else
            {
                // Compatibilidade com itens antigos (salvos com UTC)
                DateTime createdUtc = reader.ReadDateTime();
                _createdWorldTime = ConvertUtcToWorld(createdUtc);
            }

            _freshness = (OSUFoodFreshness)reader.ReadInt();

            StartDecayTimer();
        }

        private class HotTimer : Timer
        {
            private readonly PlayerMobile _pm;
            private readonly int _hp;
            private readonly int _stam;
            private readonly int _mana;
            private int _ticksLeft;

            public HotTimer(PlayerMobile pm, TimeSpan duration, int hp, int stam, int mana)
                : base(TimeSpan.FromMinutes(5.0), TimeSpan.FromMinutes(5.0))
            {
                _pm = pm;
                _hp = hp;
                _stam = stam;
                _mana = mana;

                _ticksLeft = Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes / 5.0));
            }

            protected override void OnTick()
            {
                if (_pm == null || _pm.Deleted || _pm.NetState == null)
                {
                    Stop();
                    return;
                }

                if (_hp != 0)
                    _pm.Hits = Math.Min(_pm.HitsMax, _pm.Hits + _hp);

                if (_stam != 0)
                    _pm.Stam = Math.Min(_pm.StamMax, _pm.Stam + _stam);

                if (_mana != 0)
                    _pm.Mana = Math.Min(_pm.ManaMax, _pm.Mana + _mana);

                _ticksLeft--;
                if (_ticksLeft <= 0)
                    Stop();
            }
        }
    }
}
