# dev-server-agent 개발일지

## 2026-03-07: Health Check Endpoint 추가 + Register Race Condition 수정

### 배경
- 클라이언트 `ApiClient.PingServer()`가 `HEAD /`로 서버 상태를 확인하지만, 서버에 해당 endpoint가 없어 404 반환
- `RegisterAsync()`에서 `GetByDeviceIdAsync` → `CreateAsync` 사이 동시 요청 시 `IX_accounts_DeviceId` unique constraint 위반으로 500 에러 발생

### 변경 내용

**1. Health Check Endpoint (Program.cs)**
- `app.MapGet("/", () => Results.Ok())` 추가
- `MapGet`은 HEAD 요청도 자동 처리하므로 클라이언트 ping 동작 호환

**2. Register Race Condition (AuthService.cs)**
- `CreateAsync` 호출을 try-catch로 감싸서 unique constraint 위반 시 기존 계정 재조회 후 반환
- Core 레이어에 EF Core 의존성이 없으므로 일반 `Exception` catch 후 `GetByDeviceIdAsync`로 재조회하여 conflict 여부 판단
- conflict가 아닌 경우 예외를 재throw

### 검증
- `dotnet build`: 성공 (0 Warning, 0 Error)
- `dotnet test`: 141개 테스트 전부 통과
