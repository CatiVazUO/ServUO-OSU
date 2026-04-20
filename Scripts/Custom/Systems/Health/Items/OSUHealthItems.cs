using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Engines.Craft;
using Server.Custom.Reinos;
using Server.Custom.OSUDrag;
using Server.Custom.Systems.Health;
using Server.Custom.Systems.Health.Gumps;

namespace Server.Items
{
    public class OSUBloodyBandage : Item
    {
        [Constructable]
        public OSUBloodyBandage() : this(1)
        {
        }

        [Constructable]
        public OSUBloodyBandage(int amount) : base(0xE22)
        {
            Stackable = true;
            Amount = amount;
            Name = "bandagens sujas";
            Hue = 0;
            Weight = 0.1;
        }

        public OSUBloodyBandage(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendMessage("Você está longe demais.");
                return;
            }

            from.SendMessage("Escolha uma fonte de água para lavar as bandagens.");
            from.Target = new BloodyBandageWashTarget(this);
        }

        private class BloodyBandageWashTarget : Target
        {
            private readonly OSUBloodyBandage _item;

            public BloodyBandageWashTarget(OSUBloodyBandage item) : base(3, false, TargetFlags.None)
            {
                _item = item;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (_item == null || _item.Deleted)
                    return;

                Point3D p;
                Map map = from.Map;

                Item item = targeted as Item;
                if (item != null)
                    p = item.GetWorldLocation();
                else if (targeted is IPoint3D)
                    p = new Point3D(((IPoint3D)targeted).X, ((IPoint3D)targeted).Y, ((IPoint3D)targeted).Z);
                else
                {
                    from.SendMessage("Isso não serve como água.");
                    return;
                }

                if (!OSUHealthSystem.IsWetTile(map, p.X, p.Y) && (item == null || !OSUHealthSystem.IsContaminated(item)))
                {
                    from.SendMessage("Essa não parece ser uma fonte de água.");
                    return;
                }

                Bandage clean = new Bandage(_item.Amount);
                if (from.Backpack != null)
                    from.Backpack.DropItem(clean);
                else
                    clean.MoveToWorld(from.Location, from.Map);

                from.PlaySound(0x240);
                from.SendMessage("Você lava as bandagens e elas voltam ao normal.");
                _item.Delete();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class OSUMedicatedBandage : Bandage
    {
        private OSUMedicatedBandageType _medicineType;
        private int _extraHealBonus;
        private int _extraPoisonBleedChance;

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUMedicatedBandageType MedicineType { get { return _medicineType; } set { _medicineType = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ExtraHealBonus { get { return _extraHealBonus; } set { _extraHealBonus = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ExtraPoisonBleedChance { get { return _extraPoisonBleedChance; } set { _extraPoisonBleedChance = value; } }

        [Constructable]
        public OSUMedicatedBandage() : this(1, OSUMedicatedBandageType.HealingBonus, 653)
        {
        }

        [Constructable]
        public OSUMedicatedBandage(int amount, OSUMedicatedBandageType type, int hue) : base(amount)
        {
            _medicineType = type;
            Hue = hue;
            Name = "bandagens com remédio";

            switch (type)
            {
                case OSUMedicatedBandageType.HealingBonus:
                    _extraHealBonus = 5;
                    break;
                case OSUMedicatedBandageType.Antiseptic:
                    _extraPoisonBleedChance = 8;
                    break;
            }
        }

        public OSUMedicatedBandage(Serial serial) : base(serial)
        {
        }

        public override bool StackWith(Mobile from, Item item, bool playSound)
        {
            OSUMedicatedBandage other = item as OSUMedicatedBandage;
            if (other == null)
                return false;

            return other._medicineType == _medicineType && other.Hue == Hue && base.StackWith(from, item, playSound);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write((int)_medicineType);
            writer.Write(_extraHealBonus);
            writer.Write(_extraPoisonBleedChance);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _medicineType = (OSUMedicatedBandageType)reader.ReadInt();
            _extraHealBonus = reader.ReadInt();
            _extraPoisonBleedChance = reader.ReadInt();
            Name = "bandagens com remédio";
        }
    }

    public class OSUMedicationTub : DyeTub, IHospitalBoundItem
    {
        private int _cityId;
        private string _constructionKey;
        private OSUMedicatedBandageType _medicineType;
        private int _costPerBandage;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return _cityId; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return _constructionKey; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUMedicatedBandageType MedicineType { get { return _medicineType; } set { _medicineType = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CostPerBandage { get { return _costPerBandage; } set { _costPerBandage = value; } }

        [Constructable]
        public OSUMedicationTub() : this(0, String.Empty, OSUMedicatedBandageType.HealingBonus, 1150, 10)
        {
        }

        public OSUMedicationTub(int cityId, string constructionKey, OSUMedicatedBandageType type, int hue, int costPerBandage) : base()
        {
            _cityId = cityId;
            _constructionKey = constructionKey ?? String.Empty;
            _medicineType = type;
            _costPerBandage = costPerBandage;
            DyedHue = hue;
            Movable = false;
            Redyable = false;
            Name = "cuba medicinal";
        }

        public string GetEffectDescription()
        {
            switch (_medicineType)
            {
                case OSUMedicatedBandageType.HealingBonus:
                    return "<BASEFONT COLOR=#FFFFFF>Esse remédio dá um pequeno bônus de cura por bandagem usada.</BASEFONT>";
                case OSUMedicatedBandageType.SpeedBonus:
                    return "<BASEFONT COLOR=#FFFFFF>Esse remédio faz a bandagem agir um pouco mais rápido.</BASEFONT>";
                case OSUMedicatedBandageType.Antiseptic:
                    return "<BASEFONT COLOR=#FFFFFF>Esse remédio aumenta um pouco a chance de parar poison ou bleed.</BASEFONT>";
                default:
                    return "<BASEFONT COLOR=#FFFFFF>Remédio sem efeito configurado.</BASEFONT>";
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 2))
            {
                pm.SendMessage("Você está longe demais.");
                return;
            }

            pm.SendMessage("Escolha uma pilha de bandagens limpas.");
            pm.Target = new MedicationTubTarget(this);
        }

        public bool TryDipBandages(PlayerMobile from, Bandage bandage)
        {
            if (from == null || bandage == null || bandage.Deleted)
                return false;

            if (bandage is OSUMedicatedBandage)
            {
                from.SendMessage("Essas bandagens já têm remédio.");
                return false;
            }

            int total = Math.Max(1, bandage.Amount) * _costPerBandage;

            if (from.Backpack == null || !from.Backpack.ConsumeTotal(typeof(Gold), total))
            {
                from.SendMessage("Você não tem moedas suficientes.");
                return false;
            }

            if (_cityId >= 0)
                ReinoTreasurySystem.RecordDonationToKingdom(_cityId, total, 0, 0, 0);

            OSUMedicatedBandage med = new OSUMedicatedBandage(bandage.Amount, _medicineType, DyedHue);
            if (from.Backpack != null)
                from.Backpack.DropItem(med);
            else
                med.MoveToWorld(from.Location, from.Map);

            from.PlaySound(0x025);
            from.SendMessage("As bandagens foram embebidas no remédio.");
            bandage.Delete();
            return true;
        }

        private class MedicationTubTarget : Target
        {
            private readonly OSUMedicationTub _tub;

            public MedicationTubTarget(OSUMedicationTub tub) : base(2, false, TargetFlags.None)
            {
                _tub = tub;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                Bandage bandage = targeted as Bandage;
                if (pm == null || bandage == null)
                {
                    from.SendMessage("Escolha uma pilha de bandagens.");
                    return;
                }

                pm.SendGump(new OSUMedicationConfirmGump(pm, _tub, bandage));
            }
        }

        public OSUMedicationTub(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(_cityId);
            writer.Write(_constructionKey ?? String.Empty);
            writer.Write((int)_medicineType);
            writer.Write(_costPerBandage);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _cityId = reader.ReadInt();
            _constructionKey = reader.ReadString();
            _medicineType = (OSUMedicatedBandageType)reader.ReadInt();
            _costPerBandage = reader.ReadInt();
            Movable = false;
            Redyable = false;
        }
    }



    public class OSUHospitalStretcherCompanion : Item, IHospitalBoundItem
    {
        private int _cityId;
        private string _constructionKey;
        private int _ownerSerial;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return _cityId; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return _constructionKey; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int OwnerSerial { get { return _ownerSerial; } }

        [Constructable]
        public OSUHospitalStretcherCompanion() : this(0xA61, 0, String.Empty, 0, 0)
        {
        }

        public OSUHospitalStretcherCompanion(int itemId, int cityId, string constructionKey, int ownerSerial, int hue) : base(itemId)
        {
            _cityId = cityId;
            _constructionKey = constructionKey ?? String.Empty;
            _ownerSerial = ownerSerial;
            Hue = 0;
            Movable = false;
        }

        public OSUHospitalStretcherCompanion(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            OSUBaseHospitalStretcher owner = World.FindItem(_ownerSerial) as OSUBaseHospitalStretcher;
            if (owner == null || owner.Deleted)
            {
                from.SendMessage("Essa maca não pode ser usada agora.");
                return;
            }

            owner.OnDoubleClick(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(_cityId);
            writer.Write(_constructionKey ?? String.Empty);
            writer.Write(_ownerSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _cityId = reader.ReadInt();
            _constructionKey = reader.ReadString();
            _ownerSerial = reader.ReadInt();
            Movable = false;
        }
    }

    public abstract class OSUBaseHospitalStretcher : Item, IHospitalBoundItem
    {
        protected int _cityId;
        protected string _constructionKey;
        protected int _occupantSerial;
        protected int _companionSerial;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return _cityId; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return _constructionKey; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int OccupantSerial { get { return _occupantSerial; } }

        protected abstract bool IsRecoveryStretcher { get; }
        protected abstract int CompanionItemId { get; }
        protected abstract int StretcherHue { get; }
        protected abstract int LieZOffset { get; }
        protected virtual int LieXOffset { get { return 1; } }
        protected virtual int LieYOffset { get { return 0; } }
        protected virtual Direction LieDirection { get { return Direction.East; } }
        protected abstract string EmptyUseMessage { get; }

        protected OSUBaseHospitalStretcher(int itemId, int cityId, string constructionKey, string name) : base(itemId)
        {
            _cityId = cityId;
            _constructionKey = constructionKey ?? String.Empty;
            Movable = false;
            Name = name;
            Hue = StretcherHue;
        }

        public OSUBaseHospitalStretcher(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!pm.InRange(GetWorldLocation(), 1))
            {
                pm.SendMessage("Você está longe demais da maca.");
                return;
            }

            if (_occupantSerial == 0)
            {
                OnEmptyDoubleClick(pm);
                return;
            }

            PlayerMobile occupant = World.FindMobile(_occupantSerial) as PlayerMobile;

            if (occupant == null || occupant.Deleted)
            {
                _occupantSerial = 0;
                OnEmptyDoubleClick(pm);
                return;
            }

            if (pm == occupant)
            {
                OnOccupantDoubleClick(pm);
                return;
            }

            OnOccupiedByOtherDoubleClick(pm, occupant);
        }

        protected virtual void OnEmptyDoubleClick(PlayerMobile pm)
        {
            ForceLay(pm);
        }

        protected virtual void OnOccupantDoubleClick(PlayerMobile pm)
        {
            ForceStandUp(pm);
        }

        protected virtual void OnOccupiedByOtherDoubleClick(PlayerMobile actor, PlayerMobile occupant)
        {
            actor.SendMessage("Essa maca já está ocupada.");
        }

        protected virtual void EnsureCompanion()
        {
            if (Map == null || Map == Map.Internal)
                return;

            Item existing = World.FindItem(_companionSerial);
            if (existing != null && !existing.Deleted)
                return;

            OSUHospitalStretcherCompanion comp = new OSUHospitalStretcherCompanion(CompanionItemId, _cityId, _constructionKey, this.Serial.Value, StretcherHue);
            comp.MoveToWorld(new Point3D(X + 1, Y, Z), Map);
            _companionSerial = comp.Serial.Value;
        }

        protected virtual void DeleteCompanion()
        {
            Item existing = World.FindItem(_companionSerial);
            if (existing != null && !existing.Deleted)
                existing.Delete();
        }

        protected virtual void ForceLay(PlayerMobile pm)
        {
            if (pm == null)
                return;

            _occupantSerial = pm.Serial.Value;

            OSUHealthProfile profile = OSUHealthSystem.GetProfile(pm, true);
            if (IsRecoveryStretcher)
                profile.HospitalStretcherSerial = this.Serial.Value;
            else
                profile.SurgeryStretcherSerial = this.Serial.Value;

            pm.Direction = LieDirection;
            pm.Blessed = false;
            OSUDragSystem.ForceLayDown(pm, this, LieXOffset, LieYOffset, LieZOffset);
            pm.Frozen = false;
            pm.SendMessage(EmptyUseMessage);
        }

        public virtual void ReceiveTransferredOccupant(PlayerMobile pm)
        {
            if (pm == null)
                return;

            _occupantSerial = pm.Serial.Value;

            OSUHealthProfile profile = OSUHealthSystem.GetProfile(pm, true);
            profile.PortableStretcherSerial = 0;
            if (IsRecoveryStretcher)
                profile.HospitalStretcherSerial = this.Serial.Value;
            else
                profile.SurgeryStretcherSerial = this.Serial.Value;

            pm.Blessed = false;
            pm.Direction = LieDirection;
            OSUDragSystem.ForceLayDown(pm, this, LieXOffset, LieYOffset, LieZOffset);
            pm.Frozen = false;
        }

        public virtual void ForceStandUp(PlayerMobile pm)
        {
            if (pm == null)
                return;

            _occupantSerial = 0;

            OSUHealthProfile profile = OSUHealthSystem.GetProfile(pm, false);
            if (profile != null)
            {
                if (IsRecoveryStretcher && profile.HospitalStretcherSerial == this.Serial.Value)
                    profile.HospitalStretcherSerial = 0;
                if (!IsRecoveryStretcher && profile.SurgeryStretcherSerial == this.Serial.Value)
                    profile.SurgeryStretcherSerial = 0;
            }

            if (!OSUHealthSystem.ShouldRemainLying(pm))
            {
                pm.Frozen = false;
                OSUDragSystem.ReleaseForcedLay(pm);
            }

            pm.SendMessage("Você se levanta da maca.");
        }

        protected OSUHospitalRecoveryStretcher FindFreeRecoveryStretcher()
        {
            foreach (Item item in World.Items.Values)
            {
                OSUHospitalRecoveryStretcher recovery = item as OSUHospitalRecoveryStretcher;
                if (recovery == null || recovery.Deleted)
                    continue;

                if (recovery._occupantSerial != 0)
                    continue;

                if (recovery.Map != Map)
                    continue;

                if (recovery._cityId != _cityId)
                    continue;

                if (!String.Equals(recovery._constructionKey ?? String.Empty, _constructionKey ?? String.Empty, StringComparison.OrdinalIgnoreCase))
                    continue;

                return recovery;
            }

            return null;
        }

        protected OSUSurgeryStretcher FindFreeSurgeryStretcher()
        {
            foreach (Item item in World.Items.Values)
            {
                OSUSurgeryStretcher surgery = item as OSUSurgeryStretcher;
                if (surgery == null || surgery.Deleted)
                    continue;

                if (surgery._occupantSerial != 0)
                    continue;

                if (surgery.Map != Map)
                    continue;

                if (surgery._cityId != _cityId)
                    continue;

                if (!String.Equals(surgery._constructionKey ?? String.Empty, _constructionKey ?? String.Empty, StringComparison.OrdinalIgnoreCase))
                    continue;

                return surgery;
            }

            return null;
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            EnsureCompanion();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);

            PlayerMobile occ = World.FindMobile(_occupantSerial) as PlayerMobile;
            if (occ != null && !occ.Deleted && Map != null)
            {
                occ.Direction = LieDirection;
                occ.Frozen = true;
                OSUDragSystem.ForceLayDown(occ, this, LieXOffset, LieYOffset, LieZOffset);
            }
        }

        public override void OnAfterDelete()
        {
            DeleteCompanion();

            Mobile mob = World.FindMobile(_occupantSerial);
            PlayerMobile pm = mob as PlayerMobile;
            if (pm != null)
                ForceStandUp(pm);

            base.OnAfterDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2);
            writer.Write(_cityId);
            writer.Write(_constructionKey ?? String.Empty);
            writer.Write(_occupantSerial);
            writer.Write(_companionSerial);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            _cityId = reader.ReadInt();
            _constructionKey = reader.ReadString();
            _occupantSerial = reader.ReadInt();
            _companionSerial = version >= 2 ? reader.ReadInt() : 0;
            Movable = false;
            Hue = StretcherHue;
            Timer.DelayCall(TimeSpan.FromSeconds(0.2), EnsureCompanion);
        }
    }


    public class OSUHospitalRecoveryStretcher : OSUBaseHospitalStretcher
    {
        protected override bool IsRecoveryStretcher { get { return true; } }
        protected override int CompanionItemId { get { return 2657; } }
        protected override int StretcherHue { get { return 0; } }
        protected override int LieZOffset { get { return 0; } }
        protected override string EmptyUseMessage { get { return "Você se deita na maca de recuperação."; } }

        [Constructable]
        public OSUHospitalRecoveryStretcher() : this(0, String.Empty)
        {
        }

        public OSUHospitalRecoveryStretcher(int cityId, string constructionKey)
            : base(2656, cityId, constructionKey, "maca de recuperação")
        {
        }

        public OSUHospitalRecoveryStretcher(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }

        protected override void OnOccupiedByOtherDoubleClick(PlayerMobile actor, PlayerMobile occupant)
        {
            if (actor == null || occupant == null || occupant.Deleted)
            {
                actor.SendMessage("Essa maca já está ocupada.");
                return;
            }

            if (actor.AccessLevel < AccessLevel.GameMaster && !OSUHospitalAccess.CanAccessHospital(actor, _cityId, _constructionKey))
            {
                actor.SendMessage("Você não pode mover esse paciente.");
                return;
            }

            OSUSurgeryStretcher surgery = FindFreeSurgeryStretcher();
            if (surgery == null)
            {
                actor.SendMessage("Não há uma maca cirúrgica livre.");
                return;
            }

            _occupantSerial = 0;

            OSUHealthProfile profile = OSUHealthSystem.GetProfile(occupant, false);
            if (profile != null && profile.HospitalStretcherSerial == this.Serial.Value)
                profile.HospitalStretcherSerial = 0;

            surgery.ReceiveTransferredOccupant(occupant);
            actor.SendMessage("O paciente foi movido para a maca cirúrgica.");
        }
    }

    public class OSUHospitalStretcher : OSUHospitalRecoveryStretcher
    {
        [Constructable]
        public OSUHospitalStretcher() : base()
        {
        }

        public OSUHospitalStretcher(int cityId, string constructionKey) : base(cityId, constructionKey)
        {
        }

        public OSUHospitalStretcher(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUSurgeryStretcher : OSUBaseHospitalStretcher
    {
        protected override bool IsRecoveryStretcher { get { return false; } }
        protected override int CompanionItemId { get { return 4606; } }
        protected override int StretcherHue { get { return 2101; } }
        protected override int LieZOffset { get { return 2; } }
        protected override string EmptyUseMessage { get { return "Você se deita na maca cirúrgica."; } }

        [Constructable]
        public OSUSurgeryStretcher() : this(0, String.Empty)
        {
        }

        public OSUSurgeryStretcher(int cityId, string constructionKey)
            : base(4605, cityId, constructionKey, "maca cirúrgica")
        {
        }

        public OSUSurgeryStretcher(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }

        protected override void OnOccupantDoubleClick(PlayerMobile pm)
        {
            OSUHealthProfile profile = OSUHealthSystem.GetProfile(pm, false);
            if (profile != null && profile.ComaUntilUtc > DateTime.UtcNow)
                return;

            if (OSUHealthSystem.GetSurgeryState(pm) != null)
            {
                pm.SendMessage("Você não pode sair da maca enquanto a cirurgia estiver em andamento.");
                return;
            }

            ForceStandUp(pm);
        }

        protected override void OnOccupiedByOtherDoubleClick(PlayerMobile actor, PlayerMobile occupant)
        {
            if (occupant == null || occupant.Deleted)
            {
                _occupantSerial = 0;
                actor.SendMessage("A maca não está ocupada.");
                return;
            }

            if (actor.AccessLevel < AccessLevel.GameMaster && !OSUHospitalAccess.CanAccessHospital(actor, _cityId, _constructionKey))
            {
                actor.SendMessage("Você não pode usar a maca cirúrgica.");
                return;
            }

            OSUHospitalRecoveryStretcher recovery = FindFreeRecoveryStretcher();
            if (recovery == null)
            {
                actor.SendMessage("Não há uma maca de recuperação livre.");
                return;
            }

            _occupantSerial = 0;

            OSUHealthProfile profile = OSUHealthSystem.GetProfile(occupant, false);
            if (profile != null && profile.SurgeryStretcherSerial == this.Serial.Value)
                profile.SurgeryStretcherSerial = 0;

            OSUHealthSystem.CancelSurgeryForPatient(occupant, "A cirurgia foi interrompida porque o paciente foi retirado da maca cirúrgica.");
            recovery.ReceiveTransferredOccupant(occupant);
            actor.SendMessage("O paciente foi movido para uma maca de recuperação.");
        }
    }

    public class OSUDiseaseSource : Item
    {
        private OSUDiseaseType _disease;
        private int _radius;
        private bool _active;
        private bool _affectsWetTilesOnly;

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUDiseaseType Disease { get { return _disease; } set { _disease = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Radius { get { return _radius; } set { _radius = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active { get { return _active; } set { _active = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AffectsWetTilesOnly { get { return _affectsWetTilesOnly; } set { _affectsWetTilesOnly = value; InvalidateProperties(); } }

        [Constructable]
        public OSUDiseaseSource() : base(0x1B72)
        {
            Visible = false;
            Movable = false;
            _active = true;
            _radius = 2;
            _disease = OSUDiseaseType.Influenza;
            Name = "fonte de doença";
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Doença: " + OSUHealthSystem.GetDisplayName(_disease));
            list.Add("Raio: " + _radius);
            list.Add(_active ? "Ativo" : "Inativo");
            list.Add(_affectsWetTilesOnly ? "Só em tiles molhados" : "Área inteira");
        }

        public OSUDiseaseSource(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write((int)_disease);
            writer.Write(_radius);
            writer.Write(_active);
            writer.Write(_affectsWetTilesOnly);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _disease = (OSUDiseaseType)reader.ReadInt();
            _radius = reader.ReadInt();
            _active = reader.ReadBool();
            _affectsWetTilesOnly = reader.ReadBool();
            Visible = false;
            Movable = false;
        }
    }

    public class OSUMedicalBag : Item
    {
        [Constructable]
        public OSUMedicalBag() : base(0x098F)
        {
            Name = "maleta médica";
            Weight = 2.0;
        }

        public OSUMedicalBag(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!OSUHealthSystem.CanUseMedicalExam(from))
            {
                from.SendMessage("Você precisa da feat Exame Médico.");
                return;
            }

            from.SendMessage("Quem você deseja examinar?");
            from.Target = new MedicalBagTarget();
        }

        private class MedicalBagTarget : Target
        {
            public MedicalBagTarget() : base(10, false, TargetFlags.Beneficial)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile patient = targeted as PlayerMobile;
                if (patient == null)
                {
                    from.SendMessage("Escolha um jogador.");
                    return;
                }

                OSUHealthSystem.OpenHealthStatusGump(from, patient);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class OSUSurgeryDecorItem : Item, IHospitalBoundItem
    {
        private int _cityId;
        private string _constructionKey;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return _cityId; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return _constructionKey; } }

        [Constructable]
        public OSUSurgeryDecorItem() : this(0x1801, 0, String.Empty, "decoração cirúrgica", 0)
        {
        }

        [Constructable]
        public OSUSurgeryDecorItem(int itemId, int cityId, string constructionKey, string name, int hue)
            : base(itemId)
        {
            _cityId = cityId;
            _constructionKey = constructionKey ?? String.Empty;
            Name = name;
            Movable = false;
            Hue = hue;
        }

        public OSUSurgeryDecorItem(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(_cityId);
            writer.Write(_constructionKey ?? String.Empty);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _cityId = reader.ReadInt();
            _constructionKey = reader.ReadString();
            Movable = false;
        }
    }

    public class OSUMesaCirurgicaNorte : OSUSurgeryDecorItem
    {
        [Constructable]
        public OSUMesaCirurgicaNorte() : this(0, String.Empty)
        {
        }

        public OSUMesaCirurgicaNorte(int cityId, string constructionKey)
            : base(4610, cityId, constructionKey, "mesa cirúrgica", 2101)
        {
        }

        public OSUMesaCirurgicaNorte(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUMesaCirurgicaCentro : OSUSurgeryDecorItem
    {
        [Constructable]
        public OSUMesaCirurgicaCentro() : this(0, String.Empty)
        {
        }

        public OSUMesaCirurgicaCentro(int cityId, string constructionKey)
            : base(4611, cityId, constructionKey, "mesa cirúrgica", 2101)
        {
        }

        public OSUMesaCirurgicaCentro(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUMesaCirurgicaSul : OSUSurgeryDecorItem
    {
        [Constructable]
        public OSUMesaCirurgicaSul() : this(0, String.Empty)
        {
        }

        public OSUMesaCirurgicaSul(int cityId, string constructionKey)
            : base(4609, cityId, constructionKey, "mesa cirúrgica", 2101)
        {
        }

        public OSUMesaCirurgicaSul(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUSurgeryToolItem : Item, IHospitalBoundItem
    {
        private int _cityId;
        private string _constructionKey;
        private OSUSurgeryToolType _toolType;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId { get { return _cityId; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionKey { get { return _constructionKey; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUSurgeryToolType ToolType { get { return _toolType; } }

        public OSUSurgeryToolItem(int cityId, string constructionKey, OSUSurgeryToolType toolType, int itemId, string name)
            : base(itemId)
        {
            _cityId = cityId;
            _constructionKey = constructionKey ?? String.Empty;
            _toolType = toolType;
            Name = name;
            Movable = false;
        }

        [Constructable]
        public OSUSurgeryToolItem()
            : this(0, String.Empty, OSUSurgeryToolType.FacaDisseccao, 3781, "instrumento cirúrgico")
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!OSUHealthSystem.CanUseSurgery(from))
            {
                from.SendMessage("Você precisa da feat Cirurgia.");
                return;
            }

            from.SendMessage(_toolType == OSUSurgeryToolType.Anestesico
                ? "Escolha o paciente para anestesiar."
                : "Escolha o paciente da cirurgia.");

            from.Target = new SurgeryToolTarget(this);
        }

        private class SurgeryToolTarget : Target
        {
            private readonly OSUSurgeryToolItem _tool;

            public SurgeryToolTarget(OSUSurgeryToolItem tool)
                : base(2, false, TargetFlags.Beneficial)
            {
                _tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile surgeon = from as PlayerMobile;

                if (surgeon == null)
                    return;

                if (_tool._toolType == OSUSurgeryToolType.AlcoolCirurgico && targeted == from)
                {
                    string selfMsg;
                    OSUHealthSystem.TrySterilizeHands(surgeon, out selfMsg);
                    from.SendMessage(selfMsg);
                    return;
                }

                PlayerMobile patient = targeted as PlayerMobile;
                if (patient == null)
                {
                    from.SendMessage("Escolha um paciente válido.");
                    return;
                }

                string msg;
                OSUHealthSystem.TryUseSurgeryTool(surgeon, patient, _tool._toolType, _tool, out msg);
                from.SendMessage(msg);
            }
        }

        public OSUSurgeryToolItem(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(_cityId);
            writer.Write(_constructionKey ?? String.Empty);
            writer.Write((int)_toolType);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            _cityId = reader.ReadInt();
            _constructionKey = reader.ReadString();
            _toolType = (OSUSurgeryToolType)reader.ReadInt();
            Movable = false;
        }
    }

    public class OSUAnestesicoCirurgico : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUAnestesicoCirurgico() : this(0, String.Empty)
        {
        }

        public OSUAnestesicoCirurgico(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.Anestesico, 3626, "anestésico")
        {
        }

        public OSUAnestesicoCirurgico(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUFacaDisseccaoCirurgica : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUFacaDisseccaoCirurgica() : this(0, String.Empty)
        {
        }

        public OSUFacaDisseccaoCirurgica(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.FacaDisseccao, 3781, "bisturi")
        {
        }

        public OSUFacaDisseccaoCirurgica(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUTesouraCirurgica : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUTesouraCirurgica() : this(0, String.Empty)
        {
        }

        public OSUTesouraCirurgica(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.Tesoura, 3998, "tesoura cirúrgica")
        {
        }

        public OSUTesouraCirurgica(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUAguaEsterilCirurgica : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUAguaEsterilCirurgica() : this(0, String.Empty)
        {
        }

        public OSUAguaEsterilCirurgica(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.AguaEsteril, 4088, "água estéril")
        {
        }

        public OSUAguaEsterilCirurgica(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUGazesCirurgicas : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUGazesCirurgicas() : this(0, String.Empty)
        {
        }

        public OSUGazesCirurgicas(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.Gazes, 3617, "gaze cirúrgica")
        {
        }

        public OSUGazesCirurgicas(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUVelaCauterizadora : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUVelaCauterizadora() : this(0, String.Empty)
        {
        }

        public OSUVelaCauterizadora(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.VelaCauterizadora, 5169, "vela cauterizadora")
        {
        }

        public OSUVelaCauterizadora(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUBrasaCauterizadora : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUBrasaCauterizadora() : this(0, String.Empty)
        {
        }

        public OSUBrasaCauterizadora(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.BrasaCauterizadora, 4024, "brasa cauterizadora")
        {
        }

        public OSUBrasaCauterizadora(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUSanguessugaCirurgica : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUSanguessugaCirurgica() : this(0, String.Empty)
        {
        }

        public OSUSanguessugaCirurgica(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.Sanguessuga, 2425, "sanguessuga")
        {
        }

        public OSUSanguessugaCirurgica(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSULinhaDeSutura : OSUSurgeryToolItem
    {
        [Constructable]
        public OSULinhaDeSutura() : this(0, String.Empty)
        {
        }

        public OSULinhaDeSutura(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.LinhaSutura, 3997, "kit de sutura")
        {
        }

        public OSULinhaDeSutura(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUAdagaDeSangria : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUAdagaDeSangria() : this(0, String.Empty)
        {
        }

        public OSUAdagaDeSangria(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.AdagaSangria, 3921, "adaga de drenagem")
        {
        }

        public OSUAdagaDeSangria(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUCuteloCirurgico : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUCuteloCirurgico() : this(0, String.Empty)
        {
        }

        public OSUCuteloCirurgico(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.CuteloCirurgico, 3779, "cutelo cirúrgico")
        {
        }

        public OSUCuteloCirurgico(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUTochaCauterizadora : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUTochaCauterizadora() : this(0, String.Empty)
        {
        }

        public OSUTochaCauterizadora(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.TochaCauterizadora, 3940, "tocha cauterizadora")
        {
        }

        public OSUTochaCauterizadora(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class OSUAlcoolCirurgico : OSUSurgeryToolItem
    {
        [Constructable]
        public OSUAlcoolCirurgico() : this(0, String.Empty)
        {
        }

        public OSUAlcoolCirurgico(int cityId, string constructionKey)
            : base(cityId, constructionKey, OSUSurgeryToolType.AlcoolCirurgico, 3623, "álcool cirúrgico")
        {
        }

        public OSUAlcoolCirurgico(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class AventalCirurgico : FullApron
    {
        [Constructable]
        public AventalCirurgico() : base(0x0481)
        {
            Name = "avental cirúrgico";
            Weight = 4.0;
        }

        public AventalCirurgico(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Concede +10 segundos em cirurgias.");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class LuvasCirurgicas : LeatherGloves
    {
        [Constructable]
        public LuvasCirurgicas()
        {
            Hue = 0x0481;
            Name = "luvas cirúrgicas";
            Weight = 1.0;
        }

        public LuvasCirurgicas(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Concede +10 segundos em cirurgias.");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class MascaraCirurgica : BaseClothing
    {
        [Constructable]
        public MascaraCirurgica() : base(0x0E0F, Layer.Helm, 0)
        {
            Name = "máscara cirúrgica";
            Weight = 1.0;
        }

        public MascaraCirurgica(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Concede +10 segundos em cirurgias.");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
