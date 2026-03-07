# 개발일지 - dev-agent

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
