# Planning-Implementation Consistency Report v2

Date: 2026-03-07
Author: QA Agent

## Scope

이번 보고서는 v1에서 미검증된 시스템들을 중심으로 검증합니다.

| Target | Planning Doc | Code/Data |
|--------|-------------|-----------|
| 재화 시스템 | 07_재화시스템.md | resource-labels.data.json, GameEnums.cs (ResourceType), Resources.cs |
| 퀘스트 시스템 | 07_재화시스템.md (퀘스트 보상 섹션) | quest.data.json, QuestDataTable.cs |
| 출석 시스템 | 07_재화시스템.md (7일 출석 섹션) | attendance.data.json, AttendanceSystem.cs, AttendanceDataTable.cs |
| 펫 시스템 | 09_펫시스템.md | pet.data.json, PetTable.cs, Pet.cs, PetManager.cs |
| 모험 시스템 | 12_모험시스템.md | Chapter.cs, EncounterGenerator.cs, EncounterDataTable.cs, EnemyTemplate.cs |
| 챕터 보물상자 | 12_모험시스템.md (챕터 보물상자 섹션) | chapter-treasure.data.json, ChapterTreasureTable.cs, ChapterTreasure.cs |
| 전승 시스템 | 02_캐릭터성장시스템.md | heritage.data.json, HeritageTable.cs, Heritage.cs |

---

## 1. Issues Found

### [BUG-005] 일일 퀘스트 2개 누락 (Major)

- **Severity**: Major
- **Planning**: `07_재화시스템.md` 일일 퀘스트 5개 정의
  - 챕터 3회 클리어 → 보석 30
  - 던전 3회 완료 → 골드 450
  - 탑 2회 도전 → 장비석 3
  - **아레나 1회 전투 → 보석 20** (누락)
  - **여행 5회 → 골드 350** (누락)
- **Implementation**: `quest.data.json`에 3개만 존재 (daily_chapter, daily_dungeon, daily_tower)
- **Root Cause**: 아레나(PvP)와 탐방(Travel) 기능이 미구현 상태이므로, 해당 퀘스트도 데이터에서 빠진 것으로 추정
- **Impact**: 기획서와 데이터 불일치. 미구현 기능과 연동된 퀘스트이므로, 기획서에 "미구현" 표기가 필요하거나 향후 구현 시 데이터 추가 필요
- **Action**: Planning Agent에 보고 — 기획서에 미구현 표기 추가 또는 향후 구현 시 quest.data.json에 추가

### [BUG-006] 재화 타입 3종 미등록 (Minor)

- **Severity**: Minor
- **Planning**: `07_재화시스템.md`에 정의된 재화
  - 입장권 (PvP 경기장 진입)
  - 행운 코인 (30일 챕터 행운 머신)
  - 보물상자 키 (가챠 재화)
- **Implementation**: `GameEnums.cs` ResourceType enum에 해당 타입 없음
  - ARENA_TOKEN (입장권) — 없음
  - LUCKY_COIN (행운 코인) — 없음
  - CHEST_KEY (보물상자 키) — 없음
- **Root Cause**: PvP, 30일 챕터, 보물상자 키 시스템이 미구현
- **Impact**: 미구현 기능의 재화이므로 현재 게임 동작에는 영향 없음
- **Action**: 미구현 목록에 추가. 기획서에 미구현 표기 필요

### [BUG-007] 몬스터 일수 스케일링 공식 불일치 (Major)

- **Severity**: Major
- **Planning**: `12_모험시스템.md:141`
  ```
  일수 배율 = 1 + (currentDay / totalDays) * dayProgressMaxBonus
  ```
  - `BattleDataTable.enemy.dayProgressMaxBonus` 참조라고 명시
  - 개발일지(`개발일지_yongsuc2.md:289`)에서 `dayProgressMaxBonus` 0.8→0.3으로 조정 기록 존재
- **Implementation**: `EnemyTemplate.cs:73`
  ```csharp
  float dayBonus = 1 + dayProgress;
  ```
  - `dayProgress = CurrentDay / TotalDays` (Chapter.cs:381)
  - `dayProgressMaxBonus` 파라미터를 사용하지 않음
  - `battle.data.json`의 `enemy` 객체에 `dayProgressMaxBonus` 필드 없음
  - `BattleDataTable.cs`의 `EnemyScalingConfig`에 `dayProgressMaxBonus` 프로퍼티 없음
