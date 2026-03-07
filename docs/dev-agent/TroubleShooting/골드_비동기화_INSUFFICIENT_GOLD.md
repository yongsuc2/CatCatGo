# 골드 비동기화 (INSUFFICIENT_GOLD)

## 발생일
2026-03-08

## 환경
- Unity 클라이언트 <-> ASP.NET Core 서버 (Docker)
- 관련 커밋: 220d9e9d

## 증상
- 클라이언트 UI에 충분한 골드가 표시되어 있으나, 골드 소비 요청(장비 강화, 가챠 등) 시 서버가 `INSUFFICIENT_GOLD` 에러를 반환
- 특히 마일스톤 보상 수령, 퀘스트 보상 등 서버측에서 재화가 변동된 직후 로그인한 경우 발생

**재현 흐름:**
1. 서버에서 재화 변동 발생 (마일스톤 보상, 일일 퀘스트 보상 등)
2. 클라이언트가 로그인하여 `SyncApi.Load()` 호출
3. SyncApi 스냅샷은 최대 120초 전 데이터 (`AutoSyncIntervalSeconds = 120f`, `ServerSyncService.cs` line 28) — 변동 이전 잔고가 반환됨
4. 클라이언트는 스냅샷 기준 골드 잔고를 UI에 표시
5. 골드 소비 요청 시 서버 `ResourceService`가 실시간 잔고 기준으로 검증 → 불일치 발생

## 원인
`ServerSyncService.LoadFullSync()` (`ServerSyncService.cs` line 100-137)가 `SyncApi.Load()` 응답의 `SaveState` 스냅샷 데이터만 사용하여 재화 잔고를 복원했다. SyncApi 스냅샷은 마지막 동기화 시점의 전체 게임 상태를 직렬화한 것이므로, 스냅샷 생성 이후 서버에서 발생한 재화 변동(마일스톤 보상 Grant, 퀘스트 보상 등)이 누락된다.

서버 `ResourceService`는 자체적으로 실시간 잔고를 관리하므로, 클라이언트가 스냅샷 기반 잔고를 표시하면 두 값이 불일치하게 된다.

## 해결 방법

### 1. 실시간 잔고 조회 API 추가 (신규 파일)
- **`Assets/_Project/Scripts/Network/ResourceApi.cs`** (신규)
  - `GET api/resource/balance` 엔드포인트 호출
  - `ResourceBalanceResponse`(`Dictionary<string, double>`) 형태로 전체 재화 잔고 수신

- **`Assets/_Project/Scripts/Shared/Responses/ResourceResponse.cs`** (신규)
  - `ResourceBalanceResponse` 클래스 정의

### 2. 로그인 동기화 흐름에 실시간 잔고 동기화 단계 추가
`ServerSyncService`에 `SyncResourceBalances()` 메서드를 추가하여 (`ServerSyncService.cs` line 139-162), `LoadFullSync()` 완료 후 항상 실시간 잔고를 서버에서 가져와 덮어쓰도록 변경.

- `LoadFullSync()`의 모든 분기에서 `SyncResourceBalances()`를 콜백 체인에 포함 (line 106, 112, 135)
- `ResourceApi.GetBalance()` 응답의 각 재화를 `Enum.TryParse<ResourceType>`으로 변환 후 `Player.Resources.SetAmount()`로 반영 (line 148-151)
- 잔고 조회 실패 시 스냅샷 데이터를 유지하고 경고 로그 출력 (line 158)

### 수정 후 동기화 흐름
```
InitializeConnection() -> AuthApi.AutoLogin() -> LoadFullSync()
  LoadFullSync()
    SyncApi.Load() 호출
      ├─ 실패/빈 데이터 → SyncResourceBalances() → onComplete
      └─ 성공 → SaveState.ApplyFullSync() → SyncResourceBalances() → onComplete
                                                └─ ResourceApi.GetBalance()
                                                     └─ 각 재화를 Player.Resources.SetAmount()로 덮어쓰기
```

## 예방 방법
- 재화 관련 기능 추가 시, 클라이언트 표시 잔고와 서버 실시간 잔고의 동기화 시점을 반드시 검증
- SyncApi 스냅샷의 `AutoSyncIntervalSeconds`(120초) 내에 서버측 재화 변동이 발생할 수 있음을 전제로 설계
- 서버에서 재화를 Grant/Spend하는 모든 경로에서 delta 응답을 통해 클라이언트 잔고가 갱신되는지 확인

## 관련 문서
- `Assets/_Project/Scripts/Services/ServerSyncService.cs` (LoadFullSync, SyncResourceBalances)
- `Assets/_Project/Scripts/Network/ResourceApi.cs` (GetBalance)
- `Assets/_Project/Scripts/Shared/Responses/ResourceResponse.cs` (ResourceBalanceResponse)
- `docs/planning-agent/SystemDesign/07_재화시스템.md`
- `docs/dev-agent/TroubleShooting/신규계정_스태미나부족.md` (동일 패턴: 서버-클라이언트 재화 불일치)
