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

### 클라이언트 코드 검수/리팩토링 검증 (Task #3)

**목적**: dev-agent의 클라이언트 코드 전면 검수/리팩토링 결과 검증

**검증 대상**: main 워킹 디렉토리 미커밋 변경사항 (44개 수정 + 6개 신규)

**검증 항목 및 결과:**
- 하드코딩 -> JSON 추출 12건: PASS (모든 값 원본과 일치)
  - battle.data.json: stamina, chapterStaminaCost, newGameResources
  - dungeon.data.json: goblinMiner 섹션
  - pet.data.json: growth 섹션
  - talent.data.json: statLabels, gradeLabels, heritageRouteLabels
  - collection.data.json (신규): 컬렉션 엔트리 10건
  - chapter-treasure.data.json: totalDays, survivalMilestones, clearMilestone
- 중복 코드 제거 3건: PASS
  - SkillGradeHelper (TierToGrade 중앙집중화)
  - SkillRegistryHelper + SkillTierDataLoader (스킬 유틸 공유)
  - DateHelper (GetTodayString 공유)
- 버그 수정 3건 (GetComponent 중복 호출): PASS
- 죽은 코드 삭제 3건: PASS
- deprecated API 수정 (enableWordWrapping -> textWrappingMode): PASS
- 테스트 호환성: PASS (삭제된 메서드 참조 0건)

**발견된 이슈:**
- [BUG-014 Minor] CollectionDataTable.EnsureLoaded()에서 JSON 로드 실패 시 _entries가 null로 남음

**Output**
- docs/qa-agent/client-code-review-verification-report.md
- docs/qa-agent/Bugs/BUG-014_collection-datatable-null-safety.md

## 2026-03-08

### 서버-클라이언트 연동 버그 리뷰 (BUG-008 ~ BUG-013, BUG-015)

**Scope**
- 서버-클라이언트 API 응답 패턴 통일 검증
- Shared 모델 일치 여부 검증
- 비즈니스 로직 연동 검증
- 재능 마일스톤 delta key 형식 검증

**Findings**
- [BUG-008 Major] 서버 응답 패턴 이중화 -> ToActionResult() 적용으로 통일 -> Closed
- [BUG-009 Minor] ProductDto StartAt/EndAt nullable 불일치 -> 클라이언트 long? 변경 -> Closed
- [BUG-010 Major] RewardData.Amount double vs int 타입 불일치 -> 서버 래퍼 구조 통일 -> Closed
- [BUG-011 Minor] SyncApi.Sync() Version 미설정 -> version 파라미터 추가, SaveApi 삭제 -> Closed
- [BUG-012 Minor] ResourceApi/LinkSocialRequest 미구현 -> 미구현 사항으로 Closed
- [BUG-013 Critical] 서버 에러 시 로컬 폴백 실행 -> Result.Fail 반환으로 변경 -> Closed
- [BUG-015 Critical] 재능 마일스톤 delta key 형식 불일치 ("10" vs "LV_10") -> 서버 수정 -> Closed

### 버그 리포트 최신화 자체 검토

**Scope**: docs/qa-agent/Bugs/ 전체 (BUG-001 ~ BUG-015, 15건)

**검토 결과:**
- Closed 14건: 모두 실제 코드에서 수정 완료 확인 (코드 grep으로 직접 검증)
- Open 1건 (BUG-014): CollectionDataTable null safety 미수정 확인 — Open 상태 정확
- 버그 리포트 내 관련 파일 경로: 모두 유효 확인
- 잘못된 상태의 버그 리포트: 없음

**테스트 케이스 문서 현행화 확인:**
- consistency-report-v1: 검증 시점(2026-03-07) 기준 정확 (이후 수정된 BUG 상태는 개별 버그 문서에서 추적)
- consistency-report-v2: 동일 — 스냅샷 문서로서 정확

**결론: 잘못된 내용 없음. 미래 작업을 Todo 리스트로 등록**

**Output**
- docs/qa-agent/Todo/qa-todo-list.md
