using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Custom.Systems.Creation.Engine;
using Server.Custom.Systems.DefQual; // OSUDefQualRegistry + OSUCreationAttribute

namespace Server.Custom.Systems.SkillXP.Gumps
{
    public class OSULevelUpStatsGump : Gump
    {
        private readonly PlayerMobile _pm;

        private enum Buttons
        {
            Close = 0,

            // -1/+1
            StrMinus1 = 10, StrPlus1 = 11,
            DexMinus1 = 12, DexPlus1 = 13,
            IntMinus1 = 14, IntPlus1 = 15,
            HpMinus1 = 16, HpPlus1 = 17,
            StamMinus1 = 18, StamPlus1 = 19,
            ManaMinus1 = 20, ManaPlus1 = 21,

            // -5/+5
            StrMinus5 = 30, StrPlus5 = 31,
            DexMinus5 = 32, DexPlus5 = 33,
            IntMinus5 = 34, IntPlus5 = 35,
            HpMinus5 = 36, HpPlus5 = 37,
            StamMinus5 = 38, StamPlus5 = 39,
            ManaMinus5 = 40, ManaPlus5 = 41,

            Confirm = 100 // botão 591
        }

        public OSULevelUpStatsGump(PlayerMobile pm) : base(0, 0)
        {
            _pm = pm;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            if (_pm == null || _pm.Deleted)
                return;

            EnsureDraftInitialized();
            Build();
        }

        private void EnsureDraftInitialized()
        {
            // cria baseline + draft UMA vez por “sessão” do gump
            if (_pm.OSULvlDraftActive)
                return;

            int baseHp = (_pm.OSUBaseHP > 0) ? _pm.OSUBaseHP : _pm.HitsMax;
            int baseStam = (_pm.OSUBaseStam > 0) ? _pm.OSUBaseStam : _pm.StamMax;
            int baseMana = (_pm.OSUBaseMana > 0) ? _pm.OSUBaseMana : _pm.ManaMax;

            _pm.OSULvlBaseStr = _pm.RawStr;
            _pm.OSULvlBaseDex = _pm.RawDex;
            _pm.OSULvlBaseInt = _pm.RawInt;
            _pm.OSULvlBaseHP = baseHp;
            _pm.OSULvlBaseStam = baseStam;
            _pm.OSULvlBaseMana = baseMana;

            _pm.OSULvlDraftStr = _pm.OSULvlBaseStr;
            _pm.OSULvlDraftDex = _pm.OSULvlBaseDex;
            _pm.OSULvlDraftInt = _pm.OSULvlBaseInt;
            _pm.OSULvlDraftHP = _pm.OSULvlBaseHP;
            _pm.OSULvlDraftStam = _pm.OSULvlBaseStam;
            _pm.OSULvlDraftMana = _pm.OSULvlBaseMana;

            _pm.OSULvlDraftActive = true;
        }

        private void ClearDraft()
        {
            _pm.OSULvlDraftActive = false;
        }

        // custo por ponto: acima de 100 custa 3
        private int CostForOneStep(int currentValue)
        {
            return (currentValue >= 100) ? 3 : 1;
        }

        private int IncreaseCost(int start, int end)
        {
            if (end <= start)
                return 0;

            int cost = 0;
            for (int v = start; v < end; v++)
                cost += CostForOneStep(v);

            return cost;
        }

        private int TotalSpent()
        {
            int spent = 0;

            spent += IncreaseCost(_pm.OSULvlBaseStr, _pm.OSULvlDraftStr);
            spent += IncreaseCost(_pm.OSULvlBaseDex, _pm.OSULvlDraftDex);
            spent += IncreaseCost(_pm.OSULvlBaseInt, _pm.OSULvlDraftInt);
            spent += IncreaseCost(_pm.OSULvlBaseHP, _pm.OSULvlDraftHP);
            spent += IncreaseCost(_pm.OSULvlBaseStam, _pm.OSULvlDraftStam);
            spent += IncreaseCost(_pm.OSULvlBaseMana, _pm.OSULvlDraftMana);

            return spent;
        }