- **Impact**: 60일차에서 일수 배율이 기획 의도(최대 1.3 = 1+0.3)가 아닌 2.0(= 1+1.0)이 됨
  - 몬스터가 기획 의도보다 **약 54% 더 강해짐** (2.0 / 1.3 = 1.54)
  - 챕터 후반부 난이도가 기획보다 과도하게 높아지는 밸런스 문제 발생
- **Action**: Dev Agent에 보고
  1. `battle.data.json`의 `enemy`에 `"dayProgressMaxBonus": 0.3` 추가
  2. `BattleDataTable.cs`의 `EnemyScalingConfig`에 `DayProgressMaxBonus` 프로퍼티 추가
  3. `EnemyTemplate.cs:73`을 `float dayBonus = 1 + dayProgress * BattleDataTable.Data.Enemy.DayProgressMaxBonus;`로 수정

### [BUG-008] 챕터 보물상자 보석 보상 — 기획서 "x id" vs 코드 고정값 (Minor)

- **Severity**: Minor
- **Planning**: `12_모험시스템.md` 보물상자 보상 테이블
  | 마일스톤 | 골드 | 보석 | 장비석 | 파워스톤 |
  |---------|------|------|--------|---------|
  | 15일 | 150*id | 10 | 1 | 0 |
  | 25일 | 250*id | 25 | 3 | 0 |
  | 40일 | 400*id | 40 | 5 | 1 |
  | 클리어 | 600*id | 60 | 8 | 2 |
- **Implementation**: `ChapterTreasureTable.cs:117-123` BuildReward 메서드
  ```csharp
  if (gold > 0) resources.Add(new ResourceReward(ResourceType.GOLD, gold * id));  // 골드만 id 곱셈
  if (gems > 0) resources.Add(new ResourceReward(ResourceType.GEMS, gems));       // 보석은 고정
  if (eqStone > 0) resources.Add(new ResourceReward(ResourceType.EQUIPMENT_STONE, eqStone)); // 장비석 고정
  if (pwStone > 0) resources.Add(new ResourceReward(ResourceType.POWER_STONE, pwStone));     // 파워스톤 고정
  ```
- **Analysis**: 기획서를 다시 보면, 골드만 "150*id" 형태이고 보석/장비석/파워스톤은 고정 수치로 보임. 코드와 일치함.
- **Result**: **PASS** — 기획서 표기가 "골드만 챕터 ID 비례 스케일링"이고, 코드도 동일하게 구현됨

---

## 2. Verified Items (PASS)

### 재화 시스템

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| ResourceType 13종 | 07_재화시스템.md | GameEnums.cs: 13개 enum (GOLD~PET_FOOD) | PASS |
| 골드 라벨 "골드" | 07_재화시스템.md | resource-labels.data.json: "GOLD": "골드" | PASS |
| 보석 라벨 "보석" | 07_재화시스템.md | resource-labels.data.json: "GEMS": "보석" | PASS |
| 스태미나 최대 100 | 07_재화시스템.md | Resources.cs: STAMINA_MAX = 100 | PASS |
| 스태미나 분당 1 회복 | 07_재화시스템.md | Resources.cs: STAMINA_REGEN_PER_MINUTE = 1 | PASS |
| 일일 리셋: 도전권 5 | 07_재화시스템.md | battle.data.json: dailyReset.challengeToken = 5 | PASS |
| 일일 리셋: 곡괭이 10 | 07_재화시스템.md | battle.data.json: dailyReset.pickaxe = 10 | PASS |
| 전용 책 4종 매핑 | 07_재화시스템.md | heritage.data.json: bookTypeMap (SKULL→SKULL_BOOK 등) | PASS |

