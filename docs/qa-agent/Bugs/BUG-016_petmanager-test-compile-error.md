# BUG-016: PetManagerTests 컴파일 에러 - 삭제된 GetTotalPassiveBonus 참조

## 상태
Closed

## 심각도
Major

## 발견일
2026-03-08

## 발견 경위
dev-agent의 클라이언트 코드 자체 검토(커밋 2a7c281e) 결과 검증 중 발견.
CQ-1 작업으로 `PetManager.GetTotalPassiveBonus()` 메서드를 삭제했으나, 해당 메서드를 호출하는 테스트 코드가 그대로 남아있음.

## 증상
`PetManagerTests.cs`에서 삭제된 `PetManager.GetTotalPassiveBonus()` 메서드를 호출하는 테스트 2개가 남아있어 Unity Editor 테스트 컴파일 시 에러 발생.

- `GetTotalPassiveBonusSumsAllPets()` (line 126-137)
- `GetTotalPassiveBonusReturnsZeroForEmptyList()` (line 139-145)

## 원인
죽은 코드 삭제 시 관련 테스트 코드도 함께 삭제하지 않은 누락.

## 영향 범위
- Unity Editor 테스트 전체가 컴파일 실패하여 실행 불가능
- 다른 테스트의 회귀 검증이 불가능해짐

## 관련 파일
- 코드(삭제됨): `Assets/_Project/Scripts/Services/PetManager.cs` (GetTotalPassiveBonus 메서드)
- 테스트: `Assets/_Project/Tests/Editor/Services/PetManagerTests.cs:126-145`

## 수정 방안
`PetManagerTests.cs`에서 line 125-145의 두 테스트 메서드를 삭제한다.

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-08 | qa-agent | 버그 발견 및 등록 | - |
| 2026-03-08 | dev-agent | PetManagerTests에서 삭제된 메서드 테스트 제거 + EquipmentManager/EquipmentManagerTests 전체 삭제 | f0e1d4e5, b0a70f93 |
| 2026-03-08 | qa-agent | 수정 검증 완료 — PetManagerTests.cs에 HatchEgg 테스트 2개만 정상 잔존, EquipmentManager 참조 전체 제거 확인 | - |
