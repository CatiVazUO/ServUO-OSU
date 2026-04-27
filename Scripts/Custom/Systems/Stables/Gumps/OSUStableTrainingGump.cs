using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Stables.Engine;
using Server.Custom.Systems.Stables.Mobiles;

namespace Server.Custom.Systems.Stables.Gumps
{
    public class OSUStableTrainingGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly OSUStableMaster _npc;
        private readonly BaseCreature _pet;
        private readonly int _addStr;
        private readonly int _addDex;
        private readonly int _addInt;

        private const int ButtonStrMinus = 101;
        private const int ButtonStrPlus = 102;
        private const int ButtonDexMinus = 103;
        private const int ButtonDexPlus = 104;
        private const int ButtonIntMinus = 105;
        private const int ButtonIntPlus = 106;
        private const int ButtonConfirm = 506;

        public OSUStableTrainingGump(PlayerMobile from, OSUStableMaster npc, BaseCreature pet)
            : this(from, npc, pet, 0, 0, 0)
        {
        }

        private OSUStableTrainingGump(PlayerMobile from, OSUStableMaster npc, BaseCreature pet, int addStr, int addDex, int addInt) : base(0, 0)
        {
            _from = from;
            _npc = npc;
            _pet = pet;

            int totalPoints = GetTotalTrainablePoints(_pet);
            addStr = Math.Max(0, addStr);
            addDex = Math.Max(0, addDex);
            addInt = Math.Max(0, addInt);

            int used = addStr + addDex + addInt;
            if (used > totalPoints && used > 0)
            {
                double ratio = (double)totalPoints / (double)used;
                addStr = (int)Math.Floor(addStr * ratio);
                addDex = (int)Math.Floor(addDex * ratio);
                addInt = Math.Max(0, totalPoints - addStr - addDex);
            }

            _addStr = addStr;
            _addDex = addDex;
            _addInt = addInt;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddStableFrame();
            AddPage(1);

            int remaining = GetRemainingPoints();
            int baseStr = GetBaseStrForPreview(_pet);
            int baseDex = GetBaseDexForPreview(_pet);
            int baseInt = GetBaseIntForPreview(_pet);

            AddLabel(391, 88, 1152, @"Treinamento");

            AddLabel(200, 144, 1152, @"Animal:");
            AddLabel(455, 144, 1152, SafeLabel(_pet != null ? _pet.Name : "Nome"));

            AddLabel(200, 174, 1152, @"Pontos Ganhos No Ultimo Nível:");
            AddLabel(455, 174, 1152, GetLastLevelPoints(_pet).ToString());

            AddLabel(200, 204, 1152, @"Pontos Nunca Distribuidos:");
            AddLabel(455, 204, 1152, GetNeverDistributedPoints(_pet).ToString());

            AddLabel(200, 234, 1152, @"Nível:");
            AddLabel(455, 234, 1152, _pet != null ? _pet.OSUPetLevel.ToString() : "0");

            AddLabel(200, 266, 1152, @"Habilidades Especiais:");
            AddLabel(455, 268, 1152, GetAbilityLabel(_pet != null ? _pet.OSUPetAbilitySlot5 : null));
            AddLabel(455, 292, 1152, GetAbilityLabel(_pet != null ? _pet.OSUPetAbilitySlot10 : null));

            AddImageTiled(200, 326, 455, 12, 634);

            AddLabel(235, 355, 1152, @"Força:");
            AddAttributeControls(331, 353, 406, 353, 368, 355, ButtonStrMinus, ButtonStrPlus, _addStr, baseStr + _addStr, remaining);

            AddLabel(235, 402, 1152, @"Destreza:");
            AddAttributeControls(331, 399, 406, 399, 368, 401, ButtonDexMinus, ButtonDexPlus, _addDex, baseDex + _addDex, remaining);

            AddLabel(235, 449, 1152, @"Inteligencia:");
            AddAttributeControls(331, 447, 406, 447, 368, 449, ButtonIntMinus, ButtonIntPlus, _addInt, baseInt + _addInt, remaining);

            AddLabel(489, 380, 1152, @"Pontos Para Distribuição");
            AddLabel(561, 411, 1152, remaining.ToString());

            AddButton(549, 441, 506, 506, ButtonConfirm, GumpButtonType.Reply, 0);
        }

