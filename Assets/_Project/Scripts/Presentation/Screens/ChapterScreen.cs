using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;
using CatCatGo.Services;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;
using CatCatGo.Presentation.Battle;
using CatCatGo.Presentation.Components;
using BattleInstance = CatCatGo.Domain.Battle.Battle;
using ChapterInstance = CatCatGo.Domain.Chapter.Chapter;
using ChapterEncounter = CatCatGo.Domain.Chapter.Encounter;
using SessionSkillWrapper = CatCatGo.Domain.Chapter.SessionSkillWrapper;

namespace CatCatGo.Presentation.Screens
{
    public class ChapterScreen : BaseScreen
    {
        private enum ScreenState
        {
            Idle,
            Encounter,
            Battling,
            EliteReward,
            Result,
        }

        private enum BattleType
        {
            Normal,
            Elite,
            MidBoss,
            Boss,
        }

        private struct ChapterResult
        {
            public bool IsVictory;
            public int ChapterId;
            public int Gold;
            public int Gems;
            public int Day;
            public int TotalDays;
            public int EnemyRemainingHp;
            public int EnemyMaxHp;
        }

        private ScreenState _state = ScreenState.Idle;
        private BattleType _battleType = BattleType.Normal;

        private RectTransform _rootPanel;
        private RectTransform _idlePanel;
        private RectTransform _chapterHeader;
        private RectTransform _encounterPanel;
        private RectTransform _eliteRewardPanel;
        private RectTransform _resultPanel;

        private TextMeshProUGUI _chapterTitle;
        private TextMeshProUGUI _dayProgress;
        private Slider _dayProgressBar;
        private TextMeshProUGUI _sessionGoldText;
        private TextMeshProUGUI _skillCountText;
        private TextMeshProUGUI _jungbakText;
        private TextMeshProUGUI _daebakText;
        private TextMeshProUGUI _encounterTitle;
        private TextMeshProUGUI _encounterDesc;
        private TextMeshProUGUI _dayLabel;
        private RectTransform _optionsContainer;
        private Button _rerollButton;
        private TextMeshProUGUI _rerollText;

        private TextMeshProUGUI _idleNextChapter;
        private Button _startButton;

        private BattleView _battleView;
        private PlayerStatsBarView _statsBar;
        private DamageGraphView _damageGraph;
        private DamageGraphView _healGraph;

        private Button _speedButton;
        private TextMeshProUGUI _speedButtonLabel;
        private Button _graphToggle;
        private RectTransform _graphContainer;

        private TextMeshProUGUI _resultTitle;
        private TextMeshProUGUI _resultInfo;
        private TextMeshProUGUI _resultRewards;
        private Button _resultHomeButton;
        private Button _resultContinueButton;

        private RectTransform _eliteOptionsContainer;
        private RectTransform _sessionSkillsContainer;

        private RectTransform _settingsOverlay;
        private RectTransform _settingsSkillList;

        private ChapterEncounter _encounter;
        private ChapterResult _chapterResult;
        private List<SessionSkillWrapper> _eliteRewardChoices;

        private float _battleSpeed;
        private bool _showDamageGraph;
        private string _playerBattleName;
        private readonly Dictionary<string, int> _damageMap = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _healMap = new Dictionary<string, int>();

        private void Awake()
        {
            _battleSpeed = PlayerPrefs.GetInt("battleSpeed", 1) == 2 ? 2f : 1f;
            _showDamageGraph = PlayerPrefs.GetInt("showDamageGraph", 0) == 1;
            BuildUI();
        }

