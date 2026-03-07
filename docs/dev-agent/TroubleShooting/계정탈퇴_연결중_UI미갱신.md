# 계정 탈퇴 후 서버 상태 "연결중..." 유지

## 발생일
2026-03-07

## 환경
- Unity 클라이언트, 설정 화면
- 관련 파일: SettingsScreen.cs, ServerSyncService.cs

## 증상
- 계정 탈퇴 → 새 계정 자동 생성 후, 서버 상태 UI가 "연결중..."으로 고정
- 실제로는 서버 연결이 정상 완료되었으나 UI에 반영되지 않음

## 원인
`SettingsScreen.OnDeleteAccount`의 타이밍 문제:

1. 계정 탈퇴 성공 → `AuthApi.AutoLogin` 호출 → 새 계정 생성 성공
2. `ServerSyncService.Instance.RetryConnection()` 호출 (비동기 코루틴 시작)
3. `UI.Refresh()` 즉시 호출 → 이 시점에 `ServerSyncService.State == Connecting`
4. 코루틴이 완료되어 `State = Online`으로 전환되어도 UI 갱신 트리거 없음

`SettingsScreen`이 `ServerSyncService.OnConnectionStateChanged` 이벤트를 **구독하지 않아서** 상태 변경이 UI에 반영되지 않았음.

## 해결 방법
`SettingsScreen`에 `OnEnable/OnDisable`에서 `ServerSyncService.OnConnectionStateChanged` 이벤트 구독/해제 추가. 상태 변경 시 `Refresh()` 호출.

```csharp
private void OnEnable()
{
    if (ServerSyncService.Instance != null)
        ServerSyncService.Instance.OnConnectionStateChanged += OnConnectionStateChanged;
}

private void OnDisable()
{
    if (ServerSyncService.Instance != null)
        ServerSyncService.Instance.OnConnectionStateChanged -= OnConnectionStateChanged;
}

private void OnConnectionStateChanged(ConnectionState newState)
{
    Refresh();
}
```

## 예방 방법
- 비동기 상태를 표시하는 UI는 반드시 해당 상태의 변경 이벤트를 구독할 것
- `Refresh()`를 한 번 호출하는 것으로 비동기 상태의 최종 결과를 보장할 수 없음

## 관련 문서
- Assets/_Project/Scripts/Presentation/Screens/SettingsScreen.cs
- Assets/_Project/Scripts/Services/ServerSyncService.cs
