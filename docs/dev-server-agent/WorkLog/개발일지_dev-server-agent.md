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
