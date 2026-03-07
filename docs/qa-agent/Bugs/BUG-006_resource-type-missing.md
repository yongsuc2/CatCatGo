# BUG-006: 재화 타입 3종 미등록 (입장권, 행운 코인, 보물상자 키)

## 상태
Closed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
consistency-report-v2 작성 중, 07_재화시스템.md에 정의된 재화 목록과 GameEnums.cs의 ResourceType enum을 비교하여 발견.

## 증상
기획서(07_재화시스템.md)에 정의된 재화 중 3종이 GameEnums.cs ResourceType enum에 등록되어 있지 않음.

**미등록 재화:**
1. **입장권** (PvP 경기장 진입) -- ARENA_TOKEN 없음
2. **행운 코인** (30일 챕터 행운 머신) -- LUCKY_COIN 없음
3. **보물상자 키** (가챠 재화) -- CHEST_KEY 없음

**현재 등록된 ResourceType (13종):**
GOLD, GEMS, STAMINA, CHALLENGE_TOKEN, PICKAXE, EQUIPMENT_STONE, POWER_STONE, SKULL_BOOK, KNIGHT_BOOK, RANGER_BOOK, GHOST_BOOK, PET_EGG, PET_FOOD

## 원인
PvP, 30일 챕터, 보물상자 키 시스템이 아직 미구현 상태이므로 관련 재화 타입도 코드에 추가되지 않음.

## 영향 범위
- 현재 게임 동작에는 영향 없음 (미구현 기능의 재화)
- 기획서에 미구현 표기가 필요
- 향후 해당 기능 구현 시 ResourceType enum에 추가 필요

## 관련 파일
- 기획: docs/planning-agent/SystemDesign/07_재화시스템.md (재화 전체 맵 섹션)
- 코드/데이터: Assets/_Project/Scripts/Domain/Enums/GameEnums.cs (라인 129-144, ResourceType enum)

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | planning-agent | 07_재화시스템.md에서 입장권, 행운 코인, 보물상자 키 항목에 **(미구현)** 표기 추가. 재화 전체 맵 및 재화 흐름도에도 미구현 표기 반영 | - |
