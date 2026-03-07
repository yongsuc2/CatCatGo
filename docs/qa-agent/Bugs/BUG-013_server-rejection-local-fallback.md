# BUG-013: 서버 비즈니스 에러 시 로컬 폴백 실행으로 서버-클라이언트 상태 불일치

## 상태
Fixed

## 심각도
Critical

## 발견일
2026-03-07

## 발견 경위
서버-클라이언트 비즈니스 로직 연동 검증(Task #5) 수행 중, GameManager의 Online API 호출 패턴을 분석하여 발견.

## 증상
패턴 B API 호출에서 서버가 비즈니스 에러(재화 부족, 세션 없음 등)를 반환하면, 클라이언트가 `OnApiFailed()` 호출 후 **로컬 폴백 로직을 실행**한다. 로컬 로직은 서버 측 검증을 거치지 않으므로, 서버에서 거부한 작업이 클라이언트에서 로컬로 수행될 수 있다.

**재현 시나리오 (장비 강화 예시):**
1. 서버 잔액: 강화석 0개
2. 클라이언트가 `EquipmentApi.Upgrade("equip_001")` 호출
3. 서버: `INSUFFICIENT_EQUIPMENT_STONE` 에러 반환
4. 클라이언트 GameManager (line 969-970):
   ```
   OnApiFailed(response.Data?.ErrorCode);
   callback(UpgradeEquipment(equipmentId));  // 로컬 강화 실행!
   ```
5. 로컬 `UpgradeEquipment()`가 클라이언트 로컬 데이터로 강화를 수행
6. **결과: 서버에서는 강화 안됨, 클라이언트에서는 강화됨 -> 상태 불일치**

**동일 패턴이 적용된 모든 API (GameManager 내):**
- TalentUpgradeAsync -> 로컬 TalentUpgrade() 실행
- ClaimTalentMilestoneAsync -> 로컬 ClaimTalentMilestone() 실행
- UpgradeEquipmentAsync -> 로컬 UpgradeEquipment() 실행
- EquipItemAsync -> 로컬 EquipItem() 실행
- UnequipItemAsync -> 로컬 UnequipItem() 실행
- SellEquipmentAsync -> 로컬 SellEquipment() 실행
- ForgeEquipmentAsync -> 로컬 ForgeEquipment() 실행
- BulkForgeAsync -> 로컬 BulkForge() 실행
- HatchPetAsync -> 로컬 HatchPet() 실행
- FeedPetAsync -> 로컬 FeedPet() 실행
- DeployPetAsync -> 로컬 DeployPet() 실행
- PullGachaAsync -> 로컬 PullGacha() 실행
- TowerChallengeAsync -> 로컬 TowerChallenge() 실행
- DungeonChallengeAsync -> 로컬 DungeonChallenge() 실행
- GoblinMineAsync -> 로컬 GoblinMine() 실행
- GoblinCartAsync -> 로컬 GoblinCart() 실행
- CatacombStartAsync -> 로컬 CatacombStart() 실행
- ChapterStartAsync -> 로컬 StartChapter() 실행
- HeritageUpgradeAsync -> 로컬 UpgradeHeritage() 실행
- 등 30여 개 API

## 원인
GameManager의 Online API 패턴이 "서버 우선, 실패 시 로컬 폴백"으로 설계되어 있으나, 서버가 비즈니스 에러를 반환한 경우(재화 부족, 조건 미충족 등)에도 오프라인 폴백과 동일하게 로컬 로직을 실행한다.

오프라인 상태에서의 로컬 실행은 정상(오프라인 모드)이나, 서버가 명시적으로 거부한 경우에는 로컬 실행도 하지 않아야 한다.

## 영향 범위
- 서버와 클라이언트 간 게임 상태 불일치 발생 (재화, 장비, 펫, 재능 등)
- 다음 서버 동기화 시 충돌 발생 가능
- 서버 검증을 우회하는 효과 (핵/치트와 유사)

## 관련 파일
- 코드(클라이언트): `Assets/_Project/Scripts/Services/GameManager.cs` (line 905-980, TalentUpgradeAsync 등 30여 개 메서드)

## 수정 방안
서버 에러 시 로컬 폴백을 실행하지 않도록 변경:
```csharp
// Before (현재 코드)
if (!response.IsSuccess || response.Data == null || !response.Data.Success)
{
    OnApiFailed(response.Data?.ErrorCode);
    callback(UpgradeEquipment(equipmentId));  // 로컬 폴백
    return;
}

// After (수정 제안)
if (!response.IsSuccess || response.Data == null || !response.Data.Success)
{
    OnApiFailed(response.Data?.ErrorCode);
    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));  // 실패 반환
    return;
}
```

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-agent | GameManager 30여 개 Online API 메서드에서 서버 에러 시 로컬 폴백 제거, Result.Fail/null 반환으로 변경 | - |
