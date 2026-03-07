# 서버 API 스펙

## 작성일
2026-03-07

## 개요
CatCatGo 게임 서버 REST API 전체 스펙. 서버 코드 변경 시 이 문서도 함께 업데이트할 것.

- **Base URL**: `http://localhost:5000` (개발), 운영 환경은 별도
- **인증**: JWT Bearer Token (`Authorization: Bearer {token}`)
- **Swagger UI**: `{Base URL}/swagger`

---

## 공통 응답 형식

모든 게임 API 응답은 `ApiResponse<T>` 래퍼를 사용합니다.

### 성공 응답
```json
{
  "success": true,
  "data": { ... },
  "delta": {
    "resources": { "GOLD": 1500.0 },
    "talent": { "atkLevel": 5 },
    "serverTimestamp": 1741305600000
  }
}
```

### 실패 응답
```json
{
  "success": false,
  "errorCode": "INSUFFICIENT_GOLD",
  "error": "INSUFFICIENT_GOLD"
}
```

### StateDelta 필드 목록
| 필드 | 타입 | 설명 |
|------|------|------|
| `resources` | `Dictionary<string, float>?` | 변동된 재화의 현재 잔액 |
| `talent` | `TalentDelta?` | 재능 레벨 변동 |
| `heritage` | `HeritageDelta?` | 전승 루트/레벨 변동 |
| `addedEquipments` | `List<EquipmentDeltaData>?` | 새로 획득한 장비 |
| `removedEquipmentIds` | `List<string>?` | 삭제된 장비 ID |
| `upgradedEquipments` | `List<EquipmentUpgradeDelta>?` | 강화된 장비 |
| `equipmentSlotChanges` | `List<EquipSlotDelta>?` | 장비 슬롯 변동 |
| `addedPets` | `List<PetDeltaData>?` | 새로 획득한 펫 |
| `updatedPets` | `List<PetUpdateDelta>?` | 변동된 펫 정보 |
| `activePetId` | `string?` | 활성 펫 변경 |
| `clearedChapterMax` | `int?` | 최고 클리어 챕터 |
| `bestSurvivalDays` | `Dictionary<string, int>?` | 챕터별 최고 생존일 |
| `addedClaimedMilestones` | `List<string>?` | 새로 수령한 마일스톤 |
| `tower` | `TowerDelta?` | 타워 진행 변동 |
| `catacomb` | `CatacombDelta?` | 카타콤 상태 변동 |
| `dungeons` | `DungeonDelta?` | 던전 진행 변동 |
| `goblinOreCount` | `int?` | 고블린 광석 수량 |
| `pityCount` | `int?` | 가챠 천장 카운터 |
| `addedCollectionIds` | `List<string>?` | 새 컬렉션 ID |
| `missionUpdates` | `List<MissionDelta>?` | 미션 상태 변동 |
| `attendance` | `AttendanceDelta?` | 출석 상태 변동 |
| `chapterSession` | `ChapterSessionDelta?` | 챕터 세션 상태 변동 |
| `serverTimestamp` | `long` | 서버 UTC 타임스탬프 (ms) |

### 주요 HTTP 상태 코드
| 코드 | 의미 |
|------|------|
| 200 | 성공 |
| 400 | 잘못된 요청 (필수 필드 누락 등) |
| 401 | 인증 실패 (토큰 없음/만료/무효) |
| 404 | 리소스 없음 |
| 500 | 서버 내부 오류 |

---

## Auth (인증)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/auth/register` | X | 계정 생성 (DeviceId 기반) |
| POST | `/api/auth/login` | X | 로그인 |
| POST | `/api/auth/refresh` | X | 토큰 갱신 |
| POST | `/api/auth/reset-data` | O | 게임 데이터 초기화 (계정 유지) |
| DELETE | `/api/auth/account` | O | 계정 삭제 (전체 데이터 포함) |

### POST /api/auth/register
- **Request**: `{ "deviceId": string, "displayName": string? }`
- **Response**: `LoginResponse { accountId, accessToken, refreshToken, expiresAt, isNewAccount }`
- **비고**: 동일 DeviceId로 재호출 시 기존 계정 반환 (isNewAccount=false)

