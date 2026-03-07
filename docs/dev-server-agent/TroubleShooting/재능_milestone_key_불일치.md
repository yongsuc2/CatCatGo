# 재능 마일스톤 delta key 형식 불일치

## 발생일
2026-03-08

## 환경
- 클라이언트: Unity 2D (CatCatGo)
- 서버: CatCatGo.Server.Core (ASP.NET Core)
- 관련 커밋: e8e98817

## 증상
1. **마일스톤 수령 상태 미반영**: 재능 마일스톤을 수령해도 클라이언트 UI(TalentScreen)에서 수령 완료(claimed) 상태로 표시되지 않음. 수령 완료 버튼이 계속 활성화되어 재클릭 가능 (서버에서 `ALREADY_CLAIMED`로 차단되므로 실제 중복 수령은 없으나 UX 문제)
2. **골드 획득량 배율(GOLD_BOOST) 미적용**: `Player.GetGoldMultiplier()`가 `"LV_10"` 형식으로 `ClaimedMilestones`를 조회하는데, 실제 저장된 키는 `"10"` 형식이므로 GOLD_BOOST 보상이 인식되지 않음
3. **"모두 수령" 개수 표시 오류**: `Talent.GetClaimableMilestones()` (`Talent.cs` line 139-144)도 동일한 키 형식 불일치로 이미 수령한 마일스톤을 미수령으로 판정

## 원인
서버 `TalentService.cs`의 두 메서드에서 `StateDeltaBuilder.AddClaimedMilestone()`에 전달하는 키 형식과 클라이언트 `Talent.GetMilestoneKey()`가 반환하는 키 형식이 불일치했다.

| 위치 | 키 형식 (수정 전) | 코드 |
|------|------------------|------|
| 서버 `ClaimMilestoneAsync` | `"10"` | `AddClaimedMilestone(milestoneLevel.ToString())` |
| 서버 `ClaimAllMilestonesAsync` | `"10"` | `AddClaimedMilestone(milestone.ToString())` |
| 클라이언트 `Talent.GetMilestoneKey` (`Talent.cs` line 134-137) | `"LV_10"` | `return $"LV_{level}";` |

**키 전파 경로:**
1. 서버가 delta 응답에 `ClaimedMilestones: ["10"]`을 포함하여 전송
2. 클라이언트 `GameState.ApplyDelta()`가 delta의 ClaimedMilestones를 `Player.ClaimedMilestones` HashSet에 추가 → `{"10"}` 저장
3. UI 표시 시 `Talent.GetMilestoneKey(10)` = `"LV_10"`으로 HashSet 조회 → `"10"` != `"LV_10"` → claimed 인식 실패

## 해결 방법
서버측에서 delta key를 클라이언트와 동일한 `$"LV_{level}"` 형식으로 변경했다.

**수정 내용 (`Server/src/CatCatGo.Server.Core/Services/TalentService.cs`):**
- `ClaimMilestoneAsync` (line 146): `AddClaimedMilestone($"LV_{milestoneLevel}")`
- `ClaimAllMilestonesAsync` (line 171): `AddClaimedMilestone($"LV_{milestone}")`

클라이언트는 이미 올바른 형식(`"LV_10"`)을 사용하고 있으므로 수정 불필요.

## 예방 방법
- 서버-클라이언트 간 키/식별자 형식을 공유 상수 또는 공통 유틸리티로 정의하여 형식 불일치 가능성 차단
- delta 키 형식 변경 시 클라이언트의 `GetMilestoneKey()` 등 기대 형식을 반드시 교차 확인
- 서버 테스트에서 delta 키 형식이 클라이언트 기대 형식과 일치하는지 검증하는 테스트 포함 (커밋 e8e98817에서 추가됨)

## 관련 문서
- `Server/src/CatCatGo.Server.Core/Services/TalentService.cs` (ClaimMilestoneAsync line 146, ClaimAllMilestonesAsync line 171)
- `Server/src/CatCatGo.Server.Core/Services/StateDeltaBuilder.cs` (AddClaimedMilestone)
- `Assets/_Project/Scripts/Domain/Entities/Talent.cs` (GetMilestoneKey line 134-137, GetClaimableMilestones line 139-144)
- `Assets/_Project/Scripts/Domain/Entities/Player.cs` (ClaimedMilestones, GetGoldMultiplier)
- `Assets/_Project/Scripts/Services/GameState.cs` (ApplyDelta)
- `docs/qa-agent/Bugs/BUG-015_talent-milestone-key-mismatch.md` (QA 버그 리포트)
- `docs/planning-agent/SystemDesign/02_캐릭터성장시스템.md` (재능 등급 시스템)
- `docs/planning-agent/SystemDesign/화면기획/재능화면_기획서.md` (마일스톤 UI)
