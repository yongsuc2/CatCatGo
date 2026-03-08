# 클라이언트 코드 자체 검토 결과 Todo

> 작성일: 2026-03-08
> 작성자: dev-agent

## 코드 품질 개선

| ID | 우선순위 | 작업 | 상세 | 상태 |
|----|---------|------|------|------|
| CQ-1 | 중 | PetManager 죽은 코드 삭제 | `GetTotalPassiveBonus()` 호출처 없음. Player.ComputePetBonus()가 동일 역할 수행 | 완료 |
| FI-1 | 중 | GameState.DeserializeEquipmentDelta WeaponSubType 복원 | StateDelta에 필드 추가 + DeserializeEquipmentDelta에서 파싱하여 Equipment 생성자에 전달 | 완료 |

## 향후 개선 작업

| ID | 우선순위 | 작업 | 상세 | 상태 |
|----|---------|------|------|------|
| FI-2 | 저 | Battle.RunToCompletion maxTurns 초과 시 DEFEAT 로그 누락 | maxTurns 초과로 패배 처리 시 BattleLogType.DEATH 로그를 남기지 않아 UI에서 패배 사유를 표시하기 어려움 | 완료 |
| FI-3 | 저 | EquipmentSlot.Unequip 시 SlotLevel 초기화 미수행 | 기획서상 "강화 레벨은 슬롯에 귀속"으로 의도된 동작 | 해당없음 |
| FI-4 | 중 | 펫 뽑기(PET/BASIC_PET) PityCount 증가 안 함 | TreasureChest.Pull에서 펫 뽑기 시 PityCount를 증가시키지 않으나, 기획서상 펫 뽑기는 천장 시스템 없음으로 의도된 동작 | 해당없음 |
