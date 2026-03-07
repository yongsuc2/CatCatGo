# Planning Agent 작업일지

## 2026-03-08 트러블슈팅 문서 3건 작성

### 수행 작업
- 최근 수정된 버그 3건에 대한 트러블슈팅 문서를 `/doc` skill 포맷으로 작성
- 수정한 Agent의 문서 디렉토리에 배치 (버그 1, 3은 dev-agent, 버그 2는 dev-server-agent)

### 주요 변경사항
- `docs/dev-agent/TroubleShooting/골드_비동기화_INSUFFICIENT_GOLD.md` (신규)
  - 커밋 220d9e9d: SyncApi 스냅샷의 시간차로 인한 재화 잔고 불일치 → ResourceApi + SyncResourceBalances 추가
- `docs/dev-server-agent/TroubleShooting/재능_milestone_key_불일치.md` (신규)
  - 커밋 e8e98817: 서버 delta key "10" vs 클라이언트 "LV_10" 형식 불일치 → 서버에서 $"LV_{level}" 형식으로 통일
- `docs/dev-agent/TroubleShooting/JSON_데이터_파일_truncation.md` (신규)
  - 커밋 6455da43: battle/dungeon/pet JSON 파일의 닫는 중괄호 누락 → 6개 파일 복원

### 이슈/메모
- dev-server-agent/TroubleShooting 디렉토리가 미존재하여 신규 생성

---

## 2026-03-07 밸런스 수치 하드코딩 제거 (데이터 테이블 참조 교체)

### 작업 배경
기획 문서에 밸런스 관련 구체적 수치(데미지, 보상량, 확률, 배율, 스탯, 비용 등)가 직접 표기되어 있으면 데이터 테이블과의 이중 관리로 인한 불일치 위험이 발생한다. 모든 수치를 데이터 테이블 참조로 교체하여 단일 정보원(Single Source of Truth) 원칙을 확립한다.

### 수정 파일 및 교체 내용

#### 1. `docs/Planning/SystemDesign/00_게임개요.md`
- 탐방 배율 "3배 ~ 50배" -> 데이터 테이블 참조
- 챕터 진입 "스태미나 5개" -> "스태미나 소모 (소모량은 게임 설정 참조)"

#### 2. `docs/Planning/SystemDesign/02_캐릭터성장시스템.md`
- 등급 구조 수치 테이블(서브등급 수, 총 레벨, 시작/종료 레벨) -> `talent.data.json`의 `gradeConfig` 참조

#### 3. `docs/Planning/SystemDesign/03_장비시스템.md`
- 강화 비용 구간별 배율 테이블(1.15/1.25/1.35/1.45/1.55/1.65) -> `equipment-constants.data.json`의 `upgradeCostTiers` 참조
- 장비 판매가 테이블(10/30/100/300/1000/3000) -> `equipment-labels.data.json`의 `sellPrices` 참조

#### 4. `docs/Planning/SystemDesign/04_스킬시스템.md`
- 티어 스케일링 비율 "1 : 2.3 : 4 : 6" -> 데이터 테이블 참조로 변경

#### 5. `docs/Planning/SystemDesign/05_스테이지던전시스템.md`
- 챕터 진입 "스태미나 5개" -> 게임 설정 참조
- 타워 규모 "100층, 각 층 10단계" -> 타워 데이터 참조
- 던전 일일 제한 "3회" -> `dungeon.data.json` 참조
- 고블린 광부 목표 "광석 30개" -> 게임 설정 참조

#### 6. `docs/Planning/SystemDesign/07_재화시스템.md`
- 스태미나 용도 "메인 챕터 진입(5개)" -> "메인 챕터 진입"
- 퀘스트 보상 테이블(수치 포함 5행+4행) -> `quest.data.json` 참조
- 7일 출석 보상 테이블(7행) -> `attendance.data.json` 참조
- 탐방 "50배속" -> "최대 배속"

#### 7. `docs/Planning/SystemDesign/10_이벤트시스템.md`
- 한정 가챠 "180회 천장" -> `gacha.data.json`의 `pityThreshold` 참조

#### 8. `docs/Planning/SystemDesign/11_과금시스템.md`
- "탐방 50배속" (3곳) -> "탐방 최대 배속"

#### 9. `docs/Planning/SystemDesign/12_모험시스템.md`
- 챕터 진입 "스태미나 5개" -> 게임 설정 참조
- 악마 HP 대가 "20%" -> `encounter.data.json`의 `demon.hpCostPercent` 참조
- 치유의 샘 회복 "15%" -> `encounter.data.json`의 `chance.springHealPercent` 참조
- 인카운터 가중치 "전투 40%, 악마 7%, 우연 53%" -> `encounter.data.json`의 `weights` 참조
- 전투 골드 보상 공식 수치 -> `battle.data.json`의 `combatGoldReward` 참조
- 챕터 클리어 보상 공식 수치 -> `encounter.data.json`의 `chapterClearReward` 참조
- 챕터 보물상자 보상 테이블(4행) -> `chapter-treasure.data.json` 참조
- 탐방 골드 공식/배율 옵션 수치 -> 게임 설정 데이터 참조