        private void AddStableFrame()
        {
            AddPage(0);
            AddImageTiled(171, 71, 520, 440, 388);
            AddImageTiled(172, 47, 523, 29, 634);
            AddImageTiled(143, 75, 37, 424, 635);
            AddImageTiled(676, 77, 37, 428, 635);
            AddImageTiled(171, 494, 518, 29, 634);
            AddImage(134, 38, 1361);
            AddImage(665, 38, 1361);
            AddImage(664, 483, 1361);
            AddImage(133, 484, 1361);
            AddImage(177, 110, 464);
        }

        private void AddAttributeControls(int minusX, int minusY, int plusX, int plusY, int labelX, int labelY, int minusButtonId, int plusButtonId, int added, int shownValue, int remaining)
        {
            if (added > 0)
                AddButton(minusX, minusY, 548, 548, minusButtonId, GumpButtonType.Reply, 0);

            if (remaining > 0)
                AddButton(plusX, plusY, 547, 547, plusButtonId, GumpButtonType.Reply, 0);

            AddLabel(labelX, labelY, 1152, shownValue.ToString());
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_from == null || _pet == null || _from.Deleted || _pet.Deleted)
                return;

            if (!OSUStablePetSystem.HasTrainablePoints(_pet))
            {
                _from.SendMessage("Esse animal não tem pontos para treinar agora.");
                return;
            }

            int s = _addStr;
            int d = _addDex;
            int i = _addInt;
            int remaining = GetRemainingPoints();

            switch (info.ButtonID)
            {
                case ButtonStrPlus:
                    if (remaining > 0)
                        s++;
                    Reopen(s, d, i);
                    break;

                case ButtonDexPlus:
                    if (remaining > 0)
                        d++;
                    Reopen(s, d, i);
                    break;

                case ButtonIntPlus:
                    if (remaining > 0)
                        i++;
                    Reopen(s, d, i);
                    break;

                case ButtonStrMinus:
                    if (s > 0)
                        s--;
                    Reopen(s, d, i);
                    break;

                case ButtonDexMinus:
                    if (d > 0)
                        d--;
                    Reopen(s, d, i);
                    break;

                case ButtonIntMinus:
                    if (i > 0)
                        i--;
                    Reopen(s, d, i);
                    break;

                case ButtonConfirm:
                    ConfirmTraining();
                    break;
            }
        }

        private void ConfirmTraining()
        {
            if (GetRemainingPoints() > 0)
            {
                _from.SendMessage("Você precisa distribuir todos os pontos antes de confirmar o treinamento.");
                Reopen(_addStr, _addDex, _addInt);
                return;
            }

            string reason;
            if (!OSUStablePetSystem.TryRedistributeLastLevel(_pet, _addStr, _addDex, _addInt, _from, out reason))
            {
                _from.SendMessage(reason);
                Reopen(_addStr, _addDex, _addInt);
                return;
            }

            _from.SendMessage(reason);
            _from.CloseGump(typeof(OSUStableTrainingGump));
        }

        private void Reopen(int addStr, int addDex, int addInt)
        {
            _from.CloseGump(typeof(OSUStableTrainingGump));
            _from.SendGump(new OSUStableTrainingGump(_from, _npc, _pet, addStr, addDex, addInt));
        }

        private int GetRemainingPoints()
        {
            return Math.Max(0, GetTotalTrainablePoints(_pet) - (_addStr + _addDex + _addInt));
        }

        private static int GetTotalTrainablePoints(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            return Math.Max(0, pet.OSUPetLastGainStr + pet.OSUPetLastGainDex + pet.OSUPetLastGainInt);
        }

        private static int GetLastLevelPoints(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            if (pet.OSUPetLastGainLevel == pet.OSUPetLevel && pet.OSUPetLastTrainedLevel != pet.OSUPetLevel)
                return GetTotalTrainablePoints(pet);

            return 0;
        }

        private static int GetNeverDistributedPoints(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            return Math.Max(0, GetTotalTrainablePoints(pet) - GetLastLevelPoints(pet));
        }

        private static int GetBaseStrForPreview(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            return Math.Max(1, pet.RawStr - Math.Max(0, pet.OSUPetLastGainStr));
        }

        private static int GetBaseDexForPreview(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            return Math.Max(1, pet.RawDex - Math.Max(0, pet.OSUPetLastGainDex));
        }

        private static int GetBaseIntForPreview(BaseCreature pet)
        {
            if (pet == null)
                return 0;

            return Math.Max(1, pet.RawInt - Math.Max(0, pet.OSUPetLastGainInt));
        }

        private static string GetAbilityLabel(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return "Nenhuma";

            return SafeLabel(value);
        }

        private static string SafeLabel(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return String.Empty;

            return text.Replace("<", "").Replace(">", "");
        }
    }
}
