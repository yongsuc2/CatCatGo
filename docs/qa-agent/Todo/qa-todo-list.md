# QA Agent Todo List

작성일: 2026-03-08

---

## 미해결 버그

### TODO-001: BUG-014 수정 요청 (CollectionDataTable null safety)
- **우선순위**: 낮음
- **대상**: dev-agent
- **내용**: `CollectionDataTable.EnsureLoaded()`에서 JSON 로드 실패 시 `_entries`가 null로 남아 NRE 가능
- **참조**: `docs/qa-agent/Bugs/BUG-014_collection-datatable-null-safety.md`
- **비고**: JSON 파일이 정상 포함되어 있으면 발생하지 않으므로 심각도 낮음

---

## 미검증 시스템 Consistency Report

### TODO-002: 이벤트 시스템 (10_이벤트시스템.md) 기획-구현 일관성 검증
- **우선순위**: 보통
- **내용**: 이벤트 시스템 기획서와 EventManager 등 관련 코드의 일치 여부 검증
- **참조**: `docs/planning-agent/SystemDesign/10_이벤트시스템.md`

### TODO-003: 과금 시스템 (11_과금시스템.md) 기획-구현 일관성 검증
- **우선순위**: 보통
- **내용**: 과금 시스템 기획서와 상점/IAP 관련 코드의 일치 여부 검증
- **참조**: `docs/planning-agent/SystemDesign/11_과금시스템.md`

### TODO-004: 밸런스 데이터시트 (13_밸런스데이터시트.md) 검증
- **우선순위**: 보통
- **내용**: 새로 추가된 밸런스 데이터시트 기획서와 실제 데이터 테이블의 일치 여부 검증
- **참조**: `docs/planning-agent/SystemDesign/13_밸런스데이터시트.md`

---

## 화면 기획서 검증

### TODO-005: 화면 기획서 기획-구현 일관성 검증
- **우선순위**: 보통
- **내용**: 7개 화면 기획서와 실제 UI 코드(Presentation/Screens/)의 일치 여부 검증
- **대상 기획서**:
  - `화면기획/로비화면_기획서.md`
  - `화면기획/모험화면_기획서.md`
  - `화면기획/장비화면_기획서.md`
  - `화면기획/재능화면_기획서.md`
  - `화면기획/펫화면_기획서.md`
  - `화면기획/상점화면_기획서.md`
  - `화면기획/아이콘_리소스_목록.md`

---

## 가챠 시스템 확장 검증

### TODO-006: 가챠 시스템 코드 확장 검증 (상자 3종 + 펫 2종)
- **우선순위**: 높음
- **내용**: 최근 커밋(f7f67b09)에서 추가된 가챠 시스템 확장 코드와 기획서(06_가챠시스템.md)의 일치 여부 검증
- **검증 항목**:
  - 신규 상자 3종 (실버/골드/다이아 보물상자) 데이터 정합성
  - 신규 펫 2종 추가 데이터 정합성
  - gacha.data.json 확장 데이터와 기획서 일치 여부
  - UI 코드 연동 검증
- **참조**: 커밋 f7f67b09, `docs/planning-agent/SystemDesign/06_가챠시스템.md`

---

## 데이터 무결성 검증

### TODO-007: JSON 데이터 파일 이중 보관 동기화 검증
- **우선순위**: 높음
- **내용**: `Assets/_Project/Data/Json/`과 `Assets/Resources/_Project/Data/Json/` 간 모든 JSON 파일이 동일한지 재검증
- **비고**: v1에서 18개 파일 동기화 확인했으나, 이후 데이터 변경이 있었으므로 재검증 필요
