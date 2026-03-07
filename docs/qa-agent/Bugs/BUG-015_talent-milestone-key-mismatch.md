# BUG-015: 재능 마일스톤 delta key 형식 불일치 (서버 "10" vs 클라이언트 "LV_10")

## 상태
Closed

## 심각도
Critical

## 발견일
2026-03-08

## 발견 경위
재능 마일스톤 수령 흐름의 서버-클라이언트 코드 리뷰 과정에서 발견.
서버 `TalentService`의 `ClaimMilestoneAsync`/`ClaimAllMilestonesAsync`가 delta에 넣는 key 형식과
클라이언트 `Talent.GetMilestoneKey()`가 반환하는 key 형식이 불일치함.

## 증상
1. 마일스톤을 수령해도 클라이언트 UI에서 **수령 완료(claimed) 상태로 표시되지 않음**
   - 서버가 delta에 `"10"`을 보내면 → `Player.ClaimedMilestones`에 `"10"` 추가
   - UI는 `Talent.GetMilestoneKey(10)` = `"LV_10"`으로 체크 → `"10"` != `"LV_10"` → claimed 인식 실패
2. 수령 완료된 마일스톤이 계속 **claimable(수령 가능) 상태로 표시**되어 재클릭 가능
   - 서버에서 `ALREADY_CLAIMED`로 차단되므로 실제 중복 수령은 발생하지 않으나, UX 문제
3. **골드 획득량 배율(GOLD_BOOST) 미적용**
   - `Player.GetGoldMultiplier()`도 `Talent.GetMilestoneKey(m.Level)` = `"LV_10"` 형식으로 체크
   - `ClaimedMilestones`에 `"10"` 형태로 저장되므로 GOLD_BOOST 보상이 인식되지 않음

## 원인
서버 `TalentService.cs`의 두 메서드에서 `AddClaimedMilestone()`에 `milestoneLevel.ToString()` (= `"10"`)을 전달하지만,
클라이언트는 `$"LV_{level}"` (= `"LV_10"`) 형식을 사용.

**서버 코드 (문제 위치):**
- `ClaimMilestoneAsync` line 146: `AddClaimedMilestone(milestoneLevel.ToString())`
- `ClaimAllMilestonesAsync` line 171: `AddClaimedMilestone(milestone.ToString())`

**클라이언트 코드 (기대 형식):**
- `Talent.GetMilestoneKey(level)` line 134-137: `return $"LV_{level}";`

## 영향 범위
- 재능 마일스톤 수령 UI 전체 (TalentScreen 등급 보상 카드)
- 골드 획득량 배율 계산 (Player.GetGoldMultiplier)
- "모두 수령" 기능의 미수령 개수 표시 (Talent.GetClaimableMilestones)

## 관련 파일
- 기획: `docs/planning-agent/SystemDesign/화면기획/재능화면_기획서.md`
- 서버: `Server/src/CatCatGo.Server.Core/Services/TalentService.cs` (line 146, 171)
- 서버 delta: `Server/src/CatCatGo.Server.Core/Services/StateDeltaBuilder.cs` (AddClaimedMilestone)
- 클라이언트 엔티티: `Assets/_Project/Scripts/Domain/Entities/Talent.cs` (GetMilestoneKey, line 134-137)
- 클라이언트 플레이어: `Assets/_Project/Scripts/Domain/Entities/Player.cs` (ClaimedMilestones, GetGoldMultiplier)
- 클라이언트 GameState: `Assets/_Project/Scripts/Services/GameState.cs` (ApplyDelta, line 196-199)
- 클라이언트 UI: `Assets/_Project/Scripts/Presentation/Screens/TalentScreen.cs`
- 서버 테스트: `Server/tests/CatCatGo.Server.Tests/TalentServiceTests.cs`

## 수정 방안
서버측에서 delta key를 `$"LV_{milestoneLevel}"` 형식으로 변경:
1. `ClaimMilestoneAsync`: `AddClaimedMilestone($"LV_{milestoneLevel}")`
2. `ClaimAllMilestonesAsync`: `AddClaimedMilestone($"LV_{milestone}")`

클라이언트는 수정 불필요 (이미 올바른 형식 사용).

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-08 | qa-agent | 버그 발견 및 등록 | - |
| 2026-03-08 | dev-server-agent | delta key를 LV_{level} 형식으로 수정 + 테스트 추가 | e8e98817 |
| 2026-03-08 | qa-agent | 수정 검증 완료 (코드 추적 + 서버 테스트 129개 전체 통과) | - |
