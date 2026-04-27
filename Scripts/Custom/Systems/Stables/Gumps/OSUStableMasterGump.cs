using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Systems.Stables.Engine;
using Server.Custom.Systems.Stables.Mobiles;

namespace Server.Custom.Systems.Stables.Gumps
{
    public class OSUStableMasterGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly OSUStableMaster _npc;
        private readonly List<BaseMount> _ready;
        private readonly int _selectedAction;

        private const int ActionTraining = 1;
        private const int ActionBreeding = 2;
        private const int ActionCastration = 3;
        private const int ActionBranding = 4;
        private const int ActionClaimReady = 5;
        private const int ActionOk = 10;

        public OSUStableMasterGump(PlayerMobile from, OSUStableMaster npc)
            : this(from, npc, 0)
        {
        }

        public OSUStableMasterGump(PlayerMobile from, OSUStableMaster npc, int selectedAction) : base(0, 0)
        {
            _from = from;
            _npc = npc;
            _selectedAction = selectedAction;
            _ready = OSUStablePetSystem.GetReadyServicePets(from, npc != null ? npc.GovernmentCityId : -1);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddStableFrame();

            AddPage(1);
            AddLabel(400, 90, 1152, @"Estábulos");

            if (_ready.Count > 0)
            {
                AddLabel(227, 145, 1152, @"Retirada");
                AddButton(195, 142, 540, 518, ActionClaimReady, GumpButtonType.Reply, 0);
                AddImageTiled(192, 177, 234, 12, 634);
            }

            AddActionButton(211, 205, 241, 205, @"Treinar Animal", ActionTraining);
            AddActionButton(211, 236, 241, 235, @"Cruzar Animais", ActionBreeding);
            AddActionButton(211, 265, 241, 265, @"Castrar Animal", ActionCastration);
            AddActionButton(211, 295, 241, 295, @"Marcar Animal", ActionBranding);

            AddHtml(449, 146, 211, 206, BuildCostHtml(), false, false);
            AddHtml(199, 397, 461, 87, BuildDescriptionHtml(_selectedAction), false, false);

            AddButton(379, 306, 506, 506, ActionOk, GumpButtonType.Reply, 0);
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

            AddImageTiled(192, 173, 220, 12, 634);
            AddImageTiled(202, 362, 455, 12, 634);
            AddImageTiled(415, 144, 13, 212, 635);
        }

        private void AddActionButton(int buttonX, int buttonY, int labelX, int labelY, string label, int action)
        {
            int normalId = (_selectedAction == action) ? 541 : 454;
            AddButton(buttonX, buttonY, normalId, 543, action, GumpButtonType.Reply, 0);
            AddLabel(labelX, labelY, 1152, label);
        }

        private static string BuildCostHtml()
        {
            return "<BASEFONT COLOR=#FFFFFF>" +
                "<B>Custo dos serviços</B><BR><BR>" +
                "Treinar: " + OSUStablePetSystem.TrainingCostGold + " moedas<BR>" +
                "Cruzar: " + OSUStablePetSystem.BreedingCostGold + " moedas<BR>" +
                "Castrar: " + OSUStablePetSystem.CastrationCostGold + " moedas<BR>" +
                "Marcar: " + OSUStablePetSystem.BrandingCostGold + " moedas<BR><BR>" +
                "Retirada atrasada: " + OSUStablePetSystem.LateClaimFeeGold + " moedas por período." +
                "</BASEFONT>";
        }

        private static string BuildDescriptionHtml(int selectedAction)
        {
            string text;

            switch (selectedAction)
            {
                case ActionTraining:
                    text = "<B>Treinar Animal</B><BR>Abre o treinamento do animal. " +
                        "Nele você distribui os pontos que o animal ganhou ao subir de nível a última vez, podendo alterar os pontos " +
                        "distribuidos entre força,  destreza e inteligência.";
                    break;
                case ActionBreeding:
                    text = "<B>Cruzar Animais</B><BR>Escolha um macho e uma fêmea do mesmo tipo. É necessário deixar os animais" +
                        "no estabulo por 4 dias, depois desse período você já pode retira-los do estábulo, mas a cria ainda fica sob " +
                        "os cuidados do estábulo por 10 dias, depois disso você pode retira-la também.";
                    break;
                case ActionCastration:
                    text = "<B>Castrar Animal</B><BR>Inicia a castração do animal. Esse caminho impede o uso" +
                        " reprodutivo futuro e só pode ser escolhido antes do primeiro cruzamento do animal.";
                    break;
                case ActionBranding:
                    text = "<B>Marcar Animal</B><BR>Marca o animal como pertencente ao dono. Em animais de " +
                        "fazenda, a marca permite uso para recursos sem que o animal seja um dos seus seguidores. Em algums minutos " +
                        "o animal marcado vira um animal de pasto e você perde o controle sobre ele, porém ele se mantem seu e provém" +
                        "recursos só pra você.";

                    break;
                default:
                    text = "Escolha uma ação do estábulo. Depois de selecionar, aperte OK para confirmar.";
                    break;
            }

            return "<BASEFONT COLOR=#FFFFFF>" + text + "</BASEFONT>";
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_from == null || _npc == null || _from.Deleted || _npc.Deleted)
                return;

