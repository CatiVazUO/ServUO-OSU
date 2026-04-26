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

        public OSUStableMasterGump(PlayerMobile from, OSUStableMaster npc) : base(80, 60)
        {
            _from = from;
            _npc = npc;
            _ready = OSUStablePetSystem.GetReadyServicePets(from, npc != null ? npc.GovernmentCityId : -1);

            Closable = true;
            Dragable = true;
            AddPage(0);
            AddBackground(0, 0, 520, 360, 5054);
            AddAlphaRegion(15, 15, 490, 330);
            AddLabel(190, 20, 1152, "Estábulo do Reino");
            AddHtml(35, 55, 450, 45, "<BASEFONT COLOR=#FFFFFF>Serviços atuais: treino, cruzamento, castração, marcação e retirada de serviços prontos.</BASEFONT>", false, true);

            if (_ready.Count > 0)
                AddHtml(35, 100, 450, 40, "<BASEFONT COLOR=#00FF7F><B>Você tem serviço pronto para retirar.</B></BASEFONT>", false, true);
            else
                AddHtml(35, 100, 450, 40, "<BASEFONT COLOR=#AAAAAA>Nenhum serviço pronto no momento.</BASEFONT>", false, true);

            AddButton(40, 155, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(75, 155, 1152, "Treinar animal");

            AddButton(40, 190, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(75, 190, 1152, "Cruzar animais");

            AddButton(40, 225, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(75, 225, 1152, "Castrar animal");

            AddButton(40, 260, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddLabel(75, 260, 1152, "Marcar animal");

            AddButton(40, 295, 4005, 4007, 5, GumpButtonType.Reply, 0);
            AddLabel(75, 295, 1152, "Retirar serviços prontos");

            AddHtml(280, 150, 190, 145,
                "<BASEFONT COLOR=#FFFFFF>" +
                "<B>Custos atuais de teste</B><BR>" +
                "Treino: " + OSUStablePetSystem.TrainingCostGold + " moedas<BR>" +
                "Cruzar: " + OSUStablePetSystem.BreedingCostGold + " moedas<BR>" +
                "Castrar: " + OSUStablePetSystem.CastrationCostGold + " moedas<BR>" +
                "Marcar: " + OSUStablePetSystem.BrandingCostGold + " moedas<BR><BR>" +
                "<B>Obs.</B> Os tempos estão em segundos para teste. Depois você troca para dias." +
                "</BASEFONT>", false, true);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_from == null || _npc == null || _from.Deleted || _npc.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 1:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.TrainingFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Treinar Animais.");
                        return;
                    }
                    _from.Target = new TrainingTarget(_from, _npc);
                    _from.SendMessage("Escolha o animal que você quer treinar.");
                    break;
                case 2:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.BreedingFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Cruzar Animais.");
                        return;
                    }
                    _from.Target = new BreedingFirstTarget(_from, _npc);
                    _from.SendMessage("Escolha o primeiro animal do cruzamento.");
                    break;
                case 3:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.CastrationFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Castrar Animais.");
                        return;
                    }
                    _from.Target = new CastrationTarget(_from, _npc);
                    _from.SendMessage("Escolha o animal que você quer castrar.");
                    break;
                case 4:
                    if (!OSUStablePetSystem.CanUseStableService(_from, OSUStablePetSystem.BrandingFeatId))
                    {
                        _from.SendMessage("Você não tem o feat de Marcar Animais.");
                        return;
                    }
                    _from.Target = new BrandingTarget(_from, _npc);
                    _from.SendMessage("Escolha o animal que você quer marcar.");
                    break;
                case 5:
                    _from.CloseGump(typeof(OSUStableClaimGump));
                    _from.SendGump(new OSUStableClaimGump(_from, _npc));
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
                    _from.SendMessage("Esse animal não tem pontos do último nível para redistribuir.");
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
                _from.SendMessage("Agora escolha o segundo animal do cruzamento.");
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

    public class OSUStableTrainingGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly OSUStableMaster _npc;
        private readonly BaseCreature _pet;

        private const int EntryStr = 1;
        private const int EntryDex = 2;
        private const int EntryInt = 3;

        public OSUStableTrainingGump(PlayerMobile from, OSUStableMaster npc, BaseCreature pet) : base(120, 90)
        {
            _from = from;
            _npc = npc;
            _pet = pet;

            Closable = true;
            Dragable = true;
            AddBackground(0, 0, 420, 240, 5054);
            AddAlphaRegion(15, 15, 390, 210);
            AddLabel(115, 20, 1152, "Redistribuir último nível");
            AddLabel(35, 55, 1152, "Animal: " + (_pet != null ? _pet.Name : "?"));

            int total = pet != null ? (pet.OSUPetLastGainStr + pet.OSUPetLastGainDex + pet.OSUPetLastGainInt) : 0;
            AddLabel(35, 82, 1152, "Pontos do último nível: " + total);
            AddLabel(35, 115, 1152, "Força");
            AddTextEntry(135, 115, 70, 20, 0, EntryStr, pet != null ? pet.OSUPetLastGainStr.ToString() : "0");
            AddLabel(35, 145, 1152, "Destreza");
            AddTextEntry(135, 145, 70, 20, 0, EntryDex, pet != null ? pet.OSUPetLastGainDex.ToString() : "0");
            AddLabel(35, 175, 1152, "Inteligência");
            AddTextEntry(135, 175, 70, 20, 0, EntryInt, pet != null ? pet.OSUPetLastGainInt.ToString() : "0");
            AddButton(280, 175, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(315, 175, 1152, "Confirmar");
            AddHtml(230, 70, 150, 85, "<BASEFONT COLOR=#FFFFFF>No nível 5 e no 10, ao treinar, o animal tem 50% de chance de ganhar uma habilidade especial.</BASEFONT>", false, true);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID != 1 || _pet == null || _npc == null || _from == null)
                return;

            int s = GetEntry(info, EntryStr);
            int d = GetEntry(info, EntryDex);
            int i = GetEntry(info, EntryInt);

            string reason;
            if (!OSUStablePetSystem.TryRedistributeLastLevel(_pet, s, d, i, _from, out reason))
            {
                _from.SendMessage(reason);
                return;
            }

            _from.SendMessage(reason);
            _from.CloseGump(typeof(OSUAnimalStatusGump));
            _from.SendGump(new OSUAnimalStatusGump(_from, _pet));
        }

        private static int GetEntry(RelayInfo info, int id)
        {
            TextRelay tr = info.GetTextEntry(id);
            int v;
            if (tr == null || !Int32.TryParse(tr.Text, out v))
                return 0;
            return Math.Max(0, v);
        }
    }

    public class OSUStableClaimGump : Gump
    {
        private readonly PlayerMobile _from;
        private readonly OSUStableMaster _npc;
        private readonly List<BaseMount> _list;

        public OSUStableClaimGump(PlayerMobile from, OSUStableMaster npc) : base(130, 90)
        {
            _from = from;
            _npc = npc;
            _list = OSUStablePetSystem.GetReadyServicePets(from, npc != null ? npc.GovernmentCityId : -1);

            AddBackground(0, 0, 520, 320, 5054);
            AddAlphaRegion(15, 15, 490, 290);
            AddLabel(160, 20, 1152, "Serviços prontos para retirada");

            int y = 60;
            for (int idx = 0; idx < _list.Count && idx < 8; idx++)
            {
                BaseMount pet = _list[idx];
                int fee = OSUStablePetSystem.GetLateFee(pet);
                AddButton(35, y, 4005, 4007, 100 + idx, GumpButtonType.Reply, 0);
                AddHtml(70, y - 3, 390, 24,
                    "<BASEFONT COLOR=#FFFFFF><B>" + pet.Name + "</B> - " +
                    ((OSUStableServiceKind)pet.OSUPetServiceKind).ToString() +
                    (fee > 0 ? (" - taxa extra atual: " + fee + " moedas") : "") +
                    "</BASEFONT>", false, false);
                y += 30;
            }

            if (_list.Count == 0)
                AddHtml(40, 75, 420, 30, "<BASEFONT COLOR=#AAAAAA>Nenhum serviço pronto neste estábulo.</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_from == null || _npc == null)
                return;

            if (info.ButtonID < 100)
                return;

            int idx = info.ButtonID - 100;
            if (idx < 0 || idx >= _list.Count)
                return;

            BaseMount pet = _list[idx];
            string reason;
            if (!OSUStablePetSystem.TryClaimReadyService(_from, _npc, pet, out reason))
            {
                _from.SendMessage(reason);
                _from.SendGump(new OSUStableClaimGump(_from, _npc));
                return;
            }

            _from.SendMessage(reason);
            _from.SendGump(new OSUStableClaimGump(_from, _npc));
        }
    }

    public class OSUAnimalStatusGump : Gump
    {
        public OSUAnimalStatusGump(Mobile from, BaseCreature pet) : base(120, 70)
        {
            OSUStablePetSystem.EnsureInitialized(pet);

            AddBackground(0, 0, 460, 335, 5054);
            AddAlphaRegion(15, 15, 430, 305);
            AddLabel(150, 20, 1152, "Status do Animal");
            AddHtml(30, 55, 390, 245,
                "<BASEFONT COLOR=#FFFFFF>" +
                "<B>Nome:</B> " + pet.Name + "<BR>" +
                "<B>Atributos:</B> STR " + pet.RawStr + " / DEX " + pet.RawDex + " / INT " + pet.RawInt + "<BR>" +
                "<B>Nível:</B> " + pet.OSUPetLevel + "<BR>" +
                "<B>XP para o próximo:</B> " + pet.OSUPetXP + "/" + pet.OSUPetNextLevelXP + "<BR>" +
                "<B>Lealdade:</B> " + pet.Loyalty + "/" + BaseCreature.MaxLoyalty + " (" + OSUStablePetSystem.GetLoyaltyLabel(pet) + ")<BR>" +
                "<B>Pontos redistribuíveis:</B> " + (pet.OSUPetLastGainStr + pet.OSUPetLastGainDex + pet.OSUPetLastGainInt) + "<BR>" +
                "<B>Habilidade lvl 5:</B> " + (String.IsNullOrWhiteSpace(pet.OSUPetAbilitySlot5) ? "nenhuma" : pet.OSUPetAbilitySlot5) + "<BR>" +
                "<B>Habilidade lvl 10:</B> " + (String.IsNullOrWhiteSpace(pet.OSUPetAbilitySlot10) ? "nenhuma" : pet.OSUPetAbilitySlot10) + "<BR>" +
                "<B>Marcado:</B> " + (pet.OSUPetMarked ? "sim" : "não") + "<BR>" +
                "<B>Castrado:</B> " + (pet.OSUPetCastrated ? "sim" : "não") + "<BR>" +
                "<B>Vidas:</B> " + pet.OSUPetLivesRemaining + "/" + pet.OSUPetLivesMax + "<BR>" +
                "<B>Cruzamentos:</B> " + pet.OSUPetBreedCount + "/" + pet.OSUPetBreedCountMax +
                "</BASEFONT>", false, true);
        }
    }
}
