# 클라이언트 코드 검수/리팩토링 검증 보고서

**검증일**: 2026-03-07
**검증 대상**: main 워킹 디렉토리 미커밋 변경사항 (dev-agent 재작업 결과)
**변경 규모**: 44개 파일 수정 + 6개 신규 파일

---

## 1. 검증 요약

| 항목 | 결과 |
|------|------|
| 하드코딩 -> JSON 추출 (12건) | PASS - 모든 값 일치 확인 |
| 중복 코드 제거 (3건) | PASS - 동치성 확인 |
| 버그 수정 (3건 GetComponent 중복) | PASS |
| 죽은 코드 삭제 (3건) | PASS |
| deprecated API 수정 | PASS - textWrappingMode 사용 확인 |
| 테스트 호환성 | PASS - 삭제된 메서드 참조 없음 |
| null safety | 1건 Minor 이슈 발견 (BUG-014) |

**총평**: 변경사항은 기존 동작을 동등하게 유지하며, 범위 내에서 적절하게 수행됨. Critical/Major 이슈 없음.

---

## 2. 하드코딩 -> JSON 추출 검증 (12건)

### 2.1 battle.data.json
| 항목 | JSON 값 | 원래 하드코딩 값 | 일치 |
|------|---------|----------------|------|
| stamina.max | 100 | STAMINA_MAX = 100 | O |
| stamina.regenPerMinute | 1 | STAMINA_REGEN_PER_MINUTE = 1 | O |
| chapterStaminaCost | 5 | staminaCost 5 | O |
| newGameResources.gold | 500 | gold 500 | O |
| newGameResources.gems | 500 | gems 500 | O |
| newGameResources.stamina | 100 | stamina 100 | O |

**사용처 확인**: Resources.cs (stamina), GameManager.cs (newGameResources, chapterStaminaCost)

### 2.2 dungeon.data.json (goblinMiner 섹션)
| 항목 | JSON 값 | 원래 하드코딩 값 | 일치 |
|------|---------|----------------|------|
| orePerMine | 1 | OrePerMine = 1 | O |
| cartThreshold | 30 | CartThreshold = 30 | O |
| cartReward.goldMin | 200 | goldMin 200 | O |
| cartReward.goldMax | 500 | goldMax 500 | O |
| cartReward.stoneMin | 1 | stoneMin 1 | O |
| cartReward.stoneMax | 3 | stoneMax 3 | O |

**사용처 확인**: GoblinMiner.cs - DungeonDataTable.GoblinMiner 참조

### 2.3 pet.data.json (growth 섹션)
| 항목 | JSON 값 | 원래 하드코딩 값 | 일치 |
|------|---------|----------------|------|
| expPerFood | 10 | EXP_PER_FOOD = 10 | O |
| baseExpPerLevel | 100 | base 100 | O |
| expPerLevelGrowth | 20 | growth 20 | O |
| statPerLevel | 2 | STAT_PER_LEVEL = 2 | O |
| hpPerLevel | 4 | HP_PER_LEVEL = 4 | O |

**사용처 확인**: Pet.cs, PetScreen.cs - PetTable.Growth 참조

### 2.4 talent.data.json (라벨 문자열)
- statLabels, gradeLabels, heritageRouteLabels 한국어 문자열 이동 완료
- TalentScreen.cs에서 `TalentTable.GetStatLabel()`, `TalentTable.GetGradeLabel()`, `TalentTable.GetHeritageRouteLabel()` 호출 확인

### 2.5 collection.data.json (신규)
- 10개 컬렉션 엔트리 이동 확인
- CollectionDataTable.cs 로딩 구조 확인
- Collection.cs에서 `CollectionDataTable.GetAllEntries()` 호출 확인

### 2.6 chapter-treasure.data.json
| 항목 | JSON 값 | 원래 하드코딩 | 일치 |
|------|---------|-------------|------|
| totalDays | 60 | 60 | O |
| survivalMilestones day 15 | gold:150, gems:10, eqStone:1, pwStone:0 | 동일 | O |
| survivalMilestones day 25 | gold:250, gems:25, eqStone:3, pwStone:0 | 동일 | O |
| survivalMilestones day 40 | gold:400, gems:40, eqStone:5, pwStone:1 | 동일 | O |
| clearMilestone | gold:600, gems:60, eqStone:8, pwStone:2 | 동일 | O |

---

## 3. 중복 코드 제거 검증 (3건)

