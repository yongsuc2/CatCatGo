using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;
using BattleInstance = CatCatGo.Domain.Battle.Battle;

namespace CatCatGo.Presentation.Battle
{
    public class BattleView : MonoBehaviour
    {
        private const float APPROACH_DURATION = 0.35f;
        private const float HIT_DURATION = 0.3f;
        private const float RETREAT_DURATION = 0.35f;
        private const float PAUSE_DURATION = 0.2f;
        private const float CONSECUTIVE_SKILL_BOOST = 1.45f;
        private const int MAX_ENEMIES = 3;

        public Action<BattleState> OnBattleComplete;
        public Action<List<BattleLogEntry>> OnTurnEntries;

        private RectTransform _root;
        private RectTransform _battleField;
        private CharacterView _playerView;
        private CharacterView[] _enemyViews;
        private TextMeshProUGUI _turnText;
        private TextMeshProUGUI _vsText;
        private TextMeshProUGUI _battleLabel;
        private DamagePopupPool _popupPool;
        private RectTransform _popupLayer;

        private readonly Queue<ProjectileView> _projectilePool = new Queue<ProjectileView>();

        private float _speedMultiplier = 1f;
        private bool _cancelled;
        private Coroutine _battleCoroutine;
        private BattleInstance _battle;
        private int _activeEnemyIndex;

        private class RunningHps
        {
            public int PlayerHp;
            public int PlayerRage;
            public int[] EnemyHps;
            public int[] EnemyRages;
        }

        public void Initialize(Transform parent)
        {
            if (_root != null) return;

            var rootGo = new GameObject("BattleView");
            rootGo.transform.SetParent(parent, false);
            _root = rootGo.GetComponent<RectTransform>();

            if (_root == null) _root = rootGo.AddComponent<RectTransform>();
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = Vector2.zero;
            _root.offsetMax = Vector2.zero;

            var turnGo = new GameObject("TurnCounter");
            turnGo.transform.SetParent(_root, false);
            var turnRt = turnGo.GetComponent<RectTransform>();

            if (turnRt == null) turnRt = turnGo.AddComponent<RectTransform>();
            turnRt.anchorMin = new Vector2(0.5f, 1f);
            turnRt.anchorMax = new Vector2(0.5f, 1f);
            turnRt.pivot = new Vector2(0.5f, 1f);
            turnRt.anchoredPosition = new Vector2(0f, -2f);
            turnRt.sizeDelta = new Vector2(200f, 24f);
            _turnText = turnGo.AddComponent<TextMeshProUGUI>();
            _turnText.fontSize = 28f;
            _turnText.color = ColorPalette.TextDim;
            _turnText.alignment = TextAlignmentOptions.Center;
            _turnText.raycastTarget = false;

            var labelGo = new GameObject("BattleLabel");
            labelGo.transform.SetParent(_root, false);
            var labelRt = labelGo.GetComponent<RectTransform>();

            if (labelRt == null) labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.5f, 1f);
            labelRt.anchorMax = new Vector2(0.5f, 1f);
            labelRt.pivot = new Vector2(0.5f, 1f);
            labelRt.anchoredPosition = new Vector2(0f, -22f);
            labelRt.sizeDelta = new Vector2(200f, 20f);
            _battleLabel = labelGo.AddComponent<TextMeshProUGUI>();
            _battleLabel.fontSize = 28f;
            _battleLabel.color = ColorPalette.Hp;
            _battleLabel.alignment = TextAlignmentOptions.Center;
            _battleLabel.fontStyle = FontStyles.Bold;
            _battleLabel.raycastTarget = false;

            var fieldGo = new GameObject("BattleField");
            fieldGo.transform.SetParent(_root, false);
            _battleField = fieldGo.GetComponent<RectTransform>();

            if (_battleField == null) _battleField = fieldGo.AddComponent<RectTransform>();
            _battleField.anchorMin = new Vector2(0f, 0.05f);
            _battleField.anchorMax = new Vector2(1f, 0.92f);
            _battleField.offsetMin = Vector2.zero;
            _battleField.offsetMax = Vector2.zero;
            var fieldBg = fieldGo.AddComponent<Image>();
            fieldBg.color = new Color(0.05f, 0.05f, 0.12f, 0.5f);
            fieldBg.raycastTarget = false;