### POST /api/auth/login
- **Request**: `{ "deviceId": string }`
- **Response**: `LoginResponse`
- **에러**: 401 (계정 없음 또는 밴)

### POST /api/auth/refresh
- **Request**: `{ "refreshToken": string }`
- **Response**: `LoginResponse`
- **에러**: 401 (유효하지 않은 토큰)

### POST /api/auth/reset-data
- **Request**: 없음
- **Response**: `{ "message": "All game data has been reset." }`
- **동작**: 계정은 유지하고 모든 게임 데이터(세이브, 재화, 장비, 펫, 챕터 등) 삭제

### DELETE /api/auth/account
- **Request**: 없음
- **Response**: `{ "message": "Account has been deleted." }`
- **동작**: 계정 + 모든 관련 데이터 영구 삭제

---

## Save (세이브)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| GET | `/api/save` | O | 세이브 데이터 로드 |
| POST | `/api/save/sync` | O | 세이브 데이터 동기화 |

---

## Resource (재화)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| GET | `/api/resource/balance` | O | 전체 재화 잔액 조회 |
| POST | `/api/resource/spend` | O | 재화 소비 |

---

## Talent (재능/성장)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/talent/upgrade` | O | 스탯 레벨업 |
| POST | `/api/talent/claim-milestone` | O | 마일스톤 보상 수령 |
| POST | `/api/talent/claim-all-milestones` | O | 모든 수령 가능한 마일스톤 일괄 수령 |
| GET | `/api/talent/status` | O | 재능 상태 조회 |

### POST /api/talent/upgrade
- **Request**: `{ "statType": "ATK" | "HP" | "DEF" }`
- **Response**: `ApiResponse<object>` (data 없음, delta에 Resources + Talent)
- **Delta**: `{ resources: { GOLD: float }, talent: { atkLevel?, hpLevel?, defLevel? } }`
- **에러**: `INSUFFICIENT_GOLD`, `MAX_SUB_LEVEL_REACHED`

### POST /api/talent/claim-milestone
- **Request**: `{ "milestoneLevel": int }`
- **Response**: `ApiResponse<TalentMilestoneResponse>` (data: rewardType, rewardAmount)
- **Delta**: `{ addedClaimedMilestones: [string], resources?: { GOLD: float } }`
- **에러**: `LEVEL_NOT_REACHED`, `ALREADY_CLAIMED`

### POST /api/talent/claim-all-milestones
- **Request**: 없음
- **Response**: `ApiResponse<TalentClaimAllResponse>` (data: claimedCount)
- **Delta**: `{ addedClaimedMilestones: [string], resources?: { GOLD: float } }`

---

## Equipment (장비)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/equipment/upgrade` | O | 장비 강화 (구: enhance) |
| POST | `/api/equipment/equip` | O | 장비 장착 |
| POST | `/api/equipment/unequip` | O | 장비 해제 |
| POST | `/api/equipment/sell` | O | 장비 판매 |
| POST | `/api/equipment/forge` | O | 장비 합성 |
| POST | `/api/equipment/bulk-forge` | O | 자동 합성 (전체) |

### POST /api/equipment/upgrade
- **Request**: `{ "equipmentId": string }`
- **Response**: `ApiResponse<object>` (delta에 Resources + UpgradedEquipments)
- **에러**: `EQUIPMENT_NOT_FOUND`, `INSUFFICIENT_GOLD`, `MAX_LEVEL`

### POST /api/equipment/equip
- **Request**: `{ "equipmentId": string }`
- **Response**: `ApiResponse<object>` (delta에 EquipmentSlotChanges)

### POST /api/equipment/unequip
- **Request**: `{ "slotType": string, "slotIndex": int }`
- **Response**: `ApiResponse<object>` (delta에 EquipmentSlotChanges)

### POST /api/equipment/sell
- **Request**: `{ "equipmentId": string }`
- **Response**: `ApiResponse<object>` (delta에 Resources + RemovedEquipmentIds)