### 3.1 SkillGradeHelper (TierToGrade 중앙집중화)
- ActiveSkill.cs, PassiveSkill.cs에서 개별 `TierToGrade` dict 삭제
- `SkillGradeHelper.GetGradeForTier(tier)` 호출로 대체
- GameEnums.cs에 집중화된 dict 값 동일 확인 (1->NORMAL, 2->LEGENDARY, 3->MYTHIC, 4->IMMORTAL)

### 3.2 SkillRegistryHelper + SkillTierDataLoader (스킬 유틸 공유)
- ActiveSkillTierData.cs, PassiveSkillTierData.cs -> SkillTierDataLoader로 위임 (동일 API 유지)
- ActiveSkillRegistry.cs, PassiveSkillRegistry.cs에서 `Pct()`, `V()` -> SkillRegistryHelper 위임
- `GetTierSuffix()` 공유 확인: {1:"", 2:" II", 3:" III", 4:" IV"}

### 3.3 DateHelper (GetTodayString 공유)
- DailyResetSystem.cs, AttendanceSystem.cs -> `DateHelper.GetTodayString()` 호출
- 포맷 동일: `$"{now.Year}-{now.Month}-{now.Day}"`

---

## 4. 버그 수정 검증 (3건)

| 파일 | 수정 내용 | 확인 |
|------|----------|------|
| ProjectileView.cs | 중복 GetComponent<RectTransform>() 호출 제거 | O |
| ProgressBarView.cs | 동일 | O |
| DamageGraphView.cs | 동일 | O |

---

## 5. 죽은 코드 삭제 검증 (3건)

| 파일 | 삭제 내용 | 확인 |
|------|----------|------|
| DailyDungeon.cs | GetTotalRemainingCount() 삭제 (GetRemainingCount()와 동일) | O |
| UIManager.cs | 빈 Update() 메서드 삭제 | O |
| ServerSyncService.cs | 불필요한 주석 1건 삭제 | O |

- DailyDungeonTests.cs에서 `GetTotalRemainingCount()` -> `GetRemainingCount()` 호출로 변경 확인
- 다른 테스트 파일에서 삭제된 메서드 참조 없음 확인 (grep 결과 0건)

---

## 6. deprecated API 수정 검증

- `enableWordWrapping` -> `textWrappingMode` (TextMeshPro)
- DebugScreen.cs 포함 전체 프로젝트에서 `textWrappingMode` 사용 확인 (28개소)
- `enableWordWrapping` 잔존 없음

---

## 7. 테스트 호환성 검증

- 삭제된 메서드 참조 검색:
  - `IsSynergyWith`: 테스트 파일 0건 (ActiveSkill에서 제거됨, HasHeritageSynergy에 인라인됨)
  - `GetTotalRemainingCount`: 테스트 파일 0건 (DailyDungeonTests 업데이트 완료)
  - `TierToGrade`: GameEnums.cs만 존재 (SkillGradeHelper에 집중화)

---

## 8. 신규 파일 검증 (6건)

| 파일 | 역할 | 상태 |
|------|------|------|
| CollectionDataTable.cs | collection.data.json 로딩 | BUG-014 (Minor) |
| collection.data.json | 컬렉션 데이터 10건 | OK |
| SkillTierDataLoader.cs | 스킬 티어 JSON 공통 로더 | OK |
| SkillRegistryHelper.cs | 스킬 레지스트리 공통 유틸 | OK |
| DateHelper.cs | 날짜 포맷 공유 유틸 | OK |
| chapter-treasure.data.json | 챕터 보물 데이터 | OK (기존 값 동일) |

---

## 9. 발견된 이슈

### BUG-014 (Minor): CollectionDataTable null safety
- `EnsureLoaded()`에서 JSON 로드 실패 시 `_entries`가 null인 채로 남음
- `Collection` 생성자에서 `GetAllEntries()` 반환값에 대한 null 체크 없음
- **영향**: JSON 파일이 정상 포함되어 있으면 실제 발생하지 않음
- **권장**: `if (data == null) { _entries = new List<CollectionEntryData>(); return; }` 방어 코드 추가

---

## 10. 결론

dev-agent의 클라이언트 코드 검수/리팩토링 결과는 **검증 통과**입니다.

- 모든 하드코딩 값이 JSON과 정확히 일치
- 중복 코드 제거가 원본과 동등한 동작 보장
- 삭제된 API에 대한 테스트 참조 없음
- Critical/Major 이슈 없음
- Minor 이슈 1건 (BUG-014) 등록됨