### 출석 시스템

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| 1일: 보석 50 | 07_재화시스템.md | attendance.data.json: day 1, GEMS 50 | PASS |
| 2일: 골드 3000 | 07_재화시스템.md | attendance.data.json: day 2, GOLD 3000 | PASS |
| 3일: 골드 장비뽑기 1회 | 07_재화시스템.md | attendance.data.json: day 3, EQUIPMENT_GACHA | PASS |
| 4일: 장비석 5 | 07_재화시스템.md | attendance.data.json: day 4, EQUIPMENT_STONE 5 | PASS |
| 5일: 펫 알 2 + 사료 10 | 07_재화시스템.md | attendance.data.json: day 5, PET_EGG 2, PET_FOOD 10 | PASS |
| 6일: 에픽 펫 (랜덤) | 07_재화시스템.md | attendance.data.json: day 6, PET, petGrade EPIC | PASS |
| 7일: 보석 200 | 07_재화시스템.md | attendance.data.json: day 7, GEMS 200 | PASS |
| 7일 주기 자동 리셋 | 07_재화시스템.md | AttendanceSystem.cs: IsComplete() + ResetCycle() | PASS |
| 하루 1회 제한 | 07_재화시스템.md | AttendanceSystem.cs: CanCheckIn() 날짜 체크 | PASS |

### 퀘스트 시스템 (구현된 항목만)

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| 일일: 챕터 3회 → 보석 30 | 07_재화시스템.md | quest.data.json: daily_chapter | PASS |
| 일일: 던전 3회 → 골드 450 | 07_재화시스템.md | quest.data.json: daily_dungeon | PASS |
| 일일: 탑 2회 → 장비석 3 | 07_재화시스템.md | quest.data.json: daily_tower | PASS |
| 주간: 챕터 15회 → 보석 200 | 07_재화시스템.md | quest.data.json: weekly_chapter | PASS |
| 주간: 장비 뽑기 20회 → 장비석 10 | 07_재화시스템.md | quest.data.json: weekly_gacha | PASS |
| 주간: 장비 판매 10회 → 골드 500 | 07_재화시스템.md | quest.data.json: weekly_sell | PASS |
| 주간: 탑 10회 → 도면 3 | 07_재화시스템.md | quest.data.json: weekly_tower | PASS |

### 펫 시스템

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| S티어 9종 | 09_펫시스템.md | pet.data.json: 9 templates with tier "S" | PASS |
| A티어 6종 | 09_펫시스템.md | pet.data.json: 6 templates with tier "A" | PASS |
| B티어 3종 | 09_펫시스템.md | pet.data.json: 3 templates with tier "B" | PASS |
| 등급 5단계 | 09_펫시스템.md | GameEnums.cs PetGrade: COMMON~IMMORTAL (5개) | PASS |
| 등급 배율 | 09_펫시스템.md | pet.data.json gradeMultipliers: 1/1.5/2/3/5 | PASS |
| 부화시 항상 COMMON | 09_펫시스템.md | PetManager.cs:35: PetGrade.COMMON | PASS |
| 가중치 기반 랜덤 | 09_펫시스템.md | PetTable.cs:124-127: WeightedPick | PASS |
| S티어 weight 2~3 | 09_펫시스템.md: 낮은 가중치 | pet.data.json: S tier weight 2-3 | PASS |
| A티어 weight 8 | 09_펫시스템.md: 중간 가중치 | pet.data.json: A tier weight 8 | PASS |
| B티어 weight 15 | 09_펫시스템.md: 높은 가중치 | pet.data.json: B tier weight 15 | PASS |
| 미사용 펫 패시브 보너스 10% | 09_펫시스템.md | pet.data.json: inactiveBonusRate 0.1 | PASS |

#### 펫 패시브 타입 매칭 (18종 전수 검증)

| Pet | Planning Passive | Data passiveType | Result |
|-----|-----------------|-----------------|--------|
| Elsa | 방어막 | SHIELD_ON_START | PASS |
| Piggy | 흡혈 | LIFESTEAL | PASS |
| Freya | 공격력 강화 | STAT_MODIFIER (ATK) | PASS |
| Slime King | 방어력 강화 | STAT_MODIFIER (DEF) | PASS |
| Flash | 연타 | MULTI_HIT | PASS |
| Unicorn | 재생 | REGEN | PASS |
| Ice Wind Fox | 반격 | COUNTER | PASS |
| Little Elle | 부활 | REVIVE | PASS |
| Cleopatra | 치명타 강화 | STAT_MODIFIER (CRIT) | PASS |
| Purple Demon Fox | 흡혈 | LIFESTEAL | PASS |
| Baby Dragon | 공격력 강화 | STAT_MODIFIER (ATK) | PASS |
| Monopoly | 방어막 | SHIELD_ON_START | PASS |
| Glazed Shroom | 재생 | REGEN | PASS |
| Flame Fox | 연타 | MULTI_HIT | PASS |
| Cactus Fighter | 반격 | COUNTER | PASS |
| Brown Bunny | 공격력 강화 | STAT_MODIFIER (ATK) | PASS |
| Blue Bird | 재생 | REGEN | PASS |
| Green Frog | 방어력 강화 | STAT_MODIFIER (DEF) | PASS |

