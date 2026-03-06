---
name: team
description: 팀을 구성하여 작업을 수행. catcat-pd를 반드시 spawn하고, PD가 작업을 분석하여 필요한 Agent를 추가 spawn.
user-invocable: true
disable-model-invocation: false
---

# team - 팀 구성 및 작업 실행

## 사용법

`/team <작업 설명>` 형태로 사용한다.

작업 설명이 없으면 사용자에게 어떤 작업을 수행할지 질문한다.

## 실행 절차

### 1. 작업 설명 확인

사용자가 `/team` 뒤에 작업 설명을 입력했는지 확인한다.
- 입력했으면 → 2단계로 진행
- 입력하지 않았으면 → 사용자에게 "어떤 작업을 수행할까요?" 질문 후 대기

### 2. 팀 생성

TeamCreate 도구로 팀을 생성한다.
- `team_name`: 작업 내용을 요약한 짧은 영문 kebab-case 이름 (예: `ui-refactor`, `battle-system`)
- `description`: 사용자가 요청한 작업 설명

### 3. catcat-pd를 팀 멤버로 spawn (필수)

**반드시** catcat-pd를 팀 멤버로 spawn한다. 이 단계는 생략할 수 없다.

> **주의**: catcat-pd는 subagent(blocking)가 아니라 **팀 멤버**로 spawn해야 한다.
> Agent 도구 호출 시 반드시 `team_name`을 지정하여 팀에 합류시킨다.

Agent 도구로 catcat-pd를 spawn할 때 다음을 포함한다:

- `subagent_type`: `"catacat-pd"`
- `team_name`: 2단계에서 생성한 팀 이름
- `name`: `"catcat-pd"`
- `prompt`에 포함할 내용:
  1. 사용자가 요청한 작업 내용 전문
  2. 다음 지시사항:

```
## 작업 요청
{사용자의 작업 설명}

## 지시사항
당신은 이 팀의 PD입니다. 위 작업을 분석하고 실행하세요.

### Agent spawn 및 검증 절차
1. 작업을 분석하여 필요한 Agent를 판단하세요.
2. 필요한 Agent를 team_name을 지정하여 팀 멤버로 spawn하세요. 병렬 작업 시 isolation: "worktree"를 사용하세요.
3. **[필수] config 검증**: Agent spawn 후 `~/.claude/teams/{team_name}/config.json`을 읽어서 각 member의 `model` 필드를 확인하세요.
   - `"inherit"`가 발견되면 → **팀 구성 실패**로 판단합니다. 해당 Agent 이름과 함께 사용자에게 "model inherit 버그 감지, 팀 구성 실패" 보고 후 작업을 중단하세요.
4. **[필수] 메시지 수신 테스트**: spawn된 각 Agent에게 SendMessage로 "합류 확인 메시지를 보내주세요"라고 요청하세요. 각 Agent로부터 응답 메시지를 수신해야 정상입니다.
   - 응답이 오지 않는 Agent가 있으면 → config.json의 해당 member 상태를 재확인하고, 사용자에게 보고하세요.
   - 모든 Agent로부터 응답 수신 완료 → "팀 구성 완료, 메시지 수신 정상" 보고 후 본 작업 시작.

### Open 버그 확인
5. 본 작업 시작 전 `docs/QA/Bugs/` 디렉토리에서 Open 상태 버그를 확인하세요.
   - Critical 버그가 있으면 본 작업보다 우선하여 수정을 할당하세요.
   - Major/Minor 버그는 이번 작업에 포함할지 판단하여 선별하세요.

### 본 작업 수행
6. Task 도구를 사용하여 작업을 생성하고 Agent에게 할당하세요.
7. 결과를 검증하고, main 반영까지 완료하세요.

사용 가능한 Agent:
| Agent | 담당 |
|-------|------|
| planning-agent | 기획 문서, 정합성 검토 |
| dev-agent | 코드 구현, 컴파일 에러, 도구 개발 |
| graphics-agent | 그래픽 리소스, UI/UX |
| qa-agent | 테스트, 기획-구현 검증 |
| claude-code-engineer | Claude Code 설정, 사용법 |
```

### 4. catcat-pd spawn 후 config 검증

catcat-pd가 spawn된 직후 `~/.claude/teams/{team_name}/config.json`을 Read 도구로 읽어서 검증한다.

**검증 항목:**
- catcat-pd member의 `model` 필드가 `"inherit"`인지 확인
- `"inherit"`가 발견되면 → **팀 구성 실패**. 사용자에게 "catcat-pd model inherit 버그 감지, 팀 구성 실패" 보고 후 **즉시 중단** (수정 시도하지 않음)
- 정상이면 → 5단계로 진행

### 5. 완료 대기 및 보고

catcat-pd는 팀 멤버로 비동기 동작하므로, 메시지를 통해 결과를 수신한다.
catcat-pd로부터 작업 완료 메시지를 받으면 결과를 사용자에게 요약 보고한다.
