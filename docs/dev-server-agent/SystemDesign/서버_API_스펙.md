# 서버 API 스펙

## 작성일
2026-03-07

## 개요
CatCatGo 게임 서버 REST API 전체 스펙. 서버 코드 변경 시 이 문서도 함께 업데이트할 것.

- **Base URL**: `http://localhost:5000` (개발), 운영 환경은 별도
- **인증**: JWT Bearer Token (`Authorization: Bearer {token}`)
- **Swagger UI**: `{Base URL}/swagger`

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

## Battle (전투)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/battle/start` | O | 전투 세션 시작 |
| POST | `/api/battle/report` | O | 전투 결과 보고 (서버 검증) |

---

## Chapter (모험/챕터)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/chapter/start` | O | 챕터 시작 (스태미나 차감) |
| POST | `/api/chapter/encounter` | O | 인카운터 생성 |
| POST | `/api/chapter/encounter/resolve` | O | 인카운터 해결 |
| POST | `/api/chapter/skill/select` | O | 스킬 선택 |
| POST | `/api/chapter/skill/reroll` | O | 스킬 리롤 |
| POST | `/api/chapter/abandon` | O | 챕터 포기 |
| GET | `/api/chapter/state` | O | 현재 챕터 상태 조회 |
| POST | `/api/chapter/treasure/claim` | O | 보물상자 수령 |

---

## Gacha (가챠)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/gacha/pull` | O | 1회 뽑기 |
| POST | `/api/gacha/pull10` | O | 10연차 |
| GET | `/api/gacha/pity` | O | 천장 카운터 조회 |
| POST | `/api/gacha/pet-pull` | O | 펫 가챠 |

---

## Talent (재능/성장)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/talent/upgrade` | O | 스탯 레벨업 |
| POST | `/api/talent/claim-milestone` | O | 마일스톤 보상 수령 |
| GET | `/api/talent/status` | O | 재능 상태 조회 |

---

## Equipment (장비)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/equipment/enhance` | O | 장비 강화 |
| POST | `/api/equipment/forge` | O | 장비 합성 |
| POST | `/api/equipment/equip` | O | 장비 장착 |
| POST | `/api/equipment/unequip` | O | 장비 해제 |
| POST | `/api/equipment/sell` | O | 장비 판매 |

---

## Pet (펫)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/pet/feed` | O | 펫 먹이주기 |
| POST | `/api/pet/upgrade` | O | 펫 등급업 |
| POST | `/api/pet/equip` | O | 활성 펫 변경 |

---

## Heritage (전승)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/heritage/upgrade` | O | 전승 루트 레벨업 |
| GET | `/api/heritage/status` | O | 전승 상태 조회 |

---

## Daily (일일)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| GET | `/api/daily/attendance` | O | 출석 상태 조회 |
| POST | `/api/daily/attendance/claim` | O | 출석 보상 수령 |
| GET | `/api/daily/quest` | O | 퀘스트 진행도 조회 |
| POST | `/api/daily/quest/claim` | O | 퀘스트 보상 수령 |

---

## Content (컨텐츠)

| Method | Endpoint | 인증 | 설명 |
|--------|----------|------|------|
| POST | `/api/content/tower/challenge` | O | 타워 도전 |
| POST | `/api/content/dungeon/enter` | O | 던전 입장 |
| POST | `/api/content/dungeon/result` | O | 던전 결과 |
| POST | `/api/content/travel/start` | O | 탐방 시작 |
| POST | `/api/content/travel/complete` | O | 탐방 완료 |
| POST | `/api/content/goblin/mine` | O | 고블린 광산 |
| POST | `/api/content/catacomb/run` | O | 카타콤 |

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

## 공통 응답 형식

### 성공
```json
{
  "필드1": "값1",
  "필드2": "값2"
}
```

### 에러
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "traceId": "00-..."
}
```

### 주요 HTTP 상태 코드
| 코드 | 의미 |
|------|------|
| 200 | 성공 |
| 400 | 잘못된 요청 (필수 필드 누락 등) |
| 401 | 인증 실패 (토큰 없음/만료/무효) |
| 404 | 리소스 없음 |
| 500 | 서버 내부 오류 |

---

## 미구현 항목

| 항목 | 사유 | 관련 기획서 |
|------|------|-----------|
| AntiCheatService | 미들웨어/크론잡 별도 인프라 필요 | 치트_검증_목록.md |
| BattleVerifier 전투 재현 | Domain 어셈블리 서버 참조 구조 필요 | 01_전투시스템.md |

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|----------|
| 2026-03-07 | 초기 작성 (60개 API) |