### POST /api/equipment/forge
- **Request**: `{ "equipmentIds": [string] }`
- **Response**: `ApiResponse<object>` (delta에 AddedEquipments + RemovedEquipmentIds)

### POST /api/equipment/bulk-forge
- **Request**: 없음
- **Response**: `ApiResponse<BulkForgeResponse>` (data: forgedCount)

---

## Pet (펫)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/pet/hatch` | O | 펫 부화 (구: GachaService.PetPullAsync) |
| POST | `/api/pet/feed` | O | 펫 먹이주기 |
| POST | `/api/pet/deploy` | O | 활성 펫 변경 (구: equip) |

### POST /api/pet/hatch
- **Request**: 없음
- **Response**: `ApiResponse<PetHatchResponse>` (delta에 AddedPets + Resources)
- **에러**: `INSUFFICIENT_GEMS`

### POST /api/pet/feed
- **Request**: `{ "petId": string, "amount": int }`
- **Response**: `ApiResponse<object>` (delta에 UpdatedPets + Resources)
- **에러**: `PET_NOT_FOUND`, `INSUFFICIENT_PET_FOOD`

### POST /api/pet/deploy
- **Request**: `{ "petId": string }`
- **Response**: `ApiResponse<object>` (delta에 ActivePetId)
- **에러**: `PET_NOT_FOUND`

---

## Gacha (가챠)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/gacha/pull` | O | 장비 뽑기 (1~N회) |
| POST | `/api/gacha/pull10` | O | 10연차 |
| GET | `/api/gacha/pity` | O | 천장 카운터 조회 |

### POST /api/gacha/pull
- **Request**: `{ "chestType": "EQUIPMENT", "count": 1 }`
- **Response**: `ApiResponse<GachaPullResponse>` (data: results[], delta에 AddedEquipments + Resources + PityCount)
- **에러**: `INSUFFICIENT_GEMS`

### POST /api/gacha/pull10
- **Request**: 없음
- **Response**: `ApiResponse<GachaPullResponse>` (10개 결과)
- **에러**: `INSUFFICIENT_GEMS`

### GET /api/gacha/pity
- **Response**: `{ pityCount: int, threshold: int }`

---

## Heritage (전승)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/heritage/upgrade` | O | 전승 루트 레벨업 |
| GET | `/api/heritage/status` | O | 전승 상태 조회 |

### POST /api/heritage/upgrade
- **Request**: `{ "route": "SKULL" | "KNIGHT" | "RANGER" | "GHOST" }`
- **Response**: `ApiResponse<object>` (delta에 Heritage + Resources)
- **에러**: `HERITAGE_LOCKED`, `INVALID_ROUTE`, `INSUFFICIENT_{BOOK}`, `INSUFFICIENT_GOLD`

---

## Chapter (모험/챕터)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/chapter/start` | O | 챕터 시작 (스태미나 차감) |
| POST | `/api/chapter/advance-day` | O | 하루 진행 (구: encounter) |
| POST | `/api/chapter/resolve-encounter` | O | 인카운터 해결 (구: encounter/resolve) |
| POST | `/api/chapter/select-skill` | O | 스킬 선택 (구: skill/select) |
| POST | `/api/chapter/reroll` | O | 스킬 리롤 (구: skill/reroll) |
| POST | `/api/chapter/battle-result` | O | 전투 결과 보고 (신규) |
| POST | `/api/chapter/abandon` | O | 챕터 포기 |
| GET | `/api/chapter/state` | O | 현재 챕터 상태 조회 |

### POST /api/chapter/start
- **Request**: `{ "chapterId": int, "chapterType": "SIXTY_DAY" }`
- **Response**: `ApiResponse<ChapterStartResponse>` (data: chapterId, seed, delta에 ChapterSession + Resources)
- **에러**: `SESSION_ALREADY_ACTIVE`, `INSUFFICIENT_STAMINA`

### POST /api/chapter/advance-day
- **Request**: `{ "sessionId": string }`
- **Response**: `ApiResponse<object>` (delta에 ChapterSession)
- **에러**: `NO_ACTIVE_SESSION`