        private int Remaining()
        {
            int total = _pm.OSUPendingStatPoints;
            int rem = total - TotalSpent();
            if (rem < 0) rem = 0;
            return rem;
        }

        private int GetEffectiveMax(OSUCreationAttribute attr)
        {
            int max = 115; // cap padrão

            var flags = _pm.OSUDefQualFlags;
            if (flags == null)
                return max;

            for (int i = 0; i < flags.Count; i++)
            {
                var def = OSUDefQualRegistry.GetById(flags[i]);
                if (def != null)
                    max = def.GetAttributeMax(_pm, attr, max);
            }

            return max;
        }

        private OSUCreationAttribute MapAttr(int idx)
        {
            switch (idx)
            {
                default:
                case 0: return OSUCreationAttribute.Str;
                case 1: return OSUCreationAttribute.Dex;
                case 2: return OSUCreationAttribute.Int;
                case 3: return OSUCreationAttribute.HP;
                case 4: return OSUCreationAttribute.Vit;
                case 5: return OSUCreationAttribute.Mana;
            }
        }

        private void AdjustStat(int idx, int delta)
        {
            int v = GetDraft(idx);
            int baseline = GetBase(idx);

            if (delta < 0)
            {
                int newV = v + delta;
                if (newV < baseline)
                    newV = baseline;

                SetDraft(idx, newV);
                return;
            }

            int max = GetEffectiveMax(MapAttr(idx));
            if (v >= max)
                return;

            int stepsWanted = delta;
            int stepsByMax = max - v;
            int steps = stepsWanted > stepsByMax ? stepsByMax : stepsWanted;
            if (steps <= 0)
                return;

            int rem = Remaining();

            for (int i = 0; i < steps; i++)
            {
                int cost = CostForOneStep(v);
                if (rem < cost)
                    break;

                v++;
                rem -= cost;
            }

            SetDraft(idx, v);
        }


        private int GetDraft(int idx)
        {
            switch (idx)
            {
                default:
                case 0: return _pm.OSULvlDraftStr;
                case 1: return _pm.OSULvlDraftDex;
                case 2: return _pm.OSULvlDraftInt;
                case 3: return _pm.OSULvlDraftHP;
                case 4: return _pm.OSULvlDraftStam;
                case 5: return _pm.OSULvlDraftMana;
            }
        }

        private int GetBase(int idx)
        {
            switch (idx)
            {
                default:
                case 0: return _pm.OSULvlBaseStr;
                case 1: return _pm.OSULvlBaseDex;
                case 2: return _pm.OSULvlBaseInt;
                case 3: return _pm.OSULvlBaseHP;
                case 4: return _pm.OSULvlBaseStam;
                case 5: return _pm.OSULvlBaseMana;
            }
        }

        private void SetDraft(int idx, int v)
        {
            switch (idx)
            {
                case 0: _pm.OSULvlDraftStr = v; break;
                case 1: _pm.OSULvlDraftDex = v; break;
                case 2: _pm.OSULvlDraftInt = v; break;
                case 3: _pm.OSULvlDraftHP = v; break;
                case 4: _pm.OSULvlDraftStam = v; break;
                case 5: _pm.OSULvlDraftMana = v; break;
            }
        }


