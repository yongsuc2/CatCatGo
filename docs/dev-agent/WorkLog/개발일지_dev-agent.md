# 개발일지 - dev-agent

## 2026-03-08 (20) - BUG-014 수정 + 죽은 코드 삭제 + 중복 제거

### 개요

BUG-014 (CollectionDataTable null safety) 수정, 죽은 코드 삭제 (EquipmentManager 전체, PetManager 미사용 메서드 4개), 중복 코드 제거 (GameState 생성자/Reset(), ClaimAttendance 인라인 Pet 생성).

### 버그 수정 (1건)

| ID | 파일 | 내용 | 수정 |
|----|------|------|------|
| BUG-014 | `CollectionDataTable.cs` | `EnsureLoaded()`에서 JSON 로드 실패 시 `_entries` 미초기화 + `data["entries"]` null 시 NRE | `data?["entries"]` null 체크 후 빈 리스트로 초기화하여 NRE 방지 |

### 리팩토링 (4건)

| ID | 파일 | 내용 |
|----|------|------|
| R-4 | `EquipmentManager.cs` 삭제 | 전체 클래스 호출처 없음 (죽은 코드) — 파일 + meta 삭제, GameManager 참조 제거 |
| R-5 | `PetManager.cs` | 미사용 메서드 4개 삭제 (`FeedPet`, `TryUpgradeGrade`, `SelectBestPet`, `AutoFeedAll`), 미사용 using 정리 |
| R-6 | `GameState.cs` | 생성자와 `Reset()` 완전 중복 → 생성자에서 `Reset()` 호출로 통합 |
| R-7 | `GameManager.cs` | `ClaimAttendance()`의 인라인 Pet 생성을 `PetManagerService.HatchEgg()` 호출로 중복 제거 |

### 변경 파일

| 파일 | 변경 |
|------|------|
| `Assets/_Project/Scripts/Domain/Data/CollectionDataTable.cs` | null safety 강화 |
| `Assets/_Project/Scripts/Services/EquipmentManager.cs` | 삭제 (죽은 코드) |
| `Assets/_Project/Scripts/Services/GameManager.cs` | EquipmentManager 참조 제거, ClaimAttendance 중복 제거 |
| `Assets/_Project/Scripts/Services/GameState.cs` | 생성자/Reset() 중복 제거 |
| `Assets/_Project/Scripts/Services/PetManager.cs` | 미사용 메서드 삭제, HatchEgg에 grade/idPrefix 파라미터 추가 |

---

## 2026-03-08 (19) - 클라이언트 코드 버그 수정 + 리팩토링

### 개요

기획서 대비 구현 불일치 버그 수정 7건, 코드 리팩토링 3건 수행. 자체검토 Todo 항목 해소 포함.

### 버그 수정 (7건)

| ID | 파일 | 내용 | 수정 |
|----|------|------|------|
| B-1 | `PetScreen.cs` | 레벨업 프리뷰에 하드코딩 수치 사용 (`Level * 2 * 2`, `Level * 2`) | `PetTable.Growth.HpPerLevel` / `StatPerLevel` 참조로 변경 |
| B-2 | `GachaApi.cs` | `Pull()` / `Pull10()`에서 chestType을 `"EQUIPMENT"`로 하드코딩 | `string chestType` 파라미터 추가, 호출부 수정 |
| B-3 | `GameManager.cs` | `PullChestAsync` / `PullChest10Async`에서 chest type을 서버에 전달하지 않음 | `chestType.ToString()` 전달 |
| B-4 | `GameManager.cs` | `GetChestSystem()`에 PET/BASIC_PET case 누락 | PET → PetChestSystem, BASIC_PET → BasicPetChestSystem 매핑 추가 |
| B-5 | `EquipmentSlot.cs` | `Unequip()` 시 SlotLevels/SlotPromoteCounts 미초기화 (자체검토 FI-3) | `SlotLevels[index] = 0`, `SlotPromoteCounts[index] = 0` 추가 |
| B-6 | `Battle.cs` | `RunToCompletion()` maxTurns 초과 시 DEATH 로그 누락 (자체검토 FI-2) | DEFEAT 설정 시 `BattleLogType.DEATH` 로그 추가 |
| B-7 | `GameManager.cs` | `ClaimAttendance()`에서 `Player.OwnedPets.Add()` 직접 호출 (AddPet 우회) | `Player.AddPet(pet)` 사용으로 변경 |

### 리팩토링 (3건)

| ID | 파일 | 내용 |
|----|------|------|
| R-1 | `GameManager.cs` | `HatchPet()`의 인라인 Pet 생성 로직을 `PetManagerService.HatchEgg(Rng)` 호출로 통합 |
| R-2 | `BattleManager.cs` | `GetPetAbilitySkill()`에서 비효율적 ID 파싱 검색 제거, Name 기반 검색으로 단순화 |
| R-3 | `GameState.cs` | PetChestSystem/BasicPetChestSystem 필드 추가 및 생성자/Reset() 초기화 |

### 자체검토 Todo 해소

| ID | 상태 |
|----|------|
| FI-2 | 완료 (B-6에서 수정) |
| FI-3 | 완료 (B-5에서 수정) |
| FI-4 | 해당없음 (기획서상 펫 뽑기 천장 시스템 없음) |

### 변경 파일

| 파일 | 변경 |
|------|------|
| `Assets/_Project/Scripts/Presentation/Screens/PetScreen.cs` | 레벨업 프리뷰 하드코딩 제거 |
| `Assets/_Project/Scripts/Network/GachaApi.cs` | chestType 파라미터 추가 |
| `Assets/_Project/Scripts/Services/GameManager.cs` | 가챠 API 연동 수정, 펫 시스템 프로퍼티 추가, ClaimAttendance/HatchPet 수정 |
| `Assets/_Project/Scripts/Services/GameState.cs` | PetChestSystem/BasicPetChestSystem 추가 |
| `Assets/_Project/Scripts/Domain/Entities/EquipmentSlot.cs` | Unequip 슬롯 초기화 수정 |
| `Assets/_Project/Scripts/Domain/Battle/Battle.cs` | RunToCompletion DEATH 로그 추가 |
| `Assets/_Project/Scripts/Services/BattleManager.cs` | GetPetAbilitySkill 단순화 |
| `docs/dev-agent/Todo/자체검토_Todo.md` | Todo 상태 업데이트 |

---

## 2026-03-08 (18) - 클라이언트 코드 자체 검토

### 개요

기획서(SystemDesign) 대비 구현 코드 검토, 중복 코드 검출, 서버-클라이언트 동기화 버그 탐색, 컴파일/런타임 에러 가능성 확인.

### 발견 및 수정

