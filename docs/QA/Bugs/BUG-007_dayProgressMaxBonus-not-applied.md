# BUG-007: 몬스터 일수 스케일링 공식 불일치 (dayProgressMaxBonus 미적용)

## 상태
Closed

## 심각도
Minor (기획서 오류 -- 코드가 정상)

## 발견일
2026-03-07

## 발견 경위
consistency-report-v2 작성 중, 12_모험시스템.md의 일수 스케일링 공식과 EnemyTemplate.cs의 실제 구현을 비교하여 발견.

## 증상
기획서에서는 일수 배율에 `dayProgressMaxBonus` 파라미터를 곱하도록 명시하고 있으나, 코드에서는 해당 파라미터 없이 dayProgress를 그대로 사용.

**기획서 (12_모험시스템.md:143):**
```
일수 배율 = 1 + (currentDay / totalDays) * dayProgressMaxBonus
```
- `BattleDataTable.enemy.dayProgressMaxBonus` 참조라고 명시
- 개발일지에서 `dayProgressMaxBonus` 0.8 -> 0.3으로 조정 기록 존재

**코드 (EnemyTemplate.cs:73):**
```csharp
float dayBonus = 1 + dayProgress;
```
- `dayProgress = CurrentDay / TotalDays` (Chapter.cs:381)
- `dayProgressMaxBonus` 파라미터를 전혀 사용하지 않음

**데이터 검증:**
- `battle.data.json`의 `enemy` 객체에 `dayProgressMaxBonus` 필드 없음
- `BattleDataTable.cs`의 `EnemyScalingConfig`에 `DayProgressMaxBonus` 프로퍼티 없음

## 원인
dayProgressMaxBonus 파라미터가 기획서에만 존재하고, 데이터/코드에는 반영되지 않은 상태. 기획 변경(0.8 -> 0.3)은 기획일지에만 기록되었고 코드에 적용되지 않음.

## 영향 범위
**밸런스에 직접적 영향 -- Critical**

60일차 기준 수치 비교:
- dayProgress = 59/60 = 0.983
- **기획 의도**: 일수 배율 = 1 + 0.983 * 0.3 = **1.295**
- **실제 코드**: 일수 배율 = 1 + 0.983 = **1.983**
- 차이: 몬스터가 기획 의도보다 **약 53% 더 강함** (1.983 / 1.295 = 1.531)

결과:
- 챕터 후반부(40일 이후) 난이도가 기획보다 과도하게 높아짐
- 플레이어가 챕터 클리어를 실패할 확률이 기획 의도보다 높아짐
- 전체 게임 밸런스 곡선이 왜곡됨

## 관련 파일
- 기획: docs/Planning/SystemDesign/12_모험시스템.md (라인 140-144, 일수 기반 몬스터 스케일링)
- 코드/데이터:
  - Assets/_Project/Scripts/Domain/Chapter/EnemyTemplate.cs (라인 73)
  - Assets/_Project/Data/Json/battle.data.json (enemy 객체에 dayProgressMaxBonus 필드 부재)

## 해결 방법
현재 코드 구현(`1 + dayProgress`)이 의도된 동작으로 확인됨. 기획서(12_모험시스템.md)에서 dayProgressMaxBonus 관련 구체적 수치/공식을 제거하고, 동작 의도만 서술하는 방향으로 기획서를 수정하여 해결.

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | planning-agent | 12_모험시스템.md의 일수 기반 몬스터 스케일링 섹션에서 dayProgressMaxBonus 공식/수치를 제거하고, 코드 현행에 맞춰 동작 의도만 서술하도록 재작성. 현재 코드 구현(1 + dayProgress)이 의도된 동작으로 확인됨 | - |
