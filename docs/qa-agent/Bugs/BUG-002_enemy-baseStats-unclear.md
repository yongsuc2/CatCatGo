# BUG-002: enemy.data.json baseStats 필드 용도 불명확

## 상태
Closed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
consistency-report-v1 작성 중, enemy.data.json에 개별 적 스탯과 별도로 baseStats 평균값이 존재하는 것을 발견. 코드에서 참조 여부가 불명확했음.

## 증상
enemy.data.json에 baseStats 필드가 적 타입별 평균 스탯을 포함하고 있었으나, 개별 적이 각자 스탯을 가지고 있어 baseStats가 사용될 경우 개별 스탯이 무시될 위험이 있었음.

## 원인
레거시 데이터. 개별 적 스탯 시스템 도입 전의 잔여 필드.

## 영향 범위
코드에서 baseStats를 참조하지 않았으므로 실제 영향 없었음.

## 관련 파일
- 기획: docs/planning-agent/SystemDesign/01_전투시스템.md
- 코드/데이터: Assets/_Project/Data/Json/enemy.data.json

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-agent | EnemyTable 죽은 코드 삭제 시 baseStats 필드 제거 | 9eae6bac |
