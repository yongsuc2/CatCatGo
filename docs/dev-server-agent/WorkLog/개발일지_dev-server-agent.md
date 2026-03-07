# dev-server-agent 개발일지

## 2026-03-07: Phase 4-5 서버 API 설계 문서 스펙 정비

### 작업 목표
서버 API를 `클라이언트_서버연동_설계_상세.md` 설계 문서 스펙에 맞게 전면 정비

### 해결한 문제 4가지

#### 1. StateDelta 응답 포맷 도입
- **이전**: 서비스가 raw result 타입 반환 (StateDelta 없음)
- **이후**: 모든 서비스가 `ApiResponse<T>` 반환, `StateDelta` 포함
- 신규 파일:
  - `Server/src/CatCatGo.Shared/Models/StateDelta.cs` — 클라이언트와 동일한 delta 모델
  - `Server/src/CatCatGo.Shared/Models/ApiResponse.cs` — `{ Success, Error, ErrorCode, Data, Delta }` 공통 응답
  - `Server/src/CatCatGo.Server.Core/Services/StateDeltaBuilder.cs` — Fluent builder 패턴

#### 2. Request/Response 타입 정렬
- `EquipmentId`: `Guid` → `string`
- `PetId`: `Guid` → `string`
- `ForgeRequest.EquipmentIds`: `List<Guid>` → `List<string>`
- 모든 컨트롤러 Request DTO 타입을 설계 문서와 일치시킴

#### 3. API 경로 정렬
| 이전 | 이후 (설계 문서) |
|------|-----------------|
| `/api/equipment/enhance` | `/api/equipment/upgrade` |
| `/api/pet/upgrade` | `/api/pet/hatch` |
| `/api/pet/equip` | `/api/pet/deploy` |
| `/api/chapter/encounter` | `/api/chapter/advance-day` |
| `/api/chapter/encounter/resolve` | `/api/chapter/resolve-encounter` |
| `/api/chapter/skill/select` | `/api/chapter/select-skill` |
| `/api/chapter/skill/reroll` | `/api/chapter/reroll` |
| `/api/chapter/treasure/claim` | `/api/treasure/claim` (분리) |
| `/api/content/dungeon/enter` + `result` | `/api/content/dungeon/challenge` (통합) |
| `/api/content/catacomb/run` | `start` + `battle` + `end` (분할) |

#### 4. 누락 API 추가
- `POST /api/talent/claim-all-milestones` — 마일스톤 일괄 수령
- `POST /api/equipment/bulk-forge` — 벌크 합성
- `POST /api/content/dungeon/sweep` — 던전 소탕
- `POST /api/content/goblin/cart` — 광석 교환
- `POST /api/content/catacomb/battle` — 카타콤 전투
- `POST /api/content/catacomb/end` — 카타콤 종료
- `POST /api/chapter/battle-result` — 전투 결과
- `POST /api/daily/quest/claim-all` — 퀘스트 일괄 수령
- `POST /api/treasure/claim` — 보물 수령 (ChapterService에서 분리)

### 변경 파일 목록

**신규 (5개)**
- `Server/src/CatCatGo.Shared/Models/StateDelta.cs`
- `Server/src/CatCatGo.Shared/Models/ApiResponse.cs`
- `Server/src/CatCatGo.Server.Core/Services/StateDeltaBuilder.cs`
- `Server/src/CatCatGo.Server.Core/Services/TreasureService.cs`
- `Server/src/CatCatGo.Server.Api/Controllers/TreasureController.cs`

**서비스 수정 (8개)**
- `TalentService.cs`, `EquipmentService.cs`, `PetService.cs`, `GachaService.cs`
- `ChapterService.cs`, `ContentService.cs`, `DailyService.cs`, `HeritageService.cs`

**컨트롤러 수정 (8개)**
- `TalentController.cs`, `EquipmentController.cs`, `PetController.cs`, `GachaController.cs`
- `ChapterController.cs`, `ContentController.cs`, `DailyController.cs`, `HeritageController.cs`

**테스트 수정 (8개)**
- 모든 서비스 테스트를 새 API 시그니처에 맞게 전면 재작성

**기타**
- `Program.cs` — `TreasureService` DI 등록

### 검증 결과
- `dotnet build` — 0 errors, 0 warnings
- `dotnet test` — 128 passed, 0 failed