### POST /api/chapter/resolve-encounter
- **Request**: `{ "sessionId": string, "choiceIndex": int }`
- **Response**: `ApiResponse<object>` (delta에 ChapterSession)

### POST /api/chapter/select-skill
- **Request**: `{ "sessionId": string, "skillId": string }`
- **Response**: `ApiResponse<object>` (delta에 ChapterSession)
- **에러**: `INVALID_SKILL_ID`

### POST /api/chapter/reroll
- **Request**: `{ "sessionId": string }`
- **Response**: `ApiResponse<object>` (delta에 ChapterSession)
- **에러**: `NO_REROLLS_REMAINING`

### POST /api/chapter/battle-result
- **Request**: `{ "sessionId": string, "battleSeed": int, "result": string, "turnCount": int, "playerRemainingHp": int }`
- **Response**: `ApiResponse<object>` (delta에 ChapterSession)

### POST /api/chapter/abandon
- **Request**: `{ "sessionId": string }`
- **Response**: `ApiResponse<object>` (delta에 ChapterSession + BestSurvivalDays)

---

## Treasure (보물)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/treasure/claim` | O | 보물 수령 (구: chapter/treasure/claim) |

### POST /api/treasure/claim
- **Request**: `{ "milestoneId": string }`
- **Response**: `ApiResponse<object>` (delta에 AddedClaimedMilestones + Resources)
- **에러**: `MILESTONE_NOT_FOUND`, `ALREADY_CLAIMED`

---

## Content (컨텐츠)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/content/tower/challenge` | O | 타워 도전 |
| POST | `/api/content/dungeon/challenge` | O | 던전 도전 (구: enter+result 통합) |
| POST | `/api/content/dungeon/sweep` | O | 던전 소탕 (신규) |
| POST | `/api/content/goblin/mine` | O | 고블린 광산 채굴 |
| POST | `/api/content/goblin/cart` | O | 광석 교환 (신규) |
| POST | `/api/content/catacomb/start` | O | 카타콤 시작 (구: catacomb/run 분할) |
| POST | `/api/content/catacomb/battle` | O | 카타콤 전투 (신규) |
| POST | `/api/content/catacomb/end` | O | 카타콤 종료 (신규) |

### POST /api/content/tower/challenge
- **Request**: 없음
- **Response**: `ApiResponse<TowerResult>` (delta에 Tower + Resources)
- **에러**: `INSUFFICIENT_CHALLENGE_TOKEN`

### POST /api/content/dungeon/challenge
- **Request**: `{ "dungeonType": string }` (BEEHIVE, TIGER_CLIFF 등)
- **Response**: `ApiResponse<object>` (delta에 Dungeons)
- **에러**: `DAILY_LIMIT_REACHED`

### POST /api/content/dungeon/sweep
- **Request**: `{ "dungeonType": string }`
- **Response**: `ApiResponse<object>` (delta에 Dungeons + Resources)
- **에러**: `NO_CLEAR_RECORD`, `DAILY_LIMIT_REACHED`

### POST /api/content/goblin/mine
- **Request**: 없음
- **Response**: `ApiResponse<GoblinMineResult>` (data: oreGained, delta에 GoblinOreCount + Resources)
- **에러**: `INSUFFICIENT_PICKAXE`

### POST /api/content/goblin/cart
- **Request**: 없음
- **Response**: `ApiResponse<object>` (delta에 GoblinOreCount + Resources)
- **에러**: `INSUFFICIENT_ORE`

### POST /api/content/catacomb/start
- **Request**: 없음
- **Response**: `ApiResponse<object>` (delta에 Catacomb)
- **에러**: `RUN_ALREADY_ACTIVE`

### POST /api/content/catacomb/battle
- **Request**: 없음
- **Response**: `ApiResponse<object>` (delta에 Catacomb)

### POST /api/content/catacomb/end
- **Request**: 없음
- **Response**: `ApiResponse<object>` (delta에 Catacomb + Resources)

---

