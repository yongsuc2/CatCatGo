# Agent Teams 통신 구조

## 작성일
2026-03-06

## 최종 수정일
2026-03-06

## 요약
- Agent Teams는 실험적 기능으로, 기본값 비활성화. `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1`로 활성화
- `.claude/agents/` 파일은 "Sub-agents"이며, Agent Teams와는 별개 기능
- 팀원끼리 1:1 직접 메시지 가능 (팀 리드를 거치지 않음)
- 각 팀원은 독립된 context window를 가지며, 정보 공유는 메시지 또는 파일 시스템을 통해 수행
- 공유 Task List로 작업을 조율하며, 팀원이 자율적으로 task를 claim하고 협업 가능

## 활성화 방법

settings.json (사용자 레벨 `~/.claude/settings.json` 또는 프로젝트 레벨 `.claude/settings.json`)에 추가:

```json
{
  "env": {
    "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS": "1"
  }
}
```

또는 쉘 환경 변수로 설정:
```bash
export CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1
```

활성화 후 Claude Code 세션을 새로 시작해야 적용된다.

## Agent Teams vs Sub-agents 구분

| 항목 | Sub-agents (.claude/agents/) | Agent Teams |
|------|-----|-----|
| 컨텍스트 | 자체 context, 결과만 메인에 반환 | 완전 독립 context window |
| 통신 | 메인 에이전트에게만 보고 | 팀원끼리 직접 메시지 교환 |
| 조율 | 메인 에이전트가 관리 | 공유 Task List로 자율 조율 |
| 토큰 비용 | 낮음 (결과 요약 반환) | 높음 (팀원마다 별도 인스턴스) |
| 적합한 작업 | 결과만 필요한 집중 작업 | 토론/협업이 필요한 복잡한 작업 |

## 사용 방법

Agent Teams는 별도 설정 파일이 필요하지 않다. 활성화 후 자연어로 Claude에게 팀 생성을 요청하면 된다.

팀 생성 요청 예시:
```
Create an agent team to refactor these modules in parallel.
Spawn three reviewers: security, performance, test coverage.
```

Claude가 자동으로 팀 리드 역할을 하며 팀원을 spawn한다.

## 디스플레이 모드

| 모드 | 설명 | 설정 |
|------|------|------|
| in-process (기본) | 메인 터미널에서 Shift+Down으로 팀원 간 전환 | `"teammateMode": "in-process"` |
| split-pane | 각 팀원이 별도 pane. tmux 또는 iTerm2 필요 | `"teammateMode": "tmux"` |
| auto (기본값) | tmux 세션이면 split-pane, 아니면 in-process | `"teammateMode": "auto"` |

Windows 환경에서는 split-pane이 지원되지 않으므로 in-process 모드를 사용한다.

## 상세 내용

### 1. 메시지 통신 방식

팀원끼리 팀 리드를 거치지 않고 직접 1:1 메시지를 보낸다. Mailbox 시스템을 통해 자동 전달되며, 수신자가 polling할 필요 없이 inbox에 도착한다.

| 메시지 타입 | 동작 |
|------------|------|
| message | 특정 팀원 1명에게 직접 전송 |
| broadcast | 전체 팀원에게 동시 전송. 비용이 팀원 수만큼 배수로 발생하므로 자제 권장 |

파일, 이미지 등 바이너리 첨부 기능은 없다. 파일 공유는 경로를 알려주거나 내용을 텍스트로 복사하는 방식.

### 2. Context 공유 -- 완전히 독립

각 팀원은 자기만의 독립된 context window를 가진다.

- 팀 리드의 대화 이력을 상속받지 않음
- 팀원 A가 파일을 읽은 결과가 팀원 B의 context에 자동으로 들어가지 않음
- 정보를 공유하려면 메시지로 직접 전달해야 함

팀원 생성 시 로드되는 것:
- CLAUDE.md (프로젝트 context)
- MCP servers, skills
- 팀 리드가 보낸 spawn prompt

### 3. 공유되는 자원

| 자원 | 저장 위치 | 설명 |
|------|----------|------|
| Task List | `~/.claude/tasks/{team-name}/` | 전체 팀 공유. pending/in_progress/completed 상태. 의존성(dependency) 지원 |
| Team Config | `~/.claude/teams/{team-name}/config.json` | members 배열 (name, agent ID, agent type). 팀원이 이 파일을 읽어 다른 팀원 발견 |
| 파일 시스템 | 작업 디렉토리 (같은 repo) | 한 팀원이 수정한 파일을 다른 팀원이 읽을 수 있음. 같은 파일 동시 수정은 충돌 위험 |

Task claiming은 file locking으로 race condition을 방지한다. 의존성이 있는 task는 선행 task 완료 시 자동으로 unblock된다.

### 4. Hooks 지원

| Hook 이벤트 | 설명 |
|-------------|------|
| TeammateIdle | 팀원이 idle 상태가 될 때 실행. exit code 2로 피드백 전달하여 계속 작업시킬 수 있음 |
| TaskCompleted | task가 완료 처리될 때 실행. exit code 2로 완료를 막고 피드백 전달 가능 |

### 5. 사용자도 팀원에게 직접 소통 가능

- In-process 모드: `Shift+Down`으로 팀원 간 전환, 직접 메시지 입력. `Ctrl+T`로 task list 토글
- Split-pane 모드: 팀원 pane을 클릭하여 해당 세션에 직접 입력

### 6. 팀 종료

- 개별 팀원: "Ask the researcher teammate to shut down"
- 전체 팀: "Clean up the team" (팀 리드에게 요청)
- 반드시 팀 리드를 통해 정리해야 함. 팀원이 cleanup을 실행하면 리소스가 불일치 상태가 될 수 있음

### 7. 주의사항/제한사항

- 같은 파일을 두 팀원이 동시 수정하면 덮어쓰기 발생. 팀원별 담당 파일을 분리할 것
- broadcast는 팀원 수 x 메시지 비용이 발생하므로 꼭 필요할 때만 사용
- 팀원은 자기 팀을 만들거나 다른 팀원을 spawn할 수 없음 (nested teams 불가)
- /resume과 /rewind는 in-process 팀원을 복원하지 못함
- 세션당 하나의 팀만 운영 가능
- 팀 리드는 변경 불가 (팀 생성 세션이 리드)
- Windows Terminal, VS Code 통합 터미널에서는 split-pane 미지원

## 권장 팀 규모

- 3~5명의 팀원이 적정
- 팀원당 5~6개 task가 적절
- 토큰 비용이 팀원 수에 비례하여 증가하므로, 병렬 작업의 가치가 있는 경우에만 사용

## 출처
- Claude Code Agent Teams: https://code.claude.com/docs/en/agent-teams
- Claude Code Sub-agents: https://code.claude.com/docs/en/sub-agents

## 관련 문서
- docs/Claude_Code_Engineer/Knowledge/Claude_토큰_사용_구조.md