#### 펫 최대 등급 매칭

| Pet | Planning maxGrade | Data maxGrade | Result |
|-----|------------------|---------------|--------|
| Elsa | 불멸 | IMMORTAL | PASS |
| Piggy | 불멸 | IMMORTAL | PASS |
| Freya | 불멸 | IMMORTAL | PASS |
| Slime King | 불멸 | IMMORTAL | PASS |
| Flash | 불멸 | IMMORTAL | PASS |
| Unicorn | 불멸 | IMMORTAL | PASS |
| Ice Wind Fox | 전설 | LEGENDARY | PASS |
| Little Elle | 전설 | LEGENDARY | PASS |
| Cleopatra | 불멸 | IMMORTAL | PASS |
| Purple Demon Fox | 전설 | LEGENDARY | PASS |
| Baby Dragon | 전설 | LEGENDARY | PASS |
| Monopoly | 전설 | LEGENDARY | PASS |
| Glazed Shroom | 전설 | LEGENDARY | PASS |
| Flame Fox | 전설 | LEGENDARY | PASS |
| Cactus Fighter | 전설 | LEGENDARY | PASS |
| Brown Bunny | 전설 | LEGENDARY | PASS |
| Blue Bird | 전설 | LEGENDARY | PASS |
| Green Frog | 전설 | LEGENDARY | PASS |

### 모험 시스템

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| 챕터 스태미나 5 소모 | 12_모험시스템.md | GameManager.cs:123: staminaCost = 5 | PASS |
| 60일 구성 | 12_모험시스템.md | Chapter.cs:57: TotalDays = 60 | PASS |
| 강제 전투: 20일 엘리트 | 12_모험시스템.md | encounter.data.json: elite = 20 | PASS |
| 강제 전투: 30일 중간보스 | 12_모험시스템.md | encounter.data.json: midBoss = 30 | PASS |
| 선택적 엘리트: 40일 30% | 12_모험시스템.md | encounter.data.json: day 40, chance 0.3 | PASS |
| 선택적 엘리트: 50일 30% | 12_모험시스템.md | encounter.data.json: day 50, chance 0.3 | PASS |
| 60일 최종 보스 | 12_모험시스템.md | Chapter.cs:366: IsBossDay() = CurrentDay >= TotalDays | PASS |
| 인카운터 가중치 40/7/53 | 12_모험시스템.md | encounter.data.json weights: COMBAT 40, DEMON 7, CHANCE 53 | PASS |
| 악마: 체력 20% 소모 | 12_모험시스템.md | encounter.data.json: hpCostPercent = 0.2 | PASS |
| 우연: 치유의 샘 15% 회복 | 12_모험시스템.md | encounter.data.json: springHealPercent = 0.15 | PASS |
| 리롤 세션당 2회 | 12_모험시스템.md | encounter.data.json: rerollsPerSession = 2 | PASS |
| 대박 카운터 임계값 7 | 12_모험시스템.md | encounter.data.json: daebak = 7 | PASS |
| 중박 카운터 임계값 12 | 12_모험시스템.md | encounter.data.json: jungbak = 12 | PASS |
| 보스 순환 3패턴 | 12_모험시스템.md | EnemyTable.cs:180: themeLocalIndex % BossRotation.Count | PASS |
| 챕터 스케일링 1.3x | 12_모험시스템.md | battle.data.json: scalingPerChapter = 1.3 | PASS |
| 최고 생존 일수 기록 | 12_모험시스템.md | Player.cs:278-288: UpdateBestSurvivalDay | PASS |
| 클리어 sentinel = totalDays+1 | 12_모험시스템.md | ChapterTreasureTable.cs:84: GetClearSentinelDay = totalDays+1 | PASS |