            switch (info.ButtonID)
            {
                case ActionTraining:
                case ActionBreeding:
                case ActionCastration:
                case ActionBranding:
                    _from.CloseGump(typeof(OSUStableMasterGump));
                    _from.SendGump(new OSUStableMasterGump(_from, _npc, info.ButtonID));
                    break;

                case ActionClaimReady:
                    ClaimFirstReadyService();
                    break;

                case ActionOk:
                    ConfirmSelectedAction();
                    break;
            }
        }

        private void ClaimFirstReadyService()
        {
            List<BaseMount> readyNow = OSUStablePetSystem.GetReadyServicePets(_from, _npc.GovernmentCityId);

            if (readyNow.Count == 0)
            {
                _from.SendMessage("Você não tem nenhum serviço pronto para retirar neste estábulo.");
                _from.CloseGump(typeof(OSUStableMasterGump));
                _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                return;
            }

            BaseMount pet = readyNow[0];
            string reason;

            if (!OSUStablePetSystem.TryClaimReadyService(_from, _npc, pet, out reason))
            {
                _from.SendMessage(reason);
                _from.CloseGump(typeof(OSUStableMasterGump));
                _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                return;
            }

            _from.SendMessage(reason);
            _from.CloseGump(typeof(OSUStableMasterGump));
            _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
        }

        private void ConfirmSelectedAction()
        {
            switch (_selectedAction)
            {
                case ActionTraining:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.TrainingFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Treinar Animais.");
                        _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                        return;
                    }
                    _from.Target = new TrainingTarget(_from, _npc);
                    _from.SendMessage("Escolha o animal que você quer treinar.");
                    break;

                case ActionBreeding:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.BreedingFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Cruzar Animais.");
                        _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                        return;
                    }
                    _from.Target = new BreedingFirstTarget(_from, _npc);
                    _from.SendMessage("Escolha o primeiro animal do cruzamento.");
                    break;

                case ActionCastration:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.CastrationFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Castrar Animais.");
                        _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                        return;
                    }
                    _from.Target = new CastrationTarget(_from, _npc);
                    _from.SendMessage("Escolha o animal que você quer castrar.");
                    break;

                case ActionBranding:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.BrandingFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Marcar Animais.");
                        _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                        return;
                    }
                    _from.Target = new BrandingTarget(_from, _npc);
                    _from.SendMessage("Escolha o animal que você quer marcar.");
                    break;

                default:
                    _from.SendMessage("Escolha primeiro uma ação do estábulo.");
                    _from.CloseGump(typeof(OSUStableMasterGump));
                    _from.SendGump(new OSUStableMasterGump(_from, _npc, _selectedAction));
                    break;
            }
        }

        private class TrainingTarget : Target
        {
            private readonly PlayerMobile _from;
            private readonly OSUStableMaster _npc;

            public TrainingTarget(PlayerMobile from, OSUStableMaster npc) : base(2, false, TargetFlags.None)
            {
                _from = from;
                _npc = npc;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature pet = targeted as BaseCreature;
                if (pet == null)
                    return;

                OSUStablePetSystem.EnsureInitialized(pet);

                if (!pet.Controlled || pet.ControlMaster != _from)
                {
                    _from.SendMessage("Esse animal não está sob seu controle.");
                    return;
                }

                if (!OSUStablePetSystem.HasTrainablePoints(pet))
                {
                    _from.SendMessage("Esse animal não tem pontos para treinar agora.");
                    return;
                }

                _from.CloseGump(typeof(OSUStableTrainingGump));
                _from.SendGump(new OSUStableTrainingGump(_from, _npc, pet));
            }
        }

        private class BreedingFirstTarget : Target
        {
            private readonly PlayerMobile _from;
            private readonly OSUStableMaster _npc;

            public BreedingFirstTarget(PlayerMobile from, OSUStableMaster npc) : base(2, false, TargetFlags.None)
            {
                _from = from;
                _npc = npc;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature first = targeted as BaseCreature;
                if (first == null)
                    return;

                _from.Target = new BreedingSecondTarget(_from, _npc, first);
                _from.SendMessage("Escolha o segundo animal do cruzamento.");
            }
        }

        private class BreedingSecondTarget : Target
        {
            private readonly PlayerMobile _from;
            private readonly OSUStableMaster _npc;
            private readonly BaseCreature _first;

            public BreedingSecondTarget(PlayerMobile from, OSUStableMaster npc, BaseCreature first) : base(2, false, TargetFlags.None)
            {
                _from = from;
                _npc = npc;
                _first = first;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature second = targeted as BaseCreature;
                if (second == null)
                    return;

                string reason;
                if (!OSUStablePetSystem.TryStartBreeding(_from, _npc, _first, second, _npc.GovernmentCityId, out reason))
                {
                    _from.SendMessage(reason);
                    return;
                }

                _from.SendMessage(reason);
            }
        }

        private class CastrationTarget : Target
        {
            private readonly PlayerMobile _from;
            private readonly OSUStableMaster _npc;

            public CastrationTarget(PlayerMobile from, OSUStableMaster npc) : base(2, false, TargetFlags.None)
            {
                _from = from;
                _npc = npc;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature pet = targeted as BaseCreature;
                if (pet == null)
                    return;

                string reason;
                if (!OSUStablePetSystem.TryStartCastration(_from, pet, _npc.GovernmentCityId, out reason))
                {
                    _from.SendMessage(reason);
                    return;
                }

                _from.SendMessage(reason);
            }
        }

        private class BrandingTarget : Target
        {
            private readonly PlayerMobile _from;
            private readonly OSUStableMaster _npc;

            public BrandingTarget(PlayerMobile from, OSUStableMaster npc) : base(2, false, TargetFlags.None)
            {
                _from = from;
                _npc = npc;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseCreature pet = targeted as BaseCreature;
                if (pet == null)
                    return;

                string reason;
                if (!OSUStablePetSystem.TryMarkAnimal(_from, pet, _npc.GovernmentCityId, out reason))
                {
                    _from.SendMessage(reason);
                    return;
                }

                _from.SendMessage(reason);
            }
        }
    }
}
