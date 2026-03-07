# BUG-011: SyncApi.Sync()에서 SaveSyncRequest.Version 미설정 (항상 0)

## 상태
Fixed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
서버-클라이언트 비즈니스 로직 연동 검증(Task #5) 수행 중, SyncApi.Sync() 호출 로직을 검토하여 발견.

## 증상
`SyncApi.Sync()` 메서드에서 `SaveSyncRequest` 생성 시 `Version` 필드를 설정하지 않아 항상 기본값 0으로 전송된다.

반면 `SaveApi.Sync()` 메서드는 version 파라미터를 받아 설정한다.

**SyncApi** (`Assets/_Project/Scripts/Network/SyncApi.cs` line 16-25):
```csharp
public static void Sync(string data, long clientTimestamp, Action<ApiResponse<SaveSyncResponse>> callback)
{
    var request = new SaveSyncRequest
    {
        Data = data,
        ClientTimestamp = clientTimestamp,
        Checksum = ComputeChecksum(data),
        // Version 미설정 -> 0
    };
    ApiClient.Instance.Post("api/save/sync", request, callback);
}
```

**SaveApi** (`Assets/_Project/Scripts/Network/SaveApi.cs` line 16-26):
```csharp
public static void Sync(string saveDataJson, int version, Action<ApiResponse<SaveSyncResponse>> callback)
{
    var request = new SaveSyncRequest
    {
        Data = saveDataJson,
        ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Version = version,
        Checksum = ComputeChecksum(saveDataJson)
    };
}
```

## 원인
SyncApi가 SaveApi와 중복 구현된 것으로 보이며, SyncApi 작성 시 Version 파라미터가 메서드 시그니처에서 누락됨. ServerSyncService.SyncSaveToServer()는 SyncApi.Sync()를 호출하므로 실제 동기화에서 Version이 항상 0.

## 영향 범위
- 서버에서 Version 기반 충돌 감지 로직이 있다면 동기화 실패 또는 예기치 않은 동작 발생 가능
- 두 개의 동일 목적 API 클래스 존재 (SaveApi, SyncApi): 코드 혼란 유발

## 관련 파일
- 코드(클라이언트): `Assets/_Project/Scripts/Network/SyncApi.cs` (line 16-25)
- 코드(클라이언트): `Assets/_Project/Scripts/Network/SaveApi.cs` (line 16-26)
- 코드(클라이언트): `Assets/_Project/Scripts/Services/ServerSyncService.cs` (line 157, SyncApi.Sync 호출부)

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-agent | SyncApi.Sync()에 version 파라미터 추가, ServerSyncService에서 version 추적/전달 구현, 미사용 SaveApi.cs 삭제 | - |
