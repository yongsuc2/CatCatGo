# BUG-012: 클라이언트에 ResourceApi 미구현 + LinkSocialRequest 미구현

## 상태
Closed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
서버-클라이언트 API 엔드포인트 매칭 검증(Task #4) 수행 중, 서버 컨트롤러와 클라이언트 API 클래스를 1:1 대조하여 발견.

## 증상

### 1. ResourceApi 미구현
서버에 `ResourceController`(api/resource/balance, api/resource/spend)가 구현되어 있으나, 클라이언트에 대응하는 `ResourceApi.cs`가 존재하지 않는다. 클라이언트에 `ResourceSpendRequest`, `ResourceBalanceResponse`, `ResourceSpendResponse` 모델도 없다.

### 2. LinkSocialRequest 미구현
서버 `LoginRequest.cs`에 `LinkSocialRequest` 클래스가 정의되어 있으나, 클라이언트 `AuthRequests.cs`에 대응 클래스가 없다. 단, 서버 컨트롤러에도 LinkSocial 엔드포인트가 없으므로 실제 기능이 미구현된 것으로 판단.

## 원인
아직 구현되지 않은 기능으로 보이며, 서버 측에서만 먼저 준비된 상태.

## 영향 범위
- 클라이언트에서 재화 잔액 조회/소비 API를 호출할 수 없음
- 현재는 StateDelta를 통해 재화 변경을 처리하므로 당장 심각한 문제는 아니지만, 직접 재화 API 호출이 필요한 경우 문제됨

## 관련 파일
- 코드(서버): `Server/src/CatCatGo.Server.Api/Controllers/ResourceController.cs`
- 코드(서버): `Server/src/CatCatGo.Shared/Requests/ResourceRequest.cs`
- 코드(서버): `Server/src/CatCatGo.Shared/Responses/ResourceResponse.cs`
- 코드(서버): `Server/src/CatCatGo.Shared/Requests/LoginRequest.cs` (LinkSocialRequest)
- 코드(클라이언트): 대응 파일 없음

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | qa-agent | 미구현 사항으로 리뷰 범위 외 -- 향후 구현 시 대응. Closed 처리. | - |
