# Planning Agent 작업일지

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
