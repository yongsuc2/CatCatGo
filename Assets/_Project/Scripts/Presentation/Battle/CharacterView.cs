using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Enums;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Battle
{
    public class CharacterView : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Image _spriteImage;
        private Slider _hpBar;
        private Image _hpFill;
        private Slider _rageBar;
        private Image _rageFill;
        private RectTransform _shieldOverlay;
        private Image _shieldImage;
        private TextMeshProUGUI _nameLabel;
        private RectTransform _statusContainer;
        private TextMeshProUGUI _hpText;

        private Vector2 _originalPosition;
        private Coroutine _phaseCoroutine;
        private Coroutine _animCoroutine;
        private Coroutine _idleCoroutine;
        private RectTransform _spriteRt;

        private Sprite[] _walkFrames;
        private Sprite[] _attackFrames;
        private bool _useFrames;
        private const float FRAME_INTERVAL = 0.15f;

        private const float BOB_AMPLITUDE = 4f;
        private const float BOB_SPEED = 2.5f;
        private const float BREATHE_MIN = 0.97f;
        private const float BREATHE_MAX = 1.03f;
        private const float BREATHE_SPEED = 2f;
        private const float APPROACH_SCALE = 1.15f;
        private const float HIT_SQUASH_X = 1.2f;
        private const float HIT_SQUASH_Y = 0.85f;
        private const float HIT_SHAKE_INTENSITY = 8f;

        private readonly List<StatusEffectIconView> _statusIcons = new List<StatusEffectIconView>();
        private int _maxHp;
        private bool _hasRage;

        public Vector2 OriginalPosition => _originalPosition;

        public void Setup(string name, int maxHp, int maxRage, bool isBoss, Color placeholderColor)
        {
            _maxHp = maxHp;
            _hasRage = maxRage > 0;

            StopFrameAnimation();
            _walkFrames = null;
            _attackFrames = null;
            _useFrames = false;

            if (_rectTransform == null)
                BuildUI(isBoss, placeholderColor);
            else
            {
                StopIdleAnimation();
                float charSize = isBoss ? 150f : 108f;
                _rectTransform.sizeDelta = new Vector2(charSize, charSize + 50f);
                _spriteRt = _spriteImage.GetComponent<RectTransform>();
                _spriteRt.anchoredPosition = new Vector2(0f, 20f);
                _spriteRt.sizeDelta = new Vector2(charSize, charSize);
                _spriteRt.localScale = Vector3.one;
                _spriteImage.preserveAspect = false;
                _spriteImage.sprite = PlaceholderGenerator.CreateRect((int)charSize, (int)charSize, placeholderColor, "C");
            }

            _nameLabel.text = name;
            _hpBar.maxValue = maxHp;
            _hpBar.value = maxHp;
            UpdateHpText(maxHp, maxHp);

            if (_hasRage && _rageBar != null)
            {
                _rageBar.maxValue = maxRage;
                _rageBar.value = 0;
                _rageBar.gameObject.SetActive(true);
            }
            else if (_rageBar != null)
            {
                _rageBar.gameObject.SetActive(false);
            }

            _shieldOverlay.gameObject.SetActive(false);
            ClearStatusEffects();
            StartIdleAnimation();
        }

        public void SetFrames(Sprite[] walkFrames, Sprite[] attackFrames)
        {
            _walkFrames = walkFrames;
            _attackFrames = attackFrames;
            _useFrames = walkFrames != null && walkFrames.Length > 0;

            if (!_useFrames || _spriteImage == null) return;

            _spriteImage.preserveAspect = true;
            _spriteImage.sprite = walkFrames[0];

            _spriteRt = _spriteImage.GetComponent<RectTransform>();
            _spriteRt.sizeDelta = new Vector2(135f, 270f);
            _spriteRt.anchoredPosition = new Vector2(0f, 80f);

            _rectTransform.sizeDelta = new Vector2(135f, 340f);

            StartFrameAnimation(_walkFrames);
            StartIdleAnimation();
        }

        public void UpdateHp(int current, int max)
        {
            _maxHp = max;
            _hpBar.maxValue = max;
            _hpBar.value = Mathf.Max(0, current);

            float ratio = max > 0 ? (float)current / max : 0f;
            if (ratio > 0.5f)
                _hpFill.color = ColorPalette.Heal;
            else if (ratio > 0.2f)
                _hpFill.color = ColorPalette.Gold;
            else
                _hpFill.color = ColorPalette.Hp;

            UpdateHpText(current, max);
        }

        public void UpdateRage(int current, int max)
        {
            if (!_hasRage || _rageBar == null) return;
            _rageBar.maxValue = max;
            _rageBar.value = Mathf.Max(0, current);
        }

        public void UpdateShield(int shield, int maxHp)
        {
            if (shield <= 0)
            {
                _shieldOverlay.gameObject.SetActive(false);
                return;
            }

            _shieldOverlay.gameObject.SetActive(true);
            float ratio = maxHp > 0 ? Mathf.Clamp01((float)shield / maxHp) : 0f;
            _shieldOverlay.anchorMin = new Vector2(0f, 0f);
            _shieldOverlay.anchorMax = new Vector2(ratio, 1f);
            _shieldOverlay.offsetMin = Vector2.zero;
            _shieldOverlay.offsetMax = Vector2.zero;
        }

        public void UpdateStatusEffects(List<StatusEffect> effects)
        {
            ClearStatusEffects();

            var grouped = new Dictionary<StatusEffectType, (int count, int maxTurns)>();
            foreach (var eff in effects)
            {
                if (grouped.TryGetValue(eff.Type, out var existing))
                    grouped[eff.Type] = (existing.count + 1, Mathf.Max(existing.maxTurns, eff.RemainingTurns));
                else
                    grouped[eff.Type] = (1, eff.RemainingTurns);
            }

            foreach (var kvp in grouped)
            {
                var iconGo = new GameObject($"Status_{kvp.Key}");
                iconGo.transform.SetParent(_statusContainer, false);
                var icon = iconGo.AddComponent<StatusEffectIconView>();
                icon.Setup(kvp.Key, kvp.Value.count, kvp.Value.maxTurns);
                _statusIcons.Add(icon);
            }
        }

        public void SetPhase(AttackPhase phase, float duration, Vector2 targetPosition)
        {
            if (_phaseCoroutine != null)
                StopCoroutine(_phaseCoroutine);

            if (_useFrames)
            {
                if (phase == AttackPhase.Hit)
                    StartFrameAnimation(_attackFrames);
                else
                    StartFrameAnimation(_walkFrames);
            }

            switch (phase)
            {
                case AttackPhase.Approach:
                    StopIdleAnimation();
                    _phaseCoroutine = StartCoroutine(ApproachTo(targetPosition, duration));
                    break;
                case AttackPhase.Hit:
                    _phaseCoroutine = StartCoroutine(HitImpact(duration));
                    break;
                case AttackPhase.Retreat:
                    _phaseCoroutine = StartCoroutine(RetreatTo(_originalPosition, duration));
                    break;
                case AttackPhase.Idle:
                    _rectTransform.anchoredPosition = _originalPosition;
                    ResetSpriteTransform();
                    StartIdleAnimation();
                    break;
            }
        }

        public void SetOriginalPosition(Vector2 pos)
        {
            _originalPosition = pos;
            if (_rectTransform != null)
                _rectTransform.anchoredPosition = pos;
        }

        public void ResetToOriginalPosition()
        {
            if (_phaseCoroutine != null)
            {
                StopCoroutine(_phaseCoroutine);
                _phaseCoroutine = null;
            }
            StopFrameAnimation();
            StopIdleAnimation();
            if (_useFrames && _walkFrames != null && _walkFrames.Length > 0)
                _spriteImage.sprite = _walkFrames[0];
            if (_rectTransform != null)
                _rectTransform.anchoredPosition = _originalPosition;
            ResetSpriteTransform();
        }

        private IEnumerator ApproachTo(Vector2 target, float duration)
        {
            Vector2 start = _rectTransform.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float eased = t * t;
                _rectTransform.anchoredPosition = Vector2.Lerp(start, target, eased);

                if (_spriteRt != null)
                {
                    float scale = Mathf.Lerp(1f, APPROACH_SCALE, eased);
                    _spriteRt.localScale = new Vector3(scale, scale, 1f);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            _rectTransform.anchoredPosition = target;
            _phaseCoroutine = null;
        }

        private IEnumerator HitImpact(float duration)
        {
            Vector2 origin = _rectTransform.anchoredPosition;
            float elapsed = 0f;
            Color originalColor = _spriteImage != null ? _spriteImage.color : Color.white;

            if (_spriteRt != null)
                _spriteRt.localScale = new Vector3(HIT_SQUASH_X, HIT_SQUASH_Y, 1f);

            if (_spriteImage != null)
                _spriteImage.color = Color.white;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float decay = 1f - t;
                float intensity = HIT_SHAKE_INTENSITY * decay;
                float offsetX = Random.Range(-intensity, intensity);
                float offsetY = Random.Range(-intensity, intensity);
                _rectTransform.anchoredPosition = origin + new Vector2(offsetX, offsetY);

                if (_spriteRt != null)
                {
                    float sx = Mathf.Lerp(1f, HIT_SQUASH_X, decay * 0.5f);
                    float sy = Mathf.Lerp(1f, HIT_SQUASH_Y, decay * 0.5f);
                    _spriteRt.localScale = new Vector3(sx, sy, 1f);
                }

                if (_spriteImage != null)
                    _spriteImage.color = Color.Lerp(originalColor, Color.white, decay * 0.6f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            _rectTransform.anchoredPosition = origin;
            if (_spriteImage != null)
                _spriteImage.color = originalColor;
            ResetSpriteTransform();
            _phaseCoroutine = null;
        }

        private IEnumerator RetreatTo(Vector2 target, float duration)
        {
            Vector2 start = _rectTransform.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float eased = 1f - (1f - t) * (1f - t);
                _rectTransform.anchoredPosition = Vector2.Lerp(start, target, eased);

                if (_spriteRt != null)
                {
                    float scale = Mathf.Lerp(APPROACH_SCALE, 1f, eased);
                    _spriteRt.localScale = new Vector3(scale, scale, 1f);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            _rectTransform.anchoredPosition = target;
            ResetSpriteTransform();
            _phaseCoroutine = null;
        }

        private IEnumerator IdleBobAndBreathe()
        {
            float time = Random.Range(0f, Mathf.PI * 2f);
            Vector2 baseSpritePos = _spriteRt != null ? _spriteRt.anchoredPosition : Vector2.zero;

            while (true)
            {
                time += Time.deltaTime;

                if (_spriteRt != null)
                {
                    float bobY = Mathf.Sin(time * BOB_SPEED) * BOB_AMPLITUDE;
                    _spriteRt.anchoredPosition = baseSpritePos + new Vector2(0f, bobY);

                    float breathe = Mathf.Lerp(BREATHE_MIN, BREATHE_MAX, (Mathf.Sin(time * BREATHE_SPEED) + 1f) * 0.5f);
                    _spriteRt.localScale = new Vector3(breathe, breathe, 1f);
                }

                yield return null;
            }
        }

        private void StartIdleAnimation()
        {
            StopIdleAnimation();
            if (_spriteRt != null)
                _idleCoroutine = StartCoroutine(IdleBobAndBreathe());
        }

        private void StopIdleAnimation()
        {
            if (_idleCoroutine != null)
            {
                StopCoroutine(_idleCoroutine);
                _idleCoroutine = null;
            }
        }

        private void ResetSpriteTransform()
        {
            if (_spriteRt == null) return;
            _spriteRt.localScale = Vector3.one;
        }

        private void StartFrameAnimation(Sprite[] frames)
        {
            StopFrameAnimation();
            if (frames != null && frames.Length > 0)
                _animCoroutine = StartCoroutine(AnimateFrames(frames));
        }

        private void StopFrameAnimation()
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }
        }

        private IEnumerator AnimateFrames(Sprite[] frames)
        {
            int index = 0;
            while (true)
            {
                _spriteImage.sprite = frames[index];
                index = (index + 1) % frames.Length;
                yield return new WaitForSeconds(FRAME_INTERVAL);
            }
        }

        private void ClearStatusEffects()
        {
            foreach (var icon in _statusIcons)
            {
                if (icon != null && icon.gameObject != null)
                    Destroy(icon.gameObject);
            }
            _statusIcons.Clear();
        }

        private void UpdateHpText(int current, int max)
        {
            if (_hpText != null)
                _hpText.text = NumberFormatter.FormatInt(Mathf.Max(0, current));
        }

        private void BuildUI(bool isBoss, Color placeholderColor)
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            if (_rectTransform == null)
                _rectTransform = gameObject.GetComponent<RectTransform>();

                if (_rectTransform == null) _rectTransform = gameObject.AddComponent<RectTransform>();

            float charSize = isBoss ? 150f : 108f;
            _rectTransform.sizeDelta = new Vector2(charSize, charSize + 50f);

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(transform, false);
            var spriteRt = spriteGo.GetComponent<RectTransform>();

            if (spriteRt == null) spriteRt = spriteGo.AddComponent<RectTransform>();
            spriteRt.anchoredPosition = new Vector2(0f, 20f);
            spriteRt.sizeDelta = new Vector2(charSize, charSize);
            _spriteImage = spriteGo.AddComponent<Image>();
            _spriteImage.sprite = PlaceholderGenerator.CreateRect((int)charSize, (int)charSize, placeholderColor, "C");
            _spriteImage.raycastTarget = false;
            _spriteRt = spriteRt;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(transform, false);
            var nameRt = nameGo.GetComponent<RectTransform>();

            if (nameRt == null) nameRt = nameGo.AddComponent<RectTransform>();
            nameRt.anchoredPosition = new Vector2(0f, -charSize / 2f + 10f);
            nameRt.sizeDelta = new Vector2(140f, 28f);
            _nameLabel = nameGo.AddComponent<TextMeshProUGUI>();
            _nameLabel.fontSize = 22f;
            _nameLabel.color = ColorPalette.Text;
            _nameLabel.alignment = TextAlignmentOptions.Center;
            _nameLabel.textWrappingMode = TextWrappingModes.NoWrap;
            _nameLabel.overflowMode = TextOverflowModes.Ellipsis;
            _nameLabel.raycastTarget = false;
            nameGo.SetActive(false);

            float barY = -charSize / 2f - 4f;

            _hpBar = CreateBar("HpBar", barY, new Vector2(charSize + 20f, 36f), ColorPalette.Hp);
            _hpFill = _hpBar.fillRect.GetComponent<Image>();

            var hpTextGo = new GameObject("HpText");
            hpTextGo.transform.SetParent(_hpBar.transform, false);
            var hpTextRt = hpTextGo.GetComponent<RectTransform>();

            if (hpTextRt == null) hpTextRt = hpTextGo.AddComponent<RectTransform>();
            hpTextRt.anchorMin = Vector2.zero;
            hpTextRt.anchorMax = Vector2.one;
            hpTextRt.offsetMin = Vector2.zero;
            hpTextRt.offsetMax = Vector2.zero;
            _hpText = hpTextGo.AddComponent<TextMeshProUGUI>();
            _hpText.fontSize = 27f;
            _hpText.color = Color.white;
            _hpText.alignment = TextAlignmentOptions.Center;
            _hpText.textWrappingMode = TextWrappingModes.NoWrap;
            _hpText.overflowMode = TextOverflowModes.Overflow;
            _hpText.raycastTarget = false;

            _shieldOverlay = CreateOverlay("Shield", _hpBar.transform, new Color(0.3f, 0.6f, 1f, 0.5f));
            _shieldOverlay.gameObject.SetActive(false);

            _rageBar = CreateBar("RageBar", barY - 10f, new Vector2(charSize + 10f, 5f), ColorPalette.Rage);
            _rageFill = _rageBar.fillRect.GetComponent<Image>();
            _rageBar.gameObject.SetActive(false);

            var statusGo = new GameObject("StatusContainer");
            statusGo.transform.SetParent(transform, false);
            _statusContainer = statusGo.GetComponent<RectTransform>();

            if (_statusContainer == null) _statusContainer = statusGo.AddComponent<RectTransform>();
            _statusContainer.anchoredPosition = new Vector2(0f, barY - 45f);
            _statusContainer.sizeDelta = new Vector2(100f, 20f);
            var layout = statusGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private Slider CreateBar(string name, float yOffset, Vector2 size, Color fillColor)
        {
            var barGo = new GameObject(name);
            barGo.transform.SetParent(transform, false);
            var barRt = barGo.GetComponent<RectTransform>();

            if (barRt == null) barRt = barGo.AddComponent<RectTransform>();
            barRt.anchoredPosition = new Vector2(0f, yOffset);
            barRt.sizeDelta = size;

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(barGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();

            if (bgRt == null) bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = ColorPalette.ProgressBarBackground;

            var fillAreaGo = new GameObject("FillArea");
            fillAreaGo.transform.SetParent(barGo.transform, false);
            var fillAreaRt = fillAreaGo.GetComponent<RectTransform>();

            if (fillAreaRt == null) fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.offsetMin = Vector2.zero;
            fillAreaRt.offsetMax = Vector2.zero;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();

            if (fillRt == null) fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = fillColor;

            var slider = barGo.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
            slider.interactable = false;

            return slider;
        }

        private RectTransform CreateOverlay(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();

            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            _shieldImage = img;
            return rt;
        }

        private void OnDestroy()
        {
            StopFrameAnimation();
            StopIdleAnimation();
            ClearStatusEffects();
        }
    }
}