#### 10. `docs/Planning/SystemDesign/13_밸런스데이터시트.md` (전면 재작성)
- 기존: 모든 섹션에 구체적 수치가 직접 표기 (전투 계수, 적 스탯, 인카운터 가중치, 장비 스탯, 가챠 확률, 스킬 티어별 수치, 펫 스탯, 재능/전승 수치, 보물상자 보상, 출석 보상)
- 수정: 수치를 모두 제거하고 데이터 테이블 경로만 안내하는 색인 문서로 변환
- 장비 패시브 삭제 관련 밸런스 영향 분석 메모는 유지

#### 11. `docs/Planning/SystemDesign/화면기획/장비화면_기획서.md`
- 판매 가격표(6행) -> `equipment-labels.data.json`의 `sellPrices` 참조

### 수정하지 않은 항목 (판단 근거)
- **연출 타이밍** (350ms, 300ms, 600ms 등): 밸런스가 아닌 UI 연출 수치
- **색상 코드** (#ff5252 등): UI 스타일 수치
- **UI 와이어프레임 예시 수치** (282K, 53.4K 등): 레이아웃 설명용 더미 값
- **시스템 규칙** (100% 반환, 확률 캡 100%, HP 50% 이하 발동): 밸런스 조정 대상이 아닌 시스템 메카닉 규칙
- **구조 설명** (도전권 1장, 각 층 5전투, 서브등급당 30레벨): 시스템 구조 정의

### 검증 방법
모든 데이터 테이블 참조는 `Assets/_Project/Data/Json/` 하위에 실제 파일이 존재하고, 해당 필드가 존재함을 확인했다.

---

## 2026-03-07 기획 문서 버그 3건 수정 (BUG-005, BUG-006, BUG-007)

### 작업 내용
QA에서 보고된 기획 문서 관련 버그 3건을 수정했다.

### BUG-007: 기획서 수치 제거 (dayProgressMaxBonus)
- **파일**: `docs/Planning/SystemDesign/12_모험시스템.md`
- **문제**: 일수 기반 몬스터 스케일링 섹션에 `dayProgressMaxBonus` 공식과 구체적 수치가 하드코딩되어 있었음. 코드에는 해당 파라미터가 존재하지 않으며, 현재 코드(`1 + dayProgress`)가 의도된 동작임
- **수정**: 구체적 수치/공식(`1 + (currentDay / totalDays) * dayProgressMaxBonus`)을 제거하고, 동작 의도만 서술하도록 재작성. 스케일링 세부 수치는 `BattleDataTable`, `EnemyTemplate.cs` 코드 참조로 전환

### BUG-005: 일일 퀘스트 미구현 표기
- **파일**: `docs/Planning/SystemDesign/07_재화시스템.md`
- **문제**: 일일 퀘스트 5개 중 "아레나 1회 전투", "여행 5회"가 `quest.data.json`에 미구현 상태이나 기획서에 표기 없음
- **수정**: 해당 2개 퀘스트 항목에 **(미구현)** 표기 추가

### BUG-006: 재화 타입 미구현 표기
- **파일**: `docs/Planning/SystemDesign/07_재화시스템.md`
- **문제**: 입장권(ARENA_TOKEN), 행운 코인(LUCKY_COIN), 보물상자 키(CHEST_KEY) 3종이 `GameEnums.cs` ResourceType에 미등록 상태이나 기획서에 표기 없음
- **수정**: 재화 상세 섹션의 입장권, 행운 코인 제목에 **(미구현 -- XXX 미등록)** 표기 추가. 재화 전체 맵 및 재화 흐름도에도 미구현 표기 반영

### 버그 문서 상태 변경
- `docs/QA/Bugs/BUG-005_daily-quest-missing.md` -- Open -> Closed
- `docs/QA/Bugs/BUG-006_resource-type-missing.md` -- Open -> Closed
- `docs/QA/Bugs/BUG-007_dayProgressMaxBonus-not-applied.md` -- Open -> Closed, 심각도 Critical -> Minor (기획서 오류), 권장 수정 방안을 해결 방법으로 변경

---

## 2026-03-07 기획 문서 정합성 검토 2차 (BUG-004 해결 + 용어 정리 + 장비 패시브 현행화)

### 작업 내용
QA Agent의 정합성 보고서(consistency-report-v1)에서 보고된 BUG-004를 포함하여, 기획 문서 간 교차 검토 및 코드 대조를 통해 발견한 불일치를 수정했다.

### 작업 1: 삭제된 equipment-passive.data.json 참조 정리 (BUG-004)

K-15 작업에서 장비 패시브 시스템(`EquipmentPassiveTable` + `equipment-passive.data.json`)이 삭제되었으나, 3곳의 기획 문서에서 여전히 참조하고 있었음.

**수정 파일:**
- `docs/Planning/SystemDesign/06_가챠시스템.md` — 데이터 파일 테이블에서 삭제된 파일 참조 제거, `equipment-base-stats` → `equipment-labels` 경로 수정
- `docs/Planning/SystemDesign/13_밸런스데이터시트.md` — 장비 패시브 섹션을 "삭제됨" 상태로 변경 + 밸런스 영향 분석 메모 추가, 데이터 소스 테이블에서 삭제된 파일 제거 및 `equipment-labels` 추가

**근거:**
- `docs/Dev_Client/Todo/작업목록_kkwndud.md:28` (K-15 완료 기록)
- `grep` 결과: 코드에 `EquipmentPassive` 관련 클래스 0건
- `equipment-passive.data.json` 파일 미존재 (glob 결과)

### 작업 2: 기획 문서 간 용어/설명 불일치 정리

**수정 파일:**
- `docs/Planning/SystemDesign/04_스킬시스템.md`
  - "인카운터별 티어 제한" 테이블을 "인카운터별 스킬 제공 방식"으로 전면 재작성 (코드 근거 추가)
  - "천사(중박)", "악마(대박)" 용어를 코드의 인카운터 유형명(CHANCE, DEMON)과 일치시킴
  - "스킬 획득 경로" 테이블에서 용어를 모험시스템 문서와 일치시키고, 금상자/스킬 교환 경로 추가
- `docs/Planning/SystemDesign/12_모험시스템.md`
  - "금상자" 보상의 명확한 정의 추가 (코드 참조: `ChapterScreen.ShowEliteReward()`)
  - 강제 전투 테이블의 승리 보상 칸을 간결화

**발견한 코드-기획 불일치:**
| 항목 | 기획서 | 실제 코드 | 근거 |
|------|--------|----------|------|
| 금상자 티어 제한 | "tier 3 (신화급)" | 제한 없음 (전체 스킬 풀) | `ChapterScreen.cs:752` BuildSkillPool 제한 없이 사용 |
| 우연 스킬 상자 티어 | "tier 1~2" | 제한 없음 (전체 스킬 풀) | `EncounterGenerator.cs:136` maxTier 미지정 |
| DEMON 인카운터 | "전체" | 제한 없음 (전체 스킬 풀) | `EncounterGenerator.cs:76` maxTier 미지정 |

### 작업 3: 장비 패시브 섹션 현행화

작업 1과 함께 수행. 밸런스 데이터시트의 장비 패시브 섹션에 삭제 상태 및 밸런스 영향 분석 메모를 추가했다.

### 수정 파일 목록
- `docs/Planning/SystemDesign/04_스킬시스템.md`
- `docs/Planning/SystemDesign/06_가챠시스템.md`
- `docs/Planning/SystemDesign/12_모험시스템.md`
- `docs/Planning/SystemDesign/13_밸런스데이터시트.md`

---

## 2026-03-07 기획 문서 정합성 수정

### 작업 내용
밸런스 데이터시트(`13_밸런스데이터시트.md`)를 실제 JSON 데이터 파일과 대조 검토하여 불일치 항목을 수정했다.

### 수정 파일
- `docs/Planning/SystemDesign/13_밸런스데이터시트.md` - 전면 재작성
- `docs/Planning/SystemDesign/00_게임개요.md` - 챕터 유형, 인카운터 유형 수정
- `docs/Planning/SystemDesign/07_재화시스템.md` - 출석 보상 순서 수정

### 발견한 주요 불일치 (40건 이상)

| 항목 | 문서 (수정 전) | 실제 데이터 (수정 후) | 근거 |
|------|-------------|----------------|------|
| 챕터당 스케일링 | 1.25x | 1.3x | `battle.data.json` scalingPerChapter |
| 데미지 공식 | `ATK - DEF x 0.5` (감산) | `k/(k+DEF)` (k=100) | `battle.data.json` defenseConstant |
| 인카운터 가중치 | 전투40/천사25/악마10/우연25 | 전투40/악마7/우연53 | `encounter.data.json` weights |
| 적 기본 스탯 (일반) | HP=80 ATK=8 DEF=3 | HP=106 ATK=13 DEF=4 | `enemy.data.json` baseStats |
| 아메바 스탯 | HP=90 ATK=8 DEF=2 | HP=120 ATK=11 DEF=3 | `enemy.data.json` templates |
| 분노 공격 T2 계수 | 0.85 | 1.73 | `active-skill-tier.data.json` |
| 흡혈 T2 | 12% | 18% | `passive-skill-tier.data.json` |
| 부활 T1 | 30% | 17% | `passive-skill-tier.data.json` |
| 출석 2일차 보상 | 에픽 펫 | 골드 3,000 | `attendance.data.json` |
| 데이터 소스 경로 | `src/domain/data/json/` | `Assets/_Project/Data/Json/` | 실제 프로젝트 구조 |

### 추가 보완
- 기존 문서에 누락된 스킬 추가: 반격 마스터리, 배수진, 불굴, 압도, 분쇄, 기절
- 게임개요: 미구현 챕터 유형(30일/5일)에 "미구현" 상태 명시
- 재능 등급 테이블을 캐릭터성장시스템 문서와 일치하도록 상세화

### 브랜치
`feature/planning-agent/balance-data-consistency`