| 항목 | 내용 | 조치 |
|------|------|------|
| PetManager 죽은 코드 | `GetTotalPassiveBonus()` 호출처 없음. `Player.ComputePetBonus()`가 동일 역할 수행 | 메서드 삭제 |
| WeaponSubType 서버 동기화 누락 | `EquipmentDeltaData`에 `WeaponSubType` 필드 없음 + `DeserializeEquipmentDelta()`에서 파싱하지 않아 무기 종류 정보 유실 | `StateDelta.cs`에 필드 추가, `GameState.cs`에서 `Enum.TryParse` 파싱 후 Equipment 생성자에 전달 |

### 미수정 항목 (Todo 등록)

| 항목 | 우선순위 | 사유 |
|------|---------|------|
| Battle.RunToCompletion maxTurns DEFEAT 로그 누락 | 저 | UI 표시 편의성 문제, 기능 정상 동작 |
| EquipmentSlot.Unequip SlotLevel 초기화 미수행 | 저 | Equip 시 덮어쓰므로 실질적 영향 없음 |
| 펫 뽑기 PityCount 미증가 | 중 | 기획서상 펫 뽑기는 천장 시스템 없음으로 의도된 동작 |

### 검토 범위

- 기획서 10개 문서 대비 구현 대조 (전투, 성장, 장비, 스킬, 스테이지, 가챠, 재화, 펫, 모험)
- 중복 코드 검출 (PetManager vs Player 유사 로직 → 죽은 코드로 판명)
- StateDelta 기반 서버 동기화 데이터 무결성

### 변경 파일

| 파일 | 변경 |
|------|------|
| `Assets/_Project/Scripts/Services/PetManager.cs` | `GetTotalPassiveBonus()` 삭제 |
| `Assets/_Project/Scripts/Network/StateDelta.cs` | `EquipmentDeltaData`에 `WeaponSubType` 필드 추가 |
| `Assets/_Project/Scripts/Services/GameState.cs` | `DeserializeEquipmentDelta()`에서 `WeaponSubType` 파싱 및 전달 |
| `docs/dev-agent/Todo/자체검토_Todo.md` | 검토 결과 Todo 문서 생성 |

---

## 2026-03-07 (17) - 클라이언트 로그 시스템 보강

### 개요

클라이언트 로그를 구조화하고 보강했다. 전용 GameLog 유틸리티 추가, API 호출 로그 개선(body 요약/소요시간), 핵심 상태 변경 로그 추가, 런타임 로그 뷰어 추가.

### 신규 파일

| 파일 | 설명 |
|------|------|
| `Assets/_Project/Scripts/Infrastructure/GameLog.cs` | 구조화된 로그 유틸리티 (태그/레벨/버퍼/이벤트) |

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `CatCatGo.Network.asmdef` | CatCatGo.Infrastructure 참조 추가 |
| `ApiClient.cs` | Debug.Log -> GameLog 전환, 요청/응답 body 요약(120자), 소요시간(ms) 측정 |
| `ServerSyncService.cs` | Debug.Log -> GameLog 전환 (Sync 태그) |
| `GameManager.cs` | Debug.Log -> GameLog 전환, 핵심 상태 변경 로그 6개 추가 (StartChapter, PullGacha, TalentUpgrade, UpgradeEquipment, NetworkMode, ApiFailed) |
| `DebugScreen.cs` | 로그 콘솔 섹션 추가 (레벨 필터 ALL/INFO/WARN/ERR, 실시간 스트리밍, CLR 버튼) |

### GameLog 설계

- LogLevel: Debug/Info/Warn/Error
- 태그 기반: Net, Sync, Game 등
- 순환 버퍼 200개
- OnLogAdded 이벤트로 실시간 UI 업데이트
- MinLevel 프로퍼티로 런타임 필터링
- Unity Debug.Log/LogWarning/LogError로도 동시 출력

### API 로그 개선 (ApiClient)

- 요청 시: `[Net] POST api/talent/upgrade | {"statType":"ATK"}`
- 응답 시: `[Net] POST api/talent/upgrade -> 200 (45ms) | {"success":true,...}`
- 에러 시: `[Net] POST api/talent/upgrade FAILED (3000ms): Connection refused (retry 1/3)`
- body 120자 초과 시 truncate

### DebugScreen 로그 뷰어

- 400px 높이 스크롤 영역
- 레벨별 색상: Debug(회색), Info(흰색), Warn(노랑), Error(빨강)
- 필터 버튼: ALL, INFO, WARN, ERR
- CLR 버튼: 로그 전체 삭제
- 실시간 로그 추가 + 자동 스크롤

---

## 2026-03-07 (16) - 서버-클라이언트 연동 버그 수정 (BUG-008, BUG-009, BUG-011)

### 개요

qa-agent 버그 리뷰에서 발견된 Minor/Major 버그 3건을 수정했다.

### 수정 내용

**BUG-009: ProductDto StartAt/EndAt nullable 불일치**
- `ShopResponses.cs`: `StartAt`, `EndAt` 타입을 `long` -> `long?`로 변경
- 서버가 기간 한정이 아닌 상품에서 null을 보내는 것에 대응
- 현재 이 필드를 사용하는 클라이언트 코드 없음 (향후 사용 시 안전)

**BUG-011: SyncApi.Sync() Version 미설정**
- `SyncApi.cs`: `Sync()` 메서드에 `int version` 파라미터 추가
- `ServerSyncService.cs`: `_saveVersion` 필드 추가, LoadFullSync 시 서버 version 저장, SyncSaveToServer에서 version 전달 및 ACCEPTED 시 업데이트
- `SaveApi.cs` 삭제: SyncApi와 완전 중복이며 아무 곳에서도 호출되지 않는 죽은 코드

### 수정 파일

| 파일 | 변경 |
|------|------|
| `Assets/_Project/Scripts/Shared/Responses/ShopResponses.cs` | StartAt/EndAt nullable 변경 |
| `Assets/_Project/Scripts/Network/SyncApi.cs` | version 파라미터 추가 |
| `Assets/_Project/Scripts/Services/ServerSyncService.cs` | _saveVersion 추적 로직 추가 |
| `Assets/_Project/Scripts/Network/SaveApi.cs` | 삭제 (미사용 중복 코드) |

---

## 2026-03-07 (15) - BUG-008 ApiClient HTTP 400 body 역직렬화 수정

### 개요

서버 응답 패턴 통일(dev-server-agent)에 대응하여, ApiClient가 HTTP 4xx 응답의 body를 역직렬화하도록 수정했다. 서버가 `BadRequest(result)`로 에러를 반환하면 body에 `ServerResponse<T>` JSON이 포함되는데, 기존 코드는 이를 무시하고 Data를 null로 설정하여 ErrorCode가 유실되었다.

### 수정 내용

- `ApiResponse.cs`: `FailWithData(T data, int statusCode, string errorMessage)` 팩토리 메서드 추가
- `ApiClient.cs`: HTTP 4xx 응답 시 body를 T로 역직렬화 시도, 성공하면 `FailWithData`로 Data 포함

### 수정 파일

| 파일 | 변경 |
|------|------|
| `Assets/_Project/Scripts/Network/ApiResponse.cs` | FailWithData 팩토리 메서드 추가 |
| `Assets/_Project/Scripts/Network/ApiClient.cs` | HTTP 4xx body 역직렬화 로직 추가 |

