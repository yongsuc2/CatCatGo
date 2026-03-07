# SaveSync 400 에러 무한 재시도

## 발생일
2026-03-07

## 환경
- Unity 클라이언트 ↔ ASP.NET Core 서버 (Docker)
- 관련 파일: SyncApi.cs, ServerSyncService.cs, SaveController.cs

## 증상
- 서버 로그에 `POST api/save/sync → 400` 이 초당 수십 회 반복 출력
- 서버 로그가 읽을 수 없을 정도로 폭주
- 결과적으로 클라이언트가 오프라인 전환됨

## 원인
2가지 원인이 복합적으로 작용:

1. **Checksum 누락**: 서버 `SaveSyncRequest`에 `required string Checksum` 필드가 있으나, 클라이언트 `SyncApi.Sync()`에서 Checksum을 계산/전송하지 않음. ASP.NET Core의 System.Text.Json이 `required` 키워드 미충족으로 400 ValidationError 반환.
   - 서버: `Server/src/CatCatGo.Shared/Requests/SaveSyncRequest.cs` → `public required string Checksum { get; set; }`
   - 클라이언트: `Assets/_Project/Scripts/Network/SyncApi.cs` → Checksum 미설정

2. **실패 시 재시도 간격 미적용**: `ServerSyncService.SyncSaveToServer()`에서 sync 실패 시 `_lastSyncTime`을 갱신하지 않음. `Update()`의 조건 `Time.realtimeSinceStartup - _lastSyncTime < AutoSyncIntervalSeconds`가 항상 통과하여 매 프레임 재시도.
   - `Assets/_Project/Scripts/Services/ServerSyncService.cs` 의 else 분기에서 `_lastSyncTime` 미갱신

## 해결 방법
1. `SyncApi.Sync()`에 SHA256 Checksum 계산 추가 (서버의 `SaveService.ComputeChecksum`과 동일한 방식: `SHA256 → hex lowercase`)
2. `ServerSyncService.SyncSaveToServer()`의 실패 분기(IsOffline, else)에서도 `_lastSyncTime = Time.realtimeSinceStartup` 설정

## 예방 방법
- 서버의 `required` 필드가 추가되면 클라이언트의 Request DTO도 반드시 동기화
- API 실패 시 재시도 로직에는 반드시 backoff/cooldown 적용 (실패해도 타이머 갱신)

## 관련 문서
- Server/src/CatCatGo.Server.Core/Services/SaveService.cs (ComputeChecksum 구현)
- Assets/_Project/Scripts/Network/SyncApi.cs
- Assets/_Project/Scripts/Services/ServerSyncService.cs
