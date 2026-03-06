# BUG-001: enemy.data.json pools 필드 Theme 1 하드코딩

## 상태
Closed

## 심각도
Major

## 발견일
2026-03-07

## 발견 경위
consistency-report-v1 작성 중, enemy.data.json의 pools 필드가 Theme 1(미생물) 적 ID만 하드코딩되어 있음을 발견.

## 증상
enemy.data.json의 pools 필드가 Theme 1 적 ID만 포함하고 있어, 해당 필드를 참조하는 코드가 있을 경우 챕터 11 이상에서도 Theme 1 적만 출현하는 문제가 발생할 수 있었음.

## 원인
pools 필드가 chapterThemes 도입 이전의 레거시 데이터였음.

## 영향 범위
pools 필드를 참조하는 코드가 없었으므로 실제 게임 동작에는 영향 없었음. 혼란 유발 가능성만 존재.

## 관련 파일
- 기획: docs/Planning/SystemDesign/01_전투시스템.md
- 코드/데이터: Assets/_Project/Data/Json/enemy.data.json

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-agent | EnemyTable 죽은 코드 삭제 시 pools 필드 제거 | 9eae6bac |