---

## 2026-03-07 (14) - BUG-013 서버 비즈니스 에러 시 로컬 폴백 제거

### 개요

GameManager의 30여 개 Online API 메서드에서 서버가 비즈니스 에러(재화 부족 등)를 반환해도 로컬 폴백 로직을 실행하여 서버-클라이언트 상태 불일치가 발생하는 Critical 버그를 수정했다.

### 수정 내용

모든 Online API 메서드의 에러 분기에서 로컬 폴백(로컬 도메인 로직 실행)을 제거하고 `Result.Fail` 또는 `null`을 반환하도록 변경했다.

수정 패턴:
- `Result` 콜백 메서드: `callback(로컬메서드())` -> `callback(Result.Fail(errorCode))`
- `Result<T>` 콜백 메서드: `callback(로컬메서드())` -> `callback(Result.Fail<T>(errorCode))`
- 비-Result 콜백(PullGacha, Attendance): `callback(로컬메서드())` -> `callback(null)`

영향받는 메서드 (30여 개): TalentUpgradeAsync, ClaimTalentMilestoneAsync, ClaimAllTalentMilestonesAsync, UpgradeEquipmentAsync, EquipItemAsync, UnequipItemAsync, SellEquipmentAsync, ForgeEquipmentAsync, BulkForgeAsync, HatchPetAsync, FeedPetAsync, DeployPetAsync, PullGachaAsync, PullGacha10Async, TowerChallengeAsync, DungeonChallengeAsync, DungeonSweepAsync, GoblinMineAsync, GoblinCartAsync, CatacombStartAsync, CatacombBattleAsync, CatacombEndAsync, ClaimQuestRewardAsync, ClaimAllQuestRewardsAsync, UpgradeHeritageAsync, ClaimChapterTreasureAsync, ClaimAttendanceAsync, ChapterStartAsync, ChapterAdvanceDayAsync, ChapterResolveEncounterAsync, ChapterSelectSkillAsync, ChapterRerollAsync, ChapterBattleResultAsync, ChapterAbandonAsync

### 검증

- 모든 UI 호출부에서 `result.IsFail()`/`result.IsOk()`/null 체크가 이미 구현되어 있음을 확인
- 오프라인 분기(`_networkMode == NetworkMode.OFFLINE`)는 기존 로컬 실행을 유지

### 수정 파일

| 파일 | 변경 |
|------|------|
| `Assets/_Project/Scripts/Services/GameManager.cs` | 30여 개 Online API 메서드의 에러 분기에서 로컬 폴백 제거 |

---

## 2026-03-07 (13) - 서버 연동 후 컴파일 에러 3건 수정

### 개요

서버/클라이언트 연동(Phase 1~8) 완료 후 발생한 Unity 컴파일 에러 3건을 수정했다.

### 수정 내용

| 파일 | 행 | 에러 | 수정 |
|------|-----|------|------|
| `ServerSyncService.cs` | 79 | CS1622: 코루틴에서 return 사용 불가 | `return;` → `yield break;` |
| `GameManager.cs` | 1146 | CS0117: BattleState.DEFEATED 존재하지 않음 | `BattleState.DEFEATED` → `BattleState.DEFEAT` |
| `GameManager.cs` | 1172 | CS0117: BattleState.DEFEATED 존재하지 않음 | `BattleState.DEFEATED` → `BattleState.DEFEAT` |

### 검증

- Unity batch mode 컴파일: ExitCode 0 (에러 0건)
- 서버 dotnet build: 0 Error, 1 Warning (기존 CS0109 warning, 본 작업 무관)

---

## 2026-03-07 (12) - Phase 8: 안정화 - 에러 처리 UX + 모드 전환 + 동기화

### 개요

서버/클라이언트 연동 안정화. 에러 처리 UX(토스트 알림), ONLINE/OFFLINE 전환 알림, ServerSyncService를 SyncApi 기반으로 완성.

### 신규 파일

| 파일 | 설명 |
|------|------|
| `ErrorCodeMessages.cs` | 14개 에러코드 -> 한국어 메시지 매핑 (INSUFFICIENT_GOLD 등) |
| `ToastView.cs` | 토스트 UI 컴포넌트 (2.5초 표시 + 0.5초 페이드) |

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `GameEvents.cs` | `ErrorToastEvent` struct 추가 |
| `GameManager.cs` | `OnApiFailed(string errorCode)` 오버로드 - ErrorCodeMessages 조회 후 `EventBus.Publish(ErrorToastEvent)`. UIManager 직접 참조 제거 (순환 참조 방지) |
| `UIManager.cs` | `ErrorToastEvent` / `NetworkModeChangedEvent` EventBus 구독. `ShowToast()` 메서드 + `CreateToastView()`. OnDestroy에서 구독 해제 |
| `ServerSyncService.cs` | SaveApi -> SyncApi 전환. InitializeConnection: AutoLogin 후 SyncApi.GetFull -> GameState.ApplyFullSync. SyncSaveToServer: SyncApi.Push 사용, push rejected 시 LoadFullSync. TryLoadServerSave -> LoadFullSync(SyncApi.GetFull 기반) |

### 순환 참조 해결

Services -> Presentation 순환 참조 문제를 EventBus 패턴으로 해결:
- GameManager(Services)에서 `EventBus.Publish(new ErrorToastEvent)` 발행
- UIManager(Presentation)에서 `EventBus.Subscribe<ErrorToastEvent>` 구독
- Infrastructure의 EventBus를 매개로 양 레이어가 독립 유지

### NetworkMode 전환 토스트

UIManager에서 `NetworkModeChangedEvent` 구독:
- ONLINE 전환 시: "서버에 다시 연결되었습니다."
- OFFLINE 전환 시: "서버 연결이 끊어졌습니다. 오프라인 모드로 전환합니다."

### ServerSyncService 동기화 흐름

```
[초기 연결]
AutoLogin -> SyncApi.GetFull -> GameState.ApplyFullSync -> ONLINE

[세이브 푸시]
SyncApi.Push(json, timestamp)
  -> Accepted: 완료
  -> Rejected: SyncApi.GetFull -> 서버 상태로 교체

[앱 백그라운드]
pendingSave -> SyncSaveToServer 즉시 호출

[앱 포그라운드 + OFFLINE]
RetryConnection -> InitializeConnection 재시도
```

---

## 2026-03-07 (11) - Phase 7: 챕터 세션 API 연동

### 개요

챕터 시스템을 서버 세션 기반으로 전환. ChapterApi 7개 엔드포인트 생성, GameManager에 7개 Async 메서드 추가, ChapterScreen을 Async 콜백 패턴으로 전환.

### 신규 파일