---

## 2026-03-07: Health Check Endpoint 추가 + Register Race Condition 수정

### 배경
- 클라이언트 `ApiClient.PingServer()`가 `HEAD /`로 서버 상태를 확인하지만, 서버에 해당 endpoint가 없어 404 반환
- `RegisterAsync()`에서 `GetByDeviceIdAsync` → `CreateAsync` 사이 동시 요청 시 `IX_accounts_DeviceId` unique constraint 위반으로 500 에러 발생

### 변경 내용

**1. Health Check Endpoint (Program.cs)**
- `app.MapMethods("/", new[] { "GET", "HEAD" }, () => Results.Ok())` 추가
- `MapGet`은 HEAD를 자동 처리하지 않아 405가 반환되므로 `MapMethods`로 GET/HEAD 명시적 처리

**2. Register Race Condition (AuthService.cs)**
- `CreateAsync` 호출을 try-catch로 감싸서 unique constraint 위반 시 기존 계정 재조회 후 반환
- Core 레이어에 EF Core 의존성이 없으므로 일반 `Exception` catch 후 `GetByDeviceIdAsync`로 재조회하여 conflict 여부 판단
- conflict가 아닌 경우 예외를 재throw

### 검증
- `dotnet build`: 성공 (0 Warning, 0 Error)
- `dotnet test`: 141개 테스트 전부 통과
- `docker compose build && docker compose up -d`: 정상 기동
- `HEAD /`: 200 OK
- `GET /`: 200 OK
- `POST /api/auth/register`: 정상 등록 및 중복 deviceId 기존 계정 반환 확인

---

## 2026-03-07: BUG-010 RewardData 구조/타입 불일치 수정 + BUG-008 서버 응답 패턴 통일

### BUG-010: RewardData 서버-클라이언트 구조/타입 불일치

**문제:**
- 서버 `RewardData`: `{ Type(string), Amount(double) }` — 단일 보상
- 클라이언트 `RewardData`: `{ Resources: List<RewardResourceData> }` — 래퍼 구조, `Amount(int)`
- 역직렬화 실패: 서버가 `{"type":"GOLD","amount":1000}`을 보내면 클라이언트 `Resources`는 null

**수정 내용 (`ContentService.cs`):**
- `RewardData` 클래스를 클라이언트 래퍼 구조로 변경: `{ Resources: List<RewardResourceData> }`
- `RewardResourceData` 클래스 추가: `{ Type(string), Amount(int) }`
- `RewardData.From(type, amount)` 팩토리 메서드 추가
- goldReward 계산을 `double` → `int`로 변경 (실제 정수값만 사용 확인)
- 모든 사용처 수정: TowerChallenge, DungeonChallenge, DungeonSweep, GoblinCart, CatacombBattle, CatacombEnd

**JSON 응답 형태 변경:**
- Before: `{"reward":{"type":"GOLD","amount":1000}}`
- After: `{"reward":{"resources":[{"type":"GOLD","amount":1000}]}}`

### BUG-008: 서버 응답 패턴 이중화

**문제:**
- 패턴 A (Auth, Battle, Save, Arena, Shop): 서비스 에러 시 HTTP 에러 코드 반환
- 패턴 B (Chapter 외 9개): 서비스 에러 시에도 항상 HTTP 200 + `ApiResponse.Success=false` 반환
- 에러 처리 방식 불일치로 유지보수 위험

**수정 내용 (9개 컨트롤러):**
각 컨트롤러에 `ToActionResult()` 헬퍼 추가:
```csharp
private IActionResult ToActionResult<T>(ApiResponse<T> result) =>
    result.Success ? Ok(result) : BadRequest(result);
```
- Chapter, Content, Daily, Equipment, Gacha, Heritage, Pet, Talent, Treasure 컨트롤러의 모든 POST 액션 적용
- GET 엔드포인트(status, state, attendance, quest, pity)는 ApiResponse가 아닌 직접 객체 반환이므로 변경 없음

**클라이언트 대응 필요:**
- ApiClient가 HTTP 400 body를 역직렬화하지 않아 ErrorCode 유실 → Task #10 (dev-agent 담당)

### 검증
- `dotnet build` — 0 Warning, 0 Error
- `dotnet test` — 128개 테스트 전부 통과