        public override void OnScreenEnter()
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null)
            {
                SetState(ScreenState.Idle);
            }
            else
            {
                if (chapter.SessionMaxHp == 0)
                {
                    var stats = Game.Player.ComputeStats();
                    chapter.InitSessionHp(stats.MaxHp);
                }
                SetState(ScreenState.Encounter);
                AdvanceDay();
            }
            Refresh();
        }

        public override void OnScreenExit()
        {
            UI.SetNavBarVisible(true);
            if (_state == ScreenState.Battling)
                _battleView.StopBattle();
        }

        public override void Refresh()
        {
            UpdateStatsBar();
            UpdateSessionSkillsDisplay();
        }

        private void SetState(ScreenState state)
        {
            _state = state;

            bool showNavBar = state == ScreenState.Idle || state == ScreenState.Result;
            UI.SetNavBarVisible(showNavBar);

            _idlePanel.gameObject.SetActive(state == ScreenState.Idle);
            _chapterHeader.gameObject.SetActive(state != ScreenState.Idle && state != ScreenState.Result);
            _encounterPanel.gameObject.SetActive(state == ScreenState.Encounter);
            _eliteRewardPanel.gameObject.SetActive(state == ScreenState.EliteReward);
            _resultPanel.gameObject.SetActive(state == ScreenState.Result);

            bool battling = state == ScreenState.Battling;
            if (_battleView != null)
            {
                if (!battling)
                    _battleView.Hide();
            }

            _speedButton.gameObject.SetActive(battling);
            _graphToggle.gameObject.SetActive(battling);
            bool showGraph = _showDamageGraph && (state == ScreenState.Battling || state == ScreenState.Result);
            _graphContainer.gameObject.SetActive(showGraph);

            if (state != ScreenState.Idle && state != ScreenState.Result)
                UpdateChapterHeader();
        }

        private void UpdateChapterHeader()
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null) return;

            _chapterTitle.text = $"\ucc55\ud130 {chapter.Id}";
            _dayProgress.text = $"{chapter.CurrentDay}\uc77c / {chapter.TotalDays}\uc77c";
            _dayProgressBar.maxValue = chapter.TotalDays;
            _dayProgressBar.value = chapter.CurrentDay;
            _sessionGoldText.text = NumberFormatter.FormatInt(chapter.SessionGold);
            _skillCountText.text = $"\uc2a4\ud0ac {chapter.SessionSkills.Count}";

            var threshold = EncounterDataTable.CounterThreshold;
            _jungbakText.text = $"\uc911\ubc15 {chapter.JungbakCount}/{threshold.Jungbak}";
            _daebakText.text = $"\ub300\ubc15 {chapter.DaebakCount}/{threshold.Daebak}";
        }

        private void UpdateStatsBar()
        {
            var chapter = Game.CurrentChapter;
            var playerStats = Game.Player.ComputeStats();

            if (chapter != null)
            {
                int atk = playerStats.Atk;
                int def = playerStats.Def;

                var battleStats = Stats.Create(
                    maxHp: chapter.SessionMaxHp,
                    hp: chapter.SessionCurrentHp,
                    atk: playerStats.Atk,
                    def: playerStats.Def,
                    crit: playerStats.Crit);
                var petAbility = Game.BattleManagerService.GetPetAbilitySkill(Game.Player);
                var allPassives = new List<PassiveSkill>(chapter.GetBattlePassiveSkills());
                if (petAbility != null) allPassives.Add(petAbility);
                var tempUnit = new BattleUnit("temp", battleStats, null, allPassives.ToArray(), true);
                atk = tempUnit.BaseAtk;
                def = tempUnit.BaseDef;

                _statsBar.SetStats(chapter.SessionCurrentHp, chapter.SessionMaxHp, atk, def);
            }
            else
            {
                _statsBar.SetStats(playerStats.Hp, playerStats.MaxHp, playerStats.Atk, playerStats.Def);
            }
        }

        private void UpdateSessionSkillsDisplay()
        {
            foreach (Transform child in _sessionSkillsContainer)
                Destroy(child.gameObject);

            var chapter = Game.CurrentChapter;
            if (chapter == null || _state == ScreenState.Battling) return;

            foreach (var skill in chapter.SessionSkills)
            {
                var iconGo = new GameObject($"Skill_{skill.Id}");
                iconGo.transform.SetParent(_sessionSkillsContainer, false);
                var rt = iconGo.GetComponent<RectTransform>();

                if (rt == null) rt = iconGo.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(144f, 144f);

                var img = iconGo.AddComponent<Image>();
                img.sprite = SpriteManager.Instance.GetSkillIcon(skill.Id);
                img.preserveAspect = true;
            }
        }

        private void StartChapter()
        {
            int nextId = Game.Player.ClearedChapterMax + 1;
            Game.StartChapter(nextId, ChapterType.SIXTY_DAY);

            var chapter = Game.CurrentChapter;
            if (chapter == null) return;

            var stats = Game.Player.ComputeStats();
            chapter.InitSessionHp(stats.MaxHp);

            _damageMap.Clear();
            _healMap.Clear();

            SetState(ScreenState.Encounter);
            AdvanceDay();
        }

        private void AdvanceDay()
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null) return;

            if (chapter.IsBossDay())
            {
                StartSpecialBattle(BattleType.Boss);
                return;
            }

            var enc = chapter.AdvanceDay();

            if (enc == null && chapter.IsEliteDay())
            {
                StartSpecialBattle(BattleType.Elite);
                return;
            }

            if (enc == null && chapter.IsMidBossDay())
            {
                StartSpecialBattle(BattleType.MidBoss);
                return;
            }

            if (enc == null && chapter.IsOptionalEliteDay())
            {
                StartSpecialBattle(BattleType.Elite);
                return;
            }

            if (enc == null && chapter.IsBossDay())
            {
                StartSpecialBattle(BattleType.Boss);
                return;
            }

            if (enc != null && enc.Type == EncounterType.COMBAT)
            {
                var stats = Game.Player.ComputeStats();
                var ch = chapter;
                var battleStats = Stats.Create(
                    maxHp: ch.SessionMaxHp, hp: ch.SessionCurrentHp,
                    atk: stats.Atk, def: stats.Def, crit: stats.Crit);
                var petAbility = Game.BattleManagerService.GetPetAbilitySkill(Game.Player);
                var allPassives = new List<PassiveSkill>(ch.GetBattlePassiveSkills());
                if (petAbility != null) allPassives.Add(petAbility);
                var pu = new BattleUnit("Capybara", battleStats, ch.GetSessionActiveSkills().ToArray(), allPassives.ToArray(), true);
                var b = chapter.CreateCombatBattle(pu);
                if (b != null)
                {
                    _battleType = BattleType.Normal;
                    StartBattle(b, false);
                }
                Refresh();
                return;
            }

            if (enc != null && enc.Options.Count == 1)
            {
                var ch = chapter;
                var result = ch.ResolveEncounter(0, ch.SessionCurrentHp, ch.SessionMaxHp);
                if (result != null)
                {
                    float goldMult = Game.Player.GetGoldMultiplier();
                    foreach (var r in result.Reward.Resources)
                    {
                        int amt = r.Type == ResourceType.GOLD ? Mathf.FloorToInt(r.Amount * goldMult) : r.Amount;
                        Game.Player.Resources.Add(r.Type, amt);
                    }
                }
                Refresh();
                AdvanceDay();
                return;
            }

            _encounter = enc;
            ShowEncounter();
            Refresh();
        }

        private void ShowEncounter()
        {
            if (_encounter == null) return;

            SetState(ScreenState.Encounter);

            var ch = Game.CurrentChapter;
            if (ch != null)
                _dayLabel.text = $"--- {ch.CurrentDay}\uc77c\ucc28 ---";

            _encounterTitle.text = EncounterDataTable.GetLabel(_encounter.Type);
            _encounterDesc.text = EncounterDataTable.GetDescription(_encounter.Type);

            foreach (Transform child in _optionsContainer)
                Destroy(child.gameObject);

            var chapter = Game.CurrentChapter;

            for (int i = 0; i < _encounter.Options.Count; i++)
            {
                int idx = i;
                var opt = _encounter.Options[i];

                var optGo = new GameObject($"Option_{i}");
                optGo.transform.SetParent(_optionsContainer, false);
                var optRt = optGo.GetComponent<RectTransform>();

                if (optRt == null) optRt = optGo.AddComponent<RectTransform>();
                optRt.sizeDelta = new Vector2(0f, 120f);
                var optLe = optGo.AddComponent<LayoutElement>();
                optLe.flexibleWidth = 1f;
                optLe.preferredHeight = 120f;

                var optBg = optGo.AddComponent<Image>();
                optBg.color = ColorPalette.CardLight;

                var optBtn = optGo.AddComponent<Button>();
                optBtn.targetGraphic = optBg;
                optBtn.onClick.AddListener(() => SelectOption(idx));

                var optLayout = optGo.AddComponent<VerticalLayoutGroup>();
                optLayout.padding = new RectOffset(12, 12, 8, 8);
                optLayout.spacing = 4f;
                optLayout.childForceExpandWidth = true;
                optLayout.childForceExpandHeight = false;

                var labelRowGo = new GameObject("LabelRow");
                labelRowGo.transform.SetParent(optGo.transform, false);
                labelRowGo.AddComponent<RectTransform>();
                var labelRowLe = labelRowGo.AddComponent<LayoutElement>();
                labelRowLe.preferredHeight = 76f;
                var labelRowHlg = labelRowGo.AddComponent<HorizontalLayoutGroup>();
                labelRowHlg.spacing = 8f;
                labelRowHlg.childForceExpandWidth = false;
                labelRowHlg.childForceExpandHeight = true;
                labelRowHlg.childAlignment = TextAnchor.MiddleLeft;

                if (!string.IsNullOrEmpty(opt.SkillId))
                {
                    var optIconGo = new GameObject("SkillIcon");
                    optIconGo.transform.SetParent(labelRowGo.transform, false);
                    var optIconLe = optIconGo.AddComponent<LayoutElement>();
                    optIconLe.preferredWidth = 72f;
                    optIconLe.preferredHeight = 72f;
                    var optIconImg = optIconGo.AddComponent<Image>();
                    optIconImg.sprite = SpriteManager.Instance.GetSkillIcon(opt.SkillId);
                    optIconImg.preserveAspect = true;
                    optIconImg.raycastTarget = false;
                }

                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(labelRowGo.transform, false);
                labelGo.AddComponent<RectTransform>();
                var labelLe = labelGo.AddComponent<LayoutElement>();
                labelLe.flexibleWidth = 1f;
                var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
                labelTmp.text = opt.Label;
                labelTmp.fontSize = 30f;
                labelTmp.color = ColorPalette.Text;
                labelTmp.fontStyle = FontStyles.Bold;
                labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
                labelTmp.enableWordWrapping = false;
                labelTmp.raycastTarget = false;

                var descGo = new GameObject("Desc");
                descGo.transform.SetParent(optGo.transform, false);
                descGo.AddComponent<RectTransform>();
                var descLe = descGo.AddComponent<LayoutElement>();
                descLe.preferredHeight = 56f;
                var descTmp = descGo.AddComponent<TextMeshProUGUI>();
                descTmp.text = opt.Description;
                descTmp.fontSize = 26f;
                descTmp.color = ColorPalette.TextDim;
                descTmp.alignment = TextAlignmentOptions.TopLeft;
                descTmp.enableWordWrapping = true;
                descTmp.raycastTarget = false;
            }

            bool canReroll = chapter != null && chapter.SessionRerollsRemaining > 0;
            _rerollButton.gameObject.SetActive(canReroll);
            if (canReroll)
                _rerollText.text = $"\ub9ac\ub864 ({chapter.SessionRerollsRemaining})";
        }

        private void SelectOption(int index)
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null || _encounter == null) return;

            var result = chapter.ResolveEncounter(index, chapter.SessionCurrentHp, chapter.SessionMaxHp);
            if (result != null)
            {
                float goldMult = Game.Player.GetGoldMultiplier();
                foreach (var r in result.Reward.Resources)
                {
                    int amt = r.Type == ResourceType.GOLD ? Mathf.FloorToInt(r.Amount * goldMult) : r.Amount;
                    Game.Player.Resources.Add(r.Type, amt);
                }
            }

            _encounter = null;
            Refresh();
            AdvanceDay();
        }

        private void RerollEncounter()
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null) return;

            var newEnc = chapter.RerollEncounter();
            if (newEnc != null)
            {
                _encounter = newEnc;
                ShowEncounter();
            }
        }

        private void StartSpecialBattle(BattleType type)
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null) return;

            var stats = Game.Player.ComputeStats();
            var battleStats = Stats.Create(
                maxHp: chapter.SessionMaxHp, hp: chapter.SessionCurrentHp,
                atk: stats.Atk, def: stats.Def, crit: stats.Crit);
            var petAbility = Game.BattleManagerService.GetPetAbilitySkill(Game.Player);
            var specialPassives = new List<PassiveSkill>(chapter.GetBattlePassiveSkills());
            if (petAbility != null) specialPassives.Add(petAbility);
            var pu = new BattleUnit("Capybara", battleStats, chapter.GetSessionActiveSkills().ToArray(), specialPassives.ToArray(), true);

            BattleInstance b = null;
            switch (type)
            {
                case BattleType.Elite:
                    b = chapter.CreateEliteBattle(pu);
                    break;
                case BattleType.MidBoss:
                    b = chapter.CreateMidBossBattle(pu);
                    break;
                case BattleType.Boss:
                    b = chapter.CreateBossBattle(pu);
                    break;
            }

            if (b == null) return;
            _battleType = type;
            StartBattle(b, type == BattleType.Boss);
        }

        private void StartBattle(BattleInstance battle, bool isBoss)
        {
            _damageMap.Clear();
            _healMap.Clear();
            _playerBattleName = battle.Player.Name;

            string label = _battleType == BattleType.Elite ? "\uc5d8\ub9ac\ud2b8"
                         : _battleType == BattleType.MidBoss ? "\ubcf4\uc2a4"
                         : _battleType == BattleType.Boss ? "\ucd5c\uc885 \ubcf4\uc2a4"
                         : null;

            SetState(ScreenState.Battling);

            _battleView.OnBattleComplete = OnBattleComplete;
            _battleView.OnTurnEntries = AccumulateBattleEntries;
            _battleView.StartBattle(battle, isBoss, label, _battleSpeed);
        }

        private void AccumulateBattleEntries(List<BattleLogEntry> entries)
        {
            var result = BattleLogCategorizer.Categorize(entries, _playerBattleName);

            foreach (var kv in result.DamageMap)
            {
                if (_damageMap.ContainsKey(kv.Key)) _damageMap[kv.Key] += kv.Value;
                else _damageMap[kv.Key] = kv.Value;
            }

            foreach (var kv in result.HealMap)
            {
                if (_healMap.ContainsKey(kv.Key)) _healMap[kv.Key] += kv.Value;
                else _healMap[kv.Key] = kv.Value;
            }

            if (_showDamageGraph)
            {
                _damageGraph.SetData(_damageMap);
                _healGraph.SetData(_healMap);
            }
        }

        private void OnBattleComplete(BattleState state)
        {
            var chapter = Game.CurrentChapter;
            bool isBoss = _battleType == BattleType.Boss;

            if (isBoss)
            {
                HandleBossBattleEnd(state, chapter);
            }
            else
            {
                HandleNormalBattleEnd(state, chapter);
            }
        }

        private void HandleBossBattleEnd(BattleState state, ChapterInstance chapter)
        {
            int chId = chapter?.Id ?? 0;
            int chDay = chapter?.CurrentDay ?? 0;
            int chTotalDays = chapter?.TotalDays ?? 0;
            int remainingHp = 0;
            int totalMaxHp = 0;

            if (chapter?.CurrentBattle != null)
            {
                foreach (var e in chapter.CurrentBattle.Enemies)
                {
                    remainingHp += Mathf.Max(0, e.CurrentHp);
                    totalMaxHp += e.MaxHp;
                }
            }

            if (state == BattleState.VICTORY)
            {
                chapter?.OnBossDefeated();
                if (chapter != null)
                {
                    Game.Player.ClearedChapterMax = Mathf.Max(Game.Player.ClearedChapterMax, chapter.Id);
                    Game.TravelSystem.MaxClearedChapter = Game.Player.ClearedChapterMax;
                    int clearGold = Mathf.FloorToInt(EncounterDataTable.GetChapterClearGold(chapter.Id) * Game.Player.GetGoldMultiplier());
                    int clearGems = EncounterDataTable.GetChapterClearGems(chapter.Id);
                    Game.Player.Resources.Add(ResourceType.GOLD, clearGold);
                    Game.Player.Resources.Add(ResourceType.GEMS, clearGems);
                }
            }
            else
            {
                chapter?.OnBattleEnd(state);
            }

            if (chapter != null)
                Game.Player.UpdateBestSurvivalDay(chapter.Id, chapter.CurrentDay, chapter.IsCompleted());

            Game.CurrentChapter = null;
            _battleView.StopBattle();

            _chapterResult = new ChapterResult
            {
                IsVictory = state == BattleState.VICTORY,
                ChapterId = chId,
                Gold = state == BattleState.VICTORY ? EncounterDataTable.GetChapterClearGold(chId) : 0,
                Gems = state == BattleState.VICTORY ? EncounterDataTable.GetChapterClearGems(chId) : 0,
                Day = chDay,
                TotalDays = chTotalDays,
                EnemyRemainingHp = remainingHp,
                EnemyMaxHp = totalMaxHp,
            };

            Game.SaveGame();
            ShowResult();
        }

        private void HandleNormalBattleEnd(BattleState state, ChapterInstance chapter)
        {
            if (chapter != null && state == BattleState.VICTORY && chapter.CurrentBattle != null)
                chapter.UpdateSessionHpAfterBattle(chapter.CurrentBattle.Player.CurrentHp);

            int goldRaw = chapter?.OnBattleEnd(state) ?? 0;
            int goldEarned = goldRaw > 0 ? Mathf.FloorToInt(goldRaw * Game.Player.GetGoldMultiplier()) : 0;
            if (goldEarned > 0)
                Game.Player.Resources.Add(ResourceType.GOLD, goldEarned);

            if (state == BattleState.DEFEAT)
            {
                int chId = chapter?.Id ?? 0;
                int chDay = chapter?.CurrentDay ?? 0;
                int chTotalDays = chapter?.TotalDays ?? 0;
                int remainingHp = 0;
                int totalMaxHp = 0;

                if (chapter?.CurrentBattle != null)
                {
                    foreach (var e in chapter.CurrentBattle.Enemies)
                    {
                        remainingHp += Mathf.Max(0, e.CurrentHp);
                        totalMaxHp += e.MaxHp;
                    }
                }

                if (chapter != null)
                    Game.Player.UpdateBestSurvivalDay(chapter.Id, chapter.CurrentDay, false);

                Game.CurrentChapter = null;
                _battleView.StopBattle();

                _chapterResult = new ChapterResult
                {
                    IsVictory = false,
                    ChapterId = chId,
                    Gold = 0,
                    Gems = 0,
                    Day = chDay,
                    TotalDays = chTotalDays,
                    EnemyRemainingHp = remainingHp,
                    EnemyMaxHp = totalMaxHp,
                };

                Game.SaveGame();
                ShowResult();
            }
            else if (_battleType == BattleType.Elite || _battleType == BattleType.MidBoss)
            {
                _battleView.StopBattle();
                ShowEliteReward();
            }
            else
            {
                _battleView.StopBattle();
                SetState(ScreenState.Encounter);
                _encounter = null;
                Refresh();
                AdvanceDay();
            }
        }

        private void ShowEliteReward()
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null)
            {
                SetState(ScreenState.Encounter);
                AdvanceDay();
                return;
            }

            var ownedMap = new Dictionary<string, int>();
            foreach (var s in chapter.SessionSkills)
                ownedMap[s.Id] = s.Tier;

            var tier3Pool = new List<SessionSkillWrapper>();
            foreach (var s in ActiveSkillRegistry.GetAll())
            {
                if (s.Tier != 3 || ActiveSkillRegistry.IsSpecialSkill(s.Id)) continue;
                if (ownedMap.TryGetValue(s.Id, out int owned) && owned >= s.Tier) continue;
                tier3Pool.Add(new SessionSkillWrapper(s));
            }
            foreach (var s in PassiveSkillRegistry.GetAll())
            {
                if (s.Tier != 3 || PassiveSkillRegistry.IsSpecialSkill(s.Id)) continue;
                if (ownedMap.TryGetValue(s.Id, out int owned) && owned >= s.Tier) continue;
                tier3Pool.Add(new SessionSkillWrapper(s));
            }

            var shuffled = tier3Pool.OrderBy(_ => UnityEngine.Random.value).ToList();
            _eliteRewardChoices = shuffled.Take(3).ToList();

            SetState(ScreenState.EliteReward);
            BuildEliteRewardOptions();
            Refresh();
        }

        private void BuildEliteRewardOptions()
        {
            foreach (Transform child in _eliteOptionsContainer)
                Destroy(child.gameObject);

            if (_eliteRewardChoices == null) return;

            for (int i = 0; i < _eliteRewardChoices.Count; i++)
            {
                int idx = i;
                var skill = _eliteRewardChoices[i];

                var cardGo = new GameObject($"SkillCard_{i}");
                cardGo.transform.SetParent(_eliteOptionsContainer, false);
                var cardRt = cardGo.GetComponent<RectTransform>();

                if (cardRt == null) cardRt = cardGo.AddComponent<RectTransform>();
                cardRt.sizeDelta = new Vector2(0f, 80f);
                var cardLe = cardGo.AddComponent<LayoutElement>();
                cardLe.flexibleWidth = 1f;
                cardLe.preferredHeight = 80f;

                var cardBg = cardGo.AddComponent<Image>();
                cardBg.color = ColorPalette.CardLight;

                var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
                cardLayout.padding = new RectOffset(10, 10, 6, 6);
                cardLayout.spacing = 4f;
                cardLayout.childForceExpandWidth = true;
                cardLayout.childForceExpandHeight = false;

                var nameRowGo = new GameObject("NameRow");
                nameRowGo.transform.SetParent(cardGo.transform, false);
                nameRowGo.AddComponent<RectTransform>();
                var nameRowLe = nameRowGo.AddComponent<LayoutElement>();
                nameRowLe.preferredHeight = 68f;
                var nameRowLayout = nameRowGo.AddComponent<HorizontalLayoutGroup>();
                nameRowLayout.spacing = 8f;
                nameRowLayout.childForceExpandWidth = false;
                nameRowLayout.childForceExpandHeight = true;
                nameRowLayout.childAlignment = TextAnchor.MiddleLeft;

                var iconImgGo = new GameObject("SkillIcon");
                iconImgGo.transform.SetParent(nameRowGo.transform, false);
                var iconImgLe = iconImgGo.AddComponent<LayoutElement>();
                iconImgLe.preferredWidth = 64f;
                iconImgLe.preferredHeight = 64f;
                var iconImg = iconImgGo.AddComponent<Image>();
                iconImg.sprite = SpriteManager.Instance.GetSkillIcon(skill.Id);
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                var nameGo = new GameObject("Name");
                nameGo.transform.SetParent(nameRowGo.transform, false);
                nameGo.AddComponent<RectTransform>();
                var nameLe = nameGo.AddComponent<LayoutElement>();
                nameLe.flexibleWidth = 1f;
                var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
                nameTmp.text = skill.Name;
                nameTmp.fontSize = 22f;
                nameTmp.color = ColorPalette.Text;
                nameTmp.fontStyle = FontStyles.Bold;
                nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
                nameTmp.enableWordWrapping = false;
                nameTmp.raycastTarget = false;

                var descGo = new GameObject("Desc");
                descGo.transform.SetParent(cardGo.transform, false);
                descGo.AddComponent<RectTransform>();
                var descLe = descGo.AddComponent<LayoutElement>();
                descLe.preferredHeight = 44f;
                var descTmp = descGo.AddComponent<TextMeshProUGUI>();
                descTmp.text = skill.Description;
                descTmp.fontSize = 22f;
                descTmp.color = ColorPalette.TextDim;
                descTmp.alignment = TextAlignmentOptions.TopLeft;
                descTmp.enableWordWrapping = true;
                descTmp.raycastTarget = false;

                var btn = cardGo.AddComponent<Button>();
                btn.targetGraphic = cardBg;
                btn.onClick.AddListener(() => SelectEliteReward(idx));
            }
        }

        private void SelectEliteReward(int index)
        {
            var chapter = Game.CurrentChapter;
            if (chapter == null || _eliteRewardChoices == null || index >= _eliteRewardChoices.Count) return;

            var chosen = _eliteRewardChoices[index];
            int existingIdx = chapter.SessionSkills.FindIndex(s => s.Id == chosen.Id);
            if (existingIdx >= 0)
                chapter.SessionSkills[existingIdx] = chosen;
            else
                chapter.SessionSkills.Add(chosen);

            chapter.RecalcSessionMaxHp();
            _eliteRewardChoices = null;

            SetState(ScreenState.Encounter);
            Refresh();
            AdvanceDay();
        }

        private void ShowResult()
        {
            SetState(ScreenState.Result);

            _resultTitle.text = _chapterResult.IsVictory ? "\uc2b9\ub9ac!" : "\ud328\ubc30";
            _resultTitle.color = _chapterResult.IsVictory ? ColorPalette.Heal : ColorPalette.Hp;

            string info = $"\ucc55\ud130 {_chapterResult.ChapterId}";
            if (!_chapterResult.IsVictory)
            {
                info += $"\n{_chapterResult.Day}\uc77c\ucc28 \ud328\ubc30 (/{_chapterResult.TotalDays})";
                if (_chapterResult.EnemyMaxHp > 0)
                {
                    float remainPct = (float)_chapterResult.EnemyRemainingHp / _chapterResult.EnemyMaxHp * 100f;
                    info += $"\n\uc801 \ub0a8\uc740 HP: {remainPct:F1}%";
                }
            }
            _resultInfo.text = info;

            string rewards = "";
            if (_chapterResult.Gold > 0)
                rewards += $"+{NumberFormatter.FormatInt(_chapterResult.Gold)} G\n";
            if (_chapterResult.Gems > 0)
                rewards += $"+{NumberFormatter.FormatInt(_chapterResult.Gems)} Gems\n";
            _resultRewards.text = rewards;

            _resultContinueButton.gameObject.SetActive(_chapterResult.IsVictory);
        }

        private void OnResultHome()
        {
            SetState(ScreenState.Idle);
            Refresh();
        }

        private void OnResultContinue()
        {
            StartChapter();
        }

        private void BuildSettingsOverlay()
        {
            var overlayGo = new GameObject("SettingsOverlay");
            overlayGo.transform.SetParent(transform, false);
            _settingsOverlay = overlayGo.GetComponent<RectTransform>();

            if (_settingsOverlay == null) _settingsOverlay = overlayGo.AddComponent<RectTransform>();
            UIManager.StretchFull(_settingsOverlay);

            var overlayBg = overlayGo.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.85f);

            var innerGo = new GameObject("Inner");
            innerGo.transform.SetParent(overlayGo.transform, false);
            var innerRt = innerGo.GetComponent<RectTransform>();

            if (innerRt == null) innerRt = innerGo.AddComponent<RectTransform>();
            innerRt.anchorMin = new Vector2(0.05f, 0.1f);
            innerRt.anchorMax = new Vector2(0.95f, 0.9f);
            innerRt.offsetMin = Vector2.zero;
            innerRt.offsetMax = Vector2.zero;
            innerGo.AddComponent<Image>().color = ColorPalette.Card;

            var innerLayout = innerGo.AddComponent<VerticalLayoutGroup>();
            innerLayout.padding = new RectOffset(16, 16, 16, 16);
            innerLayout.spacing = 10f;
            innerLayout.childForceExpandWidth = true;
            innerLayout.childForceExpandHeight = false;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(innerGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 36f;
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "\ubaa8\ud5d8 \uc124\uc815";
            titleTmp.fontSize = 28f;
            titleTmp.color = ColorPalette.Text;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;

            var scrollGo = new GameObject("SkillScroll");
            scrollGo.transform.SetParent(innerGo.transform, false);
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
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

            var skillListGo = new GameObject("SkillList");
            skillListGo.transform.SetParent(viewportGo.transform, false);
            _settingsSkillList = skillListGo.GetComponent<RectTransform>();

            if (_settingsSkillList == null) _settingsSkillList = skillListGo.AddComponent<RectTransform>();
            _settingsSkillList.anchorMin = new Vector2(0, 1);
            _settingsSkillList.anchorMax = new Vector2(1, 1);
            _settingsSkillList.pivot = new Vector2(0.5f, 1);
            _settingsSkillList.offsetMin = Vector2.zero;
            _settingsSkillList.offsetMax = Vector2.zero;

            var skillListLayout = skillListGo.AddComponent<VerticalLayoutGroup>();
            skillListLayout.spacing = 6f;
            skillListLayout.childForceExpandWidth = true;
            skillListLayout.childForceExpandHeight = false;

            var skillListFitter = skillListGo.AddComponent<ContentSizeFitter>();
            skillListFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = _settingsSkillList;
            scrollRect.viewport = viewportRt;

            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(innerGo.transform, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 120f;
            var btnRowLayout = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 12f;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            var abandonBtn = UIManager.CreateButton(btnRowGo.transform, "\ubaa8\ud5d8 \ud3ec\uae30\ud558\uae30", () =>
            {
                HideSettingsOverlay();
                AbandonChapter();
            }, "AbandonBtn");
            abandonBtn.GetComponent<Image>().color = ColorPalette.Hp;

            var continueBtn = UIManager.CreateButton(btnRowGo.transform, "\uacc4\uc18d\ud558\uae30", HideSettingsOverlay, "ContinueBtn");
            continueBtn.GetComponent<Image>().color = ColorPalette.ButtonPrimary;

            _settingsOverlay.gameObject.SetActive(false);
        }

        private void ShowSettingsOverlay()
        {
            foreach (Transform child in _settingsSkillList)
                Destroy(child.gameObject);

            var chapter = Game.CurrentChapter;
            if (chapter != null)
            {
                foreach (var skill in chapter.SessionSkills)
                {
                    var skillGo = new GameObject("Skill_" + skill.Name);
                    skillGo.transform.SetParent(_settingsSkillList, false);
                    skillGo.AddComponent<Image>().color = ColorPalette.CardLight;
                    var skillLe = skillGo.AddComponent<LayoutElement>();
                    skillLe.preferredHeight = 56f;

                    var skillLayout = skillGo.AddComponent<VerticalLayoutGroup>();
                    skillLayout.padding = new RectOffset(10, 10, 4, 4);
                    skillLayout.spacing = 2f;
                    skillLayout.childForceExpandWidth = true;
                    skillLayout.childForceExpandHeight = false;

                    var nameRowGo = new GameObject("NameRow");
                    nameRowGo.transform.SetParent(skillGo.transform, false);
                    var nameRowLe = nameRowGo.AddComponent<LayoutElement>();
                    nameRowLe.preferredHeight = 60f;
                    var nameRowHlg = nameRowGo.AddComponent<HorizontalLayoutGroup>();
                    nameRowHlg.spacing = 8f;
                    nameRowHlg.childForceExpandWidth = false;
                    nameRowHlg.childForceExpandHeight = true;
                    nameRowHlg.childAlignment = TextAnchor.MiddleLeft;

                    var sIconGo = new GameObject("SkillIcon");
                    sIconGo.transform.SetParent(nameRowGo.transform, false);
                    var sIconLe = sIconGo.AddComponent<LayoutElement>();
                    sIconLe.preferredWidth = 56f;
                    sIconLe.preferredHeight = 56f;
                    var sIconImg = sIconGo.AddComponent<Image>();
                    sIconImg.sprite = SpriteManager.Instance.GetSkillIcon(skill.Id);
                    sIconImg.preserveAspect = true;
                    sIconImg.raycastTarget = false;

                    var nameGo = new GameObject("Name");
                    nameGo.transform.SetParent(nameRowGo.transform, false);
                    var nameLe = nameGo.AddComponent<LayoutElement>();
                    nameLe.flexibleWidth = 1f;
                    var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
                    nameTmp.text = skill.Name;
                    nameTmp.fontSize = 22f;
                    nameTmp.color = ColorPalette.Text;
                    nameTmp.alignment = TextAlignmentOptions.Left;
                    nameTmp.raycastTarget = false;

                    var descGo = new GameObject("Desc");
                    descGo.transform.SetParent(skillGo.transform, false);
                    var descLe = descGo.AddComponent<LayoutElement>();
                    descLe.preferredHeight = 22f;
                    var descTmp = descGo.AddComponent<TextMeshProUGUI>();
                    descTmp.text = skill.Description;
                    descTmp.fontSize = 22f;
                    descTmp.color = ColorPalette.TextDim;
                    descTmp.alignment = TextAlignmentOptions.Left;
                    descTmp.enableWordWrapping = true;
                    descTmp.raycastTarget = false;
                }

                if (chapter.SessionSkills.Count == 0)
                {
                    var emptyGo = new GameObject("Empty");
                    emptyGo.transform.SetParent(_settingsSkillList, false);
                    var emptyLe = emptyGo.AddComponent<LayoutElement>();
                    emptyLe.preferredHeight = 40f;
                    var emptyTmp = emptyGo.AddComponent<TextMeshProUGUI>();
                    emptyTmp.text = "\ud68d\ub4dd\ud55c \uc2a4\ud0ac\uc774 \uc5c6\uc2b5\ub2c8\ub2e4";
                    emptyTmp.fontSize = 22f;
                    emptyTmp.color = ColorPalette.TextDim;
                    emptyTmp.alignment = TextAlignmentOptions.Center;
                    emptyTmp.raycastTarget = false;
                }
            }

            _settingsOverlay.gameObject.SetActive(true);
        }

        private void HideSettingsOverlay()
        {
            _settingsOverlay.gameObject.SetActive(false);
        }

        private void AbandonChapter()
        {
            var chapter = Game.CurrentChapter;
            int chId = chapter?.Id ?? 0;
            int chDay = chapter?.CurrentDay ?? 0;
            int chTotalDays = chapter?.TotalDays ?? 0;

            if (_state == ScreenState.Battling)
                _battleView.StopBattle();

            if (chapter != null)
                Game.Player.UpdateBestSurvivalDay(chapter.Id, chapter.CurrentDay, false);

            Game.CurrentChapter = null;
            Game.SaveGame();

            _chapterResult = new ChapterResult
            {
                IsVictory = false,
                ChapterId = chId,
                Gold = 0,
                Gems = 0,
                Day = chDay,
                TotalDays = chTotalDays,
            };

            ShowResult();
        }

        private void ToggleSpeed()
        {
            _battleSpeed = _battleSpeed >= 2f ? 1f : 2f;
            PlayerPrefs.SetInt("battleSpeed", (int)_battleSpeed);
            _speedButtonLabel.text = _battleSpeed >= 2f ? "2x" : "1x";
            _battleView.SetSpeedMultiplier(_battleSpeed);
        }

        private void ToggleGraph()
        {
            _showDamageGraph = !_showDamageGraph;
            PlayerPrefs.SetInt("showDamageGraph", _showDamageGraph ? 1 : 0);
            _graphContainer.gameObject.SetActive(_showDamageGraph && _state == ScreenState.Battling);
            if (_showDamageGraph)
            {
                _damageGraph.SetData(_damageMap);
                _healGraph.SetData(_healMap);
            }
        }

        private void BuildUI()
        {
            _rootPanel = gameObject.GetComponent<RectTransform>();
            if (_rootPanel == null)
                _rootPanel = gameObject.GetComponent<RectTransform>();

                if (_rootPanel == null) _rootPanel = gameObject.AddComponent<RectTransform>();

            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();

            if (scrollRt == null) scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();

            if (viewportRt == null) viewportRt = viewportGo.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();

            if (contentRt == null) contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 0f);
            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 6f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildIdlePanel(contentGo.transform);
            BuildChapterHeader(contentGo.transform);
            BuildBattleArea(contentGo.transform);
            BuildBattleControls(contentGo.transform);
            BuildEncounterPanel(contentGo.transform);
            BuildEliteRewardPanel(contentGo.transform);
            BuildStatsBar(contentGo.transform);
            BuildSessionSkillsDisplay(contentGo.transform);
            BuildGraphArea(contentGo.transform);
            BuildResultPanel(contentGo.transform);
            BuildSettingsOverlay();
        }

        private void BuildIdlePanel(Transform parent)
        {
            var go = new GameObject("IdlePanel");
            go.transform.SetParent(parent, false);
            _idlePanel = go.GetComponent<RectTransform>();

            if (_idlePanel == null) _idlePanel = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 420f;

            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 16, 16);
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var nextGo = new GameObject("NextChapter");
            nextGo.transform.SetParent(go.transform, false);
            nextGo.AddComponent<RectTransform>();
            var nextLe = nextGo.AddComponent<LayoutElement>();
            nextLe.preferredHeight = 36f;
            _idleNextChapter = nextGo.AddComponent<TextMeshProUGUI>();
            _idleNextChapter.fontSize = 32f;
            _idleNextChapter.color = ColorPalette.Text;
            _idleNextChapter.alignment = TextAlignmentOptions.Center;
            _idleNextChapter.raycastTarget = false;

            var staminaGo = new GameObject("Stamina");
            staminaGo.transform.SetParent(go.transform, false);
            staminaGo.AddComponent<RectTransform>();
            var staminaLe = staminaGo.AddComponent<LayoutElement>();
            staminaLe.preferredHeight = 32f;
            var staminaTmp = staminaGo.AddComponent<TextMeshProUGUI>();
            staminaTmp.text = "\uc2a4\ud0dc\ubbf8\ub098 \uc18c\ubaa8: 5";
            staminaTmp.fontSize = 26f;
            staminaTmp.color = ColorPalette.TextDim;
            staminaTmp.alignment = TextAlignmentOptions.Center;
            staminaTmp.raycastTarget = false;

            var btnGo = new GameObject("StartButton");
            btnGo.transform.SetParent(go.transform, false);
            btnGo.AddComponent<RectTransform>();
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredHeight = 120f;
            var btnBg = btnGo.AddComponent<Image>();
            btnBg.color = ColorPalette.ButtonPrimary;
            _startButton = btnGo.AddComponent<Button>();
            _startButton.targetGraphic = btnBg;
            _startButton.onClick.AddListener(StartChapter);

            var btnText = new GameObject("Text");
            btnText.transform.SetParent(btnGo.transform, false);
            var btnTextRt = btnText.GetComponent<RectTransform>();

            if (btnTextRt == null) btnTextRt = btnText.AddComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;
            var btnTmp = btnText.AddComponent<TextMeshProUGUI>();
            btnTmp.fontSize = 36f;
            btnTmp.color = Color.white;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.raycastTarget = false;

            _idleNextChapter.text = $"\ucc55\ud130 {Game.Player.ClearedChapterMax + 1}";
            btnTmp.text = $"\ucc55\ud130 {Game.Player.ClearedChapterMax + 1} \uc2dc\uc791";

            var treasureGo = new GameObject("TreasureButton");
            treasureGo.transform.SetParent(go.transform, false);
            treasureGo.AddComponent<RectTransform>();
            var treasureLe = treasureGo.AddComponent<LayoutElement>();
            treasureLe.preferredHeight = 120f;
            var treasureBg = treasureGo.AddComponent<Image>();
            treasureBg.color = ColorPalette.ButtonSecondary;
            var treasureBtn = treasureGo.AddComponent<Button>();
            treasureBtn.targetGraphic = treasureBg;
            treasureBtn.onClick.AddListener(() => UI.ShowScreen(ScreenType.ChapterTreasure));

            var treasureTextGo = new GameObject("Text");
            treasureTextGo.transform.SetParent(treasureGo.transform, false);
            var treasureTextRt = treasureTextGo.GetComponent<RectTransform>();

            if (treasureTextRt == null) treasureTextRt = treasureTextGo.AddComponent<RectTransform>();
            treasureTextRt.anchorMin = Vector2.zero;
            treasureTextRt.anchorMax = Vector2.one;
            treasureTextRt.offsetMin = Vector2.zero;
            treasureTextRt.offsetMax = Vector2.zero;
            var treasureTmp = treasureTextGo.AddComponent<TextMeshProUGUI>();
            treasureTmp.text = "\ubcf4\ubb3c\uc0c1\uc790";
            treasureTmp.fontSize = 36f;
            treasureTmp.color = Color.white;
            treasureTmp.alignment = TextAlignmentOptions.Center;
            treasureTmp.raycastTarget = false;
        }

        private void BuildChapterHeader(Transform parent)
        {
            var go = new GameObject("ChapterHeader");
            go.transform.SetParent(parent, false);
            _chapterHeader = go.GetComponent<RectTransform>();

            if (_chapterHeader == null) _chapterHeader = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 80f;

            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 6, 6);
            layout.spacing = 3f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleRow = new GameObject("TitleRow");
            titleRow.transform.SetParent(go.transform, false);
            titleRow.AddComponent<RectTransform>();
            var titleRowLe = titleRow.AddComponent<LayoutElement>();
            titleRowLe.preferredHeight = 40f;
            var titleRowLayout = titleRow.AddComponent<HorizontalLayoutGroup>();
            titleRowLayout.childForceExpandWidth = false;
            titleRowLayout.childForceExpandHeight = false;
            titleRowLayout.childAlignment = TextAnchor.MiddleLeft;

            var chTitleGo = new GameObject("ChapterTitle");
            chTitleGo.transform.SetParent(titleRow.transform, false);
            chTitleGo.AddComponent<RectTransform>();
            var chTitleLe = chTitleGo.AddComponent<LayoutElement>();
            chTitleLe.flexibleWidth = 1f;
            chTitleLe.preferredHeight = 40f;
            _chapterTitle = chTitleGo.AddComponent<TextMeshProUGUI>();
            _chapterTitle.fontSize = 33f;
            _chapterTitle.color = ColorPalette.Text;
            _chapterTitle.fontStyle = FontStyles.Bold;
            _chapterTitle.alignment = TextAlignmentOptions.MidlineLeft;
            _chapterTitle.raycastTarget = false;

            var dayTextGo = new GameObject("DayProgress");
            dayTextGo.transform.SetParent(titleRow.transform, false);
            dayTextGo.AddComponent<RectTransform>();
            var dayTextLe = dayTextGo.AddComponent<LayoutElement>();
            dayTextLe.preferredWidth = 160f;
            dayTextLe.preferredHeight = 40f;
            _dayProgress = dayTextGo.AddComponent<TextMeshProUGUI>();
            _dayProgress.fontSize = 33f;
            _dayProgress.color = ColorPalette.TextDim;
            _dayProgress.alignment = TextAlignmentOptions.MidlineRight;
            _dayProgress.raycastTarget = false;

            var progressGo = new GameObject("ProgressBar");
            progressGo.transform.SetParent(go.transform, false);
            var progressRt = progressGo.GetComponent<RectTransform>();

            if (progressRt == null) progressRt = progressGo.AddComponent<RectTransform>();
            var progressLe = progressGo.AddComponent<LayoutElement>();
            progressLe.preferredHeight = 12f;

            var pBg = new GameObject("Bg");
            pBg.transform.SetParent(progressGo.transform, false);
            var pBgRt = pBg.GetComponent<RectTransform>();

            if (pBgRt == null) pBgRt = pBg.AddComponent<RectTransform>();
            pBgRt.anchorMin = Vector2.zero;
            pBgRt.anchorMax = Vector2.one;
            pBgRt.offsetMin = Vector2.zero;
            pBgRt.offsetMax = Vector2.zero;
            var pBgImg = pBg.AddComponent<Image>();
            pBgImg.color = ColorPalette.ProgressBarBackground;

            var pFillArea = new GameObject("FillArea");
            pFillArea.transform.SetParent(progressGo.transform, false);
            var pFillAreaRt = pFillArea.GetComponent<RectTransform>();

            if (pFillAreaRt == null) pFillAreaRt = pFillArea.AddComponent<RectTransform>();
            pFillAreaRt.anchorMin = Vector2.zero;
            pFillAreaRt.anchorMax = Vector2.one;
            pFillAreaRt.offsetMin = Vector2.zero;
            pFillAreaRt.offsetMax = Vector2.zero;

            var pFill = new GameObject("Fill");
            pFill.transform.SetParent(pFillArea.transform, false);
            var pFillRt = pFill.GetComponent<RectTransform>();

            if (pFillRt == null) pFillRt = pFill.AddComponent<RectTransform>();
            pFillRt.anchorMin = Vector2.zero;
            pFillRt.anchorMax = Vector2.one;
            pFillRt.offsetMin = Vector2.zero;
            pFillRt.offsetMax = Vector2.zero;
            var pFillImg = pFill.AddComponent<Image>();
            pFillImg.color = ColorPalette.ProgressBarFill;

            _dayProgressBar = progressGo.AddComponent<Slider>();
            _dayProgressBar.fillRect = pFillRt;
            _dayProgressBar.targetGraphic = pFillImg;
            _dayProgressBar.interactable = false;

            var goldRow = new GameObject("GoldRow");
            goldRow.transform.SetParent(go.transform, false);
            goldRow.AddComponent<RectTransform>();
            var goldRowLe = goldRow.AddComponent<LayoutElement>();
            goldRowLe.preferredHeight = 32f;
            var goldRowLayout = goldRow.AddComponent<HorizontalLayoutGroup>();
            goldRowLayout.childForceExpandWidth = false;
            goldRowLayout.childForceExpandHeight = false;
            goldRowLayout.childAlignment = TextAnchor.MiddleLeft;

            var goldLabelGo = new GameObject("GoldLabel");
            goldLabelGo.transform.SetParent(goldRow.transform, false);
            goldLabelGo.AddComponent<RectTransform>();
            var goldLabelLe = goldLabelGo.AddComponent<LayoutElement>();
            goldLabelLe.flexibleWidth = 1f;
            goldLabelLe.preferredHeight = 40f;
            var goldLabelTmp = goldLabelGo.AddComponent<TextMeshProUGUI>();
            goldLabelTmp.text = "\ud68d\ub4dd \uace8\ub4dc";
            goldLabelTmp.fontSize = 33f;
            goldLabelTmp.color = ColorPalette.TextDim;
            goldLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            goldLabelTmp.raycastTarget = false;

            var goldValGo = new GameObject("GoldValue");
            goldValGo.transform.SetParent(goldRow.transform, false);
            goldValGo.AddComponent<RectTransform>();
            var goldValLe = goldValGo.AddComponent<LayoutElement>();
            goldValLe.preferredWidth = 120f;
            goldValLe.preferredHeight = 40f;
            _sessionGoldText = goldValGo.AddComponent<TextMeshProUGUI>();
            _sessionGoldText.fontSize = 33f;
            _sessionGoldText.color = ColorPalette.Gold;
            _sessionGoldText.alignment = TextAlignmentOptions.MidlineRight;
            _sessionGoldText.raycastTarget = false;

            var skillRow = new GameObject("SkillRow");
            skillRow.transform.SetParent(go.transform, false);
            skillRow.AddComponent<RectTransform>();
            var skillRowLe = skillRow.AddComponent<LayoutElement>();
            skillRowLe.preferredHeight = 40f;
            _skillCountText = skillRow.AddComponent<TextMeshProUGUI>();
            _skillCountText.fontSize = 33f;
            _skillCountText.color = ColorPalette.TextDim;
            _skillCountText.alignment = TextAlignmentOptions.MidlineLeft;
            _skillCountText.raycastTarget = false;

            var counterRow = new GameObject("CounterRow");
            counterRow.transform.SetParent(go.transform, false);
            counterRow.AddComponent<RectTransform>();
            var counterRowLe = counterRow.AddComponent<LayoutElement>();
            counterRowLe.preferredHeight = 40f;
            var counterRowLayout = counterRow.AddComponent<HorizontalLayoutGroup>();
            counterRowLayout.spacing = 12f;
            counterRowLayout.childForceExpandWidth = true;
            counterRowLayout.childForceExpandHeight = true;

            var jungbakGo = new GameObject("Jungbak");
            jungbakGo.transform.SetParent(counterRow.transform, false);
            jungbakGo.AddComponent<RectTransform>();
            _jungbakText = jungbakGo.AddComponent<TextMeshProUGUI>();
            _jungbakText.fontSize = 33f;
            _jungbakText.color = ColorPalette.Gold;
            _jungbakText.alignment = TextAlignmentOptions.MidlineLeft;
            _jungbakText.raycastTarget = false;

            var daebakGo = new GameObject("Daebak");
            daebakGo.transform.SetParent(counterRow.transform, false);
            daebakGo.AddComponent<RectTransform>();
            _daebakText = daebakGo.AddComponent<TextMeshProUGUI>();
            _daebakText.fontSize = 33f;
            _daebakText.color = ColorPalette.Gold;
            _daebakText.alignment = TextAlignmentOptions.MidlineLeft;
            _daebakText.raycastTarget = false;

            var settingsBtnGo = new GameObject("SettingsBtn");
            settingsBtnGo.transform.SetParent(titleRow.transform, false);
            settingsBtnGo.AddComponent<RectTransform>();
            var settingsBtnLe = settingsBtnGo.AddComponent<LayoutElement>();
            settingsBtnLe.preferredWidth = 72f;
            settingsBtnLe.preferredHeight = 40f;
            var settingsBtnBg = settingsBtnGo.AddComponent<Image>();
            settingsBtnBg.color = ColorPalette.ButtonSecondary;
            var settingsBtn = settingsBtnGo.AddComponent<Button>();
            settingsBtn.targetGraphic = settingsBtnBg;
            settingsBtn.onClick.AddListener(ShowSettingsOverlay);

            var settingsBtnTextGo = new GameObject("Text");
            settingsBtnTextGo.transform.SetParent(settingsBtnGo.transform, false);
            var settingsBtnTextRt = settingsBtnTextGo.GetComponent<RectTransform>();

            if (settingsBtnTextRt == null) settingsBtnTextRt = settingsBtnTextGo.AddComponent<RectTransform>();
            UIManager.StretchFull(settingsBtnTextRt);
            var settingsBtnTmp = settingsBtnTextGo.AddComponent<TextMeshProUGUI>();
            settingsBtnTmp.text = "\uc124\uc815";
            settingsBtnTmp.fontSize = 28f;
            settingsBtnTmp.color = Color.white;
            settingsBtnTmp.alignment = TextAlignmentOptions.Center;
            settingsBtnTmp.raycastTarget = false;
        }

        private void BuildBattleArea(Transform parent)
        {
            var go = new GameObject("BattleArea");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 260f;

            _battleView = go.AddComponent<BattleView>();
            _battleView.Initialize(go.transform);
        }

        private void BuildBattleControls(Transform parent)
        {
            var controlsGo = new GameObject("BattleControls");
            controlsGo.transform.SetParent(parent, false);
            controlsGo.AddComponent<RectTransform>();
            var controlsLe = controlsGo.AddComponent<LayoutElement>();
            controlsLe.preferredHeight = 60f;

            var controlsLayout = controlsGo.AddComponent<HorizontalLayoutGroup>();
            controlsLayout.spacing = 8f;
            controlsLayout.childForceExpandWidth = false;
            controlsLayout.childForceExpandHeight = true;
            controlsLayout.childAlignment = TextAnchor.MiddleRight;

            var speedGo = new GameObject("SpeedButton");
            speedGo.transform.SetParent(controlsGo.transform, false);
            speedGo.AddComponent<RectTransform>();
            var speedLe = speedGo.AddComponent<LayoutElement>();
            speedLe.preferredWidth = 80f;
            speedLe.preferredHeight = 60f;
            var speedBg = speedGo.AddComponent<Image>();
            speedBg.color = _battleSpeed >= 2f ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
            _speedButton = speedGo.AddComponent<Button>();
            _speedButton.targetGraphic = speedBg;
            _speedButton.onClick.AddListener(ToggleSpeed);

            var speedTextGo = new GameObject("Text");
            speedTextGo.transform.SetParent(speedGo.transform, false);
            var speedTextRt = speedTextGo.GetComponent<RectTransform>();

            if (speedTextRt == null) speedTextRt = speedTextGo.AddComponent<RectTransform>();
            speedTextRt.anchorMin = Vector2.zero;
            speedTextRt.anchorMax = Vector2.one;
            speedTextRt.offsetMin = Vector2.zero;
            speedTextRt.offsetMax = Vector2.zero;
            _speedButtonLabel = speedTextGo.AddComponent<TextMeshProUGUI>();
            _speedButtonLabel.text = _battleSpeed >= 2f ? "2x" : "1x";
            _speedButtonLabel.fontSize = 28f;
            _speedButtonLabel.color = Color.white;
            _speedButtonLabel.alignment = TextAlignmentOptions.Center;
            _speedButtonLabel.fontStyle = FontStyles.Bold;
            _speedButtonLabel.raycastTarget = false;

            var graphGo = new GameObject("GraphToggle");
            graphGo.transform.SetParent(controlsGo.transform, false);
            graphGo.AddComponent<RectTransform>();
            var graphLe = graphGo.AddComponent<LayoutElement>();
            graphLe.preferredWidth = 120f;
            graphLe.preferredHeight = 60f;
            var graphBg = graphGo.AddComponent<Image>();
            graphBg.color = _showDamageGraph ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
            _graphToggle = graphGo.AddComponent<Button>();
            _graphToggle.targetGraphic = graphBg;
            _graphToggle.onClick.AddListener(ToggleGraph);

            var graphTextGo = new GameObject("Text");
            graphTextGo.transform.SetParent(graphGo.transform, false);
            var graphTextRt = graphTextGo.GetComponent<RectTransform>();

            if (graphTextRt == null) graphTextRt = graphTextGo.AddComponent<RectTransform>();
            graphTextRt.anchorMin = Vector2.zero;
            graphTextRt.anchorMax = Vector2.one;
            graphTextRt.offsetMin = Vector2.zero;
            graphTextRt.offsetMax = Vector2.zero;
            var graphTmp = graphTextGo.AddComponent<TextMeshProUGUI>();
            graphTmp.text = "\uadf8\ub798\ud504";
            graphTmp.fontSize = 28f;
            graphTmp.color = Color.white;
            graphTmp.alignment = TextAlignmentOptions.Center;
            graphTmp.raycastTarget = false;

            _speedButton.gameObject.SetActive(false);
            _graphToggle.gameObject.SetActive(false);
        }

        private void BuildEncounterPanel(Transform parent)
        {
            var go = new GameObject("EncounterPanel");
            go.transform.SetParent(parent, false);
            _encounterPanel = go.GetComponent<RectTransform>();

            if (_encounterPanel == null) _encounterPanel = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.flexibleHeight = 1f;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.spacing = 6f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var dayLabelGo = new GameObject("DayLabel");
            dayLabelGo.transform.SetParent(go.transform, false);
            dayLabelGo.AddComponent<RectTransform>();
            var dayLabelLe = dayLabelGo.AddComponent<LayoutElement>();
            dayLabelLe.preferredHeight = 40f;
            _dayLabel = dayLabelGo.AddComponent<TextMeshProUGUI>();
            _dayLabel.fontSize = 33f;
            _dayLabel.color = ColorPalette.TextDim;
            _dayLabel.alignment = TextAlignmentOptions.Center;
            _dayLabel.raycastTarget = false;

            var titleGo = new GameObject("EncounterTitle");
            titleGo.transform.SetParent(go.transform, false);
            titleGo.AddComponent<RectTransform>();
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 24f;
            _encounterTitle = titleGo.AddComponent<TextMeshProUGUI>();
            _encounterTitle.fontSize = 24f;
            _encounterTitle.color = ColorPalette.Text;
            _encounterTitle.fontStyle = FontStyles.Bold;
            _encounterTitle.alignment = TextAlignmentOptions.Center;
            _encounterTitle.raycastTarget = false;

            var descGo = new GameObject("EncounterDesc");
            descGo.transform.SetParent(go.transform, false);
            descGo.AddComponent<RectTransform>();
            var descLe = descGo.AddComponent<LayoutElement>();
            descLe.preferredHeight = 28f;
            _encounterDesc = descGo.AddComponent<TextMeshProUGUI>();
            _encounterDesc.fontSize = 22f;
            _encounterDesc.color = ColorPalette.TextDim;
            _encounterDesc.alignment = TextAlignmentOptions.Center;
            _encounterDesc.enableWordWrapping = true;
            _encounterDesc.raycastTarget = false;

            var optionsGo = new GameObject("Options");
            optionsGo.transform.SetParent(go.transform, false);
            _optionsContainer = optionsGo.GetComponent<RectTransform>();

            if (_optionsContainer == null) _optionsContainer = optionsGo.AddComponent<RectTransform>();
            var optionsLayout = optionsGo.AddComponent<VerticalLayoutGroup>();
            optionsLayout.spacing = 4f;
            optionsLayout.childForceExpandWidth = true;
            optionsLayout.childForceExpandHeight = false;
            var optionsFitter = optionsGo.AddComponent<ContentSizeFitter>();
            optionsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rerollGo = new GameObject("RerollButton");
            rerollGo.transform.SetParent(go.transform, false);
            rerollGo.AddComponent<RectTransform>();
            var rerollLe = rerollGo.AddComponent<LayoutElement>();
            rerollLe.preferredHeight = 100f;
            var rerollBg = rerollGo.AddComponent<Image>();
            rerollBg.color = ColorPalette.ButtonSecondary;
            _rerollButton = rerollGo.AddComponent<Button>();
            _rerollButton.targetGraphic = rerollBg;
            _rerollButton.onClick.AddListener(RerollEncounter);

            var rerollTextGo = new GameObject("Text");
            rerollTextGo.transform.SetParent(rerollGo.transform, false);
            var rerollTextRt = rerollTextGo.GetComponent<RectTransform>();

            if (rerollTextRt == null) rerollTextRt = rerollTextGo.AddComponent<RectTransform>();
            rerollTextRt.anchorMin = Vector2.zero;
            rerollTextRt.anchorMax = Vector2.one;
            rerollTextRt.offsetMin = Vector2.zero;
            rerollTextRt.offsetMax = Vector2.zero;
            _rerollText = rerollTextGo.AddComponent<TextMeshProUGUI>();
            _rerollText.fontSize = 30f;
            _rerollText.color = Color.white;
            _rerollText.alignment = TextAlignmentOptions.Center;
            _rerollText.raycastTarget = false;

            _encounterPanel.gameObject.SetActive(false);
        }

        private void BuildEliteRewardPanel(Transform parent)
        {
            var go = new GameObject("EliteRewardPanel");
            go.transform.SetParent(parent, false);
            _eliteRewardPanel = go.GetComponent<RectTransform>();

            if (_eliteRewardPanel == null) _eliteRewardPanel = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.flexibleHeight = 1f;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 8, 8);
            layout.spacing = 6f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(go.transform, false);
            titleGo.AddComponent<RectTransform>();
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 26f;
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "\uae08\uc0c1\uc790! \uc2a4\ud0ac\uc744 \uc120\ud0dd\ud558\uc138\uc694";
            titleTmp.fontSize = 24f;
            titleTmp.color = ColorPalette.Gold;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;

            var optionsGo = new GameObject("EliteOptions");
            optionsGo.transform.SetParent(go.transform, false);
            _eliteOptionsContainer = optionsGo.GetComponent<RectTransform>();

            if (_eliteOptionsContainer == null) _eliteOptionsContainer = optionsGo.AddComponent<RectTransform>();
            var optionsLayout = optionsGo.AddComponent<VerticalLayoutGroup>();
            optionsLayout.spacing = 6f;
            optionsLayout.childForceExpandWidth = true;
            optionsLayout.childForceExpandHeight = false;
            var optionsFitter = optionsGo.AddComponent<ContentSizeFitter>();
            optionsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _eliteRewardPanel.gameObject.SetActive(false);
        }

        private void BuildStatsBar(Transform parent)
        {
            var go = new GameObject("StatsBar");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;
            _statsBar = go.AddComponent<PlayerStatsBarView>();
        }

        private void BuildSessionSkillsDisplay(Transform parent)
        {
            var go = new GameObject("SessionSkills");
            go.transform.SetParent(parent, false);
            _sessionSkillsContainer = go.GetComponent<RectTransform>();

            if (_sessionSkillsContainer == null) _sessionSkillsContainer = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 34f;

            var gridLayout = go.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(28f, 28f);
            gridLayout.spacing = new Vector2(3f, 3f);
            gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
            gridLayout.childAlignment = TextAnchor.MiddleLeft;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void BuildGraphArea(Transform parent)
        {
            var go = new GameObject("GraphContainer");
            go.transform.SetParent(parent, false);
            _graphContainer = go.GetComponent<RectTransform>();

            if (_graphContainer == null) _graphContainer = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.flexibleHeight = 0f;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var dmgGo = new GameObject("DamageGraph");
            dmgGo.transform.SetParent(go.transform, false);
            dmgGo.AddComponent<RectTransform>();
            var dmgLe = dmgGo.AddComponent<LayoutElement>();
            dmgLe.preferredHeight = 100f;
            _damageGraph = dmgGo.AddComponent<DamageGraphView>();
            _damageGraph.Initialize("\ub525 \uadf8\ub798\ud504", ColorPalette.Hp);

            var healGo = new GameObject("HealGraph");
            healGo.transform.SetParent(go.transform, false);
            healGo.AddComponent<RectTransform>();
            var healLe = healGo.AddComponent<LayoutElement>();
            healLe.preferredHeight = 100f;
            _healGraph = healGo.AddComponent<DamageGraphView>();
            _healGraph.Initialize("\ud68c\ubcf5 \uadf8\ub798\ud504", ColorPalette.Heal);

            _graphContainer.gameObject.SetActive(false);
        }

        private void BuildResultPanel(Transform parent)
        {
            var go = new GameObject("ResultPanel");
            go.transform.SetParent(parent, false);
            _resultPanel = go.GetComponent<RectTransform>();

            if (_resultPanel == null) _resultPanel = go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 400f;

            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 10f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var titleGo = new GameObject("ResultTitle");
            titleGo.transform.SetParent(go.transform, false);
            titleGo.AddComponent<RectTransform>();
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 48f;
            _resultTitle = titleGo.AddComponent<TextMeshProUGUI>();
            _resultTitle.fontSize = 42f;
            _resultTitle.fontStyle = FontStyles.Bold;
            _resultTitle.alignment = TextAlignmentOptions.Center;
            _resultTitle.raycastTarget = false;

            var infoGo = new GameObject("ResultInfo");
            infoGo.transform.SetParent(go.transform, false);
            infoGo.AddComponent<RectTransform>();
            var infoLe = infoGo.AddComponent<LayoutElement>();
            infoLe.preferredHeight = 60f;
            _resultInfo = infoGo.AddComponent<TextMeshProUGUI>();
            _resultInfo.fontSize = 28f;
            _resultInfo.color = ColorPalette.Text;
            _resultInfo.alignment = TextAlignmentOptions.Center;
            _resultInfo.enableWordWrapping = true;
            _resultInfo.raycastTarget = false;

            var rewardsGo = new GameObject("ResultRewards");
            rewardsGo.transform.SetParent(go.transform, false);
            rewardsGo.AddComponent<RectTransform>();
            var rewardsLe = rewardsGo.AddComponent<LayoutElement>();
            rewardsLe.preferredHeight = 40f;
            _resultRewards = rewardsGo.AddComponent<TextMeshProUGUI>();
            _resultRewards.fontSize = 30f;
            _resultRewards.color = ColorPalette.Gold;
            _resultRewards.alignment = TextAlignmentOptions.Center;
            _resultRewards.raycastTarget = false;

            var btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(go.transform, false);
            btnRow.AddComponent<RectTransform>();
            var btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 120f;
            var btnRowLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 10f;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            _resultHomeButton = CreateResultButton("\ud648", btnRow.transform, ColorPalette.ButtonSecondary, OnResultHome);
            _resultContinueButton = CreateResultButton("\ub2e4\uc74c \ucc55\ud130", btnRow.transform, ColorPalette.ButtonPrimary, OnResultContinue);
            CreateResultButton("\ubcf4\ubb3c\uc0c1\uc790", btnRow.transform, ColorPalette.ButtonSecondary, () => UI.ShowScreen(ScreenType.ChapterTreasure));

            _resultPanel.gameObject.SetActive(false);
        }

        private Button CreateResultButton(string text, Transform parent, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject($"Btn_{text}");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var bgImg = go.AddComponent<Image>();
            bgImg.color = bgColor;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();

            if (textRt == null) textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return btn;
        }
    }
}
