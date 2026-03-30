
using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Custom;
using Server.Custom.Systems.Culture;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Custom.Systems.Skills.Abilities
{
    public class BarbeariaAbility : IOSUAbility
    {
        public const int AbilityId = 200003;

        public OSUAbilityDefinition Definition { get; private set; }
        public string CommandText { get { return ""; } }

        public BarbeariaAbility()
        {
            Definition = new OSUAbilityDefinition(
                id: AbilityId,
                name: "Barbearia",
                desc: "Você sabe cortar cabelos, aparar barbas e usar tintas de cabelo e barba.",
                costPicks: 1,
                commandText: "",
                iconId: 0,
                requiredAbilityId: 0,
                requiredFeatId: 0,
                requirementTextOverride: "Nenhum"
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
        }

        public void OnCommand(PlayerMobile pm, CommandEventArgs e)
        {
        }
    }

    public static class OSUBarbeariaHelper
    {
        public const int HairItemBase = 13050;
        public const int HairGumpMaleBase = 54000;
        public const int HairGumpFemaleBase = 64000;

        public const int BeardItemBase = 15160;
        public const int BeardGumpBase = 53500;

        private static readonly int[] m_HairDyePalette = new int[]
        {
            1101, 1102, 1103, 1104, 1105, 1106,
            1107, 1108, 1109, 1110, 1111, 1112,
            1113, 1114, 1115, 1116, 1117, 1118,
            1119, 1120, 1121, 1122, 1123, 1124,
            1125, 1126, 1127, 1128, 1129, 1130,
            1131, 1132, 1133, 1134, 1135, 1136,
            1137, 1138, 1139, 1140, 1141, 1142
        };

        public static bool HasBarbearia(PlayerMobile pm)
        {
            return (pm != null && pm.HasOSUAbility(BarbeariaAbility.AbilityId));
        }

        public static int HairGumpToItem(bool female, int gumpId)
        {
            if (gumpId <= 0)
                return 0;

            int offset = gumpId - (female ? HairGumpFemaleBase : HairGumpMaleBase);
            if (offset < 0)
                return 0;

            return HairItemBase + offset;
        }

        public static int HairItemToGump(bool female, int itemId)
        {
            if (itemId <= 0)
                return 0;

            int offset = itemId - HairItemBase;
            if (offset < 0)
                return 0;

            return (female ? HairGumpFemaleBase : HairGumpMaleBase) + offset;
        }

        public static int BeardGumpToItem(int gumpId)
        {
            if (gumpId <= 0)
                return 0;

            int offset = gumpId - BeardGumpBase;
            if (offset < 0)
                return 0;

            return BeardItemBase + offset;
        }

        public static int BeardItemToGump(int itemId)
        {
            if (itemId <= 0)
                return 0;

            int offset = itemId - BeardItemBase;
            if (offset < 0)
                return 0;

            return BeardGumpBase + offset;
        }

        public static int GetBodyVariant(PlayerMobile pm)
        {
            return GetCustomInt(pm, "OSUBodyVariant", 0);
        }

        public static int GetFaceIndex(PlayerMobile pm)
        {
            return GetCustomInt(pm, "OSUFaceIndex", 0);
        }

        public static void RefreshPaperdoll(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || pm.NetState == null)
                return;

            string title = (pm.Name ?? String.Empty) + String.Format(" [OSUPD:{0}:{1}]", GetBodyVariant(pm), GetFaceIndex(pm));
            pm.NetState.Send(new DisplayPaperdoll(pm, title, true));
        }

        public static int GetPreviewBodyGumpId(PlayerMobile pm)
        {
            bool female = pm != null && pm.Female;
            bool alt = GetBodyVariant(pm) == 1;

            if (female)
                return alt ? 149 : 140;

            return alt ? 131 : 122;
        }

        public static int GetPreviewFaceGumpId(PlayerMobile pm)
        {
            int faceIndex = GetFaceIndex(pm);
            if (faceIndex < 0) faceIndex = 0;
            if (faceIndex > 7) faceIndex = 7;

            bool female = pm != null && pm.Female;
            bool alt = GetBodyVariant(pm) == 1;

            int baseId;
            if (!female)
                baseId = alt ? 132 : 123;
            else
                baseId = alt ? 150 : 141;

            return baseId + faceIndex;
        }

        public static int GetSelectedHairIndex(PlayerMobile target, int[] styles)
        {
            int currentGump = HairItemToGump(target != null && target.Female, target != null ? target.HairItemID : 0);
            if (styles == null || styles.Length == 0)
                return 0;

            for (int i = 0; i < styles.Length; i++)
            {
                if (styles[i] == currentGump)
                    return i;
            }

            return 0;
        }

        public static int GetSelectedBeardIndex(PlayerMobile target, int[] styles)
        {
            int currentGump = BeardItemToGump(target != null ? target.FacialHairItemID : 0);
            if (styles == null || styles.Length == 0)
                return 0;

            for (int i = 0; i < styles.Length; i++)
            {
                if (styles[i] == currentGump)
                    return i;
            }

            return 0;
        }

        public static OSUCultureDefinition GetBarberCulture(PlayerMobile barber)
        {
            if (barber == null)
                return null;

            return OSUCultureRegistry.GetById(barber.OSUCultureId);
        }

        public static int[] GetAllowedHairStyles(PlayerMobile barber, PlayerMobile target)
        {
            OSUCultureDefinition culture = GetBarberCulture(barber);
            int[] baseStyles = target != null && target.Female
                ? (culture != null ? (culture.FemaleHairGumpIds ?? new int[0]) : new int[0])
                : (culture != null ? (culture.MaleHairGumpIds ?? new int[0]) : new int[0]);

            int current = target != null ? HairItemToGump(target.Female, target.HairItemID) : 0;
            return MergeCurrentStyle(baseStyles, current);
        }

        public static int[] GetAllowedBeardStyles(PlayerMobile barber, PlayerMobile target)
        {
            OSUCultureDefinition culture = GetBarberCulture(barber);
            int[] baseStyles = culture != null ? (culture.MaleBeardGumpIds ?? new int[0]) : new int[0];
            int current = target != null ? BeardItemToGump(target.FacialHairItemID) : 0;
            return MergeCurrentStyle(baseStyles, current);
        }

        public static int[] GetAllHairHues()
        {
            return (int[])m_HairDyePalette.Clone();
        }

        public static int[] GetAllBeardHues()
        {
            return (int[])m_HairDyePalette.Clone();
        }

        public static int GetUsesByResource(CraftResource resource)
        {
            switch (resource)
            {
                case CraftResource.DullCopper:
                    return 60;
                case CraftResource.ShadowIron:
                    return 80;
                case CraftResource.Copper:
                    return 100;
                case CraftResource.Bronze:
                    return 120;
                case CraftResource.Gold:
                    return 140;
                case CraftResource.Agapite:
                    return 160;
                case CraftResource.Verite:
                    return 180;
                case CraftResource.Valorite:
                    return 220;
                default:
                    return 40;
            }
        }

        private static int[] MergeCurrentStyle(int[] baseStyles, int currentStyle)
        {
            List<int> list = new List<int>();
            HashSet<int> seen = new HashSet<int>();

            if (currentStyle > 0)
            {
                list.Add(currentStyle);
                seen.Add(currentStyle);
            }

            if (baseStyles != null)
            {
                for (int i = 0; i < baseStyles.Length; i++)
                {
                    int style = baseStyles[i];
                    if (style <= 0 || seen.Contains(style))
                        continue;

                    seen.Add(style);
                    list.Add(style);
                }
            }

            return list.ToArray();
        }

        private static int GetCustomInt(object obj, string propName, int fallback)
        {
            if (obj == null || String.IsNullOrEmpty(propName))
                return fallback;

            try
            {
                var prop = obj.GetType().GetProperty(propName);
                if (prop == null || !prop.CanRead)
                    return fallback;

                object raw = prop.GetValue(obj, null);
                if (raw is int)
                    return (int)raw;
            }
            catch
            {
            }

            return fallback;
        }
    }

    public class TesouraDeBarbeiro : BaseTool
    {
        public override CraftSystem CraftSystem { get { return null; } }

        [Constructable]
        public TesouraDeBarbeiro() : this(CraftResource.Iron)
        {
        }

        [Constructable]
        public TesouraDeBarbeiro(CraftResource resource) : base(OSUBarbeariaHelper.GetUsesByResource(resource), 0xF9F)
        {
            Name = "tesoura de barbeiro";
            Weight = 1.0;
            Resource = resource;
        }

        public TesouraDeBarbeiro(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (Deleted || UsesRemaining <= 0)
            {
                pm.SendMessage("Essa tesoura não possui mais usos.");
                Delete();
                return;
            }

            if (!IsChildOf(pm.Backpack) && Parent != pm)
            {
                pm.SendMessage("A tesoura precisa estar com você.");
                return;
            }

            if (!OSUBarbeariaHelper.HasBarbearia(pm))
            {
                pm.SendMessage("Você não sabe usar tesoura de barbeiro.");
                return;
            }

            pm.SendMessage("Escolha o jogador que vai receber o corte.");
            pm.Target = new BarberTarget(pm, this, BarberServiceType.Haircut);
        }

        public bool ConsumeUse(PlayerMobile barber)
        {
            if (Deleted)
                return false;

            if (UsesRemaining <= 0)
            {
                if (barber != null)
                    barber.SendMessage("Essa tesoura não possui mais usos.");

                Delete();
                return false;
            }

            UsesRemaining--;
            InvalidateProperties();

            if (UsesRemaining <= 0 && BreakOnDepletion)
            {
                if (barber != null)
                    barber.SendMessage("A tesoura de barbeiro se desgastou e foi destruída.");

                Delete();
                return true;
            }

            if (barber != null)
                barber.SendMessage("Usos restantes da tesoura: {0}", UsesRemaining);

            return true;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Usos restantes: {0}", UsesRemaining);
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

    public class TintaDeCabeloBarbeiro : Item
    {
        [Constructable]
        public TintaDeCabeloBarbeiro() : base(0xEFF)
        {
            Name = "tinta de cabelo";
            Weight = 1.0;
        }

        public TintaDeCabeloBarbeiro(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack) && Parent != pm)
            {
                pm.SendMessage("A tinta precisa estar com você.");
                return;
            }

            if (!OSUBarbeariaHelper.HasBarbearia(pm))
            {
                pm.SendMessage("Você não sabe usar tinta de cabelo.");
                return;
            }

            pm.SendMessage("Escolha o jogador que vai receber a tintura de cabelo.");
            pm.Target = new BarberTarget(pm, this, BarberServiceType.HairDye);
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

    public class TintaDeBarbaBarbeiro : Item
    {
        [Constructable]
        public TintaDeBarbaBarbeiro() : base(0xEFF)
        {
            Name = "tinta de barba";
            Weight = 1.0;
            Hue = 1150;
        }

        public TintaDeBarbaBarbeiro(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            if (!IsChildOf(pm.Backpack) && Parent != pm)
            {
                pm.SendMessage("A tinta precisa estar com você.");
                return;
            }

            if (!OSUBarbeariaHelper.HasBarbearia(pm))
            {
                pm.SendMessage("Você não sabe usar tinta de barba.");
                return;
            }

            pm.SendMessage("Escolha o jogador que vai receber a tintura de barba.");
            pm.Target = new BarberTarget(pm, this, BarberServiceType.BeardDye);
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

    public enum BarberServiceType
    {
        Haircut,
        HairDye,
        BeardDye
    }

    public class BarberTarget : Target
    {
        private readonly PlayerMobile _barber;
        private readonly Item _tool;
        private readonly BarberServiceType _service;

        public BarberTarget(PlayerMobile barber, Item tool, BarberServiceType service) : base(2, false, TargetFlags.None)
        {
            _barber = barber;
            _tool = tool;
            _service = service;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            PlayerMobile target = targeted as PlayerMobile;

            if (_barber == null || _barber.Deleted)
                return;

            if (_tool == null || _tool.Deleted)
            {
                _barber.SendMessage("O item usado não está mais disponível.");
                return;
            }

            if (!OSUBarbeariaHelper.HasBarbearia(_barber))
            {
                _barber.SendMessage("Você não possui a habilidade de barbearia.");
                return;
            }

            if (target == null || target.Deleted)
            {
                _barber.SendMessage("Você precisa escolher um jogador.");
                return;
            }

            if (!_barber.InRange(target.Location, 2))
            {
                _barber.SendMessage("Você está longe demais.");
                return;
            }

            if (_service == BarberServiceType.Haircut)
            {
                target.CloseGump(typeof(BarberHaircutGump));
                target.SendGump(new BarberHaircutGump(_barber, target, _tool as TesouraDeBarbeiro));
                _barber.SendMessage("Você ofereceu um corte de cabelo.");
                target.SendMessage("Escolha o novo cabelo ou barba.");
            }
            else if (_service == BarberServiceType.HairDye)
            {
                target.CloseGump(typeof(BarberDyeGump));
                target.SendGump(new BarberDyeGump(_barber, target, false, _tool));
                _barber.SendMessage("Você ofereceu uma tintura de cabelo.");
                target.SendMessage("Escolha a nova cor do cabelo.");
            }
            else
            {
                if (target.Female || target.FacialHairItemID <= 0)
                {
                    _barber.SendMessage("Esse jogador não tem barba para pintar.");
                    return;
                }

                target.CloseGump(typeof(BarberDyeGump));
                target.SendGump(new BarberDyeGump(_barber, target, true, _tool));
                _barber.SendMessage("Você ofereceu uma tintura de barba.");
                target.SendMessage("Escolha a nova cor da barba.");
            }
        }
    }

    public class BarberHaircutGump : Gump
    {
        private const int BtnPrev = 1;
        private const int BtnNext = 2;
        private const int BtnTabHair = 3;
        private const int BtnTabBeard = 4;
        private const int BtnOk = 5;

        private readonly PlayerMobile _barber;
        private readonly PlayerMobile _target;
        private readonly TesouraDeBarbeiro _scissors;
        private readonly bool _showBeard;
        private readonly int _hairIndex;
        private readonly int _beardIndex;

        public BarberHaircutGump(PlayerMobile barber, PlayerMobile target, TesouraDeBarbeiro scissors)
            : this(barber, target, scissors, false,
                  OSUBarbeariaHelper.GetSelectedHairIndex(target, OSUBarbeariaHelper.GetAllowedHairStyles(barber, target)),
                  OSUBarbeariaHelper.GetSelectedBeardIndex(target, OSUBarbeariaHelper.GetAllowedBeardStyles(barber, target)))
        {
        }

        public BarberHaircutGump(PlayerMobile barber, PlayerMobile target, TesouraDeBarbeiro scissors, bool showBeard, int hairIndex, int beardIndex)
            : base(0, 0)
        {
            _barber = barber;
            _target = target;
            _scissors = scissors;
            _showBeard = showBeard && target != null && !target.Female;
            _hairIndex = hairIndex;
            _beardIndex = beardIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImageTiled(222, 253, 204, 303, 391);
            AddImageTiled(221, 200, 199, 53, 398);
            AddImageTiled(204, 156, 220, 54, 650);
            AddImageTiled(215, 544, 209, 54, 650);
            AddImageTiled(176, 182, 57, 380, 651);
            AddImageTiled(406, 196, 57, 371, 651);
            AddImage(182, 551, 426);
            AddImageTiled(221, 242, 199, 21, 463);
            AddImage(413, 551, 426);
            AddImage(182, 157, 426);
            AddImage(413, 157, 426);

            if (_target != null && !_target.Female)
            {
                AddButton(234, 214, _showBeard ? 455 : 454, 454, BtnTabHair, GumpButtonType.Reply, 0);
                AddButton(331, 215, _showBeard ? 454 : 455, 454, BtnTabBeard, GumpButtonType.Reply, 0);
                AddLabel(272, 220, 0, "Cabelo");
                AddLabel(371, 223, 0, "Barba");
            }
            else
            {
                AddButton(234, 214, 454, 454, 0, GumpButtonType.Reply, 0);
                AddLabel(272, 220, 0, "Cabelo");
            }

            DrawPreview();
            DrawControls();
        }

        private void DrawPreview()
        {
            if (_target == null)
                return;

            int px = 233;
            int py = 279;

            AddImage(px, py, OSUBarbeariaHelper.GetPreviewBodyGumpId(_target), _target.Hue);
            AddImage(px, py, OSUBarbeariaHelper.GetPreviewFaceGumpId(_target), _target.Hue);

            int[] hairStyles = OSUBarbeariaHelper.GetAllowedHairStyles(_barber, _target);
            int[] beardStyles = OSUBarbeariaHelper.GetAllowedBeardStyles(_barber, _target);

            int hairGump = SafeGet(hairStyles, _hairIndex);
            if (hairGump > 0)
                AddImage(px, py, hairGump, _target.HairHue);

            if (!_target.Female)
            {
                int beardGump = _showBeard ? SafeGet(beardStyles, _beardIndex) : OSUBarbeariaHelper.BeardItemToGump(_target.FacialHairItemID);
                if (beardGump > 0)
                    AddImage(px, py, beardGump, _target.FacialHairHue);
            }
        }

        private void DrawControls()
        {
            int[] list = _showBeard ? OSUBarbeariaHelper.GetAllowedBeardStyles(_barber, _target) : OSUBarbeariaHelper.GetAllowedHairStyles(_barber, _target);
            int index = _showBeard ? _beardIndex : _hairIndex;

            if (list == null || list.Length == 0)
            {
                AddLabel(248, 500, 0, "Sem opções");
                return;
            }

            if (index < 0) index = 0;
            if (index >= list.Length) index = list.Length - 1;

            AddButton(230, 279, 451, 451, BtnPrev, GumpButtonType.Reply, 0);
            AddButton(389, 279, 450, 450, BtnNext, GumpButtonType.Reply, 0);
            AddButton(282, 525, 560, 560, BtnOk, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_barber == null || _barber.Deleted || _target == null || _target.Deleted)
                return;

            if (_scissors == null || _scissors.Deleted)
            {
                _target.SendMessage("A tesoura usada não está mais disponível.");
                return;
            }

            if (!OSUBarbeariaHelper.HasBarbearia(_barber))
            {
                _target.SendMessage("O barbeiro não possui mais a habilidade de barbearia.");
                return;
            }

            if (!_barber.InRange(_target.Location, 2))
            {
                _target.SendMessage("O barbeiro está longe demais.");
                return;
            }

            int[] hairStyles = OSUBarbeariaHelper.GetAllowedHairStyles(_barber, _target);
            int[] beardStyles = OSUBarbeariaHelper.GetAllowedBeardStyles(_barber, _target);

            int hairIndex = Clamp(_hairIndex, hairStyles.Length);
            int beardIndex = Clamp(_beardIndex, beardStyles.Length);

            switch (info.ButtonID)
            {
                case BtnPrev:
                    if (_showBeard)
                        beardIndex = WrapPrev(beardIndex, beardStyles.Length);
                    else
                        hairIndex = WrapPrev(hairIndex, hairStyles.Length);
                    _target.SendGump(new BarberHaircutGump(_barber, _target, _scissors, _showBeard, hairIndex, beardIndex));
                    break;
                case BtnNext:
                    if (_showBeard)
                        beardIndex = WrapNext(beardIndex, beardStyles.Length);
                    else
                        hairIndex = WrapNext(hairIndex, hairStyles.Length);
                    _target.SendGump(new BarberHaircutGump(_barber, _target, _scissors, _showBeard, hairIndex, beardIndex));
                    break;
                case BtnTabHair:
                    _target.SendGump(new BarberHaircutGump(_barber, _target, _scissors, false, hairIndex, beardIndex));
                    break;
                case BtnTabBeard:
                    if (!_target.Female)
                        _target.SendGump(new BarberHaircutGump(_barber, _target, _scissors, true, hairIndex, beardIndex));
                    break;
                case BtnOk:
                    ApplySelection(hairStyles, beardStyles, hairIndex, beardIndex);
                    break;
            }
        }

        private void ApplySelection(int[] hairStyles, int[] beardStyles, int hairIndex, int beardIndex)
        {
            bool changedHair = false;
            bool changedBeard = false;

            int hairGump = SafeGet(hairStyles, hairIndex);
            int hairItem = OSUBarbeariaHelper.HairGumpToItem(_target.Female, hairGump);
            if (hairItem > 0 && hairItem != _target.HairItemID)
            {
                _target.HairItemID = hairItem;
                changedHair = true;
            }

            if (!_target.Female && _showBeard)
            {
                int beardGump = SafeGet(beardStyles, beardIndex);
                int beardItem = OSUBarbeariaHelper.BeardGumpToItem(beardGump);
                if (beardItem > 0 && beardItem != _target.FacialHairItemID)
                {
                    _target.FacialHairItemID = beardItem;
                    changedBeard = true;
                }
            }

            if (!changedHair && !changedBeard)
            {
                _target.SendMessage("Nenhuma mudança foi feita.");
                return;
            }

            _target.Delta(MobileDelta.Hair | MobileDelta.FacialHair);
            _target.ProcessDelta();
            OSUBarbeariaHelper.RefreshPaperdoll(_target);

            if (changedHair)
                OSUHairGrowthSystem.ResetHairGrowthTimer(_target);

            if (changedBeard)
                OSUHairGrowthSystem.ResetBeardGrowthTimer(_target);

            _scissors.ConsumeUse(_barber);

            _barber.SendMessage("Corte aplicado.");
            _target.SendMessage("Seu visual foi atualizado.");
        }

        private static int SafeGet(int[] list, int index)
        {
            if (list == null || list.Length == 0)
                return 0;

            if (index < 0) index = 0;
            if (index >= list.Length) index = list.Length - 1;
            return list[index];
        }

        private static int WrapPrev(int index, int len)
        {
            if (len <= 0) return 0;
            index--;
            if (index < 0) index = len - 1;
            return index;
        }

        private static int WrapNext(int index, int len)
        {
            if (len <= 0) return 0;
            index++;
            if (index >= len) index = 0;
            return index;
        }

        private static int Clamp(int index, int len)
        {
            if (len <= 0) return 0;
            if (index < 0) return 0;
            if (index >= len) return len - 1;
            return index;
        }
    }

    public class BarberDyeGump : Gump
    {
        private const int BtnOk = 1;
        private const int BtnColorBase = 100;

        private readonly PlayerMobile _barber;
        private readonly PlayerMobile _target;
        private readonly Item _dyeItem;
        private readonly bool _beard;
        private readonly int _selectedHue;

        public BarberDyeGump(PlayerMobile barber, PlayerMobile target, bool beard, Item dyeItem)
            : this(barber, target, beard, beard ? target.FacialHairHue : target.HairHue, dyeItem)
        {
        }

        public BarberDyeGump(PlayerMobile barber, PlayerMobile target, bool beard, int selectedHue, Item dyeItem)
            : base(0, 0)
        {
            _barber = barber;
            _target = target;
            _beard = beard;
            _selectedHue = selectedHue;
            _dyeItem = dyeItem;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            int[] colors = _beard ? OSUBarbeariaHelper.GetAllBeardHues() : OSUBarbeariaHelper.GetAllHairHues();

            AddPage(0);
            AddImageTiled(501, 200, 137, 356, 391);
            AddImageTiled(221, 200, 275, 360, 398);
            AddImageTiled(204, 156, 435, 54, 650);
            AddImageTiled(215, 544, 419, 54, 650);
            AddImageTiled(176, 182, 57, 380, 651);
            AddImageTiled(619, 196, 57, 371, 651);
            AddImage(182, 551, 426);
            AddButton(527, 502, 560, 248, BtnOk, GumpButtonType.Reply, 0);
            AddImage(626, 551, 426);
            AddImage(182, 157, 426);
            AddImage(626, 157, 426);
            AddImageTiled(481, 201, 19, 359, 651);

            DrawPreview();
            DrawColorButtons(colors);
        }

        private void DrawPreview()
        {
            if (_target == null)
                return;

            int px = 475;
            int py = 233;

            AddImage(px, py, OSUBarbeariaHelper.GetPreviewBodyGumpId(_target), _target.Hue);
            AddImage(px, py, OSUBarbeariaHelper.GetPreviewFaceGumpId(_target), _target.Hue);

            int hairGump = OSUBarbeariaHelper.HairItemToGump(_target.Female, _target.HairItemID);
            if (hairGump > 0)
                AddImage(px, py, hairGump, _beard ? _target.HairHue : _selectedHue);

            if (!_target.Female)
            {
                int beardGump = OSUBarbeariaHelper.BeardItemToGump(_target.FacialHairItemID);
                if (beardGump > 0)
                    AddImage(px, py, beardGump, _beard ? _selectedHue : _target.FacialHairHue);
            }
        }

        private void DrawColorButtons(int[] colors)
        {
            int maxButtons = Math.Min(colors.Length, 42);
            int cols = 3;
            int startX = 227;
            int startY = 210;
            int stepX = 90;
            int stepY = 25;

            for (int i = 0; i < maxButtons; i++)
            {
                int x = startX + ((i % cols) * stepX);
                int y = startY + ((i / cols) * stepY);
                int hue = colors[i];
                bool selected = hue == _selectedHue;

                AddButton(x, y, selected ? 454 : 455, 454, BtnColorBase + i, GumpButtonType.Reply, 0);
                AddLabel(x + 24, y - 2, hue, hue.ToString());
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_barber == null || _barber.Deleted || _target == null || _target.Deleted)
                return;

            if (_dyeItem == null || _dyeItem.Deleted)
            {
                _target.SendMessage("A tinta usada não está mais disponível.");
                return;
            }

            if (!OSUBarbeariaHelper.HasBarbearia(_barber))
            {
                _target.SendMessage("O barbeiro não possui mais a habilidade de barbearia.");
                return;
            }

            if (!_barber.InRange(_target.Location, 2))
            {
                _target.SendMessage("O barbeiro está longe demais.");
                return;
            }

            int[] colors = _beard ? OSUBarbeariaHelper.GetAllBeardHues() : OSUBarbeariaHelper.GetAllHairHues();
            int maxButtons = Math.Min(colors.Length, 48);

            if (info.ButtonID >= BtnColorBase)
            {
                int idx = info.ButtonID - BtnColorBase;
                if (idx >= 0 && idx < maxButtons)
                {
                    _target.SendGump(new BarberDyeGump(_barber, _target, _beard, colors[idx], _dyeItem));
                    return;
                }
            }

            if (info.ButtonID == BtnOk)
            {
                if (_beard)
                {
                    if (_target.Female || _target.FacialHairItemID <= 0)
                    {
                        _target.SendMessage("Você não tem barba para pintar.");
                        return;
                    }

                    _target.FacialHairHue = _selectedHue +1;
                }
                else
                {
                    if (_target.HairItemID <= 0)
                    {
                        _target.SendMessage("Você não tem cabelo para pintar.");
                        return;
                    }

                    _target.HairHue = _selectedHue +1;
                }

                _target.Delta(MobileDelta.Hair | MobileDelta.FacialHair);
                _target.ProcessDelta();
                OSUBarbeariaHelper.RefreshPaperdoll(_target);

                _dyeItem.Delete();

                _barber.SendMessage("Tintura aplicada.");
                _target.SendMessage("Seu visual foi atualizado.");
            }
        }
    }
}
