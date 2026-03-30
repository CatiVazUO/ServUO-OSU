using Server;
using Server.Custom.Systems.SkillXP;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class OSUGMGump : OSUBaseGump
    {
        public enum Mode
        {
            Feats = 0,
            Abilities = 1
        }

        private readonly Mobile _gm;
        private readonly PlayerMobile _target;
        private readonly Mode _mode;
        private readonly int _page;

        private const int PerPage = 12;
        private const int ButtonBase = 1000;

        private class Entry
        {
            public int Id;
            public string Name;
            public bool Owned;
        }

        // ✅ agora recebe posição
        public OSUGMGump(Mobile gm, PlayerMobile target, Mode mode, int page, int x, int y)
            : base(x, y)
        {
            _gm = gm;
            _target = target;
            _mode = mode;
            _page = (page < 0 ? 0 : page);

            if (_gm == null || _target == null)
                return;

            Build();
        }

        // fallback antigo (se algo chamar sem posição)
        public OSUGMGump(Mobile gm, PlayerMobile target, Mode mode, int page)
            : this(gm, target, mode, page, 0, 0)
        {
        }

        private void Build()
        {
            AddLabel(747, 310, LabelHue, "GM GUMP");

            // Tabs (mesmo estilo do SkillGump)
            AddButton(564, 360, 442, 441, (int)Buttons.TabFeats, GumpButtonType.Reply, 0);
            AddButton(702, 360, 442, 441, (int)Buttons.TabHabs, GumpButtonType.Reply, 0);

            AddLabel(609, 370, LabelHue, "Feats");
            AddLabel(747, 370, LabelHue, "Habs");

            // Cabeçalho
            AddLabel(582, 430, LabelHue, "Nome");
            AddLabel(902, 430, LabelHue, "Dar");

            AddLabel(582, 405, LabelHue, "Alvo: " + (_target != null ? _target.Name : "(null)"));

            List<Entry> list = BuildEntries();

            int totalPages = (list.Count + PerPage - 1) / PerPage;
            if (totalPages < 1) totalPages = 1;

            int currentPage = _page;
            if (currentPage >= totalPages)
                currentPage = totalPages - 1;

            AddLabel(873, 772, LabelHue, "Página: " + (currentPage + 1) + "/" + totalPages);

            int start = currentPage * PerPage;
            int end = start + PerPage;
            if (end > list.Count) end = list.Count;

            int y = 479;

            if (list.Count == 0)
            {
                AddLabel(576, y, LabelHue, "(nenhum)");
            }
            else
            {
                for (int i = start; i < end; i++)
                {
                    Entry en = list[i];

                    AddLabel(576, y, LabelHue, en.Name);

                    // ✅ 2361 se não tem, 2360 se já tem
                    int art = en.Owned ? 2360 : 2361;

                    // botão por índice ABSOLUTO (i), não relativo
                    AddButton(925, y, art, art, ButtonBase + i, GumpButtonType.Reply, 0);

                    y += 34;
                }
            }

            // paginação (igual skill gump)
            if (currentPage > 0)
                AddButton(563, 815, 448, 448, (int)Buttons.Prev, GumpButtonType.Reply, 0);

            bool isLast = (currentPage >= totalPages - 1);
            if (!isLast)
                AddButton(917, 815, 449, 449, (int)Buttons.Next, GumpButtonType.Reply, 0);
        }

        private List<Entry> BuildEntries()
        {
            List<Entry> list = new List<Entry>();

            if (_target == null)
                return list;

            if (_mode == Mode.Abilities)
            {
                List<OSUAbilityDefinition> defs = OSUAbilitySystem.GetAll();
                for (int i = 0; i < defs.Count; i++)
                {
                    OSUAbilityDefinition d = defs[i];
                    if (d == null) continue;

                    list.Add(new Entry
                    {
                        Id = d.Id,
                        Name = d.Name + " (" + d.Id + ")",
                        Owned = _target.HasOSUAbility(d.Id)
                    });
                }
            }
            else
            {
                Dictionary<int, Entry> byId = new Dictionary<int, Entry>();

                Array values = Enum.GetValues(typeof(SkillName));
                for (int s = 0; s < values.Length; s++)
                {
                    SkillName sn = (SkillName)values.GetValue(s);

                    List<OSUFeatDefinition> feats = OSUFeatSystem.GetFeats(sn);
                    if (feats == null) continue;

                    for (int i = 0; i < feats.Count; i++)
                    {
                        OSUFeatDefinition f = feats[i];
                        if (f == null) continue;

                        if (!byId.ContainsKey(f.Id))
                        {
                            byId[f.Id] = new Entry
                            {
                                Id = f.Id,
                                Name = f.Name + " (" + f.Id + ")",
                                Owned = _target.HasOSUFeat(f.Id)
                            };
                        }
                    }
                }

                foreach (Entry e in byId.Values)
                    list.Add(e);
            }

            list.Sort(delegate (Entry a, Entry b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return list;
        }

        private void RefreshSame(int newPage)
        {
            _gm.CloseGump(typeof(OSUGMGump));
            _gm.SendGump(new OSUGMGump(_gm, _target, _mode, newPage, this.X, this.Y));
        }

        private void SwitchMode(Mode m)
        {
            _gm.CloseGump(typeof(OSUGMGump));
            _gm.SendGump(new OSUGMGump(_gm, _target, m, 0, this.X, this.Y));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (_gm == null || _target == null)
                return;

            int id = info.ButtonID;

            if (id == (int)Buttons.TabFeats)
            {
                SwitchMode(Mode.Feats);
                return;
            }

            if (id == (int)Buttons.TabHabs)
            {
                SwitchMode(Mode.Abilities);
                return;
            }

            if (id == (int)Buttons.Prev)
            {
                RefreshSame(_page - 1);
                return;
            }

            if (id == (int)Buttons.Next)
            {
                RefreshSame(_page + 1);
                return;
            }

            if (id >= ButtonBase)
            {
                int index = id - ButtonBase;

                List<Entry> list = BuildEntries();
                if (index < 0 || index >= list.Count)
                {
                    RefreshSame(_page);
                    return;
                }

                Entry en = list[index];

                if (_mode == Mode.Abilities)
                {
                    if (_target.HasOSUAbility(en.Id))
                    {
                        _gm.SendMessage(0x22, "O jogador já possui esta habilidade.");
                    }
                    else
                    {
                        _target.AddOSUAbility(en.Id);

                        try
                        {
                            IOSUAbility obj = OSUAbilitySystem.GetAbilityById(en.Id);
                            if (obj != null)
                                obj.OnPurchased(_target);
                        }
                        catch { }

                        _gm.SendMessage(0x55, "Habilidade adicionada: " + en.Name);
                        _target.SendMessage(0x55, "Você recebeu a habilidade: " + en.Name);
                    }
                }
                else
                {
                    if (_target.HasOSUFeat(en.Id))
                    {
                        _gm.SendMessage(0x22, "O jogador já possui esta feat.");
                    }
                    else
                    {
                        _target.AddOSUFeat(en.Id);
                        _gm.SendMessage(0x55, "Feat adicionada: " + en.Name);
                        _target.SendMessage(0x55, "Você recebeu a feat: " + en.Name);
                    }
                }

                RefreshSame(_page);
                return;
            }
        }

        private enum Buttons
        {
            TabFeats = 1,
            TabHabs = 2,
            Next = 3,
            Prev = 4
        }
    }
}
