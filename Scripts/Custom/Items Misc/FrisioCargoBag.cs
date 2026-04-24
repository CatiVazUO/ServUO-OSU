using System;
using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Custom.Systems.Stables.Engine
{
    public class OSUFrisioCargoBagTool : BaseTool
    {
        public override CraftSystem CraftSystem { get { return null; } }

        [Constructable]
        public OSUFrisioCargoBagTool() : base(50, 0x0F33)
        {
            Name = "bolsa de carga";
            Weight = 1.0;
            Hue = 0;
        }

        public OSUFrisioCargoBagTool(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!CheckAccessible(this, from, true))
                return;

            from.Target = new InternalTarget(this);
            from.SendMessage("Escolha um frisio ou um frisio de carga.");
        }

        private class InternalTarget : Target
        {
            private readonly OSUFrisioCargoBagTool m_Tool;

            public InternalTarget(OSUFrisioCargoBagTool tool) : base(2, false, TargetFlags.None)
            {
                m_Tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Tool == null || m_Tool.Deleted)
                    return;

                BaseCreature bc = targeted as BaseCreature;
                if (bc == null)
                {
                    from.SendMessage("Isso não é um cavalo válido para essa bolsa.");
                    return;
                }

                string reason;
                if (!OSUFrisioCargoUtility.TryApplyBag(from, m_Tool, bc, out reason))
                {
                    from.SendMessage(reason);
                    return;
                }
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
            int version = reader.ReadInt();
        }
    }

    public static class OSUFrisioCargoUtility
    {
        public static bool TryApplyBag(Mobile from, OSUFrisioCargoBagTool tool, BaseCreature targeted, out string reason)
        {
            reason = null;

            if (from == null || tool == null || targeted == null || targeted.Deleted)
            {
                reason = "Alvo inválido.";
                return false;
            }

            if (targeted.Map == null || targeted.Map == Map.Internal)
            {
                reason = "Esse cavalo não está em um local válido.";
                return false;
            }

            BaseMount mountTarget = targeted as BaseMount;
            if (mountTarget != null && mountTarget.Rider != null)
            {
                reason = "Desmonte o cavalo antes de usar a bolsa.";
                return false;
            }

            if (targeted.IsDeadPet || !targeted.Alive)
            {
                reason = "Esse cavalo não está em condições de receber a bolsa.";
                return false;
            }

            if (targeted.OSUPetAwaitingResurrection)
            {
                reason = "Esse cavalo está caído e não pode receber a bolsa agora.";
                return false;
            }

            if (targeted.OSUPetServiceKind != 0)
            {
                reason = "Esse cavalo está em um serviço do estábulo.";
                return false;
            }

            if (from.AccessLevel < AccessLevel.GameMaster)
            {
                if (!targeted.Controlled || targeted.ControlMaster != from)
                {
                    reason = "Você só pode usar essa bolsa em um cavalo seu.";
                    return false;
                }
            }

            if (targeted is HorseFrisioCarga)
            {
                HorseFrisioCarga cargo = (HorseFrisioCarga)targeted;
                cargo.EnsureCargoBackpack();
                cargo.CargoBagUsesRemaining = Math.Max(1, tool.UsesRemaining);
                cargo.InvalidateProperties();
                from.Emote("*ajusta a bolsa de carga no cavalo*");
                from.SendMessage("Os usos da bolsa desse cavalo foram substituídos por {0}.", cargo.CargoBagUsesRemaining);
                tool.Delete();
                return true;
            }

            HorseFrisio frisio = targeted as HorseFrisio;
            if (frisio == null)
            {
                reason = "Essa bolsa só pode ser usada em um frisio ou em um frisio de carga.";
                return false;
            }

            HorseFrisioCarga converted;
            if (!ConvertFrisioToCargo(from, frisio, Math.Max(1, tool.UsesRemaining), out converted, out reason))
                return false;

            from.Emote("*prende uma bolsa de carga ao cavalo*");
            from.SendMessage("O frisio foi transformado em um frisio de carga.");
            tool.Delete();
            return true;
        }

        public static bool ConvertFrisioToCargo(Mobile from, HorseFrisio source, int bagUses, out HorseFrisioCarga converted, out string reason)
        {
            converted = null;
            reason = null;

            if (source == null || source.Deleted)
            {
                reason = "O frisio não foi encontrado.";
                return false;
            }

            if (source.Rider != null)
            {
                reason = "Desmonte o cavalo antes de converter.";
                return false;
            }

            converted = new HorseFrisioCarga();
            CopyCommonState(source, converted);
            converted.CargoBagUsesRemaining = Math.Max(1, bagUses);
            converted.EnsureCargoBackpack();
            converted.MoveToWorld(source.Location, source.Map);
            FinishOwnershipTransfer(source, converted);
            source.Delete();
            return true;
        }

        public static bool ConvertCargoToFrisio(HorseFrisioCarga source, bool spillContents, out HorseFrisio converted, out string reason)
        {
            converted = null;
            reason = null;

            if (source == null || source.Deleted)
            {
                reason = "O frisio de carga não foi encontrado.";
                return false;
            }

            if (source.Rider != null)
            {
                reason = "Desmonte o cavalo antes de converter.";
                return false;
            }

            converted = new HorseFrisio();
            CopyCommonState(source, converted);
            converted.MoveToWorld(source.Location, source.Map);

            if (spillContents)
                SpillCargoContents(source);

            FinishOwnershipTransfer(source, converted);
            source.Delete();
            return true;
        }

        public static void CollapseCargoHorse(HorseFrisioCarga source)
        {
            if (source == null || source.Deleted)
                return;

            HorseFrisio converted;
            string reason;

            if (ConvertCargoToFrisio(source, true, out converted, out reason))
            {
                if (converted.ControlMaster != null)
                    converted.ControlMaster.SendMessage("A bolsa de carga do cavalo se desgastou totalmente e ele voltou à forma normal.");
            }
        }

        private static void SpillCargoContents(HorseFrisioCarga source)
        {
            Container pack = source.Backpack;
            if (pack == null)
                return;

            Map map = source.Map;
            Point3D loc = source.Location;

            while (pack.Items.Count > 0)
            {
                Item item = pack.Items[0];
                if (item == null || item.Deleted)
                    continue;

                item.MoveToWorld(loc, map);
            }
        }

        private static void FinishOwnershipTransfer(BaseCreature source, BaseCreature target)
        {
            Mobile master = source.ControlMaster;
            Mobile summon = source.SummonMaster;
            bool controlled = source.Controlled;
            bool summoned = source.Summoned;
            OrderType order = source.ControlOrder;
            IDamageable controlTarget = source.ControlTarget;

            if (master != null)
                source.ControlMaster = null;

            if (summon != null)
                source.SummonMaster = null;

            target.Controlled = controlled;
            target.Summoned = summoned;
            target.ControlOrder = order;
            target.ControlTarget = controlTarget;

            if (master != null)
                target.ControlMaster = master;
            else if (summon != null)
                target.SummonMaster = summon;
        }

        private static void CopyCommonState(BaseCreature source, BaseCreature target)
        {
            if (source == null || target == null)
                return;

            Server.Custom.Systems.Stables.Engine.OSUStablePetSystem.EnsureInitialized(source);

            target.Name = source.Name;
            target.Hue = source.Hue;
            target.Female = source.Female;
            target.Direction = source.Direction;
            target.RawStr = source.RawStr;
            target.RawDex = source.RawDex;
            target.RawInt = source.RawInt;
            target.Hits = Math.Min(source.Hits, target.HitsMax);
            target.Stam = Math.Min(source.Stam, target.StamMax);
            target.Mana = Math.Min(source.Mana, target.ManaMax);
            target.DamageMin = source.DamageMin;
            target.DamageMax = source.DamageMax;
            target.Karma = source.Karma;
            target.Fame = source.Fame;
            target.VirtualArmor = source.VirtualArmor;
            target.Tamable = source.Tamable;
            target.MinTameSkill = source.MinTameSkill;
            target.ControlSlots = source.ControlSlots > 0 ? source.ControlSlots : 1;
            target.Loyalty = source.Loyalty;
            target.Home = source.Home;
            target.RangeHome = source.RangeHome;
            target.Blessed = source.Blessed;
            target.CorpseNameOverride = source.CorpseNameOverride;

            target.Owners.Clear();
            for (int i = 0; i < source.Owners.Count; i++)
            {
                Mobile owner = source.Owners[i];
                if (owner != null)
                    target.Owners.Add(owner);
            }

            CopyStableState(source, target);
        }

        private static void CopyStableState(BaseCreature source, BaseCreature target)
        {
            target.OSUPetInitialized = source.OSUPetInitialized;
            target.OSUPetLevel = source.OSUPetLevel;
            target.OSUPetXP = source.OSUPetXP;
            target.OSUPetNextLevelXP = source.OSUPetNextLevelXP;
            target.OSUPetLastGainStr = source.OSUPetLastGainStr;
            target.OSUPetLastGainDex = source.OSUPetLastGainDex;
            target.OSUPetLastGainInt = source.OSUPetLastGainInt;
            target.OSUPetLastGainLevel = source.OSUPetLastGainLevel;
            target.OSUPetLevelOneStr = source.OSUPetLevelOneStr;
            target.OSUPetLevelOneDex = source.OSUPetLevelOneDex;
            target.OSUPetLevelOneInt = source.OSUPetLevelOneInt;
            target.OSUPetCastrated = source.OSUPetCastrated;
            target.OSUPetSterile = source.OSUPetSterile;
            target.OSUPetMarked = source.OSUPetMarked;
            target.OSUPetBrandOwnerSerial = source.OSUPetBrandOwnerSerial;
            target.OSUPetBrandOwnerName = source.OSUPetBrandOwnerName;
            target.OSUPetLivesRemaining = source.OSUPetLivesRemaining;
            target.OSUPetLivesMax = source.OSUPetLivesMax;
            target.OSUPetAwaitingResurrection = source.OSUPetAwaitingResurrection;
            target.OSUPetDownedUntilUtc = source.OSUPetDownedUntilUtc;
            target.OSUPetLastCommandUtc = source.OSUPetLastCommandUtc;
            target.OSUPetBreedCount = source.OSUPetBreedCount;
            target.OSUPetBreedCountMax = source.OSUPetBreedCountMax;
            target.OSUPetBreedGroup = source.OSUPetBreedGroup;
            target.OSUPetAbilitySlot5 = source.OSUPetAbilitySlot5;
            target.OSUPetAbilitySlot10 = source.OSUPetAbilitySlot10;
            target.OSUPetServiceOwnerSerial = source.OSUPetServiceOwnerSerial;
            target.OSUPetServiceKind = source.OSUPetServiceKind;
            target.OSUPetServiceCityId = source.OSUPetServiceCityId;
            target.OSUPetServiceReadyUtc = source.OSUPetServiceReadyUtc;
            target.OSUPetServiceClaimFromUtc = source.OSUPetServiceClaimFromUtc;
            target.OSUPetServicePartnerSerial = source.OSUPetServicePartnerSerial;
            target.OSUPetPendingOffspringTypeName = source.OSUPetPendingOffspringTypeName;
            target.OSUPetPendingOffspringFemale = source.OSUPetPendingOffspringFemale;
            target.OSUPetPendingOffspringStr = source.OSUPetPendingOffspringStr;
            target.OSUPetPendingOffspringDex = source.OSUPetPendingOffspringDex;
            target.OSUPetPendingOffspringInt = source.OSUPetPendingOffspringInt;
            target.OSUPetPendingOffspringBreedMax = source.OSUPetPendingOffspringBreedMax;
            target.OSUPetPendingOffspringGroup = source.OSUPetPendingOffspringGroup;
            target.OSUPetStoredControlSlots = source.OSUPetStoredControlSlots;
            target.OSUPetServiceRoomIndex = source.OSUPetServiceRoomIndex;
            target.OSUPetServiceStage = source.OSUPetServiceStage;
            target.OSUPetLastTrainedLevel = source.OSUPetLastTrainedLevel;
            target.OSUPetPastureAtUtc = source.OSUPetPastureAtUtc;
        }
    }
}
