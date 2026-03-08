---
name: team
description: 팀을 구성하여 작업을 수행. team-lead가 PD 역할을 직접 수행하고, 필요한 Agent를 spawn하여 작업을 위임.
user-invocable: true
disable-model-invocation: false
---

# team - 팀 구성 및 작업 실행

## 사용법

`/team <작업 설명>` 형태로 사용한다.

작업 설명이 없으면 사용자에게 어떤 작업을 수행할까요? 질문 후 대기한다.

## 실행 절차

### 1. 작업 설명 확인

사용자가 `/team` 뒤에 작업 설명을 입력했는지 확인한다.
- 입력했으면 → 2단계로 진행
- 입력하지 않았으면 → 사용자에게 "어떤 작업을 수행할까요?" 질문 후 대기

### 2. 팀 생성

TeamCreate 도구로 팀을 생성한다.
- `team_name`: 작업 내용을 요약한 짧은 영문 kebab-case 이름 (예: `ui-refactor`, `battle-system`)
- `description`: 사용자가 요청한 작업 설명

팀을 생성하면 현재 세션이 자동으로 **team-lead**가 된다. team-lead가 곧 PD 역할을 수행한다.

### 3. 버그 확인

본 작업 시작 전 `docs/qa-agent/Bugs/` 디렉토리에서 Open 상태 버그 중 심각도가 Critical인 버그가 있으면 본 작업보다 우선하여 수정을 할당한다.

### 4. 작업 분석 및 Agent spawn

작업을 분석하여 필요한 Agent를 판단하고 팀 멤버로 spawn한다.

- 병렬 작업 시 `isolation: "worktree"`를 사용
- spawn 시 반드시 `team_name`을 지정하여 팀에 합류시킨다
- 각 Agent에게 구체적인 작업 내용과 완료 후 보고 지시를 prompt에 포함한다

**사용 가능한 Agent:**

| Agent | subagent_type | 담당 영역 |
|-------|--------------|----------|
| planning-agent | planning-agent | 기획 문서, 정합성 검토, 아이디어 |
| dev-agent | dev-agent | Unity 클라이언트 코드 구현 (Assets/) |
| dev-server-agent | dev-server-agent | C# 서버 코드 구현 (Server/) |
| graphics-agent | graphics-agent | 그래픽 리소스, UI/UX |
| qa-agent | qa-agent | 테스트, 기획-구현 검증, 버그 탐지 |
| claude-code-engineer | claude-code-engineer | Claude Code 설정, 사용법 |

### 5. config 검증 (필수)

Agent spawn 후 `~/.claude/teams/{team_name}/config.json`을 Read 도구로 읽어서 각 member의 `model` 필드를 확인한다.

- `"inherit"`가 발견되면 → **팀 구성 실패**. 사용자에게 "model inherit 버그 감지, 팀 구성 실패" 보고 후 **즉시 중단** (수정 시도하지 않음)
- 정상이면 → 6단계로 진행

### 6. Task 생성 및 할당

Task 도구를 사용하여 작업을 생성하고 Agent에게 할당한다.
- 작업 간 의존성이 있으면 순서를 조율한다
- 각 Agent에게 SendMessage로 작업 시작을 지시한다

### 7. 결과 검증 (PD 역할)

Agent가 작업을 완료하면 **반드시 아래 절차를 순서대로 수행**한다. Agent의 보고만 믿지 말고 직접 확인한다.

#### 7-1. 코드 변경 직접 확인 (필수)
- `git diff` 또는 `git show`로 **실제 변경된 코드를 직접 읽는다**
- Agent가 보고한 내용과 실제 diff가 일치하는지 대조한다
- 변경된 코드가 의도대로 동작하는지 판단한다

#### 7-2. 검증 체크리스트

| 검증 항목 | 기준 | 확인 방법 |
|-----------|------|-----------|
| 요구사항 충족 | 사용자가 요청한 내용이 빠짐없이 반영되었는지 | diff와 요구사항 대조 |
| 작업 규칙 준수 | CLAUDE.md의 절대 원칙, 커밋 포맷(`[영역] 메시지` + `Ref:`) 등 | 커밋 메시지 확인 |
| 코드 품질 | 주석 금지, 중복 금지, over-engineering 금지 | diff에서 직접 확인 |
| 부작용 없음 | 변경이 다른 기능을 깨뜨리지 않는지 | 관련 코드 grep/read |
| 문서 동기화 | 변경에 따른 관련 문서 업데이트 여부 | 해당 docs/ 확인 |
| 트러블슈팅 문서 | 버그/에러를 해결한 경우 `TroubleShooting/`에 문서 작성 여부 | 파일 존재 확인 |
| 기획서 수치 규칙 | SystemDesign 문서에 구체적 수치가 하드코딩되지 않았는지 | 해당 문서 확인 |
| WorkLog 작성 | 작업 내용이 WorkLog에 기록되었는지 | 파일 존재 및 내용 확인 |

#### 7-3. 검증 실패 시
- 구체적 근거(파일 경로, 라인, diff 내용)와 함께 해당 Agent에게 피드백
- Agent가 수정 완료 후 **다시 7-1부터 재검증**

### 8. main 브랜치 반영

- 검증을 통과한 결과만 main 브랜치에 merge
- merge 전 충돌 여부 확인
- merge 후 정상 상태 확인
- **각 Agent의 커밋은 반드시 분리하여 반영** — 다른 Agent의 변경사항이 섞이지 않도록 한다

### 9. 결과 보고

작업 완료 시 사용자에게 요약 보고한다:
- 커밋 내역
- 수정/변경 사항 요약
- 검증 결과

### 10. 팀 유지 (중요)

- **작업 완료 후 Agent를 shutdown하지 않는다.** 사용자가 추가 작업을 요청할 수 있다.
- Agent shutdown은 **사용자가 명시적으로 팀 해산/종료를 지시할 때만** 수행한다.
- 사용자가 추가 작업을 요청하면 기존 Agent를 활용하여 계속 진행한다.
- 사용자가 "팀 해산", "종료", "끝" 등을 명시적으로 지시하면 그때 Agent shutdown과 TeamDelete를 수행한다.

## 버그 처리 조율

### 버그 처리 시점

| 시점 | 대상 | 행동 |
|------|------|------|
| 발견 즉시 | Critical | 현재 작업을 중단하고 즉시 수정 할당 |
| `/team` 실행 시 | Open 전체 | `docs/qa-agent/Bugs/`의 Open 버그를 검토하고, 이번 작업에 포함할 버그를 선별 |
| `/bugfix` 실행 시 | Open 전체 | 축적된 Open 버그를 심각도 순으로 정리하고 수정 할당 |

### 버그 할당 기준
- 코드/데이터 버그 → dev-agent
- 기획 문서 불일치 → planning-agent
- 리소스 문제 → graphics-agent

### 버그 수정 흐름
1. PD(team-lead)가 Open 버그를 검토하여 적절한 Agent에게 수정 할당 (상태 → In Progress)
2. Agent가 수정 완료 후 보고
3. qa-agent에게 수정 검증 요청
4. 검증 통과 시 qa-agent가 버그 상태를 Closed로 변경하고 수정 이력 기록
