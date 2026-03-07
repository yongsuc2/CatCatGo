# 골드 비동기화 (INSUFFICIENT_GOLD)

## 발생일
2026-03-08

## 환경
- 엔진: Unity 2D (CatCatGo 클라이언트)
- 서버: CatCatGo.Server.Core
- 관련 커밋: 220d9e9d

## 증상
클라이언트에서 골드를 소비하는 요청(장비 강화, 가챠 등)을 보낼 때, 서버가 `INSUFFICIENT_GOLD` 에러를 반환한다. 클라이언트 UI에는 충분한 골드가 표시되어 있으나, 서버의 실시간 잔고와 불일치하여 요청이 거부된다.

**구체적 흐름:**
1. 클라이언트가 로그인 시 `SyncApi.Load`를 호출하여 서버 스냅샷 데이터를 로드
2. SyncApi 스냅샷은 최대 120초 전 데이터이므로, 그 사이 서버에서 발생한 재화 변동이 반영되지 않음
3. 클라이언트는 스냅샷 기준 골드 잔고를 표시하고, 이를 기반으로 요청을 보냄
4. 서버 `ResourceService`는 실시간 잔고 기준으로 검증하므로 잔고 불일치 시 거부

## 원인
`ServerSyncService.LoadFullSync()`가 `SyncApi.Load()` 응답의 스냅샷 데이터만 사용하여 재화 잔고를 복원했다. SyncApi 스냅샷은 마지막 동기화 시점의 `SaveState` 전체를 직렬화한 것이므로, 스냅샷 생성 이후에 서버측에서 발생한 재화 변동(마일스톤 보상, 퀘스트 보상 등)이 누락된다.

**문제 코드 위치:** `Assets/_Project/Scripts/Services/ServerSyncService.cs` - `LoadFullSync()` 메서드

## 해결 방법

### 1. 실시간 잔고 조회 API 추가
서버에 재화 잔고를 실시간으로 조회하는 전용 API 엔드포인트를 신규 생성했다.

- **신규 파일:** `Assets/_Project/Scripts/Network/ResourceApi.cs`
  - `GET api/resource/balance` 엔드포인트 호출
  - `ResourceBalanceResponse` (Dictionary<string, double>) 형태로 전체 재화 잔고 수신

- **신규 파일:** `Assets/_Project/Scripts/Shared/Responses/ResourceResponse.cs`
  - `ResourceBalanceResponse` 클래스 정의 (Balances 딕셔너리)

### 2. 로그인 동기화 흐름에 실시간 잔고 동기화 추가
`ServerSyncService`에 `SyncResourceBalances()` 메서드를 추가하여, `LoadFullSync()` 완료 후 항상 실시간 잔고를 서버에서 가져와 덮어쓰도록 변경했다.

- **수정 파일:** `Assets/_Project/Scripts/Services/ServerSyncService.cs`
  - `SyncResourceBalances()` 메서드 추가
  - `LoadFullSync()`의 모든 분기(성공/실패/빈 데이터)에서 `SyncResourceBalances()`를 콜백 체인에 포함
  - `ResourceApi.GetBalance()` 응답의 각 재화를 `Player.Resources.SetAmount()`로 반영

### 수정 후 동기화 흐름
```
LoadFullSync()
  └─ SyncApi.Load() 호출
       ├─ 실패/빈 데이터 → SyncResourceBalances() → 완료
       └─ 성공 → SaveState 적용 → SyncResourceBalances() → 완료
                                     └─ ResourceApi.GetBalance() 호출
                                          └─ 실시간 잔고로 Player.Resources 덮어쓰기
```

## 예방 방법
- 재화 관련 기능 추가 시, 클라이언트 표시 잔고와 서버 실시간 잔고의 동기화 시점을 반드시 검증
- SyncApi 스냅샷에 의존하는 데이터 중 실시간성이 필요한 항목은 별도 API로 보완 동기화 필요

## 관련 문서
- `docs/planning-agent/SystemDesign/07_재화시스템.md`
- `docs/qa-agent/Bugs/BUG-015_talent-milestone-key-mismatch.md` (마일스톤 보상 미인식으로 인한 연쇄 영향)