        private void Build()
        {
            AddPage(0);

            AddImageTiled(575, 197, 412, 442, 374);
            AddLabel(748, 214, 0x481, $"Nivel {_pm.OSULevel}");
            AddLabel(743, 254, 0x481, "Atributos");

            AddImage(892, 534, 341);
            AddImage(552, 534, 342);
            AddImage(552, 169, 339);
            AddImage(892, 169, 340);
            AddImageTiled(664, 171, 232, 48, 355);
            AddImageTiled(969, 288, 33, 254, 346);
            AddImageTiled(662, 609, 236, 48, 356);
            AddImageTiled(557, 292, 33, 242, 345);


            // STR
            AddLabel(655, 300, 0x481, "FORÇA");
            AddImageTiled(656, 324, 49, 28, 399);
            AddLabel(670, 329, 0x481, _pm.OSULvlDraftStr.ToString());

            // HP
            AddLabel(858, 300, 0x481, "VIDA");
            AddImageTiled(851, 324, 49, 28, 399);
            AddLabel(865, 328, 0x481, _pm.OSULvlDraftHP.ToString());

            // DEX
            AddLabel(645, 371, 0x481, "DESTREZA");
            AddImageTiled(656, 395, 49, 28, 399);
            AddLabel(670, 399, 0x481, _pm.OSULvlDraftDex.ToString());

            // STAM
            AddLabel(853, 371, 0x481, "VIGOR");
            AddImageTiled(851, 395, 49, 28, 399);
            AddLabel(865, 399, 0x481, _pm.OSULvlDraftStam.ToString());

            // INT
            AddLabel(637, 443, 0x481, "INTELIGENCIA");
            AddImageTiled(656, 467, 49, 28, 399);
            AddLabel(670, 471, 0x481, _pm.OSULvlDraftInt.ToString());

            // MANA
            AddLabel(854, 442, 0x481, "MANA");
            AddImageTiled(851, 465, 49, 28, 399);
            AddLabel(865, 469, 0x481, _pm.OSULvlDraftMana.ToString());

            int rem = Remaining();
            AddLabel(690, 530, 0x481, $"Pontos Restantes: {rem}/{_pm.OSUPendingStatPoints}");

            // STR
            AddButton(605, 322, 451, 451, (int)Buttons.StrMinus5, GumpButtonType.Reply, 0);
            AddButton(624, 326, 453, 453, (int)Buttons.StrMinus1, GumpButtonType.Reply, 0);
            AddButton(717, 327, 452, 452, (int)Buttons.StrPlus1, GumpButtonType.Reply, 0);
            AddButton(735, 322, 450, 450, (int)Buttons.StrPlus5, GumpButtonType.Reply, 0);

            // HP
            AddButton(799, 322, 451, 451, (int)Buttons.HpMinus5, GumpButtonType.Reply, 0);
            AddButton(818, 326, 453, 453, (int)Buttons.HpMinus1, GumpButtonType.Reply, 0);
            AddButton(912, 326, 452, 452, (int)Buttons.HpPlus1, GumpButtonType.Reply, 0);
            AddButton(931, 322, 450, 450, (int)Buttons.HpPlus5, GumpButtonType.Reply, 0);

            // DEX
            AddButton(605, 393, 451, 451, (int)Buttons.DexMinus5, GumpButtonType.Reply, 0);
            AddButton(624, 397, 453, 453, (int)Buttons.DexMinus1, GumpButtonType.Reply, 0);
            AddButton(716, 396, 452, 452, (int)Buttons.DexPlus1, GumpButtonType.Reply, 0);
            AddButton(735, 392, 450, 450, (int)Buttons.DexPlus5, GumpButtonType.Reply, 0);

            // STAM
            AddButton(799, 393, 451, 451, (int)Buttons.StamMinus5, GumpButtonType.Reply, 0);
            AddButton(818, 397, 453, 453, (int)Buttons.StamMinus1, GumpButtonType.Reply, 0);
            AddButton(910, 396, 452, 452, (int)Buttons.StamPlus1, GumpButtonType.Reply, 0);
            AddButton(931, 392, 450, 450, (int)Buttons.StamPlus5, GumpButtonType.Reply, 0);

            // INT
            AddButton(605, 464, 451, 451, (int)Buttons.IntMinus5, GumpButtonType.Reply, 0);
            AddButton(624, 469, 453, 453, (int)Buttons.IntMinus1, GumpButtonType.Reply, 0);
            AddButton(716, 466, 452, 452, (int)Buttons.IntPlus1, GumpButtonType.Reply, 0);
            AddButton(735, 462, 450, 450, (int)Buttons.IntPlus5, GumpButtonType.Reply, 0);

            // MANA
            AddButton(799, 464, 451, 451, (int)Buttons.ManaMinus5, GumpButtonType.Reply, 0);
            AddButton(818, 468, 453, 453, (int)Buttons.ManaMinus1, GumpButtonType.Reply, 0);
            AddButton(910, 466, 452, 452, (int)Buttons.ManaPlus1, GumpButtonType.Reply, 0);
            AddButton(929, 462, 450, 450, (int)Buttons.ManaPlus5, GumpButtonType.Reply, 0);

            // Confirmar
            AddButton(758, 568, 591, 591, (int)Buttons.Confirm, GumpButtonType.Reply, 0);
        }