## Daily (일일)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| GET | `/api/daily/attendance` | O | 출석 상태 조회 |
| POST | `/api/daily/attendance/claim` | O | 출석 보상 수령 |
| GET | `/api/daily/quest` | O | 퀘스트 진행도 조회 |
| POST | `/api/daily/quest/claim` | O | 퀘스트 보상 수령 |
| POST | `/api/daily/quest/claim-all` | O | 퀘스트 일괄 수령 (신규) |

### POST /api/daily/attendance/claim
- **Response**: `ApiResponse<AttendanceClaimResponse>` (data: day, delta에 Attendance + Resources)
- **에러**: `ALREADY_CLAIMED_TODAY`

### POST /api/daily/quest/claim
- **Request**: `{ "eventId": string, "missionId": string }`
- **Response**: `ApiResponse<object>` (delta에 MissionUpdates + Resources)
- **에러**: `QUEST_NOT_FOUND`, `QUEST_NOT_COMPLETED`, `ALREADY_REWARDED`

### POST /api/daily/quest/claim-all
- **Request**: `{ "eventId": string }`
- **Response**: `ApiResponse<QuestClaimAllResponse>` (data: claimedCount)

---

## Battle (전투)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/battle/start` | O | 전투 세션 시작 |
| POST | `/api/battle/report` | O | 전투 결과 보고 (서버 검증) |

---

## Shop (상점/결제)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| GET | `/api/shop/catalog` | O | 상품 목록 |
| POST | `/api/shop/purchase` | O | 구매 (영수증 검증) |
| GET | `/api/shop/history` | O | 구매 이력 |
| POST | `/api/shop/consume` | O | 소모성 상품 사용 |
| GET | `/api/shop/subscription` | O | 월정액 상태 조회 |
| POST | `/api/shop/rtdn` | O | Google RTDN 콜백 |
| POST | `/api/shop/s2s-notification` | O | Apple S2S 콜백 |

---

## Arena (아레나/PvP)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/arena/match` | O | 매칭 요청 |
| POST | `/api/arena/result` | O | 대전 결과 |
| GET | `/api/arena/ranking` | O | 랭킹 조회 (query: season) |
| POST | `/api/arena/defense` | O | 방어 덱 설정 |
| POST | `/api/arena/retry` | O | 보석 재도전 |
| GET | `/api/arena/season` | O | 시즌 정보 |

---

## 미구현 항목

| 항목 | 사유 | 관련 기획서 |
|------|------|-----------|
| AntiCheatService | 미들웨어/크론잡 별도 인프라 필요 | 치트_검증_목록.md |
| BattleVerifier 전투 재현 | Domain 어셈블리 서버 참조 구조 필요 | 01_전투시스템.md |

---

## 삭제된 API (이전 버전에서 제거)

| 이전 엔드포인트 | 사유 |
|----------------|------|
| `POST /api/equipment/enhance` | `upgrade`로 경로 변경 |
| `POST /api/pet/upgrade` | `hatch`로 변경 (펫 부화) |
| `POST /api/pet/equip` | `deploy`로 경로 변경 |
| `POST /api/gacha/pet-pull` | `PetService.HatchAsync`로 이동 |
| `POST /api/chapter/encounter` | `advance-day`로 경로 변경 |
| `POST /api/chapter/encounter/resolve` | `resolve-encounter`로 경로 변경 |
| `POST /api/chapter/skill/select` | `select-skill`로 경로 변경 |
| `POST /api/chapter/skill/reroll` | `reroll`로 경로 변경 |
| `POST /api/chapter/treasure/claim` | `/api/treasure/claim`으로 분리 |
| `POST /api/content/dungeon/enter` | `dungeon/challenge`로 통합 |
| `POST /api/content/dungeon/result` | `dungeon/challenge`로 통합 |
| `POST /api/content/travel/start` | 설계 문서에서 제거 |
| `POST /api/content/travel/complete` | 설계 문서에서 제거 |
| `POST /api/content/catacomb/run` | `start` + `battle` + `end`로 분할 |

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|----------|
| 2026-03-07 | 초기 작성 (60개 API) |
| 2026-03-07 | Phase 4-5: StateDelta 응답 포맷 정비, API 경로 정렬, 누락 API 9개 추가, 삭제 API 14개 기록 |
