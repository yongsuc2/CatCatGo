# Planning Agent 작업일지

## 2026-03-07 기획 문서 정합성 수정

### 작업 내용
밸런스 데이터시트(`13_밸런스데이터시트.md`)를 실제 JSON 데이터 파일과 대조 검토하여 불일치 항목을 수정했다.

### 수정 파일
- `docs/Planning/SystemDesign/13_밸런스데이터시트.md` - 전면 재작성
- `docs/Planning/SystemDesign/00_게임개요.md` - 챕터 유형, 인카운터 유형 수정
- `docs/Planning/SystemDesign/07_재화시스템.md` - 출석 보상 순서 수정

### 발견한 주요 불일치 (40건 이상)

| 항목 | 문서 (수정 전) | 실제 데이터 (수정 후) | 근거 |
|------|-------------|----------------|------|
| 챕터당 스케일링 | 1.25x | 1.3x | `battle.data.json` scalingPerChapter |
| 데미지 공식 | `ATK - DEF x 0.5` (감산) | `k/(k+DEF)` (k=100) | `battle.data.json` defenseConstant |
| 인카운터 가중치 | 전투40/천사25/악마10/우연25 | 전투40/악마7/우연53 | `encounter.data.json` weights |
| 적 기본 스탯 (일반) | HP=80 ATK=8 DEF=3 | HP=106 ATK=13 DEF=4 | `enemy.data.json` baseStats |
| 아메바 스탯 | HP=90 ATK=8 DEF=2 | HP=120 ATK=11 DEF=3 | `enemy.data.json` templates |
| 분노 공격 T2 계수 | 0.85 | 1.73 | `active-skill-tier.data.json` |
| 흡혈 T2 | 12% | 18% | `passive-skill-tier.data.json` |
| 부활 T1 | 30% | 17% | `passive-skill-tier.data.json` |
| 출석 2일차 보상 | 에픽 펫 | 골드 3,000 | `attendance.data.json` |
| 데이터 소스 경로 | `src/domain/data/json/` | `Assets/_Project/Data/Json/` | 실제 프로젝트 구조 |

### 추가 보완
- 기존 문서에 누락된 스킬 추가: 반격 마스터리, 배수진, 불굴, 압도, 분쇄, 기절
- 게임개요: 미구현 챕터 유형(30일/5일)에 "미구현" 상태 명시
- 재능 등급 테이블을 캐릭터성장시스템 문서와 일치하도록 상세화

### 브랜치
`feature/planning-agent/balance-data-consistency`