        private void Reopen()
        {
            _pm.CloseGump(typeof(OSULevelUpStatsGump));
            _pm.SendGump(new OSULevelUpStatsGump(_pm));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_pm == null || _pm.Deleted)
                return;

            int bid = info.ButtonID;

            // ✅ IMPORTANTE: se o jogador fechou no X (ButtonID=0), NÃO reabre
            if (bid == 0)
            {
                ClearDraft(); // você pediu que ao fechar ele perca o progresso
                return;
            }

            switch ((Buttons)bid)
            {
                case Buttons.StrMinus1: AdjustStat(0, -1); break;
                case Buttons.StrPlus1: AdjustStat(0, +1); break;
                case Buttons.StrMinus5: AdjustStat(0, -5); break;
                case Buttons.StrPlus5: AdjustStat(0, +5); break;

                case Buttons.DexMinus1: AdjustStat(1, -1); break;
                case Buttons.DexPlus1: AdjustStat(1, +1); break;
                case Buttons.DexMinus5: AdjustStat(1, -5); break;
                case Buttons.DexPlus5: AdjustStat(1, +5); break;

                case Buttons.IntMinus1: AdjustStat(2, -1); break;
                case Buttons.IntPlus1: AdjustStat(2, +1); break;
                case Buttons.IntMinus5: AdjustStat(2, -5); break;
                case Buttons.IntPlus5: AdjustStat(2, +5); break;

                case Buttons.HpMinus1: AdjustStat(3, -1); break;
                case Buttons.HpPlus1: AdjustStat(3, +1); break;
                case Buttons.HpMinus5: AdjustStat(3, -5); break;
                case Buttons.HpPlus5: AdjustStat(3, +5); break;

                case Buttons.StamMinus1: AdjustStat(4, -1); break;
                case Buttons.StamPlus1: AdjustStat(4, +1); break;
                case Buttons.StamMinus5: AdjustStat(4, -5); break;
                case Buttons.StamPlus5: AdjustStat(4, +5); break;

                case Buttons.ManaMinus1: AdjustStat(5, -1); break;
                case Buttons.ManaPlus1: AdjustStat(5, +1); break;
                case Buttons.ManaMinus5: AdjustStat(5, -5); break;
                case Buttons.ManaPlus5: AdjustStat(5, +5); break;


                case Buttons.Confirm:
                    {
                        if (_pm.OSUPendingStatPoints <= 0)
                        {
                            _pm.SendMessage(0x35, "Você não tem pontos de atributos pendentes.");
                            return;
                        }

                        if (Remaining() != 0)
                        {
                            _pm.SendMessage(0x35, $"Você precisa distribuir todos os {_pm.OSUPendingStatPoints} pontos antes de confirmar.");
                            Reopen();
                            return;
                        }

                        // aplica no personagem
                        _pm.RawStr = _pm.OSULvlDraftStr;
                        _pm.RawDex = _pm.OSULvlDraftDex;
                        _pm.RawInt = _pm.OSULvlDraftInt;

                        _pm.OSUBaseHP = _pm.OSULvlDraftHP;
                        _pm.OSUBaseStam = _pm.OSULvlDraftStam;
                        _pm.OSUBaseMana = _pm.OSULvlDraftMana;

                        _pm.Hits = _pm.HitsMax;
                        _pm.Stam = _pm.StamMax;
                        _pm.Mana = _pm.ManaMax;

                        _pm.OSUPendingStatPoints = 0;

                        ClearDraft();
                        _pm.SendMessage(0x35, "Atributos aplicados com sucesso.");
                        return;
                    }
            }

            Reopen();
        }
    }
}
