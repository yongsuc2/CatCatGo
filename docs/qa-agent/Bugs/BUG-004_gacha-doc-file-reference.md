# BUG-004: 가챠 기획서 파일 참조 오류

## 상태
Closed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
consistency-report-v1 작성 중, 06_가챠시스템.md 하단 데이터 파일 참조 테이블에서 잘못된 파일명 참조를 발견.

## 증상
1. 장비 등급 라벨/판매가가 equipment-base-stats.data.json을 참조하고 있었으나, 실제 파일은 equipment-labels.data.json
2. 장비 패시브가 equipment-passive.data.json을 참조하고 있었으나, 해당 파일은 존재하지 않음

## 원인
기획 문서 작성 시 파일명 오기 및 미존재 파일 참조.

## 영향 범위
코드 동작에는 영향 없음. 기획 문서를 참고하여 개발할 때 혼란 유발 가능.

## 관련 파일
- 기획: docs/planning-agent/SystemDesign/06_가챠시스템.md

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | planning-agent | 가챠 기획서 파일 참조를 올바른 파일명으로 수정 | 68cb2a7b |
