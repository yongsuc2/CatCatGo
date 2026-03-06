# QA Agent WorkLog

## 2026-03-07

### Planning-Implementation Consistency Report v1

**Scope**
- Battle/Skill/Equipment/Character Growth/Dungeon/Gacha systems
- 18 JSON data files sync verification (Data/Json vs Resources)
- 50 enemies + 3 dungeon enemies skill mapping
- 14 planning documents vs code/data comparison

**Findings**
- [BUG-001 Major] enemy.data.json pools field hardcoded to Theme 1
- [BUG-002 Minor] enemy.data.json baseStats field purpose unclear
- [BUG-003 Major] Game overview doc describes unimplemented chapter types (30-day/5-day)
- [BUG-004 Minor] Gacha doc references non-existent file (equipment-passive.data.json)

**Output**
- docs/QA/TestCases/consistency-report-v1.md

### Planning-Implementation Consistency Report v2

**Scope**
- 재화/퀘스트/출석 시스템 (07_재화시스템.md vs quest.data.json, attendance.data.json, resource-labels.data.json)
- 펫 시스템 (09_펫시스템.md vs pet.data.json) — 18종 전수 검증 (패시브 타입, 최대 등급)
- 모험 시스템 (12_모험시스템.md vs Chapter.cs, EncounterGenerator.cs, EnemyTemplate.cs)
- 챕터 보물상자 (12_모험시스템.md vs chapter-treasure.data.json)
- 전승 시스템 (02_캐릭터성장시스템.md vs heritage.data.json)

**Findings**
- [BUG-005 Major] 일일 퀘스트 2개 누락 (아레나 1회, 여행 5회) — 미구현 기능 연동
- [BUG-006 Minor] ResourceType 3종 미등록 (ARENA_TOKEN, LUCKY_COIN, CHEST_KEY) — 미구현 기능
- [BUG-007 Major] dayProgressMaxBonus 미적용 — EnemyTemplate.cs:73에서 `1 + dayProgress` 사용, 기획서는 `1 + dayProgress * 0.3`. 몬스터가 기획 의도보다 약 54% 더 강해짐

**New Test Cases**
- TC-007 ~ TC-012 (6개 추가)

**Coverage Update**
- v1 + v2로 12개 시스템 기획서 중 10개 검증 완료
- 미검증: 이벤트 시스템 (10), 과금 시스템 (11)
- 미구현: PvP 시스템 (08)

**Output**
- docs/QA/TestCases/consistency-report-v2.md

### 버그 추적 문서 일괄 등록

consistency-report-v1, v2에서 인라인으로 기록된 BUG-001 ~ BUG-007을 `docs/QA/Bugs/` 디렉토리에 개별 문서로 등록.

**등록 시 코드/데이터 검증 결과:**
- BUG-001 (pools 하드코딩): 커밋 9eae6bac에서 제거됨 -> Closed
- BUG-002 (baseStats 불명확): 커밋 9eae6bac에서 제거됨 -> Closed
- BUG-003 (챕터 타입 미구현 미표기): 00_게임개요.md에 "미구현" 표기 완료 (커밋 68cb2a7b) -> Closed
- BUG-004 (가챠 문서 파일 참조 오류): 커밋 68cb2a7b에서 수정됨 -> Closed
- BUG-005 (일일 퀘스트 2개 누락): quest.data.json에 여전히 3개만 존재 -> Open
- BUG-006 (재화 타입 3종 미등록): GameEnums.cs에 여전히 없음 -> Open
- BUG-007 (dayProgressMaxBonus 미적용): EnemyTemplate.cs:73에서 여전히 `1 + dayProgress` -> Open (Critical)

**Output**
- docs/QA/Bugs/BUG-001_enemy-pools-hardcoded.md
- docs/QA/Bugs/BUG-002_enemy-baseStats-unclear.md
- docs/QA/Bugs/BUG-003_chapter-type-mismatch.md
- docs/QA/Bugs/BUG-004_gacha-doc-file-reference.md
- docs/QA/Bugs/BUG-005_daily-quest-missing.md
- docs/QA/Bugs/BUG-006_resource-type-missing.md
- docs/QA/Bugs/BUG-007_dayProgressMaxBonus-not-applied.md