            var vsGo = new GameObject("VsText");
            vsGo.transform.SetParent(_battleField, false);
            var vsRt = vsGo.GetComponent<RectTransform>();

            if (vsRt == null) vsRt = vsGo.AddComponent<RectTransform>();
            vsRt.anchorMin = new Vector2(0.5f, 0.5f);
            vsRt.anchorMax = new Vector2(0.5f, 0.5f);
            vsRt.anchoredPosition = Vector2.zero;
            vsRt.sizeDelta = new Vector2(60f, 30f);
            _vsText = vsGo.AddComponent<TextMeshProUGUI>();
            _vsText.text = "VS";
            _vsText.fontSize = 32f;
            _vsText.color = new Color(1f, 1f, 1f, 0.3f);
            _vsText.alignment = TextAlignmentOptions.Center;
            _vsText.fontStyle = FontStyles.Bold;
            _vsText.raycastTarget = false;

            _playerView = CreateCharacterView("PlayerView", _battleField);

            _enemyViews = new CharacterView[MAX_ENEMIES];
            for (int i = 0; i < MAX_ENEMIES; i++)
                _enemyViews[i] = CreateCharacterView($"EnemyView_{i}", _battleField);

            var popupLayerGo = new GameObject("PopupLayer");
            popupLayerGo.transform.SetParent(_root, false);
            _popupLayer = popupLayerGo.GetComponent<RectTransform>();

            if (_popupLayer == null) _popupLayer = popupLayerGo.AddComponent<RectTransform>();
            _popupLayer.anchorMin = Vector2.zero;
            _popupLayer.anchorMax = Vector2.one;
            _popupLayer.offsetMin = Vector2.zero;
            _popupLayer.offsetMax = Vector2.zero;

            var poolGo = new GameObject("PopupPool");
            poolGo.transform.SetParent(_root, false);
            _popupPool = poolGo.AddComponent<DamagePopupPool>();

            _root.gameObject.SetActive(false);
        }

        private CharacterView CreateCharacterView(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var view = go.AddComponent<CharacterView>();
            go.SetActive(false);
            return view;
        }

        public void StartBattle(BattleInstance battle, bool isBoss, string label, float speedMultiplier)
        {
            _battle = battle;
            _speedMultiplier = speedMultiplier;
            _cancelled = false;
            _activeEnemyIndex = 0;

            _root.gameObject.SetActive(true);

            _turnText.text = "Turn 0";
            _battleLabel.text = string.IsNullOrEmpty(label) ? "" : label;
            _battleLabel.gameObject.SetActive(!string.IsNullOrEmpty(label));

            _playerView.gameObject.SetActive(true);
            _playerView.Setup(battle.Player.Name, battle.Player.MaxHp, battle.Player.MaxRage, false, new Color(0.3f, 0.7f, 0.4f));

            if (SpriteManager.Instance != null)
            {
                var sprite = SpriteManager.Instance.GetPlayerSprite();
                if (sprite != null)
                    _playerView.SetSprite(sprite);

                var idleFrames = SpriteManager.Instance.GetAnimationFrames("player", "idle");
                var attackFrames = SpriteManager.Instance.GetAnimationFrames("player", "attack");
                _playerView.SetAnimationFrames(idleFrames, attackFrames);
            }

            _playerView.SetOriginalPosition(new Vector2(-180f, 0f));
            _playerView.UpdateHp(battle.Player.CurrentHp, battle.Player.MaxHp);
            _playerView.UpdateRage(battle.Player.Rage, battle.Player.MaxRage);
            _playerView.UpdateShield(battle.Player.Shield, battle.Player.MaxHp);
            _playerView.UpdateStatusEffects(battle.Player.StatusEffects);

            int enemyCount = battle.Enemies.Count;
            float enemySpacing = 90f;
            float enemyStartX = 160f;

            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (i < enemyCount)
                {
                    var enemy = battle.Enemies[i];
                    bool thisIsBoss = isBoss && i == 0;
                    _enemyViews[i].gameObject.SetActive(true);
                    _enemyViews[i].Setup(enemy.Name, enemy.MaxHp, enemy.MaxRage, thisIsBoss, new Color(0.7f, 0.3f, 0.3f));

                    if (SpriteManager.Instance != null && !string.IsNullOrEmpty(enemy.TemplateId))
                    {
                        var sprite = SpriteManager.Instance.GetBattleSprite(enemy.TemplateId);
                        if (sprite != null)
                            _enemyViews[i].SetSprite(sprite);

                        var enemyIdleFrames = SpriteManager.Instance.GetAnimationFrames(enemy.TemplateId, "idle");
                        var enemyAttackFrames = SpriteManager.Instance.GetAnimationFrames(enemy.TemplateId, "attack");
                        _enemyViews[i].SetAnimationFrames(enemyIdleFrames, enemyAttackFrames);
                    }

                    _enemyViews[i].SetOriginalPosition(new Vector2(enemyStartX + i * enemySpacing, 0f));
                    _enemyViews[i].UpdateHp(enemy.CurrentHp, enemy.MaxHp);
                    _enemyViews[i].UpdateRage(enemy.Rage, enemy.MaxRage);
                    _enemyViews[i].UpdateShield(enemy.Shield, enemy.MaxHp);
                    _enemyViews[i].UpdateStatusEffects(enemy.StatusEffects);
                }
                else
                {
                    _enemyViews[i].gameObject.SetActive(false);
                }
            }