### 챕터 보물상자

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| 마일스톤 15/25/40일 | 12_모험시스템.md | chapter-treasure.data.json: 3 survival milestones | PASS |
| 클리어 마일스톤 | 12_모험시스템.md | chapter-treasure.data.json: clearMilestone | PASS |
| 15일: 골드 150*id | 12_모험시스템.md | chapter-treasure.data.json: gold=150, BuildReward gold*id | PASS |
| 25일: 골드 250*id | 12_모험시스템.md | chapter-treasure.data.json: gold=250 | PASS |
| 40일: 골드 400*id | 12_모험시스템.md | chapter-treasure.data.json: gold=400 | PASS |
| 클리어: 골드 600*id | 12_모험시스템.md | chapter-treasure.data.json: gold=600 | PASS |
| 1회 수령 제한 | 12_모험시스템.md | ChapterTreasure.cs:11: ClaimedMilestones 체크 | PASS |

### 전승(Heritage) 시스템

| Item | Planning | Implementation | Result |
|------|----------|---------------|--------|
| 4종 루트 (해골/기사/레인저/유령) | 02_캐릭터성장시스템.md | GameEnums.cs: HeritageRoute 4종 | PASS |
| 전용 책 매핑 | 07_재화시스템.md | heritage.data.json: bookTypeMap | PASS |
| 해골: ATK+3/레벨 | heritage.data.json | passivePerLevel.SKULL.atk = 3 | PASS |
| 기사: DEF+2, HP+10/레벨 | heritage.data.json | passivePerLevel.KNIGHT.def=2, maxHp=10 | PASS |
| 레인저: ATK+2, CRIT+0.005/레벨 | heritage.data.json | passivePerLevel.RANGER.atk=2, crit=0.005 | PASS |
| 유령: ATK+1, HP+5/레벨 | heritage.data.json | passivePerLevel.GHOST.atk=1, maxHp=5 | PASS |
| HERO 등급에서 해금 | 02_캐릭터성장시스템.md | Heritage.cs:20: IsUnlocked = HERO | PASS |

---

## 3. Updated Unimplemented Features List

v1의 미구현 목록을 갱신합니다.

| Feature | Planning Doc | Status | Related Missing Items |
|---------|-------------|--------|----------------------|
| 30일 챕터 (행운 상인/코인 머신) | 00_게임개요.md | 미구현 | LUCKY_COIN 재화, ChapterType 미정의 |
| 5일 챕터 (웨이브 전투) | 00_게임개요.md | 미구현 | ChapterType 미정의 |
| PvP 경기장 | 08_PvP시스템.md | 미구현 | ARENA_TOKEN 재화, daily_arena 퀘스트 |
| 탐방(Travel) | 12_모험시스템.md | 미구현 | daily_travel 퀘스트 |
| 보물상자 키 재화 | 07_재화시스템.md | 미구현 | CHEST_KEY 재화 |
| 봉인 전투 (협동 보스) | 05_스테이지던전시스템.md | 미구현 (기획서에 "미래" 명시) | - |
| 이벤트 시스템 | 10_이벤트시스템.md | 코드 존재 (EventManager), 기획 세부 미검증 | - |
| 과금 시스템 | 11_과금시스템.md | 기획 존재, 구현 미검증 | - |

---

## 4. New Test Cases

### [TC-007] 일수 스케일링 밸런스 검증
- **Precondition**: 챕터 1, 59일차 도달
- **Input**: 60일차 진입 (dayProgress = 59/60 ≈ 0.983)
- **Expected (기획)**: 일수 배율 = 1 + 0.983 * 0.3 = 1.295
- **Actual (코드)**: 일수 배율 = 1 + 0.983 = 1.983
- **Reference**: 12_모험시스템.md, EnemyTemplate.cs:73
- **Severity**: Critical (BUG-007 관련, 밸런스에 직접 영향)

### [TC-008] 펫 부화 가중치 분포
- **Precondition**: 펫 알 100개 보유
- **Input**: 100회 부화 실행
- **Expected**: B티어 약 60% (weight 45/83), A티어 약 29% (weight 48/83), S티어 약 11% (weight 20/83 중 weight 3이 2개, weight 2가 7개 → 20)
  - 실제 가중치 합: S=2*7+3*2=20, A=8*6=48, B=15*3=45, 전체=113
  - S=17.7%, A=42.5%, B=39.8%
- **Reference**: 09_펫시스템.md, pet.data.json
- **Severity**: Major (밸런스 검증)