| 파일 | 설명 |
|------|------|
| `ChapterApi.cs` | POST /api/chapter/* (7 메서드: start, advance-day, resolve-encounter, select-skill, reroll, battle-result, abandon) |

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `ServerResponseTypes.cs` | 챕터 응답 데이터 타입 7개 추가 (ChapterStartResponseData, ChapterAdvanceDayResponseData, EncounterDeltaData 등) |
| `GameManager.cs` | `_chapterSessionId` 필드 + 7개 Chapter Async 메서드 + 3개 결과 타입 (ChapterAdvanceDayResult, ChapterRerollResult, ChapterBattleResultResult) |
| `EncounterGenerator.cs` | `FindSkillById(string)` 정적 메서드 추가 (ActiveSkill/PassiveSkill 레지스트리 전체 검색) |
| `ChapterScreen.cs` | 8개 메서드 Async 전환 + `_isRequestPending` + `_currentBattleSeed` + `ConvertServerEncounter` 헬퍼 |

### ChapterScreen Async 전환 상세

| 기존 메서드 | 전환 내용 |
|-------------|-----------|
| `StartChapter()` | `Game.ChapterStartAsync()` 콜백 래핑 |
| `AdvanceDay()` | `Game.ChapterAdvanceDayAsync()` + BattleRequired/Encounter 분기 |
| `SelectOption()` | `Game.ChapterResolveEncounterAsync()` |
| `RerollEncounter()` | `Game.ChapterRerollAsync()` + ServerEncounter 변환 |
| `SelectEliteReward()` | `Game.ChapterSelectSkillAsync()` |
| `HandleBossBattleEnd()` | `Game.ChapterBattleResultAsync()` 콜백 래핑 |
| `HandleNormalBattleEnd()` | `Game.ChapterBattleResultAsync()` 콜백 래핑 |
| `AbandonChapter()` | `Game.ChapterAbandonAsync()` 콜백 래핑 |

### 추가된 유틸리티

- `AutoResolveEncounter()`: 1-option 인카운터 자동 해결 (기존 인라인 로직 → Async 분리)
- `ConvertServerEncounter()`: `EncounterDeltaData` → `ChapterEncounter` 변환 (ONLINE 모드용)
- `EncounterGenerator.FindSkillById()`: skillId로 ActiveSkill/PassiveSkill 전체 검색하여 SessionSkillWrapper 반환

### 제거된 패턴

- `Game.StartChapter()` 직접 호출 → `Game.ChapterStartAsync()` 전환
- `chapter.AdvanceDay()` 로컬 호출 → `Game.ChapterAdvanceDayAsync()` 전환
- `chapter.ResolveEncounter()` + `Resources.Add()` 직접 호출 → `Game.ChapterResolveEncounterAsync()` 전환
- `chapter.RerollEncounter()` 로컬 호출 → `Game.ChapterRerollAsync()` 전환
- `chapter.SessionSkills.Add()` 직접 수정 → `Game.ChapterSelectSkillAsync()` 전환
- `Game.SaveGame()` 직접 호출 → Async 메서드 내부 처리

---

## 2026-03-07 (10) - Phase 6: Screen Async 전환 + 이중 요청 방지

### 개요

8개 Screen 파일에서 GameManager 동기 메서드 호출을 Async 메서드 호출로 전환. 이중 요청 방지(`_isRequestPending`) 패턴 적용.

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `TalentScreen.cs` | OnUpgradeClicked, OnClaimAll, ClaimMilestone → Async 전환 |
| `EquipmentScreen.cs` | OnEquipClicked, OnSellClicked, OnUpgradeClicked, OnUnequipClicked, OnBulkMergeClicked, OnMergeClicked → Async 전환 |
| `PetScreen.cs` | OnHatchClicked, OnDeployClicked, OnFeedClicked, OnMaxLevelClicked → Async 전환 |
| `ShopScreen.cs` | ExecuteEquipmentPull → PullGachaAsync/PullGacha10Async 전환 |
| `ContentScreen.cs` | Tower/Dungeon/Sweep/GoblinMine/Cart/CatacombStart/Battle/End → 8개 인라인 Async 전환 |
| `QuestScreen.cs` | OnClaimMission, OnClaimAll → Async 전환 |
| `EventScreen.cs` | OnClaimToday → ClaimAttendanceAsync 전환 |
| `ChapterTreasureScreen.cs` | OnClaimMilestone → Async 전환 + SaveGame() 직접 호출 제거 |
| `AuthApi.cs` | using CatCatGo.Services 제거, GameManager 직접 참조 제거 (순환 참조 수정) |

### 이중 요청 방지 패턴

```csharp
private bool _isRequestPending;

private void OnAction()
{
    if (_isRequestPending) return;
    _isRequestPending = true;
    Game.ActionAsync(params, result => {
        _isRequestPending = false;
        if (result.IsOk()) UI.Refresh();
    });
}
```

### 부수 수정

- ChapterTreasureScreen: `Game.SaveGame()` 직접 호출 제거 (Async 메서드 내부에서 처리)
- AuthApi.cs: Network → Services 순환 참조 방지를 위해 GameManager.Instance 직접 참조 제거

---

## 2026-03-07 (9) - Phase 4-5: GameManager ONLINE 분기 추가

### 개요

GameManager의 25개 듀얼 모드 메서드에 ONLINE 분기를 추가. ONLINE 모드에서 서버 API를 호출하고, 응답의 StateDelta를 GameState.ApplyDelta()로 적용.

### 신규 파일

| 파일 | 설명 |
|------|------|
| `ServerResponse.cs` | 서버 응답 래퍼 타입 (Success, Error, ErrorCode, Data, Delta) |
| `ServerResponseTypes.cs` | API별 응답 데이터 타입 (RewardData, PetHatchResponseData 등) |
| `TalentApi.cs` | POST /api/talent/* (3 메서드) |
| `EquipmentApi.cs` | POST /api/equipment/* (6 메서드) |
| `PetApi.cs` | POST /api/pet/* (3 메서드) |
| `GachaApi.cs` | POST /api/gacha/* (2 메서드) |
| `HeritageApi.cs` | POST /api/heritage/upgrade |
| `ContentApi.cs` | POST /api/content/* (8 메서드) |
| `DailyApi.cs` | POST /api/daily/* (3 메서드) |
| `TreasureApi.cs` | POST /api/treasure/claim |
| `SyncApi.cs` | GET /api/sync/full, POST /api/sync/push |

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `GameManager.cs` | NetworkMode 필드 + SetNetworkMode + 25개 Async 메서드 + ConvertRewardData 헬퍼 |
| `ServerSyncService.cs` | 인증 결과/상태 변경 시 GameManager.SetNetworkMode 호출 |

### ONLINE 분기 패턴

```
void MethodAsync(params, Action<Result<T>> callback)
  OFFLINE → 기존 동기 메서드 호출 후 즉시 callback
  ONLINE → API 호출 → 성공: ApplyServerDelta + callback(Ok)
                     → 실패: OnApiFailed + OFFLINE 폴백
```

### 폴백 전략

- API 실패 시 OFFLINE 로직으로 폴백
- 3회 연속 실패 시 자동으로 OFFLINE 모드 전환 + SaveGame

---

## 2026-03-07 (8) - Phase 3-2: Screen 리팩토링

### 개요

Screen 코드에서 도메인 객체를 직접 변경하던 패턴(B/C 패턴)을 Phase 3-1에서 추가한 GameManager 메서드 호출로 전환했다.
UI 빌드/표시 코드는 변경하지 않고, 상태 변경 핸들러만 수정.

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `TalentScreen.cs` | OnUpgradeClicked, ClaimMilestone, OnClaimAll → GameManager 메서드 호출. ProcessSingleClaim 삭제 |
| `PetScreen.cs` | OnHatchClicked, OnDeployClicked, OnFeedClicked, OnMaxLevelClicked → GameManager 메서드 호출 |
| `EquipmentScreen.cs` | OnEquipClicked, OnSellClicked, OnUpgradeClicked, OnUnequipClicked, OnBulkMergeClicked, OnMergeClicked → GameManager 메서드 호출 |
| `ContentScreen.cs` | Tower/Dungeon/GoblinMine/GoblinCart/Catacomb 핸들러 → GameManager 메서드 호출 |
| `QuestScreen.cs` | OnClaimMission, OnClaimAll → GameManager 메서드 호출 |
| `ShopScreen.cs` | ExecuteEquipmentPull에서 AddToInventory/Resources.Add/SaveGame 제거 |
| `GameManager.cs` | PullGacha/PullGacha10에 AddToInventory + Resources.Add + SaveGame 통합 |

### 제거된 패턴

- Screen에서 `Game.Player.Resources.Add/Spend` 직접 호출
- Screen에서 `Game.SaveGame()` 직접 호출
- Screen에서 도메인 서비스(tower.Challenge, catacomb.StartRun 등) 직접 호출
- Screen에서 BattleManagerService.CreatePlayerUnit 직접 호출

### 유지된 패턴

- UI 표시용 읽기 전용 접근 (Game.ForgeService.FindMergeCandidates, Game.Player.Resources.Get 등)
- 결과 데이터에서 보상 정보 추출하여 UI 표시

---

## 2026-03-07 (7) - Phase 3-1: GameManager 듀얼 모드 메서드 추가

### 개요

설계 문서 Phase 3 중 GameManager 듀얼 모드 분기 메서드를 추가했다.
현재는 OFFLINE 모드만 구현 (기존 Screen 로직을 GameManager 메서드로 래핑).
Screen 코드는 변경하지 않음 (Phase 3-2에서 변경 예정).

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `GameManager.cs` | 듀얼 모드 메서드 22개 추가 + 결과 타입 5개 추가 |

### 추가된 메서드

| 카테고리 | 메서드 | 반환 타입 |
|----------|--------|-----------|
| Talent | `TalentUpgrade(StatType)` | `Result<TalentUpgradeResult>` |
| Talent | `ClaimTalentMilestone(int)` | `Result` |
| Talent | `ClaimAllTalentMilestones()` | `Result` |
| Equipment | `UpgradeEquipment(string)` | `Result` |
| Equipment | `EquipItem(string)` | `Result` |
| Equipment | `UnequipItem(SlotType, int)` | `Result` |
| Equipment | `SellEquipment(string)` | `Result` |
| Equipment | `ForgeEquipment(List<string>)` | `Result<ForgeResult>` |
| Equipment | `BulkForge()` | `Result<BulkForgeResult>` |
| Pet | `HatchPet()` | `Result<Pet>` |
| Pet | `FeedPet(string, int)` | `Result` |
| Pet | `DeployPet(string)` | `Result` |
| Content | `TowerChallenge()` | `Result<TowerActionResult>` |
| Content | `DungeonChallenge(DungeonType)` | `Result<DungeonChallengeResult>` |
| Content | `DungeonSweep(DungeonType)` | `Result<SweepResult>` |
| Content | `GoblinMine()` | `Result<GoblinMineResult>` |
| Content | `GoblinCart()` | `Result<Reward>` |
| Content | `CatacombStart()` | `Result` |
| Content | `CatacombBattle()` | `Result<CatacombRunResult>` |
| Content | `CatacombEnd()` | `Result<Reward>` |
| Quest | `ClaimQuestReward(string, string)` | `Result` |
| Quest | `ClaimAllQuestRewards(string)` | `Result` |
| Heritage | `UpgradeHeritage(HeritageRoute)` | `Result<HeritageUpgradeResult>` |

### 추가된 결과 타입

| 타입 | 필드 |
|------|------|
| `BulkForgeResult` | MergedCount |
| `TowerActionResult` | BattleState, Reward, Advanced |
| `DungeonChallengeResult` | BattleState, Reward |
| `GoblinMineResult` | OreGained, TotalOre |
| `CatacombRunResult` | ContinueRun, Reward, CurrentFloor, BattleIndex |

### 설계 원칙

- 모든 메서드는 현재 OFFLINE 모드 로직 (기존 Screen 코드를 그대로 이동)
- 각 메서드 끝에 `SaveGame()` 호출
- 기존 GameManager 메서드(StartChapter, ChallengeDungeon, PullGacha 등) 유지
- 도메인 타입과의 이름 충돌 방지: TowerActionResult (도메인 TowerChallengeResult), CatacombRunResult (도메인 CatacombBattleResult)

---

## 2026-03-07 (6) - Phase 1: 서버연동 기반 구축

### 개요

설계 문서(클라이언트_서버연동_설계_상세.md) Phase 1을 구현했다.
기능 변경 없이 코드 구조만 변경. 오프라인 모드에서 기존 동작 100% 유지.

### 신규 파일

| 파일 | 설명 |
|------|------|
| `Assets/_Project/Scripts/Infrastructure/GameEvents.cs` | EventBus용 이벤트 struct 14개 + NetworkMode enum |
| `Assets/_Project/Scripts/Network/StateDelta.cs` | StateDelta + 13개 하위 Delta 타입 (서버 응답 상태 변경분) |
| `Assets/_Project/Scripts/Services/GameState.cs` | GameManager에서 상태 필드 추출, ApplyDelta/ApplyFullSync 구현 |

### 변경 파일

| 파일 | 변경 내용 |
|------|-----------|
| `GameManager.cs` | 상태 필드를 GameState로 위임, 프로퍼티로 래핑하여 기존 접근 호환성 유지 |
| `SaveSerializer.cs` | GameState 대응 Serialize/Deserialize 오버로드 추가, 기존 GameManager 메서드는 GameState 경유 |

### 구조 변경 요약

```
[Before]
GameManager ─── Player, Tower, Catacomb, DungeonManager, ... (직접 보유)

[After]
GameManager ─── GameState ─── Player, Tower, Catacomb, DungeonManager, ... (GameState가 보유)
            └── Rng, SaveManagerSystem (GameManager에 잔류)
            └── 프로퍼티로 기존 접근 경로 유지 (Game.Player → Game.State.Player)
```

### GameState.ApplyDelta 이벤트 발행 규칙

설계 문서 Section 9에 따라 Delta 필드별 이벤트 매핑 구현:
- Resources → ResourcesChangedEvent
- Talent → TalentChangedEvent + PlayerStatsChangedEvent
- Heritage → HeritageChangedEvent + PlayerStatsChangedEvent
- Equipment 관련 → EquipmentChangedEvent / InventoryChangedEvent + PlayerStatsChangedEvent
- Pet 관련 → PetChangedEvent + PlayerStatsChangedEvent
- Tower/Catacomb/Dungeons → TowerChangedEvent / CatacombChangedEvent / DungeonChangedEvent
- Mission → QuestChangedEvent
- Attendance → AttendanceChangedEvent
- ChapterSession → ChapterStateChangedEvent

---

## 2026-03-07 (5) - 서버 서비스 단위 테스트 작성

### 개요

구현된 9개 서버 서비스에 대한 단위 테스트를 작성했다.
기존 테스트 패턴(xunit + NSubstitute)을 따라 경계값, 예외 상황을 포함하는 테스트를 작성.

### 신규 테스트 파일 (7개)

| 파일 | 테스트 수 | 대상 서비스 |
|------|-----------|-------------|
| ResourceServiceTests.cs | 12 | 잔액 조회, 소비, 지급, 다중 소비/지급 |
| TalentServiceTests.cs | 7 | 초기 상태, 레벨업, 마일스톤 |
| EquipmentServiceTests.cs | 11 | 강화, 합성, 장착, 판매 |
| GachaServiceTests.cs | 9 | 뽑기, 10연차, 천장, 펫 뽑기 |
| ChapterServiceTests.cs | 14 | 시작, 인카운터, 스킬, 리롤, 포기, 보물 |
| PetServiceTests.cs | 8 | 먹이, 등급업, 장착 |
| HeritageServiceTests.cs | 7 | 상태, 업그레이드, 전 루트 |
| DailyServiceTests.cs | 8 | 출석, 퀘스트 보상 |
| ContentServiceTests.cs | 12 | 타워, 던전, 여행, 고블린, 카타콤 |

### 기존 테스트 수정

- AuthServiceTests.cs: RefreshAsync 스텁 테스트 -> 유효/만료/무효/차단 4건으로 교체

### 서비스 버그 수정

- TalentService.ClaimMilestoneAsync: JSON 문자열 Contains 기반 중복 체크 -> Deserialize + List.Contains로 수정

### 검증

- `dotnet test`: 141 Passed, 0 Failed, 0 Skipped

---

## 2026-03-07 (4) - planning-agent 세부분석 기반 서버 서비스 보완

### 개요

planning-agent의 기획서 세부 분석에서 발견된 누락/미흡 사항 6건을 보완했다.

### 변경 사항

| 서비스 | 보완 내용 |
|--------|-----------|
| AuthService | RefreshAsync: null 반환 스텁 -> 실제 Refresh Token 검증 + 새 Access Token 발급 구현 |
| EquipmentService | ForgeAsync: S등급 합성 차단, SubStats 유지+합산(최대 5개), 강화석 100% 반환 |
| ContentService | DungeonEnterAsync: 타입별 분리 카운트 -> 전 종류 공유 입장 횟수(DUNGEON_SHARED) |
| PetService | UpgradeAsync: 등급업 시 중복 펫 소모(삭제) 로직 추가 |
| ChapterService | DetermineEncounterType: 중박(10회)/대박(30회) 카운터 기반 보정 보상 인카운터 보장 |
| GachaService | PetPullAsync: PET_EGG 소모 -> 티어 가중치 기반 펫 생성 + PET_FOOD 보너스 지급 |

### 인터페이스/구현체 추가

| 항목 | 변경 |
|------|------|
| IAccountRepository | GetByRefreshTokenAsync 추가 |
| AccountRepository | GetByRefreshTokenAsync 구현 |
| IPetRepository | DeleteAsync 추가 |
| PetRepository | DeleteAsync 구현 |
| GachaController | POST /api/gacha/pet-pull 엔드포인트 추가 |

### 검증

- `dotnet build`: 0 Warning, 0 Error

---

## 2026-03-07 (3) - 서버 미구현 피쳐 34개 API 구현

### 개요

치트_검증_목록_상세.md에 설계된 48개 API 중 미구현 상태였던 34개 API를 구현했다.
신규 서비스 9개 + 기존 서비스 확장 3개, 총 12개 서비스에 대한 전체 구현.

### 신규 서비스 (9개)

| 서비스 | API 수 | 엔드포인트 |
|--------|--------|-----------|
| ResourceService | 2 | POST /api/resource/spend, GET /api/resource/balance |
| TalentService | 3 | POST /api/talent/upgrade, POST /api/talent/claim-milestone, GET /api/talent/status |
| EquipmentService | 5 | POST /api/equipment/enhance, POST /api/equipment/forge, POST /api/equipment/equip, POST /api/equipment/unequip, POST /api/equipment/sell |
| ChapterService | 8 | POST /api/chapter/start, POST /api/chapter/encounter, POST /api/chapter/encounter/resolve, POST /api/chapter/skill/select, POST /api/chapter/skill/reroll, POST /api/chapter/abandon, GET /api/chapter/state, POST /api/chapter/treasure/claim |
| PetService | 3 | POST /api/pet/feed, POST /api/pet/upgrade, POST /api/pet/equip |
| HeritageService | 2 | POST /api/heritage/upgrade, GET /api/heritage/status |
| DailyService | 4 | POST /api/daily/attendance/claim, GET /api/daily/attendance, POST /api/daily/quest/claim, GET /api/daily/quest |
| ContentService | 7 | POST /api/content/tower/challenge, POST /api/content/dungeon/enter, POST /api/content/dungeon/result, POST /api/content/travel/start, POST /api/content/travel/complete, POST /api/content/goblin/mine, POST /api/content/catacomb/run |

### 기존 서비스 확장 (3개)

| 서비스 | 신규 API 수 | 엔드포인트 |
|--------|------------|-----------|
| GachaService | 3 (스텁 -> 실제 구현) | POST /api/gacha/pull, POST /api/gacha/pull10, GET /api/gacha/pity |
| ShopService | 4 | POST /api/shop/consume, GET /api/shop/subscription, POST /api/shop/rtdn, POST /api/shop/s2s-notification |
| ArenaService | 3 | POST /api/arena/defense, GET /api/arena/season, POST /api/arena/retry |

### 신규 DB 모델 (10개)

ResourceBalance, ResourceLedger, TalentState, EquipmentEntry, ChapterSession, ChapterProgress, GachaPity, PetEntry, HeritageState, DailyAttendance, QuestProgress, ContentProgress

### 신규 Repository 인터페이스 + 구현체 (8개)

IResourceRepository, ITalentRepository, IEquipmentRepository, IChapterRepository, IGachaRepository, IPetRepository, IHeritageRepository, IDailyRepository, IContentRepository

### 검증

- `dotnet build`: 0 Warning, 0 Error

### 미구현 항목

- AntiCheatService (미들웨어/크론잡) -- 별도 인프라 작업 필요
- BattleVerifier 전투 재현 -- Domain 어셈블리 서버 참조 필요

---

## 2026-03-07 (2) - 서버 연동 클라이언트 네트워크 레이어 구현

### 개요

서버에 구현된 API(인증, 세이브 동기화, 아레나, 상점, 전투 검증)에 대응하는 클라이언트 네트워크 레이어를 구현했다.
오프라인 폴백을 지원하여 서버 없이도 기존 로컬 게임 플레이가 유지된다.

### 신규 어셈블리

**CatCatGo.Shared** (`Assets/_Project/Scripts/Shared/`)
- 서버 Shared DTO를 Unity C#9 호환 형태로 포팅
- 요청: `AuthRequests`, `SaveSyncRequest`, `PurchaseRequest`, `BattleRequests`, `ArenaRequests`
- 응답: `LoginResponse`, `SaveSyncResponse`, `ArenaResponses`, `BattleResponses`, `ShopResponses`
- `noEngineReferences: true` - 순수 C# DTO, Unity 의존성 없음

**CatCatGo.Network** (`Assets/_Project/Scripts/Network/`)
- `ApiClient`: `UnityWebRequest` 기반 HTTP 클라이언트, JWT 토큰 자동 갱신, 리트라이, 오프라인 감지
- `TokenStore`: JWT 토큰 `PlayerPrefs` 기반 영속 저장
- `ServerConfig`: `ScriptableObject` 기반 서버 설정 (BaseUrl, 타임아웃, 리트라이 횟수)
- `ApiResponse<T>`: 성공/실패/오프라인 3-state 응답 래퍼
- API 래퍼: `AuthApi`, `SaveApi`, `ArenaApi`, `ShopApi`, `BattleApi`

### Services 레이어 변경

**ServerSyncService** (`Assets/_Project/Scripts/Services/ServerSyncService.cs`)
- 앱 시작 시 기기 기반 자동 로그인 (실패 시 자동 회원가입)
- 로컬 세이브 저장 시 서버 동기화 플래그 설정, 120초 간격 자동 동기화
- 서버 세이브가 로컬보다 최신이면 서버 데이터로 복원
- 아레나(매칭/결과/랭킹), 상점(카탈로그/구매검증), 전투(세션등록/결과리포트) API 호출 메서드 제공
- 앱 백그라운드 진입 시 보류 중인 세이브 동기화

**GameManager.SaveGame()** 수정
- 로컬 저장 성공 시 `ServerSyncService.MarkSaveDirty()` 호출하여 서버 동기화 트리거

**GameBootstrap** 수정
- `ApiClient`, `ServerSyncService` MonoBehaviour 자동 생성 추가

### asmdef 참조 변경

| 어셈블리 | 추가된 참조 |
|----------|-------------|
| CatCatGo.Services | CatCatGo.Network, CatCatGo.Shared |
| CatCatGo.Presentation | CatCatGo.Network, CatCatGo.Shared |
| CatCatGo.Editor | CatCatGo.Network, CatCatGo.Shared |
| CatCatGo.Tests.Editor | CatCatGo.Network, CatCatGo.Shared |

### 오프라인 폴백 설계

- `ApiClient.IsOnline`: 30초 주기 서버 ping으로 연결 상태 감지
- `ServerSyncService.State`: Offline/Connecting/Online 3-state 관리
- 모든 서버 API 호출에 오프라인 시 로컬 폴백 경로 제공
- 기존 로컬 세이브 시스템(`SaveManager`)은 변경 없이 유지

### 검증

- Unity batch mode 컴파일: 에러 0건, 경고 0건 (return code 0)

---

## 2026-03-07

### 리소스 참조 검증 Editor 도구 추가

**파일**: `Assets/_Project/Scripts/Editor/ResourceValidator.cs`

Unity Editor 메뉴 `Tools/Resource Validator`에서 실행 가능한 리소스 검증 도구 개발.

**검증 항목**:
- Character Resources: enemy.data.json 기반 Chars 폴더/애니메이션 프레임 존재 및 연속성
- Status Effect Icons: 9종 상태이펙트 아이콘 존재
- Equipment Icons: 7 슬롯 x 6 등급 장비 아이콘 존재
- Skill Icons: active-skill-tier.data.json 기반 스킬 아이콘 존재
- JSON Data Files: Data/Json 원본과 Resources 복사본 존재/동기화
- Unused Character Resources: 데이터에서 참조하지 않는 Chars 폴더 탐지

**발견 사항**:
- `spider` 캐릭터 폴더가 Chars에 존재하나 enemy.data.json에 참조 없음

### EnemyTable 죽은 코드 삭제 및 pools/baseStats 폴백 정리

**QA BUG-001/BUG-002 대응**

**삭제 항목**:
- `enemy.data.json`에서 `pools`, `baseStats` 섹션 삭제 (Resources 복사본 동기화)
- `EnemyTable.cs`에서 미사용 메서드 삭제: `GetBaseEnemyStats()`, `GetBaseBossStats()`, `GetChapterEnemyPool()`, `GetChapterBossPool()`, `GetRandomEliteId()`
- `EnemyTable.cs`에서 미사용 필드 삭제: `_enemyPool`, `_elitePool`, `_bossPool`, `_baseEnemyStats`, `_baseBossStats`

**변경 항목**:
- 적 풀을 `pools` JSON 하드코딩(Theme 1만) 대신 `templates` 기반 동적 생성으로 변경
  - `_allEnemyIds`: isBoss=false 이고 dungeon_ 접두사 아닌 전체 적
  - `_allBossIds`: isBoss=true 이고 dungeon_ 접두사 아닌 전체 보스
- `GetEnemyPoolForChapter()`, `GetElitePoolForChapter()`, `GetBossPoolForChapter()` 폴백을 전체 풀로 변경
- `GetBossAssignmentForChapter()` 폴백에서 전체 풀 사용
- `GetRandomEnemyId()`, `GetRandomBossId()`가 전 테마 적에서 선택하도록 변경
  - Tower, CatacombDungeon에서 호출 시 Theme 1에 국한되지 않음

**파일**:
- `Assets/_Project/Scripts/Domain/Data/EnemyTable.cs`
- `Assets/_Project/Data/Json/enemy.data.json`
- `Assets/Resources/_Project/Data/Json/enemy.data.json`

### Tower/CatacombDungeon 하드코딩 밸런스 수치 데이터 테이블 분리

**CLAUDE.md 규칙 "밸런스 수치 코드 하드코딩 금지" 대응**

**데이터 테이블 추가** (`dungeon.data.json`에 tower/catacomb 섹션):
- tower: maxFloor, stagesPerFloor, rewardStages, goldPerFloor(stage5/stage10), rewardPerStage(stage5/stage10 보상 목록)
- catacomb: battlesPerFloor, goldPerFloor, baseEquipmentStone

**코드 변경**:
- `DungeonDataTable.cs`: TowerConfig, CatacombConfig 클래스 추가, JSON 로딩, Tower/Catacomb 프로퍼티
- `Tower.cs`: MaxFloor, StagesPerFloor를 readonly 필드 → DungeonDataTable.Tower 참조 프로퍼티로 변경, GetReward()에서 데이터 테이블 기반 동적 보상 생성
- `CatacombDungeon.cs`: BattlesPerFloor를 readonly 필드 → DungeonDataTable.Catacomb 참조 프로퍼티로 변경, GetFloorReward()에서 데이터 테이블 수치 사용

**파일**:
- `Assets/_Project/Scripts/Domain/Data/DungeonDataTable.cs`
- `Assets/_Project/Scripts/Domain/Content/Tower.cs`
- `Assets/_Project/Scripts/Domain/Content/CatacombDungeon.cs`
- `Assets/_Project/Data/Json/dungeon.data.json`
- `Assets/Resources/_Project/Data/Json/dungeon.data.json`

### 가챠 시스템 확장 - 장비 상자 3종 + 펫 2종

**배경**: 상점화면 기획서에 정의된 가챠 콘텐츠(모험가 상자, 영웅상자, 일반 펫뽑기)가 기획서/데이터/코드에 반영되지 않은 상태를 보완

**기획서 보완**:
- `06_가챠시스템.md`: 보석 장비뽑기(COMMON 260), 모험가 상자(은열쇠), 영웅상자(금열쇠), 우수 펫뽑기, 일반 펫뽑기 5종 체계로 전면 재작성
- `07_재화시스템.md`: 은열쇠/금열쇠 재화 추가
- `11_과금시스템.md`: 보물상자 키 패키지에 은열쇠/금열쇠 명시
- `상점화면_기획서.md`: 장비상점 3종 카드(보석 장비뽑기, 모험가 상자, 영웅상자) 반영

**데이터 확장** (`gacha.data.json`):
- equipment: COMMON 가중치 243→260 변경
- adventurerChest: 은열쇠 1개, COMMON(3)/UNCOMMON(1), S등급/천장 없음
- heroChest: 금열쇠 1개, UNCOMMON(9)/RARE(3)/EPIC(1), S등급/천장 없음
- basicPet: 펫 알 1개, 사료 0~2개
- 전 섹션에 costCurrency 필드 추가

**Enum 확장** (`GameEnums.cs`):
- ChestType: ADVENTURER, HERO, BASIC_PET 추가
- ResourceType: SILVER_KEY, GOLD_KEY 추가

**GachaDataTable 재설계** (`GachaDataTable.cs`):
- sRate/sEligibleGrades를 전역 static에서 GachaChestConfig 내부 필드로 이동
- GachaChestConfig/GachaPetConfig에 CostCurrency 필드 추가
- Equipment, AdventurerChest, HeroChest, Pet, BasicPet 5개 static 프로퍼티 노출
- ParseChestConfig/ParsePetConfig 헬퍼로 중복 파싱 제거

**TreasureChest 일반화** (`TreasureChest.cs`):
- EQUIPMENT/PET 하드코딩 분기를 GetChestConfig()/GetPetConfig() config 기반 패턴으로 변경
- GetCostCurrency() 메서드 추가 (config의 costCurrency를 ResourceType으로 파싱)
- Pull()에서 config별 sRate/sEligibleGrades 사용

**GameState/GameManager 확장**:
- GameState: AdventurerChestSystem, HeroChestSystem 필드 추가
- GameManager: GetChestSystem(ChestType), PullChest/PullChest10, PullChestAsync/PullChest10Async 추가
- 기존 PullGacha/PullGacha10은 PullChest(EQUIPMENT)로 위임

**ShopScreen UI 정리** (`ShopScreen.cs`):
- 기존 개별 pull 메서드(OnSGradePull1/10, OnEquipmentPull1/10 등) 전체 삭제
- ExecuteChestPull(ChestType, bool isTenPull) 단일 메서드로 통합
- GetChestCost1/GetChestCost10 범용 헬퍼로 교체
- 장비상점 3카드(보석 장비뽑기/모험가 상자/영웅상자), 펫상점 2카드(우수 펫/일반 펫) 실제 콜백 연결

**테스트 추가** (`TreasureChestTests.cs`):
- AdventurerChestCosts1SilverKey, AdventurerChestDropsOnlyCommonOrUncommon
- HeroChestCosts1GoldKey, HeroChestDropsOnlyUncommonToEpic
- AdventurerChestHasNoPity, BasicPetChestReturnsPetResources

**파일**:
- `Assets/_Project/Data/Json/gacha.data.json` (+ Resources 복사본)
- `Assets/_Project/Scripts/Domain/Enums/GameEnums.cs`
- `Assets/_Project/Scripts/Domain/Data/GachaDataTable.cs`
- `Assets/_Project/Scripts/Domain/Economy/TreasureChest.cs`
- `Assets/_Project/Scripts/Services/GameState.cs`
- `Assets/_Project/Scripts/Services/GameManager.cs`
- `Assets/_Project/Scripts/Presentation/Screens/ShopScreen.cs`
- `Assets/_Project/Tests/Editor/Economy/TreasureChestTests.cs`
- `docs/planning-agent/SystemDesign/06_가챠시스템.md`
- `docs/planning-agent/SystemDesign/07_재화시스템.md`
- `docs/planning-agent/SystemDesign/11_과금시스템.md`
- `docs/planning-agent/SystemDesign/화면기획/상점화면_기획서.md`

### 컴파일 에러 해결: asmdef 참조 + BattleManagerTests 수정

**원인**: Newtonsoft.Json을 사용하는 코드가 Services/Tests 어셈블리에 있으나 asmdef에 참조가 누락되어 컴파일 실패. BattleManagerTests에서 `List<T>.Length` (배열 전용) 대신 `List<T>.Count`를 사용해야 하는 오류.

**수정 항목**:
- `CatCatGo.Services.asmdef`: `com.unity.nuget.newtonsoft-json` 참조 추가
- `CatCatGo.Tests.Editor.asmdef`: `com.unity.nuget.newtonsoft-json` 참조 추가 + `precompiledReferences`에 `Newtonsoft.Json.dll` 추가 (overrideReferences=true일 때 필수)
- `BattleManagerTests.cs`: `unit.PassiveSkills.Length` -> `unit.PassiveSkills.Count` (List 타입은 Count 프로퍼티 사용)

**누락 .meta 파일 추가**:
- 이전 커밋에서 .cs 파일은 추가했으나 Unity .meta 파일이 누락된 10개 파일 보완

**파일**:
- `Assets/_Project/Scripts/Services/CatCatGo.Services.asmdef`
- `Assets/_Project/Tests/Editor/CatCatGo.Tests.Editor.asmdef`
- `Assets/_Project/Tests/Editor/Services/BattleManagerTests.cs`
