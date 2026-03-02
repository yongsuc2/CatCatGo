using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Services;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class MainScreen : BaseScreen
    {
        private TextMeshProUGUI _hpText;
        private TextMeshProUGUI _atkText;
        private TextMeshProUGUI _defText;
        private TextMeshProUGUI _critText;

        private TextMeshProUGUI _talentGradeText;
        private TextMeshProUGUI _clearedChapterText;
        private TextMeshProUGUI _towerFloorText;

        private TextMeshProUGUI _adventureInfo;
        private TextMeshProUGUI _contentInfo;
        private TextMeshProUGUI _growthInfo;
        private TextMeshProUGUI _gachaInfo;

        private void Awake()
        {
            BuildLayout();
        }

        private void BuildLayout()
        {
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();

            if (scrollRt == null) scrollRt = scrollGo.AddComponent<RectTransform>();
            UIManager.StretchFull(scrollRt);

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();

            if (viewportRt == null) viewportRt = viewportGo.AddComponent<RectTransform>();
            UIManager.StretchFull(viewportRt);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();

            if (contentRt == null) contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 12;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.padding = new RectOffset(16, 16, 16, 16);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            BuildHeader(contentGo.transform);
            BuildPlayerStatsCard(contentGo.transform);
            BuildInfoCard(contentGo.transform);
            BuildMenuGrid(contentGo.transform);
        }

        private void BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 60;

            var headerText = headerGo.AddComponent<TextMeshProUGUI>();
            headerText.text = "카피바라 고!";
            headerText.fontSize = 42f;
            headerText.color = ColorPalette.Gold;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;
        }

        private void BuildPlayerStatsCard(Transform parent)
        {
            var cardGo = new GameObject("PlayerStatsCard");
            cardGo.transform.SetParent(parent, false);
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPalette.Card;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 4;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.padding = new RectOffset(16, 16, 12, 12);

            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 200;

            var headerRow = new GameObject("StatsHeader");
            headerRow.transform.SetParent(cardGo.transform, false);
            var headerLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;
            var headerLe = headerRow.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 36;

            var titleGo = new GameObject("StatsTitle");
            titleGo.transform.SetParent(headerRow.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "전투력";
            titleText.fontSize = 28f;
            titleText.color = ColorPalette.Text;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            var detailBtnGo = new GameObject("DetailButton");
            detailBtnGo.transform.SetParent(headerRow.transform, false);
            var detailBtnLe = detailBtnGo.AddComponent<LayoutElement>();
            detailBtnLe.preferredWidth = 80;
            var detailBtnImage = detailBtnGo.AddComponent<Image>();
            detailBtnImage.color = ColorPalette.ButtonSecondary;
            var detailBtn = detailBtnGo.AddComponent<Button>();
            detailBtn.targetGraphic = detailBtnImage;
            detailBtn.onClick.AddListener(OnStatsDetailClicked);
            var detailBtnText = new GameObject("BtnText");
            detailBtnText.transform.SetParent(detailBtnGo.transform, false);
            var dbt = detailBtnText.AddComponent<TextMeshProUGUI>();
            dbt.text = "상세";
            dbt.fontSize = 22f;
            dbt.color = ColorPalette.Text;
            dbt.alignment = TextAlignmentOptions.Center;
            dbt.raycastTarget = false;
            UIManager.StretchFull(dbt.rectTransform);

            _hpText = CreateStatRow(cardGo.transform, "체력");
            _atkText = CreateStatRow(cardGo.transform, "공격력");
            _defText = CreateStatRow(cardGo.transform, "방어력");
            _critText = CreateStatRow(cardGo.transform, "치명타");
        }

        private TextMeshProUGUI CreateStatRow(Transform parent, string label)
        {
            var rowGo = new GameObject("Row_" + label);
            rowGo.transform.SetParent(parent, false);
            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 36f;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 28f;
            labelText.color = ColorPalette.TextDim;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.raycastTarget = false;

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(rowGo.transform, false);
            var valueText = valueGo.AddComponent<TextMeshProUGUI>();
            valueText.text = "";
            valueText.fontSize = 28f;
            valueText.color = ColorPalette.Text;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.raycastTarget = false;

            return valueText;
        }

        private void BuildInfoCard(Transform parent)
        {
            var cardGo = new GameObject("InfoCard");
            cardGo.transform.SetParent(parent, false);
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPalette.Card;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 4;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.padding = new RectOffset(16, 16, 12, 12);

            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 160;

            _talentGradeText = CreateInfoRow(cardGo.transform, "재능 등급");
            _clearedChapterText = CreateInfoRow(cardGo.transform, "클리어 챕터");
            _towerFloorText = CreateInfoRow(cardGo.transform, "탑 층수");
        }

        private TextMeshProUGUI CreateInfoRow(Transform parent, string label)
        {
            var rowGo = new GameObject("Row_" + label);
            rowGo.transform.SetParent(parent, false);
            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 36f;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 28f;
            labelText.color = ColorPalette.TextDim;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.raycastTarget = false;

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(rowGo.transform, false);
            var valueText = valueGo.AddComponent<TextMeshProUGUI>();
            valueText.text = "";
            valueText.fontSize = 28f;
            valueText.color = ColorPalette.Text;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.raycastTarget = false;

            return valueText;
        }

        private void BuildMenuGrid(Transform parent)
        {
            var gridGo = new GameObject("MenuGrid");
            gridGo.transform.SetParent(parent, false);
            var gridLe = gridGo.AddComponent<LayoutElement>();
            gridLe.preferredHeight = 320;

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(400, 140);
            grid.spacing = new Vector2(12, 12);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.padding = new RectOffset(8, 8, 4, 4);

            _adventureInfo = CreateMenuCard(gridGo.transform, "모험", ScreenType.Chapter);
            _contentInfo = CreateMenuCard(gridGo.transform, "콘텐츠", ScreenType.Content);
            _growthInfo = CreateMenuCard(gridGo.transform, "성장", ScreenType.Talent);
            _gachaInfo = CreateMenuCard(gridGo.transform, "뽑기", ScreenType.Gacha);
        }

        private TextMeshProUGUI CreateMenuCard(Transform parent, string title, ScreenType targetScreen)
        {
            var cardGo = new GameObject("Card_" + title);
            cardGo.transform.SetParent(parent, false);
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPalette.Card;

            var button = cardGo.AddComponent<Button>();
            button.targetGraphic = cardImage;
            button.onClick.AddListener(() => UI.ShowScreen(targetScreen));

            var layout = cardGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(12, 12, 12, 12);

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 36;
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 32f;
            titleText.color = ColorPalette.Text;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

            var infoGo = new GameObject("Info");
            infoGo.transform.SetParent(cardGo.transform, false);
            var infoLe = infoGo.AddComponent<LayoutElement>();
            infoLe.preferredHeight = 28f;
            var infoText = infoGo.AddComponent<TextMeshProUGUI>();
            infoText.text = "";
            infoText.fontSize = 28f;
            infoText.color = ColorPalette.TextDim;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.raycastTarget = false;

            return infoText;
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            var player = Game.Player;
            var stats = player.ComputeStats();
            var talentGrade = player.GetTalentGrade();
            string subGradeLabel = TalentTable.GetSubGradeLabel(player.Talent.GetTotalLevel());
            Color gradeColor = ColorPalette.GetTalentGradeColor(talentGrade);
            string gradeHex = ColorUtility.ToHtmlStringRGB(gradeColor);

            _hpText.text = $"{stats.Hp} / {stats.MaxHp}";
            _atkText.text = NumberFormatter.FormatInt(stats.Atk);
            _defText.text = NumberFormatter.FormatInt(stats.Def);
            _critText.text = $"{stats.Crit:F1}%";

            _talentGradeText.text = $"<color=#{gradeHex}>{subGradeLabel}</color>";
            _clearedChapterText.text = $"{player.ClearedChapterMax}";
            _towerFloorText.text = $"{Game.Tower.CurrentFloor}-{Game.Tower.CurrentStage}";

            _adventureInfo.text = $"스태미나: {(int)player.Resources.Stamina}/{player.Resources.GetStaminaMax()}";
            _contentInfo.text = "탑/던전";
            _growthInfo.text = "재능/유산";
            _gachaInfo.text = $"보석: {NumberFormatter.Format(player.Resources.Gems)}";
        }

        private void OnStatsDetailClicked()
        {
            UI.ShowPopupFromType<StatsDetailPopup>(null);
        }
    }

    public class StatsDetailPopup : BasePopup
    {
        private TextMeshProUGUI _contentText;

        public override void Show(object data = null)
        {
            base.Show(data);
            BuildLayout();
            RefreshContent();
        }

        private void BuildLayout()
        {
            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.padding = new RectOffset(20, 20, 20, 20);

            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(transform, false);
            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 40;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "스탯 상세";
            titleText.fontSize = 32f;
            titleText.color = ColorPalette.Text;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            var closeBtnGo = new GameObject("CloseButton");
            closeBtnGo.transform.SetParent(headerGo.transform, false);
            var closeBtnLe = closeBtnGo.AddComponent<LayoutElement>();
            closeBtnLe.preferredWidth = 40;
            var closeBtnImage = closeBtnGo.AddComponent<Image>();
            closeBtnImage.color = ColorPalette.ButtonSecondary;
            var closeBtn = closeBtnGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImage;
            closeBtn.onClick.AddListener(Hide);
            var closeBtnText = new GameObject("X");
            closeBtnText.transform.SetParent(closeBtnGo.transform, false);
            var cbt = closeBtnText.AddComponent<TextMeshProUGUI>();
            cbt.text = "X";
            cbt.fontSize = 28f;
            cbt.color = ColorPalette.Text;
            cbt.alignment = TextAlignmentOptions.Center;
            cbt.raycastTarget = false;
            UIManager.StretchFull(cbt.rectTransform);

            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();

            if (scrollRt == null) scrollRt = scrollGo.AddComponent<RectTransform>();
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1;

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();

            if (viewportRt == null) viewportRt = viewportGo.AddComponent<RectTransform>();
            UIManager.StretchFull(viewportRt);
            viewportGo.AddComponent<Image>().color = Color.clear;
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();

            if (contentRt == null) contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var contentVlg = contentGo.AddComponent<VerticalLayoutGroup>();
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            contentVlg.padding = new RectOffset(8, 8, 8, 8);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            var textGo = new GameObject("BreakdownText");
            textGo.transform.SetParent(contentGo.transform, false);
            var textLe = textGo.AddComponent<LayoutElement>();
            textLe.flexibleWidth = 1;
            _contentText = textGo.AddComponent<TextMeshProUGUI>();
            _contentText.fontSize = 24f;
            _contentText.color = ColorPalette.Text;
            _contentText.alignment = TextAlignmentOptions.Left;
            _contentText.raycastTarget = false;
            _contentText.textWrappingMode = TextWrappingModes.Normal;
        }

        private void RefreshContent()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Player == null) return;

            var breakdown = gm.Player.GetStatsBreakdown();
            var combatPassives = gm.Player.GetCombatPassives();
            string goldHex = ColorUtility.ToHtmlStringRGB(ColorPalette.Gold);
            string dimHex = ColorUtility.ToHtmlStringRGB(ColorPalette.TextDim);

            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"<color=#{goldHex}>── 기본 스탯 ──</color>");
            sb.AppendLine();

            AppendStatCategory(sb, "최대 체력", breakdown.Total.MaxHp,
                breakdown.Base.MaxHp, breakdown.Talent.MaxHp, breakdown.Grade.MaxHp,
                breakdown.Equipment.MaxHp, breakdown.Heritage.MaxHp, breakdown.Pet.MaxHp, dimHex);

            AppendStatCategory(sb, "공격력", breakdown.Total.Atk,
                breakdown.Base.Atk, breakdown.Talent.Atk, breakdown.Grade.Atk,
                breakdown.Equipment.Atk, breakdown.Heritage.Atk, breakdown.Pet.Atk, dimHex);

            AppendStatCategory(sb, "방어력", breakdown.Total.Def,
                breakdown.Base.Def, breakdown.Talent.Def, breakdown.Grade.Def,
                breakdown.Equipment.Def, breakdown.Heritage.Def, breakdown.Pet.Def, dimHex);

            sb.AppendLine($"치명타 확률          {breakdown.Total.Crit:F1}%");
            AppendSourceFloat(sb, "기본", breakdown.Base.Crit, dimHex);
            AppendSourceFloat(sb, "재능", breakdown.Talent.Crit, dimHex);
            AppendSourceFloat(sb, "등급", breakdown.Grade.Crit, dimHex);
            AppendSourceFloat(sb, "장비", breakdown.Equipment.Crit, dimHex);
            AppendSourceFloat(sb, "유산", breakdown.Heritage.Crit, dimHex);
            AppendSourceFloat(sb, "펫", breakdown.Pet.Crit, dimHex);
            sb.AppendLine();

            sb.AppendLine($"<color=#{goldHex}>── 전투 스탯 ──</color>");
            sb.AppendLine();
            sb.AppendLine($"치명타 데미지       {combatPassives.CritDamage * 100f:F0}%");
            sb.AppendLine($"흡혈률             {combatPassives.LifestealRate * 100f:F1}%");
            sb.AppendLine($"회피율             {combatPassives.EvasionRate * 100f:F1}%");
            sb.AppendLine($"반격 확률          {combatPassives.CounterChance * 100f:F1}%");

            _contentText.text = sb.ToString();
        }

        private void AppendStatCategory(System.Text.StringBuilder sb, string label, int total,
            int baseStat, int talent, int grade, int equipment, int heritage, int pet, string dimHex)
        {
            sb.AppendLine($"{label}          {NumberFormatter.FormatInt(total)}");
            AppendSourceInt(sb, "기본", baseStat, dimHex);
            AppendSourceInt(sb, "재능", talent, dimHex);
            AppendSourceInt(sb, "등급", grade, dimHex);
            AppendSourceInt(sb, "장비", equipment, dimHex);
            AppendSourceInt(sb, "유산", heritage, dimHex);
            AppendSourceInt(sb, "펫", pet, dimHex);
            sb.AppendLine();
        }

        private void AppendSourceInt(System.Text.StringBuilder sb, string sourceName, int value, string dimHex)
        {
            if (value == 0) return;
            sb.AppendLine($"  <color=#{dimHex}>{sourceName}</color>             {NumberFormatter.FormatInt(value)}");
        }

        private void AppendSourceFloat(System.Text.StringBuilder sb, string sourceName, float value, string dimHex)
        {
            if (value == 0f) return;
            sb.AppendLine($"  <color=#{dimHex}>{sourceName}</color>             {value:F1}%");
        }
    }
}
