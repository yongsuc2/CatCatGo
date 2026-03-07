# 개발일지 - dev-agent

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
