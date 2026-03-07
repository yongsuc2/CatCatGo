# BUG-008: 서버 응답 패턴 이중화로 인한 구조적 불일치

## 상태
Fixed

## 심각도
Major (구조적 문제 - 현재 동작에는 영향 없으나 유지보수 위험)

## 발견일
2026-03-07

## 발견 경위
서버-클라이언트 연동 버그 리뷰(Task #3, #4, #5) 수행 중 API 응답 처리 흐름을 추적하여 발견.

## 증상
서버가 두 가지 응답 패턴을 혼용하여, 클라이언트에서 에러 처리 방식이 API마다 다르다.

**패턴 A** (Auth, Battle, Save, Arena, Shop):
- 서비스가 도메인 객체를 직접 반환
- 에러 시 컨트롤러가 `BadRequest()` / `Unauthorized()` 등 HTTP 에러 코드 반환
- 클라이언트가 HTTP 상태 코드로 성공/실패 판별

**패턴 B** (Chapter, Content, Daily, Equipment, Gacha, Heritage, Pet, Talent, Treasure):
- 서비스가 `ApiResponse<T>`를 반환, 에러 시 `ApiResponse<T>.Fail(errorCode)` 반환
- 컨트롤러가 `Ok(apiResponse)` -> 항상 HTTP 200 반환
- 클라이언트가 `ServerResponse<T>.Success`와 `ServerResponse<T>.ErrorCode`로 판별

**클라이언트 측 에러 처리 (GameManager):**
GameManager에서 패턴 B API 호출 시 `!response.IsSuccess || response.Data == null || !response.Data.Success` 를 3중 체크하므로, 현재 코드에서는 에러가 올바르게 감지됩니다.

**문제점:**
1. 에러 처리 방식이 API마다 다르므로 새 API 추가 시 실수 가능성 높음
2. `ApiClient.SendRequest<T>`는 HTTP 200이면 무조건 `ApiResponse<T>.Success`를 반환하므로, GameManager를 거치지 않는 직접 호출에서는 `ServerResponse.Success` 체크를 누락할 수 있음
3. 패턴 B에서 서버 에러 시 로컬 폴백 로직 실행 (`callback(TalentUpgrade(statType))` 등) - 서버 에러와 오프라인을 동일하게 처리

## 영향 범위
- 현재 GameManager 경유 호출에서는 에러가 올바르게 처리됨
- 새 API 추가 시 패턴 혼동으로 에러 처리 누락 위험
- 서버 에러와 오프라인을 구분하지 않고 동일한 로컬 폴백 실행

## 관련 파일
- 코드(서버): `Server/src/CatCatGo.Server.Api/Controllers/ChapterController.cs` (패턴 B 컨트롤러)
- 코드(서버): `Server/src/CatCatGo.Server.Api/Controllers/AuthController.cs` (패턴 A 컨트롤러)
- 코드(클라이언트): `Assets/_Project/Scripts/Network/ApiClient.cs` (line 161-179, HTTP 상태 코드만 체크)
- 코드(클라이언트): `Assets/_Project/Scripts/Network/ServerResponse.cs` (패턴 B 역직렬화 모델)
- 코드(클라이언트): `Assets/_Project/Scripts/Services/GameManager.cs` (line 912 등, 3중 체크 패턴)

## 권장 수정
서버 응답 패턴을 통일하는 것을 권장:
- 방안: 패턴 B 컨트롤러에서 `if (!result.Success) return BadRequest(result)` 패턴 적용
- 이를 통해 패턴 A와 동일하게 HTTP 에러 코드 기반으로 통일

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-server-agent | 패턴 B 컨트롤러 9개에 ToActionResult() 적용 — 서비스 에러 시 BadRequest 반환으로 통일 | - |
| 2026-03-07 | dev-agent | ApiClient HTTP 4xx body 역직렬화 추가 (FailWithData), ApiResponse에 FailWithData 팩토리 메서드 추가 | - |