### [TC-009] 출석 7일 완료 후 자동 리셋
- **Precondition**: 출석 7일 모두 완료
- **Input**: 다음 일일 리셋 발생
- **Expected**: 출석 카운트 초기화, 1일차부터 다시 시작
- **Reference**: 07_재화시스템.md, AttendanceSystem.cs
- **Severity**: Major

### [TC-010] 미구현 퀘스트로 인한 일일 미션 완료 불가 검증
- **Precondition**: 모든 구현된 일일 퀘스트 완료
- **Input**: 일일 퀘스트 목록 확인
- **Expected**: 3개 퀘스트만 존재 (아레나, 여행 미표시)
- **Reference**: 07_재화시스템.md, quest.data.json
- **Severity**: Minor (미구현 기능이므로)

### [TC-011] 챕터 보물상자 클리어 마일스톤 수령
- **Precondition**: 챕터 5 클리어, BestSurvivalDays[5] = 61 (sentinel)
- **Input**: "ch5_clear" 마일스톤 수령 요청
- **Expected**: 골드 3000 (600*5), 보석 60, 장비석 8, 파워스톤 2 지급
- **Reference**: 12_모험시스템.md, ChapterTreasureTable.cs
- **Severity**: Major

### [TC-012] 펫 등급 승급 시 동일 펫 필요 확인
- **Precondition**: COMMON Elsa 1마리 보유, 중복 펫 없음
- **Input**: 등급 승급 시도
- **Expected**: "No duplicate pets available" 실패
- **Reference**: 09_펫시스템.md, PetManager.cs:48-50
- **Severity**: Major

---

## 5. Summary

### v2에서 새로 발견된 이슈

| ID | Severity | System | Description |
|----|----------|--------|-------------|
| BUG-005 | Major | 퀘스트 | 일일 퀘스트 2개 누락 (아레나, 여행) |
| BUG-006 | Minor | 재화 | ResourceType 3종 미등록 (미구현 기능) |
| BUG-007 | Major | 모험/전투 | dayProgressMaxBonus 미적용 (몬스터 54% 더 강함) |

### 전체 이슈 현황 (v1 + v2)

| ID | Severity | Status | Action |
|----|----------|--------|--------|
| BUG-001 | Major | Open | pools 폴백은 챕터 51+ 에서만 발생. Dev Agent에서 정리 중 (Task #10) |
| BUG-002 | Minor | Open | baseStats 폴백 용도 확인 필요 |
| BUG-003 | Major | Open | 기획서 미구현 표기 필요 → Planning Agent |
| BUG-004 | Minor | Open | 기획서 파일 참조 오류 → Planning Agent |
| BUG-005 | Major | New | 미구현 기능 연동 퀘스트 누락 → Planning Agent |
| BUG-006 | Minor | New | 미구현 재화 타입 → 미구현 목록 관리 |
| BUG-007 | Major | New | dayProgressMaxBonus 미적용 → Dev Agent |

### 검증 커버리지

| System | Planning Doc | v1 검증 | v2 검증 | Status |
|--------|-------------|---------|---------|--------|
| 전투 시스템 | 01_전투시스템.md | O | - | 완료 |
| 캐릭터 성장 | 02_캐릭터성장시스템.md | O | - | 완료 |
| 장비 시스템 | 03_장비시스템.md | O | - | 완료 |
| 스킬 시스템 | 04_스킬시스템.md | O | - | 완료 |
| 스테이지/던전 | 05_스테이지던전시스템.md | O | - | 완료 |
| 가챠 시스템 | 06_가챠시스템.md | O | - | 완료 |
| 재화 시스템 | 07_재화시스템.md | - | O | 완료 |
| PvP 시스템 | 08_PvP시스템.md | - | - | 미구현 |
| 펫 시스템 | 09_펫시스템.md | - | O | 완료 |
| 이벤트 시스템 | 10_이벤트시스템.md | - | - | 미검증 |
| 과금 시스템 | 11_과금시스템.md | - | - | 미검증 |
| 모험 시스템 | 12_모험시스템.md | - | O | 완료 |

### 우선 조치 사항

1. **BUG-007 (Critical)**: dayProgressMaxBonus 미적용 — 밸런스에 직접 영향. Dev Agent에 즉시 수정 요청
2. **BUG-005**: 미구현 기능 연동 퀘스트 — Planning Agent에 기획서 정리 요청
3. **BUG-003 + BUG-004**: 기획서 미갱신/오류 — Planning Agent에 수정 요청
