# Planning-Implementation Consistency Report v1

Date: 2026-03-07
Author: QA Agent

## Scope

| Target | Planning Doc | Code/Data |
|--------|-------------|-----------|
| Battle System | 01_전투시스템.md | battle.data.json, enemy.data.json, ActiveSkillRegistry.cs, PassiveSkillRegistry.cs, SkillValidator.cs |
| Skill System | 04_스킬시스템.md | active-skill-tier.data.json, passive-skill-tier.data.json, ActiveSkillRegistry.cs, PassiveSkillRegistry.cs |
| Equipment System | 03_장비시스템.md | equipment-base-stats.data.json, equipment-constants.data.json, equipment-substats.data.json, equipment-labels.data.json |
| Character Growth | 02_캐릭터성장시스템.md | talent.data.json |
| Stage/Dungeon | 05_스테이지던전시스템.md | dungeon.data.json, encounter.data.json |
| Gacha System | 06_가챠시스템.md | gacha.data.json |
| Data Sync | - | Data/Json/ vs Resources/_Project/Data/Json/ |

---

## 1. Data Sync Verification (Data/Json vs Resources)

**Result: PASS**

All 18 JSON files are identical between Assets/_Project/Data/Json/ and Assets/Resources/_Project/Data/Json/.

Verified files: battle, enemy, equipment-base-stats, equipment-substats, active-skill-tier, passive-skill-tier, gacha, dungeon, encounter, talent, pet, quest, equipment-constants, equipment-labels, attendance, chapter-treasure, heritage, resource-labels

---

## 2. Issues Found

### [BUG-001] enemy.data.json pools field hardcoded to Theme 1 (Major)

- **Severity**: Major
- **Location**: Assets/_Project/Data/Json/enemy.data.json:119-123
- **Planning**: Enemy pools should be determined dynamically by chapterThemes chapter range
- **Data**: pools field is hardcoded to Theme 1 (Microbes) enemy IDs only
- **Impact**: If any code references pools instead of chapterThemes, chapters 11+ would only spawn Theme 1 enemies
- **Action**: Verify if pools field is used in code. If unused, remove to prevent confusion.

### [BUG-002] enemy.data.json baseStats field purpose unclear (Minor)

- **Severity**: Minor
- **Location**: Assets/_Project/Data/Json/enemy.data.json:124-128
- **Data**: baseStats contains average stats per enemy type, but each enemy has individual stats
- **Impact**: If code uses baseStats for calculations, individual enemy stats would be ignored
- **Action**: Verify baseStats usage in code

### [BUG-003] Chapter type description mismatch between planning docs (Major)

- **Severity**: Major
- **Planning**: 00_게임개요.md describes 3 chapter types: 60-day, 30-day, 5-day
  - 30-day: Lucky Merchant + Lucky Coin Machine
  - 5-day: Wave combat, cumulative turn system
- **Implementation**: 05_스테이지던전시스템.md states all chapters are 60-day
- **Data**: encounter.data.json only has SIXTY_DAY in weights
- **Conclusion**: 30-day/5-day chapters are NOT implemented. Game overview doc is misleading.
- **Action**: Update game overview doc to reflect current state, or add to unimplemented list

### [BUG-004] Gacha planning doc references non-existent files (Minor)

- **Severity**: Minor
- **Location**: docs/planning-agent/SystemDesign/06_가챠시스템.md bottom table
- **Issue 1**: Equipment grade labels/sell prices references equipment-base-stats.data.json but actual location is equipment-labels.data.json
- **Issue 2**: Equipment passive references equipment-passive.data.json but this file does not exist
- **Action**: Fix file references in planning doc

---

## 3. Verified Items (PASS)

### Battle System

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| Max Rage 100 | 01_전투시스템.md | battle.data.json: rage.maxRage = 100 | PASS |
| Crit Multiplier 1.5 | 01_전투시스템.md | battle.data.json: damage.critMultiplier = 1.5 | PASS |
| Max Turns 100 | 01_전투시스템.md | battle.data.json: maxTurns = 100 | PASS |
| Dual Spawn Chance 50% | 01_전투시스템.md | battle.data.json: enemy.dualSpawnChance = 0.5 | PASS |
| Dual Stat Multiplier 0.7 | 01_전투시스템.md | battle.data.json: enemy.dualStatMultiplier = 0.7 | PASS |
| Counter Coefficient 1.0 | 01_전투시스템.md | battle.data.json: damage.counterCoefficient = 1.0 | PASS |

### Enemy Skill Mapping (50 enemies + 3 dungeon enemies)

All 53 enemies skill compositions match the planning document 01_전투시스템.md.

Mapping rules verified:
- Rage = ilban_attack + bunno_attack + rage_accumulate (3 builtins)
- Poison Inject = venom_sword + poison_inject (upper + lowest pair)
- Flame Summon = bunno_flame + flame_summon (rage attack chain)
- Lightning Summon = bunno_thunder + lightning_summon (rage attack chain)
- Regen = regen, Shield = iron_shield, Counter = counter
- Multi Hit = multi_hit, Lifesteal = lifesteal

### Skill Tier Data

| Item | Verified | Result |
|------|----------|--------|
| 4-tier system | All skill families have tier 1-4 data (except angel_power, demon_power = tier 4 only) | PASS |
| Skill hierarchy | ActiveSkillRegistry.cs Hierarchy matches BUILTIN/UPPER/LOWER/LOWEST | PASS |
| SkillValidator rules | 5 validation rules match planning doc | PASS |
| MAX_SKILL_CHAIN_DEPTH = 3 | Planning doc max chain depth = 3 | PASS |

