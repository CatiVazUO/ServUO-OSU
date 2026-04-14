using System;
using System.Collections.Generic;
using Server.Custom.Systems.PlayerMadeStatues;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Gumps
{
    public class SculptorMainGump : Gump
    {
        public enum SculptorMenuMode
        {
            Platforms = 1,
            Sculptures = 2
        }

        private readonly PlayerMobile m_From;
        private readonly SculptorTools m_Tool;
        private readonly SculptorMenuMode m_Mode;
        private readonly int m_MaterialId;
        private readonly StatuePlatformSize m_SelectedPlatformSize;
        private readonly int m_SelectedPlatformRecipeIndex;
        private readonly int m_PlatformPage;
        private readonly StatueCraftCategory m_SelectedSculptureCategory;
        private readonly int m_SelectedSculptureRecipeIndex;
        private readonly int m_SculpturePage;
        private readonly GenericSign m_SelectedSign;
        private readonly Mobile m_SelectedLiveModel;

        public SculptorMainGump(PlayerMobile from, SculptorTools tool, int materialId)
            : this(from, tool, SculptorMenuMode.Platforms, materialId, StatuePlatformSize.Small, -1, 0, StatueCraftCategory.Small, -1, 0, null, null)
        {
        }

        public SculptorMainGump(PlayerMobile from, SculptorTools tool, SculptorMenuMode mode, int materialId, StatuePlatformSize selectedPlatformSize,
            int selectedPlatformRecipeIndex, int platformPage, StatueCraftCategory selectedSculptureCategory, int selectedSculptureRecipeIndex,
            int sculpturePage, GenericSign selectedSign, Mobile selectedLiveModel)
            : base(0, 0)
        {
            m_From = from;
            m_Tool = tool;
            m_Mode = mode;
            m_MaterialId = NormalizeMaterial(materialId);
            m_SelectedPlatformSize = selectedPlatformSize;
            m_SelectedPlatformRecipeIndex = selectedPlatformRecipeIndex;
            m_PlatformPage = platformPage < 0 ? 0 : platformPage;
            m_SelectedSculptureCategory = selectedSculptureCategory;
            m_SelectedSculptureRecipeIndex = selectedSculptureRecipeIndex;
            m_SculpturePage = sculpturePage < 0 ? 0 : sculpturePage;
            m_SelectedSign = selectedSign;
            m_SelectedLiveModel = selectedLiveModel;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            DrawBase();
            if (m_Mode == SculptorMenuMode.Platforms)
                DrawPlatformsPage();
            else
                DrawSculpturesPage();
        }

        private int NormalizeMaterial(int materialId)
        {
            StatueMaterialOption[] mats = StatueMaterialOptions.All;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].MaterialId == materialId)
                    return materialId;
            }
            return mats.Length > 0 ? mats[0].MaterialId : materialId;
        }

        private void DrawBase()
        {
            AddPage(0);
            AddImageTiled(173, 72, 798, 440, 382);
            AddImageTiled(172, 47, 794, 29, 634);
            AddImageTiled(143, 75, 37, 424, 635);
            AddImageTiled(961, 77, 37, 428, 635);
            AddImageTiled(171, 494, 798, 29, 634);
            AddImage(134, 38, 1361);
            AddImage(950, 38, 1361);
            AddImage(949, 483, 1361);
            AddImage(133, 484, 1361);
            AddLabel(489, 84, 0, "Ferramenta de Escultor");
            AddImage(321, 106, 464);

            AddToggleButton(201, 132, m_Mode == SculptorMenuMode.Platforms, 1);
            AddLabel(235, 135, 0, "Plataformas");
            AddToggleButton(405, 133, m_Mode == SculptorMenuMode.Sculptures, 2);
            AddLabel(437, 136, 0, "Esculturas");

            AddImageTiled(186, 157, 768, 12, 634);
            AddImageTiled(625, 178, 13, 311, 635);
            AddImageTiled(384, 179, 13, 309, 635);
            AddImageTiled(653, 183, 297, 182, 391);
            AddImageTiled(654, 382, 297, 99, 391);
            AddImageTiled(184, 362, 199, 12, 634);

            DrawMaterialPanel();
            DrawPreview();
            DrawPageButtons();
            DrawChanceLabel();
            AddButton(860, 107, 495, 495, 9000, GumpButtonType.Reply, 0);
        }

        private void DrawMaterialPanel()
        {
            StatueMaterialOption[] mats = StatueMaterialOptions.All;
            int[] xs = new int[] { 186, 186, 187, 187, 295, 295, 296 };
            int[] labelXs = new int[] { 207, 207, 208, 208, 316, 316, 317 };
            int[] ys = new int[] { 388, 417, 443, 472, 390, 419, 445 };

            for (int i = 0; i < mats.Length && i < xs.Length; i++)
            {
                bool selected = mats[i].MaterialId == m_MaterialId;
                AddToggleButton(xs[i], ys[i], selected, 3000 + i);
                AddLabel(labelXs[i], ys[i] - 1, 0, mats[i].Name);
            }
        }

        private void DrawPreview()
        {
            if (m_Mode == SculptorMenuMode.Sculptures && m_SelectedSculptureCategory == StatueCraftCategory.LiveModel && m_SelectedLiveModel != null && !m_SelectedLiveModel.Deleted)
            {
                string name = m_SelectedLiveModel.Name;
                if (string.IsNullOrEmpty(name))
                    name = m_SelectedLiveModel.GetType().Name;
                AddLabel(667, 194, 1152, name);
            }
            else
            {
                AddItem(732, 190, GetPreviewItemID(), StatueMaterialOptions.GetHue(m_MaterialId));
            }

            DrawRequirements();
        }

        private void DrawRequirements()
        {
            List<SculptorRequirement> reqs = GetCurrentRequirements();
            int y = 389;
            for (int i = 0; i < reqs.Count && i < 4; i++)
            {
                AddLabel(661, y, 1152, reqs[i].DisplayName);
                AddLabel(910, y, 1152, reqs[i].Amount.ToString());
                y += 22;
            }
        }

        private void DrawChanceLabel()
        {
            AddLabel(861, 343, 1152, "Chance: " + GetCurrentSuccessChance().ToString() + "%");
        }

        private void DrawPageButtons()
        {
            int totalPages = GetCurrentPageCount();
            int currentPage = GetCurrentPageIndex() + 1;
            AddLabel(499, 472, 0, currentPage.ToString() + "/" + totalPages.ToString());

            if (totalPages > 1 && GetCurrentPageIndex() > 0)
                AddButton(404, 460, 498, 498, 8001, GumpButtonType.Reply, 0);

            if (totalPages > 1 && GetCurrentPageIndex() < totalPages - 1)
                AddButton(567, 460, 499, 499, 8002, GumpButtonType.Reply, 0);
        }

        private void DrawPlatformsPage()
        {
            AddLabel(218, 184, 0, "Pequena");
            AddToggleButton(193, 186, m_SelectedPlatformSize == StatuePlatformSize.Small, 1001);
            AddLabel(219, 217, 0, "Média");
            AddToggleButton(193, 219, m_SelectedPlatformSize == StatuePlatformSize.Medium, 1002);
            AddLabel(219, 251, 0, "Grande");
            AddToggleButton(193, 253, m_SelectedPlatformSize == StatuePlatformSize.Large, 1003);
            AddLabel(219, 285, 0, "Gigante");
            AddToggleButton(193, 287, m_SelectedPlatformSize == StatuePlatformSize.Giant, 1004);
            AddLabel(218, 317, 0, "XXL");
            AddToggleButton(192, 319, m_SelectedPlatformSize == StatuePlatformSize.XXL, 1005);

            AddLabel(676, 136, 0, "Adicionar Placa");
            AddButton(645, 133, 538, 538, 2100, GumpButtonType.Reply, 0);

            List<IPlatformRecipeProvider> list = GetPlatformRecipesForSize(m_SelectedPlatformSize);
            int start = m_PlatformPage * 8;
            int end = Math.Min(start + 8, list.Count);
            int y = 184;
            for (int i = start; i < end; i++)
            {
                int globalIndex = GetGlobalPlatformIndex(list[i]);
                bool selected = (globalIndex == m_SelectedPlatformRecipeIndex) || (m_SelectedPlatformRecipeIndex < 0 && i == start);
                AddLabel(442, y, 0, list[i].RecipeName);
                AddToggleButton(417, y + 2, selected, 2000 + globalIndex);
                y += 29;
            }
        }

        private void DrawSculpturesPage()
        {
            AddLabel(218, 184, 0, "Esculturas Pequenas");
            AddToggleButton(193, 186, m_SelectedSculptureCategory == StatueCraftCategory.Small, 4001);

            AddLabel(218, 216, 0, "Esculturas Grandes");
            AddToggleButton(193, 219, m_SelectedSculptureCategory == StatueCraftCategory.Large, 4002);

            if (StatueCraftSystem.HasSculptorIIAbility(m_From))
            {
                AddLabel(219, 251, 0, "Modelo Vivo");
                AddToggleButton(193, 253, m_SelectedSculptureCategory == StatueCraftCategory.LiveModel, 4003);
            }

            if (m_SelectedSculptureCategory == StatueCraftCategory.LiveModel)
                return;

            List<ISculptureRecipeProvider> list = GetSculptureRecipesForCategory(m_SelectedSculptureCategory);
            int start = m_SculpturePage * 8;
            int end = Math.Min(start + 8, list.Count);
            int y = 184;
            for (int i = start; i < end; i++)
            {
                int globalIndex = GetGlobalSculptureIndex(list[i]);
                bool selected = (globalIndex == m_SelectedSculptureRecipeIndex) || (m_SelectedSculptureRecipeIndex < 0 && i == start);
                AddLabel(443, y, 0, list[i].RecipeName);
                AddToggleButton(418, y + 2, selected, 5000 + globalIndex);
                y += 32;
            }
        }

        private void AddToggleButton(int x, int y, bool selected, int buttonId)
        {
            int art = selected ? 433 : 454;
            AddButton(x, y, art, art, buttonId, GumpButtonType.Reply, 0);
        }

        private int GetPreviewItemID()
        {
            if (m_Mode == SculptorMenuMode.Platforms)
            {
                IPlatformRecipeProvider recipe = GetSelectedPlatformRecipe();
                if (recipe != null)
                    return recipe.ItemID;

                List<IPlatformRecipeProvider> list = GetPlatformRecipesForSize(m_SelectedPlatformSize);
                return list.Count > 0 ? list[0].ItemID : 4645;
            }

            if (m_SelectedSculptureCategory == StatueCraftCategory.LiveModel)
                return 4645;

            ISculptureRecipeProvider sculpture = GetSelectedSculptureRecipe();
            if (sculpture != null)
                return sculpture.ItemID;

            List<ISculptureRecipeProvider> sculptures = GetSculptureRecipesForCategory(m_SelectedSculptureCategory);
            return sculptures.Count > 0 ? sculptures[0].ItemID : 4645;
        }

        private int GetCurrentSuccessChance()
        {
            if (m_Mode == SculptorMenuMode.Platforms)
            {
                IPlatformRecipeProvider recipe = GetSelectedPlatformRecipe();
                if (recipe == null)
                {
                    List<IPlatformRecipeProvider> list = GetPlatformRecipesForSize(m_SelectedPlatformSize);
                    if (list.Count > 0)
                        recipe = list[0];
                }
                return SculptorDef.GetPlatformSuccessChance(recipe, m_MaterialId);
            }

            if (m_SelectedSculptureCategory == StatueCraftCategory.LiveModel)
            {
                if (m_SelectedLiveModel != null && !m_SelectedLiveModel.Deleted)
                {
                    StatueMobileProfile profile = StatueMobileProfileReader.GetFrom(m_SelectedLiveModel);
                    return SculptorDef.GetLiveModelSuccessChance(profile);
                }
                return 80;
            }

            ISculptureRecipeProvider sculpture = GetSelectedSculptureRecipe();
            if (sculpture == null)
            {
                List<ISculptureRecipeProvider> list = GetSculptureRecipesForCategory(m_SelectedSculptureCategory);
                if (list.Count > 0)
                    sculpture = list[0];
            }
            return SculptorDef.GetSculptureSuccessChance(sculpture, m_MaterialId);
        }

        private List<SculptorRequirement> GetCurrentRequirements()
        {
            if (m_Mode == SculptorMenuMode.Platforms)
            {
                IPlatformRecipeProvider recipe = GetSelectedPlatformRecipe();
                if (recipe == null)
                {
                    List<IPlatformRecipeProvider> list = GetPlatformRecipesForSize(m_SelectedPlatformSize);
                    if (list.Count > 0)
                        recipe = list[0];
                }
                return SculptorDef.BuildRequirementList(recipe, m_MaterialId);
            }

            if (m_SelectedSculptureCategory == StatueCraftCategory.LiveModel)
            {
                if (m_SelectedLiveModel != null && !m_SelectedLiveModel.Deleted)
                {
                    StatueMobileProfile profile = StatueMobileProfileReader.GetFrom(m_SelectedLiveModel);
                    bool mountedDouble = (m_SelectedLiveModel != null && m_SelectedLiveModel.Mounted);
                    return SculptorDef.BuildRequirementList(profile, m_MaterialId, mountedDouble);
                }
                return new List<SculptorRequirement>();
            }

            ISculptureRecipeProvider sculpture = GetSelectedSculptureRecipe();
            if (sculpture == null)
            {
                List<ISculptureRecipeProvider> list = GetSculptureRecipesForCategory(m_SelectedSculptureCategory);
                if (list.Count > 0)
                    sculpture = list[0];
            }
            return SculptorDef.BuildRequirementList(sculpture, m_MaterialId);
        }

        private int GetCurrentPageCount()
        {
            if (m_Mode == SculptorMenuMode.Platforms)
            {
                int count = GetPlatformRecipesForSize(m_SelectedPlatformSize).Count;
                return Math.Max(1, (count + 7) / 8);
            }

            if (m_SelectedSculptureCategory == StatueCraftCategory.LiveModel)
                return 1;

            int scount = GetSculptureRecipesForCategory(m_SelectedSculptureCategory).Count;
            return Math.Max(1, (scount + 7) / 8);
        }

        private int GetCurrentPageIndex()
        {
            return m_Mode == SculptorMenuMode.Platforms ? m_PlatformPage : m_SculpturePage;
        }

        private List<IPlatformRecipeProvider> GetPlatformRecipesForSize(StatuePlatformSize size)
        {
            List<IPlatformRecipeProvider> list = new List<IPlatformRecipeProvider>();
            for (int i = 0; i < StatueRecipeRegistry.Platforms.Count; i++)
            {
                if (StatueRecipeRegistry.Platforms[i].PlatformSize == size)
                    list.Add(StatueRecipeRegistry.Platforms[i]);
            }
            return list;
        }

        private List<ISculptureRecipeProvider> GetSculptureRecipesForCategory(StatueCraftCategory cat)
        {
            List<ISculptureRecipeProvider> list = new List<ISculptureRecipeProvider>();
            for (int i = 0; i < StatueRecipeRegistry.Sculptures.Count; i++)
            {
                ISculptureRecipeProvider recipe = StatueRecipeRegistry.Sculptures[i];

                if (recipe.Category != cat)
                    continue;

                if (!SculptingLearnedRecipes.IsRecipeVisible(m_From, recipe))
                    continue;

                list.Add(recipe);
            }
            return list;
        }

        private int GetGlobalPlatformIndex(IPlatformRecipeProvider recipe)
        {
            for (int i = 0; i < StatueRecipeRegistry.Platforms.Count; i++)
            {
                if (object.ReferenceEquals(StatueRecipeRegistry.Platforms[i], recipe))
                    return i;
                if (StatueRecipeRegistry.Platforms[i].RecipeName == recipe.RecipeName && StatueRecipeRegistry.Platforms[i].PlatformSize == recipe.PlatformSize && StatueRecipeRegistry.Platforms[i].ItemID == recipe.ItemID)
                    return i;
            }
            return -1;
        }

        private int GetGlobalSculptureIndex(ISculptureRecipeProvider recipe)
        {
            for (int i = 0; i < StatueRecipeRegistry.Sculptures.Count; i++)
            {
                if (object.ReferenceEquals(StatueRecipeRegistry.Sculptures[i], recipe))
                    return i;
                if (StatueRecipeRegistry.Sculptures[i].RecipeName == recipe.RecipeName && StatueRecipeRegistry.Sculptures[i].Category == recipe.Category && StatueRecipeRegistry.Sculptures[i].ItemID == recipe.ItemID)
                    return i;
            }
            return -1;
        }

        private IPlatformRecipeProvider GetSelectedPlatformRecipe()
        {
            if (m_SelectedPlatformRecipeIndex >= 0 && m_SelectedPlatformRecipeIndex < StatueRecipeRegistry.Platforms.Count)
                return StatueRecipeRegistry.Platforms[m_SelectedPlatformRecipeIndex];
            return null;
        }

        private ISculptureRecipeProvider GetSelectedSculptureRecipe()
        {
            if (m_SelectedSculptureRecipeIndex >= 0 && m_SelectedSculptureRecipeIndex < StatueRecipeRegistry.Sculptures.Count)
                return StatueRecipeRegistry.Sculptures[m_SelectedSculptureRecipeIndex];
            return null;
        }

        private void Reopen(PlayerMobile from, SculptorMenuMode mode, int materialId, StatuePlatformSize platformSize, int platformRecipeIndex, int platformPage, StatueCraftCategory sculptureCategory, int sculptureRecipeIndex, int sculpturePage, GenericSign sign, Mobile liveModel)
        {
            from.CloseGump(typeof(SculptorMainGump));
            from.SendGump(new SculptorMainGump(from, m_Tool, mode, materialId, platformSize, platformRecipeIndex, platformPage, sculptureCategory, sculptureRecipeIndex, sculpturePage, sign, liveModel));
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            PlayerMobile from = state.Mobile as PlayerMobile;
            if (from == null || m_Tool == null || m_Tool.Deleted)
                return;
            if (info.ButtonID == 0)
                return;

            if (info.ButtonID == 1)
            {
                Reopen(from, SculptorMenuMode.Platforms, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID == 2)
            {
                Reopen(from, SculptorMenuMode.Sculptures, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID >= 3000 && info.ButtonID < 3000 + StatueMaterialOptions.All.Length)
            {
                int idx = info.ButtonID - 3000;
                StatueMaterialOption[] mats = StatueMaterialOptions.All;
                if (idx >= 0 && idx < mats.Length)
                    Reopen(from, m_Mode, mats[idx].MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID == 8001)
            {
                if (m_Mode == SculptorMenuMode.Platforms)
                    Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, Math.Max(0, m_PlatformPage - 1), m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                else
                    Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, Math.Max(0, m_SculpturePage - 1), m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID == 8002)
            {
                if (m_Mode == SculptorMenuMode.Platforms)
                    Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, Math.Min(GetCurrentPageCount() - 1, m_PlatformPage + 1), m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                else
                    Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, Math.Min(GetCurrentPageCount() - 1, m_SculpturePage + 1), m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID >= 1001 && info.ButtonID <= 1005)
            {
                StatuePlatformSize size = StatuePlatformSize.Small;
                if (info.ButtonID == 1002) size = StatuePlatformSize.Medium;
                else if (info.ButtonID == 1003) size = StatuePlatformSize.Large;
                else if (info.ButtonID == 1004) size = StatuePlatformSize.Giant;
                else if (info.ButtonID == 1005) size = StatuePlatformSize.XXL;
                Reopen(from, SculptorMenuMode.Platforms, m_MaterialId, size, -1, 0, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID >= 2000 && info.ButtonID < 3000)
            {
                int index = info.ButtonID - 2000;
                Reopen(from, SculptorMenuMode.Platforms, m_MaterialId, m_SelectedPlatformSize, index, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID == 2100)
            {
                from.SendMessage("Escolha uma placa pronta na sua mochila.");
                from.Target = new SignSelectionTarget(this);
                return;
            }

            if (info.ButtonID == 4001)
            {
                Reopen(from, SculptorMenuMode.Sculptures, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, StatueCraftCategory.Small, -1, 0, m_SelectedSign, null);
                return;
            }

            if (info.ButtonID == 4002)
            {
                Reopen(from, SculptorMenuMode.Sculptures, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, StatueCraftCategory.Large, -1, 0, m_SelectedSign, null);
                return;
            }

            if (info.ButtonID == 4003)
            {
                if (!StatueCraftSystem.HasSculptorIIAbility(from))
                {
                    from.SendMessage("Você precisa da habilidade Esculpir II para esculpir a partir de modelos vivos.");
                    Reopen(from, SculptorMenuMode.Sculptures, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, StatueCraftCategory.Small, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, null);
                    return;
                }

                if (!StatueCraftSystem.HasSculptingHeight(from))
                {
                    from.CloseGump(typeof(SculptorMainGump));
                    from.SendMessage("Você precisa estar mais alto pra escupir uma escultura desse porte.");
                    return;
                }

                from.SendMessage("Escolha o modelo vivo.");
                from.Target = new LiveModelSelectionTarget(this);
                return;
            }

            if (info.ButtonID >= 5000 && info.ButtonID < 6000)
            {
                int index = info.ButtonID - 5000;
                Reopen(from, SculptorMenuMode.Sculptures, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, index, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                return;
            }

            if (info.ButtonID == 9000)
            {
                if (m_Mode == SculptorMenuMode.Platforms)
                {
                    IPlatformRecipeProvider recipe = GetSelectedPlatformRecipe();
                    if (recipe == null)
                    {
                        List<IPlatformRecipeProvider> list = GetPlatformRecipesForSize(m_SelectedPlatformSize);
                        if (list.Count > 0)
                            recipe = list[0];
                    }

                    if (recipe == null)
                    {
                        from.SendMessage("Escolha um tipo de plataforma.");
                        Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                        return;
                    }

                    bool withSign = m_SelectedSign != null && !m_SelectedSign.Deleted && from.Backpack != null && m_SelectedSign.IsChildOf(from.Backpack);
                    SculptorActions.BeginPlatform(from, m_Tool, recipe, m_MaterialId, withSign);
                    return;
                }

                if (m_SelectedSculptureCategory == StatueCraftCategory.LiveModel)
                {
                    if (m_SelectedLiveModel == null || m_SelectedLiveModel.Deleted)
                    {
                        from.SendMessage("Escolha primeiro o modelo vivo.");
                        Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                        return;
                    }

                    from.SendMessage("Agora escolha a plataforma pronta onde a estátua será colocada.");
                    from.Target = new LiveModelPlacementTarget(m_Tool, m_MaterialId, m_SelectedLiveModel);
                    return;
                }

                ISculptureRecipeProvider recipeS = GetSelectedSculptureRecipe();
                if (recipeS == null)
                {
                    List<ISculptureRecipeProvider> listS = GetSculptureRecipesForCategory(m_SelectedSculptureCategory);
                    if (listS.Count > 0)
                        recipeS = listS[0];
                }

                if (recipeS == null)
                {
                    from.SendMessage("Escolha uma escultura.");
                    Reopen(from, m_Mode, m_MaterialId, m_SelectedPlatformSize, m_SelectedPlatformRecipeIndex, m_PlatformPage, m_SelectedSculptureCategory, m_SelectedSculptureRecipeIndex, m_SculpturePage, m_SelectedSign, m_SelectedLiveModel);
                    return;
                }

                if (m_SelectedSculptureCategory == StatueCraftCategory.Small)
                    SculptorActions.BeginSmall(from, m_Tool, recipeS, m_MaterialId);
                else if (m_SelectedSculptureCategory == StatueCraftCategory.Large)
                    SculptorActions.BeginLarge(from, m_Tool, recipeS, m_MaterialId);
            }
        }

        private class SignSelectionTarget : Target
        {
            private readonly SculptorMainGump m_Gump;

            public SignSelectionTarget(SculptorMainGump gump) : base(12, false, TargetFlags.None)
            {
                m_Gump = gump;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (pm == null || m_Gump == null)
                    return;

                GenericSign sign = targeted as GenericSign;
                if (sign == null || sign.Deleted)
                {
                    pm.SendMessage("Isso não é uma placa válida.");
                    m_Gump.Reopen(pm, m_Gump.m_Mode, m_Gump.m_MaterialId, m_Gump.m_SelectedPlatformSize, m_Gump.m_SelectedPlatformRecipeIndex, m_Gump.m_PlatformPage, m_Gump.m_SelectedSculptureCategory, m_Gump.m_SelectedSculptureRecipeIndex, m_Gump.m_SculpturePage, m_Gump.m_SelectedSign, m_Gump.m_SelectedLiveModel);
                    return;
                }

                if (pm.Backpack == null || !sign.IsChildOf(pm.Backpack))
                {
                    pm.SendMessage("A placa precisa estar na sua mochila.");
                    m_Gump.Reopen(pm, m_Gump.m_Mode, m_Gump.m_MaterialId, m_Gump.m_SelectedPlatformSize, m_Gump.m_SelectedPlatformRecipeIndex, m_Gump.m_PlatformPage, m_Gump.m_SelectedSculptureCategory, m_Gump.m_SelectedSculptureRecipeIndex, m_Gump.m_SculpturePage, m_Gump.m_SelectedSign, m_Gump.m_SelectedLiveModel);
                    return;
                }

                pm.SendMessage("Placa selecionada.");
                m_Gump.Reopen(pm, m_Gump.m_Mode, m_Gump.m_MaterialId, m_Gump.m_SelectedPlatformSize, m_Gump.m_SelectedPlatformRecipeIndex, m_Gump.m_PlatformPage, m_Gump.m_SelectedSculptureCategory, m_Gump.m_SelectedSculptureRecipeIndex, m_Gump.m_SculpturePage, sign, m_Gump.m_SelectedLiveModel);
            }
        }

        private class LiveModelSelectionTarget : Target
        {
            private readonly SculptorMainGump m_Gump;

            public LiveModelSelectionTarget(SculptorMainGump gump) : base(7, false, TargetFlags.None)
            {
                m_Gump = gump;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                Mobile model = targeted as Mobile;
                StatueMobileProfile profile;
                string message;

                if (pm == null || m_Gump == null)
                    return;

                if (model == null)
                {
                    pm.SendMessage("Esse alvo não é um modelo vivo válido.");
                    m_Gump.Reopen(pm, SculptorMenuMode.Sculptures, m_Gump.m_MaterialId, m_Gump.m_SelectedPlatformSize, m_Gump.m_SelectedPlatformRecipeIndex, m_Gump.m_PlatformPage, StatueCraftCategory.LiveModel, -1, 0, m_Gump.m_SelectedSign, null);
                    return;
                }

                if (!SculptorActions.TryGetLiveModelData(pm, model, out profile, out message))
                {
                    pm.SendMessage(message ?? "Não é possível usar esse modelo vivo.");
                    pm.CloseGump(typeof(SculptorMainGump));
                    return;
                }

                m_Gump.Reopen(pm, SculptorMenuMode.Sculptures, m_Gump.m_MaterialId, m_Gump.m_SelectedPlatformSize, m_Gump.m_SelectedPlatformRecipeIndex, m_Gump.m_PlatformPage, StatueCraftCategory.LiveModel, -1, 0, m_Gump.m_SelectedSign, model);
            }
        }

        private class LiveModelPlacementTarget : Target
        {
            private readonly SculptorTools m_Tool;
            private readonly int m_MaterialId;
            private readonly Mobile m_Model;

            public LiveModelPlacementTarget(SculptorTools tool, int materialId, Mobile model) : base(12, false, TargetFlags.None)
            {
                m_Tool = tool;
                m_MaterialId = materialId;
                m_Model = model;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                BaseStatuePlatformItem platform = targeted as BaseStatuePlatformItem;

                if (pm == null)
                    return;

                if (platform == null)
                {
                    pm.SendMessage("Isso não é uma plataforma válida.");
                    return;
                }

                SculptorActions.BeginLiveModelSelected(pm, m_Tool, m_MaterialId, m_Model, platform);
            }
        }
    }
}