            _battleCoroutine = StartCoroutine(RunBattleLoop(battle));
        }

        public void StopBattle()
        {
            _cancelled = true;
            if (_battleCoroutine != null)
            {
                StopCoroutine(_battleCoroutine);
                _battleCoroutine = null;
            }
            CleanupBattle();
        }

        public void SetSpeedMultiplier(float speed)
        {
            _speedMultiplier = speed;
        }

        public void Hide()
        {
            if (_root != null)
                _root.gameObject.SetActive(false);
        }

        private IEnumerator RunBattleLoop(BattleInstance battle)
        {
            string playerName = battle.Player.Name;
            var enemyNames = battle.Enemies.Select(e => e.Name).ToList();

            while (battle.State == BattleState.IN_PROGRESS && !_cancelled)
            {
                int prevPlayerRage = battle.Player.Rage;
                var prevEnemyRages = battle.Enemies.Select(e => e.Rage).ToArray();

                var result = battle.ExecuteTurn();
                _turnText.text = $"Turn {result.TurnNumber}";
                OnTurnEntries?.Invoke(result.Entries);

                var playerEntries = new List<BattleLogEntry>();
                var enemyEntriesBySource = new Dictionary<string, List<BattleLogEntry>>();
                var statusEntries = new List<BattleLogEntry>();

                foreach (var entry in result.Entries)
                {
                    if (!IsDamageOrHeal(entry.Type)) continue;

                    if (entry.Type == BattleLogType.DOT_DAMAGE || entry.Type == BattleLogType.HOT_HEAL)
                    {
                        statusEntries.Add(entry);
                    }
                    else if (entry.Source == playerName)
                    {
                        playerEntries.Add(entry);
                    }
                    else
                    {
                        if (!enemyEntriesBySource.TryGetValue(entry.Source, out var list))
                        {
                            list = new List<BattleLogEntry>();
                            enemyEntriesBySource[entry.Source] = list;
                        }
                        list.Add(entry);
                    }
                }

                var running = new RunningHps
                {
                    PlayerHp = ComputePreAnimationHp(result.PlayerHp, result.Entries, playerName, true),
                    EnemyHps = new int[battle.Enemies.Count],
                };
                for (int i = 0; i < battle.Enemies.Count; i++)
                {
                    running.EnemyHps[i] = ComputePreAnimationHp(
                        result.EnemyHps[i], result.Entries, battle.Enemies[i].Name, false);
                }

                var curState = new RunningHps
                {
                    PlayerHp = running.PlayerHp,
                    PlayerRage = prevPlayerRage,
                    EnemyHps = (int[])running.EnemyHps.Clone(),
                    EnemyRages = (int[])prevEnemyRages.Clone(),
                };

                if (playerEntries.Count > 0 && !_cancelled)
                {
                    string targetEnemy = playerEntries[0].Target;
                    int targetIdx = enemyNames.IndexOf(targetEnemy);
                    if (targetIdx >= 0) _activeEnemyIndex = targetIdx;

                    var groups = SplitToAnimationGroups(ReorderBySkillType(playerEntries));
                    string prevKey = null;
                    foreach (var group in groups)
                    {
                        if (_cancelled) break;

                        int tgtIdx = enemyNames.IndexOf(group[0].Target);
                        if (tgtIdx >= 0 && running.EnemyHps[tgtIdx] <= 0) break;

                        string skillKey = GetGroupSkillKey(group);
                        float boost = (skillKey != null && skillKey == prevKey) ? CONSECUTIVE_SKILL_BOOST : 1f;
                        float speed = _speedMultiplier * boost;

                        var midHps = ComputeMidTurnHp(running, group, playerName, battle);
                        yield return AnimateHitGroup(group, true, speed, midHps, battle, playerName, enemyNames, curState);

                        running = midHps;
                        curState.PlayerRage = battle.Player.Rage;
                        prevKey = skillKey;
                    }
                }

                foreach (var kvp in enemyEntriesBySource)
                {
                    if (_cancelled || running.PlayerHp <= 0) break;

                    int eIdx = enemyNames.IndexOf(kvp.Key);
                    if (eIdx >= 0 && running.EnemyHps[eIdx] <= 0) continue;
                    if (eIdx >= 0) _activeEnemyIndex = eIdx;

                    var groups = SplitToAnimationGroups(ReorderBySkillType(kvp.Value));
                    string prevEKey = null;
                    foreach (var group in groups)
                    {
                        if (_cancelled || running.PlayerHp <= 0) break;

                        string skillKey = GetGroupSkillKey(group);
                        float boost = (skillKey != null && skillKey == prevEKey) ? CONSECUTIVE_SKILL_BOOST : 1f;
                        float speed = _speedMultiplier * boost;

                        var midHps = ComputeMidTurnHp(running, group, playerName, battle);
                        yield return AnimateHitGroup(group, false, speed, midHps, battle, playerName, enemyNames, curState);

                        running = midHps;
                        for (int ei = 0; ei < battle.Enemies.Count; ei++)
                            curState.EnemyRages[ei] = battle.Enemies[ei].Rage;
                        prevEKey = skillKey;
                    }
                }

                if (statusEntries.Count > 0 && !_cancelled)
                {
                    var statusHps = ComputeMidTurnHp(running, statusEntries, playerName, battle);

                    _playerView.UpdateHp(statusHps.PlayerHp, battle.Player.MaxHp);
                    for (int i = 0; i < battle.Enemies.Count; i++)
                    {
                        if (_enemyViews[i].gameObject.activeSelf)
                            _enemyViews[i].UpdateHp(statusHps.EnemyHps[i], battle.Enemies[i].MaxHp);
                    }

                    SpawnPopupsForEntries(statusEntries, playerName, enemyNames);
                    yield return new WaitForSeconds(HIT_DURATION / _speedMultiplier);
                    yield return new WaitForSeconds(PAUSE_DURATION / _speedMultiplier);

                    running = statusHps;
                }

                if (playerEntries.Count == 0 && enemyEntriesBySource.Count == 0 && statusEntries.Count == 0)
                {
                    yield return new WaitForSeconds(PAUSE_DURATION / _speedMultiplier);
                }

                SyncViewsToActual(battle);
            }

            if (!_cancelled)
            {
                yield return new WaitForSeconds(0.5f);
                OnBattleComplete?.Invoke(battle.State);
            }

            _battleCoroutine = null;
        }

        private IEnumerator AnimateHitGroup(
            List<BattleLogEntry> group,
            bool isPlayerAttacking,
            float speed,
            RunningHps newHps,
            BattleInstance battle,
            string playerName,
            List<string> enemyNames,
            RunningHps curState)
        {
            CharacterView attacker = isPlayerAttacking ? _playerView : _enemyViews[_activeEnemyIndex];
            CharacterView target = isPlayerAttacking ? _enemyViews[_activeEnemyIndex] : _playerView;

            if (attacker == null || target == null) yield break;

            bool hasProjectile = group.Any(e =>
                (e.Type == BattleLogType.SKILL_DAMAGE || e.Type == BattleLogType.CRIT)
                && !string.IsNullOrEmpty(e.SkillIcon)
                && e.SkillName != "\uc77c\ubc18 \uacf5\uaca9");

            if (hasProjectile)
            {
                SpawnPopupsForEntries(group, playerName, enemyNames);
                LaunchProjectile(attacker.OriginalPosition, target.OriginalPosition, APPROACH_DURATION / speed, ColorPalette.Crit);
            }

            Vector2 attackDir = target.OriginalPosition - attacker.OriginalPosition;
            Vector2 approachTarget = attacker.OriginalPosition + attackDir.normalized * 40f;

            attacker.SetPhase(AttackPhase.Approach, APPROACH_DURATION / speed, approachTarget);
            yield return new WaitForSeconds(APPROACH_DURATION / speed);
            if (_cancelled) yield break;

            if (!hasProjectile)
                SpawnPopupsForEntries(group, playerName, enemyNames);

            _playerView.UpdateHp(newHps.PlayerHp, battle.Player.MaxHp);
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                if (_enemyViews[i].gameObject.activeSelf)
                    _enemyViews[i].UpdateHp(newHps.EnemyHps[i], battle.Enemies[i].MaxHp);
            }

            _playerView.UpdateRage(battle.Player.Rage, battle.Player.MaxRage);
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                if (_enemyViews[i].gameObject.activeSelf)
                    _enemyViews[i].UpdateRage(battle.Enemies[i].Rage, battle.Enemies[i].MaxRage);
            }

            bool hasRage = group.Any(e => e.Type == BattleLogType.RAGE_ATTACK);
            if (isPlayerAttacking)
            {
                curState.PlayerRage = battle.Player.Rage;
            }
            else if (hasRage)
            {
                int eIdx = enemyNames.IndexOf(group[0].Source);
                if (eIdx >= 0) curState.EnemyRages[eIdx] = 0;
            }

            target.SetPhase(AttackPhase.Hit, HIT_DURATION / speed, target.OriginalPosition);
            yield return new WaitForSeconds(HIT_DURATION / speed);
            if (_cancelled) yield break;

            attacker.SetPhase(AttackPhase.Retreat, RETREAT_DURATION / speed, attacker.OriginalPosition);
            yield return new WaitForSeconds(RETREAT_DURATION / speed);
            if (_cancelled) yield break;

            attacker.SetPhase(AttackPhase.Idle, 0f, attacker.OriginalPosition);
            target.SetPhase(AttackPhase.Idle, 0f, target.OriginalPosition);
            yield return new WaitForSeconds(PAUSE_DURATION / speed);

            curState.PlayerHp = newHps.PlayerHp;
            for (int i = 0; i < newHps.EnemyHps.Length; i++)
                curState.EnemyHps[i] = newHps.EnemyHps[i];
        }

        private void SyncViewsToActual(BattleInstance battle)
        {
            _playerView.UpdateHp(battle.Player.CurrentHp, battle.Player.MaxHp);
            _playerView.UpdateRage(battle.Player.Rage, battle.Player.MaxRage);
            _playerView.UpdateShield(battle.Player.Shield, battle.Player.MaxHp);
            _playerView.UpdateStatusEffects(battle.Player.StatusEffects);
            _playerView.ResetToOriginalPosition();

            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                if (!_enemyViews[i].gameObject.activeSelf) continue;
                var e = battle.Enemies[i];
                _enemyViews[i].UpdateHp(e.CurrentHp, e.MaxHp);
                _enemyViews[i].UpdateRage(e.Rage, e.MaxRage);
                _enemyViews[i].UpdateShield(e.Shield, e.MaxHp);
                _enemyViews[i].UpdateStatusEffects(e.StatusEffects);
                _enemyViews[i].ResetToOriginalPosition();

                if (!e.IsAlive())
                    _enemyViews[i].gameObject.SetActive(false);
            }
        }

        private void SpawnPopupsForEntries(List<BattleLogEntry> entries, string playerName, List<string> enemyNames)
        {
            foreach (var entry in entries)
            {
                if (entry.Value <= 0 && entry.Type != BattleLogType.REVIVE) continue;

                bool isHeal = entry.Type == BattleLogType.LIFESTEAL
                           || entry.Type == BattleLogType.HOT_HEAL
                           || entry.Type == BattleLogType.REVIVE;
                bool isCrit = entry.Type == BattleLogType.CRIT;
                bool isRage = entry.Type == BattleLogType.RAGE_ATTACK;

                Vector2 targetPos;
                if (entry.Target == playerName)
                {
                    targetPos = _playerView.OriginalPosition;
                }
                else
                {
                    int idx = enemyNames.IndexOf(entry.Target);
                    if (idx >= 0 && idx < _enemyViews.Length && _enemyViews[idx].gameObject.activeSelf)
                        targetPos = _enemyViews[idx].OriginalPosition;
                    else
                        targetPos = Vector2.zero;
                }

                float yJitter = UnityEngine.Random.Range(-10f, 10f);
                float xJitter = UnityEngine.Random.Range(-15f, 15f);

                var popup = _popupPool.Get(_popupLayer);
                var popupRt = popup.GetComponent<RectTransform>();
                popupRt.anchorMin = new Vector2(0.5f, 0.5f);
                popupRt.anchorMax = new Vector2(0.5f, 0.5f);
                popupRt.anchoredPosition = targetPos + new Vector2(xJitter, 40f + yJitter);
                popup.Show(entry.Value, isHeal, isCrit, isRage, entry.SkillId, _speedMultiplier);
            }
        }

        private void LaunchProjectile(Vector2 from, Vector2 to, float duration, Color color)
        {
            ProjectileView proj;
            if (_projectilePool.Count > 0)
            {
                proj = _projectilePool.Dequeue();
            }
            else
            {
                var go = new GameObject("Projectile");
                go.transform.SetParent(_battleField, false);
                proj = go.AddComponent<ProjectileView>();
            }

            proj.Launch(from, to, duration, color, () =>
            {
                proj.gameObject.SetActive(false);
                _projectilePool.Enqueue(proj);
            });
        }

        private void CleanupBattle()
        {
            _popupPool.ReturnAll();
            _playerView.ResetToOriginalPosition();
            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                _enemyViews[i].ResetToOriginalPosition();
                _enemyViews[i].gameObject.SetActive(false);
            }
            foreach (var proj in _projectilePool)
                proj.ForceStop();
        }

        private static bool IsDamageOrHeal(BattleLogType type)
        {
            return type == BattleLogType.ATTACK
                || type == BattleLogType.SKILL_DAMAGE
                || type == BattleLogType.COUNTER
                || type == BattleLogType.CRIT
                || type == BattleLogType.DOT_DAMAGE
                || type == BattleLogType.RAGE_ATTACK
                || type == BattleLogType.LIFESTEAL
                || type == BattleLogType.HOT_HEAL
                || type == BattleLogType.REVIVE;
        }

        private static bool IsHealType(BattleLogType type)
        {
            return type == BattleLogType.LIFESTEAL
                || type == BattleLogType.HOT_HEAL
                || type == BattleLogType.REVIVE;
        }

        private int ComputePreAnimationHp(int finalHp, List<BattleLogEntry> allEntries, string unitName, bool isPlayer)
        {
            int delta = 0;
            foreach (var entry in allEntries)
            {
                if (!IsDamageOrHeal(entry.Type)) continue;
                if (entry.Target != unitName) continue;
                if (IsHealType(entry.Type))
                    delta -= entry.Value;
                else
                    delta += entry.Value;
            }
            return Mathf.Max(0, finalHp + delta);
        }

        private RunningHps ComputeMidTurnHp(RunningHps before, List<BattleLogEntry> entries, string playerName, BattleInstance battle)
        {
            int playerHp = before.PlayerHp;
            int[] enemyHps = (int[])before.EnemyHps.Clone();
            var enemyNames = battle.Enemies.Select(e => e.Name).ToList();

            foreach (var entry in entries)
            {
                if (IsHealType(entry.Type))
                {
                    if (entry.Target == playerName)
                        playerHp += entry.Value;
                    else
                    {
                        int idx = enemyNames.IndexOf(entry.Target);
                        if (idx >= 0) enemyHps[idx] += entry.Value;
                    }
                }
                else
                {
                    if (entry.Target == playerName)
                        playerHp -= entry.Value;
                    else
                    {
                        int idx = enemyNames.IndexOf(entry.Target);
                        if (idx >= 0) enemyHps[idx] -= entry.Value;
                    }
                }
            }

            return new RunningHps
            {
                PlayerHp = Mathf.Clamp(playerHp, 0, battle.Player.MaxHp),
                EnemyHps = enemyHps.Select((hp, i) => Mathf.Clamp(hp, 0, battle.Enemies[i].MaxHp)).ToArray(),
            };
        }

        private static List<BattleLogEntry> ReorderBySkillType(List<BattleLogEntry> entries)
        {
            var reordered = new List<BattleLogEntry>();
            var skillBuffer = new List<BattleLogEntry>();
            var lifestealBuffer = new List<BattleLogEntry>();

            void FlushBuffers()
            {
                if (skillBuffer.Count > 0)
                {
                    var byName = new Dictionary<string, List<BattleLogEntry>>();
                    var order = new List<string>();
                    foreach (var e in skillBuffer)
                    {
                        string key = e.SkillName ?? "";
                        if (!byName.ContainsKey(key))
                        {
                            byName[key] = new List<BattleLogEntry>();
                            order.Add(key);
                        }
                        byName[key].Add(e);
                    }
                    foreach (var name in order)
                        reordered.AddRange(byName[name]);
                    skillBuffer.Clear();
                }
                reordered.AddRange(lifestealBuffer);
                lifestealBuffer.Clear();
            }

            foreach (var entry in entries)
            {
                bool isBoundary = entry.Type == BattleLogType.ATTACK
                    || entry.Type == BattleLogType.RAGE_ATTACK
                    || (entry.Type == BattleLogType.CRIT && entry.SkillName == "\uc77c\ubc18 \uacf5\uaca9");

                if (isBoundary)
                {
                    FlushBuffers();
                    reordered.Add(entry);
                }
                else if (entry.Type == BattleLogType.LIFESTEAL)
                {
                    lifestealBuffer.Add(entry);
                }
                else
                {
                    skillBuffer.Add(entry);
                }
            }
            FlushBuffers();

            return reordered;
        }

        private static List<List<BattleLogEntry>> SplitToAnimationGroups(List<BattleLogEntry> entries)
        {
            var groups = new List<List<BattleLogEntry>>();

            foreach (var entry in entries)
            {
                if (entry.Type == BattleLogType.LIFESTEAL)
                {
                    if (groups.Count > 0)
                        groups[groups.Count - 1].Add(entry);
                    else
                        groups.Add(new List<BattleLogEntry> { entry });
                }
                else
                {
                    groups.Add(new List<BattleLogEntry> { entry });
                }
            }
            return groups;
        }

        private static string GetGroupSkillKey(List<BattleLogEntry> group)
        {
            if (group.Count == 0) return null;
            var primary = group[0];
            if (primary.Type == BattleLogType.SKILL_DAMAGE || primary.Type == BattleLogType.CRIT)
                return primary.SkillName;
            return null;
        }
    }
}