### Equipment System

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| 7 slot types (Ring x2) | 03_장비시스템.md | equipment-base-stats + slotMaxCount.RING = 2 | PASS |
| 6 grade tiers | 03_장비시스템.md | equipment-constants: gradeOrder | PASS |
| Sub-stat count by grade (0-5) | 03_장비시스템.md | equipment-substats: substatCountByGrade | PASS |
| Sub-stat pools per slot | 03_장비시스템.md | equipment-substats: pools | PASS |
| Sell prices | 03_장비시스템.md | equipment-labels: sellPrices | PASS |
| Upgrade cost tiers | 03_장비시스템.md | equipment-constants: upgradeCostTiers | PASS |
| Merge rules (low 3, high 2) | 03_장비시스템.md | equipment-constants: mergeCount | PASS |
| mergeEnhanceMax = 2 | 03_장비시스템.md | equipment-constants | PASS |
| Promote levels 10/20/30 | 03_장비시스템.md | equipment-constants: promoteLevels | PASS |
| 3 weapon subtypes | 03_장비시스템.md | equipment-labels: weaponSubTypeLabels | PASS |

### Character Growth System

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| Sub-grade count (2+5+10+17+33=67) | 02_캐릭터성장시스템.md | talent.data.json: gradeConfig | PASS |
| Levels per stat/tier | levelsPerStat=10, levelsPerTier=30 | talent.data.json | PASS |
| Stat per level (HP:15, ATK:3, DEF:2) | 02_캐릭터성장시스템.md | talent.data.json | PASS |
| Milestone interval 10 levels | 02_캐릭터성장시스템.md | talent.data.json: milestoneConfig.interval = 10 | PASS |
| Main grade bonuses (5) | 02_캐릭터성장시스템.md | talent.data.json: mainGradeBonuses | PASS |

### Dungeon System

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| Daily limit 3 | 05_스테이지던전시스템.md | dungeon.data.json: dailyLimit = 3 | PASS |
| Giant Beehive enemy | 05_스테이지던전시스템.md | dungeon.data.json: GIANT_BEEHIVE.enemyId = dungeon_queen_bee | PASS |
| Ancient Tree enemy | 05_스테이지던전시스템.md | dungeon.data.json: ANCIENT_TREE.enemyId = dungeon_old_tree | PASS |
| Tiger Cliff enemy | 05_스테이지던전시스템.md | dungeon.data.json: TIGER_CLIFF.enemyId = dungeon_tiger | PASS |
| Reward types | Planning doc per dungeon | dungeon.data.json: baseRewards | PASS |

---

## 4. Unimplemented Features

| Feature | Planning Doc | Status |
|---------|-------------|--------|
| 30-day Chapter (Lucky Merchant/Coin Machine) | 00_게임개요.md | Not implemented (no data/code) |
| 5-day Chapter (Wave Combat) | 00_게임개요.md | Not implemented (no data/code) |
| Sealed Battle (Co-op Boss) | 05_스테이지던전시스템.md | Not implemented (doc says future) |
| PvP System | 08_PvP시스템.md | Planning doc exists, implementation unverified |
| Event System | 10_이벤트시스템.md | Planning doc exists, implementation unverified |
| Monetization System | 11_과금시스템.md | Planning doc exists, implementation unverified |

---

## 5. Proposed Test Cases

### [TC-001] Chapter Enemy Pool Consistency
- **Precondition**: Enter chapter 11+
- **Input**: Combat encounter at chapter 15
- **Expected**: Only Theme 2 (Insect) enemies appear
- **Reference**: 01_전투시스템.md, enemy.data.json chapterThemes
- **Severity**: Critical (BUG-001 related)

### [TC-002] Enemy Skill Activation
- **Precondition**: Battle start
- **Input**: Battle with bacteria enemy
- **Expected**: Bacteria activates poison inject (venom_sword -> poison_inject) on normal attack
- **Reference**: 01_전투시스템.md, 04_스킬시스템.md
- **Severity**: Major

### [TC-003] Dual Enemy Stat Multiplier
- **Precondition**: Normal combat encounter with 2 enemies
- **Input**: Check 2 enemies stats
- **Expected**: Each enemy stats at 0.7x of original
- **Reference**: 01_전투시스템.md
- **Severity**: Major

### [TC-004] Equipment Merge Weapon Type Guard
- **Precondition**: Epic Sword + Epic Staff merge attempt
- **Input**: Execute merge
- **Expected**: Merge rejected (different weapon subtypes)
- **Reference**: 03_장비시스템.md
- **Severity**: Major

### [TC-005] Skill Tier Upgrade Replacement
- **Precondition**: Have Thunder Shuriken tier 1
- **Input**: Acquire same skill (Thunder Shuriken) again
- **Expected**: Replaced with tier 2 (1 slot maintained), tier 1 removed
- **Reference**: 04_스킬시스템.md
- **Severity**: Major

### [TC-006] Talent Sub-Grade Promotion
- **Precondition**: Disciple Grade 1, ATK/HP/DEF all at level 10
- **Input**: Sub-grade promotion
- **Expected**: Promote to Disciple Grade 2, all 3 stats reset to 0, sub-grade bonus applied
- **Reference**: 02_캐릭터성장시스템.md
- **Severity**: Major

---

## 6. Summary

- **Data Sync**: Good (Data/Json and Resources fully synced)
- **Planning-Data Consistency**: Mostly good (4 issues found)
- **Planning-Code Consistency**: Good (skill registry and validator follow planning docs)
- **Unimplemented Items**: 30-day/5-day chapters and others (planning docs need cleanup)

### Recommended Priority Actions

1. **BUG-001**: Verify pools field usage in code -> Request to Dev Agent
2. **BUG-003**: Update game overview doc for unimplemented chapter types -> Report to Planning Agent
3. **BUG-004**: Fix gacha doc file references -> Report to Planning Agent
