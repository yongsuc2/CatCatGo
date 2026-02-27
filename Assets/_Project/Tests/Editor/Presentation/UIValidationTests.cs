using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Tests.Presentation
{
    [TestFixture]
    public class UIValidationTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void UIConstants_MinButtonHeight_IsAtLeast120()
        {
            Assert.GreaterOrEqual(UIConstants.MIN_BUTTON_HEIGHT, 120f);
        }

        [Test]
        public void UIConstants_MinBattleControlHeight_IsAtLeast60()
        {
            Assert.GreaterOrEqual(UIConstants.MIN_BATTLE_CONTROL_HEIGHT, 60f);
        }

        [Test]
        public void UIConstants_MinButtonFontSize_IsAtLeast28()
        {
            Assert.GreaterOrEqual(UIConstants.MIN_BUTTON_FONT_SIZE, 28f);
        }

        [Test]
        public void ValidateButtons_ProperButton_NoViolations()
        {
            var btnGo = CreateButton(_root.transform, "GoodButton", 120f, 30f);
            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.AreEqual(0, violations.Count, "120px button should pass");
        }

        [Test]
        public void ValidateButtons_TooShortButton_ReportsHeightViolation()
        {
            CreateButton(_root.transform, "ShortButton", 40f, 30f);
            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.IsTrue(violations.Count > 0, "40px button should fail");

            bool hasHeightViolation = false;
            foreach (var v in violations)
            {
                if (v.Rule == "ButtonHeight")
                {
                    hasHeightViolation = true;
                    Assert.AreEqual(40f, v.Actual);
                    Assert.AreEqual(UIConstants.MIN_BUTTON_HEIGHT, v.Expected);
                }
            }
            Assert.IsTrue(hasHeightViolation, "Should report ButtonHeight violation");
        }

        [Test]
        public void ValidateButtons_SmallFont_ReportsFontViolation()
        {
            CreateButton(_root.transform, "SmallFontButton", 120f, 18f);
            var violations = UIValidator.ValidateButtons(_root.transform);

            bool hasFontViolation = false;
            foreach (var v in violations)
            {
                if (v.Rule == "ButtonFontSize")
                {
                    hasFontViolation = true;
                    Assert.AreEqual(18f, v.Actual);
                }
            }
            Assert.IsTrue(hasFontViolation, "Should report ButtonFontSize violation");
        }

        [Test]
        public void ValidateButtons_BattleControlButton_UsesLowerMinimum()
        {
            CreateButton(_root.transform, "SpeedBtn", 60f, 28f);
            var violations = UIValidator.ValidateButtons(
                _root.transform, UIConstants.MIN_BATTLE_CONTROL_HEIGHT);
            Assert.AreEqual(0, violations.Count, "60px battle control should pass with lower minimum");
        }

        [Test]
        public void ValidateButtons_NestedButtons_AllChecked()
        {
            var parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform, false);

            CreateButton(parent.transform, "Button1", 120f, 30f);
            CreateButton(parent.transform, "Button2", 30f, 22f);
            CreateButton(parent.transform, "Button3", 120f, 30f);

            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.IsTrue(violations.Count >= 1, "Should find at least 1 violation");

            bool foundButton2 = false;
            foreach (var v in violations)
            {
                if (v.Path.Contains("Button2"))
                    foundButton2 = true;
            }
            Assert.IsTrue(foundButton2, "Should detect Button2 as violation");
        }

        [Test]
        public void ValidateButtons_NoButtons_NoViolations()
        {
            var textGo = new GameObject("JustText");
            textGo.transform.SetParent(_root.transform, false);
            textGo.AddComponent<LayoutElement>().preferredHeight = 10f;

            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.AreEqual(0, violations.Count, "Non-button elements should not trigger violations");
        }

        [Test]
        public void ValidateButtons_ButtonWithoutLayoutElement_NoViolation()
        {
            var btnGo = new GameObject("NoLayoutBtn");
            btnGo.transform.SetParent(_root.transform, false);
            btnGo.AddComponent<Image>();
            btnGo.AddComponent<Button>();

            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.AreEqual(0, violations.Count, "Button without LayoutElement should be skipped");
        }

        [Test]
        public void ValidateButtons_ButtonWithoutText_OnlyHeightChecked()
        {
            var btnGo = new GameObject("NoTextBtn");
            btnGo.transform.SetParent(_root.transform, false);
            btnGo.AddComponent<Image>();
            btnGo.AddComponent<Button>();
            btnGo.AddComponent<LayoutElement>().preferredHeight = 40f;

            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.AreEqual(1, violations.Count);
            Assert.AreEqual("ButtonHeight", violations[0].Rule);
        }

        [Test]
        public void ValidateButtons_ExactMinimum_Passes()
        {
            CreateButton(_root.transform, "ExactMin", UIConstants.MIN_BUTTON_HEIGHT, UIConstants.MIN_BUTTON_FONT_SIZE);
            var violations = UIValidator.ValidateButtons(_root.transform);
            Assert.AreEqual(0, violations.Count, "Exact minimum should pass");
        }

        [Test]
        public void ValidateButtons_MultipleViolationTypes_AllReported()
        {
            CreateButton(_root.transform, "BadButton", 30f, 14f);
            var violations = UIValidator.ValidateButtons(_root.transform);

            bool hasHeight = false;
            bool hasFont = false;
            foreach (var v in violations)
            {
                if (v.Rule == "ButtonHeight") hasHeight = true;
                if (v.Rule == "ButtonFontSize") hasFont = true;
            }
            Assert.IsTrue(hasHeight && hasFont, "Should report both height and font violations");
        }

        [Test]
        public void UIViolation_ToString_ContainsAllInfo()
        {
            var violation = new UIViolation
            {
                Path = "Root/Panel/Button",
                Rule = "ButtonHeight",
                Actual = 40f,
                Expected = 120f,
            };
            string str = violation.ToString();
            Assert.IsTrue(str.Contains("ButtonHeight"));
            Assert.IsTrue(str.Contains("Root/Panel/Button"));
            Assert.IsTrue(str.Contains("40"));
            Assert.IsTrue(str.Contains("120"));
        }

        private GameObject CreateButton(Transform parent, string name, float height, float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>();
            go.AddComponent<Button>();
            go.AddComponent<LayoutElement>().preferredHeight = height;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;

            return go;
        }
    }
}
