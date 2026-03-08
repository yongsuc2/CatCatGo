# BUG-017: 서버 EquipmentDeltaData에 WeaponSubType 필드 누락

## 상태
Closed

## 심각도
Major

## 발견일
2026-03-08

## 발견 경위
dev-agent의 FI-1(WeaponSubType 복원) 수정 검증 중 발견.
클라이언트 `StateDelta.EquipmentDeltaData`와 `GameState.DeserializeEquipmentDelta`에 WeaponSubType 파싱을 추가했으나, 서버측 EquipmentDeltaData에 해당 필드가 없어 서버가 이 값을 전송하지 않음.

## 증상
1. 서버에서 장비 추가 delta를 보낼 때 `WeaponSubType` 필드가 포함되지 않음
2. 클라이언트에서 delta를 역직렬화하면 `WeaponSubType`이 항상 null
3. 서버 경유로 새로 획득한 무기의 `WeaponSubTypeValue`가 null이 됨
4. 무기 종류(검/지팡이/활) 구분이 사라져 합성 시 잘못된 매칭이 발생할 수 있음

## 원인
서버측 모델 클래스에 필드 추가가 누락됨.

**서버 모델 (필드 없음):**
- `Server/src/CatCatGo.Shared/Models/StateDelta.cs:49-60` — `EquipmentDeltaData` 클래스에 `WeaponSubType` 프로퍼티 없음

**서버 변환 (값 미설정):**
- `Server/src/CatCatGo.Server.Core/Services/GachaService.cs:99-109` — `ToEquipmentDeltaData`에서 WeaponSubType 미설정

**클라이언트 (필드 있음, 파싱 로직 있음):**
- `Assets/_Project/Scripts/Network/StateDelta.cs:61` — `WeaponSubType` 필드 존재
- `Assets/_Project/Scripts/Services/GameState.cs:384-386` — 파싱 로직 존재

## 영향 범위
- 가챠로 획득한 무기의 WeaponSubType이 null이 되어 합성 시 무기 종류 구분이 불가능
- 장비 화면에서 무기 종류별 필터링/표시에 영향
- SaveSerializer를 통한 풀 동기화(FullSync)에서는 정상 동작 (별도 경로)

## 관련 파일
- 서버 모델: `Server/src/CatCatGo.Shared/Models/StateDelta.cs:49-60`
- 서버 변환: `Server/src/CatCatGo.Server.Core/Services/GachaService.cs:99-109`
- 클라이언트 모델: `Assets/_Project/Scripts/Network/StateDelta.cs:61`
- 클라이언트 파싱: `Assets/_Project/Scripts/Services/GameState.cs:384-386`
- 기획서: `docs/planning-agent/SystemDesign/03_장비시스템.md` (무기 종류 섹션)

## 수정 방안
1. 서버 `EquipmentDeltaData`에 `public string? WeaponSubType { get; set; }` 프로퍼티 추가
2. 서버 `GachaService.ToEquipmentDeltaData`에서 장비의 WeaponSubType을 설정
3. 서버 `EquipmentService` 등 장비를 delta에 넣는 다른 경로도 동일하게 수정

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-08 | qa-agent | 버그 발견 및 등록 | - |
| 2026-03-08 | dev-server-agent | 서버 EquipmentDeltaData/EquipmentEntry에 WeaponSubType 추가, GachaService/EquipmentService에서 값 설정 | efaea97f |
| 2026-03-08 | qa-agent | 수정 검증 완료 — 서버 모델, GachaService, EquipmentService 모두 WeaponSubType 전달 확인, 서버 테스트 129개 통과 | - |
